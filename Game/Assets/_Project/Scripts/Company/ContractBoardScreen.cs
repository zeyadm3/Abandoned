using System.Text;
using Abandoned.Contracts;
using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Company
{
    /// <summary>
    /// The contract board (M8.1b; GDD 14; job sheets on cork since UI step 8): the three offers side by side
    /// with everything a crew needs to choose. The host takes one (TAKEN is stamped on it); everyone else
    /// reads along. Close with the button or Esc.
    /// </summary>
    public class ContractBoardScreen : MonoBehaviour
    {
        private ScreenPanel screen;
        private readonly StringBuilder key = new();
        private int briefed = -1;
        private bool wasOpen;

        private void Update()
        {
            if (ContractBoard.Open && CompanyService.Current == null) ContractBoard.Open = false;
            if (ContractBoard.Open && Core.PadInput.BackPressed)
                ContractBoard.Open = false;
            CursorOwner.Set(this, ContractBoard.Open);

            CompanyService company = CompanyService.Current;
            bool open = ContractBoard.Open && company != null;
            if (screen == null || screen.Root.panel == null)
            {
                if (!open) return;
                screen = ScreenPanel.Create(wide: true);
                if (screen == null) return;
                screen.Panel.AddToClassList("panel--xwide");
                // MenuKit.Panel sizes inline, which beats the stylesheet: the board's width is set here.
                screen.Panel.style.width = 1320f;
                screen.Panel.style.maxWidth = Length.Percent(96f);
                StyleSheet stationStyles = Resources.Load<StyleSheet>("HqStations");
                if (stationStyles != null) screen.Panel.styleSheets.Add(stationStyles);
                screen.ApplyArchiveStyles();
            }
            screen.Show(open);
            if (!open)
            {
                if (wasOpen && ContractBoard.FromTelephone) DispatchBriefingAudio.Stop();
                briefed = -1;
                wasOpen = false;
                return;
            }

            if (!wasOpen) briefed = company.Selected;
            wasOpen = true;
            if (ContractBoard.FromTelephone && company.Selected != briefed)
            {
                briefed = company.Selected;
                if (briefed >= 0) DispatchTelephone.PlayLine();
            }

            key.Clear().Append(ContractBoard.FromTelephone).Append('|').Append(company.IsServer).Append('|').Append(company.Selected).Append('|').Append(CompanyService.CrewSize);
            foreach (Contract c in company.Board) key.Append('|').Append(c.Seed);
            screen.Build(key.ToString(), panel => Fill(panel, company));
        }

        // UI step 8: job sheets pinned to cork, each a little crooked; TAKEN is stamped on the chosen one.
        private static void Fill(VisualElement panel, CompanyService company)
        {
            MenuKit.Text(panel, ContractBoard.FromTelephone ? Loc.T("DISPATCH TELEPHONE") : Loc.T("CONTRACT BOARD"), "heading");
            MenuKit.Text(panel, company.IsServer ? Loc.T("Pick today's job, then take the van.") : Loc.T("The host picks the job."), "subtitle");
            if (ContractBoard.FromTelephone) Briefing(panel, company);
            var cards = new VisualElement();
            cards.AddToClassList("hud-cards");
            cards.AddToClassList("board__cork");
            panel.Add(cards);
            var board = company.Board;
            for (int i = 0; i < board.Count; i++)
            {
                Contract c = board[i];
                bool taken = company.Selected == i;
                var sheet = new VisualElement();
                sheet.AddToClassList("sheet");
                cards.Add(sheet);
                var pin = new VisualElement { pickingMode = PickingMode.Ignore };
                pin.AddToClassList("sheet__pin");
                sheet.Add(pin);
                Sheet(sheet, Loc.T(c.Location).ToUpperInvariant(), "sheet__place");
                Sheet(sheet, Loc.T(c.ModifierName).ToUpperInvariant(), "sheet__job");
                LocationDefinition location = company.Contracts?.Locations?.Resolve(c) ?? LocationCatalog.Current?.Resolve(c);
                if (location != null && !string.IsNullOrEmpty(location.Description)) Sheet(sheet, location.LocalDescription, "sheet__text");
                if (company.Messages != null && company.Messages.Quip(c.Seed) is string quip && quip.Length > 0) Sheet(sheet, $"\"{Loc.T(quip)}\"", "sheet__quip");
                if (company.ModifierOf(c) is ContractModifier m && !string.IsNullOrEmpty(m.Description)) Sheet(sheet, m.LocalDescription, "sheet__text");
                int crew = CompanyService.CrewSize;
                Fact(sheet, Loc.T("QUOTA"), $"${company.QuotaFor(c):N0}", true, crew < 4 ? Loc.T("scaled for a crew of {0}", crew) : null);
                Fact(sheet, Loc.T("BONUS"), $"+{c.PayoutBonus:P0}", false);
                Fact(sheet, Loc.T("LOOT"), $"${c.LootMin / 1000}k - ${c.LootMax / 1000}k", false);
                Fact(sheet, Loc.T("THREAT"), Contract.ThreatName(c.ThreatLevel), c.ThreatLevel >= 2, Contract.ThreatNote(c.ThreatLevel));
                // 0.20.6: the forecast upgrade names the first monster on site; without it, nobody knows.
                string first = company.HasUpgrade(TruckUpgradeKind.Forecast) && Threats.ThreatForecast.Current != null ? Threats.ThreatForecast.Current.Opening(c.Seed) : null;
                Fact(sheet, Loc.T("KNOWN THREATS"), first != null ? Loc.T("first on site: {0}", Loc.T(first)) : "???", false,
                    first == null && !company.HasUpgrade(TruckUpgradeKind.Forecast) ? Loc.T("the threat forecast (company upgrade) would say") : null);
                Fact(sheet, Loc.T("STABILITY"), $"{c.Stability:P0}", c.Stability < 0.6f);
                Fact(sheet, Loc.T("POWER"), c.PowerOff ? Loc.T("OFF") : Loc.T("ON"), c.PowerOff);
                Fact(sheet, Loc.C("contract", "WINDOW"), Loc.T("{0:0} MIN", c.WindowSeconds / 60f), false);
                // QA B-18: a Sealed job is a wasted trip without something to open shutters with.
                if (company.ModifierOf(c) is ContractModifier sealedJob && sealedJob.Sealed)
                    Fact(sheet, Loc.T("TOOLS"), Loc.T("BOLT CUTTERS OR A CROWBAR"), true, Loc.T("every door is shuttered: bring one"));
                if (company.IsServer)
                {
                    int index = i;
                    Button take = MenuKit.Button(sheet, taken ? Loc.T("Taken - drive the van") : Loc.T("Take this job"), () => company.Select(index), Audio.SoundId.UiConfirm, small: true);
                    take.AddToClassList("sheet__take");
                    take.SetEnabled(!taken);
                }
                if (taken)
                {
                    Label stamp = UiKit.Stamp(sheet, Loc.T("TAKEN"), good: true);
                    stamp.AddToClassList("sheet__stamp");
                    UiKit.Slam(stamp);
                }
            }
            MenuKit.Button(panel, ContractBoard.FromTelephone ? Loc.T("Hang up") : Loc.T("Close"), () => ContractBoard.Open = false, Audio.SoundId.UiBack);
        }

        private static void Briefing(VisualElement panel, CompanyService company)
        {
            var briefing = new VisualElement();
            briefing.AddToClassList("dispatch-briefing");
            panel.Add(briefing);
            MenuKit.Text(briefing, Loc.T("DISPATCH / LINE OPEN"), "section");
            if (company.Selected < 0 || company.Selected >= company.Board.Count)
            {
                MenuKit.Text(briefing, Loc.T("Choose a job sheet below. Dispatch will attach its briefing to this line."));
                return;
            }
            Contract c = company.Board[company.Selected];
            MenuKit.Text(briefing, $"{Loc.T(c.Location).ToUpperInvariant()} / {Loc.T(c.ModifierName).ToUpperInvariant()}", "subtitle");
            string spoken = DispatchBriefingAudio.TranscriptFor(company);
            if (!string.IsNullOrEmpty(spoken)) MenuKit.Text(briefing, $"\"{Loc.T(spoken)}\"");
            MenuKit.Text(briefing, Loc.T("Recover ${0:N0} for a crew of {1}. The departure window is {2:0} minutes. Threat: {3}.",
                    company.QuotaFor(c), CompanyService.CrewSize, c.WindowSeconds / 60f, Contract.ThreatName(c.ThreatLevel)) + " " +
                (c.PowerOff ? Loc.T("Power is off; bring a charged flashlight.") : Loc.T("Power is on.")) + " " +
                Loc.T("Floors start at {0:P0} stability.", c.Stability));
            if (company.ModifierOf(c) is ContractModifier modifier)
            {
                if (!string.IsNullOrEmpty(modifier.Description)) MenuKit.Text(briefing, modifier.LocalDescription);
                if (modifier.Sealed) MenuKit.Text(briefing, Loc.T("Shuttered access: pack bolt cutters or a crowbar."), "text--error");
            }
            MenuKit.Text(briefing, Loc.T("Load salvage into the truck and leave with the crew. The haul is appraised at departure."));
            MenuKit.Button(briefing, Loc.T("Reconnect briefing line"), DispatchTelephone.PlayLine, Audio.SoundId.UiClick, small: true);
        }

        private static Label Sheet(VisualElement parent, string text, string cls)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            parent.Add(l);
            return l;
        }

        // Label column, value column: every value starts at the same x, warnings only change colour.
        private static void Fact(VisualElement sheet, string label, string value, bool warn, string note = null)
        {
            VisualElement row = MenuKit.Row(sheet);
            row.AddToClassList("sheet__fact");
            Sheet(row, label, "sheet__fact-label");
            var cell = new VisualElement { pickingMode = PickingMode.Ignore };
            cell.AddToClassList("sheet__fact-cell");
            row.Add(cell);
            Label shown = Sheet(cell, value, "sheet__fact-value");
            shown.EnableInClassList("sheet__fact-value--warn", warn);
            if (note != null) Sheet(cell, note, "sheet__fact-note");
        }

        private void OnDisable()
        {
            if (ContractBoard.FromTelephone) DispatchBriefingAudio.Stop();
            wasOpen = false;
            CursorOwner.Set(this, false);
            ContractBoard.Open = false;
            screen?.Show(false);
        }

        private void OnDestroy() => screen?.Remove();
    }
}
