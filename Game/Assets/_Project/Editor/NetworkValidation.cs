using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        /// <summary>A loot prefab is a host-controlled network object whose carrier can own its physics.</summary>
        public static void ValidateLootPrefab(GameObject root, string path, List<string> errors)
        {
            var networkObject = root.GetComponent<NetworkObject>();
            if (networkObject == null) { errors.Add($"{path}: missing NetworkObject on the root (run Generate Loot Prefabs)."); return; }
            if (!networkObject.DontDestroyWithOwner)
                errors.Add($"{path}: NetworkObject.DontDestroyWithOwner must be on (loot outlives a leaving carrier).");
            if (root.GetComponent<NetworkLoot>() == null) errors.Add($"{path}: missing NetworkLoot.");
            var nt = root.GetComponent<NetworkTransform>();
            if (nt == null) errors.Add($"{path}: missing NetworkTransform.");
            else if (nt.AuthorityMode != NetworkTransform.AuthorityModes.Owner)
                errors.Add($"{path}: NetworkTransform must be owner-authoritative (the carrier owns carried physics).");
            if (root.GetComponent<NetworkRigidbody>() != null)
                errors.Add($"{path}: no NetworkRigidbody; Grabbable manages kinematic state from physics authority.");

            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(NetworkContentBuilder.PrefabListPath);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (list == null || asset == null || !list.Contains(asset))
                errors.Add($"{path}: not in {NetworkContentBuilder.PrefabListPath} (Tools/Abandoned/Rebuild Content).");
        }

        /// <summary>The structure sync: one host-spawned object per level, registered, never tied to an owner.</summary>
        public static void ValidateStructureNetPrefab(GameObject root, string path, List<string> errors)
        {
            var networkObject = root.GetComponent<NetworkObject>();
            if (networkObject == null) { errors.Add($"{path}: missing NetworkObject (Tools/Abandoned/Create Structure Net Prefab)."); return; }
            if (!networkObject.DontDestroyWithOwner) errors.Add($"{path}: DontDestroyWithOwner must be on (the building outlives any player).");
            if (root.GetComponent<StructureNetSync>() == null) errors.Add($"{path}: missing StructureNetSync.");
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
            ValidateScenePlacedObjects(path, errors);
            ValidateStructureSync(path, errors);
        }

        /// <summary>A session scene with a structure must spawn its sync, or clients would never see damage.</summary>
        private static void ValidateStructureSync(string path, List<string> errors)
        {
            var simulations = Object.FindObjectsByType<Abandoned.Structure.StructureSimulation>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (simulations.Length == 0) return;
            if (simulations.Length > 1) errors.Add($"{path}: {simulations.Length} StructureSimulations; a session scene runs one building.");
            foreach (var simulation in simulations)
            {
                var spawner = simulation.GetComponent<StructureNetSpawner>();
                if (spawner == null) { errors.Add($"{path}: '{simulation.name}' has no StructureNetSpawner (clients would never see structural state)."); continue; }
                var prefab = new SerializedObject(spawner).FindProperty("syncPrefab").objectReferenceValue as NetworkObject;
                if (prefab == null || AssetDatabase.GetAssetPath(prefab) != NetworkContentBuilder.StructureNetPrefabPath)
                    errors.Add($"{path}: '{simulation.name}' StructureNetSpawner must spawn {NetworkContentBuilder.StructureNetPrefabPath}.");
            }
        }

        /// <summary>
        /// Scene management is off, so clients spawn in-scene objects from their source prefab's hash:
        /// each one needs a unique id of its own and a registered source prefab.
        /// </summary>
        private static void ValidateScenePlacedObjects(string path, List<string> errors)
        {
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(NetworkContentBuilder.PrefabListPath);
            var seen = new HashSet<uint>();
            int placed = 0;
            foreach (NetworkObject no in Object.FindObjectsByType<NetworkObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (no.GetComponent<NetworkManager>() != null) continue;
                var serialized = new SerializedObject(no);
                uint hash = serialized.FindProperty("GlobalObjectIdHash").uintValue;
                if (hash == 0 || !seen.Add(hash)) errors.Add($"{path}: '{no.name}' has a missing or duplicate GlobalObjectIdHash (resave the scene).");
                GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(no.gameObject);
                if (source == null) errors.Add($"{path}: '{no.name}' is a scene NetworkObject without a source prefab; clients can't spawn it.");
                else if (list == null || !list.Contains(source)) errors.Add($"{path}: '{no.name}''s prefab isn't a registered network prefab.");
                else if (serialized.FindProperty("InScenePlacedSourceGlobalObjectIdHash").uintValue == 0)
                    errors.Add($"{path}: '{no.name}' has no InScenePlacedSourceGlobalObjectIdHash (resave the scene).");
                if (source != null) placed++;
            }
            ValidateSavedInScenePlacement(path, placed, errors);
        }

        /// <summary>
        /// Opening the scene in the editor re-runs NGO's validation in memory, so the checks above can
        /// pass while the saved file (what play mode and builds load) still lacks the ids. Count them there.
        /// </summary>
        private static void ValidateSavedInScenePlacement(string path, int placed, List<string> errors)
        {
            if (placed == 0 || !File.Exists(path)) return;
            string text = File.ReadAllText(path);
            int sourceHashes = Regex.Matches(text, @"propertyPath: InScenePlacedSourceGlobalObjectIdHash\s*\n\s*value: [1-9]").Count;
            int marked = Regex.Matches(text, @"propertyPath: m_InScenePlaced\s*\n\s*value: 1").Count;
            if (sourceHashes < placed || marked < placed)
                errors.Add($"{path}: only {Mathf.Min(sourceHashes, marked)} of {placed} scene NetworkObjects are saved as in-scene placed " +
                           "(clients would double them; rebuild the scene with Tools/Abandoned/Rebuild Content).");
        }
    }
}
