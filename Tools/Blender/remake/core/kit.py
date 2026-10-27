"""Geometry kit for the 2026 remake: metric, deterministic, Unity-axis aware.

Every model is built in Blender's native axes with +Y as the model's FRONT (Unity +Z) and +Z up. The exporter
(export.py) turns that into Unity's frame, so builders never think about axis swaps. One Blender unit = one metre.
All helpers return the created object and never depend on the 3D view, so they run the same in the live Blender
(through the MCP) and in batch mode (--background).
"""
import math

import bmesh
import bpy
from mathutils import Matrix, Vector


# --------------------------------------------------------------------------------------------------------- scene

def reset():
    """Empty the file safely (works in the live Blender too, unlike read_factory_settings which would unload the
    MCP add-on): batch-remove every object, then purge the orphaned data they leave behind."""
    if bpy.context.object and bpy.context.object.mode != 'OBJECT':
        bpy.ops.object.mode_set(mode='OBJECT')
    bpy.data.batch_remove(list(bpy.data.objects))
    for col in list(bpy.data.collections):
        bpy.data.collections.remove(col)
    bpy.data.orphans_purge(do_local_ids=True, do_linked_ids=True, do_recursive=True)
    scene = bpy.context.scene
    scene.unit_settings.system = 'METRIC'
    scene.unit_settings.scale_length = 1.0
    return scene


def collection(name):
    """A child collection of the scene, created once and reused (keeps the outliner readable per asset)."""
    col = bpy.data.collections.get(name)
    if col is None:
        col = bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(col)
    return col


def link(ob, col=None):
    (col or bpy.context.scene.collection).objects.link(ob)
    return ob


def empty(name, location=(0, 0, 0), parent=None, col=None, size=.05):
    """A named empty: anchors (Anchor_*), sockets (Socket_*) and LOD holders are all empties."""
    ob = bpy.data.objects.new(name, None)
    ob.empty_display_size = size
    ob.location = location
    ob.parent = parent
    return link(ob, col)


# ----------------------------------------------------------------------------------------------------- primitives

def from_bmesh(name, bm, col=None, smooth=False):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    if smooth:
        me.shade_smooth()
    return link(bpy.data.objects.new(name, me), col)


def box(name, size, location=(0, 0, 0), bevel=0.0, segments=2, col=None, rotation=(0, 0, 0)):
    """An axis-aligned box of size (x, y, z) centred on location; bevel in metres (manufactured edges)."""
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.scale(bm, vec=Vector(size), verts=bm.verts)
    if bevel > 0 and min(size) > bevel * 2.2:
        bmesh.ops.bevel(bm, geom=list(bm.edges), offset=bevel, segments=segments, profile=.5, affect='EDGES')
    ob = from_bmesh(name, bm, col)
    ob.location = location
    ob.rotation_euler = [math.radians(a) for a in rotation]
    return ob


def cylinder(name, radius, depth, location=(0, 0, 0), sides=24, axis='Z', bevel=0.0, col=None, cap=True):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=cap, cap_tris=False, segments=sides, radius1=radius, radius2=radius, depth=depth)
    if bevel > 0 and cap:
        # Only the two cap rims: an edge between a cap (vertical normal) and a side face.
        rims = [e for e in bm.edges if len(e.link_faces) == 2 and
                (abs(e.link_faces[0].normal.z) > .9) != (abs(e.link_faces[1].normal.z) > .9)]
        bmesh.ops.bevel(bm, geom=rims, offset=min(bevel, radius * .3), segments=2, profile=.5, affect='EDGES')
    _orient(bm, axis)
    ob = from_bmesh(name, bm, col, smooth=True)
    ob.location = location
    auto_smooth(ob)
    return ob


def cone(name, r1, r2, depth, location=(0, 0, 0), sides=24, axis='Z', col=None):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=sides, radius1=r1, radius2=r2, depth=depth)
    _orient(bm, axis)
    ob = from_bmesh(name, bm, col, smooth=True)
    ob.location = location
    auto_smooth(ob)
    return ob


def sphere(name, radius, location=(0, 0, 0), segments=24, rings=12, scale=(1, 1, 1), col=None):
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=segments, v_segments=rings, radius=radius)
    bmesh.ops.scale(bm, vec=Vector(scale), verts=bm.verts)
    ob = from_bmesh(name, bm, col, smooth=True)
    ob.location = location
    return ob


def rod(name, a, b, radius=.02, sides=12, col=None):
    """A cylinder from point a to point b (pipes, rails, handles, bars)."""
    a, b = Vector(a), Vector(b)
    d = b - a
    ob = cylinder(name, radius, d.length, sides=sides, col=col)
    ob.matrix_world = Matrix.Translation((a + b) / 2) @ d.to_track_quat('Z', 'Y').to_matrix().to_4x4()
    return ob


