using Abandoned.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Abandoned.Tests
{
    public class VersionInfoTests
    {
        [Test]
        public void VersionComesFromPlayerSettingsAndTheEditorIsUnstamped()
        {
            Assert.AreEqual(PlayerSettings.bundleVersion, VersionInfo.Version);
            Assert.IsNotNull(VersionInfo.Build, "BuildInfo must load from Resources (Tools/unity.sh rebuild)");
            Assert.AreEqual(BuildInfo.EditorCommit, VersionInfo.Commit, "the committed asset stays unstamped");
            StringAssert.Contains(Application.version, VersionInfo.Display);
        }

        [Test]
        public void BuildsMatchOnVersionAndCommitTheEditorOnVersionOnly()
        {
            string a = VersionInfo.FormatKey("0.3.0", "abc1234");
            Assert.IsTrue(VersionInfo.AreCompatible(a, VersionInfo.FormatKey("0.3.0", "abc1234")));
            Assert.IsFalse(VersionInfo.AreCompatible(a, VersionInfo.FormatKey("0.3.0", "def5678")), "different commits desync silently");
            Assert.IsFalse(VersionInfo.AreCompatible(a, VersionInfo.FormatKey("0.4.0", "abc1234")));
            Assert.IsTrue(VersionInfo.AreCompatible(a, VersionInfo.FormatKey("0.3.0", BuildInfo.EditorCommit)), "editor vs build testing");
            Assert.IsFalse(VersionInfo.AreCompatible(a, VersionInfo.FormatKey("0.2.0", BuildInfo.EditorCommit)));
            Assert.IsFalse(VersionInfo.AreCompatible(a, null));
        }
    }
}
