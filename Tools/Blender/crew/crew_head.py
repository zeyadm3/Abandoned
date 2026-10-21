"""Realistic head for the crew redesign (v2): one dense surface sculpted with anatomical fields.

Eyes at 1.65 m (the game's StandingEyeHeight). Real adult size: 15.5 cm wide, 20 cm deep, 23 cm chin to crown.
The surface is a loft of horizontal sections (about 1 mm apart) whose radius is given at five angles from
the front; nose, lips, brow, cheekbones, chin and eyelids are added as displacement fields with numpy.
A stocky labourer: wide jaw, heavy brow, crew cut, short beard. Facing -Y, the character's left is +X.
"""
import math
import bpy
import numpy as np
import crew_geo as G
from crew_head_parts import LID_GAP, LID_W, ear, eyes, hair_shell, lid_curves, neck

EYE_Z, CY = 1.65, .008
EYE_X, EYE_R = .0315, .0122
# Height above the eyes, then radius at 0 (front), 45, 90 (side), 135 and 180 (back) degrees.
KEYS = [(.128, [.003] * 5), (.122, [.03, .03, .03, .032, .034]), (.11, [.055, .055, .054, .058, .062]),
        (.092, [.074, .07, .067, .075, .083]), (.068, [.086, .081, .077, .087, .096]), (.04, [.092, .086, .08, .092, .102]),
        (.016, [.093, .087, .081, .093, .103]), (-.004, [.088, .086, .081, .093, .102]), (-.025, [.09, .087, .081, .09, .098]),
        (-.048, [.091, .085, .078, .085, .09]), (-.072, [.09, .081, .074, .076, .08]), (-.096, [.086, .077, .07, .072, .075]),
        (-.116, [.077, .068, .066, .067, .07]), (-.13, [.06, .055, .056, .06, .064]), (-.14, [.03, .03, .03, .032, .034]),
        (-.1435, [.003] * 5)]
NA, NH = 360, 280


def _catmull(p0, p1, p2, p3, t):
    return .5 * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t + (-p0 + 3 * p1 - 3 * p2 + p3) * t ** 3)


def _profile(h, alpha):
    """Section radius at heights h and angles alpha from the front (radians), smoothly interpolated."""
    hs = np.array([k[0] for k in KEYS])[::-1]
    rs = np.array([k[1] for k in KEYS])[::-1]
    full = np.concatenate([rs, rs[:, 3:0:-1]], axis=1)  # 0, 45 ... 315 degrees, mirrored
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


def g(X, H, x0, h0, sx, sh):
    return np.exp(-((X - x0) / sx) ** 2 - ((H - h0) / sh) ** 2)


def sstep(e0, e1, v):
    t = np.clip((v - e0) / (e1 - e0), 0, 1)
    return t * t * (3 - 2 * t)


