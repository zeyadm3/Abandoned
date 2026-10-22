"""Blender script (run by fetch_models.py): turns a Poly Haven ARM map (R = AO, G = roughness, B = metal) into the
mask URP Lit reads (R = metal, G = AO, B = 0, A = smoothness).

    Blender --background --factory-startup --python pack_mask.py -- <arm.jpg> <mask.png>
"""
import sys

import bpy
import numpy as np

source, target = sys.argv[sys.argv.index("--") + 1:][:2]
arm = bpy.data.images.load(source)
arm.colorspace_settings.name = "Non-Color"  # the stored values, not colour-managed ones
w, h = arm.size
pixels = np.empty(w * h * 4, dtype=np.float32)
arm.pixels.foreach_get(pixels)
pixels = pixels.reshape(-1, 4)

mask = np.empty_like(pixels)
mask[:, 0] = pixels[:, 2]
mask[:, 1] = pixels[:, 0]
mask[:, 2] = 0.0
mask[:, 3] = 1.0 - pixels[:, 1]

out = bpy.data.images.new("mask", w, h, alpha=True, float_buffer=False)
out.colorspace_settings.name = "Non-Color"
out.alpha_mode = "STRAIGHT"
out.pixels.foreach_set(mask.ravel())
# AO, metal and smoothness hold no fine detail: 1024 is what the scanned surfaces use too (ScannedSet.SupportMapSize).
if w > 1024:
    out.scale(1024, h * 1024 // w)
out.filepath_raw = target
out.file_format = "PNG"
out.save()
