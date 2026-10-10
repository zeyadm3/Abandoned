using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.19.10 more achievements: the ten new definitions appended to the catalog.</summary>
    public static class Release01910
    {
        [MenuItem("Tools/Abandoned/Content/More Achievements (0.19.10)")]
        public static void Author()
        {
            AchievementsBuilder.CreateMissing();
            BuildScript.ApplyPlayerSettings();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
