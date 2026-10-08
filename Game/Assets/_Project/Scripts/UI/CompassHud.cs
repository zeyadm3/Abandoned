using Abandoned.Extraction;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// A heading tape along the top edge during a job (tester feedback, 2026-10-09: the mall is easy to get
    /// turned around in). North is world +Z on every machine, so "the stairs are east" means the same thing
    /// to the whole crew. Local and cosmetic; hidden at the HQ, for the dead and under menus (it's on the HUD layer).
    /// </summary>
    public class CompassHud : MonoBehaviour
    {
        // The tape runs from -180 to 540 degrees, so any heading has its 90 degrees of marks on both sides.
        private const float PixelsPerDegree = 2f, Width = 360f;
        private const int TapeStart = -180, TapeEnd = 540, Step = 15;
        private static readonly string[] Points = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

        private VisualElement strip, tape;

        private void Update()
        {
            RunState run = RunState.Current;
            NetworkPlayer me = NetworkPlayer.Local;
            Camera view = Camera.main;
            bool show = run != null && run.IsSpawned && run.State.Phase != RunPhase.Departed
                        && me != null && !me.IsDead && view != null;
            if (strip == null || strip.panel == null)
            {
                if (!show || HudLayer.Root == null) return;
                Build();
            }
            MenuKit.Show(strip, show);
            if (!show) return;
            float heading = Mathf.Repeat(view.transform.eulerAngles.y, 360f);
            // Translate, not left: sliding the tape every frame must not re-run layout.
            tape.style.translate = new Translate(Width * 0.5f - (heading - TapeStart) * PixelsPerDegree, 0f);
        }

        private void Build()
        {
            HudLayer.Remove(strip);
            strip = HudLayer.Add(new VisualElement(), "compass");
            strip.style.width = Width;
            tape = new VisualElement { pickingMode = PickingMode.Ignore };
            tape.AddToClassList("compass__tape");
            strip.Add(tape);
            for (int degrees = TapeStart; degrees <= TapeEnd; degrees += Step)
            {
                int bearing = (degrees % 360 + 360) % 360;
                bool point = bearing % 45 == 0;
                var mark = new Label(point ? Points[bearing / 45] : "") { pickingMode = PickingMode.Ignore };
                mark.AddToClassList(point ? "compass__point" : "compass__tick");
                if (bearing % 90 == 0) mark.AddToClassList("compass__point--cardinal");
                if (bearing == 0) mark.AddToClassList("compass__point--north");
                mark.style.left = (degrees - TapeStart) * PixelsPerDegree;
                tape.Add(mark);
            }
            var needle = new VisualElement { pickingMode = PickingMode.Ignore };
            needle.AddToClassList("compass__needle");
            strip.Add(needle);
        }

        private void OnDisable()
        {
            if (strip != null) MenuKit.Show(strip, false);
        }

        private void OnDestroy() => HudLayer.Remove(strip);
    }
}
