"""Procedural Cycles materials for the crew redesign previews. Later baked to BaseColor/Normal/Mask for Unity."""
import bpy

PALETTE = {
    'orange': (0.95, 0.45, 0.10), 'yellow': (0.95, 0.80, 0.15), 'navy': (0.15, 0.22, 0.42),
    'blue': (0.24, 0.55, 0.72), 'grey': (0.50, 0.50, 0.52), 'olive': (0.33, 0.36, 0.20),
    'teal': (0.10, 0.50, 0.52), 'white': (0.90, 0.90, 0.88), 'red': (0.70, 0.10, 0.08),
}


def srgb(c):
    """Catalog colours are authored in sRGB; Cycles wants linear."""
    def ch(x):
        return x / 12.92 if x <= 0.04045 else ((x + 0.055) / 1.055) ** 2.4
    return (ch(c[0]), ch(c[1]), ch(c[2]), 1.0)


class Graph:
    def __init__(self, name):
        self.mat = bpy.data.materials.new(name)
        self.mat.use_nodes = True
        self.nt = self.mat.node_tree
        self.nt.nodes.clear()
        self.out = self.node('ShaderNodeOutputMaterial')
        self.bsdf = self.node('ShaderNodeBsdfPrincipled')
        self.link(self.bsdf, 'BSDF', self.out, 'Surface')

    def node(self, kind, **props):
        n = self.nt.nodes.new(kind)
        for k, v in props.items():
            if k.startswith('in_'):
                n.inputs[k[3:].replace('_', ' ')].default_value = v
            else:
                setattr(n, k, v)
        return n

    def link(self, a, out, b, inp):
        self.nt.links.new(a.outputs[out], b.inputs[inp])

    def coords(self):
        return self.node('ShaderNodeTexCoord')

    def noise(self, scale, detail=4.0, rough=.55, coord='Object'):
        n = self.node('ShaderNodeTexNoise', in_Scale=scale, in_Detail=detail, in_Roughness=rough)
        self.link(self.coords(), coord, n, 'Vector')
        return n

    def mix(self, a, b, fac, blend='MIX'):
        m = self.node('ShaderNodeMix', data_type='RGBA', blend_type=blend)
        for sock, val in ((6, a), (7, b), (0, fac)):
            if isinstance(val, tuple):
                m.inputs[sock].default_value = val
            elif isinstance(val, float):
                m.inputs[sock].default_value = val
            else:
                self.nt.links.new(val, m.inputs[sock])
        return m.outputs[2]

    def ramp(self, src, a, b, ca=(0, 0, 0, 1), cb=(1, 1, 1, 1)):
        r = self.node('ShaderNodeValToRGB')
        r.color_ramp.elements[0].position, r.color_ramp.elements[0].color = a, ca
        r.color_ramp.elements[1].position, r.color_ramp.elements[1].color = b, cb
        self.nt.links.new(src, r.inputs[0])
        return r.outputs[0]

    def math(self, op, a, b=0.0):
        m = self.node('ShaderNodeMath', operation=op)
        for i, v in enumerate((a, b)):
            if isinstance(v, float):
                m.inputs[i].default_value = v
            else:
                self.nt.links.new(v, m.inputs[i])
        return m.outputs[0]

    def bump(self, height, strength, distance=.002, normal=None):
        b = self.node('ShaderNodeBump', in_Strength=strength, in_Distance=distance)
        self.nt.links.new(height, b.inputs['Height'])
        if normal is not None:
            self.nt.links.new(normal, b.inputs['Normal'])
        return b.outputs['Normal']

    def grime(self, colour, amount=.55, low=.45, dirt=(0.1, 0.078, 0.058, 1)):
        """Crevice occlusion plus mud rising from the floor (object Z is rest-pose height in metres)."""
        ao = self.node('ShaderNodeAmbientOcclusion', samples=8, in_Distance=.035)
        crev = self.ramp(ao.outputs['AO'], .35, 1.0, (1, 1, 1, 1), (0, 0, 0, 1))
        c = self.mix(colour, self.mix(colour, dirt, .75), self.math('MULTIPLY', crev, amount))
        sep = self.node('ShaderNodeSeparateXYZ')
        self.link(self.coords(), 'Object', sep, 'Vector')
        rise = self.math('MULTIPLY', self.ramp(sep.outputs['Z'], 0.0, low, (1, 1, 1, 1), (0, 0, 0, 1)), 1.0)
        blot = self.ramp(self.noise(9.0, 6.0, .6).outputs['Fac'], .38, .62)
        mud = self.math('MULTIPLY', rise, self.math('ADD', self.math('MULTIPLY', blot, .7), .25))
        return self.mix(c, dirt, self.math('MINIMUM', mud, .5))

    def edges(self, radius=.004):
        """1 on worn convex edges (bevel normal vs true normal)."""
        bev = self.node('ShaderNodeBevel', samples=6, in_Radius=radius)
        geo = self.node('ShaderNodeNewGeometry')
        dot = self.node('ShaderNodeVectorMath', operation='DOT_PRODUCT')
        self.link(bev, 'Normal', dot, 0)
        self.link(geo, 'Normal', dot, 1)
        return self.ramp(dot.outputs['Value'], .86, .99, (1, 1, 1, 1), (0, 0, 0, 1))

    def set(self, sock, val):
        if isinstance(val, (float, int, tuple)):
            self.bsdf.inputs[sock].default_value = val
        else:
            self.nt.links.new(val, self.bsdf.inputs[sock])


