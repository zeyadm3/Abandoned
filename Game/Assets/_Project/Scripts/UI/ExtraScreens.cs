using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// Screens added from 0.18.7 on live on one object made at start-up and kept across scenes, so a new
    /// screen needs no change to the session prefab. Each screen draws only while it is open.
    /// </summary>
    public class ExtraScreens : MonoBehaviour
    {
        public static ExtraScreens Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Instance != null || Application.isBatchMode) return;
            var go = new GameObject("ExtraScreens");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<ExtraScreens>();
            go.AddComponent<LootCatalogueScreen>();
            go.AddComponent<PingView>(); // 0.19.0
            go.AddComponent<GhostActions>(); // 0.19.2
            go.AddComponent<AchievementWatcher>(); // 0.19.10
            // @@screens
        }
    }
}
