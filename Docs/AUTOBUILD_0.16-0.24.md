# Autonomous build, 0.16.0 to 0.24.6 (started 2026-10-10)

User brief (2026-10-10): build every roadmap version from 0.16.0 to 0.24.6, including the 0.22 UI revamp,
without stopping and without testing; make Mac + Windows builds at the end of each version (zipped into
`~/Documents/Abandoned-builds/dev/`). Blockers go to ntfy.sh (topic in the session log). Everything below
compiles and was authored in batch mode; **nothing was played**. Each version is one commit.

How a version is made: runtime code under Scripts/<Area>/, tunables as a ScriptableObject in a Resources
folder (code defaults until the asset exists), an editor step `Editor/Releases/Release<version>.cs` with a
menu item under Tools/Abandoned/... and a batch entry `AuthorAndExit`, run by
`Tools/unity.sh exec Abandoned.EditorTools.Release<version>.AuthorAndExit`. Steps add to the saved scenes
(they never regenerate the mall or HQ). Mall placements are found by `Editor/Mall/MallSites` (free walls and
floors, checked like the 0.16.0 lockers). Run-long networked state rides on the RunState prefab
(`NetworkContentBuilder.RunStateExtras`, append only). Sounds added from 0.16.1 are synthesised
(`Audio/Synth/ExtraPatches` dispatches them). One-time tips for new usables: `UI/Hints/IHintSource`.

## 0.16.1 breaker rooms
One breaker cabinet per horror power sector (6), on a bare wall inside the sector, back-of-house first.
E throws it (host-checked, `RunBreakers` on RunState): that sector's ceiling lights come back for 120 s
through danger blackouts and power-off contracts alike; the throw is heard 40 m away and the panel hums
(noise 0.4 every 6 s) while lit, then 45 s cool-down. Tunables: Data/Extraction/Resources/BreakerConfig.
Menu: Tools/Abandoned/Mall/Add Breaker Rooms (0.16.1).

## 0.16.2 seeded route changes
Locked stores (RunShutters) and doors shut at the start (RunDoors) already differed per job. New: `RunRoutes`
(RunState) sends 2–5 collapsible floor sections, at least 9 m apart, down 3 s into each job through the
normal structure path (synced, rubble mounds, NavMesh carving), and with 40 % chance one of the two
escalators (never both). Stairs, protected routes and the ground floor can't collapse, so stores and the
truck stay reachable. Tunables: Data/Extraction/Resources/RouteConfig.

