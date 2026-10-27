"""Blink and lip-sync shapes for the crew heads (2026 remake, after the handover): shape keys on the finished game
mesh, found from the face's own landmarks (face.EYE_Z, face.CY, the eye spacing and the head's scale), so every
character gets them with no hand work.

- Blink: the eye openings close to a slit, lids meeting a little below the eye line and coming forward over the eyeball.
- JawOpen: the chin and lower lip turn down and back about a pivot under the ears.
The mouth's dark inside (mouth_bag) sits behind the lips, so an open jaw shows a mouth, not the inside of the head.
The crew face looks down -Y; heights are in metres in the crew frame (eyes at 1.65 m)."""
import math

import numpy as np

from remake.crew import face


def _smooth(a, b, x):
    t = np.clip((x - a) / (b - a), 0, 1)
    return t * t * (3 - 2 * t)


def mouth_bag(kit, material, sc):
    """A dark ellipsoid behind the lips (hidden while the mouth is shut)."""
    z = face.EYE_Z - .076 * sc
    ob = kit.sphere('MouthBag', 1.0, (0, face.CY - .062 * sc, z), segments=12, rings=8, scale=(.021 * sc, .013 * sc, .011 * sc))
    ob.data.materials.append(material)
    return ob


def add(ob, sc, eye_x):
    """Shape keys Blink and JawOpen on `ob`: the decimated head, already turned to the game's facing by skinned.merge
    (skinned.TURN, half a turn about Z), so its face looks down +Y; the landmarks are in the crew frame (face down -Y),
    so depths are mirrored on the way in and out."""
    if ob.data.shape_keys is None:
        ob.shape_key_add(name='Basis', from_mix=False)
    co = np.zeros(len(ob.data.vertices) * 3)
    ob.data.vertices.foreach_get('co', co)
    co = co.reshape(-1, 3)
    co[:, 0] *= -1
    co[:, 1] *= -1
    x, y, z = co[:, 0], co[:, 1], co[:, 2]
    front = y < face.CY - .03 * sc

    blink = co.copy()
    for s in (-1, 1):
        dx = (x - s * eye_x * sc) / (.02 * sc)
        dz = (z - face.EYE_Z) / (.015 * sc)
        w = np.clip(1 - (dx * dx + dz * dz), 0, 1) ** .6 * front
        blink[:, 2] = np.where(w > 0, face.EYE_Z - .0018 * sc + (blink[:, 2] - face.EYE_Z) * (1 - .9 * w), blink[:, 2])
        blink[:, 1] = blink[:, 1] - .0022 * sc * w
    _key(ob, 'Blink', blink)

    jaw = co.copy()
    mouth = face.EYE_Z - .075 * sc
    w = (_smooth(mouth + .003 * sc, mouth - .012 * sc, z) * _smooth(face.EYE_Z - .2 * sc, face.EYE_Z - .16 * sc, z)
         * np.clip(1 - (np.abs(x) / (.075 * sc)) ** 4, 0, 1) * _smooth(face.CY + .035 * sc, face.CY - .02 * sc, y))
    py, pz = face.CY + .01 * sc, face.EYE_Z - .035 * sc
    theta = math.radians(16) * w
    dy, dz = y - py, z - pz
    jaw[:, 1] = py + dy * np.cos(theta) - dz * np.sin(theta)
    jaw[:, 2] = pz + dy * np.sin(theta) + dz * np.cos(theta)
    _key(ob, 'JawOpen', jaw)


def _key(ob, name, co):
    co = co.copy()
    co[:, 0] *= -1
    co[:, 1] *= -1
    key = ob.shape_key_add(name=name, from_mix=False)
    key.data.foreach_set('co', co.reshape(-1).astype(np.float32))
    return key
