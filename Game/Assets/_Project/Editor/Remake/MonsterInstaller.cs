using Abandoned.Threats;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Puts the remodelled monsters (Tools/Blender/remake/threats, Art/Remake/Threats) on the existing threat prefabs.
    /// Only the visual and its animator change (ThreatRigBuilder.Upgrade): speed, damage, AI configs, NavMesh agents and
    /// the catalog order stay as authored.
    /// </summary>
    public static class MonsterInstaller
    {
        public static readonly string[] Monsters = { "BlindOne", "Stalker", "Collector", "Hunter", "Weight", "Thing", "Crawlers", "LastHunter" };

        [MenuItem(ToolMenu.Prefabs + "Install Remade Monsters (remake)", priority = ToolMenu.PrefabsP + 5)]
        public static void Install()
        {
            int done = 0;
            RemakeMaterials.EnsureModelMaterials(RemakePaths.Threats);
            foreach (string name in Monsters)
            {
                string model = $"{RemakePaths.Threats}/{name}.fbx";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(model) == null) continue;
                AssetDatabase.ImportAsset(model, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                string path = $"{ThreatContentBuilder.Folder}/{name}.prefab";
                var definition = AssetDatabase.LoadAssetAtPath<ThreatDefinition>($"{ThreatContentBuilder.DataFolder}/{name}Definition.asset");
                if (definition == null || AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                {
                    Debug.LogWarning($"[Remake] no threat prefab for {name} yet (Create Threat Prefabs first).");
                    continue;
                }
                GameObject prefab = ThreatRigBuilder.Upgrade(path, name, definition);
                NetworkObjectIds.StampPrefab(prefab);
                done++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[Remake] {done} remade monsters installed.");
        }
    }
}
