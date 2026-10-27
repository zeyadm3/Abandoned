"""Flatten a model's procedural surfaces into one Unity texture set.

bake_asset(mesh, name, out_dir) -> unwraps the mesh (one atlas), bakes with Cycles (GPU/Metal):
  <name>_BaseColor.png  sRGB albedo
  <name>_Normal.png     tangent-space normal, OpenGL (+Y), which is Unity's convention
  <name>_Mask.png       R metallic, G ambient occlusion, B 0, A smoothness  (URP Lit: _MetallicGlossMap + _OcclusionMap)
and replaces the baked faces' materials with one "RM_<name>" material that uses the images (so a later export
carries the right name). Faces whose surface is in materials.SHARED keep their own material slot and are not baked.

Optionally bakes from a high-poly source (selected-to-active) so sculpted detail lands in the normal and AO maps.
"""
import math
import os

import bpy
import numpy as np

from . import materials as M

TEXEL_DENSITY = 512  # pixels per metre


def setup(samples=16):
    sc = bpy.context.scene
    try:
        sc.render.engine = 'CYCLES'
    except TypeError:
        pass
    sc.cycles.samples = samples
    sc.cycles.use_denoising = False
    try:
        prefs = bpy.context.preferences.addons['cycles'].preferences
        prefs.compute_device_type = 'METAL'
        prefs.get_devices()
        for d in prefs.devices:
            d.use = True
        sc.cycles.device = 'GPU'
    except Exception:  # noqa: BLE001 - CPU fallback is fine
        sc.cycles.device = 'CPU'
    sc.render.bake.margin = 8
    sc.render.bake.use_clear = True


def atlas_size(ob, density=TEXEL_DENSITY, lo=256, hi=2048):
    area = sum(p.area for p in ob.data.polygons) * (ob.matrix_world.to_scale().length / math.sqrt(3)) ** 2
    side = math.sqrt(max(area, 1e-6) / 6.0) * density * 2.2  # ~6 faces' worth per side, island padding
    size = 2 ** round(math.log2(max(lo, min(hi, side))))
    return int(max(lo, min(hi, size)))


def unwrap(ob, margin=.004):
    """One non-overlapping atlas UV (UVMap). Smart project keeps hard-surface islands straight."""
    me = ob.data
    while me.uv_layers:
        me.uv_layers.remove(me.uv_layers[0])
    me.uv_layers.new(name='UVMap')
    _select_only(ob)
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=math.radians(72), island_margin=margin, area_weight=1.0,
                             correct_aspect=True, scale_to_bounds=False)
    bpy.ops.uv.select_all(action='SELECT')
    bpy.ops.uv.pack_islands(rotate=True, scale=True, margin=margin)
    bpy.ops.object.mode_set(mode='OBJECT')


def _select_only(ob, also=()):
    bpy.context.view_layer.update()
    for o in bpy.context.view_layer.objects:
        if o is not None:
            o.select_set(False)
    for o in also:
        o.select_set(True)
    ob.select_set(True)
    bpy.context.view_layer.objects.active = ob


def _image(name, size, color=(0, 0, 0, 1), non_color=False):
    img = bpy.data.images.get(name)
    if img and tuple(img.size) != (size, size):
        bpy.data.images.remove(img)
        img = None
    if img is None:
        img = bpy.data.images.new(name, size, size, alpha=True, float_buffer=False)
    img.generated_color = color
    img.colorspace_settings.name = 'Non-Color' if non_color else 'sRGB'
    return img


def _baked_materials(ob):
    return [m for m in ob.data.materials if m and m.name.replace('RM_', '').split('.')[0] not in M.SHARED]


def _target_nodes(mats, img):
    """Every baked material gets an active image node pointing at img (Cycles writes into the active node)."""
    added = []
    for m in mats:
        n = m.node_tree.nodes.new('ShaderNodeTexImage')
        n.image = img
        m.node_tree.nodes.active = n
        added.append((m, n))
    return added


def _clear(added):
    for m, n in added:
        m.node_tree.nodes.remove(n)


def _bake(ob, kind, img, mats, high=None, **kw):
    added = _target_nodes(mats, img)
    sc = bpy.context.scene
    sc.render.bake.use_selected_to_active = high is not None
    if high is not None:
        sc.render.bake.cage_extrusion = kw.pop('cage', .02)
        sc.render.bake.max_ray_distance = kw.pop('ray', .05)
        _select_only(ob, also=high if isinstance(high, (list, tuple)) else [high])
    else:
        _select_only(ob)
    bpy.ops.object.bake(type=kind, **kw)
    _clear(added)


