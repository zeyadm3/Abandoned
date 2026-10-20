using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.19.10 more achievements: the ten new definitions appended to the catalog.</summary>
    public static class Release01910
    {
        [MenuItem(ToolMenu.Features + "0.19.10 More Achievements", priority = ToolMenu.FeaturesP + 14)]
        public static void Author()
        {
            AchievementsBuilder.CreateMissing();
            BuildScript.ApplyPlayerSettings();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
