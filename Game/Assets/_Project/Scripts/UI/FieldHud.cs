using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    public static class FieldHud
    {
        public static VisualElement Element(VisualElement parent, params string[] classes)
        {
            var element = new VisualElement { pickingMode = PickingMode.Ignore };
            foreach (string name in classes) element.AddToClassList(name);
            parent.Add(element);
            return element;
        }
        public static Label Text(VisualElement parent, string text, params string[] classes)
        {
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            foreach (string name in classes) label.AddToClassList(name);
            parent.Add(label);
            return label;
        }
        public static void Set(Label label, string text)
        {
            if (label != null && label.text != text) label.text = text;
        }
        public static void PaintFrame(MeshGenerationContext context)
        {
            Rect bounds = context.visualElement.contentRect;
            if (bounds.width <= 0 || bounds.height <= 0) return;
            Painter2D painter = context.painter2D;
            painter.lineWidth = 1f;
            painter.strokeColor = new Color(.65f, .53f, .40f, .35f);
            Rect edge = new(10, 10, Mathf.Max(0, bounds.width - 20), Mathf.Max(0, bounds.height - 20));
            painter.BeginPath();
            painter.MoveTo(edge.min); painter.LineTo(new Vector2(edge.xMax, edge.yMin));
            painter.LineTo(edge.max); painter.LineTo(new Vector2(edge.xMin, edge.yMax));
            painter.ClosePath(); painter.Stroke();
        }
    }
}
