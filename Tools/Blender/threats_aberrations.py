"""Late-job aberrations completely redesigned with psychological horror, grotesque body parts,
screaming faces, matted hair, and visceral depth.
"""
import math
from mathutils import Vector


def weight(a):
    """The Weight: a colossal, crushing ceiling horror. Heavy vaulted rib arches, hanging weeping corpse limbs,
    thick tension cables, and an agonized giant mask weeping dark bile and tangled in coarse wire-hair."""
    a.loft('CeilingCarapace', [
        (0, .16, 3.05, .54, .53), (0, .14, 3.35, 1.18, 1.01),
        (0, .15, 3.81, 1.63, 1.28), (0, .15, 4.17, 1.60, 1.22),
        (0, .16, 4.49, 1.22, .91), (0, .19, 4.64, .53, .49)
    ], 'Charcoal', sides=22)
    a.anatomy('Iron', .32, .28, fingers=0, cuffs=True)

    # Massive load-bearing vault ribs with protruding bone spurs
    for row in range(7):
        y = -.90 + row * .34
        radius = 1.61 - .10 * abs(row - 3)
        points = []
        for j in range(13):
            u = j / 12 * math.pi
            points.append((-math.cos(u) * radius, y, 3.78 + math.sin(u) * .88, .075))
        a.tube(f'VaultedLoadRib_{row}', points, 'Ash', sides=10)
        # Jagged bone spurs pointing downwards from the ribs
        for sign in [-1, 1]:
            a.tube(f'CeilingSpur_{row}_{sign}', [
                (sign * radius * 0.75, y, 3.82, 0.035),
                (sign * radius * 0.70, y, 3.45, 0.008)
            ], 'Bone', sides=5)

    # Heavy tension cables and wet sinewy load tendons
    for sign in [-1, 1]:
        for j in range(3):
            y = -.62 + j * .64
            a.tube(f'LoadTendon_{sign}_{j}', [
                (sign * 1.25, y, 3.91, .12), (sign * 1.52, y + .10, 3.36, .14),
                (sign * 1.66, y + .13, 2.93, .09), (sign * 1.70, y + .11, 2.59, .055)
            ], 'Gore', sides=12)
            a.tube(f'TensionCable_{sign}_{j}', [
                (sign * 1.32, y, 4.03, .023),
                (sign * 1.60, y, 3.64, .025), (sign * 1.70, y + .10, 3.02, .018)
            ], 'Iron')

    # Head: Trapped colossal agonized face embedded in the ceiling mass
    a.loft('CeilingMaskHead', [
        (0, -.81, 4.04, .22, .24), (0, -.87, 4.23, .36, .36),
        (0, -.83, 4.51, .36, .34), (0, -.75, 4.68, .22, .22)
    ], 'Iron', 'head')
    
    # Trapped pale screaming face
    a.ellipsoid('ColossalTrappedFace', (0, -1.18, 4.35), (0.46, 0.28, 0.44), 'Bone', 'head')
    # Horror maw with needle teeth weeping from the ceiling
    a.horror_maw('CeilingFaceMaw', (0, -1.22, 4.24), width=0.28, height=0.12, depth=0.18,
                 teeth_count=18, bone='head', gum_mat='Gore', tooth_mat='Bone', gaping=0.12)
    # Burning retroreflective eyes looking down on the salvage crew
    a.horror_eyes('CeilingFaceEyes', (0, -1.21, 4.40), eye_dist=0.16, radius=0.038, bone='head',
                  eye_mat='Eyes', socket_mat='Charcoal', gore_mat='Gore')

    # Coarse tangled hair hanging down from the ceiling around the titan mask
    a.hair_curtain('CeilingHairDrape', (0, -0.92, 4.65), count=22, length=1.20, material='Hair',
                   bone='head', spread_x=0.45, spread_y=0.28, z_drop=0.15, forward_bias=-0.14, wave_amp=0.045)


