using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// Counts the 0.19.10 achievement stats on this player's machine: what this player did (breakers, lift
    /// rides, vents, pings, coming back as a new hire) and what the job paid for (sets, boss requests, rush
    /// orders, overtime, from the departure's bonus lines).
    /// </summary>
    public class AchievementWatcher : MonoBehaviour
    {
        private RunState hooked;
        private int shownBreakers, shownPings;
        private bool wasDead, wasInVent, wasOnLift;
        private VaultPhase shownVault;

        private void Update()
        {
            RunState run = RunState.Current;
            if (run != hooked)
            {
                if (hooked != null) hooked.Departed -= OnDeparted;
                hooked = run;
                if (hooked != null) hooked.Departed += OnDeparted;
                shownVault = VaultPhase.Locked;
            }
            NetworkPlayer me = NetworkPlayer.Local;
            if (me == null || !me.IsSpawned) return;
            // Back from the dead mid-job with the agency's fee on the board: a new hire.
            if (wasDead && !me.IsDead && run != null && run.IsSpawned && run.State.Phase == RunPhase.Running && RunHires.Current != null)
                Achievements.Increment(Achievements.StatHires);
            wasDead = me.IsDead;
            bool inVent = me.InVent;
            if (wasInVent && !inVent && !me.IsDead) Achievements.Increment(Achievements.StatVents);
            wasInVent = inVent;
            FreightLift lift = FreightLift.Active;
            bool onLift = lift != null && RunLift.Current != null && RunLift.Current.Moving && lift.OnDeck(me.transform.position);
            if (wasOnLift && !onLift) Achievements.Increment(Achievements.StatLiftRides);
            wasOnLift = onLift;
            if (RunVault.Current != null && RunVault.Current.IsSpawned)
            {
                if (RunVault.Current.Phase == VaultPhase.Open && shownVault != VaultPhase.Open) Achievements.Increment(Achievements.StatVaults);
                shownVault = RunVault.Current.Phase;
            }
        }

        /// <summary>This player threw a breaker or pinged (called from where they press it).</summary>
        public static void Breaker() => Achievements.Increment(Achievements.StatBreakers);
        public static void Ping() => Achievements.Increment(Achievements.StatPings);

        private static void OnDeparted(RunResults results)
        {
            if (results == null || results.Wiped || results.Stats == null) return;
            foreach (string line in results.Stats)
            {
                if (line.StartsWith("SET COMPLETE")) Achievements.Increment(Achievements.StatSets);
                else if (line.StartsWith("BOSS REQUEST: the")) Achievements.Increment(Achievements.StatBossRequests);
                else if (line.StartsWith("RUSH ORDER:")) Achievements.Increment(Achievements.StatRushOrders);
                else if (line.StartsWith("OVERTIME")) Achievements.Increment(Achievements.StatOvertime);
            }
        }
    }
}
