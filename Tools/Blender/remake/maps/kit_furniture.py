"""Map kit, furniture: desks, chairs, tables, benches, beds, storage. Each function draws one variant at a real size
(the family's typical size in KitFamilies.cs) into a Parts; Unity fits it to the copy it replaces.
X length, +Y front, Z up, on the floor."""
import math

from remake.maps.parts import arc


# --------------------------------------------------------------------------------------------------- desks, tables

def desk(p, v):
    w, d, h = 1.5, .75, .76
    top = 'WoodDark' if v == 1 else p.surface('Laminate', (.42, .38, .32), .5, var=.05, grime=.35, wear=.2)
    p.slab(top, -w / 2, w / 2, -d / 2, d / 2, h - .032, h, bevel=.006)
    steel = p.paint('DeskGrey', (.32, .33, .32), wear=.35)
    if v == 0:
        # Steel pedestal desk: three drawers on the left, a panel on the right, a modesty panel at the back.
        x0, x1 = -w / 2 + .02, -w / 2 + .44
        p.slab(steel, x0, x1, -d / 2 + .03, d / 2 - .02, .04, h - .032)
        p.slab(steel, w / 2 - .045, w / 2 - .02, -d / 2 + .03, d / 2 - .02, .0, h - .032)
        p.slab(steel, x1, w / 2 - .045, -d / 2 + .03, -d / 2 + .05, .3, h - .04)
        p.slab('SteelDark', x0 + .01, x1 - .01, -d / 2 + .05, d / 2 - .05, 0, .04)
        for i, (z0, z1) in enumerate(((.05, .3), (.31, .5), (.51, .7))):
            p.slab(steel, x0 + .012, x1 - .012, d / 2 - .02, d / 2 - .002, z0, z1, bevel=.003)
            p.rod('Aluminium', (x0 + .14, d / 2 + .012, z1 - .05), (x1 - .14, d / 2 + .012, z1 - .05), .007)
        p.slab(steel, x1 + .02, w / 2 - .06, d / 2 - .04, d / 2 - .02, h - .1, h - .036)
    else:
        # Wooden desk: two side panels, a modesty panel and a pencil drawer.
        for x in (-w / 2 + .03, w / 2 - .03):
            p.slab('WoodDark', x - .02, x + .02, -d / 2 + .02, d / 2 - .02, 0, h - .032, bevel=.004)
        p.slab('WoodDark', -w / 2 + .05, w / 2 - .05, -d / 2 + .04, -d / 2 + .06, .25, h - .04)
        p.slab('WoodDark', -.35, .35, d / 2 - .06, d / 2 - .03, h - .11, h - .036)
        p.rod('Brass', (-.06, d / 2 - .01, h - .075), (.06, d / 2 - .01, h - .075), .006)


def student_desk(p, v):
    w, d, h = .7, .55, .75
    p.slab(p.surface('DeskTop', (.55, .45, .3), .45, var=.12, scale=3, grime=.4, wear=.3), -w / 2, w / 2, -d / 2, d / 2, h - .025, h, bevel=.004)
    frame = p.paint('SchoolBlue', (.06, .12, .25), wear=.4)
    p.slab('Plastic', -w / 2 + .01, w / 2 - .01, -d / 2 + .01, d / 2 - .01, h - .035, h - .025, bevel=.002)
    if v == 0:
        p.legs(frame, w, d, h - .035, inset=.04, r=.013)
        for y in (-d / 2 + .04, d / 2 - .04):
            p.rod(frame, (-w / 2 + .04, y, .12), (w / 2 - .04, y, .12), .01)
    else:
        for x in (-w / 2 + .05, w / 2 - .05):
            p.tube(frame, [(x, d / 2 - .02, 0.012), (x, -d / 2 + .05, .012), (x, -d / 2 + .05, h - .035), (x, d / 2 - .06, h - .035)], .013)
    # Book basket under the top.
    p.slab('SteelDark', -w / 2 + .06, w / 2 - .06, -d / 2 + .06, d / 2 - .12, h - .17, h - .16)
    for x in (-w / 2 + .06, w / 2 - .06):
        p.slab('SteelDark', x - .004, x + .004, -d / 2 + .06, d / 2 - .12, h - .17, h - .04)


