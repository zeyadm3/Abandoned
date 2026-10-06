# ABANDONED — Build Progress

Living log for autonomous work. A new session must be able to continue from this file alone.
Read CLAUDE.md first, then this file, then Docs/PLAYBOOK.md for the next task's prompt.

## Current state (2026-10-06, autonomous build M3–M10 on branch `autobuild-2`)
- **Milestones 0, 1 and 2 are done** and hand-tested by the user. M3–M10 are being built autonomously
  on branch `autobuild-2` (worktree `~/Documents/Abandoned-autobuild2`) per the user's brief in CLAUDE.md
  (decisions log, 2026-10-06 autonomous build). Don't wait for plan approval; record decisions here and
  put human-only items under "Needs you".
- **M3 progress:** M3.1 done (embedded Facepunch Transport fork + SteamBootstrap) + M3.1-fix (review
  fixes: Steam shuts down after NGO, test runs block Steam). M3.2 done (NetworkBootstrap + TransportMode,
  networked Player prefab, spawn slots, placeholder network panel, F1 net view; TestBuilding has no
  scene player any more - it auto-hosts in the editor) + M3.2-fix (remote ragdoll collider, spawn facing).
  M3.3 done (BuildScript + `Tools/unity.sh build-mac|build-win|build|build-dev`, VersionInfo/BuildInfo,
  multi-process localhost nettest `Tools/nettest.sh` with scenario registry; 'basic' passes 4/4)
  + M3.3-fix (builds stamp `<commit>-dirty` from uncommitted changes; dirty/unknown builds only match
  the same build run). M3.4 done (networked loot: host-validated pickup/drop/throw/pocket RPCs, carrier
  owns carried physics, replicated value/hold state, carrier-reported impacts, feedback on every
  machine, shatter despawns; 'loot' nettest passes 4/4; see "M3.4 notes")
  + M3.4-fix (follower copies of loot knock players down using their network motion; a client-carried
  item striking resting loot damages and pushes it on the host; see "M3.4-fix notes").
  M3.5 done (shared carrying: carry points on Heavy/Huge loot, host-simulated from the carriers'
  streamed hold targets, crew rule, slowest-carrier speed, tether, sway, load split, HUD + F1;
  'sharedcarry' nettest passes 4/4; see "M3.5 notes").
  **Next:** networked structure, Steam lobby/invite/relay, robustness (PLAYBOOK 3.4-3.6). Add a
  nettest scenario for each (see "M3.3 notes").
- **Verification state after M3.5:** compile clean; rebuild OK; verify ALL PASS; EditMode 131/131;
  PlayMode 159/159; screenshots OK; `Tools/unity.sh nettest sharedcarry` 4/4 PASS, `nettest loot` 4/4
  PASS and `nettest basic` 4/4 PASS (dev build e2d7d6b-dirty). Shareable zips: build them only AFTER
  the task's commit.
- **Steam safety:** Steam is never initialised in batch mode or any test run (including the editor's
  Test Runner window, via `SteamTestRunGuard` -> `SteamInitPolicy.TestRunActive`) unless Unity gets
  `-steam`. Bootstrap tests inject `FakeSteamClient`, which the test-run guard lets through.
  Never launch Steam from automation.
- `spike/facepunch-transport` (45fc975) is local only; never merge it (its package files were taken
  into the fork in M3.1, its Editor/Spike files were not).

## Needs you
- [ ] Real Steam connection test (M3.1+): Steam running on both machines, App ID 480. Start the game
      from the editor/Mac build, check the F1 overlay shows "Steam: on <your name>"; then host + join
      over the Facepunch transport with the Windows friend (lands with M3.2 NetworkBootstrap/lobby).
- [ ] Quit Steam and start the game: the menu/F1 overlay should say "Steam isn't running - start
      Steam and try again." and nothing should throw.
- [ ] M3.2 Multiplayer Play Mode: Window > Multiplayer > Multiplayer Play Mode, enable 1-3 virtual
      players, press Play. The main editor auto-hosts; in each virtual player press Esc if needed,
      leave "Direct IP", Join 127.0.0.1:7777. Check: each player stands on its own spawn point, you see
      the others move smoothly (interpolated), hear their footsteps, see them fall over when they
      ragdoll (K with F1 on), only your own camera moves with your mouse, F1 shows client ids + RTT.
- [ ] M3.2 feel: remote players' movement smoothness (NGO interpolation) and whether the 30 Hz tick
      feels OK (NetworkConfig.TickRate).
- [ ] M3.3 builds: unzip `~/Documents/Abandoned-builds/dev/Abandoned-<version>-<commit>-Mac.zip`, open the app
      (first time: right-click > Open, it's ad-hoc signed, not notarised); send the `-Windows.zip` to the
      Windows friend (SmartScreen: More info > Run anyway). Both should open TestBuilding with the Host/Join panel.
- [ ] M3.3 decide: company name and bundle identifier (Player Settings still say DefaultCompany /
      com.Unity-Technologies...; they set the save-data folder, so pick before M6 saves exist).
- [ ] M3.4 networked loot in Multiplayer Play Mode (host + 1-2 virtual players, Join 127.0.0.1:7777):
      a client picks up the laptop (E), carries it, drops it (right mouse) and throws it (hold and
      release left mouse); the host and other players see it carried in front of that player and land in
      the same place. Two players grabbing the same item: only one gets it, the other sees "Someone else
      has it". Pocket the gold watch (E): it vanishes for everyone and comes back out (hold Tab + right
      mouse) in front of the player. Drop the vase: everyone hears it, sees "$X -> $0" and shards, and
      it's gone everywhere. F1 shows owner / hold state / "sim here" over each item.
- [ ] M3.4-fix in Multiplayer Play Mode: the host throws the safe (or the server rack) at a client's
      player -> that client goes down (ragdoll) on its own screen and everyone sees it. A client carries
      the laptop and swings/walks it into a resting vase or TV -> the struck item makes its sound, shows
      "-$X" (or shatters) on every screen and gets pushed, instead of acting like a wall. Judge whether
      the push (one round trip late on the carrier's screen) feels acceptable.
- [ ] M3.4 feel: carried items seen on other screens follow the carrier smoothly (NetworkTransform
      interpolation at 30 Hz), and a thrown item doesn't visibly hitch when the host takes it back.
- [ ] M3.5 shared carrying in Multiplayer Play Mode (host + 2 virtual players, Join 127.0.0.1:7777),
      in TestBuilding: the server rack by the ground-floor front wall (another upstairs, the safe, vending
      machine, piano and statue are upstairs). One player aims at the rack and presses E: HUD says "Carrying 1/2 - needs
      1 more person (dragging)" and they can drag it slowly. A second player presses E on it: both HUDs say
      "Carrying 2/2 - lifted", it rises ~30 cm, and walking together carries it. Check: one player can't
      walk away from their handle (blocked, then pulled along); a crouching carrier slows the other; walking
      in different directions makes it rock; RMB (or K to ragdoll, F1 on) drops it for everyone. Try the
      piano/statue: two players can only nudge it, the third lifts it. F1 shows "Carrying n/m", crew speed
      cap and handle markers over the item, and point / share / tether in the CARRY box.
- [ ] M3.5 feel: lift height (SharedCarryConfig.LiftClearance 0.3 m), how springy/swaying the load is
      (Spring/Damping/WobbleGain), tether slack (0.9 m) and whether carrying at the slowest carrier's
      walk speed feels right. Over real latency the carriers lead the item by about RTT/2 + 100 ms; judge
      whether the tether makes that feel sticky.
- [ ] M3.2 LAN: two Macs (or Mac + Windows friend on the same network), host on one, join with the
      host's LAN IP:7777 (macOS firewall may ask to allow incoming connections).

## M3 plan: Facepunch fork (from the spike; pre-approved and DONE in M3.1)
1. Embed a copy of `com.community.netcode.transport.facepunch` 2.0.0 in `Game/Packages/` (not a git
   URL), replacing its bundled Facepunch.Steamworks with **2.5.2** (`Facepunch.Steamworks.Posix.dll`
   for Editor/macOS/Linux, `Win64.dll`, universal `libsteam_api.dylib`, `steam_api64.dll`), each with
   correct plugin platform settings (the spike branch has working .meta files to copy).
2. Patch the transport: don't call `SteamClient.Shutdown()` in `Shutdown()` (own Steam lifetime in a
   SteamBootstrap); return false from StartClient/StartServer when Steam isn't valid; implement RTT.
