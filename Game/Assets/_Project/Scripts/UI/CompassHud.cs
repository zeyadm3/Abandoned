using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>Directions only: no bearings, destination icons or enemy information.</summary>
    public class CompassHud : MonoBehaviour
    {
        private const float PixelsPerDegree = .8f, Width = 150f;
        private const int TapeStart = -180, TapeEnd = 540;
        private static readonly string[] Points = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" }; // @loc:compass
        private VisualElement strip, tape;
        private void Update()
        {
            NetworkPlayer me = NetworkPlayer.Local;
            Camera view = Camera.main;
            bool show = me != null && me.IsSpawned && !me.IsDead && view != null;
            if (strip == null || strip.panel == null)
            {
                if (!show || HudLayer.Root == null) return;
                Build();
            }
            MenuKit.Show(strip, show);
            if (!show) return;
            VisualElement mission = HudLayer.Root.Q(className: "field-mission");
            strip.style.top = HudLayer.TryLocalRect(mission, HudLayer.Root, out Rect area) && mission.resolvedStyle.display != DisplayStyle.None
                ? area.yMax + 8f : 28f;
            float heading = Mathf.Repeat(view.transform.eulerAngles.y, 360f);
            tape.style.translate = new Translate(Width * .5f - (heading - TapeStart) * PixelsPerDegree, 0f);
        }
        private void Build()
        {
            HudLayer.Remove(strip);
            strip = HudLayer.Add(new VisualElement(), "compass", "field-compass");
            tape = FieldHud.Element(strip, "field-compass__tape");
            for (int degrees = TapeStart; degrees <= TapeEnd; degrees += 15)
            {
                int direction = (degrees % 360 + 360) % 360;
                Label mark = FieldHud.Text(tape, direction % 45 == 0 ? Loc.C("compass", Points[direction / 45]) : "·", "field-compass__mark");
                mark.style.left = (degrees - TapeStart) * PixelsPerDegree;
            }
            FieldHud.Element(strip, "field-compass__needle");
        }
        private void OnDisable() => MenuKit.Show(strip, false);
        private void OnDestroy() => HudLayer.Remove(strip);
    }
}
