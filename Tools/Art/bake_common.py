"""Shared by the Blender scripts bake_tree.py and bake_kit.py: finds a Poly Haven material's maps and writes the three
the Unity side reads by material name (Editor/TestMap/ShowcaseNature.cs):

    <material>_diff     colour; .png with the alpha map in A for leaves, needles, wire and grass, .jpg otherwise
    <material>_nor_gl   OpenGL normal map, .jpg
    <material>_mask     R metal, G occlusion, B 0, A smoothness (Poly Haven's ARM map rearranged for URP Lit), .png
"""
import os

import bpy
import numpy as np

CARD_WORDS = ("twig", "leaves", "leaf", "grass", "fern", "nettle", "weed", "shrub", "moss", "wire", "needle")


def is_card(material):
    return material is not None and any(w in material.name.lower() for w in CARD_WORDS)


def find(texdir, base, kind):
    """The texture file for material <base> and map <kind> (diff, alpha, nor_gl, arm), any resolution, never EXR."""
    want = f"{base}_{kind}_".lower()
    for f in sorted(os.listdir(texdir)):
        if f.lower().startswith(want) and not f.lower().endswith(".exr"):
            return os.path.join(texdir, f)
    return None


def pixels(path, non_color):
    img = bpy.data.images.load(path)
    if non_color:
        img.colorspace_settings.name = "Non-Color"
    w, h = img.size
    a = np.empty(w * h * 4, dtype=np.float32)
    img.pixels.foreach_get(a)
    return img, a.reshape(-1, 4), w, h


def save(outdir, name, data, w, h, alpha, non_color, fmt, size):
    out = bpy.data.images.new(name, w, h, alpha=alpha, float_buffer=False)
    if non_color:
        out.colorspace_settings.name = "Non-Color"
    out.alpha_mode = "STRAIGHT"
    out.pixels.foreach_set(np.ascontiguousarray(data, dtype=np.float32).ravel())
    if w > size:
        out.scale(size, h * size // w)
    path = os.path.join(outdir, name + (".png" if fmt == "PNG" else ".jpg"))
    out.filepath_raw = path
    out.file_format = fmt
    out.save(quality=92) if fmt == "JPEG" else out.save()
    bpy.data.images.remove(out)
    return path


def pack_material(material, texdir, outdir, size=2048, alias=None):
    """Writes the three maps for one material (named for the material); returns True if it is alpha-tested.
    <alias> is where to look when no map carries the material's name (a model with one material whose maps are
    named after the model, e.g. old_military_crate's military_crate_m_01)."""
    base = material.name
    look = base if find(texdir, base, "diff") or not alias else alias
    diff = find(texdir, look, "diff")
    alpha = find(texdir, look, "alpha") or find(texdir, look, "opacity")
    nor, arm = find(texdir, look, "nor_gl"), find(texdir, look, "arm")
    cut = alpha is not None
    if diff:
        img, d, w, h = pixels(diff, False)
        if alpha:
            aimg, a, aw, ah = pixels(alpha, True)
            if (aw, ah) != (w, h):
                aimg.scale(w, h)
                a = np.empty(w * h * 4, dtype=np.float32)
                aimg.pixels.foreach_get(a)
                a = a.reshape(-1, 4)
            d[:, 3] = a[:, 0]
            bpy.data.images.remove(aimg)
        save(outdir, base + "_diff", d, w, h, cut, False, "PNG" if cut else "JPEG", size)
        bpy.data.images.remove(img)
    if nor:
        img, n, w, h = pixels(nor, True)
        save(outdir, base + "_nor_gl", n, w, h, False, True, "JPEG", size)
        bpy.data.images.remove(img)
    if arm:
        img, a, w, h = pixels(arm, True)
        m = np.empty_like(a)
        m[:, 0], m[:, 1], m[:, 2], m[:, 3] = a[:, 2], a[:, 0], 0.0, 1.0 - a[:, 1]
        save(outdir, base + "_mask", m, w, h, True, True, "PNG", 1024)
        bpy.data.images.remove(img)
    print(f"  {base}: diff={'y' if diff else 'n'} alpha={'y' if cut else 'n'} nor={'y' if nor else 'n'} arm={'y' if arm else 'n'}", flush=True)
    return cut
