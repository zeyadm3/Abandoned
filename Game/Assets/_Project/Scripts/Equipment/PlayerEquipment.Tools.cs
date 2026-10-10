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

        /// <summary>Host: spawn one of the newer gear's network objects.</summary>
        private NetworkObject SpawnGear(EquipmentKind kind, Vector3 at, Quaternion rotation) =>
            Spawn(GearPrefabs.Current.For(kind), at, rotation);
    }
}
