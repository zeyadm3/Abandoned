# Remodel v2: architecture per map

Parked on 2026-10-28 (user: "for the architecture remodel per map just put it in a remodelv2.md file and I'll do it
later"). This is the one part of the 2026 remake that was not built. Docs/REMAKE_2026.md has the rest.

## What is left

The 2026 remake swapped **props** (a 75-model kit for 5,327 code-built props, `KitSwap`), vehicles, the HQ stations,
loot, gear, characters and monsters. The **architecture** was not touched: walls, floor slabs, stairs, columns,
facades and roofs are still the code-built geometry from each map's builder, mostly scaled cubes, carrying the
0.24.13 `Abandoned/Surface` materials and the `CoplanarFix` offsets.

Scaled-cube renderers per map (from `Game/Logs/remake/maps/<Map>_inventory.json`, `Art > Remake: Map Inventory`):

| Map | Renderers | Scaled cubes (architecture, mostly) |
|---|---|---|
| HQ | 5,189 | 3,401 |
| Mall | 10,319 | 3,628 |
| School | 24,945 | 9,999 |
| Iron Factory | 9,005 | 1,352 |
| Island Prison | 12,578 | 248 (built from its own meshes) |
| Tutorial House | 3,602 | 626 |
| Hospital | 29,533 | 9,650 |
| Motel | 24,585 | 8,508 |
| Police Station | 33,893 | 13,130 |
| Fire Station | 30,641 | 11,765 |
| Skyscraper | 47,815 | 26,210 |

## Why it was not done blind

- A wall or slab is a box whose size, materials and UV density were tuned per map (0.24.13 measured UV density; the
  scans tile from it). A kit module dropped on top changes how every surface reads.
- `CoplanarFix` moved faces by 1.25-2.5 mm to stop flicker. New modules land on those faces again; the pass must re-run
  and `GraphicsAudit`'s "within 0.1 mm (can flicker)" stay near 0.
- Structural slabs are `StructuralSection`s with crack and break stages (pre-fractured siblings). A remodelled slab
  needs its own fractured variants, or collapse falls back to the old look.
- Colliders, NavMesh and occlusion are baked from the current boxes. A swap must keep colliders where they are
  (like KitSwap does) or every map needs a NavMesh and occlusion rebake.
- It has to be looked at, map by map, in the editor.

## Suggested approach (when you pick it up)

1. **One map at a time, HQ first** (smallest, the most seen, and it was hand-edited: work on the saved scene, never
   regenerate it).
2. **Kit:** `Tools/Blender/remake/arch/` with a shared core (wall in 1/2/4 m lengths, with skirting and cornice trims;
   window and door surrounds; column with base and cap; slab edge; stair stringer and nosing; parapet and coping) and a
   style pack per map (palettes are in REMAKE_2026.md's art bible). Each module in `Art/Remake/Maps/Arch/<Map>`.
   Slabs come with intact, cracked and broken stages (the old `meridian_surfaces.py` way).
3. **Swap, like KitSwap:** match boxes by name and role (`Wall*`, `Slab*`, `Column*`, `Stair*`, `Roof*`, facade
   parents), fit a module to the box's bounds (tile, don't stretch), switch the box's renderer off, keep its
   collider. Record every swap so it can be undone (`KitSwapRecord` already does this).
4. **Detail first, replace second:** the cheapest real gain is *adding* trims (skirting, cornices, door and window
   casings, column caps) along existing walls, with no collider, rather than replacing the walls. Try that on the HQ
   before any full replacement.
5. After each map: `AvOverhaul.AfterMaps` (surfaces, CoplanarFix, lighting, MapSite), the level audit, the
   GraphicsAudit flicker count, and the mall's 12,500 render budget.

Budget: architecture modules 0.2-4k triangles at LOD0 (REMAKE_2026.md).

## What was built (2026-10-28)

User: "work on every single map ... don't stop to test or verify", then "full authority ... BUT YOU CAN NOT CHANGE THE
LAYOUTS", and "completely redo the lighting and shadows to match". Built and authored into the saved scenes;
**never played or tested** (the user does all QA once every map is done).

### How it works

One pass for all eleven maps instead of a kit per map, because the maps name their walls differently but all draw them
the same way: a scaled box (a cube or a 24-vertex box mesh). Code: `Game/Assets/_Project/Editor/Remake/Arch/`.

- **Find** (`ArchClassify`): every active box renderer. What it is made of comes from its material's name
  (`ArchProfiles.Classify`: brick, concrete, stone, profiled cladding, plaster, tile, boards, steel, glass, floor,
  ceiling; markings, lamps, vehicles, rails, bars, decals and props are never touched). What it is comes from its shape:
  wall, slab, ceiling, column, beam, stair tread, glass pane, pitched roof sheet. Left alone: anything inside something
  that moves or acts (doors, shutters, lockers, loot, vehicles, lights, machines), anything in a LOD group or a prop, and
  boxes too small or thin to be building fabric. Each map's report says how many were left alone and why.
- **Look around** (`ArchSpace`): every box's neighbours (exact box-in-box tests, physics for other geometry) answer:
  which stretches of a face are buried in a neighbour, is there a floor under it, a ceiling over it, sky above it (outside),
  is an end free (an opening's jamb), is the underside free (an opening's head), is there a drop off a slab edge.
- **Draw** a mesh fitted exactly to the box, in metres, in the map's style (`ArchStyles`, the art bible's palettes):
  - walls (`ArchWall`): chamfered exposed corners; faces cut with real relief (trapezoid ribs on profiled cladding,
    formwork V-joints on concrete, stone panel joints, board grooves, lap siding); skirting where a face meets a floor,
    a cornice where it meets a ceiling, wainscot and chair rails inside, casings up the jambs and along the heads of
    openings, coping on exposed tops, a plinth and storey bands outside. Every trim runs only along the open stretches.
  - slabs, ceilings, treads, roofs (`ArchSlab`): eased exposed edges (the walking plane unchanged), a nosing where there
    is a drop (galleries, landings, catwalks), a soffit finish on exposed undersides, a 1200 x 600 T-bar grid under
    suspended ceilings (on world lines so tiles meet), anti-slip nosings on stair treads (unless the map already models
    one), profiled ribs down the slope of pitched sheet roofs.
  - columns, beams, glazing (`ArchColumn`): steel columns become rolled H-sections on bolted base plates (on top of any
    existing pad), concrete and plaster ones get chamfered arrises, a plinth and a capital, clad ones stone joints and a
    metal foot; steel beams become I-sections; glass panes get a frame on every free edge.
