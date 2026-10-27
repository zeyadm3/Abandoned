"""Turn a crew-frame assembly (parts parented to WorkerRig-named pivots) into a skinned game mesh.

1. merge(): every part's evaluated mesh (modifiers applied) in rest pose, turned 180 degrees so the model faces +Y
   (the remake frame), merged into one object; each vertex remembers the pivot its part hung from.
2. weight(): each vertex is weighted to its pivot's bone and that bone's neighbours (parent, children) by distance
   to the bone segments, so elbows, knees, shoulders and hips bend smoothly instead of splitting.
3. game_mesh(): a decimated copy at the triangle budget (UVs kept) that the high mesh is baked onto.
"""
import math

import bmesh
import bpy
import numpy as np
from mathutils import Matrix, Vector

from ..core import kit, rig

TURN = Matrix.Rotation(math.pi, 4, 'Z')
# Crew pivot names -> skeleton bones (HeadAnchor rides on the head).
PIVOT_BONE = {'Body': 'Body', 'Torso': 'Torso', 'Head': 'Head', 'HeadAnchor': 'Head', 'ArmL': 'ArmL', 'ArmR': 'ArmR',
              'ElbowL': 'ElbowL', 'ElbowR': 'ElbowR', 'LegL': 'LegL', 'LegR': 'LegR', 'KneeL': 'KneeL', 'KneeR': 'KneeR'}
BONES = list(rig.CREW.keys())


def _joint(ob):
    p = ob.parent
    while p is not None and p.name.split('.')[0] not in PIVOT_BONE:
        p = p.parent
    return PIVOT_BONE[p.name.split('.')[0]] if p is not None else 'Body'


def merge(objs, name, skip=()):
    """One mesh (remake frame) from crew parts; attribute 'bone' holds the part's bone index per vertex."""
    dg = bpy.context.evaluated_depsgraph_get()
    bm = bmesh.new()
    layer = bm.verts.layers.int.new('bone')
    mats = []
    for ob in objs:
        if ob.type not in ('MESH', 'CURVE', 'FONT') or ob.name.split('.')[0] in skip:
            continue
        ev = ob.evaluated_get(dg)
        me = ev.to_mesh()
        m = TURN @ ob.matrix_world
        me.transform(m)
        if m.determinant() < 0:
            me.flip_normals()
        remap = []
        for slot in ob.material_slots:
            if slot.material not in mats:
                mats.append(slot.material)
            remap.append(mats.index(slot.material))
        for p in me.polygons:
            p.material_index = remap[p.material_index] if remap else 0
        if not me.uv_layers:
            me.uv_layers.new(name='UVMap')
        start = len(bm.verts)
        bm.from_mesh(me)
        bm.verts.ensure_lookup_table()
        b = BONES.index(_joint(ob))
        for v in bm.verts[start:]:
            v[layer] = b
        ev.to_mesh_clear()
    out = kit.from_bmesh(name, bm)
    for m in mats:
        out.data.materials.append(m)
    return out


def _neighbours(bone):
    out = {bone}
    parent = rig.CREW[bone][2]
    if parent:
        out.add(parent)
    for b, (_, _, p) in rig.CREW.items():
        if p == bone:
            out.add(b)
    return out


def weight(ob, falloff=.05, rigid=()):
    """Distance-to-segment weights limited to the part's bone and its neighbours; `rigid` bones never blend
    (the head: a hat or face must not smear into the neck)."""
    segs = {b: (Vector(h), Vector(t)) for b, (h, t, _) in rig.CREW.items()}
    for b in BONES:
        if ob.vertex_groups.get(b) is None:
            ob.vertex_groups.new(name=b)
    attr = ob.data.attributes['bone'].data
    own = np.empty(len(attr), np.int32)
    attr.foreach_get('value', own)
    groups = {b: ([], []) for b in BONES}
    for v, bi in zip(ob.data.vertices, own):
        bone = BONES[bi]
        p = v.co
        if bone in rigid:
            groups[bone][0].append(v.index)
            groups[bone][1].append(1.0)
            continue
        cands = []
        for b in _neighbours(bone):
            h, t = segs[b]
            d = t - h
            k = max(0.0, min(1.0, (p - h).dot(d) / max(d.length_squared, 1e-9)))
            cands.append((b, (p - (h + d * k)).length))
        best = min(c[1] for c in cands)
        ws = [(b, 1.0 / (1.0 + ((dist - best) / falloff) ** 2) ** 2) for b, dist in cands]
        # The part's own bone always dominates near its middle.
        ws = [(b, w * (1.6 if b == bone else 1.0)) for b, w in ws]
        tot = sum(w for _, w in ws)
        for b, w in ws:
            if w / tot > .02:
                groups[b][0].append(v.index)
                groups[b][1].append(w / tot)
    for b, (idx, ws) in groups.items():
        vg = ob.vertex_groups[b]
        for i, w in zip(idx, ws):
            vg.add([i], w, 'REPLACE')
    return ob


def blend_neck(ob, low=1.46, high=1.56):
    """Head meshes: the neck's lower part follows the torso, fading into the head above `low`."""
    head, torso = ob.vertex_groups['Head'], ob.vertex_groups['Torso']
    for v in ob.data.vertices:
        z = v.co.z
        t = min(1.0, max(0.0, (z - low) / (high - low)))
        t = t * t * (3 - 2 * t)
        head.add([v.index], t, 'REPLACE')
        torso.add([v.index], 1 - t, 'REPLACE')


def game_mesh(high, budget, name):
    """Decimated copy for the game (UV seams and material borders kept); `high` stays as the bake source."""
    low = high.copy()
    low.data = high.data.copy()
    low.name = name
    kit.link(low)
    kit.decimate_to(low, budget, delimit=('UV', 'MATERIAL'))
    return low


def bind(meshes, armature):
    for me in meshes:
        me.parent = armature
        me.matrix_parent_inverse = Matrix.Identity(4)
        mod = me.modifiers.get('Armature') or me.modifiers.new('Armature', 'ARMATURE')
        mod.object = armature


def copy_weights(src, dst):
    """Transfer vertex weights from the high mesh onto its decimated copy (nearest vertex)."""
    for vg in src.vertex_groups:
        if dst.vertex_groups.get(vg.name) is None:
            dst.vertex_groups.new(name=vg.name)
    m = dst.modifiers.new('Weights', 'DATA_TRANSFER')
    m.object = src
    m.use_vert_data = True
    m.data_types_verts = {'VGROUP_WEIGHTS'}
    m.vert_mapping = 'NEAREST'
    m.layers_vgroup_select_src = 'ALL'
    m.layers_vgroup_select_dst = 'NAME'
    kit.apply(dst)
