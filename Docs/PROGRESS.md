# ABANDONED — Build Progress

Living log for autonomous work. Read CLAUDE.md, then this file. Detailed per-task notes live in
`Docs/progress/M<N>.md` (read only when a task needs them); implementation-level decisions in
`Docs/progress/DECISIONS.md`; task prompts in Docs/PLAYBOOK.md.

## Current state (2026-10-06, autonomous build M3–M10 on branch `autobuild-2`)
- M0–M2 done and hand-tested. M3–M10 built autonomously on `autobuild-2`
  (worktree `~/Documents/Abandoned-autobuild2`). Don't wait for plan approval; record decisions,
  put human-only items under "Needs you". Push `autobuild-2` and tags freely (user OK'd).
- M3: 3.1–3.5 (+fixes) done: Facepunch fork + SteamBootstrap, NetworkBootstrap + networked player,
  builds + multi-process nettest, networked loot, shared carrying. M3.6 networked structure done (`nettest collapse`). M3.7 Steam lobby/invites done (fake-Steam tested; real Steam needs you). M3.8 robustness done (`nettest robust`).
- Company "Zeyad Games", bundle id `com.zeyadgames.abandoned` (user decision 2026-10-06).
- Last verified (after M3.8): compile clean; verify ALL PASS; EditMode 168/168; PlayMode 180/180;
  nettest basic/loot/sharedcarry/collapse/robust 4/4.
- Steam safety: Steam never initialises in batch mode or test runs unless Unity gets `-steam`. Never
  launch Steam from automation. `spike/facepunch-transport` is local only; never merge it.

- M4: 4.1 proximity voice done (`nettest voice`; see Docs/progress/M4.md).

## Next
Fix M3 review findings -> tag `milestone-3`, zipped builds -> M4.2 occlusion + radio -> M4.3 voice as noise.

## Needs you (details per item in Docs/progress/M3.md)
- [ ] Real Steam test (App ID 480, both machines, Steam running): F1 shows "Steam: on <name>". Esc ->
      Steam -> "Host (friends-only Steam lobby)" -> "Invite friends" opens the overlay; invite the Windows
      friend. They accept (game running: joins at once; game closed: Steam launches it with
      +connect_lobby and it joins). Both panels list both names; F1 shows the lobby id. Also try their
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
      they stood. Then the host presses Disconnect (Esc): every virtual player's scene reloads and the
      panel says "The host left the game." with an OK button, and nobody auto-hosts.
- [ ] Voice (M4.1) in MPPM or LAN (Direct IP uses the raw microphone): allow the mic when macOS asks.
      Hold V and talk: the other player hears you from your position, quieter with distance and silent
      past ~25 m; ((•)) shows over your head on their screen, "● TALKING" on yours. Esc panel: Open mic,
      volume slider, mute. Judge delay (~120 ms buffer) and quality (16 kHz mu-law). Over Steam (two
      machines) the same with Steam voice.
- [ ] LAN: host on one machine, join with its LAN IP:7777.

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
- M3.8: one `nettest loot --no-build` run (right after basic) failed on all 4 instances: no client ever
  connected within 40 s. The next 4 loot runs passed. If it recurs, check host.log for the listen port
  and whether a previous run's process was still alive.
- M3.5: one full PlayMode run failed `NetworkLootHitTests.HostThrownSafeKnocksDownTheClientPlayerOnTheClient`
  (client never ragdolled within 4 s); it passed alone 3x and in two further full runs. The safe now has
  SharedCarryable (idle when nobody holds it). If it recurs, look at that test's timing first.
- One `Tools/unity.sh all` run printed no PlayMode results line; rerun passed 103/103 and a second full
  `all` passed. Possible flake; if it recurs, check Game/Logs/batch/PlayMode.log for a crash.

## Unity-generated churn left uncommitted on purpose
DefaultVolumeProfile.asset, PC_RPAsset.asset, UniversalRenderPipelineGlobalSettings.asset (player builds
fill its runtime-settings list), probuilder Settings.json,
ProjectSettings/Packages/com.unity.multiplayer.tools/, ProjectSettings/SceneTemplateSettings.json.
