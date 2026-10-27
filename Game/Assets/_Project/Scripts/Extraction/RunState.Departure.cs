using Abandoned.Core;
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
    /// <summary>The truck's lever: starting the departure, calling it off, and who may.</summary>
    public partial class RunState
    {
        // ---- Starting the truck ----

        /// <summary>This machine's player pressed the ignition.</summary>
        public void RequestDepart()
        {
            if (IsServer) TryDepart(NetworkManager.LocalClientId);
            else if (!RefusedLever()) RequestDepartRpc();
        }

        [Rpc(SendTo.Server)]
        private void RequestDepartRpc(RpcParams rpcParams = default) => TryDepart(rpcParams.Receive.SenderClientId);

        /// <summary>Only the player who started departure can cancel, throughout the countdown.</summary>
        public bool CanCancel => State.Phase == RunPhase.Honking && HonkRemaining > 0f && NetworkManager != null && NetworkManager.LocalClientId == State.DepartureInitiator;

        /// <summary>This machine's player pulled the lever again while the truck honks: stay (QA B-09).</summary>
        public void RequestCancelDepart()
        {
            if (IsServer) TryCancelDepart(NetworkManager.LocalClientId);
            else if (!RefusedLever()) RequestCancelDepartRpc();
        }

        [Rpc(SendTo.Server)]
        private void RequestCancelDepartRpc(RpcParams rpcParams = default) => TryCancelDepart(rpcParams.Receive.SenderClientId);

        private void TryCancelDepart(ulong client)
        {
            if (State.Phase != RunPhase.Honking || HonkRemaining <= 0f || client != State.DepartureInitiator || !AtLever(client)) return;
            RunNetState s = State;
            s.Phase = RunPhase.Running;
            s.HonkEnd = 0d;
            state.Value = s;
            Debug.Log($"[Run] {NetworkPlayer.NameOf(client)} stopped the truck.");
        }

        // Client: the host keeps the lever to themselves on this crew.
        private static bool RefusedLever()
        {
            Company.CompanyService company = Company.CompanyService.Current;
            if (company == null || company.LocalMay(Company.CrewRule.Lever)) return false;
            UI.ToastFeed.Show(Loc.T("HOST ONLY"), Loc.T(Company.CompanyService.Refusal(Company.CrewRule.Lever)), null, UI.ToastFeed.Kind.Warn);
            return true;
        }

        // Host: a living player at the lever, whom the host's crew rules let pull it.
        private bool AtLever(ulong client)
        {
            TruckCargo truck = TruckCargo.Current;
            NetworkObject player = NetworkManager.ConnectedClients.TryGetValue(client, out NetworkClient c) ? c.PlayerObject : null;
            if (truck == null || player == null) return false;
            if (player.TryGetComponent(out NetworkPlayer np) && np.IsDead) return false;
            Company.CompanyService company = Company.CompanyService.Current;
            if (company != null && company.IsSpawned && !company.Allows(client, Company.CrewRule.Lever)) return false;
            return Vector3.Distance(player.transform.position, truck.Ignition.position) <= config.IgnitionRange;
        }

        /// <summary>
        /// Host (the open world): the crew drove off with the job still running: it leaves now, as when the horn
        /// stops; whoever and whatever is aboard goes with it.
        /// </summary>
        public void ServerDepartNow()
        {
            if (!IsServer || State.Phase is not (RunPhase.Running or RunPhase.Honking)) return;
            Tally();
            Depart();
        }

        private void TryDepart(ulong client)
        {
            if (State.Phase != RunPhase.Running || !AtLever(client)) return;
            Tally();
            if (State.Overloaded) return;
            RunNetState s = State;
            s.Phase = RunPhase.Honking;
            s.HonkEnd = Now + HonkSeconds;
            s.DepartureInitiator = client;
            state.Value = s;
            Debug.Log($"[Run] {NetworkPlayer.NameOf(client)} started the truck; leaving in {HonkSeconds:0} s.");
        }

        /// <summary>
        /// Host, as the truck leaves (0.18+): each bonus adds pay on top of the haul and a line for the appraisal,
        /// given what is aboard. Not called for a wiped crew (no bonus then).
        /// </summary>
        public static event Action<IReadOnlyCollection<LootItem>, List<(int pay, string line)>> Settling;

        private RunPhase shownPhase;

        private void Honk()
        {
            if (Time.time < nextHorn || TruckCargo.Current == null) return;
            nextHorn = Time.time + config.HornInterval;
            // 2026 remake: the company's fitted horn.
            GameAudio.Play(Abandoned.Company.TruckLook.CurrentHorn, TruckCargo.Current.Ignition.position, 1f);
        }
    }
}
