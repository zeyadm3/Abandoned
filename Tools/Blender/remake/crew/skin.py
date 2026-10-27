"""Skin for the playable workers: every variation is relative to the character's own tone (the 2026-10-22 crew skin
mixed toward fixed light colours, which blotched dark skin). Hair, beard and brows are painted with their own
strengths, so stubble can be a shadow while a crew cut is solid."""
import os
import sys

_CREW = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), 'crew')
if _CREW not in sys.path:
    sys.path.insert(0, _CREW)
import crew_materials as CM  # noqa: E402


def _shift(c, k, warm=0.0):
    return (min(1, c[0] * k + warm), min(1, c[1] * k), min(1, c[2] * k - warm * .3))


def skin(name, tone, hair_colour, hair=.97, beard=.95, brow=.95, freckles=0.0):
    g = CM.Graph(name)
    attr = lambda n: g.node('ShaderNodeAttribute', attribute_type='GEOMETRY', attribute_name=n).outputs['Fac']  # noqa: E731
    base = CM.srgb(tone)
    blotch = g.noise(40.0, 6.0, .6).outputs['Fac']
    base = g.mix(base, CM.srgb(_shift(tone, 1.06, .03)), g.math('MULTIPLY', g.ramp(blotch, .45, .75), .3))
    base = g.mix(base, CM.srgb(_shift(tone, .96, .06)), g.math('MULTIPLY', attr('flush'), .35))
    base = g.mix(base, CM.srgb(_shift(tone, .82, .05)), g.math('MULTIPLY', attr('lip'), .6))
    if freckles:
        fr = g.node('ShaderNodeTexVoronoi', in_Scale=260.0)
        g.link(g.coords(), 'Object', fr, 'Vector')
        dots = g.ramp(fr.outputs['Distance'], .0, .18, (1, 1, 1, 1), (0, 0, 0, 1))
        base = g.mix(base, CM.srgb(_shift(tone, .78, .04)), g.math('MULTIPLY', g.math('MULTIPLY', dots, attr('flush')), freckles))
    hc = CM.srgb(hair_colour)
    for mask, amount in (('hair', hair), ('beard', beard), ('brow', brow)):
        if amount > 0:
            base = g.mix(base, hc, g.math('MULTIPLY', attr(mask), amount))
    g.set('Base Color', base)
    g.set('Subsurface Weight', .2)
    g.set('Subsurface Radius', (1.0, .45, .25))
    g.set('Subsurface Scale', .006)
    rough = g.math('ADD', g.math('MULTIPLY', g.noise(25.0).outputs['Fac'], .15), .5)
    g.set('Roughness', g.math('SUBTRACT', rough, g.math('MULTIPLY', attr('lip'), .1)))
    pores = g.node('ShaderNodeTexVoronoi', in_Scale=1400.0)
    g.link(g.coords(), 'Object', pores, 'Vector')
    fine = g.noise(500.0, 4.0).outputs['Fac']
    g.set('Normal', g.bump(g.math('ADD', g.math('MULTIPLY', pores.outputs['Distance'], .6), g.math('MULTIPLY', fine, .4)), .2, .0003))
    return g.mat
