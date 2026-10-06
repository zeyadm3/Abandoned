# ABANDONED — Build Progress

Living log for autonomous work. A new session must be able to continue from this file alone.
Read CLAUDE.md first, then this file, then Docs/PLAYBOOK.md for the next task's prompt.

## Current state
- **Branch:** `autobuild` (do NOT commit to `main`; main is at `43ca2f9`, tagged `milestone-0` at `eb27120`).
- **Milestone:** 2 — The Weight, done and tagged `milestone-2` locally (M1 tagged `milestone-1`). Next: Milestone 3 after the user's hands-on feel check.
- **Current task:** none — waiting for the user's Play-mode checks and "push autobuild". Next is M3 (start with the Facepunch fork, see spike result).
- **Pushing:** `autobuild`, `milestone-1` and `milestone-2` pushed to origin (user said "push autobuild"). `spike/facepunch-transport` stays local. Push further autobuild commits only after the user confirms a step works.

## How to verify (batch mode, Unity must be closed)
```
Tools/unity.sh compile      # zero errors, no warnings from Assets/_Project
Tools/unity.sh rebuild      # regenerates Player prefab + TestBuilding scene (and later loot/structure content)
Tools/unity.sh verify       # Verify Project Setup + Content Validator (prefabs wired, no missing scripts, SO rules)
Tools/unity.sh editmode     # EditMode tests -> Game/TestResults/EditMode.xml
Tools/unity.sh playmode     # PlayMode tests -> Game/TestResults/PlayMode.xml
Tools/unity.sh screenshots  # PNGs -> Game/Screenshots/ (gitignored); look at them
Tools/unity.sh all          # everything above in order
```
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
enabled CinemachineCamera/input/look (disable on non-owners); remote players' footsteps need
replicated grounded state.

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

## Open problems
- One `Tools/unity.sh all` run printed no PlayMode results line; rerun passed 103/103 and a second full
  `all` passed. Possible flake; if it recurs, check Game/Logs/batch/PlayMode.log for a crash.

## Unity-generated churn left uncommitted on purpose
DefaultVolumeProfile.asset, PC_RPAsset.asset, probuilder Settings.json,
ProjectSettings/Packages/com.unity.multiplayer.tools/, ProjectSettings/SceneTemplateSettings.json.