def lathe(name, profile, sides=32, location=(0, 0, 0), col=None, cap_bottom=True, cap_top=True):
    """Revolve a (radius, height) profile around Z: vases, bottles, lamp shades, wheels, drums."""
    bm = bmesh.new()
    rings = []
    for r, z in profile:
        ring = []
        for i in range(sides):
            t = i / sides * math.tau
            ring.append(bm.verts.new((math.cos(t) * r, math.sin(t) * r, z)))
        rings.append(ring)
    for lo, hi in zip(rings, rings[1:]):
        for i in range(sides):
            j = (i + 1) % sides
            bm.faces.new((lo[i], lo[j], hi[j], hi[i]))
    if cap_bottom and profile[0][0] > 1e-4:
        bm.faces.new(list(reversed(rings[0])))
    if cap_top and profile[-1][0] > 1e-4:
        bm.faces.new(rings[-1])
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
    ob = from_bmesh(name, bm, col, smooth=True)
    ob.location = location
    auto_smooth(ob)
    return ob


def extrude_outline(name, outline, depth, location=(0, 0, 0), bevel=0.0, col=None, axis='Y'):
    """Extrude a closed 2D outline [(u, v), ...] by depth. axis='Y': the outline lies in X/Z (a front elevation,
    the usual way to draw panels, brackets, signs, gun-safe doors); axis='Z': in X/Y (a plan)."""
    bm = bmesh.new()
    if axis == 'Y':
        verts = [bm.verts.new((u, -depth / 2, v)) for u, v in outline]
        offset = Vector((0, depth, 0))
    else:
        verts = [bm.verts.new((u, v, -depth / 2)) for u, v in outline]
        offset = Vector((0, 0, depth))
    face = bm.faces.new(verts)
    ext = bmesh.ops.extrude_face_region(bm, geom=[face])
    bmesh.ops.translate(bm, vec=offset, verts=[v for v in ext['geom'] if isinstance(v, bmesh.types.BMVert)])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    if bevel > 0:
        bmesh.ops.bevel(bm, geom=list(bm.edges), offset=bevel, segments=2, profile=.5, affect='EDGES', clamp_overlap=True)
    ob = from_bmesh(name, bm, col)
    ob.location = location
    return ob


def loft(name, sections, closed=True, col=None, cap=True, smooth=True):
    """Skin a list of rings (each a list of 3D points, same count) into a surface: limbs, torsos, hoses, cables."""
    bm = bmesh.new()
    rings = [[bm.verts.new(p) for p in ring] for ring in sections]
    n = len(rings[0])
    for lo, hi in zip(rings, rings[1:]):
        for i in range(n if closed else n - 1):
            j = (i + 1) % n
            bm.faces.new((lo[i], lo[j], hi[j], hi[i]))
    if cap and closed:
        bm.faces.new(list(reversed(rings[0])))
        bm.faces.new(rings[-1])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    ob = from_bmesh(name, bm, col, smooth=smooth)
    if smooth:
        auto_smooth(ob)
    return ob


def ellipse_ring(centre, rx, ry, n=16, z_axis=(0, 0, 1), squash=None):
    """A ring of n points around centre in the plane perpendicular to z_axis (for lofts)."""
    c, axis = Vector(centre), Vector(z_axis).normalized()
    ref = Vector((1, 0, 0)) if abs(axis.x) < .9 else Vector((0, 1, 0))
    u = axis.cross(ref).normalized()
    v = axis.cross(u).normalized()
    pts = []
    for i in range(n):
        t = i / n * math.tau
        k = squash(t) if squash else 1.0
        pts.append(c + u * math.cos(t) * rx * k + v * math.sin(t) * ry * k)
    return pts


def tube_along(name, points, radius, sides=10, col=None):
    """A tube following a polyline (cables, hoses, pipe runs with bends)."""
    pts = [Vector(p) for p in points]
    sections = []
    for i, p in enumerate(pts):
        d = (pts[min(i + 1, len(pts) - 1)] - pts[max(i - 1, 0)]).normalized()
        r = radius(i / max(len(pts) - 1, 1)) if callable(radius) else radius
        sections.append(ellipse_ring(p, r, r, sides, d))
    return loft(name, sections, col=col)


def _orient(bm, axis):
    if axis == 'X':
        bmesh.ops.rotate(bm, verts=bm.verts, matrix=Matrix.Rotation(math.pi / 2, 3, 'Y'))
    elif axis == 'Y':
        bmesh.ops.rotate(bm, verts=bm.verts, matrix=Matrix.Rotation(math.pi / 2, 3, 'X'))


# ------------------------------------------------------------------------------------------------------ modifiers

def auto_smooth(ob, angle=35.0):
    """Smooth shading with hard edges past `angle` (4.1+ removed mesh auto smooth; mark sharp edges instead)."""
    me = ob.data
    me.shade_smooth()
    bm = bmesh.new()
    bm.from_mesh(me)
    limit = math.radians(angle)
    for e in bm.edges:
        if len(e.link_faces) == 2 and e.calc_face_angle(0) > limit:
            e.smooth = False
    bm.to_mesh(me)
    bm.free()
    return ob


def bevel_mod(ob, width=.004, segments=2, angle=30):
    m = ob.modifiers.new('Bevel', 'BEVEL')
    m.width, m.segments, m.limit_method, m.angle_limit = width, segments, 'ANGLE', math.radians(angle)
    m.harden_normals = True
    return m