def fabric(name, colour, rough=.86, weave=900.0, low=.5, wrinkle=.25, kind='twill', sheen=.25, fade=.0):
    """Woven cloth: colour variation, crevice grime, mud from the floor, a weave normal and soft folds."""
    g = Graph(name)
    base = srgb(colour) if len(colour) == 3 else colour
    var = g.noise(3.5, 3.0, .5).outputs['Fac']
    tone = g.mix(base, g.mix(base, (0, 0, 0, 1), .22), g.ramp(var, .3, .75))
    if fade:
        # Sun and wash fading on raised areas: lighter, greyer patches.
        tone = g.mix(tone, (0.55, 0.53, 0.5, 1), g.math('MULTIPLY', g.ramp(g.noise(2.0, 4.0).outputs['Fac'], .5, .8), fade))
    g.set('Base Color', g.grime(tone, .6, low))
    g.set('Roughness', g.math('ADD', g.math('MULTIPLY', var, .08), rough))
    if kind == 'ripstop':
        a = g.node('ShaderNodeTexWave', wave_type='BANDS', bands_direction='X', in_Scale=weave * .08, in_Distortion=.2)
        b = g.node('ShaderNodeTexWave', wave_type='BANDS', bands_direction='Z', in_Scale=weave * .08, in_Distortion=.2)
        g.link(g.coords(), 'Object', a, 'Vector')
        g.link(g.coords(), 'Object', b, 'Vector')
        grid = g.math('MAXIMUM', g.ramp(a.outputs['Fac'], .9, 1.0), g.ramp(b.outputs['Fac'], .9, 1.0))
        fine = g.noise(weave * 1.4, 1.0).outputs['Fac']
        height = g.math('ADD', g.math('MULTIPLY', grid, .7), g.math('MULTIPLY', fine, .3))
    else:
        tw = g.node('ShaderNodeTexWave', wave_type='BANDS', bands_direction='DIAGONAL', in_Scale=weave, in_Distortion=1.5)
        g.link(g.coords(), 'Object', tw, 'Vector')
        height = tw.outputs['Fac']
    folds = g.noise(7.0, 3.0, .5).outputs['Fac']
    g.set('Normal', g.bump(height, .14, .0006, g.bump(folds, wrinkle, .02)))
    g.set('Sheen Weight', sheen)
    return g.mat


def tape(name='CrewTape', colour=(0.78, 0.78, 0.74)):
    """Retro-reflective strip: silvery, slightly glossy, dirty at the edges."""
    g = Graph(name)
    dirt = g.ramp(g.noise(14.0, 6.0, .65).outputs['Fac'], .45, .75)
    g.set('Base Color', g.grime(g.mix(srgb(colour), (0.35, 0.33, 0.3, 1), g.math('MULTIPLY', dirt, .5)), .5, .35))
    g.set('Roughness', .3)
    g.set('Metallic', .35)
    g.set('Coat Weight', .3)
    dots = g.node('ShaderNodeTexVoronoi', in_Scale=2600.0)
    g.link(g.coords(), 'Object', dots, 'Vector')
    g.set('Normal', g.bump(dots.outputs['Distance'], .15, .0002))
    return g.mat


