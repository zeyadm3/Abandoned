using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>Where this machine hears from: the active AudioListener (the local player's camera).</summary>
    public static class VoiceListener
    {
        // Searching the scene every frame per speaker is wasteful when there's no listener (headless).
        private const float SearchInterval = 0.5f;
        private static AudioListener cached;
        private static float nextSearch;

        public static AudioListener Current
        {
            get
            {
                if ((cached == null || !cached.isActiveAndEnabled) && Time.unscaledTime >= nextSearch)
                {
                    cached = Object.FindAnyObjectByType<AudioListener>();
                    nextSearch = Time.unscaledTime + SearchInterval;
                }
                return cached != null && cached.isActiveAndEnabled ? cached : null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            cached = null;
            nextSearch = 0f;
        }
    }
}
