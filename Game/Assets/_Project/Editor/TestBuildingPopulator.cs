using Abandoned.Core;
using Abandoned.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Adds the gameplay objects to the TestBuilding scene after its geometry is built:
    /// the F1 debug toggle, camera brain, player spawn point, player, props and loot.
    /// Coordinates follow TestBuildingBuilder: ground floor y = 0, upper floor y = 4.
    /// </summary>
    public static class TestBuildingPopulator
    {
        // Parking lot, in front of the narrow door, facing the building.
        public static readonly Vector3 SpawnPosition = new(6f, 0.05f, -6f);

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
        };

        private static readonly (string name, Vector3 center, Vector3 size)[] Props =
        {
            ("Crate", new(14f, 0.25f, 6f), new(0.5f, 0.5f, 0.5f)),
            ("Desk", new(2f, 0.375f, 10f), new(1.6f, 0.75f, 0.8f)),
            ("Pedestal", new(2f, 0.5f, 5f), new(0.6f, 1f, 0.6f)),
        };

        public static void Populate(Transform root)
        {
            new GameObject("DebugView").AddComponent<DebugViewToggle>();

            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null && camera.GetComponent<CinemachineBrain>() == null)
                camera.gameObject.AddComponent<CinemachineBrain>();

            Transform spawns = GreyboxFactory.Group("Spawns", root);
            var spawn = new GameObject("PlayerSpawn_0").transform;
            spawn.SetParent(spawns, false);
            spawn.SetPositionAndRotation(SpawnPosition, Quaternion.identity);
            spawn.gameObject.AddComponent<PlayerSpawnPoint>();

            PlayerPrefabBuilder.PlaceInScene(spawn.position, spawn.rotation);
            PlaceProps(GreyboxFactory.Group("Props", root));
            PlaceLoot(GreyboxFactory.Group("Loot", root));
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
