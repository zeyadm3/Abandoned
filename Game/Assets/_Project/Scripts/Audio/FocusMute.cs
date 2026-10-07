using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Settings > Audio > "Mute when in the background": pauses all game sound while the window isn't
    /// focused. Voice keeps flowing over the network; only what this machine plays is silenced.
    /// </summary>
    public class FocusMute : MonoBehaviour
    {
        private static FocusMute instance;
        private bool focused = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Application.isBatchMode || instance != null) return;
            var go = new GameObject("FocusMute");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<FocusMute>();
        }

        private void OnEnable() => GameSettings.Changed += Apply;

        private void OnDisable()
        {
            GameSettings.Changed -= Apply;
            AudioListener.pause = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            focused = hasFocus;
            Apply();
        }

        private void Apply() => AudioListener.pause = !focused && GameSettings.MuteWhenUnfocused;

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
