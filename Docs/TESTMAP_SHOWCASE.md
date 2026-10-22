# TestMap graphics showcase (2026-10-22)

The user, after four graphics passes: "I hate the graphics", and that the game lacks graphical fidelity in general
(not Ghost of Tsushima, but "some level of graphical fidelity"). Asked to redo **only the TestMap** with new lighting,
shadows and fidelity, plus static copies of the player characters and monsters to look at, and **not to test, verify
or run anything**: the user tests, then decides whether the approach goes to the rest of the game.

What made the difference this time is geometry, not shaders: every earlier pass improved materials and light on
boxes built in code. Here the boxes are architecture done properly, and the objects are photo-scanned models.

## Build it
**Tools > Abandoned > Maps > Test Map > Rebuild Test Map** (the existing menu item; nothing new in the Tools menu).
Batch: `Tools/unity.sh exec Abandoned.EditorTools.TestMapBuilder.Build`. Expect a few minutes: the first build imports
41 models and fixes their textures, then bakes a NavMesh, places reflection probes and decals and runs the flicker fix.

The models are in the repository. To fetch them again: `python3 Tools/Art/fetch_models.py` (Poly Haven, CC0; Blender
repacks each ARM map as R metal / G occlusion / A smoothness for URP Lit).

## What is there
- **The warehouse** (same footprint, floors, stairs, colliders and section names as before):
  - **Surfaces:** scanned surfaces in world space (concrete floor, oak upstairs, painted block, plaster, brick skin
    outside, tread-plate stairs), and a concrete roof on steel beams. Timber joists sit under every upper tile (they go
    with it when it collapses) and steel girders run on the columns.
  - **Openings:** steel door and window frames, glass, sills and lintels, an open steel door leaf and a roller-shutter
    coil box. The upstairs windows on the north, west and east walls are new.
  - **Trim and fittings:** open guard rails in place of the solid panels (the panel's collider is unchanged), stair
    stringers and handrails, skirting and yellow floor lines.
- **Furniture and clutter:** 41 Poly Haven scans. Shelving on the north wall, barrels, a pallet with crates in the
  atrium, an office upstairs. The loot still rests on the same crate, desk and pedestal (now a scanned crate, a
  scanned desk stretched to the old size, and a cast plinth).
- **Light:** 55 real `LightFixture`s (33 in the warehouse, 6 outside, 16 in the gallery). They are scanned fluorescent battens under the joists and below the roof,
  tungsten pendants in the atrium and stairwell, sodium wall packs and street lamps outside, and spots in the gallery.
  - Some flicker and one is dead.
  - ShadowDirector casts shadows from the nearest ones.
  - LightingRebuild calibrates each to the floor it lights (target 1.7).
  - Late-afternoon sun, light haze, the TestMap preset in AvLook (brighter key, warm whites), weathering 0.75, room
    reflection probes, wear decals and bounced light.
- **Gallery** (north of the warehouse; follow the path round the west side):
  - The crew in four coverall colours (orange, navy, olive, rust) and all eight monsters.
  - They stand on plinths with brass name plates, each under its own spot, and the room is kept dim.
  - They are frozen copies: same models and materials, no scripts, no animator, no colliders. Each monster is posed
    from 35 % through its own Idle take.

## Code
- `Editor/TestMap/` holds the pieces:
  - `TestMapShowcase` runs everything.
  - `ShowcaseModels` imports and places the scans.
  - `ShowcaseMaterials` builds the scanned surfaces through MaterialRemaster.
  - `ShowcaseWalls`, `ShowcaseStructure`, `ShowcaseDressing`, `ShowcaseLighting`, `ShowcaseExterior`,
    `ShowcaseGallery` and `ShowcaseStatues` build the rest.
- TestMapBuilder now calls `TestMapShowcase.Dress` after the populator and `TestMapShowcase.Finish` after the first
  save.
- AvLook, LightingRebuild and SurfaceAging gained a TestMap branch.
- The red truck block in front of the loading door is gone. The TestMap is still not in `AvOverhaul.MapScenes`, so
  Rebuild All Maps does not touch it.

## Kept clear for the play tests
TestMapNavigationTests, TestMapStructureTests and JuiceTests walk fixed routes:

- both front doors (x 6 and 14);
- the stair approach and landing (x 16-20);
- the balcony railing at (4.8, 4, 14);
- the upper north corridor the server rack is dragged along;
- every upper tile's centre.

No solid object stands in any of them. The loot positions are unchanged.

## Not checked (user's instruction: no tests, no runs)
Compiled outside Unity only (`dotnet build` of the generated projects: 0 errors). Never rebuilt, rendered or played.
Look at these first:
- **Model facing:** front and back are unknown. Each scan is turned so its long side follows the space it was given,
  but a shelf or desk may face the wall.
- **Fluorescent fittings:** whether `mounted_fluorescent_lights` is one fitting or a row of them. It is shrunk to
  fit 1.6 x 0.4 x 0.5 m.
- **Street lamp light:** the light sits under the middle of the lamp's bounds, not under its head.
- **Monster poses:** generic takes sampled in the editor. If one stands in its bind pose, `SampleAnimation` didn't
  apply.
- **Brightness:** the gallery's dim room and bright spots (×0.3 and ×1.8 after calibration), and the sun at 1.6.
- **Play tests:** the TestMap PlayMode tests (layout and colliders are meant to be unchanged).
