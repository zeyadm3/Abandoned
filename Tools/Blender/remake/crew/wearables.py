"""Hats and accessories for the wardrobe, remodelled at real size for the 2026 heads.

Origin = the worker's crown (WorkerRig.HeadAnchor, 1.77 m), remake frame (+Y forward, +Z up). The neutral head is
15.5 cm wide, about 20 cm deep, eyes 12 cm below the crown, mouth 19.5 cm below; face pieces sit at those heights,
body pieces at the belt (-0.79), back (-0.49), neck (-0.3) or left shoulder. Characters with smaller heads scale
head pieces by their HatScale in game.

Batch:
  Blender --background --factory-startup --python Tools/Blender/remake/crew/wearables.py -- [ids...] [--preview]
Writes Art/Remake/Characters/Wearables/<Hat|Acc>_<id>.fbx and Textures/Wear_<id>_*.png.
"""
import math
import os
import sys

import bpy
from mathutils import Vector

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(os.path.dirname(_HERE)))
exec(open(os.path.join(os.path.dirname(_HERE), 'boot.py')).read())

from remake.core import bake, export, kit, materials, preview  # noqa: E402

OUT = export.art_dir('Characters', 'Wearables')
TEX = export.art_dir('Characters', 'Textures')
L = None

EYE, MOUTH, FRONT = -.12, -.195, .095  # below the crown; the face's front surface (+Y)


def M(name):
    return L[name]


def mat(ob, name):
    return materials.assign(ob, M(name) if isinstance(name, str) else name)


def skull_shell(name, r=.105, depth=.12, back=1.0, front=1.0, top=.004, cut=0.0):
    """A dome over the skull: profile radius r, `depth` down from the crown; front/back stretch it along Y."""
    prof = []
    for i in range(14):
        t = i / 13
        a = t * math.pi / 2
        prof.append((r * math.sin(a) + .002, top - depth * (1 - math.cos(a)) * 1.0))
    ob = kit.lathe(name, prof, sides=40, cap_bottom=False, cap_top=False)
    for v in ob.data.vertices:
        if v.co.y > 0:
            v.co.y *= front
        else:
            v.co.y *= back
    kit.solidify(ob, .006)
    return ob


def brim(name, r_in, r_out, z, tilt=0.0, front_only=False, thick=.008):
    sides = 40
    import bmesh
    bm = bmesh.new()
    inner, outer = [], []
    for i in range(sides):
        a = i / sides * math.tau
        k = 1.0
        if front_only:
            k = max(0.0, math.sin(a)) ** .6
        ro = r_in + (r_out - r_in) * k
        inner.append(bm.verts.new((math.cos(a) * r_in, math.sin(a) * r_in, z)))
        outer.append(bm.verts.new((math.cos(a) * ro, math.sin(a) * ro, z - tilt * (ro - r_in))))
    for i in range(sides):
        j = (i + 1) % sides
        bm.faces.new((inner[i], inner[j], outer[j], outer[i]))
    ob = kit.from_bmesh(name, bm, smooth=True)
    kit.solidify(ob, thick, 0)
    return ob


# ------------------------------------------------------------------------------------------------------- hats

def hat_hardhat(color='PlasticYellow'):
    shell = mat(skull_shell('Shell', .108, .095, front=1.12, back=1.05, top=.022), color)
    rim = mat(brim('Brim', .106, .124, -.073, .2, thick=.006), color)
    peak = mat(brim('Peak', .11, .16, -.07, .35, front_only=True, thick=.006), color)
    ridge = mat(kit.box('Ridge', (.022, .19, .012), (0, 0, .018), bevel=.005), color)
    band = mat(kit.cylinder('Headband', .1, .02, (0, 0, -.07), sides=36, cap=False), 'Plastic')
    kit.solidify(band, .004)
    return [shell, rim, peak, ridge, band]


def hat_beanie():
    knit = materials.fabric('Knit', (.42, .08, .06), .95, 400)
    body = mat(skull_shell('Beanie', .1, .11, front=1.05, back=1.1, top=.03), knit)
    cuff = mat(kit.cylinder('Cuff', .103, .045, (0, 0, -.085), sides=40, cap=False), knit)
    kit.solidify(cuff, .012)
    return [body, cuff]


