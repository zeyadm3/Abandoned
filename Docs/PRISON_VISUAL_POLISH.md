# Gallows Rock visual overhaul — 2026-10-19

Implementation only. The user explicitly requested **no testing or verification** and will do that work.
Unity was not launched; no scene rebuild, compilation, audits, tests, player builds or new review renders were run.
The saved Prison scene therefore still needs the rebuild below before these changes appear in play.

## Apply in Unity

In Edit mode, choose **Tools > Abandoned > Prison > Rebuild Island Prison**. The existing builder generates the
scene, material/mesh assets, structural sections, navigation and occlusion data. Save the scene when finished.
The audit/render menu is separate and was not invoked.

## Interior detail follow-up — 2026-10-19

The next source pass concentrates on the cells, corridors and occupied rooms. It is also implementation only;
the user's instruction against tests and verification remains in effect. Apply through the same rebuild menu.

- Cells gain eight shared furnishing conditions with different bedding, wear and pinned traces; spring/slat
  bunks, stitched/folded textiles, shaped sanitary fittings and plumbing, shelf/desk brackets, conduit and vents,
  scratching/tallies, paper remnants, and detailed door rollers/tracks/locks. Opposite rows choose their variations
  independently. New fine details stay inside the existing furnishing/cell envelopes, and the old ransacked
  mattress/blanket no longer project across the entry and centre lane. `IslandCellFurnishings`,
  `IslandCellSurfaceDetails` and `IslandCellHardware` feed the existing cell kits and side-block builders.
- Broadway and the side aisles gain overhead services, hangers, junctions and wall finishes. Gallery seams,
  splices and rivets live under each gallery's existing visual so they disappear with that structural section.
- Dining/kitchen fixtures, visiting booths, admin/control fittings and library shelving gain additional fixed
  construction detail: table welds/anchors and seat fittings, serving wells, range controls, exhaust baffles,
  open-frame prep tables/desks, filing drawers, dead control instruments, empty armoury rack fittings, booth
  gaskets/speaking grilles/phone tethers and bolted seats. Library ranks have open shelves and varied spines
  instead of solid blocks; the control cage has actual bars in place of opaque wall panels. `IslandInteriorDetails`,
  `IslandLibraryShelves` and `IslandOfficeDetails` are called by the existing interior builder. Small booth-seat
  colliders remain part of the navigation sources; the entire room-detail group is not excluded from navigation.
- `IslandInteriorSupport` attaches floor-mounted dining tables, serving fittings, trays/litter, kitchen ranges
  and prep fittings to their supporting collapsible tile. Attached detail follows the same collapse and restore
  events; it does not become floating scenery over the laundry.

Shared mesh kits and the existing prison materials carry the added density. The corridor's fixed cosmetic group
is excluded from navigation, and the existing loot, hiding and doorway routes remain the basis of the layout.
No additional realtime lights or per-cell update loops are introduced by this detail pass.

## Architecture and atmosphere

`Editor/Prison/IslandArchitecturePolish` adds the stepped civic entrance crown and stone seal, canopy coffers,
window reveals and sills, institutional dado finishes in the cell stacks, barracks/villa stonework, water-tower
bracing and hoops, and caged lantern models. Repeated detail uses shared meshes/materials. This group is excluded
from the navigation bake; wall-backed and overhead trim does not introduce new collision across routes.

The existing noon setting is retained. Outdoor fog density changes from .0045 to .0018 and interior fog from .024
to .009 so the bay and long galleries retain depth. Interior atmosphere bounds extend through the basement,
including crouched eye positions. The tall crossed light-shaft rectangles are removed from Broadway; actual
sunlight through the monitor and sparse lit dust provide the skylight treatment.
`IslandBackdrop` now carries daylight hill, mountain, skyline and bridge colours with height-aware sea haze, so
the far shore retains layered depth and the summits do not become a near-black wall beneath the noon sky.

`IslandLighting` raises the pendant output, improves the working/faulty/dead distribution, models open bulb cages
instead of opaque boxes, and places six working, power-controlled lamps along the basement haul corridor.
`IslandPresentation` adds a restrained photographic grade, local box-projected reflections in Broadway, the side
aisles, laundry and boiler area, and four local shadow-casting fixtures (two hall, two basement).
`Structure/IslandReflectionRefresh` captures nearby probes one at a time, slices captures across faces, and
invalidates them when the job's power/danger state changes. Headless sessions skip the captures.

## Basement repairs

`IslandBasement` and `IslandBasementDetails` preserve the masonry door openings in both tiled partition skins,
open up the laundry-cart rims, raise the shower fittings, adjust the shower seat and its loot height, lift the
fallen washer out of the floor, and replace solid bench blocks with framed fittings.
`IslandCellhouse` trims the downstand beam around the existing stair opening so it does not cross the last treads.

New fittings include detailed washer controls and vents, steam/water/return mains, collars and wall anchors,
grated drains, shower valves and screens, boiler gauges and tank plumbing, the mangle guard/wheel, and a wall sink.
These changes retain the existing layout, basement route, loot system and collapsible dining-floor behavior.

## Texture and visibility changes

`IslandPrisonSurfaces` is the prison's precision texture-scale pass, applied both to ordinary boxes/pipes and to
repeated kits before they are merged. This avoids stretching a single texture across a whole fitting or losing
thin-surface UV dimensions to the factory pass's coarse rounding.
`IslandPrisonArt` and `IslandPrisonMaterials` repair and bind the prison's material maps, including fittings that previously reset to
textureless colours during regeneration. Prison-specific material copies keep the other maps independent.
`IslandPrisonRendering` limits occluders to appropriate opaque, permanent geometry and installs the map's camera
range policy. Saved scene cameras receive a 5 km range; `Core/LevelCameraRange` keeps the active main camera at
that range after camera-controller updates and restores inherited values when the island unloads.
The texture generator also regenerates a set when a normal or mask PNG is missing.

No claim of successful compilation, visual quality, traversal or performance verification is made for this pass.
