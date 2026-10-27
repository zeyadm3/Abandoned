"""Remaster every loot item (2026 remake): its current in-game look, exported by Unity (Editor/Remake/LootExport) in its
prefab's own space, becomes a realistic baked model at exactly the same size and place.

Per item: import the OBJ, weld and clean it, give hard edges real bevels (scaled to the item), classify each material
by its name, colour and texture into a worn physical surface (wood, metal, glass, plastic, fabric, ceramic, paper,
stone, rubber, screen...), keep its colour or texture as the base, bake one texture set (512 to 1024 by size) with
edge wear and crevice grime, decimate to a budget by carry class, and export with LODs.

Batch:
  Blender --background --factory-startup --python Tools/Blender/remake/loot/remaster.py -- [ids...] [--preview]
Reads Game/Logs/remake/loot/manifest.json; writes Art/Remake/Loot/Loot_<id>.fbx and Textures/Loot_<id>_*.png.
"""
import json
import math
import os
import re
import sys

import bpy
from mathutils import Matrix

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(os.path.dirname(_HERE)))
exec(open(os.path.join(os.path.dirname(_HERE), 'boot.py')).read())

from remake.core import bake, export, kit, materials, preview  # noqa: E402

REPO = os.path.abspath(os.path.join(_HERE, '..', '..', '..', '..'))
SRC = os.path.join(REPO, 'Game', 'Logs', 'remake', 'loot')
BUDGET = {'OneHand': 2500, 'TwoHand': 4500, 'Heavy': 7000, 'Huge': 9000}

KINDS = [  # (pattern on material name, kind)
    (r'glass|lens|window|bottle(?!cap)|crystal', 'glass'),
    (r'screen|display|monitor_face|lcd|crt', 'screen'),
    (r'emit|lamp_on|bulb|neon|glow', 'emit'),
    (r'chrome|mirror', 'chrome'),
    (r'gold|brass|bronze|copper', 'brass'),
    (r'steel|metal|iron|alum|silver|tin|wire|rail|frame|bolt|knob', 'metal'),
    (r'rust', 'rust'),
    (r'wood|oak|pine|walnut|timber|plank|veneer', 'wood'),
    (r'fabric|cloth|cushion|textile|canvas|linen|carpet|rug|uphol|felt|velvet|wool|sofa', 'fabric'),
    (r'leather', 'leather'),
    (r'rubber|tyre|tire|grip', 'rubber'),
    (r'ceram|porcel|china|tile|enamel|vase', 'ceramic'),
    (r'paper|card|book|page|label|box', 'paper'),
    (r'stone|marble|granite|concrete|plaster|statue', 'stone'),
    (r'plastic|pvc|vinyl|casing|shell', 'plastic'),
]


def classify(name, colour, metallic):
    low = name.lower()
    for pat, kind in KINDS:
        if re.search(pat, low):
            return kind
    if metallic > .5:
        return 'metal'
    return 'paint'


def surface_for(kind, colour, key):
    """A worn physical surface of the item's colour (colour in linear space)."""
    c = tuple(max(.02, min(.95, x)) for x in colour[:3])
    S = materials.surface
    if kind == 'glass':
        return materials.library()['Glass']
    if kind == 'screen':
        return materials.library()['Screen']
    if kind == 'emit':
        return materials.library()['EmitWarm']
    table = {
        'chrome': lambda: S(key, (.82, .82, .83), .12, 1, var=.02, grime=.3, bump=.02),
        'brass': lambda: S(key, c if sum(c) > .9 else (.72, .52, .24), .35, 1, var=.12, grime=.5, wear=.3),
        'metal': lambda: S(key, c, .42, 1, var=.08, grime=.45, wear=.35, bump=.08),
        'rust': lambda: S(key, c, .8, .5, var=.2, rust=.7, grime=.5, bump=.3),
        'wood': lambda: S(key, c, .62, 0, var=.28, scale=3, grime=.45, bump=.25, wear=.25),
        'fabric': lambda: materials.fabric(key, c, .9, 380, grime=.45),
        'leather': lambda: S(key, c, .55, 0, var=.18, wear=.4, grime=.35, bump=.35, bump_scale=180),
        'rubber': lambda: S(key, c, .85, 0, var=.06, grime=.35, bump=.2, bump_scale=200),
        'ceramic': lambda: S(key, c, .18, 0, var=.05, grime=.45, coat=.7, bump=.02),
        'paper': lambda: S(key, c, .88, 0, var=.12, grime=.4, bump=.12),
        'stone': lambda: S(key, c, .8, 0, var=.22, scale=3, grime=.5, bump=.45, bump_scale=60),
        'plastic': lambda: S(key, c, .45, 0, var=.06, grime=.4, wear=.25, bump=.04),
        'paint': lambda: S(key, c, .5, 0, var=.1, grime=.45, wear=.4, wear_color=(.4, .4, .41), bump=.05),
    }
    return table[kind]()


