"""Late-job aberrations: ceiling mass, false human, skittering swarm and the final executioner."""
import math
from mathutils import Vector


def weight(a):
    a.loft('CeilingCarapace', [(0, .16, 3.05, .54, .53), (0, .14, 3.35, 1.18, 1.01),
        (0, .15, 3.81, 1.63, 1.28), (0, .15, 4.17, 1.60, 1.22),
        (0, .16, 4.49, 1.22, .91), (0, .19, 4.64, .53, .49)], 'Charcoal', sides=22)
    a.anatomy('Iron', .32, .28, fingers=3, cuffs=True)
    # Heavy overlapping vault segments carry the ceiling silhouette, rather than an oversized round torso.
    for row in range(7):
        y = -.90 + row * .34
        radius = 1.61 - .10 * abs(row - 3)
        points = []
        for j in range(13):
            u = j / 12 * math.pi
            points.append((-math.cos(u) * radius, y, 3.78 + math.sin(u) * .88, .075))
        a.tube('VaultedLoadRib', points, 'Ash', sides=10)
        for sign in [-1, 1]:
            a.plate('CarapaceEdgeFastening', (sign * radius * .92, y, 3.85), .20, .17, .20, 'Iron', tilt=sign * .32)
    for sign in [-1, 1]:
        for j in range(3):
            y = -.62 + j * .64
            a.tube('LoadTendon', [(sign * 1.25, y, 3.91, .12), (sign * 1.52, y + .10, 3.36, .14),
                (sign * 1.66, y + .13, 2.93, .09), (sign * 1.70, y + .11, 2.59, .055)], 'Canvas', sides=12)
            a.tube('CarapaceTensionCable', [(sign * 1.32, y, 4.03, .023),
                (sign * 1.60, y, 3.64, .025), (sign * 1.70, y + .10, 3.02, .018)], 'Iron')
        for i in range(4):
            a.ellipsoid('SuspendedUndersideFold', (sign * (.31 + i * .14), -.12 + (i % 2) * .31, 3.18),
                        (.34, .52, .49), 'Canvas')
    a.loft('CeilingMaskHead', [(0, -.81, 4.04, .19, .23), (0, -.87, 4.23, .32, .34),
        (0, -.83, 4.51, .33, .31), (0, -.75, 4.68, .19, .20)], 'Iron', 'head')
    a.plate('TrappedPaleFace', (0, -1.178, 4.35), .44, .29, .045, 'Bone', 'head')
    a.ring('FaceYoke', (0, -1.16, 4.35), .31, .245, .042, 'Rust')
    a.plate('BlankVisorIndent', (0, -1.214, 4.39), .26, .019, .012, 'Charcoal', 'head')
    for sign in [-1, 1]:
        a.line('FaceBracket', (sign * .26, -1.10, 4.20), (sign * .28, -.99, 4.54), .028, 'Iron', 'head')
    for i in range(5):
        a.plate('UndersideBreather', ((i - 2) * .06, -1.217, 4.28), .025, .045, .012, 'Charcoal', 'head')
    a.tube('CarapaceCrownSeam', [(0, -.80, 4.56, .045), (.03, -.30, 4.72, .055),
        (0, .19, 4.84, .06), (-.025, .69, 4.68, .045), (0, 1.10, 4.49, .035)], 'Iron')


