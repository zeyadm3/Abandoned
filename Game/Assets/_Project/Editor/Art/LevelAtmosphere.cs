using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A level's mood (M7.2): gradient ambient, fog, the sun (soft shadows), and a global
    /// post-processing volume (ACES and restrained bloom, without grain or vignette), with
    /// post-processing switched on for the scene's cameras. Realtime only: no lightmaps (power-off
    /// runs switch lights and floors collapse, which baked light can't follow).
    /// </summary>
    public static class LevelAtmosphere
    {
        public readonly struct Mood
        {
            public readonly Color Sky, Equator, Ground, Fog;
            public readonly float FogDensity, Bloom, Vignette, Grain, Saturation, Contrast;
            public readonly Color SunColor;
            public readonly float SunIntensity;
            public readonly Vector3 SunEuler;

            public Mood(Color sky, Color equator, Color ground, Color fog, float fogDensity, float bloom, float vignette,
                float grain, float saturation, float contrast, Color sunColor, float sunIntensity, Vector3 sunEuler)
            {
                Sky = sky; Equator = equator; Ground = ground; Fog = fog; FogDensity = fogDensity;
                Bloom = bloom; Vignette = vignette; Grain = grain; Saturation = saturation; Contrast = contrast;
                SunColor = sunColor; SunIntensity = sunIntensity; SunEuler = sunEuler;
            }
        }

        /// <summary>A dim, dusty, cold mall; warm late-afternoon sun outside and through the skylight.</summary>
        public static readonly Mood Mall = new(
            sky: new Color(0.09f, 0.105f, 0.12f), equator: new Color(0.055f, 0.064f, 0.07f), ground: new Color(0.026f, 0.025f, 0.022f),
            fog: new Color(0.19f, 0.205f, 0.21f), fogDensity: 0.014f, bloom: 0.2f, vignette: 0f, grain: 0f,
            saturation: -8f, contrast: 6f, sunColor: new Color(1f, 0.91f, 0.77f), sunIntensity: 1.7f, sunEuler: new Vector3(66f, -32f, 0f));

        /// <summary>The company HQ: warmer and cleaner (a safe place), light haze.</summary>
        public static readonly Mood Hq = new(
            sky: new Color(0.46f, 0.44f, 0.42f), equator: new Color(0.36f, 0.33f, 0.3f), ground: new Color(0.16f, 0.15f, 0.13f),
            fog: new Color(0.3f, 0.28f, 0.26f), fogDensity: 0.006f, bloom: 0.18f, vignette: 0f, grain: 0f,
            saturation: -5f, contrast: 8f, sunColor: new Color(1f, 0.92f, 0.8f), sunIntensity: 1.4f, sunEuler: new Vector3(50f, -30f, 0f));

        public static void Apply(string levelName, Mood mood, Transform parent)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = mood.Sky;
            RenderSettings.ambientEquatorColor = mood.Equator;
            RenderSettings.ambientGroundColor = mood.Ground;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = mood.Fog;
            RenderSettings.fogDensity = mood.FogDensity;

            foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (l.type != LightType.Directional) continue;
                l.transform.rotation = Quaternion.Euler(mood.SunEuler);
                l.color = mood.SunColor;
                l.intensity = mood.SunIntensity;
                l.shadows = LightShadows.Soft;
                RenderSettings.sun = l;
            }

            var volume = new GameObject("PostProcessing").AddComponent<Volume>();
            volume.transform.SetParent(parent, false);
            volume.isGlobal = true;
            volume.sharedProfile = Profile(levelName, mood);

            foreach (Camera c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                c.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        }

        /// <summary>Loads or creates Art/Lighting/{level}_Volume.asset and sets its overrides in place.</summary>
        private static VolumeProfile Profile(string levelName, Mood mood)
        {
            string path = $"{LightingAssets.Folder}/{levelName}_Volume.asset";
            LightingAssets.FixturePanel(); // makes sure the folder exists
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            Get<Tonemapping>(profile).mode.Override(TonemappingMode.ACES);
            Bloom bloom = Get<Bloom>(profile);
            bloom.threshold.Override(1.1f);
            bloom.intensity.Override(mood.Bloom);
            bloom.scatter.Override(0.6f);
            Vignette vignette = Get<Vignette>(profile);
            vignette.intensity.Override(mood.Vignette);
            vignette.smoothness.Override(0.45f);
            FilmGrain grain = Get<FilmGrain>(profile);
            grain.type.Override(FilmGrainLookup.Thin2);
            grain.intensity.Override(mood.Grain);
            ColorAdjustments color = Get<ColorAdjustments>(profile);
            color.saturation.Override(mood.Saturation);
            color.contrast.Override(mood.Contrast);
            EditorUtility.SetDirty(profile);
            return profile;
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
