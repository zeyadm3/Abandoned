# Island Prison: build plan (replaces the Prison location)

Status (2026-10-19): **all steps written** (the user: "build everything, don't test or verify"; and: "like the mall map
in terms of detail and level design"). Compiled outside Unity (dotnet, 0 errors), `check.py` 0 missing. The scene has
**not** been rebuilt in Unity, baked or played. Working name **Gallows Rock Penitentiary** (fictional, inspired by
Alcatraz; the user can rename it: `IslandPrisonLayout.DisplayName`, Location_Prison and the company.txt blocks).
Scene stays `Scenes/Prison.unity` and `Data/Contracts/Locations/Location_Prison.asset`, so contracts don't change.
Rebuild (once written): **Tools > Abandoned > Prison > Rebuild Island Prison**.

Built the same way as the Iron Factory (Docs/IRON_FACTORY_PLAN.md): layout as data in `Editor/Prison/`, one builder
menu makes the whole scene, shared run plumbing via `Editor/LocationPlumbing.cs`, clearance checks keep routes open,
one commit per step, then a look pass. The old Prison stops coming from `SalvageLocationBuilder` (the School keeps it).

## The user's answers

| Question | Answer |
|---|---|
| Old Prison | **Replaced**: Prison.unity becomes the island |
| Name | **Fictional**, Alcatraz-inspired |
| Size | **~3x the mall**, Iron Factory scale (island ~160 x 130 m, play area ~120 x 100 m) |
| Exit | **A boat replaces the truck**, moored at the island dock |
| Buildings | Cellhouse + **lighthouse and warden's house** + **dock barracks and guard tower** |
| Deadly hazard | **Cliffs and the sea**: fall in and you die, loot is lost |
| Signature collapse | **Three-tier cell galleries** |
| Hiding | **Open cells (sliding bars), laundry carts/bins, solitary (D-block) cells, guard lockers** |
| Loot | **Inmate contraband, guard/armoury gear, warden's valuables, lighthouse & maritime** |
| Mood | **Night fog + foghorn**, faint city lights across the bay, the lighthouse beam |
| Restore | **The lighthouse beam** |
| Monsters | **Existing roster** with island spawns |

## Layout (plan view, north up; the island climbs from the dock to the plateau)

```
                         ~~~~~~~~~~~~~~~ SEA ~~~~~~~~~~~~~~~
        ~~  ┌─────────────┐                                        ~~
        ~~  │ LIGHTHOUSE  │ WARDEN'S HOUSE (2 storeys, burned roof)  ~~
        ~~  └─────────────┘      │ garden path                       ~~
  ~~ cliff  ┌───────────────────────────────────────────────────┐  cliff ~~
  ~~        │ DINING HALL │ KITCHEN          (CELLHOUSE, plateau 12 m) │  ~~
  ~~ REC    ├─────────────┴───────────────────────────────────────┤  ~~
  ~~ YARD   │ D-BLOCK │  C-BLOCK (3 tiers) │ "BROADWAY" │ B-BLOCK (3 tiers) │
  ~~ (walled│ solitary│                    │  corridor  │                   │
  ~~  cliff │         │                    │            │                   │
  ~~  edge) ├─────────┴──── LIBRARY / CONTROL ROOM / ADMIN ─────────────────┤
  ~~        └───────────────────────────────┬───────────────────────────┘  ~~
  ~~             basement: LAUNDRY, SHOWERS, │ old tunnels ("the dungeon")    ~~
  ~~                                         │ switchback road (never falls)  ~~
  ~~      cliff stairs (shortcut, can fall) ─┤                                ~~
  ~~                                ┌────────┴────────┐  GUARD TOWER          ~~
  ~~                                │ DOCK BARRACKS    │  (ladder, loot up top)~~
  ~~                                │ armoury, lockers │                      ~~
  ~~  ══════ PIER ══════  BOAT (cargo hold, only exit)  └──────────────────┘   ~~
```

- **Dock (0 m):** crew spawns on the pier, the boat moored alongside with a gangway, the barracks (3 storeys
  against the cliff, armoury and guard lockers), the guard tower (ladder, a small lookout room with loot).
- **Switchback road (0 m to 12 m):** the long, protected haul route up to the plateau. It never collapses; broken
  railings in places, so a careless carry can go over the edge. **Cliff stairs** are the short, steep way: a few
  flights can collapse (then the road is the only way).
- **Cellhouse (plateau, 12 m):** B and C blocks, three tiers each (ground + two gallery tiers at 4 m spacing, as
  the runtime expects), "Broadway" down the middle, D-block solitary on the west, dining hall and kitchen at the
  north end, library/control room/admin at the south (entrance) end. The basement holds the laundry (carts and
  bins) and showers; the old tunnels below are the "dungeon".
- **Rec yard:** walled, open, on the west cliff edge; good sightlines, no cover.
- **North tip:** the burned-out warden's house (weak upper floor, valuables) and the lighthouse (spiral stair,
  the beam's lamp room at the top).

## Systems

| Piece | Kind | Notes |
|---|---|---|
| Sea and cliffs | runtime | Generalise `Structure/LavaVolume` into a kill volume with its own death line and config (lava keeps "You fell into the molten iron.", the sea gets "Lost to the sea."). Host-only, loot despawned and out of the haul. No rubble mound ever lands in the sea. |
| The boat | builder + existing truck components | A new original boat model carrying the same pieces as the truck: `TruckCargo` (the hold), `TruckIgnition` (the engine, "cast off" in the prompt), `TruckDisplay`, `TruckSanctuary`, `TruckFloodlights`. The ramp becomes a gangway. The deck stays level and still (no physics bobbing: loot would slide); the hull rocks visually only. Wording for prompts/toasts ("boat" instead of "truck") per location, in 9 languages. |
| Cell galleries | existing `StructuralSection` walkways | One section per 4 m cell, rusted ones weaker, as the factory catwalks. Each block keeps one protected end stair and a protected tier-1 spine so extraction is never cut off. |
| Cell doors | builder | Sliding barred doors, hiding uses the existing `HidingLocker` with new `HideSpotKind`s (cell, laundry cart, solitary). Barred cells: bars don't block sight in reality, so a hider is seen only if a monster watched them go in (the existing `FindsHider` rule) – same as lockers. |
| Lighthouse beam | new runtime + config (SO) | A switch in the lamp room (reuses `BreakerPanel` + `RunBreakers` with its own sector). While on (`RestoreSeconds`), a sweeping beam lights the outdoor routes. "Briefly blinds monsters it passes" needs a new small host-side stun hook on `Threat` (none exists today); I'll write it as its own step so it can be dropped. |
| Loot | new `LootDefinition` assets only | Four new spawn tags (contraband, armoury, warden, maritime) so no other map changes. ~20 items, original procedural models, e.g. soap pistol, dummy head, spoon digger, keyring, riot shield, gun-cabinet safe, grandfather clock, portrait, piano, ship's bell, diving helmet, brass lens, **Fresnel lens (Huge jackpot, in the lighthouse)**. |
| Threats | existing roster | Spawns in the cellhouse tiers, basement, tunnels, barracks and warden's house; none on the pier. Monsters use the road and stairs (NavMesh links), never ladders. |
| Surroundings | builder | The sea plane, cliffs and rocks, the bay, distant city lights and a bridge silhouette (fictional), fog. Replaces the mountain horizon for this map. |
| Sound | existing synth | A distant foghorn, surf on the rocks, wind; the lighthouse motor hum while lit. |

F1 debug: kill volumes, gallery capacities, hiding spots, beam state and sweep.

## Build order (each step is a commit)

1. Layout data (`IslandPrisonLayout`), terrain/cliffs, sea + generalised kill volume, cellhouse shell and floors.
2. Cell galleries (three tiers), stairs, ladders, protected spines, structural sections; basement and tunnels.
3. Dock, barracks, guard tower, switchback road and cliff stairs, warden's house, lighthouse.
4. The boat (model, hold, engine, gangway, per-location wording).
5. Cells, doors and all four hiding types; clearance checks.
6. Lighthouse beam (and, separately, the monster blind hook).
7. Island loot (definitions, models, prefabs, spawn points).
8. Threat spawns, NavMesh, occlusion bake.
9. Surroundings, lighting, fog, sound.
10. Look pass (surfaces, darkness and flicker, set pieces, clutter), as the factory's L1-L6.
11. Compile + rebuild + audits (when the user asks), Docs/PROGRESS.md + CLAUDE.md milestone note.

## What was built (files in Editor/Prison)

| File | What |
|---|---|
| `IslandPrisonLayout` | Every number: levels, quay, slip, road, cliff stair, cellhouse, stacks, rooms, outbuildings, bounds |
| `IslandPrisonBuilder`, `PrisonBuilder` | The rebuild (menu), sections, plumbing, NavMesh, atmosphere, occlusion |
| `IslandPrisonArt`, `IslandKit`, `IslandMeshKit`, `IslandCellKits` | Materials (Art/Prison), wall/railing/ramp helpers, merged mesh kits for cell fittings |
| `IslandTerrain` | Plateau, quay, piers, cliffs, boulders, sea, foam, sea kill volume, surf and foghorn |
| `IslandRoad`, `IslandFlights` | Switchback road, cliff stair, every flight and landing as sections |
| `IslandCellhouse` | Floor tiles, walls, barred windows, roof and lantern, trusses, inner walls, basement shell |
| `IslandCellStacks`, `IslandSideBlocks` | B/C stacks, galleries, stair towers, the wound; D-block solitary and A-block |
| `IslandInteriors`, `IslandBasement` | Dining hall, kitchen, library, admin, hall; laundry (carts), showers, boiler |
| `IslandBarracks`, `IslandWardenHouse`, `IslandLighthouse`, `IslandGrounds` | Quay buildings and tower, the villa, the lighthouse and lens store, yard, water tower, club, parapets |
| `IslandBoat` | The launch: truck components, gangway, moorings |
| `IslandLighting`, `IslandDecay`, `IslandThreatPlan`, `IslandHideSpots` | Lamps, cause-driven decay and the bay, 22 threat spawns, hiding-place wiring |
| `IslandLootItems`, `IslandLootModels`, `IslandLoot` | 24 items, their models, prefabs/catalog/icons |

The rebuild prints one `[IslandPrison] Built ...` line with the counts (sections, galleries, cells, hiding places,
loot points, threats, lamps).

## Outside pass (2026-10-19, after the user's first walk: "the outside needs a lot of work ... it feels like a dot in the ocean")

Written and compiled outside Unity (0 errors, 0 warnings); not rebuilt or seen.
- `IslandLandform`: the island is one faceted, walkable terrain mesh (2 m grid, own collider) from a height function:
  the plateau stays flat at 12 m wherever something stands (`FlatZones`), the open lawns get shallow hollows, the north
  corners round away, and beyond the brink the ground breaks in rocky steps to an irregular coast (sheer by the yard,
  a long slope and a point under the lighthouse, a steep goat path up from the quay's west end), carrying on under
  the water to the seabed. Grass / earth / rock / seabed by slope, foam where it meets the water. The old plateau boxes,
  strata and rectangle foam are gone; the sea now takes you just below the surface (you can stand at the water's edge).
- `IslandVegetation`: up to 170 trees (cypress, wind-bent pines; solid trunks), up to 900 shrubs and ice-plant mats
  (soft), up to 220 boulders on the slopes (solid), gull-white rocks near the water.
- `IslandExterior`: cellhouse pilasters, downpipes, roof rail, vents, tanks and flagpole, graffiti smears; the prison
  wall and guard post north of the yard; the gatehouse where the road meets the plateau; the ruined powerhouse with a
  30 m chimney; sheds; a rusted pickup; the old north jetty below the point (walk down the slope and out onto it); a
  power line on poles to every building with sagging (and some fallen) wires. New loot points in each.
- `IslandBackdrop`: the mainland across the bay (hills, mountains behind), a lit city, a 1.4 km suspension bridge,
  two smaller islands, an anchored freighter, the moon; on its own unlit haze shader (`Abandoned/IslandBackdrop`) so
  it reads through the fog, lamps glowing through the mist. Outdoor fog thinned to 0.011; the mall's overcast night sky.

## Noon (2026-10-19, user: "make it so the time is noon not night time")

The island's mood is a clear noon: high sun (68 degrees, intensity 1.5), pale sky ambient, light sea haze (fog 0.0045,
`IslandPrisonBuilder.NoonHaze`), Unity's built-in procedural sky (Art/Prison/Sky/IslandNoonSky.mat), bay water
grey-green blue, the far shore in daylight colours fading into the same haze (no moon, windows as dark glass). The
cellhouse keeps its dim interior haze. The job's Night modifier still darkens it as before.

## What to check first (in Unity)

1. Run the rebuild; read the Console for `[IslandPrison]` warnings and errors.
2. Walk off the boat, up the road and the cliff stair; into the cellhouse; up both stair towers to tier 2.
3. Hide in a barred cell, a laundry cart, a solitary cell, a guard locker.
4. Throw the lamp switch at the top of the lighthouse; watch the beam sweep and a monster flinch.
5. Carry the piano out of the warden's house and the lens from the lighthouse store to the boat; depart.
6. Collapse a gallery and a dining-hall tile; fall into the sea once.
7. `Tools > Abandoned > Mall > Audit All Levels` (includes the Prison).

## Decisions taken without asking (reversible)

- Gallery tiers 4 m apart (the runtime's storey height), not a real prison's ~3 m.
- The switchback road is the protected route; the cliff stairs are the collapsible shortcut.
- The boat's deck does not move physically (loot would slide off); only its hull rocks visually.
- Barracks loot spots stay close to the pier, so a crew always has something within reach of the boat.
- The quay is part of the NavMesh: the whole island is the level, so monsters can follow you down to the boat
  (the boat is safe only while departing, as the truck is).
- The industries building (not picked) is out; the laundry carts are in the cellhouse basement.
- No searchlight loot (the guard tower has a fixed searchlight fitting), so nothing scenic looks like loot.
- The mall's existing Grandfather Clock spawns in the warden's hall (its gallery tag) instead of a second clock item.
- The old greybox prison's plan stays in `SalvageLocationBuilder` (no menu reaches it) until you say it can go.
