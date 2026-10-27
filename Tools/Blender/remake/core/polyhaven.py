"""Poly Haven (CC0) models and textures, fetched by script so every asset can be regenerated in batch mode.

Uses Poly Haven's public API (https://api.polyhaven.com). Downloads are cached outside the repository in
~/Library/Caches/AbandonedRemake/polyhaven; only the cleaned, baked result is written into Art/Remake.
Every asset used must be listed in Docs/ASSET_CREDITS.md (credit_line() gives the line).
"""
import json
import os
import urllib.request

import bpy

from . import kit

API = 'https://api.polyhaven.com'
CACHE = os.path.expanduser('~/Library/Caches/AbandonedRemake/polyhaven')
UA = {'User-Agent': 'AbandonedRemake/1.0 (game asset pipeline; contact via Steam page)'}
USED = []


def _get(url):
    req = urllib.request.Request(url, headers=UA)
    with urllib.request.urlopen(req, timeout=60) as r:
        return r.read()


def _json(url):
    return json.loads(_get(url).decode())


def search(kind='models', category=None):
    """{id: info} for every asset of kind ('models', 'textures', 'hdris'), optionally one category."""
    url = f'{API}/assets?t={kind}' + (f'&c={category}' if category else '')
    return _json(url)


def _download(url, dest):
    if not os.path.exists(dest):
        os.makedirs(os.path.dirname(dest), exist_ok=True)
        data = _get(url)
        with open(dest, 'wb') as f:
            f.write(data)
    return dest


def fetch_model(asset_id, res='1k'):
    """Download the glTF of a model (and its textures) into the cache; returns the .gltf path."""
    files = _json(f'{API}/files/{asset_id}')
    gltf = files['gltf'][res]['gltf']
    folder = os.path.join(CACHE, 'models', asset_id, res)
    path = _download(gltf['url'], os.path.join(folder, os.path.basename(gltf['url'])))
    for rel, info in gltf.get('include', {}).items():
        _download(info['url'], os.path.join(folder, rel))
    USED.append(('model', asset_id))
    return path


def fetch_texture(asset_id, res='2k', maps=('Diffuse', 'nor_gl', 'Rough', 'AO', 'Metal', 'Displacement')):
    """Download a texture set (PNG/JPG per map) into the cache; returns {map: path}."""
    files = _json(f'{API}/files/{asset_id}')
    out = {}
    for m in maps:
        entry = files.get(m, {}).get(res)
        if not entry:
            continue
        fmt = 'png' if 'png' in entry else 'jpg'
        url = entry[fmt]['url']
        out[m] = _download(url, os.path.join(CACHE, 'textures', asset_id, res, os.path.basename(url)))
    USED.append(('texture', asset_id))
    return out


def import_model(asset_id, res='1k', col=None):
    """Import a Poly Haven model; returns its mesh objects (Poly Haven models are metric and Z-up)."""
    path = fetch_model(asset_id, res)
    before = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=path)
    new = [o for o in bpy.data.objects if o not in before]
    if col:
        for o in new:
            for c in o.users_collection:
                c.objects.unlink(o)
            col.objects.link(o)
    return [o for o in new if o.type == 'MESH']


def textured(name, maps, uv_scale=1.0):
    """A material from a fetched texture set (feeds the same bake as the procedural surfaces)."""
    mat = bpy.data.materials.new('RM_' + name)
    mat.use_nodes = True
    nt = mat.node_tree
    bsdf = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
    coord = nt.nodes.new('ShaderNodeTexCoord')
    mapping = nt.nodes.new('ShaderNodeMapping')
    mapping.inputs['Scale'].default_value = (uv_scale, uv_scale, uv_scale)
    nt.links.new(coord.outputs['UV'], mapping.inputs['Vector'])

    def tex(path, non_color):
        t = nt.nodes.new('ShaderNodeTexImage')
        t.image = bpy.data.images.load(path, check_existing=True)
        t.image.colorspace_settings.name = 'Non-Color' if non_color else 'sRGB'
        nt.links.new(mapping.outputs['Vector'], t.inputs['Vector'])
        return t
    if 'Diffuse' in maps:
        nt.links.new(tex(maps['Diffuse'], False).outputs['Color'], bsdf.inputs['Base Color'])
    if 'Rough' in maps:
        nt.links.new(tex(maps['Rough'], True).outputs['Color'], bsdf.inputs['Roughness'])
    if 'Metal' in maps:
        nt.links.new(tex(maps['Metal'], True).outputs['Color'], bsdf.inputs['Metallic'])
    if 'nor_gl' in maps:
        nm = nt.nodes.new('ShaderNodeNormalMap')
        nt.links.new(tex(maps['nor_gl'], True).outputs['Color'], nm.inputs['Color'])
        nt.links.new(nm.outputs['Normal'], bsdf.inputs['Normal'])
    return mat


def credit_line(kind, asset_id):
    return f'| {asset_id} | Poly Haven {kind} | CC0 | https://polyhaven.com/a/{asset_id} |'


def clean(objs, size=None):
    """Poly Haven models arrive with their own pivots and sometimes nested empties: flatten to one mesh at origin,
    optionally fitted to a (x, y, z) box resting on the ground."""
    meshes = [o for o in objs if o.type == 'MESH']
    for o in meshes:
        o.parent = None
    body = kit.join(meshes, meshes[0].name.split('.')[0])
    if size:
        kit.fit_to_box([body], size)
        kit.bake_transform(body)
    return body