def table(p, v):
    w, d, h = 1.4, .9, .75
    if v == 0:
        top = p.surface('CaféTop', (.6, .58, .52), .4, var=.06, grime=.35, wear=.2)
        p.slab(top, -w / 2, w / 2, -d / 2, d / 2, h - .03, h, bevel=.008)
        p.slab('Aluminium', -w / 2, w / 2, -d / 2, d / 2, h - .036, h - .03, bevel=.002)
        for x in (-w / 2 + .3, w / 2 - .3):
            p.cyl('SteelDark', .03, h - .04, (x, 0, (h - .04) / 2), sides=12)
            p.slab('SteelDark', x - .03, x + .03, -d / 2 + .1, d / 2 - .1, 0, .03, bevel=.006)
    else:
        p.slab('Wood', -w / 2, w / 2, -d / 2, d / 2, h - .035, h, bevel=.008)
        p.slab('Wood', -w / 2 + .06, w / 2 - .06, -d / 2 + .06, d / 2 - .06, h - .12, h - .035, bevel=.004)
        for sx in (-1, 1):
            for sy in (-1, 1):
                x, y = sx * (w / 2 - .09), sy * (d / 2 - .09)
                p.lathe('Wood', [(.026, 0), (.022, .05), (.03, .12), (.022, .2), (.024, .45), (.032, .55), (.03, h - .12)], (x, y, 0), sides=12)


def folding_table(p, v):
    w, d, h = 1.8, .75, .74
    p.slab(p.surface('FoldTop', (.62, .6, .55), .55, var=.05, grime=.45, bump=.1), -w / 2, w / 2, -d / 2, d / 2, h - .04, h, bevel=.012)
    for x in (-w / 2 + .2, w / 2 - .2):
        p.tube('Steel', [(x, -d / 2 + .06, h - .045), (x, -d / 2 + .06, .015), (x, d / 2 - .06, .015), (x, d / 2 - .06, h - .045)], .012)
        p.rod('Steel', (x, -d / 2 + .06, .3), (x * .4, 0, h - .05), .008)
        for y in (-d / 2 + .06, d / 2 - .06):
            p.sphere('Rubber', .016, (x, y, .012), scale=(1, 1, .6), segments=8, rings=4)


def workbench(p, v):
    w, d, h = 2.0, .8, .9
    p.slab('WoodRaw', -w / 2, w / 2, -d / 2, d / 2, h - .05, h, bevel=.006)
    frame = p.paint('BenchGreen', (.08, .14, .09), wear=.55)
    for sx in (-1, 1):
        for sy in (-1, 1):
            x, y = sx * (w / 2 - .05), sy * (d / 2 - .05)
            p.slab(frame, x - .025, x + .025, y - .025, y + .025, 0, h - .05)
    for y in (-d / 2 + .05, d / 2 - .05):
        p.slab(frame, -w / 2 + .05, w / 2 - .05, y - .02, y + .02, h - .12, h - .05)
    p.slab('WoodRaw', -w / 2 + .05, w / 2 - .05, -d / 2 + .05, d / 2 - .05, .15, .17)
    if v == 0:
        p.box('CastIron', (.18, .12, .1), (w / 2 - .25, d / 2 - .04, h + .05), bevel=.01)
        p.rod('Steel', (w / 2 - .25, d / 2 + .02, h + .05), (w / 2 - .25, d / 2 + .16, h + .05), .009)


def bench(p, v):
    w, d, h = 1.8, .4, .45
    for i in range(3):
        y = -d / 2 + .02 + i * (d - .04) / 2.2
        p.slab('Wood', -w / 2, w / 2, y, y + (d - .04) / 3.3, h - .035, h, bevel=.006)
    for x in (-w / 2 + .15, w / 2 - .15):
        p.tube('SteelDark', [(x, -d / 2 + .04, 0), (x, -d / 2 + .04, h - .035), (x, d / 2 - .04, h - .035), (x, d / 2 - .04, 0)], .016)


