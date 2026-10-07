using System.Text;
using Abandoned.Audio;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Abandoned.Equipment
{
    /// <summary>
    /// The HQ's gear screens (M8.1b): the shop (company money buys gear for everyone) and the gear rack
    /// (fill your two hand slots from what the company owns). Esc or Close shuts them.
    /// </summary>
    public class GearScreens : MonoBehaviour
    {
        private ScreenPanel screen;
        private readonly StringBuilder key = new();

        private void Update()
        {
            bool open = ShopTerminal.Open || GearRack.Open;
            if (open && (CompanyService.Current == null || Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
                ShopTerminal.Open = GearRack.Open = false;
            open = ShopTerminal.Open || GearRack.Open;
            CursorOwner.Set(this, open);

            CompanyService company = CompanyService.Current;
            if (screen == null)
            {
                if (!open || company == null) return;
                if ((screen = ScreenPanel.Create(wide: true)) == null) return;
            }
            screen.Show(open && company != null);
            if (!open || company == null) return;

            PlayerEquipment mine = Mine();
            key.Clear().Append(ShopTerminal.Open ? "shop" : "rack").Append('|').Append(company.State.Money).Append('|').Append(company.State.Level);
            for (int i = 0; i < company.Equipment.Items.Count; i++) key.Append('|').Append(company.OwnedCount(i));
            if (mine != null) key.Append('|').Append(mine.State[0]).Append(',').Append(mine.State[1]);
            foreach (PlayerEquipment e in PlayerEquipment.All) if (e != null) key.Append(';').Append(e.State[0]).Append(',').Append(e.State[1]);
            screen.Build(key.ToString(), panel =>
            {
                if (ShopTerminal.Open) Shop(panel, company);
                else Rack(panel, company, mine);
                MenuKit.Button(panel, "Close", () => ShopTerminal.Open = GearRack.Open = false, SoundId.UiBack);
            });
        }

        private static void Shop(VisualElement panel, CompanyService company)
        {
            EquipmentCatalog catalog = company.Equipment;
            int money = company.State.Money;
            MenuKit.Text(panel, "SHOP", "heading");
            MenuKit.Text(panel, $"COMPANY MONEY {(money < 0 ? $"DEBT ${-money:N0}" : $"${money:N0}")}   LEVEL {company.State.Level}", "subtitle");
            var scroll = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.AddToClassList("scroll");
            panel.Add(scroll);
            for (int i = 0; i < catalog.Items.Count; i++)
            {
                EquipmentDefinition d = catalog.Items[i];
                VisualElement row = MenuKit.Row(scroll);
                row.AddToClassList("hud-row");
                Label about = MenuKit.Text(row, $"<b>{d.DisplayName}</b>{(d.Consumable ? " (single use)" : "")}\n{d.Description}");
                about.AddToClassList("hud-cell--name");
                MenuKit.Text(row, $"${d.Price:N0}\nowned {company.OwnedCount(i)}", "hud-cell");
                bool locked = d.UnlockLevel > company.State.Level;
                int index = i;
                MenuKit.Button(row, locked ? $"Level {d.UnlockLevel}" : "Buy", () => company.RequestBuy(index), SoundId.Coins, small: true)
                    .SetEnabled(!locked && money >= d.Price);
            }
        }

        private static void Rack(VisualElement panel, CompanyService company, PlayerEquipment mine)
        {
            MenuKit.Text(panel, "GEAR RACK", "heading");
            if (mine == null) return;
            MenuKit.Text(panel, "Fill your two hand slots from what the company owns.", "subtitle");
            EquipmentCatalog catalog = mine.Catalog;
            for (int slot = 0; slot < 2; slot++)
            {
                EquipmentDefinition held = mine.InSlot(slot);
                MenuKit.Text(panel, $"HAND SLOT {slot + 1} ({InputBindings.Display(slot == 0 ? "HandSlot1" : "HandSlot2")}): {(held != null ? held.DisplayName : "empty")}", "section");
                VisualElement row = MenuKit.Row(panel);
                row.AddToClassList("wrap");
                int s = slot;
                MenuKit.Button(row, "Empty", () => mine.RequestEquip(s, -1), SoundId.UiClick, small: true);
                for (int i = 0; i < catalog.Items.Count; i++)
                {
                    if (company.OwnedCount(i) <= 0) continue;
                    int free = mine.Available(i, mine) - (mine.State[1 - slot] == i ? 1 : 0);
                    int index = i;
                    Button b = MenuKit.Button(row, $"{catalog.Items[i].DisplayName} ({Mathf.Max(0, free)})", () => mine.RequestEquip(s, index), SoundId.UiConfirm, small: true);
                    b.SetEnabled(free > 0 || mine.State[slot] == i);
                    b.EnableInClassList("menu-button--on", mine.State[slot] == i);
                }
            }
            MenuKit.Text(panel, $"{InputBindings.Display("Flashlight")}: flashlight. Hold {InputBindings.Display("Radio")}: radio. " +
                                $"Single-use gear is used with {InputBindings.Display("Use")} when your hands are empty.", "text").AddToClassList("text--small");
        }

        private static PlayerEquipment Mine()
        {
            foreach (PlayerEquipment e in PlayerEquipment.All) if (e != null && e.IsOwner) return e;
            return null;
        }

        private void OnDisable()
        {
            CursorOwner.Set(this, false);
            ShopTerminal.Open = GearRack.Open = false;
            screen?.Show(false);
        }

        private void OnDestroy() => screen?.Remove();
    }
}
