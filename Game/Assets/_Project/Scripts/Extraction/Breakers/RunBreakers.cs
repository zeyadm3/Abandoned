using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The run's breakers (0.16.1), on the run state's object: when each power sector's breaker was thrown.
    /// A thrown breaker brings its sector's lights back for a few minutes, through a power cut, the danger's
    /// blackouts or a power-off contract, and the panel hums loudly all the while. Host-written; every
    /// machine's lights read <see cref="Restored"/>.
    /// </summary>
    public class RunBreakers : NetworkBehaviour
    {
        public const int MaxSectors = 8;

        private readonly NetworkList<double> until = new();
        private float nextHum;

        public static RunBreakers Current { get; private set; }

        /// <summary>Which sectors are lit by their breaker right now (bit per sector); changes are what lights watch.</summary>
        public int RestoredMask { get; private set; }

        private RunState run;

        public override void OnNetworkSpawn()
        {
            Current = this;
            run = GetComponent<RunState>();
            if (IsServer && until.Count == 0)
                for (int i = 0; i < MaxSectors; i++) until.Add(0d);
        }

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
        }

        public bool Restored(int sector) => (RestoredMask & (1 << sector)) != 0;

        /// <summary>Seconds left on a sector's breaker (0 when off).</summary>
        public float Remaining(int sector) =>
            sector < 0 || sector >= until.Count || run == null ? 0f : Mathf.Max(0f, (float)(until[sector] - run.Now));

        /// <summary>Seconds before the panel can be thrown again (0 when ready).</summary>
        public float Cooldown(int sector)
        {
            if (sector < 0 || sector >= until.Count || run == null) return 0f;
            double ready = until[sector] + BreakerConfig.Current.CooldownSeconds;
            return until[sector] <= 0d ? 0f : Mathf.Max(0f, (float)(ready - run.Now));
        }

        private void Update()
        {
            if (!IsSpawned || run == null) return;
            int mask = 0;
            for (int i = 0; i < until.Count && i < MaxSectors; i++)
                if (until[i] > run.Now) mask |= 1 << i;
            RestoredMask = mask;
            if (!IsServer || mask == 0 || Time.time < nextHum) return;
            BreakerConfig config = BreakerConfig.Current;
            nextHum = Time.time + config.HumInterval;
            foreach (BreakerPanel panel in BreakerPanel.All)
                if (panel != null && Restored(panel.Sector)) NoiseSystem.Emit(panel.transform.position, config.HumNoise, NoiseSource.Other);
        }

        public void RequestThrow(int sector) => ThrowRpc(sector);

        [Rpc(SendTo.Server)]
        private void ThrowRpc(int sector, RpcParams rpcParams = default)
        {
            if (sector < 0 || sector >= until.Count) return;
            BreakerPanel panel = BreakerPanel.ForSector(sector);
            NetworkObject player = NetworkManager.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out NetworkClient c) ? c.PlayerObject : null;
            if (panel == null || player == null || !player.TryGetComponent(out NetworkPlayer np) || np.IsDead || np.IsHiding) return;
            BreakerConfig config = BreakerConfig.Current;
            if (Vector3.Distance(player.transform.position, panel.transform.position) > config.Reach + 1f) return;
            if (Remaining(sector) > 0f || Cooldown(sector) > 0f) return;
            until[sector] = run.Now + config.RestoreSeconds;
            NoiseSystem.Emit(panel.transform.position, config.ThrowNoise, NoiseSource.Other);
            ThrownRpc(sector);
            Debug.Log($"[Run] Breaker {sector} thrown by player {rpcParams.Receive.SenderClientId}.");
        }

        [Rpc(SendTo.Everyone)]
        private void ThrownRpc(int sector)
        {
            BreakerPanel panel = BreakerPanel.ForSector(sector);
            Vector3 at = panel != null ? panel.transform.position : Vector3.zero;
            GameAudio.Play(SoundId.BreakerThrow, at, 1f);
            GameAudio.PlayUi(SoundId.PowerUp, 0.6f);
            CameraShake.Emit(at, 600f);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible || !IsSpawned) return;
            GUI.Label(new Rect(Screen.width - 360f, 118f, 350f, 22f),
                $"BREAKERS lit {System.Convert.ToString(RestoredMask, 2)} ({BreakerPanel.All.Count} panels)");
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}
