"""Export the crew redesign to Unity: one FBX per rig joint, baked face and eye textures, first-person gloves.

/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup \
    --python Tools/Blender/crew/crew_export.py -- "$PWD"

Writes Game/Assets/_Project/Art/Custom/Characters/Crew/. Each Crew_<Part>.fbx has its origin at that part's
pivot and one object per Unity material key (<Part>_<Key>); Editor/Art/WorkerVisualBuilder assigns the
project's character materials by key. Axes follow Tools/Blender/environment.py (Unity x,y,z = Blender x,z,y),
so the model built facing -Y is turned 180 degrees first.
"""
import math
import os
import sys
import bmesh
import bpy
from mathutils import Matrix, Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import crew_geo as G  # noqa: E402
import crew_materials as CM  # noqa: E402
import crew_body as B  # noqa: E402

REPO = sys.argv[sys.argv.index('--') + 1] if '--' in sys.argv else os.getcwd()
OUT = os.path.join(REPO, 'Game/Assets/_Project/Art/Custom/Characters/Crew')

# Rig joint -> exported part, and the pivot each part's origin sits on (rest pose, Blender space).
PARTS = {'Body': 'Hips', 'Torso': 'Torso', 'Head': 'Head', 'ArmL': 'UpperArmL', 'ArmR': 'UpperArmR',
         'ElbowL': 'ForearmL', 'ElbowR': 'ForearmR', 'LegL': 'ThighL', 'LegR': 'ThighR', 'KneeL': 'ShinL', 'KneeR': 'ShinR'}
# Blender material -> Unity key (Editor/Art/WorkerVisualBuilder.Materials).
KEYS = {'CrewJacket': 'Suit', 'CrewJacketDark': 'Trim', 'CrewTrim': 'Trim', 'CrewVest': 'Vest', 'CrewWebbing': 'Webbing',
        'CrewTrousers': 'Trousers', 'CrewPatch': 'Rubber', 'CrewTape': 'Tape', 'CrewRubber': 'Rubber', 'CrewKneePad': 'Rubber',
        'CrewPlastic': 'Rubber', 'CrewGlove': 'Glove', 'CrewGlovePalm': 'GlovePalm', 'CrewBoot': 'Boot', 'CrewSole': 'Rubber',
        'CrewSteel': 'Steel', 'CrewLace': 'Lace', 'CrewSkin': 'Skin', 'CrewSkinBake': 'Head', 'CrewEye': 'Eye', 'CrewLash': 'Rubber',
        'CrewChemLight': 'ChemLight'}
SKIP = ('Hair', 'Cornea', 'Caruncle', 'CrewNumber')
# Triangle budget per part (about 30k for the whole worker).
BUDGET = {'Head': 6000, 'Torso': 7000, 'Hips': 1600, 'UpperArmL': 1400, 'UpperArmR': 1400, 'ForearmL': 3000, 'ForearmR': 3000,
          'ThighL': 1100, 'ThighR': 1100, 'ShinL': 2600, 'ShinR': 2600}
GLOVE = ('GloveCuff', 'GloveStrap', 'Hand', 'KnucklePad', 'Finger', 'Thumb')
TURN = Matrix.Rotation(math.pi, 4, 'Z')


def setup():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    G.COLL = None
    sc = bpy.context.scene
    sc.render.engine = 'CYCLES'
    sc.cycles.samples = 4
    sc.cycles.use_denoising = False
    prefs = bpy.context.preferences.addons['cycles'].preferences
    prefs.compute_device_type = 'METAL'
    prefs.get_devices()
    for d in prefs.devices:
        d.use = True
    sc.cycles.device = 'GPU'


def joint_of(ob):
    p = ob.parent
    while p is not None and p.name.split('.')[0] not in PARTS:
        p = p.parent
    return p


def bake(ob, name, size):
    """Bake the object's procedural base colour into an image saved next to the FBX."""
    img = bpy.data.images.new(name, size[0], size[1])
    mat = ob.data.materials[0]
    node = mat.node_tree.nodes.new('ShaderNodeTexImage')
    node.image = img
    mat.node_tree.nodes.active = node
    bpy.ops.object.select_all(action='DESELECT')
    ob.select_set(True)
    bpy.context.view_layer.objects.active = ob
    bpy.ops.object.bake(type='DIFFUSE', pass_filter={'COLOR'}, margin=8, use_clear=True)
    img.filepath_raw = os.path.join(OUT, name + '.png')
    img.file_format = 'PNG'
    img.save()
    mat.node_tree.nodes.remove(node)
    print('[crew-export] baked', name)


def gather(objs, origin, turn):
    """Evaluated meshes (modifiers applied) moved so origin is (0,0,0), turned into the kit's axes, merged.
    Returns the merged bmesh and its material list (face material indices point into it)."""
    dg = bpy.context.evaluated_depsgraph_get()
    bm, mats = bmesh.new(), []
    for ob in objs:
        ev = ob.evaluated_get(dg)
        me = ev.to_mesh()
        m = turn @ Matrix.Translation(-origin) @ ob.matrix_world
        me.transform(m)
        if m.determinant() < 0:
            me.flip_normals()
        remap = []
        for mat in ev.material_slots:
            if mat.material not in mats:
                mats.append(mat.material)
            remap.append(mats.index(mat.material))
        for poly in me.polygons:
            poly.material_index = remap[poly.material_index] if remap else 0
        bm.from_mesh(me)
        ev.to_mesh_clear()
    return bm, mats


