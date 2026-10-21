"""Crew redesign preview renders (art review only; nothing here touches the Unity project).

/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup \
    --python Tools/Blender/crew/crew.py -- <repo> <out dir> [turn portrait lineup dark] [--fast]
"""
import math
import os
import sys
import bpy
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import crew_geo as G  # noqa: E402
import crew_materials as CM  # noqa: E402
import crew_body as B  # noqa: E402
import crew_hats as H  # noqa: E402

ARGS = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
REPO = ARGS[0] if ARGS else os.getcwd()
OUT = ARGS[1] if len(ARGS) > 1 else os.path.join(REPO, 'Docs/renders/crew-redesign')
SHOTS = [a for a in ARGS[2:] if not a.startswith('--')] or ['turn', 'face', 'portrait', 'lineup', 'dark']
FAST = '--fast' in ARGS
ART = os.path.join(REPO, 'Game/Assets/_Project/Art')


def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    G.COLL = None
    sc = bpy.context.scene
    sc.render.engine = 'CYCLES'
    prefs = bpy.context.preferences.addons['cycles'].preferences
    prefs.compute_device_type = 'METAL'
    prefs.get_devices()
    for d in prefs.devices:
        d.use = True
    sc.cycles.device = 'GPU'
    sc.cycles.samples = 48 if FAST else 256
    sc.cycles.use_adaptive_sampling = True
    sc.cycles.use_denoising = True
    sc.cycles.max_bounces = 8
    sc.view_settings.view_transform = 'AgX'
    try:
        sc.view_settings.look = 'AgX - Punchy'
    except TypeError:
        pass
    B.FONT = bpy.data.fonts.load(os.path.join(ART, 'ThirdParty/Fonts/SairaStencilOne/SairaStencilOne-Regular.ttf'))
    return sc


def aim(ob, target):
    ob.rotation_euler = (Vector(target) - ob.location).to_track_quat('-Z', 'Y').to_euler()


def camera(loc, target, lens=50.0, shift=(0, 0)):
    cam = bpy.data.objects.new('Camera', bpy.data.cameras.new('Camera'))
    bpy.context.scene.collection.objects.link(cam)
    cam.location = loc
    cam.data.lens = lens
    cam.data.shift_x, cam.data.shift_y = shift
    aim(cam, target)
    bpy.context.scene.camera = cam
    return cam


def light(name, loc, target, power, colour=(1, 1, 1), size=1.0, size_y=None, kind='AREA', spot=None):
    data = bpy.data.lights.new(name, kind)
    data.energy, data.color = power, colour
    if kind == 'AREA':
        data.shape = 'RECTANGLE' if size_y else 'DISK'
        data.size = size
        if size_y:
            data.size_y = size_y
    elif kind == 'SPOT':
        data.spot_size, data.spot_blend = math.radians(spot or 40), .35
        data.shadow_soft_size = size
    ob = bpy.data.objects.new(name, data)
    bpy.context.scene.collection.objects.link(ob)
    ob.location = loc
    aim(ob, target)
    return ob


def world(colour, strength=1.0):
    w = bpy.data.worlds.new('World')
    w.use_nodes = True
    bg = w.node_tree.nodes['Background']
    bg.inputs[0].default_value = (*colour, 1)
    bg.inputs[1].default_value = strength
    bpy.context.scene.world = w


def sweep(colour=(0.032, 0.029, 0.026), rough=.8):
    """A seamless studio cyclorama: floor curving up into a back wall."""
    import bmesh
    bm = bmesh.new()
    prof = [(y, 0.0) for y in (-9, -4, 0, 2.0)]
    for i in range(1, 13):
        a = math.radians(90 * i / 12)
        prof.append((2.0 + 2.5 * math.sin(a), 2.5 - 2.5 * math.cos(a)))
    prof += [(4.5, 6), (4.5, 12)]
    rows = [[bm.verts.new((x, y, z)) for y, z in prof] for x in (-14, 14)]
    for i in range(len(prof) - 1):
        bm.faces.new((rows[0][i], rows[1][i], rows[1][i + 1], rows[0][i + 1]))
    ob = G.finish('Sweep', bm, CM.plain('Backdrop', colour, rough), subsurf=0)
    return ob


