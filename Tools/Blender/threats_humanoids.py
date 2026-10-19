"""The four original pursuit silhouettes, completely redesigned with terrifying depth, grotesque emotion,
expressive horror faces, matted wet hair, clear readable anatomical body parts, and horrifying flashlight-reactive features.
"""
import math
from mathutils import Vector


def blind(a):
    """The Blind One: an emaciated, agonizing nightmare with a gaping rictus maw of needle teeth,
    sunken weeping sockets, protruding spinal spikes, matted stringy hair, and elongated talon claws."""
    # Emaciated, starved torso with visible skeletal framework
    a.loft('StarvedTorso', [
        (0, 0, 1.0, .14, .13), (0, .01, 1.22, .18, .14),
        (0, .015, 1.55, .28, .17), (0, .02, 1.83, .25, .15), (0, 0, 1.96, .13, .10)
    ], 'Bone')
    
    # Protruding dorsal spinal spikes and split flesh along the back
    a.spinal_spikes(1.05, 1.95, 0.16, count=10, spike_len=0.12, material='Bone', bone='spine', gore_mat='Gore')

    # Individual curved exposed ribs bursting through torn chest flesh
    for i in range(7):
        z = 1.20 + i * 0.09
        w = 0.19 + math.sin((i + 1) / 8 * math.pi) * 0.12
        for sign in [-1, 1]:
            a.tube(f'ExposedRib_{i}_{sign}', [
                (sign * 0.02, 0.14, z + 0.02, 0.015),
                (sign * w * 0.95, 0.02, z + 0.01, 0.018),
                (sign * w * 0.75, -0.18, z - 0.02, 0.016),
                (sign * 0.025, -0.17, z - 0.035, 0.012)
            ], 'Bone', 'spine', sides=6)
            # Raw gory connective tissue between split ribs
            a.tube(f'IntercostalGore_{i}_{sign}', [
                (sign * w * 0.85, -0.05, z, 0.012),
                (sign * w * 0.65, -0.15, z - 0.015, 0.010)
            ], 'Gore', 'spine', sides=5)

    # Torn sternum split down the chest
    a.tube('SternumSplitGore', [(0, -0.165, 1.30, 0.018), (0, -0.175, 1.55, 0.022), (0, -0.16, 1.78, 0.016)], 'Gore', 'spine')
    a.stitches('ChestStaples', (0, -0.17, 1.32), (0, -0.17, 1.76), 'Iron', count=9, width=0.05)

    # Elongated gaunt cranial skull
    a.loft('ContortedSkull', [
        (0, -.03, 1.98, .10, .11), (0, -.035, 2.12, .18, .17),
        (0, -.04, 2.36, .21, .20), (0, -.01, 2.56, .17, .16), (0, .02, 2.66, .06, .08)
    ], 'Bone', 'head')

    # Gaping unhinged rictus maw with needle-sharp bone teeth and weeping jaw tendons
    a.horror_maw('BlindMaw', (0, -0.19, 2.14), width=0.20, height=0.10, depth=0.16, teeth_count=18,
                 bone='head', gum_mat='Gore', tooth_mat='Bone', gaping=0.13)

    # Weeping empty orbital hollows (eyeless, bleeding dark bile)
    for sign in [-1, 1]:
        a.ellipsoid(f'SunkenEyeChasm_{sign}', (sign * 0.065, -0.21, 2.32), (0.055, 0.035, 0.06), 'Charcoal', 'head')
        a.tube(f'EyeChasmTear_{sign}', [
            (sign * 0.065, -0.215, 2.29, 0.008),
            (sign * 0.075, -0.21, 2.22, 0.006),
            (sign * 0.07, -0.20, 2.15, 0.004)
        ], 'Gore', 'head', sides=5)

    # Large raw acoustic listening chasms (fleshy vibrating ear membranes)
    for sign in [-1, 1]:
        a.ellipsoid(f'AuditoryChasm_{sign}', (sign * 0.23, -0.01, 2.36), (0.16, 0.16, 0.44), 'Charcoal', 'head')
        a.tube(f'TornAuditoryRim_{sign}', [
            (sign * 0.19, -0.02, 2.14, 0.028), (sign * 0.32, -0.03, 2.26, 0.032),
            (sign * 0.34, 0.01, 2.48, 0.038), (sign * 0.26, 0.03, 2.60, 0.024),
            (sign * 0.21, -0.02, 2.46, 0.018)
        ], 'Ash', 'head', sides=7)
        # Twitching acoustic tendrils inside the ear chasm
        for j in range(4):
            z = 2.26 + j * 0.07
            a.tube(f'AcousticTendril_{sign}_{j}', [
                (sign * 0.22, -0.07, z, 0.014),
                (sign * 0.30, -0.08, z + 0.02, 0.016),
                (sign * 0.26, -0.03, z + 0.05, 0.008)
            ], 'Gore', 'head', sides=5)

    # Wet, stringy, matted horror hair hanging in tangled clumps over the skull, temples, and neck
    a.hair_curtain('BlindHairCrown', (0, 0.03, 2.64), count=16, length=0.48, material='Hair',
                   bone='head', spread_x=0.20, spread_y=0.18, z_drop=0.08, forward_bias=-0.04, wave_amp=0.035)
    a.hair_curtain('BlindHairTemples', (0, -0.06, 2.48), count=10, length=0.36, material='Hair',
                   bone='head', spread_x=0.22, spread_y=0.12, z_drop=0.06, forward_bias=-0.08, wave_amp=0.025)

    # Elongated neck tendons
    for sign in [-1, 1]:
        a.tube(f'StretchedNeckTendon_{sign}', [
            (sign * 0.13, -0.07, 1.83, 0.028),
            (sign * 0.09, -0.09, 2.01, 0.024),
            (sign * 0.10, -0.12, 2.16, 0.016)
        ], 'Gore', 'neck')

    # Spindly, emaciated limbs with defined joints and terrifying talon claws
    a.anatomy('Bone', .13, .145, fingers=0)
    for sign, side in [(-1, 'L'), (1, 'R')]:
        hand_pos = a.bones['forearm' + side][1]
        a.horror_hand('forearm' + side, hand_pos, arm_width=0.12, fingers=5, finger_len=0.32,
                      claw_len=0.11, material='Bone', claw_mat='Charcoal', sign=sign)
        # Twisted digitigrade heel and clawed skeletal feet
        foot_pos = a.bones['shin' + side][1]
        a.tube(f'AchillesTendon_{side}', [
            (foot_pos.x, foot_pos.y + 0.08, foot_pos.z + 0.22, 0.022),
            (foot_pos.x, foot_pos.y + 0.09, foot_pos.z + 0.04, 0.026),
            (foot_pos.x, foot_pos.y + 0.02, foot_pos.z, 0.018)
        ], 'Gore', 'shin' + side)
        for toe in range(4):
            tx = foot_pos.x + (toe - 1.5) * 0.045
            a.tube(f'ToeClaw_{side}_{toe}', [
                (tx, foot_pos.y - 0.08, foot_pos.z + 0.05, 0.022),
                (tx, foot_pos.y - 0.22, foot_pos.z + 0.02, 0.016),
                (tx, foot_pos.y - 0.30, foot_pos.z - 0.02, 0.005)
            ], 'Charcoal', 'shin' + side, sides=5)


