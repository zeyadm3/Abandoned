using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The 0.24.12 look, added on top of each map's mood (LevelAtmosphere / MeridianAtmosphereVolume call it, and
    /// the menu applies it to the saved volume assets without rebuilding a map): volumetric fog, eye adaptation, a
    /// per-map HDR grade (white balance, split-toned shadows), lens dirt in the bloom and a trace of chromatic
    /// aberration. Values are picked per map from its name.
    /// </summary>
    public static class AvLook
    {
        private readonly struct Preset
        {
            public readonly float Density, Falloff, MaxDistance, Ambient, MainLight, LocalLights, Noise, Key, Temperature, Tint;
            public readonly Color Albedo, Shadows;

            public Preset(float density, float falloff, float maxDistance, float ambient, float mainLight, float localLights, float noise,
                float key, float temperature, float tint, Color albedo, Color shadows)
            {
                Density = density; Falloff = falloff; MaxDistance = maxDistance; Ambient = ambient; MainLight = mainLight;
                LocalLights = localLights; Noise = noise; Key = key; Temperature = temperature; Tint = tint; Albedo = albedo; Shadows = shadows;
            }
        }

        // Dusty mall under failing tubes: thick, slightly green, cold shadows.
        // Ambient is a share of the map's own fog colour; local lights are scaled well down (a 28-unit torch in dust is
        // blinding at 1:1).
        private static readonly Preset Mall = new(0.0045f, 0.04f, 45f, 0.18f, 0.8f, 0.16f, 0.3f, 0.11f, -6f, 4f, new Color(0.86f, 0.86f, 0.8f), new Color(0.94f, 1f, 1.08f));
        // The company's own building: a light haze, warm.
        private static readonly Preset Hq = new(0.0015f, 0.05f, 40f, 0.15f, 0.7f, 0.12f, 0.2f, 0.12f, 5f, 0f, new Color(0.9f, 0.87f, 0.82f), new Color(1f, 0.99f, 1.02f));
        // Iron works: smoke and heat, warm light against blue-black shadows.
        private static readonly Preset Factory = new(0.007f, 0.02f, 55f, 0.2f, 0.8f, 0.2f, 0.5f, 0.1f, 12f, 0f, new Color(0.8f, 0.76f, 0.7f), new Color(0.92f, 0.98f, 1.1f));
        // Island prison: sea mist outside, damp cold inside.
        private static readonly Preset Prison = new(0.0035f, 0.03f, 70f, 0.2f, 0.8f, 0.14f, 0.35f, 0.11f, -10f, 0f, new Color(0.88f, 0.9f, 0.92f), new Color(0.93f, 1f, 1.07f));
        // Winter afternoon school: clear air, low sun through tall windows.
        private static readonly Preset School = new(0.002f, 0.04f, 60f, 0.15f, 0.9f, 0.12f, 0.25f, 0.12f, -8f, 0f, new Color(0.9f, 0.9f, 0.92f), new Color(0.95f, 1f, 1.05f));
        // December 3 PM house and neighbourhood.
        private static readonly Preset Tutorial = new(0.0018f, 0.035f, 60f, 0.15f, 0.8f, 0.12f, 0.2f, 0.12f, -5f, 0f, new Color(0.9f, 0.9f, 0.92f), new Color(0.96f, 1f, 1.04f));
        // The TestMap graphics showcase (2026-10-22): late afternoon, light dust in the sun shafts, a brighter key so
        // nothing reads as murk, warm whites.
        private static readonly Preset TestMap = new(0.0025f, 0.04f, 50f, 0.16f, 0.95f, 0.14f, 0.25f, 0.14f, 4f, 0f, new Color(0.9f, 0.88f, 0.84f), new Color(0.97f, 1f, 1.04f));

        public const string LensDirtPath = "Assets/_Project/Art/Lighting/LensDirt.png";


        [MenuItem(ToolMenu.Art + "Apply 0.24.12 Look to Saved Maps", priority = ToolMenu.ArtP + 60)]
        public static void ApplyToSaved()
        {
            int count = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:VolumeProfile", new[] { "Assets/_Project/Art" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string file = System.IO.Path.GetFileNameWithoutExtension(path);
                if (!file.EndsWith("_Volume")) continue;
                Apply(AssetDatabase.LoadAssetAtPath<VolumeProfile>(path), file.Substring(0, file.Length - "_Volume".Length));
                count++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[AvLook] 0.24.12 look applied to {count} map volume profiles.");
        }

        public static void Apply(VolumeProfile profile, string levelName)
        {
            if (profile == null) return;
            Preset p = For(levelName);

            VolumetricFogOverride fog = Get<VolumetricFogOverride>(profile);
            fog.density.Override(p.Density);
            fog.heightFalloff.Override(p.Falloff);
            fog.maxDistance.Override(p.MaxDistance);
            fog.ambient.Override(p.Ambient);
            fog.mainLight.Override(p.MainLight);
            fog.localLights.Override(p.LocalLights);
            fog.noise.Override(p.Noise);
            fog.albedo.Override(p.Albedo);
            fog.anisotropy.Override(0.35f);
            fog.unshadowedLights.Override(0f);

            AutoExposureOverride eye = Get<AutoExposureOverride>(profile);
            eye.enabled.Override(true);
            eye.key.Override(p.Key);
            eye.strength.Override(0.45f);
            eye.minEv.Override(-0.8f);
            eye.maxEv.Override(1.25f);
            eye.speedDown.Override(2f);
            eye.speedUp.Override(0.55f);

            // 0.24.13: the scanned surfaces carry their own colour now; past -22 the grade washed them back to grey.
            ColorAdjustments adjust = Get<ColorAdjustments>(profile);
            adjust.saturation.Override(levelName.ToLowerInvariant().Contains("factory") ? -10f : -6f);
            adjust.contrast.Override(8f);
            adjust.postExposure.Override(0f);
            Get<Tonemapping>(profile).mode.Override(TonemappingMode.ACES);
            Vignette vignette = Get<Vignette>(profile);
            vignette.intensity.Override(0.12f);
            vignette.smoothness.Override(0.65f);
            Get<FilmGrain>(profile).intensity.Override(0.035f);

            WhiteBalance balance = Get<WhiteBalance>(profile);
            balance.temperature.Override(p.Temperature);
            balance.tint.Override(p.Tint);
            ShadowsMidtonesHighlights split = Get<ShadowsMidtonesHighlights>(profile);
            split.shadows.Override(new Vector4(p.Shadows.r, p.Shadows.g, p.Shadows.b, 0f));

            Bloom bloom = Get<Bloom>(profile);
            bloom.highQualityFiltering.Override(true);
            bloom.threshold.Override(1.25f);
            bloom.intensity.Override(0.12f);
            bloom.scatter.Override(0.45f);
            // Tiny HDR outliers on wet metal must not flood an entire room with a blurred white halo.
            bloom.clamp.Override(8f);
            bloom.dirtTexture.Override(LensDirt());
            bloom.dirtIntensity.Override(0.12f);
            ChromaticAberration fringe = Get<ChromaticAberration>(profile);
            fringe.intensity.Override(0f);
            EditorUtility.SetDirty(profile);
        }

        private static Preset For(string level)
        {
            string n = level.ToLowerInvariant();
            if (n.Contains("hq")) return Hq;
            if (n.Contains("factory") || n.Contains("iron")) return Factory;
            if (n.Contains("prison") || n.Contains("island") || n.Contains("gallows")) return Prison;
            if (n.Contains("school") || n.Contains("burley")) return School;
            if (n.Contains("tutorial") || n.Contains("larch")) return Tutorial;
            if (n.Contains("testmap")) return TestMap;
            return Mall;
        }

        private static T Get<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet(out T existing)) return existing;
            T component = profile.Add<T>();
            component.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }

        /// <summary>A smudged-lens texture drawn once (soft blots and a few streaks); bloom lights it only around bright lights.</summary>
        private static Texture2D LensDirt()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(LensDirtPath);
            if (existing != null) return existing;
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            var rng = new System.Random(2412);
            for (int b = 0; b < 70; b++)
            {
                float cx = (float)rng.NextDouble() * size, cy = (float)rng.NextDouble() * size;
                float radius = 6f + (float)rng.NextDouble() * (b < 10 ? 70f : 26f);
                float strength = 0.08f + (float)rng.NextDouble() * 0.22f;
                Stamp(pixels, size, cx, cy, radius, radius, strength);
            }
            for (int s = 0; s < 8; s++) // wipe streaks
            {
                float cx = (float)rng.NextDouble() * size, cy = (float)rng.NextDouble() * size;
                Stamp(pixels, size, cx, cy, 90f + (float)rng.NextDouble() * 120f, 4f + (float)rng.NextDouble() * 6f, 0.12f);
            }
            for (int i = 0; i < pixels.Length; i++) pixels[i].a = 1f;
            tex.SetPixels(pixels);
            System.IO.File.WriteAllBytes(LensDirtPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(LensDirtPath);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(LensDirtPath);
        }

        private static void Stamp(Color[] pixels, int size, float cx, float cy, float rx, float ry, float strength)
        {
            int x0 = Mathf.Max(0, (int)(cx - rx)), x1 = Mathf.Min(size - 1, (int)(cx + rx));
            int y0 = Mathf.Max(0, (int)(cy - ry)), y1 = Mathf.Min(size - 1, (int)(cy + ry));
            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                float dx = (x - cx) / rx, dy = (y - cy) / ry;
                float d = dx * dx + dy * dy;
                if (d >= 1f) continue;
                float v = strength * (1f - d) * (1f - d);
                Color c = pixels[y * size + x];
                pixels[y * size + x] = new Color(c.r + v, c.g + v * 0.97f, c.b + v * 0.92f, 1f);
            }
        }
    }
}
