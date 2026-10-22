using Abandoned.Core;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The TestMap's sky and its baked light: a photographed golden-hour sky (Poly Haven's evening_road_01, CC0) lights
    /// the scene and its reflections, the sun stands where the sky's own sun is (a little higher, so the forest doesn't
    /// put the whole yard in shade), and the static parts get baked bounce light, sky occlusion and distant shadows.
    /// Only the TestMap does this: the real maps stay realtime because their power goes off and their floors fall
    /// (M7.2). Things that move or collapse are lit by the light probes laid out here.
    /// </summary>
    public static class ShowcaseSky
    {
        private const string Folder = "Assets/_Project/Art/TestMap/Sky";
        private const string Hdri = Folder + "/evening_road_01_puresky_4k.hdr";
        public const string SettingsPath = Folder + "/TestMap_Lighting.lighting";

        // Where the photographed sun is in the panorama (u across, measured by fetch time), and where we want it:
        // west-south-west, so the light rakes across the warehouse's south front and the forecourt.
        private const float SunU = .5967f;
        public const float SunAzimuth = -152f, SunElevation = 16f;
        public static readonly Color SunColour = new(1f, .7f, .45f);
        public const float SunIntensity = 5f;

        /// <summary>Unit vector toward the sun.</summary>
        public static Vector3 ToSun
        {
            get
            {
                float a = SunAzimuth * Mathf.Deg2Rad, e = SunElevation * Mathf.Deg2Rad;
                return new Vector3(Mathf.Cos(e) * Mathf.Cos(a), Mathf.Sin(e), Mathf.Cos(e) * Mathf.Sin(a));
            }
        }

        public static void Apply(Scene scene, Transform root)
        {
            RenderSettings.skybox = SkyMaterial();
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = .65f; // the photographed sky is bright; at 1 it flattens the sun
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.defaultReflectionResolution = 256;
            RenderSettings.reflectionIntensity = 1f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            // Warm haze toward the low sun: the hills behind fade, the far ring goes to a silhouette.
            RenderSettings.fogColor = new Color(.78f, .68f, .58f);
            RenderSettings.fogDensity = .0045f;

            foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type != LightType.Directional || light.gameObject.scene != scene) continue;
                light.transform.rotation = Quaternion.LookRotation(-ToSun);
                light.color = SunColour;
                light.intensity = SunIntensity;
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 1f;
                light.lightmapBakeType = LightmapBakeType.Mixed;
                light.bounceIntensity = 1.2f;
                RenderSettings.sun = light;
            }
            Grade(root);
            Lightmapping.SetLightingSettingsForScene(scene, Settings());
            Probes(root);
            DynamicGI.UpdateEnvironment();
        }

        // The TestMap's grade on top of AvLook's: a golden-hour key, cool shadows, more contrast, haze that reaches the trees.
        private static void Grade(Transform root)
        {
            Transform post = root.Find("PostProcessing");
            VolumeProfile profile = post != null && post.TryGetComponent(out Volume volume) ? volume.sharedProfile : null;
            if (profile == null) return;
            if (profile.TryGet(out UnityEngine.Rendering.Universal.ColorAdjustments adjust))
            {
                adjust.contrast.Override(18f);
                adjust.saturation.Override(4f);
                adjust.postExposure.Override(-.15f);
            }
            if (profile.TryGet(out UnityEngine.Rendering.Universal.WhiteBalance balance)) balance.temperature.Override(10f);
            if (profile.TryGet(out UnityEngine.Rendering.Universal.ShadowsMidtonesHighlights split))
            {
                split.shadows.Override(new Vector4(.9f, .97f, 1.1f, -.05f));
                split.highlights.Override(new Vector4(1.05f, 1f, .94f, 0f));
            }
            if (profile.TryGet(out UnityEngine.Rendering.Universal.Bloom bloom))
            {
                bloom.intensity.Override(.25f);
                bloom.threshold.Override(1f);
            }
            if (profile.TryGet(out UnityEngine.Rendering.Universal.Vignette vignette)) vignette.intensity.Override(.2f);
            if (profile.TryGet(out VolumetricFogOverride fog))
            {
                fog.density.Override(.004f);
                fog.maxDistance.Override(140f);
                fog.mainLight.Override(1.3f);
            }
            EditorUtility.SetDirty(profile);
        }

        private static Material SkyMaterial()
        {
            if (AssetImporter.GetAtPath(Hdri) is TextureImporter importer &&
                (importer.textureShape != TextureImporterShape.TextureCube || importer.maxTextureSize != 4096))
            {
                importer.textureShape = TextureImporterShape.TextureCube;
                importer.generateCubemap = TextureImporterGenerateCubemap.Cylindrical;
                importer.maxTextureSize = 4096;
                importer.mipmapEnabled = true;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
            string path = Folder + "/TestMap_Sky.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Skybox/Cubemap"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetTexture("_Tex", AssetDatabase.LoadAssetAtPath<Cubemap>(Hdri));
            // The panorama's sun is at atan2(z, x) = (0.5 - u) * 360; the shader turns the view direction by _Rotation.
            material.SetFloat("_Rotation", Mathf.Repeat((.5f - SunU) * 360f - SunAzimuth, 360f));
            material.SetFloat("_Exposure", 1f);
            material.SetColor("_Tint", new Color(.5f, .5f, .5f));
            EditorUtility.SetDirty(material);
            return material;
        }

        private static LightingSettings Settings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LightingSettings>(SettingsPath);
            if (settings == null)
            {
                settings = new LightingSettings { name = "TestMap_Lighting" };
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }
            settings.bakedGI = true;
            settings.realtimeGI = false;
            settings.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
            settings.mixedBakeMode = MixedLightingMode.Shadowmask;
            settings.directionalityMode = LightmapsMode.CombinedDirectional;
            settings.lightmapResolution = 5f;
            settings.lightmapPadding = 3;
            settings.lightmapMaxSize = 2048;
            settings.lightmapCompression = LightmapCompression.HighQuality;
            settings.ao = true;
            settings.aoMaxDistance = 1.4f;
            settings.aoExponentDirect = 0f;
            settings.aoExponentIndirect = 1f;
            settings.directSampleCount = 32;
            settings.indirectSampleCount = 384;
            settings.environmentSampleCount = 384;
            settings.maxBounces = 3;
            settings.albedoBoost = 1f;
            settings.indirectScale = 1f;
            settings.filteringMode = LightingSettings.FilterMode.Auto;
            EditorUtility.SetDirty(settings);
            return settings;
        }

        /// <summary>Light probes where moving things go: through the warehouse at every height a player or a fallen floor
        /// reaches, the gallery, and over the yard; coarser over the land outside.</summary>
        private static void Probes(Transform root)
        {
            var points = new List<Vector3>();
            void Grid(Rect r, float step, params float[] heights)
            {
                for (float z = r.yMin; z <= r.yMax + .01f; z += step)
                for (float x = r.xMin; x <= r.xMax + .01f; x += step)
                foreach (float y in heights)
                    points.Add(new Vector3(x, y, z));
            }
            Grid(Rect.MinMaxRect(.6f, .6f, 19.4f, 15.4f), 2.4f, .5f, 1.8f, 3.4f, 4.6f, 6.2f);   // warehouse
            Grid(Rect.MinMaxRect(-5.4f, 20.6f, 25.4f, 33.4f), 3f, .6f, 2.2f, 4.2f);              // gallery
            Grid(Rect.MinMaxRect(-38f, -52f, 60f, 50f), 6f, .8f, 3.6f, 9f);                       // the yard
            var keep = new List<Vector3>();
            foreach (Vector3 p in points)
            {
                bool inWarehouse = p.x > 0f && p.x < 20f && p.z > 0f && p.z < 16f;
                bool inGallery = p.x > -6f && p.x < 26f && p.z > 20f && p.z < 34f;
                // Yard probes stay out of the buildings (the building grids cover those) and out of solid walls.
                if (!inWarehouse && !inGallery && (p.x > -1f && p.x < 21f && p.z > -1f && p.z < 17f ||
                                                   p.x > -7f && p.x < 27f && p.z > 19f && p.z < 35f)) continue;
                if (Physics.CheckSphere(p, .15f, ~0, QueryTriggerInteraction.Ignore)) continue;
                keep.Add(p);
            }
            Transform old = root.Find("LightProbes");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var group = new GameObject("LightProbes").AddComponent<LightProbeGroup>();
            group.transform.SetParent(root, false);
            group.probePositions = keep.ToArray();
        }

        /// <summary>Makes every static renderer lightmap-ready: models without a lightmap UV set are lit by the probes
        /// instead (their own UVs overlap and would bake as blotches), and small things don't spend lightmap space.</summary>
        public static void PrepareStatics(Scene scene)
        {
            int probes = 0, mapped = 0;
            foreach (GameObject top in scene.GetRootGameObjects())
            foreach (MeshRenderer renderer in top.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!GameObjectUtility.GetStaticEditorFlags(renderer.gameObject).HasFlag(StaticEditorFlags.ContributeGI)) continue;
                Mesh mesh = renderer.TryGetComponent(out MeshFilter filter) ? filter.sharedMesh : null;
                bool hasLightmapUv = mesh != null && mesh.isReadable ? mesh.uv2 != null && mesh.uv2.Length > 0 : mesh != null && mesh.HasVertexAttribute(VertexAttribute.TexCoord1);
                float size = renderer.bounds.size.magnitude;
                if (!hasLightmapUv || size < 1.2f)
                {
                    renderer.receiveGI = ReceiveGI.LightProbes;
                    probes++;
                }
                else
                {
                    renderer.receiveGI = ReceiveGI.Lightmaps;
                    mapped++;
                }
            }
            Debug.Log($"[Showcase] Baked light: {mapped} renderers lightmapped, {probes} lit by probes.");
        }

        /// <summary>Bakes now (the editor waits a few minutes) and saves the scene with its lighting data: the last step of
        /// Rebuild Test Map.</summary>
        public static void BakeAndSave(Scene scene)
        {
            PrepareStatics(scene);
            bool baked = Lightmapping.Bake();
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Showcase] TestMap lighting {(baked ? "baked" : "NOT baked")}: {LightmapSettings.lightmaps.Length} lightmaps.");
        }

        /// <summary>Bakes again without rebuilding (the editor stays usable; the scene saves its lighting data when done).</summary>
        [MenuItem(ToolMenu.TestMap + "Bake Test Map Lighting", priority = ToolMenu.TestMapP + 1)]
        public static void Bake()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != TestMapBuilder.ScenePath)
            {
                Debug.LogWarning("[Showcase] Open the TestMap first.");
                return;
            }
            PrepareStatics(scene);
            Lightmapping.bakeCompleted -= Saved;
            Lightmapping.bakeCompleted += Saved;
            Lightmapping.BakeAsync();
        }

        private static void Saved()
        {
            Lightmapping.bakeCompleted -= Saved;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != TestMapBuilder.ScenePath) return;
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Showcase] TestMap lighting baked: {LightmapSettings.lightmaps.Length} lightmaps.");
        }

        public static bool HasSky => File.Exists(Hdri);
    }
}
