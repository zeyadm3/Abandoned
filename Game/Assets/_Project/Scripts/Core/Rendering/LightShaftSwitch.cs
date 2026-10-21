using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Abandoned.Core
{
    /// <summary>
    /// The painted light-shaft cards from before 0.24.12 would double the real beams the volumetric fog now draws,
    /// so they hide while it runs and come back when the player turns it off (or a map has no volumetric fog).
    /// The island lighthouse's beam is a gameplay signal, not decoration, and is never touched.
    /// </summary>
    public static class LightShaftSwitch
    {
        // Material names of the interior fake shafts (builders' LightShaft materials).
        private static readonly HashSet<string> Painted = new() { "LightShaft", "ClerestoryLightShaft", "ShopLightShaft", "IronFactory_LightShaft" };
        private static readonly List<Renderer> shafts = new();
        private static bool scanned, shown = true;

        public static void Apply(bool volumetricsOn)
        {
            if (!scanned) Scan();
            var fog = VolumeManager.instance.stack.GetComponent<VolumetricFogOverride>();
            bool show = !(volumetricsOn && fog != null && fog.IsActive);
            if (show == shown) return;
            shown = show;
            foreach (Renderer r in shafts)
                if (r != null) r.enabled = show;
        }

        private static void Scan()
        {
            scanned = true;
            shafts.Clear();
            foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                Material m = r.sharedMaterial;
                if (m != null && Painted.Contains(m.name)) shafts.Add(r);
            }
            shown = true; // freshly loaded cards are visible; Apply settles them
        }

        // A level load brings new cards: look again on the next Apply.
        private static void OnLoaded(Scene scene, LoadSceneMode mode) => scanned = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            shafts.Clear();
            scanned = false;
            shown = true;
            SceneManager.sceneLoaded -= OnLoaded; // statics survive Play mode without a domain reload
            SceneManager.sceneLoaded += OnLoaded;
        }
    }
}
