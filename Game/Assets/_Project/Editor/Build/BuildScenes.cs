using System.Linq;
using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The one list of scenes that go into a player, first scene first. Builds use it and
    /// <see cref="ApplyToEditorSettings"/> copies it into File > Build Profiles so both agree.
    /// </summary>
    public static class BuildScenes
    {
        public static readonly string[] All =
        {
            // The run (M5): builds open in the mall. TestBuilding stays for tests and nettests.
            MallBuilder.ScenePath,
            TestBuildingBuilder.ScenePath,
        };

        public static void ApplyToEditorSettings()
        {
            EditorBuildSettingsScene[] wanted = All.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
            bool same = EditorBuildSettings.scenes.Length == wanted.Length &&
                        EditorBuildSettings.scenes.Zip(wanted, (a, b) => a.path == b.path && a.enabled).All(x => x);
            if (!same) EditorBuildSettings.scenes = wanted;
        }
    }
}
