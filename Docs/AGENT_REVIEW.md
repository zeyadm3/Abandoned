# Agent review (reviewer: Claude CLI)

What each map agent must fix, one section per agent, ready to paste. The reviewer compiles in the open editor,
reads the console, checks paths and CLAUDE.md rules, and runs a builder only once its map is finished.
Newest notes first inside each section. "Reviewer fixed" lines were done only because the project stopped compiling.

## ChatGPT: Hospital (Editor/Hospital, 1700) and Motel (Editor/Motel, 1750)

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

### 2026-10-23 01:00
- Nothing written yet.

## Antigravity: Skyscraper restaurant (Editor/Skyscraper*, 1950)

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
