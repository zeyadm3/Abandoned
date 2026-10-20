using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// A drawn arrowhead for the training's markers: a chevron pointing down at a target, or a solid arrow for the
    /// screen edge, turned with style.rotate. Painted in code (no texture), so it stays sharp at any UI scale.
    /// </summary>
    public sealed class TutorialArrowElement : VisualElement
    {
        private Color fill = new(0.73f, 0.6f, 0.38f);
        private readonly bool solid;

        /// <param name="solid">A filled arrow pointing up (the edge arrow); otherwise a chevron pointing down.</param>
        public TutorialArrowElement(bool solid)
        {
            this.solid = solid;
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        public Color Fill
        {
            get => fill;
            set
            {
                if (fill == value) return;
                fill = value;
                MarkDirtyRepaint();
            }
        }

        private void Draw(MeshGenerationContext context)
        {
            Rect r = contentRect;
            if (r.width <= 0f || r.height <= 0f) return;
            Painter2D p = context.painter2D;
            p.fillColor = fill;
            p.strokeColor = new Color(0.05f, 0.06f, 0.055f, 0.85f);
            p.lineWidth = 1.5f;
            p.lineJoin = LineJoin.Miter;
            p.BeginPath();
            if (solid)
            {
                // Point up, with a notch at the back so it reads as direction, not a triangle.
                p.MoveTo(new Vector2(r.center.x, r.yMin));
                p.LineTo(new Vector2(r.xMax, r.yMax));
                p.LineTo(new Vector2(r.center.x, r.yMax - r.height * 0.3f));
                p.LineTo(new Vector2(r.xMin, r.yMax));
            }
            else
            {
                // A thick V pointing down.
                float t = r.height * 0.42f;
                p.MoveTo(new Vector2(r.xMin, r.yMin));
                p.LineTo(new Vector2(r.center.x, r.yMax - t));
                p.LineTo(new Vector2(r.xMax, r.yMin));
                p.LineTo(new Vector2(r.xMax, r.yMin + t));
                p.LineTo(new Vector2(r.center.x, r.yMax));
                p.LineTo(new Vector2(r.xMin, r.yMin + t));
            }
            p.ClosePath();
            p.Fill();
            p.Stroke();
        }
    }
}
