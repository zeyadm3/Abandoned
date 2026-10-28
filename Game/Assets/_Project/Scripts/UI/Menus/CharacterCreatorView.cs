using System;
using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The character creator after "you're hired" (2026 remake), full screen and offline: you on the left (drag to
    /// turn), who you are, what you wear, its colour, a hat and an accessory on the right, from what is free to every
    /// new hire. Signing on writes the player's profile (as the wardrobe does); the HQ lockers change it later.
    /// </summary>
    public class CharacterCreatorView
    {
        private static readonly string[] Tabs = { "WORKER", "OUTFIT", "COLOUR", "HAT", "ACCESSORY" }; // @loc
        private static readonly CosmeticKind[] Kinds =
            { CosmeticKind.Character, CosmeticKind.Outfit, CosmeticKind.Coverall, CosmeticKind.Hat, CosmeticKind.Accessory };

        private readonly VisualElement preview, grid;
        private readonly Label title, about;
        private readonly List<Button> tabs = new();
        private CosmeticCatalog catalog;
        private CosmeticChoice choice;
        private Action signed;
        private int tab;

        public VisualElement Root { get; }

        public CharacterCreatorView(MenuUi menu)
        {
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("creator");
            Root.style.backgroundColor = new Color(.035f, .036f, .04f, 1f);
            VisualElement page = MenuKit.Page(Root, wide: true);
            page.AddToClassList("creator__page");
            MenuKit.Text(page, Loc.T("WHO ARE YOU?"), "heading");
            MenuKit.Text(page, Loc.T("Pick who you are and what you wear on the job. You can change it at the HQ lockers."), "subtitle");

            VisualElement columns = MenuKit.Row(page);
            columns.AddToClassList("wardrobe__body");
            columns.style.flexShrink = 1;
            columns.style.alignItems = Align.Stretch;
            var stage = new VisualElement();
            stage.AddToClassList("wardrobe__stage");
            columns.Add(stage);
            preview = new VisualElement();
            preview.AddToClassList("wardrobe__preview");
            stage.Add(preview);
            MenuKit.Text(stage, Loc.T("Drag to turn."), "wardrobe__hint");
            bool dragging = false;
            preview.RegisterCallback<PointerDownEvent>(e => { dragging = true; preview.CapturePointer(e.pointerId); });
            preview.RegisterCallback<PointerMoveEvent>(e => { if (dragging) WardrobePreview.Turn(e.deltaPosition.x * .6f); });
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
                tabs.Add(MenuKit.Button(bar, Loc.T(Tabs[i]), () => { tab = index; Rebuild(); }, SoundId.UiClick, small: true));
            }
            ScrollView scroll = MenuKit.Scroll(shelf, "wardrobe__scroll");
            grid = new VisualElement();
            grid.AddToClassList("wardrobe__grid");
            scroll.Add(grid);
            title = MenuKit.Text(shelf, "", "wardrobe__detail-name");
            about = MenuKit.Text(shelf, "", "wardrobe__detail-about");

            MenuKit.Button(page, Loc.T("Sign on"), Sign, SoundId.UiConfirm, important: true).AddToClassList("creator__sign");
        }

        public void Open(CosmeticCatalog cosmetics, Action done)
        {
            catalog = cosmetics;
            signed = done;
            choice = catalog != null ? catalog.FromProfile() : default;
            tab = 0;
            Rebuild();
        }

        private CosmeticKind Kind => Kinds[tab];

        private void Rebuild()
        {
            for (int i = 0; i < tabs.Count; i++) tabs[i].EnableInClassList("menu-button--on", i == tab);
            grid.Clear();
            if (catalog == null) return;
            IReadOnlyList<CosmeticDefinition> items = catalog.Of(Kind);
            int worn = choice.Of(Kind);
            for (int i = 0; i < items.Count; i++)
            {
                CosmeticDefinition d = items[i];
                // A new hire starts with what is free; the rest is bought or earned at the HQ wardrobe.
                if (d == null || !PlayerProfile.Unlocked(d)) continue;
                int index = i;
                var card = new Button(() => { choice = choice.With(Kind, index); Rebuild(); }) { text = "" };
                card.AddToClassList("wardrobe__card");
                card.EnableInClassList("wardrobe__card--selected", i == worn);
                card.RegisterCallback<PointerEnterEvent>(_ => GameAudio.PlayUi(SoundId.UiHover));
                var chip = new VisualElement { pickingMode = PickingMode.Ignore };
                chip.AddToClassList("wardrobe__chip");
                if (Kind == CosmeticKind.Coverall) chip.style.backgroundColor = d.Color;
                else IconChip.Fill(chip, "cosmetic/" + d.Id, d.LocalName);
                card.Add(chip);
                var name = new Label(d.LocalName) { pickingMode = PickingMode.Ignore };
                name.AddToClassList("wardrobe__card-name");
                card.Add(name);
                grid.Add(card);
            }
            CosmeticDefinition shown = catalog.Get(Kind, worn);
            title.text = shown != null ? shown.LocalName : "";
            about.text = shown != null ? shown.LocalDescription : "";
            preview.style.backgroundImage = Background.FromRenderTexture(WardrobePreview.Show(catalog, choice));
        }

        private void Sign()
        {
            if (catalog != null)
                PlayerProfile.Wear(catalog.Coverall(choice.Coverall)?.Id, catalog.Hat(choice.Hat)?.Id, catalog.Accessory(choice.Accessory)?.Id,
                    catalog.Character(choice.Character)?.Id ?? "", catalog.Outfit(choice.Outfit)?.Id ?? "");
            WardrobePreview.Hide();
            Action done = signed;
            signed = null;
            done?.Invoke();
        }
    }
}
