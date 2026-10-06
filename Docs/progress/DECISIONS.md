# Decisions made during the autobuild (implementation-level)

Project-level decisions live in CLAUDE.md's decisions log; these are the smaller ones.

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

