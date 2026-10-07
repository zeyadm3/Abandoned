using System.Text;
using Abandoned.Contracts;
using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Company
{
    /// <summary>
    /// The contract board (M8.1b; GDD 14): the three offers side by side with everything a crew needs
    /// to choose. The host takes one; everyone else reads along. Close with the button or Esc.
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

            key.Clear().Append(company.IsServer).Append('|').Append(company.Selected);
            foreach (Contract c in company.Board) key.Append('|').Append(c.Seed);
            screen.Build(key.ToString(), panel => Fill(panel, company));
        }

        private static void Fill(VisualElement panel, CompanyService company)
        {
            MenuKit.Text(panel, "CONTRACT BOARD", "heading");
            MenuKit.Text(panel, company.IsServer ? "Pick today's job, then take the van." : "The host picks the job.", "subtitle");
            var cards = new VisualElement();
            cards.AddToClassList("hud-cards");
            panel.Add(cards);
            var board = company.Board;
            for (int i = 0; i < board.Count; i++)
            {
                Contract c = board[i];
                bool taken = company.Selected == i;
                var card = new VisualElement();
                card.AddToClassList("hud-card");
                card.EnableInClassList("hud-card--taken", taken);
                cards.Add(card);
                MenuKit.Text(card, c.Location.ToUpperInvariant() + (taken ? "  (TAKEN)" : ""), "section");
                MenuKit.Text(card, $"<b>{c.ModifierName}</b>  +{c.PayoutBonus:P0} payout");
                if (company.Messages != null && company.Messages.Quip(c.Seed) is string quip && quip.Length > 0)
                    MenuKit.Text(card, $"<i>\"{quip}\"</i>").AddToClassList("text--small");
                if (company.ModifierOf(c) is Contracts.ContractModifier m && !string.IsNullOrEmpty(m.Description))
                    MenuKit.Text(card, m.Description).AddToClassList("text--small");
                MenuKit.Text(card, $"Quota <b>${c.Quota:N0}</b>");
                MenuKit.Text(card, $"Loot ${c.LootMin:N0} - ${c.LootMax:N0}", "text").AddToClassList("text--small");
                MenuKit.Text(card, $"Threat {Contract.ThreatName(c.ThreatLevel)} (known threats: ???)").AddToClassList("text--small");
                MenuKit.Text(card, $"Stability {c.Stability:P0}").AddToClassList("text--small");
                MenuKit.Text(card, c.PowerOff ? "<color=#ff7766>Power OFF</color>" : "Power on").AddToClassList("text--small");
                MenuKit.Text(card, $"Window {c.WindowSeconds / 60f:0} min").AddToClassList("text--small");
                if (company.IsServer)
                {
                    int index = i;
                    MenuKit.Button(card, taken ? "Taken - drive the van" : "Take this job", () => company.Select(index), Audio.SoundId.UiConfirm, small: true)
                        .SetEnabled(!taken);
                }
            }
            MenuKit.Button(panel, "Close", () => ContractBoard.Open = false, Audio.SoundId.UiBack);
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
