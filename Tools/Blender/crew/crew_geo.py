"""Geometry helpers for the crew redesign: lofted garments, quad spheres, tubes and straps (Blender-only)."""
import math
import bmesh
import bpy
from mathutils import Vector, Matrix

TAU = math.tau
COLL = None  # set by the builder: the collection new objects are linked into


def _sgnpow(v, e):
    return math.copysign(abs(v) ** e, v)


def finish(name, bm, mat, smooth=True, subsurf=2, uv=None):
    """Turn a bmesh into a linked object with one material and an optional subdivision modifier."""
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    for p in me.polygons:
        p.use_smooth = smooth
    if uv is not None:
        layer = me.uv_layers.new(name='UVMap')
        for poly in me.polygons:
            for li in poly.loop_indices:
                layer.data[li].uv = uv[me.loops[li].vertex_index]
    ob = bpy.data.objects.new(name, me)
    (COLL or bpy.context.scene.collection).objects.link(ob)
    if mat is not None:
        me.materials.append(mat)
    if subsurf:
        m = ob.modifiers.new('Subdivision', 'SUBSURF')
        m.levels, m.render_levels = 1, subsurf
    return ob


def ring_points(c, rx, rf, rb, p=2.0, n=32, u=(1, 0, 0), v=(0, 1, 0), wob=0.0, freq=0, phase=0.0, arc=None):
    """Superellipse section. rf is the half-depth on the -v side (front), rb on +v. p>2 squares it off."""
    u, v, c = Vector(u), Vector(v), Vector(c)
    e = 2.0 / p
    pts = []
    count = n if arc is None else n + 1
    for i in range(count):
        a = TAU * i / n if arc is None else arc[0] + (arc[1] - arc[0]) * i / n
        ca, sa = math.cos(a), math.sin(a)
        x = rx * _sgnpow(ca, e)
        y = (rf if sa < 0 else rb) * _sgnpow(sa, e)
        k = 1.0 + wob * math.sin(freq * a + phase) if wob else 1.0
        pts.append(c + u * x * k + v * y * k)
    return pts


def loft(name, rings, mat, n=32, plane='XY', cap=(True, True), subsurf=2, arc=None):
    """Rings are dicts: c (centre), rx, rf, rb, p, wob, freq, phase. Planes: XY (rings stacked on Z), XZ (on Y)."""
    axes = {'XY': ((1, 0, 0), (0, 1, 0)), 'XZ': ((1, 0, 0), (0, 0, 1)), 'YZ': ((0, 1, 0), (0, 0, 1))}[plane]
    bm = bmesh.new()
    rows, uvs, run, prev = [], {}, 0.0, None
    for r in rings:
        pts = ring_points(r['c'], r['rx'], r.get('rf', r['rx']), r.get('rb', r.get('rf', r['rx'])), r.get('p', 2.0), n,
                          axes[0], axes[1], r.get('wob', 0.0), r.get('freq', 0), r.get('phase', 0.0), arc)
        if prev is not None:
            run += (Vector(r['c']) - prev).length
        prev = Vector(r['c'])
        row = [bm.verts.new(q) for q in pts]
        for i, vert in enumerate(row):
            uvs[vert] = (i / len(row), run * 2.0)
        rows.append(row)
    closed = arc is None
    for a, b in zip(rows, rows[1:]):
        m = len(a)
        for i in range(m if closed else m - 1):
            j = (i + 1) % m
            bm.faces.new((a[i], a[j], b[j], b[i]))
    if closed:
        for which, row in ((0, rows[0]), (1, rows[-1])):
            if not cap[which]:
                continue
            centre = bm.verts.new(sum((q.co for q in row), Vector()) / len(row))
            uvs[centre] = (.5, .5)
            for i in range(len(row)):
                j = (i + 1) % len(row)
                f = (centre, row[j], row[i]) if which == 0 else (centre, row[i], row[j])
                bm.faces.new(f)
    bm.normal_update()
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.verts.index_update()
    uv = [uvs[vt] for vt in bm.verts]
    return finish(name, bm, mat, subsurf=subsurf, uv=uv)