def emit(name, bm, budget=None):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    me.shade_smooth()
    ob = bpy.data.objects.new(name, me)
    bpy.context.scene.collection.objects.link(ob)
    if budget:
        tris = sum(len(p.vertices) - 2 for p in me.polygons)
        if tris > budget:
            d = ob.modifiers.new('Budget', 'DECIMATE')
            d.ratio = budget / tris
            d.delimit = {'UV', 'NORMAL'}
            bpy.context.view_layer.objects.active = ob
            bpy.ops.object.modifier_apply(modifier=d.name)
    return ob


def export(objs, path):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={'MESH'}, axis_forward='-Z', axis_up='Y',
                             apply_unit_scale=True, bake_space_transform=True, use_mesh_modifiers=False, mesh_smooth_type='FACE',
                             add_leaf_bones=False, path_mode='STRIP', bake_anim=False)
    print('[crew-export] wrote', os.path.relpath(path, REPO))


def main():
    setup()
    os.makedirs(OUT, exist_ok=True)
    M = CM.library('orange')
    # The face bake paints the crew cut, beard and brows straight onto the skin.
    M['Skin'] = CM.skin('CrewSkinBake', under_colour=(0.045, 0.034, 0.027), under_amount=.97)
    neck_skin = CM.skin('CrewSkin')
    B.FONT = None
    j = B.build(M, 1)
    bpy.context.view_layer.update()
    for ob in list(bpy.data.objects):
        if ob.type == 'MESH' and ob.name.split('.')[0] in ('Neck', 'Ear') and ob.data.materials and ob.data.materials[0].name == 'CrewSkinBake':
            ob.data.materials[0] = neck_skin
    head = next(o for o in bpy.data.objects if o.type == 'MESH' and o.name.split('.')[0] == 'Head')
    bake(head, 'CrewHead_BaseColor', (2048, 1024))
    eye = next(o for o in bpy.data.objects if o.name.startswith('Eyeball'))
    bake(eye, 'CrewEye_BaseColor', (256, 256))

    groups = {}
    gloves = {'L': [], 'R': []}
    for ob in bpy.data.objects:
        if ob.type != 'MESH' or ob.name.split('.')[0] in SKIP:
            continue
        piv = joint_of(ob)
        if piv is None:
            continue
        part = PARTS[piv.name.split('.')[0]]
        for i, mat in enumerate(ob.data.materials):
            key = KEYS.get(mat.name.split('.')[0])
            if key is None:
                raise RuntimeError(f'no Unity key for material {mat.name} on {ob.name}')
        groups.setdefault(part, []).append(ob)
        if part.startswith('Forearm') and ob.name.split('.')[0] in GLOVE:
            gloves[part[-1]].append(ob)

    for part, objs in groups.items():
        piv = next(p for n, p in j.items() if p.name.split('.')[0] in PARTS and PARTS[p.name.split('.')[0]] == part)
        origin = G.parent_world(piv)
        merged, mats = gather(objs, origin, TURN)
        out = split(merged, mats, part, BUDGET.get(part))
        export(out, os.path.join(OUT, f'Crew_{part}.fbx'))
        for o in out:
            bpy.data.objects.remove(o)

    # First-person gloves: origin at the wrist, fingers along -Y and palm toward +Z (WorkerViewModelBuilder's frame).
    for side, objs in gloves.items():
        s = 1 if side == 'L' else -1
        wrist = Vector((s * (B.ARM_X - .29 * .015), 0, .86))
        turn = Matrix.Rotation(math.radians(-90 * s), 4, 'Z')
        merged, mats = gather(objs, wrist, turn)
        out = split(merged, mats, 'Glove' + side, 3000)
        export(out, os.path.join(OUT, f'Crew_Glove{side}.fbx'))
        for o in out:
            bpy.data.objects.remove(o)


def split(bm, mats, part, budget):
    """One object per Unity material key, each decimated in proportion to the part's budget."""
    me = bpy.data.meshes.new('tmp')
    bm.to_mesh(me)
    bm.free()
    by_key = {}
    for poly in me.polygons:
        by_key.setdefault(KEYS[mats[poly.material_index].name.split('.')[0]], []).append(poly.index)
    total = len(me.polygons)
    out = []
    for key, faces in by_key.items():
        b = bmesh.new()
        b.from_mesh(me)
        keep = set(faces)
        bmesh.ops.delete(b, geom=[f for f in b.faces if f.index not in keep], context='FACES')
        share = budget * len(faces) / total if budget else None
        ob = emit(f'{part}_{key}', b, int(share) if share else None)
        ob.data.materials.clear()
        out.append(ob)
    bpy.data.meshes.remove(me)
    return out


if __name__ == '__main__':
    main()
