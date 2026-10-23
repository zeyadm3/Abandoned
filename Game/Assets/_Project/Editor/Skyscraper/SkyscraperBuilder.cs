using System;
using System.Collections.Generic;
using Abandoned.Contracts;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Skyscraper;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Master builder for the Skyscraper Restaurant (Scenes/Skyscraper.unity).
    /// Constructs the forecourt plaza, 48m elevator, 11-flight stairwell, mechanical floor,
    /// panoramic restaurant, commercial kitchen, VIP suite, and all network and run plumbing.
    /// </summary>
    public static class SkyscraperBuilder
    {
        public const string ScenePath = L.ScenePath;

        private static readonly LevelAtmosphere.Mood Mood = new(
            sky: new Color(0.06f, 0.08f, 0.12f),
            equator: new Color(0.04f, 0.05f, 0.07f),
            ground: new Color(0.02f, 0.02f, 0.03f),
            fog: new Color(0.10f, 0.12f, 0.16f),
            fogDensity: 0.010f,
            bloom: 0.22f,
            vignette: 0.22f,
            grain: 0.10f,
            saturation: -18f,
            contrast: 14f,
            sunColor: new Color(0.72f, 0.82f, 1.0f),
            sunIntensity: 0.22f,
            sunEuler: new Vector3(58f, -42f, 0f)
        );

        [MenuItem(ToolMenu.Skyscraper + "Rebuild Skyscraper Restaurant", priority = ToolMenu.SkyscraperP)]
        public static void Rebuild() => Build();

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Rebuild the Skyscraper Restaurant in Edit mode.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            // 1. Ensure loot definitions, models, and network prefabs are authored
            LootCatalog catalog = SkyscraperLoot.EnsureContent();

            // 2. Initialize new empty scene with default lighting
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject(L.SceneName).transform;
            root.gameObject.AddComponent<LevelBuildingBounds>().EditorSetup(L.BuildingBounds);
            root.gameObject.AddComponent<BuildingInteriorAtmosphere>()
                .EditorSetup(L.RestaurantBounds, new Color(0.12f, 0.14f, 0.18f), 0.008f);

            var art = new SkyscraperArt();

            // 3. Build Architecture & Set Pieces
            TruckCargo truck = SkyscraperTowerShell.Build(root, art);
            SkyscraperLobby.Build(root, art);
            SkyscraperElevator elevator = SkyscraperElevatorBuilder.Build(root, art);
            List<Transform> stairLandings = SkyscraperStairwell.Build(root, art, out List<Transform> collapsibleLandings);
            List<Transform> mechPieces = SkyscraperMechanicalFloor.Build(root, art, out List<Transform> collapsibleCatwalks);
            List<Transform> restaurantSlabs = SkyscraperRestaurantFloor.Build(root, art, out List<Transform> collapsibleDining);
            List<Transform> intermediateSlabs = SkyscraperIntermediateFloors.Build(root, art,
                out List<Transform> collapsibleRoofTiles, out List<Transform> soundNodes);
            SkyscraperKitchen.Build(root, art);
            SkyscraperPrivateDining.Build(root, art);
            int hidingCount = SkyscraperHiding.Build(root, art);
            int lightCount = SkyscraperLighting.Build(root, art);

            // 4. Structural Simulation & Collapsible Modules
            var setup = new StructureSceneSetup();
            if (!setup.IsValid) throw new InvalidOperationException("Missing structural content for the skyscraper build.");
            StructureSimulation simulation = SkyscraperStructure.Apply(setup,
                restaurantSlabs, new HashSet<Transform>(collapsibleDining),
                mechPieces, new HashSet<Transform>(collapsibleCatwalks),
                stairLandings, new HashSet<Transform>(collapsibleLandings),
                intermediateSlabs, new HashSet<Transform>(collapsibleRoofTiles));

            // 5. Environmental Soundscape
            BuildSoundscape(root, soundNodes);

            // 6. Run Plumbing: Loot, Threats, Directors, and Power
            List<LootPointSpec> loot = SkyscraperLootPlan.Points();
            List<Vector3> threats = SkyscraperThreatPlan.Points();
            LocationPlumbing.Populate(root, L.CrewSpawns, loot, threats, simulation);

            // Rewire loot spawner to use Skyscraper's rich catalog
            var bootstrap = Object.FindFirstObjectByType<NetworkBootstrap>();
            var spawner = Object.FindFirstObjectByType<NetworkLootSpawner>();
            if (spawner != null && bootstrap != null)
                spawner.Setup(bootstrap, catalog, AssetDatabase.LoadAssetAtPath<LootSpawnConfig>(LootCatalogBuilder.SpawnConfigPath), false);

            // 7. Horror Pacing & Sector Power Configuration
            HorrorConfig horror = SetupHorrorConfig();
            SerializedWiring.Set(Object.FindFirstObjectByType<PowerController>(), "sectorsWithoutHorror", horror);

            // Register Skyscraper Network Prefabs with NetworkManager
            RegisterNetworkPrefabs(bootstrap, catalog);

            // Contract Location Definition
            SetupLocationDefinition();

            // 8. Hierarchy Organization & NavMesh Baking
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                if (go.transform != root && go.name != "NetworkManager" && go.name != "Network")
                    go.transform.SetParent(root, true);
            }

            LocationPlumbing.BakeNavMesh(root, L.SceneName, new[]
            {
                "Exterior/Truck", "Exterior/Skyline", "Exterior/TowerFacade/PorteCochere",
                "Exterior/TowerFacade/RoofCrown", "Exterior/Plaza/DrainedFountainPool",
                "Exterior/Plaza/StreetLamps", "Lobby/LobbyCeiling",
                "IntermediateFloors/FloodedFloor_12m/FloodedCeiling",
                "IntermediateFloors/LootedOffice_32m/OfficeCeiling",
                "Lights", "ElevatorShaft/ShaftHoist"
            });

            LevelAtmosphere.Apply(L.SceneName, Mood, root);
            // The building's sound bed: strong wind at the top of the tower, a faint hum, the frame settling now and then.
            var ambience = new GameObject("Ambience");
            ambience.transform.SetParent(root, false);
            ambience.AddComponent<Abandoned.Audio.LevelAmbience>().EditorSetup(.42f, .08f, .02f, new Vector2(16f, 40f));
            PlaceCamera();

            // 9. Occlusion Flags, Scene Save & Occlusion Baking
            ApplyOcclusionFlags(root);
            BuildScenes.ApplyToEditorSettings();
            NetworkObjectIds.StampScene(scene);
            AssetDatabase.SaveAssets();

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Unity could not save the Skyscraper Restaurant scene.");

            if (!OcclusionBake.Bake())
                throw new InvalidOperationException("Unity could not bake the Skyscraper Restaurant occlusion.");

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Unity could not save the Skyscraper Restaurant after occlusion bake.");

            Debug.Log($"[Skyscraper] Built {L.DisplayName}: 48m Elevator, 11-flight Stairwell, {restaurantSlabs.Count} Dining slabs " +
                      $"({collapsibleDining.Count} collapsible), {mechPieces.Count} Mechanical pieces, {hidingCount} Hiding spots, " +
                      $"{loot.Count} Loot points, {threats.Count} Threat spawns, {lightCount} Lamps.");
        }

        private static HorrorConfig SetupHorrorConfig()
        {
            string horrorPath = $"{SkyscraperLoot.DataFolder}/SkyscraperHorror.asset";
            var horror = SerializedWiring.LoadOrCreateAsset<HorrorConfig>(horrorPath);
            var so = new SerializedObject(horror);
            var levels = so.FindProperty("levels");
            levels.arraySize = 2;
            levels.GetArrayElementAtIndex(0).stringValue = "Skyscraper";
            levels.GetArrayElementAtIndex(1).stringValue = "World";
            so.FindProperty("<Building>k__BackingField").boundsValue = L.BuildingBounds;
            so.FindProperty("<ArrivalFallback>k__BackingField").vector3Value = new Vector3(0f, 0f, -4f);
            so.FindProperty("<ArrivalFigure>k__BackingField").vector3Value = new Vector3(0f, L.RestaurantY, 10f);
            so.FindProperty("<ArrivalDoor>k__BackingField").vector3Value = new Vector3(-8f, L.RestaurantY, 2f);
            so.FindProperty("<RoarPoint>k__BackingField").vector3Value = new Vector3(0f, L.RoofY, 0f);
            so.FindProperty("<SectorCell>k__BackingField").vector3Value = new Vector3(18f, 20f, 16f);
            so.FindProperty("<Sectors>k__BackingField").intValue = 5;
            so.FindProperty("<SectorsLitAtMaxDanger>k__BackingField").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(horror);
            return horror;
        }

        private static void RegisterNetworkPrefabs(NetworkBootstrap bootstrap, LootCatalog catalog)
        {
            if (bootstrap == null || bootstrap.Manager == null || catalog == null) return;
            string listPath = $"{SkyscraperArt.Folder}/SkyscraperNetworkPrefabs.asset";
            var list = SerializedWiring.LoadOrCreateAsset<NetworkPrefabsList>(listPath);
            foreach (var e in catalog.Entries)
            {
                if (e.prefab != null && !list.Contains(e.prefab))
                    list.Add(new NetworkPrefab { Prefab = e.prefab });
            }
            EditorUtility.SetDirty(list);
            if (!bootstrap.Manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Contains(list))
                bootstrap.Manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(list);
        }

        private static void SetupLocationDefinition()
        {
            string locPath = $"{SkyscraperLoot.DataFolder}/SkyscraperLocation.asset";
            var location = SerializedWiring.LoadOrCreateAsset<LocationDefinition>(locPath);
            location.EditorSetup("skyscraper", "Cloudline Skyscraper Restaurant",
                "A luxury restaurant atop an abandoned skyscraper. Reach it via the freight lift or 11 flights of stairs.",
                "Skyscraper", 2, 1, 1.4f, 1.25f, -0.1f, 1.1f, 0.15f);
            EditorUtility.SetDirty(location);
        }

        private static void PlaceCamera()
        {
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null) return;
            camera.transform.position = new Vector3(0f, 32f, -44f);
            camera.transform.LookAt(new Vector3(0f, 24f, 0f));
        }

        private static void BuildSoundscape(Transform root, List<Transform> intermediateNodes)
        {
            Transform soundGroup = new GameObject("Soundscape").transform;
            soundGroup.SetParent(root, false);
            Transform Node(string name, Vector3 pos)
            {
                var t = new GameObject(name).transform;
                t.SetParent(soundGroup, false);
                t.position = pos;
                return t;
            }

            Transform wind1 = Node("Wind_NorthCurtainL", new Vector3(-8f, L.RestaurantY + 3.0f, 21.8f));
            Transform wind2 = Node("Wind_NorthCurtainR", new Vector3(8f, L.RestaurantY + 3.0f, 21.8f));
            Transform wind3 = Node("Wind_TerraceEdge", new Vector3(-16f, L.RestaurantY + 2.0f, 16.0f));
            Transform wind4 = intermediateNodes != null && intermediateNodes.Count > 1 ? intermediateNodes[1] : wind3;

            Transform cable1 = Node("Cable_ShaftHead", new Vector3(0f, 48.5f, 0f));
            Transform cable2 = Node("Cable_MidShaft", new Vector3(0f, L.MechanicalY + 2.5f, 0f));
            Transform cable3 = Node("Cable_ShaftPit", new Vector3(0f, L.GroundY + 1.5f, 0f));

            Transform piano = Node("Piano_Lounge", new Vector3(4f, L.RestaurantY + 1.0f, 4f));
            Transform glass1 = Node("Glass_BackBar", new Vector3(8.5f, L.RestaurantY + 1.8f, 0.5f));
            Transform glass2 = Node("Glass_WineWall", new Vector3(15.5f, L.RestaurantY + 2.0f, 13f));
            Transform drip = intermediateNodes != null && intermediateNodes.Count > 0 ? intermediateNodes[0] : soundGroup;

            soundGroup.gameObject.AddComponent<SkyscraperSoundscape>().EditorSetup(
                new[] { wind1, wind2, wind3, wind4 },
                new[] { cable1, cable2, cable3 },
                new[] { piano },
                new[] { glass1, glass2 },
                new[] { drip }
            );
        }

        private static void ApplyOcclusionFlags(Transform root)
        {
            Transform skyline = root.Find("Exterior/Skyline");
            Transform truck = root.Find("Exterior/Truck");

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                GameObject go = renderer.gameObject;
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);

                bool isSection = renderer.GetComponentInParent<StructuralSection>() != null ||
                                renderer.GetComponentInParent<SectionProp>() != null;
                bool isDynamic = renderer.GetComponentInParent<MovingPlatform>() != null ||
                                renderer.GetComponentInParent<HidingLocker>() != null ||
                                renderer.GetComponentInParent<LightFixture>() != null ||
                                renderer.GetComponentInParent<BreakerPanel>() != null;
                bool isScenery = (skyline != null && go.transform.IsChildOf(skyline)) ||
                                (truck != null && go.transform.IsChildOf(truck));
                string name = go.name.ToLowerInvariant();
                bool isFloor = name.Contains("slab") || name.Contains("floor") || name.Contains("tile") ||
                               name.Contains("walkway") || name.Contains("ramp") || name.Contains("bridge") ||
                               name.Contains("landing") || name.Contains("cradle");

                if (isSection || isDynamic || isScenery || isFloor)
                {
                    flags &= ~StaticEditorFlags.OccluderStatic;
                }
                else
                {
                    Vector3 size = renderer.bounds.size;
                    int broadAxes = (size.x >= 1.2f ? 1 : 0) + (size.y >= 1.2f ? 1 : 0) + (size.z >= 1.2f ? 1 : 0);
                    if (broadAxes >= 2)
                        flags |= StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.BatchingStatic;
                }

                if (isSection || isDynamic)
                    flags &= ~(StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI | StaticEditorFlags.ReflectionProbeStatic);

                GameObjectUtility.SetStaticEditorFlags(go, flags);
            }
        }
    }
}