def hat_cap(cloth=(.12, .16, .26), name='Cap'):
    c = materials.fabric(name + 'Cloth', cloth, .85, 800)
    crown = mat(skull_shell(name, .1, .085, front=1.08, back=1.05, top=.012), c)
    bill = mat(brim('Bill', .098, .17, -.07, .1, front_only=True, thick=.006), c)
    button = mat(kit.sphere('Button', .008, (0, 0, .016)), c)
    return [crown, bill, button]


def hat_trucker():
    parts = hat_cap((.75, .72, .66), 'Trucker')
    mesh = materials.fabric('TruckerMesh', (.05, .05, .05), .9, 1200)
    back = mat(skull_shell('Mesh', .102, .07, front=.2, back=1.06, top=.0), mesh)
    return parts + [back, mat(kit.box('Patch', (.08, .01, .045), (0, .1, -.03), bevel=.004), 'PaintRed')]


def hat_bucket():
    c = materials.fabric('BucketCloth', (.3, .32, .22), .9, 600)
    body = mat(kit.lathe('Bucket', [(.1, -.07), (.1, -.02), (.09, .02), (.07, .035), (.0, .036)], sides=40, cap_bottom=False), c)
    kit.solidify(body, .005)
    rim = mat(brim('Rim', .1, .155, -.07, .45, thick=.005), c)
    return [body, rim]


def hat_miner():
    parts = hat_hardhat('PaintWhite')
    lamp = mat(kit.cylinder('Lamp', .026, .04, (0, .12, -.01), sides=20, axis='Y'), 'SteelDark')
    lens = mat(kit.cylinder('Lens', .022, .004, (0, .142, -.01), sides=20, axis='Y'), 'EmitWarm')
    bracket = mat(kit.box('Bracket', (.03, .03, .05), (0, .1, -.02), bevel=.004), 'SteelDark')
    return parts + [lamp, lens, bracket]


def hat_welding():
    shield = mat(kit.lathe('Shield', [(.0, -.23), (.07, -.22), (.11, -.15), (.12, -.06), (.11, .0), (.08, .02)], sides=32,
                           cap_bottom=False, cap_top=False), 'PaintBlack')
    for v in shield.data.vertices:
        if v.co.y < -.02:
            v.co.y = -.02 + (v.co.y + .02) * .3
        v.co.y += .03
    kit.solidify(shield, .004)
    window = mat(kit.box('Window', (.1, .01, .05), (0, .145, -.12), bevel=.004), 'GlassDark')
    strap = mat(kit.cylinder('Harness', .1, .025, (0, 0, -.05), sides=32, cap=False), 'Plastic')
    kit.solidify(strap, .004)
    knob = mat(kit.cylinder('Knob', .018, .02, (.105, 0, -.07), sides=16, axis='X'), 'Plastic')
    return [shield, window, strap, knob]


def hat_fedora():
    felt = materials.fabric('Felt', (.22, .18, .14), .8, 200)
    crown = mat(kit.lathe('Crown', [(.095, -.07), (.094, -.01), (.085, .03), (.06, .045), (.0, .03)], sides=40, cap_bottom=False), felt)
    for v in crown.data.vertices:
        v.co.y *= 1.15
        if v.co.z > .025 and abs(v.co.x) < .03:
            v.co.z -= .01
    kit.solidify(crown, .004)
    rim = mat(brim('Rim', .1, .165, -.068, -.12, thick=.005), felt)
    band = mat(kit.cylinder('Band', .097, .02, (0, 0, -.055), sides=40, cap=False), 'LeatherBlack')
    for v in band.data.vertices:
        v.co.y *= 1.15
    kit.solidify(band, .003)
    return [crown, rim, band]


def hat_box():
    box = mat(kit.box('Box', (.24, .24, .2), (0, 0, -.02), bevel=.004), 'Cardboard')
    inner = kit.box('Hollow', (.22, .22, .2), (0, 0, -.06))
    kit.boolean(box, inner)
    tape = mat(kit.box('Tape', (.245, .05, .005), (0, 0, .08)), 'PlasticBeige')
    return [box, tape]


def hat_cone():
    cone = mat(kit.cone('Cone', .1, .015, .32, (0, 0, .1), sides=32), 'PlasticRed')
    stripe = mat(kit.cone('Stripe', .075, .055, .05, (0, 0, .1), sides=32), 'PaintWhite')
    base = mat(kit.box('Base', (.24, .24, .02), (0, 0, -.07), bevel=.01), 'PlasticRed')
    hole = kit.cylinder('Hole', .095, .1, (0, 0, -.07), sides=32)
    kit.boolean(base, hole)
    return [cone, stripe, base]