def thing(a):
    a.loft('HumanShell', [(0, .02, .90, .19, .16), (0, .015, 1.13, .23, .20),
        (.025, .015, 1.51, .31, .26), (-.015, .03, 1.76, .32, .23),
        (0, .02, 1.87, .17, .14)], 'Canvas')
    a.anatomy('Ash', .13, .16, fingers=5)
    # Several offset porcelain faces share one skull, each surrounded by a separate black recess.
    a.loft('MisalignedCranialShell', [(0, -.02, 1.77, .12, .13), (.025, -.04, 1.96, .26, .22),
        (-.02, -.03, 2.18, .28, .23), (.015, .01, 2.32, .15, .15), (0, .03, 2.35, .04, .07)], 'Bone', 'head')
    for layer in range(3):
        x = (layer - 1) * .018
        y = -.273 - layer * .048
        z = 2.07 + layer * .017
        w, h = .45 - layer * .092, .43 - layer * .063
        a.plate('FaceRecess', (x, y, z), w, h, .032, 'Charcoal', 'head', tilt=(layer - 1) * .10)
        a.ring('PorcelainFaceEdge', (x, y - .018, z), w * .46, h * .48, .024 - layer * .003, 'Bone')
        a.plate('OffsetNosePlane', (x + .007, y - .035, z), .055, h * .37, .037, 'Bone', 'head', tilt=(layer - 1) * .14)
        a.plate('OffsetChin', (x, y - .033, z - h * .30), w * .47, .07, .035, 'Bone', 'head', tilt=-layer * .06)
    for sign in [-1, 1]:
        a.plate('LostEyeHollow', (sign * .054 + .018, -.423, 2.143), .052, .025, .012, 'Charcoal', 'head', tilt=sign * .12)
        a.cloth('UnbuttonedHumanLapel', [(1.79, sign * .13, -.20, .18), (1.48, sign * .14, -.257, .21),
            (1.12, sign * .15, -.22, .19), (.97, sign * .15, -.18, .16)], 'Charcoal', folds=.016)
        a.stitches('TailoredShellRepair', (sign * .22, -.18, 1.13), (sign * .25, -.19, 1.65), 'Bone', count=11, width=.042)
        for j in range(3):
            a.tube('ImpossibleNeckFold', [(sign * .08, -.10, 1.79 + j * .045, .013),
                (sign * .15, -.12, 1.82 + j * .045, .016), (sign * .13, .05, 1.84 + j * .045, .009)], 'Ash', 'neck')
    # An offset back closure implies a body assembled inside the wrong coat, without explicit gore.
    a.line('OffsetBackClosure', (.07, .264, 1.10), (.12, .253, 1.71), .011, 'Iron')
    for i in range(10):
        z = 1.13 + i * .055
        a.line('BackClosureStaple', (.035, .266, z), (.15, .266, z + .025), .006, 'Rust', sides=6)
    for side, sign in [('L', -1), ('R', 1)]:
        elbow = a.bones['forearm' + side][0]
        a.ellipsoid('MismatchedElbowCuff', elbow + Vector((0, -.02, -.015)), (.17, .19, .13), 'Charcoal', 'forearm' + side)
    for i in range(3):
        a.plate('UnfamiliarButton', (.04, -.267, 1.22 + i * .18), .037, .037, .014, 'Iron')


def crawlers(a):
    # The five original swarm centres and bone assignments stay intact for replication and animation.
    for n, (x, y) in enumerate([(0, 0), (-.55, .22), (.58, .28), (-.34, .82), (.42, .9)]):
        bone = 'spine' if n == 0 else 'armL' if n % 2 else 'armR'
        a.ellipsoid('CrawlerLowBody', (x, y + .035, .29), (.48, .66, .31), 'Charcoal', bone)
        a.ellipsoid('CrawlerThorax', (x, y - .23, .29), (.36, .26, .27), 'Iron', bone)
        for plate in range(4):
            py = y - .08 + plate * .14
            a.loft('SegmentedCarapace', [(x, py, .27, .19 - plate * .015, .12),
                (x, py, .40 + math.sin(plate * .8) * .025, .19 - plate * .018, .115),
                (x, py, .46 - plate * .012, .09, .055)], 'Ash' if plate % 2 else 'Iron', bone, sides=12)
        a.plate('CrackedCrawlerMask', (x, y - .383, .27), .265, .19, .06, 'Bone', bone)
        for sign in [-1, 1]:
            a.ellipsoid('RecessedCrawlerSensor', (x + sign * .065, y - .423, .295), (.047, .015, .036), 'Charcoal', bone)
            a.tube('FoldingMandible', [(x + sign * .095, y - .36, .22, .024),
                (x + sign * .15, y - .43, .16, .020), (x + sign * .06, y - .47, .15, .005)], 'Iron', bone, sides=7)
            for j in range(3):
                at = (x + sign * .18, y - .17 + j * .19, .29)
                knee = (x + sign * (.42 + .025 * (j % 2)), y - .28 + j * .19, .20)
                foot = (x + sign * .52, y - .36 + j * .20, .045)
                a.tube('TaperedCrawlerFemur', [(*at, .035),
                    (x + sign * .31, y - .20 + j * .19, .30, .041), (*knee, .027)], 'Ash', bone, sides=8)
                a.ellipsoid('CrawlerKnee', knee, (.071, .075, .069), 'Iron', bone)
                a.tube('HookedCrawlerFoot', [(*knee, .023),
                    (x + sign * .49, y - .32 + j * .20, .09, .018), (*foot, .007)], 'Charcoal', bone, sides=7)
        a.line('CarapaceSpine', (x, y - .11, .456), (x, y + .41, .414), .014, 'Bone', bone, sides=6)
        for j in range(3):
            a.plate('MaskBreathingSlot', (x + (j - 1) * .045, y - .421, .23), .016, .044, .012, 'Charcoal', bone)


