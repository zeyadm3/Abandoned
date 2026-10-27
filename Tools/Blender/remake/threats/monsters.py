"""The eight remade monsters: sculpted from node graphs, detailed with procedural fields, baked onto game meshes,
skinned to the threat rig and exported with their five takes.

Batch:
  Blender --background --factory-startup --python Tools/Blender/remake/threats/monsters.py -- [names...] [--preview]
Writes Art/Remake/Threats/<Name>.fbx and Textures/Threat_<Name>_*.png. Identities and proportions follow the
2026-10-08 threats (Tools/Blender/threats.py MODELS): the same heights, widths, hip heights and arm lengths, so
NavMesh agents, reach and attack timing keep fitting the monsters.
"""
import math
import os
import random
import sys

import bpy
from mathutils import Vector

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(os.path.dirname(_HERE)))
exec(open(os.path.join(os.path.dirname(_HERE), 'boot.py')).read())

from remake.core import bake, export, kit, materials, preview, sculpt  # noqa: E402
from remake.threats import anatomy, takes  # noqa: E402

OUT = export.art_dir('Threats')
TEX = export.art_dir('Threats', 'Textures')
S = materials.surface

# name: (height, width, hip height, arm length, body y) as Tools/Blender/threats.py, plus LOD0 budget and voxel size.
PROPS = {
    'BlindOne': ((2.65, .39, 1.02, 1.40, 0), 30000, .011),
    'Stalker': ((3.1, .26, .75, 1.92, 0), 30000, .011),
    'Collector': ((1.5, .36, .43, .85, -.04), 30000, .009),
    'Hunter': ((2.75, .70, .92, 1.38, 0), 40000, .016),
    'Weight': ((4.9, 1.12, 2.9, 2.25, .25), 50000, .024),
    'Thing': ((2.35, .37, .94, 1.25, 0), 35000, .011),
    'Crawlers': ((.88, .40, .25, .58, .22), 25000, .007),
    'LastHunter': ((3.9, .90, 1.12, 2.68, 0), 50000, .02),
}


class Build:
    """What a design returns: the organic body, extra organic blobs to union, hard parts, their weight points."""

    def __init__(self, body, skin):
        self.body, self.skin = body, skin
        self.blobs, self.hard, self.points, self.fields = [], [], [], []

    def hard_part(self, ob, mat, bone):
        materials.assign(ob, mat)
        lo, hi = kit.bounds([ob])
        self.points.append(((lo + hi) / 2, (hi - lo).length * .3, bone))
        self.hard.append(ob)
        return ob


# ------------------------------------------------------------------------------------------------- materials

_CREW = os.path.join(os.path.dirname(os.path.dirname(_HERE)), 'crew')
if _CREW not in sys.path:
    sys.path.insert(0, _CREW)
import crew_materials as CM  # noqa: E402


def flesh(name, tone, mottle, vein=(.18, .1, .16), dirt=(.09, .07, .05), wet=.25, rough=.5, low=.6):
    """Realistic wrong skin: blotchy mottling, a web of veins, bruising, crevice grime, filth rising from the feet,
    a wet sheen in the folds."""
    key = 'RM_' + name
    if key in bpy.data.materials:
        return bpy.data.materials[key]
    g = CM.Graph(key)
    base = CM.srgb(tone)
    blot = g.ramp(g.noise(3.0, 6.0, .65).outputs['Fac'], .35, .7)
    c = g.mix(base, CM.srgb(mottle), g.math('MULTIPLY', blot, .6))
    spots = g.ramp(g.noise(14.0, 4.0, .7).outputs['Fac'], .6, .72)
    c = g.mix(c, CM.srgb(tuple(x * .6 for x in mottle)), g.math('MULTIPLY', spots, .5))
    vor = g.node('ShaderNodeTexVoronoi', feature='DISTANCE_TO_EDGE', in_Scale=9.0)
    g.link(g.coords(), 'Object', vor, 'Vector')
    veins = g.ramp(vor.outputs['Distance'], .0, .035, (1, 1, 1, 1), (0, 0, 0, 1))
    vmask = g.ramp(g.noise(2.0, 2.0).outputs['Fac'], .45, .6)
    c = g.mix(c, CM.srgb(vein), g.math('MULTIPLY', g.math('MULTIPLY', veins, vmask), .55))
    c = g.grime(c, .8, low, (*CM.srgb(dirt)[:3], 1))
    g.set('Base Color', c)
    g.set('Roughness', g.math('SUBTRACT', rough, g.math('MULTIPLY', g.ramp(g.noise(6.0).outputs['Fac'], .4, .7), wet)))
    g.set('Subsurface Weight', .12)
    g.set('Subsurface Radius', (1.0, .35, .25))
    g.set('Subsurface Scale', .01)
    pores = g.node('ShaderNodeTexVoronoi', in_Scale=600.0)
    g.link(g.coords(), 'Object', pores, 'Vector')
    g.set('Normal', g.bump(g.math('ADD', g.math('MULTIPLY', pores.outputs['Distance'], .5), g.math('MULTIPLY', veins, -.4)),
                           .3, .001, g.bump(g.noise(40.0, 6.0).outputs['Fac'], .2, .004)))
    return g.mat


