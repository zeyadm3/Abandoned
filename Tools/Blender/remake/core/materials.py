"""Procedural PBR surfaces for the 2026 remake.

Every surface is a Principled BSDF fed by noise-driven colour variation, roughness breakup, edge wear (Cycles
pointiness) and crevice grime (Cycles AO), so a model looks used before it is baked. bake.py then flattens all of a
model's surfaces into one texture set (BaseColor / Normal / Mask) for Unity.

Surfaces are looked up by type, never by node name (node names are localised in some Blender builds).
Material names: "RM_<Surface>" while authoring; baked assets end with one "RM_<Asset>" material.
A few surfaces are never baked and keep a shared Unity material (see SHARED): glass, emissive lamps and screens,
hair cards (alpha) and decals.
"""
import bpy

# Surfaces that are not flattened into the asset's texture set; Unity maps them to shared materials by name.
SHARED = ('Glass', 'GlassDark', 'Emit', 'EmitWarm', 'EmitCold', 'EmitRed', 'EmitGreen', 'Screen', 'HairCard', 'Decal')


def _principled(mat):
    return next(n for n in mat.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')


def _input(node, *names):
    for n in names:
        if n in node.inputs:
            return node.inputs[n]
    raise KeyError(names)


def surface(name, base=(.5, .5, .5), rough=.6, metal=0.0, var=.08, scale=8.0, bump=.15, bump_scale=60.0,
            wear=0.0, wear_color=None, grime=.25, grime_color=(.05, .045, .04), rust=0.0, emission=None,
            alpha=1.0, spec=.5, coat=0.0, sheen=0.0, subsurface=0.0):
    """A worn surface. var: colour variation; wear: bright edge wear (paint chipped to metal when metal_under);
    grime: dirt in crevices (AO); rust: rust blotches; bump: micro surface strength."""
    key = 'RM_' + name
    mat = bpy.data.materials.get(key)
    if mat:
        return mat
    mat = bpy.data.materials.new(key)
    mat.use_nodes = True
    nt = mat.node_tree
    nodes, links = nt.nodes, nt.links
    bsdf = _principled(mat)
    coord = nodes.new('ShaderNodeTexCoord')

    # Large-scale colour variation and fine breakup.
    n1 = nodes.new('ShaderNodeTexNoise')
    n1.inputs['Scale'].default_value = scale
    n1.inputs['Detail'].default_value = 6
    links.new(coord.outputs['Object'], n1.inputs['Vector'])
    ramp = nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].color = (*[max(0, c * (1 - var)) for c in base], 1)
    ramp.color_ramp.elements[1].color = (*[min(1, c * (1 + var)) for c in base], 1)
    links.new(n1.outputs['Fac'], ramp.inputs['Fac'])
    colour = ramp.outputs['Color']

    if rust > 0:
        nr = nodes.new('ShaderNodeTexNoise')
        nr.inputs['Scale'].default_value = scale * .6
        nr.inputs['Detail'].default_value = 10
        nr.inputs['Roughness'].default_value = .7
        links.new(coord.outputs['Object'], nr.inputs['Vector'])
        rr = nodes.new('ShaderNodeValToRGB')
        rr.color_ramp.elements[0].position = 1 - rust * .5
        rr.color_ramp.elements[1].position = min(1, 1 - rust * .5 + .08)
        links.new(nr.outputs['Fac'], rr.inputs['Fac'])
        mix = nodes.new('ShaderNodeMix')
        mix.data_type = 'RGBA'
        links.new(rr.outputs['Color'], mix.inputs['Factor'])
        links.new(colour, mix.inputs['A'])
        mix.inputs['B'].default_value = (.23, .09, .035, 1)
        colour = mix.outputs['Result']

    if wear > 0:
        # Edge mask independent of mesh density: how far a 4 mm bevelled normal leans from the true normal.
        geo = nodes.new('ShaderNodeNewGeometry')
        bev = nodes.new('ShaderNodeBevel')
        bev.inputs['Radius'].default_value = .004
        bev.samples = 8
        dot = nodes.new('ShaderNodeVectorMath')
        dot.operation = 'DOT_PRODUCT'
        links.new(bev.outputs['Normal'], dot.inputs[0])
        links.new(geo.outputs['Normal'], dot.inputs[1])
        edge = nodes.new('ShaderNodeMath')
        edge.operation = 'SUBTRACT'
        edge.inputs[0].default_value = 1.0
        links.new(dot.outputs['Value'], edge.inputs[1])
        nw = nodes.new('ShaderNodeTexNoise')
        nw.inputs['Scale'].default_value = scale * 6
        nw.inputs['Detail'].default_value = 8
        links.new(coord.outputs['Object'], nw.inputs['Vector'])
        mul = nodes.new('ShaderNodeMath')
        mul.operation = 'MULTIPLY'
        links.new(edge.outputs[0], mul.inputs[0])
        links.new(nw.outputs['Fac'], mul.inputs[1])
        boost = nodes.new('ShaderNodeMath')
        boost.operation = 'MULTIPLY'
        boost.inputs[1].default_value = 40.0 * wear
        links.new(mul.outputs[0], boost.inputs[0])
        wr = nodes.new('ShaderNodeValToRGB')
        wr.color_ramp.elements[0].position = .45
        wr.color_ramp.elements[1].position = .55
        links.new(boost.outputs[0], wr.inputs['Fac'])
        mix = nodes.new('ShaderNodeMix')
        mix.data_type = 'RGBA'
        links.new(wr.outputs['Color'], mix.inputs['Factor'])
        links.new(colour, mix.inputs['A'])
        mix.inputs['B'].default_value = (*(wear_color or [min(1, c * 1.6 + .12) for c in base]), 1)
        colour = mix.outputs['Result']

    if grime > 0:
        ao = nodes.new('ShaderNodeAmbientOcclusion')
        ao.inputs['Distance'].default_value = .08
        gr = nodes.new('ShaderNodeValToRGB')
        gr.color_ramp.elements[0].position = .35
        gr.color_ramp.elements[1].position = .9
        links.new(ao.outputs['AO'], gr.inputs['Fac'])
        inv = nodes.new('ShaderNodeInvert')
        links.new(gr.outputs['Color'], inv.inputs['Color'])
        amt = nodes.new('ShaderNodeMath')
        amt.operation = 'MULTIPLY'
        sep = nodes.new('ShaderNodeRGBToBW')
        links.new(inv.outputs['Color'], sep.inputs['Color'])
        links.new(sep.outputs['Val'], amt.inputs[0])
        amt.inputs[1].default_value = grime
        mix = nodes.new('ShaderNodeMix')
        mix.data_type = 'RGBA'
        links.new(amt.outputs[0], mix.inputs['Factor'])
        links.new(colour, mix.inputs['A'])
        mix.inputs['B'].default_value = (*grime_color, 1)
        colour = mix.outputs['Result']

    links.new(colour, _input(bsdf, 'Base Color'))

    # Roughness breakup.
    n2 = nodes.new('ShaderNodeTexNoise')
    n2.inputs['Scale'].default_value = scale * 3
    links.new(coord.outputs['Object'], n2.inputs['Vector'])
    rmap = nodes.new('ShaderNodeMapRange')
    rmap.inputs['To Min'].default_value = max(0, rough - .12)
    rmap.inputs['To Max'].default_value = min(1, rough + .12)
    links.new(n2.outputs['Fac'], rmap.inputs['Value'])
    links.new(rmap.outputs['Result'], _input(bsdf, 'Roughness'))
    _input(bsdf, 'Metallic').default_value = metal
    _input(bsdf, 'Specular IOR Level', 'Specular').default_value = spec
    if coat:
        _input(bsdf, 'Coat Weight', 'Clearcoat').default_value = coat
    if sheen:
        _input(bsdf, 'Sheen Weight', 'Sheen').default_value = sheen
    if subsurface:
        _input(bsdf, 'Subsurface Weight', 'Subsurface').default_value = subsurface

    if bump > 0:
        n3 = nodes.new('ShaderNodeTexNoise')
        n3.inputs['Scale'].default_value = bump_scale
        n3.inputs['Detail'].default_value = 8
        links.new(coord.outputs['Object'], n3.inputs['Vector'])
        b = nodes.new('ShaderNodeBump')
        b.inputs['Strength'].default_value = bump
        b.inputs['Distance'].default_value = .002
        links.new(n3.outputs['Fac'], b.inputs['Height'])
        links.new(b.outputs['Normal'], _input(bsdf, 'Normal'))

    if emission:
        _input(bsdf, 'Emission Color', 'Emission').default_value = (*emission[:3], 1)
        _input(bsdf, 'Emission Strength').default_value = emission[3] if len(emission) > 3 else 5.0
    if alpha < 1:
        _input(bsdf, 'Alpha').default_value = alpha
        try:
            mat.surface_render_method = 'BLENDED'
        except (AttributeError, TypeError):
            pass
    mat.diffuse_color = (*base, 1)
    return mat


