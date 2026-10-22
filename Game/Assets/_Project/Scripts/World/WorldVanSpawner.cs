using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// Host: puts the crew's van in the world at this spot once every machine has the level (like the level directors),
    /// and again if it was ever despawned. One van per session.
    /// </summary>
    public class WorldVanSpawner : MonoBehaviour
    {
        [SerializeField] private NetworkObject vanPrefab;

        private NetworkObject spawned;

        private void Update()
        {
            if (vanPrefab == null || !SessionTravel.LevelReady) return;
            NetworkManager manager = NetworkBootstrap.Resolve(null)?.Manager;
            if (manager == null || !manager.IsServer || !manager.IsListening || manager.ShutdownInProgress) return;
            if ((spawned != null && spawned.IsSpawned) || (WorldVan.Current != null && WorldVan.Current.IsSpawned)) return;
            spawned = manager.SpawnManager.InstantiateAndSpawn(vanPrefab, position: transform.position, rotation: transform.rotation);
            Debug.Log($"[WorldVan] Spawned the van at {transform.position:F0}.");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, .7f, .2f, .6f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0f, 1.7f, 1f), new Vector3(2.6f, 3.4f, 7.8f));
        }

        public void EditorSetup(NetworkObject prefab) => vanPrefab = prefab;
    }
}
