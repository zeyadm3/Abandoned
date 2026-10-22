using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.EditorTools;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelLoot
    {
        public const string Data = "Assets/_Project/Data/Loot/Motel";
        public const string NetworkFolder = MotelKit.Art + "/Resources/Motel/Network";
        public static LootCatalog Build(MotelKit k)
        {
            MotelKit.Ensure(Data);
            MotelKit.Ensure(NetworkFolder);
            var entries = new List<LootCatalog.Entry>();
            foreach (var spec in MotelLootItems.Items)
            {
                string path = Data + "/" + spec.Id + ".asset";
                var d = AssetDatabase.LoadAssetAtPath<LootDefinition>(path);
                if (d == null)
                {
                    d = ScriptableObject.CreateInstance<LootDefinition>();
                    MotelLootItems.Setup(d, spec);
                    AssetDatabase.CreateAsset(d, path);
                }
                if (d.Model == null)
                {
                    d.EditorSetModel(MotelLootModels.Build(k, d, spec.Model), 0, d.Size);
                    EditorUtility.SetDirty(d);
                }
                string prefabPath = NetworkFolder + "/Loot_" + d.Id + ".prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    prefab = Prefab(d, prefabPath);
                }
                entries.Add(new LootCatalog.Entry
                {
                    definition = d, prefab = prefab
                });
            }
            var catalog = SerializedWiring.LoadOrCreateAsset<LootCatalog>(Data + "/MotelCatalog.asset");
            catalog.EditorSet(entries);
            EditorUtility.SetDirty(catalog);
            return catalog;
        }
        // Shared gameplay components, authored locally to avoid rewriting the global loot catalog/prefab library.
        private static GameObject Prefab(LootDefinition d, string path)
        {
            var root = new GameObject("Loot_" + d.Id);
            try
            {
                var no = root.AddComponent<NetworkObject>();
                no.DontDestroyWithOwner = true;
                no.AutoObjectParentSync = false;
                Transform visual = MotelKit.Group("Visual", root.transform);
                visual.localScale = d.Size;
                visual.gameObject.AddComponent<BoxCollider>();
                ModelFit.Place(d.Model, root.transform, Vector3.zero, d.Size, 0);
                var body = root.AddComponent<Rigidbody>();
                body.mass = d.PhysicsMass;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = d.CarryClass >= CarryClass.Heavy ? CollisionDetectionMode.ContinuousSpeculative : CollisionDetectionMode.ContinuousDynamic;
                root.AddComponent<Grabbable>();
                var damage = AssetDatabase.LoadAssetAtPath<LootDamageConfig>(LootCatalogBuilder.DamageConfigPath);
                SerializedWiring.Set(root.AddComponent<LootItem>(), "definition", d);
                SerializedWiring.Set(root.GetComponent<LootItem>(), "damageConfig", damage);
                SerializedWiring.Set(root.AddComponent<LootFeedback>(), "damageConfig", damage);
                if (d.CarryClass >= CarryClass.Heavy)
                {
                    SerializedWiring.Set(root.AddComponent<SharedCarryable>(), "config", AssetDatabase.LoadAssetAtPath<SharedCarryConfig>(LootPrefabGenerator.SharedCarryConfigPath));
                    root.AddComponent<NetworkSharedCarry>();
                    // Keep the clamped physics body from being shoved around by handheld salvage.
                    string frictionPath = MotelKit.Art + "/HeavyFriction.asset";
                    var friction = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(frictionPath);
                    if (friction == null)
                    {
                        friction = new PhysicsMaterial("MotelHeavy")
                        {
                            staticFriction = 1.5f,
                            dynamicFriction = 1.1f,
                            frictionCombine = PhysicsMaterialCombine.Maximum
                        };
                        AssetDatabase.CreateAsset(friction, frictionPath);
                    }
                    visual.GetComponent<Collider>().sharedMaterial = friction;
                }
                var nt = root.AddComponent<NetworkTransform>();
                nt.AuthorityMode = NetworkTransform.AuthorityModes.Owner;
                nt.SyncScaleX = nt.SyncScaleY = nt.SyncScaleZ = false;
                nt.Interpolate = true;
                var net = root.AddComponent<NetworkLoot>();
                SerializedWiring.Set(net, "networkTransform", nt);
                SerializedWiring.Set(net, "config", AssetDatabase.LoadAssetAtPath<LootNetConfig>(NetworkContentBuilder.LootNetConfigPath));
                SalvageLootPrefabBuilder.Apply(root, d);
                SerializedWiring.SetLayerRecursively(root, GameLayers.LootLayer);
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                NetworkObjectIds.StampPrefab(prefab);
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
        public static GameObject RunPrefab(HorrorConfig horror)
        {
            string path = NetworkFolder + "/MotelRunState.prefab";
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkContentBuilder.RunStatePrefabPath);
            var root = Object.Instantiate(source);
            root.name = "MotelRunState";
            try
            {
                SerializedWiring.Set(root.GetComponent<RunHorrorDirector>(), "config", horror);
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                NetworkObjectIds.StampPrefab(prefab);
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
