"""Armatures for the remake.

CREW: one skeleton shared by every playable worker, every outfit and the HR manager. Its bones sit exactly where
Editor/Art/WorkerVisualBuilder puts WorkerRig's joints (Unity space, eyes at 1.65 m), with the same names, so
Unity can bind a SkinnedMeshRenderer straight to the transforms WorkerPresentation already poses:

    Body (hips root, 0.95)  -> Torso (+0.15) -> Head (+0.42 => 1.52)
                                             -> ArmL / ArmR (+-0.19, +0.315 => 1.415) -> ElbowL / ElbowR (-0.305)
                            -> LegL / LegR (+-0.102, -0.05 => 0.90) -> KneeL / KneeR (-0.40 => 0.50)

Unity's worker faces +Z with its left at -X; in Blender we build facing +Y with the left at -X, so positions map
straight across (Unity (x, y, z) = Blender (x, z, y) for points on the model).

Monsters get their own armatures from a spec (bone name, head, tail, parent).
"""
import bpy
from mathutils import Vector

from . import kit

# name: (head position in Blender space, tail position, parent)
CREW = {
    'Body':   ((0, 0, .95), (0, 0, 1.08), None),
    'Torso':  ((0, 0, 1.10), (0, 0, 1.50), 'Body'),
    'Head':   ((0, 0, 1.52), (0, 0, 1.77), 'Torso'),
    'ArmL':   ((-.19, 0, 1.415), (-.19, 0, 1.11), 'Torso'),
    'ElbowL': ((-.19, 0, 1.11), (-.19, 0, .84), 'ArmL'),
    'ArmR':   ((.19, 0, 1.415), (.19, 0, 1.11), 'Torso'),
    'ElbowR': ((.19, 0, 1.11), (.19, 0, .84), 'ArmR'),
    'LegL':   ((-.102, 0, .90), (-.102, 0, .50), 'Body'),
    'KneeL':  ((-.102, 0, .50), (-.102, 0, .05), 'LegL'),
    'LegR':   ((.102, 0, .90), (.102, 0, .50), 'Body'),
    'KneeR':  ((.102, 0, .50), (.102, 0, .05), 'LegR'),
}

# Sockets for hats and accessories (Blender space, parented to a bone). Characters may override them.
CREW_SOCKETS = {
    'Socket_Crown': ((0, .0, 1.77), 'Head'),
    'Socket_Eyes':  ((0, .095, 1.65), 'Head'),
    'Socket_Mouth': ((0, .1, 1.575), 'Head'),
    'Socket_Neck':  ((0, .02, 1.50), 'Torso'),
    'Socket_Chest': ((0, .14, 1.30), 'Torso'),
    'Socket_Back':  ((0, -.16, 1.28), 'Torso'),
    'Socket_Belt':  ((0, .0, .98), 'Body'),
}


def build(spec, name='Rig', col=None):
    """Create an armature from {bone: (head, tail, parent)}; bones keep zero roll so Unity sees clean axes."""
    arm = bpy.data.armatures.new(name)
    ob = bpy.data.objects.new(name, arm)
    kit.link(ob, col)
    for o in bpy.context.view_layer.objects:
        o.select_set(False)
    ob.select_set(True)
    bpy.context.view_layer.objects.active = ob
    bpy.ops.object.mode_set(mode='EDIT')
    bones = {}
    for bone, (head, tail, parent) in spec.items():
        b = arm.edit_bones.new(bone)
        b.head, b.tail = Vector(head), Vector(tail)
        b.roll = 0
        bones[bone] = b
    for bone, (_, _, parent) in spec.items():
        if parent:
            bones[bone].parent = bones[parent]
            bones[bone].use_connect = False
    bpy.ops.object.mode_set(mode='OBJECT')
    return ob


def crew(col=None):
    return build(CREW, 'CrewRig', col)


