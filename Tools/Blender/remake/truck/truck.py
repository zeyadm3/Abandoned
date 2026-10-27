"""The collection truck, its visible upgrades and its cosmetics (2026 remake), all in TRUCK SPACE: Unity coordinates
relative to the truck root (+Z toward the cab, +Y up), so Unity drops every part at identity under the truck.

Keeps the functional footprint of the 0.15.7b truck (Editor/Truck): cargo box x +-1.41, z -2.6..2.6, deck 1.05 m, roof
3.4 m; cab z 2.76..5.07, x +-1.245; front axle z 3.75 (single, x +-1.16), rear axle z -1.65 (duals at x +-1.075 and
+-1.355); wheel radius 0.48. The rear doors stay the code-built hinged ones (they open and close in game).

Batch:
  Blender --background --factory-startup --python Tools/Blender/remake/truck/truck.py -- [parts...] [--preview]
Writes Art/Remake/Truck/Truck_<Part>.fbx and Textures/Truck_<Part>_*.png. Body faces painted with the TINT surface take
the player-chosen paint (RM_Truck_Body_Tint shares the body's textures).
"""
import math
import os
import random
import sys

import bpy
from mathutils import Vector

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(os.path.dirname(_HERE)))
exec(open(os.path.join(os.path.dirname(_HERE), 'boot.py')).read())

from remake.core import bake, export, kit, materials, preview  # noqa: E402
from remake.gear.toolart import SWAP, ToolArt  # noqa: E402

TINT = 'TruckPaint'
FRONT_AXLE, REAR_AXLE, WHEEL_R = 3.75, -1.65, .48


class TruckArt(ToolArt):
    def __init__(self):
        super().__init__((.75, .75, .73))
        L = materials.library()
        self.Paint = materials.paint(TINT, (.74, .74, .72), wear=.4, grime=.45, rough=.35)
        self.Chassis = L['PaintBlack']
        self.Chrome = L['Chrome']
        self.Tyre = L['Tyre']
        self.Head = L['EmitWarm']
        self.Tail = L['EmitRed']
        self.Galv = L['Galvanised']
        self.Glass = L['GlassDark']

    def U(self, p):
        return SWAP @ Vector(p)

    def Loft(self, sections, mat, cap=True):
        """Unity-space rings -> one lofted surface."""
        ob = kit.loft('Loft', [[self.U(p) for p in ring] for ring in sections], cap=cap)
        materials.assign(ob, mat)
        self.parts.append(ob)
        return ob


def rounded_rect(cx, cy, z, hw, hh, r, n=6):
    """A rounded rectangle in the XY plane at depth z (Unity), corners radius r."""
    pts = []
    for qx, qy, a0 in ((1, 1, 0), (-1, 1, 90), (-1, -1, 180), (1, -1, 270)):
        ccx, ccy = cx + qx * (hw - r), cy + qy * (hh - r)
        for i in range(n + 1):
            a = math.radians(a0 + 90 * i / n)
            pts.append((ccx + math.cos(a) * r, ccy + math.sin(a) * r, z))
    return pts


# ----------------------------------------------------------------------------------------------------- the body

