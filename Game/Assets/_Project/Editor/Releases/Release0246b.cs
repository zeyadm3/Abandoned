using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Application logo release: Apple's native appearances and Windows' light artwork.</summary>
    public static class Release0246b
    {
        [MenuItem("Tools/Abandoned/Branding/Install Application Logo (0.24.6b)")]
        public static void Author()
        {
            AssetDatabase.Refresh();
            ApplicationBranding.WriteMacIconSource();
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("[Branding0246b] Saved light/dark Mac icon sources and the light Windows icon as 0.24.6b.");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
