"""Code-driven sculpting: dense displacement on a high-poly copy, then the detail is baked onto the game mesh.

Without AI generators or scans (user, 2026-10-27), realism comes from layered procedural displacement:
  - fields: smooth falloffs (blobs, ridges, creases) placed on the surface by position, for anatomy;
  - noise: pores, wrinkles, rock and flesh texture, from Blender's noise at several scales;
  - the high copy is subdivided, displaced and used as the bake source (bake.bake_asset(high=...)).
Functions take a mesh object and move its vertices along their normals with numpy (fast, deterministic).
"""
import math

import bmesh
import bpy
import numpy as np
from mathutils import Vector, noise

from . import kit


def high_copy(ob, levels=2, name=None):
    """A subdivided copy for sculpting and baking (keeps the original as the game mesh)."""
    c = ob.copy()
    c.data = ob.data.copy()
    c.name = name or ob.name + '_High'
    kit.link(c, ob.users_collection[0] if ob.users_collection else None)
    c.modifiers.clear()
    m = c.modifiers.new('Sub', 'SUBSURF')
    m.levels = m.render_levels = levels
    kit.apply(c)
    return c


def _arrays(ob):
    me = ob.data
    n = len(me.vertices)
    co = np.empty(n * 3, np.float64)
    me.vertices.foreach_get('co', co)
    me.calc_normals_split() if hasattr(me, 'calc_normals_split') else None
    nor = np.empty(n * 3, np.float64)
    me.vertices.foreach_get('normal', nor)
    return co.reshape(-1, 3), nor.reshape(-1, 3)


def _write(ob, co):
    ob.data.vertices.foreach_set('co', co.astype(np.float32).ravel())
    ob.data.update()


def displace(ob, fn, strength=1.0):
    """Move every vertex along its normal by strength * fn(position array Nx3) -> N."""
    co, nor = _arrays(ob)
    d = np.nan_to_num(fn(co) * strength, nan=0.0, posinf=0.0, neginf=0.0)
    _write(ob, co + nor * d[:, None])
    return ob


def blob(centre, radius, amount, squash=(1, 1, 1)):
    """A smooth bump (positive) or dent (negative) of `amount` metres at `centre`."""
    c, s = np.array(centre), np.array(squash)

    def f(co):
        r = np.linalg.norm((co - c) / s, axis=1) / radius
        return amount * np.clip(1 - r * r, 0, 1) ** 2
    return f


def ridge(a, b, radius, amount):
    """A raised (or creased, if negative) line between a and b: brows, tendons, scars, seams, wrinkles."""
    a, b = np.array(a), np.array(b)
    d = b - a
    L2 = max(float(d @ d), 1e-9)

    def f(co):
        t = np.clip(((co - a) @ d) / L2, 0, 1)
        r = np.linalg.norm(co - (a + t[:, None] * d), axis=1) / radius
        return amount * np.clip(1 - r * r, 0, 1) ** 2
    return f


def noise_field(scale, amount, octaves=4, kind='PERLIN_ORIGINAL', offset=(0, 0, 0), ridged=False):
    """Multi-octave noise (Blender's mathutils.noise) as a displacement field."""
    off = Vector(offset)

    def f(co):
        out = np.empty(len(co), np.float32)
        for i, p in enumerate(co):
            v = Vector(p) * scale + off
            if ridged:
                out[i] = noise.ridged_multi_fractal(v, 1.0, 2.0, octaves, 1.0, 2.0, noise_basis=kind) - .5
            else:
                out[i] = noise.fractal(v, 1.0, 2.0, octaves, noise_basis=kind)
        return out * amount
    return f


def wrinkles(axis_dir, freq, amount, region=None):
    """Parallel sine wrinkles across a direction (forehead lines, cloth folds, flesh rolls), masked by region."""
    d = np.array(axis_dir) / max(np.linalg.norm(axis_dir), 1e-9)

    def f(co):
        w = np.sin((co @ d) * freq * math.tau) * amount
        return w * (region(co) if region else 1.0)
    return f


def mask_sphere(centre, radius, soft=.5):
    c = np.array(centre)

    def f(co):
        r = np.linalg.norm(co - c, axis=1) / radius
        return np.clip((1 - r) / max(soft, 1e-3), 0, 1)
    return f


def combine(*fields):
    def f(co):
        return sum(g(co) for g in fields)
    return f


def smooth(ob, iterations=2, factor=.5):
    bm = bmesh.new()
    bm.from_mesh(ob.data)
    for _ in range(iterations):
        bmesh.ops.smooth_vert(bm, verts=bm.verts, factor=factor, use_axis_x=True, use_axis_y=True, use_axis_z=True)
    bm.to_mesh(ob.data)
    bm.free()
    return ob


def retopo(high, budget, name=None):
    """A game mesh from a sculpt when no clean base exists: voxel remesh then decimate to budget."""
    c = high.copy()
    c.data = high.data.copy()
    c.name = name or high.name.replace('_High', '')
    kit.link(c, high.users_collection[0] if high.users_collection else None)
    lo, hi = kit.bounds([high])
    size = max((hi - lo).length / 180.0, .004)
    m = c.modifiers.new('Remesh', 'REMESH')
    m.mode, m.voxel_size, m.use_smooth_shade = 'VOXEL', size, True
    kit.apply(c)
    kit.decimate_to(c, budget, delimit=('NORMAL',))
    c.data.shade_smooth()
    return c
