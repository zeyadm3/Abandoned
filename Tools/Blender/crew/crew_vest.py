"""Plate-carrier style salvage vest worn over the jacket (crew redesign v2).

Front and back plate bags, a side cummerbund with webbing rows, padded shoulder straps with reflective
stripes, front pouches (radio, utility, flashlight), a drag handle and the crew number on the back.
Everything is in the wolf-grey Vest material so the team colour stays on the jacket.
"""
import math
import crew_geo as G

FRONT_Z, FRONT_TILT, FRONT_BEND = 1.255, -5.0, 1.0
BACK_Z = 1.26


def front_y(x, z, depth=0.0):
    """Outer face of the front plate bag at (x, z), pushed out by `depth`."""
    return -.169 - .025 - depth + FRONT_BEND * x * x + (z - FRONT_Z) * math.tan(math.radians(-FRONT_TILT))


def pouch(M, name, x, z, w, h, d, parts, flap=True):
    y = front_y(x, z, d * .5)
    parts.append(G.slab(name, (x, y, z), (w, d, h), M['Vest'], FRONT_BEND * .6, FRONT_TILT, .01, 6))
    if flap:
        parts.append(G.slab(name + 'Flap', (x, y - d * .5 - .003, z + h * .5 - .016), (w * 1.04, .008, .036), M['Vest'], 0, FRONT_TILT, .004, 4))


def build_vest(M, torso, chest, number, strap, text):
    parts = [G.slab('FrontPlate', (0, -.169, FRONT_Z), (.28, .05, .30), M['Vest'], FRONT_BEND, FRONT_TILT, .016),
             G.slab('BackPlate', (0, .159, BACK_Z), (.27, .045, .32), M['Vest'], -.8, 0, .016)]
    front, back = parts
    for s, a0, a1 in ((1, -52, 52), (-1, 128, 232)):
        cb = G.loft('Cummerbund', [{'c': (0, 0, z), 'rx': .198, 'rf': .156, 'rb': .139, 'p': 2.3} for z in (1.105, 1.245)], M['Vest'], n=24,
                    arc=(math.radians(a0), math.radians(a1)), subsurf=1)
        t = cb.modifiers.new('Thickness', 'SOLIDIFY')
        t.thickness, t.offset = .014, 1.0
        parts.append(cb)
        for z in (1.135, 1.17, 1.205):
            row = G.loft('MolleRow', [{'c': (0, 0, z), 'rx': .213, 'rf': .171, 'rb': .154, 'p': 2.3} for z in (z - .009, z + .009)], M['Webbing'],
                         n=24, arc=(math.radians(a0 + 6), math.radians(a1 - 6)), subsurf=0)
            t = row.modifiers.new('Thickness', 'SOLIDIFY')
            t.thickness, t.offset = .003, 1.0
            parts.append(row)
    # Padded shoulder straps from the front plate over the shoulder to the back plate.
    for s in (-1, 1):
        path = [(s * .1, -.17, 1.39), (s * .11, -.13, 1.44), (s * .12, -.06, 1.47), (s * .12, .02, 1.475), (s * .115, .09, 1.45), (s * .1, .15, 1.40)]
        parts.append(strap('ShoulderStrap', path, chest, M['Vest'], .058, .005, .016, (0, 0, 1.3)))
        parts.append(strap('ShoulderTape', path, chest, M['Tape'], .016, .022, .0015, (0, 0, 1.3)))
    # Front: webbing rows, an admin panel with the crew number, three pouches.
    for z in (1.13, 1.16, 1.19, 1.22):
        parts.append(strap('FrontMolle', [(x, front_y(x, z) - .01, z) for x in (-.13, -.08, -.03, .03, .08, .13)], front, M['Webbing'],
                           .02, .001, .003, (0, .3, z)))
    pouch(M, 'RadioPouch', .088, 1.165, .066, .12, .045, parts, flap=False)
    y = front_y(.088, 1.25, .04)
    parts += [G.rbox('Radio', (.088, y + .002, 1.245), (.056, .036, .05), M['Plastic'], .006, rot=(FRONT_TILT, 0, 0)),
              G.rbox('RadioKnob', (.1, y + .004, 1.28), (.012, .012, .016), M['Plastic'], .004),
              G.tube('Antenna', [(.074, y + .004, 1.27), (.074, y + .006, 1.33), (.075, y + .008, 1.365)], M['Plastic'], .0045, sides=8)]
    pouch(M, 'UtilityPouch', -.005, 1.15, .1, .1, .05, parts)
    pouch(M, 'LightPouch', -.095, 1.16, .048, .11, .035, parts, flap=False)
    y = front_y(-.095, 1.23, .035)
    parts.append(G.tube('Flashlight', [(-.095, y + .004, 1.205), (-.095, y + .002, 1.245)], M['Plastic'], .017, sides=16))
    parts.append(G.tube('FlashlightBezel', [(-.095, y + .002, 1.242), (-.095, y + .002, 1.252)], M['Steel'], .019, sides=16))
    yy = front_y(0, 1.36, .012)
    parts.append(G.slab('AdminPanel', (0, yy, 1.355), (.17, .022, .07), M['Vest'], FRONT_BEND * .8, FRONT_TILT, .008, 6))
    parts.append(G.slab('IdPatch', (.045, yy - .012, 1.355), (.06, .004, .042), M['Patch'], 0, FRONT_TILT, .003, 3))
    parts.append(G.rbox('ChemLight', (-.12, -.165, 1.395), (.012, .012, .085), M['ChemLight'], .005, rot=(0, -8, 0)))
    # Back: drag handle, a utility panel with the big crew number, a reflective band.
    parts.append(G.tube('DragHandle', [(-.045, .183, 1.40), (-.035, .195, 1.428), (0, .198, 1.438), (.035, .195, 1.428), (.045, .183, 1.40)],
                        M['Webbing'], .009, width=(.028, .008), sides=12, subsurf=1))
    parts.append(G.slab('BackPanel', (0, .205, 1.22), (.22, .05, .2), M['Vest'], -.6, 0, .014, 6))
    parts.append(G.slab('BackPatch', (0, .233, 1.235), (.12, .005, .075), M['Patch'], 0, 0, .004, 3))
    parts.append(G.slab('BackTape', (0, .184, 1.375), (.24, .004, .022), M['Tape'], -.8, 0, .002, 6))
    for p in parts:
        G.attach(p, torso)
    text(str(number), (0, .2364, 1.233), (90, 0, 180), .06, M['Stencil'], torso)
    text(str(number), (.045, yy - .0145, 1.353), (90 + FRONT_TILT, 0, 0), .03, M['Stencil'], torso)