3. Test: Unity Transport host + clients in Multiplayer Play Mode first; Steam needs Steam running and
   the user's friend on Windows as the second account (App ID 480). Builds: Mac Mono, zipped folder.

## How to verify (batch mode, Unity must be closed)
```
Tools/unity.sh compile      # zero errors, no warnings from Assets/_Project
Tools/unity.sh rebuild      # regenerates Player prefab + TestBuilding scene (and later loot/structure content)
Tools/unity.sh verify       # Verify Project Setup + Content Validator (prefabs wired, no missing scripts, SO rules)
Tools/unity.sh editmode     # EditMode tests -> Game/TestResults/EditMode.xml
Tools/unity.sh playmode     # PlayMode tests -> Game/TestResults/PlayMode.xml
Tools/unity.sh screenshots  # PNGs -> Game/Screenshots/ (gitignored); look at them
Tools/unity.sh all          # everything above in order
Tools/unity.sh build-dev    # dev Mac player (arm64, Development) -> Game/Builds/MacDev (used by nettest)
Tools/unity.sh build-mac    # shareable Mac player (universal) -> Game/Builds/Mac + zip
Tools/unity.sh build-win    # shareable Windows Mono player -> Game/Builds/Windows + zip
Tools/unity.sh build        # both shareable builds, one zip each in ~/Documents/Abandoned-builds/dev/
Tools/unity.sh nettest [scenario] [--clients N] [--timeout S] [--rebuild|--no-build]
                            # = Tools/nettest.sh: 1 host + 3 headless clients of the dev build on 127.0.0.1
```
Zips are named `Abandoned-<version>-<commit>-<Mac|Windows>.zip` (`BUILD_ZIP_DIR` overrides the folder).
**Nettest:** rebuilds MacDev only when something in Assets/Packages/ProjectSettings is newer than
`Game/Builds/MacDev/BUILD.txt`, launches the host, waits for its `host.json.ready`, launches the clients
(`-batchmode -nographics`), kills stragglers after the timeout, then prints PASS/FAIL per instance, what
every machine saw, and exits non-zero on any failed/missing result or exception in a log.
Results + player logs: `Game/Logs/nettest/<host|clientN>.{json,log}`.
Logs: `Game/Logs/batch/<step>.log`. The script fails on compile errors, warnings in our code,
or any exception in the log even when tests pass.