def stalker(a):
    """The Stalker: a towering, gaunt phantom wraith. Deep within its tattered shroud rests an
    emaciated corpse face with piercing glowing amber eyes that flash in flashlights, long wet black hair,
    a twisted grinning maw, and razor-sharp spider talons."""
    # Towering emaciated frame under a shredded funeral coat
    a.loft('NarrowDecayedCoat', [
        (0, .015, .60, .28, .20), (0, 0, .88, .25, .18),
        (0, 0, 1.40, .20, .15), (0, 0, 1.93, .22, .16), (0, .025, 2.20, .26, .18),
        (0, 0, 2.35, .14, .115)
    ], 'Charcoal')
    
    # Shredded, tattered coat tails fluttering in the darkness
    for sign, side in [(-1, 'L'), (1, 'R')]:
        a.cloth(f'TatteredCoatTail_{side}', [
            (1.50, sign * .15, .16, .25), (1.08, sign * .18, .19, .28),
            (.69, sign * .21, .23, .28), (.35 if sign < 0 else .52, sign * .24, .24, .25)
        ], 'Charcoal', folds=.038)
        a.cloth(f'DecayedPlacket_{side}', [
            (2.20, sign * .09, -.16, .18), (1.68, sign * .11, -.17, .20),
            (1.10, sign * .13, -.19, .19), (.73, sign * .16, -.20, .16)
        ], 'Canvas', folds=.018)
        # Jagged protruding spinal vertebrae bulging through torn coat seams
        a.spinal_spikes(1.10, 2.15, 0.17, count=7, spike_len=0.08, material='Bone', bone='spine', gore_mat='Charcoal')

    # Deep frayed funeral hood structure
    a.loft('DecayedHoodCrown', [
        (0, .04, 2.43, .16, .14), (0, .06, 2.68, .20, .18),
        (.015, .08, 2.94, .17, .15), (.025, .09, 3.14, .04, .05)
    ], 'Charcoal', 'head')
    a.tube('RaggedHoodRim', [
        (-.11, -.13, 2.45, .026), (-.20, -.15, 2.66, .036),
        (-.17, -.16, 2.92, .032), (0, -.14, 3.06, .038), (.17, -.16, 2.92, .032),
        (.19, -.15, 2.68, .030), (.11, -.14, 2.46, .022)
    ], 'Canvas', 'head')

    # INSIDE THE HOOD: A terrifying, emaciated corpse face (no longer a dark empty box!)
    a.ellipsoid('CorpseHeadBase', (0, -0.06, 2.72), (0.24, 0.20, 0.32), 'Ash', 'head')
    a.loft('GauntCorpseFace', [
        (0, -0.08, 2.54, 0.10, 0.11),
        (0, -0.12, 2.66, 0.16, 0.13),
        (0, -0.13, 2.76, 0.17, 0.14),
        (0, -0.09, 2.88, 0.13, 0.12)
    ], 'Bone', 'head')

    # Piercing, glowing retroreflective predator eyes that flash brightly when hit by flashlights!
    a.horror_eyes('StalkerEyes', (0, -0.155, 2.76), eye_dist=0.10, radius=0.030, bone='head',
                 eye_mat='Eyes', socket_mat='Charcoal', gore_mat='Gore')

    # Uncanny, horrifying stitched rictus grin
    a.horror_maw('StalkerMaw', (0, -0.14, 2.60), width=0.15, height=0.06, depth=0.10,
                 teeth_count=12, bone='head', gum_mat='Gore', tooth_mat='Bone', gaping=0.06)

    # Long, wet, stringy black horror hair spilling out of the hood and clinging to the face and shoulders
    a.hair_curtain('StalkerHoodHair', (0, 0.02, 3.02), count=18, length=0.62, material='Hair',
                   bone='head', spread_x=0.22, spread_y=0.18, z_drop=0.10, forward_bias=-0.12, wave_amp=0.038)
    a.hair_curtain('StalkerFaceTendrils', (0, -0.10, 2.84), count=10, length=0.45, material='Hair',
                   bone='head', spread_x=0.16, spread_y=0.08, z_drop=0.04, forward_bias=-0.06, wave_amp=0.028)

    # Spidery, elongated multi-jointed arms and fingers
    a.anatomy('Charcoal', .09, .105, fingers=0)
    for sign, side in [(-1, 'L'), (1, 'R')]:
        hand_pos = a.bones['forearm' + side][1]
        a.horror_hand('forearm' + side, hand_pos, arm_width=0.09, fingers=5, finger_len=0.34,
                      claw_len=0.12, material='Bone', claw_mat='Iron', sign=sign)


