using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A map's photographed sky (Art/Skies, Poly Haven, CC0): drawn as the skybox, lighting the map's ambient and its
    /// reflections, and turned so its own sun stands where the map's sun already is (the map's light and shadows stay as
    /// they were tuned). The ambient keeps the level the map had: the old three-colour ambient is measured and the sky's
    /// ambient scaled to it, so every fixture LightingRebuild calibrated against it still lands the same in the rooms.
    /// </summary>
    public static class MapSky
    {
        public const string Folder = "Assets/_Project/Art/Skies";

        public static void Apply(MapSiteProfile profile)
        {
            // The light the map's rooms were tuned under: its three-colour ambient, which stays stored in the scene even
            // after this pass (so a second run measures the same target as the first).
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientIntensity = 1f;
            DynamicGI.UpdateEnvironment();
            float before = AmbientLevel();
            Material sky = Material(profile);
            Light sun = RenderSettings.sun;
            if (sun != null)
            {
                Vector3 toSun = -sun.transform.forward;
                float azimuth = Mathf.Atan2(toSun.z, toSun.x) * Mathf.Rad2Deg;
                // The panorama's sun is at atan2(z, x) = (0.5 - u) * 360; the shader turns the view direction by _Rotation.
                sky.SetFloat("_Rotation", Mathf.Repeat((.5f - profile.SkySunU) * 360f - azimuth, 360f));
            }
            RenderSettings.skybox = sky;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.defaultReflectionResolution = 256;
            // Unity applies the ambient intensity roughly to the power 2.2 in linear space: a few corrections, measuring
            // the ambient probe each time, land it on the old level.
            float intensity = 1f;
            for (int i = 0; i < 4; i++)
            {
                RenderSettings.ambientIntensity = intensity;
                DynamicGI.UpdateEnvironment();
                float now = AmbientLevel();
                if (now <= 1e-5f) break;
                intensity = Mathf.Clamp(intensity * Mathf.Pow(before / now, 1f / 2.2f), .05f, 8f);
            }
            RenderSettings.ambientIntensity = intensity;
            RenderSettings.reflectionIntensity = Mathf.Clamp(intensity, .1f, 1f);
            DynamicGI.UpdateEnvironment();
        }

        // The ambient probe's mean over the sky and the horizon (what lights a room through its windows), linear.
        private static float AmbientLevel()
        {
            var dirs = new[] { Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
            var colours = new Color[dirs.Length];
            RenderSettings.ambientProbe.Evaluate(dirs, colours);
            float sum = 0f;
            foreach (Color c in colours) sum += c.r * .2126f + c.g * .7152f + c.b * .0722f;
            return sum / dirs.Length;
        }

        private static Material Material(MapSiteProfile profile)
        {
            string hdri = $"{Folder}/{profile.Sky}_4k.hdr";
            if (AssetImporter.GetAtPath(hdri) is TextureImporter importer &&
                (importer.textureShape != TextureImporterShape.TextureCube || importer.maxTextureSize != 4096))
            {
                importer.textureShape = TextureImporterShape.TextureCube;
                importer.generateCubemap = TextureImporterGenerateCubemap.Cylindrical;
                importer.maxTextureSize = 4096;
                importer.mipmapEnabled = true;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
            string path = $"{Folder}/MapSky_{profile.Scene}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Skybox/Cubemap"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetTexture("_Tex", AssetDatabase.LoadAssetAtPath<Cubemap>(hdri));
            material.SetFloat("_Exposure", profile.SkyExposure);
            material.SetColor("_Tint", new Color(.5f, .5f, .5f));
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
