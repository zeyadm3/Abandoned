using Abandoned.Core;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// This player's own training choices (Prefs, so they stay across sessions): the trainer's voice and the
    /// guide arrows. Both start on; the pause menu's Training section switches them. Read every frame, so the
    /// values are kept here after the first read.
    /// </summary>
    public static class TutorialSettings
    {
        private const string VoiceKey = "tutorial.voice", ArrowsKey = "tutorial.arrows";
        private static int voice = -1, arrows = -1;

        public static bool Voice
        {
            get
            {
                if (voice < 0) voice = Prefs.GetInt(VoiceKey, 1) != 0 ? 1 : 0;
                return voice == 1;
            }
            set
            {
                voice = value ? 1 : 0;
                Prefs.SetInt(VoiceKey, voice);
                Prefs.Save();
            }
        }

        public static bool Arrows
        {
            get
            {
                if (arrows < 0) arrows = Prefs.GetInt(ArrowsKey, 1) != 0 ? 1 : 0;
                return arrows == 1;
            }
            set
            {
                arrows = value ? 1 : 0;
                Prefs.SetInt(ArrowsKey, arrows);
                Prefs.Save();
            }
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => voice = arrows = -1;
    }
}