def last(a):
    a.loft('ExecutionerTorso', [(0, 0, .96, .28, .24), (0, 0, 1.40, .36, .30),
        (0, .02, 2.04, .49, .35), (0, .05, 2.65, .54, .38),
        (0, .02, 2.96, .37, .26), (0, 0, 3.10, .20, .16)], 'Charcoal')
    a.anatomy('Iron', .23, .28, fingers=4, cuffs=True)
    for sign, side in [(-1, 'L'), (1, 'R')]:
        a.plate('BrokenMantleYoke', (sign * .54, .10, 2.73), .53, .42, .54, 'Iron', tilt=sign * .18)
        a.cloth('HeavyTornMantle', [(2.92, sign * .47, .27, .46), (2.45, sign * .56, .36, .51),
            (1.92, sign * .59, .39, .43), (1.29 if sign < 0 else 1.55, sign * .57, .35, .34)],
            'Canvas', 'arm' + side, folds=.045)
        a.cloth('ApronSide', [(2.52, sign * .29, -.29, .34), (1.91, sign * .32, -.34, .35),
            (1.18, sign * .30, -.31, .31), (.74 if sign < 0 else .90, sign * .31, -.27, .27)], 'Charcoal', folds=.032)
        for i in range(3):
            # Only three purposeful, curved crown prongs per side, inside the original crown envelope.
            a.tube('BentRebarCrown', [(sign * .16, .02, 3.67, .040),
                (sign * (.31 + i * .12), .08 + i * .08, 3.80 - i * .045, .039),
                (sign * (.40 + i * .24), .20 + i * .12, 3.93 - i * .14, .025)], 'Iron', 'head', sides=9)
        a.tube('CollarTensionRod', [(sign * .19, .10, 3.09, .043), (sign * .42, .19, 2.90, .048),
            (sign * .66, .16, 2.72, .035)], 'Rust', 'neck')
        wrist = a.bones['forearm' + side][1]
        a.plate('ExecutionerWristSplint', (wrist.x, wrist.y - .105, wrist.z + .22), .22, .35, .075, 'Charcoal', 'forearm' + side)
    # The shutter armour is a joined, worn garment of slats, with actual rolled returns and recessed fasteners.
    for i in range(7):
        z = 1.14 + i * .215
        width = .88 - i * .047
        a.plate('OverlappingShutterScale', (0, -.36 - i * .008, z), width, .245, .065, 'Iron')
        a.line('RolledSlatEdge', (-width * .43, -.408 - i * .008, z - .085),
               (width * .43, -.408 - i * .008, z - .085), .013, 'Rust')
        for sign in [-1, 1]:
            a.ellipsoid('SlatRivet', (sign * width * .34, -.406 - i * .008, z + .045), (.027, .017, .027), 'Bone')
    a.loft('LongExecutionerSkull', [(0, -.02, 3.10, .13, .15), (0, -.03, 3.36, .20, .23),
        (0, -.015, 3.64, .23, .23), (0, .02, 3.86, .14, .15), (0, .04, 3.90, .055, .07)], 'Bone', 'head')
    a.plate('RecessedFaceWell', (0, -.255, 3.49), .35, .49, .045, 'Charcoal', 'head')
    a.plate('LeftMaskBlade', (-.112, -.293, 3.51), .14, .47, .048, 'Iron', 'head', tilt=-.08)
    a.plate('RightMaskBlade', (.112, -.293, 3.51), .14, .47, .048, 'Iron', 'head', tilt=.08)
    a.plate('PaleVerticalFaceSeam', (0, -.283, 3.46), .054, .28, .019, 'Bone', 'head')
    a.plate('NarrowFinalEyeLine', (0, -.313, 3.565), .178, .018, .015, 'Eyes', 'head')
    a.plate('MaskJawClamp', (0, -.294, 3.29), .24, .10, .055, 'Rust', 'head')
    for i in range(8):
        z = 1.26 + i * .20
        a.plate('RearSpinalScale', (0, .34, z), .15, .17, .08, 'Iron')
        if i % 2 == 0:
            a.ring('RearHarnessLink', (0, .40, z), .063, .10, .018, 'Rust', 'spine', segments=12)
