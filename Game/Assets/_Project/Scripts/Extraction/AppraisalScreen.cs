using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Extraction
{
    /// <summary>
    /// After the truck leaves (PLAYBOOK 5.4; a till receipt since UI step 7): every item prints with what it
    /// was worth and what damage left of it, the haul counts up, QUOTA MET / MISSED is stamped on; beside it
    /// who made it out, the run's funny stats as employee awards, and the payday tally. The host starts the
    /// next run (or drives back to HQ) from here; everyone else waits for them.
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
                RunResults results = RunState.Current.Results;
                if (results != null) Audio.MusicPlayer.Play(results.QuotaMet ? Audio.MusicSting.Payday : Audio.MusicSting.QuotaMissed);
                RecordProfile(results);
            }
            shown = showing;
        }

        private void OnDisable()
        {
            Core.CursorOwner.Set(this, false);
            screen?.Show(false);
        }

        // This player's own progress (cosmetic unlocks): never from tests or headless nettests, nor from training
        // (a practice house replayed for stats and achievements would be farming).
        private static void RecordProfile(RunResults r)
        {
            if (Application.isBatchMode || r == null || Unity.Netcode.NetworkManager.Singleton == null || Tutorial.TutorialMode.Active) return;
            ulong me = Unity.Netcode.NetworkManager.Singleton.LocalClientId;
            bool escaped = false;
            foreach (RunResults.Player p in r.Players) if (p.ClientId == me) escaped = p.Extracted;
            Abandoned.Player.PlayerProfile.RecordRun(escaped, r.Haul);
            int jackpots = 0;
            foreach (RunResults.Item item in r.Items) if (item.Jackpot) jackpots++;
            RunNetState s = RunState.Current.State;
            Core.Achievements.RecordRun(escaped, r.Haul, r.QuotaMet, jackpots, s.PowerOff || s.Night);
        }

        private VisualElement paydayBox, buttonsBox;
        private string paydayKey;

        private void LateUpdate()
        {
            bool showing = Showing;
            if (screen == null)
            {
                if (!showing || (screen = ScreenPanel.Create(wide: true)) == null) return;
                screen.Panel.AddToClassList("appraisal");
            }
            screen.Show(showing);
            if (!showing) return;
            RunState run = RunState.Current;
            Company.CompanyService company = Company.CompanyService.Current;
            // The receipt prints once per run's results; the payday and buttons update in place under it
            // (the outcome arrives a moment after the results, and reprinting would restart the show).
            screen.Build(run.Results.GetHashCode().ToString(), panel =>
            {
                paydayKey = null;
                Fill(panel, run);
            });
            Company.OutcomeNet o = company != null ? company.LastOutcome : default;
            string key = $"{run.IsServer}|{o.Run}|{o.Payout}|{(company != null ? company.State.Money : 0)}|{(company != null && company.Active.IsValid)}";
            if (key == paydayKey || paydayBox == null) return;
            paydayKey = key;
            bool contract = company != null && company.Active.IsValid;
            paydayBox.Clear();
            buttonsBox.Clear();
            if (Tutorial.TutorialMode.Active)
            {
                Tutorial.TutorialAppraisal.Payday(paydayBox, run.Results);
                Tutorial.TutorialAppraisal.Buttons(buttonsBox, director, run.IsServer);
                return;
            }
            if (contract) Payday(paydayBox, company);
            if (run.IsServer && contract) MenuKit.Button(buttonsBox, Loc.T("Back to HQ"), company.ReturnToHq, Audio.SoundId.UiConfirm);
            else if (director != null && run.IsServer) MenuKit.Button(buttonsBox, Loc.T("Next run"), director.StartNextRun, Audio.SoundId.UiConfirm);
            else MenuKit.Text(buttonsBox, contract ? Loc.T("Waiting for the host to drive back to HQ...") : Loc.T("Waiting for the host to start the next run..."));
        }

        // UI step 7: a till receipt that prints line by line, the haul counting up, a stamp; the crew,
        // the run's awards and the payday beside it.
        private void Fill(VisualElement panel, RunState run)
        {
            RunResults r = run.Results;
            var columns = new VisualElement();
            columns.AddToClassList("appraisal__columns");
            panel.Add(columns);

            var receipt = new VisualElement();
            receipt.AddToClassList("receipt");
            columns.Add(receipt);
            Company.CompanyService company = Company.CompanyService.Current;
            // Names are cleaned of markup when set (NetworkPlayer.Clean), so they're safe in a rich-text line.
            Line(receipt, company != null && company.State.Name.Length > 0 ? company.State.Name.ToString().ToUpperInvariant() : Company.CompanySave.DefaultName.ToUpperInvariant(), "receipt__head");
            Line(receipt, Loc.T("APPRAISAL - RUN {0:0000} - {1}:{2:00} ON SITE", r.Seed % 10000, (int)r.Seconds / 60, (int)r.Seconds % 60), "receipt__small");
            Line(receipt, new string('-', 44), "receipt__rule");
            var lines = new System.Collections.Generic.List<VisualElement>();
            var scroll = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.AddToClassList("receipt__items");
            receipt.Add(scroll);
            foreach (RunResults.Item item in r.Items)
            {
                VisualElement row = Item(scroll, item);
                row.style.display = DisplayStyle.None;
                lines.Add(row);
            }
            if (r.Items.Length == 0) Line(scroll, Loc.T("NOTHING. THE TRUCK LEFT EMPTY."), "receipt__line");
            Line(receipt, new string('-', 44), "receipt__rule");
            VisualElement total = MenuKit.Row(receipt);
            total.AddToClassList("receipt__total");
            Line(total, Loc.T("HAUL"), "receipt__total-label");
            Label haul = Line(total, "$0", "receipt__total-value");
            VisualElement quotaRow = MenuKit.Row(receipt);
            Line(quotaRow, Loc.T("QUOTA"), "receipt__line");
            Line(quotaRow, $"${r.Quota:N0}", "receipt__line-value");
            Label stamp = UiKit.Stamp(receipt, r.QuotaMet ? Loc.T("QUOTA MET") : Loc.T("QUOTA MISSED"), r.QuotaMet);
            stamp.AddToClassList("receipt__stamp");

            // Print: a line every so often (with a tick), then the total counts up and the stamp comes down.
            const long step = 110;
            for (int i = 0; i < lines.Count; i++)
            {
                VisualElement row = lines[i];
                receipt.schedule.Execute(() =>
                {
                    row.style.display = DisplayStyle.Flex;
                    Audio.GameAudio.PlayUi(Audio.SoundId.UiClick, 0.3f);
                }).StartingIn(300 + step * i);
            }
            long printed = 300 + step * lines.Count;
            float countStart = -1f;
            receipt.schedule.Execute(() =>
            {
                if (countStart < 0f) countStart = Time.unscaledTime;
                float k = Mathf.Clamp01((Time.unscaledTime - countStart) / 0.9f);
                haul.text = $"${Mathf.RoundToInt(r.Haul * k):N0}";
            }).StartingIn(printed).Every(30).Until(() => countStart >= 0f && Time.unscaledTime - countStart > 0.95f);
            receipt.schedule.Execute(() =>
            {
                haul.text = $"${r.Haul:N0}";
                UiKit.Slam(stamp);
                Audio.GameAudio.PlayUi(r.QuotaMet ? Audio.SoundId.Coins : Audio.SoundId.UiError);
            }).StartingIn(printed + 1000);

            var side = new VisualElement();
            side.AddToClassList("appraisal__side");
            columns.Add(side);
            MenuKit.Text(side, Loc.T("APPRAISAL"), "heading");
            MenuKit.Text(side, Loc.T("CREW"), "section");
            foreach (RunResults.Player p in r.Players)
            {
                // The host sends a stand-in name ("Player 2") as a word for each machine to word.
                string name = Loc.Value(p.Name);
                MenuKit.Text(side, p.Extracted ? Loc.T("<color=#7ee07e>\u25CF</color> {0}: made it out", name) : p.Died ? Loc.T("<color=#ff5c4a>\u2716</color> {0}: died in there", name) :
                    p.PocketValueLost > 0 ? Loc.T("<color=#ff5c4a>\u25CB</color> {0}: left behind (lost ${1:N0} in their pockets)", name, p.PocketValueLost)
                    : Loc.T("<color=#ff5c4a>\u25CB</color> {0}: left behind", name));
            }
            if (r.Blame.Length > 0)
            {
                // 0.19.3: the blame sheet, a job title for everyone.
                MenuKit.Text(side, Loc.T("WHO DID WHAT"), "section");
                // The host sends these as English with their values; each machine words them (Loc.Wire).
                foreach (string line in r.Blame) MenuKit.Text(side, Loc.FromWire(line), "text");
            }
            if (r.Stats.Length > 0)
            {
                MenuKit.Text(side, Loc.T("EMPLOYEE AWARDS"), "section");
                foreach (string line in r.Stats)
                {
                    VisualElement award = MenuKit.Row(side);
                    award.AddToClassList("award");
                    UiKit.Icon(award, "board/award", "small").AddToClassList("award__icon");
                    MenuKit.Text(award, Loc.FromWire(line), "text").AddToClassList("award__text");
                }
            }
            paydayBox = new VisualElement();
            side.Add(paydayBox);
            buttonsBox = new VisualElement();
            buttonsBox.AddToClassList("appraisal__buttons");
            side.Add(buttonsBox);
        }

        private static VisualElement Item(VisualElement parent, RunResults.Item item)
        {
            VisualElement row = MenuKit.Row(parent);
            row.AddToClassList("receipt__row");
            string name = Loc.T(item.Name).ToUpperInvariant();
            if (item.Pocketed) name = Loc.T("{0} (POCKET)", name);
            if (item.Jackpot) name += " *";
            Line(row, name, "receipt__line");
            if (item.DamageLost > 0)
            {
                Line(row, $"<s>${item.StartValue:N0}</s>", "receipt__was");
                Line(row, $"${item.FinalValue:N0}", "receipt__line-value").AddToClassList("receipt__line-value--damaged");
            }
            else Line(row, $"${item.FinalValue:N0}", "receipt__line-value");
            return row;
        }

        private static Label Line(VisualElement parent, string text, string cls)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            parent.Add(l);
            return l;
        }

        // GDD 13: what the run did to the company, as a tally.
        private static void Payday(VisualElement panel, Company.CompanyService company)
        {
            Company.OutcomeNet o = company.LastOutcome;
            if (o.Run == 0) return;
            MenuKit.Text(panel, Loc.T("PAYDAY"), "section");
            Tally(panel, Loc.T("Payout"), $"${o.Payout:N0}", "text--money");
            if (o.Penalty > 0) Tally(panel, Loc.T("Quota penalty"), $"-${o.Penalty:N0}", "text--error");
            if (o.Costs > 0) Tally(panel, Loc.T("Running costs"), $"-${o.Costs:N0}", "text--error");
            Tally(panel, Loc.T("Experience"), o.LevelledUp ? Loc.T("+{0} XP  -  LEVEL {1}!", o.Xp, o.NewLevel) : Loc.T("+{0} XP", o.Xp), o.LevelledUp ? "text--good" : null);
            int balance = company.State.Money;
            Tally(panel, Loc.T("Company money"), balance < 0 ? Loc.T("DEBT ${0:N0}", -balance) : $"${balance:N0}", balance < 0 ? "text--error" : "text--money").AddToClassList("tally--total");
            if (o.Bankrupt) MenuKit.Text(panel, Loc.T("<color=#ff5c4a><b>BANKRUPT.</b> Three missed quotas in a row. The company folds; a new one starts with the basic kit.</color>"));
            else if (o.MissedInARow > 0) MenuKit.Text(panel, Loc.T("<color=#ff5c4a>Missed quotas in a row: {0}/{1}</color>", o.MissedInARow, company.Config.MissesToBankruptcy));
        }

        private static VisualElement Tally(VisualElement parent, string label, string value, string cls)
        {
            VisualElement row = MenuKit.Row(parent);
            row.AddToClassList("tally");
            MenuKit.Text(row, label, "tally__label");
            Label v = MenuKit.Text(row, value, "tally__value");
            if (cls != null) v.AddToClassList(cls);
            return row;
        }

        private void OnDestroy() => screen?.Remove();
    }
}
