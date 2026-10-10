using System;
using System.Collections.Generic;

namespace Abandoned.Extraction
{
    /// <summary>
    /// What this job is asking for beyond the quota (0.18+): each bonus feature adds a line source; the run
    /// HUD shows the lines under the haul when the scan is up.
    /// </summary>
    public static class RunGoals
    {
        private static readonly List<Func<string>> Sources = new();

        public static void Add(Func<string> source)
        {
            if (!Sources.Contains(source)) Sources.Add(source);
        }

        public static void Remove(Func<string> source) => Sources.Remove(source);

        public static string Text()
        {
            var lines = new List<string>();
            foreach (Func<string> s in Sources)
                if (s?.Invoke() is string line && line.Length > 0) lines.Add(line);
            return string.Join("\n", lines);
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Sources.Clear();
    }
}
