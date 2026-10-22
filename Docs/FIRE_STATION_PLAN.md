# Fire Station: Millbrook Fire Station No. 4 (2026-10-23)

A new salvage location for the open world (Docs/OPEN_WORLD_PLAN.md, "Upcoming salvage locations" 5): its own scene,
`Scenes/FireStation.unity`, streamed in by distance when the van comes near. Built by code with the station kit the
police station introduced (`Editor/PoliceStation/Station*.cs`, Docs/POLICE_STATION_PLAN.md "Files"): layout data,
geometry, the shared run plumbing, the NavMesh and the occlusion bake.

**Menu:** Tools > Abandoned > Maps > Fire Station > Rebuild Fire Station (and Add Fire Station Loot).
**Code:** `Editor/FireStation/`, runtime `Scripts/Audio/FireStationSoundscape.cs`. **Loot data:** `Data/Loot/FireStation/`.
**Art:** `Art/FireStation/`.

Written without compiling, running or testing (the brief: Claude CLI compiles and reviews). The scene does not exist
until the rebuild menu is run in Unity.

## The building
A 1930s brick firehouse, 48 x 36 m (12 x 9 cells of 4 m), two storeys over the quarters and a double-height apparatus
bay, with an 18 m hose tower on the north-west corner. South faces the street: three open bay doors (4.2 x 4.6 m);
the truck backs up to the middle one, which is the carry route. A personnel door to the watch office and a rear door
from the hall.

Ground floor (north up, x 0-11 left to right):
```
z8  T T W W W W . G G L L L      T hose tower base (its own structure)   W hose workshop (breakers)   G turnout gear room
z7  T T W W W W . G G L L L      L laundry and decontamination           . the hall (column 6)
z6  A A A A A u . K K K K K      A apparatus bay (double height)   u under the hose gallery   K kitchen and mess
z5  A A A A A u . K K K K K
z4  A A A A A s . D D D D D      s the bay stair, up to the hose gallery   D day room
z3  A A A A A s . D D D D D
z2  A A A A A A p D D D D D      p the fire pole lands here
z1  A A A A A A O O O C C C      O watch office (alarm board)   C station office
z0  A A A A A A O O O C C C
     ^ bay doors at x 4, 12 (the haul lane), 20
```
Upper floor:
```
z8  T T S S S S . F F F F F      S storage loft (WEAK, old timber over the workshop)   F fitness room (weak)
z7  T T S S S S . F F F F F
z6  - - - - - m . F F F F F      m hose gallery (mezzanine over the bay, railed)
z5  - - - - - m . Q Q R R R      Q captain's quarters   R bathroom
z4  - - - - - - . Q Q R R R
z3  - - - - - - . B B B B B      B bunk room (dormitory)
z2  - - - - - - P B B B B B      P the pole hole, in the upper hall
z0-1 - - - - - - . B B B B B     - open to the bay below
```
The hose tower: an 8 x 8 m brick shaft to 18 m with three steel platforms (5, 10 and 15 m), alternately on its north
and south halves, joined by fixed ladders (players only, hands empty). A 0.9 m slot along its east wall stays open from
top to bottom: the hoses hang in it, and it is the only way to get anything down from the platforms: drop it.

## Signature pieces
- **The fire pole**: a pierced slab in the upper hall with a brass pole down to the hall by the bay. It is a fixed
  ladder (`Core.Ladder`), so it is climbed both ways, hands empty, like the factory's ladders.
- **The bay**: Engine 4 parked in the west lane (solid), the middle lane clear for hauling, the antique hand pumper
  (the jackpot) on show in the east lane's back corner, turnout gear racks, hose racks.
- **The hose tower**: loot up top is small (it has to be dropped down the slot); the bronze station bell (a jackpot)
  stands at the tower's foot.

## Structure
- Every slab a 4 m section; the bay stair a Stair section; the tower platforms Walkway sections.
- Weak on purpose: **the storage loft** (health 0.6, capacity 0.5; old timber over the hose workshop), the **fitness
  room** (0.7 / 0.6, over the kitchen, turnout room and laundry), the bathroom (0.8 / 0.7). The bunk room and captain's
  quarters fall at full strength. The upper hall, the hose gallery and the pole slab never fall. Tower platforms: the
  top one is rusted (0.6 / 0.5), the middle one 0.8 / 0.7, the first never falls.
