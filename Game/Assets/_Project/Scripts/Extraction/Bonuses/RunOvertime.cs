using System.Collections.Generic;
using Abandoned.Loot;
using Abandoned.UI;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Overtime (0.18.6), on the run state's object: from the moment the haul first meets quota, a multiplier
    /// on everything over quota climbs each minute the crew stays (while the building keeps decaying round
    /// them). Paid at departure on top, through <see cref="RunState.Settling"/>.
    /// </summary>
    public class RunOvertime : NetworkBehaviour
    {
        // Server time the haul first met quota this job; 0 = not yet.
        private readonly NetworkVariable<double> metAt = new();
        private RunState run;
        private bool toasted;

        public override void OnNetworkSpawn()
        {
            run = GetComponent<RunState>();
            RunGoals.Add(Goal);
            if (IsServer) RunState.Settling += Settle;
        }

        public override void OnNetworkDespawn()
        {
            RunGoals.Remove(Goal);
            RunState.Settling -= Settle;
        }

        /// <summary>The multiplier now (1 until quota is met).</summary>
        public float Multiplier
        {
            get
            {
                if (run == null || metAt.Value <= 0d) return 1f;
                BonusConfig config = BonusConfig.Current;
                float minutes = (float)(run.Now - metAt.Value) / 60f;
                return Mathf.Min(config.OvertimeMax, 1f + config.OvertimePerMinute * minutes);
            }
        }

        private string Goal() => metAt.Value > 0d ? $"OVERTIME x{Multiplier:0.00} on everything over quota" : null;

        private void Update()
        {
            if (!IsSpawned || run == null) return;
            if (metAt.Value > 0d && !toasted)
            {
                toasted = true;
                ToastFeed.Show("QUOTA MET", "Overtime from here: the longer you stay, the more everything over quota pays.", null, ToastFeed.Kind.Good);
            }
            if (IsServer && metAt.Value <= 0d && run.State.Phase == RunPhase.Running && run.State.Quota > 0 && run.State.Haul >= run.State.Quota)
                metAt.Value = run.Now;
        }

        private void Settle(IReadOnlyCollection<LootItem> aboard, List<(int pay, string line)> lines)
        {
            if (metAt.Value <= 0d || run == null) return;
            // What is aboard as the truck leaves (the same items the appraisal counts), not the synced haul:
            // that one is a tally a moment old, and loot thrown in during the honk would go unpaid.
            int haul = 0;
            foreach (LootItem item in aboard)
                if (item != null && !item.IsShattered) haul += item.CurrentValue;
            float over = haul - run.State.Quota;
            int pay = Mathf.RoundToInt(Mathf.Max(0f, over) * (Multiplier - 1f));
            if (pay > 0) lines.Add((pay, $"OVERTIME x{Multiplier:0.00} on ${over:N0} over quota. +${pay:N0}"));
        }
    }
}