def textured_surface(key, texture, kind):
    """Keep an item's own picture (a painting, a label, a pattern) and add wear and grime around it."""
    mat = bpy.data.materials.new('RM_' + key)
    mat.use_nodes = True
    nt = mat.node_tree
    bsdf = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
    tex = nt.nodes.new('ShaderNodeTexImage')
    try:
        tex.image = bpy.data.images.load(texture, check_existing=True)
    except RuntimeError:
        return None
    uv = nt.nodes.new('ShaderNodeTexCoord')
    nt.links.new(uv.outputs['UV'], tex.inputs['Vector'])
    ao = nt.nodes.new('ShaderNodeAmbientOcclusion')
    ao.inputs['Distance'].default_value = .06
    mix = nt.nodes.new('ShaderNodeMix')
    mix.data_type = 'RGBA'
    mix.blend_type = 'MULTIPLY'
    mix.inputs['Factor'].default_value = .6
    nt.links.new(tex.outputs['Color'], mix.inputs['A'])
    nt.links.new(ao.outputs['AO'], mix.inputs['B'])
    nt.links.new(mix.outputs['Result'], bsdf.inputs['Base Color'])
    bsdf.inputs['Roughness'].default_value = .2 if kind in ('ceramic', 'glass') else .55
    bsdf.inputs['Metallic'].default_value = 1.0 if kind in ('metal', 'brass', 'chrome') else 0.0
    return mat


def srgb_to_linear(c):
    return tuple(x / 12.92 if x <= .04045 else ((x + .055) / 1.055) ** 2.4 for x in c[:3])


def read_obj(path, name):
    """Our own OBJ (LootExport): Unity space mirrored in X. Map it straight to the remake frame:
    Blender (x, y, z) = Unity (x, z, y) = OBJ (-x, z, y), a proper rotation, so the windings stay right."""
    import bmesh
    bm = bmesh.new()
    uv_layer = bm.loops.layers.uv.new('UVMap')
    verts, uvs, mats = [], [], []
    current = 0
    for line in open(path):
        if line.startswith('v '):
            _, x, y, z = line.split()
            verts.append(bm.verts.new((-float(x), float(z), float(y))))
        elif line.startswith('vt '):
            _, u, v = line.split()[:3]
            uvs.append((float(u), float(v)))
        elif line.startswith('usemtl '):
            key = line.split(None, 1)[1].strip()
            if key not in mats:
                mats.append(key)
            current = mats.index(key)
        elif line.startswith('f '):
            idx = [int(p.split('/')[0]) - 1 for p in line.split()[1:]]
            try:
                f = bm.faces.new([verts[i] for i in idx])
            except ValueError:
                continue
            f.material_index = current
            for loop, i in zip(f.loops, idx):
                if i < len(uvs):
                    loop[uv_layer].uv = uvs[i]
    if not bm.faces:
        bm.free()
        return None
    ob = kit.from_bmesh(name, bm)
    for key in mats:
        ob.data.materials.append(bpy.data.materials.get(key) or bpy.data.materials.new(key))
    return ob