def cab(a):
    # The shell: rounded sections from the cab back to the sloped front; the windscreen leans back.
    secs = []
    for z, top, bot, hw in ((2.76, 2.78, .9, 1.245), (3.2, 2.78, .9, 1.245), (4.3, 2.78, .9, 1.245), (4.75, 2.72, .9, 1.24),
                            (4.95, 2.4, .9, 1.23), (5.07, 1.7, .9, 1.22)):
        secs.append(rounded_rect(0, (top + bot) / 2, z, hw, (top - bot) / 2, .12))
    a.Loft(secs, a.Paint)
    # Windscreen and side windows (dark glass panels set just proud of the shell).
    a.Beam((0, 1.75, 5.06), (0, 2.6, 4.77), 2.1, .02, a.Glass)
    for s in (-1, 1):
        a.Box((s * 1.248, 2.15, 3.95), (.02, .75, 1.55), a.Glass)
        a.Box((s * 1.25, 1.35, 4.05), (.012, .9, .015), a.Dark)            # door seam
        a.Box((s * 1.25, 1.35, 3.05), (.012, .9, .015), a.Dark)
        a.Box((s * 1.262, 1.55, 3.3), (.025, .04, .16), a.Chrome)          # handle
        a.Rod((s * 1.25, 2.5, 4.8), (s * 1.55, 2.4, 4.85), .03, a.Chassis)  # mirror arm
        a.Box((s * 1.6, 2.15, 4.85), (.06, .42, .2), a.Chassis)
        a.Box((s * 1.63, 2.15, 4.85), (.01, .38, .16), a.Chrome)
        for k, y in enumerate((.62, .36)):                                 # steps
            a.Box((s * 1.18, y, 3.45), (.2, .04, .45), a.Galv)
    # Grille, headlights, indicators, badge plate (no lettering), bumper.
    a.Box((0, 1.25, 5.08), (1.5, .55, .03), a.Dark)
    for i in range(7):
        a.Box((0, 1.02 + i * .075, 5.095), (1.45, .022, .02), a.Chrome)
    for s in (-1, 1):
        a.Box((s * .92, 1.18, 5.08), (.42, .22, .05), a.Chrome)
        a.Box((s * .92, 1.18, 5.1), (.36, .17, .02), a.Head)
        a.Box((s * 1.18, 1.18, 5.07), (.08, .16, .04), materials.library()['EmitWarm'])
    a.Box((0, 1.6, 5.07), (.36, .08, .02), a.Chrome)
    a.Box((0, .74, 5.12), (2.5, .26, .16), a.Chassis)
    a.Box((0, .74, 5.2), (2.3, .2, .02), a.Galv)
    # Roof: sun visor and marker lamps.
    a.Box((0, 2.8, 4.86), (2.2, .04, .24), a.Paint, (-8, 0, 0))
    for x in (-.6, -.2, .2, .6):
        a.Box((x, 2.84, 4.92), (.12, .05, .06), a.Head)
    # Wheel arches: dark liners under the cab.
    for s in (-1, 1):
        arch = kit.lathe('Arch', [(.56, -.16), (.6, -.16), (.6, .16), (.56, .16)], sides=24)
        arch.rotation_euler = (0, math.pi / 2, 0)
        arch.location = a.U((s * 1.16, WHEEL_R, FRONT_AXLE))
        materials.assign(arch, a.Chassis)
        a.parts.append(arch)


def chassis(a):
    for s in (-1, 1):
        a.Beam((s * .5, .62, -2.95), (s * .5, .62, 4.95), .09, .22, a.Chassis)
    for z in (-2.7, -1.0, .6, 2.2, 4.4):
        a.Beam((-.5, .62, z), (.5, .62, z), .3, .06, a.Chassis)
    # Fuel tank, air tanks, battery box, exhaust under the cab.
    a.Cylinder((.95, .68, 1.6), (.5, 1.2, .5), a.Galv, (90, 0, 0))
    for k in range(4):
        a.Ring((.95, .68, 1.1 + k * .32), .52, .03, a.Chassis, (90, 0, 0))
    a.Cylinder((-.95, .7, 1.4), (.32, .9, .32), a.Chassis, (90, 0, 0))
    a.Box((-.95, .62, 2.3), (.4, .36, .5), a.Chassis)
    a.Box((-.95, .82, 2.3), (.42, .04, .52), a.Galv)
    a.Rod((.7, .4, 2.6), (.7, .4, -.3), .09, a.Steel)
    a.Cylinder((.7, .4, .8), (.18, .7, .18), a.Steel, (90, 0, 0))
    # Axles, springs and the rear under-ride bar.
    for z in (FRONT_AXLE, REAR_AXLE):
        a.Rod((-1.0, WHEEL_R, z), (1.0, WHEEL_R, z), .12, a.Chassis)
        for s in (-1, 1):
            a.Beam((s * .55, .58, z - .55), (s * .55, .58, z + .55), .08, .08, a.Steel)
    a.Beam((-1.2, .5, -2.95), (1.2, .5, -2.95), .1, .14, a.Galv)
    for s in (-1, 1):
        a.Rod((s * .9, .5, -2.95), (s * .6, .7, -2.6), .06, a.Galv)
        # Side under-run guard between the axles.
        for y in (.55, .78):
            a.Beam((s * 1.3, y, -1.0), (s * 1.3, y, 1.0), .05, .06, a.Galv)