- **Materials**: the box keeps its own surface (all are triplanar `Abandoned/Surface` in object metres, so the scans sit
  at the same size on the new geometry). Trims use each map's finishes (`ArchMaterials`): copies of the map's own scanned
  surfaces, tinted, in `Art/Remake/Maps/Arch/Materials/<Map>_<Key>.mat`. Where the geometry now carries what a scan only
  pictured, the body takes a finish instead (HQ and Factory profiled cladding: plain galvanised / sooty sheet over the
  real ribs, instead of the corrugated-steel photo).
- **Stand** (`ArchRebuild`): the drawing is a child of its box (scale undone so it reads in metres), so it moves with
  CoplanarFix and hides with its floor on collapse (SectionPresentation and SectionProp hide every renderer under the
  section). The box's renderer is removed and recorded (`ArchRecord` on the scene's `_Arch` object); its collider and
  every component stay, so **layout, NavMesh, occlusion, spawns, hiding spots and collapse are unchanged** (no rebakes).
  Meshes are de-duplicated into one bank per map (`Art/Remake/Maps/Arch/<Map>/ArchMeshes.asset`). A box whose drawing
  would exceed 4,000 quads stays a plain box. Re-running restores everything first; Undo restores only.
- **Shadows**: columns and beams always cast (the shapes a room reads by), glass never, everything else keeps the map's
  setting. Then the lighting is redone on top of the new architecture by the full map chain (below).

### Chain and menus