def thing(a):
    """The Thing: an uncanny, grotesque cluster of human forms trying to mimic humanity.
    Multiple contorted human faces weeping and screaming across one cranial mass, long stringy hair,
    misaligned jaws with needle teeth, split chest with exposed viscera, and extra clawed finger joints."""
    # Stitched false-human torso
    a.loft('MutatedHumanTorso', [
        (0, .02, .90, .20, .17), (0, .015, 1.13, .25, .21),
        (.025, .015, 1.51, .33, .28), (-.015, .03, 1.76, .34, .25),
        (0, .02, 1.87, .19, .15)
    ], 'Canvas')
    
    # Chest split wide open down the sternum with jagged rib teeth and exposed viscera
    a.tube('SplitChestChasm', [(0, -0.16, 1.05, 0.024), (0, -0.22, 1.45, 0.042), (0, -0.18, 1.80, 0.030)], 'Gore', 'spine')
    for r in range(6):
        z = 1.15 + r * 0.11
        for sign in [-1, 1]:
            a.tube(f'MawRibTooth_{r}_{sign}', [
                (sign * 0.08, -0.19, z, 0.014),
                (sign * 0.01, -0.25, z - 0.02, 0.005)
            ], 'Bone', 'spine', sides=5)
    
    # Multi-headed / misaligned cranial horror mass
    a.loft('ClusterCranium', [
        (0, -.02, 1.77, .13, .14), (.025, -.04, 1.96, .28, .24),
        (-.02, -.03, 2.18, .30, .25), (.015, .01, 2.32, .18, .17), (0, .03, 2.38, .06, .08)
    ], 'Bone', 'head')

    # Primary weeping face with gaping maw
    a.horror_maw('ThingPrimaryMaw', (0, -0.22, 2.06), width=0.19, height=0.08, depth=0.12,
                 teeth_count=16, bone='head', gum_mat='Gore', tooth_mat='Bone', gaping=0.10)
    a.horror_eyes('ThingPrimaryEyes', (0, -0.23, 2.18), eye_dist=0.11, radius=0.032, bone='head',
                  eye_mat='Eyes', socket_mat='Charcoal', gore_mat='Gore')

    # Secondary distorted face grafted onto the right temple
    a.ellipsoid('SecondaryFaceGraft', (0.16, -0.14, 2.22), (0.16, 0.14, 0.18), 'Bone', 'head')
    a.horror_maw('ThingSecondaryMaw', (0.17, -0.18, 2.14), width=0.12, height=0.06, depth=0.08,
                 teeth_count=10, bone='head', gum_mat='Gore', tooth_mat='Bone', gaping=0.07)
    a.horror_eyes('ThingSecondaryEyes', (0.18, -0.18, 2.24), eye_dist=0.06, radius=0.022, bone='head',
                  eye_mat='Eyes', socket_mat='Charcoal', gore_mat='Gore')

    # Long greasy black human hair hanging in ragged stringy clumps across both faces
    a.hair_curtain('ThingCrownHair', (0, 0.04, 2.36), count=20, length=0.65, material='Hair',
                   bone='head', spread_x=0.25, spread_y=0.18, z_drop=0.08, forward_bias=-0.14, wave_amp=0.04)
    a.hair_curtain('ThingFaceStrands', (0.05, -0.12, 2.28), count=12, length=0.48, material='Hair',
                   bone='head', spread_x=0.18, spread_y=0.08, z_drop=0.04, forward_bias=-0.08, wave_amp=0.03)

    # Limbs with horror hands and elongated multi-jointed fingers
    a.anatomy('Ash', .13, .16, fingers=0)
    for sign, side in [(-1, 'L'), (1, 'R')]:
        hand_pos = a.bones['forearm' + side][1]
        a.horror_hand('forearm' + side, hand_pos, arm_width=0.13, fingers=5, finger_len=0.30,
                      claw_len=0.10, material='Bone', claw_mat='Charcoal', sign=sign)


