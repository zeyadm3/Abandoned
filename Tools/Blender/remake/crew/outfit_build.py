"""Build and export the outfits, skinned on the crew skeleton, three builds each.

Batch:
  Blender --background --factory-startup --python Tools/Blender/remake/crew/outfit_build.py -- [ids...] [--preview]

Writes Art/Remake/Characters/Outfit_<id>.fbx with meshes Outfit_<id>_B0 / _B1 / _B2 (stocky, average, slim; same
UVs), textures Outfit_<id>_* and Outfit_<id>.json. Faces of the outfit's tint slots use the material
RM_Outfit_<id>_Tint (same textures; Unity tints it with the player's colour).
"""
import json
import os
import sys

import bpy
import numpy as np
from mathutils import Vector

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(os.path.dirname(_HERE)))
exec(open(os.path.join(os.path.dirname(_HERE), 'boot.py')).read())

from remake.core import bake, export, kit, preview, rig  # noqa: E402
from remake.crew import outfits, skinned  # noqa: E402

OUT = export.art_dir('Characters')
TEX = export.art_dir('Characters', 'Textures')
BUDGET = 16000


def _fade(z, lo, hi):
    t = min(1.0, max(0.0, (z - lo) / (hi - lo)))
    return t * t * (3 - 2 * t)


def reshape(ob, b):
    """Move vertices toward each bone's axis by the build's girth factors (stocky = unchanged)."""
    if b['torso'] == 1.0 and b['arms'] == 1.0 and b['legs'] == 1.0:
        return
    names = [vg.name for vg in ob.vertex_groups]
    segs = {n: (Vector(h), Vector(t)) for n, (h, t, _) in rig.CREW.items()}
    for v in ob.data.vertices:
        p = v.co.copy()
        delta = Vector()
        total = 0.0
        for g in v.groups:
            bone = names[g.group]
            w = g.weight
            if w <= 0:
                continue
            total += w
            if bone in ('Body', 'Torso'):
                k = _fade(p.z, .78, .9) * (1 - _fade(p.z, 1.44, 1.52))
                fx = 1 + (b['torso'] - 1) * k
                fy = 1 + ((b['front'] if p.y > 0 else b['back']) - 1) * k
                delta += Vector((p.x * (fx - 1), p.y * (fy - 1), 0)) * w
            elif bone == 'Head':
                continue
            else:
                h, t = segs[bone]
                d = t - h
                s = max(0.0, min(1.0, (p - h).dot(d) / d.length_squared))
                radial = p - (h + d * s)
                arm = bone.startswith(('Arm', 'Elbow'))
                f = b['arms'] if arm else b['legs']
                # Hands and boots keep their size.
                k = _fade(p.z, .86, .92) if arm else _fade(p.z, .26, .34)
                delta += radial * ((f - 1) * k) * w
        if total > 0:
            v.co = p + delta / total
    ob.data.update()


def build(oid, spec, show=False):
    kit.reset()
    objs, M = outfits.build(oid, spec)
    bpy.context.view_layer.update()
    tint = outfits.tint_materials(M, spec)
    full = skinned.merge(objs, f'Outfit_{oid}_Full', skip=('CrewNumber',))
    skinned.weight(full)
    for o in objs:
        bpy.data.objects.remove(o, do_unlink=True)
    low = skinned.game_mesh(full, BUDGET, f'Outfit_{oid}')
    tinted = [p.index for p in low.data.polygons if low.data.materials[p.material_index] in tint]
    bake.bake_asset(low, f'Outfit_{oid}', TEX, size=2048, high=full, high_all=True, samples=12)
    bpy.data.objects.remove(full, do_unlink=True)
    tmat = bpy.data.materials.new(f'RM_Outfit_{oid}_Tint')
    low.data.materials.append(tmat)
    ti = len(low.data.materials) - 1
    for i in tinted:
        low.data.polygons[i].material_index = ti
    arm = rig.crew()
    meshes = []
    for bi, b in enumerate(outfits.BUILDS):
        m = low.copy()
        m.data = low.data.copy()
        m.name = f'Outfit_{oid}_B{bi}'
        kit.link(m)
        reshape(m, b)
        meshes.append(m)
    bpy.data.objects.remove(low, do_unlink=True)
    skinned.bind(meshes, arm)
    export.export_skinned(arm, meshes, os.path.join(OUT, f'Outfit_{oid}.fbx'))
    meta = {'id': oid, 'name': spec['name'], 'blurb': spec['blurb'], 'price': spec.get('price', 0),
            'tint': list(spec['tint']), 'builds': [b['name'] for b in outfits.BUILDS], 'boots': spec['boots']}
    with open(os.path.join(OUT, f'Outfit_{oid}.json'), 'w') as f:
        json.dump(meta, f, indent=1)
    if show:
        preview.save(f'Outfit_{oid}')
    print(f'[remake-crew] outfit {oid} tris={kit.tris(meshes[0])} tinted_faces={len(tinted)}')


def main():
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    show = '--preview' in args
    ids = [a for a in args if not a.startswith('--')] or list(outfits.OUTFITS)
    for oid in ids:
        build(oid, outfits.OUTFITS[oid], show)


if __name__ == '__main__':
    main()