def bolted_seat(p, v):
    w = 1.5
    p.slab('SteelDark', -w / 2, w / 2, -.04, .04, .38, .43, bevel=.004)
    for x in (-w / 2 + .12, w / 2 - .12):
        p.slab('SteelDark', x - .03, x + .03, -.04, .04, 0, .38)
        p.slab('SteelDark', x - .08, x + .08, -.1, .1, 0, .012)
    shell = p.surface('SeatShell', (.08, .14, .3), .45, var=.05, grime=.4, wear=.2)
    for i in range(3):
        x = -w / 2 + .25 + i * .5
        p.slab(shell, x - .22, x + .22, -.2, .25, .43, .47, bevel=.02)
        p.box(shell, (.44, .04, .42), (x, -.24, .7), bevel=.018, rot=(-12, 0, 0))


def chair(p, v):
    w, d = .46, .48
    if v == 0:
        # Stacking school chair: steel tube frame, moulded shell.
        frame = 'Steel'
        for x in (-w / 2 + .03, w / 2 - .03):
            p.tube(frame, [(x, d / 2 - .03, 0), (x, d / 2 - .04, .44), (x, -d / 2 + .05, .44), (x, -d / 2 + .02, 0)], .011)
            p.tube(frame, [(x, -d / 2 + .05, .44), (x, -d / 2 + .02, .82)], .011)
        shell = p.surface('ChairShell', (.12, .22, .35), .45, var=.06, grime=.45, wear=.2)
        p.slab(shell, -w / 2 + .01, w / 2 - .01, -d / 2 + .04, d / 2, .44, .465, bevel=.015)
        p.box(shell, (w - .04, .025, .26), (0, -d / 2 + .02, .7), bevel=.01, rot=(-8, 0, 0))
    else:
        # Wooden chair: turned legs, a seat, a slatted back.
        p.slab('Wood', -w / 2, w / 2, -d / 2, d / 2, .43, .46, bevel=.008)
        for sx in (-1, 1):
            for sy in (-1, 1):
                p.cyl('Wood', .017, .43, (sx * (w / 2 - .04), sy * (d / 2 - .04), .215), sides=10)
        for x in (-w / 2 + .03, w / 2 - .03):
            p.slab('Wood', x - .016, x + .016, -d / 2 + .015, -d / 2 + .045, .46, .9, bevel=.004)
        for z in (.58, .7, .83):
            p.slab('Wood', -w / 2 + .03, w / 2 - .03, -d / 2 + .02, -d / 2 + .04, z, z + .05, bevel=.004)
        for sy in (-1, 1):
            p.rod('Wood', (-w / 2 + .04, sy * (d / 2 - .04), .15), (w / 2 - .04, sy * (d / 2 - .04), .15), .009)


def office_chair(p, v):
    for i in range(5):
        a = math.radians(90 + i * 72)
        tip = (math.cos(a) * .3, math.sin(a) * .3, .06)
        p.rod('Plastic', (0, 0, .09), tip, .018)
        p.sphere('Plastic', .025, (tip[0], tip[1], .028), segments=10, rings=5)
    p.cyl('Chrome', .022, .3, (0, 0, .22), sides=12)
    p.cyl('Plastic', .035, .1, (0, 0, .1), sides=12)
    cloth = p.fabric('ChairCloth', (.06, .06, .065))
    p.box(cloth, (.5, .48, .08), (0, .02, .45), bevel=.03)
    p.box(cloth, (.46, .07, .5), (0, -.24, .8), bevel=.03, rot=(-6, 0, 0))
    p.slab('Plastic', -.04, .04, -.26, -.2, .44, .6)
    for x in (-.27, .27):
        p.rod('Plastic', (x, -.05, .47), (x, -.05, .64), .014)
        p.box('Plastic', (.06, .24, .03), (x, .0, .65), bevel=.012)


def armchair(p, v):
    cloth = p.fabric('ArmchairCloth', (.22, .12, .08))
    p.box(cloth, (.85, .8, .25), (0, 0, .3), bevel=.04)
    p.box(cloth, (.62, .62, .13), (0, .06, .48), bevel=.05)
    p.box(cloth, (.7, .18, .5), (0, -.31, .67), bevel=.06, rot=(-8, 0, 0))
    for x in (-.36, .36):
        p.box(cloth, (.14, .78, .26), (x, .0, .55), bevel=.05)
    for sx in (-1, 1):
        for sy in (-1, 1):
            p.cyl('WoodDark', .02, .17, (sx * .35, sy * .33, .085), sides=10)


