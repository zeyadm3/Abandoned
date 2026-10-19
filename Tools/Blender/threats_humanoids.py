"""The four original pursuit silhouettes, redesigned around material, anatomy and readable negative space."""
import math
from mathutils import Vector


def blind(a):
    a.loft('StarvedRibcage', [(0, 0, 1.0, .14, .13), (0, .01, 1.22, .19, .15),
        (0, .015, 1.55, .30, .19), (0, .02, 1.83, .27, .16), (0, 0, 1.96, .13, .10)], 'Bone')
    a.loft('WaxCranium', [(0, -.025, 2.03, .08, .095), (0, -.03, 2.16, .17, .17),
        (0, -.035, 2.40, .205, .19), (0, 0, 2.57, .15, .14), (0, .025, 2.65, .04, .055)], 'Bone', 'head')
    a.anatomy('Bone', .145, .155)
    # Listening organs are folded membranes instead of cartoon ears; their dark interiors keep the face eyeless.
    for sign in [-1, 1]:
        a.ellipsoid('ListeningFoldShadow', (sign * .242, -.005, 2.38), (.16, .15, .43), 'Charcoal', 'head')
        a.tube('CartilageFold', [(sign * .21, -.015, 2.17, .026), (sign * .31, -.025, 2.27, .027),
            (sign * .33, .01, 2.48, .035), (sign * .25, .03, 2.59, .019), (sign * .22, -.02, 2.46, .016)], 'Ash', 'head')
        for j in range(4):
            z = 2.28 + j * .06
            a.tube('InnerListeningMembrane', [(sign * .23, -.08, z, .012),
                (sign * .29, -.085, z + .018, .014), (sign * .27, -.035, z + .052, .009)], 'Bone', 'head', sides=6)
        a.tube('NeckTendon', [(sign * .14, -.08, 1.84, .026), (sign * .09, -.09, 2.02, .021),
            (sign * .105, -.12, 2.17, .013)], 'Ash', 'neck')
        a.line('Collarbone', (sign * .035, -.15, 1.83), (sign * .29, -.075, 1.80), .026, 'Ash')
        for i in range(7):
            z = 1.23 + i * .085
            width = .17 + math.sin((i + 1) / 8 * math.pi) * .11
            a.tube('SubdermalRib', [(sign * .025, -.17, z + .025, .013),
                (sign * width * .75, -.195, z, .021), (sign * width, -.105, z - .04, .015)], 'Ash', sides=8)
    a.tube('SealedMouthCrease', [(-.105, -.181, 2.14, .009), (-.03, -.20, 2.12, .012),
        (.035, -.20, 2.13, .010), (.10, -.17, 2.15, .006)], 'Charcoal', 'head', sides=6)
    a.tube('VerticalFacialFold', [(0, -.19, 2.17, .012), (.013, -.223, 2.31, .014),
        (-.009, -.21, 2.48, .008)], 'Ash', 'head', sides=7)
    for i in range(8):
        a.ellipsoid('Vertebra', (0, .17, 1.10 + i * .105), (.08, .10, .06), 'Ash')
    a.plate('OldSternumRestraint', (0, -.184, 1.42), .085, .26, .026, 'Iron')


