"""Outfits for the playable workers, built from the 2026-10-22 crew garments (Tools/Blender/crew) and exported
skinned on the crew skeleton in three builds.

An outfit is a recipe: materials for each garment slot, whether the plate carrier or a hi-vis vest goes on, the
footwear, extras (hoods, reflective bands, a bib, quilting) and how much the garments puff out. The faces of the
`tint` slots take the player's colour in game (the coverall colour, now "colour" in the wardrobe).

Builds come from one mesh reshaped (builds.reshape), so all three share UVs and one texture set.
"""
import math
import os
import sys

import bpy

_CREW = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), 'crew')
if _CREW not in sys.path:
    sys.path.insert(0, _CREW)
import crew_body as B  # noqa: E402
import crew_geo as G  # noqa: E402
import crew_materials as CM  # noqa: E402
from crew_vest import build_vest  # noqa: E402

NEUTRAL = (.78, .77, .74)  # tinted garments are baked near-white so the player's colour reads true

OUTFITS = {
    'salvage': dict(name='Salvage Tactical', blurb='The company issue: ripstop jacket, plate carrier, cargo trousers.',
                    jacket=('ripstop', NEUTRAL), trousers=(.34, .31, .25), vest='plate', boots='work', tint=('Jacket',),
                    price=0),
    'coveralls': dict(name='Work Coveralls', blurb='One piece, one zip, a hundred washes.',
                      jacket=('twill', NEUTRAL), trousers=NEUTRAL, vest=None, boots='work', tint=('Jacket', 'Trousers'),
                      extras=('belt_only',), price=0),
    'hivis': dict(name='Road Crew', blurb='Seen from a mile off. Monsters included.',
                  jacket=('twill', (.08, .1, .16)), trousers=(.13, .2, .33), vest='hivis', boots='work', tint=('Vest',),
                  extras=('denim',), price=900),
    'hazmat': dict(name='Hazmat Suit', blurb='Rated for chemicals. Not rated for anything with teeth.',
                   jacket=('rubber', NEUTRAL), trousers=NEUTRAL, vest=None, boots='rubber', gloves='rubber',
                   tint=('Jacket', 'Trousers'), puff=.016, extras=('hood_down',), price=1800),
    'parka': dict(name='Winter Parka', blurb='Quilted, fur-trimmed, for the Michigan jobs.',
                  jacket=('quilt', NEUTRAL), trousers=(.12, .12, .13), vest=None, boots='work', tint=('Jacket',),
                  puff=.028, extras=('fur_collar', 'quilting'), price=1500),
    'turnout': dict(name='Turnout Gear', blurb='Bought at a fire-station auction. Still smells of smoke.',
                    jacket=('canvas', NEUTRAL), trousers=(.58, .5, .32), vest=None, boots='rubber', tint=('Jacket',),
                    puff=.012, extras=('reflective_bands', 'collar_high'), price=2400),
    'mechanic': dict(name='Mechanic Overalls', blurb='Bib overalls over a work shirt. Pockets for everything.',
                     jacket=('twill', (.42, .4, .36)), trousers=NEUTRAL, vest=None, boots='work', tint=('Trousers',),
                     extras=('bib',), price=700),
    'rain': dict(name='Rain Slicker', blurb='For the island job, and the leaks everywhere else.',
                 jacket=('slicker', NEUTRAL), trousers=(.1, .11, .1), vest=None, boots='rubber', tint=('Jacket',),
                 puff=.008, extras=('hood_down',), price=1100),
    'hoodie': dict(name='Off the Clock', blurb='Hoodie and jeans. Technically not uniform.',
                   jacket=('knit', NEUTRAL), trousers=(.12, .17, .28), vest=None, boots='sneaker', gloves='none_dark',
                   tint=('Jacket',), extras=('hood_down', 'denim', 'kangaroo'), price=600),
    'medic': dict(name='Field Medic', blurb='Scrubs under a utility vest. From the hospital job.',
                  jacket=('scrubs', NEUTRAL), trousers=NEUTRAL, vest='plate', boots='sneaker', tint=('Jacket', 'Trousers'),
                  puff=-.004, price=1300),
}


