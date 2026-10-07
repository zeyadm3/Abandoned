using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Gives loot definitions their CC0 art models (M7.1) and sizes each Size box to hug its model
    /// (keeping the authored largest side). Only fills in definitions that have no model yet, so a
    /// model picked or tuned in the inspector is kept. Items without a match keep their placeholder.
    /// </summary>
    public static class LootModelBuilder
    {
        public const string Furniture = ThirdPartyModelImport.Root + "Kenney/FurnitureKit/";

        // id -> Kenney Furniture Kit model, yaw (so the Size box lines up)
        private static readonly (string id, string model, float yaw)[] Models =
        {
            ("laptop", "laptop", 0f),
            ("flatscreen_tv", "televisionModern", 0f),
            ("espresso_machine", "kitchenCoffeeMachine", 0f),
            ("vintage_tv", "televisionVintage", 0f),
            ("hifi_speaker", "speaker", 0f),
            ("microwave", "kitchenMicrowave", 0f),
            ("toaster", "toaster", 0f),
            ("blender", "kitchenBlender", 0f),
            ("designer_lamp", "lampRoundTable", 0f),
            ("computer_monitor", "computerScreen", 0f),
            ("retro_radio", "radio", 0f),
            ("washing_machine", "washer", 0f),
            ("fridge", "kitchenFridge", 0f),
            ("designer_chair", "loungeDesignChair", 0f),
            ("designer_sofa", "loungeDesignSofa", 0f),
            ("bear_head", "bear", 0f),
        };

        [MenuItem("Tools/Abandoned/Art/Assign Loot Models")]
        public static void AssignMissing()
        {
            foreach ((string id, string modelName, float yaw) in Models)
            {
                LootDefinition definition = LootCatalogBuilder.Load(id);
                if (definition == null || definition.Model != null) continue;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>($"{Furniture}{modelName}.fbx");
                if (model == null)
                {
                    Debug.LogWarning($"[Art] No model {modelName} for loot {id}.");
                    continue;
                }
                float largest = Mathf.Max(definition.Size.x, Mathf.Max(definition.Size.y, definition.Size.z));
                definition.EditorSetModel(model, yaw, ModelFit.SizeFor(model, yaw, largest));
                EditorUtility.SetDirty(definition);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
