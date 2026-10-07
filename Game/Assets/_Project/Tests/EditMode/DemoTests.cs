using System.IO;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>M8.3: the demo build's rules, save file and build flavour.</summary>
    public class DemoTests
    {
        [TearDown]
        public void TearDown() => Demo.Force(null);

        [Test]
        public void TheDemoEndsAfterItsJobs()
        {
            Demo.Force(true, 3);
            Assert.IsFalse(Demo.IsOver(2));
            Assert.IsTrue(Demo.IsOver(3));
            Demo.Force(false);
            Assert.IsFalse(Demo.IsOver(99), "the full game never ends");
        }

        [Test]
        public void TheDemoKeepsItsOwnSave()
        {
            Demo.Force(true);
            StringAssert.EndsWith(SaveStore.DemoFileName, new SaveStore("x").Path);
            Demo.Force(false);
            StringAssert.EndsWith(SaveStore.FileName, new SaveStore("x").Path);
        }

        [Test]
        public void TheDemoBuildIsAShareableStylePlayer()
        {
            Assert.AreEqual(Path.Combine("Builds", "MacDemo"), BuildScript.OutputFolder(BuildPlatform.Mac, BuildFlavor.Demo));
            Assert.IsFalse(BuildScript.OptionsOf(BuildFlavor.Demo).HasFlag(BuildOptions.Development));
            Assert.IsTrue(BuildScript.ShipsSteamAppId(BuildFlavor.Demo), "App ID 480 until the real one exists");
            Assert.IsNotNull(Resources.Load<DemoConfig>(DemoConfig.ResourcePath), "the demo's config ships in Resources");
        }
    }
}
