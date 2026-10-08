using System;
using System.Collections.Generic;
using Abandoned.Contracts;
using Abandoned.Core;
using Abandoned.Equipment;
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
    public partial class CompanyService : NetworkBehaviour
    {
        public const string HomeLevel = "HQ";

        [SerializeField] private CompanyConfig config;
        [SerializeField] private ContractConfig contracts;
        [SerializeField] private EquipmentCatalog equipment;
        [SerializeField] private TruckUpgradeCatalog truckUpgrades;
        [Tooltip("The boss's voicemails and the board's one-liners (M10.7).")]
        [SerializeField] private CompanyMessages messages;

        private readonly NetworkVariable<CompanyNetState> state = new();
        private readonly NetworkVariable<int> boardSeed = new();
        private readonly NetworkVariable<int> selected = new(-1);
        private readonly NetworkVariable<Contract> active = new();
        private readonly NetworkVariable<OutcomeNet> lastOutcome = new();
        private NetworkList<OwnedGear> owned;
        private readonly NetworkVariable<bool> companyGame = new();

        /// <summary>
        /// Company rules (gear stock, the trolley for solo drags, radios, gear only at the HQ) apply to a game
        /// that started at the HQ; a level hosted directly (dev, tests) plays without them.
        /// </summary>
        public static bool RulesApply => Current != null && Current.IsSpawned && Current.companyGame.Value;

        private void Awake() => owned = new NetworkList<OwnedGear>();

        public EquipmentCatalog Equipment => equipment;

        /// <summary>How many of a catalog item the company owns (every machine).</summary>
        public int OwnedCount(int index)
        {
            if (owned == null) return 0;
            foreach (OwnedGear g in owned) if (g.Index == index) return g.Count;
            return 0;
        }

        private CompanySave save;
        private SaveStore store;
        private RunState hookedRun;
        // The joinable setting last applied, and to which lobby: a lobby re-created mid-run (Steam
        // dropped and came back) starts joinable and must be closed again.
        private ulong lobbyApplied;
        private bool lobbyJoinable;
        private SteamLobby lobby;
        private float nextLobbyLookup;
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
            // Every machine: a level gained on payday gets its sting (M10.8).
            lastOutcome.OnValueChanged += (before, now) =>
            {
                if (now.LevelledUp && now.Run != before.Run) Audio.MusicPlayer.Play(Audio.MusicSting.LevelUp);
            };
            SpawnCrewRules();
            if (!IsServer) return;
            // Closing the window mid-job is choosing to leave (only a crash or a lost connection is forgiven).
            Application.quitting += MarkLeavingMidJob;
            store = PersistsToDisk ? new SaveStore(SaveFolderOverride) : null;
            save = store != null ? store.Load() : CompanySave.New();
            if (CompanyLedger.SettleAbandoned(save, config, out bool voided) is RunOutcome abandoned)
            {
                Debug.LogWarning($"[Company] The last job was abandoned: settled as an empty haul (penalty ${abandoned.Penalty:N0}).");
                store?.Save(save);
            }
            else if (voided)
            {
                Debug.LogWarning("[Company] The last job was cut short (crash or lost connection): voided, no penalty.");
                store?.Save(save);
            }
            state.Value = CompanyNetState.Of(save, truckUpgrades);
            PublishGear();
            boardSeed.Value = NewSeed();
            companyGame.Value = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == HomeLevel;
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
            Application.quitting -= MarkLeavingMidJob;
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
            if (lobby == null && Time.unscaledTime >= nextLobbyLookup)
            {
                nextLobbyLookup = Time.unscaledTime + 1f;
                lobby = FindAnyObjectByType<SteamLobby>();
            }
            SteamLobbyFlow flow = lobby != null ? lobby.Flow : null;
            if (flow != null && flow.InLobby && (flow.LobbyId != lobbyApplied || home != lobbyJoinable))
            {
                lobbyApplied = flow.LobbyId;
                lobbyJoinable = home;
                flow.SetJoinable(home);
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

        private void Saved()
        {
            // A failed write (file locked, disk full) is logged by the store and retried with the next
            // save; what everyone sees must still update.
            store?.Save(save);
            state.Value = CompanyNetState.Of(save, truckUpgrades);
            PublishGear();
        }

        private void PublishGear()
        {
            if (equipment == null) return;
            owned.Clear();
            for (int i = 0; i < equipment.Items.Count; i++)
            {
                int count = save.CountOf(equipment.Items[i].Id);
                if (count > 0) owned.Add(new OwnedGear { Index = (byte)i, Count = (short)count });
            }
            // Bankruptcy can take back gear that is still in someone's hands.
            PlayerEquipment.ServerTrimToStock(NetworkManager);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Current = null;
            SaveFolderOverride = null;
        }
    }
}
