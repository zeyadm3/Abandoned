using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Structure;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Abandoned.Tests
{
    /// <summary>
    /// One copy of a small test building per in-process "machine" (same names and positions, so the
    /// same section ids and layout hash), each bound to its own NetworkManager the way a real machine
    /// has exactly one. The host's copy runs the simulation; StructureNetSync makes the others mirrors.
    /// <see cref="KeepApart"/> stops each machine's players and loot touching other machines' copies
    /// and leaves only the host's load sources registered (real clients never solve loads).
    /// </summary>
    public sealed class NetStructureKit
    {
        public const string SyncPrefabPath = "Assets/_Project/Prefabs/Network/StructureNet.prefab";
        public const float Top = 3f;

        public readonly struct Tile
        {
            public readonly string Name;
            public readonly Vector3 TopCentre;
            public readonly float Health, Capacity;
            public readonly bool Collapsible;

            public Tile(string name, Vector3 topCentre, float health = 1f, float capacity = 1f, bool collapsible = true)
            {
                Name = name;
                TopCentre = topCentre;
                Health = health;
                Capacity = capacity;
                Collapsible = collapsible;
            }
        }

        private readonly NetTestHarness net;
        private readonly StructureTestRig rig = StructureTestRig.Create(ground: false);
        private readonly Dictionary<NetworkBootstrap, StructureSimulation> simulations = new();
        private readonly List<GameObject> roots = new();

        private NetStructureKit(NetTestHarness net) => this.net = net;

        /// <summary>Builds the tiles once per machine (before or after they connect).</summary>
        public static NetStructureKit Build(NetTestHarness net, IReadOnlyList<Tile> tiles, float stability = 1f, int seed = 1)
        {
            var kit = new NetStructureKit(net);
            foreach (NetworkBootstrap machine in net.Machines) kit.AddCopy(machine, tiles, stability, seed);
            return kit;
        }

        private void AddCopy(NetworkBootstrap machine, IReadOnlyList<Tile> tiles, float stability, int seed)
        {
            var root = new GameObject($"Structure ({machine.name})");
            root.SetActive(false);
            var simulation = root.AddComponent<StructureSimulation>();
            root.AddComponent<StructureNetBinding>().Bind(machine.Manager);
#if UNITY_EDITOR
            var so = new SerializedObject(simulation);
            so.FindProperty("config").objectReferenceValue = rig.Config;
            so.FindProperty("stability").floatValue = stability;
            so.FindProperty("seed").intValue = seed;
            so.FindProperty("childrenOnly").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SyncPrefabPath);
            Assert.IsNotNull(prefab, "StructureNet prefab missing; run RebuildContent");
            root.AddComponent<StructureNetSpawner>().EditorSetup(prefab.GetComponent<NetworkObject>());
#endif
            foreach (Tile t in tiles)
                rig.AddTile(t.Name, t.TopCentre, collapsible: t.Collapsible, health: t.Health, capacity: t.Capacity)
                    .transform.SetParent(root.transform, true);
            root.SetActive(true); // Awake numbers the sections; Start applies stability on the host's copy
            simulations[machine] = simulation;
            roots.Add(root);
        }

        public StructureSimulation On(NetworkBootstrap machine) => simulations[machine];

        public StructuralSection Section(NetworkBootstrap machine, string name) =>
            simulations[machine].Sections.Single(s => s.name == name);

        public IEnumerable<StructuralSection> Copies(string name) => net.Machines.Select(m => Section(m, name));

        /// <summary>Until every machine has the sync bound to its own copy, and every client copy mirrors.</summary>
        public IEnumerator WaitForSync() => NetTestHarness.WaitFor(() => net.Machines.All(m =>
        {
            StructureNetSync sync = StructureNetSync.For(m.Manager);
            return sync != null && sync.Simulation == simulations[m] && (m == net.Host || simulations[m].IsMirror);
        }), "the structure sync on every machine");

        public void KeepApart()
        {
            var perMachine = net.Machines.Select(m => MachineColliders(m).ToList()).ToList();
            var structures = net.Machines.Select(m => simulations[m].GetComponentsInChildren<Collider>(true).ToList()).ToList();
            for (int a = 0; a < perMachine.Count; a++)
            for (int b = 0; b < structures.Count; b++)
            {
                if (a == b) continue;
                foreach (Collider x in perMachine[a])
                foreach (Collider y in structures[b])
                    Physics.IgnoreCollision(x, y, true);
            }
            foreach (NetworkBootstrap m in net.Clients)
            foreach (NetworkObject no in m.Manager.SpawnManager.SpawnedObjectsList)
            foreach (ILoadSource source in no.GetComponentsInChildren<ILoadSource>(true))
                LoadSources.Unregister(source);
        }

        private static IEnumerable<Collider> MachineColliders(NetworkBootstrap machine) =>
            machine.Manager.SpawnManager == null
                ? Enumerable.Empty<Collider>()
                : machine.Manager.SpawnManager.SpawnedObjectsList.Where(no => no != null)
                    .SelectMany(no => no.GetComponentsInChildren<Collider>(true));

        public void Destroy()
        {
            foreach (GameObject go in roots) if (go != null) Object.DestroyImmediate(go);
            roots.Clear();
            rig.Destroy();
        }
    }
}
