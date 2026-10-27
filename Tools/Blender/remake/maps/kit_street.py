"""Map kit, street and building fittings: cars, bus shelters, meters, signals, signs, poles, water tanks, windows.
X length, +Y front, Z up, on the ground."""
import math

from remake.core import kit, materials


CARS = [  # (paint, length, height, width, roof start/end, hatch)
    ((.05, .1, .2), 4.6, 1.45, 1.8, False),
    ((.32, .04, .03), 4.15, 1.5, 1.76, True),
    ((.28, .29, .29), 4.7, 1.75, 1.9, True),
    ((.62, .58, .5), 4.6, 1.45, 1.8, False),
]


def car(p, v):
    colour, L, H, W, hatch = CARS[v]
    paint = materials.surface('K_Car%d' % v, colour, .35, 0, var=.05, wear=.3, wear_color=(.4, .4, .41), grime=.55,
                              grime_color=(.12, .1, .08), coat=.6, rust=.05)
    p._made['car%d' % v] = paint
    h0, belt = .3, .55 + H * .27
    f, r = L / 2, -L / 2
    wb = L * .3
    body = [(r + .02, h0), (f - .02, h0), (f + .02, h0 + .2), (f, belt - .08), (f - .25, belt),
            (f * .35 + .3, belt + .06)]
    body += [(r * .55, belt + .08), (r + .15, belt + .03), (r, belt - .1), (r - .02, h0 + .2)] if not hatch else \
        [(r + .25, belt + .1), (r + .05, belt + .02), (r, h0 + .25)]
    ob = p.outline(paint, body, W, axis='Y', bevel=.04)
    for x in (wb, -wb):
        cut = kit.cylinder('arch', .4, W + .2, (x, 0, .33), sides=24, axis='Y')
        kit.boolean(ob, cut)
    # The glasshouse: windows set in, a painted roof and pillars.
    top = H - .03
    rf, rr = (f * .35 + .3) - .85, (r * .55 if not hatch else r + .25)
    roof_r = rr + .5 if not hatch else rr + .12
    glass = [(f * .35 + .3, belt + .06), (rf, top - .02), (roof_r, top), (rr, belt + .08 if not hatch else belt + .1)]
    p.outline('GlassDark', glass, W - .3, axis='Y')
    p.outline(paint, [(rf + .05, top - .04), (rf, top), (roof_r - .02, top + .02), (roof_r + .04, top - .03)], W - .24, axis='Y', bevel=.02)
    for y in (-(W - .3) / 2, (W - .3) / 2):
        p.rod(paint, (f * .35 + .3, y, belt + .06), (rf + .02, y, top - .02), .035, sides=6)
        p.rod(paint, ((rf + roof_r) / 2, y, belt + .07), ((rf + roof_r) / 2, y, top), .04, sides=6)
        p.rod(paint, (rr + .02, y, belt + .08), (roof_r, y, top), .05, sides=6)
    # Wheels.
    for x in (wb, -wb):
        for y in (-W / 2 + .14, W / 2 - .14):
            tyre = p.lathe('Tyre', [(.2, -.1), (.3, -.11), (.33, -.08), (.335, 0), (.33, .08), (.3, .11), (.2, .1)], sides=24)
            tyre.rotation_euler = (math.radians(90), 0, 0)
            tyre.location = (x, y, .335)
            p.cyl('Aluminium', .2, .2, (x, y, .335), axis='Y', sides=16)
            p.cyl('SteelDark', .06, .21, (x, y, .335), axis='Y', sides=10)
    # Bumpers, lights, grille, mirrors, handles.
    p.slab('Plastic', f - .08, f + .05, -W / 2 + .02, W / 2 - .02, h0 - .02, h0 + .18, bevel=.03)
    p.slab('Plastic', r - .05, r + .08, -W / 2 + .02, W / 2 - .02, h0 - .02, h0 + .18, bevel=.03)
    p.slab('PaintBlack', f - .02, f + .025, -.4, .4, h0 + .22, belt - .12, bevel=.01)
    for y in (-W / 2 + .2, W / 2 - .2):
        p.box('Chrome', (.05, .3, .1), (f, y, belt - .12), bevel=.02)
        p.box('PlasticRed', (.05, .28, .12), (r, y, belt - .1), bevel=.02)
        p.box(paint, (.12, .06, .08), (rf + .25, y + (.06 if y > 0 else -.06), belt + .1), bevel=.02)
        for x in (.2, -.6):
            p.slab('Chrome', x - .08, x + .08, y + (.0 if y > 0 else -.025), y + (.025 if y > 0 else .0), belt - .07, belt - .05)


