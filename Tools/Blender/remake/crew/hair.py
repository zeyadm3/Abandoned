"""Hair volumes, hair and eye materials for the playable workers.

Short hair, stubble, beards and brows are painted onto the skin through face.masks (the skin material darkens
toward the hair colour under each mask, as the 2026-10-22 crew did). Longer styles add opaque volumes on top:
a bob shell, a bun, a ponytail, a messy top. Built in the crew frame: facing -Y, back of the head at +Y.
"""
import math
import os
import sys

import bpy
import numpy as np
from mathutils import Vector, noise

_CREW = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), 'crew')
if _CREW not in sys.path:
    sys.path.insert(0, _CREW)
import crew_geo as G  # noqa: E402
import crew_materials as CM  # noqa: E402

TOP = 1.783  # crown height of the neutral head


def hair_material(name, colour, grey=0.0):
    """Opaque strand-look hair for volumes: banding along the strands, fine grain, a little grey."""
    g = CM.Graph(name)
    tw = g.node('ShaderNodeTexWave', wave_type='BANDS', bands_direction='Z', in_Scale=900.0, in_Distortion=18.0, in_Detail=6.0)
    g.link(g.coords(), 'Object', tw, 'Vector')
    grain = g.noise(3000.0, 2.0).outputs['Fac']
    patch = g.noise(40.0, 3.0).outputs['Fac']
    c = g.mix(CM.srgb(colour), CM.srgb(tuple(min(1, x * 1.8 + .02) for x in colour)), g.math('MULTIPLY', g.ramp(patch, .45, .75), .45))
    c = g.mix(c, (0, 0, 0, 1), g.math('MULTIPLY', tw.outputs['Fac'], .35))
    if grey:
        c = g.mix(c, CM.srgb((.55, .53, .5)), g.math('MULTIPLY', g.ramp(grain, .5, .55), grey))
    g.set('Base Color', c)
    g.set('Roughness', .62)
    g.set('Normal', g.bump(g.math('ADD', tw.outputs['Fac'], g.math('MULTIPLY', grain, .4)), .55, .0008))
    return g.mat


def eye_material(name, iris=(0.30, 0.20, 0.10), iris2=(0.52, 0.42, 0.2)):
    """Sclera, a fibred iris of the character's colour, limbal ring, pupil (along local -Y, as crew_materials.eye)."""
    g = CM.Graph(name)
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
    iris_c = g.mix(CM.srgb(iris), CM.srgb(iris2), g.ramp(fib.outputs['Fac'], .35, .7))
    iris_c = g.mix(iris_c, CM.srgb((0.06, 0.05, 0.04)), g.ramp(front, .87, .9, (1, 1, 1, 1), (0, 0, 0, 1)))
    sclera = g.mix(CM.srgb((0.86, 0.82, 0.78)), CM.srgb((0.8, 0.55, 0.5)), g.ramp(front, .5, .2))
    c = g.mix(sclera, iris_c, g.ramp(front, .855, .865))
    c = g.mix(c, (0.005, 0.005, 0.005, 1), g.ramp(front, .982, .986))
    g.set('Base Color', c)
    g.set('Roughness', .25)
    return g.mat


def _shell(name, rings, mat, n=56, arc=None):
    ob = G.loft(name, rings, mat, n=n, cap=(False, False), subsurf=1, arc=arc)
    s = ob.modifiers.new('Thickness', 'SOLIDIFY')
    s.thickness, s.offset = .006, -1.0
    return ob


def _clump_noise(ob, amount=.004, scale=40.0):
    """Break a smooth hair volume into clumps (displacement along normals by 3D noise)."""
    me = ob.data
    for v in me.vertices:
        d = noise.noise(v.co * scale) * amount
        v.co += v.normal * d
    me.update()


def bob(mat, top=TOP, length=1.58, puff=.012):
    """Chin-length bob: a shell over the skull, open at the face, ends level just above the jaw."""
    rings = []
    for z, rx, rf, rb in ((top + .006, .02, .02, .02), (top - .01, .07, .07, .075), (top - .035, .09, .088, .095),
                          (top - .07, .1, .095, .108), (top - .11, .103, .09, .112), (top - .15, .102, .085, .108),
                          (length + .02, .1, .08, .1), (length, .098, .075, .097)):
        rings.append({'c': (0, .008, z), 'rx': rx + puff * .3, 'rf': rf + puff * .2, 'rb': rb + puff, 'p': 2.1})
    ob = _shell('HairBob', rings, mat, arc=(math.radians(-55), math.radians(235)))
    fringe = G.loft('HairFringe', [{'c': (0, -.07, top - .03), 'rx': .07, 'rf': .03, 'rb': .03},
                                   {'c': (0, -.088, top - .07), 'rx': .075, 'rf': .018, 'rb': .02},
                                   {'c': (0, -.093, top - .095), 'rx': .07, 'rf': .01, 'rb': .012}], mat, n=32, subsurf=1)
    return [ob, fringe]


def bun(mat, top=TOP):
    cap = bun_cap(mat, top)
    knot = G.quad_sphere('HairBun', (0, .105, top - .045), (.042, .036, .04), mat, cuts=6)
    tie = G.loft('HairTie', [{'c': (0, .072, top - .045), 'rx': .03, 'rf': .03, 'rb': .03},
                             {'c': (0, .078, top - .045), 'rx': .03, 'rf': .03, 'rb': .03}], mat, n=24, cap=(False, False))
    return [cap, knot, tie]


def bun_cap(mat, top=TOP):
    return _shell('HairCap', [{'c': (0, .008, z), 'rx': rx, 'rf': rf, 'rb': rb, 'p': 2.1} for z, rx, rf, rb in
                             ((top + .004, .02, .02, .02), (top - .02, .08, .08, .085), (top - .055, .096, .092, .102),
                              (top - .085, .097, .088, .106), (top - .115, .094, .07, .104))], mat,
                  arc=(math.radians(-48), math.radians(228)))


def ponytail(mat, top=TOP, length=.26):
    out = [bun_cap(mat, top)]
    pts = [(0, .1, top - .06, 1.0), (0, .125, top - .1, .95), (0, .13, top - .1 - length * .45, .8),
           (0, .118, top - .1 - length * .85, .55), (0, .108, top - .1 - length, .25)]
    tail = G.tube('Ponytail', pts, mat, .03, sides=14, subsurf=1)
    _clump_noise(tail, .003, 60)
    tie = G.loft('HairTie', [{'c': (0, .1, top - .065), 'rx': .021, 'rf': .021, 'rb': .021},
                             {'c': (0, .103, top - .075), 'rx': .021, 'rf': .021, 'rb': .021}], mat, n=20, cap=(False, False))
    return out + [tail, tie]


def messy(mat, top=TOP):
    cap = _shell('HairMessy', [{'c': (0, .006, z), 'rx': rx, 'rf': rf, 'rb': rb, 'p': 2.1} for z, rx, rf, rb in
                               ((top + .02, .03, .03, .03), (top + .005, .075, .08, .078), (top - .025, .094, .095, .098),
                                (top - .06, .099, .093, .104), (top - .09, .096, .05, .103))], mat,
                 arc=(math.radians(-62), math.radians(242)))
    G.bend_folds(cap, .006, .03)
    return [cap]


STYLES = {'bob': bob, 'bun': bun, 'ponytail': ponytail, 'messy': messy}
