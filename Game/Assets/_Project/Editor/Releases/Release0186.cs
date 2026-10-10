using Abandoned.Extraction;
using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.18.6 overtime: the bonus tunables and RunOvertime on the RunState prefab. No scene changes.</summary>
    public static class Release0186
    {
        [MenuItem("Tools/Abandoned/Content/Overtime (0.18.6)")]
        public static void Author()
        {
            ReleaseKit.Config<BonusConfig>(ReleaseKit.ExtractionResources, BonusConfig.ResourcePath);
            NetworkContentBuilder.CreateRunStatePrefab();
            BuildScript.ApplyPlayerSettings();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