def two_tone(ob, mat, test):
    """Give faces whose centre passes test(centre) a second material (a yoke, a toe cap, a panel)."""
    ob.data.materials.append(mat)
    for poly in ob.data.polygons:
        if test(poly.center):
            poly.material_index = len(ob.data.materials) - 1
    return ob


def quad_sphere(name, centre, radii, mat, cuts=6, shape=None, keep=None, subsurf=2, rot=None, power=1.0):
    """Cube projected to an ellipsoid; shape(d, p) sculpts it, keep(d) trims it to a cap or shell.
    power < 1 squares it off into a soft rounded block (pads, plates)."""
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=2.0)
    bmesh.ops.subdivide_edges(bm, edges=bm.edges[:], cuts=cuts, use_grid_fill=True)
    bmesh.ops.remove_doubles(bm, verts=bm.verts[:], dist=1e-5)
    R = Matrix.Identity(3)
    if rot:
        R = (Matrix.Rotation(math.radians(rot[2]), 3, 'Z') @ Matrix.Rotation(math.radians(rot[1]), 3, 'Y')
             @ Matrix.Rotation(math.radians(rot[0]), 3, 'X'))
    dirs = {}
    for vt in bm.verts:
        d = vt.co.normalized()
        dirs[vt] = d
        q = Vector((_sgnpow(d.x, power), _sgnpow(d.y, power), _sgnpow(d.z, power))) if power != 1.0 else d
        p = Vector((q.x * radii[0], q.y * radii[1], q.z * radii[2]))
        if shape:
            p = shape(d, p)
        vt.co = Vector(centre) + R @ p
    if keep:
        gone = [vt for vt in bm.verts if not keep(dirs[vt])]
        bmesh.ops.delete(bm, geom=gone, context='VERTS')
    bm.normal_update()
    ob = finish(name, bm, mat, subsurf=subsurf)
    sph = ob.data.uv_layers.new(name='UVMap')
    for poly in ob.data.polygons:
        for li in poly.loop_indices:
            co = ob.data.vertices[ob.data.loops[li].vertex_index].co - Vector(centre)
            sph.data[li].uv = (.5 + math.atan2(co.y, co.x) / TAU, .5 + co.z * 2.0)
    return ob


def tube(name, pts, mat, radius=.01, width=None, sides=10, subsurf=1, caps=True, up=(0, 0, 1)):
    """Path tube. pts are (x, y, z) or (x, y, z, scale). width=(w, h) gives a flat strap section instead of round."""
    P = [Vector(q[:3]) for q in pts]
    S = [q[3] if len(q) > 3 else 1.0 for q in pts]
    bm = bmesh.new()
    rows, uvs, run = [], {}, 0.0
    prev_n = Vector(up)
    for i, p in enumerate(P):
        t = (P[min(i + 1, len(P) - 1)] - P[max(i - 1, 0)]).normalized()
        nrm = (prev_n - t * prev_n.dot(t))
        if nrm.length < 1e-6:
            nrm = t.orthogonal()
        nrm.normalize()
        prev_n = nrm
        b = t.cross(nrm).normalized()
        if i:
            run += (P[i] - P[i - 1]).length
        row = []
        for k in range(sides):
            a = TAU * k / sides
            if width:
                ca, sa = math.cos(a), math.sin(a)
                off = b * _sgnpow(ca, .25) * width[0] * .5 + nrm * _sgnpow(sa, .25) * width[1] * .5
            else:
                off = (b * math.cos(a) + nrm * math.sin(a)) * radius
            vt = bm.verts.new(p + off * S[i])
            uvs[vt] = (k / sides, run * 4.0)
            row.append(vt)
        rows.append(row)
    for a, c in zip(rows, rows[1:]):
        for k in range(sides):
            j = (k + 1) % sides
            bm.faces.new((a[k], a[j], c[j], c[k]))
    if caps:
        for row, flip in ((rows[0], True), (rows[-1], False)):
            f = bm.faces.new(row if not flip else list(reversed(row)))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.verts.index_update()
    return finish(name, bm, mat, subsurf=subsurf, uv=[uvs.get(vt, (0, 0)) for vt in bm.verts])


