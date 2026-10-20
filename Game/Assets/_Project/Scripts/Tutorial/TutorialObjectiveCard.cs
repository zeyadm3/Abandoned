using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The lesson card at the top of the screen, under the compass: the chapter, the lesson's name in large
    /// letters, what to do (the player's keys picked out), this lesson's own progress bar with its numbers, and
    /// the keys it uses lighting up as they're pressed. When a lesson is done the bar turns green and DONE is
    /// stamped on it before the next lesson slides in. It rebuilds itself when the HUD is emptied.
    /// </summary>
    public sealed class TutorialObjectiveCard
    {
        private const string KeyColour = "#E2C68C";
        private const float MaxWidth = 580f;

        private readonly TutorialKeyHints keys = new();
        private VisualElement card, meter, bar;
        private Label chapter, count, title, text, readout, stamp;
        private TutorialStepDefinition shownStep;
        private int shownCrew = -1;

        public bool Built => card != null && card.panel != null;

        public void Show(bool visible)
        {
            if (!visible)
            {
                MenuKit.Show(card, false);
                return;
            }
            if (!Built) Build();
            MenuKit.Show(card, true);
            Place();
        }

        /// <summary>Fills the card for a lesson (only when it changed); <paramref name="slideIn"/> when it is new.</summary>
        public void Lesson(TutorialStepDefinition step, int index, int total, int crew, bool slideIn)
        {
            if (!Built || step == null) return;
            if (step == shownStep && crew == shownCrew) return;
            shownStep = step;
            shownCrew = crew;
            string localized = Loc.T(crew > 1 && !string.IsNullOrEmpty(step.CrewDescription) ? step.CrewDescription : step.Description);
            Set(chapter, Loc.T("CHAPTER {0}  /  {1}", step.Chapter + 1, step.ChapterTitle));
            Set(count, Loc.T("LESSON {0:00} / {1:00}", index + 1, total));
            Set(title, step.Title.ToUpperInvariant());
            Set(text, KeysPickedOut(localized));
            keys.Build(card, card.IndexOf(meter), step, localized);
            card.RemoveFromClassList("training-card--done");
            UiKit.SetBar(bar, 0f);
            Set(readout, "");
            if (!slideIn) return;
            // Off for a frame, then the transition brings it in.
            card.AddToClassList("training-card--enter");
            card.schedule.Execute(() => card.RemoveFromClassList("training-card--enter")).StartingIn(30);
        }

        public void Progress(float fraction, string numbers, bool done)
        {
            if (!Built) return;
            UiKit.SetBar(bar, done ? 1f : fraction);
            Set(readout, done ? Loc.T("DONE") : numbers ?? "");
            MenuKit.Show(readout, done || !string.IsNullOrEmpty(numbers));
            card.EnableInClassList("training-card--done", done);
        }

        public void Tick() => keys.Tick();

        public void Remove()
        {
            HudLayer.Remove(card);
            card = null;
            shownStep = null;
        }

        private void Build()
        {
            HudLayer.Remove(card);
            card = HudLayer.Add(new VisualElement(), "training-card");
            var head = Element(card, "training-card__head");
            chapter = Text(head, "training-card__chapter");
            count = Text(head, "training-card__count");
            title = Text(card, "training-card__title");
            text = Text(card, "training-card__text");
            text.enableRichText = true;
            meter = Element(card, "training-card__meter");
            bar = UiKit.Bar(meter, "training-card__bar");
            readout = Text(meter, "training-card__readout");
            stamp = Text(card, "training-card__stamp");
            stamp.text = Loc.T("DONE");
            shownStep = null;
            shownCrew = -1;
        }

        // Under the compass (which sits under the haul), so the three read top to bottom; centred in the room left
        // between the checklist on the left and the battery on the right, whatever the screen's shape.
        private void Place()
        {
            VisualElement hud = HudLayer.Root;
            if (hud == null || !HudLayer.Finite(hud.layout.width) || hud.layout.width <= 0f) return;
            float top = 96f;
            if (HudLayer.TryLocalRect(hud.Q(className: "field-compass"), hud, out Rect compass)) top = Mathf.Max(top, compass.yMax + 12f);
            else if (HudLayer.TryLocalRect(hud.Q(className: "field-mission"), hud, out Rect mission)) top = Mathf.Max(top, mission.yMax + 12f);
            float screen = hud.layout.width, left = 28f, right = screen - 28f;
            if (HudLayer.TryLocalRect(hud.Q(className: "training-note"), hud, out Rect note)) left = Mathf.Max(left, note.xMax + 16f);
            if (HudLayer.TryLocalRect(hud.Q(className: "field-power"), hud, out Rect power)) right = Mathf.Min(right, power.xMin - 16f);
            float width = Mathf.Clamp(right - left, 300f, MaxWidth);
            float x = Mathf.Clamp(screen * 0.5f - width * 0.5f, left, Mathf.Max(left, right - width));
            card.style.top = top;
            card.style.left = x;
            card.style.width = width;
        }

        // "{key:Sprint}" becomes the player's key, picked out in amber.
        private static string KeysPickedOut(string localized)
        {
            if (string.IsNullOrEmpty(localized)) return "";
            var sb = new System.Text.StringBuilder(localized.Length + 32);
            int at = 0;
            while (true)
            {
                int start = localized.IndexOf("{key:", at, System.StringComparison.Ordinal);
                int end = start >= 0 ? localized.IndexOf('}', start) : -1;
                if (start < 0 || end < 0)
                {
                    sb.Append(localized, at, localized.Length - at);
                    break;
                }
                sb.Append(localized, at, start - at);
                string action = localized.Substring(start + 5, end - start - 5);
                sb.Append("<b><color=").Append(KeyColour).Append(">[").Append(InputBindings.Display(action)).Append("]</color></b>");
                at = end + 1;
            }
            return sb.ToString();
        }

        private static VisualElement Element(VisualElement parent, string cls)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.AddToClassList(cls);
            parent.Add(e);
            return e;
        }

        private static Label Text(VisualElement parent, string cls)
        {
            var l = new Label { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            parent.Add(l);
            return l;
        }

        private static void Set(Label label, string value)
        {
            if (label != null && label.text != value) label.text = value;
        }
    }
}
