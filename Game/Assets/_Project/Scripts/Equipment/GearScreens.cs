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
        // The terminal's page (UI step 8): 0 = gear, 1 = the truck.
        private static int page;

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
            screen.Panel.EnableInClassList("crt", ShopTerminal.Open);

            PlayerEquipment mine = Mine();
            key.Clear().Append(ShopTerminal.Open ? "shop" : "rack").Append(page).Append('|').Append(company.State.Money).Append('|').Append(company.State.Level)
                .Append('|').Append(company.State.TruckUpgrades);
            for (int i = 0; i < company.Equipment.Items.Count; i++) key.Append('|').Append(company.OwnedCount(i));
            if (mine != null) key.Append('|').Append(mine.State[0]).Append(',').Append(mine.State[1]);
            foreach (PlayerEquipment e in PlayerEquipment.All) if (e != null) key.Append(';').Append(e.State[0]).Append(',').Append(e.State[1]);
            screen.Build(key.ToString(), panel =>
            {
                if (ShopTerminal.Open)
                {
                    Terminal(panel, company);
                    return;
                }
                Rack(panel, company, mine);
                MenuKit.Button(panel, "Close", () => ShopTerminal.Open = GearRack.Open = false, SoundId.UiBack);
            });
        }

        // ---- The supply terminal (UI step 8): an amber CRT you click, Lethal Company-style but no typing ----

        private static void Terminal(VisualElement panel, CompanyService company)
        {
            var screenArea = new VisualElement();
            screenArea.AddToClassList("crt__screen");
            panel.Add(screenArea);
            var lines = new VisualElement { pickingMode = PickingMode.Ignore };
            lines.AddToClassList("crt__scanlines");
            lines.style.backgroundImage = Scanlines();

            int money = company.State.Money;
            Crt(screenArea, "ZEYAD SALVAGE CO. - SUPPLY TERMINAL v2.1", "crt__title");
            Crt(screenArea, $"FUNDS {(money < 0 ? $"-${-money:N0} (DEBT)" : $"${money:N0}")}     CLEARANCE LEVEL {company.State.Level}", "crt__line");
            Crt(screenArea, new string('=', 72), "crt__rule");
            VisualElement tabs = MenuKit.Row(screenArea);
            CrtButton(tabs, page == 0 ? "[ GEAR ]" : "  GEAR  ", () => page = 0).EnableInClassList("crt__button--on", page == 0);
            CrtButton(tabs, page == 1 ? "[ TRUCK ]" : "  TRUCK  ", () => page = 1).EnableInClassList("crt__button--on", page == 1);
            Crt(tabs, "", "crt__line").style.flexGrow = 1;
            CrtButton(tabs, "[ LOG OFF ]", () => ShopTerminal.Open = false);
            Crt(screenArea, new string('-', 72), "crt__rule");

            var list = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            list.AddToClassList("crt__list");
            screenArea.Add(list);
            Label about = Crt(screenArea, "> POINT AT AN ITEM FOR ITS SPEC SHEET.", "crt__about");
            if (page == 0) GearPage(list, about, company);
            else TruckPage(list, about, company);
            Crt(screenArea, "_", "crt__cursor");
            screenArea.Add(lines);
        }

        private static void GearPage(VisualElement list, Label about, CompanyService company)
        {
            EquipmentCatalog catalog = company.Equipment;
            int money = company.State.Money;
            for (int i = 0; i < catalog.Items.Count; i++)
            {
                EquipmentDefinition d = catalog.Items[i];
                bool locked = d.UnlockLevel > company.State.Level;
                int index = i;
                string name = (d.DisplayName + (d.Consumable ? " (1 USE)" : "")).ToUpperInvariant();
                string right = locked ? $"LEVEL {d.UnlockLevel}" : $"${d.Price:N0}";
                Row(list, about, $"{name}", $"OWNED {company.OwnedCount(i)}", right, d.Description,
                    locked || money < d.Price ? null : () => company.RequestBuy(index), locked);
            }
        }

        private static void TruckPage(VisualElement list, Label about, CompanyService company)
        {
            TruckUpgradeCatalog upgrades = company.TruckUpgrades;
            if (upgrades == null) return;
            int money = company.State.Money;
            for (int i = 0; i < upgrades.Items.Count; i++)
            {
                TruckUpgradeDefinition d = upgrades.Items[i];
                if (d == null) continue;
                bool owned = company.OwnsUpgrade(i);
                TruckUpgradeDefinition previous = upgrades.Previous(d);
                if (!owned && previous != null && !company.OwnsUpgrade(upgrades.IndexOf(previous))) continue;
                int index = i;
                bool locked = !owned && d.UnlockLevel > company.State.Level;
                string right = owned ? "FITTED" : locked ? $"LEVEL {d.UnlockLevel}" : $"${d.Price:N0}";
                Row(list, about, d.DisplayName.ToUpperInvariant(), owned ? "" : "UPGRADE", right, d.Description,
                    owned || !company.UpgradeAvailable(index) || money < d.Price ? null : () => company.RequestBuyUpgrade(index), locked);
            }
        }

        // One line of stock: name, a note, the price; clickable when it can be bought; hover shows the spec.
        private static void Row(VisualElement list, Label about, string name, string note, string price, string spec, System.Action buy, bool locked)
        {
            var row = new Button(() =>
            {
                if (buy == null) { GameAudio.PlayUi(SoundId.UiError); return; }
                GameAudio.PlayUi(SoundId.Coins);
                buy();
            });
            row.AddToClassList("crt__row");
            row.EnableInClassList("crt__row--locked", locked);
            row.EnableInClassList("crt__row--cant", buy == null && !locked);
            Crt(row, "> " + name, "crt__name");
            Crt(row, note, "crt__note");
            Crt(row, price, "crt__price");
            row.RegisterCallback<PointerEnterEvent>(_ =>
            {
                about.text = "> " + spec.ToUpperInvariant();
                GameAudio.PlayUi(SoundId.UiClick, 0.2f);
            });
            list.Add(row);
        }

        private static Label Crt(VisualElement parent, string text, string cls)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            parent.Add(l);
            return l;
        }

        private static Button CrtButton(VisualElement parent, string text, System.Action click)
        {
            var b = new Button(() =>
            {
                GameAudio.PlayUi(SoundId.UiClick);
                click();
            }) { text = text };
            b.AddToClassList("crt__button");
            parent.Add(b);
            return b;
        }

        private static Texture2D scanlines;

        // Dark lines every other pixel row, repeated over the screen.
        private static Texture2D Scanlines()
        {
            if (scanlines != null) return scanlines;
            scanlines = new Texture2D(1, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            scanlines.SetPixels(new[] { new Color(0f, 0f, 0f, 0.28f), new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 0.28f), new Color(0f, 0f, 0f, 0f) });
            scanlines.Apply();
            return scanlines;
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
                    Button b = MenuKit.Button(row, "", () => mine.RequestEquip(s, index), SoundId.UiConfirm, small: true);
                    b.AddToClassList("rack__item");
                    Prepend(b, UiKit.Icon(null, "item/" + catalog.Items[i].Id, "small"));
                    MenuKit.Text(b, $"{catalog.Items[i].DisplayName} ({Mathf.Max(0, free)})", "rack__name").pickingMode = PickingMode.Ignore;
                    b.SetEnabled(free > 0 || mine.State[slot] == i);
                    b.EnableInClassList("menu-button--on", mine.State[slot] == i);
                }
            }
            MenuKit.Text(panel, $"{InputBindings.Display("Flashlight")}: flashlight. Hold {InputBindings.Display("Radio")}: radio. " +
                                $"Single-use gear is used with {InputBindings.Display("Use")} when your hands are empty.", "text").AddToClassList("text--small");
        }

        private static VisualElement Prepend(VisualElement button, VisualElement icon)
        {
            icon.AddToClassList("rack__icon");
            button.Insert(0, icon);
            return icon;
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
