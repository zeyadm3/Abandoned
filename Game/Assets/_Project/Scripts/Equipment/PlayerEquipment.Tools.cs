using Abandoned.Core;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>The gear added from 0.17 on, host-side: what each one does when used. Tunables in <see cref="ToolConfig"/>.</summary>
    public partial class PlayerEquipment
    {
        private float nextToolUse;

        // Where its owner is looking: body yaw and the replicated camera pitch (the host has no remote camera).
        private Vector3 Aim => Quaternion.Euler(player.State.Pitch, transform.eulerAngles.y, 0f) * Vector3.forward;

        /// <summary>Host: a tool that is never used up. True when <paramref name="d"/> was one (handled here).</summary>
        private bool UseTool(EquipmentDefinition d)
        {
            switch (d.Kind)
            {
                case EquipmentKind.Sledgehammer: Smash(); return true;
                case EquipmentKind.SprayChalk: return true; // drawn while held (TickHeldTool)
                // @@tools
                default: return false;
            }
        }

        /// <summary>Host: a consumable added from 0.17 on. True when it was used (and is spent).</summary>
        private bool UseConsumable(EquipmentDefinition d)
        {
            switch (d.Kind)
            {
                case EquipmentKind.CrashMat: return LayMat();
                case EquipmentKind.GlowSticks: return ThrowLight(EquipmentKind.GlowSticks, 0.7f);
                case EquipmentKind.Flares: return ThrowLight(EquipmentKind.Flares, 1f);
                case EquipmentKind.MovingBlanket: return Wrap();
                // @@consumables
                default: return false;
            }
        }

        // Host (0.17.0): a full swing at the floor in front: three or four break a sound section, through
        // its usual Cracking warning. Protected routes and stairs can't collapse, so they shrug it off.
        private void Smash()
        {
            ToolConfig config = ToolConfig.Current;
            if (Time.time < nextToolUse) return;
            nextToolUse = Time.time + config.SledgeCooldown;
            int mask = 1 << Mathf.Max(0, GameLayers.StructureLayer);
            if (!Physics.Raycast(Eye, Aim, out RaycastHit hit, config.SledgeReach, mask, QueryTriggerInteraction.Ignore))
            {
                SoundRpc(Audio.SoundId.SledgeSwing, Eye + Aim);
                return;
            }
            StructuralSection section = hit.collider.GetComponentInParent<StructuralSection>();
            NoiseSystem.Emit(hit.point, config.SledgeNoise, NoiseSource.Other);
            SoundRpc(Audio.SoundId.SledgeHit, hit.point);
            if (section == null || !section.CanCollapse)
            {
                HintRpc("Nothing gives here: this floor won't break");
                return;
            }
            section.ApplyImpact(config.SledgeMomentum);
        }

        // Host (0.17.1): the mat unrolls flat on the floor ahead (under the hole you mean to drop through).
        private bool LayMat()
        {
            Vector3 forward = Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * Vector3.forward;
            Vector3 probe = transform.position + forward * 1.6f + Vector3.up * 1f;
            if (Physics.Linecast(Eye, probe, wallMask, QueryTriggerInteraction.Ignore) ||
                !Physics.Raycast(probe, Vector3.down, out RaycastHit floor, 2f, wallMask, QueryTriggerInteraction.Ignore))
            {
                HintRpc("No room to lay the mat here");
                return false;
            }
            return SpawnGear(EquipmentKind.CrashMat, floor.point, Quaternion.Euler(0f, transform.eulerAngles.y, 0f)) != null;
        }

        private float nextHeldTool;

        // Owner (0.17.6+): tools that act every few frames while the button is held.
        private void TickHeldTool(Player.PlayerInputFrame input)
        {
            if (!input.UseHeld || Time.time < nextHeldTool || carrier == null || carrier.Held != null || carrier.IsRagdolled) return;
            EquipmentDefinition d = InHand;
            if (d == null || d.Kind != EquipmentKind.SprayChalk || look == null || look.EyeRoot == null) return;
            nextHeldTool = Time.time + ToolConfig.Current.ChalkInterval;
            Transform eye = look.EyeRoot;
            SprayRpc(eye.position, eye.forward);
        }

        // Host: a dot of chalk where the owner's eye ray meets a surface (checked from the host's own physics).
        [Rpc(SendTo.Server)]
        private void SprayRpc(Vector3 eye, Vector3 direction, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || player == null || player.IsDead) return;
            if (!Has(EquipmentKind.SprayChalk) || InHand == null || InHand.Kind != EquipmentKind.SprayChalk) return;
            if (Vector3.Distance(eye, transform.position + Vector3.up * 1.5f) > 1.5f || direction.sqrMagnitude < 0.5f) return;
            if (!Physics.Raycast(eye, direction.normalized, out RaycastHit hit, ToolConfig.Current.ChalkReach, wallMask, QueryTriggerInteraction.Ignore)) return;
            Extraction.RunChalk.Current?.ServerAdd(hit.point, hit.normal, (byte)Mathf.Max(0, player.CrewSeat));
        }

        // Host (0.17.7): lob a glow stick or a flare the way the noise maker is thrown (a gentler arc for a stick).
        private bool ThrowLight(EquipmentKind kind, float strength)
        {
            Transform t = transform;
            Vector3 at = Eye + t.forward * 0.6f;
            if (Physics.Linecast(Eye, at + t.forward * 0.15f, out RaycastHit hit, wallMask, QueryTriggerInteraction.Ignore))
                at = Eye + t.forward * Mathf.Max(0f, hit.distance - 0.2f);
            NetworkObject thrown = SpawnGear(kind, at, t.rotation);
            if (thrown == null) return false;
            thrown.GetComponent<ThrownLight>().Launch(Aim * throwSpeed.x * strength + Vector3.up * throwSpeed.y * 0.6f);
            return true;
        }

        // Host (0.18.5): wrap the loot you're looking at (not one in someone's hands).
        private bool Wrap()
        {
            int lootMask = LayerMask.GetMask(GameLayers.Loot);
            Networking.NetworkLoot loot = Physics.Raycast(Eye, Aim, out RaycastHit hit, ToolConfig.Current.BlanketReach, lootMask, QueryTriggerInteraction.Ignore)
                ? hit.collider.GetComponentInParent<Networking.NetworkLoot>() : null;
            if (loot == null)
            {
                HintRpc("Look at the item you want to wrap");
                return false;
            }
            if (!loot.ServerWrap())
            {
                HintRpc("That one is already wrapped");
                return false;
            }
            return true;
        }

        /// <summary>Host: spawn one of the newer gear's network objects.</summary>
        private NetworkObject SpawnGear(EquipmentKind kind, Vector3 at, Quaternion rotation) =>
            Spawn(GearPrefabs.Current.For(kind), at, rotation);
    }
}
