using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The HQ wardrobe (GDD 18; redone in 0.12.5; workers and outfits since the 2026 remake), landscape: you on the
    /// left (drag to turn; whatever you select is tried on), workers / outfits / colours / hats / accessories as cards
    /// in the middle, and the selected item
    /// on the right with what it takes: wear it, buy it once with company money (the host checks the funds),
    /// or earn it with an achievement. The crew sees what you wear at once.
    /// </summary>
    public class WardrobeView
    {
        // 2026 remake: who you are and what you wear come first; the old coverall colour now tints the outfit.
        private static readonly string[] Tabs = { "WORKER", "OUTFIT", "COLOUR", "HATS", "ACCESSORIES" }; // @loc
        private static readonly CosmeticKind[] Kinds =
            { CosmeticKind.Character, CosmeticKind.Outfit, CosmeticKind.Coverall, CosmeticKind.Hat, CosmeticKind.Accessory };

        private readonly VisualElement preview, grid, swatch;
        private readonly Label funds, status, title, about, unlock;
        private readonly Button action;
        private readonly List<Button> tabs = new();
        private PlayerCosmetics me;
        private int tab, selected;
        private bool wasPending;

        public VisualElement Root { get; }

        public WardrobeView(MenuUi menu)
        {
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            VisualElement panel = MenuKit.Page(Root, wide: true);
            panel.AddToClassList("wardrobe");

            MenuKit.Text(panel, Loc.T("WORKER KIT / ISSUE AND IDENTIFICATION"), "menu-record");
            VisualElement header = MenuKit.Row(panel);
            header.AddToClassList("wardrobe__header");
            header.style.flexWrap = Wrap.Wrap;
            MenuKit.Text(header, Loc.T("WARDROBE"), "heading").AddToClassList("wardrobe__title");
            var money = new VisualElement();
            money.AddToClassList("wardrobe__money");
            header.Add(money);
            funds = MenuKit.Text(money, "", "wardrobe__funds");
            status = MenuKit.Text(money, "", "wardrobe__status");

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
            DragToTurn(preview);

            var shelf = new VisualElement();
            shelf.AddToClassList("wardrobe__shelf");
            columns.Add(shelf);
            VisualElement bar = MenuKit.Row(shelf);
            bar.AddToClassList("wardrobe__tabs");
            bar.style.flexWrap = Wrap.Wrap;
            for (int i = 0; i < Tabs.Length; i++)
            {
                int index = i;
                Button b = MenuKit.Button(bar, Loc.T(Tabs[i]), () => { tab = index; selected = Worn(); Rebuild(); }, SoundId.UiClick, small: true);
                tabs.Add(b);
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
            action = MenuKit.Button(detail, "", Act, SoundId.UiConfirm);
            action.AddToClassList("wardrobe__action");

            MenuKit.Button(panel, Loc.T("Done"), menu.Back, SoundId.UiBack).AddToClassList("wardrobe__done");
            // Purchases answer from the host a moment later; the money can change while it's open.
            Root.schedule.Execute(Poll).Every(150);
        }

        /// <summary>Opened at the lockers: start on what you're wearing.</summary>
        public void Refresh()
        {
            me = NetworkPlayer.Local != null ? NetworkPlayer.Local.GetComponent<PlayerCosmetics>() : null;
            selected = Worn();
            Rebuild();
        }

        private CosmeticKind Kind => Kinds[tab];

        private IReadOnlyList<CosmeticDefinition> Items => me == null || me.Catalog == null ? new List<CosmeticDefinition>() : me.Catalog.Of(Kind);

        private int Worn() => me == null ? 0 : me.Choice.Of(Kind);

        private CosmeticChoice With(CosmeticChoice c, int index) => c.With(Kind, index);

        private void Rebuild()
        {
            for (int i = 0; i < tabs.Count; i++) tabs[i].EnableInClassList("menu-button--on", i == tab);
            grid.Clear();
            Poll();
            if (me == null || me.Catalog == null)
            {
                title.text = Loc.T("Nobody to dress");
                about.text = Loc.T("The wardrobe works in a game, at the HQ lockers.");
                unlock.text = "";
                MenuKit.Show(action, false);
                return;
            }
            IReadOnlyList<CosmeticDefinition> items = Items;
            int worn = Worn();
            for (int i = 0; i < items.Count; i++)
            {
                CosmeticDefinition d = items[i];
                if (d == null) continue;
                int index = i;
                var card = new Button(() => { selected = index; Rebuild(); }) { text = "" };
                card.AddToClassList("wardrobe__card");
                card.EnableInClassList("wardrobe__card--selected", i == selected);
                card.EnableInClassList("wardrobe__card--locked", !PlayerProfile.Unlocked(d));
                card.RegisterCallback<PointerEnterEvent>(_ => GameAudio.PlayUi(SoundId.UiHover));
                var chip = new VisualElement { pickingMode = PickingMode.Ignore };
                chip.AddToClassList("wardrobe__chip");
                if (Kind == CosmeticKind.Coverall) chip.style.backgroundColor = d.Color;
                else IconChip.Fill(chip, "cosmetic/" + d.Id, d.LocalName);
                card.Add(chip);
                var name = new Label(d.LocalName) { pickingMode = PickingMode.Ignore };
                name.AddToClassList("wardrobe__card-name");
                card.Add(name);
                var tag = new Label(Tag(d, i == worn)) { pickingMode = PickingMode.Ignore };
                tag.AddToClassList("wardrobe__card-tag");
                tag.EnableInClassList("wardrobe__card-tag--reward", d.Unlock == CosmeticUnlock.Reward && !PlayerProfile.Unlocked(d));
                card.Add(tag);
                grid.Add(card);
            }
            ShowDetail();
        }

        private static string Tag(CosmeticDefinition d, bool worn)
        {
            if (worn) return Loc.T("WEARING");
            if (PlayerProfile.Unlocked(d)) return d.Unlock == CosmeticUnlock.Free ? Loc.T("FREE") : Loc.T("OWNED");
            return d.Unlock == CosmeticUnlock.Buy ? $"${d.Price:N0}" : Loc.T("REWARD");
        }

        private void ShowDetail()
        {
            CosmeticDefinition d = me.Definition(Kind, selected);
            if (d == null) return;
            // Try it on: the preview wears the selection even before it's yours.
            preview.style.backgroundImage = Background.FromRenderTexture(WardrobePreview.Show(me.Catalog, With(me.Choice, selected)));
            title.text = d.LocalName;
            about.text = d.LocalDescription;
            swatch.style.backgroundColor = Kind == CosmeticKind.Coverall ? d.Color : new Color(0f, 0f, 0f, 0f);
            bool unlocked = PlayerProfile.Unlocked(d), worn = selected == Worn();
            MenuKit.Show(action, true);
            action.SetEnabled(true);
            if (worn)
            {
                unlock.text = Loc.T("You're wearing it.");
                action.text = Loc.T("Wearing");
                action.SetEnabled(false);
            }
            else if (unlocked)
            {
                unlock.text = d.Unlock == CosmeticUnlock.Reward ? Loc.T("Earned: {0}.", AchievementName(d)) : d.Unlock == CosmeticUnlock.Buy ? Loc.T("Bought. It's yours.") : Loc.T("Free for every crew member.");
                action.text = Loc.T("Wear it");
            }
            else if (d.Unlock == CosmeticUnlock.Buy)
            {
                int money = CompanyService.Current != null ? CompanyService.Current.State.Money : 0;
                unlock.text = CompanyService.Current == null ? Loc.T("${0:N0}, paid by the company at the HQ.", d.Price) : Loc.T("${0:N0} from the company's ${1:N0}.", d.Price, money);
                action.text = me.PurchasePending ? Loc.T("Paying...") : Loc.T("Buy for ${0:N0}", d.Price);
                action.SetEnabled(!me.PurchasePending && CompanyService.Current != null && money >= d.Price);
            }
            else
            {
                AchievementDefinition a = PlayerProfile.Achievement(d.RewardAchievement);
                unlock.text = a != null ? Loc.T("Reward for \"{0}\": {1}", a.LocalName, a.LocalDescription) : Loc.T("A reward for an achievement.");
                action.text = Loc.T("Locked");
                action.SetEnabled(false);
            }
        }

        private static string AchievementName(CosmeticDefinition d) => PlayerProfile.Achievement(d.RewardAchievement)?.LocalName ?? Loc.T("an achievement");

        private void Act()
        {
            CosmeticDefinition d = me != null ? me.Definition(Kind, selected) : null;
            if (d == null) return;
            if (PlayerProfile.Unlocked(d)) me.Wear(With(me.Choice, selected));
            else if (d.Unlock == CosmeticUnlock.Buy) me.RequestBuy(Kind, selected);
            Rebuild();
        }

        private void Poll()
        {
            CompanyService company = CompanyService.Current;
            funds.text = company != null && company.IsSpawned ? Loc.T("COMPANY FUNDS  ${0:N0}", company.State.Money) : Loc.T("COMPANY FUNDS  -");
            status.text = me != null ? me.PurchaseMessage : "";
            bool pending = me != null && me.PurchasePending;
            // The host answered: show what changed (owned, worn, money).
            if (wasPending && !pending) Rebuild();
            wasPending = pending;
        }

        private static void DragToTurn(VisualElement target)
        {
            bool dragging = false;
            target.RegisterCallback<PointerDownEvent>(e => { dragging = true; target.CapturePointer(e.pointerId); });
            target.RegisterCallback<PointerMoveEvent>(e => { if (dragging) WardrobePreview.Turn(e.deltaPosition.x * 0.6f); });
            target.RegisterCallback<PointerUpEvent>(e => { dragging = false; target.ReleasePointer(e.pointerId); });
        }
    }
}
