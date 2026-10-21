"""The realistic crew member (v2): a stocky labourer in tactical salvage gear (Blender: Z up, facing -Y).

1.78 m tall, eyes at 1.65 (the game's StandingEyeHeight). Pivots follow WorkerRig's names: Body 0.95,
Torso 1.10, Head 1.52 (neck), HeadAnchor 1.749, arms (+-0.19, 1.415) with a new elbow at 1.11,
legs (+-0.097, 0.90), knees 0.50. The character's left is +X. Team colour lives on the jacket only.
"""
import math
import bpy
from mathutils import Vector
import crew_geo as G
from crew_head import build_head
from crew_vest import build_vest

FONT = None
ARM_X, ARM_Z, ELBOW_Z = .19, 1.415, 1.11
LEG_X, HIP_Z, KNEE_Z = .102, .90, .50
SPREAD = 9.0


def R(z, rx, rf=None, rb=None, p=2.0, x=0.0, y=0.0, **k):
    d = {'c': (x, y, z), 'rx': rx, 'rf': rf if rf is not None else rx, 'rb': rb if rb is not None else (rf if rf is not None else rx), 'p': p}
    d.update(k)
    return d


# Outer surface of the jacket: broad chest, a gut pushing the front out, sloping trapezius.
TORSO = [R(.955, .196, .15, .136, 2.3), R(1.0, .192, .152, .133, 2.3), R(1.06, .19, .154, .13, 2.3), R(1.14, .19, .149, .128, 2.3),
         R(1.22, .19, .142, .127, 2.3), R(1.30, .193, .135, .127, 2.3), R(1.36, .195, .127, .125, 2.3), R(1.40, .188, .117, .119, 2.2),
         R(1.43, .175, .105, .11, 2.1), R(1.455, .14, .088, .095, 2.0), R(1.475, .095, .075, .082, 2.0), R(1.49, .075, .068, .075, 2.0)]
HIPS = [R(.78, .155, .11, .115, 2.2), R(.84, .18, .125, .135, 2.3), R(.90, .19, .132, .145, 2.3), R(.96, .188, .14, .138, 2.3),
        R(1.0, .185, .145, .132, 2.3)]


def band(name, target, z0, z1, rx, rf, rb, mat, c=(0, 0), off=.003, thick=.003, n=48, p=2.2):
    ob = G.loft(name, [R(z0, rx, rf, rb, p, *c), R(z1, rx, rf, rb, p, *c)], mat, n=n, cap=(False, False), subsurf=0)
    return G.hug(ob, target, off, thick)


def strap(name, pts, target, mat, width=.045, off=.002, thick=.005, centre=(0, 0, 1.25)):
    """Flat webbing pressed onto a surface: a ribbon three vertices wide, shrink-wrapped, then thickened."""
    import bmesh
    P = [Vector(q) for q in pts]
    bm = bmesh.new()
    rows = []
    for i, p in enumerate(P):
        t = (P[min(i + 1, len(P) - 1)] - P[max(i - 1, 0)]).normalized()
        out = (p - Vector(centre)).normalized()
        side = t.cross(out).normalized()
        rows.append([bm.verts.new(p + side * width * s) for s in (-.5, 0, .5)])
    for a, b in zip(rows, rows[1:]):
        for k in range(2):
            bm.faces.new((a[k], a[k + 1], b[k + 1], b[k]))
    ob = G.finish(name, bm, mat, subsurf=0)
    s = ob.modifiers.new('Smooth', 'SUBSURF')
    s.levels = s.render_levels = 2
    return G.hug(ob, target, off, thick)


def text(label, at, rot, size, mat, piv):
    cu = bpy.data.curves.new('CrewNumber', 'FONT')
    cu.body, cu.size, cu.align_x, cu.align_y, cu.extrude = label, size, 'CENTER', 'CENTER', .0006
    if FONT is not None:
        cu.font = FONT
    ob = bpy.data.objects.new('CrewNumber', cu)
    G.COLL.objects.link(ob)
    ob.data.materials.append(mat)
    ob.location, ob.rotation_euler = at, [math.radians(a) for a in rot]
    return G.attach(ob, piv)