def materials(spec):
    """The crew material dictionary for one outfit (keys used by crew_body / crew_vest)."""
    kind, colour = spec['jacket']
    M = CM.library(colour)  # builds every crew key; we override per outfit below
    if kind == 'rubber':
        M['Jacket'] = CM.rubber('OutfitSuit', colour, .38)
    elif kind == 'slicker':
        M['Jacket'] = CM.rubber('OutfitSlicker', colour, .22)
    elif kind == 'quilt':
        M['Jacket'] = CM.fabric('OutfitParka', colour, .62, 1800.0, .4, .45, 'ripstop', .45)
    elif kind == 'knit':
        M['Jacket'] = CM.fabric('OutfitKnit', colour, .95, 500.0, .5, .5, 'twill', .6, .1)
    elif kind == 'canvas':
        M['Jacket'] = CM.fabric('OutfitCanvas', colour, .9, 700.0, .6, .35, 'twill', .2, .2)
    elif kind == 'scrubs':
        M['Jacket'] = CM.fabric('OutfitScrubs', colour, .88, 1800.0, .4, .3, 'twill', .3)
    elif kind == 'twill':
        M['Jacket'] = CM.fabric('OutfitTwill', colour, .84, 1100.0, .5, .3, 'twill', .25, .08)
    M['JacketDark'] = M['Jacket'] if kind in ('rubber', 'slicker', 'quilt', 'knit', 'scrubs') else \
        CM.fabric('OutfitDark', tuple(c * .5 for c in colour), .82, 1200.0, .45, .2, 'twill', .2)
    trousers = spec['trousers']
    if 'Trousers' in spec['tint'] and kind in ('rubber', 'scrubs'):
        M['Trousers'] = M['Jacket']
    elif 'denim' in spec.get('extras', ()):
        M['Trousers'] = CM.fabric('OutfitDenim', trousers, .9, 1600.0, .5, .3, 'twill', .1, .25)
    else:
        M['Trousers'] = CM.fabric('OutfitTrousers', trousers, .85, 900.0, .55, .3, 'twill', .2, .15)
    if spec.get('vest') == 'hivis':
        M['Vest'] = CM.fabric('OutfitHiVis', NEUTRAL, .7, 2000.0, .4, .1, 'twill', .3)
        M['Webbing'] = M['Tape']
    gloves = spec.get('gloves')
    if gloves == 'rubber':
        M['Glove'] = M['GlovePalm'] = CM.rubber('OutfitRubberGlove', (.08, .22, .1), .35)
    elif gloves == 'none_dark':
        M['Glove'] = CM.fabric('OutfitKnitGlove', (.05, .05, .055), .9, 700.0)
        M['GlovePalm'] = M['Glove']
    if spec['boots'] == 'rubber':
        M['Boot'] = CM.rubber('OutfitRubberBoot', (.03, .03, .03), .35)
    elif spec['boots'] == 'sneaker':
        M['Boot'] = CM.fabric('OutfitSneaker', (.55, .55, .57), .8, 1500.0)
        M['Sole'] = CM.rubber('OutfitSneakerSole', (.8, .79, .76), .6)
    return M


def tint_materials(M, spec):
    return {M[k] for k in spec['tint'] if k in M}


# ---------------------------------------------------------------------------------------------- footwear variants

def rubber_boot(M, x):
    """A knee-height rubber boot: smooth shaft, heavy tread, a pull-tab."""
    shaft = G.loft('RubberShaft', [B.R(z, rx, rf, rb, x=x, y=.01) for z, rx, rf, rb in
                                   ((.42, .072, .075, .076), (.34, .068, .07, .074), (.22, .064, .066, .07),
                                    (.12, .063, .066, .068), (.08, .062, .066, .068))], M['Boot'], cap=(False, False))
    foot = [(.076, .045, .036, .056, .072), (.063, .055, .05, .096, .087), (.02, .06, .055, .116, .097),
            (-.03, .062, .055, .09, .082), (-.09, .063, .05, .065, .067), (-.15, .062, .045, .05, .057),
            (-.205, .055, .04, .04, .052), (-.234, .032, .028, .028, .05)]
    upper = G.loft('RubberFoot', [{'c': (x, y, zc), 'rx': rx, 'rf': dn, 'rb': u, 'p': 2.3} for y, rx, dn, u, zc in foot],
                   M['Boot'], plane='XZ')
    sole = [(.086, .052), (.072, .065), (.02, .069), (-.05, .069), (-.14, .071), (-.205, .064), (-.243, .036)]
    return [shaft, upper,
            G.loft('RubberSole', [{'c': (x, y, .018), 'rx': rx, 'rf': .018, 'p': 4.0} for y, rx in sole], M['Sole'], plane='XZ'),
            G.rbox('PullTab', (x, .085, .42), (.02, .008, .04), M['Boot'], .004)]