def skin(meshes, armature, auto=True, groups=None):
    """Parent meshes to the armature with an Armature modifier. auto=True: Blender's heat weights;
    groups: {mesh: bone} to rigidly weight a whole mesh to one bone (buttons, boots, helmets)."""
    groups = groups or {}
    for me in meshes:
        me.parent = armature
        me.matrix_parent_inverse = armature.matrix_world.inverted()
        mod = me.modifiers.get('Armature') or me.modifiers.new('Armature', 'ARMATURE')
        mod.object = armature
        if me in groups:
            vg = me.vertex_groups.get(groups[me]) or me.vertex_groups.new(name=groups[me])
            vg.add(range(len(me.data.vertices)), 1.0, 'REPLACE')
    auto_meshes = [m for m in meshes if m not in groups]
    if auto and auto_meshes:
        for o in bpy.context.view_layer.objects:
            o.select_set(False)
        for m in auto_meshes:
            m.select_set(True)
        armature.select_set(True)
        bpy.context.view_layer.objects.active = armature
        try:
            bpy.ops.object.parent_set(type='ARMATURE_AUTO')
        except RuntimeError as e:  # heat weighting fails on non-manifold meshes; fall back to envelope
            print('[remake-rig] auto weights failed, using envelopes:', e)
            bpy.ops.object.parent_set(type='ARMATURE_ENVELOPE')
    return meshes


def weight_by_nearest(me, armature, bones=None, falloff=.06):
    """Distance-based weights to bone segments, smooth across joints. Robust for clothing layers and cards
    where heat weighting fails; bones restricts the candidates."""
    arm = armature.data
    segs = []
    for b in arm.bones:
        if bones and b.name not in bones:
            continue
        segs.append((b.name, armature.matrix_world @ b.head_local, armature.matrix_world @ b.tail_local))
    for name, _, _ in segs:
        if me.vertex_groups.get(name) is None:
            me.vertex_groups.new(name=name)
    mw = me.matrix_world
    for v in me.data.vertices:
        p = mw @ v.co
        ds = []
        for name, h, t in segs:
            d = t - h
            k = max(0.0, min(1.0, (p - h).dot(d) / max(d.length_squared, 1e-9)))
            ds.append((name, (p - (h + d * k)).length))
        ds.sort(key=lambda x: x[1])
        best = ds[0][1]
        ws = [(n, 1.0 / (1e-4 + (d - best) / falloff + 1) ** 4) for n, d in ds[:3] if d - best < falloff * 2]
        tot = sum(w for _, w in ws)
        for n, w in ws:
            me.vertex_groups[n].add([v.index], w / tot, 'REPLACE')
    mod = me.modifiers.get('Armature') or me.modifiers.new('Armature', 'ARMATURE')
    mod.object = armature
    me.parent = armature
    me.matrix_parent_inverse = armature.matrix_world.inverted()


def sockets(armature, spec=None, col=None):
    """Empties parented to bones (exported as transforms under the bone in Unity)."""
    out = []
    for name, (pos, bone) in (spec or CREW_SOCKETS).items():
        e = kit.empty(name, pos, col=col, size=.03)
        e.parent = armature
        e.parent_type = 'BONE'
        e.parent_bone = bone
        b = armature.data.bones[bone]
        e.matrix_world = armature.matrix_world @ __import__('mathutils').Matrix.Translation(Vector(pos))
        out.append(e)
        _ = b
    return out


def pose_action(armature, name, keys, fps=30):
    """Make an action from {frame: {bone: (rx, ry, rz) degrees or ((rx,ry,rz),(lx,ly,lz))}}; returns the action."""
    import math
    armature.animation_data_create()
    act = bpy.data.actions.new(name)
    act.use_fake_user = True
    armature.animation_data.action = act
    for frame, bones in sorted(keys.items()):
        for bone, val in bones.items():
            pb = armature.pose.bones[bone]
            pb.rotation_mode = 'XYZ'
            rot, loc = (val, None) if not isinstance(val[0], (tuple, list)) else val
            pb.rotation_euler = [math.radians(a) for a in rot]
            pb.keyframe_insert('rotation_euler', frame=frame, group=bone)
            if loc is not None:
                pb.location = loc
                pb.keyframe_insert('location', frame=frame, group=bone)
    for fc in act.fcurves:
        for kp in fc.keyframe_points:
            kp.interpolation = 'BEZIER'
            kp.easing = 'EASE_IN_OUT'
    bpy.context.scene.render.fps = fps
    return act
