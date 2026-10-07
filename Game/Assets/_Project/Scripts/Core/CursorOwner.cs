using System.Collections.Generic;

namespace Abandoned.Core
{
    /// <summary>
    /// Screens that need the mouse (the appraisal, the contract board, later menus) claim it here; while
    /// any claim is held the player's look leaves the cursor alone, so clicks land on buttons.
    /// </summary>
    public static class CursorOwner
    {
        private static readonly HashSet<object> Claims = new();

        public static bool UiActive => Claims.Count > 0;

        private static bool captureRequested;

        /// <summary>A screen closed back into the game (Resume): the player's look takes the mouse again.</summary>
        public static void RequestCapture() => captureRequested = true;

        public static bool ConsumeCaptureRequest()
        {
            bool requested = captureRequested;
            captureRequested = false;
            return requested;
        }

        public static void Set(object owner, bool claimed)
        {
            if (claimed) Claims.Add(owner);
            else Claims.Remove(owner);
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Claims.Clear();
            captureRequested = false;
        }
    }
}
