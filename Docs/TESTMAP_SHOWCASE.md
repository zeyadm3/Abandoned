# TestMap graphics showcase

## Site pass (2026-10-22, second pass)

The user, on the first pass: "it still looks basic"; the bar is a modern AAA-looking indie. Asked for much better level
design ("not a 20x16 box"), trees and landscaping, much better light and shadows (baked or bounced light allowed for the
TestMap's static parts), better materials, props, atmosphere and sound, keeping the statue gallery. Only the TestMap
changes; the user decides afterwards whether any of it goes to the real maps.

The warehouse block the tests walk is **unchanged** (tile grid, section names, stairs, both doors, spawn row, loot,
colliders), so TestMapNavigationTests, TestMapStructureTests and JuiceTests needed no edits. What changed is everything
around it.

### Build it
**Tools > Abandoned > Maps > Test Map > Rebuild Test Map** builds and now also **bakes the lighting** (about 15 minutes
in all on this Mac: the coplanar pass and the bake are most of it). **Bake Test Map Lighting** (new, 1601) bakes again
without rebuilding. Assets are in the repository; to fetch and bake them again: `python3 Tools/Art/fetch_nature.py`
(Blender is used for the trees, maps and the grass card; `FETCH_CACHE=<dir>` keeps the raw downloads, the fir alone is
a 249 MB FBX).

### Level design (Editor/TestMap/ShowcaseSitePlan.cs has every position)
- **A fenced yard**, x -40..62, z -54..52, round the warehouse and the gallery, on a 640 m terrain. Past the fence the
  ground rises into wooded hills, highest on a ridge to the north; a ring of far hills closes the horizon.
- **The approach:** a gravel road climbs out of the forest from the south-west, cut into the hillside with ditches,
  under a power line on wooden poles, to the south gate (one leaf hanging open, concrete blocks on the verge, a
  breeze-block guard hut).
- **The forecourt:** cracked asphalt from the gate to the warehouse, weeds in the cracks, wet patches, cars under
  covers, a box trailer without its cab, a skip, pallet stacks, dead lamp posts. The spawn row and both door lanes are
  clear.
- **West:** the roofless shell of the old boiler house (broken wall tops, window holes, a fallen girder, the boiler,
  rubble, young firs growing inside) and its 26 m chimney: walkable, a second space to explore.
- **East:** a gravel lot with shipping containers stacked in a block with lanes between, a lean-to over gas bottles,
  crates and a welder, and a 21 m steel water tower. The chimney and water tower are the landmarks from anywhere.
- **North:** long grass behind the gallery, firs and small trees left to grow inside the fence.
- **Warehouse outside:** gutters and downpipes at the corners, security lamps, a camera, a hydrant.

### Nature
- 7,965 terrain trees of 16 kinds: three scanned firs (19, 14 and 14.5 m), young firs, pine saplings, small broadleaf
  trees and dead snags; tall firs deep in the forest, young ones and small trees at its edges, clearings where the
  noise says so, nothing within ~9 m of the fence or 7 m of the road.
- The scanned trees are millions of leaf cards each. `Tools/Art/bake_tree.py` keeps a random share of the cards and
  grows each kept card so the crown keeps its cover (LOD0 ~95k card faces for the tall fir, LOD1 ~18k), decimates the
  trunk, and renders an impostor (two crossed quads, front and side) for LOD2. Card textures keep their alpha coverage
  in the mips (`mipMapsPreserveCoverage`), so far trees don't go bald.
- Grass: a card rendered from scanned clumps (`bake_card.py`) as Unity's waving terrain grass, in drifts; scanned tufts,
  ferns, nettles, weeds and shrubs as instanced detail meshes. Mossy boulders, fallen trunks, stumps and brush along
  the forest edge.
- Eight scanned ground layers painted by rule: grass and dry meadow in the open, forest floor and leaf litter under
  the trees, bare dirt and gravel on steep slopes, the gravel road with dirt shoulders, asphalt and weedy gravel in
  the yard, trodden dirt round the buildings.

### Light and atmosphere (ShowcaseSky.cs)
- **Sky:** a photographed golden-hour sky (Poly Haven evening_road_01, 4K HDR) as the skybox, the ambient light and
  the reflections. Its sun is put in the west-south-west, raking across the warehouse's south front.
- **Sun:** on the same bearing at 16 degrees (the photo's is ~10; lower would put the whole yard in the forest's
  shadow), warm (1, .7, .45), intensity 5; ambient from the sky at 0.65 so the sun reads.
- **Baked light, TestMap only:** a `TestMap_Lighting` settings asset (GPU lightmapper, Shadowmask, directional
  lightmaps, 5 texels/m, baked AO, 3 bounces). The sun is Mixed: realtime shadows close to you, baked shadows and
  bounce beyond the 35 m shadow distance. Static architecture is lightmapped (583 renderers); models without lightmap
  UVs and small things are lit by 9,285 light probes, which also light everything that moves or collapses (floor tiles
  are never static). Fixtures stay realtime (their power and flicker).
- **Grade:** warm haze toward the sun (fog .0045), volumetric fog to 140 m, contrast 18, cool shadows and warm
  highlights, bloom .25, vignette .2.
- **Materials:** the warehouse, gallery and boiler house brick is now Poly Haven's factory_brick at its real 1.5 m
  repeat (the old remaster scan looked like orange blotches up close), the car park uses the same asphalt scan as the
  forecourt, containers are four scanned claddings toned down.

### Sound (Scripts/Audio/OutdoorSoundscape.cs, Editor/TestMap/ShowcaseSound.cs)
Birdsong and wind in the trees from points round the forest's edge (spatial, dulled indoors like any outdoor sound),
crickets in the long grass, a crow now and then from a treetop. Four new SoundIds (appended): BirdsongLoop,
TreeWindLoop, CricketsLoop, CrowCall, from CC0 Freesound recordings (Tools/Audio/library.csv). Inside, the existing
ambience (hum, room tone, failing tubes) carries on.

### Also fixed
- The crew statues in the gallery faced away from the entrance (their backpacks to the door); they face it now.

### What to look at first
1. **Performance.** ~8,000 LOD trees, grass and detail meshes, a 640 m terrain. Check the F1 perf line at the gate and
   in the forecourt. Levers if it's heavy: `Terrain.detailObjectDistance` (110), the fir's LOD0 card count in
   `Tools/Art/fetch_nature.py`, `treeDistance` (650).
2. **Shadowmask in play.** The realtime/baked shadow hand-off at 35 m, and whether ShadowDirector's fixture shadows
   behave with the baked sun.
3. **Footsteps outside.** The whole terrain is tagged Dirt (one surface per terrain), so the asphalt forecourt sounds
   like dirt; the old car-park slab is still Asphalt.
4. **The forest from inside the yard.** Firs are thinned scans; up close their crowns are lighter than the original.
5. **Sound levels** of the four beds (OutdoorSoundscape's levels on the Soundscape object).

### Not checked (user's instruction: no play tests, no test suites)
Built, baked and reviewed from editor screenshots only (Docs/renders/testmap-site has five; Game/Logs/tmshots has the rest). Never played; no test suite run.
Collapsing floors, loot and spawns are the same objects as before.

---

## First pass (2026-10-22)

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