def stone(name, tone=(.46, .45, .42)):
    """Poured concrete gone wrong: water stains running down, rust bleeding from rebar, moss at the base, dark cracks."""
    key = 'RM_' + name
    if key in bpy.data.materials:
        return bpy.data.materials[key]
    g = CM.Graph(key)
    base = CM.srgb(tone)
    var = g.ramp(g.noise(2.5, 5.0, .6).outputs['Fac'], .3, .75)
    c = g.mix(base, CM.srgb(tuple(x * .7 for x in tone)), var)
    streak = g.node('ShaderNodeTexWave', wave_type='BANDS', bands_direction='X', in_Scale=6.0, in_Distortion=12.0, in_Detail=4.0)
    g.link(g.coords(), 'Object', streak, 'Vector')
    c = g.mix(c, CM.srgb((.2, .19, .17)), g.math('MULTIPLY', g.ramp(streak.outputs['Fac'], .55, .9), .45))
    rust = g.ramp(g.noise(5.0, 6.0, .7).outputs['Fac'], .62, .75)
    c = g.mix(c, CM.srgb((.4, .18, .07)), g.math('MULTIPLY', rust, .7))
    cracks = g.node('ShaderNodeTexVoronoi', feature='DISTANCE_TO_EDGE', in_Scale=3.5)
    g.link(g.coords(), 'Object', cracks, 'Vector')
    crack = g.ramp(cracks.outputs['Distance'], .0, .02, (1, 1, 1, 1), (0, 0, 0, 1))
    c = g.mix(c, (.02, .02, .02, 1), crack)
    c = g.grime(c, .7, .9, (*CM.srgb((.16, .19, .09))[:3], 1))
    g.set('Base Color', c)
    g.set('Roughness', .9)
    g.set('Normal', g.bump(g.math('ADD', g.noise(60.0, 6.0).outputs['Fac'], g.math('MULTIPLY', crack, -1.5)), .6, .01))
    return g.mat


def pale():
    return flesh('ThreatPale', (.68, .64, .6), (.55, .45, .48), low=1.2)


def dead_grey():
    return flesh('ThreatDeadGrey', (.42, .43, .41), (.3, .3, .33), (.12, .1, .15), low=.9)


def raw():
    return flesh('ThreatRaw', (.5, .2, .17), (.32, .08, .07), (.15, .03, .05), wet=.35, rough=.42)


def concrete():
    return stone('ThreatConcrete')


def coat_cloth():
    return materials.fabric('ThreatCoat', (.055, .05, .046), .93, 260, grime=.5)


def burlap():
    return S('ThreatSack', (.34, .28, .19), .95, 0, var=.2, bump=.5, bump_scale=400, grime=.55)


# --------------------------------------------------------------------------------------------------- designs

