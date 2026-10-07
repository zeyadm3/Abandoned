using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Networking;
using Abandoned.Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The wardrobe (GDD 18): pick a coverall colour, a hat and an accessory; locked ones say what unlocks them.
    /// The crew sees the change at once. Built from the local player's catalog when opened.
    /// </summary>
    public class WardrobeView
    {
        private readonly MenuUi menu;
        private readonly VisualElement suits, hats, extras, preview;
        private readonly Label progress;

        public VisualElement Root { get; }

        public WardrobeView(MenuUi menu)
        {
            this.menu = menu;
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            VisualElement panel = MenuKit.Panel(Root, wide: true);
            panel.AddToClassList("panel--xwide");
            MenuKit.Text(panel, "WARDROBE", "heading");
            progress = MenuKit.Text(panel, "", "subtitle");
            // UI step 8: you, turning slowly, on the left; the choices on the right.
            VisualElement columns = MenuKit.Row(panel);
            columns.AddToClassList("wardrobe__columns");
            preview = new VisualElement { pickingMode = PickingMode.Ignore };
            preview.AddToClassList("wardrobe__preview");
            columns.Add(preview);
            var scroll = MenuKit.Scroll(columns, "wardrobe__choices");
            MenuKit.Text(scroll, "COVERALLS", "section");
            suits = new VisualElement();
            suits.AddToClassList("wrap");
            scroll.Add(suits);
            MenuKit.Text(scroll, "HATS", "section");
            hats = new VisualElement();
            hats.AddToClassList("wrap");
            scroll.Add(hats);
            MenuKit.Text(scroll, "ACCESSORIES", "section");
            extras = new VisualElement();
            extras.AddToClassList("wrap");
            scroll.Add(extras);
            MenuKit.Button(panel, "Done", menu.Back, SoundId.UiBack);
        }

        /// <summary>Rebuilt on open: unlocks may have changed since.</summary>
        public void Refresh()
        {
            PlayerCosmetics me = NetworkPlayer.Local != null ? NetworkPlayer.Local.GetComponent<PlayerCosmetics>() : null;
            suits.Clear();
            hats.Clear();
            extras.Clear();
            progress.text = $"Runs {PlayerProfile.Runs}   made it out {PlayerProfile.Escapes}   lifetime haul ${PlayerProfile.Haul:N0}";
            if (me == null || me.Catalog == null) return;
            CosmeticChoice now = me.Choice;
            preview.style.backgroundImage = Background.FromRenderTexture(WardrobePreview.Show(me.Catalog, now));
            Fill(suits, me.Catalog.Coveralls, now.Coverall, i => me.Wear(me.Choice.WithCoverall(i)), swatch: true);
            Fill(hats, me.Catalog.Hats, now.Hat, i => me.Wear(me.Choice.WithHat(i)), swatch: false);
            Fill(extras, me.Catalog.Accessories, now.Accessory, i => me.Wear(me.Choice.WithAccessory(i)), swatch: false);
        }

        private void Fill(VisualElement parent, IReadOnlyList<CosmeticDefinition> items, int worn, System.Action<int> wear, bool swatch)
        {
            for (int i = 0; i < items.Count; i++)
            {
                CosmeticDefinition d = items[i];
                if (d == null) continue;
                bool unlocked = PlayerProfile.Unlocked(d);
                int index = i;
                Button b = MenuKit.Button(parent, "", () =>
                {
                    wear(index);
                    Refresh();
                }, SoundId.UiConfirm, small: true);
                b.AddToClassList("tile");
                b.SetEnabled(unlocked);
                b.EnableInClassList("menu-button--on", i == worn);
                if (swatch)
                {
                    var chip = new VisualElement { pickingMode = PickingMode.Ignore };
                    chip.AddToClassList("swatch");
                    chip.style.backgroundColor = d.Color;
                    b.Add(chip);
                }
                // Text as its own label beside the swatch (a button's own text would run under it).
                var text = new Label(unlocked ? d.DisplayName : $"{d.DisplayName}\n{d.Requirement(PlayerProfile.Runs, PlayerProfile.Escapes, PlayerProfile.Haul)}")
                    { pickingMode = PickingMode.Ignore };
                text.AddToClassList("tile__text");
                b.Add(text);
            }
        }
    }
}
