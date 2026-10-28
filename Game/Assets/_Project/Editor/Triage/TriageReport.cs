using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The triage audit's findings. The maps hold tens of thousands of objects, so the Console gets the first few of each
    /// severity (click one to ping its object) and the whole list goes to Game/Logs/EmergencyTriage.txt.
    /// </summary>
    public sealed class TriageReport
    {
        public enum Level { P0, P1, P2 }

        private const int ConsoleLinesPerLevel = 60;
        private readonly StringBuilder file = new();
        private readonly int[] counts = new int[3];

        public int Count(Level level) => counts[(int)level];

        public void Section(string title) => file.AppendLine().AppendLine($"== {title}");

        public void Add(Level level, string message, Object context = null)
        {
            string line = $"[{Tag(level)}] {message}";
            file.AppendLine(line);
            if (counts[(int)level]++ >= ConsoleLinesPerLevel) return;
            if (level == Level.P0) Debug.LogError("[Triage] " + line, context);
            else if (level == Level.P1) Debug.LogWarning("[Triage] " + line, context);
            else Debug.Log("[Triage] " + line, context);
        }

        public string Finish(string scenes)
        {
            string summary = $"[Triage] {scenes}: {Count(Level.P0)} P0 critical, {Count(Level.P1)} P1 warning, {Count(Level.P2)} P2 info.";
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "EmergencyTriage.txt"));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, summary + "\n" + file);
            Debug.Log($"{summary} Full list: {path}");
            return summary;
        }

        private static string Tag(Level level) => level switch
        {
            Level.P0 => "P0 CRITICAL",
            Level.P1 => "P1 WARNING",
            _ => "P2 INFO",
        };

        public static string PathOf(Transform t)
        {
            var parts = new List<string>();
            for (; t != null; t = t.parent) parts.Add(t.name);
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
