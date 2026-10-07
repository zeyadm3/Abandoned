using System.Collections;
using Abandoned.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>M8.2: first-time tips show at the right moment, once, and can be switched off.</summary>
    public class HintTests
    {
        [SetUp]
        public void SetUp()
        {
            Hints.ResetSeen();
            Hints.Enabled = true;
        }

        [TearDown]
        public void TearDown()
        {
            Hints.ResetSeen();
            Hints.Enabled = true;
        }

        [UnityTest]
        public IEnumerator TheHqSaysReadTheBoardFirst()
        {
            yield return TestMapScene.Load("HQ");
            yield return WaitForTip(HintId.HqBoard, 3f);
            Assert.AreEqual(HintId.HqBoard, HintDirector.Current.Showing);
            Assert.IsTrue(Hints.Seen(HintId.HqBoard));
            Assert.IsFalse(HintDirector.Current.Show(HintId.HqBoard), "a seen tip never shows again");
            yield return MenuTests.Capture(MenuUi.Current, "M8_tip_hq");
        }

        [UnityTest]
        public IEnumerator ARunExplainsTheGoal()
        {
            yield return TestMapScene.Load("Mall");
            yield return WaitForTip(HintId.RunGoal, 6f);
            Assert.AreEqual(HintId.RunGoal, HintDirector.Current.Showing);
            StringAssert.Contains("truck", Hints.Text(HintId.RunGoal));
        }

        [UnityTest]
        public IEnumerator TipsCanBeSwitchedOff()
        {
            Hints.Enabled = false;
            yield return TestMapScene.Load("HQ");
            yield return new WaitForSeconds(1.5f);
            Assert.IsNull(HintDirector.Current.Showing);
            Assert.IsFalse(Hints.Seen(HintId.HqBoard), "nothing was used up while tips were off");
        }

        private static IEnumerator WaitForTip(HintId id, float timeout)
        {
            float until = Time.time + timeout;
            while (Time.time < until && (HintDirector.Current == null || HintDirector.Current.Showing != id)) yield return null;
        }
    }
}
