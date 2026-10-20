using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.20.4 thermal camera: the catalog item and its icon.</summary>
    public static class Release0204
    {
        [MenuItem(ToolMenu.Features + "0.20.4 Thermal Camera", priority = ToolMenu.FeaturesP + 17)]
        public static void Author() => ReleaseKit.Gear();

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
