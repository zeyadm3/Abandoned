using Abandoned.Extraction;
using Abandoned.Threats;
using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The 2026-10-08 pacing pass's tunables as assets the Inspector can edit: monster arrivals and drift
    /// (Data/Threats/Resources/ThreatPacingConfig) and the building's decay schedule and total collapse
    /// (Data/Extraction/Resources/BuildingDecayConfig). Only creates what's missing, so tuned values survive
    /// rebuilds; without the assets the game runs on the same values from code.
    /// </summary>
    public static class RunPressureContentBuilder
    {
        public const string PacingPath = "Assets/_Project/Data/Threats/Resources/ThreatPacingConfig.asset";
        public const string DecayPath = "Assets/_Project/Data/Extraction/Resources/BuildingDecayConfig.asset";

        [MenuItem("Tools/Abandoned/Content/Create Pacing And Decay Configs")]
        public static void CreateMissing()
        {
            PolishAssets.EnsureFolder("Assets/_Project/Data/Threats/Resources");
            PolishAssets.EnsureFolder("Assets/_Project/Data/Extraction/Resources");
            SerializedWiring.LoadOrCreateAsset<ThreatPacingConfig>(PacingPath);
            SerializedWiring.LoadOrCreateAsset<BuildingDecayConfig>(DecayPath);
            AssetDatabase.SaveAssets();
        }
    }
}
