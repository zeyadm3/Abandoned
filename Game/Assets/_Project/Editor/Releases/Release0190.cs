using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.19.0 ping marker: RunPings on the RunState prefab and the blip's sound row (the input action is in the asset).</summary>
    public static class Release0190
    {
        [MenuItem(ToolMenu.Features + "0.19.0 Ping Marker", priority = ToolMenu.FeaturesP + 12)]
        public static void Author()
        {
            NetworkContentBuilder.CreateRunStatePrefab();
            SoundBankBuilder.CreateMissing();
            BuildScript.ApplyPlayerSettings();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
