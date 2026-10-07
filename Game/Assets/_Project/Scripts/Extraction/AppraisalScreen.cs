using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Extraction
{
    /// <summary>
    /// After the truck leaves (PLAYBOOK 5.4; UI Toolkit since M8.1b): every item with its starting value,
    /// what damage cost and what it's worth now; haul vs quota; who made it out; the run's funny stats.
    /// The host starts the next run from here; everyone else waits for them.
    /// </summary>
    public class AppraisalScreen : MonoBehaviour
    {
        [SerializeField] private RunDirector director;

        private ScreenPanel screen;

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

        private void OnDisable()
        {
            Core.CursorOwner.Set(this, false);
            screen?.Show(false);
        }

        // This player's own progress (cosmetic unlocks): never from tests or headless nettests.
        private static void RecordProfile(RunResults r)
        {
            if (Application.isBatchMode || r == null || Unity.Netcode.NetworkManager.Singleton == null) return;
            ulong me = Unity.Netcode.NetworkManager.Singleton.LocalClientId;
            bool escaped = false;
            foreach (RunResults.Player p in r.Players) if (p.ClientId == me) escaped = p.Extracted;
            Abandoned.Player.PlayerProfile.RecordRun(escaped, r.Haul);
        }

        private void LateUpdate()
        {
            bool showing = Showing;
            if (screen == null)
            {
                if (!showing || (screen = ScreenPanel.Create(wide: true)) == null) return;
            }
            screen.Show(showing);
            if (!showing) return;
            RunState run = RunState.Current;
            Company.CompanyService company = Company.CompanyService.Current;
            Company.OutcomeNet o = company != null ? company.LastOutcome : default;
            // Rebuilt when the results, the payday or who may press the button change.
            screen.Build($"{run.Results.GetHashCode()}|{run.IsServer}|{o.Run}|{o.Payout}|{(company != null ? company.State.Money : 0)}",
                panel => Fill(panel, run, company));
        }

        private void Fill(VisualElement panel, RunState run, Company.CompanyService company)
        {
            RunResults r = run.Results;
            MenuKit.Text(panel, "APPRAISAL", "heading");
            string verdict = r.QuotaMet ? "<color=#7dff7d>QUOTA MET</color>" : "<color=#ff7766>QUOTA MISSED</color>";
            MenuKit.Text(panel, $"HAUL ${r.Haul:N0} / QUOTA ${r.Quota:N0}   {verdict}   ({(int)r.Seconds / 60}:{(int)r.Seconds % 60:00})", "subtitle");

            var scroll = new ScrollView();
            scroll.AddToClassList("scroll");
            scroll.style.maxHeight = 300;
            panel.Add(scroll);
            Row(scroll, "<b>ITEM</b>", "<b>FOUND AT</b>", "<b>DAMAGE</b>", "<b>WORTH NOW</b>");
            foreach (RunResults.Item item in r.Items)
                Row(scroll, item.Name + (item.Pocketed ? " (pocket)" : ""), $"${item.StartValue:N0}",
                    item.DamageLost > 0 ? $"<color=#ff7766>-${item.DamageLost:N0}</color>" : "-", $"${item.FinalValue:N0}");
            if (r.Items.Length == 0) MenuKit.Text(scroll, "The truck left empty.");

            MenuKit.Text(panel, "CREW", "section");
            foreach (RunResults.Player p in r.Players)
                MenuKit.Text(panel, p.Extracted ? $"{p.Name}: made it out" : p.Died ? $"<color=#ff7766>{p.Name}: died in there</color>" :
                    $"<color=#ff7766>{p.Name}: left behind{(p.PocketValueLost > 0 ? $" (lost ${p.PocketValueLost:N0} in their pockets)" : "")}</color>");
            foreach (string line in r.Stats) MenuKit.Text(panel, "• " + line).AddToClassList("text--small");

            bool contract = company != null && company.Active.IsValid;
            if (contract) Payday(panel, company);

            if (run.IsServer && contract) MenuKit.Button(panel, "Back to HQ", company.ReturnToHq, Audio.SoundId.UiConfirm);
            else if (director != null && run.IsServer) MenuKit.Button(panel, "Next run", director.StartNextRun, Audio.SoundId.UiConfirm);
            else MenuKit.Text(panel, contract ? "Waiting for the host to drive back to HQ..." : "Waiting for the host to start the next run...");
        }

        // GDD 13: what the run did to the company.
        private static void Payday(VisualElement panel, Company.CompanyService company)
        {
            Company.OutcomeNet o = company.LastOutcome;
            if (o.Run == 0) return;
            MenuKit.Text(panel, "PAYDAY", "section");
            MenuKit.Text(panel, $"Payout <b>${o.Payout:N0}</b>" + (o.Penalty > 0 ? $"   <color=#ff7766>quota penalty -${o.Penalty:N0}</color>" : "") +
                                $"   +{o.Xp} xp" + (o.LevelledUp ? $"   <color=#7dff7d>LEVEL {o.NewLevel}!</color>" : ""));
            int balance = company.State.Money;
            MenuKit.Text(panel, $"Company money: {(balance < 0 ? $"<color=#ff7766>DEBT ${-balance:N0}</color>" : $"${balance:N0}")}");
            if (o.Bankrupt) MenuKit.Text(panel, "<color=#ff5544><b>BANKRUPT.</b> Three missed quotas in a row. The company folds; a new one starts with the basic kit.</color>");
            else if (o.MissedInARow > 0) MenuKit.Text(panel, $"<color=#ff7766>Missed quotas in a row: {o.MissedInARow}/{company.Config.MissesToBankruptcy}</color>");
        }

        private static void Row(VisualElement parent, string name, string found, string damage, string now)
        {
            VisualElement row = MenuKit.Row(parent);
            row.AddToClassList("hud-row");
            MenuKit.Text(row, name, "hud-cell").AddToClassList("hud-cell--name");
            MenuKit.Text(row, found, "hud-cell");
            MenuKit.Text(row, damage, "hud-cell");
            MenuKit.Text(row, now, "hud-cell");
        }

        private void OnDestroy() => screen?.Remove();
    }
}