def sofa(p, v):
    w, d = 2.0, .9
    cloth = p.fabric('SofaCloth', (.17, .19, .2)) if v == 0 else 'Leather'
    p.box(cloth, (w, d - .05, .25), (0, 0, .3), bevel=.04)
    for i in range(3):
        x = -w / 2 + .22 + (i + .5) * (w - .44) / 3
        p.box(cloth, ((w - .44) / 3 - .01, d - .25, .14), (x, .08, .495), bevel=.05)
        p.box(cloth, ((w - .44) / 3 - .02, .2, .42), (x, -d / 2 + .2, .7), bevel=.07, rot=(-10, 0, 0))
    p.box(cloth, (w, .18, .5), (0, -d / 2 + .09, .6), bevel=.05)
    for x in (-w / 2 + .1, w / 2 - .1):
        p.box(cloth, (.2, d - .04, .3), (x, 0, .55), bevel=.06)
    for sx in (-1, 1):
        for sy in (-1, 1):
            p.cyl('WoodDark', .022, .17, (sx * (w / 2 - .1), sy * (d / 2 - .1), .085), sides=10)


def park_bench(p, v):
    w = 1.8
    for x in (-w / 2 + .12, w / 2 - .12):
        outline = [(-.28, 0), (-.22, 0), (-.18, .4), (.25, .4), (.27, 0), (.31, 0), (.3, .45), (-.14, .46), (-.2, .9), (-.25, .88), (-.2, .44)]
        ob = p.outline('CastIron', [(y, z) for y, z in outline], .05, axis='Y', bevel=.006)
        ob.rotation_euler = (0, 0, math.radians(90))
        ob.location = (x, 0, 0)
    for i in range(4):
        y = -.2 + i * .12
        p.slab('Wood', -w / 2, w / 2, y, y + .09, .42, .45, bevel=.006)
    for i in range(3):
        z = .55 + i * .11
        y = -.2 - (z - .46) * .14
        p.slab('Wood', -w / 2, w / 2, y - .015, y + .015, z, z + .085, bevel=.006)


# --------------------------------------------------------------------------------------------------- beds

def hospital_bed(p, v):
    # Length along Y, head at the back (-Y) against the wall.
    w, d = 1.0, 2.1
    frame = p.paint('BedFrame', (.62, .63, .6), wear=.3)
    p.slab(frame, -w / 2 + .06, w / 2 - .06, -d / 2 + .05, d / 2 - .05, .4, .46)
    p.box(p.fabric('Mattress', (.45, .55, .58)), (w - .18, d - .2, .16), (0, 0, .54), bevel=.04)
    p.box(p.fabric('Sheet', (.72, .72, .7)), (w - .2, .5, .1), (0, -d / 2 + .4, .66), bevel=.04)
    for y, hh in ((-d / 2 + .05, 1.0), (d / 2 - .05, .85)):
        p.slab(frame, -w / 2 + .04, w / 2 - .04, y - .03, y + .03, .35, hh, bevel=.012)
        p.slab('PlasticBeige', -w / 2 + .08, w / 2 - .08, y - .02, y + .02, .5, hh - .04, bevel=.01)
    for x in (-w / 2 + .04, w / 2 - .04):
        p.rod('Aluminium', (x, -d / 2 + .35, .82), (x, d / 2 - .4, .82), .012)
        for y in (-d / 2 + .4, 0, d / 2 - .45):
            p.rod('Aluminium', (x, y, .46), (x, y, .82), .01)
    for sx in (-1, 1):
        for sy in (-1, 1):
            x, y = sx * (w / 2 - .12), sy * (d / 2 - .12)
            p.rod('SteelDark', (x, y, .4), (x, y, .1), .02)
            p.cyl('Rubber', .05, .03, (x, y, .05), axis='X', sides=12)


def bunk(p, v):
    w, d = 2.0, .8
    steel = p.paint('BunkSteel', (.3, .33, .32), wear=.5)
    tiers = (.4, 1.35) if v == 0 else (.42,)
    top = tiers[-1] + (.45 if v == 0 else .25)
    for sx in (-1, 1):
        for sy in (-1, 1):
            x, y = sx * (w / 2 - .02), sy * (d / 2 - .02)
            p.slab(steel, x - .025, x + .025, y - .025, y + .025, 0, top)
    for z in tiers:
        p.slab(steel, -w / 2, w / 2, -d / 2, d / 2, z - .04, z, bevel=.003)
        p.box(p.fabric('BunkMattress', (.26, .27, .24)), (w - .08, d - .08, .12), (0, 0, z + .06), bevel=.035)
        p.box(p.fabric('Blanket', (.18, .17, .14)), (w * .55, d - .06, .04), (.2, 0, z + .13), bevel=.015)
    if v == 0:
        for z in (.75, 1.0):
            p.rod(steel, (w / 2 - .02, d / 2 - .02, z), (w / 2 - .02, -d / 2 + .02, z), .012)


