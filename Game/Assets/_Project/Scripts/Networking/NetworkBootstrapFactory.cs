using Netcode.Transports.Facepunch;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Builds a NetworkManager + <see cref="NetworkBootstrap"/> at runtime (tests, tools). Not a scene
    /// session: no auto-host, no replacing other managers, so several can live in one process.
    /// </summary>
    public static class NetworkBootstrapFactory
    {
        public static NetworkBootstrap Create(NetworkConfig networkConfig, GameObject playerPrefab, string name = "Network",
            NetworkPrefabsList prefabs = null)
        {
            // Built inactive so Awake runs only once everything is wired.
            var managerObject = new GameObject($"{name} NetworkManager");
            managerObject.SetActive(false);
            var manager = managerObject.AddComponent<NetworkManager>();
            var utp = managerObject.AddComponent<UnityTransport>();
            var facepunch = managerObject.AddComponent<FacepunchTransport>();
            // NGO registers the player prefab itself; loot and the rest come from the shared prefab list.
            manager.NetworkConfig = new Unity.Netcode.NetworkConfig { NetworkTransport = utp, PlayerPrefab = playerPrefab };
            if (prefabs != null) manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(prefabs);

            var bootstrapObject = new GameObject(name);
            bootstrapObject.SetActive(false);
            var bootstrap = bootstrapObject.AddComponent<NetworkBootstrap>();
            bootstrap.Setup(networkConfig, manager, utp, facepunch, sceneSession: false);
            managerObject.SetActive(true);
            bootstrapObject.SetActive(true);
            return bootstrap;
        }
    }
}
