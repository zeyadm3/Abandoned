using Abandoned.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The UI overhaul's widgets (step 1), on top of <see cref="MenuKit"/>: key caps for the player's own
    /// bindings, icons, progress bars, rubber stamps, tags, hazard tape, and the little animations that make
    /// the screens feel like R.E.P.O.-style physical things (a jolt, a slam). Styles in Art/UI/Menu.uss.
    /// </summary>
    public static class UiKit
    {
        /// <summary>A key cap for an input action (the player's own binding; mouse buttons as a glyph).</summary>
        public static VisualElement KeyCap(VisualElement parent, string action) => Key(parent, InputBindings.Display(action));

        /// <summary>A key cap with this text ("E", "LMB", "Space"...).</summary>
        public static VisualElement Key(VisualElement parent, string key)
        {
            string glyph = key switch
            {
                "LMB" => "mouse/mouse_left",
                "RMB" => "mouse/mouse_right",
                "MMB" => "mouse/mouse_scroll",
                "Scroll Up" => "mouse/mouse_scroll_up",
                "Scroll Down" => "mouse/mouse_scroll_down",
                _ => null,
            };
            Texture2D icon = glyph != null ? UiIcons.Get(glyph) : null;
            VisualElement cap;
            if (icon != null)
            {
                cap = new VisualElement();
                cap.style.backgroundImage = icon;
                cap.AddToClassList("keycap--mouse");
            }
            else cap = new Label(key.Length > 6 ? key.Substring(0, 6) : key);
            cap.AddToClassList("keycap");
            cap.pickingMode = PickingMode.Ignore;
            parent?.Add(cap);
            return cap;
        }

        /// <summary>An icon by id (see <see cref="UiIcons"/>); an empty box when it's missing.</summary>
        public static VisualElement Icon(VisualElement parent, string id, string size = null)
        {
            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList("icon");
            if (size != null) icon.AddToClassList("icon--" + size);
            Texture2D t = UiIcons.Get(id);
            if (t != null) icon.style.backgroundImage = t;
            parent?.Add(icon);
            return icon;
        }

        public static void SetIcon(VisualElement icon, string id)
        {
            Texture2D t = UiIcons.Get(id);
            icon.style.backgroundImage = t != null ? new StyleBackground(t) : new StyleBackground(StyleKeyword.None);
        }

        /// <summary>A progress bar; set it with <see cref="SetBar"/>.</summary>
        public static VisualElement Bar(VisualElement parent, params string[] classes)
        {
            var bar = new VisualElement { pickingMode = PickingMode.Ignore };
            bar.AddToClassList("bar");
            foreach (string c in classes) bar.AddToClassList(c);
            var fill = new VisualElement { pickingMode = PickingMode.Ignore, name = "fill" };
            fill.AddToClassList("bar__fill");
            bar.Add(fill);
            parent?.Add(bar);
            return bar;
        }

        public static void SetBar(VisualElement bar, float fraction, string state = null)
        {
            VisualElement fill = bar?.Q("fill");
            if (fill == null) return;
            fill.style.width = Length.Percent(Mathf.Clamp01(fraction) * 100f);
            fill.EnableInClassList("bar__fill--good", state == "good");
            fill.EnableInClassList("bar__fill--bad", state == "bad");
        }

        /// <summary>A rubber stamp ("QUOTA MET"); hidden until <see cref="Slam"/>.</summary>
        public static Label Stamp(VisualElement parent, string text, bool good)
        {
            var stamp = new Label(text) { pickingMode = PickingMode.Ignore };
            stamp.AddToClassList("stamp");
            stamp.AddToClassList("stamp--hidden");
            stamp.EnableInClassList("stamp--good", good);
            parent?.Add(stamp);
            return stamp;
        }

        /// <summary>Brings a stamp (or anything with a --hidden class) down hard on the next frame.</summary>
        public static void Slam(VisualElement e, string hiddenClass = "stamp--hidden") =>
            e.schedule.Execute(() => e.RemoveFromClassList(hiddenClass)).StartingIn(16);

        public static Label Tag(VisualElement parent, string text)
        {
            var tag = new Label(text) { pickingMode = PickingMode.Ignore };
            tag.AddToClassList("tag");
            parent?.Add(tag);
            return tag;
        }

        /// <summary>A strip of yellow-and-black hazard tape.</summary>
        public static VisualElement Hazard(VisualElement parent)
        {
            var tape = new VisualElement { pickingMode = PickingMode.Ignore };
            tape.AddToClassList("hazard");
            for (int i = 0; i < 80; i++)
            {
                var bar = new VisualElement { pickingMode = PickingMode.Ignore };
                bar.AddToClassList("hazard__bar");
                tape.Add(bar);
            }
            parent?.Add(tape);
            return tape;
        }

        /// <summary>A quick wobble (a button being hovered, a value changing): rotation only, so layout and transitions stay put.</summary>
        public static void Jolt(VisualElement e)
        {
            float[] steps = { -1.6f, 1.2f, -0.6f, 0f };
            for (int i = 0; i < steps.Length; i++)
            {
                float angle = steps[i];
                e.schedule.Execute(() => e.style.rotate = new Rotate(angle)).StartingIn(i * 30);
            }
        }
    }
}
