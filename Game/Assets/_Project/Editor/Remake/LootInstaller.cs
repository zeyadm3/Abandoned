using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Puts the remastered loot models (Tools/Blender/remake/loot/remaster.py, Art/Remake/Loot) on the loot definitions
    /// and regenerates their prefabs. Each model was baked from the item's own prefab space (LootExport), so it already
    /// sits at the right size and turn: yaw is 0 and the Size box (the collider, spawn fit, gameplay weight) is kept as
    /// authored. Prefabs are regenerated in place, so GUIDs and network ids stay.
    /// </summary>
    public static class LootInstaller
    {
        [MenuItem(ToolMenu.Loot + "Install Remastered Loot (remake)", priority = ToolMenu.LootP + 41)]
        public static void Install()
        {
            foreach (LootDefinition definition in Assign()) LootPrefabGenerator.Generate(definition);
            LootPrefabGenerator.WriteCatalog();
            AssetDatabase.SaveAssets();
        }

        /// <summary>Points every definition that has a remaster at it; returns those definitions (prefabs not regenerated).</summary>
        public static System.Collections.Generic.List<LootDefinition> Assign()
        {
            RemakeMaterials.EnsureModelMaterials(RemakePaths.Loot);
            var installed = new System.Collections.Generic.List<LootDefinition>();
            int missing = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:LootDefinition"))
            {
                var definition = AssetDatabase.LoadAssetAtPath<LootDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (definition == null) continue;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>($"{RemakePaths.Loot}/Loot_{definition.Id}.fbx");
                if (model == null)
                {
                    missing++;
                    continue;
                }
                if (definition.Model != model || definition.ModelYaw != 0f)
                {
                    definition.EditorSetModel(model, 0f, definition.Size);
                    EditorUtility.SetDirty(definition);
                }
                installed.Add(definition);
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[Remake] {installed.Count} remastered loot models installed ({missing} definitions have no remaster yet).");
            return installed;
        }
    }
}
