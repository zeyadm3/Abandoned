"""Vehicles of the 2026 remake that weren't the truck: the crew's arrival van and the tenants' vans (in place of the
Kenney Car Kit's van and delivery van) and the Island Prison's supply launch (its hull and wheelhouse).

Vans: length along Blender Y (Unity Z), front +Y, on the ground, centred; Editor/Art/Vehicles fits them into the old
boxes. The launch is drawn in the boat's own space (IslandBoat: the "Truck" root at the middle of the hold, +Y the bow,
the hold floor at z 1.05), so it drops in at the origin.

Batch:
  Blender --background --factory-startup --python Tools/Blender/remake/vehicles/vehicles.py -- [van delivery boat_hull boat_wheelhouse] [--preview]
Writes Art/Remake/Vehicles/Vehicle_<name>.fbx and Textures/.
"""
import math
import os
import sys

import bpy

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(os.path.dirname(_HERE)))
exec(open(os.path.join(os.path.dirname(_HERE), 'boot.py')).read())

from remake.core import bake, export, kit, materials, preview  # noqa: E402
from remake.maps.parts import Parts  # noqa: E402


def _profile(p, mat, points, width):
    """A side elevation (y, z) extruded across the width (X), the way a van's body is drawn."""
    ob = p.outline(mat, points, width, axis='Y', bevel=.035)
    ob.rotation_euler = (0, 0, math.radians(90))
    return ob


def _wheel(p, y, x, r=.37):
    tyre = p.lathe('Tyre', [(.22, -.11), (r - .04, -.12), (r, -.08), (r + .005, 0), (r, .08), (r - .04, .12), (.22, .11)], sides=24)
    tyre.rotation_euler = (0, math.radians(90), 0)
    tyre.location = (x, y, r)
    p.cyl('Steel', .21, .2, (x, y, r), axis='X', sides=16)
    p.cyl('SteelDark', .07, .22, (x, y, r), axis='X', sides=10)
    for k in range(5):
        a = math.radians(72 * k)
        p.cyl('Chrome', .014, .03, (x + (.1 if x > 0 else -.1), y + math.cos(a) * .12, r + math.sin(a) * .12), axis='X', sides=6)


