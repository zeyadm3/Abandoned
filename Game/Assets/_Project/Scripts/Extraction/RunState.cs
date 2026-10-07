using System;
using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The run as everyone sees it (host-authoritative, GDD 10): haul vs quota from the truck's cargo,
    /// the extraction window, and the departure. Anyone may start the truck (the host checks they're at
    /// the ignition and it isn't overloaded); it honks, then leaves with whoever is in it. The host then
    /// sends everyone the results for the appraisal. One per level session, host-spawned by RunDirector.
    /// </summary>
    public class RunState : NetworkBehaviour
    {
        private const float TallyInterval = 0.25f;

        [SerializeField] private ExtractionConfig config;

        private readonly NetworkVariable<RunNetState> state = new();
        private float nextTally, nextHorn, startedAt;

        public static RunState Current { get; private set; }

        public ExtractionConfig Config => config;
        public RunNetState State => state.Value;
        public RunResults Results { get; private set; }
        public double Now => NetworkManager != null ? NetworkManager.ServerTime.Time : 0d;
        public float WindowRemaining => Mathf.Max(0f, (float)(State.WindowEnd - Now));
        public bool WindowClosed => IsSpawned && Now >= State.WindowEnd;
        public float HonkRemaining => State.Phase == RunPhase.Honking ? Mathf.Max(0f, (float)(State.HonkEnd - Now)) : 0f;

        /// <summary>Every machine: the truck left and the results arrived.</summary>
        public event Action<RunResults> Departed;

        /// <summary>Host, before anything else in the run: which run this is (the loot spawner's seed).</summary>
        public static Func<int> SeedSource { get; set; }

        /// <summary>Host: the contract's terms for the run (quota, window, bonus, power); null = the config's defaults.</summary>
        public static Func<RunTerms?> TermsSource { get; set; }

        /// <summary>Host: this run's terms (the modifier's host-only effects live here).</summary>
        public RunTerms Terms { get; private set; } = new(0, 0f, 0f, false);

        public readonly struct RunTerms
        {
            public readonly int Quota;
            public readonly float Window, Bonus;
            public readonly bool PowerOff, Night, Storm;
            /// <summary>Host-only modifier effects: extra opening threats, threat hearing, floor decay.</summary>
            public readonly int ExtraThreats;
            public readonly float Hearing, Decay;

            public RunTerms(int quota, float window, float bonus, bool powerOff, bool night = false, bool storm = false,
                int extraThreats = 0, float hearing = 1f, float decay = 1f)
            {
                Quota = quota;
                Window = window;
                Bonus = bonus;
                PowerOff = powerOff;
                Night = night;
                Storm = storm;
                ExtraThreats = extraThreats;
                Hearing = hearing;
                Decay = decay;
            }
        }

        public override void OnNetworkSpawn()
        {
            Current = this;
            if (!IsServer) return;
            startedAt = Time.time;
            RunTerms terms = TermsSource?.Invoke() ?? new RunTerms(config.Quota, config.WindowSeconds, 0f, false);
            Terms = terms;
            state.Value = new RunNetState
            {
                Phase = RunPhase.Running,
                Quota = terms.Quota,
                Seed = SeedSource?.Invoke() ?? 0,
                CargoCapacity = config.CargoCapacity,
                Window = terms.Window,
                Bonus = terms.Bonus,
                PowerOff = terms.PowerOff,
                Night = terms.Night,
                Storm = terms.Storm,
                WindowEnd = Now + terms.Window,
            };
        }

        /// <summary>Host: change this run's terms mid-run (tests, and trying modifiers in dev).</summary>
        public void ServerSetTerms(RunTerms terms)
        {
            if (!IsServer) return;
            Terms = terms;
            RunNetState s = state.Value;
            s.PowerOff = terms.PowerOff;
            s.Night = terms.Night;
            s.Storm = terms.Storm;
            state.Value = s;
        }

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
        }

        private void Update()
        {
            if (!IsSpawned) return;
            // Every machine: the lever clunks when someone starts the truck.
            if (State.Phase == RunPhase.Honking && shownPhase != RunPhase.Honking && TruckCargo.Current != null)
                GameAudio.Play(SoundId.Lever, TruckCargo.Current.Ignition.position, 1f);
            shownPhase = State.Phase;
            if (State.Phase == RunPhase.Honking) Honk();
            if (!IsServer) return;
            if (State.Phase == RunPhase.Running && Time.time >= nextTally) Tally();
            // Nobody left to drive: the run is over (the truck "leaves" with whatever is already in it).
            if (State.Phase == RunPhase.Running && EveryoneDead()) Depart();
            if (State.Phase == RunPhase.Honking && Now >= State.HonkEnd) Depart();
        }

        /// <summary>Host: the danger director's verdict for this moment of the run.</summary>
        public void SetDanger(int level)
        {
            if (!IsServer || State.Danger == level) return;
            RunNetState s = State;
            s.Danger = (byte)Mathf.Clamp(level, 0, 255);
            state.Value = s;
        }

        public float Elapsed => Time.time - startedAt;

        // ---- Starting the truck ----

        /// <summary>This machine's player pressed the ignition.</summary>
        public void RequestDepart()
        {
            if (IsServer) TryDepart(NetworkManager.LocalClientId);
            else RequestDepartRpc();
        }

        [Rpc(SendTo.Server)]
        private void RequestDepartRpc(RpcParams rpcParams = default) => TryDepart(rpcParams.Receive.SenderClientId);

        private void TryDepart(ulong client)
        {
            TruckCargo truck = TruckCargo.Current;
            NetworkObject player = NetworkManager.ConnectedClients.TryGetValue(client, out NetworkClient c) ? c.PlayerObject : null;
            if (State.Phase != RunPhase.Running || truck == null || player == null) return;
            if (player.TryGetComponent(out NetworkPlayer np) && np.IsDead) return;
            if (Vector3.Distance(player.transform.position, truck.Ignition.position) > config.IgnitionRange) return;
            Tally();
            if (State.Overloaded) return;
            RunNetState s = State;
            s.Phase = RunPhase.Honking;
            s.HonkEnd = Now + config.HonkSeconds;
            state.Value = s;
            Debug.Log($"[Run] Player {client} started the truck; leaving in {config.HonkSeconds:0} s.");
        }

        private RunPhase shownPhase;

        private void Honk()
        {
            if (Time.time < nextHorn || TruckCargo.Current == null) return;
            nextHorn = Time.time + config.HornInterval;
            GameAudio.Play(SoundId.Horn, TruckCargo.Current.Ignition.position, 1f);
        }

        // ---- Host ----

        private void Tally()
        {
            nextTally = Time.time + TallyInterval;
            TruckCargo truck = TruckCargo.Current;
            if (truck == null) return;
            int haul = 0;
            float volume = 0f;
            foreach (LootItem item in truck.ItemsInside())
            {
                haul += item.CurrentValue;
                volume += TruckCargo.VolumeOf(item);
            }
            RunNetState s = State;
            s.Haul = haul;
            s.CargoVolume = Mathf.Round(volume * 1000f) / 1000f; // litres: a laptop is 2.6 l
            if (!s.Equals(State)) state.Value = s;
        }

        private bool EveryoneDead()
        {
            bool any = false;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.NetworkManager != NetworkManager) continue;
                if (!p.IsDead) return false;
                any = true;
            }
            return any;
        }

        private void Depart()
        {
            TruckCargo truck = TruckCargo.Current;
            var items = new List<RunResults.Item>();
            if (truck != null)
                foreach (LootItem item in truck.ItemsInside())
                    items.Add(new RunResults.Item { Name = item.Definition.DisplayName, StartValue = item.FullValue, FinalValue = item.CurrentValue });

            var players = new List<RunResults.Player>();
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.NetworkManager != NetworkManager) continue;
                Vector3 at = p.Ragdoll.IsRagdolled ? p.Ragdoll.BodyPosition : p.transform.position;
                bool extracted = !p.IsDead && truck != null && truck.Carries(at);
                int lost = 0;
                foreach (Grabbable pocketed in p.Carrier.Inventory.Items)
                {
                    if (pocketed == null || !pocketed.TryGetComponent(out LootItem item)) continue;
                    // Pockets ride along with their owner; left behind, they're gone (GDD 10).
                    if (extracted) items.Add(new RunResults.Item { Name = item.Definition.DisplayName, StartValue = item.FullValue, FinalValue = item.CurrentValue, Pocketed = true });
                    else lost += item.CurrentValue;
                }
                players.Add(new RunResults.Player { ClientId = p.OwnerClientId, Name = $"Player {p.OwnerClientId + 1}", Extracted = extracted, Died = p.IsDead, PocketValueLost = lost });
            }

            RunResults results = RunResults.From(items, players, State.Quota, State.Seed, Time.time - startedAt);
            if (RunStatsSource != null) results.Stats = RunStatsSource(results);
            RunNetState s = State;
            s.Phase = RunPhase.Departed;
            s.Haul = results.Haul;
            state.Value = s;
            Debug.Log($"[Run] Truck left: {items.Count} items, haul ${results.Haul:N0} / quota ${results.Quota:N0}, " +
                      $"{players.FindAll(p => p.Extracted).Count}/{players.Count} players aboard.");
            ResultsRpc(results);
        }

        /// <summary>Host: funny stats for the appraisal (the run's stats tracker plugs in here, 5.4).</summary>
        public static Func<RunResults, string[]> RunStatsSource { get; set; }

        [Rpc(SendTo.Everyone)]
        private void ResultsRpc(RunResults results)
        {
            Results = results;
            Departed?.Invoke(results);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Current = null;
            SeedSource = null;
            RunStatsSource = null;
            TermsSource = null;
        }
    }
}
