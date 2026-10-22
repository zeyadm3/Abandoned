# Police Station: Millbrook 3rd Precinct (2026-10-23)

A new salvage location for the open world (Docs/OPEN_WORLD_PLAN.md, "Upcoming salvage locations" 4): its own scene,
`Scenes/PoliceStation.unity`, which the world streams in by distance when the van comes near. Built by code like the
Iron Factory and the island prison: layout data, then geometry, then the shared run plumbing (`LocationPlumbing`),
then the NavMesh and the occlusion bake.

**Menu:** Tools > Abandoned > Maps > Police Station > Rebuild Police Station (and Add Police Station Loot).
**Code:** `Editor/PoliceStation/` (the station kit `Station*.cs`, shared with the fire station, plus `PoliceStation*.cs`),
runtime `Scripts/Audio/PoliceStationSoundscape.cs`. **Loot data:** `Data/Loot/PoliceStation/`. **Art:** `Art/PoliceStation/`.

Written without compiling, running or testing (the brief: Claude CLI compiles and reviews). The scene does not exist
until the rebuild menu is run in Unity.

## The building
A two-storey brick precinct with a basement, 56 x 40 m on the mall's 4 m grid (14 x 10 cells), storeys 4 m apart
(slab 0.25 m), flat roof at 8 m with a parapet. South faces the street (public entrance), east is the sally port, where
the company truck backs up to the open vehicle door. That is the carry route: everything goes out through the sally port.

Ground floor (north up, x 0-13 left to right, z 0-9 bottom to top; `.` = open hall):
```
z9  L L L H H H H I I O W W W c      L locker room (officers' lockers: hiding)     H holding cells (hiding)
z8  L L L H H H H I I O W W W c      I interview room   O observation   W briefing room   c janitor closet
z7  L L L H H H H I I O W W W ^      ^ rear stair (up to the north)
z6  L L L H H H H B B B D D D ^      B booking   D dispatch
z5  . s M H H H H B B B D D D .      s basement stair (hole, down to the south)   M main stair (up to the north)
z4  . s M . . . . B B B P P P P      P sally port (vehicle door in the east wall, the truck outside)
z3  . . . . . . . . . . P P P P      main hall (row 3): the haul spine to the sally port
z2  R R R Y Y Y Y Q Q Q P P P P      R records   Y lobby (public door in the south wall)   Q squad room
z1  R R R Y Y Y Y Q Q Q P P P P
z0  R R R Y Y Y Y Q Q Q P P P P
```
Upper floor:
```
z9  E E E E C C C K K K K . . .      E evidence room (WEAK floor over the locker room and cells)
z8  E E E E C C C K K K K . . .      C captain's office   K break room
z7  E E E E C C C K K K K . . o      o rear stair hole (it lands on row 8)
z6  . . . . . . . . . . . . . o      upper hall (row 6) and landings: never collapse
z5  . . m V V V V V V V G G G G      m main stair hole (it lands on row 6)   V detectives' bureau (open plan)
z4  . . m V V V V V V V G G G G      G gym (weak floor, over the sally port)
z0-3 A A A V V V V V V V G G G G     A records archive (weak)
```
Basement (cells x 0-7, z 4-9 only):
```
z7-9  F F F F F X X X      F firing range (four lanes)   X armoury (jackpot: the rifle locker)
z6    . . . . . . . .      basement corridor
z4-5  b u P P P G G G      b boiler room   u under the basement stair   P property storage   G generator room (breakers)
```
Doors are 1.8 m; the haul doors are 3.2 m (lobby, sally port, booking, evidence room, property storage, armoury), so
Huge loot fits. Exterior: the lobby's glazed door (3.2 m), the sally port's raised roller door (6 m x 3.4 m), and
windows on both storeys (glass, solid; some boarded).

## Structure
- Every slab is a 4 m structural section (`StationFloors`), every flight a non-collapsing Stair section.
- Upper floor: halls and landings never fall (`SectionKeepClear`, not collapsible). Rooms do. Weak on purpose:
  **the evidence room** (health 0.6, capacity 0.5: a full evidence haul plus a crew is what brings it down into the
  locker room and cells), **the gym** over the sally port (0.7 / 0.6: dropping weights on it is a bad idea), and the
  **records archive** (0.8 / 0.7).
- Ground floor over the basement: the locker room's shower corner (four tiles, 0.7 / 0.6) is water-rotted and drops
  into the firing range. Everything else on the ground never falls.
- Rubble never piles on the haul spine (row 3), the cell corridor, the stair cells or the sally port (`SectionKeepClear`).
- Fittings standing on or hanging from a collapsible slab go with it (`SectionProp`), lights included.
- Stability 0.70, seed 7313.

## Run plumbing
- Crew spawns between the truck and the sally port door; the truck (the usual `TruckBuilder` truck) faces east, its
  ramp toward the building; the collection bay markings come from `MeridianCollectionBayBuilder`.
- The NavMesh covers the building only (the lot, the truck and the roof are left out), so monsters reach the crew only
  once they are inside.
- Power: six breakers in the basement generator room (one per sector, `BreakerPanel`, lights only).
- Sound: `LevelAmbience` (wind, fluorescent hum, the building settling) and `PoliceStationSoundscape` (a police
  scanner crackling in dispatch, cell doors knocking in the cell block, a distant siren, the basement dripping).
  Existing `SoundId`s only: no new sound assets.

