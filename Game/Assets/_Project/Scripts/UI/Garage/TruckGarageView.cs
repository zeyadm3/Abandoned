using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Company;
using Abandoned.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The HQ truck garage (2026 remake), laid out like the wardrobe: the truck turning on the left (drag to turn; any
    /// fitting you select is tried on), the fittings of one slot as cards, the selection on the right with what it takes:
    /// fit it, take it off, or buy it once with company money (the host checks the funds and the crew's spending rule).
    /// Everyone sees the truck change at once; the company's truck wears it on every job.
    /// </summary>
    public class TruckGarageView
    {
        private static readonly string[] Tabs = { "PAINT", "LIVERY", "BUMPER", "ROOF", "WHEELS", "MUD FLAPS", "ORNAMENT", "HORN" }; // @loc

        private readonly VisualElement preview, grid, swatch;
        private readonly Label funds, title, about, unlock;
        private readonly Button action, remove, listen;
        private readonly List<Button> tabs = new();
        private int tab, selected = -1;
        private ulong shownState;

        public VisualElement Root { get; }

        public TruckGarageView(MenuUi menu)
        {
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            VisualElement panel = MenuKit.Page(Root, wide: true);
            panel.AddToClassList("wardrobe");

            MenuKit.Text(panel, Loc.T("FLEET / COLLECTION VEHICLE 01"), "menu-record");
            VisualElement header = MenuKit.Row(panel);
            header.AddToClassList("wardrobe__header");
            header.style.flexWrap = Wrap.Wrap;
            MenuKit.Text(header, Loc.T("TRUCK GARAGE"), "heading").AddToClassList("wardrobe__title");
            var money = new VisualElement();
            money.AddToClassList("wardrobe__money");
            header.Add(money);
            funds = MenuKit.Text(money, "", "wardrobe__funds");

            VisualElement columns = MenuKit.Row(panel);
            columns.AddToClassList("wardrobe__body");
            columns.style.flexShrink = 1;
            columns.style.alignItems = Align.Stretch;
            var stage = new VisualElement();
            stage.AddToClassList("wardrobe__stage");
            columns.Add(stage);
            preview = new VisualElement();
            preview.AddToClassList("wardrobe__preview");
            stage.Add(preview);
            MenuKit.Text(stage, Loc.T("Drag to turn. Anything you select is tried on."), "wardrobe__hint");
            bool dragging = false;
            preview.RegisterCallback<PointerDownEvent>(e => { dragging = true; preview.CapturePointer(e.pointerId); });
            preview.RegisterCallback<PointerMoveEvent>(e => { if (dragging) TruckGaragePreview.Turn(e.deltaPosition.x * 0.6f); });
            preview.RegisterCallback<PointerUpEvent>(e => { dragging = false; preview.ReleasePointer(e.pointerId); });

            var shelf = new VisualElement();
            shelf.AddToClassList("wardrobe__shelf");
            columns.Add(shelf);
            VisualElement bar = MenuKit.Row(shelf);
            bar.AddToClassList("wardrobe__tabs");
            bar.style.flexWrap = Wrap.Wrap;
            for (int i = 0; i < Tabs.Length; i++)
            {
                int index = i;
                tabs.Add(MenuKit.Button(bar, Loc.T(Tabs[i]), () => { tab = index; selected = Fitted(); Rebuild(); }, SoundId.UiClick, small: true));
            }
            ScrollView scroll = MenuKit.Scroll(shelf, "wardrobe__scroll");
            grid = new VisualElement();
            grid.AddToClassList("wardrobe__grid");
            scroll.Add(grid);

            var detail = new VisualElement();
            detail.AddToClassList("wardrobe__detail");
            columns.Add(detail);
            ScrollView detailBody = MenuKit.Scroll(detail, "wardrobe__detail-body");
            swatch = new VisualElement();
            swatch.AddToClassList("wardrobe__detail-swatch");
            detailBody.Add(swatch);
            title = MenuKit.Text(detailBody, "", "wardrobe__detail-name");
            about = MenuKit.Text(detailBody, "", "wardrobe__detail-about");
            unlock = MenuKit.Text(detailBody, "", "wardrobe__detail-unlock");
            listen = MenuKit.Button(detail, Loc.T("Sound the horn"), () => Listen(), SoundId.UiClick, small: true);
            remove = MenuKit.Button(detail, Loc.T("Take it off"), () => Company?.RequestFitTruckCosmetic(Slot, -1), SoundId.UiClick, small: true);
            action = MenuKit.Button(detail, "", Act, SoundId.UiConfirm);
            action.AddToClassList("wardrobe__action");

            MenuKit.Button(panel, Loc.T("Done"), menu.Back, SoundId.UiBack).AddToClassList("wardrobe__done");
            Root.schedule.Execute(Poll).Every(150);
        }

        private static CompanyService Company => CompanyService.Current;
        private static TruckCosmeticCatalog Catalog => TruckCosmeticCatalog.Load();
        private TruckCosmeticSlot Slot => (TruckCosmeticSlot)tab;

        /// <summary>Opened at the garage desk: start on what is fitted.</summary>
        public void Refresh()
        {
            selected = Fitted();
            Rebuild();
        }

        private int Fitted() => Company != null ? Company.FittedTruckCosmetic(Slot) : -1;

        private List<int> Items()
        {
            var list = new List<int>();
            if (Catalog == null) return list;
            for (int i = 0; i < Catalog.Items.Count; i++)
                if (Catalog.Items[i] != null && Catalog.Items[i].Slot == Slot) list.Add(i);
            return list;
        }

        private void Rebuild()
        {
            for (int i = 0; i < tabs.Count; i++) tabs[i].EnableInClassList("menu-button--on", i == tab);
            grid.Clear();
            Poll();
            if (Company == null || Catalog == null)
            {
                title.text = Loc.T("No truck to kit out");
                about.text = Loc.T("The garage works at the HQ, in a company game.");
                unlock.text = "";
                MenuKit.Show(action, false);
                MenuKit.Show(remove, false);
                MenuKit.Show(listen, false);
                return;
            }
            int fitted = Fitted();
            foreach (int index in Items())
            {
                TruckCosmeticDefinition d = Catalog.At(index);
                var card = new Button(() => { selected = index; Rebuild(); }) { text = "" };
                card.AddToClassList("wardrobe__card");
                card.EnableInClassList("wardrobe__card--selected", index == selected);
                card.EnableInClassList("wardrobe__card--locked", !Company.OwnsTruckCosmetic(index));
                card.RegisterCallback<PointerEnterEvent>(_ => GameAudio.PlayUi(SoundId.UiHover));
                var chip = new VisualElement { pickingMode = PickingMode.Ignore };
                chip.AddToClassList("wardrobe__chip");
                if (Slot == TruckCosmeticSlot.Paint) chip.style.backgroundColor = d.Color;
                else chip.Add(new Label(d.LocalName.Substring(0, 1)) { pickingMode = PickingMode.Ignore });
                card.Add(chip);
                var name = new Label(d.LocalName) { pickingMode = PickingMode.Ignore };
                name.AddToClassList("wardrobe__card-name");
                card.Add(name);
                var tag = new Label(Tag(d, index, index == fitted)) { pickingMode = PickingMode.Ignore };
                tag.AddToClassList("wardrobe__card-tag");
                card.Add(tag);
                grid.Add(card);
            }
            ShowDetail();
        }

        private static string Tag(TruckCosmeticDefinition d, int index, bool fitted)
        {
            if (fitted) return Loc.T("FITTED");
            if (Company.OwnsTruckCosmetic(index)) return d.Stock ? Loc.T("STOCK") : Loc.T("OWNED");
            return d.UnlockLevel > Company.State.Level ? Loc.T("LEVEL {0}", d.UnlockLevel) : $"${d.Price:N0}";
        }

        private void ShowDetail()
        {
            TruckCosmeticDefinition d = Catalog.At(selected);
            // Try it on: the preview truck wears the selection even before the company owns it.
            CompanyNetState state = Company.State;
            if (d != null)
            {
                ulong mask = ~(0xFFUL << ((int)Slot * 8));
                state.TruckLook = (state.TruckLook & mask) | ((ulong)(selected + 1) << ((int)Slot * 8));
            }
            preview.style.backgroundImage = Background.FromRenderTexture(TruckGaragePreview.Show(state, Company.TruckUpgrades));
            MenuKit.Show(listen, Slot == TruckCosmeticSlot.Horn && d != null);
            int fitted = Fitted();
            bool optional = Catalog.StockOf(Slot) < 0;
            MenuKit.Show(remove, optional && fitted >= 0);
            if (d == null)
            {
                title.text = Loc.T("Nothing fitted");
                about.text = Loc.T("Pick something on the left to try it on the truck.");
                unlock.text = "";
                MenuKit.Show(action, false);
                swatch.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
                return;
            }
            title.text = d.LocalName;
            about.text = d.LocalDescription;
            swatch.style.backgroundColor = Slot == TruckCosmeticSlot.Paint ? d.Color : new Color(0f, 0f, 0f, 0f);
            MenuKit.Show(action, true);
            action.SetEnabled(true);
            bool owned = Company.OwnsTruckCosmetic(selected);
            if (selected == fitted)
            {
                unlock.text = Loc.T("It's on the truck.");
                action.text = Loc.T("Fitted");
                action.SetEnabled(false);
            }
            else if (owned)
            {
                unlock.text = d.Stock ? Loc.T("Came with the truck.") : Loc.T("Bought. The company owns it.");
                action.text = Loc.T("Fit it");
            }
            else if (d.UnlockLevel > Company.State.Level)
            {
                unlock.text = Loc.T("The garage stocks it from company level {0}.", d.UnlockLevel);
                action.text = Loc.T("Locked");
                action.SetEnabled(false);
            }
            else
            {
                int money = Company.State.Money;
                unlock.text = Loc.T("${0:N0} from the company's ${1:N0}.", d.Price, money);
                action.text = Loc.T("Buy for ${0:N0}", d.Price);
                action.SetEnabled(money >= d.Price);
            }
        }

        private void Act()
        {
            TruckCosmeticDefinition d = Catalog != null ? Catalog.At(selected) : null;
            if (d == null || Company == null) return;
            if (Company.OwnsTruckCosmetic(selected)) Company.RequestFitTruckCosmetic(Slot, selected);
            else Company.RequestBuyTruckCosmetic(selected);
        }

        private void Listen()
        {
            TruckCosmeticDefinition d = Catalog != null ? Catalog.At(selected) : null;
            if (d != null) GameAudio.PlayUi(d.Horn);
        }

        private void Poll()
        {
            CompanyService company = Company;
            funds.text = company != null && company.IsSpawned ? Loc.T("COMPANY FUNDS  ${0:N0}", company.State.Money) : Loc.T("COMPANY FUNDS  -");
            // The host answered (bought, fitted, money): show what changed.
            ulong key = company != null ? company.State.TruckLook ^ company.State.TruckCosmetics ^ (ulong)company.State.Money : 0UL;
            if (key != shownState)
            {
                shownState = key;
                if (company != null) Rebuild();
            }
        }
    }
}