def jacket(M, torso):
    chest = G.loft('Jacket', TORSO, M['Jacket'], n=48)
    G.bend_folds(chest, .006, .08)
    parts = [chest, G.loft('Hem', [R(.95, .203, .155, .141, 2.3), R(.972, .203, .155, .141, 2.3)], M['Jacket'], n=48, cap=(False, False)),
             G.loft('Tshirt', [R(1.44, .074, .07, .072, y=.012), R(1.49, .071, .066, .07, y=.012)], M['Trim'], n=32, cap=(False, False))]
    parts += [G.quad_sphere('Shoulder', (s * ARM_X, 0, 1.392), (.065, .069, .069), M['Jacket'], cuts=6) for s in (-1, 1)]
    collar = G.loft('Collar', [R(1.445, .098, .088, .094, 2.1), R(1.49, .101, .091, .097, 2.1), R(1.53, .105, .094, .1, 2.1)],
                    M['Jacket'], n=48, arc=(math.radians(-72), math.radians(252)))
    s = collar.modifiers.new('Thickness', 'SOLIDIFY')
    s.thickness, s.offset = .007, 1.0
    parts.append(collar)
    parts.append(strap('Zip', [(0, -.16, z) for z in (1.47, 1.43, 1.39, 1.30, 1.2, 1.1, 1.0, .96)], chest, M['JacketDark'], .026, .0005, .004,
                       (0, 0, 1.2)))
    parts.append(G.rbox('ZipPull', (0, -.098, 1.455), (.01, .005, .026), M['Steel'], .002, rot=(-20, 0, 0)))
    for p in parts:
        G.attach(p, torso)
    return chest


def trousers(M, body):
    hips = G.loft('Trousers', HIPS, M['Trousers'], n=48)
    G.bend_folds(hips, .005, .08)
    belt = G.loft('Belt', [R(.962, .207, .163, .149, 2.3), R(1.012, .209, .165, .15, 2.3)], M['Webbing'], n=56, cap=(False, False), subsurf=1)
    s = belt.modifiers.new('Thickness', 'SOLIDIFY')
    s.thickness, s.offset = .012, 1.0
    parts = [hips, belt,
             G.rbox('Buckle', (0, -.18, .987), (.06, .016, .044), M['Plastic'], .006),
             G.rbox('BuckleBar', (0, -.189, .987), (.034, .006, .02), M['Steel'], .003),
             G.rbox('BeltPouch', (.2, -.04, .955), (.04, .085, .095), M['Vest'], .012, subsurf=1),
             G.rbox('BeltPouchFlap', (.222, -.04, .99), (.008, .088, .03), M['Vest'], .004),
             G.rbox('HammerLoop', (-.214, .02, .985), (.012, .03, .05), M['Webbing'], .004)]
    # A claw hammer hung on the right hip: the trade, not a weapon.
    parts.append(G.tube('HammerHandle', [(-.226, .02, .99), (-.226, .02, .90), (-.224, .02, .80)], M['Rubber'], .013, sides=10, subsurf=1))
    parts.append(G.rbox('HammerHead', (-.226, .02, 1.005), (.026, .11, .026), M['Steel'], .006))
    for p in parts:
        G.attach(p, body)


