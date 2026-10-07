namespace Abandoned.Core
{
    /// <summary>
    /// A screen that needs the mouse (the appraisal, later menus) claims it here; the player's look then
    /// leaves the cursor alone, so a click lands on the button instead of re-locking the mouse.
    /// </summary>
    public static class CursorOwner
    {
        public static bool UiActive { get; set; }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => UiActive = false;
    }
}
