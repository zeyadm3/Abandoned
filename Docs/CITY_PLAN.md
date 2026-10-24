# The city: the drawn map as the open world (2026-10-24)

The user's reference is a drawn city plan: a sea down the west side, a river running south-east across it, a
dense grid of blocks, yellow arterial roads, a roundabout north of the river and two parks. The open world
(Docs/OPEN_WORLD_PLAN.md) becomes that city: 6 km of wooded hills stop being the world and become its outskirts,
and the nine salvage locations stand on blocks in it. Driving between them is driving through a city.

**This is a plan for review, not a build.** Nothing is built until the questions at the end are answered.
The first build is a **rough draft**: ground, water, roads and plain extruded block massing. No facades, no
interiors, no loot, no detail on any building until the draft reads right from the van.

## What already exists (reused, not rebuilt)

| Piece | Where | Used for |
|---|---|---|
| Moving origin, anchor, stand-ins | `Scripts/World/WorldRoot`, `WorldState` | Unchanged. The city is a child of `WorldRoot`. |
| One place loaded at a time | `Scripts/World/PoiStreamer` | Unchanged. The city never loads a second salvage scene. |
| Merge thousands of boxes into one saved mesh per material | `Editor/Tutorial/TutorialMeshMerge` | Every city cell's draw cost. |
| Far copies, merged, triangle-budgeted, screen-height culled | `Editor/World/WorldProxies` | The far ring of the city. |
| A street grid with kerbs, markings and junctions merged into one mesh | `Editor/Skyscraper/SkyscraperDowntownStreets` | The city's road meshes. |
| Seeded tower massing (podium, shaft, setback, crown) | `Editor/Skyscraper/SkyscraperDowntownBuildings` | Downtown blocks. |
| Plain locked house copies on lots, merged near/far | `Editor/Tutorial/TutorialStreetHouseKit`, `TutorialTown` | Residential blocks. |
| Outer-ring stock (kerbs, lamps, parked cars, shelters) | `Editor/PoliceStation/StationRingStock` | Streetscape, shared. |
| Sea plane and surf | `Editor/Prison/IslandTerrain.Sea` | The west coast. |
| Terrain tiles, paint, scanned trees and grass | `Editor/World/WorldTerrain`, `WorldTerrainPaint` | Outskirts and parks only. |
| Road events that need no NavMesh | `Scripts/World/RoadEventDirector`, `RoadMonster` | Unchanged; they work on any road. |

No new package, no Asset Store asset. Everything below is code-built from existing materials.

## Size and shape

The drawing is about 2.3 : 1. Fitted into the world, keeping places about 2 km apart:

- World grows from 6144 m to **8192 m square** (`WorldPlan.Size`), 8 x 8 terrain tiles of 1024 m.
- The **city** fills a band of about **6000 x 3000 m** in the middle; the rest stays the outskirts it already has.
- **Sea** west of about x = -3000, with a ragged coast; the **river** crosses from about (-2600, 300) to
  (3400, -1400), 60-90 m wide; **bridges** carry every arterial that crosses it.
