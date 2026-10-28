# 0.25.0 whole-game audit (2026-10-28)

User: "finish all the remodeling and check everything that's new, all new features, and basically check the entire game
for bugs, dead code, bloated code, features that are not working as intended and fix them all; make the game Steam
ready (no Steam App ID yet)". Standing rule: never run the game (no Play mode, tests or nettests).

How: eight reviewers read every runtime script by area (Networking + Voice, Core + Onboarding + Contracts, Extraction,
Audio + Structure, Threats + Player + Interaction, Company + Equipment + Loot, UI + Tutorial + Skyscraper, Editor +
Tests) and fixed what they found. **Checked by compiling only**: `dotnet build` of all four projects (also a player-config
build without editor/dev defines), then Unity's own compile (with Netcode code generation): 0 errors. Then Rebuild
Entire Game was finished on all 11 maps. **Nothing was played or tested.**

## Fixed: bugs that would have shown in play

- **Motel hiding spots**: lockers and vents shared one hiding number; locker 100+ counted as a vent duct, so 21 of the
  Motel's 23 hiding spots would have dropped the player 60 m under the map. Hiding is now a `short`, vents start at 1000.
- **Engine upgrades**: the truck's countdown ignored them (always 6 s). Engine II (4 s) now works; Engine I was 7 s
  (longer than the countdown) and is now 5 s, text in all nine languages updated.
- **Shopping carts never appeared**: the mall rebuild dropped their bays. Rebuild All Maps now re-adds them after the
  mall (5 bays in the saved mall).
- **Police and Fire stations** were missing from the build's contract-destination list (`BuildScenes.SalvageScenes`),
  so the content validator and tests would fail on them.
- **Skyscraper lift**: after a power cut stalled the cab mid-shaft, calling it back to the floor it had left was refused
  ("already here"), stranding the crew; "Overloaded" used a hard-coded 450 kg.
- Store alarms with no shutter could read as wired (bit 31); the security desk could hand controls to a hidden player;
  difficulty could be changed mid-job; a hand-edited save with empty lists threw; truck horn didn't reset to stock;
  the voicemail screen vanished after a language change; HQ van could index past the board; a dart could be thrown back
  into physics while held; night vision toggled when clicking menus; third person leaked onto crewmates' copies.
- Crashes from empty object lists in the Hospital and Motel soundscapes; an Animator without a controller; the Blind One
  and Hunter leaving stale listeners and a 450 kg phantom floor load if destroyed without a despawn.
- **Settings**: a damaged key-binding save took all input down (now falls back to defaults); dragging brightness made a
  fullscreen window blink (resolution re-applied on every change); restore-defaults missed several Video/Graphics values;
  Voice-page timers kept running after Settings closed; the interview look ignored sensitivity and invert-Y.
- **Open logs** opened nothing on Mac (space in "Zeyad Games" in a hand-built file URL).
- **Daily job** id depended on the player's calendar (Thai/Hijri culture gave another day and leaderboard).
- **Hiring**: a damaged progress value ping-ponged between the HQ and the HR office; a build without the training house
  left a new hire stuck at the HQ with invites held back.
- Crew roster: Kick stayed on "Sure?"; crewmates' names never updated after the row was built.

## Fixed: security and Steam readiness

- Host-to-client RPCs any client could call (fake chat under another name, fake loot damage/shatter, sounds, toy scores,
  revive notices) are now server-only; client-to-host ones are owner-only. VentRpc validates its arguments.
- The network test harness (39 files) and every F1 debug overlay are compiled out of player builds.
- A Release/Demo build launched outside Steam now restarts through Steam (`RestartAppIfNecessary`, never with App ID 480
  or in shareable/dev builds), and hosts through Steam by default so "Invite friends" works without the player finding
  the transport toggle. Dev and shareable builds unchanged (Direct IP for LAN/MPPM).
- `EditorSetup` methods no longer compile into the player.
- Version 0.25.0 (`BuildScript.Version`).

## Removed: dead code

- Debug switch `ThreatDirector.ExploreWithoutMonsters`.
- The cut gear's behaviour (support jack, pulley, crash mat, flares, moving blanket, floodlight, spray chalk, backpack,
  Armored Bay): use branches, models, config fields, `ChalkDot`, `RunTerms.Armored`, and the release steps 0.17.1,
  0.17.6, 0.18.5, 0.20.0 (0.17.7 is glow sticks only). The components whose prefabs are still in the network prefab
  list (CrashMat, PlacedFloodlight, SupportJack, Pulley, RunChalk as an empty stub) stay until those prefabs are removed
  in Unity; `EquipmentKind`/`SoundId` values stay (append-only).
- About 60 unused members and tunables (27 in MenuEffectsConfig alone), and 25 unused editor files (the pre-Meridian
  mall builders, skyscraper skyline/street/rooftop details, old truck parts, probes).
- Per-frame allocations (LINQ, `LayerMask.GetMask`, lambdas, strings, HashSets) in noise, hearing, interaction, lava,
  shadows, sound-bank lookups, HUDs and gear screens.

## Also fixed while finishing the remodel

- Rebuild Entire Game hung on the Police Station in the coplanar fix (a lookup table wiped in full for every small
  plane); it now takes 10 minutes. `AvOverhaul.MapPasses(path)` resumes an interrupted chain map by map.

## Left for you (decisions or Unity work)

- **Bankruptcy** wipes truck cosmetics and upgrades with everything else (a comment says cosmetics should survive).
- **"FLOOR GIVING WAY" warning** (`UI/FloorWarningHud`) has been always hidden since commit 028d4c03a; restore it?
- **Kicked players** can rejoin at once through the friends-only lobby (no ban list).
- English text that is now slightly wrong but is a translation key: the total-collapse line says "the mall" on every
  map; two lines say crew rules / invites are "in the pause menu" (they're on the HQ crew board). Rewording needs the
  nine translations redone.
- The Skyscraper lift display is English only (5 lines to translate).
- Packages to drop before release (editor/AI tooling that can ship code or noise): `com.coplaydev.unity-mcp`,
  `com.unity.ai.assistant` (pre-release), `com.unity.ai.inference`, and `SENTIS_ANALYTICS_ENABLED` in the Standalone
  defines. Not touched: package changes need your OK.
- The Unity splash screen is on (`m_ShowUnitySplashScreen`); Unity 6 lets you turn it off.
- Small: Hospital/Motel emergency-light and network-content classes are duplicates (merge needs scene component swaps);
  `ControlsView` leaks an input asset per language change; F5 (third person) also works after rebinding.
- 207 lines in the text sheets are never asked for (old text); harmless.
- The editor-tools reviewer stopped at the usage limit before writing its report; its changes compile and the rebuild
  ran after them.

## What to check first when you play

1. Hide in a Motel locker and a vent; hide in lockers on the Mall.
2. Buy Engine I / II: the truck leaves 5 s / 4 s after the lever.
3. Mall: shopping carts at the five bays on the ground floor.
4. Skyscraper: cut the power with the lift between floors, then call it.
5. Settings: drag brightness in fullscreen; restore defaults on Video and Graphics; rebind a key and restart.
6. A release-flavour build (once the App ID exists) launched from Finder/Explorer restarts through Steam; hosting
   creates a Steam lobby and invites work.
7. The remodelled maps (Docs/remodelv2.md "What was built").
