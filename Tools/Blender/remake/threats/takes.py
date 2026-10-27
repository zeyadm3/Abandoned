"""The threat rig and its five takes, kept identical in bone names, lengths and frame rate to Tools/Blender/threats.py
(the game's ThreatMotion enum, ThreatAnimationSync and ThreatModelImporter's clip rules depend on them):
Idle 72, Walk 36, Chase 24, Attack 30, Special 60 frames at 30 fps. The motion is reworked for weight and menace:
eased keys instead of linear, overlapping secondary motion, and each monster's own Special.
"""
import math

import bpy
from mathutils import Vector

from ..core import kit

TAKES = [('Idle', 72), ('Walk', 36), ('Chase', 24), ('Attack', 30), ('Special', 60)]


def rig(name, height, width, leg, arm, body_y=0.0):
    """The same bones, at the same places, as threats.py rig_begin (root, spine, neck, head, arm/forearm, leg/shin)."""
    data = bpy.data.armatures.new(name + '_Skeleton')
    ob = bpy.data.objects.new(name + '_Rig', data)
    kit.link(ob)
    bpy.context.view_layer.update()
    for o in bpy.context.view_layer.objects:
        if o is not None:
            o.select_set(False)
    ob.select_set(True)
    bpy.context.view_layer.objects.active = ob
    bpy.ops.object.mode_set(mode='EDIT')
    segs = {}

    def bone(n, head, tail, parent=None):
        eb = data.edit_bones.new(n)
        eb.head, eb.tail = head, tail
        if parent:
            eb.parent = data.edit_bones[parent]
        segs[n] = (Vector(head), Vector(tail))

    y = body_y
    bone('root', (0, 0, 0), (0, 0, .25))
    bone('spine', (0, y, leg), (0, y, height * .77), 'root')
    bone('neck', (0, y, height * .77), (0, y, height * .89), 'spine')
    bone('head', (0, y, height * .89), (0, y, height), 'neck')
    for suffix, sign in (('L', -1), ('R', 1)):
        shoulder = (sign * width, y, height * .76)
        elbow = (sign * (width + .04), y - .06, height * .76 - arm * .5)
        hand = (sign * (width + .08), y - .12, height * .76 - arm)
        bone('arm' + suffix, shoulder, elbow, 'spine')
        bone('forearm' + suffix, elbow, hand, 'arm' + suffix)
        hip = (sign * width * .43, y, leg)
        knee = (sign * width * .46, y + .03, leg * .48)
        foot = (sign * width * .50, y - .12, .1)
        bone('leg' + suffix, hip, knee, 'root')
        bone('shin' + suffix, knee, foot, 'leg' + suffix)
    bpy.ops.object.mode_set(mode='OBJECT')
    for p in ob.pose.bones:
        p.rotation_mode = 'XYZ'
    return ob, segs


def _ease(u):
    return u * u * (3 - 2 * u)


def animate(rig_ob, name):
    """Bake the five takes as separate actions (FBX takes)."""
    rig_ob.animation_data_create()
    actions = []
    pb = rig_ob.pose.bones
    for anim, length in TAKES:
        act = bpy.data.actions.new(anim)
        act.use_fake_user = True
        rig_ob.animation_data.action = act
        frames = list(range(1, length + 1, 2))
        if frames[-1] != length:
            frames.append(length)
        for f in frames:
            u = (f - 1) / (length - 1)
            ph = u * math.tau
            for p in pb:
                p.rotation_euler = (0, 0, 0)
                p.location = (0, 0, 0)
            s = math.sin(ph)
            if anim == 'Idle':
                breathe = math.sin(ph * 2)
                pb['spine'].rotation_euler = (.03 * breathe, s * .03, 0)
                pb['neck'].rotation_euler = (-.02 * breathe, 0, math.sin(ph + 1) * .06)
                pb['head'].rotation_euler = (math.sin(ph * 3) * .03, math.sin(ph * 2 + .5) * .05, math.sin(ph * 1.5) * .1)
                for side, sign in (('L', 1), ('R', -1)):
                    pb['arm' + side].rotation_euler = (math.sin(ph + sign) * .04, 0, sign * .04 * breathe)
                    pb['forearm' + side].rotation_euler = (max(0, math.sin(ph * 6 + sign)) * .1 - .1, 0, 0)
                pb['root'].location = (0, 0, -.01 * (1 + breathe))
            elif anim in ('Walk', 'Chase'):
                fast = anim == 'Chase'
                stride = .7 if fast else .38
                for side, sign in (('L', 1), ('R', -1)):
                    leg = s * stride * sign
                    pb['leg' + side].rotation_euler = (leg, 0, 0)
                    pb['shin' + side].rotation_euler = (max(0, -s * sign) * stride * 1.1 + .05, 0, 0)
                    pb['arm' + side].rotation_euler = (-s * stride * .55 * sign - (.45 if fast else .1), 0, sign * .08)
                    pb['forearm' + side].rotation_euler = (-(.5 if fast else .2) - max(0, s * sign) * .2, 0, math.sin(ph * 2 + sign) * .08)
                pb['spine'].rotation_euler = ((.22 if fast else .05) + abs(s) * .04, s * .06, s * .05)
                pb['neck'].rotation_euler = (-(.12 if fast else .03), 0, -s * .05)
                pb['root'].location = (0, 0, abs(math.cos(ph)) * (.06 if fast else .03) - .02)
                lurch = .12 if name in ('BlindOne', 'Thing') else .04
                pb['head'].rotation_euler = (0, math.sin(ph * 2) * .04, math.sin(ph * 5) * lurch)
            elif anim == 'Attack':
                wind = _ease(min(1, u / .35))
                strike = _ease(max(0, min(1, (u - .35) / .2)))
                recover = _ease(max(0, (u - .6) / .4))
                k = wind * (1 - recover)
                lunge = strike * (1 - recover)
                pb['spine'].rotation_euler = (-.25 * k + .75 * lunge, 0, .12 * k)
                pb['neck'].rotation_euler = (.2 * lunge, 0, 0)
                pb['head'].rotation_euler = (.15 * lunge, math.sin(ph * 3) * .2 * lunge, 0)
                for side in ('L', 'R'):
                    pb['arm' + side].rotation_euler = (-1.9 * k + .4 * lunge, 0, 0)
                    pb['forearm' + side].rotation_euler = (-.9 * k + .7 * lunge, 0, 0)
                pb['root'].location = (0, -.25 * lunge, -.06 * lunge)
            else:
                special(pb, name, u, ph, s)
            for p in pb:
                p.keyframe_insert('rotation_euler', frame=f)
                p.keyframe_insert('location', frame=f)
        for fc in act.fcurves:
            for kp in fc.keyframe_points:
                kp.interpolation = 'BEZIER'
        actions.append(act)
    rig_ob.animation_data.action = actions[0]
    sc = bpy.context.scene
    sc.render.fps = 30
    sc.frame_start, sc.frame_end = 1, 72
    return actions


