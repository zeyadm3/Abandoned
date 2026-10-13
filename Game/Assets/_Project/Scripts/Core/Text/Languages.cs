using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// The languages the game ships in and the one in use (0.24.8). The player's pick in Settings wins; without
    /// one the game follows Steam's language for it, then the computer's, then English. Each machine has its
    /// own language: nothing here is networked, and batch runs (tests, nettests) are always English.
    /// </summary>
    public static class Languages
    {
        public const string English = "en";
        private const string ChoiceKey = "settings.language";

        public static readonly LanguageInfo[] All =
        {
            new(English, "English", TextScript.Latin, PluralRule.OneOther, new[] { "english" }, new[] { SystemLanguage.English }),
            // Traditional Chinese readers get Simplified rather than English until there is a Traditional sheet.
            new("zh-CN", "简体中文", TextScript.Chinese, PluralRule.None, new[] { "schinese", "tchinese" },
                new[] { SystemLanguage.ChineseSimplified, SystemLanguage.Chinese, SystemLanguage.ChineseTraditional }),
            new("ja", "日本語", TextScript.Japanese, PluralRule.None, new[] { "japanese" }, new[] { SystemLanguage.Japanese }),
            new("ko", "한국어", TextScript.Korean, PluralRule.None, new[] { "koreana", "korean" }, new[] { SystemLanguage.Korean }),
            // One neutral Spanish serves both of Steam's: Spain and Latin America.
            new("es", "Español", TextScript.Latin, PluralRule.OneOther, new[] { "spanish", "latam" }, new[] { SystemLanguage.Spanish }),
            new("pt-BR", "Português (Brasil)", TextScript.Latin, PluralRule.ZeroOneOther, new[] { "brazilian", "portuguese" },
                new[] { SystemLanguage.Portuguese }),
            new("de", "Deutsch", TextScript.Latin, PluralRule.OneOther, new[] { "german" }, new[] { SystemLanguage.German }),
            new("fr", "Français", TextScript.Latin, PluralRule.ZeroOneOther, new[] { "french" }, new[] { SystemLanguage.French }),
            new("ru", "Русский", TextScript.Cyrillic, PluralRule.Russian, new[] { "russian" }, new[] { SystemLanguage.Russian }),
            new("pl", "Polski", TextScript.Latin, PluralRule.Polish, new[] { "polish" }, new[] { SystemLanguage.Polish }),
        };

        private static readonly LanguageInfo StretchedText = new("xx", "Stretched text (dev)", TextScript.Latin, PluralRule.OneOther,
            new string[0], new SystemLanguage[0], stretched: true);

        private static LanguageInfo current;
        private static string steamName;

        /// <summary>Raised after the language changed: menus rebuild, fonts reorder.</summary>
        public static event Action Changed;

        /// <summary>Goes up with every change, so a HUD piece can tell its fixed labels were built for another language.</summary>
        public static int Version { get; private set; }

        public static LanguageInfo Current => current ??= Saved() ?? Detect();

        /// <summary>The player picked this language in Settings; otherwise it follows Steam and the computer.</summary>
        public static bool Chosen => Saved() != null;

        /// <summary>What Settings lists. Developer builds add the stretched-text language.</summary>
        public static IEnumerable<LanguageInfo> Offered
        {
            get
            {
                foreach (LanguageInfo language in All) yield return language;
                if (DevTools.Enabled) yield return StretchedText;
            }
        }

        public static void Choose(string code)
        {
            LanguageInfo language = Find(code);
            if (language == null) return;
            Prefs.SetString(ChoiceKey, language.Code);
            Prefs.Save();
            Use(language);
        }

        /// <summary>
        /// Steam's language for this game, once Steam is up ("schinese", "german"...): the player's choice in the
        /// game's Steam properties. It can arrive after the first menu was built, so the language may switch here.
        /// </summary>
        public static void UseSteamLanguage(string apiName)
        {
            steamName = apiName;
            if (current != null && !Chosen) Use(Detect());
        }

        public static LanguageInfo Find(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;
            foreach (LanguageInfo language in All)
                if (string.Equals(language.Code, code, StringComparison.OrdinalIgnoreCase)) return language;
            return DevTools.Enabled && code == StretchedText.Code ? StretchedText : null;
        }

        public static LanguageInfo FromSteam(string apiName)
        {
            if (string.IsNullOrEmpty(apiName)) return null;
            foreach (LanguageInfo language in All)
                foreach (string name in language.SteamNames)
                    if (string.Equals(name, apiName, StringComparison.OrdinalIgnoreCase)) return language;
            return null;
        }

        public static LanguageInfo FromSystem(SystemLanguage system)
        {
            foreach (LanguageInfo language in All)
                foreach (SystemLanguage s in language.SystemLanguages)
                    if (s == system) return language;
            return null;
        }

        // Batch runs keep their choices in memory (Prefs), so they only leave English when a test asks them to.
        private static LanguageInfo Saved() => Find(Prefs.GetString(ChoiceKey, ""));

        private static LanguageInfo Detect() =>
            Application.isBatchMode ? All[0] : FromSteam(steamName) ?? FromSystem(Application.systemLanguage) ?? All[0];

        private static void Use(LanguageInfo next)
        {
            if (next == null || next == current) return;
            current = next;
            Version++;
            TextTable.Reload();
            Changed?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            current = null;
            steamName = null;
            Version = 0;
            Changed = null;
        }
    }
}
