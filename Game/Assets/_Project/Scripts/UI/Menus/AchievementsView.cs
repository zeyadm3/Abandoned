using Abandoned.Core;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>Every achievement with its progress (M9.5), from the main or pause menu.</summary>
    public class AchievementsView
    {
        private readonly VisualElement list;
        private readonly Label summary;

        public VisualElement Root { get; }

        public AchievementsView(MenuUi menu)
        {
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            VisualElement panel = MenuKit.Panel(Root, wide: true);
            MenuKit.Text(panel, "ACHIEVEMENTS", "heading");
            summary = MenuKit.Text(panel, "", "subtitle");
            var scroll = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.AddToClassList("scroll");
            panel.Add(scroll);
            list = scroll;
            MenuKit.Button(panel, "Back", menu.Back, Audio.SoundId.UiBack);
        }

        public void Refresh()
        {
            list.Clear();
            AchievementCatalog catalog = Achievements.Catalog;
            if (catalog == null) return;
            int got = 0;
            foreach (AchievementDefinition a in catalog.Items)
            {
                if (a == null) continue;
                bool unlocked = Achievements.IsUnlocked(a);
                if (unlocked) got++;
                VisualElement row = MenuKit.Row(list);
                row.AddToClassList("hud-row");
                Label name = MenuKit.Text(row, $"<b>{(unlocked ? "<color=#7dff7d>" + a.DisplayName + "</color>" : a.DisplayName)}</b>\n{a.Description}", "text");
                name.AddToClassList("hud-cell--name");
                long progress = System.Math.Min(Achievements.Stat(a.Stat), a.Threshold);
                MenuKit.Text(row, unlocked ? "DONE" : a.Threshold > 1 ? $"{progress:N0} / {a.Threshold:N0}" : "-", "hud-cell");
            }
            summary.text = $"{got} / {catalog.Items.Count} UNLOCKED";
        }
    }
}