def crawlers(a):
    """The Crawlers: skittering human-spider chimeras. Each entity features an inverted, screaming pale human face,
    dark weeping hollows, long black hair dragging across the floor like spider silk, and barbed scythe legs."""
    for n, (x, y) in enumerate([(0, 0), (-.55, .22), (.58, .28), (-.34, .82), (.42, .9)]):
        bone = 'spine' if n == 0 else 'armL' if n % 2 else 'armR'
        
        # Abdomen and thorax
        a.ellipsoid(f'CrawlerBody_{n}', (x, y + .035, .29), (.48, .66, .31), 'Charcoal', bone)
        a.ellipsoid(f'CrawlerThorax_{n}', (x, y - .23, .29), (.36, .26, .27), 'Gore', bone)

        # Segmented dorsal bone plates with spinal spikes
        for plate in range(4):
            py = y - .08 + plate * .14
            a.loft(f'CarapacePlate_{n}_{plate}', [
                (x, py, .27, .19 - plate * .015, .12),
                (x, py, .40 + math.sin(plate * .8) * .025, .19 - plate * .018, .115),
                (x, py, .46 - plate * .012, .09, .055)
            ], 'Bone' if plate % 2 == 0 else 'Charcoal', bone, sides=12)
            # Upward bone spur
            a.tube(f'CarapaceSpur_{n}_{plate}', [
                (x, py, .44, .018),
                (x, py, .56, .004)
            ], 'Bone', bone, sides=5)

        # INVERTED HUMAN FACE ON HEAD: Upside-down pale human face screaming in agony
        head_y = y - 0.38
        a.ellipsoid(f'InvertedHumanFace_{n}', (x, head_y, 0.28), (0.24, 0.18, 0.22), 'Bone', bone)
        
        # Screaming horror maw with needle teeth
        a.horror_maw(f'CrawlerMaw_{n}', (x, head_y - 0.08, 0.23), width=0.14, height=0.06, depth=0.08,
                     teeth_count=12, bone=bone, gum_mat='Gore', tooth_mat='Bone', gaping=0.07)
        # Cluster of weeping eyes that flash in flashlights
        a.horror_eyes(f'CrawlerEyes_{n}', (x, head_y - 0.06, 0.32), eye_dist=0.07, radius=0.024, bone=bone,
                      eye_mat='Eyes', socket_mat='Charcoal', gore_mat='Gore')

        # Long, stringy black hair draping from the face down onto the floor like spider webbing
        a.hair_curtain(f'CrawlerHairWeb_{n}', (x, head_y + 0.05, 0.35), count=12, length=0.45, material='Hair',
                       bone=bone, spread_x=0.18, spread_y=0.12, z_drop=0.04, forward_bias=-0.08, wave_amp=0.035)

        # Multi-jointed barbed skeletal spider limbs with sharp chitinous talons
        for sign in [-1, 1]:
            # Segmented mandibles flanking the screaming maw
            a.tube(f'Mandible_{n}_{sign}', [
                (x + sign * .07, head_y - .02, .22, .022),
                (x + sign * .14, head_y - .10, .18, .018),
                (x + sign * .04, head_y - .14, .16, .005)
            ], 'Iron', bone, sides=6)
            for j in range(3):
                at = (x + sign * .18, y - .17 + j * .19, .29)
                knee = (x + sign * (.46 + .03 * (j % 2)), y - .28 + j * .19, .38)
                foot = (x + sign * .58, y - .36 + j * .20, .03)
                # Upper leg with barbed bone spines
                a.tube(f'BarbedFemur_{n}_{sign}_{j}', [
                    (*at, .036),
                    (x + sign * .33, y - .22 + j * .19, .36, .044),
                    (*knee, .028)
                ], 'Bone', bone, sides=7)
                a.ellipsoid(f'LimbKnee_{n}_{sign}_{j}', knee, (.075, .08, .075), 'Gore', bone)
                # Lower scythe leg ending in needle claw
                a.tube(f'ScytheTibia_{n}_{sign}_{j}', [
                    (*knee, .026),
                    (x + sign * .54, y - .32 + j * .20, .14, .018),
                    (*foot, .004)
                ], 'Iron', bone, sides=6)


