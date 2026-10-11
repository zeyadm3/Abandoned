using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.Threats;
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
        // A Stalker past this much of its build-up shows as a red mark at its bearing; the strip shows 90 degrees
        // either side, so a mark pinned at an edge means "behind you, turn this way".
        private const float WarnFrom = 0.35f, EdgeDegrees = 86f;
        private static readonly string[] Points = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

        private VisualElement strip, tape, threat, window;
        private Label bearing, direction;

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
            string degrees = $"{Mathf.RoundToInt(heading) % 360:000}\u00b0";
            if (bearing.text != degrees) bearing.text = degrees;
            string point = Points[Mathf.RoundToInt(heading / 45f) % Points.Length];
            if (direction.text != point) direction.text = point;
            // Translate, not left: sliding the tape every frame must not re-run layout.
            float visibleWidth = window.layout.width > 0f ? window.layout.width : Width;
            tape.style.translate = new Translate(visibleWidth * 0.5f - (heading - TapeStart) * PixelsPerDegree, 0f);

            // Looking at a Stalker is the counter, so tell its prey which way to look.
            Stalker stalker = Stalker.PressingOn(me);
            bool warn = stalker != null && stalker.Dread01 >= WarnFrom;
            MenuKit.Show(threat, warn);
            if (!warn) return;
            Vector3 to = stalker.transform.position - view.transform.position;
            float edge = Mathf.Min(EdgeDegrees, Mathf.Max(0f, (visibleWidth * .5f - 6f) / PixelsPerDegree));
            float off = Mathf.Clamp(Mathf.DeltaAngle(heading, Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg), -edge, edge);
            threat.style.translate = new Translate(off * PixelsPerDegree, 0f);
        }

        private void Build()
        {
            HudLayer.Remove(strip);
            strip = HudLayer.Add(new VisualElement(), "compass", "h-navigation");
            strip.style.width = Width;
            var readout = new VisualElement { pickingMode = PickingMode.Ignore };
            readout.AddToClassList("h-navigation__readout");
            strip.Add(readout);
            var caption = new Label("BEARING") { pickingMode = PickingMode.Ignore };
            caption.AddToClassList("h-micro");
            readout.Add(caption);
            direction = new Label("N") { pickingMode = PickingMode.Ignore };
            direction.AddToClassList("h-navigation__direction");
            readout.Add(direction);
            bearing = new Label("000\u00b0") { pickingMode = PickingMode.Ignore };
            bearing.AddToClassList("h-navigation__bearing");
            readout.Add(bearing);
            window = new VisualElement { pickingMode = PickingMode.Ignore };
            window.AddToClassList("h-navigation__window");
            strip.Add(window);
            tape = new VisualElement { pickingMode = PickingMode.Ignore };
            tape.AddToClassList("compass__tape");
            window.Add(tape);
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
            window.Add(needle);
            threat = new VisualElement { pickingMode = PickingMode.Ignore };
            threat.AddToClassList("compass__threat");
            window.Add(threat);
            MenuKit.Show(threat, false);
        }

        private void OnDisable()
        {
            if (strip != null) MenuKit.Show(strip, false);
        }

        private void OnDestroy() => HudLayer.Remove(strip);
    }
}
