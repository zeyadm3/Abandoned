"""Parameterised realistic heads for the eight playable workers (built on Tools/Blender/crew/crew_head.py).

Every face is one dense loft (about 1 mm sections) sculpted with anatomical displacement fields; the parameters
below change the person: skull size, jaw, brow, cheekbones, nose, lips, chin, eye spacing, fullness and age
lines; masks decide where scalp hair, beard and brows are painted (plus hair volumes from hair.py). Built in the
crew sources' frame (facing -Y, the character's left at +X); export.py turns it to the remake's +Y front.
"""
import math
import os
import sys

import bpy
import numpy as np

_CREW = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), 'crew')
if _CREW not in sys.path:
    sys.path.insert(0, _CREW)
import crew_geo as G  # noqa: E402
from crew_head import KEYS, NA, NH, _catmull, g, sstep  # noqa: E402
from crew_head_parts import LID_GAP, LID_W, ear, eyes, lid_curves, neck  # noqa: E402

EYE_Z, CY, EYE_R = 1.65, .008, .0122

# Neutral (the 2026-10-22 labourer) values; a character overrides any of them.
BASE = dict(scale=1.0, width=1.0, depth=1.0, brow=1.0, temples=1.0, cheek=1.0, nose_wing=1.0, nose_tip=1.0,
            bridge=1.0, nose_w=1.0, philtrum=1.0, fullness=1.0, jaw=1.0, chin=1.0, sockets=1.0, lips=1.0,
            mouth_w=1.0, age=1.0, eye_x=.0315, fold=1.0, chin_split=1.0)


def _profile(h, alpha, P):
    hs = np.array([k[0] for k in KEYS])[::-1]
    rs = np.array([k[1] for k in KEYS])[::-1].copy()
    # Front/back depth and side width scale separately (columns: 0, 45, 90, 135, 180 degrees).
    rs[:, 2] *= P['width']
    rs[:, 1] *= (P['width'] + P['depth']) / 2
    rs[:, 3] *= (P['width'] + P['depth']) / 2
    rs[:, 0] *= P['depth']
    rs[:, 4] *= P['depth']
    # Jaw: the lower rows widen or narrow with the jaw parameter (square vs tapered faces).
    for row, hk in enumerate(hs):
        if hk < -.06:
            k = 1 + (P['jaw'] - 1) * .06 * min(1.0, (-.06 - hk) / .05)
            rs[row, 1:4] *= k
    full = np.concatenate([rs, rs[:, 3:0:-1]], axis=1)
    a = (alpha % (2 * np.pi)) / (np.pi / 4)
    i = np.floor(a).astype(int)
    t = a - i
    ring = np.empty((len(hs), len(alpha)))
    for row in range(len(hs)):
        c = full[row]
        ring[row] = _catmull(c[(i - 1) % 8], c[i % 8], c[(i + 1) % 8], c[(i + 2) % 8], t)
    j = np.clip(np.searchsorted(hs, h) - 1, 0, len(hs) - 2)
    out = np.empty((len(h), len(alpha)))
    for k, hk in enumerate(h):
        jj = j[k]
        tt = (hk - hs[jj]) / (hs[jj + 1] - hs[jj])
        p = [ring[max(jj - 1, 0)], ring[jj], ring[jj + 1], ring[min(jj + 2, len(hs) - 1)]]
        out[k] = np.maximum(_catmull(*p, tt), .002)
    return out


