using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Emergency override: forces the open scene to a dark night look (near-black trilight ambient, dim moonlight, dense
    /// fog and a priority-999 global volume) when its lighting reads blown out. It sits on top of the map's own tuned look
    /// (AvLook, LightingRebuild), so it is a stop-gap: rebuilding the map puts the tuned look back. Volume components are
    /// found by name, so the same code serves URP and HDRP.
    /// </summary>
    public static class HorrorAtmosphereRestorer
    {
        private const string ProfilePath = "Assets/Settings/EmergencyHorrorGlobalProfile.asset";
        private const string VolumeName = "Emergency Horror Volume";

        [MenuItem(ToolMenu.Art + "Emergency Triage: Force Restore Horror Atmosphere (Open Scene)", priority = ToolMenu.ArtP + 120)]
        public static void Restore()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!Application.isBatchMode && !EditorUtility.DisplayDialog("Force horror atmosphere",
                    $"This overrides {scene.name}'s tuned lighting with a dark night look (ambient, sun, fog and a priority-999 volume). " +
                    "Rebuilding the map restores the tuned look. Daylight maps (the Prison, the training house) will turn to night.", "Override", "Cancel"))
                return;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.020f, 0.022f, 0.030f);
            RenderSettings.ambientEquatorColor = new Color(0.012f, 0.013f, 0.016f);
            RenderSettings.ambientGroundColor = new Color(0.005f, 0.005f, 0.006f);
            RenderSettings.ambientIntensity = 0.1f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.012f, 0.014f, 0.018f);
            RenderSettings.fogDensity = 0.045f;

            int suns = Moonlight(scene);
            VolumeProfile profile = Profile();
            Volume(scene, profile);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[Triage] {scene.name}: horror atmosphere forced ({suns} directional light(s), volume '{VolumeName}' -> {ProfilePath}). Save the scene to keep it.");
        }

        private static int Moonlight(Scene scene)
        {
            Light[] suns = Object.FindObjectsByType<Light>(FindObjectsSortMode.None)
                .Where(l => l.gameObject.scene == scene && l.type == LightType.Directional && l.isActiveAndEnabled).ToArray();
            Undo.RecordObjects(suns, "Force horror moonlight");
            foreach (Light sun in suns)
            {
                sun.intensity = Mathf.Clamp(sun.intensity, 0.05f, 0.12f);
                sun.color = new Color(0.62f, 0.70f, 0.86f);
                sun.shadows = LightShadows.Soft;
                sun.shadowStrength = 0.95f;
                sun.shadowBias = 0.02f;
                sun.shadowNormalBias = 0.2f;
                // URP ignores the light's own bias while its additional data says "use pipeline settings".
                foreach (Component extra in sun.GetComponents<Component>())
                    if (extra != null && extra.GetType().Name == "UniversalAdditionalLightData")
                    {
                        Undo.RecordObject(extra, "Force horror moonlight");
                        extra.GetType().GetProperty("usePipelineSettings")?.SetValue(extra, false);
                    }
                EditorUtility.SetDirty(sun);
            }
            return suns.Length;
        }

        private static VolumeProfile Profile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }
            bool hdrp = GraphicsSettings.currentRenderPipeline != null && GraphicsSettings.currentRenderPipeline.GetType().Name.Contains("HDRenderPipeline");
            Set(profile, "Tonemapping", hdrp, ("mode", "ACES"));
            Set(profile, "ColorAdjustments", hdrp, ("postExposure", -0.65f), ("contrast", 28f), ("saturation", -22f));
            Set(profile, "Vignette", hdrp, ("intensity", 0.42f), ("smoothness", 0.45f));
            // A fixed exposure stops the eye-adaptation histogram from lifting dark rooms to grey or blowing out lamps:
            // HDRP's Exposure component, or this project's own URP eye adaptation (Abandoned.Core.AutoExposureOverride).
            if (hdrp) Set(profile, "Exposure", true, ("mode", "Fixed"));
            else Set(profile, "AutoExposureOverride", false, ("enabled", false));
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static void Volume(Scene scene, VolumeProfile profile)
        {
            GameObject go = scene.GetRootGameObjects().FirstOrDefault(g => g.name == VolumeName);
            if (go == null)
            {
                go = new GameObject(VolumeName);
                SceneManager.MoveGameObjectToScene(go, scene);
                Undo.RegisterCreatedObjectUndo(go, "Create emergency horror volume");
            }
            // Not `??`: a missing component is Unity's fake null, not C# null.
            Volume volume = go.GetComponent<Volume>();
            if (volume == null) volume = Undo.AddComponent<Volume>(go);
            volume.isGlobal = true;
            volume.priority = 999f;
            volume.weight = 1f;
            volume.sharedProfile = profile;
            EditorUtility.SetDirty(volume);
        }

        private static void Set(VolumeProfile profile, string typeName, bool hdrp, params (string field, object value)[] values)
        {
            Type type = ComponentType(typeName, hdrp);
            if (type == null)
            {
                Debug.LogWarning($"[Triage] No volume component named {typeName} in this project; skipped.");
                return;
            }
            VolumeComponent component = profile.components.FirstOrDefault(c => c != null && c.GetType() == type);
            if (component == null)
            {
                component = profile.Add(type);
                // A component added to a profile asset is only saved if it is a sub-asset of that profile.
                component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(component, profile);
            }
            component.active = true;
            foreach ((string field, object value) in values)
            {
                if (type.GetField(field, BindingFlags.Instance | BindingFlags.Public)?.GetValue(component) is not VolumeParameter parameter)
                {
                    Debug.LogWarning($"[Triage] {typeName}.{field} not found; skipped.");
                    continue;
                }
                PropertyInfo property = parameter.GetType().GetProperty("value", BindingFlags.Instance | BindingFlags.Public);
                Type target = property.PropertyType;
                property.SetValue(parameter, target.IsEnum ? Enum.Parse(target, (string)value) : Convert.ChangeType(value, target));
                parameter.overrideState = true;
            }
            EditorUtility.SetDirty(component);
        }

        private static Type ComponentType(string name, bool hdrp)
        {
            string preferred = hdrp ? "UnityEngine.Rendering.HighDefinition" : "UnityEngine.Rendering.Universal";
            Type[] matches = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); } })
                .Where(t => t.Name == name && typeof(VolumeComponent).IsAssignableFrom(t) && !t.IsAbstract).ToArray();
            return matches.FirstOrDefault(t => t.Namespace == preferred) ?? matches.FirstOrDefault();
        }
    }
}
