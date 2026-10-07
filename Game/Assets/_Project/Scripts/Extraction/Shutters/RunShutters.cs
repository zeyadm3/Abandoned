using System.Collections.Generic;
using System.Linq;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Which shutters are down this run (M10.4), on the run state's object: the host rolls a few stores
    /// locked from the run seed (all of them, and the front entrance, on a Sealed job), makes the loot
    /// locked inside worth more, and opens a shutter for a player at it with bolt cutters (a quiet cut
    /// that takes a moment) or a crowbar (a few loud strikes). Everyone sees the mask; shutters animate locally.
    /// </summary>
    public class RunShutters : NetworkBehaviour
    {
        [SerializeField, Range(0, 8)] private int lockedMin = 2, lockedMax = 3;
        [Tooltip("Loot inside a store that starts the run locked is worth this much more.")]
        [SerializeField, Range(1f, 3f)] private float lockedValueBonus = 1.5f;
        [SerializeField, Min(0.2f)] private float cutSeconds = 1.6f;
        [SerializeField, Range(1, 10)] private int pryStrikes = 4;
        [SerializeField, Min(0.1f)] private float pryCooldown = 0.6f;
        [SerializeField, Min(1f)] private float reach = 3f;

        // -1 until the host has rolled this run's locks (shutters hold still until then).
        private readonly NetworkVariable<int> down = new(-1);
        private readonly Dictionary<int, float> cuts = new();
        private readonly Dictionary<int, int> pries = new();
        private readonly Dictionary<ulong, float> nextStrike = new();

        public static RunShutters Current { get; private set; }

        public bool Ready => down.Value != -1;
        public bool IsDown(int index) => Ready && index >= 0 && index < 32 && (down.Value & (1 << index)) != 0;
        public int DownMask => down.Value;

        private bool rolled;

        public override void OnNetworkSpawn() => Current = this;

        // Host, once the run state has its seed and terms (its own spawn may run after ours).
        private void RollOnce()
        {
            rolled = true;
            RunState run = GetComponent<RunState>();
            int mask = Roll(run != null ? run.State.Seed : 0, run != null && run.Terms.Sealed);
            down.Value = mask;
            BoostLockedLoot(mask);
        }

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
        }

        private int Roll(int seed, bool sealedJob)
        {
            List<RollerShutter> stores = RollerShutter.All.Where(s => s != null && !s.Entrance).OrderBy(s => s.Index).ToList();
            int mask = 0;
            if (sealedJob)
            {
                foreach (RollerShutter s in RollerShutter.All) if (s != null) mask |= 1 << s.Index;
                return mask;
            }
            var random = new System.Random(seed ^ 0x5b17e5);
            int count = Mathf.Min(stores.Count, random.Next(lockedMin, lockedMax + 1));
            for (int i = 0; i < count; i++)
            {
                int pick = random.Next(stores.Count);
                mask |= 1 << stores[pick].Index;
                stores.RemoveAt(pick);
            }
            return mask;
        }

        // Host: stock behind a locked shutter is the good stuff.
        private void BoostLockedLoot(int mask)
        {
            if (mask == 0) return;
            foreach (LootItem item in FindObjectsByType<LootItem>(FindObjectsSortMode.None))
            {
                if (item == null || item.Definition == null || item.Definition.Jackpot) continue;
                foreach (RollerShutter s in RollerShutter.All)
                    if (s != null && !s.Entrance && (mask & (1 << s.Index)) != 0 && s.Room.Contains(item.transform.position))
                    {
                        item.ScaleValue(lockedValueBonus);
                        break;
                    }
            }
        }

        /// <summary>This machine's player pressed E on a locked shutter.</summary>
        public void RequestOpen(int index) => RequestOpenRpc(index);

        [Rpc(SendTo.Server)]
        private void RequestOpenRpc(int index, RpcParams rpcParams = default)
        {
            ulong client = rpcParams.Receive.SenderClientId;
            RollerShutter shutter = RollerShutter.All.FirstOrDefault(s => s != null && s.Index == index);
            if (shutter == null || !IsDown(index) || cuts.ContainsKey(index)) return;
            if (!rolled) return;
            NetworkObject player = NetworkManager.ConnectedClients.TryGetValue(client, out NetworkClient c) ? c.PlayerObject : null;
            if (player == null || !player.TryGetComponent(out NetworkPlayer np) || np.IsDead || np.Ragdoll.IsRagdolled) return;
            if (Vector3.Distance(player.transform.position, shutter.transform.position) > reach) return;
            PlayerEquipment gear = PlayerEquipment.Of(NetworkManager, client);
            if (gear == null) return;
            Vector3 at = shutter.transform.position + Vector3.up * 1.2f;
            if (gear.Has(EquipmentKind.BoltCutters))
            {
                cuts[index] = Time.time + cutSeconds;
                NoiseSystem.Emit(at, 0.15f, NoiseSource.Other);
                SoundRpc(SoundId.BoltCut, at);
                return;
            }
            if (!gear.Has(EquipmentKind.Crowbar)) return;
            if (nextStrike.TryGetValue(client, out float next) && Time.time < next) return;
            nextStrike[client] = Time.time + pryCooldown;
            NoiseSystem.Emit(at, 0.75f, NoiseSource.Other);
            SoundRpc(SoundId.CrowbarHit, at);
            pries[index] = (pries.TryGetValue(index, out int n) ? n : 0) + 1;
            if (pries[index] >= pryStrikes) Open(index, at);
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned) return;
            if (!rolled) RollOnce();
            if (cuts.Count == 0) return;
            foreach (int index in cuts.Keys.ToList())
            {
                if (Time.time < cuts[index]) continue;
                cuts.Remove(index);
                RollerShutter shutter = RollerShutter.All.FirstOrDefault(s => s != null && s.Index == index);
                Open(index, shutter != null ? shutter.transform.position + Vector3.up * 1.2f : transform.position);
            }
        }

        private void Open(int index, Vector3 at)
        {
            pries.Remove(index);
            down.Value &= ~(1 << index);
            // Rolling a shutter up is never quiet.
            NoiseSystem.Emit(at, 0.45f, NoiseSource.Other);
            SoundRpc(SoundId.ShutterOpen, at);
            Debug.Log($"[Run] Shutter {index} opened.");
        }

        [Rpc(SendTo.Everyone)]
        private void SoundRpc(SoundId id, Vector3 at) => GameAudio.Play(id, at, 1f);

        private void OnGUI()
        {
            if (!DebugView.Visible || !IsSpawned) return;
            GUI.Label(new Rect(Screen.width - 360f, 74f, 350f, 22f), $"SHUTTERS down {System.Convert.ToString(down.Value, 2)} ({RollerShutter.All.Count} in level)");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}