def rbox(name, centre, size, mat, bevel=.01, segments=3, rot=(0, 0, 0), subsurf=0):
    """Rounded box (bevel modifier) for pockets, buckles and plates."""
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    # Vertices stay in rest-pose world space: materials read object Z as height above the floor.
    turn = (Matrix.Rotation(math.radians(rot[2]), 3, 'Z') @ Matrix.Rotation(math.radians(rot[1]), 3, 'Y')
            @ Matrix.Rotation(math.radians(rot[0]), 3, 'X'))
    for vt in bm.verts:
        vt.co = Vector(centre) + turn @ Vector((vt.co.x * size[0], vt.co.y * size[1], vt.co.z * size[2]))
    ob = finish(name, bm, mat, smooth=True, subsurf=subsurf)
    if bevel:
        m = ob.modifiers.new('Bevel', 'BEVEL')
        m.width = min(bevel, min(size) * .45)
        m.segments = segments
        m.limit_method = 'NONE'
        ob.modifiers.move(len(ob.modifiers) - 1, 0)
    ob.data.uv_layers.new(name='UVMap')
    return ob


def hug(ob, target, offset=.004, thickness=0.0, mode='NEAREST_SURFACEPOINT'):
    """Press a strap/band/patch onto a garment surface, then give it real thickness."""
    m = ob.modifiers.new('Hug', 'SHRINKWRAP')
    m.target, m.offset, m.wrap_method = target, offset, mode
    m.wrap_mode = 'OUTSIDE_SURFACE' if mode != 'PROJECT' else 'ON_SURFACE'
    if thickness:
        s = ob.modifiers.new('Thickness', 'SOLIDIFY')
        s.thickness, s.offset = thickness, 1.0
    return ob


def bend_folds(ob, strength=.006, scale=.05, name='Folds'):
    """Low-frequency displacement so a garment reads as cloth, not plastic."""
    tex = bpy.data.textures.new(ob.name + name, 'CLOUDS')
    tex.noise_scale, tex.noise_depth = scale, 2
    m = ob.modifiers.new(name, 'DISPLACE')
    m.texture, m.strength, m.mid_level, m.texture_coords = tex, strength, .5, 'GLOBAL'
    return ob


def pivot(name, at, parent=None):
    e = bpy.data.objects.new(name, None)
    (COLL or bpy.context.scene.collection).objects.link(e)
    e.empty_display_size = .05
    e.location = at
    if parent is not None:
        e.parent = parent
        e.location = Vector(at) - parent_world(parent)
    return e


def parent_world(e):
    """Rest-pose position of a pivot in its crew member's own space (the root's placement is left out),
    so parts are always authored as if the character stood at the origin facing -Y."""
    if e.parent is None:
        return Vector()
    p = Vector(e.location)
    while e.parent is not None and e.parent.parent is not None:
        e = e.parent
        p += Vector(e.location)
    return p


def attach(ob, piv):
    """Parent a world-space part to a pivot so rotating the pivot swings it about the joint."""
    ob.parent = piv
    ob.matrix_parent_inverse = Matrix.Translation(-parent_world(piv))
    return ob


def slab(name, centre, size, mat, bend=0.0, tilt=0.0, bevel=.012, cuts=10):
    """A padded panel (plate bag, pouch) that curves around the body: y += bend * x^2, tilted about X."""
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.subdivide_edges(bm, edges=bm.edges[:], cuts=cuts, use_grid_fill=True)
    turn = Matrix.Rotation(math.radians(tilt), 3, 'X')
    for vt in bm.verts:
        x, y, z = vt.co.x * size[0], vt.co.y * size[1], vt.co.z * size[2]
        vt.co = Vector(centre) + turn @ Vector((x, y + bend * x * x, z))
    ob = finish(name, bm, mat, smooth=True, subsurf=0)
    if bevel:
        m = ob.modifiers.new('Bevel', 'BEVEL')
        m.width, m.segments, m.limit_method, m.angle_limit = bevel, 3, 'ANGLE', math.radians(40)
        ob.modifiers.move(len(ob.modifiers) - 1, 0)
    ob.data.uv_layers.new(name='UVMap')
    return ob
