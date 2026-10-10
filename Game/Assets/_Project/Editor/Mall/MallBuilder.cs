using System;
using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    public static class MallBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Mall.unity", TilesGroup = "Tiles", FlightsGroup = "Flights";
        public const string NavMeshPath = "Assets/_Project/Scenes/Mall_NavMesh.asset";

        [MenuItem("Tools/Abandoned/Create Mall")]
        public static void Create()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Build();
        }

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Rebuild the mall in Edit mode.");
            Scene old = SceneManager.GetSceneByPath(ScenePath);
            if (!old.IsValid() || !old.isLoaded) old = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform previous = null;
            foreach (GameObject root in old.GetRootGameObjects())
                if (root.name == "Mall") previous = root.transform;
            if (previous == null) throw new InvalidOperationException("The saved Mall root is missing.");
            Transform district = previous.Find(MeridianWorldSurroundings.GroupName);
            if (district == null) throw new InvalidOperationException("The surrounding area must be saved before rebuilding the mall.");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
            // Move the exact saved district, rather than regenerate the surroundings the user approved.
            district.SetParent(null, true);
            SceneManager.MoveGameObjectToScene(district.gameObject, scene);
            EditorSceneManager.CloseScene(old, true);
            SceneManager.SetActiveScene(scene);
            Transform mall = new GameObject("Mall").transform;
            district.SetParent(mall, true);
            mall.gameObject.AddComponent<BuildingInteriorAtmosphere>().EditorSetup(BuildingBounds);

            MallGameplayData.Apply();
            ThreatPressure0156.Apply();
            int tiles = MallFloorBuilder.Build(Group(TilesGroup, mall));
            Transform flights = Group(FlightsGroup, mall);
            foreach (Flight flight in AllFlights) MallFlights.Build(flight, flights);
            MallFlights.Landings(Group("Landings", mall));
            MeridianMallShell.Build(mall);
            MallExteriorBuilder.Build(mall);
            MallExteriorBuilder.Exits(mall);
            MallPopulator.Populate(mall, authorLoot: false);
            ThreatPressure0156.AuthorSpawns(mall);
            MeridianDressingCeilings.Place(mall, Group("MallCeilings", mall));
            MeridianTenantInteriors.Build(mall);
            MeridianMallCourt.Build(mall);
            SalvageMallDressing.DryFountain(mall);
            MeridianMountainHorizon.Build(mall);
            SalvageMallAtmosphere.ConfigurePreset();
            MeridianAtmosphere.Apply(mall);
            int fixtures = MeridianLighting.Place(mall);
            MeridianMallCourt.LightColors(mall);
            SalvageMallAtmosphere.Build(mall);

            // Author collision around the fitted rooms before choosing open pockets for loot.
            var routes = new MallRouteClearance(mall);
            SolidObjectsPass.Apply(mall, "Mall", routes.Conflict);
            int lootPoints = SalvageMallLootPoints.Place(mall.Find("LootPoints"));
            SalvageMallDensity.Build(mall);
            MallExpansion0156.Build(mall);
            SalvagePickables0157.Author();
            SalvageLootPlacement0157.Build(mall);
            MallTextCleanup0158.Build(mall);
            MallRetailArtwork0158.Build(mall);
            MallSurfaceBuilder.Assign(mall);
            BakeNavMesh(mall);
            MallShutters.Place(mall);
            MeridianLightLayers.Apply(mall);
            ShadowCasterPass.Apply(mall, "Mall");
            PlayerSettings.bundleVersion = BuildScript.Version;
            EditorSceneManager.SaveScene(scene, ScenePath);
            BuildScenes.ApplyToEditorSettings();
            NetworkObjectIds.StampScene(scene);
            // The old culling cells belong to the discarded building, so author them for the new shell.
            if (!OcclusionBake.Bake()) throw new InvalidOperationException("Unity could not author occlusion data for the rebuilt mall.");
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Unity could not save the rebuilt mall.");
            Debug.Log($"[Mall0155] Authored salvage mall ({tiles} slabs, {fixtures} mains fixtures, {lootPoints} loot pads), five salvage zones and two-floor Nordic Habitat, preserved district and mountain horizon.");
        }

        public static string TileName(Vector2Int cell, int floor) => $"{(IsWalkwayTile(cell, floor) ? "Walk" : "Floor")}_{floor}_{cell.x}_{cell.y}";

        public static void BakeNavMesh(Transform mall)
        {
            var surface = mall.GetComponent<Unity.AI.Navigation.NavMeshSurface>() ?? mall.gameObject.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
            surface.collectObjects = Unity.AI.Navigation.CollectObjects.Children;
            surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            surface.ignoreNavMeshObstacle = false;
            surface.layerMask = ~LayerMask.GetMask(Abandoned.Core.GameLayers.Player, Abandoned.Core.GameLayers.Loot, Abandoned.Core.GameLayers.Debris, "Ignore Raycast");
            foreach (string name in new[] { "Exterior", "Truck", "Roof", "Exits", MeridianWorldSurroundings.GroupName, MeridianMountainHorizon.GroupName })
            {
                Transform group = mall.Find(name);
                if (group == null) continue;
                var modifier = group.GetComponent<Unity.AI.Navigation.NavMeshModifier>() ?? group.gameObject.AddComponent<Unity.AI.Navigation.NavMeshModifier>();
                modifier.ignoreFromBuild = true;
            }
            surface.BuildNavMesh();
            var previous = AssetDatabase.LoadAssetAtPath<UnityEngine.AI.NavMeshData>(NavMeshPath);
            if (previous != null)
            {
                surface.RemoveData();
                EditorUtility.CopySerialized(surface.navMeshData, previous);
                Object.DestroyImmediate(surface.navMeshData);
                surface.navMeshData = previous;
                surface.AddData();
                previous.name = "Mall_NavMesh";
                EditorUtility.SetDirty(previous);
            }
            else { surface.navMeshData.name = "Mall_NavMesh"; AssetDatabase.CreateAsset(surface.navMeshData, NavMeshPath); }
        }
    }
}