def van(p, delivery=False):
    L, W = 5.7, 2.2
    f, r = L / 2, -L / 2
    paint = p.paint('VanWhite' if not delivery else 'VanBlue', (.74, .74, .72) if not delivery else (.12, .2, .34), wear=.35, rough=.4)
    if not delivery:
        body = [(r, .38), (f - .1, .38), (f, .55), (f, .95), (f - .2, 1.17), (1.85, 1.32), (1.15, 2.28), (.95, 2.44), (r + .05, 2.44), (r, 2.3)]
    else:
        body = [(1.0, .38), (f - .1, .38), (f, .55), (f, .95), (f - .2, 1.17), (1.85, 1.32), (1.25, 2.15), (1.0, 2.2)]
    ob = _profile(p, paint, body, W)
    if delivery:
        box = p.slab(p.paint('BoxWhite', (.78, .78, .76), wear=.3), -W / 2 - .03, W / 2 + .03, r, .95, .45, 2.55, bevel=.03)
        p.slab('SteelDark', -W / 2, W / 2, r - .03, 1.0, .36, .48)
        cuts = [box, ob]
    else:
        cuts = [ob]
    for y in (f - 1.0, r + 1.05):
        for target in cuts:
            arch = kit.cylinder('arch', .45, W + .3, (0, y, .38), sides=24, axis='X')
            kit.boolean(target, arch)
    # Glass: the windscreen and the cab's door windows.
    ws = p.box('GlassDark', (W - .2, .03, 1.0), (0, 1.52, 1.82), bevel=.01)
    ws.rotation_euler = (math.radians(-50), 0, 0)
    for s in (-1, 1):
        p.slab('GlassDark', s * (W / 2) - .02, s * (W / 2) + .02, .95, 1.75, 1.45, 2.12)
        p.box(paint, (.06, .18, .12), (s * (W / 2 + .1), 1.75, 1.75), bevel=.02)  # mirror arm
        p.box('GlassDark', (.04, .14, .2), (s * (W / 2 + .17), 1.72, 1.75), bevel=.01)
        # Door seams and handles.
        for y in (.92, 1.8):
            p.slab('PaintBlack', s * (W / 2) - .012 * (s < 0) - .002, s * (W / 2) + .012 * (s > 0) + .002, y - .008, y + .008, .45, 2.15)
        p.slab('Chrome', s * (W / 2) - .02, s * (W / 2) + .02, 1.0, 1.12, 1.25, 1.29)
    if not delivery:
        # Sliding door rail, rear door seam, a company band (painted, never lettered).
        for s in (-1, 1):
            p.slab('SteelDark', s * (W / 2) - .015, s * (W / 2) + .015, -1.8, .9, 2.05, 2.08)
            p.slab(p.paint('VanBand', (.86, .42, .1), wear=.45), s * (W / 2) - .006, s * (W / 2) + .006, r + .2, .85, 1.0, 1.14)
        p.slab('PaintBlack', -.008, .008, r - .01, r + .005, .5, 2.3)
        for x in (-.55, .55):
            p.slab('GlassDark', x - .38, x + .38, r - .012, r + .004, 1.55, 2.15)
    # Front: grille, lights, bumpers.
    p.slab('PaintBlack', -.62, .62, f - .02, f + .02, .62, .92, bevel=.01)
    for s in (-1, 1):
        p.box('Chrome', (.3, .04, .16), (s * .8, f, .86), bevel=.02)
        p.box('PlasticRed', (.14, .04, .3), (s * (W / 2 - .12), r - .01, 1.15), bevel=.015)
    p.slab('Plastic', -W / 2, W / 2, f - .05, f + .09, .38, .58, bevel=.03)
    p.slab('Plastic', -W / 2, W / 2, r - .09, r + .05, .38, .55, bevel=.03)
    p.slab('SteelDark', -W / 2 + .1, W / 2 - .1, r + .3, f - .3, .3, .4)
    for y in (f - 1.0, r + 1.05):
        for x in (-W / 2 + .12, W / 2 - .12):
            _wheel(p, y, x)