def hat_nvg():
    strap = mat(skull_shell('Strap', .1, .06, top=.004), 'Fabric')
    mount = mat(kit.box('Mount', (.05, .03, .04), (0, .11, -.05), bevel=.005), 'SteelDark')
    tubes = [mat(kit.cylinder(f'Tube{s}', .02, .08, (s * .03, .17, -.1), sides=18, axis='Y', bevel=.004), 'Plastic') for s in (-1, 1)]
    lenses = [mat(kit.cylinder(f'Lens{s}', .017, .004, (s * .03, .212, -.1), sides=18, axis='Y'), 'EmitGreen') for s in (-1, 1)]
    arm = mat(kit.box('Arm', (.03, .05, .03), (0, .14, -.07), bevel=.004), 'SteelDark')
    return [strap, mount, arm] + tubes + lenses


def hat_saucepan():
    pan = mat(kit.lathe('Pan', [(.1, .0), (.105, -.06), (.1, -.075), (.0, -.075)], sides=40, cap_bottom=False), 'Aluminium')
    pan.location.z = .08
    pan.rotation_euler.x = math.pi
    kit.bake_transform(pan)
    kit.solidify(pan, .003)
    handle = mat(kit.box('Handle', (.025, .17, .015), (0, -.18, .07), bevel=.004, rotation=(-15, 0, 0)), 'PlasticRed')
    return [pan, handle]


def hat_crown():
    ring = mat(kit.cylinder('Band', .105, .05, (0, 0, -.03), sides=10, cap=False), 'RustedSteel')
    kit.solidify(ring, .006)
    points = []
    for i in range(10):
        a = i / 10 * math.tau
        p = mat(kit.cone(f'Spike{i}', .018, .002, .06, (math.cos(a) * .104, math.sin(a) * .104, .02), sides=4), 'RustedSteel')
        points.append(p)
    bolts = [mat(kit.sphere(f'Bolt{i}', .008, (math.cos(i * .63) * .108, math.sin(i * .63) * .108, -.03), 8, 6), 'Brass') for i in range(10)]
    return [ring] + points + bolts


def hat_hazmat():
    hood = mat(skull_shell('Hood', .115, .22, front=1.05, back=1.12, top=.02), 'PlasticYellow')
    visor = mat(kit.box('Visor', (.16, .02, .1), (0, .12, -.11), bevel=.01), 'Glass')
    frame = mat(kit.box('VisorFrame', (.175, .015, .115), (0, .112, -.11), bevel=.01), 'Rubber')
    cut = kit.box('Cut', (.155, .2, .095), (0, .14, -.11))
    kit.boolean(hood, cut)
    return [hood, frame, visor]


def hat_goldhat():
    return hat_hardhat('Gold')


def hat_ushanka():
    fur = materials.surface('Fur', (.32, .26, .2), .98, 0, var=.2, bump=.5, bump_scale=300, grime=.2)
    leather = materials.surface('UshankaLeather', (.1, .07, .05), .55, 0, wear=.3)
    crown = mat(skull_shell('Crown', .105, .08, top=.03), leather)
    rim = mat(kit.cylinder('FurRim', .115, .05, (0, 0, -.06), sides=40, cap=False), fur)
    kit.solidify(rim, .02)
    flaps = [mat(kit.box(f'Flap{s}', (.03, .09, .1), (s * .11, 0, -.12), bevel=.012), fur) for s in (-1, 1)]
    return [crown, rim] + flaps


def hat_headlamp():
    band = mat(kit.cylinder('Band', .1, .025, (0, 0, -.06), sides=40, cap=False), 'Fabric')
    for v in band.data.vertices:
        v.co.y *= 1.12
    kit.solidify(band, .004)
    top = mat(kit.box('TopStrap', (.02, .2, .004), (0, 0, .005), rotation=(0, 0, 0)), 'Fabric')
    lamp = mat(kit.box('Lamp', (.05, .03, .035), (0, .12, -.06), bevel=.006), 'PlasticYellow')
    lens = mat(kit.cylinder('Lens', .013, .004, (0, .137, -.06), sides=16, axis='Y'), 'EmitCold')
    return [band, top, lamp, lens]