def arm(M, piv, elbow, s):
    x = s * ARM_X

    def X(z):
        return x - s * max(0.0, 1.40 - z) * .015
    upper = G.loft('Sleeve', [R(1.43, .03, x=X(1.43)), R(1.42, .048, .05, .05, x=X(1.42)), R(1.40, .06, .062, .062, x=X(1.40)),
                              R(1.36, .066, .068, .068, x=X(1.36)), R(1.30, .068, .069, .069, x=X(1.30)), R(1.22, .064, .066, .066, x=X(1.22)),
                              R(1.15, .06, .062, .062, x=X(1.15), wob=.02, freq=5), R(1.09, .058, .06, .06, x=X(1.09))], M['Jacket'])
    G.two_tone(upper, M['JacketDark'], lambda c: c.z < 1.16 and c.y > .025)
    G.bend_folds(upper, .005, .07)
    up = [upper, band('UpperArmTape', upper, 1.245, 1.272, .084, .084, .084, M['Tape'], (X(1.26), 0), p=2.0),
          G.rbox('ShoulderPocket', (X(1.31) + s * .061, -.004, 1.31), (.014, .1, .1), M['Jacket'], .012, rot=(0, s * -4, 0), subsurf=1),
          G.rbox('ShoulderPatch', (X(1.31) + s * .07, -.004, 1.315), (.006, .075, .05), M['Patch'], .004, rot=(0, s * -4, 0))]
    for p in up:
        G.attach(p, piv)
    xe = X(1.11)
    fore = G.loft('Forearm', [R(1.14, .058, .06, .06, x=xe), R(1.08, .06, .062, .062, x=xe, wob=.02, freq=4),
                              R(1.0, .058, .06, .06, x=xe), R(.94, .052, .053, .053, x=xe), R(.90, .048, .049, .049, x=xe)], M['Jacket'])
    G.two_tone(fore, M['JacketDark'], lambda c: c.z > 1.06 and c.y > .025)
    G.bend_folds(fore, .004, .06)
    low = [fore, band('ForearmTape', fore, .965, .992, .075, .075, .075, M['Tape'], (xe, 0), p=2.0),
           G.loft('Cuff', [R(.915, .053, x=xe), R(.872, .053, x=xe)], M['Trim'], n=40),
           G.rbox('CuffTab', (xe + s * .05, .0, .893), (.008, .028, .03), M['Trim'], .004)]
    low += glove(M, s, xe)
    for p in low:
        G.attach(p, elbow)
    return X(1.315)


def glove(M, s, x):
    """A worn tactical work glove: grey synthetic palm, black back, knuckle pad, fingers in a relaxed curl."""
    def palm_side(c):
        return (c.x - x) * s < -.004
    out = [G.loft('GloveCuff', [R(.885, .052, .054, .054, x=x), R(.85, .047, .05, .05, x=x), R(.83, .04, .048, .048, x=x)], M['Glove'],
                  cap=(False, True)),
           G.rbox('GloveStrap', (x + s * .04, -.005, .858), (.012, .07, .024), M['Glove'], .005, rot=(0, s * -6, 0))]
    palm = G.quad_sphere('Hand', (x, -.004, .785), (.021, .047, .054), M['Glove'], cuts=6, power=.82)
    G.two_tone(palm, M['GlovePalm'], palm_side)
    out.append(palm)
    out.append(G.rbox('KnucklePad', (x + s * .019, -.004, .748), (.012, .086, .022), M['Rubber'], .008, rot=(0, s * -10, 0), subsurf=1))
    for fy, ln in ((-.031, .074), (-.0105, .081), (.0105, .077), (.03, .062)):
        x0, y0, z0 = x + s * .004, -.004 + fy, .742
        pts = [(x0, y0, z0, 1.0), (x0 - s * .004, y0, z0 - ln * .4, 1.0), (x0 - s * .013, y0, z0 - ln * .72, .96),
               (x0 - s * .026, y0, z0 - ln * .95, .9), (x0 - s * .032, y0, z0 - ln * 1.02, .75)]
        f = G.tube('Finger', pts, M['Glove'], .0098, sides=12, subsurf=2)
        G.two_tone(f, M['GlovePalm'], palm_side)
        out.append(f)
    th = G.tube('Thumb', [(x - s * .01, -.04, .805, 1.0), (x - s * .02, -.06, .78, 1.0), (x - s * .027, -.068, .752, .95),
                          (x - s * .03, -.069, .735, .8)], M['Glove'], .0112, sides=12, subsurf=2)
    G.two_tone(th, M['GlovePalm'], lambda c: c.y < -.064 or (c.x - x) * s < -.02)
    out.append(th)
    return out


