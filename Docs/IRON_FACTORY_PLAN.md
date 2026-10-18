# Iron Factory — build plan (replaces the Factory location)

Status (2026-10-18): steps 1-7 written and compiled outside Unity (dotnet); the catwalk plan's own checks pass
(0 problems), every machine and hiding place clears the reserved space (48/48, 38/38, no overlaps), and every one
of the 178 loot points has at least one item that fits it. Every new English line has all nine translations
(checked by search; `check.py` needs Python, not on this PC). The 19 threat spawns pass their own checks. The
scene has not been rebuilt in Unity or played. Step 8 (the Unity compile and rebuild) waits for the user.
The old factory's dressing pass, `Editor/FactoryAtmosphereBuilder.cs`, is no longer called by anything; it is
kept until the user says it can go.
Rebuild: **Tools > Abandoned > Factory > Rebuild Iron Factory** (the Console prints each tier as a map).

## What is built (steps 1-2)

- `Editor/Factory/`: `IronFactoryLayout` (plan data), `IronFactoryCatwalkPlan` (catwalk runs, stairs, ladders as
  data, with checks), `IronFactoryFloor`, `IronFactoryShell`, `IronFactoryChannel`, `IronFactoryExterior`,
  `IronFactoryCatwalks`, `IronFactoryAccess`, `IronFactoryArt`, `IronFactoryBuilder`. Shared run plumbing for
  every location moved to `Editor/LocationPlumbing.cs`.
- Lava: `Structure/LavaVolume` (host: kills players, despawns loot), `LavaSurface` (look), `LavaConfig`
  (Data/Structure/Resources). No rubble mound ever lands in it. Death line "You fell into the molten iron."
- `Extraction/LevelBuildingBounds`: the factory's own "inside" box for the horror director and total collapse.
- Catwalks: one `Walkway` section per 4 m cell, 2 m deck (1.2 m narrow bridge), guards on closed sides.
  Tier capacity x0.8 / x0.65 / x0.5; weak (rusted) cells x0.55 capacity, 55 % health. Spines never fall and
  keep rubble off. Tier 1: 138 cells, tier 2: 67, tier 3: 30. Six stairs (NavMesh links), eight ladders.
- Ladders: `Core/Ladder` + `PlayerMotor.Ladders` (owner-side, not networked). Hands empty (pockets fine);
  facing the ladder, forward climbs, back descends, jump lets go. Climbing never counts as a fall.
  `PlayerMovementConfig.LadderClimbSpeed` (2.8 m/s). Monsters don't use ladders.
- Room roofs (5 m) are part of tier 1 and have no edge rails yet.

## What is built (step 3)

- Machinery floor (`IronFactoryMachinePlan` data, `IronFactoryMachines`): 48 dead machines, each one solid box
  (monsters path round it, sight stops at it). West bank: the rolling-mill line splits it north-south with
  two 4 m gaps; ore hoppers, presses, casting moulds, a boiler. East bank: two lathe aisles, mills, presses, a
  cross conveyor with one gap, hoppers, a boiler. Dispatch floor and north concourse: stacked ingot pallets.
  All under the 5 m catwalks (tallest 4.4 m).
- `IronFactoryClearance` keeps open: the stair flights, ladder feet, room doors, the dock lane, a 3 m lane
  along the lava, and every loot and threat point. Anything that would block one is left out with a warning.
- 38 hiding places (`IronFactoryHiding`), all `HidingLocker` with a new `HideSpotKind` for the prompt:
  22 worker lockers (locker room, by the dock), 6 maintenance bays under presses and a mill stand,
  4 cold furnaces, 6 electrical cabinets (breaker hall, east wall). New prompts: "Hide under the machine",
  "Hide in the cold furnace", "Hide in the cabinet", "Put that down to hide" (9 languages, job.txt).

## What is built (step 4)

- **The machines are dead until a breaker is thrown.** The Breaker Hall holds one `BreakerPanel` per power
  sector (6), flagged `startsMachines`. Throwing one uses the existing 0.16.1 rule (that sector's lights for
  `BreakerConfig.RestoreSeconds`, 120 s, humming loudly, then a cooldown) and also starts every machine in that
  sector (same sector sums as the ceiling lights). The contract's mains power doesn't start machines.
