using Abandoned.Audio;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Hover and keyboard/gamepad focus look the same on a menu button (the stylesheet draws both from
    /// <c>menu-button--lit</c>), and a keyboard or pad arrival gets the hover sound. Nothing is added inside the
    /// button: a Button is a text element, and one with children stops measuring its own text.
    /// </summary>
    public static class MenuTactile
    {
        private const string Lit = "menu-button--lit", Pressed = "menu-button--pressed";

        public static void Attach(Button button, bool small)
        {
            bool hovered = false, focused = false;
            void Refresh() => button.EnableInClassList(Lit, (hovered || focused) && button.enabledSelf);
            button.RegisterCallback<PointerEnterEvent>(_ => { hovered = true; Refresh(); });
            button.RegisterCallback<PointerLeaveEvent>(_ => { hovered = false; button.RemoveFromClassList(Pressed); Refresh(); });
            button.RegisterCallback<FocusInEvent>(_ =>
            {
                focused = true;
                if (!hovered) GameAudio.PlayUi(SoundId.UiHover);
                Refresh();
            });
            button.RegisterCallback<FocusOutEvent>(_ => { focused = false; Refresh(); });
            // Trickle-down: the button's own clickable captures the pointer before a bubbling handler would run.
            button.RegisterCallback<PointerDownEvent>(e => { if (e.button == 0 && button.enabledSelf) button.AddToClassList(Pressed); }, TrickleDown.TrickleDown);
            button.RegisterCallback<PointerUpEvent>(_ => button.RemoveFromClassList(Pressed), TrickleDown.TrickleDown);
        }
    }
}
