# Police Station: Millbrook 3rd Precinct (2026-10-23)

A new salvage location for the open world (Docs/OPEN_WORLD_PLAN.md, "Upcoming salvage locations" 4): its own scene,
`Scenes/PoliceStation.unity`, which the world streams in by distance when the van comes near. Built by code like the
Iron Factory and the island prison: layout data, then geometry, then the shared run plumbing (`LocationPlumbing`),
then the NavMesh and the occlusion bake.

**Menu:** Tools > Abandoned > Maps > Police Station > Rebuild Police Station (and Add Police Station Loot).
**Code:** `Editor/PoliceStation/` (the station kit `Station*.cs`, shared with the fire station, plus `PoliceStation*.cs`),
runtime `Scripts/Audio/PoliceStationSoundscape.cs`. **Loot data:** `Data/Loot/PoliceStation/`. **Art:** `Art/PoliceStation/`.

**Text:** `Data/Text/Resources/Text/police_station.txt` (every item name and the location's name and description in the
nine languages).

Written without compiling, running or testing (the brief: Claude CLI compiles and reviews). The scene saved on disk is
from the first pass; it holds none of the second pass until **Rebuild Police Station** is run again.

**Second pass (2026-10-23):** the basement grew east (the evidence vault, basement records, the evidence intake under the
sally port); a roof hatch and ladder; dead cruisers in the car park and a fuller impound yard; more dressing variety in
every room; more of the upper floor over the cell block can fall; 22 hiding places; emergency lighting; 39 loot
items. The truck of its own is gone (the open world's van is the truck).

## The building
A two-storey brick precinct with a basement, 56 x 40 m on the mall's 4 m grid (14 x 10 cells), storeys 4 m apart
(slab 0.25 m), flat roof at 8 m with a parapet. South faces the street (public entrance), east is the sally port, where
the crew's van backs up to the open vehicle door. That is the carry route: everything goes out through the sally port.

Ground floor (north up, x 0-13 left to right, z 0-9 bottom to top; `.` = open hall):
```
z9  L L L H H H H I I O W W W c      L locker room (officers' lockers: hiding)     H holding cells (hiding)
z8  L L L H H H H I I O W W W c      I interview room   O observation   W briefing room   c janitor closet
z7  L L L H H H H I I O W W W ^      ^ rear stair (up to the north)
z6  L L L H H H H B B B D D D ^      B booking   D dispatch
z5  . s M H H H H B B B D D D .      s basement stair (hole, down to the south)   M main stair (up to the north)
z4  . s M . . . . B B B P P P P      P sally port (vehicle door in the east wall, the van outside)
z3  . . . . . . . . . . P P P P      main hall (row 3): the haul spine to the sally port
z2  R R R Y Y Y Y Q Q Q P P P P      R records   Y lobby (public door in the south wall)   Q squad room
z1  R R R Y Y Y Y Q Q Q P P P P
z0  R R R Y Y Y Y Q Q Q P P P P
```
Upper floor:
```
z9  E E E E C C C K K K K . . .      E evidence room (WEAK floor over the locker room and cells)
z8  E E E E C C C K K K K h . .      C captain's office (WEAK, over the cells)   K break room   h roof hatch (ladder)
z7  E E E E C C C K K K K . . o      o rear stair hole (it lands on row 8)
z6  . . . . . . . . . . . . . o      upper hall (row 6) and landings: never collapse
z5  . . m V V V V V V V G G G G      m main stair hole (it lands on row 6)   V detectives' bureau (back row x 3-6 WEAK)
z4  . . m V V V V V V V G G G G      G gym (weak floor, over the sally port)
z0-3 A A A V V V V V V V G G G G     A records archive (weak)
```
Basement (cells x 0-12, z 4-9 only):
```
z7-9  F F F F F X X X V V V r r     F firing range (four lanes)   X armoury (the rifle locker)   r basement records
                                    V evidence vault: round steel door swung open, walls of deposit drawers (a jackpot)
z6    . . . . . . . . . . . . .     basement corridor
z4-5  b u P P P G G G I I I I I     b boiler room   u under the basement stair   P property storage
                                    G generator room (breakers)   I evidence intake (counter, photo stand, lockers)
```
Doors are 1.8 m; the haul doors are 3.2 m (lobby, sally port, booking, evidence room, property storage, armoury,
the vault, the intake), so Huge loot fits. Exterior: the lobby's glazed door (3.2 m), the sally port's raised roller
door (6 m x 3.4 m), and windows on both storeys (glass, solid; some boarded).

## Roof access
A 1.3 m hatch in the roof over the upper hall by the rear stair (cell 11, 8), with a fixed ladder up to it
(`StationAccess.RoofLadder`, `Core.Ladder` rules: players only, hands empty, off the NavMesh). On the roof: the radio
mast, the plant and three loot points. The ladder needs empty hands (pockets are fine), so anything bigger comes down
by dropping it through the hatch or over the parapet.

## Structure
- Every slab is a 4 m structural section (`StationFloors`), every flight a non-collapsing Stair section.
- Upper floor: halls and landings never fall (`SectionKeepClear`, not collapsible). Rooms do. Weak on purpose:
  **the evidence room** (health 0.6, capacity 0.5: a full evidence haul plus a crew is what brings it down into the
  locker room and cells), **the gym** over the sally port (0.7 / 0.6: dropping weights on it is a bad idea), and the
  **records archive** (0.8 / 0.7).
- **Over the cell block**: the captain's office (0.6 / 0.5) and the bureau's back row (0.7 / 0.6) give way onto the
  holding cells, beside the evidence room. The cells' ceiling shows it (cracks, hanging tiles, exposed lath:
  `StationFurniture.CeilingDamage`), so the drama is readable from below before anything falls.
