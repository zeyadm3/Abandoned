using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>One language the game is written in (0.24.8). The list is <see cref="Languages.All"/>.</summary>
    public sealed class LanguageInfo
    {
        /// <summary>The tag in the text sheets and the saved setting: "en", "pt-BR", "zh-CN".</summary>
        public string Code { get; }

        /// <summary>Its own name in its own script, as Settings lists it; never translated.</summary>
        public string NativeName { get; }

        public TextScript Script { get; }
        public PluralRule Plural { get; }

        /// <summary>Steam's API names for it ("schinese", "koreana"; Spanish has two).</summary>
        public string[] SteamNames { get; }

        public SystemLanguage[] SystemLanguages { get; }

        /// <summary>Developer builds only: English padded and bracketed, to find text that is cut off or never went through <see cref="Loc"/>.</summary>
        public bool Stretched { get; }

        public bool IsEnglish => Code == Languages.English;

        public LanguageInfo(string code, string nativeName, TextScript script, PluralRule plural, string[] steamNames,
            SystemLanguage[] systemLanguages, bool stretched = false)
        {
            Code = code;
            NativeName = nativeName;
            Script = script;
            Plural = plural;
            SteamNames = steamNames;
            SystemLanguages = systemLanguages;
            Stretched = stretched;
        }
    }
}
