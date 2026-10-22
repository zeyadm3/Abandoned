"""Blender script (run by fetch_nature.py): a Poly Haven ground scan's maps for a Unity terrain layer.

    Blender --background --factory-startup --python bake_ground.py -- <raw dir> <out dir> <id>

Writes <id>_diff.jpg, <id>_nor_gl.jpg and <id>_mask.png (R metal, G occlusion, B 0, A smoothness), see bake_common.py.
"""
import os
import sys

import bpy

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from bake_common import pack_material  # noqa: E402

raw, outdir, tid = sys.argv[sys.argv.index("--") + 1:][:3]
os.makedirs(outdir, exist_ok=True)
pack_material(bpy.data.materials.new(tid), raw, outdir)
