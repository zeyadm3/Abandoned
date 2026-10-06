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
    
        [Test]
        public void DirtyOrUnknownBuildsOnlyMatchTheSameBuildRun()
        {
            const string t1 = "2026-10-06T10:00:00Z", t2 = "2026-10-06T11:00:00Z";
            string clean = VersionInfo.FormatKey("0.3.0", "abc1234", t1);
            string dirty1 = VersionInfo.FormatKey("0.3.0", "abc1234-dirty", t1);
            string dirty1Twin = VersionInfo.FormatKey("0.3.0", "abc1234-dirty", t1);
            string dirty2 = VersionInfo.FormatKey("0.3.0", "abc1234-dirty", t2);
            string unknown1 = VersionInfo.FormatKey("0.3.0", "unknown", t1);
            string unknown2 = VersionInfo.FormatKey("0.3.0", "unknown", t2);

            Assert.AreEqual("0.3.0+abc1234", clean, "exact commits ignore the build time");
            Assert.IsTrue(VersionInfo.AreCompatible(clean, VersionInfo.FormatKey("0.3.0", "abc1234", t2)));
            Assert.IsFalse(VersionInfo.AreCompatible(clean, dirty1), "edits on top of abc1234 are not abc1234");
            Assert.IsTrue(VersionInfo.AreCompatible(dirty1, dirty1Twin), "Mac + Windows from one build run");
            Assert.IsFalse(VersionInfo.AreCompatible(dirty1, dirty2), "two dirty builds may hold different edits");
            Assert.IsFalse(VersionInfo.AreCompatible(unknown1, unknown2));
            Assert.IsTrue(VersionInfo.AreCompatible(unknown1, unknown1));
            Assert.IsFalse(VersionInfo.AreCompatible(VersionInfo.FormatKey("0.3.0", "abc1234-dirty", ""),
                                                     VersionInfo.FormatKey("0.3.0", "abc1234-dirty", "")),
                           "no build time means nothing proves they match");
            Assert.IsTrue(VersionInfo.AreCompatible(dirty1, VersionInfo.FormatKey("0.3.0", BuildInfo.EditorCommit)), "editor vs build testing");
            Assert.IsFalse(VersionInfo.AreCompatible(dirty1, VersionInfo.FormatKey("0.3.1", "abc1234-dirty", t1)));
        }
    }
}
