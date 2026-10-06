using Abandoned.EditorTools;
using NUnit.Framework;
using UnityEditor;

namespace Abandoned.Tests
{
    public class SteamPluginSettingsTests
    {
        [Test]
        public void SteamBinariesHaveExactlyThePlannedPlatforms()
        {
            CollectionAssert.IsEmpty(SteamPluginSettings.Problems());
        }

        [Test]
        public void MacEditorUsesPosixAndNeverWin64()
        {
            var posix = (PluginImporter)AssetImporter.GetAtPath(SteamPluginSettings.Folder + "/Facepunch.Steamworks.Posix.dll");
            var win64 = (PluginImporter)AssetImporter.GetAtPath(SteamPluginSettings.Folder + "/Facepunch.Steamworks.Win64.dll");
            Assert.AreEqual("OSX", posix.GetEditorData("OS"));
            Assert.AreEqual("Windows", win64.GetEditorData("OS"));
            Assert.IsTrue(posix.GetCompatibleWithPlatform(BuildTarget.StandaloneOSX));
            Assert.IsFalse(posix.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64));
            Assert.IsTrue(win64.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64));
            Assert.IsFalse(win64.GetCompatibleWithPlatform(BuildTarget.StandaloneOSX));
        }
    }
}
