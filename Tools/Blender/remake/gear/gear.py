"""The buyable gear, remodelled (17 items; the 2026-10-22 cut stays cut).

Each design is the C# one (Scripts/Equipment/Tool*Models.cs) ported line for line onto toolart.ToolArt, then given
more: real bevels and turned profiles, knurling, labels' frames, wear in the paint. Grip at the origin, forward +Z
in Unity (+Y here), so first-person holds, crewmates' hands and the floor copies all line up with the old ones.

Batch:
  Blender --background --factory-startup --python Tools/Blender/remake/gear/gear.py -- [kinds...] [--preview]
Writes Art/Remake/Gear/Gear_<Kind>.fbx and Textures/Gear_<Kind>_*.png.
"""
import math
import os
import sys

import bpy
from mathutils import Vector

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(os.path.dirname(_HERE)))
exec(open(os.path.join(os.path.dirname(_HERE), 'boot.py')).read())

from remake.core import bake, export, kit, preview  # noqa: E402
from remake.gear.toolart import ToolArt  # noqa: E402

V = Vector


def flashlight(a):
    a.Rod((0, 0, -.065), (0, 0, .155), .061, a.Paint)
    a.Grip((0, 0, -.045), (0, 0, .09), .064)
    a.Disc((0, 0, .155), .078, .036, a.Steel)
    a.Disc((0, 0, .208), .102, .08, a.Paint)
    a.Ring((0, 0, .252), .108, .013, a.Steel, (90, 0, 0))
    a.Disc((0, 0, .25), .073, .006, a.Glass)
    a.Disc((0, 0, .245), .025, .009, a.Tape)
    for i in range(5):
        a.Ring((0, 0, .17 + i * .012), .098, .004, a.Dark, (90, 0, 0))
    a.Box((0, .031, .103), (.024, .009, .039), a.Rubber)
    a.Box((.034, 0, .01), (.008, .014, .092), a.Steel)
    a.Ring((0, -.025, -.075), .025, .004, a.Steel, (90, 0, 0))
    a.Cable((0, -.025, -.075), (.028, -.012, -.05), (0, -.045, -.035), .005, a.Rubber)
    # Remake: a reflector cup behind the glass and a tail switch.
    a.Ring((0, 0, .238), .085, .02, a.Steel, (90, 0, 0))
    a.Disc((0, 0, -.068), .045, .008, a.Rubber)


def radio(a):
    a.Box((0, .06, .04), (.086, .19, .052), a.Paint)
    for x in (-.043, .043):
        a.Box((x, .06, .039), (.01, .164, .058), a.Rubber)
    a.Box((0, .11, .011), (.056, .043, .007), a.Dark)
    a.Box((0, .111, .006), (.045, .027, .003), a.Screen)
    for i in range(7):
        a.Box((0, .058 + i * .0055, .009), (.052, .0025, .004), a.Dark)
    for row in range(2):
        for col in range(3):
            a.Box(((col - 1) * .016, .015 + row * .016, .009), (.011, .011, .004), a.Rubber)
    a.Cylinder((.022, .164, .038), (.021, .021, .021), a.Rubber)
    a.Cylinder((-.021, .164, .038), (.017, .021, .017), a.Steel)
    a.Rod((-.026, .168, .054), (-.03, .315, .056), .009, a.Rubber)
    a.Box((-.049, .08, .04), (.009, .053, .026), a.Rubber)
    a.Box((0, .063, .072), (.028, .11, .013), a.Steel)
    a.Box((0, -.036, .04), (.07, .012, .05), a.Dark)
    for x in (-.032, .032):
        a.Disc((x, -.021, .011), .007, .003, a.Steel)


