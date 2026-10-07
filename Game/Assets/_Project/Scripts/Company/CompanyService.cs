using System;
using System.Collections.Generic;
using Abandoned.Contracts;
using Abandoned.Extraction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// The company for the whole session (GDD 5, 13, 14): the host loads and saves it (JSON on the host's
    /// machine), everyone sees money, level and the contract board. The host picks a contract; anyone at
    /// the van drives everyone there (session travel); when the truck leaves, the run's haul is paid out
    /// (or the shortfall becomes debt) and the company file is saved; "Back to HQ" brings everyone home
    /// to a fresh board. Test and batch runs never touch the player's real save.
    /// </summary>
    public class CompanyService : NetworkBehaviour
    {
        public const string HomeLevel = "HQ";

        [SerializeField] private CompanyConfig config;
        [SerializeField] private ContractConfig contracts;

        private readonly NetworkVariable<CompanyNetState> state = new();
        private readonly NetworkVariable<int> boardSeed = new();
        private readonly NetworkVariable<int> selected = new(-1);
        private readonly NetworkVariable<Contract> active = new();
        private readonly NetworkVariable<OutcomeNet> lastOutcome = new();

        private CompanySave save;
        private SaveStore store;
        private RunState hookedRun;
        private bool lobbyOpen = true;
        private int boardLevel = -1, boardForSeed;
        private List<Contract> board = new();

        public static CompanyService Current { get; private set; }

        /// <summary>Tests: where the save goes (null = memory only in tests/batch, the real folder otherwise).</summary>
        public static string SaveFolderOverride { get; set; }

        public CompanyConfig Config => config;
        public CompanyNetState State => state.Value;
        public int Selected => selected.Value;
        public Contract Active => active.Value;
        public OutcomeNet LastOutcome => lastOutcome.Value;
        public CompanySave Save => save;

        /// <summary>The board everyone sees (rolled the same on every machine from the board seed and level).</summary>
        public IReadOnlyList<Contract> Board
        {
            get
            {
                if (boardForSeed != boardSeed.Value || boardLevel != State.Level)
                {
                    boardForSeed = boardSeed.Value;
                    boardLevel = State.Level;
                    board = ContractGenerator.Roll(contracts, Mathf.Max(1, State.Level), Mathf.Max(1, boardSeed.Value), config.LootEstimate);
                }
                return board;
            }
        }

        public override void OnNetworkSpawn()
        {
            Current = this;
            DontDestroyOnLoad(gameObject);
            if (!IsServer) return;
            store = PersistsToDisk ? new SaveStore(SaveFolderOverride) : null;
            save = store != null ? store.Load() : CompanySave.New();
            state.Value = CompanyNetState.Of(save);
            boardSeed.Value = NewSeed();
            // Players join only at the HQ, between runs (GDD 11, decision log).
            // Only for a game that started at the HQ; a session hosted straight in a level (dev, tests) stays open.
            NetworkBootstrap.JoinBlocker = () => SessionTravel.Current != null && SessionTravel.Current.StartLevel == HomeLevel
                                                 && SessionTravel.Current.Level != HomeLevel
                ? "The crew is out on a job. Join when they're back at the HQ." : null;
            Debug.Log($"[Company] {save.companyName}: ${save.money:N0}, level {save.level}{(store != null ? $" ({store.Path})" : " (not saved: test/batch run)")}.");
        }

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
            if (IsServer) NetworkBootstrap.JoinBlocker = null;
            Unhook();
        }

        // Automated runs must never overwrite the player's company.
        private static bool PersistsToDisk => SaveFolderOverride != null || !(Application.isBatchMode || SteamInitPolicy.TestRunActive);

        private static int NewSeed() => new System.Random(Environment.TickCount).Next(1, int.MaxValue);

        private void Update()
        {
            if (!IsServer || !IsSpawned) return;
            // The Steam lobby takes joiners only while everyone is at the HQ.
            bool home = SessionTravel.Current == null || SessionTravel.Current.Level == HomeLevel;
            if (home != lobbyOpen)
            {
                lobbyOpen = home;
                SteamLobby lobby = FindAnyObjectByType<SteamLobby>();
                lobby?.Flow?.SetJoinable(home);
            }
            // Pay out when a run's truck leaves (the run state lives in the level).
            if (RunState.Current != hookedRun)
            {
                Unhook();
                hookedRun = RunState.Current;
                if (hookedRun != null) hookedRun.Departed += OnDeparted;
            }
        }

        private void Unhook()
        {
            if (hookedRun != null) hookedRun.Departed -= OnDeparted;
            hookedRun = null;
        }

        // ---- The board and the van ----

        /// <summary>The contract's modifier asset (null for none/unknown).</summary>
        public ContractModifier ModifierOf(Contract c)
        {
            foreach (ContractModifier m in contracts.Modifiers) if (m != null && m.Id == c.ModifierId) return m;
            return null;
        }

        /// <summary>Host: choose a contract on the board (-1 = none).</summary>
        public void Select(int index)
        {
            if (IsServer && index >= -1 && index < Board.Count) selected.Value = index;
        }

        /// <summary>Anyone at the van: drive to the selected contract.</summary>
        public void RequestDepart()
        {
            if (IsServer) Depart();
            else DepartRpc();
        }

        [Rpc(SendTo.Server)]
        private void DepartRpc() => Depart();

        private void Depart()
        {
            if (selected.Value < 0 || selected.Value >= Board.Count || SessionTravel.Current == null) return;
            if (SessionTravel.Current.Level != HomeLevel) return;
            Contract c = Board[selected.Value];
            active.Value = c;
            Debug.Log($"[Company] Taking the {c.ModifierName} contract at {c.Location}: quota ${c.Quota:N0}, stability {c.Stability:P0}.");
            SessionTravel.Current.Travel(c.Scene);
        }

        /// <summary>Host, from the appraisal: everyone home to a fresh board.</summary>
        public void ReturnToHq()
        {
            if (!IsServer || SessionTravel.Current == null) return;
            active.Value = default;
            selected.Value = -1;
            boardSeed.Value = NewSeed();
            SessionTravel.Current.Travel(HomeLevel);
        }

        // ---- Payday ----

        private void OnDeparted(RunResults results)
        {
            if (!IsServer || !active.Value.IsValid) return;
            RunOutcome o = CompanyLedger.Apply(save, config, results.Haul, results.Quota, active.Value.PayoutBonus);
            store?.Save(save);
            state.Value = CompanyNetState.Of(save);
            lastOutcome.Value = OutcomeNet.Of(o, save.runs + save.bankruptcies * 1000);
            Debug.Log($"[Company] Run {save.runs}: payout ${o.Payout:N0}, penalty ${o.Penalty:N0}, +{o.Xp} xp, money ${save.money:N0}" +
                      (o.Bankrupt ? " - BANKRUPT, a new company starts" : ""));
        }

        /// <summary>Host: spend (the shop, 6.4). Saves at once.</summary>
        public bool TrySpend(string itemId, int price)
        {
            if (!IsServer || !CompanyLedger.TryBuy(save, itemId, price)) return false;
            store?.Save(save);
            state.Value = CompanyNetState.Of(save);
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Current = null;
            SaveFolderOverride = null;
        }
    }
}
