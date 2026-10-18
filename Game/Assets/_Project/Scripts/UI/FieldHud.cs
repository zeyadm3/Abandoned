using Abandoned.Core;
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
            // Static wear only: REC/TAPE are a visual motif, with no capture/recording system.
            if (GameSettings.ReduceMenuEffects) return;
            painter.strokeColor = new Color(.44f, .13f, .09f, .18f);
            for (int i = 0; i < 38; i++)
            {
                float y = 13f + i * Mathf.Max(1f, (bounds.height - 26f) / 38f);
                float length = 4f + (i * 17 % 23);
                painter.BeginPath(); painter.MoveTo(new Vector2(3f, y)); painter.LineTo(new Vector2(length, y + i % 3)); painter.Stroke();
                painter.BeginPath(); painter.MoveTo(new Vector2(bounds.width - 3f, y)); painter.LineTo(new Vector2(bounds.width - length, y - i % 3)); painter.Stroke();
            }
            painter.strokeColor = new Color(.76f, .69f, .52f, .025f);
            for (float y = 4f; y < bounds.height; y += 6f)
            {
                painter.BeginPath(); painter.MoveTo(new Vector2(8, y)); painter.LineTo(new Vector2(bounds.width - 8, y)); painter.Stroke();
            }
        }
    }
}