def collector(a):
    """The Collector: a wretched, mutated carrion hunchback. A bulging hump fused with grotesque grafted faces,
    a hammered iron executioner mask bolted into rotting flesh, broken teeth, matted hair tufts, and butcher claws."""
    # Grotesque hunched torso
    a.loft('TwistedHunchbackTorso', [
        (0, .03, .39, .26, .24), (0, .10, .60, .36, .32),
        (0, .15, .90, .40, .36), (0, .10, 1.13, .28, .24), (0, -.07, 1.21, .15, .13)
    ], 'Canvas')

    # Bulging pulsating hump fused with grafted horror flesh and necrotic growth
    a.loft('MutatedFleshHump', [
        (0, .44, .47, .28, .22), (.025, .49, .64, .46, .34),
        (0, .50, .99, .48, .36), (-.035, .43, 1.25, .32, .26), (0, .40, 1.35, .14, .14)
    ], 'Gore')

    # Grafted faces and agonizing human masks trapped inside the pulsating flesh hump
    for f in range(3):
        fy = 0.72 + (f % 2) * 0.12
        fz = 0.65 + f * 0.22
        fx = (f - 1) * 0.22
        a.ellipsoid(f'GraftedSkull_{f}', (fx, fy, fz), (0.17, 0.14, 0.20), 'Bone')
        a.ellipsoid(f'GraftedMouth_{f}', (fx, fy + 0.05, fz - 0.05), (0.10, 0.04, 0.06), 'Charcoal')
        a.hair_curtain(f'HumpHair_{f}', (fx, fy - 0.02, fz + 0.10), count=6, length=0.32, material='Hair',
                       bone='spine', spread_x=0.08, spread_y=0.06, forward_bias=0.08)

    # Rusted barbed wire and butcher ropes strapping the horror mass down
    for sign in [-1, 1]:
        a.tube(f'BarbedWireHarness_{sign}', [
            (sign * .25, .60, 1.22, .026), (sign * .30, .08, 1.12, .028),
            (sign * .26, -.26, .92, .024), (sign * .23, -.28, .57, .024), (sign * .31, .46, .51, .022)
        ], 'Iron')
        for i in range(4):
            z = 0.58 + i * 0.19
            a.line(f'WireSpur_{sign}_{i}', (sign * 0.32, 0.50, z), (sign * 0.42, 0.65, z + 0.04), 0.008, 'Rust')

    # Head and Face: Rusted hammered executioner plate bolted into decaying skin
    a.ellipsoid('NecroticSkull', (0, -.29, 1.26), (.44, .42, .40), 'Gore', 'head')
    a.plate('HammeredIronMask', (0, -.51, 1.28), .38, .34, .07, 'Iron', 'head')
    
    # Horrific snarling mouth with broken human teeth protruding under the mask
    a.horror_maw('CollectorMaw', (0, -0.49, 1.18), width=0.18, height=0.08, depth=0.12,
                 teeth_count=14, bone='head', gum_mat='Gore', tooth_mat='Bone', gaping=0.09)

    # Bulging bloodshot cataract eye peering through a jagged cracked visor
    a.horror_eyes('CollectorVisorEye', (0.05, -0.525, 1.30), eye_dist=0.01, radius=0.034, bone='head',
                  eye_mat='Eyes', socket_mat='Charcoal', gore_mat='Gore')

    # Heavy iron rivets and rusted surgical staples pinning the mask to the face
    for sign in [-1, 1]:
        for z in [1.17, 1.38]:
            a.ellipsoid(f'MaskBoneBolt_{sign}_{z}', (sign * .16, -.52, z), (.025, .018, .025), 'Iron', 'head')
            a.line(f'Staple_{sign}_{z}', (sign * 0.16, -0.52, z), (sign * 0.22, -0.42, z), 0.009, 'Rust', 'head')

    # Greasy coarse clumps of hair sprouting around the scalp and rusted mask rim
    a.hair_curtain('CollectorScalpHair', (0, -0.22, 1.44), count=14, length=0.42, material='Hair',
                   bone='head', spread_x=0.24, spread_y=0.18, z_drop=0.08, forward_bias=-0.06, wave_amp=0.03)

    # Heavy scarred butcher arms with clawed hands
    a.anatomy('Gore', .15, .17, fingers=0)
    for sign, side in [(-1, 'L'), (1, 'R')]:
        hand_pos = a.bones['forearm' + side][1]
        a.horror_hand('forearm' + side, hand_pos, arm_width=0.14, fingers=4, finger_len=0.28,
                      claw_len=0.09, material='Bone', claw_mat='Rust', sign=sign)


