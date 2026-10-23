# Agent review (reviewer: Claude CLI)

What each map agent must fix, one section per agent, ready to paste. The reviewer compiles in the open editor,
reads the console, checks paths and CLAUDE.md rules, and runs a builder only once its map is finished.
Newest notes first inside each section. "Reviewer fixed" lines were done only because the project stopped compiling.

## For every map agent (read first)

- **Open world fit (2026-10-23 13:00): all five new maps are in the world** and stream in as the van arrives
  (Docs/OPEN_WORLD_PLAN.md status 8, shots in Docs/renders/open-world). Where the van stops is read from an object named
  `VanParking` or `VanDock` in your scene (its position and facing): keep that name if you move it. Anything that is
  only backdrop (skyline blocks, horizon rings) and has no collider: put it under one group and tell the reviewer the
  path, so the world hides it.
- **Contract locations and loot catalog:** a map is only a job once `LocationCatalog` has a contract location for its
  scene and its loot is in the loot catalog (`Tools > Abandoned > Content > Loot > Generate Loot Prefabs`). Hospital,
  Motel and Skyscraper have neither yet (see your section); the Police and Fire stations have both.
- **How your map fits the open world** (Docs/OPEN_WORLD_PLAN.md "The moving origin"): your scene is **never moved**. It
  loads additively exactly where you built it (around 0,0,0) and the world's land moves under it, so keep authoring at
  the origin and keep absolute coordinates in your configs. Do **not** add code that moves your root.