def special(pb, name, u, ph, s):
    """Each monster's own Special take (what it does when it hears, finds or commits)."""
    if name == 'BlindOne':  # listening: head cocked, slow sweep, fingers twitching
        pb['neck'].rotation_euler = (.3, 0, s * .2)
        pb['head'].rotation_euler = (0, s * .5, math.sin(ph * 2) * .25)
        for side, sign in (('L', 1), ('R', -1)):
            pb['forearm' + side].rotation_euler = (-.3 + math.sin(ph * 9 + sign) * .06, 0, 0)
    elif name == 'Stalker':  # freezes, head tilts too far, one hand creeping
        pb['head'].rotation_euler = (0, .55 + math.sin(ph * 4) * .03, 0)
        pb['armR'].rotation_euler = (-.5 * (1 - math.cos(ph)) * .5, 0, 0)
        pb['forearmR'].rotation_euler = (-.3, 0, math.sin(ph * 9) * .06)
    elif name == 'Collector':  # rummaging in its sack
        for side in ('L', 'R'):
            pb['arm' + side].rotation_euler = (-1.2 + math.sin(ph * 3) * .25, 0, 0)
            pb['forearm' + side].rotation_euler = (-.8 + math.sin(ph * 6) * .2, 0, 0)
        pb['spine'].rotation_euler = (.35 + abs(s) * .1, 0, s * .1)
    elif name in ('Hunter', 'LastHunter'):  # pounds the ground with both fists
        hit = abs(math.sin(ph * 2))
        pb['spine'].rotation_euler = (.6 * hit, 0, 0)
        for side in ('L', 'R'):
            pb['arm' + side].rotation_euler = (-2.2 * (1 - hit) + .3, 0, 0)
            pb['forearm' + side].rotation_euler = (-.4, 0, 0)
        pb['root'].location = (0, 0, -.12 * hit)
    elif name == 'Weight':  # heaves its mass, sinking and rising
        pb['root'].location = (0, 0, -.3 * abs(s))
        pb['spine'].rotation_euler = (.1 * abs(s), s * .12, 0)
        pb['head'].rotation_euler = (-.2 * abs(s), 0, 0)
    elif name == 'Thing':  # the head turns all the way round and back
        pb['head'].rotation_euler = (0, s * 1.4, 0)
        pb['neck'].rotation_euler = (0, 0, math.sin(ph * 3) * .2)
        pb['spine'].rotation_euler = (math.sin(ph * 2) * .15, 0, 0)
    elif name == 'Crawlers':  # skitter in place
        pb['root'].rotation_euler = (0, 0, s * .35)
        pb['armL'].rotation_euler = (math.sin(ph * 4) * .5, 0, 0)
        pb['armR'].rotation_euler = (-math.sin(ph * 4) * .5, 0, 0)
        pb['legL'].rotation_euler = (-math.sin(ph * 4) * .4, 0, 0)
        pb['legR'].rotation_euler = (math.sin(ph * 4) * .4, 0, 0)
    else:
        pb['spine'].rotation_euler = (-.25 * abs(s), 0, 0)
        pb['head'].rotation_euler = (0, 0, s * .3)
