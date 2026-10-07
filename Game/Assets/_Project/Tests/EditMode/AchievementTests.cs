using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>M9.5: stats unlock achievements once, and unlocks reach Steam when it runs.</summary>
    public class AchievementTests
    {
        [SetUp]
        public void SetUp() => Achievements.ResetAll();

        [TearDown]
        public void TearDown()
        {
            Achievements.ResetAll();
            Achievements.Backend = null;
        }

        private static AchievementDefinition Find(string id)
        {
            foreach (AchievementDefinition a in Achievements.Catalog.Items) if (a.Id == id) return a;
            return null;
        }

        [Test]
        public void AStatUnlocksAtItsThresholdOnce()
        {
            Assert.IsNotNull(Achievements.Catalog, "the catalog ships in Resources (run Rebuild Content)");
            var unlocked = new List<string>();
            void Note(AchievementDefinition a) => unlocked.Add(a.Id);
            Achievements.Unlocked += Note;
            Achievements.Increment(Achievements.StatEscapes);
            Achievements.Increment(Achievements.StatEscapes);
            Achievements.Unlocked -= Note;
            CollectionAssert.AreEqual(new[] { "made_it_out" }, unlocked, "unlocked once, not again");
            Assert.IsFalse(Achievements.IsUnlocked(Find("career_salvager")), "25 escapes is still far off");
            Assert.AreEqual(2, Achievements.Stat(Achievements.StatEscapes));
        }

        [Test]
        public void ARunRecordsEveryStatItTouches()
        {
            Achievements.RecordRun(escaped: true, crewHaul: 120000, quotaMet: true, jackpotsOut: 1, dark: true);
            foreach (string id in new[] { "first_job", "made_it_out", "six_figures", "the_greed_item", "lights_out" })
                Assert.IsTrue(Achievements.IsUnlocked(Find(id)), id);
            Achievements.RecordRun(escaped: false, crewHaul: 0, quotaMet: false, jackpotsOut: 2, dark: true);
            Assert.AreEqual(1, Achievements.Stat(Achievements.StatJackpots), "a jackpot you didn't get out with doesn't count");
        }

        [Test]
        public void UnlocksReachSteamWhenItRuns()
        {
            var config = ScriptableObject.CreateInstance<NetworkConfig>();
            var fake = new FakeSteamClient();
            SteamBootstrap bootstrap = SteamBootstrap.Create(config, fake);
            try
            {
                Achievements.Increment(Achievements.StatRuns); // earned while Steam was off
                Assert.IsTrue(bootstrap.TryInitialize(false, new string[0]));
                CollectionAssert.Contains(fake.AchievementsSet, "ACH_FIRST_JOB", "offline unlocks catch up when Steam starts");
                Achievements.Increment(Achievements.StatDeaths);
                CollectionAssert.Contains(fake.AchievementsSet, "ACH_OCCUPATIONAL_HAZARD");
            }
            finally
            {
                Object.DestroyImmediate(bootstrap.gameObject);
                Object.DestroyImmediate(config);
            }
        }
    }
}
