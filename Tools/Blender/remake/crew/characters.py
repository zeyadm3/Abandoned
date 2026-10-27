"""Build and export the playable workers' heads (skinned on the crew skeleton).

Batch:
  Blender --background --factory-startup --python Tools/Blender/remake/crew/characters.py -- [ids...] [--preview]

Writes Art/Remake/Characters/Head_<id>.fbx (head, neck, ears, eyes, lashes, painted hair, hair volumes; plus
the clear corneas), Art/Remake/Characters/Textures/Head_<id>_*.png and Head_<id>.json (crown and eye heights,
the build, the name), which Editor/Remake/CrewLookBuilder reads.
"""
import json
import os
import sys

import bpy

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(os.path.dirname(_HERE)))
exec(open(os.path.join(os.path.dirname(_HERE), 'boot.py')).read())

from remake.core import bake, export, kit, materials, preview, rig  # noqa: E402
from remake.crew import face, hair, roster, skin, skinned  # noqa: E402
import crew_geo as G  # noqa: E402  (face.py put Tools/Blender/crew on the path)
import crew_materials as CM  # noqa: E402

OUT = export.art_dir('Characters')
TEX = export.art_dir('Characters', 'Textures')
HEAD_BUDGET = 9000


def pivots():
    root = G.pivot('Crew', (0, 0, 0))
    body = G.pivot('Body', (0, 0, .95), root)
    torso = G.pivot('Torso', (0, 0, 1.10), body)
    head = G.pivot('Head', (0, 0, 1.52), torso)
    return head


def build(cid, spec, show=False):
    kit.reset()
    G.COLL = bpy.context.scene.collection
    under = spec.get('under', .97)
    hair_c = spec['hair_colour']
    M = {
        'Skin': skin.skin(f'Skin_{cid}', spec['tone'], hair_c, hair=under, beard=spec.get('beard_amount', .95),
                          freckles=spec.get('freckles', 0.0)),
        'Lip': CM.plain(f'Lip_{cid}', tuple(min(1, c * 1.02 + .02) for c in (spec['tone'][0] * .95, spec['tone'][1] * .7,
                                                                              spec['tone'][2] * .72)), .45),
        'Lash': CM.plain(f'Lash_{cid}', tuple(c * .6 for c in hair_c) if sum(hair_c) < .6 else (.06, .05, .04), .6),
        'Eye': hair.eye_material(f'Eye_{cid}', *spec['iris']),
        'Cornea': materials.surface('Glass', (.8, .85, .85), .05, alpha=.25, var=0, grime=0, bump=0),
    }
    head = pivots()
    parts, head_ob, top = face.build_head(M, head, spec['face'])
    if spec.get('style'):
        hm = hair.hair_material(f'Hair_{cid}', hair_c, spec.get('hair_grey', 0.0))
        for p in hair.STYLES[spec['style']](hm, top=top):
            G.attach(p, head)
            parts.append(p)
    bpy.context.view_layer.update()

    solid = [p for p in parts if not p.name.startswith('Cornea')]
    full = skinned.merge(solid, f'Head_{cid}_Full')
    cornea = skinned.merge([p for p in parts if p.name.startswith('Cornea')], f'Head_{cid}_Cornea')
    for ob in (full, cornea):
        skinned.weight(ob, rigid=('Head',))
        skinned.blend_neck(ob)
    # Bake sources: everything but the eyeballs merged, and the eyeballs as their own objects (their iris is drawn
    # in each eyeball's object space, which a merge would lose).
    high = skinned.merge([p for p in solid if not p.name.startswith('Eyeball')], f'Head_{cid}_High')
    eyeballs = [p for p in solid if p.name.startswith('Eyeball')]
    for e in eyeballs:
        e.parent = None
        e.matrix_world = skinned.TURN @ e.matrix_world
    for p in parts:
        if p not in eyeballs:
            bpy.data.objects.remove(p, do_unlink=True)
    low = skinned.game_mesh(full, HEAD_BUDGET, f'Head_{cid}')
    bpy.data.objects.remove(full, do_unlink=True)
    bake.bake_asset(low, f'Head_{cid}', TEX, size=2048, high=[high] + eyeballs, high_all=True, samples=16)
    cornea.data.materials.clear()
    cornea.data.materials.append(materials.library()['Glass'])
    for ob in [high] + eyeballs:
        bpy.data.objects.remove(ob, do_unlink=True)

    arm = rig.crew()
    skinned.bind([low, cornea], arm)
    export.export_skinned(arm, [low, cornea], os.path.join(OUT, f'Head_{cid}.fbx'))
    meta = {'id': cid, 'name': spec['name'], 'build': spec['build'], 'blurb': spec['blurb'],
            'crown': round(top, 4), 'scale': spec['face'].get('scale', 1.0), 'style': spec.get('style') or '',
            'tone': spec['tone'], 'hair': hair_c}
    with open(os.path.join(OUT, f'Head_{cid}.json'), 'w') as f:
        json.dump(meta, f, indent=1)
    if show:
        preview.save(f'Head_{cid}')
    print(f'[remake-crew] head {cid} tris={kit.tris(low)} crown={top:.3f}')


def main():
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    show = '--preview' in args
    ids = [a for a in args if not a.startswith('--')] or list(roster.CHARACTERS)
    for cid in ids:
        build(cid, roster.CHARACTERS[cid], show)


if __name__ == '__main__':
    main()
