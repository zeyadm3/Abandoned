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
            CityCells.Reset();
            // Before anything is laid out: each map's real footprint, read from its own saved scene.
            Debug.Log("[CityDraft] Measuring the ten places...");
            CitySites.Measure();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Sky();
            var root = new GameObject("City").transform;

            Debug.Log("[CityDraft] Ground...");
            int cells = CityGround.Build(root, Paving(), Park(), Wild());
            Debug.Log("[CityDraft] Water...");
            int crossings = CityWater.Build(root, Sea(), River(), Bank(), Pier());
            Debug.Log("[CityDraft] Roads...");
            (int roadCells, int bridges) = CityRoads.Build(root, Asphalt(), Deck());
            Debug.Log("[CityDraft] Blocks...");
            (int blockCells, int buildings) = CityMassing.Build(root, BlockMaterials());
            Debug.Log("[CityDraft] Places...");
            int sites = CitySites.Build(root, Pad());
            float crossing = CityIsland.Build(root, Rock(), Paving());
            Debug.Log("[CityDraft] Cells and far copies...");
            (int cellCount, int farTriangles) = CityCells.Finish(root, Far());

            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[CityDraft] done: {ScenePath}\n" +
                      $"{cells} ground cells over {CityPlan.Bounds.width:0} x {CityPlan.Bounds.height:0} m of city " +
                      $"plus {CityGround.Outskirts:0} m of outskirts, {crossings} river crossings piered.\n" +
                      $"{roadCells} cells carry road, {bridges} bridge slabs over the river.\n" +
                      $"{buildings} buildings of massing over {blockCells} cells.\n" +
                      $"{cellCount} streamed cells, far copies {farTriangles} triangles in all.\n" +
                      $"{sites} places on the city, Gallows Rock {crossing:0} m off the quay. " +
                      "Next: the city takes the World's place, then the perf pass.");
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
        private static Material Asphalt() => PolishAssets.Material("City_Asphalt", new Color(.17f, .17f, .18f), .82f);
        private static Material Deck() => PolishAssets.Material("City_Deck", new Color(.21f, .21f, .22f), .84f);
        // What the city is at a distance: one material for a whole cell, read through fog.
        private static Material Far() => PolishAssets.Material("City_Far", new Color(.42f, .42f, .43f), .9f);
        // A site's pad shows where a map will stand; its own scene streams in over it and hides it.
        private static Material Pad() => PolishAssets.Material("City_SitePad", new Color(.34f, .33f, .31f), .9f);
        private static Material Rock() => PolishAssets.Material("City_Rock", new Color(.30f, .29f, .27f), .94f);

        // One material per district kind, indexed by the enum, so the draft reads as quarters from the air.
        private static Material[] BlockMaterials()
        {
            var byKind = new Material[System.Enum.GetValues(typeof(CityKind)).Length];
            byKind[(int)CityKind.Downtown] = PolishAssets.Material("City_Block_Downtown", new Color(.46f, .47f, .50f), .7f);
            byKind[(int)CityKind.Retail] = PolishAssets.Material("City_Block_Retail", new Color(.56f, .50f, .43f), .82f);
            byKind[(int)CityKind.Civic] = PolishAssets.Material("City_Block_Civic", new Color(.57f, .55f, .57f), .78f);
            byKind[(int)CityKind.Residential] = PolishAssets.Material("City_Block_Residential", new Color(.50f, .44f, .39f), .85f);
            byKind[(int)CityKind.Industrial] = PolishAssets.Material("City_Block_Industrial", new Color(.44f, .41f, .37f), .86f);
            byKind[(int)CityKind.Waterfront] = PolishAssets.Material("City_Block_Waterfront", new Color(.47f, .45f, .41f), .86f);
            byKind[(int)CityKind.Suburb] = PolishAssets.Material("City_Block_Suburb", new Color(.54f, .50f, .44f), .88f);
            byKind[(int)CityKind.Park] = PolishAssets.Material("City_Block_Park", new Color(.30f, .36f, .26f), .92f);
            return byKind;
        }
    }
}
