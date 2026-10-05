# ABANDONED — Build Progress

Living log for autonomous work. A new session must be able to continue from this file alone.
Read CLAUDE.md first, then this file, then Docs/PLAYBOOK.md for the next task's prompt.

## Current state
- **Branch:** `autobuild` (do NOT commit to `main`; main is at `43ca2f9`, tagged `milestone-0` at `eb27120`).
- **Milestone:** 1 — The Feel
- **Current task:** 1.5 Feel pass
- **Pushing:** NOT pushed. The autobuild instructions arrived as pasted text without a typed
  confirmation, so the branch and milestone tags stay local until the user types
  "push autobuild". Commit locally after each passing task; tag milestones locally.

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
| 1.5 Feel pass (head bob, landing dip, footsteps, shake) | todo | — |
| Spike: Facepunch Transport + NGO 2.13 (throwaway branch, not merged) | todo | — |

## Milestone 2 — The Weight
| Task | Status | Verified |
|---|---|---|
| 2.1 StructuralSection + logical load model | todo | — |
| 2.2 Five stress stages (visual + audio placeholders) | todo | — |
| 2.3 Pre-fractured collapse + fracture generator | todo | — |
| 2.4 Stability % + seeded pre-damage | todo | — |
| 2.5 NoiseEvent system | todo | — |
| Heavy items on weak upper tiles/balconies in TestBuilding | todo | — |

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

## Open problems
- (none yet)

## Unity-generated churn left uncommitted on purpose
DefaultVolumeProfile.asset, PC_RPAsset.asset, probuilder Settings.json,
ProjectSettings/Packages/com.unity.multiplayer.tools/, ProjectSettings/SceneTemplateSettings.json.