def bus_shelter(p, v):
    w, h, d = 3.9, 2.6, 1.7
    for x in (-w / 2 + .05, w / 2 - .05):
        for y in (-d / 2 + .05, d / 2 - .1):
            p.slab('Aluminium', x - .04, x + .04, y - .04, y + .04, 0, h - .1)
    p.slab('Aluminium', -w / 2, w / 2, -d / 2, d / 2, h - .12, h - .04, bevel=.01)
    p.slab('GlassDark', -w / 2 + .05, w / 2 - .05, -d / 2 - .01, d / 2 + .02, h - .05, h - .03)
    p.slab('Glass', -w / 2 + .09, w / 2 - .09, -d / 2 + .04, -d / 2 + .06, .15, h - .2)
    for x in (-w / 2 + .05, w / 2 - .05):
        p.slab('Glass', x - .01, x + .01, -d / 2 + .09, d / 2 - .4, .15, h - .2)
    p.slab('Aluminium', -w / 2 + .09, w / 2 - .09, -d / 2 + .03, -d / 2 + .07, .1, .15)
    p.slab('Wood', -1.2, 1.2, -d / 2 + .1, -d / 2 + .45, .44, .48, bevel=.006)
    for x in (-1.0, 1.0):
        p.slab('SteelDark', x - .03, x + .03, -d / 2 + .15, -d / 2 + .4, 0, .44)


def parking_meter(p, v):
    p.cyl('PaintGrey', .035, 1.12, (0, 0, .56), sides=12)
    p.cyl('SteelDark', .08, .03, (0, 0, .015), sides=12)
    p.box(p.paint('Meter', (.2, .22, .2), wear=.5), (.16, .13, .26), (0, 0, 1.25), bevel=.02)
    head = p.lathe(p.paint('Meter', (.2, .22, .2), wear=.5), [(.0, 1.38), (.07, 1.38), (.08, 1.42), (.06, 1.48), (.0, 1.5)], sides=16)
    head.scale = (1, .8, 1)
    p.slab('GlassDark', -.05, .05, .065, .07, 1.3, 1.42)
    p.slab('Chrome', -.02, .02, .065, .075, 1.18, 1.22)


def traffic_signal(p, v):
    pole = p.paint('SignalPole', (.3, .31, .3), wear=.4) if v == 0 else 'Galvanised'
    x0 = -2.4
    p.lathe(pole, [(.0, 0), (.16, 0), (.16, .08), (.1, .1), (.09, 6.5), (.08, 6.8), (0, 6.8)], (x0, 0, 0), sides=16)
    p.rod(pole, (x0, 0, 6.3), (2.4, 0, 6.3), .06, sides=12)
    p.rod(pole, (x0, 0, 5.4), (x0 + 1.4, 0, 6.28), .035, sides=8)
    housing = p.paint('SignalHead', (.55, .45, .05), wear=.4) if v == 0 else 'PaintBlack'
    lenses = ('PlasticRed', 'PlasticYellow', p.surface('LensGreen', (.05, .3, .12), .1, var=.02, grime=.3))
    for x in (.6, 2.0):
        p.slab(housing, x - .17, x + .17, -.14, .14, 5.15, 6.2, bevel=.02)
        p.rod(housing, (x, 0, 6.2), (x, 0, 6.3), .03)
        for k, lens in enumerate(lenses):
            z = 5.98 - k * .33
            p.cyl(lens, .1, .02, (x, .15, z), axis='Y', sides=16)
            p.box(housing, (.26, .16, .02), (x, .22, z + .11), bevel=.004, rot=(-15, 0, 0))
    p.box('PaintBlack', (.24, .14, .36), (x0, .2, 1.1), bevel=.02)


def stop_sign(p, v):
    p.slab('Galvanised', -.025, .025, -.015, .015, 0, 2.35, bevel=.003)
    oct8 = [(.38 * math.cos(math.radians(22.5 + 45 * i)), 2.3 + .38 * math.sin(math.radians(22.5 + 45 * i))) for i in range(8)]
    inner = [(.34 * math.cos(math.radians(22.5 + 45 * i)), 2.3 + .34 * math.sin(math.radians(22.5 + 45 * i))) for i in range(8)]
    p.outline('PaintWhite', oct8, .008, at=(0, .02, 0))
    p.outline('PaintRed', inner, .006, at=(0, .026, 0))
    for z in (2.15, 2.45):
        p.cyl('SteelDark', .012, .03, (0, .03, z), axis='Y', sides=8)


def utility_pole(p, v):
    p.lathe(p.surface('PoleWood', (.2, .15, .1), .85, var=.2, scale=2, grime=.5, bump=.4), [(.0, 0), (.15, 0), (.14, 2), (.11, 8.5), (0, 8.5)], sides=14)
    p.slab('WoodRaw', -.85, .85, -.05, .05, 7.85, 7.97, bevel=.006)
    for x in (-.75, -.3, .3, .75):
        p.lathe(p.surface('Insulator', (.2, .3, .25), .2, var=.03, grime=.3, coat=.6), [(.02, 0), (.05, .02), (.04, .05), (.05, .08), (.03, .12), (0, .13)], (x, 0, 7.97), sides=12)
    for x in (-.6, .6):
        p.rod('Galvanised', (x, 0, 7.86), (0, 0, 7.4), .015)
    if v == 0:
        p.cyl('PaintGrey', .25, .8, (0, .38, 6.6), sides=18)
        p.cyl('PaintGrey', .26, .04, (0, .38, 7.02), sides=18)
        p.slab('Galvanised', -.05, .05, .1, .14, 6.3, 6.95)