- **Arterials** (the drawing's yellow roads) every 500-700 m, 14 m wide, two lanes each way: the drivable spine.
- **Minor streets** every 110-130 m, 8 m wide, drivable, filling the blocks between arterials.
- **The roundabout** sits where the drawing puts it, north of the river and east of centre: a 70 m island, five arms.
- **Two parks**, as drawn: one east of the roundabout, one south of the river.

## Where the existing maps go

The drawing decides this. The sea finally gives the island prison a home, and the river gives the factory a reason
to be where it is.

| Map | District | Why |
|---|---|---|
| **HQ** | South-west waterfront, by the docks | It is an industrial estate; the world origin stays here. |
| **Mall** (Meridian) | Retail core, two blocks south-west of the roundabout | Its district ground is 640 x 640 m: four city blocks. |
| **Skyscraper** (Cloudline) | Downtown, at the roundabout | It brings its own 720 m downtown; the city's densest blocks meet it, and `Exterior/Skyline` stays hidden. |
| **Police Station** | Civic block east of the roundabout | A night map, in the civic quarter. |
| **Fire Station** | Civic, four blocks south of the police station | Same quarter, its own street. |
| **Hospital** | Campus on the east park's north edge | Its 80 x 88 m campus wants open ground. |
| **School** (Pine Hollow) | North-east edge, where the grid thins | Keeps its own winter hour (`PlaceLook`); its snowy forest ring becomes the northern outskirt. |
| **Iron Factory** | South bank of the river, east | Riverside industry; its rail dock meets the water. |
| **Motel** (Last Light) | On the highway out of the south-east corner, outside the grid | A roadside motel belongs on the road out. |
| **Island Prison** (Gallows Rock) | **An island in the sea, about 1.4 km off the west coast** | The one thing the old world could not give it. A quay on the waterfront; its boat is the crossing. |

Each keeps its own sky, hour and weather, as `PlaceLook` already does, and its own ground rectangle, which
`WorldSurvey` measures from its saved scene. The city lays its blocks around that rectangle instead of cutting a
terrain hole.

## How it stays fast

The world scene is loaded the whole time, so everything in it has to be cheap. Five levers, all already proven here:

1. **Cells, not objects.** The city is a grid of **256 m cells** (about 23 x 12 = 276 of them). Each cell's roads,
   pavements, massing and street furniture are merged at build time into **one saved mesh per material**
   (`TutorialMeshMerge`), so a whole block is about four renderers, not four thousand objects.
2. **Two levels of detail, streamed by distance** (`CityStreamer`, new, on every machine, driven by the van):
   - **Near** (under 700 m): the full cell mesh and its box colliders.
   - **Far** (700 m to the view limit): one silhouette mesh per cell, no colliders, screen-height culled at 0.004
     like `WorldProxies`. A triangle budget per cell: about 20k near, 2k far.
   - Beyond that: nothing. Cells are mesh assets, never scenes, so showing one costs a `SetActive`, not a load.
3. **Fog does the rest.** The city raises `fogDensity` from .0006 to about .0035 (roughly 600-800 m of visibility)
   and drops the camera's far plane to match. This is the biggest lever by far, and it suits the game: dusk, rain,
   a dead city. It also means the far ring never has to be good.
4. **Colliders are boxes.** A rough-draft building is an extruded footprint with one box collider. No mesh colliders
   anywhere in the city; the van only ever hits boxes and the road.
5. **No NavMesh, no real lights, no shadow casters in the city.** Monsters live at places and on the road
   (`RoadMonster` needs no NavMesh). Street lamps are emissive meshes, not lights, except a few near the van.
   Nothing in the city casts a shadow but the sun, at two cascades.

Budgets to hold, checked by a new `CityBudgetTests` beside `MallRenderBudgetTests`: **under 700 draw calls** and
**under 1.5 M triangles** from the city at any camera position in it, and under 4,000 active city objects.

## Build order (each step small enough to drive)

1. **`CityPlan`** — the drawing as data: coastline, river polyline, arterials, minor grid, roundabout, parks,
   districts and the ten site slots. One file, no geometry; reviewable as a gizmo map before anything is built.
2. **Ground and water** — city ground plane, sea, river, coast, park ground, bridge piers. Terrain only outside.
   *Done (2026-10-24, written, not run in Unity.)* `Editor/City/CityGround`, `CityWater`, `CityMesh` and
   `CityDraftBuilder`, menu **Maps > Open World > Rebuild City Draft**. Two things settled while building it:
   - **The draft is its own scene**, `Scenes/CityDraft`, not the World. Scenes/World works and has been played, so
     the city grows beside it and takes its place at step 6 once the budgets above are met. It is deliberately kept
     out of `BuildScenes.All`: a scene path for an unfinished map stops everyone's player builds.
   - **Ground is cut at 16 m.** A cell whose squares are all one surface is a single quad; only the cells the coast
     or the river cut into are described square by square. That holds the whole ground to a few thousand triangles
     and gives the shoreline a ragged edge close to the drawing's own. Collision stays boxes: the solid squares are
     run together into as few slabs as possible, so an inland cell needs one.
3. **Roads** — arterials, minor streets, the roundabout, junctions and bridges, merged per cell; the drivable
   surface and its colliders. Drive the whole city with nothing else in it.
4. **Block massing (the rough draft)** — plain extruded boxes per block, heights seeded by district (downtown
   80-180 m, retail 12-20 m, residential 8-14 m, industrial 10-24 m), one box collider each, merged per cell.
   **Stop here and look at it.** No facades, no windows, no interiors.
5. **Cell streaming and LOD** — `CityStreamer`, far silhouettes, fog and far plane, the budget tests.
6. **The ten places** — slots, `WorldSurvey` measurement, roads reaching each one, the prison's island and quay.
7. **Perf pass and review renders** — budget tests, `GraphicsAudit`, aerial and cab shots per district.

Detail (facades, shopfronts, streetscape, props) is a later pass, district by district, once the draft drives well.

## Risks

| Risk | Likelihood | Mitigation |
|---|---|---|
| A 6 x 3 km city is too much geometry for the always-loaded scene | High | Cells, two LODs and fog; budget tests from step 5, not at the end |
| Authoring time: the full city builder may run for hours (the Skyscraper's bake already took 14,802 s) | High | No occlusion bake in the city; merged meshes are saved assets, rebuilt per cell only when its data changes |
| The moving origin has to shift an 8 km world | Medium | Cells are children of `WorldRoot`, which already moves; only the anchor's scene is loaded |
| Places' own horizons (mall mountains, school forest, skyline) stand inside the city | Medium | `hideInWorld`, already how the Skyscraper's skyline is handled |
| 8192 m breaks the van's float precision | Low | The moving origin keeps engine coordinates under about 1 km |
| The city makes this look like a driving game, not a horror game | Medium | Fog, dusk, no traffic, no pedestrians, lights mostly dead |

## Answers (user, 2026-10-24)

1. **World size: 8192 m.** `WorldPlan.Size` grows from 6144 to 8192, 8 x 8 terrain tiles of 1024 m, places about
   2 km apart.
2. **Blocks of about 120 m**, as the drawing has them: roughly 50 blocks across the city.
3. **Every street is drivable**, arterials and minor streets alike, so the crew can cut through the grid and pick
   their own route. Every kerb line therefore needs collision.
4. **The prison island is built now**: an island about 1.4 km off the west coast, a quay on the waterfront, the
   boat as the crossing. This closes the 2026-10-19 "the Prison stays a planned site" note in Docs/OPEN_WORLD_PLAN.md.
5. **The crew may walk anywhere.** Get out at any kerb and walk the city; buildings are solid, so nobody gets
   inside. Costs nothing extra (the city has no NavMesh either way), but pavements and block faces need their
   colliders from step 4 on, not as a later pass.
6. **All interior play stays in the nine locations.** The city is solid massing, permanently. A building you can
   enter is a contract, not scenery, which is also what keeps the budgets in "How it stays fast" reachable.
