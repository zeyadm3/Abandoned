using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Player-facing text out of code (0.19.9), so the game can be translated later: Resources/Text/&lt;language&gt;
    /// is a JSON list of key/text pairs. Code asks for a key and keeps its English as the fallback, so a
    /// missing line never shows a key. "{key:Interact}" in a text is the player's own key for that action.
    /// Moved so far: the first-time tips ("hint.&lt;HintId&gt;").
    /// </summary>
    public static class TextTable
    {
        [Serializable]
        private class Entry
        {
            public string key;
            public string text;
        }

        [Serializable]
        private class File
        {
            public List<Entry> entries = new();
        }

        private static Dictionary<string, string> table;
        private static string loaded;

        /// <summary>The language in use ("en" until there are translations; a Settings choice later).</summary>
        public static string Language { get; set; } = "en";

        public static string Get(string key)
        {
            if (table == null || loaded != Language) Load();
            return table.TryGetValue(key, out string text) ? text : null;
        }

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

        private static void Load()
        {
            loaded = Language;
            table = new Dictionary<string, string>();
            var asset = Resources.Load<TextAsset>($"Text/{Language}");
            if (asset == null) return;
            File file = JsonUtility.FromJson<File>(asset.text);
            if (file?.entries == null) return;
            foreach (Entry e in file.entries) if (!string.IsNullOrEmpty(e.key)) table[e.key] = e.text;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            table = null;
            Language = "en";
        }
    }
}