def solidify(ob, thickness=.01, offset=-1):
    m = ob.modifiers.new('Solidify', 'SOLIDIFY')
    m.thickness, m.offset = thickness, offset
    return m


def subdivide(ob, levels=1):
    m = ob.modifiers.new('Subsurf', 'SUBSURF')
    m.levels = m.render_levels = levels
    return m


def array(ob, count, offset, merge=False):
    m = ob.modifiers.new('Array', 'ARRAY')
    m.count = count
    m.use_relative_offset = False
    m.use_constant_offset = True
    m.constant_offset_displace = offset
    m.use_merge_vertices = merge
    return m


def boolean(ob, cutter, operation='DIFFERENCE', apply_now=True, hide_cutter=True):
    m = ob.modifiers.new('Bool', 'BOOLEAN')
    m.object, m.operation, m.solver = cutter, operation, 'EXACT'
    if apply_now:
        apply(ob)
    if hide_cutter:
        bpy.data.objects.remove(cutter, do_unlink=True)
    return ob


def apply(ob):
    """Apply every modifier (evaluated mesh replaces the data); no operator or context needed."""
    dg = bpy.context.evaluated_depsgraph_get()
    me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg), preserve_all_data_layers=True, depsgraph=dg)
    old = ob.data
    ob.modifiers.clear()
    ob.data = me
    if old.users == 0:
        bpy.data.meshes.remove(old)
    return ob


def bake_transform(ob):
    """Apply location/rotation/scale into the mesh (children keep their world placement)."""
    mw = ob.matrix_world.copy()
    if ob.type == 'MESH':
        ob.data.transform(mw if ob.parent is None else ob.matrix_parent_inverse @ ob.matrix_basis)
        if mw.determinant() < 0:
            ob.data.flip_normals()
    for c in ob.children:
        c.matrix_parent_inverse = Matrix.Identity(4)
        c.matrix_world = c.matrix_world
    ob.matrix_basis = Matrix.Identity(4)
    return ob


def join(objects, name, col=None):
    """Merge meshes into one object (materials kept per face). Origin stays at the world origin."""
    objects = [o for o in objects if o and o.type == 'MESH']
    bm = bmesh.new()
    mats = []
    dg = bpy.context.evaluated_depsgraph_get()
    for o in objects:
        ev = o.evaluated_get(dg)
        me = ev.to_mesh()
        me.transform(o.matrix_world)
        if o.matrix_world.determinant() < 0:
            me.flip_normals()
        remap = []
        for slot in o.material_slots:  # originals; ev.material_slots hold depsgraph copies
            if slot.material not in mats:
                mats.append(slot.material)
            remap.append(mats.index(slot.material))
        for p in me.polygons:
            p.material_index = remap[p.material_index] if remap else 0
        bm.from_mesh(me)
        ev.to_mesh_clear()
    out = from_bmesh(name, bm, col)
    for m in mats:
        out.data.materials.append(m)
    bpy.data.batch_remove(objects)
    return out


def set_origin(ob, point):
    """Move the object's origin to `point` (world) without moving its geometry."""
    p = Vector(point)
    local = ob.matrix_world.inverted() @ p
    ob.data.transform(Matrix.Translation(-local))
    ob.matrix_world = ob.matrix_world @ Matrix.Translation(local)
    return ob


def bounds(objects):
    """World AABB (min, max) of mesh objects."""
    lo = Vector((1e9, 1e9, 1e9))
    hi = -lo
    for o in objects:
        if o.type != 'MESH':
            continue
        for c in o.bound_box:
            w = o.matrix_world @ Vector(c)
            lo = Vector(map(min, lo, w))
            hi = Vector(map(max, hi, w))
    return lo, hi


def fit_to_box(objects, size, ground=True, keep_aspect=True):
    """Scale a set of objects uniformly (or not) so their AABB fits `size` (x, y, z); centre on X/Y, base on Z=0."""
    lo, hi = bounds(objects)
    ext = hi - lo
    if keep_aspect:
        s = min(size[i] / max(ext[i], 1e-6) for i in range(3))
        scale = Vector((s, s, s))
    else:
        scale = Vector([size[i] / max(ext[i], 1e-6) for i in range(3)])
    centre = (lo + hi) / 2
    pivot = Vector((centre.x, centre.y, lo.z if ground else centre.z))
    m = Matrix.Translation((0, 0, 0)) @ Matrix.Diagonal(scale.to_4d()) @ Matrix.Translation(-pivot)
    for o in objects:
        if o.parent is None:
            o.matrix_world = m @ o.matrix_world
    return scale


def tris(ob):
    me = ob.data
    me.calc_loop_triangles()
    return len(me.loop_triangles)


def decimate_to(ob, budget, delimit=('UV', 'NORMAL', 'MATERIAL')):
    """Collapse-decimate a mesh down to a triangle budget (keeps UV seams, sharp edges and material borders)."""
    t = tris(ob)
    if t <= budget:
        return ob
    m = ob.modifiers.new('Budget', 'DECIMATE')
    m.ratio = max(budget / t, .01)
    m.delimit = set(delimit)
    m.use_collapse_triangulate = False
    return apply(ob)