def last(a):
    """The Last Hunter: the supreme executioner horror. Towering titan crowned with massive rusted spikes
    driven through its cracked skull, long tangled death shrouds and tresses of decayed black hair,
    an iron-jawed screaming visage with burning deathly eyes, and colossal barbed gauntlets."""
    # Towering monolithic torso
    a.loft('ExecutionerTorso', [
        (0, 0, .96, .30, .26), (0, 0, 1.40, .38, .32),
        (0, .02, 2.04, .52, .38), (0, .05, 2.65, .58, .42),
        (0, .02, 2.96, .40, .28), (0, 0, 3.10, .22, .18)
    ], 'Charcoal')
    
    # Protruding dorsal spinal spikes and executioner chains
    a.spinal_spikes(1.05, 2.95, 0.28, count=10, spike_len=0.18, material='Bone', bone='spine', gore_mat='Gore')

    # Shroud drapery and torn funeral banners
    for sign, side in [(-1, 'L'), (1, 'R')]:
        a.cloth(f'HeavyFuneralMantle_{side}', [
            (2.92, sign * .47, .27, .48), (2.45, sign * .56, .36, .54),
            (1.92, sign * .59, .39, .46), (1.25 if sign < 0 else 1.50, sign * .57, .35, .36)
        ], 'Canvas', 'arm' + side, folds=.055)

    # Colossal skull crowned with brutal rusted rebar spikes
    a.loft('TitanExecutionerSkull', [
        (0, -.02, 3.10, .14, .16), (0, -.03, 3.36, .22, .25),
        (0, -.015, 3.64, .25, .25), (0, .02, 3.86, .15, .16), (0, .04, 3.92, .06, .08)
    ], 'Bone', 'head')

    # Brutal rebar spike crown driven through the skull
    for sign in [-1, 1]:
        for i in range(4):
            a.tube(f'CrownSpike_{sign}_{i}', [
                (sign * (.14 + i * 0.07), .01, 3.70, .042),
                (sign * (.26 + i * 0.12), .06 + i * .06, 3.95 - i * .04, .036),
                (sign * (.38 + i * 0.18), .15 + i * .10, 4.22 - i * .10, .010)
            ], 'Iron', 'head', sides=7)

    # Terrifying iron-jawed screaming visage
    a.horror_maw('LastExecutionerMaw', (0, -0.28, 3.32), width=0.26, height=0.12, depth=0.16,
                 teeth_count=18, bone='head', gum_mat='Gore', tooth_mat='Bone', gaping=0.14)
    # Burning death eyes that reflect flashlights with terrifying intensity
    a.horror_eyes('LastExecutionerEyes', (0, -0.29, 3.55), eye_dist=0.14, radius=0.038, bone='head',
                  eye_mat='Eyes', socket_mat='Charcoal', gore_mat='Gore')

    # Long billowing tresses of rotten, matted black hair cascading down the executioner's back and shoulders
    a.hair_curtain('ExecutionerHairCrown', (0, 0.08, 3.82), count=24, length=0.95, material='Hair',
                   bone='head', spread_x=0.34, spread_y=0.22, z_drop=0.14, forward_bias=-0.08, wave_amp=0.048)

    # Colossal arms with barbed crushing gauntlets
    a.anatomy('Iron', .25, .29, fingers=0, cuffs=True)
    for sign, side in [(-1, 'L'), (1, 'R')]:
        hand_pos = a.bones['forearm' + side][1]
        a.horror_hand('forearm' + side, hand_pos, arm_width=0.25, fingers=4, finger_len=0.38,
                      claw_len=0.15, material='Bone', claw_mat='Iron', sign=sign)