- **No truck of your own.** In the open world the crew's van is the job's truck (`TruckCargo` on the van). If your map
  has a collection truck, the world hides it; tell the reviewer its path. Where the van should park: give one point
  in your plan (your scene's coordinates) and the side of your ground the road should reach.
- **Your ground.** The world cuts a hole in its terrain over a rectangle you name (your scene's coordinates) and meets
  your ground at its edge height. Put in your plan: the rectangle of your own ground, its height at the edge, and the
  paths of anything that only makes sense alone (sky domes, far horizon rings, distant hills) so the world hides them.
- **Build scenes before the scene exists:** each of you added your scene to `BuildScenes.All`. Until the scene file is
  saved, `BuildScript.Build` stops ("Scene ... doesn't exist"), so the normal Mac/Windows/dev builds are blocked for
  everyone. Fine once the reviewer has run your builder; just don't add a scene path for a map that isn't finished.
- **New player-facing text** must be in a sheet (Game/Assets/_Project/Data/Text/Resources/Text/*.txt) in all nine
  languages: `python3 Tools/Text/check.py` must report 0 missing (CLAUDE.md, 0.24.8).

## ChatGPT: Hospital (Editor/Hospital, 1700) and Motel (Editor/Motel, 1750)

### 2026-10-23 13:00 (reviewer ran both builders, the audits, the content check and the open world)
- **Not a job yet.** `ContentValidator`: "BuildScenes.SalvageScenes contains 'Hospital' / 'Motel' with no contract
  location" (Data/Contracts/Resources/LocationCatalog.asset), and all 37 Hospital and 38 Motel loot items are "not in
  the loot catalog". Add the contract locations and generate the loot prefabs so the items enter the catalog; then the
  107 "LootCatalog: an entry is missing its definition or prefab" errors (yours and the Skyscraper's) should go too.
- Audits (Audit All Levels): Hospital walk-through objects 0, invisible colliders 81, blocked stairs 0; Motel 0, 64, 0.
  Get the invisible colliders down (CLAUDE.md 0.12.2: keep at 0) or say which are meant (a kerb, a guard rail).
- Looks: the Motel reads well (shot: Game/Logs/renders/review). The Hospital's outside is a plain dark box with rows
  of small windows, and its corridors are sparse tiled runs; it needs the depth the Police Station has (entrance
  canopy, signage-free but readable wings, ward furniture, clutter).
- Open world: both fit (Hospital 80 x 88 m, Motel 120 x 68 m) and the van stops at your `VanDock`. Good.
- "needs a TruckCargo / TruckIgnition" for both scenes is the validator's old rule; in the open world the van is the
  truck, so this one is for the user to settle, not you.

### 2026-10-23 05:55 (commits ce310a66, f81367a5, 9e59241e)
- Text: hospital.txt and motel.txt added; nothing missing from either. Good. Both compile in the shared project.
- **Paths (user to decide):** new runtime code outside Editor/Hospital and Editor/Motel:
  `Scripts/Audio/HospitalSoundConfig.cs`, `HospitalSoundscape.cs`, `MotelSoundConfig.cs`, `MotelSoundscape.cs`,
  `Scripts/Structure/HospitalEmergencyLight.cs`, `MotelEmergencyLight.cs`, `Scripts/UI/MotelWorldSign.cs` (plus the
  earlier `Scripts/Networking/*NetworkContent.cs`). Four near-identical soundscape/emergency-light pairs: one shared
  component with per-map data (a ScriptableObject) would follow CLAUDE.md's data-driven rule.
- The reviewer's own run of both builders (05:37) finished in 3 s and 2 s; their scenes (saved 05:28) are what the
  reviewer will screenshot once the editor answers again.

### 2026-10-23 02:20 (commits e65d2522, c1432020)
- **Paths:** `Scripts/Networking/HospitalNetworkContent.cs` and `Scripts/Networking/MotelNetworkContent.cs` are outside
  Editor/Hospital and Editor/Motel. If they are needed at run time, ask the user to approve the location (or fold them
  into the existing network content code); otherwise move them under your Editor folder.
- `HospitalLootItems.cs` / `MotelLootItems.cs` declare a nested `Item` type: fine if it stays nested (one top-level
  class per file).
- Both builders are about 800 lines in total each: a two-storey hospital and a motel at that size are likely thin.
  The reviewer will run each builder and send screenshots before calling them finished.
- HOSPITAL_PLAN says "World code must move the full POI root before activation". It won't (see "How your map fits"):
  drop that line from the plan. Your "no second truck" choice is right.

### 2026-10-23 01:00
- **Reviewer fixed (blocked the compile):** `HospitalKit.cs:67` and `MotelKit.cs:67` called `FormattableString.Invariant`
  without `using System;`. Changed to `System.FormattableString.Invariant`. Keep it that way, or add `using System;`.
- **Reviewer fixed (blocked the compile):** `MotelBuilder.cs` used `ToolMenu.Motel` / `ToolMenu.MotelP`, which did not
  exist. The reviewer added `Motel = Maps + "Motel/"` and `MotelP = 1750` to `Editor/ToolMenu.cs`. **Do not add them
  again** (a second definition breaks the compile). Still to do on your side: the Motel line in `Docs/TOOL_MENU.md`
  and a line in `Editor/RebuildAllMaps.cs` (CLAUDE.md: a new map gets both).
- You may not compile, so before you hand a file over, check every type and member you use exists in the project
  (`grep` for its definition). Both breaks above were names that did not exist yet.

## Claude desktop: Police Station (Editor/PoliceStation, 1800) and Fire Station (Editor/FireStation, 1850)

### 2026-10-23 13:00 (reviewer ran both builders, the audits, the content check and the open world)
- Builders: Police 13 s, Fire 35 s. Contract locations and loot catalog are in order. Good.
- Audits: Police walk-through 0, invisible colliders 130, blocked stairs 0; Fire 4, 55, 0. Bring both counts down
  (CLAUDE.md: keep at 0), starting with the Fire Station's 4 walk-through objects.
- Loot weight rule (`ContentValidator`): `Loot_fs_rescue_spreader` and `Loot_fs_roof_ladder` are Heavy/Huge but under
  50 kg. Raise the weight or make them TwoHand.
- Looks: the Police Station is the best of the five. The Fire Station's apparatus bay is empty (the builder's log says
  its three engines were left out because they "would block a doorway"); an empty engine bay is the place's
  signature. Park them clear of the doors. Its sky is a flat grey.
- Open world: both fit (Police 116 x 90 m, Fire 124 x 100 m), the van stops at your `VanParking`. Good.

### 2026-10-23 05:55 (commits 86e2aaec, 56b78b7a)
- fire_station.txt and police_station.txt added; nothing missing. `StationFurniture` split (Domestic part): no file
  over 300 lines now. Both compile. Scenes saved 03:58; screenshots pending the editor.

### 2026-10-23 02:20 (commits 7be97196, 12d2b67e)
- Both compiled first time in the shared project. Good.
- **Paths:** `Scripts/Audio/PoliceStationSoundscape.cs` and `Scripts/Audio/FireStationSoundscape.cs` are outside
  Editor/PoliceStation and Editor/FireStation. Runtime code needs the user's OK for its location; the shared
  `Scripts/Audio/OutdoorSoundscape.cs` (MapSite) may already cover a map's outdoor bed.
- `Editor/PoliceStation/StationFurniture.cs` is 337 lines (CLAUDE.md: nothing over ~300 without a reason): split it.
- `StationPlan.cs` declares six nested types (`Finish`, `Room`, `Doorway`, `Flight`, `Side`, `Opening`) and
  `StationLootKit.cs` / `StationWalls.cs` three each: keep them nested, or one file each if they grow.
- Public fields in `StationLootKit.cs` / `StationWalls.cs` (CLAUDE.md: `[SerializeField] private`, not public fields):
  fine for plain data records inside an editor builder; not for components.

## Antigravity: Skyscraper restaurant (Editor/Skyscraper*, 1950)

### 2026-10-23 13:00 (reviewer ran the builder, the audits, the content check and the open world)
- **The builder took 14,802 s (4.1 h)** and froze the shared editor for everyone the whole time (most likely its
  occlusion bake). Cut the bake's cost (a coarser `smallestOccluder` / `smallestHole`, or bake only the tower's solid
  core) and check the builder runs in minutes.
- **Runtime error** in the world and alone: `UnassignedReferenceException: The variable visual of StructuralSection
  has not been assigned` from `Skyscraper/IntermediateFloors/RoofTerrace_50m/WindowCleaningGantry/SuspendedCradle`.
  Assign its `visual`.
- **Network:** `'ElevatorCab' is a scene NetworkObject without a source prefab; clients can't spawn it`. Make the cab a
  prefab (registered in the network prefab list) or keep it out of NGO; as it is, no client sees the lift.
- **Not a job yet:** no contract location in LocationCatalog for 'Skyscraper', and about 34 loot items are not in the
  loot catalog (generate the loot prefabs). The 34 loot names are still missing from the text sheets (see 05:55).
- Loot weight rule: `brass_meat_slicer` is Heavy/Huge under 50 kg; `caviar_tin_reserve`, `grand_cru_bordeaux`,
  `vintage_cognac_1945` and `vintage_single_malt_whisky` are Pocket items of 1 kg or more.
- Audits: walk-through objects 92, invisible colliders 20, blocked stairs 0. 92 walk-through objects is high.
- Looks: the tower reads as a translucent glass box (you see every floor through it); give the facade mullions, spandrel
  panels and some opaque cladding. Its own collection truck is still in the scene (hidden in the world).
- Open world: fits (48 x 57 m of street); the downtown `Exterior/Skyline` blocks are hidden in the world (they stood in
  the woods). Keep them under that path.

### 2026-10-23 05:55 (commits fda29be4, f653ccb6)
- **Missing text: 34 Skyscraper loot names** (Data/Loot/Skyscraper/*.asset DisplayName/Description: aged iberico
  ham, black truffle crate, ... `python3 Tools/Text/check.py --missing` lists all 34). Add them to skyscraper.txt in
  all nine languages. The elevator lines are now in the sheet. Good.
- Runtime code moved to a new area `Scripts/Skyscraper/` (`SkyscraperElevator`, `SkyscraperElevatorButton`,
  `SkyscraperSoundscape`): a new Scripts/<Area> folder means a new namespace area under CLAUDE.md; the user decides.
- Compiles. Scene saved 03:58; the reviewer's rebuild of it stalled when the editor stopped answering (see the
  plan's status); screenshots pending.

### 2026-10-23 02:20
- **Missing text:** five lines in `Scripts/Extraction/SkyscraperElevator*.cs` are not in any sheet ("fell with the
  elevator", "Lift (No Power)", "Lift (Out of Order)", "Lift (In Transit...)", "Lift (Already Here)"). Add them, in all
  nine languages, to a sheet under Data/Text/Resources/Text (`python3 Tools/Text/check.py --missing` prints them).
  Also `$"{Loc.T(label)}: {Loc.T(floorName)}"` builds the floor names at run time; the check cannot see them.
- `SkyscraperLootItems.cs` declares a nested `ItemSpec`: fine nested.

### 2026-10-23 01:15
The Skyscraper folder had 114 compile errors for over 10 minutes, which stopped every Play-mode test in the project.
**Reviewer fixed, smallest change that compiles; check each one is what you meant:**
- `SkyscraperStructure.cs:50`: `SectionType.Stairs` does not exist; the value is `SectionType.Stair`.
- `SkyscraperLootPlan.cs` (25 lines): `LootPointSpec` is `(string name, string tag, Vector3 at, CarryClass min,
  CarryClass max, bool isJackpot)` (`Editor/LootPointSpec.cs`). You passed `(name, at, tag, 1, 2, jackpot)`. Reordered
  to `(name, tag, at, (CarryClass)1, (CarryClass)2, jackpot)`, with your numbers kept. Check that 1/2 are the carry
  classes you want (`OneHand`, `TwoHand`, ... in `Scripts/Interaction/CarryClass.cs`) and write them by name.
- `SkyscraperArt` has no `Mahogany`. Thirteen uses in `SkyscraperHiding.cs`, `SkyscraperRestaurantFloor.cs` and
  `SkyscraperPrivateDining.cs` now use `Parquet`. If you want a real mahogany, add the field to `SkyscraperArt` (built
  like `Parquet`) and switch them back.

### 2026-10-23 01:00
- **Reviewer fixed (blocked the compile):** `Scripts/Extraction/SkyscraperElevatorButton.cs` implemented `IUsable` with
  methods that don't exist on it (`UsePrompt(NetworkPlayer)`, `OnUse(NetworkPlayer)`). The interface is
  `string UsePrompt(GameObject user)` and `void Use(GameObject user)` (`Scripts/Core/IUsable.cs`). Changed the two
  signatures; the bodies are yours as written. `CanUse` and `UseHoldDuration` are not part of any interface the game
  calls, so nothing calls them.
- **Paths:** runtime scripts landed in `Scripts/Extraction/` (`SkyscraperElevator.cs`, `SkyscraperElevatorButton.cs`),
  outside the expected Skyscraper paths. A new runtime area needs the user's OK; the existing lift is
  `Scripts/Extraction/Lift/` (`FreightLift`, `MovingPlatform` riders) and is the base the plan names. Ask the user
  whether to keep these files or build on `FreightLift`.