- Rubble never piles on the haul lane (the bay's middle lane), the hall, the stair or under the gallery.
- Stability 0.68, seed 4141.

## Run plumbing
- Crew spawns on the apron between the truck and the middle bay door; the truck faces the street.
- The NavMesh covers the building only (lot, truck, roof, tower platforms and ladders are left out): monsters do not
  climb the tower; it is a refuge with a price (you are stuck, the clock runs, you drop your loot to get it down).
- Power: six breakers in the hose workshop.
- Sound: `LevelAmbience` and `FireStationSoundscape` (the watch office's radio, hose couplings clinking against the
  tower walls, drips in the bay drains and the showers). Existing sounds only.

## Loot (Data/Loot/FireStation, the station's own tags)
| Tag | Where | Items |
|---|---|---|
| `fire_apparatus` | bay, hose workshop | brass nozzle, fire axe, Halligan bar, breathing-air cylinder, leather helmet, thermal camera, rescue spreader, engine siren, portable pump, foam drum (sloshes); jackpot: **antique hand pumper** |
| `fire_quarters` | day room, kitchen, bunk room, captain's quarters, fitness room | alarm clock (rings), chili pot (sloshes), dalmatian statue, poker chips (rattle), television, espresso machine, guitar, dumbbells; jackpot: **vintage jukebox** |
| `fire_tower` | tower base and platforms, turnout room | rolled hose, brass coupling, hydrant spanner, signal lantern, brass whistle; jackpot: **bronze station bell** |
| `fire_watch` | watch office, station office, storage loft | alarm tape register, cast-iron alarm box, muster trophy, logbook, radio set, speaking trumpet, chief's helmet, parade banner |

## Hiding places (HidingLocker)
Turnout lockers (`Locker`), the bunk room's lockers (`Locker`), the SCBA cabinets in the bay and the workshop
(`Cabinet`), the linen closet in the laundry and the captain's wardrobe (`Closet`).

## Monsters
Deep inside, at least 22 m from the middle bay door: the bay's back corner, the hose workshop, the kitchen, the day
room, the laundry, the bunk room, the fitness room and the loft. None on the tower.

## The open world
Built round its own origin, everything under one root (`FireStation`), on a lot of about 108 x 96 m (x -30..78,
z -40..56), ground at y 0. The world places it.

## Files
| File | What |
|---|---|
| `FireStationLayout` | The plan: rooms, doors, the gallery's rails, the bay stair, the pole's pierced slab, the tower's custom cells, strength, haul lane |
| `FireStationTower` | The hose tower (shell, platforms as Walkway sections, ladders, hoses, lamps, its loot) and the fire pole |
| `FireStationBay` | The apparatus bay: Engine 4, turnout and hose racks, SCBA cabinets, the pumper's corner, lane lines, the gallery |
| `FireStationGround`, `FireStationUpper` | The quarters' rooms, hiding places, loot and monster points |
| `FireStationLoot` | The 34 loot items (data) |
| `FireStationExterior` | The lot, yards, hydrants, bay-door drums, roof siren, truck |
| `FireStationRecipe`, `FireStationBuilder` | Mood, light colour, spawns, NavMesh exclusions, sound; the menu items |
| `Scripts/Audio/FireStationSoundscape.cs` | Runtime: radio static, couplings and chains in the tower, drips (F1 shows the points) |
The station kit itself lives in `Editor/PoliceStation/Station*.cs` (Docs/POLICE_STATION_PLAN.md "Files").

## What to check first
1. **Compile**, then **Rebuild Fire Station** and read the Console: one `[FireStation] Built ...` line with every
   count, and a warning for each fitting, loot point or monster spawn left out (with why).
2. **The pole.** Walk into the opening from the hall's south side upstairs and climb down; climb back up from the
   mat. It uses the factory's ladder rules (`Core.Ladder`, hands empty). A faster slide would need a change in
   `PlayerMotor.Ladders` (not this task's code). Check the guard rails stop anyone falling in from the other sides.
3. **The hose tower.** Climb all three ladders; step on and off each platform; drop a light item down the slot by the
   hoses and see it land at the foot (fragile things should break). Check the platforms are not on the NavMesh
   (monsters should not path up there) and that the top platform creaks with a crew on it.
4. **The bay stair and the gallery**: the stair's head has short guards each side; nobody should be able to step off
   the gallery's south edge beside the flight. Monsters reach upstairs only by this stair.
5. **The haul**: the jukebox (day room, wide west door, hall, wide bay door at row 2, middle lane, truck), the pumper
   (east lane corner straight out of the east or middle door), the bell (tower foot through the tower's 2.8 m door).
6. **Weak floors**: the storage loft over the workshop should go under a crew and a couple of items; the fitness room
   less easily. Neither should start failing on its own.
7. **Light**: high bays in the bay, troffers elsewhere, four bare lamps in the tower (on the power like the rest).
8. **Not done here (other owners)**: as for the police station: the open world must place and stream the scene and
   list `Data/Loot/FireStation/Location_FireStation.asset` before a contract can roll it; `ConfigureHorrorScenes`
   drops `FireStation` from the HorrorConfig list whenever another map is rebuilt; no map-site, remaster or lighting
   profile yet.

## Decisions for CLAUDE.md (to record; this doc does not edit it)
- 2026-10-23 (same brief as the police station): **Millbrook Fire Station No. 4** (`Scenes/FireStation.unity`,
  `Editor/FireStation/`), an open-world POI built with the station kit. The carry route is the middle bay door. Loot
  on its own tags (`fire_apparatus`, `fire_quarters`, `fire_tower`, `fire_watch`), ids prefixed `fs_`, data in
  Data/Loot/FireStation.
- **The fire pole is a ladder** (`Core.Ladder` in a pierced slab), climbable both ways, not a fast slide.
- **The hose tower is ladder-only** and off the NavMesh: a monster-free refuge where only light loot can be found and
  the only way down for it is to drop it.
