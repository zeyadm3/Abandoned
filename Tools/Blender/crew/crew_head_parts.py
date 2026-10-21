"""Neck, ears, eyes and hair for the realistic crew head (see crew_head.py)."""
import math
import bmesh
import bpy
import numpy as np
from mathutils import Vector
import crew_geo as G

LID_W = .0148
LID_GAP = .0018  # eyelid thickness over the eyeball


def lid_curves(out):
    """Upper and lower lid edge heights for a distance `out` from the eye centre (positive = outer corner)."""
    u = np.clip(1 - (np.asarray(out) / LID_W) ** 2, 0, 1)
    return .0049 * u ** .75 + .06 * np.asarray(out), -.0035 * u ** .85 + .03 * np.asarray(out)


def neck(M):
    """A thick labourer's neck running from inside the collar up under the skull, with an Adam's apple."""
    ob = G.loft('Neck', [{'c': (0, .014, z), 'rx': rx, 'rf': rf, 'rb': rb} for z, rx, rf, rb in
                         ((1.40, .08, .072, .078), (1.46, .077, .068, .076), (1.52, .073, .064, .074), (1.56, .068, .058, .07),
                          (1.59, .058, .045, .06))], M['Skin'], n=40, cap=(False, True))
    return [ob]


def hair_shell(M, head, m):
    """Crew cut, short beard and brows as a shell grown off the scalp by each mask, so every edge fades to skin."""
    me = head.data
    n = len(me.vertices)
    co = np.zeros(n * 3)
    me.vertices.foreach_get('co', co)
    co = co.reshape(-1, 3)
    nrm = np.zeros(n * 3)
    me.vertex_normals.foreach_get('vector', nrm)
    nrm = nrm.reshape(-1, 3)
    H = co[:, 2] - 1.65
    hair, beard, brow = (m[k].reshape(-1) for k in ('hair', 'beard', 'brow'))
    top = np.clip((H - .05) / .06, 0, 1)
    off = .0004 + np.maximum.reduce([hair * (.0016 + .0042 * top), beard * .0017, brow * .0012])
    cover = np.maximum.reduce([hair, beard, brow])
    shell = co + nrm * off[:, None]
    keep = cover > .002
    index = -np.ones(n, dtype=np.int64)
    index[keep] = np.arange(keep.sum())
    loops = np.zeros(len(me.polygons) * 4, dtype=np.int64)
    me.polygons.foreach_get('vertices', loops)
    quads = loops.reshape(-1, 4)
    quads = quads[np.all(keep[quads], axis=1)]
    out = bpy.data.meshes.new('Hair')
    out.from_pydata(shell[keep].tolist(), [], index[quads].tolist())
    out.update()
    for key, val in (('beard', beard), ('brow', brow), ('cover', cover)):
        a = out.attributes.new(key, 'FLOAT', 'POINT')
        a.data.foreach_set('value', val[keep].astype(np.float32))
    out.shade_smooth()
    out.materials.append(M['Hair'])
    ob = bpy.data.objects.new('Hair', out)
    G.COLL.objects.link(ob)
    return ob


def ear(M, s, eye_z, cy):
    """Helix rim, antihelix, concha bowl, tragus and lobe as a height field on a tilted elliptical disc."""
    NR, NT = 28, 72
    centre = Vector((s * .074, cy + .014, eye_z - .022))
    tilt = math.radians(15)
    up, back, out = Vector((0, math.sin(tilt), math.cos(tilt))), Vector((0, math.cos(tilt), -math.sin(tilt))), Vector((s, 0, 0))
    bm = bmesh.new()
    rows = []
    flush = []
    for i in range(1, NR + 1):
        rho = i / NR
        row = []
        for j in range(NT):
            th = 2 * math.pi * j / NT
            a, b = rho * math.cos(th), rho * math.sin(th)
            bw = b * (1 + .15 * a) * (.85 if a < -.5 else 1.0)
            wr = 1 - math.exp(-((math.atan2(b, a) + 1.86) / .7) ** 2)
            h = (.0032 * math.exp(-((rho - .9) / .08) ** 2) * wr - .0012 * math.exp(-((rho - .74) / .06) ** 2) * wr
                 + .0022 * math.exp(-((rho - .55) / .07) ** 2) * (1 / (1 + math.exp(-(a + .2) * 12))) * (1 / (1 + math.exp(-(b + .6) * 12)))
                 - .0055 * math.exp(-(((a + .12) / .38) ** 2 + ((b + .15) / .42) ** 2))
                 + .0035 * math.exp(-(((a + .2) / .15) ** 2 + ((b + .95) / .12) ** 2))
                 + .0015 * math.exp(-(((a + .82) / .2) ** 2 + (b / .5) ** 2)))
            stick = .004 + .011 * max(0.0, min(1.0, (b + 1) / 2)) ** 1.2 * (1 - .5 * max(0.0, -a))
            row.append(bm.verts.new(centre + up * (a * .031) + back * (bw * .0175) + out * (stick + h)))
        rows.append(row)
    hub = bm.verts.new(centre + out * (.004 - .0055))
    for j in range(NT):
        k = (j + 1) % NT
        bm.faces.new((hub, rows[0][j], rows[0][k]))
        for i in range(NR - 1):
            bm.faces.new((rows[i][j], rows[i + 1][j], rows[i + 1][k], rows[i][k]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    ob = G.finish('Ear', bm, M['Skin'], subsurf=1)
    a = ob.data.attributes.new('flush', 'FLOAT', 'POINT')
    a.data.foreach_set('value', np.full(len(ob.data.vertices), .55, dtype=np.float32))
    t = ob.modifiers.new('Thickness', 'SOLIDIFY')
    t.thickness, t.offset = .004, 0.0
    ob.modifiers.move(len(ob.modifiers) - 1, 0)
    ob.data.uv_layers.new(name='UVMap')
    return ob


def eyes(M, fe, cy, ex, ez, er):
    """Eyeballs (iris drawn by the material along local -Y), a clear cornea bulge, lash lines, inner corners."""
    parts = []
    for s in (-1, 1):
        c = Vector((s * ex, cy - fe, ez + .001))
        for name, radius, shift, mat in (('Eyeball', er, 0.0, 'Eye'), ('Cornea', .0079, -(er - .0079) - .0011, 'Cornea')):
            bm = bmesh.new()
            bm.loops.layers.uv.new('UVMap')
            bmesh.ops.create_uvsphere(bm, u_segments=48, v_segments=32, radius=radius, calc_uvs=True)
            for v in bm.verts:
                v.co.y += shift
            ob = G.finish(name, bm, M[mat], subsurf=0)
            ob.location = c
            parts.append(ob)
        RL = er + LID_GAP
        for edge, k in (('UpperLash', 0), ('LowerLash', 1)):
            pts = []
            for o in np.linspace(-LID_W * .92, LID_W * .92, 11):
                zu, zl = lid_curves(o)
                dz = float(zu if k == 0 else zl)
                f = fe + math.sqrt(max(RL * RL - o * o - dz * dz, 0)) + .0004
                pts.append((s * (ex + o), cy - f, ez + .001 + dz + (.0003 if k == 0 else -.0002)))
            parts.append(G.tube(edge, pts, M['Lash'], .00075 if k == 0 else .00042, sides=6, subsurf=1))
        parts.append(G.quad_sphere('Caruncle', (s * (ex - .0128), cy - fe - .0035, ez + .0007), (.0022, .002, .0018),
                                   M['Lip'], cuts=3))
    return parts
