"""Blender script (run by fetch_nature.py): packs the maps of every material in a Poly Haven model for Unity.

    Blender --background --factory-startup --python bake_kit.py -- <model.fbx> <textures dir> <out dir>

The FBX itself is copied unchanged by fetch_nature.py (Unity keeps its material names); only the maps are rewritten,
see bake_common.py.
"""
import os
import sys

import bpy

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from bake_common import pack_material  # noqa: E402

fbx, texdir, outdir = sys.argv[sys.argv.index("--") + 1:][:3]
os.makedirs(outdir, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=fbx)
materials = {m for o in bpy.data.objects if o.type == "MESH" for m in o.data.materials if m}
model_id = os.path.splitext(os.path.basename(fbx))[0]
for m in sorted(materials, key=lambda m: m.name):
    pack_material(m, texdir, outdir, alias=model_id if len(materials) == 1 else None)
print("PACKED", len(materials), flush=True)
