"""Deterministic Voronoi fracture for structural pieces (slabs, stair flights, walls).

Cell Fracture moved out of Blender's bundled add-ons, so this is our own: each chunk is the source mesh clipped by
the bisecting planes between its seed and every other seed (exact for convex sources such as slabs and flights;
for concave ones, fracture the convex parts). Same seed, same chunks, on every machine, which is what the game's
pre-fractured collapse relies on (the host broadcasts a seed; every client plays the same break).
"""
import random

import bmesh
import bpy
from mathutils import Vector

from . import kit


def seeds(lo, hi, count, rng, bias_edges=.0):
    pts = []
    for _ in range(count):
        p = Vector([rng.uniform(lo[i], hi[i]) for i in range(3)])
        if bias_edges and rng.random() < bias_edges:
            axis = rng.randrange(2)
            p[axis] = lo[axis] if rng.random() < .5 else hi[axis]
        pts.append(p)
    return pts


def voronoi(ob, count=9, seed=1, flatten_z=True, inset=.0015, col=None, prefix='Chunk_'):
    """Split `ob` into `count` chunks; returns the chunk objects (origins at their centres), ob untouched.
    flatten_z: seeds spread in X/Y only, so a slab breaks into slab-thick pieces like a real floor."""
    rng = random.Random(seed)
    lo, hi = kit.bounds([ob])
    if flatten_z:
        mid = (lo.z + hi.z) / 2
        pts = [Vector((p.x, p.y, mid)) for p in seeds(lo, hi, count, rng)]
    else:
        pts = seeds(lo, hi, count, rng)
    src = bmesh.new()
    src.from_mesh(ob.data)
    src.transform(ob.matrix_world)
    chunks = []
    for i, p in enumerate(pts):
        bm = src.copy()
        for j, q in enumerate(pts):
            if i == j:
                continue
            n = (q - p).normalized()
            mid = (p + q) / 2 - n * inset
            geom = bm.verts[:] + bm.edges[:] + bm.faces[:]
            res = bmesh.ops.bisect_plane(bm, geom=geom, plane_co=mid, plane_no=n, clear_outer=True)
            cut = [e for e in res['geom_cut'] if isinstance(e, bmesh.types.BMEdge)]
            if cut:
                try:
                    bmesh.ops.edgeloop_fill(bm, edges=cut)
                except (ValueError, RuntimeError):
                    bmesh.ops.holes_fill(bm, edges=cut)
        if len(bm.faces) < 4:
            bm.free()
            continue
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        c = kit.from_bmesh(f'{prefix}{len(chunks):02d}', bm, col)
        for m in ob.data.materials:
            c.data.materials.append(m)
        lo_c, hi_c = kit.bounds([c])
        kit.set_origin(c, (lo_c + hi_c) / 2)
        chunks.append(c)
    src.free()
    return chunks


def crack_lines(ob, seed=1, count=5, depth=.004, width=.006):
    """A 'cracked' stage: shallow grooves cut along random lines across the top face (geometry the normal bake
    picks up). Returns ob for chaining; the intact stage is ob before this call (copy it first)."""
    rng = random.Random(seed)
    lo, hi = kit.bounds([ob])
    for k in range(count):
        a = Vector((rng.uniform(lo.x, hi.x), rng.uniform(lo.y, hi.y), hi.z))
        ang = rng.uniform(0, 3.1416)
        length = rng.uniform(.4, 1.0) * max(hi.x - lo.x, hi.y - lo.y)
        d = Vector((__import__('math').cos(ang), __import__('math').sin(ang), 0))
        pts = [a + d * length * t / 6 + Vector((rng.uniform(-.06, .06), rng.uniform(-.06, .06), 0)) for t in range(7)]
        cutter = kit.tube_along(f'Crack{k}', [p + Vector((0, 0, .0)) for p in pts], width)
        cutter.scale.z = depth / width
        kit.boolean(ob, cutter, 'DIFFERENCE')
    return ob