def leg(M, piv, knee, s):
    x = s * LEG_X
    thigh = G.loft('Thigh', [R(.94, .118, .122, .128, x=x, y=.004), R(.86, .114, .115, .122, x=x, y=.004), R(.76, .104, .102, .11, x=x, y=.004),
                             R(.66, .094, .09, .098, x=x, y=.004), R(.58, .086, .082, .088, x=x, y=.004), R(.52, .081, .079, .081, x=x, y=.004),
                             R(.49, .08, .079, .08, x=x, y=.004)], M['Trousers'])
    G.bend_folds(thigh, .006, .07)
    for p in (thigh, G.rbox('CargoPocket', (x + s * .098, -.006, .70), (.03, .13, .16), M['Trousers'], .016, rot=(0, s * -6, 0), subsurf=1),
              G.rbox('CargoFlap', (x + s * .106, -.006, .773), (.022, .136, .045), M['Trousers'], .01, rot=(0, s * -6, 0), subsurf=1)):
        G.attach(p, piv)
    shin = G.loft('Shin', [R(.53, .081, .079, .081, x=x, y=.004), R(.46, .077, .072, .084, x=x, y=.004), R(.38, .075, .068, .085, x=x, y=.004),
                           R(.30, .072, .068, .077, x=x, y=.004), R(.265, .075, .073, .078, x=x, y=.004, wob=.02, freq=6),
                           R(.235, .074, .074, .076, x=x, y=.006, wob=.035, freq=7, phase=1), R(.215, .066, .068, .071, x=x, y=.008, wob=.02, freq=7),
                           R(.2, .058, .06, .064, x=x, y=.01)], M['Trousers'])
    G.bend_folds(shin, .005, .06)
    parts = [shin,
             G.quad_sphere('KneePad', (x, -.08, .50), (.068, .03, .08), M['KneePad'], cuts=6, power=.75,
                           shape=lambda d, p: Vector((p.x, p.y * (.3 if d.y > 0 else 1.0) - .02 * d.x * d.x, p.z))),
             band('KneeStrapLow', shin, .445, .465, .09, .09, .09, M['Webbing'], (x, .004), .002, .004),
             band('KneeStrapHigh', shin, .535, .555, .09, .09, .09, M['Webbing'], (x, .004), .002, .004)]
    parts += boot(M, x)
    for p in parts:
        G.attach(p, knee)