def blind_one(p):
    h, w, leg, arm, y = p
    body, m = anatomy.humanoid(h, w, leg, arm, y, torso_r=.135, hip_r=.105, neck_r=.048, head=(.1, .125, .17),
                               upper_arm=.042, fore_arm=.033, thigh=.058, shin=.043, hand=.042, fingers=4,
                               finger_len=.22, finger_r=.014, hunch=.2)
    b = Build(body, pale())
    skull = body.nodes[m['skull']][0]
    # Huge listening membranes where ears should be, and a ridge of vertebrae.
    for s in (-1, 1):
        b.blobs.append(anatomy.blob(f'Ear{s}', skull + Vector((s * .11, .02, .0)), (.018, .07, .11)))
    for k in range(9):
        z = leg + .15 + k * (h * .76 - leg - .2) / 8
        b.blobs.append(anatomy.blob(f'Vert{k}', (0, y + .1 + .02 * math.sin(k), z), (.025, .03, .022)))
    chest = body.nodes[m['chest']][0]
    # Ribs pressing through (ridges around the chest), sunken belly, no eyes, a lipless slit.
    fields = []
    for k in range(7):
        z = chest.z - .05 - k * .055
        for s in (-1, 1):
            fields.append(sculpt.ridge((s * .02, chest.y - .12, z + .02), (s * .14, chest.y - .02, z - .03), .016, .007))
    fields.append(sculpt.blob((0, chest.y - .14, chest.z - .45), .12, -.025))
    fields.append(sculpt.ridge((-.035, skull.y - .12, skull.z - .07), (.035, skull.y - .12, skull.z - .07), .006, -.008))
    for s in (-1, 1):
        fields.append(sculpt.blob((s * .04, skull.y - .115, skull.z + .01), .03, -.012))
    b.fields = fields + [sculpt.noise_field(30, .0025, 4), sculpt.noise_field(140, .0008, 3)]
    return b


def stalker(p):
    h, w, leg, arm, y = p
    body, m = anatomy.humanoid(h, w, leg, arm, y, torso_r=.1, hip_r=.085, neck_r=.04, head=(.085, .1, .12),
                               upper_arm=.034, fore_arm=.028, thigh=.048, shin=.038, hand=.04, fingers=4,
                               finger_len=.25, finger_r=.013, hunch=.32)
    b = Build(body, dead_grey())
    sh_z = h * .76
    # The coat hangs from the shoulders as if from a hanger: open down the front, arms free of it, the hem in rags.
    rings = []
    for k, (z, rx, ry) in enumerate(((sh_z + .05, .15, .12), (sh_z - .04, .24, .17), (sh_z - .5, .25, .19),
                                     (sh_z - 1.1, .29, .23), (sh_z - 1.6, .33, .26), (.42, .38, .29))):
        rings.append(kit.ellipse_ring((0, y - .3 * (1 - k / 6), z), rx, ry, 28))
    coat = kit.loft('Coat', rings, cap=False)
    import bmesh
    bmc = bmesh.new()
    bmc.from_mesh(coat.data)
    front = [f for f in bmc.faces if f.calc_center_median().y < y - .3 * .5 - .12 and abs(f.calc_center_median().x) < .07]
    bmesh.ops.delete(bmc, geom=front, context='FACES')
    bmc.to_mesh(coat.data)
    bmc.free()
    kit.solidify(coat, .012)
    rng = random.Random(4)
    for v in coat.data.vertices:
        if v.co.z < .8:
            v.co.z += rng.uniform(0, .22) * (.8 - v.co.z) / .4
    hat_c = S('ThreatFelt', (.04, .035, .03), .85, 0, var=.1, bump=.2, bump_scale=200)
    skull = body.nodes[m['skull']][0]
    hat = kit.lathe('Hat', [(.0, .0), (.25, .0), (.26, .012), (.11, .015), (.1, .13), (.0, .14)], sides=40)
    hat.location = skull + Vector((0, -.02, .07))
    b.hard_part(coat, coat_cloth(), 'spine')
    b.hard_part(hat, hat_c, 'head')
    collar = kit.loft('Collar', [kit.ellipse_ring((0, y - .3, sh_z + z), .17 + z, .14 + z * .6, 24) for z in (.05, .2)], cap=False)
    kit.solidify(collar, .01)
    b.hard_part(collar, coat_cloth(), 'neck')
    eyes = [kit.sphere(f'Eye{s}', .012, skull + Vector((s * .035, -.1, -.01))) for s in (-1, 1)]
    for e in eyes:
        b.hard_part(e, materials.library()['EmitWarm'], 'head')
    b.fields = [sculpt.noise_field(25, .002, 4), sculpt.noise_field(120, .0008, 3)]
    return b


