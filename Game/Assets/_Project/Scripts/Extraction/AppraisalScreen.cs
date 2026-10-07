using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// After the truck leaves (PLAYBOOK 5.4, placeholder OnGUI): every item with its starting value,
    /// what damage cost and what it's worth now; haul vs quota; who made it out; the run's funny stats.
    /// The host starts the next run from here; everyone else waits for them.
    /// </summary>
    public class AppraisalScreen : MonoBehaviour
    {
        [SerializeField] private RunDirector director;
        [SerializeField] private int fontSize = 15;

        private GUIStyle box, label, title, right;
        private Vector2 scroll;

        public bool Showing => RunState.Current != null && RunState.Current.State.Phase == RunPhase.Departed && RunState.Current.Results != null;

        private bool shown;

        // The appraisal owns the mouse while it's up, so a click is a click on the button.
        private void Update()
        {
            bool showing = Showing;
            Core.CursorOwner.Set(this, showing);
            // The money comes in with the screen: a cash register sound once per appraisal.
            if (showing && !shown)
            {
                Audio.GameAudio.PlayUi(Audio.SoundId.Coins);
                RecordProfile(RunState.Current.Results);
            }
            shown = showing;
        }

        private void OnDisable() => Core.CursorOwner.Set(this, false);

        // This player's own progress (cosmetic unlocks): never from tests or headless nettests.
        private static void RecordProfile(RunResults r)
        {
            if (Application.isBatchMode || r == null || Unity.Netcode.NetworkManager.Singleton == null) return;
            ulong me = Unity.Netcode.NetworkManager.Singleton.LocalClientId;
            bool escaped = false;
            foreach (RunResults.Player p in r.Players) if (p.ClientId == me) escaped = p.Extracted;
            Abandoned.Player.PlayerProfile.RecordRun(escaped, r.Haul);
        }

        private void OnGUI()
        {
            if (!Showing) return;
            RunState run = RunState.Current;
            RunResults r = run.Results;
            box ??= new GUIStyle(GUI.skin.box) { fontSize = fontSize };
            label ??= new GUIStyle(GUI.skin.label) { fontSize = fontSize, richText = true };
            title ??= new GUIStyle(label) { fontSize = fontSize + 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            right ??= new GUIStyle(label) { alignment = TextAnchor.MiddleRight };

            float w = Mathf.Min(760f, Screen.width - 40f), h = Mathf.Min(620f, Screen.height - 40f);
            GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h), box);
            GUILayout.Label("APPRAISAL", title);
            string verdict = r.QuotaMet ? "<color=#7dff7d>QUOTA MET</color>" : "<color=#ff7766>QUOTA MISSED</color>";
            GUILayout.Label($"Haul <b>${r.Haul:N0}</b> / quota ${r.Quota:N0}   {verdict}   ({(int)r.Seconds / 60}:{(int)r.Seconds % 60:00})", title);

            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(h * 0.45f));
            Row("<b>Item</b>", "<b>Found at</b>", "<b>Damage</b>", "<b>Worth now</b>");
            foreach (RunResults.Item item in r.Items)
                Row(item.Name + (item.Pocketed ? " (pocket)" : ""), $"${item.StartValue:N0}",
                    item.DamageLost > 0 ? $"<color=#ff7766>-${item.DamageLost:N0}</color>" : "-", $"${item.FinalValue:N0}");
            if (r.Items.Length == 0) GUILayout.Label("The truck left empty.", label);
            GUILayout.EndScrollView();

            foreach (RunResults.Player p in r.Players)
                GUILayout.Label(p.Extracted ? $"{p.Name}: made it out" : p.Died ? $"<color=#ff7766>{p.Name}: died in there</color>" :
                    $"<color=#ff7766>{p.Name}: left behind{(p.PocketValueLost > 0 ? $" (lost ${p.PocketValueLost:N0} in their pockets)" : "")}</color>", label);
            foreach (string line in r.Stats) GUILayout.Label("• " + line, label);

            Company.CompanyService company = Company.CompanyService.Current;
            bool contract = company != null && company.Active.IsValid;
            if (contract) DrawPayday(company);

            GUILayout.FlexibleSpace();
            if (run.IsServer && contract)
            {
                if (GUILayout.Button("Back to HQ", GUILayout.Height(40f))) company.ReturnToHq();
            }
            else if (director != null && run.IsServer)
            {
                if (GUILayout.Button("Next run", GUILayout.Height(40f))) director.StartNextRun();
            }
            else GUILayout.Label(contract ? "Waiting for the host to drive back to HQ..." : "Waiting for the host to start the next run...", label);
            GUILayout.EndArea();
        }

        // GDD 13: what the run did to the company.
        private void DrawPayday(Company.CompanyService company)
        {
            Company.OutcomeNet o = company.LastOutcome;
            if (o.Run == 0) return;
            string money = $"Payout <b>${o.Payout:N0}</b>" + (o.Penalty > 0 ? $"   <color=#ff7766>quota penalty -${o.Penalty:N0}</color>" : "") +
                           $"   +{o.Xp} xp" + (o.LevelledUp ? $"   <color=#7dff7d>LEVEL {o.NewLevel}!</color>" : "");
            GUILayout.Label(money, label);
            int balance = company.State.Money;
            GUILayout.Label($"Company money: {(balance < 0 ? $"<color=#ff7766>DEBT ${-balance:N0}</color>" : $"${balance:N0}")}", label);
            if (o.Bankrupt) GUILayout.Label("<color=#ff5544><b>BANKRUPT.</b> Three missed quotas in a row. The company folds; a new one starts with the basic kit.</color>", label);
            else if (o.MissedInARow > 0) GUILayout.Label($"<color=#ff7766>Missed quotas in a row: {o.MissedInARow}/{company.Config.MissesToBankruptcy}</color>", label);
        }

        private void Row(string name, string found, string damage, string now)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, label, GUILayout.Width(300f));
            GUILayout.Label(found, right, GUILayout.Width(130f));
            GUILayout.Label(damage, right, GUILayout.Width(130f));
            GUILayout.Label(now, right, GUILayout.Width(130f));
            GUILayout.EndHorizontal();
        }
    }
}
