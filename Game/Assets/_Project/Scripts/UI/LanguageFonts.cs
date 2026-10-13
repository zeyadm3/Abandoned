using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Which fallback font answers first (0.24.8). The interface fonts are Latin only, so every other script
    /// comes from the Noto fallbacks on the panel's text settings; Chinese, Japanese and Korean share characters
    /// but draw them differently, so the font of the language in use has to be asked before the other two.
    /// Works on a runtime copy, like the UI scale: the project's asset is never edited.
    /// </summary>
    public static class LanguageFonts
    {
        // Substrings of the fallback font assets' names (Editor/Releases/Release0248), in the order asked.
        private static readonly string[] ChineseFirst = { "NotoSansSC", "NotoSansJP", "NotoSansKR" };
        private static readonly string[] JapaneseFirst = { "NotoSansJP", "NotoSansSC", "NotoSansKR" };
        private static readonly string[] KoreanFirst = { "NotoSansKR", "NotoSansJP", "NotoSansSC" };

        /// <summary>The text settings every panel of the game shares; null without a menu (headless, bare test rigs).</summary>
        public static PanelTextSettings Shared { get; private set; }

        /// <summary>Puts the shared text settings on a panel (the menu document, the truck's board).</summary>
        public static void Apply(PanelSettings panel)
        {
            if (panel == null) return;
            if (Shared == null)
            {
                if (panel.textSettings == null) return;
                Shared = Object.Instantiate(panel.textSettings);
                Shared.name = panel.textSettings.name + " (language)";
                Order();
            }
            panel.textSettings = Shared;
        }

        /// <summary>
        /// Orders the fallbacks for the language in use. A character already drawn keeps the font it was first
        /// found in until the game restarts, so switching between Chinese and Japanese mid-session can leave
        /// some shared characters in the other's shapes.
        /// </summary>
        public static void Order()
        {
            if (Shared == null || Shared.fallbackFontAssets == null) return;
            string[] lead = Languages.Current.Script switch
            {
                TextScript.Japanese => JapaneseFirst,
                TextScript.Korean => KoreanFirst,
                _ => ChineseFirst,
            };
            List<FontAsset> fonts = Shared.fallbackFontAssets;
            fonts.RemoveAll(f => f == null);
            // Stable: Latin/Cyrillic and Arabic keep their places in front, only the three CJK fonts swap.
            var ordered = new List<FontAsset>(fonts.Count);
            foreach (FontAsset f in fonts) if (Rank(f, lead) < 0) ordered.Add(f);
            for (int i = 0; i < lead.Length; i++)
                foreach (FontAsset f in fonts) if (Rank(f, lead) == i) ordered.Add(f);
            Shared.fallbackFontAssets = ordered;
        }

        private static int Rank(FontAsset font, string[] lead)
        {
            for (int i = 0; i < lead.Length; i++) if (font.name.Contains(lead[i])) return i;
            return -1;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Shared = null;
    }
}