def hat_souwester():
    oil = materials.surface('Oilskin', (.75, .55, .05), .28, 0, var=.08, wear=.3, coat=.4)
    body = mat(skull_shell('Body', .102, .09, top=.015), oil)
    rim = mat(brim('Rim', .1, .17, -.07, .1, thick=.005), oil)
    for v in rim.data.vertices:
        if v.co.y < 0:
            v.co.y *= 1.35
            v.co.z -= .03 * (-v.co.y / .2)
    return [body, rim]


def hat_firehelmet():
    shell = mat(skull_shell('Shell', .11, .1, front=1.1, back=1.25, top=.03), 'PaintRed')
    tail = mat(brim('Tail', .108, .19, -.08, .25, thick=.006), 'PaintRed')
    for v in tail.data.vertices:
        if v.co.y > 0:
            v.co.y = .108 + (v.co.y - .108) * .35 if v.co.y > .108 else v.co.y
    shield = mat(kit.box('Shield', (.07, .01, .08), (0, .125, -.0), bevel=.01, rotation=(15, 0, 0)), 'Brass')
    comb = mat(kit.box('Comb', (.02, .22, .03), (0, -.01, .03), bevel=.008), 'PaintRed')
    return [shell, tail, shield, comb]


# ---------------------------------------------------------------------------------------------------- accessories

def acc_goggles():
    strap = mat(kit.cylinder('Strap', .1, .025, (0, 0, EYE + .005), sides=40, cap=False), 'Fabric')
    for v in strap.data.vertices:
        v.co.y = v.co.y * 1.1 + .008
    kit.solidify(strap, .003)
    frame = mat(kit.box('Frame', (.165, .035, .055), (0, FRONT + .025, EYE + .005), bevel=.014), 'PlasticGrey')
    lens = mat(kit.box('Lens', (.15, .01, .042), (0, FRONT + .043, EYE + .005), bevel=.012), 'Glass')
    return [strap, frame, lens]


def acc_dustmask():
    m = mat(kit.sphere('Mask', .055, (0, FRONT + .005, MOUTH + .01), 24, 12, scale=(1.05, .65, 1.0)), 'Paper')
    cut = kit.box('Back', (.2, .1, .2), (0, FRONT - .05, MOUTH))
    kit.boolean(m, cut)
    kit.solidify(m, .003)
    strap = mat(kit.cylinder('Strap', .1, .006, (0, 0, MOUTH + .03), sides=40, cap=False), 'Rubber')
    for v in strap.data.vertices:
        v.co.y *= 1.05
    kit.solidify(strap, .002)
    return [m, strap]


def acc_bandana():
    cloth = materials.fabric('Bandana', (.45, .05, .04), .9, 600)
    b = mat(kit.cone('Bandana', .1, .085, .09, (0, .005, MOUTH - .005), sides=40), cloth)
    for v in b.data.vertices:
        v.co.y *= 1.12
        if v.co.y > .05 and v.co.z < MOUTH - .02:
            v.co.z -= .04 * (v.co.y / .12)
    kit.solidify(b, .004)
    return [b]


def acc_earmuffs():
    arc = mat(kit.tube_along('Headband', [(math.cos(a) * -.11, 0, math.sin(a) * .11 - .1 + .01) for a in
                                           [i / 12 * math.pi for i in range(13)]], .008), 'PlasticGrey')
    cups = [mat(kit.cylinder(f'Cup{s}', .042, .04, (s * .1, .005, EYE - .015), sides=24, axis='X', bevel=.01), 'PaintRed') for s in (-1, 1)]
    pads = [mat(kit.cylinder(f'Pad{s}', .036, .015, (s * .078, .005, EYE - .015), sides=24, axis='X'), 'LeatherBlack') for s in (-1, 1)]
    return [arc] + cups + pads


def acc_toolbelt():
    belt = mat(kit.cylinder('Belt', .21, .05, (0, 0, -.79), sides=48, cap=False), 'Leather')
    for v in belt.data.vertices:
        v.co.y *= .8
    kit.solidify(belt, .008)
    pouches = []
    for s in (-1, 1):
        p = mat(kit.box(f'Pouch{s}', (.07, .1, .12), (s * .2, .03, -.83), bevel=.01), 'Leather')
        pouches.append(p)
        pouches.append(mat(kit.rod(f'Hammer{s}', (s * .225, .06, -.79), (s * .225, .06, -.98), .012), 'WoodDark'))
        pouches.append(mat(kit.box(f'Tape{s}', (.05, .045, .05), (s * .19, -.04, -.83), bevel=.008), 'PlasticYellow'))
    buckle = mat(kit.box('Buckle', (.05, .01, .045), (0, .172, -.79), bevel=.004), 'Steel')
    return [belt, buckle] + pouches


