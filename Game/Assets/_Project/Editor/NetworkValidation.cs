using System.Collections.Generic;
using System.Linq;
using Abandoned.Networking;
using Abandoned.Player;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Network rules the generic content checks can't see: the Player prefab is a proper NGO player
    /// (owner-authoritative), it's registered, and gameplay scenes spawn players through NGO instead of
    /// carrying a scene-placed one (a second, unnetworked player would break host authority).
    /// </summary>
    public static class NetworkValidation
    {
        public static void ValidatePlayerPrefab(GameObject root, string path, List<string> errors)
        {
            if (root.GetComponent<NetworkObject>() == null) errors.Add($"{path}: missing NetworkObject on the root.");
            if (root.GetComponent<NetworkPlayer>() == null) errors.Add($"{path}: missing NetworkPlayer.");
            var nt = root.GetComponent<NetworkTransform>();
            if (nt == null) errors.Add($"{path}: missing NetworkTransform.");
            else if (nt.AuthorityMode != NetworkTransform.AuthorityModes.Owner)
                errors.Add($"{path}: NetworkTransform must be owner-authoritative (clients own their movement).");

            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(NetworkContentBuilder.PrefabListPath);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (list == null || asset == null || !list.Contains(asset))
                errors.Add($"{path}: not in {NetworkContentBuilder.PrefabListPath} (Tools/Abandoned/Rebuild Content).");
        }

        /// <summary>Checks the open scene (it has a NetworkBootstrap).</summary>
        public static void ValidateSessionScene(string path, List<string> errors)
        {
            var bootstraps = Object.FindObjectsByType<NetworkBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (bootstraps.Length != 1) { errors.Add($"{path}: needs exactly one NetworkBootstrap (has {bootstraps.Length})."); return; }

            NetworkManager manager = bootstraps[0].Manager;
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabBuilder.PrefabPath);
            if (manager == null) errors.Add($"{path}: NetworkBootstrap has no NetworkManager.");
            else
            {
                if (manager.transform.parent != null) errors.Add($"{path}: the NetworkManager must be a root object.");
                if (manager.NetworkConfig?.PlayerPrefab != playerPrefab)
                    errors.Add($"{path}: NetworkManager's player prefab must be {PlayerPrefabBuilder.PrefabPath}.");
                if (manager.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>() == null ||
                    manager.GetComponent<Netcode.Transports.Facepunch.FacepunchTransport>() == null)
                    errors.Add($"{path}: NetworkManager needs both UnityTransport and FacepunchTransport.");
            }

            if (Object.FindFirstObjectByType<PlayerMotor>(FindObjectsInactive.Include) != null)
                errors.Add($"{path}: has a scene-placed player; NGO spawns players (remove it and rebuild).");

            var spawns = Object.FindObjectsByType<PlayerSpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int max = bootstraps[0].Config != null ? bootstraps[0].Config.MaxPlayers : 4;
            if (spawns.Length < max) errors.Add($"{path}: {spawns.Length} PlayerSpawnPoints for {max} players.");
            if (spawns.Select(s => s.Index).Distinct().Count() != spawns.Length)
                errors.Add($"{path}: PlayerSpawnPoint indices must be distinct.");
        }
    }
}
