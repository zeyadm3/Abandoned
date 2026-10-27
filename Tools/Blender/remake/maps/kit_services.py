"""Map kit, services and stock: bathrooms, kitchens, laundry, heating and cooling, crates, drums, bins, carts.
X length, +Y front, Z up, on the floor."""
import math


def sink(p, v):
    if v == 0:
        # Pedestal basin.
        p.lathe('Porcelain', [(.0, 0), (.1, 0), (.08, .1), (.07, .5), (.11, .68), (.14, .72)], (0, .02, 0), sides=20)
        p.box('Porcelain', (.56, .44, .12), (0, .02, .78), bevel=.04)
        p.box('Porcelain', (.42, .3, .06), (0, .05, .83), bevel=.03)
        p.slab('Porcelain', -.28, .28, -.2, -.16, .78, .95, bevel=.01)
    else:
        # Vanity unit with an inset basin.
        p.slab('PaintWhite', -.3, .3, -.24, .22, .08, .82, bevel=.005)
        p.slab('SteelDark', -.28, .28, -.22, .2, 0, .08)
        for x0, x1 in ((-.29, -.003), (.003, .29)):
            p.slab('PaintWhite', x0, x1, .22, .24, .1, .8, bevel=.004)
        p.slab(p.surface('VanityTop', (.62, .6, .56), .3, var=.08, grime=.35, coat=.4), -.31, .31, -.25, .25, .82, .85, bevel=.006)
        p.box('Porcelain', (.4, .3, .04), (0, .03, .86), bevel=.015)
    p.cyl('Chrome', .014, .14, (0, -.13, .92 if v == 0 else .93), sides=10)
    p.rod('Chrome', (0, -.13, .99 if v == 0 else 1.0), (0, -.03, .99 if v == 0 else 1.0), .011)
    for x in (-.08, .08):
        p.cyl('Chrome', .018, .03, (x, -.14, .88 if v == 0 else .87), sides=10)


def toilet(p, v):
    # Bowl to the front (+Y), cistern at the back.
    p.lathe('Porcelain', [(0, 0), (.12, 0), (.11, .08), (.13, .25), (.17, .38), (.19, .4)], (0, .1, 0), sides=20).scale = (1, 1.25, 1)
    p.box('Porcelain', (.38, .46, .03), (0, .1, .415), bevel=.012)
    p.box('PaintWhite', (.36, .42, .02), (0, .1, .44), bevel=.01)
    p.box('Porcelain', (.42, .18, .36), (0, -.22, .58), bevel=.025)
    p.box('Porcelain', (.44, .2, .03), (0, -.22, .775), bevel=.01)
    p.cyl('Chrome', .012, .03, (-.15, -.12, .7), axis='Y', sides=8)


def bathtub(p, v):
    w, d, h = 1.7, .75, .55
    enamel = p.surface('Enamel', (.82, .81, .77), .15, var=.03, grime=.5, coat=.8)
    p.slab(enamel, -w / 2, w / 2, -d / 2, d / 2, 0, h, bevel=.03)
    p.box(p.surface('TubInside', (.75, .74, .7), .2, var=.04, grime=.6, coat=.6), (w - .14, d - .14, .02), (0, 0, h + .001), bevel=.01)
    for x in (-w / 2 + .04, w / 2 - .04):
        p.slab(enamel, x - .04, x + .04, -d / 2 + .02, d / 2 - .02, h - .02, h + .03, bevel=.02)
    p.cyl('Chrome', .02, .1, (-w / 2 + .12, 0, h + .08), sides=10)


def fridge(p, v):
    w, d, h = .75, .72, 1.8
    body = p.surface('FridgeWhite', (.75, .74, .7), .35, var=.04, grime=.45, coat=.3, wear=.15)
    p.slab(body, -w / 2, w / 2, -d / 2, d / 2 - .05, .05, h, bevel=.02)
    p.slab('PlasticGrey', -w / 2 + .03, w / 2 - .03, -d / 2 + .03, d / 2 - .06, 0, .05)
    for z0, z1 in ((.06, 1.15), (1.17, h - .01)):
        p.slab(body, -w / 2 + .005, w / 2 - .005, d / 2 - .05, d / 2, z0, z1, bevel=.02)
        p.rod('Aluminium', (w / 2 - .07, d / 2 + .03, z0 + .12 if z0 > 1 else z1 - .5), (w / 2 - .07, d / 2 + .03, z0 + .4 if z0 > 1 else z1 - .1), .012)


