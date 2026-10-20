# 14 Larch Court — house art refinement

Original assets for the tutorial's existing 1970s winter ranch. The expanded room layout, lesson locations, salvage supports, furniture collision footprints and structural section ownership are retained.

## Asset work

- 19 original FBX assets, each with three authored LOD levels: sofa, armchair, bed, toilet, kitchen sink, vanity basin, bath, pleated curtain, cabinet door, books, crockery, vent, outlet, radiator, picture, dining chair, dresser, nightstand and fir foliage.
- 16 original seamless 1024px PBR sets (48 PNG maps): wood, plank flooring, upholstery, carpet, rug, tile, linoleum, laminate, plaster, block, siding, shingles, cardboard, bark, paint and floral cloth.
- Colour maps use sRGB; normal maps use BC5 normal import; metallic/occlusion/smoothness masks use linear BC7. Mipmaps, trilinear filtering and 8x anisotropy are enabled. Architectural cube UVs repeat in metres.
- The legacy custom-environment importer previously forced every image to 256px sRGB. Larch Court has an explicit exception that preserves its PBR map types and desktop texture resolution on reimport.
- New windows have restrained transparent glass; the school's frosted glass material is unchanged.

## Scene detail

Fitted cabinet fronts and brass pulls, open sink and bath basins, rounded upholstery, bed panels, drawer faces, curtain rods and finials, skirting, ceiling moulding, domestic wall hardware, framed pictures, books and crockery. Furniture models retain their original collider owners, including the bed on the collapsing bedroom floor.

The kitchen window is no longer hidden behind upper cabinets. Overlapping roof sections no longer put gutters and icicles inside adjoining rooms. Exterior details include downspouts, straps, number 14, a doorbell and louvred attic vents. Basement details include furnace louvers, service panels, duct joints, valves, water-heater controls and a drilled workbench pegboard.

## Regeneration

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --python Tools/Blender/tutorial/build.py -- "$PWD"
Tools/unity.sh exec Abandoned.EditorTools.TutorialHouseBuilder.BuildAndExit
```

Unity does not require Blender at import/runtime: exported FBX and PNG files are included. Preserve `.meta` files when regenerating. The existing **Tools > Abandoned > Maps > Tutorial House > Rebuild Tutorial House** menu also rebuilds the saved scene using the included assets.

## Review and validation

Matched 1600x1000 saved-scene views are in `Docs/renders/tutorial-refinement/before/` and `after/`: exterior, living room, kitchen, dining room, bedroom, bathroom, master bedroom and basement. Interior inspection uses a temporary warm-neutral light at eye height; these images do not represent the player's flashlight or runtime tutorial state. Capture changes are discarded without saving.

```sh
Tools/unity.sh exec Abandoned.EditorTools.TutorialHouseReview.AfterAndExit
Tools/unity.sh editmode 'Abandoned.Tests.Tutorial'
Tools/unity.sh playmode 'Abandoned.Tests.TutorialPlayTests'
```

The new art tests validate imported PBR types, model bounds/material bindings, collapse ownership, completed NavMesh hauling routes and the absence of indoor roof icicles. Existing tutorial tests exercise lesson progression and the collapsing floor at runtime.

All new geometry and surface imagery are original procedural work for ABANDONED. No downloaded packs, plugins or packages were added. Source: `Tools/Blender/tutorial/build.py` and `textures.py`, using the project's original `meridian_core.py` geometry helpers.
