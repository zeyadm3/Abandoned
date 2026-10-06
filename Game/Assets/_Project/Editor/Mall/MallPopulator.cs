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
        public const int DefaultSeed = 1;

        /// <summary>Parking lot, facing the main entrance (slot 0 is the host's).</summary>
        public static readonly Vector3 SpawnPosition = new(Tile * TilesX / 2f, 0.05f, -8f);
        public static readonly float[] SpawnOffsetsX = { 0f, 1.5f, -1.5f, 3f };

        // Authored weakness on top of the seeded pre-damage (health, capacity multiplier):
        // the gallery floor (the jackpot sits behind weak structure, GDD 7.4) and the atrium bridge.
        private static readonly Dictionary<string, (float health, float capacity)> WeakSpots = new()
        {
            ["Floor_2_4_8"] = (0.8f, 0.55f), ["Floor_2_5_8"] = (0.75f, 0.5f), ["Floor_2_6_8"] = (0.75f, 0.5f),
            ["Floor_2_7_8"] = (0.8f, 0.55f), ["Walk_1_5_5"] = (0.85f, 0.6f), ["Walk_1_6_5"] = (0.85f, 0.6f),
        };

        public static void Populate(Transform root)
        {
            new GameObject("DebugView").AddComponent<DebugViewToggle>();
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

            NetworkSceneBuilder.Add();
            TagSurfaces(root);
            AddStructure(root);

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
                    setup.AddSection(tile, type, collapsible: f > 0, health, capacity);
                }
            }
            foreach (Flight flight in Flights)
                setup.AddSection(root.Find($"{MallBuilder.FlightsGroup}/{flight.Name}"), SectionType.Stair, flight.CanCollapse, fractured: false);
            foreach (string weak in WeakSpots.Keys)
                if (!matched.Contains(weak)) Debug.LogError($"Mall weak spot '{weak}' matches no tile (layout changed?).");
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
