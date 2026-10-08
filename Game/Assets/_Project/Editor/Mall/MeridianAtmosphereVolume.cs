using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.EditorTools
{
    public static class MeridianAtmosphereVolume
    {
        private const string Folder = "Assets/_Project/Art/Generated/MeridianAtmosphere";

        public static void Apply(Transform parent, MeridianPresentationConfig config)
        {
            MeridianSkyBuilder.Apply(config);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = config.AmbientSky;
            RenderSettings.ambientEquatorColor = config.AmbientEquator;
            RenderSettings.ambientGroundColor = config.AmbientGround;
            RenderSettings.reflectionIntensity = config.ReflectionIntensity;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = config.ExteriorFog;
            RenderSettings.fogDensity = config.ExteriorFogDensity;
            foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type != LightType.Directional) continue;
                light.transform.rotation = Quaternion.Euler(config.MoonEuler);
                light.color = config.MoonColor;
                light.intensity = config.MoonIntensity;
                light.shadows = LightShadows.Soft;
                RenderSettings.sun = light;
            }
            PolishAssets.EnsureFolder(Folder);
            string path = Folder + "/Meridian_Volume.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            Get<Tonemapping>(profile).mode.Override(TonemappingMode.ACES);
            Bloom bloom = Get<Bloom>(profile);
            bloom.threshold.Override(1.1f);
            bloom.intensity.Override(config.Bloom);
            bloom.scatter.Override(0.55f);
            Vignette vignette = Get<Vignette>(profile);
            vignette.intensity.Override(config.Vignette);
            vignette.smoothness.Override(0.45f);
            FilmGrain grain = Get<FilmGrain>(profile);
            grain.type.Override(FilmGrainLookup.Thin2);
            grain.intensity.Override(config.Grain);
            ColorAdjustments color = Get<ColorAdjustments>(profile);
            color.saturation.Override(config.Saturation);
            color.contrast.Override(config.Contrast);
            EditorUtility.SetDirty(profile);
            var volume = new GameObject("PostProcessing").AddComponent<Volume>();
            volume.transform.SetParent(parent, false);
            volume.isGlobal = true;
            volume.sharedProfile = profile;
            foreach (Camera camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        }

        private static T Get<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet(out T existing)) return existing;
            T component = profile.Add<T>();
            component.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }
    }
}