def bed(p, v):
    # Length along Y, headboard at the back (-Y) against the wall.
    w, d = (1.5, 2.0) if v == 0 else (.95, 2.0)
    p.slab('WoodDark', -w / 2, w / 2, -d / 2, d / 2, .12, .32, bevel=.01)
    p.box(p.fabric('BedMattress', (.7, .68, .62)), (w - .06, d - .06, .22), (0, 0, .43), bevel=.05)
    duvet = p.fabric('Duvet' + str(v), (.32, .16, .12) if v == 0 else (.2, .25, .3))
    p.box(duvet, (w + .02, d * .72, .1), (0, .24, .58), bevel=.05)
    for x in ((-w / 4, w / 4) if v == 0 else (0,)):
        p.box(p.fabric('Pillow', (.75, .74, .7)), (w / 2 - .1 if v == 0 else .6, .38, .14), (x, -d / 2 + .3, .6), bevel=.06)
    p.slab('WoodDark', -w / 2 - .02, w / 2 + .02, -d / 2 - .05, -d / 2, 0, 1.1, bevel=.012)
    for sx in (-1, 1):
        p.slab('WoodDark', sx * (w / 2) - .03, sx * (w / 2) + .03, d / 2 - .06, d / 2, 0, .4)


def bedside(p, v):
    w, d, h = .5, .4, .55
    p.slab('WoodDark', -w / 2, w / 2, -d / 2, d / 2, .05, h, bevel=.006)
    p.slab('Wood', -w / 2 + .02, w / 2 - .02, d / 2, d / 2 + .015, h - .17, h - .03, bevel=.004)
    p.slab('Wood', -w / 2 + .02, w / 2 - .02, d / 2, d / 2 + .015, .08, h - .19, bevel=.004)
    p.sphere('Brass', .015, (0, d / 2 + .025, h - .1), segments=10, rings=6)
    p.legs('WoodDark', w, d, .05, inset=.04, r=.018, square=True)


# --------------------------------------------------------------------------------------------------- storage

def locker(p, v):
    n = 3 if v == 0 else 2
    w, d, h = .3 * n, .5, 1.8
    steel = p.paint('Locker' + str(v), (.18, .26, .3) if v == 0 else (.38, .36, .3), wear=.45)
    p.slab(steel, -w / 2, w / 2, -d / 2, d / 2 - .02, .1, h, bevel=.004)
    p.slab('SteelDark', -w / 2 + .01, w / 2 - .01, -d / 2 + .02, d / 2 - .04, 0, .1)
    for i in range(n):
        x0 = -w / 2 + i * .3 + .008
        x1 = x0 + .284
        doors = ((.11, h - .01),) if v == 1 else ((.11, h / 2 + .03), (h / 2 + .05, h - .01))
        for z0, z1 in doors:
            p.slab(steel, x0, x1, d / 2 - .02, d / 2 - .005, z0, z1, bevel=.003)
            for k in range(4):
                z = z1 - .06 - k * .022
                p.slab('SteelDark', x0 + .06, x1 - .06, d / 2 - .006, d / 2 - .002, z, z + .008, bevel=.001)
            p.slab('Chrome', x1 - .045, x1 - .025, d / 2 - .006, d / 2 + .012, (z0 + z1) / 2 - .05, (z0 + z1) / 2 + .05, bevel=.003)


def filing_cabinet(p, v):
    w, d, h = .5, .65, 1.3
    steel = p.paint('Filing', (.42, .43, .4), wear=.35)
    p.slab(steel, -w / 2, w / 2, -d / 2, d / 2 - .02, .0, h, bevel=.005)
    for i in range(4):
        z0 = .03 + i * (h - .05) / 4
        z1 = z0 + (h - .05) / 4 - .012
        p.slab(steel, -w / 2 + .01, w / 2 - .01, d / 2 - .02, d / 2, z0, z1, bevel=.004)
        p.slab('Aluminium', -.08, .08, d / 2, d / 2 + .02, z1 - .08, z1 - .06, bevel=.004)
        p.slab('Aluminium', -.045, .045, d / 2, d / 2 + .004, z1 - .05, z1 - .02, bevel=.001)


