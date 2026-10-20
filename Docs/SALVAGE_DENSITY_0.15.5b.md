# Salvage mall density pass — 0.15.5b

The user found 0.15.5 too empty. This pass fills the existing mall with stocked display surfaces,
layered recovery bays, dismantled shop fittings and packing/demolition remnants. It reuses the 45
original asset families and shared worn PBR materials, preserving the two-floor Nordic Habitat layout.

Unity authoring added **366 stock props** and **1,165 packing/demolition details**, organized into
**132 structural-section render batches**. The saved scene, navigation and culling data and both
0.15.5b.EEA player builds have been produced. These are construction counts, not gameplay or QA results.

The edit is additive: `Mall/_SALVAGE_DENSITY` holds all new content. Reauthoring replaces that layer
while retaining the saved building, original tenants, district, lighting, vehicles and gameplay objects.
The full mall builder also includes this layer for future regeneration.

## Denser retail and operations

- **A — Tech:** loaded display shelves, electronics cartons, partly emptied cases and grouped backroom
  recovery stock. Smaller boards and monitors occupy real display surfaces rather than random floor spots.
- **B — Furniture, both floors:** stocked material shelves, office-set accessories, flat-pack remnants,
  packing stacks and showroom partitions. Heavy staged furniture stays downstairs; upstairs emphasizes
  samples and office furnishings.
- **C — Hardware:** loaded tool shelving, workbench inventory, pipe and chest recovery groups, and packed
  tool-crib stock. The main hauling aisle stays available.
- **D — Apparel:** dense folded textiles, rack stock, nested handling bins, packed bales and stripped
  display remnants. Packing scraps reinforce the interrupted clearance operation.
- **E — Concourse:** taller pallet/crate sorting groups, classified staging stock and additional dry-fountain
  scrap. The original floodlights and generator remain the worksite lighting focus.

## Scene and physics authoring

`SalvageDensityPlacement` uses the current scene's fixture bounds and reserved carry routes, flights,
doorways, monster spawns and loot pads as construction constraints. Floor-standing objects retain simple
fitted collision. Small surface stock sits within its supporting fixture's collision; thin floor scraps
have no player-blocking collider. Added dressing binds to its actual supporting `StructuralSection`.

`SalvageDensityBatcher` combines geometry by material, source LOD and supporting slab. Saved mesh assets
live under `Assets/_Project/Art/Generated/Salvage0155/Density0155b/Meshes`. Three section-level LODs retain
the stock silhouettes; the most distant level drops flat packing scraps. Source holders retain their
collision and sparse labels. This avoids one separate renderer per small piece while allowing each
slab's art and collision to disappear independently when it collapses.

Existing authoritative loot definitions, spawn quantities, pickup flow, economy and shared carrying
remain in use. This is scene dressing, not a new pool of networked physics objects. `SalvageDensityDebug`
provides F1 construction counters and editor bounds for the section batches.

## Authoring and review

1. Open `Assets/_Project/Scenes/Mall.unity` in Unity.
2. To reproduce the layer, use **Tools > Abandoned > Maps > Mall > Updates > 0.15.5b Fill Salvage Mall**.
3. Review the denser stores and staging bays in your own playtest.

The authoring command saves the scene, navigation and culling data. The batch entry
`Abandoned.EditorTools.SalvageMallDensity.AuthorAndBuildPlayersAndExit` also builds Mac and Windows
players using one shared build stamp as **0.15.5b.EEA**. No tests, QA audits, screenshots or gameplay
verification are part of this pass; the user performs review afterward.
