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