def acc_rucksack():
    bag = mat(kit.box('Bag', (.3, .16, .42), (0, -.25, -.5), bevel=.04), materials.fabric('Ruck', (.25, .27, .2), .85, 900))
    lid = mat(kit.box('Lid', (.31, .17, .08), (0, -.25, -.27), bevel=.03), materials.fabric('Ruck', (.25, .27, .2), .85, 900))
    pocket = mat(kit.box('Pocket', (.2, .05, .16), (0, -.34, -.58), bevel=.02), materials.fabric('Ruck', (.25, .27, .2), .85, 900))
    straps = [mat(kit.box(f'Strap{s}', (.05, .02, .4), (s * .09, -.165, -.48), bevel=.006), 'Fabric') for s in (-1, 1)]
    roll = mat(kit.cylinder('Roll', .045, .3, (0, -.25, -.74), sides=20, axis='X'), 'PaintGreen')
    return [bag, lid, pocket, roll] + straps


def acc_gasmask():
    face = mat(kit.sphere('Face', .075, (0, FRONT - .005, (EYE + MOUTH) / 2), 28, 14, scale=(1.0, .7, 1.25)), 'Rubber')
    cut = kit.box('Back', (.25, .1, .3), (0, FRONT - .07, (EYE + MOUTH) / 2))
    kit.boolean(face, cut)
    kit.solidify(face, .004)
    eyes = [mat(kit.cylinder(f'Eye{s}', .022, .012, (s * .033, FRONT + .045, EYE), sides=24, axis='Y'), 'GlassDark') for s in (-1, 1)]
    rims = [mat(kit.cylinder(f'Rim{s}', .026, .01, (s * .033, FRONT + .04, EYE), sides=24, axis='Y'), 'Aluminium') for s in (-1, 1)]
    filt = mat(kit.cylinder('Filter', .035, .045, (.03, FRONT + .06, MOUTH - .01), sides=24, axis='Y', bevel=.004), 'PaintGreen')
    filt.rotation_euler.z = math.radians(25)
    strap = mat(kit.cylinder('Straps', .1, .02, (0, 0, EYE + .02), sides=40, cap=False), 'Rubber')
    kit.solidify(strap, .003)
    return [face, filt, strap] + eyes + rims


def acc_dogtags():
    chain = mat(kit.tube_along('Chain', [(math.cos(a) * .07, math.sin(a) * .06 + .02, -.33 - .08 * max(0, math.sin(a))) for a in
                                          [i / 16 * math.tau for i in range(17)]], .0015), 'Silver')
    tags = [mat(kit.box(f'Tag{i}', (.028, .002, .05), (.005 * i, .1 + .002 * i, -.43 - .01 * i), bevel=.004), 'Silver') for i in range(2)]
    return [chain] + tags


def acc_radio():
    body = mat(kit.box('Radio', (.055, .035, .09), (-.15, .11, -.33), bevel=.008), 'Plastic')
    ant = mat(kit.rod('Antenna', (-.165, .11, -.285), (-.165, .11, -.2), .005), 'Plastic')
    clip = mat(kit.box('Clip', (.04, .01, .03), (-.15, .09, -.29), bevel=.003), 'Steel')
    knob = mat(kit.cylinder('Knob', .007, .012, (-.135, .11, -.28), sides=12), 'Plastic')
    return [body, ant, clip, knob]


def acc_moustache():
    hair = materials.surface('Stache', (.05, .035, .025), .7, 0, var=.15, bump=.4, bump_scale=600, grime=0)
    parts = []
    for s in (-1, 1):
        pts = [(s * .004, FRONT + .016, MOUTH + .018), (s * .03, FRONT + .01, MOUTH + .012), (s * .055, FRONT - .002, MOUTH + .0),
               (s * .068, FRONT - .006, MOUTH + .014), (s * .07, FRONT - .006, MOUTH + .026)]
        parts.append(mat(kit.tube_along(f'Wing{s}', pts, lambda t: .009 * (1 - t * .75), 10), hair))
    return parts


