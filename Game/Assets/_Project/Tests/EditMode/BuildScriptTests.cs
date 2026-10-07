using System.IO;
using Abandoned.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;

namespace Abandoned.Tests
{
    public class BuildScriptTests
    {
        [Test]
        public void OutputFoldersAndExecutablesPerPlatformAndFlavor()
        {
            Assert.AreEqual(Path.Combine("Builds", "Mac"), BuildScript.OutputFolder(BuildPlatform.Mac, BuildFlavor.Shareable));
            Assert.AreEqual(Path.Combine("Builds", "MacDev"), BuildScript.OutputFolder(BuildPlatform.Mac, BuildFlavor.Dev));
            Assert.AreEqual(Path.Combine("Builds", "WindowsRelease"), BuildScript.OutputFolder(BuildPlatform.Windows, BuildFlavor.Release));
            Assert.AreEqual(Path.Combine("Builds", "Windows", "Abandoned.exe"), BuildScript.ExecutablePath(BuildPlatform.Windows, BuildFlavor.Shareable));
            Assert.AreEqual(Path.Combine("Builds", "MacDev", "Abandoned.app"), BuildScript.ExecutablePath(BuildPlatform.Mac, BuildFlavor.Dev));
            Assert.AreEqual(BuildTarget.StandaloneOSX, BuildScript.TargetOf(BuildPlatform.Mac));
            Assert.AreEqual(BuildTarget.StandaloneWindows64, BuildScript.TargetOf(BuildPlatform.Windows));
        }

        [Test]
        public void OnlyReleaseBuildsLeaveOutSteamAppIdAndOnlyDevBuildsAreDevelopment()
        {
            Assert.IsTrue(BuildScript.ShipsSteamAppId(BuildFlavor.Dev));
            Assert.IsTrue(BuildScript.ShipsSteamAppId(BuildFlavor.Shareable));
            Assert.IsFalse(BuildScript.ShipsSteamAppId(BuildFlavor.Release), "the real App ID comes from Steam");
            Assert.IsTrue(BuildScript.OptionsOf(BuildFlavor.Dev).HasFlag(BuildOptions.Development));
            Assert.IsFalse(BuildScript.OptionsOf(BuildFlavor.Shareable).HasFlag(BuildOptions.Development));
            Assert.IsTrue(BuildScript.OptionsOf(BuildFlavor.Release).HasFlag(BuildOptions.StrictMode));
        }

        [Test]
        public void PlayerSettingsUseMonoAndRunInBackground()
        {
            BuildScript.ApplyPlayerSettings();
            Assert.AreEqual(ScriptingImplementation.Mono2x, PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone));
            Assert.IsTrue(PlayerSettings.runInBackground, "an unfocused instance must keep simulating");
            Assert.AreEqual(BuildScript.ProductName, PlayerSettings.productName);
        }

        [Test]
        public void SceneListStartsWithAnExistingSessionScene()
        {
            Assert.IsNotEmpty(BuildScenes.All);
            foreach (string scene in BuildScenes.All) FileAssert.Exists(scene);
            Assert.AreEqual(MallBuilder.ScenePath, BuildScenes.All[0], "first scene is what a build opens: the run");
            CollectionAssert.Contains(BuildScenes.All, TestBuildingBuilder.ScenePath, "nettests run in TestBuilding");
        }
    }
}
