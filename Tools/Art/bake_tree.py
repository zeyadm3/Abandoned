"""Blender script (run by fetch_nature.py): turns a Poly Haven scanned tree into a game tree.

    Blender --background --factory-startup --python bake_tree.py -- <tree.fbx> <textures dir> <out dir> <id> <spec json>

Scanned trees are millions of polygons, almost all of them leaf or needle cards (fir_tree_01: 91,639 cards of ~2.5
faces each). Collapsing those cards only deletes needles, so each LOD keeps a random share of the cards and scales
every kept card up about its own centre so the crown keeps its cover. The trunk and branches are decimated. The last
LOD is an impostor: two crossed quads carrying an orthographic render of the tree (front and side), lit in Unity.

Writes <out>/<id>.fbx with one empty per variant holding <variant>_LOD0.._LOD2, and next to it the textures the
Unity side reads by material name (ShowcaseNature): <material>_diff (.png with alpha for cards, .jpg otherwise),
<material>_nor_gl.jpg and <material>_mask.png (R metal, G occlusion, A smoothness).
"""
import json, math, os, random, sys

import bmesh
import bpy
import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from bake_common import find, is_card, pack_material, pixels, save  # noqa: E402

fbx, texdir, outdir, model_id, spec = sys.argv[sys.argv.index("--") + 1:][:5]
spec = json.loads(spec)
LOD_CARDS = spec.get("lod_cards", [60000, 14000])        # card faces kept per variant at LOD0, LOD1
LOD_WOOD = spec.get("lod_wood", [0.25, 0.08])            # decimate ratio for trunk and branches
LOD_GROW = spec.get("lod_grow", [3.0, 4.5])              # most a kept card may grow at LOD0, LOD1
IMPOSTOR = spec.get("impostor", 512)                     # px per view (each view is IMPOSTOR x 2*IMPOSTOR)
os.makedirs(outdir, exist_ok=True)
random.seed(7)


def islands(bm, face_ok):
    seen, out = set(), []
    for f in bm.faces:
        if f.index in seen or not face_ok(f):
            continue
        group, stack = [], [f]
        seen.add(f.index)
        while stack:
            g = stack.pop()
            group.append(g)
            for e in g.edges:
                for h in e.link_faces:
                    if h.index not in seen and face_ok(h):
                        seen.add(h.index)
                        stack.append(h)
        out.append(group)
    return out


def make_lod(source, name, card_faces, wood_ratio, grow_cap):
    """A copy of <source> keeping about <card_faces> card faces (cards scaled up to keep cover) and decimated wood."""
    obj = source.copy()
    obj.data = source.data.copy()
    obj.name = name
    bpy.context.scene.collection.objects.link(obj)
    card_slots = {i for i, m in enumerate(obj.data.materials) if is_card(m)}
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    bm.faces.ensure_lookup_table()
    cards = islands(bm, lambda f: f.material_index in card_slots)
    total = sum(len(c) for c in cards)
    keep = min(1.0, card_faces / total) if total else 1.0
    # Up close a card grown past ~3x reads as a paddle; LOD1 is only seen from afar and may grow more.
    grow = min(1.0 / math.sqrt(keep), grow_cap) if 0.0 < keep < 1.0 else 1.0
    doomed = []
    for c in cards:
        if random.random() < keep:
            verts = list({v for f in c for v in f.verts})
            centre = sum((v.co for v in verts[1:]), verts[0].co.copy()) / len(verts)
            for v in verts:
                v.co = centre + (v.co - centre) * grow
        else:
            doomed.extend(c)
    bmesh.ops.delete(bm, geom=doomed, context="FACES")
    bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context="VERTS")
    bm.to_mesh(obj.data)
    bm.free()
    if wood_ratio < 1.0 and len(card_slots) < len(obj.data.materials):
        # Decimate the wood on its own (the modifier's ratio counts every face of the object it runs on), then join.
        bpy.ops.object.select_all(action="DESELECT")
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="DESELECT")
        bpy.ops.object.mode_set(mode="OBJECT")
        for p in obj.data.polygons:
            p.select = p.material_index not in card_slots
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.separate(type="SELECTED")
        bpy.ops.object.mode_set(mode="OBJECT")
        wood = [o for o in bpy.context.selected_objects if o is not obj][0]
        mod = wood.modifiers.new("dec", "DECIMATE")
        mod.decimate_type = "COLLAPSE"
        mod.ratio = wood_ratio
        mod.use_collapse_triangulate = True
        bpy.context.view_layer.objects.active = wood
        bpy.ops.object.modifier_apply(modifier="dec")
        bpy.ops.object.select_all(action="DESELECT")
        wood.select_set(True)
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.join()
    print(f"  {name}: {len(obj.data.polygons)} faces (cards kept {keep:.3f}, grown x{grow:.2f})", flush=True)
    return obj


