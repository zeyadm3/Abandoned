using System.IO;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    public static class MeridianAtmosphereDust
    {
        private const string Folder = "Assets/_Project/Art/Generated/MeridianAtmosphere";

        public static Material Material()
        {
            PolishAssets.EnsureFolder(Folder);
            string texturePath = Folder + "/DustDot.png", materialPath = Folder + "/Dust.mat";
            Texture2D dot = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (dot == null)
            {
                const int size = 512;
                var image = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float radius = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), Vector2.one * size * 0.5f) / (size * 0.5f);
                    float alpha = Mathf.Pow(Mathf.Clamp01(1f - radius * radius), 2f);
                    pixels[y * size + x] = new Color32(239, 229, 207, (byte)(alpha * 255f));
                }
                image.SetPixels32(pixels);
                image.Apply();
                File.WriteAllBytes(texturePath, image.EncodeToPNG());
                Object.DestroyImmediate(image);
                AssetDatabase.ImportAsset(texturePath);
                var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = 512;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
                {
                    name = "Standalone", overridden = true, maxTextureSize = 512, format = TextureImporterFormat.BC7,
                });
                importer.SaveAndReimport();
                dot = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            }
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Particles/Simple Lit"));
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.SetTexture("_BaseMap", dot);
            material.SetColor("_BaseColor", new Color(1f, 0.97f, 0.9f, 0.22f));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        public static void Motes(Transform air, Vector3 size, Material material, MeridianPresentationConfig config)
        {
            int count = Mathf.Clamp(Mathf.RoundToInt(size.x * size.y * size.z * config.DustPerCubicMetre), 0, config.DustPerZoneLimit);
            if (count == 0) return;
            var particles = air.gameObject.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(13f, 21f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.025f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.012f, 0.026f);
            main.maxParticles = count;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = count / 17f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;
            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.035f;
            noise.frequency = 0.15f;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }
}
