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

        /// <summary>The honk can still be called off (not once the doors are closing).</summary>
        public bool CanCancel => State.Phase == RunPhase.Honking && HonkRemaining > CancelCutoff;

        // The doors close in the last two seconds (TruckSanctuary); after that it's going.
        private const float CancelCutoff = 2f;

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
            if (!CanCancel || !AtLever(client)) return;
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
            UI.ToastFeed.Show("HOST ONLY", Company.CompanyService.Refusal(Company.CrewRule.Lever), null, UI.ToastFeed.Kind.Warn);
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

        private void TryDepart(ulong client)
        {
            if (State.Phase != RunPhase.Running || !AtLever(client)) return;
            Tally();
            if (State.Overloaded) return;
            RunNetState s = State;
            s.Phase = RunPhase.Honking;
            s.HonkEnd = Now + HonkSeconds;
            state.Value = s;
            Debug.Log($"[Run] {NetworkPlayer.NameOf(client)} started the truck; leaving in {HonkSeconds:0} s.");
        }

        private RunPhase shownPhase;

        private void Honk()
        {
            if (Time.time < nextHorn || TruckCargo.Current == null) return;
            nextHorn = Time.time + config.HornInterval;
            GameAudio.Play(SoundId.Horn, TruckCargo.Current.Ignition.position, 1f);
        }
    }
}