def studio(key=1.0):
    world((0.012, 0.011, 0.01), 1.0)
    sweep()
    light('Key', (-3.2, -5.0, 4.2), (0, 0, 1.0), 950 * key, (1.0, .9, .78), 3.0)
    light('Fill', (4.5, -4.5, 1.6), (0, 0, 1.0), 220 * key, (.78, .86, 1.0), 4.0)
    light('RimL', (-3.6, 2.2, 2.6), (0, 0, 1.1), 700 * key, (1.0, .82, .6), .5, 4.0)
    light('RimR', (3.6, 2.4, 2.4), (0, 0, 1.1), 550 * key, (.7, .82, 1.0), .5, 4.0)
    light('Top', (0, -.5, 5.5), (0, 0, 1.0), 160 * key, (1, .96, .9), 2.0)


def crew(tint, number=1, at=(0, 0, 0), yaw=0.0, hat=None):
    coll = bpy.data.collections.new(f'Crew{number}_{tint}')
    bpy.context.scene.collection.children.link(coll)
    M = CM.library(tint)
    j = B.build(M, number, coll)
    j['root'].location = at
    j['root'].rotation_euler.z = math.radians(yaw)
    if hat:
        for part in H.HATS[hat](M):
            G.attach(part, j['head'])
    return j


def pose(j, legs=(0, 0), knees=(0, 0), arms=(0, 0), spread=B.SPREAD, elbows=(-10, -10), torso=0.0, crouch=0.0, head=0.0, twist=0.0):
    """Angles follow WorkerPresentation: negative X swings a limb forward. Pairs are (left, right).
    The elbow is new in this design; the old worker's arms were one rigid piece."""
    j['body'].location.z = .95 - .63 * crouch
    j['torso'].rotation_euler = (math.radians(torso + crouch * 15), 0, math.radians(twist))
    j['head'].rotation_euler = (math.radians(head), 0, 0)
    for i, n in enumerate('LR'):
        s = 1 if n == 'L' else -1
        j['leg' + n].rotation_euler = (math.radians(legs[i] - crouch * 78), math.radians(-s * crouch * 5), 0)
        j['knee' + n].rotation_euler = (math.radians(knees[i] + crouch * 146), 0, 0)
        a = arms[i] if not isinstance(arms[i], tuple) else arms[i][0]
        roll = spread if not isinstance(arms[i], tuple) else arms[i][1]
        j['arm' + n].rotation_euler = (math.radians(a), math.radians(-s * roll), 0)
        j['elbow' + n].rotation_euler = (math.radians(elbows[i]), 0, 0)


def render(name, res=(2560, 1440)):
    sc = bpy.context.scene
    sc.render.resolution_x, sc.render.resolution_y = res
    if FAST:
        sc.render.resolution_percentage = 50
    os.makedirs(OUT, exist_ok=True)
    sc.render.filepath = os.path.join(OUT, name + '.png')
    bpy.ops.render.render(write_still=True)
    print('[crew] wrote', sc.render.filepath)


def shot_turn():
    reset()
    studio()
    for x, yaw in ((-1.62, 0), (-.54, -35), (.54, 90), (1.62, 180)):
        pose(crew('orange', 1, (x, 0, 0), yaw))
    camera((0, -15.5, 1.55), (0, 0, .92), 120)
    render('crew_turnaround')


SHOTS_BY_NAME = {'turn': shot_turn}

if __name__ == '__main__':
    if os.path.exists(os.path.join(os.path.dirname(os.path.abspath(__file__)), 'crew_shots.py')):
        import crew_shots  # noqa: E402
        SHOTS_BY_NAME.update(crew_shots.shots(sys.modules[__name__]))
    for s in SHOTS:
        SHOTS_BY_NAME[s]()
