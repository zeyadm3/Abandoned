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

        public static void Set(object owner, bool claimed)
        {
            if (claimed) Claims.Add(owner);
            else Claims.Remove(owner);
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Claims.Clear();
    }
}
