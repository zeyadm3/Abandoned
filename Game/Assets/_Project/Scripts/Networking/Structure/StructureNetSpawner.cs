using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Lives next to a level's StructureSimulation: whenever this machine hosts, it spawns the one
    /// StructureNetSync for the session (dynamically, so it needs no in-scene network id and comes
    /// back with every new host session).
    /// </summary>
    [RequireComponent(typeof(StructureSimulation))]
    public class StructureNetSpawner : MonoBehaviour
    {
        [SerializeField] private NetworkObject syncPrefab;

        private NetworkObject spawned;

        public NetworkObject Spawned => spawned;

        private void Update()
        {
            NetworkManager manager = TryGetComponent(out StructureNetBinding binding) && binding.Manager != null
                ? binding.Manager
                : NetworkManager.Singleton;
            if (manager == null || !manager.IsServer || !manager.IsListening || manager.ShutdownInProgress || !SessionTravel.LevelReady) return;
            if (spawned != null && spawned.IsSpawned) return;
            spawned = manager.SpawnManager.InstantiateAndSpawn(syncPrefab);
        }

#if UNITY_EDITOR
        public void EditorSetup(NetworkObject prefab) => syncPrefab = prefab;
#endif
    }
}