`AvOverhaul.AfterMaps` runs it right after the kit props (`Map remake: architecture`), so Rebuild All Maps keeps it, and
`KitSwap.Apply` takes the architecture off first. Then the chain redoes everything that depends on what the map draws:
scanned surfaces, weathering, ambient, room reflection probes, wear decals, the coplanar fix, the remaster, the lighting
rebuild (receiver-calibrated fixtures, bounce rigs, shadow settings, atmosphere, reflection fallbacks) and the map sites.
Menus (Docs/TOOL_MENU.md): **Tools > Abandoned > Art > Remake: Rebuild Architecture in Open Map / Undo Architecture in
Open Map / Rebuild Architecture in All Maps** (3107-3109). Logs: `Game/Logs/remake/arch/<Map>_rebuild.txt`, surveys
`<Map>_survey.txt`, review captures `shots/`.

### Numbers (Rebuild Entire Game, 2026-10-28)

| Map | Boxes rebuilt | Walls | Slabs | Columns | Beams | Treads | Panes | Roofs | Quads | Kept plain |
|---|---|---|---|---|---|---|---|---|---|---|
| HQ | 227 | 92 | 58 | 21 | 32 | 0 | 4 | 20 | 35,833 | 0 |
| Mall | 408 | 172 | 162 | 6 | 26 | 0 | 42 | 0 | 7,219 | 0 |
| School | 1,780 | 271 | 945 | 76 | 64 | 221 | 203 | 0 | 35,668 | 0 |
| Factory | 1,151 | 107 | 953 | 41 | 37 | 12 | 1 | 0 | 23,128 | 3 |
| Prison | 1,382 | 627 | 431 | 79 | 36 | 25 | 182 | 2 | 41,127 | 0 |
| Tutorial | 258 | 145 | 15 | 1 | 1 | 92 | 4 | 0 | 6,386 | 0 |
| Hospital | 2,772 | 1,156 | 977 | 224 | 31 | 151 | 231 | 2 | 87,833 | 0 |
| Motel | 935 | 269 | 512 | 28 | 12 | 38 | 68 | 8 | 22,182 | 0 |
| Skyscraper | 8,193 | 883 | 1,119 | 5,544 | 4 | 7 | 636 | 0 | 470,798 | 8 |
| Police Station | 1,429 | 436 | 768 | 27 | 11 | 79 | 102 | 6 | 39,180 | 0 |
| Fire Station | 1,485 | 455 | 680 | 69 | 16 | 133 | 126 | 6 | 36,766 | 0 |
| **All** | **20,020** | | | | | | | | **806,120** | **11** |

0 failures. Every box found was rebuilt; "left alone" counts per map are in `Game/Logs/remake/arch/<Map>_rebuild.txt`.
The Skyscraper's 5,544 columns are its curtain-wall mullions (clad, so plain chamfered shafts above 6 m).

The full chain hung once in the coplanar fix on the Police Station (a lookup table wiped in full for every small plane
after one big ground sheet; fixed in `GraphicsAudit.FindPairs`). If a chain stops part-way, `AvOverhaul.MapPasses(path)`
runs one map's per-scene passes, then `VisualRemaster.ApplySaved` and `MapSite.ApplySaved` finish it.

### Not done (honest list)

- No Blender modules: the pieces have to fit each box exactly (thousands of different lengths, openings and
  neighbours), so they are generated in the editor from profiles; no new textures were needed (every finish is a copy of
  a map's own CC0 scan). Blender was not used.
- Collapsing floors still break into the old pre-fractured debris (cosmetic, local); the intact and cracking stages
  show the new slab.
- Architecture drawn by non-box meshes (the Mall's 0.13.0 Meridian kit, the Tutorial's street houses, PolyHaven models)
  was already modelled and is unchanged.
- Never played: check first that nothing you used to see through is now solid-looking (or the reverse), that skirting
  and casings sit where they should (openings, room corners), stair nosings face the right way, suspended-ceiling grids
  look right, and frame rates on the Skyscraper and Hospital (most added geometry).
