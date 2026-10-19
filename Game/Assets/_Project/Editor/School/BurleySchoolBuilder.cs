using System;
using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Master world builder for Jackson P. Burley Middle School (Charlottesville, VA),
    /// recreating the complete campus structure from the aerial photo:
    /// 1951 Stripped Classical brick building, U-shaped academic wings, double-height Gymnasium,
    /// Auditorium with stage, Cafeteria & Kitchen, Library & Media Center, Science Labs,
    /// outdoor 6-lane Running Track and Football/Soccer Field with end zones and collegiate goalposts,
    /// outdoor paved Basketball Courts, Baseball Diamond, terraced hillside, northeast parking lot,
    /// surrounding Charlottesville residential neighborhood streets, Booker T. Washington Park,
    /// and the front entrance drop-off loop with extraction truck.
    /// </summary>
    public static class BurleySchoolBuilder
    {
        /// <summary>Late twilight over Charlottesville: atmospheric blue haze, warm interior sodium and fluorescent pools.</summary>
        public static readonly LevelAtmosphere.Mood Mood = new(
            sky: new Color(0.10f, 0.14f, 0.20f),
            equator: new Color(0.07f, 0.09f, 0.12f),
            ground: new Color(0.03f, 0.04f, 0.05f),
            fog: new Color(0.12f, 0.16f, 0.22f),
            fogDensity: 0.016f,
            bloom: 0.22f,
            vignette: 0.22f,
            grain: 0.12f,
            saturation: -16f,
            contrast: 14f,
            sunColor: new Color(1.0f, 0.82f, 0.62f),
            sunIntensity: 0.35f,
            sunEuler: new Vector3(50f, -40f, 0f)
        );

        [MenuItem("Tools/Abandoned/School/Rebuild Jackson P Burley Middle School")]
        public static void Rebuild() => Build();

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
                throw new InvalidOperationException("Rebuild Jackson P. Burley Middle School in Edit mode.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject(L.SceneName).transform;

            // Inside atmosphere: authentic school hall ambient haze
            root.gameObject.AddComponent<BuildingInteriorAtmosphere>().EditorSetup(
                L.Building, new Color(0.06f, 0.075f, 0.085f), 0.035f);
            root.gameObject.AddComponent<LevelBuildingBounds>().EditorSetup(L.Building);

            var art = new BurleySchoolArt();

            // 1. Structural Floor Systems & Stairs
            BurleySchoolStructure.Result structResult = BurleySchoolStructure.Build(root, art);

            // 2. Exterior Architecture Envelope & Smokestack
            BurleySchoolBuilding.Build(root, art);

            // 3. Hallways & Corridors (Green/Gold Lockers, Trophies, Signs)
            BurleySchoolCorridors.Build(root, art);

            // 4. Detailed Interior Facilities (Gym, Auditorium, Cafeteria, Library, Labs, Admin)
            BurleySchoolInteriors.Build(root, art);

            // 5. Outdoor Athletics (Track, Football Field, Basketball Court, Baseball Diamond)
            BurleySchoolAthletics.Build(root, art);

            // 6. Surrounding Neighborhood (Rose Hill Dr, Madison Ave, Houses, Booker T Park)
            BurleySchoolSurroundings.Build(root, art);

            // 7. Dynamic Lighting & Fluorescents
            BurleySchoolLighting.Build(root, art);

            // 8. Custom School Audio Ambience
            AmbienceBuilder.School(root);

            // 9. Extraction Truck & Collection Bay stationed at Front Driveway Loop
            Transform exterior = Group("Exterior", root);
            Transform groundSlabs = Group("GroundSlabs", exterior);
            GameObject apron = Box("DockApron", groundSlabs, new Vector3(L.TruckPosition.x, -0.16f, L.TruckPosition.z + 6.5f),
                new Vector3(MeridianCollectionBayBuilder.ApproachWidth + 0.6f, 0.12f, 4.4f), art.Concrete);
            apron.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);

            Transform truckRoot = Group("Truck", root);
            TruckCargo cargo = TruckBuilder.Build(truckRoot, L.TruckPosition, Vector3.back);
            MeridianCollectionBayBuilder.Build(root, cargo);

            // 10. Gameplay Spawns, Loot & Threats
            var crewSpawns = new List<Vector3>
            {
                new(43f, 0.05f, -2.5f),
                new(45f, 0.05f, -2.5f),
                new(47f, 0.05f, -2.5f),
                new(49f, 0.05f, -2.5f)
            };
            var loot = BurleySchoolLootAndThreats.GetLootPoints();
            var threats = BurleySchoolLootAndThreats.GetThreatSpawns();
            LocationPlumbing.Populate(root, crewSpawns, loot, threats, structResult.Simulation);

            // 11. NavMesh Baking (Keep exterior roofs, smokestack and neighborhood boundaries out of monster pathfinding)
            string[] navExclusions =
            {
                "BuildingShell/Roofs", "BuildingShell/RooftopHVAC", "BuildingShell/HistoricSmokestack",
                "AthleticsComplex", "CampusSurroundings", "Exterior", "Truck", "MeridianCollectionBay"
            };
            LocationPlumbing.BakeNavMesh(root, L.SceneName, navExclusions);

            // 12. Solid Props & Final NavMesh Pass
            LevelSolidity.Apply(root, L.SceneName);
            LocationPlumbing.BakeNavMesh(root, L.SceneName, navExclusions);

            // 13. Horror Configuration & Atmosphere
            LocationPlumbing.ConfigureHorrorScenes();
            LevelAtmosphere.Apply(L.SceneName, Mood, root);

            // 14. Establish Cinematic Camera Angle
            PlaceCamera();

            // 15. Stamp Network Object IDs & Save
            NetworkObjectIds.StampScene(scene);
            AssetDatabase.SaveAssets();

            if (!EditorSceneManager.SaveScene(scene, L.ScenePath))
                throw new InvalidOperationException("Unity could not save Jackson P. Burley Middle School.");

            // 16. Occlusion Culling Bake
            if (!OcclusionBake.Bake())
                Debug.LogWarning("[BurleySchool] Occlusion bake completed with warnings.");

            if (!EditorSceneManager.SaveScene(scene, L.ScenePath))
                throw new InvalidOperationException("Unity could not save Jackson P. Burley Middle School after occlusion bake.");

            Debug.Log($"[BurleySchool] Successfully built Jackson P. Burley Middle School! " +
                      $"{structResult.Pieces.Count} floor tiles, {loot.Count} loot locations, {threats.Count} threat spawns. " +
                      $"Athletics complex and neighborhood complete.");
        }

        private static void PlaceCamera()
        {
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null) return;
            camera.transform.position = new Vector3(46f, 18f, -26f);
            camera.transform.LookAt(new Vector3(46f, 4.5f, 35f));
        }
    }
}