def sneaker(M, x):
    up = [(.07, .04, .03, .05, .064), (.055, .05, .045, .07, .07), (.02, .054, .05, .085, .074), (-.03, .056, .05, .07, .07),
          (-.09, .057, .046, .055, .062), (-.15, .056, .04, .045, .055), (-.205, .05, .036, .036, .05), (-.232, .03, .026, .026, .048)]
    upper = G.loft('SneakerUpper', [{'c': (x, y, zc), 'rx': rx, 'rf': dn, 'rb': u, 'p': 2.3} for y, rx, dn, u, zc in up],
                   M['Boot'], plane='XZ')
    sole = [(.08, .048), (.066, .06), (.02, .064), (-.05, .064), (-.14, .066), (-.205, .06), (-.24, .033)]
    collar = G.loft('SneakerCollar', [B.R(.135, .058, .06, .066, x=x, y=.03), B.R(.11, .06, .062, .068, x=x, y=.03)],
                    M['Boot'], cap=(False, False))
    out = [upper, collar,
           G.loft('SneakerSole', [{'c': (x, y, .016), 'rx': rx, 'rf': .02, 'p': 4.0} for y, rx in sole], M['Sole'], plane='XZ')]
    for k in range(4):
        yk, zk = -.03 - k * .025, .115 - k * .012
        out.append(G.tube('Lace', [(x - .02, yk, zk), (x, yk - .006, zk + .004), (x + .02, yk, zk)], M['Lace'], .002, sides=6,
                          subsurf=1))
    return out


# ----------------------------------------------------------------------------------------------------- extras

def hood_down(M, torso):
    """A hood lying on the back of the shoulders."""
    hood = G.loft('HoodDown', [B.R(1.47, .12, .07, .11, y=.03), B.R(1.43, .14, .08, .14, y=.06), B.R(1.37, .12, .05, .13, y=.1),
                               B.R(1.33, .08, .03, .09, y=.12)], M['Jacket'], n=40, arc=(math.radians(15), math.radians(165)))
    s = hood.modifiers.new('Thickness', 'SOLIDIFY')
    s.thickness, s.offset = .012, 1.0
    G.bend_folds(hood, .008, .05)
    G.attach(hood, torso)


def fur_collar(M, torso):
    fur = CM.fabric('OutfitFur', (.42, .36, .28), .98, 120.0, .4, .9, 'twill', .9)
    ring = G.loft('FurCollar', [B.R(z, rx, rf, rb, 2.1, y=.01) for z, rx, rf, rb in
                                ((1.44, .13, .11, .12), (1.49, .145, .12, .135), (1.54, .13, .11, .125))], fur, n=48,
                  arc=(math.radians(-70), math.radians(250)))
    G.bend_folds(ring, .012, .02)
    G.attach(ring, torso)


def quilting(M, chest):
    """Horizontal quilt seams pressed into the parka body."""
    for z in (1.0, 1.08, 1.16, 1.24, 1.32):
        G.attach(B.band('QuiltSeam', chest, z, z + .006, .2, .158, .14, M['JacketDark'], off=.0, thick=.002), chest.parent)


def reflective_bands(M, chest):
    for z0 in (1.0, 1.25):
        G.attach(B.band('Reflective', chest, z0, z0 + .05, .205, .16, .145, M['Tape'], off=.003, thick=.003), chest.parent)


def collar_high(M, torso):
    c = G.loft('HighCollar', [B.R(1.45, .1, .09, .095, 2.1), B.R(1.55, .108, .096, .102, 2.1)], M['JacketDark'], n=40,
               arc=(math.radians(-80), math.radians(260)))
    s = c.modifiers.new('Thickness', 'SOLIDIFY')
    s.thickness, s.offset = .01, 1.0
    G.attach(c, torso)


