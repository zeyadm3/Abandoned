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
                case EquipmentKind.Lockpick: return true;   // used on a shutter (E), not in the air
                case EquipmentKind.ThermalCamera: return true; // switched on its owner's machine (ThermalCamera)
                // Not from a locker or a vent: the pilot's controls are already off there, and landing would
                // switch them back on with the player still hidden.
                case EquipmentKind.Drone: if (!player.IsHiding) LaunchDrone(); return true;
                // @@tools
                default: return false;
            }
        }

        /// <summary>Host: a consumable added from 0.17 on. True when it was used (and is spent).</summary>
        private bool UseConsumable(EquipmentDefinition d)
        {
            switch (d.Kind)
            {
                case EquipmentKind.GlowSticks: return ThrowLight(EquipmentKind.GlowSticks, 0.7f);
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
                HintRpc(Loc.Mark("Nothing gives here: this floor won't break"));
                return;
            }
            section.ApplyImpact(config.SledgeMomentum);
        }

        // Host (0.17.7): lob a glow stick the way the noise maker is thrown, on a gentler arc.
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

        private NetworkObject drone;

        // Host (0.20.5): one drone per pilot, owned by them (their machine flies it).
        private void LaunchDrone()
        {
            if (drone != null && drone.IsSpawned) return;
            NetworkObject prefab = GearPrefabs.Current.For(EquipmentKind.Drone);
            if (prefab == null) return;
            Vector3 at = Eye + Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * Vector3.forward * 0.8f;
            if (Physics.Linecast(Eye, at, wallMask, QueryTriggerInteraction.Ignore))
            {
                HintRpc(Loc.Mark("No room to launch the drone here"));
                return;
            }
            drone = NetworkManager.SpawnManager.InstantiateAndSpawn(prefab, OwnerClientId, position: at, rotation: Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
        }

        /// <summary>Host: spawn one of the newer gear's network objects.</summary>
        private NetworkObject SpawnGear(EquipmentKind kind, Vector3 at, Quaternion rotation) =>
            Spawn(GearPrefabs.Current.For(kind), at, rotation);
    }
}