def unlit(material):
    """An emission copy of a material for the impostor render: albedo only, alpha clipped."""
    m = bpy.data.materials.new(material.name + "_unlit")
    m.use_nodes = True
    nt = m.node_tree
    nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    mix = nt.nodes.new("ShaderNodeMixShader")
    emit = nt.nodes.new("ShaderNodeEmission")
    clear = nt.nodes.new("ShaderNodeBsdfTransparent")
    nt.links.new(mix.outputs[0], out.inputs[0])
    nt.links.new(clear.outputs[0], mix.inputs[1])
    nt.links.new(emit.outputs[0], mix.inputs[2])
    mix.inputs[0].default_value = 1.0
    for ext in (".png", ".jpg"):
        path = os.path.join(outdir, material.name + "_diff" + ext)
        if not os.path.exists(path):
            continue
        tex = nt.nodes.new("ShaderNodeTexImage")
        tex.image = bpy.data.images.load(path)
        nt.links.new(tex.outputs[0], emit.inputs[0])
        if ext == ".png":
            gt = nt.nodes.new("ShaderNodeMath")
            gt.operation = "GREATER_THAN"
            gt.inputs[1].default_value = 0.5
            nt.links.new(tex.outputs[1], gt.inputs[0])
            nt.links.new(gt.outputs[0], mix.inputs[0])
        break
    m.surface_render_method = "DITHERED"
    return m


def impostor(lod0, variant):
    """Renders lod0 front and side into one atlas and returns two crossed quads using it."""
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.render.film_transparent = True
    scene.render.resolution_x = IMPOSTOR
    scene.render.resolution_y = IMPOSTOR * 2
    scene.view_settings.view_transform = "Standard"
    scene.eevee.taa_render_samples = 16
    world = bpy.data.worlds.new("black")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[1].default_value = 0.0
    scene.world = world
    hidden = [o for o in scene.objects if o is not lod0 and o.type == "MESH" and not o.hide_render]
    for o in hidden:
        o.hide_render = True
    originals = list(lod0.data.materials)
    for i, m in enumerate(originals):
        lod0.data.materials[i] = unlit(m) if m else None
    co = np.empty(len(lod0.data.vertices) * 3, dtype=np.float32)
    lod0.data.vertices.foreach_get("co", co)
    co = co.reshape(-1, 3) + np.array(lod0.location)
    lo, hi = co.min(axis=0), co.max(axis=0)
    half = max(hi[0] - lo[0], hi[1] - lo[1], (hi[2] - lo[2]) / 2) / 2
    cx, cy, z0 = (hi[0] + lo[0]) / 2, (hi[1] + lo[1]) / 2, lo[2]
    cam = bpy.data.objects.new("ImpostorCam", bpy.data.cameras.new("ImpostorCam"))
    scene.collection.objects.link(cam)
    cam.data.type = "ORTHO"
    cam.data.ortho_scale = half * 4  # the frame is 2*half wide and 4*half high
    cam.data.clip_end = 400
    scene.camera = cam
    views = []
    for k, (dx, dy, rz) in enumerate([(0, -1, 0), (1, 0, math.pi / 2)]):
        cam.location = (cx + dx * 100, cy + dy * 100, z0 + half * 2)
        cam.rotation_euler = (math.pi / 2, 0, rz)
        path = os.path.join(outdir, f"_{variant}_view{k}.png")
        scene.render.filepath = path
        bpy.ops.render.render(write_still=True)
        _, px, w, h = pixels(path, False)
        views.append(px.reshape(h, w, 4))
        os.remove(path)
    for i, m in enumerate(originals):
        lod0.data.materials[i] = m
    for o in hidden:
        o.hide_render = False
    # The views side by side; colour bled into the clear pixels so mips don't halo.
    atlas = np.concatenate(views, axis=1)
    print(f"  impostor {variant}: covered {float((atlas[:, :, 3] > .35).mean()):.2f}, mean colour {atlas[atlas[:, :, 3] > .35][:, :3].mean(axis=0)}", flush=True)
    rgb, a = atlas[:, :, :3], atlas[:, :, 3:4]
    mean = (rgb * a).sum(axis=(0, 1)) / max(1e-5, float(a.sum()))
    atlas[:, :, :3] = np.where(a > 0.01, rgb / np.maximum(a, 1e-3), mean)
    atlas[:, :, 3] = (a[:, :, 0] > 0.35).astype(np.float32)
    name = f"{variant}_impostor"
    save(outdir, name + "_diff", atlas.reshape(-1, 4), IMPOSTOR * 2, IMPOSTOR * 2, True, False, "PNG", 4096)
    # Two quads through the trunk, each mapped to its half of the atlas.
    mesh = bpy.data.meshes.new(f"{variant}_LOD2")
    verts, faces, uvs = [], [], []
    for k, (ax, ay) in enumerate([(1, 0), (0, 1)]):
        b = len(verts)
        for (s, t) in [(-1, 0), (1, 0), (1, 1), (-1, 1)]:
            verts.append((cx + ax * s * half, cy + ay * s * half, z0 + t * half * 4))
            uvs.append((0.5 * k + (s + 1) / 4, t))
        faces.append((b, b + 1, b + 2, b + 3))
    mesh.from_pydata(verts, [], faces)
    layer = mesh.uv_layers.new(name="UVMap")
    for loop in mesh.loops:
        layer.data[loop.index].uv = uvs[loop.vertex_index]
    mesh.materials.append(bpy.data.materials.new(name))
    obj = bpy.data.objects.new(f"{variant}_LOD2", mesh)
    obj.location = (0, 0, 0)
    scene.collection.objects.link(obj)
    bpy.data.objects.remove(cam)
    return obj


bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=fbx)
for o in list(bpy.data.objects):
    if o.type != "MESH":
        bpy.data.objects.remove(o)
sources = [o for o in bpy.data.objects if o.type == "MESH"]
bpy.ops.object.select_all(action="DESELECT")
for o in sources:
    o.select_set(True)
bpy.context.view_layer.objects.active = sources[0]
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

for m in {m for o in sources for m in o.data.materials if m}:
    pack_material(m, texdir, outdir)

for i, src in enumerate(sources):
    variant = src.name.replace("_LOD0", "")
    src.name = variant + "_scan"  # frees the name <variant>_LOD0 for the copy
    # The variant's trunk base to the origin: its lowest point, centred on the trunk's bounds at ground level.
    co = np.empty(len(src.data.vertices) * 3, dtype=np.float32)
    src.data.vertices.foreach_get("co", co)
    co = co.reshape(-1, 3)
    base = co[co[:, 2] < co[:, 2].min() + 0.5]
    offset = np.array([(base[:, 0].min() + base[:, 0].max()) / 2, (base[:, 1].min() + base[:, 1].max()) / 2, co[:, 2].min()])
    co -= offset
    src.data.vertices.foreach_set("co", co.ravel())
    src.data.update()
    print(variant, len(src.data.polygons), "faces", flush=True)
    root = bpy.data.objects.new(variant, None)
    bpy.context.scene.collection.objects.link(root)
    lods = [make_lod(src, f"{variant}_LOD{k}", LOD_CARDS[k], LOD_WOOD[k], LOD_GROW[k]) for k in range(2)]
    lods.append(impostor(lods[0], variant))
    root.location = (i * 20.0, 0, 0)
    for lod in lods:
        lod.parent = root
        lod.location = (0, 0, 0)
    bpy.data.objects.remove(src)

bpy.ops.object.select_all(action="SELECT")
bpy.ops.export_scene.fbx(filepath=os.path.join(outdir, model_id + ".fbx"), use_selection=False, object_types={"MESH", "EMPTY"},
                         apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y",
                         bake_space_transform=True, mesh_smooth_type="FACE", add_leaf_bones=False,
                         path_mode="STRIP", embed_textures=False)
print("WROTE", model_id, flush=True)