- `Extraction/Factory/MachineHazard` (scene object, not networked): reads `RunBreakers` and the shared server
  clock (`RunState.Now`), so every machine sees the same strike; only the host deals damage
  (`NetworkPlayer.DealDamage`) and knocks players back (`NetworkPlayer.ServerKnock`, the shove cap). A warning
  lamp flashes before each strike; press and steam zones are striped on the floor; strikes are loud to monsters.
  - Press: slams every 5 s, 45 damage in the striped zone behind it ("Crushed in a press.").
  - Mill stand: rolls bite every 1 s, 10 damage to anyone riding the roller tables into it ("Pulled into the rolls.").
  - Boiler: vents every 7 s, 20 damage in the ring around it ("Scalded by steam.").
  - Roller tables and conveyors: carry whoever stands on them (`MovingPlatform.Drift`, 1.3 m/s).
- Tunables: `MachineHazardConfig` (Data/Extraction/Resources). New synthesised sounds `MachineStart`,
  `PressSlam`, `SteamBlast` (SoundCatalog, FactoryPatches) with translated captions. Hiding spots are untouched
  by hazards. Lathes, mills, hoppers, moulds and pallets stay dead (cover only).

## What is built (step 5)

- 21 factory loot items (`IronFactoryLootItems`, plain `LootDefinition` assets, no code per item) on four new
  spawn tags, so no other location's loot changes:
  - **foundry** (iron and steel): Iron Ingot, Steel Billet, Cast Gear, Chain Coil, Anvil; jackpot Founder's Bronze Bell (700 kg).
  - **workshop** (tools and gauges): Brass Pressure Gauge, Foreman's Stopwatch, Micrometer Set, Torque Wrench,
    Welding Mask, Machinist's Vise, Wooden Casting Pattern.
  - **machinery** (huge machine parts): Steam Governor, Electric Motor, Flywheel (600 kg, Huge); jackpot Crucible (900 kg).
  - **records** (office): Production Ledgers, Typewriter, Company Seal Press, Foreman's Safe.
- Original low-poly models (`IronFactoryLootModels`, Art/Factory/Loot), built inside each item's Size box.
- `IronFactoryLoot.EnsureContent` (also **Tools > Abandoned > Factory > Add Iron Factory Loot**) makes the
  definitions, models, network loot prefabs, catalog, network prefab list and icons; the rebuild runs it first.
- 178 spawn points (`IronFactoryLootPlan`), replacing step 1's placeholders: beside each machine (what it makes),
  a 6 m grid in every room (records in the offices), one catwalk cell in four (light items only; tier 1 up to
  two-handed, higher tiers one-handed), and 3 jackpot sites. One is mid-bridge over the lava on the protected
  tier-1 bridge: the crucible crossing. Points are kept clear of machines, hiding places, stairs and the lava.
