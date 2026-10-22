using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.World
{
    /// <summary>
    /// Loads each salvage location (its own scene) additively when the van comes near it and unloads it once the van
    /// is well away (the two distances give hysteresis, so it doesn't flicker at the edge). Runs on every machine, like
    /// SessionTravel; the location's NetworkObjects are spawned by the host once it is loaded (not wired yet).
    /// </summary>
    public class PoiStreamer : MonoBehaviour
    {
        [System.Serializable]
        public class Poi
        {
            public string scene;
            public Vector3 position;
        }

        [SerializeField] private Transform van;
        [SerializeField] private List<Poi> pois = new();
        [SerializeField] private float loadDistance = 600f;
        [SerializeField] private float unloadDistance = 800f;

        private readonly HashSet<string> loaded = new();
        private readonly HashSet<string> busy = new();

        public bool IsLoaded(string scene) => loaded.Contains(scene);

        private void Update()
        {
            if (van == null) return;
            foreach (Poi poi in pois)
            {
                if (busy.Contains(poi.scene)) continue;
                float d = Vector3.Distance(van.position, poi.position);
                if (!loaded.Contains(poi.scene) && d < loadDistance) Load(poi.scene);
                else if (loaded.Contains(poi.scene) && d > unloadDistance) Unload(poi.scene);
            }
        }

        private void Load(string scene)
        {
            busy.Add(scene);
            SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive).completed += _ =>
            {
                busy.Remove(scene);
                loaded.Add(scene);
            };
        }

        private void Unload(string scene)
        {
            busy.Add(scene);
            SceneManager.UnloadSceneAsync(scene).completed += _ =>
            {
                busy.Remove(scene);
                loaded.Remove(scene);
            };
        }
    }
}
