using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Structure;
using Abandoned.Threats;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The run plumbing every post-mall location shares (crew spawns, network session, loot, run director,
    /// power, threats, NavMesh). The School and Prison come from <see cref="SalvageLocationBuilder"/>, the Iron
    /// Factory from its own builder; both call these so a fix here reaches every location.
    /// </summary>
    public static class LocationPlumbing
    {
        public static void Populate(Transform root, IReadOnlyList<Vector3> crewSpawns, IEnumerable<LootPointSpec> loot,
            IEnumerable<Vector3> threats, StructureSimulation simulation)
        {
            new GameObject("DebugView", typeof(DebugViewToggle), typeof(PerfOverlay));
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null && camera.GetComponent<CinemachineBrain>() == null) camera.gameObject.AddComponent<CinemachineBrain>();

            Transform spawns = Group("Spawns", root);
            for (int i = 0; i < crewSpawns.Count; i++)
            {
                Transform spawn = Group("PlayerSpawn_" + i, spawns);
                spawn.SetPositionAndRotation(crewSpawns[i], Quaternion.LookRotation(Vector3.forward));
                spawn.gameObject.AddComponent<PlayerSpawnPoint>().EditorSetup(i);
            }

            NetworkBootstrap bootstrap = NetworkSceneBuilder.Add();
            Transform points = Group("LootPoints", root);
            foreach (LootPointSpec site in loot)
            {
                Transform point = Group(site.Name, points);
                point.position = site.At;
                point.gameObject.AddComponent<LootSpawnPoint>().EditorSetup(site.Tag, site.Min, site.Max, site.IsJackpot);
            }
            var spawner = new GameObject("LootSpawner").AddComponent<NetworkLootSpawner>();
            spawner.Setup(bootstrap, AssetDatabase.LoadAssetAtPath<LootCatalog>(LootCatalogBuilder.CatalogPath),
                AssetDatabase.LoadAssetAtPath<LootSpawnConfig>(LootCatalogBuilder.SpawnConfigPath), autoSpawn: false);

            var run = new GameObject("Run");
            var director = run.AddComponent<RunDirector>();
            GameObject runState = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkContentBuilder.RunStatePrefabPath);
            director.Setup(bootstrap, runState != null ? runState.GetComponent<NetworkObject>() : null, spawner, simulation);
            run.AddComponent<RunHud>();
            run.AddComponent<PowerController>();
            SerializedWiring.Set(run.AddComponent<AppraisalScreen>(), "director", director);

            Transform spawnPoints = Group("ThreatSpawns", root);
            foreach (Vector3 at in threats)
            {
                Transform marker = Group("ThreatSpawn_" + spawnPoints.childCount.ToString("00"), spawnPoints);
                marker.position = at;
                marker.gameObject.AddComponent<ThreatSpawnPoint>();
            }
            BuildThreats(bootstrap, simulation);
            var views = new GameObject("DebugViews");
            views.AddComponent<NoiseDebugView>();
            views.AddComponent<LootDebugView>();
            views.AddComponent<SharedCarryDebugView>();
        }

        private static void BuildThreats(NetworkBootstrap bootstrap, StructureSimulation simulation)
        {
            NetworkObject Prefab(string path)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                return go != null ? go.GetComponent<NetworkObject>() : null;
            }
            NetworkObject blind = Prefab(ThreatContentBuilder.BlindOnePrefabPath);
            var threats = new GameObject("Threats");
            var director = threats.AddComponent<ThreatDirector>();
            director.Setup(bootstrap, blind, new[]
            {
                blind, Prefab(ThreatContentBuilder.StalkerPrefabPath), Prefab(ThreatContentBuilder.CollectorPrefabPath), Prefab(ThreatContentBuilder.HunterPrefabPath)
            }, new[] { .4f, .2f, .2f, .2f });
            director.SetupCatalog(AssetDatabase.LoadAssetAtPath<ThreatCatalog>(NewThreatContentBuilder.CatalogPath));
            var danger = threats.AddComponent<DangerDirector>();
            danger.Setup(AssetDatabase.LoadAssetAtPath<DangerConfig>(NetworkContentBuilder.DangerConfigPath), simulation);
        }

        /// <param name="ignoredGroups">Paths under the root left out of the bake (exterior, truck, roof...).</param>
        public static void BakeNavMesh(Transform root, string levelName, IEnumerable<string> ignoredGroups)
        {
            var surface = root.gameObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            // Collapsible floors own disabled carving obstacles. Include their floor colliders in the initial bake;
            // the obstacle only carves its tile later if that section actually collapses.
            surface.ignoreNavMeshObstacle = false;
            surface.layerMask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            foreach (string groupPath in ignoredGroups)
            {
                Transform group = root.Find(groupPath);
                if (group == null) continue;
                var modifier = group.GetComponent<NavMeshModifier>() ?? group.gameObject.AddComponent<NavMeshModifier>();
                modifier.ignoreFromBuild = true;
            }
            surface.BuildNavMesh();
            string path = $"Assets/_Project/Scenes/{levelName}_NavMesh.asset";
            var previous = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
            if (previous == null)
            {
                surface.navMeshData.name = levelName + "_NavMesh";
                AssetDatabase.CreateAsset(surface.navMeshData, path);
            }
            else
            {
                surface.RemoveData();
                EditorUtility.CopySerialized(surface.navMeshData, previous);
                Object.DestroyImmediate(surface.navMeshData);
                surface.navMeshData = previous;
                previous.name = levelName + "_NavMesh";
                surface.AddData();
                EditorUtility.SetDirty(previous);
            }
        }

        public static void ConfigureHorrorScenes()
        {
            HorrorConfig config = AssetDatabase.LoadAssetAtPath<HorrorConfig>(HorrorContentBuilder.Path);
            if (config == null) return;
            var serialized = new SerializedObject(config);
            SerializedProperty levels = serialized.FindProperty("levels");
            if (levels == null) return;
            string[] names = { "Mall", "School", "Factory", "Prison" };
            levels.arraySize = names.Length;
            for (int i = 0; i < names.Length; i++) levels.GetArrayElementAtIndex(i).stringValue = names[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }
    }
}