def rubber(name='CrewRubber', colour=(0.06, 0.06, 0.06), rough=.5):
    g = Graph(name)
    worn = g.edges(.006)
    scuff = g.ramp(g.noise(20.0, 6.0, .7).outputs['Fac'], .5, .8)
    g.set('Base Color', g.grime(g.mix(srgb(colour), (0.2, 0.19, 0.17, 1), g.math('MULTIPLY', worn, scuff)), .4, .3))
    g.set('Roughness', g.math('ADD', g.math('MULTIPLY', scuff, .25), rough))
    g.set('Normal', g.bump(g.noise(60.0, 4.0).outputs['Fac'], .2, .001))
    return g.mat


def leather(name, colour, rough=.55, crease=40.0):
    g = Graph(name)
    worn = g.edges(.005)
    var = g.ramp(g.noise(6.0, 4.0).outputs['Fac'], .3, .8)
    base = g.mix(srgb(colour), g.mix(srgb(colour), (1, 1, 1, 1), .25), g.math('ADD', g.math('MULTIPLY', worn, .8), g.math('MULTIPLY', var, .15)))
    g.set('Base Color', g.grime(base, .7, .3))
    g.set('Roughness', rough)
    vor = g.node('ShaderNodeTexVoronoi', feature='DISTANCE_TO_EDGE', in_Scale=crease)
    g.link(g.coords(), 'Object', vor, 'Vector')
    g.set('Normal', g.bump(g.noise(crease * .5, 5.0).outputs['Fac'], .25, .0015, g.bump(vor.outputs['Distance'], .15, .001)))
    return g.mat


def metal(name='CrewSteel', colour=(0.62, 0.6, 0.56), rough=.38):
    g = Graph(name)
    rust = g.ramp(g.noise(25.0, 8.0, .7).outputs['Fac'], .62, .78)
    g.set('Base Color', g.mix(srgb(colour), (0.22, 0.1, 0.04, 1), rust))
    g.set('Metallic', g.math('SUBTRACT', 1.0, rust))
    g.set('Roughness', g.math('ADD', g.math('MULTIPLY', rust, .5), rough))
    return g.mat


def attr(g, name):
    return g.node('ShaderNodeAttribute', attribute_type='GEOMETRY', attribute_name=name).outputs['Fac']


def skin(name='CrewSkin', colour=(0.60, 0.43, 0.34), under_colour=(0.22, 0.17, 0.15), under_amount=.8):
    """Weathered skin: subsurface, pores, flushed nose/cheeks/ears, lip colour, shadow under hair and beard."""
    g = Graph(name)
    base = srgb(colour)
    blotch = g.noise(40.0, 6.0, .6).outputs['Fac']
    base = g.mix(base, srgb((0.62, 0.42, 0.34)), g.math('MULTIPLY', g.ramp(blotch, .45, .75), .35))
    base = g.mix(base, srgb((0.64, 0.36, 0.3)), g.math('MULTIPLY', attr(g, 'flush'), .3))
    base = g.mix(base, srgb((0.5, 0.31, 0.28)), g.math('MULTIPLY', attr(g, 'lip'), .6))
    under = g.math('MAXIMUM', g.math('MAXIMUM', attr(g, 'hair'), attr(g, 'beard')), attr(g, 'brow'))
    base = g.mix(base, srgb(under_colour), g.math('MULTIPLY', under, under_amount))
    g.set('Base Color', base)
    g.set('Subsurface Weight', .25)
    g.set('Subsurface Radius', (1.0, .45, .25))
    g.set('Subsurface Scale', .006)
    rough = g.math('ADD', g.math('MULTIPLY', g.noise(25.0).outputs['Fac'], .15), .5)
    g.set('Roughness', g.math('SUBTRACT', rough, g.math('MULTIPLY', attr(g, 'lip'), .1)))
    pores = g.node('ShaderNodeTexVoronoi', in_Scale=1400.0)
    g.link(g.coords(), 'Object', pores, 'Vector')
    fine = g.noise(500.0, 4.0).outputs['Fac']
    g.set('Normal', g.bump(g.math('ADD', g.math('MULTIPLY', pores.outputs['Distance'], .6), g.math('MULTIPLY', fine, .4)), .2, .0003))
    return g.mat