def collector(p):
    h, w, leg, arm, y = p
    body, m = anatomy.humanoid(h, w, leg, arm, y, torso_r=.17, hip_r=.13, neck_r=.065, head=(.095, .1, .11),
                               upper_arm=.055, fore_arm=.045, thigh=.072, shin=.055, hand=.06, fingers=4,
                               finger_len=.11, finger_r=.014, hunch=.3)
    b = Build(body, raw())
    # The sack of everything it has taken, lashed to its back, things poking out.
    sack = anatomy.blob('Sack', (0, y + .36, h * .6), (.34, .3, .42))
    sm = sack.modifiers.new('Lumps', 'DISPLACE')
    tex = bpy.data.textures.new('SackLumps', 'CLOUDS')
    tex.noise_scale = .12
    sm.texture, sm.strength = tex, .06
    kit.apply(sack)
    b.hard_part(sack, burlap(), 'spine')
    L = materials.library()
    junk = [(kit.rod('Pipe', (.12, y + .3, h * .75), (.25, y + .45, h * 1.05), .025), 'RustedSteel'),
            (kit.rod('Pipe2', (-.1, y + .35, h * .8), (-.3, y + .3, h * 1.0), .018), 'Copper'),
            (kit.cylinder('Clock', .08, .04, (-.18, y + .58, h * .62), sides=24, axis='Y'), 'PaintWhite'),
            (kit.sphere('DollHead', .06, (.15, y + .5, h * .9)), 'PlasticBeige'),
            (kit.lathe('Wheel', [(.17, -.012), (.19, -.012), (.19, .012), (.17, .012)], sides=36), 'SteelDark')]
    wheel = junk[-1][0]
    wheel.rotation_euler = (math.radians(80), 0, math.radians(20))
    wheel.location = (0, y + .62, h * .72)
    for ob, mt in junk:
        b.hard_part(ob, L[mt], 'spine')
    # A hubcap for a face, two holes punched in it.
    skull = body.nodes[m['skull']][0]
    cap = kit.cylinder('Hubcap', .1, .025, skull + Vector((0, -.1, 0)), sides=32, axis='Y', bevel=.008)
    for s in (-1, 1):
        kit.boolean(cap, kit.cylinder(f'Hole{s}', .016, .1, skull + Vector((s * .035, -.1, .02)), sides=12, axis='Y'))
    b.hard_part(cap, L['Aluminium'], 'head')
    for s in (-1, 1):
        b.hard_part(kit.sphere(f'Eye{s}', .01, skull + Vector((s * .035, -.085, .02))), L['EmitWarm'], 'head')
    for k in range(3):
        b.hard_part(kit.cylinder(f'Rope{k}', .2 + k * .02, .02, (0, y + .2, h * (.45 + k * .12)), sides=24, cap=False),
                    S('Rope', (.4, .34, .22), .9, 0, bump=.4, bump_scale=300), 'spine')
    b.fields = [sculpt.noise_field(20, .004, 4), sculpt.noise_field(90, .0015, 3)]
    return b


def hunter(p, last=False):
    h, w, leg, arm, y = p
    g = h / 2.75
    body, m = anatomy.humanoid(h, w, leg, arm, y, torso_r=.3 * g, hip_r=.22 * g, neck_r=.13 * g, head=(.12 * g, .13 * g, .13 * g),
                               upper_arm=.11 * g, fore_arm=.12 * g, thigh=.14 * g, shin=.12 * g, hand=.1 * g, fingers=3,
                               finger_len=.13 * g, finger_r=.04 * g, hunch=.12 * g, foot_len=.3 * g)
    b = Build(body, concrete())
    L = materials.library()
    rng = random.Random(7 if last else 3)
    sh_z = h * .76
    # Rebar torn through the back and shoulders.
    for k in range(10 if last else 6):
        base = Vector((rng.uniform(-w * .7, w * .7), y + .25 * g, rng.uniform(leg + .3, sh_z + .05)))
        tip = base + Vector((rng.uniform(-.15, .15), rng.uniform(.25, .5), rng.uniform(.05, .3))) * g
        b.hard_part(kit.rod(f'Rebar{k}', base, tip, .016 * g, 8), L['RustedSteel'], 'spine')
    for s, n in ((-1, 'L'), (1, 'R')):
        for k in range(3):
            z = sh_z - arm * (.65 + k * .08)
            ring = kit.lathe(f'Band{n}{k}', [(.155 * g, -.012), (.17 * g, -.012), (.17 * g, .012), (.155 * g, .012)], sides=20)
            ring.location = (s * (w + .06), y - .09, z)
            b.hard_part(ring, L['RustedSteel'], 'forearm' + n)
    fields = [sculpt.noise_field(3, .012 * g, 2, ridged=True), sculpt.noise_field(14, .004, 3)]
    if last:
        # Seams of something burning inside the stone.
        for k in range(8):
            a = Vector((rng.uniform(-w, w), y - .3 * g, rng.uniform(leg, sh_z)))
            c = a + Vector((rng.uniform(-.3, .3), 0, rng.uniform(-.4, .4))) * g
            seam = kit.tube_along(f'Seam{k}', [a, a.lerp(c, .5) + Vector((0, -.04, 0)), c], .012 * g, 6)
            b.hard_part(seam, L['EmitRed'], 'spine')
        skull = body.nodes[m['skull']][0]
        for s in (-1, 1):
            b.hard_part(kit.sphere(f'Eye{s}', .02 * g, skull + Vector((s * .05 * g, -.13 * g, 0))), L['EmitRed'], 'head')
    b.fields = fields
    return b


