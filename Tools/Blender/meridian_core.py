"""Metric, deterministic mesh helpers for the original Meridian 0.13 kit."""
import bpy, math, os
from mathutils import Vector

OBJECTS = []
MATERIALS = {}
OUT = ''
UV_METRES = {'Terrazzo': 4, 'Tile': 4, 'Carpet': 2, 'Concrete': 4,
             'Wood': 4, 'BridgeMetal': 2, 'Block': 4, 'Asphalt': 4,
             'Ceiling': 1, 'Plaster': 4, 'EscalatorSteel': 2,
             'StorefrontTrim': 2, 'FacadeStone': 4, 'Rust': 2}

def xyz(p): return (p[0], p[2], p[1])

def initialize(output, sets):
    global OUT
    OUT = output
    os.makedirs(OUT, exist_ok=True)
    bpy.context.scene.unit_settings.system = 'METRIC'
    bpy.context.scene.unit_settings.scale_length = 1
    for key in sets:
        mat = bpy.data.materials.new('MALL_' + key)
        mat.use_nodes = True
        shader = mat.node_tree.nodes.get('Principled BSDF')
        shader.inputs['Roughness'].default_value = .6
        for suffix in ('BaseColor', 'Normal', 'Mask'):
            path = os.path.join(OUT, 'Textures', 'MALL_' + key + '_' + suffix + '.png')
            if not os.path.exists(path): continue
            tex = mat.node_tree.nodes.new('ShaderNodeTexImage')
            tex.image = bpy.data.images.load(path, check_existing=True)
            tex.image.colorspace_settings.name = 'sRGB' if suffix == 'BaseColor' else 'Non-Color'
            if suffix == 'BaseColor': mat.node_tree.links.new(tex.outputs['Color'], shader.inputs['Base Color'])
            if suffix == 'Normal':
                normal = mat.node_tree.nodes.new('ShaderNodeNormalMap')
                mat.node_tree.links.new(tex.outputs['Color'], normal.inputs['Color'])
                mat.node_tree.links.new(normal.outputs['Normal'], shader.inputs['Normal'])
        MATERIALS[key] = mat

def begin():
    global OBJECTS
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    OBJECTS = []

def register(obj, key):
    obj.data.materials.append(MATERIALS[key]); OBJECTS.append(obj)
    return obj

def planar_uv(obj, key):
    """Metre-space UVs keep tiles aligned; trims use longitudinal strips."""
    uv = obj.data.uv_layers.new(name='UVMap') if not obj.data.uv_layers else obj.data.uv_layers.active
    metres = UV_METRES.get(key, 1)
    spans=[max(v.co[a] for v in obj.data.vertices)-min(v.co[a] for v in obj.data.vertices) for a in range(3)]
    for polygon in obj.data.polygons:
        axis = max(range(3), key=lambda a: abs(polygon.normal[a]))
        axes = [(1, 2), (0, 2), (0, 1)][axis]
        for loop in polygon.loop_indices:
            p = obj.data.vertices[obj.data.loops[loop].vertex_index].co
            if key=='StorefrontTrim':
                # Select an authored trim strip, keeping its long direction in metre-space.
                longest=max(axes,key=lambda a:spans[a]); across=axes[0] if axes[1]==longest else axes[1]
                brass=any(word in obj.name for word in ('Brass','Rim','Divider'))
                start,spread=(.80,.17) if brass else (.14,.46)
                uv.data[loop].uv=(p[longest]/metres+.5,start+spread*(p[across]/max(spans[across],.001)+.5))
            else:uv.data[loop].uv = (p[axes[0]] / metres + .5, p[axes[1]] / metres + .5)

def box(name, p, s, key='Concrete', bevel=.02, rot=0, detail=2):
    bpy.ops.mesh.primitive_cube_add(size=1, location=xyz(p))
    obj = bpy.context.object; obj.name = name
    obj.dimensions = xyz(s); obj.rotation_euler.z = -math.radians(rot)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel and min(s) > .014:
        mod = obj.modifiers.new('RoundedManufacturedEdge', 'BEVEL')
        mod.width = min(bevel, min(s) * .18); mod.segments = detail
        mod.harden_normals = True
        bpy.ops.object.modifier_apply(modifier=mod.name)
    planar_uv(obj, key)
    return register(obj, key)