def assign(ob, mat, faces=None):
    """Give an object a material (all faces, or the listed face indices as an extra slot)."""
    if mat.name not in [m.name for m in ob.data.materials if m]:
        ob.data.materials.append(mat)
    idx = [m.name for m in ob.data.materials].index(mat.name)
    for p in ob.data.polygons:
        if faces is None or p.index in faces:
            p.material_index = idx
    return ob


# ------------------------------------------------------------------------------------------------- the library
# Grounded, worn, realistic. Colours are linear-ish albedo (keep metals <= .75, dielectrics .03-.85).

def library():
    S = surface
    return {
        # metals
        'Steel': S('Steel', (.55, .56, .58), .38, 1, var=.05, wear=.3, grime=.35, bump=.08),
        'SteelDark': S('SteelDark', (.2, .21, .22), .45, 1, var=.06, wear=.4, grime=.4),
        'Galvanised': S('Galvanised', (.6, .62, .63), .45, 1, var=.12, scale=3, grime=.3),
        'Aluminium': S('Aluminium', (.75, .76, .77), .32, 1, var=.04, grime=.2),
        'Brass': S('Brass', (.72, .52, .24), .35, 1, var=.1, grime=.45, wear=.3),
        'Copper': S('Copper', (.75, .42, .28), .38, 1, var=.12, grime=.5, rust=.2),
        'Chrome': S('Chrome', (.85, .86, .87), .12, 1, var=.02, grime=.2, bump=.02),
        'RustedSteel': S('RustedSteel', (.32, .2, .14), .75, .6, var=.2, rust=.7, grime=.4, bump=.3),
        'CastIron': S('CastIron', (.16, .16, .16), .7, .9, var=.08, rust=.3, grime=.4, bump=.35, bump_scale=120),
        # painted
        'PaintRed': S('PaintRed', (.42, .05, .04), .45, 0, wear=.5, wear_color=(.45, .45, .46), grime=.35, coat=.2),
        'PaintYellow': S('PaintYellow', (.75, .52, .05), .45, 0, wear=.55, wear_color=(.4, .4, .41), grime=.4),
        'PaintOrange': S('PaintOrange', (.75, .22, .03), .45, 0, wear=.5, wear_color=(.4, .4, .41), grime=.35),
        'PaintGreen': S('PaintGreen', (.09, .17, .1), .5, 0, wear=.45, wear_color=(.4, .4, .41), grime=.4),
        'PaintBlue': S('PaintBlue', (.06, .12, .3), .45, 0, wear=.45, wear_color=(.4, .4, .41), grime=.35),
        'PaintWhite': S('PaintWhite', (.72, .71, .67), .5, 0, wear=.35, grime=.45, var=.06),
        'PaintGrey': S('PaintGrey', (.3, .31, .31), .5, 0, wear=.4, wear_color=(.45, .45, .46), grime=.4),
        'PaintBlack': S('PaintBlack', (.03, .03, .03), .45, 0, wear=.35, wear_color=(.35, .35, .36), grime=.2),
        'PaintCream': S('PaintCream', (.68, .6, .45), .55, 0, wear=.35, grime=.45),
        # polymers
        'Plastic': S('Plastic', (.08, .08, .08), .5, 0, var=.05, grime=.3, bump=.05),
        'PlasticGrey': S('PlasticGrey', (.35, .35, .34), .5, 0, var=.05, grime=.35, bump=.05),
        'PlasticBeige': S('PlasticBeige', (.6, .55, .45), .45, 0, var=.06, grime=.4, bump=.05),
        'PlasticYellow': S('PlasticYellow', (.8, .55, .02), .4, 0, var=.05, grime=.35),
        'PlasticRed': S('PlasticRed', (.5, .03, .02), .4, 0, var=.05, grime=.3),
        'Rubber': S('Rubber', (.025, .025, .025), .85, 0, var=.05, grime=.2, bump=.2, bump_scale=200),
        'Tyre': S('Tyre', (.02, .02, .02), .9, 0, var=.04, grime=.5, grime_color=(.12, .1, .08), bump=.3),
        # organics / natural
        'Leather': S('Leather', (.18, .1, .055), .55, 0, var=.15, wear=.3, grime=.3, bump=.35, bump_scale=180),
        'LeatherBlack': S('LeatherBlack', (.03, .028, .026), .5, 0, var=.1, wear=.35, grime=.2, bump=.35, bump_scale=180),
        'Wood': S('Wood', (.32, .19, .1), .6, 0, var=.25, scale=3, grime=.35, bump=.2),
        'WoodDark': S('WoodDark', (.12, .07, .04), .5, 0, var=.2, scale=3, grime=.3, coat=.2),
        'WoodRaw': S('WoodRaw', (.5, .38, .24), .75, 0, var=.25, scale=2.5, grime=.4, bump=.3),
        'Fabric': S('Fabric', (.25, .25, .24), .9, 0, var=.08, grime=.3, bump=.25, bump_scale=300, sheen=.3),
        'Canvas': S('Canvas', (.35, .32, .24), .9, 0, var=.1, grime=.4, bump=.3, bump_scale=250),
        'Paper': S('Paper', (.7, .67, .58), .85, 0, var=.08, grime=.35, bump=.1),
        'Cardboard': S('Cardboard', (.45, .33, .2), .9, 0, var=.12, grime=.35, bump=.15),
        'Glass': S('Glass', (.8, .85, .85), .05, 0, alpha=.25, var=0, grime=0, bump=0),
        'GlassDark': S('GlassDark', (.05, .06, .06), .08, 0, alpha=.6, var=0, grime=0, bump=0),
        'Ceramic': S('Ceramic', (.78, .77, .73), .2, 0, var=.04, grime=.4, coat=.6, bump=.02),
        'Porcelain': S('Porcelain', (.85, .84, .8), .15, 0, var=.03, grime=.3, coat=.8, bump=.01),
        'Concrete': S('Concrete', (.42, .41, .39), .9, 0, var=.15, scale=2, grime=.4, bump=.4, bump_scale=90),
        'Stone': S('Stone', (.35, .34, .32), .85, 0, var=.2, scale=3, grime=.45, bump=.5, bump_scale=40),
        'Gold': S('Gold', (.83, .62, .22), .25, 1, var=.05, grime=.3),
        'Silver': S('Silver', (.8, .8, .8), .25, 1, var=.04, grime=.4),
        # emissive and screens (shared, never baked)
        'Emit': S('Emit', (1, 1, 1), .4, 0, emission=(1, .95, .85, 6), var=0, grime=0, bump=0),
        'EmitWarm': S('EmitWarm', (1, .8, .5), .4, 0, emission=(1, .7, .4, 6), var=0, grime=0, bump=0),
        'EmitCold': S('EmitCold', (.8, .9, 1), .4, 0, emission=(.7, .85, 1, 6), var=0, grime=0, bump=0),
        'EmitRed': S('EmitRed', (1, .1, .05), .4, 0, emission=(1, .08, .03, 8), var=0, grime=0, bump=0),
        'EmitGreen': S('EmitGreen', (.2, 1, .3), .4, 0, emission=(.2, 1, .25, 8), var=0, grime=0, bump=0),
        'Screen': S('Screen', (.02, .03, .03), .1, 0, emission=(.15, .4, .3, 2), var=0, grime=0, bump=0),
    }


def skin(name, tone=(.55, .38, .3), rough=.48, freckle=.0):
    """Realistic-ish skin: subsurface, colour variation (redness, pores) and a fine pore bump."""
    mat = surface(name, tone, rough, 0, var=.12, scale=14, bump=.08, bump_scale=900, grime=.12,
                  grime_color=tuple(c * .5 for c in tone), subsurface=.15)
    return mat


def fabric(name, color, rough=.88, weave=300.0, grime=.35):
    return surface(name, color, rough, 0, var=.1, grime=grime, bump=.25, bump_scale=weave, sheen=.3)


def paint(name, color, wear=.45, grime=.35, rough=.45):
    return surface(name, color, rough, 0, wear=wear, wear_color=(.42, .42, .43), grime=grime, coat=.1)