def boat_hull(p):
    """The launch's hull round the hold: a hard-chined steel workboat, the hold open on top."""
    stern, mid, bow = -2.7, 5.9, 12.0
    hull_paint = p.paint('HullTeal', (.07, .2, .22), wear=.6, rough=.5)
    antifoul = p.surface('Antifoul', (.32, .07, .05), .75, var=.2, grime=.6, rust=.25, bump=.25)
    boot = p.paint('BootTop', (.75, .74, .7), wear=.5)

    def width(y):
        if y <= mid:
            return 1.75
        t = (y - mid) / (bow - mid)
        return max(.04, 1.75 * math.cos(t * math.pi / 2) ** .8)

    def keel(y):
        if y <= mid:
            return -2.4 + max(0, (stern + .6 - y)) * .6
        t = (y - mid) / (bow - mid)
        return -2.4 + t * t * 3.6

    def sheer(y):
        return 2.05 + max(0, y - mid) / (bow - mid) * .5

    sections = []
    n = 28
    for i in range(n + 1):
        y = stern + (bow - stern) * i / n
        b, k, top = width(y), keel(y), sheer(y)
        chine = k + (top - k) * .38
        ring = [(-b, y, top), (-b * .98, y, chine), (-b * .35, y, k + .12), (0, y, k), (b * .35, y, k + .12), (b * .98, y, chine), (b, y, top)]
        sections.append(ring)
    shell = kit.loft('Hull', sections, closed=False, cap=False, smooth=False)
    kit.solidify(shell, .07, offset=1)
    kit.apply(shell)
    mats = [hull_paint, antifoul, boot]
    for m in mats:
        shell.data.materials.append(m)
    for poly in shell.data.polygons:
        z = poly.center.z
        poly.material_index = 1 if z < -1.0 else 2 if z < -.82 else 0
    p.obs.append(shell)
    # The transom (the hold's open stern sits above it) and the stern's guard.
    b, k = width(stern), keel(stern)
    outline = [(-b, 1.05), (-b * .98, k + (2.05 - k) * .38), (-b * .35, k + .12), (0, k), (b * .35, k + .12), (b * .98, k + (2.05 - k) * .38), (b, 1.05)]
    tr = p.outline(hull_paint, outline, .08, axis='Y')
    tr.location = (0, stern, 0)
    # Gunwale cap and rubbing strakes following the sheer.
    for s in (-1, 1):
        pts = [(s * (width(y) + .03), y, sheer(y) + .02) for y in [stern + i * (bow - .3 - stern) / 20 for i in range(21)]]
        p.tube('WoodDark', pts, .05, sides=8)
        pts = [(s * (width(y) + .05), y, sheer(y) - .35) for y in [stern + i * (bow - .8 - stern) / 20 for i in range(21)]]
        p.tube('Rubber', pts, .06, sides=8)
        for k2 in range(4):
            y = stern + 1.2 + k2 * 2.4
            t = p.lathe('Tyre', [(.12, -.1), (.22, -.12), (.25, 0), (.22, .12), (.12, .1)], sides=16)
            t.rotation_euler = (0, math.radians(90), 0)
            t.location = (s * (width(y) + .2), y, .9)
            p.rod('Canvas', (s * (width(y) + .2), y, 1.12), (s * width(y), y, sheer(y)), .012)
    # Foredeck, winch, bollards, the anchor and the navigation lights.
    deck = [(-width(mid), mid), (width(mid), mid)] + [(width(y), y) for y in [mid + i * (bow - mid) / 8 for i in range(1, 9)]]
    deck += [(-width(y), y) for y in [mid + i * (bow - mid) / 8 for i in range(8, 0, -1)]]
    fd = p.outline('Galvanised', [(x, y) for x, y in deck], .06, axis='Z')
    fd.location = (0, 0, 1.95)
    p.box('RustedSteel', (.7, .5, .4), (0, mid + 1.6, 2.2), bevel=.04)
    p.cyl('SteelDark', .18, .5, (0, mid + 1.6, 2.22), axis='X', sides=16)
    for x, y in ((-1.1, mid + .5), (1.1, mid + .5), (-1.2, stern + .4), (1.2, stern + .4)):
        p.cyl('CastIron', .09, .3, (x, y, sheer(y) + .15), sides=12)
        p.cyl('CastIron', .13, .04, (x, y, sheer(y) + .31), sides=12)
    p.lathe('Canvas', [(.0, 2.0), (.36, 2.0), (.36, 2.06), (.0, 2.06)], (.6, mid + 3, 0), sides=16)
    p.box('CastIron', (.08, .5, .6), (width(bow - 1.4) + .02, bow - 1.4, 1.9), bevel=.02)
    p.box('EmitRed', (.06, .1, .08), (-width(mid + 1) - .02, mid + 1, sheer(mid + 1) + .1))
    p.box('EmitGreen', (.06, .1, .08), (width(mid + 1) + .02, mid + 1, sheer(mid + 1) + .1))


