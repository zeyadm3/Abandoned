using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.19.0 ping marker: RunPings on the RunState prefab and the blip's sound row (the input action is in the asset).</summary>
    public static class Release0190
    {
        [MenuItem("Tools/Abandoned/Content/Ping Marker (0.19.0)")]
        public static void Author()
        {
            NetworkContentBuilder.CreateRunStatePrefab();
            SoundBankBuilder.CreateMissing();
            BuildScript.ApplyPlayerSettings();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