def plain(name, colour, rough=.5, coat=0.0, emit=0.0):
    g = Graph(name)
    g.set('Base Color', srgb(colour))
    g.set('Roughness', rough)
    if coat:
        g.set('Coat Weight', coat)
    if emit:
        g.set('Emission Color', srgb(colour))
        g.set('Emission Strength', emit)
    return g.mat


def hair(name='CrewHair', colour=(0.032, 0.024, 0.018)):
    """Short dark hair and beard: fine grain, faint streaks, a few grey hairs in the beard."""
    g = Graph(name)
    tw = g.node('ShaderNodeTexWave', wave_type='BANDS', bands_direction='Z', in_Scale=2600.0, in_Distortion=25.0, in_Detail=4.0)
    g.link(g.coords(), 'Object', tw, 'Vector')
    grain = g.noise(6000.0, 2.0).outputs['Fac']
    patch = g.noise(60.0, 3.0).outputs['Fac']
    c = g.mix(srgb(colour), srgb((0.11, 0.08, 0.06)), g.math('MULTIPLY', g.ramp(patch, .4, .7), .5))
    c = g.mix(c, (0, 0, 0, 1), g.math('MULTIPLY', tw.outputs['Fac'], .25))
    grey = g.math('MULTIPLY', attr(g, 'beard'), g.ramp(grain, .66, .7))
    c = g.mix(c, srgb((0.42, 0.4, 0.38)), g.math('MULTIPLY', grey, .45))
    g.set('Base Color', c)
    g.set('Roughness', .82)
    g.set('Normal', g.bump(g.math('ADD', g.math('MULTIPLY', tw.outputs['Fac'], .3), grain), .45, .0003))
    # Edges fade out instead of ending on a polygon line; the beard is a little see-through, like stubble.
    alpha = g.ramp(attr(g, 'cover'), .05, .7)
    coarse = g.noise(2400.0, 2.0).outputs['Fac']
    speck = g.math('ADD', g.math('MULTIPLY', g.ramp(coarse, .35, .65), .45), .55)
    # Scalp and stubble show through a little; brows are solid.
    alpha = g.math('MULTIPLY', alpha, g.mix(speck, (1, 1, 1, 1), attr(g, 'brow')))
    clear = g.node('ShaderNodeBsdfTransparent')
    mixer = g.node('ShaderNodeMixShader')
    g.nt.links.new(alpha, mixer.inputs[0])
    g.link(clear, 'BSDF', mixer, 1)
    g.link(g.bsdf, 'BSDF', mixer, 2)
    g.link(mixer, 'Shader', g.out, 'Surface')
    return g.mat


def eye(name='CrewEye'):
    """Sclera, a hazel-brown iris with radial fibres and a dark limbal ring, and the pupil, all along local -Y."""
    g = Graph(name)
    co = g.coords()
    nrm = g.node('ShaderNodeVectorMath', operation='NORMALIZE')
    g.link(co, 'Object', nrm, 0)
    sep = g.node('ShaderNodeSeparateXYZ')
    g.link(nrm, 'Vector', sep, 'Vector')
    front = g.math('MULTIPLY', sep.outputs['Y'], -1.0)
    ang = g.math('ARCTAN2', sep.outputs['X'], sep.outputs['Z'])
    fib = g.node('ShaderNodeTexNoise', in_Scale=30.0, in_Detail=8.0)
    comb = g.node('ShaderNodeCombineXYZ')
    g.nt.links.new(g.math('MULTIPLY', ang, 6.0), comb.inputs[0])
    g.nt.links.new(g.math('MULTIPLY', front, 40.0), comb.inputs[1])
    g.link(comb, 'Vector', fib, 'Vector')
    iris_c = g.mix(srgb((0.30, 0.20, 0.10)), srgb((0.52, 0.42, 0.2)), g.ramp(fib.outputs['Fac'], .35, .7))
    iris_c = g.mix(iris_c, srgb((0.08, 0.05, 0.03)), g.ramp(front, .87, .9, (1, 1, 1, 1), (0, 0, 0, 1)))
    sclera = g.mix(srgb((0.86, 0.82, 0.78)), srgb((0.8, 0.55, 0.5)), g.ramp(front, .5, .2))
    c = g.mix(sclera, iris_c, g.ramp(front, .855, .865))
    c = g.mix(c, (0.005, 0.005, 0.005, 1), g.ramp(front, .982, .986))
    g.set('Base Color', c)
    g.set('Roughness', .25)
    g.set('Subsurface Weight', .1)
    return g.mat