- Ground floor over the basement: the locker room's shower corner (four tiles, 0.7 / 0.6) is water-rotted and drops
  into the firing range. Everything else on the ground never falls.
- Rubble never piles on the haul spine (row 3), the cell corridor, the stair cells or the sally port (`SectionKeepClear`).
- Fittings standing on or hanging from a collapsible slab go with it (`SectionProp`, `StationBuild.BindToSlabs`), lights
  included; the count is in the build line.
- Occlusion is baked after every rebuild (`OcclusionBake`), never-breaking geometry only.
- Stability 0.70, seed 7313.

## Run plumbing
- Crew spawns between the van's bay and the sally port door. No truck of the map's own.
- The NavMesh covers the building only (the lot and the roof are left out), so monsters reach the crew only once they
  are inside.
- Power: a breaker bank in the basement generator room, a panel per HorrorConfig sector (6 by default; lights only).
  Emergency units over every hall door and solid facade door come on when the power is off
  (`StationLighting.Emergency`).
- Sound: `LevelAmbience` (wind, fluorescent hum, the building settling) and `PoliceStationSoundscape` (a police
  scanner crackling in dispatch, cell doors knocking in the cell block, a distant siren, the basement dripping).
  Existing `SoundId`s only: no new sound assets. **F1 draws every soundscape point** (`OnDrawGizmos`,
  `DebugView.Visible`).

## Loot (Data/Loot/PoliceStation, the station's own tags)
| Tag | Where | Items |
|---|---|---|
| `police_office` | records, lobby, squad room, bureau, captain, briefing, dispatch, break room | case files, desk computer, typewriter, desk phone (rings), coffee maker, bowling trophy, commendation plaque, dress cap, fingerprint kit, filing cabinet; jackpots: **police call box**, **trophy case** |
| `police_cells` | booking, holding, interview | booking camera, fingerprint scanner, breathalyser, inmate property bag, line-up height board, handcuffs |
| `police_evidence` | evidence room, property storage, the vault, the intake | seized cash, bagged jewellery, seized laptop, counterfeit handbag, car stereo, stolen painting, gold bar, slot machine, gold watch, evidence scale, body camera, vault ledger; jackpots: **seized safe**, **impounded motorcycle** |
| `police_armoury` | armoury, firing range, locker room | ammo can (rattles), tactical vest, riot helmet, police radio, breaching ram, spotting scope; jackpot: **rifle locker** |