def _bake_metallic(ob, img, mats, high=None):
    """Cycles has no metallic pass: route each material's Metallic input to an emission output and bake EMIT."""
    swaps = []
    for m in mats:
        nt = m.node_tree
        bsdf = next((n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED'), None)
        out = next((n for n in nt.nodes if n.type == 'OUTPUT_MATERIAL'), None)
        if not bsdf or not out:
            continue
        old = out.inputs['Surface'].links[0].from_socket if out.inputs['Surface'].links else None
        em = nt.nodes.new('ShaderNodeEmission')
        src = bsdf.inputs['Metallic']
        if src.links:
            nt.links.new(src.links[0].from_socket, em.inputs['Color'])
        else:
            v = src.default_value
            em.inputs['Color'].default_value = (v, v, v, 1)
        nt.links.new(em.outputs['Emission'], out.inputs['Surface'])
        swaps.append((m, em, old, out))
    _bake(ob, 'EMIT', img, mats, high=high)
    for m, em, old, out in swaps:
        if old:
            m.node_tree.links.new(old, out.inputs['Surface'])
        m.node_tree.nodes.remove(em)


def _pixels(img):
    a = np.empty(len(img.pixels), dtype=np.float32)
    img.pixels.foreach_get(a)
    return a.reshape(-1, 4)


def _save(img, path):
    img.filepath_raw = path
    img.file_format = 'PNG'
    img.save()


def bake_asset(ob, name, out_dir, size=None, high=None, ao_distance=.15, samples=16, unwrap_first=True, high_all=False):
    """Bake ob's procedural surfaces into <name>_BaseColor/_Normal/_Mask under out_dir; returns the new material."""
    os.makedirs(out_dir, exist_ok=True)
    setup(samples)
    if unwrap_first:
        unwrap(ob)
    size = size or atlas_size(ob)
    mats = _baked_materials(ob)
    if high is not None:
        for h in (high if isinstance(high, (list, tuple)) else [high]):
            mats += [m for m in _baked_materials(h) if m not in mats]
    if not mats:
        return None
    sc = bpy.context.scene

    col = _image(name + '_BaseColor', size)
    sc.render.bake.use_pass_direct = False
    sc.render.bake.use_pass_indirect = False
    sc.render.bake.use_pass_color = True
    src = high if high_all else None
    _bake(ob, 'DIFFUSE', col, mats, high=src)

    nor = _image(name + '_Normal', size, (.5, .5, 1, 1), non_color=True)
    sc.render.bake.normal_space = 'TANGENT'
    _bake(ob, 'NORMAL', nor, mats, high=high)

    rough = _image(name + '_Rough', size, non_color=True)
    _bake(ob, 'ROUGHNESS', rough, mats, high=src)
    metal = _image(name + '_Metal', size, non_color=True)
    _bake_metallic(ob, metal, mats, high=src)
    ao = _image(name + '_AO', size, (1, 1, 1, 1), non_color=True)
    sc.world = sc.world or bpy.data.worlds.new('World')
    sc.world.light_settings.distance = ao_distance
    _bake(ob, 'AO', ao, mats, high=high)

    mask = _image(name + '_Mask', size, non_color=True)
    r, m, a = _pixels(rough), _pixels(metal), _pixels(ao)
    packed = np.stack([m[:, 0], a[:, 0], np.zeros(len(m), np.float32), 1.0 - r[:, 0]], axis=1)
    mask.pixels.foreach_set(packed.ravel())

    _save(col, os.path.join(out_dir, name + '_BaseColor.png'))
    _save(nor, os.path.join(out_dir, name + '_Normal.png'))
    _save(mask, os.path.join(out_dir, name + '_Mask.png'))
    for tmp in (rough, metal, ao):
        bpy.data.images.remove(tmp)

    final = _final_material(name, col, nor, mask)
    keep = {m.name for m in mats}
    for i, slot in enumerate(ob.material_slots):
        if slot.material and slot.material.name in keep:
            slot.material = final
    _merge_slots(ob)
    print(f'[remake-bake] {name} {size}px')
    return final


def _final_material(name, col, nor, mask):
    key = 'RM_' + name
    mat = bpy.data.materials.get(key) or bpy.data.materials.new(key)
    mat.use_nodes = True
    nt = mat.node_tree
    for n in list(nt.nodes):
        if n.type not in ('BSDF_PRINCIPLED', 'OUTPUT_MATERIAL'):
            nt.nodes.remove(n)
    bsdf = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
    tc = nt.nodes.new('ShaderNodeTexImage')
    tc.image = col
    nt.links.new(tc.outputs['Color'], bsdf.inputs['Base Color'])
    tn = nt.nodes.new('ShaderNodeTexImage')
    tn.image = nor
    nm = nt.nodes.new('ShaderNodeNormalMap')
    nt.links.new(tn.outputs['Color'], nm.inputs['Color'])
    nt.links.new(nm.outputs['Normal'], bsdf.inputs['Normal'])
    tm = nt.nodes.new('ShaderNodeTexImage')
    tm.image = mask
    sep = nt.nodes.new('ShaderNodeSeparateColor')
    nt.links.new(tm.outputs['Color'], sep.inputs['Color'])
    nt.links.new(sep.outputs['Red'], bsdf.inputs['Metallic'])
    inv = nt.nodes.new('ShaderNodeMath')
    inv.operation = 'SUBTRACT'
    inv.inputs[0].default_value = 1.0
    nt.links.new(tm.outputs['Alpha'], inv.inputs[1])
    nt.links.new(inv.outputs[0], bsdf.inputs['Roughness'])
    return mat


def _merge_slots(ob):
    """Collapse duplicate material slots (all baked surfaces now share one material)."""
    me = ob.data
    names, remap = [], []
    unique = []
    for m in me.materials:
        if m.name not in names:
            names.append(m.name)
            unique.append(m)
        remap.append(names.index(m.name))
    idx = np.empty(len(me.polygons), dtype=np.int32)
    me.polygons.foreach_get('material_index', idx)
    idx = np.array([remap[i] for i in idx], dtype=np.int32)
    me.materials.clear()
    for m in unique:
        me.materials.append(m)
    me.polygons.foreach_set('material_index', idx)
    me.update()
