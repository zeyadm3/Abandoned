using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.19.2 ghosts: RunGhosts on the RunState prefab and the sound rows.</summary>
    public static class Release0192
    {
        [MenuItem(ToolMenu.Features + "0.19.2 Ghost Knock, Flicker and Nudge", priority = ToolMenu.FeaturesP + 13)]
        public static void Author()
        {
            NetworkContentBuilder.CreateRunStatePrefab();
            SoundBankBuilder.CreateMissing();
            BuildScript.ApplyPlayerSettings();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