def weight(p):
    h, w, leg, arm, y = p
    body, m = anatomy.humanoid(h, w, leg, arm, y, torso_r=.7, hip_r=.6, neck_r=.18, head=(.16, .17, .17),
                               upper_arm=.2, fore_arm=.17, thigh=.34, shin=.28, hand=.16, fingers=3, finger_len=.18,
                               finger_r=.06, hunch=.3, foot_len=.4)
    b = Build(body, flesh('ThreatBloat', (.62, .52, .5), (.48, .32, .34), (.2, .1, .16), wet=.3))
    # The mass: a belly sagging to the knees, folds of it hanging off the back.
    b.blobs.append(anatomy.blob('Belly', (0, y - .35, leg + .3), (.95, .8, 1.0)))
    b.blobs.append(anatomy.blob('BackFold', (0, y + .5, leg + .9), (.8, .5, .6)))
    b.points.append(((0, y - .35, leg + .3), 1.0, 'spine'))
    b.points.append(((0, y + .5, leg + .9), .6, 'spine'))
    b.fields = [sculpt.wrinkles((0, 0, 1), 3, .006), sculpt.noise_field(3, .05, 3), sculpt.noise_field(20, .006, 3)]
    return b


def thing(p):
    h, w, leg, arm, y = p
    body, m = anatomy.humanoid(h, w, leg, arm, y, torso_r=.13, hip_r=.11, neck_r=.06, head=(.1, .11, .12),
                               upper_arm=.045, fore_arm=.038, thigh=.065, shin=.05, hand=.05, fingers=5,
                               finger_len=.17, finger_r=.01, hunch=-.12, splay=.12)
    b = Build(body, flesh('ThreatThing', (.34, .24, .22), (.2, .1, .1), (.1, .02, .04), wet=.35))
    sh_z = h * .76
    # A second pair of arms folded up out of the shoulder blades.
    for s in (-1, 1):
        pts = [(s * .1, y + .1, sh_z - .1), (s * .25, y + .25, sh_z + .25), (s * .3, y + .15, sh_z + .55), (s * .32, y - .05, sh_z + .4)]
        body.chain(pts, [.04, .03, .022, .012], m['chest'], 'spine')
    skull = body.nodes[m['skull']][0]
    # The face is a vertical maw ringed with teeth.
    L = materials.library()
    for k in range(10):
        z = skull.z - .09 + k * .02
        for s in (-1, 1):
            tooth = kit.cone(f'Tooth{k}{s}', .007, .0005, .035, (s * .018, skull.y - .115, z), sides=6)
            tooth.rotation_euler = (math.radians(90), 0, math.radians(-s * 70))
            b.hard_part(tooth, L['PaintCream'], 'head')
    b.fields = [sculpt.ridge((0, skull.y - .12, skull.z - .11), (0, skull.y - .12, skull.z + .09), .018, -.03),
                sculpt.wrinkles((1, 0, .3), 22, .002), sculpt.noise_field(30, .003, 4)]
    return b


