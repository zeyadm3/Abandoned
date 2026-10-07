using System.Collections;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>M8.3: when the demo's jobs are done, the HQ shows the end screen and takes no more jobs.</summary>
    public class DemoEndTests
    {
        [TearDown]
        public void TearDown() => Demo.Force(null);

        [UnityTest]
        public IEnumerator TheHqEndsTheDemoAndTheHostCanStartOver()
        {
            Demo.Force(true, 0); // a demo whose jobs are all done
            yield return TestBuildingScene.Load("HQ");
            float until = Time.time + 5f;
            while (Time.time < until && (MenuUi.Current == null || MenuUi.Current.Showing != MenuScreen.DemoEnd)) yield return null;
            CompanyService company = CompanyService.Current;
            Assert.AreEqual(MenuScreen.DemoEnd, MenuUi.Current.Showing, "arriving at the HQ after the last job shows the end");
            Assert.IsTrue(company.DemoOver);
            yield return MenuTests.Capture(MenuUi.Current, "M8_demo_end");

            company.Select(0);
            yield return null;
            Assert.AreEqual(-1, company.Selected, "no more jobs in the demo");
            var board = Object.FindAnyObjectByType<ContractBoard>();
            StringAssert.Contains("demo is over", board.UsePrompt(TestBuildingScene.Player));

            MenuUi.Current.Back();
            yield return null;
            yield return null;
            Assert.AreEqual(MenuScreen.None, MenuUi.Current.Showing, "back to walking around the HQ");
            board.Use(TestBuildingScene.Player);
            yield return null;
            Assert.AreEqual(MenuScreen.DemoEnd, MenuUi.Current.Showing, "the board shows the end again");

            Demo.Force(true, 1); // starting over: a new company with all its jobs ahead
            company.RestartDemo();
            yield return null;
            Assert.IsFalse(company.DemoOver);
            Assert.AreEqual(0, company.State.Runs);
            company.Select(0);
            yield return null;
            Assert.AreEqual(0, company.Selected, "jobs again after starting over");
        }
    }
}
