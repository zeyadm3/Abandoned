"""Three wardrobe hats remodelled at real size for the v2 head (head centre 1.65, crown 1.783 with hair).

They show that the wardrobe can follow the realistic head; the rest of the hats would get the same pass.
"""
import math
from mathutils import Vector
import crew_geo as G

TOP = 1.783


def hardhat(M):
    """Cap-style safety helmet: domed shell with a centre ridge, a short front peak, the suspension band showing."""
    shell = G.quad_sphere('HardHatShell', (0, .014, 1.70), (.106, .13, .098), M['HardHat'], cuts=10,
                          keep=lambda d: d.z > -.02, shape=lambda d, p: Vector((p.x, p.y, p.z + .006 * max(0.0, -d.y) * d.z)))
    t = shell.modifiers.new('Thickness', 'SOLIDIFY')
    t.thickness, t.offset = .004, -1.0
    brim = G.loft('HardHatBrim', [{'c': (0, .014, 1.70), 'rx': .106, 'rf': .13, 'rb': .13},
                                  {'c': (0, .014, 1.692), 'rx': .113, 'rf': .178, 'rb': .142}], M['HardHat'], n=48,
                  cap=(False, False), subsurf=1)
    t = brim.modifiers.new('Thickness', 'SOLIDIFY')
    t.thickness, t.offset = .004, -1.0
    ridge = G.quad_sphere('HardHatRidge', (0, .014, 1.793), (.014, .112, .01), M['HardHat'], cuts=4)
    band = G.loft('Suspension', [{'c': (0, .008, z), 'rx': .085, 'rf': .095, 'rb': .104} for z in (1.694, 1.71)], M['Webbing'], n=40,
                  cap=(False, False), subsurf=1)
    return [shell, brim, ridge, band]


def beanie(M):
    """Rib-knit watch cap with a folded cuff."""
    crown = G.quad_sphere('Beanie', (0, .008, 1.70), (.093, .109, .093), M['Knit'], cuts=10, keep=lambda d: d.z > -.32)
    t = crown.modifiers.new('Thickness', 'SOLIDIFY')
    t.thickness, t.offset = .006, -1.0
    G.bend_folds(crown, .0015, .01, 'Ribs')
    cuff = G.loft('BeanieCuff', [{'c': (0, .008, z), 'rx': .097, 'rf': .108, 'rb': .112, 'wob': .025, 'freq': 70} for z in (1.676, 1.698, 1.718)],
                  M['Knit'], n=140, cap=(False, False), subsurf=1)
    return [crown, cuff]


def cap(M):
    """Six-panel work cap with a curved bill and a top button."""
    crown = G.quad_sphere('Cap', (0, .014, 1.708), (.092, .108, .08), M['CapCloth'], cuts=10, keep=lambda d: d.z > -.08)
    t = crown.modifiers.new('Thickness', 'SOLIDIFY')
    t.thickness, t.offset = .003, -1.0
    rows = []
    for k, (y, w, z) in enumerate(((-.088, .085, 1.708), (-.12, .082, 1.702), (-.15, .072, 1.695), (-.172, .05, 1.688), (-.18, .02, 1.685))):
        rows.append({'c': (0, y, z), 'rx': w, 'rf': .003, 'rb': .003, 'p': 2.0})
    bill = G.loft('CapBill', rows, M['CapCloth'], n=24, plane='XZ', subsurf=2)
    # The bill curves down at its sides.
    for v in bill.data.vertices:
        v.co.z -= 22 * v.co.x * v.co.x * .05
    seams = [G.tube('CapSeam', [(math.sin(a) * .094 * math.sin(t2), .014 - math.cos(a) * .11 * math.sin(t2), 1.708 + .081 * math.cos(t2))
                                for t2 in [i * .14 for i in range(12)]], M['Webbing'], .0012, sides=5, subsurf=0)
             for a in [k * math.pi / 3 for k in range(6)]]
    button = G.quad_sphere('CapButton', (0, .014, 1.789), (.009, .009, .004), M['CapCloth'], cuts=3)
    return [crown, bill, button] + seams


HATS = {'hardhat': hardhat, 'beanie': beanie, 'cap': cap}
