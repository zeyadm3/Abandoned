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
  multi-process localhost nettest `Tools/nettest.sh` with scenario registry; 'basic' passes 4/4).
  **Next:** networked loot (pickup/drop/throw via host RPCs, replicated value/damage, Despawn on shatter),
  shared carrying, networked structure, Steam lobby/invite/relay, robustness (PLAYBOOK 3.2-3.6). Add a
  nettest scenario for each (see "M3.3 notes").
- **Verification state after M3.3:** compile clean; rebuild OK; verify ALL PASS; EditMode 94/94;
  PlayMode 135/135; screenshots OK (M1_player_eye checked); `Tools/unity.sh build` OK (Mac universal +
  Windows Mono, zipped); `Tools/unity.sh nettest` 4/4 PASS.
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
not replicated; hold/pocket state only on the applying machine; carrier-owned held items mean the
host doesn't see their collisions (use `LootItem.ApplyImpact` with client reports); LootFeedback
driven by host-only events; Shatter must Despawn not Destroy; every Player prefab instance has an
enabled CinemachineCamera/input/look (disable on non-owners) [done M3.2]; remote players' footsteps need
replicated grounded state [done M3.2].

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

### M3.2-fix notes
- Remote copies switch their CharacterController off while the owner is ragdolled (the owner's root stays
  where they fell, so an upright collider there was an invisible pillar) and restore its previous state
  on getting up (`PlayerRagdoll.ApplyRemoteState`, transition-based so the test harness's disabled remote
  colliders stay off).
- `PlayerLook.SyncYawFromTransform()`: called from `NetworkPlayer.OnNetworkSpawn` (owner) and `Start`,
  because NGO applies the spawn pose after Instantiate/OnEnable; players now face their spawn point's forward.
- Tests: `RemoteCopyDropsItsStandingColliderWhileTheOwnerIsRagdolled`, `PlayersFaceTheirSpawnPointsForwardAfterLooking`.

## Open problems
- One `Tools/unity.sh all` run printed no PlayMode results line; rerun passed 103/103 and a second full
  `all` passed. Possible flake; if it recurs, check Game/Logs/batch/PlayMode.log for a crash.

## Unity-generated churn left uncommitted on purpose
DefaultVolumeProfile.asset, PC_RPAsset.asset, UniversalRenderPipelineGlobalSettings.asset (player builds
fill its runtime-settings list), probuilder Settings.json,
ProjectSettings/Packages/com.unity.multiplayer.tools/, ProjectSettings/SceneTemplateSettings.json.
