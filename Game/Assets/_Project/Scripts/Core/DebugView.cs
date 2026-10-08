using System;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// The single F1 switch every debug overlay and gizmo checks, so one key shows or hides them all.
    /// Only in the editor and Development builds (<see cref="DevTools"/>).
    /// </summary>
    public static class DebugView
    {
        public static bool Visible { get; private set; }

        public static event Action<bool> Changed;

        public static void SetVisible(bool visible)
        {
            // Player builds never show it: it draws monsters through walls (QA B-02).
            if (visible && !DevTools.Enabled) return;
            if (Visible == visible) return;
            Visible = visible;
            Changed?.Invoke(visible);
        }

        public static void Toggle() => SetVisible(!Visible);

        // Statics survive Play mode when domain reload is disabled; start every session clean.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Visible = false;
            Changed = null;
        }
    }
}