def crawlers(p):
    h, w, leg, arm, y = p
    b = anatomy.Body()
    sh = h * .76
    # Low, long body; the front legs are the arm bones, the back legs the leg bones.
    spine = b.chain([(0, y + .25, leg + .05), (0, y + .05, leg + .1), (0, y - .15, .4), (0, y - .3, .45)],
                    [.09, .12, .11, .08], bone='spine')
    head = b.chain([(0, y - .42, .48), (0, y - .52, .46)], [.075, .05], spine[-1], bone='head')
    for s, n in ((-1, 'L'), (1, 'R')):
        b.limb((s * .08, y - .2, .42), (s * (w + .05), y - .35, sh - .05), .035, .03, 2, start=spine[2], bone='arm' + n)
        b.limb((s * (w + .05), y - .35, sh - .05), (s * (w + .12), y - .5, .03), .028, .012, 3, start=len(b.nodes) - 1,
               bone='forearm' + n)
        b.limb((s * .06, y + .2, leg + .05), (s * (w * .9), y + .3, leg + .2), .04, .03, 2, start=spine[0], bone='leg' + n)
        b.limb((s * (w * .9), y + .3, leg + .2), (s * w, y + .42, .03), .028, .012, 3, start=len(b.nodes) - 1, bone='shin' + n)
        # Middle legs ride on the spine.
        b.limb((s * .08, y - .02, .38), (s * (w + .1), y - .02, .3), .03, .02, 2, start=spine[1], bone='spine')
        b.limb((s * (w + .1), y - .02, .3), (s * (w + .2), y, .02), .02, .01, 2, start=len(b.nodes) - 1, bone='spine')
    # Two smaller ones clinging to its back (move with the root's sway).
    for s in (-1, 1):
        c = b.chain([(s * .12, y + .15, leg + .2), (s * .14, y, leg + .22)], [.05, .04], bone='root')
        for k in (-1, 1):
            b.limb((s * .14, y + .07, leg + .2), (s * .14 + k * .1, y + .07, leg + .12), .015, .006, 2, start=c[0], bone='root')
    out = Build(b, flesh('ThreatChitin', (.16, .13, .1), (.08, .06, .05), (.25, .15, .08), wet=.4, rough=.35, low=.4))
    L = materials.library()
    hd = b.nodes[head[0]][0]
    for s in (-1, 1):
        for k in range(3):
            out.hard_part(kit.sphere(f'Eye{s}{k}', .009, hd + Vector((s * (.025 + k * .012), -.06, .02 + k * .01))), L['EmitRed'], 'head')
    out.fields = [sculpt.noise_field(40, .003, 4, ridged=True)]
    return out


DESIGNS = {'BlindOne': blind_one, 'Stalker': stalker, 'Collector': collector, 'Hunter': hunter, 'Weight': weight,
           'Thing': thing, 'Crawlers': crawlers, 'LastHunter': lambda p: hunter(p, last=True)}


# ------------------------------------------------------------------------------------------------------ build

def build(name, show=False):
    kit.reset()
    props, budget, voxel = PROPS[name]
    b = DESIGNS[name](props)
    high = b.body.build(name + '_High', voxel=voxel)
    if b.blobs:
        anatomy.union(high, b.blobs, voxel)
    if b.fields:
        sculpt.displace(high, sculpt.combine(*b.fields))
    materials.assign(high, b.skin)
    low = high.copy()
    low.data = high.data.copy()
    low.name = name + '_Body'
    kit.link(low)
    kit.decimate_to(low, int(budget * .8), delimit=('NORMAL',))
    hard_low = []
    for part in b.hard:
        c = part.copy()
        c.data = part.data.copy()
        kit.link(c)
        hard_low.append(c)
    mesh = kit.join([low] + hard_low, name + '_SkinnedMesh')
    bake.bake_asset(mesh, f'Threat_{name}', TEX, size=2048, high=[high] + b.hard, high_all=True, samples=12,
                    ray=.03 + props[0] * .012)
    for ob in [high] + b.hard:
        bpy.data.objects.remove(ob, do_unlink=True)
    rig_ob, _ = takes.rig(name, *props)
    anatomy.weight(mesh, b.body, rig_ob, falloff=max(.04, props[0] * .02), extra=b.points)
    actions = takes.animate(rig_ob, name)
    export.export_skinned(rig_ob, [mesh], os.path.join(OUT, name + '.fbx'), actions)
    if show:
        preview.save('Threat_' + name)
    print(f'[remake-threat] {name} tris={kit.tris(mesh)}')


def main():
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    show = '--preview' in args
    names = [a for a in args if not a.startswith('--')] or list(DESIGNS)
    for n in names:
        build(n, show)


if __name__ == '__main__':
    main()