def sculpt(X, H, A, P):
    front = np.clip(np.cos(A), 0, 1) ** .5
    ex = P['eye_x']
    d = np.zeros_like(X)
    for s in (-1, 1):
        d += .0055 * P['brow'] * g(X, H, s * .03, .02, .02, .009) * front
        d -= .0035 * P['temples'] * g(X, H, s * .07, .035, .015, .025)
        d += .005 * P['cheek'] * g(X, H, s * .056, -.012, .018, .014) * front
        d += .0085 * P['nose_wing'] * g(X, H, s * .0155 * P['nose_w'], -.041, .0075 * P['nose_w'], .0075) * front
        d -= .003 * g(X, H, s * .008 * P['nose_w'], -.0455, .0035, .0022) * front
        d += .0016 * P['philtrum'] * g(X, H, s * .0045, -.056, .0022, .006) * front
        d += .003 * P['fullness'] * g(X, H, s * .046, -.052, .016, .016) * front
        d += .0045 * P['jaw'] * g(X, H, s * .062, -.104, .013, .014) * (1 - front * .5)
        d += .0045 * P['chin'] * g(X, H, s * .011 * P['chin_split'], -.113, .016, .014) * front
        d -= .009 * P['sockets'] * g(X, H, s * ex, .0, .017, .012) * front
        ax, ah, bx, bh = s * .024, -.044, s * .031, -.077
        t = np.clip(((X - ax) * (bx - ax) + (H - ah) * (bh - ah)) / ((bx - ax) ** 2 + (bh - ah) ** 2), 0, 1)
        dist = np.hypot(X - (ax + t * (bx - ax)), H - (ah + t * (bh - ah)))
        d -= .0022 * P['fold'] * P['age'] * np.exp(-(dist / .0035) ** 2) * front
    d += .004 * P['brow'] * g(X, H, 0, .012, .012, .01) * front
    t = np.clip((.006 - H) / .046, 0, 1)
    window = 1 / (1 + np.exp(-(H + .044) / .0022)) * 1 / (1 + np.exp(-(.012 - H) / .006))
    d += (.006 + .02 * t ** 1.3) * P['bridge'] * np.exp(-(X / ((.0068 + .006 * t) * P['nose_w'])) ** 2) * window * front
    d += .006 * P['nose_tip'] * g(X, H, 0, -.036, .0105 * P['nose_w'], .01) * front
    mw = P['mouth_w']
    d += .0055 * P['lips'] * np.exp(-(X / (.024 * mw)) ** 4) * np.exp(-((H + .069) / .0055) ** 2) * front
    d += .0058 * P['lips'] * np.exp(-(X / (.021 * mw)) ** 4) * np.exp(-((H + .081) / .006) ** 2) * front
    d -= .0042 * np.exp(-((H - (-.0745 - 3.5 * X * X)) / .0014) ** 2) * np.exp(-(X / (.026 * mw)) ** 6) * front
    d -= .0028 * g(X, H, 0, -.092, .02, .004) * front
    across = np.exp(-(X / .042) ** 4) * front
    for hz in (.047, .059, .071):
        d -= .00055 * P['age'] * np.exp(-((H - hz - 120 * X * X * .02) / .0011) ** 2) * across
    for s in (-1, 1):
        d -= .0006 * P['age'] * np.exp(-((X - s * .0062) / .0011) ** 2) * sstep(.008, .014, H) * sstep(.034, .026, H) * front
        d -= .0007 * P['age'] * np.exp(-((H + .0125 + 2.5 * (X - s * ex) ** 2) / .0013) ** 2) * np.exp(-((X - s * (ex + .002)) / .013) ** 4) * front
        # Crow's feet, only with age.
        if P['age'] > 1.0:
            for k in range(3):
                d -= .0004 * (P['age'] - 1) * np.exp(-((H - (-.004 + k * .006) - .25 * (np.abs(X) - .05)) / .0009) ** 2) * \
                     np.exp(-((np.abs(X) - .058) / .008) ** 2) * front
    return d