def water_tank(p, v):
    r, legs = 1.7, 2.0
    for a in range(4):
        ang = math.radians(45 + 90 * a)
        x, y = math.cos(ang) * (r - .15), math.sin(ang) * (r - .15)
        p.slab('RustedSteel', x - .08, x + .08, y - .08, y + .08, 0, legs, bevel=.006)
    for z in (.7, 1.4):
        p.lathe('RustedSteel', [(r - .2, z - .03), (r - .15, z - .03), (r - .15, z + .03), (r - .2, z + .03)], sides=4).rotation_euler = (0, 0, math.radians(45))
    p.cyl('WoodRaw', r + .1, .12, (0, 0, legs + .06), sides=24)
    p.lathe(p.surface('TankStaves', (.3, .22, .14), .85, var=.25, scale=2, grime=.55, bump=.35), [(0, legs + .12), (r, legs + .12), (r - .05, legs + 2.2), (0, legs + 2.2)], sides=32)
    for z in (legs + .4, legs + 1.0, legs + 1.6, legs + 2.05):
        rr = r - .05 * (z - legs) / 2.1 + .015
        p.lathe('SteelDark', [(rr - .01, z - .02), (rr, z), (rr - .01, z + .02)], sides=32)
    p.lathe(p.surface('TankRoof', (.12, .1, .08), .7, var=.15, grime=.5), [(0, legs + 3.0), (r + .1, legs + 2.15), (r + .1, legs + 2.1)], sides=32)
    p.tube('SteelDark', [(r - .1, 0, legs + .15), (r + .2, 0, legs - .2), (r + .2, 0, .0)], .06)


def window(p, v):
    w, h, d = 1.6, 2.0, .2
    if v == 0:
        # Aluminium commercial window: frame, transom and a centre mullion, smoked glass.
        al = 'Aluminium'
        p.frame(al, -w / 2, w / 2, 0, h, 0, .07, .12)
        p.slab(al, -w / 2 + .07, w / 2 - .07, -.05, .05, h * .72 - .03, h * .72 + .03)
        p.slab(al, -.03, .03, -.05, .05, .07, h * .72 - .03)
        p.slab('GlassDark', -w / 2 + .06, w / 2 - .06, -.01, .01, .06, h - .06)
        p.slab('Concrete', -w / 2 - .04, w / 2 + .04, -d / 2, d / 2, -.05, .0, bevel=.008)
    elif v == 1:
        # Painted timber sash window: two sashes of four panes, a stone sill.
        wood = materials.paint('K_Sash', (.7, .69, .64), wear=.55, rough=.55)
        p._made['sash'] = wood
        p.frame(wood, -w / 2, w / 2, 0, h, 0, .09, .14)
        for z0, z1, y in ((.09, h / 2 + .02, .03), (h / 2 - .02, h - .09, -.03)):
            p.frame(wood, -w / 2 + .08, w / 2 - .08, z0, z1, y, .05, .05)
            p.slab(wood, -.016, .016, y - .02, y + .02, z0 + .05, z1 - .05)
            p.slab(wood, -w / 2 + .12, w / 2 - .12, y - .02, y + .02, (z0 + z1) / 2 - .014, (z0 + z1) / 2 + .014)
            p.slab('Glass', -w / 2 + .12, w / 2 - .12, y - .004, y + .004, z0 + .04, z1 - .04)
        p.slab('Stone', -w / 2 - .06, w / 2 + .06, -d / 2 + .02, d / 2 + .04, -.07, .0, bevel=.01)
    else:
        # Steel factory window: a grid of small panes with an opening vent.
        steel = 'SteelDark'
        p.frame(steel, -w / 2, w / 2, 0, h, 0, .06, .1)
        cols, rows = 4, 6
        for i in range(1, cols):
            x = -w / 2 + i * w / cols
            p.slab(steel, x - .012, x + .012, -.025, .025, .06, h - .06)
        for j in range(1, rows):
            z = j * h / rows
            p.slab(steel, -w / 2 + .06, w / 2 - .06, -.025, .025, z - .012, z + .012)
        p.slab(p.surface('FactoryGlass', (.35, .4, .38), .35, var=.2, grime=.8, alpha=.7), -w / 2 + .05, w / 2 - .05, -.005, .005, .05, h - .05)
        p.box(steel, (w / 2 - .04, .03, h / rows - .02), (-w / 4 + .02, .06, h / 2 + h / rows / 2), bevel=.003, rot=(-20, 0, 0))
        p.slab('Concrete', -w / 2 - .03, w / 2 + .03, -d / 2, d / 2, -.06, .0, bevel=.008)


FAMILIES = {
    'car': (car, 4, 1024), 'bus_shelter': (bus_shelter, 1, 1024), 'parking_meter': (parking_meter, 1, 512),
    'traffic_signal': (traffic_signal, 2, 512), 'stop_sign': (stop_sign, 1, 256), 'utility_pole': (utility_pole, 2, 512),
    'water_tank': (water_tank, 1, 1024), 'window': (window, 3, 512),
}
