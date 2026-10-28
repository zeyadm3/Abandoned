using System;
using System.Collections.Generic;
using Abandoned.Extraction;
using Abandoned.Structure;
using Abandoned.Tutorial;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.TutorialHouseLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the training job's level from nothing: 14 Larch Court, a one-storey ranch on a Michigan cul-de-sac at
    /// 3 PM in December (Docs/TUTORIAL.md). Lessons are data (Tools > Abandoned > Maps > Tutorial House > Build Tutorial Lessons,
    /// which this runs first); this lays out the house they happen in. Not a contract destination: the HQ's training
    /// clipboard and the main menu's Training card send the crew here.
    /// </summary>
    public static class TutorialHouseBuilder
    {
        [MenuItem(ToolMenu.Tutorial + "Rebuild Tutorial House", priority = ToolMenu.TutorialP)]
        public static void Rebuild() => Build();

        /// <summary>Batch mode: Tools/unity.sh exec Abandoned.EditorTools.TutorialHouseBuilder.BuildAndExit</summary>
        public static void BuildAndExit()
        {
            try
            {
                Build();
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Rebuild the tutorial house in Edit mode.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            // 0. The lessons, the run state's training component and the network prefab list.
            TutorialContentBuilder.Build();
            NetworkContentBuilder.CreateRunStatePrefab();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject(L.SceneName).transform;
            root.gameObject.AddComponent<BuildingInteriorAtmosphere>().EditorSetup(L.Building, new Color(.07f, .075f, .085f), .012f);
            root.gameObject.AddComponent<LevelBuildingBounds>().EditorSetup(L.Building);

            var art = new TutorialHouseArt();
            BurleySchoolSurfaces.Reset();

            // 1. Floors (two of them teach), the basement, the bulkhead stair, the yard and the ceilings.
            TutorialHouseStructure.Result structure = TutorialHouseStructure.Build(root, art);
            // 2. Walls, windows and doors from the plan; the roof and its snow.
            TutorialHouseWalls.Build(root, art);
            TutorialHouseRoof.Build(root, art);
            // 3. What the family left behind, room by room, and the basement.
            TutorialHouseRooms.Build(root, art, structure);
            TutorialHouseBasement.Build(root, art);
            TutorialHouseDetails.Architecture(root, art);
            TutorialHouseServiceDetails.Build(root, art);
            // 4. The yard, and the town round it (streets, plain houses, the edge of the walkable world).
            TutorialNeighbourhood.Build(root, art);
            // 5. A December afternoon: the low sun, the dead fixtures, the breaker, snow over the yard.
            TutorialHouseLighting.Build(root, art);
            // 6. Lessons' places: areas, spots, salvage, kit crates, doors, closets.
            TutorialHouseGameplay.Build(root, art);
            BurleySchoolSurfaces.Apply(root, art.Tiling);
            AmbienceBuilder.House(root);
            root.gameObject.AddComponent<TutorialColdBreath>().EditorSetup(art.WeatherParticle);

            // 7. The truck on the driveway, its ramp to the porch.
            Transform exterior = Group("Exterior", root);
            Transform slabs = Group("GroundSlabs", exterior);
            GameObject apron = Box("DockApron", slabs, new Vector3(L.TruckRear.x - 2.6f, L.Grade - .05f, L.TruckRear.z), new Vector3(5.6f, .1f, MeridianCollectionBayBuilder.ApproachWidth), art.Concrete);
            apron.AddComponent<Core.SurfaceTag>().EditorSet(Core.SurfaceMaterial.Concrete);
            TruckCargo cargo = TruckBuilder.Build(Group("Truck", root), L.TruckRear, L.TruckForward);
            MeridianCollectionBayBuilder.Build(root, cargo);
            // The mall's painted approach lane runs back from the ramp foot: here that is the porch and the living room.
            Transform lane = root.Find("MeridianCollectionBay/FrontCollectionApproach");
            if (lane != null) Object.DestroyImmediate(lane.gameObject);

            // 8. Run plumbing: crew spawns, the network session, the run, power, threats. No planned loot: the
            //    training's salvage is placed by the lessons (TutorialLootSpot).
            LocationPlumbing.Populate(root, L.CrewSpawns, new List<LootPointSpec>(), TutorialHouseGameplay.ThreatSpawns(), structure.Simulation);
            FaceTheHouse(root);
            TutorialHouseLighting.WirePower();
            RunDirector director = Object.FindFirstObjectByType<RunDirector>();
            if (director != null) SerializedWiring.SetInt(director, "firstRunSeed", TutorialConfig.Current.Seed);

            // 9. NavMesh: the house, the basement and the yard; never the roof, the town round the lot or the truck.
            //    Furniture, crates, closets and the yard's trees and fences are obstacles, never surfaces: the agent
            //    climbs 0.75 m, so without this the guide's arrows (and the visitor) could route over a bed.
            NotWalkable(root, "Rooms", "BasementDressing", "Training/KitCrates", "Training/Closets", "Yard");
            LocationPlumbing.BakeNavMesh(root, L.SceneName, new[] { "Roof", "Ceilings", "Neighbourhood", "Exterior", "Truck", "MeridianCollectionBay", "Weather" });

            // 10. Atmosphere, render reach, static flags, the scene camera.
            LevelAtmosphere.Apply(L.SceneName, TutorialHouseLighting.December3PM, root);
            TutorialHouseRendering.Apply(root, art);
            PlaceCamera();

            NetworkObjectIds.StampScene(scene);
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, L.ScenePath))
                throw new InvalidOperationException("Unity could not save the tutorial house.");
            BuildScenes.ApplyToEditorSettings();
            if (!OcclusionBake.Bake()) Debug.LogWarning("[TrainingHouse] Occlusion bake finished with warnings.");
            if (!EditorSceneManager.SaveScene(scene, L.ScenePath))
                throw new InvalidOperationException("Unity could not save the tutorial house after the occlusion bake.");

            Debug.Log($"[TrainingHouse] Built 14 Larch Court: {structure.Sections.Count} floor sections (the hallway braces, the south bedroom falls), " +
                      $"{Object.FindObjectsByType<TutorialLootSpot>(FindObjectsSortMode.None).Length} salvage spots, {TutorialCatalog.Current.Count} lessons.");
        }

        private static void NotWalkable(Transform root, params string[] groups)
        {
            int area = UnityEngine.AI.NavMesh.GetAreaFromName("Not Walkable");
            foreach (string path in groups)
            {
                Transform group = root.Find(path);
                if (group == null) continue;
                var modifier = Abandoned.EditorTools.SerializedWiring.GetOrAdd<Unity.AI.Navigation.NavMeshModifier>(group.gameObject);
                modifier.overrideArea = true;
                modifier.area = area;
            }
        }

        // The crew arrives facing the house, not +Z.
        private static void FaceTheHouse(Transform root)
        {
            Transform spawns = root.Find("Spawns");
            if (spawns == null) return;
            foreach (Transform spawn in spawns) spawn.rotation = Quaternion.Euler(0f, -90f, 0f);
        }

        private static void PlaceCamera()
        {
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null) return;
            camera.transform.position = new Vector3(52f, 15f, -16f);
            camera.transform.LookAt(new Vector3(14f, 1.5f, 8f));
            camera.farClipPlane = TutorialHouseRendering.FarClip;
        }
    }
}
