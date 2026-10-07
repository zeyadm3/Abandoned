# ABANDONED — Build Progress

Living log for autonomous work. Read CLAUDE.md, then this file. Detailed per-task notes live in
`Docs/progress/M<N>.md` (read only when a task needs them); implementation-level decisions in
`Docs/progress/DECISIONS.md`; task prompts in Docs/PLAYBOOK.md.

## Current state (2026-10-06, autonomous build M3–M10 on branch `autobuild-2`)
- M0–M2 done and hand-tested. M3–M10 built autonomously on `autobuild-2`
  (worktree `~/Documents/Abandoned-autobuild2`). Don't wait for plan approval; record decisions,
  put human-only items under "Needs you". Push `autobuild-2` and tags freely (user OK'd).
- **M3 done** (tag `milestone-3`; review fixed, see Docs/progress/M3.md). 3.1–3.5 (+fixes) done: Facepunch fork + SteamBootstrap, NetworkBootstrap + networked player,
  builds + multi-process nettest, networked loot, shared carrying. M3.6 networked structure done (`nettest collapse`). M3.7 Steam lobby/invites done (fake-Steam tested; real Steam needs you). M3.8 robustness done (`nettest robust`).
- Company "Zeyad Games", bundle id `com.zeyadgames.abandoned` (user decision 2026-10-06).
- Last verified (M7.6): compile clean; verify ALL PASS; EditMode 214/214; PlayMode 236/236;
  nettest perf 4/4 (M7.5 basic/company; M7.4a robust; M7.3 loot/run; M6 review: all nine 4/4).
- Steam safety: Steam never initialises in batch mode or test runs unless Unity gets `-steam`. Never
  launch Steam from automation. `spike/facepunch-transport` is local only; never merge it.

- **M4 done** (tag `milestone-4`, review fixed): 4.1 proximity voice, 4.2 wall muffling + radio, 4.3 voice as monster noise (`nettest voice`; see Docs/progress/M4.md).
- **M5 done** (tag `milestone-5`, review fixed): greybox mall, seeded loot, runs + truck + appraisal, Blind One, danger (Docs/progress/M5.md).
- **M6 done** (tag `milestone-6`, review fixed): persistent session + travel, HQ, save, contracts, payday/debt/bankruptcy, shop + gear, Stalker + Collector, ghosts (Docs/progress/M6.md).

- **M7 in progress** (version 0.7.0; Docs/progress/M7.md): 7.1 art pass done (Kenney CC0 loot models,
  mall set dressing, vehicles; credits in Docs/ASSET_CREDITS.md). 7.2 lighting done (realtime fixtures that
  fall with their ceiling and go dark with the power, fog, post-processing, dust, skylight shaft,
  flashlight shadows). 7.3 audio done (Kenney CC0 sound bank, pooled playback, synthesised ambience). 7.4a menus done (UI Toolkit:
  main menu, pause menu with crew/invites, sound+voice settings, credits). 7.4b settings done (sensitivity,
  FOV, bob/shake, subtitles, colourblind scanner). 7.4c key rebinding done. 7.5 cosmetics done
  (coveralls + hats, HQ lockers, unlocked by playing). 7.6 performance done (F1 perf line, nettest perf,
  occlusion bake, shadow budget).

## Next
-> M7 review (read the whole M7 diff for bugs, fix), then tag milestone-7 and the Mac + Windows builds.
Then M8 demo.

## Needs you (details per item in Docs/progress/M3.md)
- [ ] Real Steam test (App ID 480, both machines, Steam running): F1 shows "Steam: on <name>". Main menu ->
      Steam -> "Host a Steam lobby"; in the HQ press Esc -> "Invite friends" opens the overlay; invite the
      Windows friend. They accept (game running: joins at once; game closed: Steam launches it with
      +connect_lobby and it joins). Both Esc menus list both names under CREW; F1 shows the lobby id. Also try their
      "Join game" from your Steam friends list, a different build (clear version message), and a 5th
      person (full). With Steam quit, the menu says "Steam isn't running".
- [ ] Multiplayer Play Mode (host + 1–3 virtual players, Join 127.0.0.1:7777): spawn points, smooth
      remote movement, footsteps, ragdolls, F1 client ids/RTT; 30 Hz tick feel.
- [ ] Builds: open the Mac zip from `~/Documents/Abandoned-builds/dev/` (right-click > Open first time);
      Windows friend runs the Windows zip (SmartScreen: More info > Run anyway).
- [ ] Loot in MPPM: pickup/drop/throw/pocket replicate; two grabbers -> one wins; vase shatters for all;
      thrown safe knocks a client down; carried laptop striking resting loot damages + pushes it.
- [ ] Shared carry in MPPM: server rack 1/2 drag -> 2/2 lift; tether, crouch slows crew, sway; piano/
      statue need 3 (2 only nudge ~0.75 m); turning corners swings the rack. Judge lift/spring/tether feel.
- [ ] Structure in MPPM (host + 1–2 virtual players): stand on the weak tile upstairs (Tile_U_3_3) with
      the server rack; every screen shows the same cracks/sag, then it collapses for everyone at once and
      the players on it fall; F1 shows the same stage/health on host and clients. Re-roll (F2) restores it everywhere.
- [ ] Leaving in MPPM: a virtual player picks up the laptop and closes/stops; the host sees it drop where
      they stood. Then the host picks Esc -> "End game for everyone": every virtual player lands on the
      main menu saying "The host left the game." with an OK button, and nobody auto-hosts.
- [ ] Voice (M4.1) in MPPM or LAN (Direct IP uses the raw microphone): allow the mic when macOS asks.
      Hold V and talk: the other player hears you from your position, quieter with distance and silent
      past ~25 m; ((•)) shows over your head on their screen, "● TALKING" on yours. Esc -> Settings: Open mic,
      voices volume, mute. Judge delay (~120 ms buffer) and quality (16 kHz mu-law). Over Steam (two
      machines) the same with Steam voice. M4.2: talk from the next room (muffled + quieter); hold R
      anywhere in the building: the other player hears you band-limited with static wherever they are.
- [ ] Mall run (M5.1-5.3): Mall scene (Build profile: open Scenes/Mall, Play). Walk the mall: entrance,
      both escalators, service stairs, the atrium bridge; loot everywhere (F1 shows seed/count/value). Carry
      loot to the truck at the loading bay (east), watch "Haul $X / Quota $40,000" rise, press E on the
      yellow lever in the bay: 10 s of honking, then it leaves and the appraisal shows (items, damage, who
      made it out, funny stats); host presses "Next run": everyone is back in the parking lot with new loot
      in a restored building. Judge distances and the 15 min window. M5.5: 45 s in, the Blind One
      appears (listen for clicking): sprint and drop things near it and it hunts you; touch = death
      ("YOU DIED", body stays down); crouch-walking past it should be possible. F1 shows what it heard.
- [ ] Company loop (M6): the build opens in the HQ. Read the contract board (E), host takes a job, press E
      on the van: everyone drives to the mall on that contract's terms (Power Off = dark). Extract, see the
      payday (debt if the quota is missed), "Back to HQ". Quit and restart: money/level are kept
      (~/Library/Application Support/Zeyad Games/Abandoned/company.json). A friend can only join at the HQ.
- [ ] Look and listen (M7.1-7.3): walk the mall with sound on. Loot has Kenney models; stores have
      furniture against the walls; ceiling panels glow, a few flicker, dead ones are dark; fog, dust in
      the light, a sun shaft under the skylight; flashlight (F) casts shadows. Judge brightness (too dark /
      too bright?), footsteps and impacts per material, creak/groan/snap/crash, the wind and light hum
      (gone in a Power Off contract), grab/pocket/flashlight/lever/coin sounds. Tell me what's off.
- [ ] Menus (M7.4): a build opens on the main menu (title over the HQ). Play; Esc opens the menu (crew,
      settings, leave). Settings: sensitivity, FOV, head bob, shake, subtitles, colourblind scanner, sound
      sliders, voice; Keys...: rebind Interact to G and check the "[G] Pick up" prompt. Judge readability
      of the all-caps Kenney font and the layout at your screen size.
- [ ] Cosmetics (M7.5): at the HQ press E on the blue lockers (garage, west wall): pick a coverall and hat;
      a friend sees them. Unlocks come from playing (a few runs unlock the cap/green; cone at 5 runs).
- [ ] Performance (M7.6): in a 4-player session (or MPPM) press F1: the bottom line shows fps, batches,
      awake bodies. Note fps in the busiest spot (concourse/atrium, flashlights on) on your Mac and on the
      Windows friend's PC, and during a big collapse. Anything under ~50 fps, tell me where.
- [ ] LAN: host on one machine, join with its LAN IP:7777.

## Committing
Stage `Docs`, `Tools`, `CLAUDE.md`, `Game/Assets/_Project` AND `Game/Assets/DefaultNetworkPrefabs.asset`
(NGO's prefab list lives outside _Project; M5.2-5.5 missed it) and any `Game/ProjectSettings/*.asset`
the rebuild changed on purpose (ProjectSettings, EditorBuildSettings). Leave the churn listed at the end.

## How to verify (batch mode, Unity must be closed)
```
Tools/unity.sh compile      # zero errors, no warnings from Assets/_Project
Tools/unity.sh rebuild      # regenerates generated content (Player prefab, TestBuilding, loot, network prefabs)
Tools/unity.sh verify       # Verify Project Setup + Content Validator
Tools/unity.sh editmode     # -> Game/TestResults/EditMode.xml
Tools/unity.sh playmode     # -> Game/TestResults/PlayMode.xml
Tools/unity.sh screenshots  # PNGs -> Game/Screenshots/ (gitignored); look at them
Tools/unity.sh all          # everything above in order
Tools/unity.sh build-dev    # dev Mac player -> Game/Builds/MacDev (used by nettest)
Tools/unity.sh build        # shareable Mac + Windows zips in ~/Documents/Abandoned-builds/dev/
Tools/unity.sh nettest [scenario] [--clients N] [--timeout S] [--rebuild|--no-build]
```
Zips: `Abandoned-<version>-<commit>-<Mac|Windows>.zip`; build them only after the task's commit.
Nettest results + logs: `Game/Logs/nettest/<host|clientN>.{json,log}`; batch logs `Game/Logs/batch/<step>.log`.
The script fails on compile errors, warnings in our code, or any exception in a log.

## Open problems
- `nettest loot` right after `basic` failed twice (M3.8, M6.0): sessions ended while waiting; never
  reproduced on demand. Recurred M7.2 (`company` right after `run`): one client "Failed to connect to
  server", no stale process left, passed on rerun. nettest.sh now keeps failed runs' logs (Logs/nettest/failed-*); look there next time. If it recurs, check host.log for the listen port
  and whether a previous run's process was still alive.
- M3.5: one full PlayMode run failed `NetworkLootHitTests.HostThrownSafeKnocksDownTheClientPlayerOnTheClient`
  (client never ragdolled within 4 s); it passed alone 3x and in two further full runs. The safe now has
  SharedCarryable (idle when nobody holds it). If it recurs, look at that test's timing first.
- One `Tools/unity.sh all` run printed no PlayMode results line; rerun passed 103/103 and a second full
  `all` passed. Possible flake; if it recurs, check Game/Logs/batch/PlayMode.log for a crash.

## Unity-generated churn left uncommitted on purpose
(PC_RPAsset.asset is committed since M7.6: the rebuild sets its shadow budget on purpose.)
DefaultVolumeProfile.asset, UniversalRenderPipelineGlobalSettings.asset (player builds
fill its runtime-settings list), probuilder Settings.json,
ProjectSettings/Packages/com.unity.multiplayer.tools/, ProjectSettings/SceneTemplateSettings.json.
