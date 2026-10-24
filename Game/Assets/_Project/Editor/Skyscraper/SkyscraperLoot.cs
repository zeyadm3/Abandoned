using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Content pipeline for the Skyscraper Restaurant's salvage: ScriptableObject definitions,
    /// procedural 3D models, network loot prefabs, and the local/runtime loot catalog.
    /// </summary>
    public static class SkyscraperLoot
    {
        public const string DataFolder = "Assets/_Project/Data/Loot/Skyscraper";
        public const string PrefabsFolder = SkyscraperArt.Folder + "/Prefabs";

        public static LootCatalog EnsureContent()
        {
            EnsureFolder(DataFolder);
            EnsureFolder(PrefabsFolder);

            int createdDefs = 0;
            var entries = new List<LootCatalog.Entry>();

            foreach (SkyscraperLootItems.ItemSpec item in SkyscraperLootItems.Items)
            {
                string defPath = $"{DataFolder}/{item.Id}.asset";
                var def = AssetDatabase.LoadAssetAtPath<LootDefinition>(defPath);
                if (def == null)
                {
                    def = ScriptableObject.CreateInstance<LootDefinition>();
                    def.EditorSetup(item.Id, item.Name, item.Min, item.Max, item.Kg, item.Class,
                        item.Size, item.Fragility, item.Material, item.Noise, item.Rarity, item.Shape, item.Color);
                    def.EditorSetSpawning(item.Tags, item.Jackpot);
                    AssetDatabase.CreateAsset(def, defPath);
                    createdDefs++;
                }

                if (def.SpawnTags == null || def.SpawnTags.Length == 0)
                {
                    def.EditorSetSpawning(item.Tags, item.Jackpot);
                    EditorUtility.SetDirty(def);
                }
            }

            SkyscraperLootModels.AssignMissing(DataFolder);

            foreach (SkyscraperLootItems.ItemSpec item in SkyscraperLootItems.Items)
            {
                string defPath = $"{DataFolder}/{item.Id}.asset";
                var def = AssetDatabase.LoadAssetAtPath<LootDefinition>(defPath);
                if (def == null) continue;

                string prefabPath = $"{PrefabsFolder}/Loot_{def.Id}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                    prefab = BuildNetworkPrefab(def, prefabPath);
                else prefab = RepairNetworkPrefab(prefab,prefabPath);

                entries.Add(new LootCatalog.Entry { definition = def, prefab = prefab });
            }

            string catalogPath = $"{DataFolder}/SkyscraperCatalog.asset";
            var catalog = SerializedWiring.LoadOrCreateAsset<LootCatalog>(catalogPath);
            catalog.EditorSet(entries);
            EditorUtility.SetDirty(catalog);

            AssetDatabase.SaveAssets();
            if (createdDefs > 0)
                Debug.Log($"[SkyscraperLoot] Created {createdDefs} new salvage definitions.");

            return catalog;
        }

        private static GameObject BuildNetworkPrefab(LootDefinition d, string path)
        {
            var root = new GameObject("Loot_" + d.Id);
            try
            {
                var no = root.AddComponent<NetworkObject>();
                no.DontDestroyWithOwner = true;
                no.AutoObjectParentSync = false;

                Transform visual = new GameObject("Visual").transform;
                visual.SetParent(root.transform, false);
                visual.localScale = d.Size;
                visual.gameObject.AddComponent<BoxCollider>();

                if (d.Model != null)
                    ModelFit.Place(d.Model, root.transform, Vector3.zero, d.Size, 0f);

                var rb = root.AddComponent<Rigidbody>();
                rb.mass = d.PhysicsMass;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = d.CarryClass >= CarryClass.Heavy
                    ? CollisionDetectionMode.ContinuousSpeculative
                    : CollisionDetectionMode.ContinuousDynamic;

                root.AddComponent<Grabbable>();
                var dmgConfig = AssetDatabase.LoadAssetAtPath<LootDamageConfig>(LootCatalogBuilder.DamageConfigPath);
                SerializedWiring.Set(root.AddComponent<LootItem>(), "definition", d);
                SerializedWiring.Set(root.GetComponent<LootItem>(), "damageConfig", dmgConfig);
                SerializedWiring.Set(root.AddComponent<LootFeedback>(), "damageConfig", dmgConfig);

                if (d.CarryClass >= CarryClass.Heavy)
                {
                    var carryConfig = AssetDatabase.LoadAssetAtPath<SharedCarryConfig>(LootPrefabGenerator.SharedCarryConfigPath);
                    SerializedWiring.Set(root.AddComponent<SharedCarryable>(), "config", carryConfig);
                    root.AddComponent<NetworkSharedCarry>();

                    string frictionPath = $"{SkyscraperArt.Folder}/SkyscraperHeavyFriction.asset";
                    var friction = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(frictionPath);
                    if (friction == null)
                    {
                        friction = new PhysicsMaterial("SkyscraperHeavyFriction")
                        {
                            staticFriction = 1.4f,
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

                // NetworkSharedCarry's RequireComponent already adds this on heavy salvage.
                var net = root.GetComponent<NetworkLoot>() ?? root.AddComponent<NetworkLoot>();
                SerializedWiring.Set(net, "networkTransform", nt);
                var netConfig = AssetDatabase.LoadAssetAtPath<LootNetConfig>(NetworkContentBuilder.LootNetConfigPath);
                SerializedWiring.Set(net, "config", netConfig);

                SalvageLootPrefabBuilder.Apply(root, d);
                SerializedWiring.SetLayerRecursively(root, GameLayers.LootLayer);

                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                NetworkObjectIds.StampPrefab(savedPrefab);
                return savedPrefab;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        private static GameObject RepairNetworkPrefab(GameObject prefab,string path)
        {
            var copies=prefab.GetComponents<NetworkLoot>();
            if(copies.Length==1 && copies[0].Config!=null)return prefab;
            GameObject root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                copies=root.GetComponents<NetworkLoot>();
                var net=copies.Length>0?copies[0]:root.AddComponent<NetworkLoot>();
                for(int i=copies.Length-1;i>0;i--)Object.DestroyImmediate(copies[i]);
                SerializedWiring.Set(net,"config",AssetDatabase.LoadAssetAtPath<LootNetConfig>(NetworkContentBuilder.LootNetConfigPath));
                SerializedWiring.Set(net,"networkTransform",root.GetComponent<NetworkTransform>());
                prefab=PrefabUtility.SaveAsPrefabAsset(root,path);
                NetworkObjectIds.StampPrefab(prefab);
                return prefab;
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }

        public static void RepairAndExit()=>ReleaseKit.RunAndExit(()=>{EnsureContent();});
    }
}
