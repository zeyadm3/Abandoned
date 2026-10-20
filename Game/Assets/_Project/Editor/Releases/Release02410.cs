using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.10: the Crew Director (Left 4 Dead-style pacing). Code only; this step creates its tunables asset
    /// (Data/Threats/Resources/CrewDirectorConfig) and stamps the version. No scene changes. Details:
    /// Docs/CREW_DIRECTOR_0.24.10.md.
    /// </summary>
    public static class Release02410
    {
        [MenuItem(ToolMenu.Data + "Create Crew Director Config", priority = ToolMenu.DataP + 6)]
        public static void Author()
        {
            PolishAssets.EnsureFolder("Assets/_Project/Data/Threats/Resources");
            SerializedWiring.LoadOrCreateAsset<Abandoned.Threats.CrewDirectorConfig>(RunPressureContentBuilder.DirectorPath);
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            Debug.Log($"[CrewDirector] Config ready at {RunPressureContentBuilder.DirectorPath}. Version {BuildScript.Version}.");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
