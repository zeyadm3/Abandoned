# Cloudline Tower — the downtown round it (2026-10-24)

User: "work on the surroundings of the skyscraper map, DON'T EDIT THE MAIN SKYSCRAPER, CHATGPT IS WORKING ON THAT, THE
MAIN BUILDING IS A NO EDIT ZONE". So this pass touches only the surroundings: the six `SkyscraperDowntown*.cs` files
(three rewritten, two new, the materials file unchanged) and the shared station kit. The tower, its block, the plaza,
Broadway's arrival bay in front of it, the truck, `SkyscraperBuilder`, `SkyscraperTowerShell`, `SkyscraperLayout`,
`SkyscraperWorld` and `Docs/SKYSCRAPER_PLAN.md` are untouched. Entry points kept: `SkyscraperDowntown.Build(exterior, art)`
(called by the tower shell) and `SkyscraperDowntown.Ground` (read by the world refresh), both unchanged in signature and
value.

Written, not compiled or rebuilt: **Tools > Abandoned > Maps > Skyscraper Restaurant > Rebuild Skyscraper Restaurant**
(or Rebuild New Maps) builds it.

## What is there

- **Street grid** (`SkyscraperDowntownStreets`): eight avenues along x (Broadway, at z -28, leaves the plaza's own
  street alone) and eight cross streets that stop at them, so no two road surfaces overlap any more (they used to at
  every junction, which flickers). Every block's pavement now runs to the road's edge with a granite kerb. Centre
  dashes, edge lines and zebras on all four arms of every junction, merged into one saved mesh (`StreetMarkings`). The
  tower's block keeps its saved pavements, filled out only by thin edge strips to the kerb line.
- **Towers** (`SkyscraperDowntownBuildings`, same heights and places as before): mullion fins every 3–5 m and a
  spandrel band every four floors on both tiers (their own saved meshes), a terrace parapet at the setback, podium
  shopfronts on all four sides with sign bands (some lit), awnings, revolving doors and planters, and four crowns: a
  stepped glass crown with a lit band and a spire; an art-deco stack with brass ribs, a lantern and a needle; a blade
  with a mast and a helipad; stone fins over water tanks and a roof garden. Corner beacons on everything over 300 m;
  a window-cleaning cradle left hanging on some.
- **Street life** (`SkyscraperDowntownStreetscape`, scene-only): on every block lamps at the corners and mid-block,
  signals on the central junctions, hydrants, news boxes, mailboxes, benches, bins, parking meters, bus shelters, a
  dumpster or litter, tree pits with street trees. Cars at the kerbs of every avenue and one side of every cross
  street, taxis among them. **Broadway gridlocked outbound** where the evacuation stopped, **police barricades** a block
  either side of the tower with cruisers nosed in, an ambulance left at the kerb. The six lamps nearest the plaza are
  still lit; the rest are dark.
- **Two outer rings** (`SkyscraperDowntownRings`, scene-only): where the towers stop, the station kit's rings (Docs/
  POLICE_STATION_PLAN.md "The outer rings") with a district mix (offices, walk-ups, parking decks, shops, churches,
  houses at the edge), 12 m ring roads, and every avenue and cross street running on to the new edge. The scene now
  reaches x -486..486, z -464..508.

## Why "scene-only" (under `Exterior/Skyline`)

- The open world hides `Skyscraper/Exterior/Skyline` (WorldPlan), so in the world its own land stands where the rings
  are; the downtown grid, its towers and its markings still show there as before.
- The builder's occlusion pass (`SkyscraperBuilder.ApplyOcclusionFlags`) makes every broad renderer an occluder and
  static-batched except under `Exterior/Skyline`. The hundreds of building, car and furniture copies stay out of it, so
  the occlusion bake and the memory for static batches don't grow with them. The merged street markings are named as a
  floor for the same reason.
- The rings' ground slabs are 3 m thick, so the world's survey (which reads only slabs thinner than 2.5 m) keeps
  measuring the 720 m grid as the site's ground, as `SkyscraperWorld` sets it.
- Building, car and block-furniture copies come from `StationRingStock`: each design built once, merged into one saved
  mesh with a far mesh of its big parts (`Art/Skyscraper/Downtown/RingMeshes`).

## What to check first

1. Rebuild, then from the plaza: Broadway both ways (the jam, the barricades, the lit lamps), the truck and the
   forecourt exactly as before.
2. From the restaurant at 404 m: the neighbouring crowns, fins and bands, the beacons, the rings and their streets
   out to the edge.
3. Walk a block: kerbs, zebras, nothing standing in a road or a doorway, no road flicker at the junctions.
4. Watch the rebuild time (the occlusion bake above all), the scene's size and the frame rate looking out from the top.
5. In the open world (after Rebuild World): the downtown as before, the world's own land where the rings stand in the
   scene.
