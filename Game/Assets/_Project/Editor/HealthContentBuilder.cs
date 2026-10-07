using Abandoned.Equipment;
using Abandoned.Player;
using UnityEditor;

namespace Abandoned.EditorTools
{
    public static class HealthContentBuilder
    {
        public const string HealthPath = "Assets/_Project/Data/Player/PlayerHealthConfig.asset";
        public const string FlashlightPath = "Assets/_Project/Data/Equipment/FlashlightConfig.asset";

        [MenuItem("Tools/Abandoned/Create Health and Flashlight Content")]
        public static void Create()
        {
            SerializedWiring.LoadOrCreateAsset<PlayerHealthConfig>(HealthPath);
            SerializedWiring.LoadOrCreateAsset<FlashlightConfig>(FlashlightPath);
            AssetDatabase.SaveAssets();
        }
    }
}
