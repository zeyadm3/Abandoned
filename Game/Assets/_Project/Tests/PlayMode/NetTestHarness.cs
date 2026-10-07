using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Networking;
using Abandoned.Player;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using NetworkConfig = Abandoned.Networking.NetworkConfig;

namespace Abandoned.Tests
{
    /// <summary>
    /// Several "machines" in one process: one NetworkManager + NetworkBootstrap each, talking over
    /// Unity Transport on loopback (host on a free port). Each manager spawns its own copy of every
    /// player, all in the same physics world, so remote copies' CharacterControllers are switched off
    /// here: otherwise an owner would walk into the host's copy of itself. (Real machines don't share
    /// physics, and there remote players keep their colliders.)
    /// </summary>
    public class NetTestHarness
    {
        public const string ConfigPath = "Assets/_Project/Data/Networking/NetworkConfig.asset";
        public const string PrefabListPath = "Assets/DefaultNetworkPrefabs.asset";
        private const float ConnectTimeout = 10f;
        private const float SpawnSpacing = 2f;
        private const string TestRunnerScenePrefix = "InitTestScene";

        private readonly List<GameObject> objects = new();
        private readonly NetworkConfig config;
        private readonly GameObject playerPrefab;
        private readonly NetworkPrefabsList prefabs;

        public List<NetworkBootstrap> Machines { get; } = new();
        public NetworkBootstrap Host { get; private set; }
        public IEnumerable<NetworkBootstrap> Clients => Machines.Where(m => m != Host);
        public ushort Port { get; private set; }
        public NetworkConfig Config => config;

        public NetTestHarness()
        {
#if UNITY_EDITOR
            config = AssetDatabase.LoadAssetAtPath<NetworkConfig>(ConfigPath);
            playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerTestRig.PrefabPath);
            prefabs = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(PrefabListPath);
#endif
            if (config == null || playerPrefab == null || prefabs == null) throw new InvalidOperationException("Network content missing; run RebuildContent.");
        }

        /// <summary>A fresh empty scene: leftovers (TestBuilding's auto-hosted session) would share the process.</summary>
        public static IEnumerator CleanWorld()
        {
            // A scene session from an earlier test outlives its scene on purpose; tests start clean.
            NetworkBootstrap.DestroyPersistent();
            Scene fresh = SceneManager.CreateScene("NetTest_" + Time.frameCount);
            SceneManager.SetActiveScene(fresh);
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                Scene s = SceneManager.GetSceneAt(i);
                // The test runner lives in its own init scene; unloading it would stop the run.
                if (s == fresh || s.name.StartsWith(TestRunnerScenePrefix)) continue;
                yield return SceneManager.UnloadSceneAsync(s);
            }
            foreach (NetworkManager nm in UnityEngine.Object.FindObjectsByType<NetworkManager>(FindObjectsSortMode.None))
                UnityEngine.Object.DestroyImmediate(nm.gameObject);
            yield return null;
        }

        /// <summary>Ground plus one spawn point per player slot, 2 m apart along x.</summary>
        public void BuildArena()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(80f, 1f, 80f);
            objects.Add(ground);
            var sun = new GameObject("TestSun");
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            sun.AddComponent<Light>().type = LightType.Directional;
            objects.Add(sun);
            for (int i = 0; i < config.MaxPlayers; i++)
            {
                var point = new GameObject($"Spawn_{i}");
                point.transform.position = SpawnPosition(i);
                point.AddComponent<PlayerSpawnPoint>().EditorSetup(i);
                objects.Add(point);
            }
            Physics.SyncTransforms();
        }

        public static Vector3 SpawnPosition(int slot) => new(slot * SpawnSpacing, 0.05f, 0f);

        public NetworkBootstrap AddMachine(string name)
        {
            NetworkBootstrap machine = NetworkBootstrapFactory.Create(config, playerPrefab, name, prefabs);
            Machines.Add(machine);
            return machine;
        }

        public void StartHost(NetworkBootstrap machine)
        {
            Assert.IsTrue(machine.StartHost(0, "127.0.0.1"), machine.LastError);
            Host = machine;
            Port = machine.HostPort;
            Assert.AreNotEqual(0, Port, "host should report the OS-picked port");
        }

        /// <summary>Host + n clients, all connected with every player spawned everywhere.</summary>
        public IEnumerator StartSession(int clients)
        {
            StartHost(AddMachine("Host"));
            for (int i = 0; i < clients; i++) AddMachine($"Client{i + 1}");
            yield return ConnectClients();
        }

        public IEnumerator ConnectClients()
        {
            foreach (NetworkBootstrap c in Clients)
                if (!c.IsRunning) Assert.IsTrue(c.StartClient("127.0.0.1", Port), c.LastError);
            int players = Machines.Count;
            yield return WaitFor(() => Machines.All(m => PlayersOn(m).Count() == players), $"{players} players on every machine");
            DisableRemoteColliders();
        }

        public static IEnumerable<NetworkPlayer> PlayersOn(NetworkBootstrap machine) =>
            NetworkPlayer.All.Where(p => p.NetworkManager == machine.Manager);

        /// <summary>The copy of <paramref name="ownerClientId"/>'s player that lives on <paramref name="machine"/>.</summary>
        public static NetworkPlayer PlayerOf(NetworkBootstrap machine, ulong ownerClientId) =>
            PlayersOn(machine).Single(p => p.OwnerClientId == ownerClientId);

        /// <summary>A machine's own (owned) player.</summary>
        public static NetworkPlayer OwnPlayer(NetworkBootstrap machine) => PlayerOf(machine, machine.Manager.LocalClientId);

        public void DisableRemoteColliders()
        {
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (!p.IsOwner) p.GetComponent<CharacterController>().enabled = false;
        }

        public static IEnumerator WaitFor(Func<bool> condition, string what, float timeout = ConnectTimeout)
        {
            float start = Time.realtimeSinceStartup;
            while (!condition())
            {
                if (Time.realtimeSinceStartup - start > timeout) Assert.Fail($"Timed out waiting for {what}.");
                yield return null;
            }
        }

        public static IEnumerator WaitSeconds(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) yield return null;
        }

        public void Track(GameObject go) => objects.Add(go);

        public void Destroy()
        {
            // Clients first, so the host doesn't log them dropping mid-shutdown.
            foreach (NetworkBootstrap m in Machines.AsEnumerable().Reverse())
                if (m != null && m.Manager != null) m.Manager.Shutdown();
            foreach (NetworkBootstrap m in Machines)
            {
                if (m == null) continue;
                GameObject manager = m.Manager != null ? m.Manager.gameObject : null;
                UnityEngine.Object.DestroyImmediate(m.gameObject);
                if (manager != null) UnityEngine.Object.DestroyImmediate(manager);
            }
            Machines.Clear();
            foreach (GameObject go in objects) if (go != null) UnityEngine.Object.DestroyImmediate(go);
            objects.Clear();
            if (SteamBootstrap.Instance != null) UnityEngine.Object.DestroyImmediate(SteamBootstrap.Instance.gameObject);
        }
    }
}
