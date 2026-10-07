using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Structure;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Adds the gameplay objects to the TestMap scene after its geometry is built:
    /// the F1 debug toggle, camera brain, player spawn points, the network session objects, props and
    /// loot. There is no scene-placed player: NGO spawns one per connected player (the solo player hosts).
    /// Coordinates follow TestMapBuilder: ground floor y = 0, upper floor y = 4.
    /// </summary>
    public static class TestMapPopulator
    {
        // Parking lot, in front of the narrow door, facing the building. Slot 0 (the host) stands here.
        public static readonly Vector3 SpawnPosition = new(6f, 0.05f, -6f);

        // One point per player (GDD: up to 4), side by side so nobody spawns inside someone else.
        // Kept clear of the truck spot at x 12.75..15.25.
        public static readonly float[] SpawnOffsetsX = { 0f, 1.5f, -1.5f, 3f };

        private const float Ground = 0f;
        private const float Upper = 4f;

        // id, position (x, z used; y is the surface below), yaw, surface height, roll seed.
        // A mix of light, heavy and fragile items, three of them upstairs (GDD 7.4: jackpots up high).
        private static readonly (string id, Vector3 position, float yaw, float surface)[] Loot =
        {
            ("cash_bundle", new(6f, 0f, 2.5f), 20f, Ground),           // just inside the narrow door
            ("gold_watch", new(14f, 0f, 6f), 0f, Ground + 0.5f),        // on a crate
            ("laptop", new(2f, 0f, 10f), 10f, Ground + 0.75f),          // on a desk
            ("small_painting", new(1f, 0f, 13f), 90f, Ground),          // standing by the west wall
            ("flatscreen_tv", new(13f, 0f, 11f), 0f, Ground),           // standing on the floor
            ("antique_vase", new(2f, 0f, 5f), 0f, Ground + 1f),         // on a pedestal: one drop = $0
            ("server_rack", new(10f, 0f, 1.5f), 0f, Ground),            // heavy, by the front wall
            ("glass_sculpture", new(6f, 0f, 14f), 0f, Upper),           // balcony over the atrium
            ("safe", new(2f, 0f, 14f), 0f, Upper),                      // upper corner room
            ("marble_statue", new(14f, 0f, 10f), 0f, Upper),            // jackpot on the east balcony
            // M2 structure test pieces (see WeakSpots):
            ("grand_piano", new(2f, 0f, 6f), 0f, Upper),                // on the rotten west balcony
            ("vending_machine", new(14f, 0f, 2f), 0f, Upper),           // drag it onto the statue balcony
            ("server_rack", new(17.6f, 0f, 15.2f), 90f, Upper),         // drag it west across the weak north tiles
        };

        private const float TestStability = 0.85f;
        private const int TestSeed = 2026;

        // Authored weak sections so collapses can be tried straight away:
        // (initial health, capacity multiplier). Capacities: floor 3000, balcony 2400 kg before stability.
        private static readonly Dictionary<string, (float health, float capacity)> WeakSpots = new()
        {
            ["Balcony_U_3_2"] = (0.45f, 0.95f), // statue (2000 kg) creaking at ~95%; drag the vending machine on
            ["Balcony_U_0_1"] = (0.4f, 0.24f),  // piano (500 kg) at ~94%; step on and it starts to go
            ["Tile_U_3_3"] = (0.7f, 0.08f),     // rotten boards (~220 kg): a dragged server rack breaks through
            ["Balcony_U_2_3"] = (0.5f, 0.12f),  // and the next one west too (~270 kg)
        };

        private static readonly (string name, Vector3 center, Vector3 size)[] Props =
        {
            ("Crate", new(14f, 0.25f, 6f), new(0.5f, 0.5f, 0.5f)),
            ("Desk", new(2f, 0.375f, 10f), new(1.6f, 0.75f, 0.8f)),
            ("Pedestal", new(2f, 0.5f, 5f), new(0.6f, 1f, 0.6f)),
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

            NetworkSceneBuilder.Add();
            TagSurfaces(root);
            PlaceProps(GreyboxFactory.Group("Props", root));
            PlaceLoot(GreyboxFactory.Group("Loot", root));
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

            // Ground tiles: nothing below them in this building, so they can crack but never fall.
            foreach (Transform tile in Children(root, "GroundFloor/Tiles"))
                AddSection(setup, tile, SectionType.Floor, false, matched);
            foreach (Transform tile in Children(root, "UpperFloor/Tiles"))
                AddSection(setup, tile, tile.name.StartsWith("Balcony") ? SectionType.Balcony : SectionType.Floor, true, matched);
            // GDD 6.4: never collapse the only route out. The stairs are this building's only way down
            // until a rope point/fallback exists, so they creak and crack but can't fall here.
            foreach (Transform segment in Children(root, "Stairs"))
                AddSection(setup, segment, SectionType.Stair, false, matched, fractured: false);

            foreach (string weak in WeakSpots.Keys)
                if (!matched.Contains(weak)) Debug.LogError($"TestMap weak spot '{weak}' matches no section (renamed?).");
            setup.AddSimulation(TestStability, TestSeed);
        }

        private static void AddSection(StructureSceneSetup setup, Transform piece, SectionType type, bool collapsible,
            HashSet<string> matched, bool fractured = true)
        {
            (float health, float capacity) = (1f, 1f);
            if (WeakSpots.TryGetValue(piece.name, out var weak))
            {
                (health, capacity) = weak;
                matched.Add(piece.name);
            }
            setup.AddSection(piece, type, collapsible, health, capacity, fractured);
        }

        private static IEnumerable<Transform> Children(Transform root, string path)
        {
            Transform group = root.Find(path);
            if (group == null)
            {
                Debug.LogError($"TestMap: '{path}' not found; the builder and populator disagree.");
                yield break;
            }
            foreach (Transform child in group) yield return child;
        }

        /// <summary>Concrete ground floor, creaky wooden upper floor and balconies, metal stairs.</summary>
        private static void TagSurfaces(Transform root)
        {
            Tag(root.Find("GroundFloor/Tiles"), SurfaceMaterial.Concrete);
            Tag(root.Find("UpperFloor/Tiles"), SurfaceMaterial.Wood);
            Tag(root.Find("Stairs"), SurfaceMaterial.Metal);
            Tag(root.Find("Exterior/GroundSlabs"), SurfaceMaterial.Dirt);
            Tag(root.Find("Exterior/Parking"), SurfaceMaterial.Asphalt);
        }

        private static void Tag(Transform group, SurfaceMaterial material)
        {
            if (group == null)
            {
                Debug.LogError($"TagSurfaces: missing group for {material}.");
                return;
            }
            foreach (Transform child in group)
                child.gameObject.AddComponent<SurfaceTag>().EditorSet(material);
        }

        private static void PlaceProps(Transform props)
        {
            Material material = GreyboxFactory.GetMaterial("Greybox_Prop", new Color(0.55f, 0.42f, 0.3f));
            foreach ((string name, Vector3 center, Vector3 size) in Props)
                GreyboxFactory.Box(name, props, center, size, material);
            GreyboxFactory.MarkStatic(props);
        }

        private static void PlaceLoot(Transform parent)
        {
            for (int i = 0; i < Loot.Length; i++)
            {
                (string id, Vector3 position, float yaw, float surface) = Loot[i];
                LootPrefabGenerator.PlaceInScene(id, position, yaw, surface, 1001 + i, parent);
            }
        }
    }
}
