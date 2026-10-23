using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A dev Mac player (Builds/MacDev, what Tools/nettest.sh launches) with the open world's scenes, built from the open
    /// editor. Same options as BuildScript's dev flavour, but only scenes that exist: places still being written by
    /// other agents (in BuildScenes.All before their scenes are saved) would stop the normal build.
    /// </summary>
    public static class WorldDevBuild
    {
        public static readonly string[] Scenes =
        {
            HqBuilder.ScenePath, WorldBuilder.ScenePath, WorldVanTestScene.ScenePath, MallBuilder.ScenePath, TestMapBuilder.ScenePath,
        };

        [MenuItem(ToolMenu.OpenWorld + "Dev Mac Build (nettest)", priority = ToolMenu.OpenWorldP + 40)]
        public static void BuildMenu() => Build();

        public static bool Build()
        {
            BuildScript.ApplyPlayerSettings();
            string folder = BuildScript.OutputFolder(BuildPlatform.Mac, BuildFlavor.Dev);
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);
            UnityEditor.OSXStandalone.UserBuildSettings.architecture = OSArchitecture.ARM64;
            var options = new BuildPlayerOptions
            {
                scenes = Scenes.Where(File.Exists).ToArray(),
                locationPathName = BuildScript.ExecutablePath(BuildPlatform.Mac, BuildFlavor.Dev),
                target = BuildTarget.StandaloneOSX,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildScript.OptionsOf(BuildFlavor.Dev),
            };
            BuildInfoAsset.Stamp(BuildFlavor.Dev, null);
            BuildReport report;
            try { report = BuildPipeline.BuildPlayer(options); }
            finally { BuildInfoAsset.Reset(); }
            bool ok = report.summary.result == BuildResult.Succeeded;
            if (ok) File.WriteAllText(Path.Combine(folder, "BUILD.txt"), "open-world dev build\n");
            Debug.Log($"[WorldDevBuild] {(ok ? "OK" : "FAILED")}: {report.summary.result}, {report.summary.totalErrors} error(s), {report.summary.totalTime.TotalSeconds:0} s.");
            return ok;
        }
    }
}
