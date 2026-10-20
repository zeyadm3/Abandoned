using Abandoned.Extraction;
using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.18.1 rush orders: the bonus tunables and RunRush on the RunState prefab. No scene changes.</summary>
    public static class Release0181
    {
        [MenuItem(ToolMenu.Features + "0.18.1 Rush Orders", priority = ToolMenu.FeaturesP + 7)]
        public static void Author()
        {
            ReleaseKit.Config<BonusConfig>(ReleaseKit.ExtractionResources, BonusConfig.ResourcePath);
            NetworkContentBuilder.CreateRunStatePrefab();
            BuildScript.ApplyPlayerSettings();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