def stove(p, v):
    w, d, h = .75, .65, .9
    body = p.surface('StoveWhite', (.72, .71, .67), .35, var=.04, grime=.5, coat=.3, wear=.15)
    p.slab(body, -w / 2, w / 2, -d / 2, d / 2, .06, h, bevel=.008)
    p.slab('SteelDark', -w / 2 + .02, w / 2 - .02, -d / 2 + .02, d / 2 - .04, 0, .06)
    p.slab('SteelDark', -w / 2 + .01, w / 2 - .01, -d / 2 + .01, d / 2 - .01, h - .005, h, bevel=.002)
    for x in (-.17, .17):
        for y in (-.14, .14):
            p.cyl('CastIron', .09, .015, (x, y, h + .006), sides=16)
            p.cyl('CastIron', .04, .02, (x, y, h + .01), sides=12)
    p.slab(body, -w / 2 + .02, w / 2 - .02, d / 2, d / 2 + .02, .12, .68, bevel=.01)
    p.slab('GlassDark', -.22, .22, d / 2 + .02, d / 2 + .025, .28, .6, bevel=.004)
    p.rod('Aluminium', (-.25, d / 2 + .06, .64), (.25, d / 2 + .06, .64), .011)
    p.slab(body, -w / 2, w / 2, -d / 2 - .02, -d / 2 + .05, h, h + .14, bevel=.008)
    for i in range(4):
        p.cyl('Plastic', .022, .025, (-.24 + i * .16, d / 2 + .03, .76), axis='Y', sides=12)


def washer(p, v):
    w, d, h = .7, .7, .9
    body = p.surface('WasherWhite', (.75, .75, .72), .35, var=.04, grime=.45, coat=.3, wear=.12)
    p.slab(body, -w / 2, w / 2, -d / 2, d / 2, .02, h, bevel=.02)
    p.slab('PlasticGrey', -w / 2 + .03, w / 2 - .03, -d / 2 + .03, d / 2 - .03, 0, .02)
    p.lathe('Chrome', [(.0, 0), (.22, 0), (.24, .02), (.2, .04), (.0, .045)], (0, d / 2, .42), sides=24).rotation_euler = (math.radians(-90), 0, 0)
    p.cyl('GlassDark', .17, .02, (0, d / 2 + .02, .42), axis='Y', sides=24)
    p.slab('PlasticGrey', -w / 2 + .02, w / 2 - .02, d / 2 - .01, d / 2 + .01, h - .13, h - .02, bevel=.008)
    p.cyl('Chrome', .035, .025, (.18, d / 2 + .02, h - .075), axis='Y', sides=16)
    p.slab('PlasticGrey', -.3, -.08, d / 2 + .005, d / 2 + .015, h - .11, h - .04, bevel=.004)