def stalker(a):
    a.loft('NarrowTailoredCoat', [(0, .015, .60, .28, .20), (0, 0, .88, .25, .18),
        (0, 0, 1.40, .205, .15), (0, 0, 1.93, .22, .16), (0, .025, 2.20, .26, .18),
        (0, 0, 2.35, .14, .115)], 'Charcoal')
    a.anatomy('Charcoal', .092, .105)
    for sign, side in [(-1, 'L'), (1, 'R')]:
        a.cloth('SplitCoatTail', [(1.50, sign * .15, .16, .25), (1.08, sign * .18, .19, .28),
            (.69, sign * .21, .23, .28), (.39 if sign < 0 else .55, sign * .23, .23, .23)], 'Charcoal', folds=.027)
        a.cloth('FrayedFrontPlacket', [(2.20, sign * .09, -.16, .18), (1.68, sign * .11, -.17, .20),
            (1.10, sign * .13, -.19, .19), (.73, sign * .16, -.20, .16)], 'Canvas', folds=.012)
        a.plate('HighCoatLapel', (sign * .16, -.12, 2.15), .20, .43, .045, 'Iron', tilt=sign * .22)
        a.tube('RaisedCollar', [(sign * .27, 0, 2.23, .045), (sign * .23, .11, 2.39, .055),
            (sign * .12, .11, 2.44, .03)], 'Charcoal', 'neck')
        a.stitches('TailSeam', (sign * .21, -.165, 1.05), (sign * .19, -.14, 1.92), 'Iron', count=13, width=.021)
        shoulder = a.bones['arm' + side][0]
        a.ellipsoid('WeatheredShoulderCape', shoulder + Vector((0, .025, -.035)), (.24, .32, .34), 'Canvas', 'arm' + side)
        wrist = a.bones['forearm' + side][1]
        a.plate('SleeveCuffClasp', (wrist.x, wrist.y - .056, wrist.z + .10), .11, .12, .035, 'Iron', 'forearm' + side)
    # A real hood cavity around a nearly lost face; the surrounding folded rim carries the silhouette.
    a.loft('HoodCrown', [(0, .04, 2.43, .15, .13), (0, .06, 2.66, .185, .17),
        (.015, .075, 2.92, .155, .14), (.025, .08, 3.10, .035, .05)], 'Charcoal', 'head')
    a.ellipsoid('UnlitHoodInterior', (0, -.10, 2.73), (.27, .15, .40), 'Charcoal', 'head')
    a.tube('FoldedHoodEdge', [(-.105, -.13, 2.47, .022), (-.19, -.145, 2.66, .034),
        (-.155, -.15, 2.90, .030), (0, -.13, 3.02, .035), (.15, -.15, 2.90, .030),
        (.175, -.145, 2.68, .028), (.105, -.14, 2.48, .018)], 'Canvas', 'head')
    for sign in [-1, 1]:
        a.plate('RecessedEye', (sign * .048, -.185, 2.75), .040, .012, .008, 'Eyes', 'head', tilt=sign * .08)
    for i in range(6):
        z = 1.14 + i * .16
        a.plate('OldCoatToggle', (0, -.205, z), .042, .026, .025, 'Iron')
    a.line('LowBelt', (-.18, -.16, 1.28), (.18, -.16, 1.28), .025, 'Iron')


