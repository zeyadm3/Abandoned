using System.Collections.Generic;
using Abandoned.Extraction;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>A small deterioration meter and dispatch text; dispatch remains readable through menus.</summary>
    public class HorrorRunHud : MonoBehaviour
    {
        private VisualElement clock, progress, radio;
        private Label status, line;
        private readonly VisualElement[] marks = new VisualElement[6];
        private string shownRadio;
        private string observedRadio;
        private readonly Queue<string> pending = new();
        private float readUntil;
        private RunState lastRun;

        private void Update()
        {
            RunState run = RunState.Current;
            NetworkPlayer me = NetworkPlayer.Local;
            bool active = run != null && run.IsSpawned && run.State.Phase != RunPhase.Departed;
            if (clock == null || clock.panel == null)
            {
                if (HudLayer.Root == null) return;
                Build();
            }
            if (run != lastRun)
            {
                lastRun = run;
                shownRadio = null;
                observedRadio = null;
                pending.Clear();
                readUntil = 0f;
            }
            MenuKit.Show(clock, active && me != null && !me.IsDead);
            if (active)
            {
                bool final = RunHorrorDirector.Current != null && RunHorrorDirector.Current.FinalPhase;
                int danger = Mathf.Clamp(run.State.Danger, 0, marks.Length);
                for (int i = 0; i < marks.Length; i++) marks[i].EnableInClassList("horror-danger__mark--on", i < danger);
                clock.EnableInClassList("horror-danger--final", final);
                string text = final ? "LOCKDOWN" : run.WindowClosed ? "OVERSTAY" : "BUILDING SIGNAL";
                if (status.text != text) status.text = text;
                float elapsed = run.State.Window > 0f ? 1f - run.WindowRemaining / run.State.Window : 1f;
                UiKit.SetBar(progress, elapsed, run.WindowClosed || final ? "bad" : null);
            }
            string incoming = RunHorrorDirector.RadioLine;
            bool urgent = RunHorrorDirector.Current != null && RunHorrorDirector.Current.FinalPhase;
            if (active && !string.IsNullOrEmpty(incoming) && incoming != observedRadio && Time.time <= RunHorrorDirector.RadioUntil)
            {
                observedRadio = incoming;
                if (urgent)
                {
                    pending.Clear();
                    Present(incoming, RunHorrorDirector.RadioUntil);
                }
                else if (Time.time >= readUntil) Present(incoming, RunHorrorDirector.RadioUntil);
                else if (pending.Count < 6) pending.Enqueue(incoming);
            }
            if (active && Time.time >= readUntil && pending.Count > 0) Present(pending.Dequeue(), 0f);
            bool showRadio = active && !string.IsNullOrEmpty(shownRadio) && Time.time < readUntil &&
                             (MenuUi.Current == null || !MenuUi.Current.ClipMode);
            MenuKit.Show(radio, showRadio);
            radio.EnableInClassList("horror-radio--urgent", urgent);
        }

        private void Present(string text, float until)
        {
            shownRadio = text;
            line.text = text;
            readUntil = Mathf.Max(until, Time.time + Mathf.Clamp(text.Length * 0.065f, 5f, 10f));
        }

        private void Build()
        {
            HudLayer.Remove(clock);
            HudLayer.Remove(radio);
            clock = HudLayer.Add(new VisualElement(), "horror-danger");
            status = new Label("BUILDING SIGNAL") { pickingMode = PickingMode.Ignore };
            status.AddToClassList("horror-danger__label");
            clock.Add(status);
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.AddToClassList("horror-danger__marks");
            clock.Add(row);
            for (int i = 0; i < marks.Length; i++)
            {
                marks[i] = new VisualElement { pickingMode = PickingMode.Ignore };
                marks[i].AddToClassList("horror-danger__mark");
                row.Add(marks[i]);
            }
            progress = UiKit.Bar(clock, "horror-danger__progress");
            radio = new VisualElement { pickingMode = PickingMode.Ignore };
            radio.AddToClassList("horror-radio");
            var caption = new Label("DISPATCH / CH. 04") { pickingMode = PickingMode.Ignore };
            caption.AddToClassList("horror-radio__caption");
            radio.Add(caption);
            line = new Label() { pickingMode = PickingMode.Ignore };
            line.AddToClassList("horror-radio__line");
            radio.Add(line);
            // It is also a caption, so menus must not hide an arrival or evacuation warning.
            VisualElement surface = MenuUi.Current != null ? MenuUi.Current.Surface : HudLayer.Root;
            surface.Add(radio);
        }

        private void OnDestroy()
        {
            HudLayer.Remove(clock);
            HudLayer.Remove(radio);
        }
    }
}