- Names in data_loot.txt, 9 languages ("Welding Mask" reuses the cosmetic's block).

## What is built (step 6)

- 19 threat spawns (`IronFactoryThreatPlan`, replacing step 1's placeholders): 11 on the ground (deep on both
  banks, the north concourse, the back rooms) and 8 on catwalk tiers 1-2, all at least 30 m from the dock.
  Tier 3 and the narrow bridge start monster-free. Ground points are checked clear of machines, hiding places,
  stairs and the lava; catwalk points must be real walkway cells.
- NavMesh: monsters reach the tiers by the six stairs (NavMesh links), never by ladders. The bake leaves out the
  exterior, truck, roof, rail dock, light rigs and ladders. Collapsible catwalks carve their cell when they fall.
- Occlusion: baked on the saved scene at the end of every rebuild (`OcclusionBake`, as the mall). Shell, rooms and
  machines occlude; floors and catwalks never do, so a collapse never hides what is seen through its hole.
- Rail dock (`IronFactoryDock`): a siding along the south wall that breaks around the truck's approach, two dead
  boxcars (solid cover), buffer stops, a steel canopy over the dock opening and a dock plate. The truck, ramp and
  cargo bay are the shared ones (step 1), backed up to the opening.

## What is built (step 7)

- Surroundings (`IronFactorySurroundings`): a chain-link perimeter fence (solid, keeps the crew on site), open
  ground out to the mall's mountain basin (re-centred on the plant), a four-track rail yard with dead boxcars and
  ore hoppers, three slag heaps, two cooling towers, three 72 m smokestacks with flue ducts, a water tower.
  Never any ground under the plant (it would roof the lava). All static and out of the NavMesh.
- Lighting (`IronFactoryLighting`): roof trusses, 20 high-bay `LightFixture`s under them that reach tiers 2-3
  (power, blackouts and breakers treat them like any ceiling light), and two sodium flood masts over the dock.
- Fog: warm smoky interior haze (`BuildingInteriorAtmosphere`), the night mood outside (`IronFactoryBuilder.Mood`).
- Sound: factory ambience (`AmbienceBuilder.Factory`: draughts, hum, iron settling often) and a new synthesised
  `LavaRumble` loop playing from each glow along the channel, so the roar follows you down its length.

## The user's answers

| Question | Answer |
|---|---|
| Size | ~3x the mall: **120 x 100 m** footprint (30 x 25 tiles of 4 m) |
| Lava | **Instant death, loot destroyed** |
| Catwalks | **Three tiers** above the machinery floor |
| Hiding | **Worker lockers, under machinery, cold furnaces / electrical cabinets** |
| Collapse | **Most catwalks can collapse; protected spines can't** (extraction never cut off) |
| Machinery | **Dead until power is restored**, then running and hazardous |
| Exit | **One rail loading dock** (like the mall's single entrance) |
| Loot | **Iron & steel goods, tools & gauges, huge machine parts, office & records** |
| Pit | **Molten channel ~20 x 60 m** splitting the plant in two; bridges are the only crossings |
| Monsters | **Existing roster** with factory spawns |
| Old scene | **Replaced**: Factory.unity becomes the Iron Factory |
| Outside | **Rail yard, slag heaps, smokestacks, cooling towers**, mountain horizon |

## Layout (plan view, north up)

```
 z=100 ┌──────────────────────────────────────────────────────────────┐
       │ LOCKER ROOMS / WASHROOM │  FOREMEN OFFICES + ADMIN (2 storeys) │
       ├───────────────┬─────────┴─────┬────────────┬─────────────────┤
       │ BLAST FURNACE │               │            │  MACHINE SHOP    │
       │ + ORE HOPPERS │   WEST BANK   │  MOLTEN    │  (lathes, mills) │
       │               │  casting floor│  CHANNEL   │  PATTERN SHOP    │
       │ BREAKER HALL  │  rolling mill │  20 x 60 m │  FOUNDRY STORES  │
       │ (power)       │               │ x 52..72   │                  │
       ├───────────────┴───────────────┴────────────┴─────────────────┤
       │      DISPATCH FLOOR (finished stock, crane hooks)            │
 z=0   └────────────────── RAIL LOADING DOCK (truck, only exit) ──────┘
```

- **Machinery floor (0 m):** dead presses, rolling-mill stands, conveyors, ore carts on rails and
  hoppers laid out as a maze of cover with sightline breaks. Both banks are reachable on the ground
  only around the channel's north and south ends (the channel runs z 20..80).
- **Catwalk tier 1 (5 m):** the main maze over the machines. Two **protected spines** (one per bank)
  run dock -> north rooms, plus **one protected bridge** over the channel. Everything else can fail.
- **Catwalk tier 2 (10 m):** crane rails, furnace-top platforms, control pulpits; two collapsible bridges.
- **Tier 3 (15 m):** roof-truss gantry and the overhead crane holding a crucible over the channel; one
  narrow bridge. A fall from here kills (fall damage is lethal at 12 m) — that's the gamble for the
  best loot up top.
- Tiers link by stairs (haul-friendly, on the spines) and ladders (fast, hands must be free).
- A catwalk that collapses over the channel falls into the lava: no rubble mound there, loot on it is lost.

## Systems

| Piece | Kind | Notes |
|---|---|---|
| `Structure/LavaVolume` | new runtime, host-only | Players inside die (death cause "lava", worded via `Loc`); loot is despawned and leaves the haul. Clients only see glow/sound. |
| Lava look | builder + material | Emissive scrolling material, a few non-shadow point lights (stays inside `ShadowDirector`'s budget), heat sound via `GameAudio`. |
| Machine hazards | new runtime + `MachineHazardConfig` (SO) | Off until the factory's power sector is on (`PowerController` / `SectorPower` / breaker hall). Then presses stamp, conveyors push, steam vents blast on host-timed cycles; damage host-side; loud, so they draw monsters. |
| Hiding | existing `HidingLocker` + `NetworkPlayer.HidingIn` | Lockers as-is; crawl spaces under machines and furnace mouths / cabinets reuse the same component with new models and enter poses. No new network state. |
| Catwalks | existing `StructuralSection` modules | Lighter capacity than mall slabs; spines flagged can't-collapse. Section count ~700 (≈15 KB first sync, fine). |
| Loot | new `LootDefinition` assets only | Ingots, cast gears, anvil, chain coil, wrenches, brass gauges, welding mask, motor, crucible, flywheel, ledgers, foreman's safe, typewriter. Original procedural models; no new code. |
| Threats | existing roster | Spawns on the floor and tiers 1–2; NavMesh links for stairs (monsters don't use ladders). |
| Surroundings | builder | Rail sidings, slag heaps, smokestacks, cooling towers; reuses the mall's mountain horizon. |

Code goes in `Editor/Factory/` (layout data, shell, floor, catwalks, channel, hiding, dressing,
surroundings, atmosphere), one class per file, under ~300 lines each, menu
**Tools > Abandoned > Factory > Rebuild Iron Factory**. All new text through `Loc` and `check.py`.
F1 debug: lava volume, hazard cycles, catwalk capacities, hiding spots.

## Build order (each step is a commit)

1. Layout data + shell + floor + molten channel + `LavaVolume`.
2. Three catwalk tiers, stairs, ladders, structural sections, protected spines.
3. Machinery-floor maze + all three hiding types.
4. Breaker hall, power-on machine hazards.
5. Factory loot definitions, models and spawn points.
6. Threat spawns, NavMesh, occlusion bake, truck at the rail dock.
7. Rail yard / smokestack surroundings, lighting, fog, sound.
8. Compile + rebuild (when the user asks), Docs/PROGRESS.md + CLAUDE.md milestone note.

## Look pass (plan, from the user's answers on 2026-10-18)

Goal: scary and beatable, not a blank box. The user's choices:

| Question | Answer |
|---|---|
| Look | **Grounded realistic, like the mall** (soot, rust, oil, scorched brick) |
| Darkness | **Mostly dark, pools of light** to move between (lava, working lamps) |
| Fairness helper | **Glowing hiding spots** (no red "safe route" lamps: the map stays honest) |
| Flicker | **Buzz-and-stutter pendants** and **swinging lamps** |
| Textures | **The mall's PBR kit + new code-made maps** (nothing bought or downloaded) |
| Set pieces | **Roof holes with moonlight**, **the crane and hanging crucible over the lava**, **a cold blast furnace** |
| Clutter | **Dense but readable** (routes stay clear) |
| Small scares | **Lamps swing when a monster passes overhead**, **chains rattling far away** |

Steps (each one a commit, each testable in a few minutes):

- **L1 Surfaces.** Reuse the mall kit (`MeridianArt.Material`: Concrete, Rust, BridgeMetal, Block, Crack, Scorch,
  Stain, Water). Generate the factory's own maps in code: soot-darkened brick (darker toward the roof), catwalk
  grating (cut-out, see-through), riveted steel plate for machines and tanks, hazard paint. Heat tells the story:
  scorch and cooled spatter near the lava, damp stains and moss in the cold corners. Oil and rust-drip decals under
  the catwalks.
- **L2 Darkness and flicker.** Fewer working pendants, more dead and faulty ones: pools of light, not a lit hall. A
  buzz-and-stutter flicker for faulty pendants (with the existing light buzz sound). Lamps hang from chains and
  swing (local, cosmetic): after a press slams or a section collapses nearby, and when a monster walks the
  catwalk above them, so a swinging lamp is a tell.
- **L3 Set pieces.** Roof holes with cold moonlight shafts and drifting dust (landmarks, light pools). The overhead
  crane on the tier-3 rails, its hook and a crucible hanging over the lava, swaying and creaking (scenery; the
  loot crucible stays separate). A cold blast furnace tower on the west bank, its open mouth a dark doorway.
- **L4 Clutter.** Scrap piles, cable runs sagging between columns, hanging chains, drums, tool carts, slag
  lumps, broken pallets: dense but outside every route the clearance check protects, batched per area.
- **L5 Glowing hiding spots.** A faint pilot light on each one, emissive only (no extra lights): a blue pilot flame
  in each cold furnace, an amber fuse lamp on each cabinet, a dim tag light on the lockers and bays.
- **L6 Distant chains.** A synthesised chain rattle, played now and then far from the crew in an empty part of the
  plant (local, cosmetic, never near a real threat, never pointing anywhere).

## Decisions taken without asking (reversible)

- Tier heights 5 / 10 / 15 m.
- The channel runs north-south through the centre, so the dock is reachable from both banks without crossing it.
- Under-machinery crawl spaces stay usable when the power is on; hazards don't reach inside them.
