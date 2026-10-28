using System;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>Calibrates the saved maps without regenerating architecture, loot, navigation or gameplay.</summary>
    public static class LightingRebuild
    {
        public const string BounceConfigPath = "Assets/_Project/Data/Core/Resources/IndirectLightingConfig.asset";
        private const string ReflectionFallbackPath = "Assets/_Project/Art/Lighting/UnlitRoomReflection.asset";
        private const string RigName = "_LightingBounce";

        [MenuItem(ToolMenu.Art + "Rebuild Lighting and Shadows", priority = ToolMenu.ArtP + 81)]
        public static void ApplyMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try { ApplySaved(); }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        [MenuItem(ToolMenu.Art + "Rebuild Lighting and Shadows", true)]
        private static bool CanApply() => !EditorApplication.isPlayingOrWillChangePlaymode;

        public static void ApplyAndExit()
        {
            try { ApplySaved(); EditorApplication.Exit(0); }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        public static void ApplySaved()
        {
            RenderPipelineSetup.Apply();
            AvLook.ApplyToSaved();
            Budget();
            IndirectLightingConfig config = AssetDatabase.LoadAssetAtPath<IndirectLightingConfig>(BounceConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<IndirectLightingConfig>();
                AssetDatabase.CreateAsset(config, BounceConfigPath);
            }
            foreach (string path in AvOverhaul.MapScenes)
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                ApplyScene(scene, config);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Couldn't save " + path);
            }
            AssetDatabase.SaveAssets();
        }

        public static void ApplyScene(Scene scene, IndirectLightingConfig config)
        {
            Physics.SyncTransforms();
            float target = scene.name switch
            {
                "HQ" => 1.45f, "Tutorial" => 1.25f, "School" => 1.4f, "Factory" => 1.5f, "Prison" => 1.35f, "TestMap" => 1.7f,
                "Hospital" => 1.45f, "Motel" => 1.35f, "Skyscraper" => 1.4f, "PoliceStation" => 1.45f, "FireStation" => 1.4f, _ => 1.5f,
            };
            int fixtures = 0, thinCasters = 0;
            int mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast", "UI");
            foreach (LightFixture fixture in Object.FindObjectsByType<LightFixture>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (fixture.gameObject.scene != scene) continue;
                Light light = fixture.Lamp;
                if (light == null) continue;
                var data = light.GetUniversalAdditionalLightData();
                data.usePipelineSettings = false;
                data.softShadowQuality = SoftShadowQuality.High;
                light.shadowBias = .35f;
                light.shadowNormalBias = .25f;
                light.shadowNearPlane = .08f;
                if (light.type != LightType.Spot || light.transform.forward.y > -.6f) continue;
                if (!Physics.Raycast(light.transform.position, light.transform.forward, out RaycastHit floor,
                        Mathf.Max(light.range, 24f), mask, QueryTriggerInteraction.Ignore) || floor.normal.y < .65f || floor.distance < 1.5f) continue;
                // Calibrate the received light, rather than assign the same intensity to a 3m dome and a 15m high bay.
                light.range = Mathf.Max(light.range, floor.distance * 1.85f);
                float variation = .92f + Mathf.Repeat(Mathf.Abs(light.transform.position.x * 1.73f + light.transform.position.z * .87f), .16f);
                light.intensity = Mathf.Clamp(target * variation / LightingMath.RangeAttenuation(floor.distance, light.range), 2f, 220f);
                light.innerSpotAngle = light.spotAngle * .45f;
                fixtures++;
            }
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!RestoreThinCaster(renderer)) continue;
                renderer.shadowCastingMode = ShadowCastingMode.TwoSided;
                thinCasters++;
            }
            Atmosphere(scene.name);
            foreach (BuildingInteriorAtmosphere interior in Object.FindObjectsByType<BuildingInteriorAtmosphere>(FindObjectsSortMode.None))
            {
                // Distance haze and volumetric extinction must not both obscure the first room.
                RendererFeatures.Set(interior, "interiorDensity", p => p.floatValue = scene.name == "Factory" ? .008f : .005f);
            }
            foreach (Volume volume in Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
                if (volume.sharedProfile != null && volume.isGlobal) AvLook.Apply(volume.sharedProfile, scene.name);
            var fallback = AssetDatabase.LoadAssetAtPath<Cubemap>(ReflectionFallbackPath);
            if (fallback == null)
            {
                fallback = RoomReflections.CreateFallback();
                fallback.hideFlags = HideFlags.None;
                AssetDatabase.CreateAsset(fallback, ReflectionFallbackPath);
            }
            foreach (ReflectionProbe probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None))
            {
                probe.mode = ReflectionProbeMode.Custom;
                probe.customBakedTexture = fallback;
                probe.enabled = true;
            }
            foreach (MeridianReflectionRefresh legacy in Object.FindObjectsByType<MeridianReflectionRefresh>(FindObjectsSortMode.None))
                legacy.enabled = false; // RoomReflections owns the complete scene queue, including these three probes.
            GameObject rig = null;
            foreach (GameObject root in scene.GetRootGameObjects()) if (root.name == RigName) rig = root;
            if (rig == null) { rig = new GameObject(RigName); SceneManager.MoveGameObjectToScene(rig, scene); }
            (Abandoned.EditorTools.SerializedWiring.GetOrAdd<IndirectLighting>(rig)).EditorSetup(config);
            if (Object.FindFirstObjectByType<RoomReflections>() == null) rig.AddComponent<RoomReflections>();
            Debug.Log($"[LightingRebuild] {scene.name}: {fixtures} receiver-calibrated fixtures, {thinCasters} thin architecture casters restored, diffuse bounce rig installed.");
        }

        private static bool RestoreThinCaster(Renderer renderer)
        {
            if (renderer.shadowCastingMode != ShadowCastingMode.Off || renderer is not MeshRenderer ||
                renderer.GetComponentInParent<LightFixture>() != null) return false;
            Vector3 size = renderer.bounds.size;
            if (Mathf.Min(size.x, size.y, size.z) >= .035f ||
                (size.x >= 1.2f ? 1 : 0) + (size.y >= 1.2f ? 1 : 0) + (size.z >= 1.2f ? 1 : 0) < 2) return false;
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null || material.renderQueue >= 2450) continue;
                string n = material.name.ToLowerInvariant();
                if (n.Contains("wall") || n.Contains("ceiling") || n.Contains("roof") || n.Contains("plaster") || n.Contains("brick")) return true;
            }
            return false;
        }

        private static void Budget()
        {
            var budget = AssetDatabase.LoadAssetAtPath<ShadowBudgetConfig>(RenderPipelineSetup.BudgetPath);
            var so = new SerializedObject(budget);
            Write(so.FindProperty("lights"), new[] { 0, 2, 6, 10 });
            Write(so.FindProperty("pointLights"), new[] { 0, 1, 1, 1 });
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(budget);
        }

        private static void Write(SerializedProperty property, int[] values)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).intValue = values[i];
        }

        private static void Atmosphere(string scene)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            (Color sky, Color equator, Color ground) = scene switch
            {
                "School" => (new Color(.45f, .49f, .55f), new Color(.36f, .4f, .45f), new Color(.24f, .26f, .29f)),
                "Tutorial" => (new Color(.48f, .51f, .55f), new Color(.39f, .42f, .46f), new Color(.25f, .27f, .3f)),
                "Prison" => (new Color(.4f, .46f, .52f), new Color(.31f, .35f, .4f), new Color(.18f, .2f, .23f)),
                "HQ" => (new Color(.3f, .31f, .32f), new Color(.23f, .23f, .22f), new Color(.14f, .13f, .11f)),
                "Factory" => (new Color(.18f, .19f, .2f), new Color(.15f, .14f, .13f), new Color(.085f, .07f, .06f)),
                // The graphics showcase: the late-afternoon sky of ShowcaseLighting.Mood.
                "TestMap" => (new Color(.5f, .54f, .6f), new Color(.4f, .41f, .42f), new Color(.22f, .21f, .19f)),
                // The open-world places (2026-10-23), each a lifted version of its builder's LevelAtmosphere mood.
                "Hospital" => (new Color(.36f, .4f, .41f), new Color(.29f, .32f, .31f), new Color(.15f, .16f, .15f)),
                "Motel" => (new Color(.38f, .35f, .37f), new Color(.33f, .28f, .26f), new Color(.17f, .15f, .13f)),
                "Skyscraper" => (new Color(.3f, .36f, .46f), new Color(.24f, .28f, .34f), new Color(.12f, .13f, .15f)),
                "PoliceStation" => (new Color(.3f, .33f, .39f), new Color(.24f, .26f, .29f), new Color(.12f, .12f, .13f)),
                "FireStation" => (new Color(.44f, .46f, .5f), new Color(.35f, .36f, .38f), new Color(.2f, .19f, .18f)),
                _ =>(new Color(.32f, .34f, .37f), new Color(.27f, .29f, .31f), new Color(.15f, .16f, .17f))
            };
            RenderSettings.ambientSkyColor = sky;
            RenderSettings.ambientEquatorColor = equator;
            RenderSettings.ambientGroundColor = ground;
            RenderSettings.fogDensity = scene == "Factory" ? .006f : scene == "TestMap" ? .006f : .0035f;
        }
    }
}
