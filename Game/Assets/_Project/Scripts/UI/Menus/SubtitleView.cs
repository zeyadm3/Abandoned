using System.Collections.Generic;
using Abandoned.Audio;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Captions at the bottom of the screen (Subtitles setting): the newest few, each fading after a
    /// couple of seconds; a caption heard again just stays up longer instead of stacking.
    /// </summary>
    public class SubtitleView
    {
        private const int MaxLines = 4;
        private const float Seconds = 2.5f;

        private readonly List<(Label label, float until)> lines = new();

        public VisualElement Root { get; }

        public SubtitleView()
        {
            Root = new VisualElement { pickingMode = PickingMode.Ignore };
            Root.AddToClassList("subtitles");
        }

        public void Add(string text)
        {
            for (int i = 0; i < lines.Count; i++)
                if (lines[i].label.text == text) { lines[i] = (lines[i].label, Time.time + Seconds); return; }
            if (lines.Count >= MaxLines)
            {
                lines[0].label.RemoveFromHierarchy();
                lines.RemoveAt(0);
            }
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            label.AddToClassList("subtitle-line");
            Root.Add(label);
            lines.Add((label, Time.time + Seconds));
        }

        public void Tick()
        {
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                if (Time.time < lines[i].until) continue;
                lines[i].label.RemoveFromHierarchy();
                lines.RemoveAt(i);
            }
        }

        public int Count => lines.Count;
    }
}
