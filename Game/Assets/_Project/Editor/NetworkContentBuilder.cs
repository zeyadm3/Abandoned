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
            WriteSteamAppIdFile(config.SteamAppId);
            AssetDatabase.SaveAssets();
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
            var player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabBuilder.PrefabPath);
            if (player != null && !list.Contains(player))
            {
                list.Add(new NetworkPrefab { Prefab = player });
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
