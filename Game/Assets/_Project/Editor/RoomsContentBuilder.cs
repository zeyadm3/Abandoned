using Abandoned.Extraction;
using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What doors and lockers (0.16.0) need besides the mall itself: their tunables as an asset the Inspector
    /// can edit (Data/Extraction/Resources/DoorConfig), the run's door state on the RunState prefab, and the
    /// sound bank's rows for their sounds. Only creates what's missing, so tuned values survive; without the
    /// config asset the game runs on the same values from code.
    /// </summary>
    public static class RoomsContentBuilder
    {
        public const string ConfigPath = "Assets/_Project/Data/Extraction/Resources/DoorConfig.asset";

        [MenuItem("Tools/Abandoned/Content/Create Door And Locker Config")]
        public static void CreateMissing()
        {
            PolishAssets.EnsureFolder("Assets/_Project/Data/Extraction/Resources");
            SerializedWiring.LoadOrCreateAsset<DoorConfig>(ConfigPath);
            NetworkContentBuilder.CreateRunStatePrefab(); // adds RunDoors when the prefab doesn't carry it yet
            SoundBankBuilder.CreateMissing();             // empty rows for the new synthesised cues
            AssetDatabase.SaveAssets();
        }
    }
}
