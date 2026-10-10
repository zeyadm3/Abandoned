using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.20.4 thermal camera: the catalog item and its icon.</summary>
    public static class Release0204
    {
        [MenuItem("Tools/Abandoned/Content/Thermal Camera (0.20.4)")]
        public static void Author() => ReleaseKit.Gear();

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
