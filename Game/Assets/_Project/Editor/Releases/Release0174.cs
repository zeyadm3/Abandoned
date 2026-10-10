using Abandoned.Extraction;
using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.17.4 aftershocks: the tunables, RunAftershocks on the RunState prefab, the shock's sound row. No scene changes.</summary>
    public static class Release0174
    {
        [MenuItem("Tools/Abandoned/Content/Aftershocks (0.17.4)")]
        public static void Author()
        {
            ReleaseKit.Config<AftershockConfig>(ReleaseKit.ExtractionResources, AftershockConfig.ResourcePath);
            NetworkContentBuilder.CreateRunStatePrefab();
            SoundBankBuilder.CreateMissing();
            BuildScript.ApplyPlayerSettings();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
