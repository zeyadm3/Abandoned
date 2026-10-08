# Original ABANDONED assets

`meridian.py` generates the realistic Meridian Mall 0.13.0 kit: 83 FBX assets and 25 original
PBR texture sets (75 PNG maps). `environment.py` retains the earlier modular kit, depot props,
wardrobe and procedural palette textures. `threats.py` builds eight original skinned silhouettes
and their Idle, Walk, Chase, Attack and Special animation takes.

Generated FBX/PNG assets are checked into `Game/Assets/_Project/Art/Custom/` so Unity does not
need Blender installed to import or build the game. Blender 4.3.2 was used for this pass and
is installed at `/Applications/Blender.app` on the development Mac.

Regenerate the new mall kit from the repository root:

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --python Tools/Blender/meridian.py -- "$PWD" all
Tools/unity.sh rebuild-horror
```

The new kit is isolated under `Art/Custom/Environment/Meridian/`. Its original sources are
`meridian_core.py` (metric geometry, tiling/trim UVs and export), `meridian_surfaces.py`
(six floor finishes, crack/sag stages and sibling fracture chunks), `meridian_architecture.py`
(walls, storefronts, flights and shell), `meridian_props.py` (fixed retail fixtures),
`meridian_decay.py` (abandonment dressing) and `meridian_textures.py` (manufactured PBR patterns).
`meridian_inspect.py` reads exported FBX bounds/hierarchies into `MeridianGeometry.json`.
`meridian_preview.py` optionally renders the kit for art review; it does not run the game.
Exact export keys, sizes, map names and material slots are in `MeridianManifest.json` and the
kit's own README. Preserve all `.meta` files when regenerating.

Texture sizes are 2048 for hero terrazzo/plaster/storefront/escalator/ceiling sets, 1024 for
standard finishes and 512 for small props/decals. Each set has `_BaseColor`, `_Normal` and `_Mask`
maps. The mask packs R metallic, G occlusion, B unused, A smoothness; Unity imports colour as
sRGB, normals as normal maps and masks as linear data, with mipmaps and explicit compressed
desktop settings. FBX slots are `MALL_<Set>`. Major assets have explicit LOD0/1/2 groups;
fractured floor files contain nine sibling chunks and each flight fracture has ten siblings.
Flights climb Unity +Z; pivots sit at the
floor/base centre or flight foot. The coordinate conversion remains Unity `(x,y,z)` to Blender
`(x,z,y)`. Nested LOD exports use `bake_space_transform=False` to avoid duplicate unit/axis
conversion; Unity preserves file-scale/axis import corrections beneath neutral placement holders.

The sign frames, notices, abstract graffiti, surface patterns and meshes are original work;
fictional text is supplied by the mall builders through `WorldTextMaterial`. No third-party
downloads were used for this kit, and the legacy HQ/cosmetic exports are unaffected.

Regenerate the earlier kit or threats only when those assets need updating:

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --python Tools/Blender/environment.py
/Applications/Blender.app/Contents/MacOS/Blender --background --python Tools/Blender/threats.py
Tools/unity.sh rebuild-horror
```

Close the Unity editor before batch regeneration. The horror regeneration command rebuilds
Mall and HQ, keeps TestMap untouched, and runs the rebuild-time flight/collider/material audits.
It does not run gameplay, EditMode/PlayMode or network tests, icon captures or screenshot tests.
All source meshes and textures in this folder are original work for ABANDONED.