## Milestone 1 — The Feel
| Task | Status | Verified |
|---|---|---|
| 1.1 Movement (controller, stamina, crouch, jump, look, prefab, spawn) | done | compile clean; verify ALL PASS; EditMode 22/22; PlayMode 26/26 (movement + TestBuilding doors/stairs/balcony/railing); screenshots checked. Feel (mouse look, snappiness) needs a human. |
| 1.2 Pickup / carry / throw / inventory | done | compile clean; verify ALL PASS; EditMode 23/23; PlayMode 39/39 (13 carry tests: reach, heavy/huge rejection, pockets 4 + full, pocket drop, hold settle, weight slowdown, drop, throw charge, heavy throw, host throw clamp, snag auto-drop, hands full). Throw/hold feel needs a human. |
| 1.3 Loot data + value damage + 10 items placed in TestBuilding | done | verify ALL PASS (+layers, loot prefab per definition); EditMode 38/38; PlayMode 48/48 (vase shatters from hand height with sound/text/shards, laptop drop partial loss, gentle place no loss, cash immune, clients don't apply damage, cooldown, shatter while held, pocketed value, 10 placed items settle undamaged, 3 upstairs). Screenshots checked. |
| 1.4 Ragdoll (capsule placeholder) | done | EditMode 39/39; PlayMode 58/58 (enter/recover, camera follows head, auto get-up, fall >4 m ragdolls, short fall/jump don't, heavy hit ragdolls, light hit doesn't, held item dropped). Screenshot checked. |
| 1.5 Feel pass (head bob, landing dip, footsteps, shake) | done | EditMode 39/39; PlayMode 70/70 (bob only when moving, dip on hard landing + recovers, footstep cadence, crouch quieter than sprint, no steps airborne, shake near/heavy only + decays, every toggle off works, TestBuilding surfaces). Actual feel needs a human. |
| Spike: Facepunch Transport + NGO 2.13 (throwaway branch, not merged) | done | See "Facepunch spike result" below. Branch `spike/facepunch-transport` (local, not merged). |

## Milestone 2 — The Weight
| Task | Status | Verified |
|---|---|---|
| 2.1 StructuralSection + logical load model | done | PlayMode: weight on tile, resting loot uses gameplay weight (not clamped mass), falling loot doesn't load, piano across two tiles splits, player = body + carried. |
| 2.2 Five stress stages (visual + audio placeholders) | done | Stages in order Stressed→Cracking→Failing→Collapsed, 2 s failing window, tint/cracks/dust/sag/sounds per stage tested; screenshots. |
| 2.3 Pre-fractured collapse + fracture generator | done | Colliders off, things on top fall, players ragdoll, cosmetic Debris-layer chunks fall and despawn, same seed = same break, cascades by host rules, can-collapse flag. |
| 2.4 Stability % + seeded pre-damage | done | Capacity/decay scaling, seeded pre-damage (same seed same result, ineligible never damaged), re-roll restores collapsed sections, determinism trace test. Keys: - / = / F2. |
| 2.5 NoiseEvent system | done | Footsteps (gameplay noise values), loot impacts, drag scraping, creaks, cracks, collapses; F1 noise view. |
| Heavy items on weak upper tiles/balconies in TestBuilding | done | Automated exit test: dragging the rack onto Tile_U_3_3 collapses it and the rack falls through; lowering stability drops the statue into the atrium. |

## Milestone 3 — Together
| Task | Status | Verified |
|---|---|---|
| 3.1 Embedded Facepunch Transport fork + SteamBootstrap | done | compile clean; verify ALL PASS (new: NetworkConfig, steam_appid.txt in sync, Steam plugin platform settings + no stray Steam binaries); EditMode 63/63 (+16: transport StartClient/StartServer false without Steam with clear log, Initialize doesn't start Steam, no Init/Shutdown/RunCallbacks in transport code, RTT from ping, fork version; init policy, player-readable errors for not running/missing library/update/missing config, fake-client init+shutdown once, plugin settings); PlayMode 116/116 (+4: batch Start leaves Steam off, RunCallbacks every frame + Shutdown on destroy, duplicate discarded, Steam dying mid-game stops pumping without throwing). Real Steam untested (Needs you). |
| 3.2 NetworkBootstrap, TransportMode, networked player | done | compile clean; rebuild OK; verify ALL PASS (+ Player prefab has NetworkObject/owner-auth NetworkTransform/NetworkPlayer and is in DefaultNetworkPrefabs; session scene has one bootstrap, a root NetworkManager with both transports + Player as player prefab, no scene-placed player, >= MaxPlayers spawn points with distinct indices); EditMode 79/79 (+15: launch args, auto-host policy incl. MPPM virtual players and -client/-connect, spawn slots, PlayerNetState); PlayMode 130/130 (+10 in-process NGO host+1-3 clients over loopback UTP: distinct spawn points, only owner camera/input/look/motor/interactor/HUD/hit trigger, owner movement replicates to host and other client, remote footsteps + floor load on host, remote ragdoll shown lying + getting up, client landings reach host as impacts, GameAuthority offline/host/client, solo hosting spawns local player, transport selection + Steam unavailable error, 5th player refused + leaver frees spawn). TestBuilding tests now use the auto-hosted NGO player. |
| 3.3 Build script + multi-process localhost nettest | done | compile clean; rebuild OK; verify ALL PASS (+ Build Settings scenes = BuildScenes.All, BuildInfo unstamped, Mono + Run In Background); EditMode 94/94 (+15: nettest args/registry, BasicNetTestCheck catches missing player/stale remote copy/disagreeing still player/short or missing moves/missing views, VersionInfo compatibility rules, build folders/targets/options/steam_appid policy/player settings/scene list); PlayMode 135/135 (+3 in-process: channel both ways with sender ids, 'basic' scenario passes host+3 clients and host sees each mover where it stopped, timeout aborts with a recorded error); `Tools/unity.sh build` Mac universal 120 MB + Windows 103 MB, signature verified, zips 47 MB/38 MB; `Tools/unity.sh nettest` 4/4 PASS (each client walked 2.15 m, all 4 machines agree), repeat run reuses the build, unknown scenario fails 0/4 with a clear message. |
| 3.3-fix Review fix: dirty builds stamped as HEAD | done | compile clean; rebuild OK; verify ALL PASS; screenshots OK; EditMode 97/97 (+3: GitInfo.Label dirty/clean/unknown, real repo gives a hex label, dirty/unknown keys only match the same build time incl. Mac+Windows pair, clean vs dirty refused); PlayMode 135/135; build-dev stamps `a7e1f70-dirty` in BUILD.txt and the player, BuildInfo.asset reset after; nettest 4/4 PASS. |
| 3.4 Networked loot | done | compile clean; rebuild OK; verify ALL PASS (+ loot prefabs have NetworkObject/owner-auth NetworkTransform/NetworkLoot, no NetworkRigidbody, DontDestroyWithOwner, registered; scene NetworkObjects have unique ids, a registered source prefab and are SAVED as in-scene placed); EditMode 111/111 (+14: impact report filter owner/bystander/rate/clamp/NaN/far point, just-released carrier until the host simulates a hit itself, request guard, hold state, config + value state, 'loot' check catches value/position/missing/extra/held/ownership/no-move/no-damage/missing views, scenario registered); PlayMode 147/147 (+12 in-process NGO host+2 clients: client pickup host-validated + physics handed over + carried weight on every machine + copies follow the carrier, out-of-reach refused by the host's view, simultaneous grabs -> exactly one holder, drop returns physics to the host and all agree where it lands, throw clamped (100 m/s -> cap) and NaN throw -> drop, carrier leaving frees the item, rolled value + damage + one -$X popup per machine, client can't damage, shatter while carried despawns everywhere with feedback on every machine, pocket hidden/no collisions/in inventory on all machines and back out, reported impact applied once with one host noise, solo TestBuilding loot is host-owned in-scene NetworkObjects; 'loot' scenario host+3 clients in-process); `Tools/unity.sh nettest loot` 4/4 PASS (each client picked up and threw its laptop, all 4 machines agree on 16 items incl. the 13 scene-placed, values incl. host damage, positions within 0.25 m); `nettest basic` 4/4 PASS. Screenshot M3_4_client_carrying_seen_from_host checked. |
| 3.4-fix Review fixes (follower loot hits players; carried strikes on resting loot) | done | compile clean; rebuild OK; verify ALL PASS; screenshots OK (no new visible content); EditMode 115/115 (+4 TrackedVelocity: steady motion converges, first sample/reset report nothing, teleport is not a hit, zero step/NaN ignored); PlayMode 151/151 (+4 in-process NGO: host-thrown safe ragdolls client 1 on its own machine and the host sees it; a resting follower copy knocks nobody down; client-carried laptop swung into a resting TV -> host applies the strike at the carrier's speed, value drops on every machine, TV pushed and the carrier sees where it went; strike reports far from the struck item or naming an item someone else carries are ignored). Both new behaviour tests fail with the fix reverted. `nettest loot` 4/4 PASS and `nettest basic` 4/4 PASS (dev build a62ec5c-dirty). |
| 3.5 Shared carrying | done | compile clean; rebuild OK; verify ALL PASS (+ Heavy/Huge loot prefabs have SharedCarryable with a config + NetworkSharedCarry, lighter loot has neither); EditMode 131/131 (+16: generated handles (rack ends of long axis, piano ends + long-side middles, max 4), crew sizes Heavy 2 / Huge 3 / override / generator 4, tether (free inside slack, outward blocked, sideways + inward free, pull beyond), HUD wording, SharedCarryState slots/grips, only Heavy/Huge prefabs share, definition validation, 'sharedcarry' verdict catches not-lifted / not-moved / no targets / stuck carrier / disagreeing or missing copy / client-owned, scenario registered); PlayMode 159/159 (+8 in-process NGO host + 2 clients on a 5 m platform: one client can't lift the rack, two do (host keeps the physics, 150 kg each on every machine, item loads nothing, targets streamed), carried past the edge every machine agrees, one lets go -> it falls 5 m, host applies damage, every machine sees it, the other carrier isn't dragged off and loses their grip; piano: 2 can't lift, the host as third does, 166.7 kg each everywhere; crouching client 1 caps client 2 and the rack at crouch speed x load (1.21 vs 2.48 m/s walk); each carrier's share loads the tile under their own feet (body + 150 kg each); one client still drags the rack (rests its 300 kg, stays upright, copies agree); a ragdolled carrier lets go and it drops; pulling apart rocks it >3 deg while the tether keeps them by their handles; 'sharedcarry' scenario host + 3 clients in-process); `Tools/unity.sh nettest sharedcarry` 4/4 PASS (rack lifted 0.39 m, 82 hold targets, carried 1.0 m, all machines within 0.25 m); `nettest loot` + `nettest basic` 4/4 PASS. Screenshot M3_5_two_clients_lift_rack checked (rack held off the floor between two carriers); M2_rack_cracking_floor re-checked (solo drag through the new path keeps the rack upright). |
| 3.1-fix Review fixes (Steam/NGO shutdown order, test-run Steam guard) | done | compile clean; rebuild OK; verify ALL PASS; screenshots OK; EditMode 64/64 (+1: real client blocked outside batch during a test run, guard armed; policy test covers test-run flag); PlayMode 120/120 (+4: ShutdownSteam while hosting shuts NGO first and Steam only after it stops listening; both OnApplicationQuit orders keep Steam alive until NGO stopped; guard armed in PlayMode). |

### M3.1 notes
- Fork: `Game/Packages/com.community.netcode.transport.facepunch` 2.0.0-abandoned.1 (CHANGELOG/README list
  the patches). Facepunch.Steamworks 2.5.2; removed linux32, 32-bit steam_api.dll and .lib import libs;
  WindowsStandalone32 dropped from the transport asmdef. Patches: no `SteamClient.Shutdown()` in
  `Shutdown()`, no `SteamClient.Init()`/`RunCallbacks()` in the transport at all (SteamBootstrap owns
  them), StartClient/StartServer return false + `FacepunchTransport.SteamNotRunningMessage` when Steam
  isn't valid, `GetCurrentRtt` = `Connection.QuickStatus().Ping`, null-safe LogLevel.
- Plugin settings are applied by `SteamPluginSettings` (Editor; Tools/Abandoned/Fix/Apply Steam Plugin
  Settings, also in `RebuildContent`) and checked by the verifier: Posix dll = mac editor + macOS +
  Linux64 players; Win64 dll = Windows editor + Win64; universal dylib = mac editor + macOS;
  steam_api64.dll = Windows editor + Win64; linux64 .so = Linux editor + Linux64.
- Runtime (Abandoned.Networking): `NetworkConfig` (Data/Networking, SteamAppId 480, InitSteamOnStart),
  `SteamBootstrap` (singleton, DontDestroyOnLoad, `Create(config)`, `TryInitialize()` (retry-safe),
  `IsAvailable`, `LastError`, `LocalSteamId`, `AvailabilityChanged`, F1 line at the bottom of the
  screen), `ISteamClient` + `FacepunchSteamClient` (seam for tests), `SteamInitPolicy` (-steam /
  -nosteam, batch off), `SteamErrorMessages`. Abandoned.Runtime references the transport assembly.
  Nothing creates SteamBootstrap in scenes yet; M3.2's NetworkBootstrap will (Facepunch mode).
- `Game/steam_appid.txt` (480) is for the editor; dev builds need a copy beside the executable (build
  script, later task). Release builds with the real App ID must not ship it.
- `Tools/unity.sh rebuild` regenerates Player.prefab/TestBuilding.unity with new fileIDs (same content);
  revert them with `git checkout` when nothing in their builders changed.

### M3.2 notes
- `NetworkBootstrap` (Abandoned.Networking, scene object "Network" next to a root "NetworkManager" with
  UnityTransport + FacepunchTransport, built by `NetworkSceneBuilder` from `TestBuildingPopulator`):
  `SelectTransport(TransportMode)`, `StartHost()`, `StartClient("ip[:port]" | SteamID64)`, `Disconnect()`,
  `Status`, `LastError`, `Slots`. Connection approval assigns the lowest free `SpawnSlots` slot (host = 0)
  and the pose from `PlayerSpawnPoint.PoseFor(slot)`; full lobby is refused with a reason. Steam mode
  calls `SteamBootstrap.Create(config)` + `TryInitialize()` first and sets `FacepunchTransport.targetSteamId`.
  `GameAuthority.SetHostCheck(IsHostOrOffline)`: true offline or on the server, false on a client.
  The NetworkManager is destroyed with the scene's bootstrap (it's DontDestroyOnLoad by NGO).
- Tunables in `NetworkConfig`: DefaultTransport, MaxPlayers 4, TickRate 30, Port 7777, DefaultJoinAddress,
  ListenAddress 0.0.0.0, ConnectTimeoutMs, MaxConnectAttempts, AutoHostInEditor.
- Auto-host (`AutoHostPolicy`): editor main instance + toggle on, or `-host`; never for MPPM virtual players
  (`CurrentPlayer.IsMainEditor`), `-client` or `-connect`. Batch/test runs host on loopback port 0.
  Builds wait for the panel ("Host (or play solo)") unless launched with `-host`/`-connect ip:port`.
- `NetworkPlayer` on the Player prefab: owner-authoritative `NetworkTransform` (position + yaw, interpolated);
  on non-owners disables camera/input/look/motor/interactor/HUD/camera feel/input overlay and the hit
  trigger, makes `PlayerRagdoll` remote. Owner writes `PlayerNetState` (grounded/sprint/crouch/ragdolled/
  resting/body position) -> remote `PlayerMotor.ApplyRemoteState` drives footsteps + `CarrierLoad`;
  remote ragdoll = capsule laid at the replicated body position. Client landings -> `ReportLandingRpc`
  (clamped) -> host copy raises `Landed` -> structural impact on the host. Pitch isn't replicated yet.
  The AudioListener stays on the scene camera (Cinemachine brain follows the one enabled player camera).
- Carried weight of remote players isn't replicated yet (carrying is local until networked loot), so a
  remote player weighs body weight only on the host for now.
- Tests: `NetTestHarness` (PlayMode) = several NetworkManagers in one process over loopback UTP; remote
  copies' CharacterControllers are disabled there because all "machines" share one physics world.
  `TestBuildingScene.Load()` waits for the auto-hosted local player. `TestProgressLog` writes
  `[Test] start/<result>` lines to the batch log (driven by `SteamTestRunGuard`, the assembly's one
  TestRunCallback), so a hung run shows which test it was in.
- `NetworkBootstrapFactory.Create` builds a non-scene session (tests/tools).

### M3.3 notes
- **Builds** (`Editor/Build/`): `BuildScript.Build(BuildPlatform, BuildFlavor)`; menus Tools/Abandoned/Build/
  Mac, Windows, Both, Dev Mac (for tests); `BatchCommands.BuildMac/BuildWindows/BuildBoth/BuildMacDev`.
  Flavours: Dev (Development, arm64 only, Builds/MacDev), Shareable (universal Mac / Win64, Builds/Mac,
  Builds/Windows), Release (Builds/<P>Release, no steam_appid.txt; for M10). Mono, StrictMode, Burst
  `*DoNotShip*` folders removed. steam_appid.txt goes beside the .app/.exe only: writing into the .app breaks
  its ad-hoc signature ("damaged" on download), and Facepunch's Init sets SteamAppId in the env anyway.
  Mac builds fail if `codesign --verify --deep --strict` fails. `BUILD.txt` in each folder (version,
  commit, flavour, time).
- **Scenes:** `BuildScenes.All` is the one list (TestBuilding for now); RebuildContent copies it into
  Build Settings (SampleScene left the build list; the file stays). Add menu/HQ/mall scenes there.
- **Version:** Player Settings > Version = `0.<milestone>.<patch>` (now 0.3.0; bump per milestone).
  `VersionInfo` (Core): `Version`, `Commit`, `Display` ("0.3.0 (abc1234)"), `CompatibilityKey`, `AreCompatible`
  (builds must match version+commit; an editor matches on version only) for M3.6's mismatch refusal.
  `BuildInfo` asset (Data/Core/Resources) is stamped with commit/time/flavour during a build and reset
  after, so it stays "editor" in git. F1 network view shows the version. `RebuildContent` applies
  `BuildScript.ApplyPlayerSettings` (productName Abandoned, Run In Background, Mono).
- **Nettest** (`Scripts/Networking/NetTest/`, namespace Abandoned.Networking): `NetTestRunner` is created
  only with `-nettest host|client` (`NetTestArgs`: -nettestPort, -nettestScenario, -nettestOut,
  -nettestClients, -nettestTimeout); it hosts/joins over UTP on 127.0.0.1, runs the scenario, writes
  `NetTestResult` JSON, quits 0/1. Host writes `<out>.ready` once listening.
  **Adding a scenario:** implement `INetTestScenario` (RunHost/RunClient coroutines using
  `NetTestContext`: `WaitFor`, `Collect`, `Receive`, `Snapshot`, `OwnPlayer`, `Fail/Abort/Note`, and
  `NetTestChannel` named messages host<->clients), register it in `NetTestScenarios.CreateRegistry`, put
  the verdict in a pure static check class (EditMode-testable like `BasicNetTestCheck`), add an in-process
  PlayMode test like `NetTestScenarioTests`, then `Tools/unity.sh nettest <name>`. Context works per
  NetworkManager (not `NetworkPlayer.Local`), so scenarios also run with 4 machines in one process.
- 'basic': host waits for 4 players, sends "move"; each client walks 2 m forward with the real motor
  (motor disabled, `Simulate` driven), stops, reports start/end; after 1.5 s settle every machine reports
  what it sees; host checks 4 views x 4 players, movers >= 1.5 m, every copy within 0.25 m of its owner.

### M3.4 notes
- **Plan/decisions (built without approval per the brief):** loot prefabs (LootPrefabGenerator) get a
  NetworkObject (DontDestroyWithOwner, no parent sync), an owner-authoritative NetworkTransform and
  `NetworkLoot`; no NetworkRigidbody: `Grabbable.SetPhysicsAuthority` makes every non-simulating copy
  kinematic (no interpolation) and the simulating one dynamic. All loot prefabs are registered in
  DefaultNetworkPrefabs (`NetworkContentBuilder.RegisterNetworkPrefabs`); `NetworkBootstrapFactory.Create`
  takes the prefab list (the test harness passes it).
- **Scripts/Networking/Loot/** (namespace Abandoned.Networking): `NetworkLoot` (NetworkVariables
  `LootValueState` + `LootHoldState`, request RPCs, impact report/relay, feedback RPCs, Despawn on
  shatter, orphan handling), `LootServerActions` (host side of pickup/release/unpocket, mirror on clients),
  `NetworkInteractionHandler` (set by every NetworkBootstrap; non-spawned items fall back to
  `LocalInteractionHandler`, so offline rigs behave as before), `ImpactReportFilter`, `LootRequestGuard`
  (one outstanding request per kind; a snagged item asks every physics step), `LootNetConfig`
  (Data/Networking/LootNetConfig.asset: MaxInheritedSpeed 8, MaxReleaseDistance 3.5, MaxReportedImpactSpeed
  25, MaxImpactPointDistance 2.5, ImpactReportInterval 0.05, ReportGraceTime 0.75, ...),
  `NetworkLootDebugView` (F1: owner, hold state, "sim here"/"follows", last clamped release speed; gizmo
  boxes green on owned items).
- **Rules:** host validates with `PickupRules` against its own copy of the player; held items move NGO
  ownership to the carrier, release removes it (host simulates thrown/resting loot); the carrier's
  reported release pose is used when within MaxReleaseDistance of the host's view of its eyes (the
  host's copy lags by interpolation). Release speed is clamped to a full-charge throw + MaxInheritedSpeed
  for remote players (non-finite -> 0). Pocketed items stay host-owned. A holder who leaves: the host
  frees the item where they last stood.
- **Impacts:** `LootItem.OnCollisionEnter` only judges hits on the machine that simulates the body
  (`CollisionImpact`); clients that own the item send `ReportImpactRpc`; the host filters (current owner,
  or the just-released owner within ReportGraceTime unless the host's own physics hit something since),
  clamps, plays the sound for everyone else, applies `ApplyImpact` + host noise. Damage/shatter feedback
  goes to clients by `DamagedRpc`/`ShatteredRpc` -> `LootItem.Replay*` -> `LootFeedback`. `LootItem.Remover`
  despawns on the host. Clients never apply damage (`HasValueAuthority` per item).
- **In-scene loot:** scene management is off, so a joining client destroys its own scene copies and
  respawns the host's from the source prefab hash. That only works if the saved scene marks them
  in-scene placed with the source hash, which NGO's OnValidate only writes once the scene is saved AND in
  Build Settings: `Editor/NetworkObjectIds.StampScene` does it at the end of `TestBuildingBuilder.Build`
  (prefab assets: `StampPrefab`). The validator checks the saved file, since opening the scene in the
  editor fixes it in memory and hides the problem. Without it every item is doubled on clients.
- **Bug found + fixed:** a freshly spawned host copy sometimes snapped to the prefab origin (NGO
  instantiates at the prefab position, then moves the transform; the interpolated body could write its
  stale pose back). `Grabbable.SnapBodyToTransform()` in `NetworkLoot.OnNetworkSpawn` (and on unpocket).
  Seen in about half of in-process 'loot' scenario runs before (items at (0,0,0), players flung), never in
  repeated runs after. A standalone reproduction (slow fixed step, ignore-collision pairs) did not
  trigger it, so there's no dedicated regression test; the in-process scenario test covers it.
- **Tests:** `NetLootKit` (spawn on the host, per-machine copies, wait for hold state) and
  `MachineSeparator` (in-process only: makes colliders of different machines ignore each other in
  FixedUpdate whenever something spawns; the machines share one physics world). Loot tests are in
  `NetworkLootPickupTests` / `NetworkLootValueTests` (base `NetworkLootTestBase`, host + 2 clients),
  `LootNetTestScenarioTests`, `TestBuildingLootTests.SoloHostSpawnsPlacedLootAsHostOwnedNetworkObjects`.
- **'loot' nettest** (`LootNetTestScenario` + `LootNetTestCheck`, `NetTestResult.lootActions/lootViews`):
  host spawns a laptop 1 m in front of each client; each client picks it up through the real handler,
  carries 0.75 s, throws (3 m/s fwd, 1.5 up); after settling the host applies a 9 m/s impact (25% loss);
  every machine reports every spawned loot item; the host's view is the truth. Timeouts record what the
  client saw (hold, owner, hints, positions).
- **Not done here (later tasks):** shared carrying (3.3); remote players' look pitch (the host's view of
  a client's eyes uses yaw only; unpocket uses the client's aim); trolley; appraisal UI (values are already
  replicated).

### M3.4-fix notes
- **Finding 1 (confirmed, fixed): loot couldn't knock down client players.** Only a player's owner has
  the hit trigger, and on a client every host-simulated item is a kinematic follower whose Rigidbody
  velocity is zero, so `PlayerHitDetector` ignored it. `Grabbable` now implements `Core/IVelocitySource`:
  dynamic -> `body.linearVelocity`; kinematic follower -> `Core/TrackedVelocity` sampled in LateUpdate
  (after NetworkTransform moves it), lightly smoothed, reset while pocketed, and a jump faster than
  40 m/s is a teleport (unpocket, snap to the host's pose), not a hit. The detector uses
  `IVelocitySource` when the body has one and still ignores other kinematic bodies. Same path covers a
  client-carried item swung into the host's player. F1 shows "v X m/s" over moving follower copies
  plus a cyan motion gizmo.
- **Finding 2 (confirmed, fixed): a client-carried item hitting resting loot.** `LootItem.CollisionImpact`
  now carries a `LootImpact` (speed, point, normal, other rigidbody). When the carrier's item hits a
  loot copy it only follows, its impact report names that item (`LootStrikes.StruckId`). The host,
  after the usual `ImpactReportFilter` checks on the reporter, applies the strike only to loot it
  simulates itself (free, host-owned, not shattered) and only if the contact point is within
  MaxImpactPointDistance of it: impact sound on every machine (the carrier never heard it), the same
  `ApplyImpact` + host noise, and a velocity-change push along the contact normal of
  speed x StruckPushTransfer x min(1, carried mass / struck mass) (a held item pushes a vase along,
  barely moves a safe). Carried targets are skipped: their own carrier reports their hits. F1 shows
  "struck X m/s" on the host. The carrier still meets the struck copy as a wall until the host's push
  arrives (one round trip); hand-test item under Needs you.
- Not changed: the host's kinematic copy of a client-carried item is still moved by transform
  (NetworkTransform); MovePosition would need a NetworkTransform subclass and the strike report already
  covers the gameplay effect.

### M3.5 notes
- **Plan/decisions (built without approval per the brief):**
  - Data: `Data/Interaction/SharedCarryConfig.asset` (RequiredHeavy 2, RequiredHuge 3, PointsHeavy 2,
    PointsHuge 4, grip 0.45-1 m, LiftClearance 0.3, springs, wobble, tether, BreakDistance 2.5, target
    timeout/deviation). `LootDefinition.RequiredCarriers` (0 = class default; military generator = 4 per
    GDD) and `CarryPoints` (local, empty = generated by `CarryPointLayout` from Size: ends of the long
    horizontal axis, then long-side middles, at centre height). The loot prefab generator adds
    `SharedCarryable` + `NetworkSharedCarry` to every Heavy/Huge prefab; no per-item code.
  - Interaction (works offline too): `SharedCarryable` holds carriers per point, grip offsets, targets,
    crew rule (`IsLifted` = carriers >= required), group speed; `SharedCarryForces` is the host-side
    physics step; `PlayerCarrier.Held` points at the shared item while you hold a handle (`IsSharing`),
    so hands-full, RMB, ragdoll-drops and HUD reuse the solo paths. `PickupRules.CanGrabPoint` (nearest
    free handle to your feet, within Reach + tolerance of your eyes); `CanPickUp` refuses shared items.
  - **Hold target model:** at grab the host stores each carrier's grip = horizontal offset feet -> handle
    (clamped 0.45-1 m). Desired handle position = feet + grip, at (handle height above the item's bottom
    + LiftClearance) above the feet. Lifted: the mean pull moves the item (accel spring, gravity off), the
    per-handle differences turn it (best-fit small rotation: yaw when carriers walk around each other,
    tilt when one is higher), an upright term stops roll-overs, and carriers' horizontal disagreement adds
    a rocking spin about the line between them (WobbleGain). Horizontal speed capped at the slowest
    carrier's speed x SpeedSlack. Under-crewed: gravity on, horizontal pull only, applied at floor level
    (a tall rack slides instead of tipping), stiff upright; Heavy drags at full pull (solo drag kept), Huge
    only creeps (CreepSpeed 0.35 m/s).
  - **Slowest carrier:** each carrier's capability = gait speed (crouch / walk; sprint only if
    AllowSprint) x their own load slowdown (share + pockets). Every machine computes the min from its
    mirrored copies (crouch state is replicated) and caps its own motor (`PlayerMotor.MaxSpeed`); the host
    caps the item too.
  - **Tether:** `PlayerMotor.SetTether(anchor, slack, pullStart, pullSpeed)` (`TetherMath`): anchor = handle
    - grip on this machine's copy. Inside 0.9 m: free; beyond: moving further away is cancelled; beyond
    1.4 m: pulled back at up to 3 m/s. The host releases a carrier whose handle is > 2.5 m from their
    desired hold point (fell off a ledge with it, stuck) with a "Lost your grip" hint, and anyone ragdolled.
  - **Load:** `Grabbable.WeightOnHolder` = share (weight / carriers) when lifted, 0 when dragged; so
    `CarrierLoad` sends each carrier's share through their own feet; `LootItem.LoadWeight` is 0 while lifted.
- **Network design (CLAUDE.md: host simulates shared carries from the carriers' input):**
  - Ownership never leaves the host for a Heavy/Huge item (a grab takes it back if needed); the existing
    host-owned NetworkTransform replicates the simulated pose; clients' copies are kinematic followers.
  - `NetworkSharedCarry.state` (`SharedCarryState`, server-write NetworkVariable): holder player
    NetworkObjectId + grip per handle (max 4). Every machine mirrors it onto its own copies
    (`SharedCarryable.SetCarrier`), retrying while a holder's player hasn't spawned yet, so carried weight
    (structure on the host), HUD and tether agree everywhere. NetworkLoot's own hold state stays Free.
  - Requests: E -> `NetworkInteractionHandler` -> client pre-checks `CanGrabPoint`, sends
    `RequestGrabRpc`; the host re-validates with its own view (`SharedCarryServer.TryGrab`) and picks the
    nearest free handle, or answers with a hint RPC. RMB / ragdoll / throw -> `RequestLetGoRpc`.
  - Input: each client carrier sends `SubmitTargetRpc(desired hold point)` once per network tick,
    **unreliable** (a lost one is superseded by the next). The host clamps it to within
    MaxTargetDeviation (1.5 m) of what it computes from its own (interpolated) view of that player and
    uses it while fresher than TargetTimeout (0.5 s), else its own view. The host's own player is computed
    directly. Why send it at all: the host's copy of a remote player lags by the interpolation buffer, the
    client's own position doesn't.
  - Impacts: the host simulates the body, so a dropped shared item's landing is judged and damaged by the
    host's own physics (normal LootItem path) and replayed everywhere by M3.4's RPCs.
- **'sharedcarry' nettest** (`SharedCarryNetTestScenario` + `SharedCarryNetTestCheck`,
  `NetTestResult.sharedActions/sharedHost`): the host spawns the server rack 1.2 m in front of the two
  easternmost clients, its long axis between them; both grab through the real handler, then walk 1.2 m
  forward together, hold 1 s, let go; the host records crew size, lift height and received targets;
  after 2.5 s every machine reports the rack; positions must agree within 0.25 m, it must have moved
  >= 0.8 m, lifted >= 0.15 m, and stay host-owned. Other clients only watch.
- **Tests:** `SharedCarryTestBase` (5 m platform arena via the new `NetworkLootTestBase.ArrangeArena`
  hook; `NetLootKit.Spawn` takes a yaw), `SharedCarryNetworkTests`, `SharedCarryLoadTests`,
  `SharedCarryNetTestScenarioTests`; EditMode `SharedCarryRulesTests`, `SharedCarryNetTestCheckTests`.
- **Not done here:** carriers can't climb stairs together any differently from walking (stairs/ramps
  just raise each carrier's hold point - untested in the building); the trolley (M6 equipment) will
  replace solo drag; remote players' look pitch still isn't replicated (not needed: targets come from feet).

## M1 review (multi-agent, adversarially verified)
4 reviewers (netcode, correctness, physics, rules) + 1 refuting verifier. Fixed before tagging:
walking into resting heavy loot ragdolled you; click-to-recapture after Esc threw the held item;
get-up could stand you on loot; hold point inside your capsule when looking down; ragdoll freed the
cursor and reset pitch; bob/footsteps ran on stale state while ragdolled; stale interactor/HUD state;
inventory kept destroyed items; held items could shove Huge loot (now high-friction material);
kerb drops landed at ground-stick speed; crouched+holding couldn't stand; far pickups auto-dropped
(grace time); footstep noise used the audio preference (now gameplay noise values); loot Noise unused
(now drives impact noise); prefabs could drift from definitions (validator checks); F1 views for loot
and ragdoll; private serialized profile fields; one class per test file. 6 regression tests added.
**Deferred to M3 (networking, rejected for M1 but real later):** loot value initialised in Start and
not replicated [done M3.4]; hold/pocket state only on the applying machine [done M3.4]; carrier-owned
held items mean the host doesn't see their collisions (use `LootItem.ApplyImpact` with client reports)
[done M3.4]; LootFeedback driven by host-only events [done M3.4]; Shatter must Despawn not Destroy
[done M3.4]; every Player prefab instance has an enabled CinemachineCamera/input/look (disable on
non-owners) [done M3.2]; remote players' footsteps need replicated grounded state [done M3.2].

## Facepunch spike result (end of M1, ~1 h)
Branch `spike/facepunch-transport` @ `45fc975` (throwaway, not merged, not pushed).
1. **Compiles:** community `com.community.netcode.transport.facepunch` 2.0.0 (git URL, commit 2444fe2,
   CHANGELOG says it targets NGO 1.0) compiles against NGO 2.13.3 with zero errors and warnings.
2. **Blocker on this Mac:** the package bundles an old Facepunch.Steamworks whose macOS
   `libsteam_api.bundle` is i386/x86_64 only. The Unity editor here is arm64 (Apple Silicon), so
   `SteamClient.Init` throws `DllNotFoundException ... (have 'i386,x86_64', need 'arm64')`.
   Steam networking would not work in the editor or in Apple Silicon Mac builds as shipped.
3. **Fix verified:** an embedded copy of the transport with Facepunch.Steamworks **2.5.2**
   (2026-04-23; universal x86_64+arm64 `libsteam_api.dylib`, `Facepunch.Steamworks.Posix.dll`,
   `Win64.dll`) compiles unchanged and the native library loads; `SteamClient.Init(480)` then
   fails only with "Could not determine Steam client install directory" because Steam wasn't running.
4. **Not tested (needs the user):** an actual host/client connection over Steam relay. Steam was
   not running and I did not launch it (it would log into the user's account unattended).
5. **Other issues found by reading the transport (to patch in M3):**
   - `Shutdown()` calls `SteamClient.Shutdown()` -> ending a network session would kill Steam
     for lobbies/rich presence. Move Steam lifetime to our own SteamBootstrap.
   - `GetCurrentRtt` always returns 0 (NGO stats/interpolation get no RTT).
   - `StartClient/StartServer` return true even when Steam failed to init.
   - Package is unmaintained (last change targets NGO 1.0).
**Recommendation for M3:** embed a forked copy of the transport in `Game/Packages/` (the user
approves package changes) with Facepunch.Steamworks 2.5.2 binaries and the three patches above.

## Decisions made during autobuild
- Movement runs on its own simulation clock (`PlayerMotor.Simulate(input, dt)`), not `Time.time`,
  so it's deterministic, testable with exact steps, and ready for M3 (owner simulates, transform syncs).
- Stamina is ticked by the motor (no own Update) to stay in step with movement.
- One shared F1 switch: `Abandoned.Core.DebugView` + one `DebugViewToggle` per scene.
- Camera: Cinemachine. Scene Main Camera has a CinemachineBrain; Player prefab has a
  CinemachineCamera hard-locked (zero damping) to `CameraRoot`.
- Crouch is hold by default; `PlayerMovementConfig.CrouchIsToggle` switches to toggle.
- Content validation: `IValidatable` on configs/definitions; `[OptionalReference]` marks
  serialized references allowed to stay empty.
- Editor builders are the source of truth for generated content (Player prefab, TestBuilding).
  `BatchCommands.RebuildContent` regenerates all of it in dependency order.

- Interaction goes through `InteractionService.Handler` (`IInteractionHandler`). Single-player uses
  `LocalInteractionHandler` (validate with `PickupRules`, then apply). M3 adds a network handler:
  client sends request -> host runs the same `PickupRules` -> applies. Hold physics runs on the
  carrier (`PlayerCarrier.FixedUpdate`), value/damage stay host-side.
- Hand slots 1/2 are deferred to equipment (M6). Loot: Pocket class -> pockets (4), everything
  else is held physically one at a time. Tab + RMB drops the last pocket item.
- `PlayerMotor.MovementVelocity` excludes the ground-stick push; drops/throws inherit it.
- HUD is OnGUI placeholder (`InteractionHud`) until the UI milestone.

- Loot: 13 GDD examples as `Data/Loot/Loot_<id>.asset` (catalog builder never overwrites tuning);
  prefabs generated into `Prefabs/Loot/` (Tools/Abandoned/Generate Loot Prefabs or right-click a
  definition > Generate Prefab). Value = seeded roll (`LootMath`), damage = speed along the contact
  normal vs `LootDamageConfig` profile per fragility. Host-only damage via `GameAuthority.IsHost`;
  `LootItem.ApplyImpact` is public so M3 can apply client-reported impacts for carrier-owned physics.
- Physics layers 8 Player, 9 Loot, 10 Debris, 11 Structure. Debris ignores Player and Loot
  (`GameLayers.ApplyCollisionRules` at startup).
- Placeholder sounds are synthesised in code (`PlaceholderAudio`); floating text via OnGUI.

- Ragdoll: 9-part primitive ragdoll (no humanoid model yet), local only. Triggers: K (Debug map),
  `PlayerMotor.Landed` fall height > `PlayerRagdollConfig.FallHeight`, heavy hits via the
  `PlayerHitDetector` trigger (CharacterControllers get no rigidbody collision callbacks).
  `IWeighted` (Core) gives gameplay weight without Player depending on Interaction.
- `PlayerMotor.IsGrounded` now reflects the state after the step's move.

- Feel: `FeelSettings` (Data/Player) with on/off per effect. Camera chain: CameraRoot (pitch) ->
  Eye (bob/dip/shake offsets) <- CinemachineCamera hard-locked. Shake is our own trauma model
  driven by `CameraShake.Emit(position, momentum)`; each camera thresholds by distance/momentum.
  Loot impacts emit momentum = gameplay weight x impact speed. Collapses will emit too (M2).
- Surfaces: `SurfaceTag` on colliders (untagged = Concrete). TestBuilding: ground floor concrete,
  upper floor + balconies wood, stairs metal, outside dirt, parking asphalt.
- `PlayerFootsteps.Stepped` event is the hook for NoiseEvents in 2.5.

- Structure: host-side `StructureSimulation` solves the logical load each FixedUpdate (downward rays on
  the Structure layer only, weight split across hit points), sections hold state, `SectionPresentation`
  is local. `StructureSignals.SectionCollapsed` (Core) lets players ragdoll without Player->Structure deps.
- Solo drag: Heavy items can be dragged by one player (`CarryConfig.SoloDragHeavy`) as a stand-in for
  the hand trolley; slow, loud, can't throw, rests its own weight on the floor. Huge still needs a team.
- Per-section `capacityMultiplier` = authored weakness (rotten floor). TestBuilding weak spots:
  Balcony_U_3_2 (statue, 45% hp, 95% cap), Balcony_U_0_1 (piano, 40% hp, 24% cap),
  Tile_U_3_3 (70% hp, 8% cap), Balcony_U_2_3 (50% hp, 12% cap). Stability 85%, seed 2026.

## M2 review (multi-agent, adversarially verified) — all fixed
Pre-damage could start weak sections Failing (now floored at MinStartHealth); stairs ignored impacts
(SectionColliderRelay on child colliders); one landing on a seam hit every section in full (impacts
now pooled per body per step and split); landing players didn't damage floors (StructureSignals.Impact);
ragdolled players stopped weighing anything; long items on ramps leaked load to the floor below (rays
from the item's top); one hit could skip straight to Failing (WarningFloor); cracks vanished after a
re-roll and floated while sagging; collapse ragdoll margin hit players on the neighbouring tile;
dead StructureConfig.RestingSpeed removed; drag-noise numbers moved to CarryConfig; ordinal-sorted
section ids; debug keys only while F1 is on and in dev builds; builder errors on drifted names;
fractured tile size shared with the builder and regenerated each rebuild; stairs can't collapse in
TestBuilding (GDD 6.4: only route out) until a rope/window fallback exists; weak tests strengthened
and StructureTests split. 9 regression tests added.

### M3.1-fix notes
- `SteamBootstrap.ShutdownSteam()`: if `NetworkManager.Singleton` is listening, it requests NGO shutdown
  and hands Steam's shutdown to `DeferredSteamShutdown`, which fires on NGO's `OnServerStopped`/
  `OnClientStopped` (raised after the transport is closed) or, as a fallback, `NetworkManager.OnDestroying`.
  On quit, whichever OnApplicationQuit runs first, NGO's own quit handler shuts down synchronously and
  Steam follows. `IsAvailable` goes false immediately.
- `SteamInitPolicy.TestRunActive` is set by `Tests/PlayMode/SteamTestRunGuard` (`[assembly: TestRunCallback]`,
  RunStarted/TestStarted). It lives in the PlayMode test assembly, which is loaded for EditMode runs too.
  The guard only applies to the real `FacepunchSteamClient`; injected fakes still init.
- PlayMode tests asmdef now references Unity.Netcode.Runtime + Unity.Networking.Transport (in-process NGO).

### M3.3-fix notes
- `GitInfo.CommitLabel()` (Editor/Build): short HEAD + `-dirty` when `git status --porcelain` shows changes
  under Game/Assets, Game/Packages or Game/ProjectSettings, excluding the known Unity churn files below and
  the BuildInfo asset the build itself stamps (otherwise every build would be dirty). Failed status = dirty.
- `BuildScript` stamps once per build and writes the same commit/time into BUILD.txt (no second git call);
  `BuildBoth` shares one build time so the Mac + Windows zips of one run stay compatible.
- `VersionInfo.FormatKey(version, commit, builtAt)`: exact commits -> `0.3.0+abc1234`; dirty/unknown ->
  `0.3.0+abc1234-dirty@<builtAtUtc>`. `AreCompatible`: same version required; editor matches any commit;
  otherwise same commit AND (exact, or identical non-empty build time).
- `Tools/unity.sh` zip_build warns when zipping a -dirty/unknown build. Rule: make shareable zips after
  the task's commit (the orchestrator's end-of-milestone builds already run on committed trees).
- If a new Unity churn file appears, add it to both the churn list below and `GitInfo.ShippedPaths`.

### M3.2-fix notes
- Remote copies switch their CharacterController off while the owner is ragdolled (the owner's root stays
  where they fell, so an upright collider there was an invisible pillar) and restore its previous state
  on getting up (`PlayerRagdoll.ApplyRemoteState`, transition-based so the test harness's disabled remote
  colliders stay off).
- `PlayerLook.SyncYawFromTransform()`: called from `NetworkPlayer.OnNetworkSpawn` (owner) and `Start`,
  because NGO applies the spawn pose after Instantiate/OnEnable; players now face their spawn point's forward.
- Tests: `RemoteCopyDropsItsStandingColliderWhileTheOwnerIsRagdolled`, `PlayersFaceTheirSpawnPointsForwardAfterLooking`.

## Open problems
- M3.5: one full PlayMode run failed `NetworkLootHitTests.HostThrownSafeKnocksDownTheClientPlayerOnTheClient`
  (client never ragdolled within 4 s); it passed alone 3x and in two further full runs. The safe now has
  SharedCarryable (idle when nobody holds it). If it recurs, look at that test's timing first.
- One `Tools/unity.sh all` run printed no PlayMode results line; rerun passed 103/103 and a second full
  `all` passed. Possible flake; if it recurs, check Game/Logs/batch/PlayMode.log for a crash.

## Unity-generated churn left uncommitted on purpose
DefaultVolumeProfile.asset, PC_RPAsset.asset, UniversalRenderPipelineGlobalSettings.asset (player builds
fill its runtime-settings list), probuilder Settings.json,
ProjectSettings/Packages/com.unity.multiplayer.tools/, ProjectSettings/SceneTemplateSettings.json.