def cargo_box(a):
    # Floor sill and the box: panels between corner posts, ribs, roof, front.
    a.Box((0, .96, 0), (2.82, .12, 5.2), a.Chassis)
    for s in (-1, 1):
        a.Box((s * 1.39, 2.2, 0), (.04, 2.32, 5.1), a.Paint)
        for k in range(13):
            z = -2.45 + k * .41
            a.Box((s * 1.415, 2.2, z), (.02, 2.28, .05), a.Paint)
        for y in (1.06, 3.36):
            a.Box((s * 1.41, y, 0), (.06, .1, 5.22), a.Galv)
        for z in (-2.58, 2.58):
            a.Box((s * 1.4, 2.2, z), (.08, 2.42, .08), a.Galv)
        for z in (-2.45, -.8, .8, 2.45):
            a.Box((s * 1.435, 1.1, z), (.02, .07, .12), a.Tail if z < 0 else a.Head)
    a.Box((0, 3.39, 0), (2.82, .04, 5.2), a.Galv)
    for k in range(9):
        a.Box((0, 3.42, -2.4 + k * .6), (2.78, .03, .06), a.Galv)
    a.Box((0, 2.2, 2.6), (2.82, 2.42, .05), a.Paint)
    # Rear frame around the opening (the doors are the game's own, hinged at x +-1.4).
    a.Box((0, 3.3, -2.6), (2.86, .18, .1), a.Galv)
    a.Box((0, 1.0, -2.62), (2.86, .12, .12), a.Galv)
    for s in (-1, 1):
        a.Box((s * 1.42, 2.2, -2.6), (.1, 2.42, .1), a.Galv)
        a.Box((s * .9, .78, -2.92), (.4, .12, .05), a.Tail)
        a.Box((s * .45, .78, -2.92), (.2, .12, .05), a.Head)


def tyre(a, x, z, width):
    """A profiled tyre: rounded sidewalls, a tread band with grooves (rims are a cosmetic part)."""
    prof = [(.3, -width / 2), (.4, -width / 2 - .01), (.46, -width / 2 + .005), (.48, -width / 2 + .03), (.48, width / 2 - .03),
            (.46, width / 2 - .005), (.4, width / 2 + .01), (.3, width / 2)]
    t = kit.lathe('Tyre', prof, sides=40, cap_bottom=False, cap_top=False)
    kit.solidify(t, .02)
    for k in range(4):
        g = kit.lathe('Groove', [(.475, -.005), (.49, -.005), (.49, .005), (.475, .005)], sides=40)
        g.location.z = (k - 1.5) * width * .22
        kit.bake_transform(g)
        kit.boolean(t, g)
    t.rotation_euler = (0, math.pi / 2, 0)
    t.location = a.U((x, WHEEL_R, z))
    materials.assign(t, a.Tyre)
    a.parts.append(t)


def wheels(a):
    for s in (-1, 1):
        tyre(a, s * 1.16, FRONT_AXLE, .28)
        tyre(a, s * 1.075, REAR_AXLE, .26)
        tyre(a, s * 1.355, REAR_AXLE, .26)


WHEEL_SPOTS = [(s * 1.16, FRONT_AXLE, s) for s in (-1, 1)] + [(s * 1.355, REAR_AXLE, s) for s in (-1, 1)]


def body(a):
    cab(a)
    chassis(a)
    cargo_box(a)
    wheels(a)


# ------------------------------------------------------------------------------------------------- cosmetics

def rims(style):
    def make(a):
        mat = {'steel': a.Galv, 'chrome': a.Chrome, 'black': a.Chassis, 'gold': materials.library()['Gold']}[style]
        for x, z, s in WHEEL_SPOTS:
            c = (x + s * .12, WHEEL_R, z)
            a.Cylinder(c, (.62, .05, .62), mat, (0, 0, 90))
            a.Cylinder((x + s * .15, WHEEL_R, z), (.22, .05, .22), a.Chrome if style != 'black' else a.Chassis, (0, 0, 90))
            spokes = 10 if style == 'chrome' else 6 if style == 'black' else 8
            for k in range(spokes):
                ang = k / spokes * math.tau
                off = Vector((0, math.cos(ang) * .2, math.sin(ang) * .2))
                if style == 'steel':
                    a.Cylinder((x + s * .135, WHEEL_R + off.y, z + off.z), (.07, .02, .07), a.Chassis, (0, 0, 90))
                else:
                    a.Beam((x + s * .14, WHEEL_R, z), (x + s * .14, WHEEL_R + off.y * 1.4, z + off.z * 1.4), .04, .025, mat)
            for k in range(8):
                ang = k / 8 * math.tau
                a.Cylinder((x + s * .175, WHEEL_R + math.cos(ang) * .085, z + math.sin(ang) * .085), (.025, .02, .025), a.Chrome, (0, 0, 90))
    return make


