using System.IO;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>Refines existing cosmetic emitters; their placement, triggers and gameplay collision remain authored.</summary>
    public static class VisualEffectsRemaster
    {
        public const string ConfigPath = "Assets/_Project/Data/Core/Resources/VisualEffectsConfig.asset";
        private const string Folder = "Assets/_Project/Art/Remaster/Effects";

        public static int ApplyToScene(Scene scene)
        {
            VisualEffectsConfig config = Settings();
            int count = 0;
            foreach (GameObject root in scene.GetRootGameObjects()) count += ApplyToRoot(root.transform, config);
            return count;
        }

        public static int ApplyToRoot(Transform root, VisualEffectsConfig config = null)
        {
            if (config == null) config = Settings();
            int count = 0;
            foreach (ParticleSystem particles in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
                if (renderer == null) continue;
                // Several authored room pools are named for their location; their material identifies the effect.
                string kind = Kind(particles.name) ?? Kind(renderer.sharedMaterial != null ? renderer.sharedMaterial.name : null);
                if (kind == null) continue;
                particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                renderer.sharedMaterial = Material(kind, config);
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = true;
                renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                renderer.sortMode = ParticleSystemSortMode.Distance;
                renderer.minParticleSize = 0f;
                renderer.maxParticleSize = kind == "steam" ? .12f : .025f;
                var main = particles.main;
                if (kind == "snow")
                {
                    main.startSize = new ParticleSystem.MinMaxCurve(config.SnowflakeSize.x, config.SnowflakeSize.y);
                    main.maxParticles = Mathf.Min(main.maxParticles, config.SnowPerEmitter);
                    main.startColor = new Color(.88f, .94f, 1f, .7f);
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;
                }
                else if (kind == "dust")
                {
                    main.startSize = new ParticleSystem.MinMaxCurve(config.DustSize.x, config.DustSize.y);
                    main.maxParticles = Mathf.Min(main.maxParticles, config.DustPerEmitter);
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;
                }
                else if (kind == "spark")
                {
                    renderer.renderMode = ParticleSystemRenderMode.Stretch;
                    renderer.lengthScale = 1.4f;
                    renderer.velocityScale = .035f;
                    main.startSize = new ParticleSystem.MinMaxCurve(.009f, .025f);
                }
                var colour = particles.colorOverLifetime;
                colour.enabled = true;
                colour.color = Fade();
                // Existing material streams may have omitted normals because Simple Lit supported only the sun.
                renderer.SetActiveVertexStreams(new System.Collections.Generic.List<ParticleSystemVertexStream>
                {
                    ParticleSystemVertexStream.Position, ParticleSystemVertexStream.Normal,
                    ParticleSystemVertexStream.Color, ParticleSystemVertexStream.UV
                });
                EditorUtility.SetDirty(particles);
                EditorUtility.SetDirty(renderer);
                count++;
            }
            return count;
        }

        public static string Kind(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            string n = name.ToLowerInvariant();
            if (n.Contains("snow")) return "snow";
            if (n.Contains("dust") || n.Contains("mote")) return "dust";
            if (n.Contains("spark") || n.Contains("ember")) return "spark";
            if (n.Contains("steam") || n.Contains("smoke") || n.Contains("plume")) return "steam";
            return null;
        }

        private static VisualEffectsConfig Settings()
        {
            var config = AssetDatabase.LoadAssetAtPath<VisualEffectsConfig>(ConfigPath);
            if (config != null) return config;
            PolishAssets.EnsureFolder(Path.GetDirectoryName(ConfigPath).Replace('\\', '/'));
            config = ScriptableObject.CreateInstance<VisualEffectsConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            return config;
        }

        private static Material Material(string kind, VisualEffectsConfig config)
        {
            PolishAssets.EnsureFolder(Folder);
            string path = $"{Folder}/{kind}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool additive = kind == "spark";
            Shader shader = Shader.Find(additive ? "Universal Render Pipeline/Particles/Unlit" : "Universal Render Pipeline/Particles/Lit");
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
            material.shader = shader;
            material.SetTexture("_BaseMap", Texture(kind));
            material.SetColor("_BaseColor", kind switch
            {
                "dust" => new Color(.84f, .8f, .7f, config.DustOpacity),
                "steam" => new Color(.72f, .75f, .79f, config.SteamOpacity),
                "spark" => new Color(2.4f, 1.1f, .35f, .85f),
                _ => Color.white
            });
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", additive ? 2f : 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", (float)CullMode.Off);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesNearFadeDistance", 0f);
            material.SetFloat("_SoftParticlesFarFadeDistance", .45f);
            material.SetVector("_SoftParticleFadeParams", new Vector4(0f, 1f / .45f, 0f, 0f));
            material.SetFloat("_CameraFadingEnabled", 1f);
            material.SetFloat("_CameraNearFadeDistance", .08f);
            material.SetFloat("_CameraFarFadeDistance", 1f);
            material.SetVector("_CameraFadeParams", new Vector4(.08f, 1f / .92f, 0f, 0f));
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_SOFTPARTICLES_ON");
            material.EnableKeyword("_FADING_ON");
            material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Texture2D Texture(string kind)
        {
            string path = $"{Folder}/{kind}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true, true);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (x + .5f) / size * 2f - 1f, v = (y + .5f) / size * 2f - 1f;
                float r = Mathf.Sqrt(u * u + v * v);
                float alpha = Mathf.Exp(-(u * u + v * v) * 7f) * Mathf.Clamp01((1f - r) * 8f);
                if (kind == "snow")
                {
                    float spokes = Mathf.Pow(Mathf.Abs(Mathf.Cos(Mathf.Atan2(v, u) * 3f)), 12f);
                    alpha *= Mathf.Lerp(.35f, 1f, spokes);
                }
                else if (kind == "steam")
                    alpha *= .45f + .55f * Mathf.PerlinNoise(u * 3f + 41.3f, v * 3f + 17.9f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.sRGBTexture = false;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Trilinear;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Gradient Fade()
        {
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(.85f, .12f), new GradientAlphaKey(.65f, .72f), new GradientAlphaKey(0f, 1f) });
            return gradient;
        }
    }
}
