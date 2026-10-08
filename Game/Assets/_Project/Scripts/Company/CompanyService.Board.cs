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
    /// <summary>The contract board, the van to a job and back, and the company's own housekeeping between jobs.</summary>
    public partial class CompanyService
    {
        // ---- The board and the van ----

        /// <summary>Players in the session, on every machine.</summary>
        public static int CrewSize
        {
            get
            {
                int n = 0;
                foreach (NetworkPlayer p in NetworkPlayer.All) if (p != null && p.IsSpawned) n++;
                return Mathf.Max(1, n);
            }
        }

        /// <summary>The quota this crew owes on a board contract (M10.9): a short crew owes less.</summary>
        public int QuotaFor(Contract c) => Mathf.Max(1000, Mathf.RoundToInt(c.Quota * contracts.CrewScale(CrewSize) / 1000f) * 1000);

        /// <summary>The contract's modifier asset (null for none/unknown).</summary>
        public ContractModifier ModifierOf(Contract c)
        {
            foreach (ContractModifier m in contracts.Modifiers) if (m != null && m.Id == c.ModifierId) return m;
            return null;
        }

        /// <summary>Host: choose a contract on the board (-1 = none).</summary>
        public void Select(int index)
        {
            if (DemoOver && index >= 0) return; // the demo's jobs are done (M8.3)
            if (IsServer && index >= -1 && index < Board.Count) selected.Value = index;
        }

        /// <summary>The demo build's company has done every job the demo allows.</summary>
        public bool DemoOver => Demo.IsOver(State.Runs);

        /// <summary>Host, demo: a brand-new company to play the demo again (only the failure count carries over).</summary>
        public void RestartDemo()
        {
            if (!IsServer || !Demo.IsDemo) return;
            CompanyLedger.GoBankrupt(save);
            save.bankruptcies = Mathf.Max(0, save.bankruptcies - 1); // starting over isn't going bankrupt
            store?.Save(save);
            state.Value = CompanyNetState.Of(save, truckUpgrades);
            selected.Value = -1;
            boardSeed.Value = NewSeed();
            PublishGear();
            Debug.Log("[Company] Demo restarted with a new company.");
        }

        /// <summary>Anyone at the van (the host, or the crew if allowed): drive to the selected contract.</summary>
        public void RequestDepart()
        {
            if (IsServer) Depart();
            else if (!RefuseLocally(CrewRule.Drive)) DepartRpc();
        }

        [Rpc(SendTo.Server)]
        private void DepartRpc(RpcParams rpcParams = default)
        {
            if (Allows(rpcParams.Receive.SenderClientId, CrewRule.Drive)) Depart();
        }

        private void Depart()
        {
            if (selected.Value < 0 || selected.Value >= Board.Count || SessionTravel.Current == null) return;
            if (SessionTravel.Current.Level != HomeLevel) return;
            Contract c = Board[selected.Value];
            c.Quota = QuotaFor(c); // the crew that leaves is the crew that owes
            active.Value = c;
            // Written before leaving: quitting mid-job must not dodge the missed quota (settled on next load).
            save.pendingQuota = c.Quota;
            save.pendingBonus = c.PayoutBonus;
            save.pendingLeftOnPurpose = false;
            Saved();
            Debug.Log($"[Company] Taking the {c.ModifierName} contract at {c.Location}: quota ${c.Quota:N0}, stability {c.Stability:P0}.");
            SessionTravel.Current.Travel(c.Scene);
        }

        /// <summary>Host, at the HQ between jobs: rename the company (QA B-24).</summary>
        public void RenameCompany(string newName)
        {
            if (!IsServer || save == null || JobInProgress) return;
            string clean = NetworkPlayer.Clean(newName);
            if (clean.Length == 0 || clean == save.companyName) return;
            save.companyName = clean;
            Saved();
            Debug.Log($"[Company] Renamed to {clean}.");
        }

        /// <summary>Host, at the HQ between jobs: throw this company away and start a new one in its slot (QA B-24).</summary>
        public void StartNewCompany()
        {
            if (!IsServer || save == null || JobInProgress) return;
            int bankruptcies = save.bankruptcies;
            CompanyLedger.GoBankrupt(save);
            save.bankruptcies = bankruptcies; // starting over by choice isn't going bankrupt
            save.companyName = CompanySave.New().companyName;
            Saved();
            selected.Value = -1;
            boardSeed.Value = NewSeed();
            Debug.Log("[Company] A new company was started in this slot.");
        }

        /// <summary>Host is on a job that hasn't been settled yet (leaving now counts as a failed job).</summary>
        public bool JobInProgress => IsServer && save != null && save.pendingQuota > 0;

        /// <summary>
        /// Host chose to end the game mid-job (pause menu): the job counts as failed on the next load. Crashes
        /// and lost connections never call this, so they're voided instead (QA B-06).
        /// </summary>
        public void MarkLeavingMidJob()
        {
            if (!JobInProgress) return;
            save.pendingLeftOnPurpose = true;
            Saved();
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
    }
}
