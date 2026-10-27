"""Hand finished models from batch Blender to the live Blender (for the MCP's `look`).

macOS keeps the live Blender out of ~/Documents (no Files & Folders permission), so batch scripts, which do have it,
save a .blend into ~/Library/Caches/AbandonedRemake/preview and the live Blender appends from there.

Batch:  preview.save('Crowbar')            -> ~/Library/Caches/AbandonedRemake/preview/Crowbar.blend
Live:   preview.show('Crowbar')            -> clears the live scene and appends that file's objects
"""
import os

import bpy

CACHE = os.path.expanduser('~/Library/Caches/AbandonedRemake/preview')


def save(name):
    os.makedirs(CACHE, exist_ok=True)
    path = os.path.join(CACHE, name + '.blend')
    bpy.ops.wm.save_as_mainfile(filepath=path, copy=True, compress=True)
    print('[remake-preview]', path)
    return path


def show(name, clear=True):
    from . import kit
    if clear:
        kit.reset()
    path = os.path.join(CACHE, name + '.blend')
    with bpy.data.libraries.load(path, link=False) as (src, dst):
        dst.objects = list(src.objects)
    for ob in dst.objects:
        if ob is not None and ob.name not in bpy.context.scene.collection.objects:
            bpy.context.scene.collection.objects.link(ob)
    return [o for o in dst.objects if o]