def detector(a, kind):
    a.Box((0, .013, .13), (.16 if kind == 1 else .13, .08, .22), a.Paint)
    for x in (-.064, .064):
        a.Box((x, .014, .13), (.018, .09, .205), a.Rubber)
    a.Box((0, .055, .125), (.108, .013, .132), a.Dark)
    a.Box((0, .063, .13), (.088, .003, .103), a.Screen)
    a.Screws((0, 0, .13), .098, .174, .057)
    for i in range(3):
        a.Box(((i - 1) * .028, .057, .032), (.02, .008, .014), a.Paint if i == 2 else a.Rubber)
    a.Grille((0, .056, .218), (.071, .024), 4, a.Steel)
    if kind == 0:
        a.Box((0, -.066, .06), (.056, .095, .062), a.Rubber, (-14, 0, 0))
        a.Rod((-.038, -.012, .235), (-.038, -.012, .292), .022, a.Steel)
        a.Rod((.038, -.012, .235), (.038, -.012, .292), .022, a.Steel)
        a.Cable((.04, -.023, .19), (.035, -.07, .08), (.033, -.012, .03), .007, a.Rubber)
    elif kind == 1:
        a.Disc((0, .014, .255), .088, .045, a.Rubber)
        a.Disc((0, .014, .28), .065, .008, a.Dark)
        a.Box((0, -.035, .13), (.1, .026, .16), a.Rubber)
        a.Rod((-.065, .046, .21), (-.065, .046, .28), .006, a.Steel)
    else:
        a.Box((0, -.076, .075), (.047, .115, .064), a.Rubber, (-18, 0, 0))
        a.Disc((0, .008, .267), .082, .067, a.Dark)
        a.Ring((0, .008, .303), .086, .013, a.Steel, (90, 0, 0))
        a.Disc((0, .008, .309), .055, .004, a.Glass)
        a.Box((.044, -.028, .27), (.021, .025, .023), a.Dark)


def night_vision(a):
    a.Box((0, 0, .09), (.17, .065, .075), a.Paint)
    for x in (-.047, .047):
        a.Rod((x, 0, -.014), (x, 0, .204), .057, a.Rubber)
        a.Ring((x, 0, .2), .069, .018, a.Steel, (90, 0, 0))
        a.Disc((x, 0, .211), .047, .005, a.Glass)
        a.Ring((x, 0, -.016), .071, .023, a.Rubber, (90, 0, 0))
        a.Disc((x, 0, -.028), .04, .006, a.Glass)
        for i in range(4):
            a.Ring((x, 0, .105 + i * .012), .061, .004, a.Dark, (90, 0, 0))
    a.Box((0, .054, .09), (.046, .05, .045), a.Steel)
    a.Box((0, .08, .09), (.071, .014, .058), a.Dark)
    a.Cylinder((.025, .043, .11), (.019, .02, .019), a.Rubber)
    a.Screws((0, 0, .09), .132, .047, .035)


def battery(a):
    a.Box((0, .02, .08), (.095, .057, .15), a.Paint)
    for i in range(3):
        a.Rod(((i - 1) * .029, .018, .025), ((i - 1) * .029, .018, .128), .027, a.Rubber)
    a.Box((0, .022, .15), (.095, .065, .025), a.Dark)
    for x in (-.024, .024):
        a.Box((x, .057, .148), (.017, .005, .019), a.Steel)
    a.Box((0, .052, .077), (.074, .004, .048), a.Tape)
    a.Box((0, -.018, .079), (.057, .006, .11), a.Dark)
    a.Screws((0, 0, .075), .073, .105, .049)


def noise_maker(a):
    a.Box((0, 0, .08), (.092, .115, .135), a.Paint)
    a.Disc((0, 0, .155), .105, .023, a.Dark)
    a.Ring((0, 0, .172), .107, .008, a.Steel, (90, 0, 0))
    for i in range(-3, 4):
        a.Box((i * .011, 0, .176), (.004, .077 - abs(i) * .009, .005), a.Steel)
    a.Box((0, .062, .065), (.048, .012, .046), a.Rubber)
    a.Ring((0, .078, .025), .035, .004, a.Steel, (90, 0, 0))
    a.Box((0, -.061, .079), (.071, .009, .096), a.Dark)
    a.Screws((0, 0, .078), .066, .1, .059)


def drone(a):
    a.Box((0, .016, .14), (.115, .06, .16), a.Paint)
    a.Box((0, .051, .14), (.073, .013, .111), a.Rubber)
    for side in (-1, 1):
        for fore in (-1, 1):
            motor = V((side * .14, .024, .14 + fore * .115))
            a.Beam((side * .04, .017, .14 + fore * .045), motor, .025, .019, a.Dark)
            a.Cylinder(motor, (.034, .027, .034), a.Steel)
            a.Box(motor + V((0, .017, 0)), (.139, .003, .015), a.Rubber, (0, side * fore * 24, 0))
            a.Ring(motor + V((0, .014, 0)), .16, .004, a.Dark)
            a.Rod(motor + V((-.06, 0, 0)), motor + V((.06, 0, 0)), .004, a.Dark)
            a.Rod(motor + V((0, 0, -.06)), motor + V((0, 0, .06)), .004, a.Dark)
    a.Box((0, -.03, .217), (.051, .035, .045), a.Dark)
    a.Disc((0, -.03, .244), .027, .009, a.Glass)
    for x in (-.045, .045):
        a.Rod((x, -.02, .095), (x, -.065, .21), .008, a.Steel)
    a.Screws((0, 0, .14), .082, .124, .051)