def lids(X, H, F, fe, ex):
    face = (F > .03).astype(float)
    for s in (-1, 1):
        dx, dz = X - s * ex, H - .001
        out = s * dx
        u = np.clip(1 - (out / LID_W) ** 2, 0, 1)
        zu, zl = lid_curves(out)
        rr = np.hypot(dx, dz)
        RL = EYE_R + LID_GAP
        lid = fe + np.sqrt(np.maximum(RL * RL - rr * rr, 0))
        tuck = fe + np.sqrt(np.maximum(EYE_R ** 2 - rr * rr, 0)) - .0035
        inside = (np.abs(out) < LID_W) & (dz < zu) & (dz > zl)
        target = np.where(inside, tuck, lid)
        ell = np.sqrt((dx / .024) ** 2 + (dz / .019) ** 2)
        w = sstep(1.0, .55, ell) * sstep(RL * 1.25, RL * .95, rr) * face
        F = F * (1 - w) + target * w
        band = (np.abs(out) < LID_W).astype(float) * u * face
        F = F - .0012 * np.exp(-((dz - zu - .0045) / .0016) ** 2) * band
        F = F + .0008 * np.exp(-((dz + .0085) / .003) ** 2) * band
    return F


# Hairline height (metres above the eyes) by angle from the front, per style; below it no scalp hair is painted.
HAIRLINES = {
    'crew':    ([0, 30, 45, 60, 75, 88, 110, 140, 180], [.085, .089, .08, .05, -.012, .012, .012, -.07, -.086]),
    'shaved':  ([0, 30, 45, 60, 75, 88, 110, 140, 180], [.088, .092, .085, .056, -.008, .016, .016, -.065, -.08]),
    'long':    ([0, 30, 45, 60, 75, 88, 110, 140, 180], [.08, .082, .075, .045, -.02, .0, -.01, -.09, -.11]),
    'receding': ([0, 30, 45, 60, 75, 88, 110, 140, 180], [.125, .12, .1, .06, -.012, .012, .012, -.07, -.086]),
    'fringe':  ([0, 30, 60, 80, 88, 110, 140, 180], [.2, .2, .2, .02, .012, .0, -.05, -.07]),   # bald on top, a ring at the back
    'none':    ([0, 180], [1.0, 1.0]),
}
BEARD_LINES = {  # beard top edge height by angle (crew_head's short beard), scaled per style
    'short': ([0, 20, 35, 55, 75, 95, 180], [-.051, -.054, -.05, -.036, -.018, -.07, -.12]),
    'full':  ([0, 20, 35, 55, 75, 95, 180], [-.049, -.05, -.042, -.026, -.01, -.06, -.12]),
    'stubble': ([0, 20, 35, 55, 75, 95, 180], [-.051, -.054, -.05, -.036, -.018, -.07, -.12]),
}


def masks(X, H, A, P):
    deg = np.degrees(np.abs((A + np.pi) % (2 * np.pi) - np.pi))
    hx, hy = HAIRLINES[P.get('hairline', 'crew')]
    thr = np.interp(deg, hx, hy)
    ear_gap = ((deg > 84) & (deg < 118) & (H < .012) & (H > -.06)).astype(float)
    scalp = sstep(thr - .007, thr + .01, H) * (1 - ear_gap)
    if P.get('hairline') == 'fringe':
        scalp = scalp * sstep(.07, .05, H)  # no hair above the ring on top
    beard_style = P.get('beard', 'none')
    beard = np.zeros_like(H)
    lipm = np.exp(-(X / .024) ** 4) * (sstep(-.091, -.084, H) * sstep(-.06, -.066, H))
    if beard_style in BEARD_LINES:
        bx, by = BEARD_LINES[beard_style]
        hb = np.interp(deg, bx, by)
        beard = sstep(hb + .008, hb - .008, H) * sstep(-.152, -.132, H) * (1 - lipm) * (1 - ear_gap)
    if beard_style in ('moustache', 'goatee'):
        must = np.exp(-(X / .03) ** 4) * sstep(-.072, -.068, H) * sstep(-.052, -.058, H) * (deg < 40)
        beard = np.maximum(beard, must * (1 - lipm))
        if beard_style == 'goatee':
            beard = np.maximum(beard, np.exp(-(X / .022) ** 4) * sstep(-.088, -.094, H) * sstep(-.14, -.13, H) * (deg < 40))
    hbrow = .019 + .006 * np.clip(1 - ((np.abs(X) - .038) / .03) ** 2, 0, 1)
    wbrow = (.0058 - .0028 * np.clip((np.abs(X) - .015) / .04, 0, 1)) * P.get('brow_thick', 1.0)
    brow = (np.exp(-((H - hbrow) / wbrow) ** 4) * sstep(.009, .014, np.abs(X)) * sstep(.062, .054, np.abs(X)) * (deg < 60))
    flush = (np.exp(-((X / .014) ** 2 + ((H + .036) / .012) ** 2))
             + .6 * np.exp(-(((np.abs(X) - .05) / .018) ** 2 + ((H + .03) / .018) ** 2)) * (deg < 70))
    return {'hair': np.clip(scalp, 0, 1), 'beard': np.clip(beard, 0, 1), 'brow': np.clip(brow, 0, 1),
            'lip': np.clip(lipm, 0, 1), 'flush': np.clip(flush, 0, 1)}