def hunter(a):
    """The Hunter: a terrifying monolithic juggernaut of stitched muscle and hammered iron.
    Asymmetrical mutated musculature, iron rebar driven through bone, cracked skull with bolted trauma plates,
    coarse bloody hair, and a gnashing maw of blunt iron and bone teeth."""
    # Massive load-bearing mutated torso
    a.loft('BrutalMutatedTorso', [
        (0, .015, .94, .38, .30), (0, .01, 1.23, .52, .40),
        (0, .02, 1.69, .68, .46), (0, .02, 2.02, .70, .42), (0, .02, 2.18, .44, .30)
    ], 'Ash')

    # Massive protruding dorsal bone spikes down the back
    a.spinal_spikes(1.02, 2.18, 0.26, count=8, spike_len=0.16, material='Bone', bone='spine', gore_mat='Gore')

    # Heavy industrial iron reinforcement brackets and chains bolted into the ribcage
    for j in range(3):
        z = 1.24 + j * 0.22
        a.plate(f'HeavyChestPlating_{j}', (0, -0.44 - j * 0.01, z), 0.72, 0.18, 0.08, 'Iron')
        a.stitches(f'IronPlateBolts_{j}', (-0.32, -0.45, z), (0.32, -0.45, z), 'Rust', count=7, width=0.03)
        # Raw torn flesh bulging between the armor plates
        a.tube(f'BulgingMuscleTear_{j}', [(-0.35, -0.40, z - 0.08, 0.026), (0, -0.46, z - 0.08, 0.034), (0.35, -0.40, z - 0.08, 0.026)], 'Gore')

    # Battering skull with iron trauma plates hammered directly into bone
    a.loft('ShatteredBatteringHead', [
        (0, -.14, 2.16, .18, .19), (0, -.17, 2.37, .25, .24),
        (0, -.13, 2.62, .24, .22), (0, -.08, 2.75, .12, .13)
    ], 'Bone', 'head')

    # Iron facial trauma mask with exposed torn cheek and screaming maw
    a.plate('IronTraumaMask', (-0.08, -0.38, 2.45), 0.26, 0.36, 0.08, 'Iron', 'head', tilt=-0.12)
    # The right cheek is torn open, revealing raw jaw muscle and needle-sharp teeth
    a.horror_maw('HunterGoreMaw', (0.02, -0.39, 2.32), width=0.22, height=0.09, depth=0.14,
                 teeth_count=16, bone='head', gum_mat='Gore', tooth_mat='Bone', gaping=0.11)

    # Glaring bloodshot retroreflective predator eye flashing in the flashlight beam
    a.horror_eyes('HunterPredatorEye', (0.07, -0.41, 2.50), eye_dist=0.01, radius=0.038, bone='head',
                  eye_mat='Eyes', socket_mat='Charcoal', gore_mat='Gore')

    # Coarse, bloody hair tufts sprouting between fractured iron plates and scalp
    a.hair_curtain('HunterScalpHair', (0, -0.06, 2.78), count=14, length=0.38, material='Hair',
                   bone='head', spread_x=0.24, spread_y=0.18, z_drop=0.06, forward_bias=-0.05, wave_amp=0.022)

    # Massive asymmetrical arms: left arm swollen with mutated muscle, right arm armored
    a.anatomy('Ash', .38, .36, fingers=0)
    for sign, side in [(-1, 'L'), (1, 'R')]:
        hand_pos = a.bones['forearm' + side][1]
        a.horror_hand('forearm' + side, hand_pos, arm_width=0.36, fingers=4, finger_len=0.32,
                      claw_len=0.14, material='Bone', claw_mat='Iron', sign=sign)
        # Sledgehammer iron fist plating and knuckle spikes
        for k in range(3):
            a.tube(f'KnuckleSpike_{side}_{k}', [
                (hand_pos.x + (k - 1) * 0.12, hand_pos.y - 0.18, hand_pos.z - 0.06, 0.035),
                (hand_pos.x + (k - 1) * 0.13, hand_pos.y - 0.32, hand_pos.z - 0.16, 0.008)
            ], 'Iron', 'forearm' + side, sides=6)