def bumper(style):
    def make(a):
        if style == 'bullbar':
            for s in (-1, 1):
                a.Rod((s * .75, .7, 5.3), (s * .75, 1.55, 5.3), .08, a.Chrome)
                a.Rod((s * .75, 1.55, 5.3), (s * .2, 1.6, 5.32), .08, a.Chrome)
                a.Rod((s * .75, .75, 5.3), (s * .75, .75, 5.15), .07, a.Chrome)
            a.Rod((-.75, 1.1, 5.3), (.75, 1.1, 5.3), .06, a.Chrome)
            a.Rod((-.2, 1.6, 5.32), (.2, 1.6, 5.32), .08, a.Chrome)
        elif style == 'pushbar':
            a.Box((0, .9, 5.32), (1.6, .5, .1), a.Chassis)
            for s in (-1, 1):
                a.Box((s * .65, .9, 5.38), (.18, .55, .08), a.Rubber)
                a.Box((s * .65, .9, 5.22), (.1, .3, .2), a.Chassis)
        elif style == 'plough':
            blade = a.Box((0, .55, 5.75), (2.9, .9, .08), materials.library()['PaintYellow'], (-25, 0, 0))
            a.Box((0, .2, 5.95), (2.9, .06, .1), a.Steel)
            for s in (-1, 1):
                a.Beam((s * .6, .7, 5.15), (s * .6, .62, 5.65), .12, .12, a.Chassis)
                a.Cylinder((s * .6, .9, 5.35), (.1, .5, .1), a.Chrome, (60, 0, 0))
    return make


def roof(style):
    def make(a):
        if style == 'beacons':
            for s in (-1, 1):
                a.Cylinder((s * .9, 2.92, 4.4), (.18, .2, .18), materials.library()['EmitWarm'])
                a.Cylinder((s * .9, 2.83, 4.4), (.2, .04, .2), a.Chassis)
            a.Box((0, 2.82, 4.4), (1.9, .04, .12), a.Chassis)
        elif style == 'rack':
            for s in (-1, 1):
                a.Beam((s * 1.15, 2.95, 3.0), (s * 1.15, 2.95, 4.5), .05, .05, a.Chassis)
                for z in (3.05, 4.45):
                    a.Rod((s * 1.15, 2.8, z), (s * 1.15, 2.95, z), .04, a.Chassis)
            for z in (3.2, 3.6, 4.0, 4.4):
                a.Rod((-1.15, 2.95, z), (1.15, 2.95, z), .035, a.Chassis)
            a.Cylinder((-.4, 3.07, 3.7), (.22, .9, .22), materials.library()['PaintRed'], (0, 0, 90))
            a.Box((.4, 3.05, 3.6), (.6, .18, .5), a.Canvas)
    return make


def mudflaps(style):
    def make(a):
        mat = {'rubber': a.Rubber, 'chrome': a.Chrome, 'hazard': materials.library()['PaintYellow']}[style]
        for s in (-1, 1):
            a.Box((s * 1.22, .5, REAR_AXLE - .7), (.55, .55, .015), mat)
            if style == 'hazard':
                for k in range(4):
                    a.Box((s * 1.22 + (k - 1.5) * .13, .5, REAR_AXLE - .71), (.05, .55, .012), a.Chassis, (0, 0, 35))
            a.Box((s * 1.22, .82, REAR_AXLE - .7), (.6, .05, .06), a.Galv)
    return make


def ornament(style):
    def make(a):
        if style == 'bulldog':
            a.Box((0, 2.86, 4.95), (.1, .06, .1), a.Chrome)
            body = kit.sphere('Dog', .07, a.U((0, 2.97, 4.95)), 16, 10, scale=(1, 1.4, .9))
            head = kit.sphere('DogHead', .05, a.U((0, 3.02, 5.05)), 16, 10)
            for o in (body, head):
                materials.assign(o, a.Chrome)
                a.parts.append(o)
        elif style == 'skull':
            sk = kit.sphere('Skull', .07, a.U((0, 2.95, 4.95)), 18, 12, scale=(.9, 1.0, 1.0))
            materials.assign(sk, materials.library()['PaintCream'])
            a.parts.append(sk)
            for s in (-1, 1):
                a.Cylinder((s * .025, 2.96, 5.02), (.025, .02, .025), a.Chassis, (90, 0, 0))
        elif style == 'eagle':
            a.Box((0, 2.9, 4.95), (.04, .1, .04), a.Chrome)
            for s in (-1, 1):
                a.Beam((0, 2.97, 4.95), (s * .22, 3.05, 4.9), .02, .07, a.Chrome)
            a.Cylinder((0, 2.98, 5.0), (.05, .1, .05), a.Chrome, (60, 0, 0))
        elif style == 'bobblehead':
            a.Cylinder((.6, 2.05, 4.75), (.05, .04, .05), a.Rubber)
            a.Box((.6, 2.11, 4.75), (.05, .08, .03), materials.library()['PaintOrange'])
            hd = kit.sphere('Bobble', .04, a.U((.6, 2.19, 4.75)), 16, 10)
            materials.assign(hd, materials.library()['PlasticBeige'])
            a.parts.append(hd)
    return make


