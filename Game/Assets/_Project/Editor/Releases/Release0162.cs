using Abandoned.Extraction;
using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.16.2 seeded route changes: the tunables asset and RunRoutes on the RunState prefab. No scene changes.</summary>
    public static class Release0162
    {
        [MenuItem("Tools/Abandoned/Content/Seeded Route Changes (0.16.2)")]
        public static void Author()
        {
            ReleaseKit.Config<RouteConfig>(ReleaseKit.ExtractionResources, RouteConfig.ResourcePath);
            NetworkContentBuilder.CreateRunStatePrefab();
            BuildScript.ApplyPlayerSettings();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
