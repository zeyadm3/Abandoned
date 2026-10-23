# Fire Station: Millbrook Fire Station No. 4 (2026-10-23)

A salvage location for the open world (Docs/OPEN_WORLD_PLAN.md, "Upcoming salvage locations" 5): its own scene,
`Scenes/FireStation.unity`, streamed in by distance when the van comes near. Built by code with the station kit the
police station introduced (`Editor/PoliceStation/Station*.cs`, Docs/POLICE_STATION_PLAN.md "Files"): layout data,
geometry, the shared run plumbing, the NavMesh and the occlusion bake.

**Menu:** Tools > Abandoned > Maps > Fire Station > Rebuild Fire Station (and Add Fire Station Loot).
**Code:** `Editor/FireStation/`, runtime `Scripts/Audio/FireStationSoundscape.cs`. **Loot data:** `Data/Loot/FireStation/`.
**Art:** `Art/FireStation/`. **Text:** `Data/Text/Resources/Text/fire_station.txt` (every item name and the location's
name and description in the nine languages).

Written without compiling, running or testing (the brief: Claude CLI compiles and reviews). The scene saved on disk is
from the first pass; it holds none of this until **Rebuild Fire Station** is run again.

**Second pass (2026-10-23): brought up to the precinct's depth.** The building grew from 48 x 36 m to 64 x 40 m with a
basement; the bay has four lanes (two engines and the ladder truck in, one empty for hauling); the hose tower has stairs
to a bell deck; the quarters are two full storeys of rooms; more hiding kinds, emergency lighting, 16 monster points,
42 loot items with two jackpots. The truck of its own is gone (the open world's van is the truck).

## The building
A 1930s brick firehouse, 64 x 40 m (16 x 10 cells of 4 m). South faces the street. The west half is the **apparatus
bay**, double height at the front; over its back third stand the bunk rooms, along its east side the hose mezzanine.
The east half is two storeys of quarters over a basement. The hose tower (8 x 12 m, to 20 m) stands on the north-west
corner as its own structure. Four bay doors (4.2 x 4.6 m) at x 4, 12, 20 and 28; **lane 2 (x 12) is the haul lane**:
empty, its door the way out, the van backs up to it. A personnel door to the watch office (x 38) and a rear door (x 34).

Ground floor (north up, x cells 0-15 left to right, 4 m each):
```
z9  T T W W W S S S . L L L L R R R     T hose tower (its own structure)   W workshop   S SCBA refill room
z8  T T W W W S S S . L L L L R R R     L laundry (carts, linen closet)    R restroom
z7  A A A A A A A A . m b . K K K K     m main stair up   b basement stair down   K kitchen (east end WEAK)
z6  A A A A A A A A . m b . K K K K
z5  A A A A A A A A . . . . K K K K
z4  A A A A A A A A . G G D D D D D     G turnout room (lockers, gear cage)   D day room (the jukebox)
z3  A A A A A A A A . G G D D D D D
z2  A A A A A A A A . G G D D D D D     . the hall (column 8 and the stair lobby)
z1  A A A A A A A A O O O C C r r r     O watch office (alarm board)   C captain's office   r records
z0  A A A A A A A A O O O C C r r r
     1   2   3   4                      bay lanes: 1 Engine 4, 2 the haul lane (empty), 3 Ladder 4, 4 Engine 9
```
Upper floor (y 4):
```
z9  T T S S S S S S . . A A A A A A     S storage loft (WEAK, old timber)   A attic storage
z8  T T S S S S S S . . A A A A A A
z7  T T S S S S S S . s X X H H H H     s the main stair's head   X radio room   H chief's quarters (closet)
z6  a a a a b b b b . s X X H H H H     a bunk room A   b bunk room B (both WEAK, over the bay)
z5  a a a a b P b b . t t t F F F F     P the pole hole (bunk room B; the pole-drop closet beside it)
z4  a a a a b b b b . t t t F F F F     t training room   F fitness room (WEAK)
z3  - - - - - - - m . t t t F F F F     m the hose mezzanine (WEAK, railed, over Engine 9)
z2  - - - - - - - m . O O O W W W W     O officers' quarters (lockers)   W bathroom
z0-1 - - - - - - - m . O O O W W W W    - the bay's double height
```
Basement (y -4, under the kitchen and the laundry):
```
z9  . . . . . . . . . a a a a a a a     a archive
z8  . . . . . . . . . = = = = = = =     = the basement corridor
z7  . . . . . . . . . f b u u u o o     f foam store   b the basement stair's foot
z6  . . . . . . . . . f b u u u o o     u utility room (breaker bank, boiler, standby generator)
z5  . . . . . . . . . . . u u u o o     o old store (the antique hand pumper, under the kitchen's weak east end)
```

## Signature pieces
- **The apparatus bay**: Engine 4 in lane 1, Ladder 4 (a 12 m aerial) in lane 3, Engine 9 in lane 4 under the
  mezzanine (when the mezzanine falls it lands on the engine). All three are solid. Lane 2 is kept clear to its door.
  At the back: gear cages and a turnout rack on the west wall, hose racks, SCBA cabinets, the pole's landing mat. The
  **Jaws of Life** (a jackpot) is in the bay.
- **The hose tower**: four steel flights switch back up its west and east halves to landings at 4, 8 and 12 m and the
  bell deck at 16 m, with open arches all round. Between the flights a 1 m shaft runs from the roof beam to the floor:
  the hoses hang in it to dry, and it is the quick way down for anything you can let go of. Monsters use the flights
  like any stair. The bronze station bell (heavy) is on the deck.
- **The fire pole**: a fixed ladder (`Core.Ladder`) through the pierced slab of bunk room B at cell (5, 5), down to the
  back of the bay. Climbed both ways, hands empty, like the factory's ladders.
- **The SCBA refill room** holds the **thermal imaging camera** (the other jackpot; one hand, worth almost as much).
- **The day room's jukebox** goes out through its wide west door, down the hall, through the wide bay door and the
  haul lane. The **antique hand pumper** is a Huge carry up the basement stair.

## Structure
- Every slab a 4 m section; both stairs Stair sections; the tower's landings Walkway sections.
- Collapsible on purpose (health / capacity):
  - **the bunk rooms over the bay** 0.65 / 0.55: they come down onto it;
  - **the hose mezzanine** 0.6 / 0.5, over Engine 9;
  - **the storage loft** 0.7 / 0.6, old timber over the workshop and the SCBA room;
  - the fitness room 0.75 / 0.65; the kitchen's east end (ground) 0.75 / 0.65, over the old store;
  - every other upper room at full strength;
  - **the tower's upper landings**: the 12 m landing 0.7 / 0.6, both halves of the bell deck 0.6 / 0.5.
- Never fall: the halls on both floors, the main stair and its lobby, the basement stair, the tower's 4, 8 and 16 m
  landings (where the flights meet), so there is always a way down the tower and out.
- Rubble never piles on the haul lane, the back of the bay by the hall door, the pole's landing, the hall, the stair
  lobby, the basement corridor or the basement stair's foot.
- Dressing standing on or hanging from a slab that can fall is bound to it (`SectionProp`, `StationBuild.BindToSlabs`);
  hiding places are bound too. The count is in the build line.
- Stability 0.68, seed 4141.

## Power and light
- **Breakers**: one bank in the basement utility room, a panel per HorrorConfig sector (6 by default); the sector of
  each fixture comes from its position (`HorrorConfig.SectorOf`), as in every map.
- Fixtures in every room: high bays in the apparatus bay, troffers and pendants elsewhere, bare lamps on each tower
  landing and the bell deck, all on the building's power.
- **Emergency lighting**: a battery unit over every door that opens on a hall and over every solid facade door
  (`StationLighting.Emergency`), lit when the power is off.

## Hiding places (HidingLocker, 28)
| Kind | Where |
|---|---|
| `Locker` (14) | turnout room (6), bunk room A (3), bunk room B (3), officers' quarters (2) |
| `Cabinet` as **gear cages** (6) | the bay's west wall (4), the turnout room, the SCBA room |
| `Cabinet` (3) | SCBA cabinets in the bay (2), the workshop |
| `Cart` (2) | laundry carts |
| `Closet` (3) | the **pole-drop closet** in bunk room B, the laundry's linen closet, the chief's wardrobe |

There is no "under the bunk" kind (`HideSpotKind` has none), so the bunk rooms have lockers and the pole closet
instead. A new kind would be runtime code in `Scripts/Extraction/Lockers/` (not this task's path).

## Monsters (16 points)
On every floor, deep inside, never on the carry route: the bay's back west corner and lane 4's back (2); the day room,
the kitchen, the laundry, the workshop and the SCBA room (5); the utility room and the archive (2); the tower's foot and
the bell deck (2); bunk room A, bunk room B, the storage loft, the fitness room and the chief's quarters (5).

## Loot (Data/Loot/FireStation, ids `fs_`, 42 items on the station's own tags)
| Tag | Where | Items |
|---|---|---|
| `fire_apparatus` (17) | bay, workshop, SCBA room, turnout room, foam store, old store | brass nozzle, fire axe, Halligan bar, breathing-air cylinder, leather helmet, **thermal imaging camera** (jackpot), **Jaws of Life** (jackpot, heavy), engine siren, portable pump, foam drum (sloshes), antique hand pumper (Huge), extinguisher, pike pole, turnout coat, SCBA mask, rescue saw, roof ladder section |
| `fire_quarters` (10) | day room, kitchen, bunk rooms, fitness, chief's and officers' quarters | alarm clock (rings), chili pot (sloshes), dalmatian statue, poker chips (rattle), television, espresso machine, guitar, dumbbells, vintage jukebox (Huge), medic bag |
| `fire_tower` (6) | the tower's floor, landings and bell deck | rolled hose, brass coupling, hydrant spanner, signal lantern, brass whistle, bronze station bell (heavy) |
| `fire_watch` (9) | watch office, captain's office, records, training room, radio room, attic, archive | alarm tape register, cast-iron alarm box, muster trophy, logbook, radio set, speaking trumpet, chief's helmet, parade banner, training dummy |

The ids `fs_thermal_camera` and `fs_rescue_spreader` were kept from the first pass and renamed, so their assets update
in place (same GUIDs). The old names stay in the sheet until the rebuild rewrites the assets.

## Sound
`LevelAmbience` and `FireStationSoundscape`: the watch office's and the radio room's radio static, hose couplings and
chains in the tower, drips in the bay drains, the bathroom, the laundry, the restroom and the basement. Existing sounds
only. **F1 draws every point** (`OnDrawGizmos`, `DebugView.Visible`).

## The open world
Authored at the scene's own origin; the world moves round it, never the scene.
- **Van parking**: an empty marker `FireStation/Exterior/VanParking` at the van's rear doors, **(12, 0, -11)**, forward
  **(0, 0, -1)** (cab south, toward the street; ramp north to the haul lane's door). Painted bay lines and a hatched
  loading box behind the doors. No truck of the map's own.
- **Ground rectangle**: x **-70..134**, z **-50..100** (third pass; was x -30..94, z -40..60); the edge's top is at **y -0.02** (every
  ground slab's top: roads, sidewalks, lots and yards are all slabs).
- **Paths for the world to hide**: none needed (no truck, sky, trees or grass). If the world would rather show its
  own town, `Exterior/Neighbourhood` holds every building and street piece (the streets' slabs stay ground).
- The NavMesh covers the building (the lot is left out): monsters reach the crew only inside.

## The town round it (third pass)
- **The town** (`FireStationTown`): Main Street in front of the apron, West Avenue (x -40..-30), East Avenue
  (x 98..108) and a back street behind the rear yard (z 64..74). Across Main Street: a gas station, three shops, a
  walk-up, row houses and a warehouse, a garage at the west corner, two shops at the east. West: a walk-up, a warehouse,
  row houses. East: a church on the corner, two houses set back in their yards, a shop. Behind: six clapboard houses
  in fenced yards and four row houses. Red fire-alarm street boxes, signals, lamps, power lines, road works on the back
  street, cars parked along the kerbs but none on the apron's side of Main Street, where the van comes and goes.
- The lot itself: the detailed engines and Ladder 4 in the bay (chocks under the front wheels), a retired pumper and a
  rescue van in the rear yard, a rolled car and a roof-cut car in the drill yard, the smoke container ribbed and
  sooted, barrel stoves, crates, a searchlight, a memorial with the old bell by the flagpole, kerbs along the
  station's sidewalks, scanned hydrants, crew cars and a covered car in the east lot, a camera and security light.

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
- Later the same day (the Hospital and Motel districts reuse the kit): three more building kinds (glass office tower,
  open parking deck with split ramps, strip mall under one canopy), `StationSite.GroundTop` (the ground's top the kit
  builds to: -0.02 here, 0 round the Hospital and Motel), and `StationCoverMarks`: open-framed furniture (tables, desks,
  bunks, gym kit, coat racks) and barred or doored hiding shells are solid on purpose, so they carry
  `IntentionalInvisibleCollider` with the reason. The first rebuild reported 79 (police) and 31 (fire) of them as
  invisible colliders; the scanned impound tyres are no longer solid.

## Files
| File | What |
|---|---|
| `FireStationLayout` | The plan: rooms on three floors, doors, the mezzanine's rails, the two stairs, the pole's pierced slab, the tower's custom cells, strength, the clear routes, van parking |
| `FireStationTower` | The hose tower (shell, four flights, landings as Walkway sections, the hose shaft, lamps, loot, monsters) and the fire pole |
| `FireStationBay` | The apparatus bay: two engines and Ladder 4, gear cages, turnout and hose racks, SCBA cabinets, lane lines |
| `FireStationGround`, `FireStationUpper`, `FireStationBasement` | Every room's dressing, hiding places, loot and monster points |
| `FireStationLoot` | The 42 loot items (data) |
| `FireStationExterior` | The lot, apron, van bay, training yard, car park, rear yard, roller drums, roof siren and plant |
| `FireStationRecipe`, `FireStationBuilder` | Mood, light colour, spawns, NavMesh exclusions, sound; the menu items |
| `Scripts/Audio/FireStationSoundscape.cs` | Runtime: radio static, couplings and chains in the tower, drips (F1 shows the points) |

Shared station kit changes in this pass (`Editor/PoliceStation/`): `StationFurniture` split into three partial files
(`.Domestic`, `.Extra`; each under 300 lines) with seeded desk and table clutter, bookcases, workbenches, radiators,
extinguishers, case boards, knocked-over chairs, litter and ceiling damage; `StationHiding.Kinds` (carts, gear cages);
`StationAccess` (fixed ladders, roof ladders); `StationLighting.Emergency`; `StationSite.VanBay`; `StationLootKit`
re-applies every item's data on each rebuild.

## Decisions for CLAUDE.md (to record; this doc does not edit it)
- 2026-10-23 (same brief as the police station): **Millbrook Fire Station No. 4** (`Scenes/FireStation.unity`,
  `Editor/FireStation/`), an open-world POI built with the station kit. Loot on its own tags (`fire_apparatus`,
  `fire_quarters`, `fire_tower`, `fire_watch`), ids prefixed `fs_`, data in Data/Loot/FireStation.
- Second pass (2026-10-23): **no truck of its own** (the van parks at `Exterior/VanParking`, rear (12, 0, -11), cab
  south); the haul lane is lane 2. **The hose tower has stairs** and is on the NavMesh (it replaced the first pass's
  ladder-only, monster-free tower); its 12 m landing and bell deck can fall. **The fire pole stays a ladder**. Two
  jackpots: the Jaws of Life and the thermal imaging camera.

## What to check first
1. **Compile**, then **Rebuild Fire Station** and read the Console: one `[FireStation] Built ...` line with every count
   (lights, emergency units, breakers, bound fittings, loot points and jackpots, hiding places by kind, monsters), and a
   warning for each fitting, loot point or monster spawn left out (with why). Expect 2 jackpot points.
2. **The haul lane**: walk from the van's ramp through the lane 2 door to the back of the bay; nothing solid in it. Then
   carry the jukebox out (day room, hall, wide bay door, lane 2) and the pumper up the basement stair.
3. **The tower**: climb the flights to the bell deck; the 12 m landing and the deck should creak and can fall, the
   others never. Drop a light item down the hose shaft and see it land at the foot. Check monsters path up the flights.
4. **The pole**: climb down from bunk room B and back up from the mat (`Core.Ladder` rules, hands empty).
5. **Weak floors**: the bunk rooms should go under a crew and a couple of items and come down into the bay; the
   mezzanine onto Engine 9; neither should start failing on its own. The halls and the stair lobby never fall.
6. **Power**: throw the utility room's breakers; each sector's fixtures go dark and the emergency units over the doors
   come on.
7. **Hiding**: try a gear cage, a laundry cart and the pole-drop closet.
8. **F1**: the soundscape points show as gizmos.
9. **Open world**: the world's survey should find `Exterior/VanParking`; the ground rectangle above is the lot's edge.
10. **Not done here (other owners)**: `LocationPlumbing.ConfigureHorrorScenes` drops `FireStation` from the
    HorrorConfig scene list whenever another map is rebuilt; no remaster or lighting profile yet; the stale generated
    assets in `Art/FireStation` and `Data/Loot/FireStation` are updated in place by the rebuild.
11. **Third pass (town and art)**: rebuild, then look round from the van bay and the street corners: buildings on every
    side, lamps lit, wires sagging between the poles, no car or lamp standing in a road, no building on a sidewalk.
    The build is bigger: watch the rebuild's time (the occlusion bake above all) and the frame rate on the street.
12. **Cars and furniture**: walk round a parked car (wheels in their arches, glass, lamps, a flat tyre sitting down),
    a cruiser, the engines; sit at a desk, look at the chairs, lockers and shelving. A Poly Haven scan that turns out
    to face the wrong way (a waiting chair, the sofa) is a one-number fix in `StationFurniture` (its local yaw).
