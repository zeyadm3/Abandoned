using Abandoned.Core;
using Abandoned.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Adds the gameplay objects to the TestBuilding scene after its geometry is built:
    /// the F1 debug toggle, camera brain, player spawn point and player.
    /// </summary>
    public static class TestBuildingPopulator
    {
        // Parking lot, in front of the narrow door, facing the building.
        public static readonly Vector3 SpawnPosition = new(6f, 0.05f, -6f);

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
        }
    }
}