## 0.16.3 freight lift
An open goods platform in the atrium's north-west void (tile 5,7, x 20–24 z 28–32), ground floor to the upper
gallery, entered from the west on both levels. Buttons: one at each landing (call) and one on the deck (send).
`RunLift` (RunState) holds the trip (from, to, start time, snap time); every machine places the platform from
it and the server clock, so it is in the same place everywhere. Players ride it through the new
`Core.MovingPlatform` (PlayerMotor adds the platform's frame move when grounded on it); loot rides the deck.
Needs power: the mains in its sector (1) or that sector's breaker. 14 s a trip, motor noise every 1.5 s.
Overloaded (gameplay weight on the deck above 450 kg when sent) it gives way halfway, free-falls to the
bottom, hurts riders 35 and stays broken for the job. Gates at both landings are solid (and carve the
NavMesh) unless the platform is parked there; a cage closes the ground level's other three sides.
Switched off (not deleted) to make room: the planter, bin and bench in that tile, one industrial dumpster
prop, and balustrade `Rail_1_4_7_(1, 0)`. Watch on the test: riding jitter for remote players (they follow
their owner's NetworkTransform), and whether the upper landing step (0.15 m gap) feels right.
Tunables: Data/Extraction/Resources/LiftConfig. Menu: Tools/Abandoned/Mall/Add Freight Lift (0.16.3).

## 0.16.4 vents
Six ducts, each a pair of floor-level grilles (found by MallSites): Basement↔Titan Tools, Cinema↔Apex
Circuit, Closed Offices↔rear promenade, Nordic Habitat↔Toy Archive, Titan Tools↔Toy Archive, Toy
Archive↔atrium. Crouch + E with nothing in your hands (pockets are fine) crawls through: the host writes
`NetworkPlayer.VentBase + duct` into the hiding value, so the player counts as hidden and **no** monster
follows them in (`Threat.HiddenFromMe`), whatever it saw. The duct is not modelled: the owner's player is
parked 60 m under the exit for the crawl (black screen with a countdown), then stands at the far grille.
Crawl time = grille distance / 1.4 m/s (at least 4 s). The duct rattles (noise 0.3 every 2 s at its middle).
Tunables: Data/Extraction/Resources/VentConfig. Menu: Tools/Abandoned/Mall/Add Vents (0.16.4).

## 0.16.5 store alarms
An alarm box on a wall inside each of the 8 stores, nearest its shopfront. `RunAlarms` (RunState) wires
each store with 35 % chance per job (by shutter index, seeded), arming 5 s into the job. The first living
player inside a wired store trips it once: an electric bell rings for 30 s (noise 1.0 every 2.5 s), the lamp
flashes. Bolt cutters in a hand slot + E at the box disarm it, before or while it rings. Armed boxes blink
red every 2 s; a disarmed or unwired box is dark. Tunables: Data/Extraction/Resources/AlarmConfig.
Menu: Tools/Abandoned/Mall/Add Store Alarms (0.16.5).

## 0.16.6 vault drill
The mall has no jewellery store (zone "Jewelry" is Nordic Habitat's furniture floor) and its ground-floor
stores had no bare wall big enough, so the safe stands in the **Cinema** (upper floor, west wall, x 0.15
z 14): a box-office safe. E sets a core drill on it: 90 s of drilling, noise 1.0 every 2 s, and every 12 s a
40 % chance it jams until someone presses E again (`RunVault` on RunState). Through, the door swings open on
4–6 of the most valuable one-hand items in the catalogue at 1.6× value, spawned by the host through the new
`NetworkLootSpawner.SpawnExtra` (cleared with the rest at the next re-roll). Tunables:
Data/Extraction/Resources/VaultConfig. Menu: Tools/Abandoned/Mall/Add Vault Safe (0.16.6).

## 0.16.7 security office
A monitor desk in the upper floor's closed offices and eight wall cameras (one per space where possible,
10 m apart). E at the desk: the local screen becomes one camera's feed in CRT green with its name and a
clock; A/D or click switches, E/jump/pause gets up. The viewer stands at the desk with controls off.
Only the watched camera renders, only on the watcher's machine (640×360, no shadows or post); nothing is
networked. Needs power in the desk's sector or its breaker. Labels are screen UI, not world text.
Menu: Tools/Abandoned/Mall/Add Security Office (0.16.7).

## 0.17.0 sledgehammer
New gear (catalog index after the battery; $3,000, company level 2; tool, never used up). Use swings at
what you aim at: 5,200 kg·m/s into a collapsible floor section (the crowbar is 1,500), 1.3 s apart, noise
0.9. A sound floor breaks in a few blows through its normal Cracking warning; protected routes and stairs
can't collapse and say so. Gear added from here on: `EquipmentKind` appends, `PlayerEquipment.Tools.cs`
(host uses), `ToolConfig` (Data/Equipment/Resources), `GearPrefabs` (prefabs gear spawns, registered with
NGO by `RegisterNetworkPrefabs`), icons drawn by UiGearIconBuilder. Menu: Tools/Abandoned/Content/Sledgehammer (0.17.0).

## 0.17.1 crash mat
New gear ($1,800, level 3, single use): use lays a 2.2 × 2.2 m pad flat on the floor 1.6 m ahead (a host-
spawned static network object that lasts the job). A soft zone 0.9 m deep above it (`Core.ImpactCushion`)
keeps 12 % of the speed of any loot impact in it, so dropped loot survives, and cuts a player's fall height
to 1.4 % (height goes with speed squared). Applied in `LootItem.ApplyImpact` and the host's landing damage.
Prefab: Prefabs/Equipment/CrashMat (GearPrefabs). Menu: Tools/Abandoned/Content/Crash Mat (0.17.1).

## 0.17.2 floor types
`StructuralSection.FloorKind` (Concrete, Glass, Timber, Tile) scales its capacity and health
(`FloorKindConfig`, Data/Structure/Resources: glass 0.45 / 0.5, timber 0.75 / 0.8, tile 0.9 / 1.0). Set on the
saved mall: the upper gallery's bridge is glass (drawn with the salvage glass material), the dining terrace
is tile, the cinema and toy archive upstairs are timber; the rest stays concrete. Non-concrete floors give
one warning of their own when they start Cracking (glass ticks and spiders, tiles pop, boards split) and
fail with their own sound in place of the concrete snap. All are Structure-bus floor sounds that walls never
hide; none is a creak. Menu: Tools/Abandoned/Mall/Set Floor Types (0.17.2).

## 0.17.3 noisy ground
24 patches (seeded, 6 m apart, on free floor found by MallSites): broken glass mostly in stores, puddles mostly
in the food court and basement, loose tiles anywhere. Every machine plays a crunch / clack / splash per 0.9 m
walked through a patch for any player (from synced positions); the host turns each step into monster noise
(glass 0.55, tiles 0.4, puddle 0.3; crouching 15 % of that). Art is flat and walk-through, merged per patch,
bound to its floor section. Tunables: Data/Extraction/Resources/GroundConfig. Menu: Tools/Abandoned/Mall/Add
Noisy Ground (0.17.3).

## 0.17.4 aftershocks
`RunAftershocks` (RunState): from 4 min into a job, every 4–7 min (seeded), an 8 s warning (a deep rumble
twice, light flicker, the radio line "TREMOR / brace yourselves"), then the shock: a heavy thud and big camera
shake for everyone, and 4–8 random collapsible sections lose 35 % health (never below 15 %) through
`StructureSimulation.AgeRandomSections`, so they crack and warn but only weight brings them down.
Tunables: Data/Extraction/Resources/AftershockConfig. Menu: Tools/Abandoned/Content/Aftershocks (0.17.4).

## 0.17.5 live water
Three 3.4 m flooded areas in the basement (the low ground). Every flood and every 0.17.3 puddle is water,
and water is live while its power sector has power (`Extraction.SectorPower.Live`: its breaker is thrown, or
the mains are on and the danger hasn't cut the sector; the lift and the security desk now use the same
check). Live water buzzes and spits (heard by everyone nearby); standing in it zaps each 0.5 s and the
host deals 22 HP/s ("Electrocuted in live water"). So a thrown breaker lights a sector and arms its water.
Tunables: GroundConfig. Menu: Tools/Abandoned/Mall/Add Flooded Areas (0.17.5).