def cornea(name='CrewCornea'):
    g = Graph(name)
    g.set('Base Color', (1, 1, 1, 1))
    g.set('Transmission Weight', 1.0)
    g.set('Roughness', 0.0)
    g.set('IOR', 1.376)
    return g.mat


def library(tint):
    """Every material one crew member needs; only Jacket carries the player's colour."""
    colour = PALETTE.get(tint, tint) if isinstance(tint, str) else tint
    return {
        'Jacket': fabric('CrewJacket', colour, .78, 1200.0, .45, .3, 'ripstop', .3, .06),
        'JacketDark': fabric('CrewJacketDark', tuple(c * .45 for c in colour), .8, 1300.0, .45, .2, 'twill', .2),
        'Trim': fabric('CrewTrim', (0.11, 0.11, 0.105), .8, 1100.0),
        'Vest': fabric('CrewVest', (0.20, 0.205, 0.19), .74, 1500.0, .4, .12, 'twill', .35, .12),
        'Webbing': fabric('CrewWebbing', (0.075, 0.075, 0.072), .7, 600.0, .4, .04),
        'Trousers': fabric('CrewTrousers', (0.34, 0.31, 0.25), .85, 900.0, .55, .3, 'twill', .2, .15),
        'Patch': fabric('CrewPatch', (0.05, 0.05, 0.05), .9, 2500.0, wrinkle=.02),
        'Tape': tape(),
        'Rubber': rubber(),
        'KneePad': rubber('CrewKneePad', (0.035, 0.035, 0.035), .35),
        'Plastic': rubber('CrewPlastic', (0.03, 0.03, 0.032), .4),
        'Glove': fabric('CrewGlove', (0.06, 0.06, 0.06), .7, 1600.0, .3, .1),
        'GlovePalm': leather('CrewGlovePalm', (0.32, 0.30, 0.27), .7, 70.0),
        'Boot': leather('CrewBoot', (0.17, 0.105, 0.065), .5, 30.0),
        'Sole': rubber('CrewSole', (0.04, 0.035, 0.03), .7),
        'Steel': metal(),
        'Lace': plain('CrewLace', (0.16, 0.12, 0.08), .8),
        'Skin': skin(),
        'Lip': plain('CrewLipPink', (0.62, 0.36, 0.34), .45),
        'Hair': hair(),
        'Eye': eye(),
        'Cornea': cornea(),
        'Lash': plain('CrewLash', (0.02, 0.015, 0.012), .6),
        'Stencil': plain('CrewStencil', (0.84, 0.8, 0.7), .6),
        'Cardboard': plain('CrewCardboard', (0.45, 0.32, 0.18), .8),
        'Lens': plain('CrewLens', (0.9, 0.88, 0.8), .05, emit=0.0),
        'ChemLight': plain('CrewChemLight', (0.35, 0.95, 0.3), .3, emit=1.5),
        'HardHat': rubber('CrewHardHat', (0.92, 0.72, 0.08), .32),
        'Knit': fabric('CrewKnit', (0.42, 0.08, 0.06), .95, 300.0, .3, .3, 'twill', .5),
        'CapCloth': fabric('CrewCapCloth', (0.12, 0.16, 0.26), .85, 1100.0, .3, .15, 'twill', .3, .2),
    }
