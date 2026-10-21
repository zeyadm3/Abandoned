using Abandoned.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.13: each map's <see cref="SurfaceWeathering"/> (how much grime and dust the Abandoned/Surface shader draws,
    /// and in what colours), in the scene's `_AV_Surfaces` object. Picked from the scene name, like AvLook.
    /// </summary>
    public static class SurfaceAging
    {
        public const string ObjectName = "_AV_Surfaces";

        private static (float strength, Color grime, Color dust) For(string scene)
        {
            string n = scene.ToLowerInvariant();
            // The company's own building is used every day: swept, but never cleaned.
            if (n.Contains("hq")) return (0.55f, new Color(0.45f, 0.4f, 0.33f), new Color(0.38f, 0.36f, 0.33f));
            // The iron works: soot, not dust.
            if (n.Contains("factory")) return (1.1f, new Color(0.3f, 0.28f, 0.26f), new Color(0.27f, 0.26f, 0.25f));
            // Salt air and damp: green-grey grime, pale salt dust.
            if (n.Contains("prison")) return (1f, new Color(0.4f, 0.42f, 0.36f), new Color(0.42f, 0.42f, 0.4f));
            // Shut since the winter it closed: dust over everything, light grime.
            if (n.Contains("school")) return (0.9f, new Color(0.46f, 0.42f, 0.36f), new Color(0.4f, 0.39f, 0.37f));
            // Someone's house, empty a few weeks.
            if (n.Contains("tutorial")) return (0.45f, new Color(0.48f, 0.44f, 0.38f), new Color(0.4f, 0.39f, 0.37f));
            // The mall: years closed.
            return (1f, new Color(0.42f, 0.38f, 0.31f), new Color(0.36f, 0.34f, 0.31f));
        }

        /// <summary>Adds or updates the scene's weathering object. Returns the strength set.</summary>
        public static float Place(Scene scene)
        {
            SurfaceWeathering weathering = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name != ObjectName) continue;
                weathering = root.GetComponent<SurfaceWeathering>();
                if (weathering == null) weathering = root.AddComponent<SurfaceWeathering>();
                break;
            }
            if (weathering == null)
            {
                var go = new GameObject(ObjectName);
                SceneManager.MoveGameObjectToScene(go, scene);
                weathering = go.AddComponent<SurfaceWeathering>();
            }
            var (strength, grime, dust) = For(scene.name);
            weathering.EditorSetup(strength, grime, dust);
            return strength;
        }
    }
}
