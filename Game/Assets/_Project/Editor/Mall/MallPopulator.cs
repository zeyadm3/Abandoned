using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Player;
using Abandoned.Structure;
using Unity.Cinemachine;
using UnityEngine;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Gameplay objects for the Mall after <see cref="MallBuilder"/> made the geometry: F1 toggle, camera
    /// brain, spawn points in the parking lot, the network session, surface tags and the structure
    /// (every tile and flight a StructuralSection; ground tiles never fall, there's no basement).
    /// </summary>
    public static class MallPopulator
    {
        public const float DefaultStability = 0.75f;
        /// <summary>The loading bay's exterior door (east wall, column 6): the truck parks facing it.</summary>
        public static float TruckDoorZ => 8.5f * Tile;
        public const int DefaultSeed = 1;

        /// <summary>Parking lot, facing the main entrance (slot 0 is the host's).</summary>
        public static readonly Vector3 SpawnPosition = new(Tile * TilesX / 2f, 0.05f, -8f);
        public static readonly float[] SpawnOffsetsX = { 0f, 1.5f, -1.5f, 3f };

        // Authored weakness on top of the seeded pre-damage (health, capacity multiplier):
        // the gallery floor (the jackpot sits behind weak structure, GDD 7.4) and the atrium bridge.
        private static readonly Dictionary<string, (float health, float capacity)> WeakSpots = new()
        {
            ["Floor_2_4_9"] = (0.65f, 0.55f), ["Floor_2_5_9"] = (0.60f, 0.5f), ["Floor_2_6_9"] = (0.65f, 0.5f),
            ["Walk_1_6_6"] = (0.8f, 0.6f), ["Walk_1_7_6"] = (0.8f, 0.6f),
        };

        public static void Populate(Transform root)
        {
            new GameObject("DebugView", typeof(DebugViewToggle), typeof(PerfOverlay));
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null && camera.GetComponent<CinemachineBrain>() == null)
                camera.gameObject.AddComponent<CinemachineBrain>();

            Transform spawns = GreyboxFactory.Group("Spawns", root);
            for (int i = 0; i < SpawnOffsetsX.Length; i++)
            {
                var spawn = new GameObject($"PlayerSpawn_{i}").transform;
                spawn.SetParent(spawns, false);
                spawn.SetPositionAndRotation(SpawnPosition + Vector3.right * SpawnOffsetsX[i], Quaternion.identity);
                spawn.gameObject.AddComponent<PlayerSpawnPoint>().EditorSetup(i);
            }

            Abandoned.Networking.NetworkBootstrap bootstrap = NetworkSceneBuilder.Add();
            TagSurfaces(root);
            AddStructure(root);
            MallLootPoints.Place(GreyboxFactory.Group("LootPoints", root));
            var spawner = new GameObject("LootSpawner").AddComponent<Abandoned.Networking.NetworkLootSpawner>();
            spawner.Setup(bootstrap,
                UnityEditor.AssetDatabase.LoadAssetAtPath<LootCatalog>(LootCatalogBuilder.CatalogPath),
                UnityEditor.AssetDatabase.LoadAssetAtPath<LootSpawnConfig>(LootCatalogBuilder.SpawnConfigPath), autoSpawn: false);

            // The truck waits outside the loading bay door, ramp facing it (GDD 10).
            TruckBuilder.Build(GreyboxFactory.Group("Truck", root), new Vector3(TilesX * Tile / 2f, 0f, -5f), Vector3.back);
            var run = new GameObject("Run");
            var director = run.AddComponent<Abandoned.Extraction.RunDirector>();
            director.Setup(bootstrap,
                UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(NetworkContentBuilder.RunStatePrefabPath).GetComponent<Unity.Netcode.NetworkObject>(),
                spawner, Object.FindAnyObjectByType<StructureSimulation>());
            run.AddComponent<Abandoned.Extraction.RunHud>();
            run.AddComponent<Abandoned.Extraction.PowerController>();
            SerializedWiring.Set(run.AddComponent<Abandoned.Extraction.AppraisalScreen>(), "director", director);

            // Threats appear out of sight of the entrance: cinema (floor 2), loading bay, food court.
            Transform threatSpawns = GreyboxFactory.Group("ThreatSpawns", root);
            foreach ((int floor, int x, int z) in new[] { (2, 1, 8), (0, 12, 10), (0, 5, 11), (1, 1, 10), (2, 12, 3), (0, 10, 2) })
            {
                var point = new GameObject($"ThreatSpawn_{floor}_{x}_{z}");
                point.transform.SetParent(threatSpawns, false);
                point.transform.position = TileTopCenter(new Vector2Int(x, z), floor);
                point.AddComponent<Abandoned.Threats.ThreatSpawnPoint>();
            }
            var threats = new GameObject("Threats");
            Unity.Netcode.NetworkObject Threat(string path) => UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<Unity.Netcode.NetworkObject>();
            // GDD 9: one threat opens each run (the Blind One most often); danger brings another.
            threats.AddComponent<Abandoned.Threats.ThreatDirector>().Setup(bootstrap, Threat(ThreatContentBuilder.BlindOnePrefabPath),
                new[] { Threat(ThreatContentBuilder.BlindOnePrefabPath), Threat(ThreatContentBuilder.StalkerPrefabPath), Threat(ThreatContentBuilder.CollectorPrefabPath),
                    Threat(ThreatContentBuilder.HunterPrefabPath) },
                new[] { 0.4f, 0.2f, 0.2f, 0.2f });
            threats.GetComponent<Abandoned.Threats.ThreatDirector>().SetupCatalog(UnityEditor.AssetDatabase.LoadAssetAtPath<Abandoned.Threats.ThreatCatalog>(NewThreatContentBuilder.CatalogPath));
            threats.AddComponent<Abandoned.Threats.DangerDirector>().Setup(
                UnityEditor.AssetDatabase.LoadAssetAtPath<Abandoned.Extraction.DangerConfig>(NetworkContentBuilder.DangerConfigPath),
                Object.FindAnyObjectByType<StructureSimulation>());

            var debugViews = new GameObject("DebugViews");
            debugViews.AddComponent<NoiseDebugView>();
            debugViews.AddComponent<LootDebugView>();
            debugViews.AddComponent<SharedCarryDebugView>();
        }

        private static void AddStructure(Transform root)
        {
            var setup = new StructureSceneSetup();
            if (!setup.IsValid) return;
            var matched = new HashSet<string>();
            Transform tiles = root.Find(MallBuilder.TilesGroup);
            for (int f = 0; f < Floors; f++)
            {
                Transform floor = tiles.Find($"Floor_{f}");
                foreach (Transform tile in floor)
                {
                    (float health, float capacity) = (1f, 1f);
                    if (WeakSpots.TryGetValue(tile.name, out var weak))
                    {
                        (health, capacity) = weak;
                        matched.Add(tile.name);
                    }
                    SectionType type = tile.name.StartsWith("Walk") ? SectionType.Balcony : SectionType.Floor;
                    setup.AddSection(tile, type, collapsible: f > 0 || MallBasementBuilder.AboveBasement(tile.position), health, capacity);
                    if (f > 0 || MallBasementBuilder.AboveBasement(tile.position)) SectionNavCarver.EditorAdd(tile.gameObject, new Vector3(Tile, 0.3f, Tile));
                }
            }
            foreach (Flight flight in Flights)
            {
                Transform piece = root.Find($"{MallBuilder.FlightsGroup}/{flight.Name}");
                setup.AddSection(piece, SectionType.Stair, flight.CanCollapse, fractured: false);
                if (!flight.CanCollapse) continue;
                // The flight's own frame: z runs up the flight from its foot.
                SectionNavCarver.EditorAdd(piece.gameObject, new Vector3(2.8f, 1f, 2f * Tile));
                var obstacle = piece.GetComponent<UnityEngine.AI.NavMeshObstacle>();
                // From just above the floor it starts on to above the one it reaches: the floor below stays walkable.
                obstacle.center = new Vector3(0f, 0.4f + StoryHeight / 2f, Tile);
                obstacle.size = new Vector3(2.8f, StoryHeight, 2f * Tile);
            }
            foreach (string weak in WeakSpots.Keys)
                if (!matched.Contains(weak)) Debug.LogError($"Mall weak spot '{weak}' matches no tile (layout changed?).");
            MallBasementBuilder.AddStructure(root, setup);
            setup.AddSimulation(DefaultStability, DefaultSeed);
        }

        /// <summary>Concrete ground floor, wooden-sounding upper floors, metal flights; dirt and asphalt outside.</summary>
        private static void TagSurfaces(Transform root)
        {
            Transform tiles = root.Find(MallBuilder.TilesGroup);
            for (int f = 0; f < Floors; f++)
                foreach (Transform tile in tiles.Find($"Floor_{f}"))
                    tile.gameObject.AddComponent<SurfaceTag>().EditorSet(f == 0 ? SurfaceMaterial.Concrete : SurfaceMaterial.Wood);
            foreach (Transform flight in root.Find(MallBuilder.FlightsGroup))
                flight.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            foreach (Transform slab in root.Find("Exterior/GroundSlabs"))
                slab.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Dirt);
            foreach (Transform piece in root.Find("Exterior/Parking"))
                piece.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);
        }
    }
}
