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
