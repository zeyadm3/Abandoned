using System;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// The single F1 switch every debug overlay and gizmo checks, so one key shows or hides them all.
    /// </summary>
    public static class DebugView
    {
        public static bool Visible { get; private set; }

        public static event Action<bool> Changed;

        public static void SetVisible(bool visible)
        {
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