def livery(style):
    """Decal panels on both box sides (painted surfaces baked into their own set, laid 3 mm proud)."""
    colours = {'stripes': ((.75, .08, .05), (.85, .85, .82)), 'flames': ((.9, .35, .03), (.85, .1, .03)),
               'hazard': ((.85, .65, .03), (.03, .03, .03)), 'camo': ((.18, .22, .12), (.32, .3, .2)),
               'grime': ((.08, .08, .07), (.3, .25, .18)), 'company': ((.85, .55, .1), (.1, .1, .1))}[style]

    def make(a):
        c1 = materials.paint(f'Livery_{style}_A', colours[0], wear=.4)
        c2 = materials.paint(f'Livery_{style}_B', colours[1], wear=.4)
        rng = random.Random(len(style))
        for s in (-1, 1):
            x = s * 1.433
            if style == 'stripes':
                for k, (y, h) in enumerate(((1.45, .18), (1.7, .07), (1.85, .04))):
                    a.Box((x, y, 0), (.006, h, 5.0), c1 if k == 0 else c2)
            elif style == 'hazard':
                for k in range(16):
                    a.Box((x, 1.3, -2.3 + k * .3), (.006, .32, .14), c1 if k % 2 else c2, (45 * s, 0, 0))
            elif style == 'flames':
                for k in range(9):
                    z0 = 1.5 - k * .12
                    length = rng.uniform(1.4, 2.6)
                    a.Beam((x, 1.3 + k * .05, z0), (x, 1.25 + k * .11, z0 - length), .006, .14 - k * .006, c1 if k % 2 else c2)
            elif style == 'camo':
                for k in range(26):
                    a.Box((x, rng.uniform(1.2, 3.2), rng.uniform(-2.4, 2.4)), (.006, rng.uniform(.2, .5), rng.uniform(.3, .8)),
                          c1 if k % 2 else c2, (rng.uniform(-30, 30), 0, 0))
            elif style == 'grime':
                for k in range(18):
                    a.Box((x, rng.uniform(1.1, 1.9), rng.uniform(-2.4, 2.4)), (.006, rng.uniform(.3, .8), .05),
                          c1 if k % 3 else c2)
            elif style == 'company':
                a.Box((x, 2.2, 0), (.006, 1.0, 3.2), c1)
                a.Box((x, 2.2, 0), (.007, .8, 3.0), c2)
                for k in range(3):
                    a.Box((x, 2.2 + (k - 1) * .2, -.9), (.008, .12, .5), c1, (0, 0, 0))
    return make


# -------------------------------------------------------------------------------------------- visible upgrades

