using System.Collections.Generic;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Structure;
using Abandoned.Voice;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>Using the gear in hand (left mouse, empty hands): crowbar strikes, medkit, battery, planks, noise maker. Host-checked.</summary>
    public partial class PlayerEquipment
    {
        [Rpc(SendTo.Server)]
        private void UseRpc(int slot, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId || slot < 0 || slot >= EquipState.SlotCount) return;
            // A knocked-down player's root stays where they fell from: nothing is used from there.
            if (player == null || player.IsDead || carrier.Held != null || carrier.IsRagdolled) return;
            EquipmentDefinition d = InSlot(slot);
            if (d == null) return;
            if (d.Kind == EquipmentKind.Crowbar) { Pry(); return; } // a tool: never used up
            if (!d.Consumable)
            {
                UseTool(d); // 0.17+ tools
                return;
            }
            bool used = d.Kind switch
            {
                EquipmentKind.Medkit => TreatInjury(),
                EquipmentKind.Battery => RefillBattery(),
                EquipmentKind.Planks => LayPlanks(),
                EquipmentKind.NoiseMaker => Throw(),
                _ => UseConsumable(d), // 0.17+ consumables
            };
            if (used) ServerConsume(slot);
        }

        private Vector3 Eye => transform.position + Vector3.up * 1.5f;

        // Host: the plank appears lying lengthwise ahead (walkways are one 4 m tile wide, the plank is
        // 4.4 m), only where it fits: spawned inside a wall, PhysX would shove it through to the far side.
        private bool LayPlanks()
        {
            if (plankPrefab == null || !plankPrefab.TryGetComponent(out Loot.LootItem item) || item.Definition == null) return false;
            Vector3 half = item.Definition.Size * 0.5f;
            Quaternion rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            Vector3 forward = rotation * Vector3.forward;
            Vector3 at = transform.position + forward * (0.6f + half.z) + Vector3.up * 0.4f;
            if (Physics.CheckBox(at, half - Vector3.one * 0.02f, rotation, wallMask, QueryTriggerInteraction.Ignore) ||
                Physics.Linecast(Eye, at, wallMask, QueryTriggerInteraction.Ignore))
            {
                HintRpc(Loc.Mark("No room to lay the planks here"));
                return false;
            }
            return Spawn(plankPrefab, at, rotation) != null;
        }

        // Host (M9.4): strike the section in front of you; enough strikes break even a sound floor (always
        // through a Cracking warning first). Loud: monsters hear it.
        private void Pry()
        {
            if (Time.time < nextPry) return;
            nextPry = Time.time + crowbarCooldown;
            int mask = 1 << Mathf.Max(0, GameLayers.StructureLayer);
            Vector3 eye = player.transform.position + Vector3.up * 1.5f;
            // Where its owner is looking: body yaw and the replicated camera pitch (the host has no remote camera).
            Vector3 aim = Quaternion.Euler(player.State.Pitch, transform.eulerAngles.y, 0f) * Vector3.forward;
            if (!Physics.Raycast(eye, aim, out RaycastHit hit, crowbarReach, mask, QueryTriggerInteraction.Ignore)) return;
            StructuralSection section = hit.collider.GetComponentInParent<StructuralSection>();
            if (section == null || !section.CanCollapse) return;
            section.ApplyImpact(crowbarMomentum);
            NoiseSystem.Emit(hit.point, 0.7f, NoiseSource.Other);
            SoundRpc(Audio.SoundId.CrowbarHit, hit.point);
        }

        // The reviver's own machine counts it (achievements are personal).
        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Server)]
        private void RevivedSomeoneRpc() => Achievements.Increment(Achievements.StatRevives);

        // Server only: these sounds report what the host decided; a client must not play them on every machine.
        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void SoundRpc(Audio.SoundId id, Vector3 at) => Audio.GameAudio.Play(id, at, 1f);

        // Host: the nearest downed crewmate within reach and in sight gets up.
        private bool TreatInjury()
        {
            NetworkPlayer best = null;
            float bestDistance = reviveReach;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.NetworkManager != NetworkManager || !p.IsDead) continue;
                Vector3 body = p.Ragdoll.BodyPosition;
                float d = Vector3.Distance(body, transform.position);
                if (d > bestDistance || Physics.Linecast(Eye, body + Vector3.up * 0.3f, wallMask, QueryTriggerInteraction.Ignore)) continue;
                (best, bestDistance) = (p, d);
            }
            if (best != null)
            {
                best.ServerRevive();
                RevivedSomeoneRpc();
                return true;
            }
            // Aim at an injured teammate to help them; an empty sightline treats yourself.
            bestDistance = reviveReach;
            Vector3 aim = Quaternion.Euler(player.State.Pitch, transform.eulerAngles.y, 0f) * Vector3.forward;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p == player || p.NetworkManager != NetworkManager || p.IsDead || p.Health >= p.MaxHealth) continue;
                Vector3 at = p.Ragdoll.IsRagdolled ? p.Ragdoll.BodyPosition : p.transform.position + Vector3.up;
                Vector3 offset = at - Eye;
                float distance = offset.magnitude;
                if (distance > bestDistance || Vector3.Dot(offset.normalized, aim) < 0.7f ||
                    Physics.Linecast(Eye, at, wallMask, QueryTriggerInteraction.Ignore)) continue;
                (best, bestDistance) = (p, distance);
            }
            if (best != null) return best.ServerHeal(best.HealthConfig.MedkitHeal);
            if (player.ServerHeal(player.HealthConfig.MedkitHeal)) return true;
            HintRpc(Loc.Mark("No injury to treat. Aim at an injured crewmate, or use the medkit when hurt."));
            return false;
        }

        private bool Throw()
        {
            Transform t = transform;
            // Facing a wall, start just short of it rather than past it in the next room.
            Vector3 at = Eye + t.forward * 0.6f;
            if (Physics.Linecast(Eye, at + t.forward * 0.15f, out RaycastHit hit, wallMask, QueryTriggerInteraction.Ignore))
                at = Eye + t.forward * Mathf.Max(0f, hit.distance - 0.2f);
            NetworkObject device = Spawn(noiseMakerPrefab, at, t.rotation);
            if (device == null) return false;
            device.GetComponent<NoiseMakerDevice>().Launch(t.forward * throwSpeed.x + Vector3.up * throwSpeed.y);
            return true;
        }

        [Rpc(SendTo.Owner)]
        private void HintRpc(string message, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;
            if (carrier != null) carrier.ShowHint(message);
        }

        private NetworkObject Spawn(NetworkObject prefab, Vector3 at, Quaternion rotation) =>
            prefab == null ? null : NetworkManager.SpawnManager.InstantiateAndSpawn(prefab, position: at, rotation: rotation);
    }
}
