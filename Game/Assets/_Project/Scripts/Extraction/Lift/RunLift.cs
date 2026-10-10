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
    /// The freight lift's state (0.16.3), on the run state's object: the host starts trips, checks power and
    /// load, and makes an overloaded lift give way halfway. Every machine places the platform from this
    /// state and the server clock (<see cref="FreightLift"/>), so it is in the same place everywhere.
    /// </summary>
    public class RunLift : NetworkBehaviour
    {
        private readonly NetworkVariable<LiftNetState> state = new();
        private RunState run;
        private float nextNoise;
        private bool arrivedShown = true;

        public static RunLift Current { get; private set; }
        public LiftNetState State => state.Value;
        public double Now => run != null ? run.Now : 0d;

        public override void OnNetworkSpawn()
        {
            Current = this;
            run = GetComponent<RunState>();
        }

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
        }

        /// <summary>0 at the bottom, 1 at the top, as of now (every machine).</summary>
        public float Height(out bool moving)
        {
            LiftNetState s = state.Value;
            LiftConfig config = LiftConfig.Current;
            moving = false;
            if (s.Start <= 0d) return s.From;
            float t = Mathf.Clamp01((float)((Now - s.Start) / config.TravelSeconds));
            if (s.SnapTime > 0d && Now >= s.SnapTime)
            {
                // Given way: from where it snapped, it falls freely to the bottom.
                float at = Mathf.Lerp(s.From, s.To, Mathf.Clamp01((float)((s.SnapTime - s.Start) / config.TravelSeconds)));
                float fallen = 0.5f * 9.81f * Mathf.Pow((float)(Now - s.SnapTime), 2f) / Mathf.Max(0.1f, FreightLift.Active != null ? FreightLift.Active.Rise : 4f);
                moving = at - fallen > 0f;
                return Mathf.Max(0f, at - fallen);
            }
            moving = t < 1f;
            return Mathf.Lerp(s.From, s.To, Mathf.SmoothStep(0f, 1f, t));
        }

        public bool Moving { get { Height(out bool m); return m; } }

        /// <summary>Is the lift's motor fed: the mains on in its sector, or its sector's breaker thrown.</summary>
        public bool Powered(FreightLift lift)
        {
            if (run == null || lift == null) return false;
            if (RunBreakers.Current != null && RunBreakers.Current.Restored(lift.Sector)) return true;
            if (run.State.PowerOff) return false;
            RunHorrorDirector horror = RunHorrorDirector.Current;
            return horror == null || horror.Config == null || horror.Config.SectorPowered(lift.Sector, run.State.Danger);
        }

        /// <summary>Host's view: gameplay weight on the deck now (players with what they hold, plus loot lying on it).</summary>
        public static float LoadOn(FreightLift lift)
        {
            float kg = 0f;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.IsDead || !lift.OnDeck(p.transform.position)) continue;
                kg += p.TryGetComponent(out CarrierLoad load) && load.LoadWeight > 0f ? load.LoadWeight : 80f;
            }
            // Held loot is already in its carrier's weight; LoadWeight is what rests on the deck by itself.
            foreach (LootItem item in FindObjectsByType<LootItem>(FindObjectsSortMode.None))
                if (item != null && lift.OnDeck(item.transform.position)) kg += item.LoadWeight;
            return kg;
        }

        public void RequestSend(int floor) => SendRpc(floor);

        [Rpc(SendTo.Server)]
        private void SendRpc(int floor, RpcParams rpcParams = default)
        {
            FreightLift lift = FreightLift.Active;
            NetworkObject player = NetworkManager.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out NetworkClient c) ? c.PlayerObject : null;
            if (lift == null || player == null || !player.TryGetComponent(out NetworkPlayer np) || np.IsDead) return;
            if (lift.ButtonDistance(player.transform.position) > LiftConfig.Current.Reach + 1f) return;
            LiftNetState s = state.Value;
            floor = Mathf.Clamp(floor, 0, 1);
            if (s.Broken || Moving || !Powered(lift)) return;
            byte here = s.Start > 0d ? s.To : s.From;
            if (here == floor) return;
            float load = LoadOn(lift);
            LiftConfig config = LiftConfig.Current;
            double now = run.Now;
            state.Value = new LiftNetState
            {
                From = here,
                To = (byte)floor,
                Start = now,
                SnapTime = load > config.LoadLimit ? now + config.TravelSeconds * config.SnapAt : 0d,
            };
            arrivedShown = false;
            NoiseSystem.Emit(lift.Deck, 0.8f, NoiseSource.Other);
            SoundRpc(SoundId.LiftStart);
            Debug.Log($"[Run] Lift sent to floor {floor} carrying {load:0} kg{(load > config.LoadLimit ? " (OVERLOADED)" : "")}.");
        }

        private void Update()
        {
            if (!IsSpawned || !IsServer) return;
            FreightLift lift = FreightLift.Active;
            LiftNetState s = state.Value;
            if (lift == null || s.Start <= 0d) return;
            float h = Height(out bool moving);
            if (moving && Time.time >= nextNoise)
            {
                nextNoise = Time.time + LiftConfig.Current.RunNoiseInterval;
                NoiseSystem.Emit(lift.Deck, LiftConfig.Current.RunNoise, NoiseSource.Other);
            }
            if (s.SnapTime > 0d && !s.Broken && run.Now >= s.SnapTime)
            {
                s.Broken = true;
                state.Value = s;
                SoundRpc(SoundId.LiftSnap);
            }
            if (moving || arrivedShown) return;
            arrivedShown = true;
            if (s.Broken)
            {
                // Hit the bottom: everyone riding it is hurt, and the whole building hears it.
                foreach (NetworkPlayer p in NetworkPlayer.All)
                    if (p != null && !p.IsDead && lift.OnDeck(p.transform.position)) p.DealDamage(LiftConfig.Current.DropDamage, "fell with the freight lift");
                NoiseSystem.Emit(lift.Deck, 1f, NoiseSource.Other);
                SoundRpc(SoundId.Crash);
                return;
            }
            // Parked: the trip is over.
            state.Value = new LiftNetState { From = s.To, To = s.To };
            SoundRpc(SoundId.LiftArrive);
        }

        [Rpc(SendTo.Everyone)]
        private void SoundRpc(SoundId id)
        {
            FreightLift lift = FreightLift.Active;
            if (lift == null) return;
            GameAudio.Play(id, lift.Deck + Vector3.up, 1f);
            if (id == SoundId.LiftSnap || id == SoundId.Crash) CameraShake.Emit(lift.Deck, id == SoundId.Crash ? 2600f : 1400f);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}
