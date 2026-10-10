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
