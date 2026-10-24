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
    /// Master world builder for Pine Hollow Middle School (fictional; a Blue Ridge town),
    /// with a winter campus, distinct salvage rooms, two academic return routes and a service basement.
    /// Rebuild is an explicit authoring action: compilation, navigation and occlusion are left to the user.
    /// </summary>
    public static class BurleySchoolBuilder
    {
        [MenuItem(ToolMenu.School + "Rebuild Pine Hollow Middle School", priority = ToolMenu.SchoolP)]
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
                throw new InvalidOperationException("Rebuild Pine Hollow Middle School in Edit mode.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject(L.SceneName).transform;

            // Inside atmosphere: authentic school hall ambient haze transitioning from winter exterior
            root.gameObject.AddComponent<BuildingInteriorAtmosphere>().EditorSetup(
                L.Building, new Color(0.055f, 0.065f, 0.075f), 0.016f);
            root.gameObject.AddComponent<LevelBuildingBounds>().EditorSetup(L.Building);

            var art = new BurleySchoolArt();
            BurleySchoolSurfaces.Reset();

            // 1. Structural Floor Systems & Stairs
            BurleySchoolStructure.Result structResult = BurleySchoolStructure.Build(root, art);

            // 2. Exterior Architecture Envelope, Portico, Snow Blankets & Smokestack
            BurleySchoolBuilding.Build(root, art);

            // 3. Hallways & Corridors (Lockers, Trophies, Signs, Amenities)
            BurleySchoolCorridors.Build(root, art);

            // 4. Detailed Interior Facilities (Gym, Auditorium, Cafeteria, Library, Labs, Admin, Boiler)
            BurleySchoolInteriors.Build(root, art);
            BurleySchoolInteriorSupport.Bind(root, structResult);
            BurleySchoolWear.Build(root, art, structResult);

            // 5. Outdoor Athletics in Snow (Track, Football Field, Basketball Court, Baseball Diamond)
            BurleySchoolAthletics.Build(root, art);

            // 6. Surrounding Winter Neighborhood & Mountain Panorama (Houses, Park, Frozen Pond, Pines)
            BurleySchoolSurroundings.Build(root, art);

            // 7. Dynamic Lighting at 3:00 PM Winter & Falling Snow Particles
            BurleySchoolLighting.Build(root, art);
            BurleySchoolInteriorSupport.BindHangingFixtures(root, structResult);

            BurleySchoolSurfaces.Apply(root, art.Tiling);

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

            // 10. Gameplay spawns use clear floor routes and authored salvage surfaces.
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

            // 11. NavMesh Baking (Ensure exterior staging, roofs, and surroundings are excluded)
            string[] navExclusions =
            {
                "BuildingShell/Roofs", "BuildingShell/RooftopHVAC", "BuildingShell/HistoricSmokestack",
                "AthleticsComplex", "CampusSurroundings", "Exterior", "Truck", "MeridianCollectionBay"
            };
            LocationPlumbing.BakeNavMesh(root, L.SceneName, navExclusions);

            // 12. Horror Configuration & 3:00 PM Winter Atmosphere
            LocationPlumbing.ConfigureHorrorScenes();
            LevelAtmosphere.Apply(L.SceneName, BurleySchoolLighting.Winter3PMMood, root);

            // 13. Render Reach (LevelCameraRange 5000m) & Occlusion Culling Static Tagging
            BurleySchoolRendering.Apply(root, art);

            // 14. Establish Cinematic Camera Angle
            PlaceCamera();

            // 15. Stamp Network Object IDs & Save
            NetworkObjectIds.StampScene(scene);
            AssetDatabase.SaveAssets();

            if (!EditorSceneManager.SaveScene(scene, L.ScenePath))
                throw new InvalidOperationException("Unity could not save Pine Hollow Middle School.");

            // 16. Occlusion Culling Bake
            if (!OcclusionBake.Bake())
                Debug.LogWarning("[BurleySchool] Occlusion bake completed with warnings.");

            if (!EditorSceneManager.SaveScene(scene, L.ScenePath))
                throw new InvalidOperationException("Unity could not save Pine Hollow Middle School after occlusion bake.");

            Debug.Log($"[BurleySchool] Successfully rebuilt Pine Hollow Middle School in winter at 3:00 PM! " +
                      $"{structResult.Pieces.Count} floor tiles, {loot.Count} loot locations, {threats.Count} threat spawns. " +
                      $"Two academic stairs, service basement, winter campus and original room furnishings authored.");
        }

        private static void PlaceCamera()
        {
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null) return;
            camera.transform.position = new Vector3(46f, 18f, -26f);
            camera.transform.LookAt(new Vector3(46f, 4.5f, 35f));
            camera.farClipPlane = BurleySchoolRendering.FarClip;
        }
    }
}