def bib(M, body, torso, chest):
    panel = G.slab('Bib', (0, -.17, 1.2), (.25, .02, .3), M['Trousers'], 1.0, -4, .01)
    G.attach(panel, torso)
    pocket = G.slab('BibPocket', (0, -.185, 1.24), (.14, .012, .12), M['Trousers'], .8, -4, .006)
    G.attach(pocket, torso)
    for s in (-1, 1):
        G.attach(B.strap('BibStrap', [(s * .1, -.18, 1.34), (s * .11, -.12, 1.44), (s * .1, .0, 1.475), (s * .07, .14, 1.36),
                                      (s * .04, .15, 1.15)], chest, M['Trousers'], .04, .004, .004), torso)
        G.attach(G.rbox('BibClip', (s * .1, -.19, 1.34), (.03, .01, .025), M['Steel'], .004), torso)


def kangaroo(M, torso):
    G.attach(G.slab('Kangaroo', (0, -.168, 1.05), (.24, .02, .13), M['Jacket'], 1.0, 0, .01), torso)


EXTRAS = {'hood_down': lambda M, j, chest: hood_down(M, j['torso']), 'fur_collar': lambda M, j, chest: fur_collar(M, j['torso']),
          'quilting': lambda M, j, chest: quilting(M, chest), 'reflective_bands': lambda M, j, chest: reflective_bands(M, chest),
          'collar_high': lambda M, j, chest: collar_high(M, j['torso']), 'bib': lambda M, j, chest: bib(M, j['body'], j['torso'], chest),
          'kangaroo': lambda M, j, chest: kangaroo(M, j['torso'])}


# ------------------------------------------------------------------------------------------------------ build

def build(oid, spec):
    """Every garment of one outfit, parented to crew pivots (crew frame). Returns (objects, materials)."""
    G.COLL = bpy.context.scene.collection
    M = materials(spec)
    root = G.pivot('Crew', (0, 0, 0))
    body = G.pivot('Body', (0, 0, .95), root)
    torso = G.pivot('Torso', (0, 0, 1.10), body)
    head = G.pivot('Head', (0, 0, 1.52), torso)
    j = {'root': root, 'body': body, 'torso': torso, 'head': head}
    chest = B.jacket(M, torso)
    if 'belt_only' in spec.get('extras', ()) or spec.get('vest') != 'plate':
        trousers_plain(M, body)
    else:
        B.trousers(M, body)
    if spec.get('vest') in ('plate', 'hivis'):
        build_vest(M, torso, chest, 1, B.strap, lambda *a, **k: None)
    boots = {'work': B.boot, 'rubber': rubber_boot, 'sneaker': sneaker}[spec['boots']]
    for s, n in ((1, 'L'), (-1, 'R')):
        j['arm' + n] = G.pivot('Arm' + n, (s * B.ARM_X, 0, B.ARM_Z), torso)
        j['elbow' + n] = G.pivot('Elbow' + n, (s * B.ARM_X, 0, B.ELBOW_Z), j['arm' + n])
        B.arm(M, j['arm' + n], j['elbow' + n], s)
        j['leg' + n] = G.pivot('Leg' + n, (s * B.LEG_X, 0, B.HIP_Z), body)
        j['knee' + n] = G.pivot('Knee' + n, (s * B.LEG_X, 0, B.KNEE_Z), j['leg' + n])
        leg_parts(M, j['leg' + n], j['knee' + n], s, boots, spec)
    for e in spec.get('extras', ()):
        if e in EXTRAS:
            EXTRAS[e](M, j, chest)
    puff = spec.get('puff', 0.0)
    if puff:
        targets = {M['Jacket'], M['JacketDark']} | ({M['Trousers']} if spec['jacket'][0] in ('rubber', 'scrubs') else set())
        for ob in bpy.data.objects:
            if ob.type == 'MESH' and ob.data.materials and ob.data.materials[0] in targets:
                d = ob.modifiers.new('Puff', 'DISPLACE')
                d.strength, d.mid_level, d.direction = puff, 0.0, 'NORMAL'
    return [o for o in bpy.data.objects if o.type in ('MESH', 'CURVE', 'FONT')], M


