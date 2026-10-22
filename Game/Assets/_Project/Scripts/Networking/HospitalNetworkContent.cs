using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Networking
{
    /// <summary>Register the POI's Resources prefabs at session startup, before NGO locks its prefab set.</summary>
    public static class HospitalNetworkContent
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded-=Loaded;
            SceneManager.sceneLoaded+=Loaded;
        }
        private static void Loaded(Scene scene,LoadSceneMode mode)
        {
            NetworkManager manager=NetworkManager.Singleton;
            if(manager!=null&&!manager.IsListening)Register(manager);
        }
        // Also callable by a world bootstrap which creates its manager after the initial sceneLoaded callback.
        public static void Register(NetworkManager manager)
        {
            if(manager==null)return;
            foreach(GameObject prefab in Resources.LoadAll<GameObject>("Hospital/Network").OrderBy(p=>p.name,System.StringComparer.Ordinal))
                if(prefab.GetComponent<NetworkObject>()!=null&&!manager.NetworkConfig.Prefabs.Contains(prefab))manager.AddNetworkPrefab(prefab);
        }
    }
}