def mesh(name, vertices, faces, key='Concrete'):
    data = bpy.data.meshes.new(name)
    data.from_pydata([xyz(v) for v in vertices], [], [tuple(reversed(f)) for f in faces])
    data.update(); obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj); planar_uv(obj, key)
    return register(obj, key)

def rod(name, a, b, radius=.025, key='FixtureTrim', sides=12):
    va, vb = Vector(xyz(a)), Vector(xyz(b)); delta = vb - va
    bpy.ops.mesh.primitive_cylinder_add(vertices=sides, radius=radius, depth=delta.length, location=(va + vb) * .5)
    obj = bpy.context.object; obj.name = name
    obj.rotation_euler = delta.to_track_quat('Z', 'Y').to_euler()
    for poly in obj.data.polygons: poly.use_smooth = len(poly.vertices) == 4
    planar_uv(obj, key); return register(obj, key)

def cylinder(name, p, radius, height, key='Concrete', sides=32):
    return rod(name, (p[0], p[1] - height / 2, p[2]), (p[0], p[1] + height / 2, p[2]), radius, key, sides)

def ring(name, p, outer, inner, height, key='Concrete', sides=32):
    verts = [(p[0] + math.cos(i * math.tau / sides) * r, y,
              p[2] + math.sin(i * math.tau / sides) * r)
             for y in (p[1] - height / 2, p[1] + height / 2)
             for r in (outer, inner) for i in range(sides)]
    faces = []
    for i in range(sides):
        j = (i + 1) % sides; n = sides
        faces += [(i,j,2*n+j,2*n+i),(n+j,n+i,3*n+i,3*n+j),
                  (2*n+i,2*n+j,3*n+j,3*n+i),(j,i,n+i,n+j)]
    return mesh(name, verts, faces, key)

def join_objects(objects, name, parent=None):
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects: obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    if len(objects)>1:bpy.ops.object.join()
    obj = bpy.context.object; obj.name = name
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    bpy.context.scene.cursor.location = (0, 0, 0)
    bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    obj.parent = parent
    # The positive Unity-to-Blender coordinate swap reverses handedness.
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.object.mode_set(mode='OBJECT')
    return obj

def group(name, parent=None):
    obj = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(obj); obj.parent = parent
    return obj

def export(name, maker=None, lod=True):
    """Keep hierarchy and names stable so Unity can construct explicit LODGroups."""
    global OBJECTS
    if maker:
        begin(); root = group(name)
        for level in range(3 if lod else 1):
            OBJECTS = []; maker(level)
            holder = group('LOD' + str(level), root) if lod else root
            separate=[o for o in OBJECTS if o.get('SeparateRenderer')]
            for obj in separate: obj.parent=root
            join_objects([o for o in OBJECTS if o not in separate], name + '_LOD' + str(level), holder)
    else:
        root = group(name); join_objects(OBJECTS, name + '_LOD0', group('LOD0', root))
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, name + '.fbx'), use_selection=True,
        object_types={'MESH', 'EMPTY'}, axis_forward='-Z', axis_up='Y', apply_unit_scale=True,
        bake_space_transform=False, use_mesh_modifiers=True, mesh_smooth_type='FACE',
        add_leaf_bones=False, path_mode='RELATIVE', bake_anim=False)
    triangles = sum(len(o.data.loop_triangles) for o in bpy.context.scene.objects if o.type == 'MESH')
    print('MERIDIAN_EXPORTED', name, 'objects', len(bpy.context.scene.objects), 'triangles', triangles)

def export_chunks(name, chunks):
    global OBJECTS
    begin(); root = group(name)
    for index, make in enumerate(chunks):
        OBJECTS = []; make()
        obj=join_objects(OBJECTS, 'Chunk_' + str(index).zfill(2), root)
        corners=[obj.matrix_world @ Vector(p) for p in obj.bound_box]
        bpy.context.scene.cursor.location=sum(corners,Vector())/8
        bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, name + '.fbx'), use_selection=True,
        object_types={'MESH', 'EMPTY'}, axis_forward='-Z', axis_up='Y', apply_unit_scale=True,
        bake_space_transform=False, use_mesh_modifiers=True, mesh_smooth_type='FACE',
        add_leaf_bones=False, path_mode='RELATIVE', bake_anim=False)
    print('MERIDIAN_EXPORTED', name, 'chunks', len(chunks))