def build(entry, show=False):
    kit.reset()
    path = os.path.join(SRC, entry['obj'])
    ob = read_obj(path, 'Loot_' + entry['id'])
    if ob is None:
        print('[remake-loot] skip (no mesh)', entry['id'])
        return
    d = ob.dimensions
    sz = entry['size']
    print(f'[remake-loot] {entry["id"]} dims x{d.x:.2f} y{d.y:.2f} z{d.z:.2f} vs size x{sz["x"]:.2f} y{sz["z"]:.2f} z{sz["y"]:.2f}')
    # Clean: weld, recompute normals, smooth with hard edges.
    import bmesh
    bm = bmesh.new()
    bm.from_mesh(ob.data)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=.0004)
    bm.to_mesh(ob.data)
    bm.free()
    kit.auto_smooth(ob, 40)
    size = max(entry['size']['x'], entry['size']['y'], entry['size']['z'])
    # Low-poly round shapes (vases, bottles, helmets) get one creased subdivision: curves smooth out, hard edges
    # (marked by angle) stay crisp.
    if len(ob.data.polygons) < 1500:
        import bmesh
        bm = bmesh.new()
        bm.from_mesh(ob.data)
        crease = bm.edges.layers.float.get('crease_edge') or bm.edges.layers.float.new('crease_edge')
        for e in bm.edges:
            if len(e.link_faces) != 2 or e.calc_face_angle(0) > math.radians(40):
                e[crease] = 1.0
        bm.to_mesh(ob.data)
        bm.free()
        sub = ob.modifiers.new('Sub', 'SUBSURF')
        sub.levels = sub.render_levels = 1
        sub.uv_smooth = 'PRESERVE_CORNERS'
        kit.apply(ob)
    # Real bevels on hard edges, scaled to the item (2-8 mm), only where the mesh is light enough.
    if len(ob.data.polygons) < 20000:
        b = ob.modifiers.new('Bevel', 'BEVEL')
        b.width = max(.0015, min(.008, size * .012))
        b.segments, b.limit_method, b.angle_limit = 2, 'ANGLE', math.radians(35)
        b.harden_normals = True
        b.use_clamp_overlap = True
        kit.apply(ob)
    # Materials: each original slot becomes a worn physical surface (or keeps its picture).
    info = {m['name']: m for m in entry['mats']}
    for i, slot in enumerate(ob.material_slots):
        name = slot.material.name.split('.')[0] if slot.material else 'Default'
        m = info.get(name, {'color': {'r': .5, 'g': .5, 'b': .5}, 'metallic': 0, 'texture': None})
        col = srgb_to_linear((m['color']['r'], m['color']['g'], m['color']['b']))
        kind = classify(name, col, m.get('metallic', 0))
        key = f'L{entry["id"]}_{i}'
        tex = m.get('texture')
        new = textured_surface(key, tex, kind) if tex and os.path.exists(tex) and kind not in ('glass', 'screen', 'emit') else None
        slot.material = new or surface_for(kind, col, key)
    kit.decimate_to(ob, BUDGET.get(entry['carry'], 4000), delimit=('UV', 'MATERIAL', 'NORMAL'))
    tex_size = 1024 if size > .8 else 512
    bake.bake_asset(ob, 'Loot_' + entry['id'], export.art_dir('Loot', 'Textures'), size=tex_size, samples=8)
    export.static_asset('Loot_' + entry['id'], [ob], 'Loot', lods=(1.0, .45, .15))
    if show:
        preview.save('Loot_' + entry['id'])
    print(f'[remake-loot] {entry["id"]} tris={kit.tris(ob)}')


def main():
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    show = '--preview' in args
    only = [a for a in args if not a.startswith('--')]
    manifest = json.load(open(os.path.join(SRC, 'manifest.json')))
    for entry in manifest['items']:
        if only and entry['id'] not in only:
            continue
        try:
            build(entry, show)
        except Exception as e:  # noqa: BLE001 - one bad item must not stop the other 238
            print(f'[remake-loot] FAILED {entry["id"]}: {e}')


if __name__ == '__main__':
    main()