39 items: office 12, cells 6, evidence 14, armoury 7; 5 jackpot items. Jackpot points (5): the evidence room
(upstairs, weak floor), the evidence vault and the armoury (basement: up the stairs with it), the lobby, the captain's
office (over the cells). Three more loot points are on the roof.

## Hiding places (HidingLocker)
22 in all: nine holding cells (`Cell`, seen through the bars), eight officers' lockers (`Locker`), three cabinets in
the evidence room, property storage and the evidence intake (`Cabinet`), the janitor's closet and the interview room
closet (`Closet`).

## Monsters
12 points on all three floors, at least 22 m from the sally port door: the locker room, the cell block, the interview
room and the briefing room; the records archive, the bureau, the evidence room and the break room; the firing range,
the basement corridor, the evidence vault and basement records. Every
spawn and loot point is checked against the built colliders before it is kept (`StationBuild`): one that would stand
inside a wall or a fitting is left out and reported.

## Outside
The street and sidewalk to the south; the patrol car park to the west with **dead cruisers** (parked rows, one burnt
out, one with its doors hanging open, two that ran into each other on the way out, the evidence van, one nosed into
the kerb in the street); the **impound yard** behind (fenced, its east gate left open, the guard's booth, a tow truck,
rows of seized cars, one craned up onto another); the east yard with the van's bay. All solid, all outside the NavMesh.

## The open world
Authored at the scene's own origin, everything under one root (`PoliceStation`); the world moves round it, never the
scene.
- **Van parking**: an empty marker `PoliceStation/Exterior/VanParking` at the van's rear doors, **(67, 0, 10)**, forward
  **(1, 0, 0)** (cab east; ramp west to the sally port door at (56, 0, 10)). Painted bay lines and a hatched loading box.
  No truck of the map's own.
- **Ground rectangle**: x **-70..128**, z **-48..100** (third pass; was x -30..86, z -30..60); the edge's top is at **y -0.02** (every
  ground slab's top: roads, sidewalks, lots and yards are all slabs).