def boot(M, x):
    """Laced leather work boot: padded collar, toe rubber, chunky lugged sole. About EU 44."""
    up = [(.076, .04, .032, .052, .072), (.063, .052, .047, .094, .087), (.02, .057, .052, .115, .097), (-.03, .059, .052, .088, .082),
          (-.09, .061, .047, .063, .067), (-.15, .06, .042, .048, .057), (-.205, .052, .038, .039, .052), (-.234, .03, .026, .026, .05)]
    upper = G.loft('BootUpper', [{'c': (x, y, zc), 'rx': rx, 'rf': dn, 'rb': u, 'p': 2.3} for y, rx, dn, u, zc in up], M['Boot'], plane='XZ')
    sole = [(.086, .05), (.072, .063), (.02, .067), (-.05, .067), (-.14, .069), (-.205, .062), (-.243, .034)]
    out = [upper,
           G.loft('BootShaft', [R(.225, .064, .066, .07, x=x, y=.01), R(.17, .063, .066, .069, x=x, y=.01), R(.12, .064, .067, .07, x=x, y=.01),
                                R(.08, .062, .066, .068, x=x, y=.01)], M['Boot'], cap=(False, False)),
           G.loft('BootCollar', [R(.232, .068, .07, .074, x=x, y=.01), R(.212, .068, .07, .074, x=x, y=.01)], M['Rubber'], n=32, cap=(False, False)),
           G.loft('Tongue', [R(.225, .022, .064, .01, x=x, y=-.004), R(.15, .022, .066, .01, x=x, y=-.004)], M['Boot'], n=16, cap=(False, False)),
           G.loft('Sole', [{'c': (x, y, .016), 'rx': rx, 'rf': .016, 'p': 4.0} for y, rx in sole], M['Sole'], plane='XZ'),
           G.loft('Midsole', [{'c': (x, y, .036), 'rx': rx * .99, 'rf': .006, 'p': 4.0} for y, rx in sole], M['Rubber'], plane='XZ'),
           G.quad_sphere('ToeRand', (x, -.2, .052), (.056, .046, .03), M['Sole'], cuts=5)]
    for k in range(3):
        zk = .205 - k * .028
        out.append(G.tube('ShaftLace', [(x - .03, -.062, zk), (x, -.076, zk + .012), (x + .03, -.062, zk)], M['Lace'], .0022, sides=6, subsurf=1))
        out.append(G.tube('ShaftLace', [(x + .03, -.062, zk), (x, -.076, zk + .012), (x - .03, -.062, zk)], M['Lace'], .0022, sides=6, subsurf=1))
        for a in (-1, 1):
            out.append(G.rbox('Hook', (x + a * .032, -.058, zk), (.006, .008, .01), M['Steel'], .002))
    for k in range(5):
        yk, zk = -.03 - k * .022, .148 - k * .016
        for a in (-1, 1):
            out.append(G.tube('Lace', [(x - a * .022, yk + .006, zk - .002), (x, yk - .003, zk + .005), (x + a * .022, yk - .012, zk - .006)],
                              M['Lace'], .0022, sides=6, subsurf=1))
            out.append(G.quad_sphere('Eyelet', (x + a * .025, yk, zk - .002), (.0035, .0035, .0035), M['Steel'], cuts=2, subsurf=0))
    return out


def build(M, number=1, coll=None):
    """Returns the root pivot plus the named joints so poses can be set like WorkerPresentation does."""
    G.COLL = coll or bpy.context.scene.collection
    root = G.pivot('Crew', (0, 0, 0))
    body = G.pivot('Body', (0, 0, .95), root)
    torso = G.pivot('Torso', (0, 0, 1.10), body)
    head = G.pivot('Head', (0, 0, 1.52), torso)
    anchor = G.pivot('HeadAnchor', (0, 0, 1.749), head)
    j = {'root': root, 'body': body, 'torso': torso, 'head': head, 'anchor': anchor}
    chest = jacket(M, torso)
    trousers(M, body)
    build_vest(M, torso, chest, number, strap, text)
    build_head(M, head)
    for s, n in ((1, 'L'), (-1, 'R')):
        j['arm' + n] = G.pivot('Arm' + n, (s * ARM_X, 0, ARM_Z), torso)
        j['elbow' + n] = G.pivot('Elbow' + n, (s * ARM_X, 0, ELBOW_Z), j['arm' + n])
        px = arm(M, j['arm' + n], j['elbow' + n], s)
        # Crew number on each shoulder patch, facing out to the side.
        text(str(number), (px + s * .0745, -.004, 1.315), (90, s * -4, s * 90), .03, M['Stencil'], j['arm' + n])
        j['leg' + n] = G.pivot('Leg' + n, (s * LEG_X, 0, HIP_Z), body)
        j['knee' + n] = G.pivot('Knee' + n, (s * LEG_X, 0, KNEE_Z), j['leg' + n])
        leg(M, j['leg' + n], j['knee' + n], s)
    return j