def crowbar(a):
    a.Rod((0, 0, -.11), (0, 0, .44), .025, a.Paint)
    a.Grip((0, 0, -.095), (0, 0, .065), .031)
    a.Rod((0, 0, .44), (0, .025, .51), .029, a.Steel)
    a.Rod((0, .025, .51), (0, .071, .547), .028, a.Steel)
    a.Beam((-.012, .071, .547), (-.012, .088, .592), .016, .012, a.Steel)
    a.Beam((.012, .071, .547), (.012, .088, .592), .016, .012, a.Steel)
    a.Box((0, .012, -.123), (.038, .012, .042), a.Steel, (-15, 0, 0))
    a.Ring((0, 0, .087), .03, .011, a.Steel, (90, 0, 0))


def sledgehammer(a):
    a.Beam((0, 0, -.115), (0, 0, .61), .032, .027, a.Wood)
    a.Grip((0, 0, -.105), (0, 0, .09), .038)
    a.Box((0, 0, .6), (.24, .085, .09), a.Dark)
    for x in (-.117, .117):
        a.Box((x, 0, .6), (.025, .081, .084), a.Steel)
    a.Box((0, .044, .6), (.041, .006, .04), a.Steel)
    a.Box((0, .048, .6), (.006, .005, .034), a.Dark)
    a.Box((0, -.002, .51), (.039, .034, .05), a.Paint)
    for i in range(4):
        a.Box((.019, 0, .025 + i * .01), (.003, .025, .003), a.Tape)


def bolt_cutters(a):
    for side in (-1, 1):
        fr, to = V((side * .062, 0, -.09)), V((side * .02, 0, .36))
        a.Beam(fr, to, .023, .019, a.Paint)
        a.Grip(fr, fr.lerp(to, .35), .031)
        a.Beam(to, (side * .046, 0, .411), .035, .022, a.Steel)
        a.Beam((side * .046, 0, .411), (side * .027, 0, .483), .039, .027, a.Dark)
        a.Beam((side * .027, 0, .483), (side * .006, 0, .516), .018, .023, a.Steel)
        a.Cylinder((side * .027, .015, .402), (.021, .009, .021), a.Steel)
        a.Box((side * .027, .021, .402), (.012, .003, .004), a.Dark)
    a.Cylinder((0, 0, .362), (.026, .035, .026), a.Steel)
    a.Box((0, -.015, .414), (.076, .006, .024), a.Steel)


def lockpick(a):
    a.Box((0, -.009, .08), (.155, .022, .13), a.Leather)
    a.Box((0, -.021, .08), (.17, .009, .142), a.Dark)
    for i in range(4):
        x = (i - 1.5) * .031
        a.Box((x, .005, .075), (.014, .014, .077), a.Rubber)
        a.Beam((x, .009, .11), (x, .009, .21 - i * .013), .004, .0025, a.Steel)
        a.Beam((x, .009, .21 - i * .013), (x + (.009 if i % 2 == 0 else -.006), .012, .219 - i * .013), .004, .0025, a.Steel)
        a.Box((x, .009, .042), (.015, .004, .012), a.Steel)
    for x in (-.078, .078):
        for i in range(8):
            a.Box((x, .004, .024 + i * .016), (.003, .002, .008), a.Tape)
    a.Box((0, .009, .017), (.15, .009, .026), a.Leather)
    a.Cylinder((.058, .016, .018), (.01, .004, .01), a.Steel)


def glow_sticks(a):
    a.Rod((0, 0, -.025), (0, 0, .235), .019, a.Paint)
    a.Rod((0, 0, .003), (0, 0, .214), .015, a.Tape)
    a.Disc((0, 0, -.018), .025, .018, a.Dark)
    a.Disc((0, 0, .226), .025, .024, a.Rubber)
    a.Ring((0, .021, .235), .026, .006, a.Rubber, (90, 0, 0))
    a.Box((.016, 0, .204), (.012, .008, .052), a.Rubber)


def planks(a):
    for i in range(3):
        x = (i - 1) * .075
        a.Box((x, 0, .19 + (i - 1) * .012), (.072, .035, .59), a.Wood)
        for edge in range(2):
            a.Box((x, .019, -.07 + edge * .53), (.06, .002, .017), a.Steel)
            for dx in (-.018, .018):
                a.Cylinder((x + dx, .021, -.07 + edge * .53), (.005, .002, .005), a.Dark)
    for z in (.035, .335):
        a.Box((0, .022, z), (.231, .006, .025), a.Canvas)
        a.Box((0, -.023, z), (.231, .006, .025), a.Canvas)
        for x in (-.114, .114):
            a.Box((x, 0, z), (.006, .05, .025), a.Canvas)
        a.Box((.05, .028, z), (.036, .009, .033), a.Steel)


