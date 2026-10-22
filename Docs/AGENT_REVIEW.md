# Agent review (reviewer: Claude CLI)

What each map agent must fix, one section per agent, ready to paste. The reviewer compiles in the open editor,
reads the console, checks paths and CLAUDE.md rules, and runs a builder only once its map is finished.
Newest notes first inside each section. "Reviewer fixed" lines were done only because the project stopped compiling.

## For every map agent (read first)

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
