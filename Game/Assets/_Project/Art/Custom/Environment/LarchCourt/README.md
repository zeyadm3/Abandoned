# Larch Court house kit

Original procedural assets for ABANDONED's tutorial house, 14 Larch Court.

19 exported FBX models with explicit LOD0/1/2 groups and 16 seamless 1024px texture sets. No external packs or runtime packages are required. Geometry uses metre units and floor-centred pivots; fixture/picture/curtain pivots are documented by their generator positions.

Materials retain `MALL_` slot names because the kit reuses the project's original Blender geometry/export helpers. `TutorialHouseModels` explicitly binds these slots to `TutorialHouseArt`; no mall material assets are used by the placed models.

Texture suffixes:
- `_BaseColor`: sRGB colour modulation for the house palette.
- `_Normal`: tangent-space RGB normal, imported as BC5.
- `_Mask`: R metallic, G occlusion, B unused, A smoothness; linear BC7.

The house-specific import rule in `CustomArtImport` prevents the legacy environment defaults from reducing these textures to 256px sRGB. Architectural UVs are metre scaled by the scene builder. FBX meshes carry their authored UVs.

Regenerate from the repository root:

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --python Tools/Blender/tutorial/build.py -- "$PWD"
Tools/unity.sh exec Abandoned.EditorTools.TutorialHouseBuilder.BuildAndExit
```

Optionally append model keys (e.g. `Tub Crockery`) to regenerate only those FBXs. Preserve existing `.meta` files. All imagery and geometry are original procedural work for this project.