def collector(a):
    a.loft('HunchedPorterBody', [(0, .03, .39, .25, .23), (0, .10, .60, .34, .30),
        (0, .15, .90, .37, .34), (0, .10, 1.13, .25, .22), (0, -.07, 1.21, .14, .12)], 'Canvas')
    a.anatomy('Canvas', .145, .17, fingers=4)
    a.loft('OversizedStitchedPack', [(0, .45, .47, .26, .20), (.025, .49, .64, .43, .32),
        (0, .48, .99, .45, .34), (-.035, .42, 1.25, .30, .25), (0, .40, 1.34, .12, .13)], 'Canvas')
    for sign in [-1, 1]:
        a.tube('PackWebbing', [(sign * .24, .59, 1.20, .032), (sign * .28, .06, 1.11, .034),
            (sign * .25, -.26, .92, .028), (sign * .22, -.28, .57, .028), (sign * .30, .45, .51, .025)], 'Charcoal')
        a.plate('PackStrapAdjuster', (sign * .25, -.30, .76), .10, .12, .025, 'Iron')
        for i in range(3):
            z = .65 + i * .20
            a.tube('RopePackLacing', [(sign * .38, .45, z, .012), (sign * .43, .68, z + .08, .013),
                (sign * .32, .74, z + .15, .010)], 'Bone')
    a.stitches('SackSpineSeam', (0, .814, .59), (0, .774, 1.21), 'Bone', count=18, width=.065)
    for i in range(4):
        a.plate('PackRepairPatch', ((i % 2 - .5) * .36, .765, .71 + (i // 2) * .29), .18, .17, .018, 'Charcoal', tilt=(i - 1.5) * .13)
    a.ellipsoid('LowCowledSkull', (0, -.29, 1.26), (.46, .43, .42), 'Charcoal', 'head')
    a.plate('HammeredMask', (0, -.507, 1.26), .36, .31, .065, 'Iron', 'head')
    a.plate('MaskCheekL', (-.12, -.55, 1.20), .12, .16, .027, 'Rust', 'head', tilt=-.17)
    a.plate('MaskCheekR', (.12, -.55, 1.20), .12, .16, .027, 'Rust', 'head', tilt=.17)
    a.plate('BlindVisorSlit', (0, -.548, 1.30), .24, .028, .015, 'Charcoal', 'head')
    for i in range(6):
        a.box('MaskVent', ((i - 2.5) * .031, -.556, 1.19), (.014, .014, .063), 'Charcoal', 'head', bevel=.002)
    for sign in [-1, 1]:
        for z in [1.16, 1.36]:
            a.ellipsoid('MaskRivet', (sign * .145, -.547, z), (.017, .012, .017), 'Bone', 'head')
    # The pack suggests hidden salvage through sewn bulk, without copying any actual pickable model.
    for i in range(5):
        a.tube('BentCopperTagLoop', [((i - 2) * .115, -.285, .82, .010),
            ((i - 2) * .12, -.33, .70, .009), ((i - 2) * .11, -.30, .65, .006)], 'Rust')
    a.cloth('FrayedPorterApron', [(.92, 0, -.295, .47), (.67, .02, -.33, .46),
        (.43, -.02, -.31, .42)], 'Charcoal', folds=.025)


def hunter(a):
    a.loft('BrutalLoadBearingTorso', [(0, .015, .94, .35, .28), (0, .01, 1.23, .48, .37),
        (0, .02, 1.69, .64, .44), (0, .02, 2.02, .67, .40), (0, .02, 2.18, .42, .29)], 'Ash')
    a.anatomy('Ash', .37, .36, fingers=3, cuffs=True)
    for sign, side in [(-1, 'L'), (1, 'R')]:
        a.plate('ForgedShoulderYoke', (sign * .58, -.06, 2.13), .59, .29, .69, 'Iron', tilt=sign * .10)
        a.plate('ShoulderCurb', (sign * .67, -.435, 2.11), .34, .13, .07, 'Rust')
        for j in range(3):
            z = 1.23 + j * .21
            a.plate('LayeredChestSlab', (sign * .28, -.421 - j * .008, z), .50, .24, .075, 'Iron', tilt=sign * (.05 + j * .018))
            for xoff in [-.16, .16]:
                a.ellipsoid('RecessedPlateRivet', (sign * .28 + xoff, -.477 - j * .008, z), (.033, .02, .033), 'Rust')
        elbow = a.bones['forearm' + side][0]
        hand = a.bones['forearm' + side][1]
        a.plate('SplintedForearm', (hand.x, hand.y - .18, (hand.z + elbow.z) * .5), .35, .45, .085, 'Iron', 'forearm' + side, tilt=-sign * .09)
        a.ellipsoid('HammerKnuckle', hand + Vector((0, -.075, -.035)), (.39, .30, .29), 'Iron', 'forearm' + side)
        for j in range(3):
            a.plate('KnuckleTooth', (hand.x + (j - 1) * .10, hand.y - .23, hand.z - .09), .07, .13, .06, 'Bone', 'forearm' + side)
        hip, knee = a.bones['leg' + side]
        a.plate('ThighRestraint', (hip.x, hip.y - .18, (hip.z + knee.z) * .5), .31, .34, .06, 'Charcoal', 'leg' + side)
        a.plate('KneeGuard', (knee.x, knee.y - .17, knee.z), .30, .25, .085, 'Iron', 'leg' + side)
    a.loft('BuriedBatteringHead', [(0, -.14, 2.16, .17, .18), (0, -.17, 2.37, .23, .22),
        (0, -.13, 2.62, .22, .20), (0, -.08, 2.75, .10, .12)], 'Charcoal', 'head')
    a.plate('ArmouredDeathMask', (0, -.375, 2.43), .39, .37, .065, 'Iron', 'head')
    a.plate('RecessedVisor', (0, -.416, 2.48), .265, .043, .018, 'Charcoal', 'head')
    for sign in [-1, 1]:
        a.plate('BuriedEye', (sign * .071, -.428, 2.484), .045, .014, .01, 'Eyes', 'head')
        a.tube('MaskCheekWeld', [(sign * .17, -.415, 2.47, .015), (sign * .14, -.431, 2.35, .018),
            (sign * .055, -.425, 2.28, .012)], 'Rust', 'head')
    for i in range(5):
        a.box('MouthBaffle', ((i - 2) * .045, -.428, 2.35), (.018, .02, .08), 'Charcoal', 'head', bevel=.003)
    for i in range(4):
        a.plate('SternumBridge', (0, -.474, 1.14 + i * .23), .14, .15, .07, 'Rust')
    a.tube('BrokenBackBrace', [(0, .30, 1.02, .09), (0, .45, 1.39, .11),
        (0, .44, 1.85, .10), (0, .22, 2.12, .07)], 'Iron')
