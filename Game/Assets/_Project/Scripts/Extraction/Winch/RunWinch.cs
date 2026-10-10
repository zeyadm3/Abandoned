using Abandoned.Audio;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The winch's pull (0.20.3), on the run state's object: which item is hooked (everyone draws the cable to
    /// it) and, on the host, the reeling: to the ramp's foot, then up into the bay, at a slow walk.
    /// </summary>
    public class RunWinch : NetworkBehaviour
    {
        private const float Speed = 1.3f, Reach = 30f, StuckSeconds = 5f;

        private readonly NetworkVariable<ulong> target = new();
        private Rigidbody pulled;
        private bool onRamp;
        private float lastProgressAt, bestDistance, nextNoise;

        public static RunWinch Current { get; private set; }

        public Transform Target =>
            target.Value != 0 && NetworkManager != null && NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(target.Value, out NetworkObject o) ? o.transform : null;

        public override void OnNetworkSpawn() => Current = this;

        public override void OnNetworkDespawn()
        {
            Release();
            if (Current == this) Current = null;
        }

        public void RequestToggle() => ToggleRpc();

        [Rpc(SendTo.Server)]
        private void ToggleRpc(RpcParams rpcParams = default)
        {
            TruckWinch winch = TruckWinch.Active;
            NetworkObject player = NetworkManager.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out NetworkClient c) ? c.PlayerObject : null;
            if (winch == null || player == null || CompanyService.Current == null || !CompanyService.Current.HasUpgrade(TruckUpgradeKind.Winch)) return;
            if (Vector3.Distance(player.transform.position, winch.Drum) > 6f) return;
            if (target.Value != 0)
            {
                Release();
                return;
            }
            LootItem best = null;
            float bestRange = Reach;
            foreach (LootItem item in FindObjectsByType<LootItem>(FindObjectsSortMode.None))
            {
                if (item == null || item.IsShattered || item.CarryClass < CarryClass.Heavy || TruckCargo.Current != null && TruckCargo.Current.Carries(item.transform.position)) continue;
                if (!item.TryGetComponent(out Grabbable g) || g.Holder != null || g.IsLifted || g.IsDragged) continue;
                float d = Vector3.Distance(item.transform.position, winch.RampFoot);
                if (d < bestRange) (best, bestRange) = (item, d);
            }
            if (best == null || !best.TryGetComponent(out NetworkObject no) || !best.TryGetComponent(out pulled)) return;
            target.Value = no.NetworkObjectId;
            pulled.useGravity = false;
            onRamp = false;
            bestDistance = float.MaxValue;
            lastProgressAt = Time.time;
            SoundRpc(SoundId.WinchStart);
        }

        private void FixedUpdate()
        {
            if (!IsServer || target.Value == 0) return;
            TruckWinch winch = TruckWinch.Active;
            Transform t = Target;
            if (winch == null || t == null || pulled == null || pulled.isKinematic || (t.TryGetComponent(out Grabbable g) && (g.Holder != null || g.IsLifted)))
            {
                Release();
                return;
            }
            Vector3 at = pulled.worldCenterOfMass;
            if (!onRamp && Vector3.Distance(Flat(at), Flat(winch.RampFoot)) < 1f) onRamp = true;
            Vector3 waypoint = onRamp ? winch.Bay : winch.RampFoot + Vector3.up * (at.y - winch.RampFoot.y);
            Vector3 to = waypoint - at;
            pulled.linearVelocity = Vector3.MoveTowards(pulled.linearVelocity, to.normalized * Speed, 6f * Time.fixedDeltaTime);
            pulled.angularVelocity *= 0.9f;
            float d = to.magnitude;
            if (d < bestDistance - 0.1f)
            {
                bestDistance = d;
                lastProgressAt = Time.time;
            }
            if (TruckCargo.Current != null && TruckCargo.Current.Carries(at) || Time.time - lastProgressAt > StuckSeconds) Release();
            else if (Time.time >= nextNoise)
            {
                nextNoise = Time.time + 1f;
                NoiseSystem.Emit(winch.Drum, 0.5f, NoiseSource.Other);
                SoundRpc(SoundId.WinchRun);
            }
        }

        private static Vector3 Flat(Vector3 v) => new(v.x, 0f, v.z);

        private void Release()
        {
            if (pulled != null) pulled.useGravity = true;
            pulled = null;
            if (IsServer && IsSpawned && target.Value != 0) target.Value = 0;
        }

        [Rpc(SendTo.Everyone)]
        private void SoundRpc(SoundId id)
        {
            if (TruckWinch.Active != null) GameAudio.Play(id, TruckWinch.Active.Drum, 1f);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}
