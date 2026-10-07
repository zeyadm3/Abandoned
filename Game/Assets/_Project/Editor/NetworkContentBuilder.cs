using System.Collections.Generic;
using System.IO;
using Abandoned.Networking;
using NetworkPrefab = Unity.Netcode.NetworkPrefab;
using NetworkPrefabsList = Unity.Netcode.NetworkPrefabsList;
using NetworkConfig = Abandoned.Networking.NetworkConfig;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>Creates the networking config and keeps steam_appid.txt in step with its App ID.</summary>
    public static class NetworkContentBuilder
    {
        public const string ConfigPath = "Assets/_Project/Data/Networking/NetworkConfig.asset";
        public const string LootNetConfigPath = "Assets/_Project/Data/Networking/LootNetConfig.asset";
        public const string NetworkPrefabFolder = "Assets/_Project/Prefabs/Network";
        public const string StructureNetPrefabPath = NetworkPrefabFolder + "/StructureNet.prefab";
        public const string RunStatePrefabPath = NetworkPrefabFolder + "/RunState.prefab";
        public const string SessionTravelPrefabPath = NetworkPrefabFolder + "/SessionTravel.prefab";
        public const string ExtractionConfigPath = "Assets/_Project/Data/Extraction/ExtractionConfig.asset";
        public const string DangerConfigPath = "Assets/_Project/Data/Extraction/DangerConfig.asset";

        /// <summary>NGO's own generated list; NGO adds network prefabs to it on import, we make sure of ours.</summary>
        public const string PrefabListPath = "Assets/DefaultNetworkPrefabs.asset";

        /// <summary>
        /// Next to the project (the editor's working directory) so Steam knows the app when the game
        /// isn't launched from Steam. Dev builds get a copy beside the executable (build script);
        /// release builds with the real App ID must NOT ship it.
        /// </summary>
        public const string SteamAppIdFile = "steam_appid.txt";

        [MenuItem("Tools/Abandoned/Create Network Content")]
        public static void CreateMissing()
        {
            var config = LoadOrCreateAsset<NetworkConfig>(ConfigPath);
            LoadOrCreateAsset<LootNetConfig>(LootNetConfigPath);
            WriteSteamAppIdFile(config.SteamAppId);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// The level's one structure sync object (host-spawned by StructureNetSpawner). Only created when
        /// missing or incomplete, so rebuilds don't churn its file (and its GUID/NGO hash never change).
        /// </summary>
        [MenuItem("Tools/Abandoned/Create Structure Net Prefab")]
        public static GameObject CreateStructureNetPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(StructureNetPrefabPath);
            if (existing != null && existing.GetComponent<Unity.Netcode.NetworkObject>() != null && existing.GetComponent<StructureNetSync>() != null)
                return existing;
            if (!AssetDatabase.IsValidFolder(NetworkPrefabFolder))
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Network");
            var root = new GameObject("StructureNet");
            var networkObject = root.AddComponent<Unity.Netcode.NetworkObject>();
            // Building state isn't anyone's; it stays with the host, whoever leaves.
            networkObject.DontDestroyWithOwner = true;
            networkObject.SynchronizeTransform = false;
            root.AddComponent<StructureNetSync>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, StructureNetPrefabPath);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            AssetDatabase.SaveAssets();
            return prefab;
        }

        /// <summary>The session's level-travel object (spawned by the bootstrap when hosting starts).</summary>
        public static GameObject CreateSessionTravelPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(SessionTravelPrefabPath);
            if (existing != null && existing.GetComponent<SessionTravel>() != null) return existing;
            var root = new GameObject("SessionTravel");
            var networkObject = root.AddComponent<Unity.Netcode.NetworkObject>();
            networkObject.DontDestroyWithOwner = true;
            networkObject.SynchronizeTransform = false;
            root.AddComponent<SessionTravel>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, SessionTravelPrefabPath);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            AssetDatabase.SaveAssets();
            return prefab;
        }

        /// <summary>The run's state object (host-spawned by RunDirector). Created when missing, like StructureNet.</summary>
        public static GameObject CreateRunStatePrefab()
        {
            var config = LoadOrCreateAsset<Abandoned.Extraction.ExtractionConfig>(ExtractionConfigPath);
            LoadOrCreateAsset<Abandoned.Extraction.DangerConfig>(DangerConfigPath);
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(RunStatePrefabPath);
            if (existing != null && existing.GetComponent<Abandoned.Extraction.RunState>() != null) return existing;
            var root = new GameObject("RunState");
            var networkObject = root.AddComponent<Unity.Netcode.NetworkObject>();
            networkObject.DontDestroyWithOwner = true;
            networkObject.SynchronizeTransform = false;
            SerializedWiring.Set(root.AddComponent<Abandoned.Extraction.RunState>(), "config", config);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, RunStatePrefabPath);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            AssetDatabase.SaveAssets();
            return prefab;
        }

        /// <summary>Makes sure every network prefab we build is in NGO's prefab list (after the prefabs exist).</summary>
        public static NetworkPrefabsList RegisterNetworkPrefabs()
        {
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(PrefabListPath);
            if (list == null)
            {
                list = ScriptableObject.CreateInstance<NetworkPrefabsList>();
                AssetDatabase.CreateAsset(list, PrefabListPath);
            }
            var prefabs = new List<GameObject> { AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabBuilder.PrefabPath) };
            prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(StructureNetPrefabPath));
            prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(RunStatePrefabPath));
            prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(SessionTravelPrefabPath));
            prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(CompanyContentBuilder.CompanyServicePrefabPath));
            prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(EquipmentContentBuilder.NoiseMakerPrefabPath));
            prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(EquipmentContentBuilder.SupportJackPrefabPath));
            prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(ThreatContentBuilder.BlindOnePrefabPath));
            prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(ThreatContentBuilder.StalkerPrefabPath));
            prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(ThreatContentBuilder.CollectorPrefabPath));
            prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(ThreatContentBuilder.HunterPrefabPath));
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { LootPrefabGenerator.Folder }))
                prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)));
            foreach (GameObject prefab in prefabs)
            {
                if (prefab == null || prefab.GetComponent<Unity.Netcode.NetworkObject>() == null || list.Contains(prefab)) continue;
                list.Add(new NetworkPrefab { Prefab = prefab });
                EditorUtility.SetDirty(list);
            }
            AssetDatabase.SaveAssets();
            return list;
        }

        public static void WriteSteamAppIdFile(uint appId)
        {
            string text = appId.ToString();
            if (File.Exists(SteamAppIdFile) && File.ReadAllText(SteamAppIdFile).Trim() == text) return;
            File.WriteAllText(SteamAppIdFile, text + "\n");
            Debug.Log($"Wrote {SteamAppIdFile} ({text}).");
        }

        public static bool SteamAppIdFileMatches(NetworkConfig config) =>
            config != null && File.Exists(SteamAppIdFile) && File.ReadAllText(SteamAppIdFile).Trim() == config.SteamAppId.ToString();
    }
}
