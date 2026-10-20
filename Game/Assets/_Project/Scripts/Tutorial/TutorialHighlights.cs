using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// Pulsing frames around the HUD instruments a lesson talks about ("Bottom left: your health and breath"),
    /// each with a tag naming it, fitted to where the instrument actually is on this screen. The lesson's
    /// Highlight line names them: vitals, compass, haul, pockets (or inventory), battery, cargo.
    /// </summary>
    public sealed class TutorialHighlights
    {
        private readonly struct Piece
        {
            public readonly string ClassName, Label;
            public readonly bool LabelBelow;

            public Piece(string className, string label, bool labelBelow)
            {
                ClassName = className;
                Label = label;
                LabelBelow = labelBelow;
            }
        }

        private static readonly Dictionary<string, Piece> Pieces = new()
        {
            ["vitals"] = new Piece("h-vitals", Loc.Mark("HEALTH AND BREATH"), false),
            ["compass"] = new Piece("field-compass", Loc.Mark("COMPASS"), true),
            ["haul"] = new Piece("field-mission", Loc.Mark("HAUL AND QUOTA"), true),
            ["pockets"] = new Piece("field-pockets", Loc.Mark("YOUR FOUR PLACES"), false),
            ["inventory"] = new Piece("field-pockets", Loc.Mark("YOUR FOUR PLACES"), false),
            ["battery"] = new Piece("field-power", Loc.Mark("FLASHLIGHT BATTERY"), true),
            ["cargo"] = new Piece("field-cargo", Loc.Mark("WHAT THE TRUCK HOLDS"), false),
        };

        private readonly List<(VisualElement frame, Piece piece)> frames = new();
        private string shown;

        /// <summary>Is this a HUD piece a lesson's Highlight line can name?</summary>
        public static bool Knows(string piece) => Pieces.ContainsKey((piece ?? "").Trim().ToLowerInvariant());

        /// <summary>Frames the pieces this lesson names (an empty line clears them).</summary>
        public void Set(string highlight)
        {
            highlight ??= "";
            bool stale = frames.Count > 0 && frames[0].frame.panel == null;
            if (highlight == shown && !stale) return;
            Clear();
            shown = highlight;
            if (HudLayer.Root == null) return;
            foreach (string raw in highlight.Split(','))
            {
                string key = raw.Trim().ToLowerInvariant();
                if (key.Length == 0 || !Pieces.TryGetValue(key, out Piece piece)) continue;
                VisualElement frame = HudLayer.Add(new VisualElement(), "training-highlight");
                frame.EnableInClassList("training-highlight--below", piece.LabelBelow);
                var tag = new Label(Loc.T(piece.Label)) { pickingMode = PickingMode.Ignore };
                tag.AddToClassList("training-highlight__label");
                frame.Add(tag);
                frames.Add((frame, piece));
            }
        }

        /// <summary>Every frame: follow the instruments (they move with the screen and with each other) and pulse.</summary>
        public void Tick(bool visible)
        {
            VisualElement hud = HudLayer.Root;
            float pulse = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 5f);
            foreach ((VisualElement frame, Piece piece) in frames)
            {
                Rect rect = default;
                bool found = visible && hud != null && HudLayer.TryLocalRect(hud.Q(className: piece.ClassName), hud, out rect);
                MenuKit.Show(frame, found);
                if (!found) continue;
                const float pad = 6f;
                frame.style.left = rect.xMin - pad;
                frame.style.top = rect.yMin - pad;
                frame.style.width = rect.width + pad * 2f;
                frame.style.height = rect.height + pad * 2f;
                frame.style.opacity = pulse;
            }
        }

        public void Clear()
        {
            foreach ((VisualElement frame, Piece _) in frames) HudLayer.Remove(frame);
            frames.Clear();
            shown = null;
        }
    }
}