def sculpt(X, H, A):
    """Radial displacement (metres) for the face and skull."""
    front = np.clip(np.cos(A), 0, 1) ** .5
    d = np.zeros_like(X)
    for s in (-1, 1):
        d += .0055 * g(X, H, s * .03, .02, .02, .009) * front                      # brow ridge
        d -= .0035 * g(X, H, s * .07, .035, .015, .025)                            # temples
        d += .005 * g(X, H, s * .056, -.012, .018, .014) * front                   # cheekbones
        d += .0085 * g(X, H, s * .0155, -.041, .0075, .0075) * front               # nostril wings
        d -= .003 * g(X, H, s * .008, -.0455, .0035, .0022) * front                # nostrils
        d += .0016 * g(X, H, s * .0045, -.056, .0022, .006) * front                # philtrum ridges
        d += .003 * g(X, H, s * .046, -.052, .016, .016) * front                   # cheek fullness
        d += .0045 * g(X, H, s * .062, -.104, .013, .014) * (1 - front * .5)       # square jaw angle
        d += .0045 * g(X, H, s * .011, -.113, .016, .014) * front                  # two-lobed chin
        d -= .009 * g(X, H, s * EYE_X, .0, .017, .012) * front                     # eye sockets
        # Nasolabial fold: from the nostril wing down to the mouth corner.
        ax, ah, bx, bh = s * .024, -.044, s * .031, -.077
        t = np.clip(((X - ax) * (bx - ax) + (H - ah) * (bh - ah)) / ((bx - ax) ** 2 + (bh - ah) ** 2), 0, 1)
        dist = np.hypot(X - (ax + t * (bx - ax)), H - (ah + t * (bh - ah)))
        d -= .0022 * np.exp(-(dist / .0035) ** 2) * front
    d += .004 * g(X, H, 0, .012, .012, .01) * front                                # glabella
    t = np.clip((.006 - H) / .046, 0, 1)
    window = 1 / (1 + np.exp(-(H + .044) / .0022)) * 1 / (1 + np.exp(-(.012 - H) / .006))
    d += (.006 + .02 * t ** 1.3) * np.exp(-(X / (.0068 + .006 * t)) ** 2) * window * front  # nose bridge
    d += .006 * g(X, H, 0, -.036, .0105, .01) * front                              # nose tip
    d += .0055 * np.exp(-(X / .024) ** 4) * np.exp(-((H + .069) / .0055) ** 2) * front     # upper lip
    d += .0058 * np.exp(-(X / .021) ** 4) * np.exp(-((H + .081) / .006) ** 2) * front     # lower lip
    d -= .0042 * np.exp(-((H - (-.0745 - 3.5 * X * X)) / .0014) ** 2) * np.exp(-(X / .026) ** 6) * front  # mouth
    d -= .0028 * g(X, H, 0, -.092, .02, .004) * front                              # under the lower lip
    # Age: three forehead creases, two frown lines, a crease under each eye.
    across = np.exp(-(X / .042) ** 4) * front
    for hz in (.047, .059, .071):
        d -= .00055 * np.exp(-((H - hz - 120 * X * X * .02) / .0011) ** 2) * across
    for s in (-1, 1):
        d -= .0006 * np.exp(-((X - s * .0062) / .0011) ** 2) * sstep(.008, .014, H) * sstep(.034, .026, H) * front
        d -= .0007 * np.exp(-((H + .0125 + 2.5 * (X - s * EYE_X) ** 2) / .0013) ** 2) * np.exp(-((X - s * (EYE_X + .002)) / .013) ** 4) * front
    return d


def lids(X, H, F, fe):
    """Wrap the eyelids around each eyeball and open an almond-shaped fissure onto it."""
    face = (F > .03).astype(float)
    for s in (-1, 1):
        dx, dz = X - s * EYE_X, H - .001
        out = s * dx
        W = LID_W
        u = np.clip(1 - (out / W) ** 2, 0, 1)
        zu, zl = lid_curves(out)
        rr = np.hypot(dx, dz)
        RL = EYE_R + LID_GAP
        lid = fe + np.sqrt(np.maximum(RL * RL - rr * rr, 0))
        tuck = fe + np.sqrt(np.maximum(EYE_R ** 2 - rr * rr, 0)) - .0035
        inside = (np.abs(out) < W) & (dz < zu) & (dz > zl)
        target = np.where(inside, tuck, lid)
        ell = np.sqrt((dx / .024) ** 2 + (dz / .019) ** 2)
        w = sstep(1.0, .55, ell) * sstep(RL * 1.25, RL * .95, rr) * face
        F = F * (1 - w) + target * w
        band = (np.abs(out) < W).astype(float) * u * face
        F = F - .0012 * np.exp(-((dz - zu - .0045) / .0016) ** 2) * band            # upper lid crease
        F = F + .0008 * np.exp(-((dz + .0085) / .003) ** 2) * band                  # lower lid
    return F


