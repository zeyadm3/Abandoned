using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The field note on the left: this chapter's lessons as a checklist (done, now, next; at most a handful at
    /// once), a strip of one block per lesson for the whole training, and how to get unstuck. It rebuilds itself
    /// when the HUD is emptied (a language change).
    /// </summary>
    public sealed class TutorialChecklist
    {
        private const int Before = 1, After = 3;

        private VisualElement box, list, strip;
        private Label header, footer;
        private int shownStep = -1, stripCount;

        public void Show(bool visible)
        {
            if (!visible)
            {
                MenuKit.Show(box, false);
                return;
            }
            if (box == null || box.panel == null) Build();
            MenuKit.Show(box, true);
            HudLayer.PlaceFieldNote(box);
        }

        public void Refresh(RunTraining training, int step)
        {
            if (box == null || box.panel == null) return;
            TutorialCatalog catalog = training.Catalog;
            TutorialStepDefinition now = catalog.At(step);
            if (now == null) return;
            Set(header, Loc.T("TRAINING  {0:00}/{1:00}  /  {2}", step + 1, catalog.Count, now.ChapterTitle));
            Set(footer, training.IsServer
                ? Loc.T("Stuck? {0} > Training: skip this lesson or restart the chapter.", InputBindings.Display("Pause"))
                : Loc.T("The host can skip a lesson or restart the chapter."));
            if (stripCount != catalog.Count)
            {
                strip?.RemoveFromHierarchy();
                strip = UiKit.Segments(null, Mathf.Clamp(catalog.Count, 1, 60));
                box.Insert(box.IndexOf(footer), strip);
                stripCount = catalog.Count;
            }
            UiKit.SetSegments(strip, catalog.Count > 0 ? (float)step / catalog.Count : 0f);
            if (step == shownStep && list.childCount > 0) return;
            shownStep = step;
            Rows(catalog, step, now.Chapter);
        }

        // The chapter's lessons around the current one: one done before it, a few still to come.
        private void Rows(TutorialCatalog catalog, int step, int chapter)
        {
            list.Clear();
            int first = catalog.ChapterStart(step), last = first;
            while (catalog.At(last + 1) is TutorialStepDefinition next && next.Chapter == chapter) last++;
            int from = Mathf.Max(first, step - Before), to = Mathf.Min(last, step + After);
            for (int i = from; i <= to; i++)
            {
                TutorialStepDefinition s = catalog.At(i);
                if (s == null) continue;
                var row = new VisualElement { pickingMode = PickingMode.Ignore };
                row.AddToClassList("training-list__row");
                row.AddToClassList(i < step ? "training-list__row--done" : i == step ? "training-list__row--now" : "training-list__row--next");
                var mark = new Label(i < step ? "✓" : i == step ? "▶" : "·") { pickingMode = PickingMode.Ignore };
                mark.AddToClassList("training-list__mark");
                row.Add(mark);
                var title = new Label(s.Title) { pickingMode = PickingMode.Ignore };
                title.AddToClassList("training-list__title");
                row.Add(title);
                list.Add(row);
            }
            int more = last - to;
            if (more > 0)
            {
                var rest = new Label(Loc.T("+{0} more in this chapter", more)) { pickingMode = PickingMode.Ignore };
                rest.AddToClassList("training-list__title");
                list.Add(rest);
            }
        }

        private void Build()
        {
            HudLayer.Remove(box);
            box = HudLayer.Add(new VisualElement(), "hud-panel", "hud-hint", "first-day", "field-note", "training-note");
            header = MenuKit.Text(box, Loc.T("TRAINING"), "section");
            header.pickingMode = PickingMode.Ignore;
            list = new VisualElement { pickingMode = PickingMode.Ignore };
            list.AddToClassList("training-list");
            box.Add(list);
            footer = MenuKit.Text(box, "", "text");
            footer.pickingMode = PickingMode.Ignore;
            footer.AddToClassList("text--small");
            footer.style.whiteSpace = WhiteSpace.Normal;
            strip = null;
            stripCount = 0;
            shownStep = -1;
        }

        public void Remove() => HudLayer.Remove(box);

        private static void Set(Label label, string value)
        {
            if (label != null && label.text != value) label.text = value;
        }
    }
}
