using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.19.2 ghosts: RunGhosts on the RunState prefab and the sound rows.</summary>
    public static class Release0192
    {
        [MenuItem("Tools/Abandoned/Content/Ghost Knock, Flicker and Nudge (0.19.2)")]
        public static void Author()
        {
            NetworkContentBuilder.CreateRunStatePrefab();
            SoundBankBuilder.CreateMissing();
            BuildScript.ApplyPlayerSettings();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