def cabinet(p, v):
    if v == 0:
        # Steel two-door storage cabinet.
        w, d, h = 1.0, .5, 1.2
        steel = p.paint('Cabinet', (.5, .5, .46), wear=.35)
        p.slab(steel, -w / 2, w / 2, -d / 2, d / 2 - .02, 0, h, bevel=.005)
        for x0, x1 in ((-w / 2 + .01, -.004), (.004, w / 2 - .01)):
            p.slab(steel, x0, x1, d / 2 - .02, d / 2 - .004, .02, h - .02, bevel=.003)
        p.rod('Chrome', (-.03, d / 2 + .01, h / 2 - .1), (-.03, d / 2 + .01, h / 2 + .1), .007)
        p.rod('Chrome', (.03, d / 2 + .01, h / 2 - .1), (.03, d / 2 + .01, h / 2 + .1), .007)
    elif v == 1:
        # Kitchen wall cabinet.
        w, d, h = .7, .33, .7
        p.slab('PaintCream', -w / 2, w / 2, -d / 2, d / 2 - .02, 0, h, bevel=.004)
        for x0, x1 in ((-w / 2 + .005, -.003), (.003, w / 2 - .005)):
            p.slab('PaintCream', x0, x1, d / 2 - .02, d / 2, .005, h - .005, bevel=.004)
            p.frame('PaintCream', x0 + .05, x1 - .05, .06, h - .06, d / 2 + .004, .02, .008)
        p.rod('Brass', (-.04, d / 2 + .015, .1), (-.04, d / 2 + .015, .2), .006)
        p.rod('Brass', (.04, d / 2 + .015, .1), (.04, d / 2 + .015, .2), .006)
    else:
        # Wooden chest of drawers.
        w, d, h = 1.0, .5, 1.1
        p.slab('WoodDark', -w / 2, w / 2, -d / 2, d / 2 - .02, .08, h, bevel=.006)
        for i in range(4):
            z0 = .1 + i * (h - .12) / 4
            z1 = z0 + (h - .12) / 4 - .015
            p.slab('Wood', -w / 2 + .02, w / 2 - .02, d / 2 - .02, d / 2, z0, z1, bevel=.005)
            for x in (-.22, .22):
                p.sphere('Brass', .016, (x, d / 2 + .012, (z0 + z1) / 2), segments=10, rings=6)
        p.legs('WoodDark', w, d, .08, inset=.04, r=.025, square=True)


def bookcase(p, v):
    w, d, h = 1.0, .35, 2.0
    wood = 'WoodDark'
    p.slab(wood, -w / 2, -w / 2 + .02, -d / 2, d / 2, 0, h)
    p.slab(wood, w / 2 - .02, w / 2, -d / 2, d / 2, 0, h)
    p.slab(wood, -w / 2, w / 2, -d / 2, -d / 2 + .01, 0, h)
    colours = [(.3, .06, .05), (.06, .12, .2), (.1, .18, .1), (.45, .38, .25), (.05, .05, .05), (.4, .28, .12)]
    import random
    rnd = random.Random(7 + v)
    for i in range(6):
        z = i * (h - .02) / 5
        p.slab(wood, -w / 2 + .02, w / 2 - .02, -d / 2 + .01, d / 2, z, z + .022)
        if i < 5:
            x = -w / 2 + .03
            while x < w / 2 - .12:
                bw = rnd.uniform(.025, .06)
                bh = rnd.uniform(.2, .3)
                if rnd.random() < .85:
                    c = colours[rnd.randrange(len(colours))]
                    p.slab(p.fabric('Book%d' % colours.index(c), c), x, x + bw, -d / 2 + .03, d / 2 - .04, z + .022, z + .022 + bh, bevel=.003)
                x += bw + .003
    p.slab(wood, -w / 2, w / 2, -d / 2, d / 2, h - .022, h)


