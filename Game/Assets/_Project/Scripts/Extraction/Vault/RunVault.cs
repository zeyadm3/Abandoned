using System.Linq;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The vault drill (0.16.6), on the run state's object. Host: someone sets the drill on the safe; it
    /// runs for a minute and a half, loud enough to bring everything, and now and then it jams until someone
    /// frees it. When it is through, the door swings open on a few small, valuable pieces.
    /// </summary>
    public class RunVault : NetworkBehaviour
    {
        private readonly NetworkVariable<VaultPhase> phase = new();
        // Seconds drilled so far, written a few times a second (the bar is a guide, not a clock).
        private readonly NetworkVariable<float> progress = new();
        private float nextNoise, nextJamCheck, nextWrite, drilled;
        private System.Random random;

        public static RunVault Current { get; private set; }
        public VaultPhase Phase => phase.Value;
        public float Progress01 => Mathf.Clamp01(progress.Value / VaultConfig.Current.DrillSeconds);

        public override void OnNetworkSpawn()
        {
            Current = this;
            if (IsServer)
            {
                RunState run = GetComponent<RunState>();
                random = new System.Random((run != null ? run.State.Seed : 0) ^ 0x0a0166);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
        }

        public void RequestUse() => UseRpc();

        [Rpc(SendTo.Server)]
        private void UseRpc(RpcParams rpcParams = default)
        {
            VaultSafe safe = VaultSafe.Active;
            NetworkObject player = NetworkManager.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out NetworkClient c) ? c.PlayerObject : null;
            if (safe == null || player == null || !player.TryGetComponent(out NetworkPlayer np) || np.IsDead) return;
            if (Vector3.Distance(player.transform.position, safe.Front) > VaultConfig.Current.Reach + 1f) return;
            if (phase.Value != VaultPhase.Locked && phase.Value != VaultPhase.Jammed) return;
            phase.Value = VaultPhase.Drilling;
            nextJamCheck = Time.time + VaultConfig.Current.JamCheckSeconds;
            SoundRpc(SoundId.VaultDrillStart);
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned || phase.Value != VaultPhase.Drilling) return;
            VaultSafe safe = VaultSafe.Active;
            if (safe == null) return;
            VaultConfig config = VaultConfig.Current;
            drilled += Time.deltaTime;
            if (Time.time >= nextNoise)
            {
                nextNoise = Time.time + config.NoiseInterval;
                NoiseSystem.Emit(safe.Front, config.DrillNoise, NoiseSource.Other);
            }
            if (Time.time >= nextWrite || drilled >= config.DrillSeconds)
            {
                nextWrite = Time.time + 0.4f;
                progress.Value = drilled;
            }
            if (drilled >= config.DrillSeconds)
            {
                Open(safe);
                return;
            }
            if (Time.time >= nextJamCheck)
            {
                nextJamCheck = Time.time + config.JamCheckSeconds;
                if (random.NextDouble() < config.JamChance)
                {
                    phase.Value = VaultPhase.Jammed;
                    SoundRpc(SoundId.VaultDrillJam);
                }
            }
        }

        private void Open(VaultSafe safe)
        {
            phase.Value = VaultPhase.Open;
            SoundRpc(SoundId.VaultOpen);
            NetworkLootSpawner spawner = NetworkLootSpawner.Current;
            if (spawner == null || spawner.Catalog == null) return;
            VaultConfig config = VaultConfig.Current;
            // Small and dear: the most valuable one-hand pieces in the catalogue.
            var picks = spawner.Catalog.Entries.Select(e => e.definition)
                .Where(d => d != null && !d.Jackpot && d.CarryClass <= CarryClass.OneHand)
                .OrderByDescending(d => d.ValueMax).Take(8).ToList();
            if (picks.Count == 0) return;
            // A mis-set range (max under min) must not throw as the door opens.
            int least = Mathf.Max(0, config.Contents.x);
            int count = random.Next(least, Mathf.Max(least, config.Contents.y) + 1);
            for (int i = 0; i < count; i++)
            {
                LootDefinition d = picks[random.Next(picks.Count)];
                Vector3 at = safe.Inside + new Vector3((i % 3 - 1) * 0.28f, 0.05f + d.Size.y * 0.5f + (i / 3) * 0.35f, 0f);
                LootItem item = spawner.SpawnExtra(d, at, safe.transform.rotation, random.Next());
                if (item != null) item.ScaleValue(config.ValueScale);
            }
            Debug.Log($"[Run] The vault is open: {count} valuables inside.");
        }

        [Rpc(SendTo.Everyone)]
        private void SoundRpc(SoundId id)
        {
            VaultSafe safe = VaultSafe.Active;
            if (safe != null) GameAudio.Play(id, safe.Front + Vector3.up, 1f);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}
