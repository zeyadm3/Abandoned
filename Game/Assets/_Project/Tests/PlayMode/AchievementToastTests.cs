using System.Collections;
using Abandoned.Core;
using Abandoned.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>M9.5: an unlock pops a toast on the HUD; the menu lists every achievement.</summary>
    public class AchievementToastTests
    {
        [TearDown]
        public void TearDown() => Achievements.ResetAll();

        [UnityTest]
        public IEnumerator AnUnlockShowsAToastAndTheList()
        {
            Achievements.ResetAll();
            yield return TestBuildingScene.Load("HQ");
            var toast = Object.FindAnyObjectByType<AchievementToast>();
            Assert.IsNotNull(toast, "the session has the toast");
            Achievements.Increment(Achievements.StatDeaths);
            yield return null;
            yield return null;
            Assert.AreEqual("occupational_hazard", toast.Showing?.Id);
            yield return MenuTests.Capture(MenuUi.Current, "M9_achievement_toast");
            MenuUi.Current.OpenPause();
            MenuUi.Current.Push(MenuScreen.Achievements);
            yield return null;
            Assert.AreEqual(MenuScreen.Achievements, MenuUi.Current.Showing);
            yield return MenuTests.Capture(MenuUi.Current, "M9_achievements");
        }
    }
}
