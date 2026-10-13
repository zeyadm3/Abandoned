using Abandoned.Core;
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
            MenuKit.Text(panel, Loc.T("ARCHIVE / THOSE BEHIND THE BUILDING"), "menu-record");
            MenuKit.Text(panel, Loc.T("CREDITS"), "heading");
            MenuKit.Text(panel, Loc.T("A game by Zeyad Games."), "subtitle");
            var scroll = MenuKit.Scroll(panel);
            string text = credits != null ? credits.text : "";
            foreach (string line in text.Split('\n'))
            {
                string t = line.TrimEnd('\r');
                if (t.Length == 0) continue;
                bool header = t.StartsWith("#");
                // The entries name their makers and licences, so they stay as written; the headings are worded.
                // @loc "ART, SOUND AND FONTS", "TECHNOLOGY"
                MenuKit.Text(scroll, header ? Loc.T(t.TrimStart('#', ' ')) : t, header ? "section" : "text");
            }
            MenuKit.Button(panel, Loc.T("Back"), menu.Back, Audio.SoundId.UiBack);
        }
    }
}