def vending(p, v):
    w, d, h = 1.0, .85, 1.85
    body = p.paint('Vending', (.35, .04, .04) if v == 0 else (.05, .1, .22), wear=.35)
    p.slab(body, -w / 2, w / 2, -d / 2, d / 2 - .05, .05, h, bevel=.012)
    p.slab('SteelDark', -w / 2 + .03, w / 2 - .03, -d / 2 + .03, d / 2 - .08, 0, .05)
    p.slab('GlassDark', -w / 2 + .05, w / 2 - .3, d / 2 - .05, d / 2 - .03, .45, h - .1, bevel=.004)
    p.frame('SteelDark', -w / 2 + .03, w / 2 - .28, .43, h - .08, d / 2 - .03, .025, .03)
    p.slab('PaintBlack', w / 2 - .26, w / 2 - .04, d / 2 - .05, d / 2 - .02, .45, h - .1, bevel=.006)
    p.slab('Screen', w / 2 - .22, w / 2 - .08, d / 2 - .02, d / 2 - .015, h - .45, h - .3)
    for i in range(12):
        x = w / 2 - .21 + (i % 3) * .05
        z = h - .7 - (i // 3) * .05
        p.slab('Aluminium', x, x + .035, d / 2 - .02, d / 2 - .01, z, z + .035, bevel=.004)
    p.slab('PaintBlack', -w / 2 + .08, w / 2 - .35, d / 2 - .06, d / 2 - .02, .12, .32, bevel=.01)


def radiator(p, v):
    w, h, d = 1.2, .6, .15
    iron = p.paint('Radiator', (.62, .6, .55), wear=.45, rough=.55)
    n = 14
    for i in range(n):
        x = -w / 2 + (i + .5) * w / n
        p.box(iron, (w / n - .012, d * .8, h - .08), (x, 0, h / 2 + .03), bevel=.012)
    p.rod(iron, (-w / 2, 0, .1), (w / 2, 0, .1), .025)
    p.rod(iron, (-w / 2, 0, h - .04), (w / 2, 0, h - .04), .025)
    for x in (-w / 2 + .05, w / 2 - .05):
        p.slab(iron, x - .03, x + .03, -d / 2, d / 2, 0, .06, bevel=.008)
    p.cyl('Brass', .02, .06, (w / 2 + .03, 0, .1), axis='X', sides=10)


def hvac(p, v):
    w, h, d = 1.2, 1.0, .8
    if v == 0:
        # Condenser: louvred cabinet, a fan grille on top.
        body = p.surface('HvacBody', (.55, .55, .52), .45, metal=.6, var=.06, grime=.5, wear=.2, rust=.08)
        p.slab(body, -w / 2, w / 2, -d / 2, d / 2, .08, h - .05, bevel=.008)
        p.slab('Galvanised', -w / 2 - .02, w / 2 + .02, -d / 2 - .02, d / 2 + .02, h - .05, h, bevel=.006)
        p.cyl('SteelDark', .3, .02, (0, 0, h + .005), sides=24)
        for k in range(5):
            p.lathe('SteelDark', [(.06 * k + .04, h + .01), (.06 * k + .05, h + .01)], sides=24)
        p.cyl('SteelDark', .06, .03, (0, 0, h + .02), sides=12)
        for i in range(12):
            z = .14 + i * .065
            p.box('SteelDark', (w - .1, .012, .03), (0, d / 2 + .006, z), bevel=.002, rot=(30, 0, 0))
        for x in (-w / 2 + .05, w / 2 - .05):
            p.slab('Galvanised', x - .04, x + .04, -d / 2, d / 2, 0, .08)
        p.rod('Copper', (w / 2, -d / 4, .3), (w / 2 + .1, -d / 4, .3), .012)
        p.rod('Copper', (w / 2, -d / 4 + .06, .25), (w / 2 + .1, -d / 4 + .06, .25), .008)
    else:
        # Wall air conditioner: a box with a front grille.
        body = p.surface('WallAc', (.72, .7, .64), .45, var=.05, grime=.55, wear=.12)
        p.slab(body, -w / 2, w / 2, -d / 2, d / 2 - .05, .0, h, bevel=.02)
        p.slab('PlasticGrey', -w / 2 + .03, w / 2 - .03, d / 2 - .05, d / 2 - .02, .05, h - .05, bevel=.01)
        for i in range(14):
            z = .1 + i * (h - .2) / 14
            p.slab(body, -w / 2 + .06, w / 2 - .06, d / 2 - .03, d / 2 - .01, z, z + .025)
        for i in range(10):
            x = -w / 2 + .1 + i * (w - .2) / 9
            p.slab('SteelDark', x - .006, x + .006, -d / 2 - .01, -d / 2 + .005, .1, h - .1)


def crate(p, v):
    w, h, d = .8, .7, .8
    wood = 'WoodRaw'
    for sy, y in ((1, d / 2), (-1, -d / 2)):
        for i in range(4):
            z0 = .01 + i * h / 4
            p.slab(wood, -w / 2, w / 2, y - .022 * (sy > 0), y + .022 * (sy < 0), z0, z0 + h / 4 - .012, bevel=.004)
        for x in (-w / 2 + .05, w / 2 - .05):
            p.slab(wood, x - .04, x + .04, y + .012 * sy - .012, y + .012 * sy + .012, 0, h)
        p.box(wood, (w * 1.15, .022, .08), (0, y + .02 * sy, h / 2), bevel=.004, rot=(0, math.degrees(math.atan2(h - .1, w - .1)), 0))
    for sx, x in ((1, w / 2), (-1, -w / 2)):
        for i in range(4):
            z0 = .01 + i * h / 4
            p.slab(wood, x - .022 * (sx > 0), x + .022 * (sx < 0), -d / 2 + .022, d / 2 - .022, z0, z0 + h / 4 - .012, bevel=.004)
    for i in range(5):
        x0 = -w / 2 + i * w / 5
        p.slab(wood, x0 + .004, x0 + w / 5 - .004, -d / 2, d / 2, h - .022, h, bevel=.004)


def plastic_crate(p, v):
    w, h, d = .4, .3, .3
    plastic = p.surface('Crate' + str(v), (.5, .08, .03) if v == 0 else (.08, .2, .35), .5, var=.05, grime=.45)
    p.slab(plastic, -w / 2, w / 2, -d / 2, d / 2, 0, .015)
    for sx in (-1, 1):
        p.slab(plastic, sx * w / 2 - .008, sx * w / 2 + .008, -d / 2, d / 2, 0, h, bevel=.003)
    for sy in (-1, 1):
        p.slab(plastic, -w / 2, w / 2, sy * d / 2 - .008, sy * d / 2 + .008, 0, h, bevel=.003)
    p.slab(plastic, -w / 2 - .006, w / 2 + .006, -d / 2 - .006, d / 2 + .006, h - .02, h, bevel=.004)
    for sx in (-1, 1):
        p.slab('PlasticGrey' if v else 'Plastic', sx * w / 2 - .011, sx * w / 2 + .011, -.06, .06, h - .08, h - .045, bevel=.004)


def barrel(p, v):
    r, h = .29, .9
    body = p.paint('Drum' + str(v), (.04, .12, .3) if v == 0 else (.32, .05, .03), wear=.55) if v < 2 else 'RustedSteel'
    p.lathe(body, [(0, 0), (r - .01, 0), (r, .01), (r, h - .01), (r - .01, h), (0, h)], sides=28)
    for z in (.01, h * .33, h * .66, h - .015):
        p.lathe(body, [(r + .001, z - .012), (r + .012, z - .004), (r + .012, z + .004), (r + .001, z + .012)], sides=28)
    p.cyl('SteelDark', .028, .012, (.15, .05, h + .004), sides=12)
    p.cyl('SteelDark', .016, .012, (-.16, -.06, h + .004), sides=10)


def pallet(p, v):
    w, d = 1.2, 1.0
    wood = 'WoodRaw'
    for i in range(7):
        y = -d / 2 + .05 + i * (d - .1) / 6
        p.slab(wood, -w / 2, w / 2, y - .05, y + .05, .122, .144, bevel=.003)
    for y in (-d / 2 + .05, 0, d / 2 - .05):
        p.slab(wood, -w / 2, w / 2, y - .05, y + .05, .1, .122, bevel=.003)
        for x in (-w / 2 + .05, 0, w / 2 - .05):
            p.slab(wood, x - .05, x + .05, y - .05, y + .05, .022, .1, bevel=.004)
    for y in (-d / 2 + .05, 0, d / 2 - .05):
        p.slab(wood, -w / 2, w / 2, y - .05, y + .05, 0, .022, bevel=.003)


def dumpster(p, v):
    w, h, d = 1.9, 1.3, 1.3
    body = p.paint('Dumpster' + str(v), (.07, .16, .09) if v == 0 else (.12, .14, .2), wear=.6)
    p.outline(body, [(-d / 2 + .15, .1), (d / 2, .1), (d / 2, h - .06), (-d / 2, h), (-d / 2, .35)], w, axis='Y', bevel=.012).rotation_euler = (0, 0, math.radians(90))
    for x in (-w / 2 + .1, w / 2 - .1):
        p.slab('SteelDark', x - .05, x + .05, -d / 2 + .15, d / 2 - .1, 0, .12)
    for x0, x1 in ((-w / 2 + .02, -.01), (.01, w / 2 - .02)):
        p.box('Plastic', (x1 - x0, d + .06, .05), ((x0 + x1) / 2, .0, h + .01), bevel=.015, rot=(math.degrees(math.atan2(.06, d)), 0, 0))
    for sx in (-1, 1):
        p.slab(body, sx * (w / 2 + .02) - .03, sx * (w / 2 + .02) + .03, -.3, .3, .55, .7, bevel=.01)
    for i in range(5):
        x = -w / 2 + .2 + i * (w - .4) / 4
        p.slab(body, x - .02, x + .02, d / 2, d / 2 + .02, .2, h - .1)


def trash_can(p, v):
    if v == 0:
        # Galvanised can with a lid.
        p.lathe('Galvanised', [(0, 0), (.22, 0), (.24, .02), (.26, .8), (0, .8)], sides=24)
        for z in (.15, .4, .65):
            p.lathe('Galvanised', [(.25 - (.65 - z) * .02, z - .015), (.265 - (.65 - z) * .02, z), (.25 - (.65 - z) * .02, z + .015)], sides=24)
        p.lathe('Galvanised', [(0, .86), (.13, .86), (.27, .82), (.28, .8), (.27, .79)], sides=24)
        p.tube('Galvanised', [(-.06, 0, .86), (-.05, 0, .9), (.05, 0, .9), (.06, 0, .86)], .008)
    else:
        # Wheelie bin.
        bin_ = p.surface('Wheelie', (.06, .14, .08), .5, var=.06, grime=.5, wear=.1)
        p.outline(bin_, [(-.25, .05), (.25, .05), (.28, .9), (-.28, .9)], .55, axis='Y', bevel=.015).rotation_euler = (0, 0, math.radians(90))
        p.box(bin_, (.6, .62, .04), (0, .02, .93), bevel=.015)
        p.rod('Plastic', (-.24, -.33, .93), (.24, -.33, .93), .016)
        for x in (-.2, .2):
            p.cyl('Rubber', .1, .05, (x, -.24, .1), axis='X', sides=16)


def cart(p, v):
    w, h, d = 1.0, .9, .55
    plastic = p.surface('CartGrey', (.2, .2, .2), .5, var=.05, grime=.45)
    for z in (.18, .78):
        p.slab(plastic, -w / 2, w / 2, -d / 2, d / 2, z, z + .04, bevel=.01)
        p.slab(plastic, -w / 2, w / 2, -d / 2, d / 2, z + .04, z + .1, bevel=.006).scale = (1, 1, 1)
    for sx in (-1, 1):
        for sy in (-1, 1):
            x, y = sx * (w / 2 - .04), sy * (d / 2 - .04)
            p.cyl(plastic, .02, .7, (x, y, .5), sides=10)
            p.rod('SteelDark', (x, y, .18), (x, y, .07), .012)
            p.cyl('Rubber', .05, .03, (x, y, .05), axis='Y', sides=12)
    p.tube('Steel', [(-w / 2 + .02, -d / 2 + .05, .88), (-w / 2 - .1, -d / 2 + .05, 1.0), (-w / 2 - .1, d / 2 - .05, 1.0), (-w / 2 + .02, d / 2 - .05, .88)], .014)


FAMILIES = {
    'sink': (sink, 2, 512), 'toilet': (toilet, 1, 512), 'bathtub': (bathtub, 1, 512), 'fridge': (fridge, 1, 512),
    'stove': (stove, 1, 512), 'washer': (washer, 1, 512), 'vending': (vending, 2, 1024), 'radiator': (radiator, 1, 512),
    'hvac': (hvac, 2, 1024), 'crate': (crate, 1, 512), 'plastic_crate': (plastic_crate, 2, 512), 'barrel': (barrel, 3, 512),
    'pallet': (pallet, 1, 512), 'dumpster': (dumpster, 2, 1024), 'trash_can': (trash_can, 2, 512), 'cart': (cart, 1, 512),
}
