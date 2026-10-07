using Abandoned.Company;
using Abandoned.Contracts;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>Company and contract data (GDD 13-14): configs, the first four modifiers, and the session's CompanyService prefab.</summary>
    public static class CompanyContentBuilder
    {
        public const string CompanyConfigPath = "Assets/_Project/Data/Company/CompanyConfig.asset";
        public const string ContractFolder = "Assets/_Project/Data/Contracts";
        public const string ContractConfigPath = ContractFolder + "/ContractConfig.asset";
        public const string CompanyServicePrefabPath = NetworkContentBuilder.NetworkPrefabFolder + "/CompanyService.prefab";

        [MenuItem("Tools/Abandoned/Create Company Content")]
        public static GameObject CreateMissing()
        {
            var company = LoadOrCreateAsset<CompanyConfig>(CompanyConfigPath);
            var contracts = LoadOrCreateAsset<ContractConfig>(ContractConfigPath);
            ContractModifier[] modifiers =
            {
                Modifier("power_off", "Power Off", "No electricity: the building is dark. Bring flashlights.", powerOff: true, bonus: 0.1f),
                Modifier("unstable", "Unstable", "Structural stability -20 %. Watch every step.", stability: -0.2f, bonus: 0.15f),
                Modifier("rush_job", "Rush Job", "A much shorter extraction window, a better bonus.", window: 0.6f, bonus: 0.25f),
                Modifier("fragile_collection", "Fragile Collection", "Lots of glass and antiques. Don't drop anything.", fragile: 3f, bonus: 0.1f),
            };
            if (contracts.Modifiers == null || contracts.Modifiers.Length == 0)
            {
                contracts.EditorSetModifiers(modifiers);
                EditorUtility.SetDirty(contracts);
            }

            var equipment = EquipmentContentBuilder.CreateMissing();
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(CompanyServicePrefabPath);
            if (existing != null && existing.GetComponent<CompanyService>() != null)
            {
                // Keep the prefab (its network hash); just make sure every reference is wired.
                var existingService = existing.GetComponent<CompanyService>();
                Set(existingService, "config", company);
                Set(existingService, "contracts", contracts);
                Set(existingService, "equipment", equipment);
                PrefabUtility.SavePrefabAsset(existing);
                return existing;
            }
            var root = new GameObject("CompanyService");
            var no = root.AddComponent<Unity.Netcode.NetworkObject>();
            no.DontDestroyWithOwner = true;
            no.SynchronizeTransform = false;
            var service = root.AddComponent<CompanyService>();
            Set(service, "config", company);
            Set(service, "contracts", contracts);
            Set(service, "equipment", equipment);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, CompanyServicePrefabPath);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            AssetDatabase.SaveAssets();
            return prefab;
        }

        // Created when missing; never overwritten, so inspector tuning is kept.
        private static ContractModifier Modifier(string id, string displayName, string description, float stability = 0f,
            float window = 1f, float bonus = 0f, bool powerOff = false, float fragile = 1f, float loot = 1f, int minLevel = 1)
        {
            string path = $"{ContractFolder}/Modifier_{id}.asset";
            var m = AssetDatabase.LoadAssetAtPath<ContractModifier>(path);
            if (m != null) return m;
            m = ScriptableObject.CreateInstance<ContractModifier>();
            m.EditorSetup(id, displayName, description, stability, window, bonus, powerOff, fragile, loot, minLevel);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }
    }
}
