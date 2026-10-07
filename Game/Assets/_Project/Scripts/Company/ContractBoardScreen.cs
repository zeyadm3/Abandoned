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

        private void Update()
        {
            if (ContractBoard.Open && CompanyService.Current == null) ContractBoard.Open = false;
            if (ContractBoard.Open && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                ContractBoard.Open = false;
            CursorOwner.Set(this, ContractBoard.Open);

            CompanyService company = CompanyService.Current;
            bool open = ContractBoard.Open && company != null;
            if (screen == null)
            {
                if (!open) return;
                screen = ScreenPanel.Create(wide: true);
                if (screen == null) return;
                screen.Panel.AddToClassList("panel--xwide");
            }
            screen.Show(open);
            if (!open) return;

            key.Clear().Append(company.IsServer).Append('|').Append(company.Selected).Append('|').Append(CompanyService.CrewSize);
            foreach (Contract c in company.Board) key.Append('|').Append(c.Seed);
            screen.Build(key.ToString(), panel => Fill(panel, company));
        }

        // UI step 8: job sheets pinned to cork, each a little crooked; TAKEN is stamped on the chosen one.
        private static void Fill(VisualElement panel, CompanyService company)
        {
            MenuKit.Text(panel, "CONTRACT BOARD", "heading");
            MenuKit.Text(panel, company.IsServer ? "Pick today's job, then take the van." : "The host picks the job.", "subtitle");
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
                sheet.style.rotate = new Rotate((c.Seed % 5 - 2) * 0.8f);
                cards.Add(sheet);
                var pin = new VisualElement { pickingMode = PickingMode.Ignore };
                pin.AddToClassList("sheet__pin");
                sheet.Add(pin);
                Sheet(sheet, c.Location.ToUpperInvariant(), "sheet__place");
                Sheet(sheet, c.ModifierName.ToUpperInvariant(), "sheet__job");
                if (company.Messages != null && company.Messages.Quip(c.Seed) is string quip && quip.Length > 0) Sheet(sheet, $"\"{quip}\"", "sheet__quip");
                if (company.ModifierOf(c) is ContractModifier m && !string.IsNullOrEmpty(m.Description)) Sheet(sheet, m.Description, "sheet__text");
                int crew = CompanyService.CrewSize;
                Fact(sheet, "QUOTA", $"${company.QuotaFor(c):N0}" + (crew < 4 ? $" (crew of {crew})" : ""), true);
                Fact(sheet, "BONUS", $"+{c.PayoutBonus:P0}", false);
                Fact(sheet, "LOOT", $"${c.LootMin / 1000}k - ${c.LootMax / 1000}k", false);
                Fact(sheet, "THREAT", Contract.ThreatName(c.ThreatLevel), c.ThreatLevel >= 2);
                Fact(sheet, "STABILITY", $"{c.Stability:P0}", c.Stability < 0.6f);
                Fact(sheet, "POWER", c.PowerOff ? "OFF" : "ON", c.PowerOff);
                Fact(sheet, "WINDOW", $"{c.WindowSeconds / 60f:0} MIN", false);
                if (company.IsServer)
                {
                    int index = i;
                    Button take = MenuKit.Button(sheet, taken ? "Taken - drive the van" : "Take this job", () => company.Select(index), Audio.SoundId.UiConfirm, small: true);
                    take.AddToClassList("sheet__take");
                    take.SetEnabled(!taken);
                }
                if (taken)
                {
                    Label stamp = UiKit.Stamp(sheet, "TAKEN", good: true);
                    stamp.AddToClassList("sheet__stamp");
                    UiKit.Slam(stamp);
                }
            }
            MenuKit.Button(panel, "Close", () => ContractBoard.Open = false, Audio.SoundId.UiBack);
        }

        private static Label Sheet(VisualElement parent, string text, string cls)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            parent.Add(l);
            return l;
        }

        private static void Fact(VisualElement sheet, string label, string value, bool warn)
        {
            VisualElement row = MenuKit.Row(sheet);
            row.AddToClassList("sheet__fact");
            Sheet(row, label, "sheet__fact-label");
            Sheet(row, value, warn ? "sheet__fact-value--warn" : "sheet__fact-value");
        }

        private void OnDisable()
        {
            CursorOwner.Set(this, false);
            ContractBoard.Open = false;
            screen?.Show(false);
        }

        private void OnDestroy() => screen?.Remove();
    }
}
