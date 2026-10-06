namespace Abandoned.Networking
{
    /// <summary>
    /// Carries "why you're back at the menu" across the scene reload that follows a client's session
    /// ending, and stops the reloaded scene from auto-hosting (the player just left a game; dropping
    /// them into a fresh solo session would hide the message and surprise them).
    /// </summary>
    public static class SessionEndNotice
    {
        /// <summary>Shown by the menu until the next session starts or the player dismisses it. Empty = none.</summary>
        public static string Message { get; private set; } = string.Empty;

        /// <summary>The current scene was loaded because a session ended: don't auto-host it.</summary>
        public static bool ReturnedFromSession { get; private set; }

        public static void Set(string message)
        {
            Message = message ?? string.Empty;
            ReturnedFromSession = true;
        }

        public static void Clear()
        {
            Message = string.Empty;
            ReturnedFromSession = false;
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clear();
    }
}
