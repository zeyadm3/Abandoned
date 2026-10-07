using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>Who made the game and whose free assets it uses (lines generated from Docs/ASSET_CREDITS.md).</summary>
    public class CreditsView
    {
        public VisualElement Root { get; }

        public CreditsView(MenuUi menu, TextAsset credits)
        {
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            VisualElement panel = MenuKit.Panel(Root, wide: true);
            MenuKit.Text(panel, "CREDITS", "heading");
            MenuKit.Text(panel, "A game by Zeyad Games.", "subtitle");
            var scroll = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.AddToClassList("scroll");
            panel.Add(scroll);
            string text = credits != null ? credits.text : "";
            foreach (string line in text.Split('\n'))
            {
                string t = line.TrimEnd('\r');
                if (t.Length == 0) continue;
                bool header = t.StartsWith("#");
                MenuKit.Text(scroll, header ? t.TrimStart('#', ' ') : t, header ? "section" : "text");
            }
            MenuKit.Button(panel, "Back", menu.Back, Audio.SoundId.UiBack);
        }
    }
}
