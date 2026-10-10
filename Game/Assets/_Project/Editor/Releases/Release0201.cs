using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.20.1 lockpick: the catalog item and its icon.</summary>
    public static class Release0201
    {
        [MenuItem("Tools/Abandoned/Content/Lockpick (0.20.1)")]
        public static void Author() => ReleaseKit.Gear();

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
