using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds Scenes/CityDraft: the rough draft of the drawn city (Docs/CITY_PLAN.md), a step at a time. Step 2 puts
    /// down the ground and the water; step 3 lays the roads over it and step 4 the block massing.
    ///
    /// It is its own scene on purpose. Scenes/World works today and has been played, so the city is grown beside it
    /// and only takes its place at step 6, once the budgets in the plan are met. For the same reason the draft is
    /// deliberately kept out of <c>BuildScenes.All</c>: a scene path for an unfinished map stops everyone's player
    /// builds (Docs/AGENT_REVIEW.md, "Build scenes before the scene exists").
    /// </summary>
    public static class CityDraftBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/CityDraft.unity";

        [MenuItem(ToolMenu.OpenWorld + "Rebuild City Draft", priority = ToolMenu.OpenWorldP + 4)]
        public static void BuildMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        public static void Build()
        {
            CityMesh.Reset();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Sky();
            var root = new GameObject("City").transform;

            Debug.Log("[CityDraft] Ground...");
            int cells = CityGround.Build(root, Paving(), Park(), Wild());
            Debug.Log("[CityDraft] Water...");
            int crossings = CityWater.Build(root, Sea(), River(), Bank(), Pier());

            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[CityDraft] done: {ScenePath}\n" +
                      $"{cells} ground cells over {CityPlan.Bounds.width:0} x {CityPlan.Bounds.height:0} m of city " +
                      $"plus {CityGround.Outskirts:0} m of outskirts, {crossings} river crossings piered. " +
                      "Next: step 3, the roads.");
        }

        // Dusk and the plan's fog, so the draft is judged at the visibility the city will actually run at.
        private static void Sky()
        {
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(28f, 138f, 0f);
            sun.intensity = .9f;
            sun.color = new Color(1f, .93f, .82f);
            sun.shadows = LightShadows.Soft;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.62f, .63f, .66f);
            RenderSettings.fogDensity = .0035f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.46f, .49f, .55f);
            RenderSettings.ambientEquatorColor = new Color(.38f, .38f, .40f);
            RenderSettings.ambientGroundColor = new Color(.22f, .21f, .20f);

            var camera = new GameObject("DraftCamera").AddComponent<Camera>();
            camera.transform.SetPositionAndRotation(new Vector3(0f, 420f, -900f), Quaternion.Euler(24f, 0f, 0f));
            camera.farClipPlane = 1400f;
            camera.tag = "MainCamera";
        }

        private static Material Paving() => PolishAssets.Material("City_Paving", new Color(.40f, .39f, .38f), .88f);
        private static Material Park() => PolishAssets.Material("City_Park", new Color(.26f, .33f, .22f), .95f);
        private static Material Wild() => PolishAssets.Material("City_Outskirts", new Color(.30f, .32f, .24f), .95f);
        private static Material Sea() => PolishAssets.Material("City_Sea", new Color(.13f, .20f, .26f), .22f);
        private static Material River() => PolishAssets.Material("City_River", new Color(.16f, .22f, .25f), .25f);
        private static Material Bank() => PolishAssets.Material("City_Bank", new Color(.28f, .27f, .25f), .92f);
        private static Material Pier() => PolishAssets.Material("City_Pier", new Color(.33f, .32f, .30f), .9f);
    }
}
