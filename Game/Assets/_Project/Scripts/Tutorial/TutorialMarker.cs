using Abandoned.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// One thing the training points at, over the world: on screen a label, the distance and a bobbing chevron
    /// above it; off screen (or behind) an arrow on the screen's edge turning to show which way to look. The
    /// marker breathes faster as the player closes in. Amber for the lesson's target, rust for the monster.
    /// </summary>
    public sealed class TutorialMarker
    {
        private static readonly Color Amber = new(0.73f, 0.6f, 0.38f), Rust = new(0.85f, 0.38f, 0.3f);
        private const float EdgeX = 70f, EdgeTop = 90f, EdgeBottom = 120f;

        private readonly bool danger;
        private VisualElement mark, edge;
        private Label label, distance, edgeDistance;
        private TutorialArrowElement chevron, edgeArrow;
        private int shownMetres = -1, edgeMetres = -1;

        public TutorialMarker(bool danger) => this.danger = danger;

        /// <summary>Its elements are gone (the HUD was emptied for a language change): words must be fetched again.</summary>
        public bool Stale => mark == null || mark.panel == null;

        public void Hide()
        {
            MenuKit.Show(mark, false);
            MenuKit.Show(edge, false);
        }

        public void Remove()
        {
            HudLayer.Remove(mark);
            HudLayer.Remove(edge);
            mark = edge = null;
        }

        /// <param name="world">Where the marker hangs (above the target).</param>
        /// <param name="away">Rust instead of amber (a floor to get off).</param>
        public void Show(Vector3 world, string text, Camera cam, bool away = false)
        {
            if (cam == null || HudLayer.World == null)
            {
                Hide();
                return;
            }
            if (mark == null || mark.panel == null) Build();
            float metres = Vector3.Distance(cam.transform.position, world);
            Vector3 view = cam.WorldToViewportPoint(world);
            bool onScreen = view.z > 0f && view.x > 0.03f && view.x < 0.97f && view.y > 0.06f && view.y < 0.94f;
            Color colour = danger || away ? Rust : Amber;
            // Closer is quicker: the chevron bobs and the label brightens as the target nears.
            float speed = Mathf.Lerp(7f, 3f, Mathf.InverseLerp(2f, 25f, metres));
            float bob = Mathf.Sin(Time.unscaledTime * speed) * 5f;
            if (onScreen)
            {
                MenuKit.Show(edge, false);
                MenuKit.Show(mark, true);
                HudLayer.Place(mark, world, cam);
                Set(label, text);
                int whole = Mathf.RoundToInt(metres);
                if (whole != shownMetres)
                {
                    shownMetres = whole;
                    distance.text = TutorialGuideText.Distance(whole);
                }
                chevron.Fill = colour;
                chevron.style.translate = new Translate(0f, bob);
                mark.EnableInClassList("training-mark--away", away);
                // Right on top of it, the marker steps back so it doesn't hide what it marks.
                mark.style.opacity = metres < 1.6f ? 0.45f : 1f;
                return;
            }
            MenuKit.Show(mark, false);
            MenuKit.Show(edge, true);
            PlaceOnEdge(view, colour, metres, bob);
        }

        // The arrow sits where a line from the screen's centre toward the target leaves the screen.
        private void PlaceOnEdge(Vector3 view, Color colour, float metres, float bob)
        {
            Rect area = edge.panel.visualTree.layout;
            if (area.width <= 0f || area.height <= 0f) return;
            Vector2 at = new(view.x * area.width, (1f - view.y) * area.height);
            if (view.z < 0f) at = new Vector2(area.width - at.x, area.height - at.y); // behind: the mirror image
            Vector2 centre = new(area.width * 0.5f, area.height * 0.5f);
            Vector2 dir = at - centre;
            if (dir.sqrMagnitude < 1f) dir = Vector2.up;
            float halfW = area.width * 0.5f - EdgeX;
            float halfH = dir.y < 0f ? area.height * 0.5f - EdgeTop : area.height * 0.5f - EdgeBottom;
            float scale = Mathf.Min(Mathf.Abs(dir.x) > 0.01f ? halfW / Mathf.Abs(dir.x) : float.MaxValue,
                                    Mathf.Abs(dir.y) > 0.01f ? halfH / Mathf.Abs(dir.y) : float.MaxValue);
            Vector2 pos = centre + dir * scale;
            Vector2 n = dir.normalized;
            // A little push toward the target, in time with the bob.
            pos += n * (bob * 0.8f);
            edge.style.left = pos.x;
            edge.style.top = pos.y;
            edgeArrow.style.rotate = new Rotate(Mathf.Atan2(n.x, -n.y) * Mathf.Rad2Deg);
            edgeArrow.Fill = colour;
            int whole = Mathf.RoundToInt(metres);
            if (whole != edgeMetres)
            {
                edgeMetres = whole;
                edgeDistance.text = TutorialGuideText.Distance(whole);
            }
            // The distance sits on the screen side of the arrow.
            edgeDistance.style.left = 32f - n.x * 30f - 14f;
            edgeDistance.style.top = 32f - n.y * 30f - 9f;
        }

        private void Build()
        {
            Remove();
            shownMetres = edgeMetres = -1;
            mark = new VisualElement { pickingMode = PickingMode.Ignore };
            mark.AddToClassList("training-mark");
            mark.EnableInClassList("training-mark--danger", danger);
            label = Text(mark, "training-mark__label");
            distance = Text(mark, "training-mark__distance");
            chevron = new TutorialArrowElement(false);
            chevron.AddToClassList("training-mark__arrow");
            mark.Add(chevron);
            HudLayer.World.Add(mark);

            edge = new VisualElement { pickingMode = PickingMode.Ignore };
            edge.AddToClassList("training-edge");
            edgeArrow = new TutorialArrowElement(true);
            edgeArrow.AddToClassList("training-edge__arrow");
            edge.Add(edgeArrow);
            edgeDistance = Text(edge, "training-edge__distance");
            HudLayer.World.Add(edge);
            Hide();
        }

        private static Label Text(VisualElement parent, string cls)
        {
            var l = new Label { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            parent.Add(l);
            return l;
        }

        private static void Set(Label l, string value)
        {
            if (l != null && l.text != value) l.text = value;
        }
    }
}