def shelving(p, v):
    w, d, h = 1.8, .6, 2.0
    if v == 0:
        # Boltless steel shelving: angle posts, five steel decks.
        post = p.paint('ShelfPost', (.12, .2, .32), wear=.4)
        for sx in (-1, 1):
            for sy in (-1, 1):
                x, y = sx * (w / 2 - .02), sy * (d / 2 - .02)
                p.slab(post, x - .02, x + .02, y - .02, y + .02, 0, h)
        for i in range(5):
            z = .1 + i * (h - .15) / 4
            p.slab('Galvanised', -w / 2 + .01, w / 2 - .01, -d / 2 + .01, d / 2 - .01, z, z + .03, bevel=.002)
            p.slab(p.paint('ShelfBeam', (.55, .3, .05), wear=.4), -w / 2, w / 2, d / 2 - .03, d / 2, z - .03, z + .03, bevel=.003)
    else:
        # Store gondola: a pegboard spine, a base deck and four sloped shelves on one side.
        steel = p.paint('Gondola', (.68, .68, .64), wear=.3)
        p.slab(steel, -w / 2, w / 2, -d / 2, -d / 2 + .04, 0, h)
        p.slab(steel, -w / 2, w / 2, -d / 2, d / 2, 0, .12, bevel=.004)
        for x in (-w / 2, w / 2):
            p.slab(steel, x - .015, x + .015, -d / 2, d / 2, 0, .14)
        for i in range(4):
            z = .55 + i * .37
            p.slab(steel, -w / 2 + .01, w / 2 - .01, -d / 2 + .04, d / 2 - .1, z, z + .02, bevel=.002)
            p.slab('PlasticGrey', -w / 2 + .01, w / 2 - .01, d / 2 - .11, d / 2 - .1, z - .02, z + .04)


def whiteboard(p, v):
    w, h = 2.4, 1.2
    p.slab(p.surface('Whiteboard', (.82, .82, .8), .12, var=.02, grime=.3, coat=.6), -w / 2, w / 2, -.02, .0, 0, h)
    p.frame('Aluminium', -w / 2, w / 2, 0, h, -.005, .025, .03)
    p.slab('Aluminium', -w / 2 + .1, w / 2 - .1, -.02, .06, -.02, .0, bevel=.003)


def reception_counter(p, v):
    w, d, h = 2.5, .8, 1.05
    front = p.surface('CounterFront', (.25, .2, .16), .5, var=.1, scale=3, grime=.3) if v == 0 else 'PaintGrey'
    p.slab(front, -w / 2, w / 2, d / 2 - .05, d / 2, 0, h - .03, bevel=.006)
    for x in (-w / 2, w / 2 - .04):
        p.slab(front, x, x + .04, -d / 2, d / 2, 0, .76)
    p.slab(p.surface('Laminate', (.42, .38, .32), .5, var=.05, grime=.35, wear=.2), -w / 2, w / 2, -d / 2, d / 2 - .03, .73, .76, bevel=.004)
    p.slab('Stone', -w / 2 - .02, w / 2 + .02, d / 2 - .22, d / 2 + .05, h - .03, h, bevel=.006)
    p.slab('SteelDark', -w / 2 + .02, w / 2 - .02, d / 2 - .06, d / 2 - .045, 0, .08)


FAMILIES = {
    'desk': (desk, 2, 1024), 'student_desk': (student_desk, 2, 512), 'table': (table, 2, 512),
    'folding_table': (folding_table, 1, 512), 'workbench': (workbench, 2, 1024), 'bench': (bench, 1, 512),
    'bolted_seat': (bolted_seat, 1, 512), 'chair': (chair, 2, 512), 'office_chair': (office_chair, 1, 512),
    'armchair': (armchair, 1, 512), 'sofa': (sofa, 2, 1024), 'park_bench': (park_bench, 1, 512),
    'hospital_bed': (hospital_bed, 1, 1024), 'bunk': (bunk, 2, 1024), 'bed': (bed, 2, 1024), 'bedside': (bedside, 1, 512),
    'locker': (locker, 2, 1024), 'filing_cabinet': (filing_cabinet, 1, 512), 'cabinet': (cabinet, 3, 512),
    'bookcase': (bookcase, 1, 1024), 'shelving': (shelving, 2, 1024), 'whiteboard': (whiteboard, 1, 512),
    'reception_counter': (reception_counter, 2, 1024),
}
