"""Blender script (run by fetch_nature.py): renders scanned plant clumps side by side into one alpha card for Unity's
terrain grass (the waving grass billboards are cheap enough to cover a whole field; the scanned clumps are not).

    Blender --background --factory-startup --python bake_card.py -- <baked model dir> <model id> <out png> <part,part,...>

Reads the model and its packed <material>_diff.png from the baked folder, stands the named parts in a row, and
renders them from the front, orthographic, colour only (Unity lights the card). Colour is bled into the clear pixels
so the mips don't fringe.
"""
import math
import os
import sys

import bpy
import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from bake_common import pixels, save  # noqa: E402

folder, model_id, out_png, parts = sys.argv[sys.argv.index("--") + 1:][:4]
parts = parts.split(",")
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=os.path.join(folder, model_id + ".fbx"))
scene = bpy.context.scene
for o in list(bpy.data.objects):
    if o.type != "MESH" or o.name not in parts:
        bpy.data.objects.remove(o)
objs = [bpy.data.objects[p] for p in parts if p in bpy.data.objects]
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)


def bounds(o):
    co = np.array([o.matrix_world @ v.co for v in o.data.vertices])
    return co.min(axis=0), co.max(axis=0)


# A tight row along x, every clump standing on z = 0, overlapping by half so the card reads as one tuft.
x = 0.0
for o in objs:
    lo, hi = bounds(o)
    o.location.x += x - lo[0]
    o.location.y -= (lo[1] + hi[1]) / 2
    o.location.z -= lo[2]
    x += (hi[0] - lo[0]) * 0.45
bpy.context.view_layer.update()  # matrix_world follows the moves only after an update
lo = np.min([bounds(o)[0] for o in objs], axis=0)
hi = np.max([bounds(o)[1] for o in objs], axis=0)
width, height = hi[0] - lo[0], hi[2] - lo[2]

for m in {m for o in objs for m in o.data.materials if m}:
    m.use_nodes = True
    nt = m.node_tree
    nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    mix = nt.nodes.new("ShaderNodeMixShader")
    emit = nt.nodes.new("ShaderNodeEmission")
    clear = nt.nodes.new("ShaderNodeBsdfTransparent")
    tex = nt.nodes.new("ShaderNodeTexImage")
    tex.image = bpy.data.images.load(os.path.join(folder, m.name + "_diff.png"))
    gt = nt.nodes.new("ShaderNodeMath")
    gt.operation = "GREATER_THAN"
    gt.inputs[1].default_value = 0.5
    nt.links.new(tex.outputs[0], emit.inputs[0])
    nt.links.new(tex.outputs[1], gt.inputs[0])
    nt.links.new(gt.outputs[0], mix.inputs[0])
    nt.links.new(clear.outputs[0], mix.inputs[1])
    nt.links.new(emit.outputs[0], mix.inputs[2])
    nt.links.new(mix.outputs[0], out.inputs[0])
    m.surface_render_method = "DITHERED"

scene.render.engine = "BLENDER_EEVEE_NEXT"
scene.render.film_transparent = True
scene.view_settings.view_transform = "Standard"
scene.eevee.taa_render_samples = 16
scene.render.resolution_x = 1024
scene.render.resolution_y = max(64, int(1024 * height / width))
world = bpy.data.worlds.new("black")
world.use_nodes = True
world.node_tree.nodes["Background"].inputs[1].default_value = 0.0
scene.world = world
cam = bpy.data.objects.new("Cam", bpy.data.cameras.new("Cam"))
scene.collection.objects.link(cam)
cam.data.type = "ORTHO"
cam.data.ortho_scale = width
cam.location = ((lo[0] + hi[0]) / 2, -50, lo[2] + height / 2)
cam.rotation_euler = (math.pi / 2, 0, 0)
scene.camera = cam
raw = out_png + ".raw.png"
scene.render.filepath = raw
bpy.ops.render.render(write_still=True)
_, px, w, h = pixels(raw, False)
os.remove(raw)
a = px[:, 3:4]
mean = (px[:, :3] * a).sum(axis=0) / max(1e-5, float(a.sum()))
px[:, :3] = np.where(a > 0.01, px[:, :3] / np.maximum(a, 1e-3), mean)
px[:, 3] = (a[:, 0] > 0.35).astype(np.float32)
save(os.path.dirname(out_png), os.path.splitext(os.path.basename(out_png))[0], px, w, h, True, False, "PNG", 1024)
print("CARD", out_png, w, h, flush=True)
