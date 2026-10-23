using Abandoned.Core;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Abandoned.World
{
    /// <summary>
    /// Fits a place's scene into the world as it streams in, on every machine: the world's camera stays in charge, so
    /// the scene's own is switched off, and so are its far horizons and its own collection truck (the world's land and
    /// the crew's van replace them). Its sun, sky and grade take over from the world's while it is loaded
    /// (<see cref="WorldRoot.UsePlaceLook"/>). The scene's files are never changed.
    /// </summary>
    public static class PoiAdapter
    {
        public static void Adopt(Scene scene, WorldPoi poi)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Camera camera in root.GetComponentsInChildren<Camera>(true))
                    if (camera.CompareTag("MainCamera") || camera.GetComponent<CinemachineBrain>() != null)
                    {
                        camera.enabled = false;
                        if (camera.TryGetComponent(out CinemachineBrain brain)) brain.enabled = false;
                    }
                foreach (AudioListener listener in root.GetComponentsInChildren<AudioListener>(true)) listener.enabled = false;
                // Without a captured look the world's sun and grade stay; two suns would double the light.
                foreach (Light light in root.GetComponentsInChildren<Light>(true))
                    if (light.type == LightType.Directional)
                    {
                        if (!poi.look.captured) light.enabled = false;
                        else if (light.enabled && RenderSettings.sun == null) RenderSettings.sun = light;
                    }
                if (!poi.look.captured)
                    foreach (Volume volume in root.GetComponentsInChildren<Volume>(true))
                        if (volume.isGlobal) volume.enabled = false;
                // A second F1 switch would toggle the overlays twice per press.
                foreach (DebugViewToggle toggle in root.GetComponentsInChildren<DebugViewToggle>(true)) toggle.enabled = false;
                // The HQ's garage is the crew's way out to the van in the yard (its staff door is locked for good).
                foreach (Company.GarageDoor door in root.GetComponentsInChildren<Company.GarageDoor>(true)) door.SetOpen(1f);
            }
            foreach (string path in poi.hideInWorld)
            {
                GameObject found = Find(scene, path);
                if (found != null) found.SetActive(false);
                else Debug.LogWarning($"[World] {poi.scene}: nothing at {path} to hide.");
            }
        }

        private static GameObject Find(Scene scene, string path)
        {
            int slash = path.IndexOf('/');
            string rootName = slash < 0 ? path : path.Substring(0, slash);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name != rootName) continue;
                if (slash < 0) return root;
                Transform t = root.transform.Find(path.Substring(slash + 1));
                if (t != null) return t.gameObject;
            }
            return null;
        }
    }
}
