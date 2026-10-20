using Abandoned.Equipment;
using Abandoned.Player;
using UnityEditor;

namespace Abandoned.EditorTools
{
    public static class HealthContentBuilder
    {
        public const string HealthPath = "Assets/_Project/Data/Player/PlayerHealthConfig.asset";
        public const string FlashlightPath = "Assets/_Project/Data/Equipment/FlashlightConfig.asset";

        [MenuItem(ToolMenu.Data + "Create Health and Flashlight Content", priority = ToolMenu.DataP + 2)]
        public static void Create()
        {
            SerializedWiring.LoadOrCreateAsset<PlayerHealthConfig>(HealthPath);
            FlashlightConfig flashlight = SerializedWiring.LoadOrCreateAsset<FlashlightConfig>(FlashlightPath);
            UpgradeVisibility(flashlight);
            AssetDatabase.SaveAssets();
        }

        private static void UpgradeVisibility(FlashlightConfig config)
        {
            var data = new SerializedObject(config);
            var revision = data.FindProperty("visibilityRevision");
            if (revision.intValue >= 1) return;
            // Apply the requested readability upgrade once, preserving later inspector tuning and battery balance.
            data.FindProperty("<Range>k__BackingField").floatValue = 26f;
            data.FindProperty("<Intensity>k__BackingField").floatValue = 28f;
            data.FindProperty("<SpotAngle>k__BackingField").floatValue = 68f;
            data.FindProperty("<Color>k__BackingField").colorValue = new UnityEngine.Color(.95f, .98f, 1f);
            data.FindProperty("innerConeRatio").floatValue = .6f;
            revision.intValue = 1;
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }
    }
}
