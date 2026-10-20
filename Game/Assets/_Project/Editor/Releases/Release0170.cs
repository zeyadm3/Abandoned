using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.17.0 sledgehammer: a new catalog item (append only), its icon and the tool tunables. No scene changes.</summary>
    public static class Release0170
    {
        [MenuItem(ToolMenu.Features + "0.17.0 Sledgehammer", priority = ToolMenu.FeaturesP + 1)]
        public static void Author() => ReleaseKit.Gear();

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