## Loot (Data/Loot/PoliceStation, the station's own tags)
| Tag | Where | Items |
|---|---|---|
| `police_office` | records, lobby, squad room, bureau, captain, briefing, dispatch, break room | case files, desk computer, typewriter, desk phone (rings), coffee maker, bowling trophy, commendation plaque, dress cap, fingerprint kit, filing cabinet; jackpots: **police call box**, **trophy case** |
| `police_cells` | booking, holding, interview | booking camera, fingerprint scanner, breathalyser, inmate property bag, line-up height board, handcuffs |
| `police_evidence` | evidence room, property storage | seized cash, bagged jewellery, seized laptop, counterfeit handbag, car stereo, stolen painting, gold bar, slot machine; jackpots: **seized safe**, **impounded motorcycle** |
| `police_armoury` | armoury, firing range, locker room | ammo can (rattles), tactical vest, riot helmet, police radio, breaching ram, spotting scope; jackpot: **rifle locker** |

Jackpot sites: the evidence room (upstairs, weak floor), property storage and the armoury (basement: up the stairs with
it), the lobby, the captain's office.

## Hiding places (HidingLocker)
Holding cells (`Cell`, seen through the bars), the officers' lockers (`Locker`), evidence and property cabinets
(`Cabinet`), the janitor's closet and the interview room closet (`Closet`).

## Monsters
Spawns deep inside, at least 22 m from the sally port door: the briefing room, the locker room, the cell block, the
interview rooms, the evidence room, the break room, the bureau, the firing range and the basement corridor. Every
spawn and loot point is checked against the built colliders before it is kept (`StationBuild`): one that would stand
inside a wall or a fitting is left out and reported.

## The open world
The scene is built around its own origin with everything under one root (`PoliceStation`), on a lot of about
116 x 90 m (x -30..86, z -30..60), ground at y 0. The world places it by moving or offsetting that; nothing in the
scene assumes a world position.

## Files
| File | What |
|---|---|
| `StationPlan`, `StationArt`, `StationFloors`, `StationWalls`, `StationFacade` | The station kit (shared with the fire station): grid plan, materials, slabs and flights, interior walls and railings from the grid, the brick shell with its windows and openings |
| `StationProps`, `StationFurniture`, `StationHiding`, `StationLighting`, `StationSite`, `StationSurfaces` | Fittings (kept off doorways, flights and openings, reported when left out), furniture models, hiding places, light and breakers, the lot and truck, world-scale UVs |
| `StationLootKit` | Loot as data: definitions, part-list models, prefabs, catalog, icons |
| `StationRecipe`, `StationContext`, `StationBuild` | The build pipeline, the same order as the factory and the island |
| `PoliceStationLayout` | The plan (rooms, doors, stairs, strength, haul route) |
| `PoliceStationGround`, `PoliceStationUpper`, `PoliceStationBasement` | Each storey's fittings, hiding places, loot and monster points |
| `PoliceStationLoot` | The 35 loot items (data) |
| `PoliceStationExterior` | The lot, cars, impound yard, entrance, roof plant, truck |
| `PoliceStationRecipe`, `PoliceStationBuilder` | Mood, light colour, spawns, sound; the menu items |
| `Scripts/Audio/PoliceStationSoundscape.cs` | Runtime: scanner static, distant door slams, drips (F1 shows the points) |

## What to check first
1. **Compile.** Written without compiling (the brief). Most likely trouble: a typo in a helper call or a name clash in
   the station kit; everything is plain Unity and existing project APIs.
2. **Run the rebuild** (Tools > Abandoned > Maps > Police Station > Rebuild Police Station) and read the Console:
   one `[PoliceStation] Built ...` line with every count, and a warning for each fitting, loot point or monster spawn
   that was left out (with why). A few left out is fine; many in one room means a coordinate in that room's code is off.
3. **Walk the carry route** with a Heavy item: evidence room (upstairs) -> hall -> main stair -> main hall (row 3) ->
   sally port -> truck; and basement armoury/property -> basement stair -> main hall. Doors on that route are 3.2 m.
4. **The weak floors:** the evidence room should hold the seized safe resting on it but go with a crew and a full
   haul; check it is not failing at the start on low-stability contracts (lever: `Strength` in the layout).
5. **Stairs:** walk the three flights both ways (the basement stair rises toward the street, the main and rear stairs
   toward the back); monsters use them through their NavMesh links. Nothing should catch your head on the way up.
6. **Light:** the precinct uses about 150 troffers on a checkerboard (fewer than one per cell). Check frame time and
   the power-off and blackout behaviour; dead/flicker ratios are in `StationLighting`.
7. **Hiding:** nine holding cells, eight lockers, two cabinets, two closets. The cell doors slide aside.
8. **Not done here (other owners):** the open world must place and stream the scene and list
   `Data/Loot/PoliceStation/Location_PoliceStation.asset` in the contract catalog before a contract can roll it (it is
   in `BuildScenes.All`, not `SalvageScenes`). `LocationPlumbing.ConfigureHorrorScenes` resets the HorrorConfig scene
   list to the four old maps every time another map is rebuilt; the station's build adds `PoliceStation` back, but a
   later rebuild of another map drops it until this one is rebuilt (worth making that method additive). The map-site
   pass (sky, trees, grass), the art remaster and the lighting rebuild have no profile for this scene yet.

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