def acc_shades():
    frame = mat(kit.box('Bridge', (.03, .006, .006), (0, FRONT + .02, EYE + .008)), 'Gold')
    lenses = [mat(kit.sphere(f'Lens{s}', .027, (s * .034, FRONT + .016, EYE - .002), 20, 10, scale=(1, .25, .85)), 'GlassDark') for s in (-1, 1)]
    arms = [mat(kit.rod(f'Arm{s}', (s * .062, FRONT + .012, EYE + .005), (s * .08, -.02, EYE + .0), .0025), 'Gold') for s in (-1, 1)]
    return [frame] + lenses + arms


def acc_halfmask():
    m = mat(kit.sphere('HalfMask', .05, (0, FRONT + .01, MOUTH + .02), 24, 12, scale=(1.05, .75, 1.0)), 'PlasticGrey')
    cut = kit.box('Back', (.2, .1, .2), (0, FRONT - .045, MOUTH))
    kit.boolean(m, cut)
    kit.solidify(m, .004)
    carts = [mat(kit.cylinder(f'Cart{s}', .028, .025, (s * .05, FRONT + .02, MOUTH + .005), sides=20, axis='X'), 'PaintRed') for s in (-1, 1)]
    return [m] + carts


def acc_headset():
    band = mat(kit.tube_along('Band', [(math.cos(a) * -.105, 0, math.sin(a) * .11 - .1 + .01) for a in
                                       [i / 12 * math.pi for i in range(13)]], .006), 'Plastic')
    cups = [mat(kit.cylinder(f'Cup{s}', .036, .03, (s * .095, .005, EYE - .015), sides=24, axis='X', bevel=.006), 'Plastic') for s in (-1, 1)]
    boom = mat(kit.tube_along('Boom', [(-.1, .02, EYE - .03), (-.08, .07, MOUTH + .02), (-.03, .11, MOUTH + .005)], .003), 'Plastic')
    mic = mat(kit.sphere('Mic', .008, (-.025, .113, MOUTH + .005)), 'Fabric')
    return [band, boom, mic] + cups


def acc_gaiter():
    g = mat(kit.cylinder('Gaiter', .072, .1, (0, .01, -.29), sides=36, cap=False), materials.fabric('Gaiter', (.16, .2, .14), .9, 500))
    kit.solidify(g, .006)
    return [g]


HATS = {'hardhat': hat_hardhat, 'beanie': hat_beanie, 'cap': hat_cap, 'bucket': hat_bucket, 'miner': hat_miner,
        'welding': hat_welding, 'fedora': hat_fedora, 'box': hat_box, 'cone': hat_cone, 'nvg': hat_nvg, 'saucepan': hat_saucepan,
        'crown': hat_crown, 'hazmat': hat_hazmat, 'goldhat': hat_goldhat,
        # new in the remake
        'trucker': hat_trucker, 'ushanka': hat_ushanka, 'headlamp': hat_headlamp, 'souwester': hat_souwester,
        'firehelmet': hat_firehelmet}
ACCESSORIES = {'goggles': acc_goggles, 'dustmask': acc_dustmask, 'bandana': acc_bandana, 'earmuffs': acc_earmuffs,
               'toolbelt': acc_toolbelt, 'rucksack': acc_rucksack, 'gasmask': acc_gasmask, 'dogtags': acc_dogtags,
               'radio': acc_radio, 'moustache': acc_moustache, 'shades': acc_shades,
               # new in the remake
               'halfmask': acc_halfmask, 'headset': acc_headset, 'gaiter': acc_gaiter}


def build(prefix, wid, maker, show):
    global L
    kit.reset()
    L = materials.library()
    parts = [p for p in maker() if p is not None and p.name in bpy.data.objects]
    for p in parts:
        kit.apply(p)
    body = kit.join(parts, f'{prefix}_{wid}')
    bake.bake_asset(body, f'Wear_{wid}', TEX, size=512 if prefix == 'Hat' else 512, samples=12)
    path = export.static_asset(f'{prefix}_{wid}', [body], os.path.join('Characters', 'Wearables'), lods=(1.0, .45, .15))
    if show:
        preview.save(f'{prefix}_{wid}')
    print(f'[remake-wear] {prefix}_{wid} -> {os.path.basename(path)}')


def main():
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    show = '--preview' in args
    only = [a for a in args if not a.startswith('--')]
    for wid, maker in HATS.items():
        if not only or wid in only:
            build('Hat', wid, maker, show)
    for wid, maker in ACCESSORIES.items():
        if not only or wid in only:
            build('Acc', wid, maker, show)


if __name__ == '__main__':
    main()
