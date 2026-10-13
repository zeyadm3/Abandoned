using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// What code calls to word something for the player (0.24.8). The English stays in the code and is the key:
    /// <c>Loc.T("Back")</c> gives "Zurück" in German, and "Back" wherever a line has no translation yet. English
    /// itself never reads the sheets, so it shows exactly what the code says. Values go in as {0}, {1}:
    /// <c>Loc.T("Saved to {0}", file)</c>, so a translation can move them. Sheets: <see cref="TextTable"/>.
    /// </summary>
    public static class Loc
    {
        private static readonly HashSet<string> missing = new();
        private static int missingFor = -1;

        /// <summary>English lines asked for in this language that no sheet translates (developer builds; the F1 text line).</summary>
        public static IReadOnlyCollection<string> Missing => missing;

        public static string T(string english)
        {
            if (string.IsNullOrEmpty(english)) return english;
            LanguageInfo language = Languages.Current;
            if (language.IsEnglish) return english;
            if (language.Stretched) return Stretch(english);
            return TextTable.Get(english) ?? Miss(english);
        }

        public static string T(string english, params object[] values) => Fill(T(english), english, values);

        /// <summary>
        /// By id instead of by its English, for a line whose English alone is ambiguous or lives in the sheet
        /// (the tips). <paramref name="english"/> may be null: then the sheet's own English is the fallback.
        /// </summary>
        public static string K(string key, string english)
        {
            LanguageInfo language = Languages.Current;
            string text = TextTable.Get(key) ?? english;
            return language.Stretched && text != null ? Stretch(text) : text;
        }

        public static string K(string key, string english, params object[] values) => Fill(K(key, english), english, values);

        /// <summary>
        /// A word that means different things in different places: <c>Loc.C("size", "Medium")</c> and
        /// <c>Loc.C("quality", "Medium")</c> are two lines in the sheets ("key: size.Medium").
        /// </summary>
        public static string C(string context, string english) => K(context + "." + english, english);

        /// <summary>
        /// A counted line: <c>Loc.N(n, "{0} item", "{0} items")</c>. {0} is the count, <paramref name="more"/>
        /// follow as {1}, {2}. The sheet's English line is the two forms joined by " | ".
        /// </summary>
        public static string N(long count, string one, string other, params object[] more)
        {
            LanguageInfo language = Languages.Current;
            string english = count == 1 ? one : other, form = english;
            if (language.Stretched) form = Stretch(english);
            else if (!language.IsEnglish)
            {
                string key = one + Plurals.Separator + other;
                string forms = TextTable.Get(key);
                if (forms != null) form = Plurals.Pick(forms, language.Plural, count);
                else Miss(key);
            }
            var values = new object[1 + (more?.Length ?? 0)];
            values[0] = count;
            more?.CopyTo(values, 1);
            return Fill(form, english, values);
        }

        private const char WireSeparator = '\u001f', WordMark = '\u001e';

        /// <summary>
        /// English with its values, for text that leaves this machine or is kept for later (a refusal the host
        /// sends, a kick reason): the receiver words it with <see cref="FromWire"/>. Values travel as text, so
        /// format numbers before passing them.
        /// </summary>
        public static string Wire(string english, params object[] values) =>
            values == null || values.Length == 0 ? english : english + WireSeparator + string.Join(WireSeparator.ToString(), values);

        /// <summary>Words text made by <see cref="Wire"/> or <see cref="Mark"/> in this machine's language. Plain text passes through.</summary>
        public static string FromWire(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.IndexOf(WireSeparator) < 0) return T(text);
            string[] parts = text.Split(WireSeparator);
            var values = new object[parts.Length - 1];
            for (int i = 1; i < parts.Length; i++)
                values[i - 1] = parts[i].Length > 0 && parts[i][0] == WordMark ? T(parts[i].Substring(1)) : parts[i];
            return T(parts[0], values);
        }

        /// <summary>A value for <see cref="Wire"/> that is itself text to translate (an item's English name), not a number or a player's name.</summary>
        public static string Word(string english) => string.IsNullOrEmpty(english) ? english : WordMark + english;

        /// <summary>
        /// Leaves the text as it is and only marks it for the sheets: English the host sends, or that is kept in
        /// data, and that each machine words for itself where it is shown (<see cref="T(string)"/> there).
        /// </summary>
        public static string Mark(string english) => english;

        // A translation with a broken placeholder must never throw into the screen that asked.
        private static string Fill(string format, string english, object[] values)
        {
            if (format == null || values == null || values.Length == 0) return format;
            try { return string.Format(format, values); }
            catch (FormatException)
            {
                try { return string.Format(english ?? format, values); }
                catch (FormatException) { return english ?? format; }
            }
        }

        private static string Miss(string english)
        {
            if (!DevTools.Enabled) return english;
            if (missingFor != Languages.Version)
            {
                missing.Clear();
                missingFor = Languages.Version;
            }
            if (missing.Count < 2000) missing.Add(english);
            return english;
        }

        // About a third longer, as German and Russian run, and bracketed so unconverted text stands out.
        private static string Stretch(string english) => "[" + english + " " + new string('~', (english.Length + 2) / 3) + "]";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            missing.Clear();
            missingFor = -1;
        }
    }
}