def trousers_plain(M, body):
    """Trousers with a belt but no hammer or belt pouch (outfits without the tactical kit)."""
    hips = G.loft('Trousers', B.HIPS, M['Trousers'], n=48)
    G.bend_folds(hips, .005, .08)
    belt = G.loft('Belt', [B.R(.962, .207, .163, .149, 2.3), B.R(1.012, .209, .165, .15, 2.3)], M['Webbing'], n=56,
                  cap=(False, False), subsurf=1)
    s = belt.modifiers.new('Thickness', 'SOLIDIFY')
    s.thickness, s.offset = .012, 1.0
    for p in (hips, belt, G.rbox('Buckle', (0, -.18, .987), (.05, .014, .04), M['Steel'], .005)):
        G.attach(p, body)


def leg_parts(M, piv, knee, s, boots, spec):
    """crew_body.leg with the outfit's footwear (and knee pads only with the tactical kit)."""
    x = s * B.LEG_X
    thigh = G.loft('Thigh', [B.R(.94, .118, .122, .128, x=x, y=.004), B.R(.86, .114, .115, .122, x=x, y=.004),
                             B.R(.76, .104, .102, .11, x=x, y=.004), B.R(.66, .094, .09, .098, x=x, y=.004),
                             B.R(.58, .086, .082, .088, x=x, y=.004), B.R(.52, .081, .079, .081, x=x, y=.004),
                             B.R(.49, .08, .079, .08, x=x, y=.004)], M['Trousers'])
    G.bend_folds(thigh, .006, .07)
    G.attach(thigh, piv)
    if spec.get('vest') == 'plate' or 'bib' in spec.get('extras', ()):
        for p in (G.rbox('CargoPocket', (x + s * .098, -.006, .70), (.03, .13, .16), M['Trousers'], .016, rot=(0, s * -6, 0), subsurf=1),
                  G.rbox('CargoFlap', (x + s * .106, -.006, .773), (.022, .136, .045), M['Trousers'], .01, rot=(0, s * -6, 0), subsurf=1)):
            G.attach(p, piv)
    rings = [B.R(.53, .081, .079, .081, x=x, y=.004), B.R(.46, .077, .072, .084, x=x, y=.004),
             B.R(.38, .075, .068, .085, x=x, y=.004)]
    if boots is rubber_boot:
        # Tucked into the boot shaft (its top is at 0.42).
        rings = rings[:2] + [B.R(.4, .068, .066, .072, x=x, y=.008)]
    elif boots is sneaker:
        # Straight-leg trousers breaking over the shoe.
        rings += [B.R(.28, .071, .068, .077, x=x, y=.006), B.R(.17, .07, .07, .076, x=x, y=.012),
                  B.R(.105, .072, .075, .08, x=x, y=.014), B.R(.09, .071, .074, .079, x=x, y=.014)]
    else:
        rings += [B.R(.30, .072, .068, .077, x=x, y=.004), B.R(.265, .075, .073, .078, x=x, y=.004),
                  B.R(.235, .074, .074, .076, x=x, y=.006), B.R(.2, .058, .06, .064, x=x, y=.01)]
    shin = G.loft('Shin', rings, M['Trousers'])
    G.bend_folds(shin, .005, .06)
    parts = [shin]
    if spec.get('vest') == 'plate':
        from mathutils import Vector
        parts.append(G.quad_sphere('KneePad', (x, -.08, .50), (.068, .03, .08), M['KneePad'], cuts=6, power=.75,
                                   shape=lambda d, p: Vector((p.x, p.y * (.3 if d.y > 0 else 1.0) - .02 * d.x * d.x, p.z))))
    parts += boots(M, x)
    for p in parts:
        G.attach(p, knee)


# ------------------------------------------------------------------------------------------------------ builds

BUILDS = [
    dict(name='Stocky', torso=1.0, front=1.0, back=1.0, arms=1.0, legs=1.0),
    dict(name='Average', torso=.93, front=.88, back=.95, arms=.9, legs=.93),
    dict(name='Slim', torso=.87, front=.8, back=.9, arms=.84, legs=.88),
]
