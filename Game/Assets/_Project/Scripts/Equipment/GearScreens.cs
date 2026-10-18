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
            if (open && (CompanyService.Current == null || Core.PadInput.BackPressed))
                ShopTerminal.Open = GearRack.Open = false;
            open = ShopTerminal.Open || GearRack.Open;
            CursorOwner.Set(this, open);

            CompanyService company = CompanyService.Current;
            if (screen == null || screen.Root.panel == null)
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
            for (int i = 0; i < company.Equipment.Items.Count; i++) key.Append('|').Append(company.OwnedCount(i)).Append(':').Append(mine != null ? mine.Available(i, mine) : 0);
            foreach (PlayerEquipment e in PlayerEquipment.All)
                if (e != null)
                    for (int slot = 0; slot < EquipState.SlotCount; slot++) key.Append(';').Append(e.State[slot]);
            if (mine != null) key.Append('|').Append(mine.GetComponent<Interaction.PlayerCarrier>().Inventory.OccupiedCount);
            screen.Build(key.ToString(), panel =>
            {
                if (ShopTerminal.Open)
                {
                    Terminal(panel, company);
                    return;
                }
                Rack(panel, company, mine);
                MenuKit.Button(panel, Loc.T("Close"), () => ShopTerminal.Open = GearRack.Open = false, SoundId.UiBack);
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
            Crt(screenArea, Loc.T("{0} - SUPPLY TERMINAL v2.1", CompanySave.DefaultName.ToUpperInvariant()), "crt__title");
            Crt(screenArea, Loc.T("FUNDS {0}     CLEARANCE LEVEL {1}", (money < 0 ? Loc.T("-${0:N0} (DEBT)", -money) : $"${money:N0}"), company.State.Level), "crt__line");
            Crt(screenArea, new string('=', 72), "crt__rule");
            VisualElement tabs = MenuKit.Row(screenArea);
            CrtButton(tabs, page == 0 ? $"[ {Loc.T("GEAR")} ]" : $"  {Loc.T("GEAR")}  ", () => page = 0).EnableInClassList("crt__button--on", page == 0);
            CrtButton(tabs, page == 1 ? $"[ {Loc.T("TRUCK")} ]" : $"  {Loc.T("TRUCK")}  ", () => page = 1).EnableInClassList("crt__button--on", page == 1);
            Crt(tabs, "", "crt__line").style.flexGrow = 1;
            CrtButton(tabs, Loc.T("[ LOG OFF ]"), () => ShopTerminal.Open = false);
            Crt(screenArea, new string('-', 72), "crt__rule");

            var list = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            list.AddToClassList("crt__list");
            screenArea.Add(list);
            Label about = Crt(screenArea, Loc.T("> POINT AT AN ITEM FOR ITS SPEC SHEET."), "crt__about");
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
                string name = (d.Consumable ? Loc.T("{0} (1 USE)", d.LocalName) : d.LocalName).ToUpperInvariant();
                string right = locked ? Loc.T("LEVEL {0}", d.UnlockLevel) : $"${d.Price:N0}";
                Row(list, about, $"{name}", Loc.T("OWNED {0}", company.OwnedCount(i)), right, d.LocalDescription,
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
                string right = owned ? Loc.T("FITTED") : locked ? Loc.T("LEVEL {0}", d.UnlockLevel) : $"${d.Price:N0}";
                Row(list, about, d.LocalName.ToUpperInvariant(), owned ? "" : Loc.T("UPGRADE"), right, d.LocalDescription,
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
                GameAudio.PlayUi(SoundId.UiHover);
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
            MenuKit.Text(panel, Loc.T("GEAR RACK"), "heading");
            if (mine == null) return;
            MenuKit.Text(panel, Loc.T("Four pockets are shared by tools and small loot."), "subtitle");
            EquipmentCatalog catalog = mine.Catalog;
            for (int slot = 0; slot < EquipState.SlotCount; slot++)
            {
                EquipmentDefinition held = mine.InSlot(slot);
                Interaction.Grabbable pocket = mine.GetComponent<Interaction.PlayerCarrier>().Inventory.AtSlot(slot);
                MenuKit.Text(panel, Loc.T("POCKET {0} ({1}): {2}", slot + 1, InputBindings.Display("HandSlot" + (slot + 1)), held != null ? held.LocalName : pocket != null ? pocket.LocalName : Loc.T("empty")), "section");
                VisualElement row = MenuKit.Row(panel);
                row.AddToClassList("wrap");
                int s = slot;
                MenuKit.Button(row, Loc.T("Empty"), () => mine.RequestEquip(s, -1), SoundId.UiClick, small: true).SetEnabled(pocket == null);
                for (int i = 0; i < catalog.Items.Count; i++)
                {
                    if (company.OwnedCount(i) <= 0) continue;
                    int free = mine.Available(i, mine);
                    for (int other = 0; other < EquipState.SlotCount; other++)
                        if (other != slot && mine.State[other] == i) free--;
                    int index = i;
                    Button b = MenuKit.Button(row, "", () => mine.RequestEquip(s, index), SoundId.UiConfirm, small: true);
                    b.AddToClassList("rack__item");
                    Prepend(b, UiKit.Icon(null, "item/" + catalog.Items[i].Id, "small"));
                    MenuKit.Text(b, $"{catalog.Items[i].LocalName} ({Mathf.Max(0, free)})", "rack__name").pickingMode = PickingMode.Ignore;
                    b.SetEnabled(pocket == null && (free > 0 || mine.State[slot] == i));
                    b.EnableInClassList("menu-button--on", mine.State[slot] == i);
                }
            }
            MenuKit.Text(panel, Loc.T("{0}: flashlight. Hold {1}: radio. Single-use gear is used with {2} when your hands are empty.",
                InputBindings.Display("Flashlight"), InputBindings.Display("Radio"), InputBindings.Display("Use")), "text").AddToClassList("text--small");
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