def upgrade(kind):
    def make(a):
        if kind == 'cargo_1':      # side racks for long things
            for s in (-1, 1):
                for z in (-2.3, -.8, .7, 2.2):
                    a.Box((s * 1.5, 2.6, z), (.16, .05, .05), a.Galv)
                    a.Rod((s * 1.58, 2.6, z), (s * 1.58, 2.75, z), .03, a.Galv)
                a.Rod((s * 1.58, 2.75, -2.4), (s * 1.58, 2.75, 2.3), .03, a.Galv)
        elif kind == 'cargo_2':    # roof pod
            a.Box((0, 3.65, 0), (2.4, .45, 4.4), materials.library()['PlasticGrey'])
            for z in (-1.6, 0, 1.6):
                a.Box((0, 3.44, z), (2.5, .06, .12), a.Chassis)
        elif kind == 'engine_1':   # vertical exhaust stack behind the cab
            a.Rod((1.32, .9, 2.68), (1.32, 3.3, 2.68), .13, a.Chrome)
            a.Cylinder((1.32, 3.32, 2.68), (.15, .06, .15), a.Chrome)
            a.Box((1.32, 2.1, 2.62), (.2, 1.2, .04), a.Galv)
        elif kind == 'engine_2':   # snorkel intake and intercooler
            a.Rod((-1.3, 1.2, 4.5), (-1.3, 2.95, 4.5), .12, a.Chassis)
            a.Rod((-1.3, 2.95, 4.5), (-1.3, 2.95, 4.85), .12, a.Chassis)
            a.Box((-1.3, 2.95, 4.9), (.16, .12, .08), a.Chassis)
            a.Box((0, .95, 5.12), (1.3, .12, .04), a.Galv)
        elif kind == 'floodlights':  # an extra light bar on the cab roof
            a.Box((0, 2.92, 4.5), (2.1, .14, .16), a.Chassis)
            for k in range(8):
                a.Box(((k - 3.5) * .24, 2.92, 4.59), (.18, .1, .02), materials.library()['EmitCold'])
        elif kind == 'forecast':   # weather mast and a radar dish
            a.Rod((-.9, 3.4, -2.2), (-.9, 4.5, -2.2), .05, a.Galv)
            dish = kit.lathe('Dish', [(.0, .0), (.18, .03), (.3, .1)], sides=24, cap_bottom=False, cap_top=False)
            kit.solidify(dish, .01)
            dish.rotation_euler = (math.radians(-60), 0, 0)
            dish.location = a.U((-.9, 4.55, -2.15))
            materials.assign(dish, a.Galv)
            a.parts.append(dish)
            for k in range(3):
                a.Rod((-.9, 4.2, -2.2), (-.9 + .25 * math.cos(k * 2.1), 4.25, -2.2 + .25 * math.sin(k * 2.1)), .012, a.Steel)
                a.Cylinder((-.9 + .25 * math.cos(k * 2.1), 4.27, -2.2 + .25 * math.sin(k * 2.1)), (.05, .04, .05), a.Dark)
        elif kind == 'winch':      # a front winch on the bumper
            a.Cylinder((0, .78, 5.3), (.24, .7, .24), a.Chassis, (0, 0, 90))
            a.Cylinder((0, .78, 5.3), (.3, .08, .3), a.Steel, (0, 0, 90))
            a.Box((0, .55, 5.42), (.3, .1, .1), a.Galv)
            a.Rod((0, .7, 5.42), (0, .45, 5.48), .04, a.Steel)
    return make


PARTS = {'Body': body}
PARTS.update({f'Rims_{s}': rims(s) for s in ('steel', 'chrome', 'black', 'gold')})
PARTS.update({f'Bumper_{s}': bumper(s) for s in ('bullbar', 'pushbar', 'plough')})
PARTS.update({f'Roof_{s}': roof(s) for s in ('beacons', 'rack')})
PARTS.update({f'Mudflaps_{s}': mudflaps(s) for s in ('rubber', 'chrome', 'hazard')})
PARTS.update({f'Ornament_{s}': ornament(s) for s in ('bulldog', 'skull', 'eagle', 'bobblehead')})
PARTS.update({f'Livery_{s}': livery(s) for s in ('stripes', 'flames', 'hazard', 'camo', 'grime', 'company')})
PARTS.update({f'Up_{k}': upgrade(k) for k in ('cargo_1', 'cargo_2', 'engine_1', 'engine_2', 'floodlights', 'forecast', 'winch')})


def build(name, show=False):
    kit.reset()
    a = TruckArt()
    PARTS[name](a)
    ob = a.finish('Truck_' + name)
    tint_faces = [p.index for p in ob.data.polygons if ob.data.materials[p.material_index].name == 'RM_' + TINT]
    big = name == 'Body'
    bake.bake_asset(ob, 'Truck_' + name, export.art_dir('Truck', 'Textures'), size=2048 if big else 512, samples=10)
    if tint_faces:
        tm = bpy.data.materials.new('RM_Truck_' + name + '_Tint')
        ob.data.materials.append(tm)
        for i in tint_faces:
            ob.data.polygons[i].material_index = len(ob.data.materials) - 1
    export.static_asset('Truck_' + name, [ob], 'Truck', lods=(1.0, .4, .12) if big else (1.0, .5))
    if show:
        preview.save('Truck_' + name)
    print(f'[remake-truck] {name} tris={kit.tris(ob)} tint_faces={len(tint_faces)}')


def main():
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    show = '--preview' in args
    for n in [x for x in args if not x.startswith('--')] or list(PARTS):
        build(n, show)


if __name__ == '__main__':
    main()