def build_head(M, head, params, nh=NH, na=NA):
    """The head surface plus neck, ears and eyes, attached to the `head` pivot. Returns (parts, head_object)."""
    P = dict(BASE)
    P.update(params)
    h = np.linspace(-.1435, .128, nh)
    a = np.linspace(0, 2 * np.pi, na, endpoint=False)
    R = _profile(h, a, P)
    A = np.broadcast_to(a, R.shape)
    H = np.broadcast_to(h[:, None], R.shape)
    R = R + sculpt(R * np.sin(A), H, A, P)
    X, F = R * np.sin(A), R * np.cos(A)
    ex = P['eye_x']
    row, col = np.argmin(np.abs(h - 0.0)), np.argmin(np.abs(a - math.atan2(ex, .085)))
    fe = F[row, col] - EYE_R + .0015
    F = lids(X, H, F, fe, ex)
    sc = P['scale']
    co = np.stack([X * sc, CY - F * sc, EYE_Z + H * sc], axis=-1).reshape(-1, 3)
    r_i, c_i = np.meshgrid(np.arange(nh - 1), np.arange(na), indexing='ij')
    q = np.stack([r_i * na + c_i, r_i * na + (c_i + 1) % na, (r_i + 1) * na + (c_i + 1) % na, (r_i + 1) * na + c_i], -1)
    me = bpy.data.meshes.new('Head')
    me.from_pydata(co.tolist(), [], q.reshape(-1, 4).tolist())
    me.update()
    m = masks(X, H, A, P)
    for key, val in m.items():
        attr = me.attributes.new(key, 'FLOAT', 'POINT')
        attr.data.foreach_set('value', val.reshape(-1).astype(np.float32))
    me.shade_smooth()
    me.materials.append(M['Skin'])
    uv = me.uv_layers.new(name='UVMap')
    vi = np.zeros(len(me.loops), dtype=np.int64)
    me.loops.foreach_get('vertex_index', vi)
    colm = (vi % na + na // 2) % na
    u4 = (colm / na).reshape(-1, 4)
    u4 = np.where(u4.max(1)[:, None] - u4 > .5, u4 + 1, u4)
    uvs = np.stack([u4.reshape(-1), (vi // na) / nh], -1).astype(np.float32)
    uv.data.foreach_set('uv', uvs.reshape(-1))
    ob = bpy.data.objects.new('Head', me)
    G.COLL.objects.link(ob)
    parts = [ob, *neck(M)]
    parts += [ear(M, s, EYE_Z, CY) for s in (-1, 1)]
    parts += eyes(M, fe * sc, CY, ex * sc, EYE_Z, EYE_R)
    for p in parts:
        G.attach(p, head)
    top = float(co[:, 2].max())
    return parts, ob, top
