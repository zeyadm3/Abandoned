using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.World;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The road events' network prefabs (Prefabs/World): a fallen fir (the TestMap's own scanned tree, on its side), a
    /// wrecked car (Kenney car kit) and the thing on the road (the horror director's scare figure). Registered with NGO.
    /// </summary>
    public static class RoadEventBuilder
    {
        public const string TreePath = WorldVanBuilder.PrefabFolder + "/RoadHazard_FallenTree.prefab";
        public const string WreckPath = WorldVanBuilder.PrefabFolder + "/RoadHazard_Wreck.prefab";
        public const string MonsterPath = WorldVanBuilder.PrefabFolder + "/RoadMonster.prefab";

        public static (NetworkObject[] hazards, NetworkObject monster, GameObject scare) Build()
        {
            PolishAssets.EnsureFolder(WorldVanBuilder.PrefabFolder);
            var hazards = new[] { FallenTree(), Wreck() };
            var horror = AssetDatabase.LoadAssetAtPath<HorrorConfig>("Assets/_Project/Data/Extraction/HorrorConfig.asset");
            GameObject scare = horror != null ? horror.ScareModel : null;
            return (hazards, Monster(scare), scare);
        }

        private static GameObject Root(string name, bool solid)
        {
            var root = new GameObject(name);
            root.AddComponent<NetworkObject>().DontDestroyWithOwner = true;
            var nt = root.AddComponent<NetworkTransform>();
            nt.AuthorityMode = NetworkTransform.AuthorityModes.Server;
            nt.SyncScaleX = nt.SyncScaleY = nt.SyncScaleZ = false;
            nt.Interpolate = true;
            if (solid) root.AddComponent<RoadHazard>();
            return root;
        }

        private static NetworkObject Save(GameObject root, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(NetworkContentBuilder.PrefabListPath);
            if (list != null && !list.Contains(prefab))
            {
                list.Add(new NetworkPrefab { Prefab = prefab });
                EditorUtility.SetDirty(list);
            }
            return prefab.GetComponent<NetworkObject>();
        }

        // A tall fir lying across the road (its length along local x, the prefab's forward is the road's direction).
        private static NetworkObject FallenTree()
        {
            GameObject root = Root("RoadHazard_FallenTree", true);
            var terrain = AssetDatabase.LoadAssetAtPath<TerrainData>("Assets/_Project/Art/TestMap/Terrain/TestMapTerrain.asset");
            GameObject tree = terrain != null && terrain.treePrototypes.Length > 0 ? terrain.treePrototypes[0].prefab : null;
            if (tree != null)
            {
                var art = (GameObject)PrefabUtility.InstantiatePrefab(tree, root.transform);
                art.transform.localPosition = new Vector3(-7f, .45f, 0f);
                art.transform.localRotation = Quaternion.Euler(0f, 0f, -88f);
                foreach (Collider c in art.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            }
            var trunk = root.AddComponent<CapsuleCollider>();
            trunk.direction = 0;
            trunk.radius = .45f;
            trunk.height = 16f;
            trunk.center = new Vector3(0f, .45f, 0f);
            root.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Wood);
            return Save(root, TreePath);
        }

        private static NetworkObject Wreck()
        {
            GameObject root = Root("RoadHazard_Wreck", true);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Vehicles.Folder + "delivery.fbx");
            var box = root.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 1.1f, 0f);
            box.size = new Vector3(5f, 2.2f, 2.3f);
            if (model != null)
            {
                // A real van's size, its length across the road (the yaw puts the model's length on x), then rolled half
                // onto its side about that length and stood back on the road.
                GameObject art = ModelFit.Place(model, root.transform, new Vector3(0f, 1.15f, 0f), new Vector3(5f, 2.3f, 2.1f), 90f, standOnFloor: true);
                art.transform.localRotation *= Quaternion.Euler(0f, 0f, 75f);
                Bounds b = ModelFit.LocalBounds(art, root.transform);
                art.transform.localPosition -= new Vector3(b.center.x, b.min.y, b.center.z);
                box.size = b.size;
                box.center = new Vector3(0f, b.size.y * .5f, 0f);
            }
            root.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            return Save(root, WreckPath);
        }

        private static NetworkObject Monster(GameObject scare)
        {
            GameObject root = Root("RoadMonster", false);
            if (scare != null)
            {
                var art = (GameObject)PrefabUtility.InstantiatePrefab(scare, root.transform);
                foreach (Collider c in art.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            }
            root.AddComponent<RoadMonster>();
            return Save(root, MonsterPath);
        }
    }
}
