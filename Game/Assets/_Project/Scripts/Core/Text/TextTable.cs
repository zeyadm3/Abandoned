using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// The translated text of the language in use (0.19.9; every language since 0.24.8). Each sheet in
    /// Resources/Text is a list of blocks, one line per language, split by a blank line:
    /// <code>
    /// en: Back
    /// de: Zurück
    /// ja: 戻る
    /// </code>
    /// The English line is what code asks for (<see cref="Loc.T(string)"/>), so a line with no translation simply
    /// stays English and never shows a key. A block may start with "key: some.id" when code asks by id instead
    /// (the tips, "hint.&lt;HintId&gt;"). "#" starts a comment; "\n" is a line break. Format and checks:
    /// Docs/LOCALIZATION_0.24.8.md and Tools/Text/check.py.
    /// </summary>
    public static class TextTable
    {
        public const string Folder = "Text";
        private const string KeyTag = "key";

        private static Dictionary<string, string> table;

        /// <summary>Lines loaded for the language in use (the F1 text line).</summary>
        public static int Count => Table.Count;

        private static Dictionary<string, string> Table => table ??= Load(Languages.Current.Code);

        /// <summary>The text for a key or an English line in the language in use, or null when there is none.</summary>
        public static string Get(string key) => key != null && Table.TryGetValue(key, out string text) ? text : null;

        /// <summary>The language changed: the next question reads the sheets again.</summary>
        public static void Reload() => table = null;

        /// <summary>Fills in the player's keys: "{key:Interact}" becomes "E" (or their own binding).</summary>
        public static string Format(string text)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains("{key:")) return text;
            int at;
            while ((at = text.IndexOf("{key:", StringComparison.Ordinal)) >= 0)
            {
                int end = text.IndexOf('}', at);
                if (end < 0) break;
                string action = text.Substring(at + 5, end - at - 5);
                text = text.Substring(0, at) + InputBindings.Display(action) + text.Substring(end + 1);
            }
            return text;
        }

        private static Dictionary<string, string> Load(string language)
        {
            var lines = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (TextAsset sheet in Resources.LoadAll<TextAsset>(Folder))
            {
                // A broken sheet must fall back to the English in code, not throw into whatever asked.
                try { Read(sheet.text, language, lines); }
                catch (Exception e) { Debug.LogWarning($"[Text] Resources/{Folder}/{sheet.name} could not be read ({e.Message}); its lines stay English."); }
            }
            return lines;
        }

        /// <summary>Adds one sheet's lines for <paramref name="language"/>. Public for the tests and the sheet checks.</summary>
        public static void Read(string sheet, string language, Dictionary<string, string> into)
        {
            if (string.IsNullOrEmpty(sheet)) return;
            string key = null, english = null, local = null;
            int start = 0;
            while (start <= sheet.Length)
            {
                int end = sheet.IndexOf('\n', start);
                if (end < 0) end = sheet.Length;
                string line = sheet.Substring(start, end - start).Trim();
                start = end + 1;
                if (line.Length == 0)
                {
                    Store(key, english, local, into);
                    key = english = local = null;
                    continue;
                }
                int colon = line.IndexOf(':');
                if (line[0] == '#' || colon <= 0) continue;
                string tag = line.Substring(0, colon);
                string text = Unescape(line.Substring(colon + 1).Trim());
                if (tag == KeyTag) key = text;
                else
                {
                    if (tag == Languages.English) english = text;
                    if (tag == language && text.Length > 0) local = text;
                }
            }
            Store(key, english, local, into);
        }

        private static void Store(string key, string english, string local, Dictionary<string, string> into)
        {
            // By id: a missing translation gives the sheet's English. By English line: nothing is stored, and
            // the caller keeps the English it asked with.
            if (!string.IsNullOrEmpty(key))
            {
                if ((local ?? english) != null) into[key] = local ?? english;
            }
            else if (!string.IsNullOrEmpty(english) && local != null && !ReferenceEquals(local, english)) into[english] = local;
        }

        // One pass, so an escaped backslash before an n ("\\n" in the sheet) stays a backslash and an n.
        private static string Unescape(string text)
        {
            if (text.IndexOf('\\') < 0) return text;
            var sb = new System.Text.StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                char next = i + 1 < text.Length ? text[i + 1] : '\0';
                if (text[i] == '\\' && (next == 'n' || next == '\\'))
                {
                    sb.Append(next == 'n' ? '\n' : '\\');
                    i++;
                }
                else sb.Append(text[i]);
            }
            return sb.ToString();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => table = null;
    }
}
