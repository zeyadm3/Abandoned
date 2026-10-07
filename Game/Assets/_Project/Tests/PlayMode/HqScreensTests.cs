using System.Collections;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>M8.1b: the HQ's screens open on the HUD layer, take the mouse, and close with Esc-equivalents.</summary>
    public class HqScreensTests
    {
        [UnityTest]
        public IEnumerator BoardShopAndRackOpenAndClose()
        {
            yield return TestBuildingScene.Load("HQ");
            yield return new WaitUntil(() => CompanyService.Current != null && CompanyService.Current.IsSpawned);
            yield return null;
            HqHud hud = Object.FindAnyObjectByType<HqHud>();
            StringAssert.StartsWith("NO JOB YET", hud.JobText);
            yield return MenuTests.Capture(MenuUi.Current, "M8_hq_hud");

            ContractBoard.Open = true;
            yield return null;
            yield return null;
            Assert.IsTrue(CursorOwner.UiActive, "the board has the mouse");
            yield return MenuTests.Capture(MenuUi.Current, "M8_hq_board");
            CompanyService.Current.Select(1);
            yield return null;
            yield return null;
            StringAssert.StartsWith("JOB:", hud.JobText, "the HUD names the job");
            ContractBoard.Open = false;

            ShopTerminal.Open = true;
            yield return null;
            yield return null;
            yield return MenuTests.Capture(MenuUi.Current, "M8_hq_shop");
            ShopTerminal.Open = false;
            GearRack.Open = true;
            yield return null;
            yield return null;
            yield return MenuTests.Capture(MenuUi.Current, "M8_hq_rack");
            GearRack.Open = false;
            yield return null;
            yield return null;
            Assert.IsFalse(CursorOwner.UiActive, "all closed: the game has the mouse");
        }
    }
}
