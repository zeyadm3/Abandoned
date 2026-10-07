using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// PlayerPrefs for the player's own settings and progress, except in batch mode (tests, headless
    /// nettests), where values live in memory: a test must neither inherit what the developer chose in
    /// the editor nor leave its own choices behind (VoiceSettings follows the same rule).
    /// </summary>
    public static class Prefs
    {
        private static readonly Dictionary<string, object> memory = new();

        private static bool InMemory => Application.isBatchMode;

        public static float GetFloat(string key, float fallback) =>
            InMemory ? memory.TryGetValue(key, out object v) && v is float f ? f : fallback : PlayerPrefs.GetFloat(key, fallback);

        public static int GetInt(string key, int fallback) =>
            InMemory ? memory.TryGetValue(key, out object v) && v is int i ? i : fallback : PlayerPrefs.GetInt(key, fallback);

        public static string GetString(string key, string fallback) =>
            InMemory ? memory.TryGetValue(key, out object v) && v is string s ? s : fallback : PlayerPrefs.GetString(key, fallback);

        public static void SetFloat(string key, float value) { if (InMemory) memory[key] = value; else PlayerPrefs.SetFloat(key, value); }

        public static void SetInt(string key, int value) { if (InMemory) memory[key] = value; else PlayerPrefs.SetInt(key, value); }

        public static void SetString(string key, string value) { if (InMemory) memory[key] = value; else PlayerPrefs.SetString(key, value); }

        public static void Delete(string key) { if (InMemory) memory.Remove(key); else PlayerPrefs.DeleteKey(key); }

        /// <summary>Writes to disk now (PlayerPrefs otherwise only flushes on a clean quit).</summary>
        public static void Save() { if (!InMemory) PlayerPrefs.Save(); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => memory.Clear();
    }
}
