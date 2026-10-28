using System.Linq;
using Abandoned.Audio;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Loot;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The loot catalogue (0.18.7), in the field guide's ledger style: every kind of loot, most valuable
    /// first; the ones the company has sold are filled in with their picture and price range, the rest blank.
    /// </summary>
    public class LootCatalogueScreen : MonoBehaviour
    {
        private ScreenPanel screen;

        private void Update()
        {
            if (LootCatalogueBook.Open && PadInput.BackPressed) LootCatalogueBook.Open = false;
            bool open = LootCatalogueBook.Open;
            CursorOwner.Set(this, open);
            if (screen == null || screen.Root.panel == null)
            {
                if (!open) return;
                if ((screen = ScreenPanel.Create(wide: true)) == null) return;
                screen.Panel.AddToClassList("field-guide");
                screen.Panel.AddToClassList("salvage-catalogue");
                screen.Panel.style.width = 1160f;
                screen.Panel.style.maxWidth = Length.Percent(94f);
            }
            screen.Show(open);
            if (!open) return;
            CompanyService company = CompanyService.Current;
            screen.Build($"catalogue|{(company != null ? company.CatalogueCount : 0)}", Fill);
        }

        private void Fill(VisualElement panel)
        {
            CompanyService company = CompanyService.Current;
            LootCatalog catalog = LootCatalogueBook.Catalog;
            var all = catalog != null ? catalog.Entries.Select(e => e.definition).Where(d => d != null && !d.Utility)
                .OrderByDescending(d => d.Jackpot).ThenByDescending(d => d.ValueMax).ToList() : new System.Collections.Generic.List<LootDefinition>();
            int found = company != null ? all.Count(d => company.Catalogued(d.Id)) : 0;
            Text(panel, Loc.T("SALVAGE ARCHIVE"), "field-guide__cover-title");
            Text(panel, Loc.T("RECOVERED {0:00} / {1:00}  —  Sale records unlock identification. Jackpots go on the trophy shelf.", found, all.Count), "field-guide__cover-note");
            var grid = new VisualElement();
            grid.AddToClassList("salvage-catalogue__grid");
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            panel.Add(grid);
            foreach (LootDefinition d in all)
            {
                bool known = company != null && company.Catalogued(d.Id);
                var cell = new VisualElement();
                cell.AddToClassList("field-guide__entry");
                cell.AddToClassList("salvage-catalogue__item");
                cell.EnableInClassList("field-guide__entry--locked", !known);
                cell.EnableInClassList("salvage-catalogue__item--jackpot", known && d.Jackpot);
                var visual = new VisualElement { pickingMode = PickingMode.Ignore };
                visual.AddToClassList("salvage-catalogue__visual");
                cell.Add(visual);
                if (known) UiKit.Icon(visual, $"loot/{d.Id}", "large");
                else Text(visual, "?", "salvage-catalogue__unknown");
                Text(cell, known ? d.LocalName.ToUpperInvariant() : Loc.T("UNIDENTIFIED SALVAGE"), "field-guide__entry-name");
                Text(cell, known ? $"${d.ValueMin:N0} — ${d.ValueMax:N0}" : Loc.T("NO SALE RECORD"), "salvage-catalogue__value");
                // The carry class as the enum names it. @loc:carry "Pocket", "OneHand", "TwoHand", "Heavy", "Huge"
                Text(cell, known ? d.Jackpot ? Loc.T("HIGH VALUE / TROPHY RECORD") : Loc.T("{0} / RECOVERED", Loc.C("carry", d.CarryClass.ToString()).ToUpperInvariant()) : Loc.T("RECOVER AND SELL TO IDENTIFY"), "salvage-catalogue__status");
                grid.Add(cell);
            }
            var close = new Button(() =>
            {
                GameAudio.PlayUi(SoundId.UiBack);
                LootCatalogueBook.Open = false;
            }) { text = Loc.T("Seal archive") };
            close.AddToClassList("field-guide__close");
            panel.Add(close);
        }

        private static Label Text(VisualElement parent, string text, string cls)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            l.style.whiteSpace = WhiteSpace.Normal;
            parent.Add(l);
            return l;
        }

        private void OnDisable()
        {
            CursorOwner.Set(this, false);
            LootCatalogueBook.Open = false;
            screen?.Show(false);
        }

        private void OnDestroy() => screen?.Remove();
    }
}
