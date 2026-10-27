"""FBX export for Unity, the remake's conventions.

Models are built with +Y as the front (Unity +Z), +Z up, +X to the model's right (Unity +X); one unit = one metre.
Static models: one root, LOD meshes named <Name>_LOD0/1/2 (Unity builds the LODGroup from the names), anchor and
socket empties (Anchor_* / Socket_*) carried as plain transforms. Skinned models: an armature plus meshes, optional
actions as named takes. Paths are relative to the repository so the same call works live (MCP) and in batch mode.
"""
import os

import bpy
from mathutils import Matrix

from . import kit

REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..', '..'))
ART = os.path.join(REPO, 'Game', 'Assets', '_Project', 'Art', 'Remake')


def art_dir(*parts):
    path = os.path.join(ART, *parts)
    os.makedirs(path, exist_ok=True)
    return path


def make_lods(ob, ratios=(1.0, .4, .12), budget=None):
    """LOD copies of a finished, baked mesh: returns [lod0, lod1, lod2], named <base>_LODn."""
    base = ob.name
    if budget:
        kit.decimate_to(ob, budget)
    ob.name = base + '_LOD0'
    out = [ob]
    t0 = kit.tris(ob)
    for i, r in enumerate(ratios[1:], start=1):
        c = ob.copy()
        c.data = ob.data.copy()
        c.name = f'{base}_LOD{i}'
        kit.link(c, ob.users_collection[0] if ob.users_collection else None)
        kit.decimate_to(c, max(int(t0 * r), 12))
        out.append(c)
    return out


def _select(objs):
    bpy.context.view_layer.update()
    for o in bpy.context.view_layer.objects:
        if o is not None:
            o.select_set(False)
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]


def _descendants(ob):
    out = [ob]
    for c in ob.children:
        out += _descendants(c)
    return out


def export_static(root, path):
    """Export root and everything under it (meshes + empties). Geometry is baked into Unity space."""
    objs = _descendants(root)
    _select(objs)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={'MESH', 'EMPTY'},
                             axis_forward='-Z', axis_up='Y', apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL',
                             bake_space_transform=True, use_mesh_modifiers=True, mesh_smooth_type='FACE',
                             use_tspace=True, add_leaf_bones=False, path_mode='STRIP', bake_anim=False,
                             use_custom_props=False)
    tri = sum(kit.tris(o) for o in objs if o.type == 'MESH' and o.name.endswith('_LOD0') or
              (o.type == 'MESH' and '_LOD' not in o.name))
    print(f'[remake-export] {os.path.relpath(path, REPO)} lod0_tris={tri}')
    return path


def export_skinned(armature, meshes, path, actions=None):
    """Export an armature and its skinned meshes; each action becomes a take named after it."""
    objs = [armature] + list(meshes)
    # Shape keys (the crew faces' blink and jaw) only survive when modifiers aren't applied; the armature is the
    # only modifier on a skinned mesh, and the exporter skins it either way.
    shapes = any(m.data.shape_keys is not None for m in meshes if m.type == 'MESH')
    _select(objs)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    kw = dict(filepath=path, use_selection=True, object_types={'ARMATURE', 'MESH', 'EMPTY'},
              axis_forward='-Z', axis_up='Y', apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL',
              bake_space_transform=False, use_mesh_modifiers=not shapes, mesh_smooth_type='FACE', use_tspace=True,
              add_leaf_bones=False, primary_bone_axis='Y', secondary_bone_axis='X', use_armature_deform_only=True,
              path_mode='STRIP')
    if actions:
        armature.animation_data_create()
        kw.update(bake_anim=True, bake_anim_use_all_bones=True, bake_anim_use_nla_strips=False,
                  bake_anim_use_all_actions=True, bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0.0)
        armature.animation_data.action = actions[0]
    else:
        kw.update(bake_anim=False)
    bpy.ops.export_scene.fbx(**kw)
    print(f'[remake-export] {os.path.relpath(path, REPO)} skinned meshes={len(meshes)} takes={len(actions or [])}')
    return path


def static_asset(name, meshes, folder, anchors=None, lods=(1.0, .4, .12), budget=None, file_name=None):
    """The common path for a finished prop: join meshes, make LODs, parent under a root with anchors, export.
    `meshes` should already carry their baked material. Returns the FBX path."""
    body = kit.join(meshes, name) if len(meshes) > 1 else meshes[0]
    body.name = name
    root = kit.empty(name + '_Root')
    levels = make_lods(body, lods, budget) if lods else [body]
    for lv in levels:
        lv.parent = root
    for a in anchors or []:
        a.parent = root
    path = export_static(root, os.path.join(art_dir(folder), (file_name or name) + '.fbx'))
    return path


def clear_parent_inverse(ob):
    ob.matrix_parent_inverse = Matrix.Identity(4)
    return ob
