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
            VisualElement panel = MenuKit.Page(Root, wide: true);
            panel.AddToClassList("achievements");
            MenuKit.Text(panel, Loc.T("RECOVERED RECORDS / WORKER HISTORY"), "menu-record");
            MenuKit.Text(panel, Loc.T("ACHIEVEMENTS"), "heading");
            summary = MenuKit.Text(panel, "", "subtitle");
            var scroll = MenuKit.Scroll(panel);
            list = scroll;
            MenuKit.Button(panel, Loc.T("Back"), menu.Back, Audio.SoundId.UiBack);
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
                row.AddToClassList("achievement-row");
                row.EnableInClassList("achievement-row--earned", unlocked);
                // 0.24.3: its icon, grey until earned.
                VisualElement icon = UiKit.Icon(row, $"achievement/{a.Id}", "large");
                icon.AddToClassList("achievement-row__icon");
                if (!unlocked) icon.style.unityBackgroundImageTintColor = new UnityEngine.Color(0.45f, 0.45f, 0.45f, 0.7f);
                var details = new VisualElement();
                details.AddToClassList("achievement-row__details");
                row.Add(details);
                MenuKit.Text(details, a.LocalName, "achievement-row__name");
                MenuKit.Text(details, a.LocalDescription, "text");
                long progress = System.Math.Min(Achievements.Stat(a.Stat), a.Threshold);
                MenuKit.Text(row, unlocked ? Loc.T("EARNED") : a.Threshold > 1 ? $"{progress:N0} / {a.Threshold:N0}" : Loc.T("LOCKED"), "hud-cell").AddToClassList("achievement-row__status");
            }
            summary.text = Loc.T("{0} / {1} UNLOCKED", got, catalog.Items.Count);
            // 0.19.10: the stats page, below the list.
            MenuKit.Text(list, Loc.T("STATS"), "section");
            foreach ((string stat, string label) in Achievements.StatLabels)
            {
                VisualElement row = MenuKit.Row(list);
                row.AddToClassList("hud-row");
                MenuKit.Text(row, Loc.T(label), "text").AddToClassList("hud-cell--name");
                MenuKit.Text(row, Achievements.Stat(stat).ToString("N0"), "hud-cell");
            }
        }
    }
}
