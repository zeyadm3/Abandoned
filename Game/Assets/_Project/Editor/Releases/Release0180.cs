using Abandoned.Extraction;
using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.18.0 boss requests: the bonus tunables and RunRequests on the RunState prefab. No scene changes.</summary>
    public static class Release0180
    {
        [MenuItem("Tools/Abandoned/Content/Boss Requests (0.18.0)")]
        public static void Author()
        {
            ReleaseKit.Config<BonusConfig>(ReleaseKit.ExtractionResources, BonusConfig.ResourcePath);
            NetworkContentBuilder.CreateRunStatePrefab();
            BuildScript.ApplyPlayerSettings();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
