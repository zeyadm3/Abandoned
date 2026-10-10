using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// What the dead can do (0.19.2), on the run state's object: knock (a hollow rap everyone hears), make the
    /// lights near a spot stutter, or nudge a small loose object. Only near the living, a few seconds apart;
    /// none of it makes noise monsters hear. Ghosts still aren't heard by the living otherwise.
    /// </summary>
    public class RunGhosts : NetworkBehaviour
    {
        public enum Act : byte { Knock, Flicker, Nudge }

        private const float Cooldown = 3.5f, NearLiving = 30f;
        private readonly Dictionary<ulong, float> next = new();

        public static RunGhosts Current { get; private set; }

        public override void OnNetworkSpawn() => Current = this;

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
        }

        public void Request(Act act, Vector3 at, Vector3 direction) => ActRpc(act, at, direction);

        [Rpc(SendTo.Server)]
        private void ActRpc(Act act, Vector3 at, Vector3 direction, RpcParams rpcParams = default)
        {
            ulong client = rpcParams.Receive.SenderClientId;
            NetworkObject sender = NetworkManager.ConnectedClients.TryGetValue(client, out NetworkClient c) ? c.PlayerObject : null;
            if (sender == null || !sender.TryGetComponent(out NetworkPlayer ghost) || !ghost.IsDead) return;
            if (next.TryGetValue(client, out float ready) && Time.time < ready) return;
            bool near = false;
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && !p.IsDead && Vector3.Distance(p.transform.position, at) < NearLiving) near = true;
            if (!near) return;
            next[client] = Time.time + Cooldown;
            if (act == Act.Nudge) Nudge(at, direction);
            ShowRpc(act, at);
        }

        // Host: the nearest small loose item gets a shove (host-simulated resting loot only).
        private static void Nudge(Vector3 at, Vector3 direction)
        {
            LootItem best = null;
            float bestDistance = 1.6f;
            foreach (Collider hit in Physics.OverlapSphere(at, 1.6f, LayerMask.GetMask(GameLayers.Loot), QueryTriggerInteraction.Ignore))
            {
                LootItem item = hit.GetComponentInParent<LootItem>();
                if (item == null || item.CarryClass > CarryClass.OneHand || !item.TryGetComponent(out Grabbable g) || g.Holder != null || g.IsPocketed) continue;
                float d = Vector3.Distance(item.transform.position, at);
                if (d < bestDistance) (best, bestDistance) = (item, d);
            }
            if (best == null || !best.TryGetComponent(out Rigidbody body) || body.isKinematic) return;
            Vector3 push = Vector3.ProjectOnPlane(direction, Vector3.up).normalized * 2.2f + Vector3.up * 1.2f;
            body.AddForce(push, ForceMode.VelocityChange);
        }

        [Rpc(SendTo.Everyone)]
        private void ShowRpc(Act act, Vector3 at)
        {
            switch (act)
            {
                case Act.Knock: GameAudio.Play(SoundId.GhostKnock, at, 1f); break;
                case Act.Flicker:
                    foreach (LightFixture f in LightFixture.All)
                        if (f != null && Vector3.Distance(f.transform.position, at) < 8f) f.DisturbHere(1.6f);
                    GameAudio.Play(SoundId.LightBuzz, at + Vector3.up * 2f, 1f);
                    break;
                default: GameAudio.Play(SoundId.GhostWhoosh, at, 1f); break;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}