- **Paths for the world to hide**: none needed (no truck, sky, trees or grass). If the world would rather show its
  own town, `Exterior/Neighbourhood` holds every building and street piece (the streets' slabs stay ground).

## The town round it (third pass)
- **The town** (`PoliceStationTown`): Main Street in front, West Avenue (x -40..-30), East Avenue (x 90..100) and a back
  street behind the impound (z 64..74), with crosswalks and signals at the corners. Across Main Street: two shops, three
  row houses, a walk-up, a diner, the church and a corner shop, a warehouse at the west corner and a gas station at the
  east. West of West Avenue: a walk-up, a garage, a warehouse. East of East Avenue: shops, a walk-up, row houses. Behind:
  five clapboard houses in fenced yards with paths and driveways, row houses and a corner shop. An abandoned police
  roadblock (barriers, sawhorses, cones, two cruisers) across Main Street east of East Avenue. Cars parked along the
  kerbs, never across the station's front or the way into the east yard.
- The lot itself: bollards, planters, a monument sign with the badge, the badge over the lobby canopy, a bike rack,
  cameras and security lights on the corners, the generator, transformer and gas bottles in the east yard, more cars
  with real wear, the evidence van and tow truck rebuilt, impounded cars with some under tarps.

## The neighbourhood and the art kit (third pass, 2026-10-23)
User: "add buildings around them, improve the entire level design ... the surrounding scenery, the inside ... and the
exterior ... more detailed assets for the cars, the chairs, everything". Shared station kit, `Editor/PoliceStation/`:
- **Town** (`StationTownBuildings`, `StationTownLandmarks`, `StationTownFacade`, `StationTownArt`, `StationTownPlan`):
  original buildings made in code, exterior only (doors shut, no rooms), each solid: row houses with stoops and
  areaway railings, shops with flats over them (shopfronts, sign bands with a maker's mark and no lettering, awnings,
  half-down roller shutters, posters), walk-up apartment blocks (fire escapes, window air-conditioners, rooftop water
  tanks, stair bulkheads, dishes), warehouses with loading docks and roller doors, an auto garage, clapboard houses
  with gable roofs, porches and shutters, a brick church with its belfry, rose window and spire, a gas station with
  canopy, pumps, kiosk and price sign. Windows have frames, mullions, sills, lintels and grime streaks; panes are glass,
  blinds, dark, boarded or broken by seed. Photographed brick, corrugated iron and weathered concrete (the Poly Haven
  ground scans already in Art/TestMap/Ground, read only) beside the code-made brick, siding and stucco.
- **Streets** (`StationStreet`): roads and sidewalks as ground slabs (so the world's survey reads them), double yellow
  centre lines, edge lines, drains, manholes, tar patches, flagged sidewalks with kerbs, crosswalks at every corner;
  scanned street lamps that still light (spot, no shadows), power poles from the scanned kit with sagging wires,
  traffic signals on mast arms, stop signs and street-name blades (blank), a bus shelter, benches, mailboxes, news
  boxes, parking meters, hydrants, dumpsters, bollards, sawhorses, concrete barriers, litter.
- **Vehicles** (`StationVehicles`, `StationVehicleShape`, `StationApparatus`, `StationMeshes`): every car is a smooth
  lofted body with cut wheel arches, a tinted glasshouse under a painted roof, pillars, lathed tyres and rims, lamps,
  grille, bumpers, plates, mirrors, handles and wipers; sedan, hatchback, SUV, pickup and police cruiser (light bar,
  push bar, spotlight, white doors). Wear: flat tyres, crazed glass, dust, an open door, burnt out on its rims, roof
  cut off. Fire pumpers and the aerial ladder truck (custom cab, roll-up compartments, pump panel, hose bed, racked
  ground ladders, dual rears, lattice aerial on its turntable), a box van and a tow truck. Meshes are saved per station
  in `Art/<Station>/Meshes`.
- **Furniture** (`StationFurniture*.cs`, same method names as before): office chairs on five-star bases with casters and
  padded cushions, scanned waiting chairs and sofa, desks with drawers, locks, CRT or flat monitors, phones and scanned
  desk lamps, binders and stationery, filing cabinets with a drawer left out, braced shelving with lidded and
  labelled boxes, lockers with vents, number plates and the odd one ajar, slatted crates, scanned barrels, lathed
  toilets, basins and pedestals, bunks with ladders and rumpled bedding, a CRT television, a fridge with notes on it,
  a range with a pot, gym kit with lathed plates.
- **The station's own walls** (`StationFacadeDetail`): each window framed, mullioned and transomed in its reveal, a
  stone lintel and keystone, the grime streak under the sill; brick pilasters on the bay lines, stone quoins, a
  dentilled cornice under the coping, downpipes with hoppers and shoes.
- Small renderers outside stop being occluders (`StationSite.TrimOccluders`): the occlusion bake works on building
  masses and walls only, so it should not grow like the Skyscraper's.

## Files
| File | What |
|---|---|
| `StationPlan`, `StationArt`, `StationFloors`, `StationWalls`, `StationFacade` | The station kit (shared with the fire station): grid plan, materials, slabs and flights, interior walls and railings from the grid, the brick shell with its windows and openings |
| `StationProps`, `StationFurniture` (+ `.Domestic`, `.Extra`), `StationHiding` (+ `.Kinds`), `StationAccess`, `StationLighting`, `StationSite`, `StationSurfaces` | Fittings (kept off doorways, flights and openings, reported when left out), furniture models and seeded clutter, hiding places (lockers, cabinets, closets, cells, carts, gear cages), fixed and roof ladders, light, emergency units and breakers, the lot, cars, fences and the van bay, world-scale UVs |
| `StationLootKit` | Loot as data: definitions, part-list models, prefabs, catalog, icons |
| `StationRecipe`, `StationContext`, `StationBuild` | The build pipeline, the same order as the factory and the island |
| `PoliceStationLayout` | The plan (rooms, doors, stairs, strength, haul route) |
| `PoliceStationGround`, `PoliceStationUpper`, `PoliceStationBasement`, `PoliceStationVault` | Each storey's fittings, hiding places, loot and monster points; the roof; the vault, basement records and intake |
| `PoliceStationLoot` | The 39 loot items (data) |
| `PoliceStationExterior` | The lot, the car park's dead cruisers, the impound yard, entrance canopy, roof plant, the van bay |
| `PoliceStationRecipe`, `PoliceStationBuilder` | Mood, light colour, spawns, sound; the menu items |
| `Scripts/Audio/PoliceStationSoundscape.cs` | Runtime: scanner static, distant door slams, drips (F1 shows the points) |

## Decisions for CLAUDE.md (to record; this doc does not edit it)
- 2026-10-23 (user brief, "build the police station and the fire station, don't test or run anything"): **the
  Millbrook 3rd Precinct** (`Scenes/PoliceStation.unity`, `Editor/PoliceStation/`), an open-world POI streamed by
  distance. The carry route is the sally port (truck backed up to the east vehicle door). Loot on its own tags
  (`police_office`, `police_cells`, `police_evidence`, `police_armoury`), ids prefixed `ps_`, data in
  Data/Loot/PoliceStation. The evidence room's weak floor is its structural headline.
- **The station kit** (`Editor/PoliceStation/Station*.cs`) builds any grid-plan station from data: one wall per
  cell edge between different rooms, doors from the plan, fittings refused (and reported) if they would block a
  doorway, flight or opening, loot and monster points checked against the built colliders. The fire station uses it.
- New maps' loot ids carry a map prefix and their own tags, so no location's loot leaks into another's.
- Second pass (2026-10-23): **no truck of its own** (the van parks at `Exterior/VanParking`, rear (67, 0, 10), cab
  east). The basement's evidence vault holds a jackpot; the roof is reached by a hatch ladder; the captain's office and
  the bureau's back row are weak over the cell block.

## What to check first
1. **Compile**, then **Rebuild Police Station** and read the Console: one `[PoliceStation] Built ...` line with every
   count (lights, emergency units, breakers, bound fittings, loot points and jackpots, hiding places by kind,
   monsters), and a warning for each fitting, loot point or monster spawn left out (with why). Expect 5 jackpot points.
2. **The vault**: down the basement stair, east along the corridor, through the wide door past the swung-open round
   door. Carry the vault's jackpot out: corridor, basement stair, main hall (row 3), sally port, van.
3. **Over the cells**: stand a crew with a heavy item in the captain's office; it and the bureau's back row should come
   down into the holding cells, not onto the cell corridor (kept clear) or the upper hall (never falls).
4. **The roof**: climb the ladder from the upper hall by the rear stair, step out of the hatch, walk the roof inside the
   parapet, climb back down.
5. **The evidence room** still holds a resting safe but goes with a crew and a full haul; nothing fails on its own.
6. **Outside**: the cruisers, the impound yard's cars and the tow truck are solid; the van's bay and the sally port
   door are clear.
7. **Power**: throw the generator room's breakers; each sector goes dark and the emergency units come on.
8. **F1**: the soundscape points show as gizmos.
9. **Open world**: the world's survey should find `Exterior/VanParking`; the ground rectangle above is the lot's edge.
10. **Not done here (other owners)**: `LocationPlumbing.ConfigureHorrorScenes` resets the HorrorConfig scene list
    whenever another map is rebuilt, dropping `PoliceStation` until this one is rebuilt (worth making it additive); no
    remaster or lighting profile yet; the generated assets in `Art/PoliceStation` and `Data/Loot/PoliceStation` are from
    the first pass and are updated in place by the rebuild.
11. **Third pass (town and art)**: rebuild, then look round from the van bay and the street corners: buildings on every
    side, lamps lit, wires sagging between the poles, no car or lamp standing in a road, no building on a sidewalk.
    The build is bigger: watch the rebuild's time (the occlusion bake above all) and the frame rate on the street.
12. **Cars and furniture**: walk round a parked car (wheels in their arches, glass, lamps, a flat tyre sitting down),
    a cruiser, the engines; sit at a desk, look at the chairs, lockers and shelving. A Poly Haven scan that turns out
    to face the wrong way (a waiting chair, the sofa) is a one-number fix in `StationFurniture` (its local yaw).