def masks(X, H, A):
    """Per-vertex 0..1 maps the skin and hair read: scalp hair, beard, brows, lips, flush."""
    deg = np.degrees(np.abs((A + np.pi) % (2 * np.pi) - np.pi))
    thr = np.interp(deg, [0, 30, 45, 60, 75, 88, 110, 140, 180], [.085, .089, .08, .05, -.012, .012, .012, -.07, -.086])
    ear_gap = ((deg > 84) & (deg < 118) & (H < .012) & (H > -.06)).astype(float)
    scalp = sstep(thr - .007, thr + .01, H) * (1 - ear_gap)
    hb = np.interp(deg, [0, 20, 35, 55, 75, 95, 180], [-.051, -.054, -.05, -.036, -.018, -.07, -.12])
    beard = sstep(hb + .008, hb - .008, H) * sstep(-.152, -.132, H)
    lipm = np.exp(-(X / .024) ** 4) * (sstep(-.091, -.084, H) * sstep(-.06, -.066, H))
    beard = beard * (1 - lipm) * (1 - ear_gap)
    hbrow = .019 + .006 * np.clip(1 - ((np.abs(X) - .038) / .03) ** 2, 0, 1)
    wbrow = .0058 - .0028 * np.clip((np.abs(X) - .015) / .04, 0, 1)
    brow = (np.exp(-((H - hbrow) / wbrow) ** 4) * sstep(.009, .014, np.abs(X)) * sstep(.062, .054, np.abs(X))
            * (deg < 60))
    flush = (np.exp(-((X / .014) ** 2 + ((H + .036) / .012) ** 2))
             + .6 * np.exp(-(((np.abs(X) - .05) / .018) ** 2 + ((H + .03) / .018) ** 2)) * (deg < 70))
    return {'hair': np.clip(scalp, 0, 1), 'beard': np.clip(beard, 0, 1), 'brow': np.clip(brow, 0, 1),
            'lip': np.clip(lipm, 0, 1), 'flush': np.clip(flush, 0, 1)}


def build_head(M, head):
    h = np.linspace(-.1435, .128, NH)
    a = np.linspace(0, 2 * np.pi, NA, endpoint=False)
    R = _profile(h, a)
    A = np.broadcast_to(a, R.shape)
    H = np.broadcast_to(h[:, None], R.shape)
    R = R + sculpt(R * np.sin(A), H, A)
    X, F = R * np.sin(A), R * np.cos(A)
    # Eyeballs sit just under the sculpted surface; then the lids wrap them.
    row, col = np.argmin(np.abs(h - 0.0)), np.argmin(np.abs(a - math.atan2(EYE_X, .085)))
    fe = F[row, col] - EYE_R + .0015
    F = lids(X, H, F, fe)
    co = np.stack([X, CY - F, EYE_Z + H], axis=-1).reshape(-1, 3)
    r_i, c_i = np.meshgrid(np.arange(NH - 1), np.arange(NA), indexing='ij')
    q = np.stack([r_i * NA + c_i, r_i * NA + (c_i + 1) % NA, (r_i + 1) * NA + (c_i + 1) % NA, (r_i + 1) * NA + c_i], -1)
    me = bpy.data.meshes.new('Head')
    me.from_pydata(co.tolist(), [], q.reshape(-1, 4).tolist())
    me.update()
    m = masks(X, H, A)
    for key, val in m.items():
        attr = me.attributes.new(key, 'FLOAT', 'POINT')
        attr.data.foreach_set('value', val.reshape(-1).astype(np.float32))
    me.shade_smooth()
    me.materials.append(M['Skin'])
    uv = me.uv_layers.new(name='UVMap')
    vi = np.zeros(len(me.loops), dtype=np.int64)
    me.loops.foreach_get('vertex_index', vi)
    # Seam at the back of the head (under the hair), never down the face; wrapping faces run past u = 1.
    col = (vi % NA + NA // 2) % NA
    u4 = (col / NA).reshape(-1, 4)  # every face is a quad, loops in order
    u4 = np.where(u4.max(1)[:, None] - u4 > .5, u4 + 1, u4)
    u = u4.reshape(-1)
    uvs = np.stack([u, (vi // NA) / NH], -1).astype(np.float32)
    uv.data.foreach_set('uv', uvs.reshape(-1))
    ob = bpy.data.objects.new('Head', me)
    G.COLL.objects.link(ob)
    parts = [ob, *neck(M), hair_shell(M, ob, m)]
    parts += [ear(M, s, EYE_Z, CY) for s in (-1, 1)]
    parts += eyes(M, fe, CY, EYE_X, EYE_Z, EYE_R)
    for p in parts:
        G.attach(p, head)
    return parts
