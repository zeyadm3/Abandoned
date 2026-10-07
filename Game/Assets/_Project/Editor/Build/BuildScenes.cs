using System.Linq;
using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The one list of scenes, first scene first. <see cref="ApplyToEditorSettings"/> copies all of it into
    /// File > Build Profiles (the editor and tests load the TestMap from there); a player gets
    /// <see cref="ForPlayer"/>: the TestMap is a feature-testing map, not a level, so only dev builds
    /// (nettest) carry it.
    /// </summary>
    public static class BuildScenes
    {
        public static readonly string[] All =
        {
            // The company HQ (M6): builds open here; players join here; contracts drive everyone to the mall.
            HqBuilder.ScenePath,
            MallBuilder.ScenePath,
            TestMapBuilder.ScenePath,
        };

        public static string[] ForPlayer(BuildFlavor flavor) =>
            flavor == BuildFlavor.Dev ? All : All.Where(s => s != TestMapBuilder.ScenePath).ToArray();

        public static void ApplyToEditorSettings()
        {
            EditorBuildSettingsScene[] wanted = All.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
            bool same = EditorBuildSettings.scenes.Length == wanted.Length &&
                        EditorBuildSettings.scenes.Zip(wanted, (a, b) => a.path == b.path && a.enabled).All(x => x);
            if (!same) EditorBuildSettings.scenes = wanted;
        }
    }
}