def boat_wheelhouse(p):
    """Forward of the hold: the wheelhouse over the old solid block, its windows, roof, mast, funnel and lifebelt."""
    y0, y1, z0, z1, w = 2.6, 5.7, 1.05, 3.65, 1.5
    white = p.paint('WheelhouseWhite', (.78, .78, .75), wear=.45)
    trim = p.paint('WheelhouseTrim', (.07, .2, .22), wear=.5)
    # Walls with window bands: below and above the glass, posts between.
    for s in (-1, 1):
        p.slab(white, s * w - .05, s * w + .05, y0, y1, z0, z0 + 1.35, bevel=.01)
        p.slab(white, s * w - .05, s * w + .05, y0, y1, z0 + 2.15, z1, bevel=.01)
        for y in (y0, y0 + 1.0, y0 + 2.0, y1):
            p.slab(white, s * w - .05, s * w + .05, y - .06, y + .06, z0 + 1.35, z0 + 2.15)
        p.slab('GlassDark', s * w - .02, s * w + .02, y0, y1, z0 + 1.35, z0 + 2.15)
    p.slab(white, -w, w, y1 - .05, y1 + .05, z0, z0 + 1.45, bevel=.01)
    p.slab(white, -w, w, y1 - .05, y1 + .05, z0 + 2.15, z1, bevel=.01)
    for x in (-w, -.05, w):
        p.slab(white, x - .06, x + .06, y1 - .05, y1 + .05, z0 + 1.45, z0 + 2.15)
    p.slab('GlassDark', -w, w, y1 - .02, y1 + .02, z0 + 1.45, z0 + 2.15)
    p.slab(white, -w, w, y0 - .05, y0 + .05, z0, z1, bevel=.01)
    p.slab(trim, -.4, .4, y0 - .07, y0 - .04, z0, z0 + 2.0)
    p.slab(trim, -w - .2, w + .2, y0 - .2, y1 + .3, z1, z1 + .14, bevel=.02)
    p.slab(trim, -w, w, y0, y1, z0 + 1.28, z0 + 1.35)
    # Mast, lamp, yard, radar; funnel; searchlight; lifebelt.
    p.cyl('Steel', .06, 4.0, (0, y1 - .4, z1 + 2.1), sides=10)
    p.sphere('EmitWarm', .1, (0, y1 - .4, z1 + 4.15))
    p.rod('Steel', (-.8, y1 - .4, z1 + 3.2), (.8, y1 - .4, z1 + 3.2), .03)
    p.box('PaintWhite', (.9, .12, .08), (0, y1 - .4, z1 + 2.5), bevel=.02)
    p.lathe(trim, [(.0, z1), (.32, z1), (.3, z1 + 1.2), (.34, z1 + 1.25), (.0, z1 + 1.25)], (0, y0 + .7, 0), sides=16)
    p.lathe('PaintBlack', [(.31, z1 + 1.1), (.33, z1 + 1.1), (.33, z1 + 1.26), (.31, z1 + 1.26)], (0, y0 + .7, 0), sides=16)
    p.cyl('Chrome', .14, .25, (.9, y1 - .2, z1 + .32), axis='Y', sides=14)
    ring = p.lathe(p.paint('Lifebelt', (.8, .25, .06), wear=.4), [(.17, -.05), (.25, -.05), (.28, 0), (.25, .05), (.17, .05), (.14, 0), (.17, -.05)], sides=20)
    ring.rotation_euler = (0, math.radians(90), 0)
    ring.location = (w + .08, y0 + .6, z0 + 1.0)


BUILDS = {
    'van': (lambda p: van(p), 1024),
    'delivery': (lambda p: van(p, delivery=True), 1024),
    'boat_hull': (boat_hull, 2048),
    'boat_wheelhouse': (boat_wheelhouse, 1024),
}
BUDGET = {'van': 9000, 'delivery': 9000, 'boat_hull': 12000, 'boat_wheelhouse': 6000}


def build(name, show=False):
    kit.reset()
    maker, tex = BUILDS[name]
    p = Parts()
    maker(p)
    bpy.context.view_layer.update()
    ob = kit.join(p.obs, f'Vehicle_{name}')
    kit.auto_smooth(ob, 35)
    kit.decimate_to(ob, BUDGET[name], delimit=('UV', 'MATERIAL', 'NORMAL'))
    bake.bake_asset(ob, f'Vehicle_{name}', export.art_dir('Vehicles', 'Textures'), size=tex, samples=10)
    export.static_asset(f'Vehicle_{name}', [ob], 'Vehicles', lods=(1.0, .45, .15))
    if show:
        preview.save(f'Vehicle_{name}')
    print(f'[remake-vehicle] {name} tris={kit.tris(ob)}')


def main():
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    show = '--preview' in args
    for name in [a for a in args if not a.startswith('--')] or list(BUILDS):
        try:
            build(name, show)
        except Exception as e:  # noqa: BLE001
            import traceback
            traceback.print_exc()
            print(f'[remake-vehicle] FAILED {name}: {e}')


if __name__ == '__main__':
    main()