def medkit(a):
    a.Box((0, .006, .1), (.22, .096, .165), a.Paint)
    a.Box((0, .013, .1), (.225, .009, .17), a.Rubber)
    a.Box((0, .052, .1), (.205, .018, .15), a.Paint)
    a.Box((0, .063, .1), (.083, .003, .025), a.Tape)
    a.Box((0, .063, .1), (.025, .003, .083), a.Tape)
    for x in (-.072, .072):
        a.Box((x, .006, .01), (.028, .041, .015), a.Dark)
        a.Box((x, .011, .001), (.019, .017, .006), a.Steel)
        a.Box((x, .015, .188), (.038, .021, .016), a.Steel)
    a.Box((0, .004, -.013), (.096, .024, .015), a.Rubber)
    for x in (-.055, .055):
        a.Box((x, .004, -.001), (.016, .033, .032), a.Steel)
    for x in (-.104, .104):
        for z in (.032, .167):
            a.Box((x, 0, z), (.027, .085, .033), a.Rubber)
    a.Screws((0, 0, .1), .171, .113, .063)


def hand_trolley(a):
    half, height = .107, .325
    for x in (-half, half):
        a.Rod((x, -.025, .09), (x, height, .14), .018, a.Paint)
        a.Cylinder((x * 1.16, -.04, .092), (.075, .029, .075), a.Rubber, (0, 0, 90))
        a.Cylinder((x * 1.18, -.04, .092), (.035, .032, .035), a.Steel, (0, 0, 90))
        a.Cylinder((x, .095, .107), (.025, .027, .025), a.Steel, (0, 0, 90))
        a.Beam((x, -.005, .13), (x, .13, .235), .014, .014, a.Steel)
        # Remake: tyre tread rings.
        for k in range(3):
            a.Ring((x * 1.16 + (k - 1) * .008, -.04, .092), .076, .004, a.Dark, (0, 0, 90))
    a.Grip((-half, height, .14), (half, height, .14), .024)
    for y in (.025, .135, .22):
        if y < height - .025:
            a.Rod((-half, y, .105), (half, y, .105), .014, a.Steel)
    a.Box((0, -.009, .175), (half * 2 + .01, .019, .15), a.Paint)
    for i in range(5):
        a.Box(((i - 2) * half * .35, .003, .175), (.008, .003, .12), a.Rubber)


KINDS = {
    'Flashlight': (flashlight, (1, .9, .5)), 'Radio': (radio, (.3, .3, .3)), 'Medkit': (medkit, (.9, .2, .2)),
    'StressScanner': (lambda a: detector(a, 0), (.2, .8, .9)), 'HandTrolley': (hand_trolley, (.9, .4, .1)),
    'Planks': (planks, (.6, .45, .25)), 'NoiseMaker': (noise_maker, (.9, .9, .2)), 'Crowbar': (crowbar, (.7, .15, .1)),
    'BoltCutters': (bolt_cutters, (.8, .2, .2)), 'MotionDetector': (lambda a: detector(a, 1), (.3, .85, .4)),
    'NightVision': (night_vision, (.35, .75, .3)), 'Battery': (battery, (.5, .56, .42)),
    'Sledgehammer': (sledgehammer, (.45, .42, .4)), 'GlowSticks': (glow_sticks, (.3, 1, .4)), 'Lockpick': (lockpick, (.6, .6, .65)),
    'ThermalCamera': (lambda a: detector(a, 2), (1, .5, .15)), 'Drone': (drone, (.3, .35, .4)),
}


def srgb_to_linear(c):
    return tuple(x / 12.92 if x <= .04045 else ((x + .055) / 1.055) ** 2.4 for x in c)


def build(kind, show=False):
    kit.reset()
    maker, colour = KINDS[kind]
    a = ToolArt(tuple(x * .8 for x in srgb_to_linear(colour)))
    maker(a)
    ob = a.finish('Gear_' + kind)
    bake.bake_asset(ob, 'Gear_' + kind, export.art_dir('Gear', 'Textures'), size=1024, samples=12)
    export.static_asset('Gear_' + kind, [ob], 'Gear', lods=(1.0, .5, .2))
    if show:
        preview.save('Gear_' + kind)
    print(f'[remake-gear] {kind}')


def main():
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    show = '--preview' in args
    for k in [a for a in args if not a.startswith('--')] or list(KINDS):
        build(k, show)


if __name__ == '__main__':
    main()
