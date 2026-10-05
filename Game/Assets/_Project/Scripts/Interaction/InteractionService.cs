using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>The active <see cref="IInteractionHandler"/>; networking swaps it in M3.</summary>
    public static class InteractionService
    {
        public static IInteractionHandler Handler { get; set; } = new LocalInteractionHandler();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Handler = new LocalInteractionHandler();
    }
}
