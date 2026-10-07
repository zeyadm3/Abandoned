using Abandoned.Networking;
using Netcode.Transports.Facepunch;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using NetworkConfig = Abandoned.Networking.NetworkConfig;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Adds the session objects to a scene being built: a root NetworkManager (Unity + Facepunch
    /// transports, the Player prefab as NGO's player prefab) and a "Network" object with the
    /// bootstrap, the placeholder session panel and the F1 network + networked-loot views.
    /// </summary>
    public static class NetworkSceneBuilder
    {
        public static NetworkBootstrap Add()
        {
            var config = AssetDatabase.LoadAssetAtPath<NetworkConfig>(NetworkContentBuilder.ConfigPath);
            var player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabBuilder.PrefabPath);
            var prefabList = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(NetworkContentBuilder.PrefabListPath);
            if (config == null || player == null || prefabList == null)
            {
                Debug.LogError("Network content missing; run Create Network Content and Create Player Prefab first.");
                return null;
            }

            // NGO requires the NetworkManager to be a root object.
            var managerObject = new GameObject("NetworkManager");
            var manager = managerObject.AddComponent<NetworkManager>();
            var utp = managerObject.AddComponent<UnityTransport>();
            var facepunch = managerObject.AddComponent<FacepunchTransport>();
            manager.NetworkConfig ??= new Unity.Netcode.NetworkConfig();
            manager.NetworkConfig.NetworkTransport = utp;
            manager.NetworkConfig.PlayerPrefab = player;
            manager.NetworkConfig.ConnectionApproval = true;
            manager.NetworkConfig.EnableSceneManagement = false;
            manager.NetworkConfig.TickRate = (uint)config.TickRate;
            manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Clear();
            manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(prefabList);
            EditorUtility.SetDirty(manager);

            var network = new GameObject("Network");
            var bootstrap = network.AddComponent<NetworkBootstrap>();
            bootstrap.Setup(config, manager, utp, facepunch, sceneSession: true);
            var travel = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkContentBuilder.SessionTravelPrefabPath);
            var company = AssetDatabase.LoadAssetAtPath<GameObject>(CompanyContentBuilder.CompanyServicePrefabPath);
            if (travel == null || company == null) Debug.LogError("Session prefabs missing; run Rebuild Content.");
            else SerializedWiring.SetArray(bootstrap, "sessionPrefabs", new Object[] { travel.GetComponent<Unity.Netcode.NetworkObject>(), company.GetComponent<Unity.Netcode.NetworkObject>() });
            SerializedWiring.Set(network.AddComponent<ReturnToMenu>(), "bootstrap", bootstrap);
            var lobby = network.AddComponent<SteamLobby>();
            SerializedWiring.Set(lobby, "bootstrap", bootstrap);
            // M7.4: the menus (main, pause, settings, credits) on one UI Toolkit document.
            var document = network.AddComponent<UnityEngine.UIElements.UIDocument>();
            document.panelSettings = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.PanelSettings>(UiContentBuilder.PanelPath);
            network.AddComponent<Abandoned.UI.HintDirector>();
            network.AddComponent<Abandoned.UI.FloorWarningHud>();
            network.AddComponent<Abandoned.UI.AchievementToast>();
            network.AddComponent<Abandoned.UI.ToastFeed>();
            network.AddComponent<Abandoned.UI.TravelScreen>();
            network.AddComponent<Abandoned.UI.ChatView>();
            network.AddComponent<Abandoned.UI.MenuUi>().EditorSetup(document, bootstrap, lobby,
                AssetDatabase.LoadAssetAtPath<Font>(UiContentBuilder.FontPath), AssetDatabase.LoadAssetAtPath<Font>(UiContentBuilder.TitleFontPath),
                AssetDatabase.LoadAssetAtPath<TextAsset>(UiContentBuilder.CreditsPath));
            var debugView = network.AddComponent<NetworkDebugView>();
            SerializedWiring.Set(debugView, "bootstrap", bootstrap);
            SerializedWiring.Set(debugView, "lobby", lobby);
            network.AddComponent<NetworkLootDebugView>();
            EditorUtility.SetDirty(bootstrap);
            return bootstrap;
        }
    }
}
