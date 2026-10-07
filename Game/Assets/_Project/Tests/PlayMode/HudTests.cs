using System.Collections;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Abandoned.Tests
{
    /// <summary>M8.1: the in-run HUD lives on the menu document, shows the run's numbers and hides behind menus.</summary>
    public class HudTests
    {
        [UnityTest]
        public IEnumerator TheRunHudShowsHaulVsQuotaAndHidesBehindTheMenu()
        {
            yield return TestMapScene.Load("Mall");
            yield return null;
            yield return null;
            Assert.IsNotNull(HudLayer.Root, "the session's menus host the HUD");
            RunHud run = Object.FindAnyObjectByType<RunHud>();
            Assert.IsNotNull(run);
            StringAssert.StartsWith("HAUL $0 / $", run.HaulText, "haul vs quota up top");
            Assert.AreEqual(DisplayStyle.Flex, HudLayer.Root.resolvedStyle.display);

            // Something to look at: the nearest loot, a couple of metres ahead.
            GameObject me = TestMapScene.Player;
            var loot = Object.FindAnyObjectByType<Networking.NetworkLoot>();
            loot.Grabbable.Body.position = me.transform.position + me.transform.forward * 1.4f + Vector3.up * 1.2f;
            loot.transform.position = loot.Grabbable.Body.position;
            loot.Grabbable.Body.isKinematic = true;
            me.GetComponent<Player.PlayerLook>().ApplyLook(Vector2.zero);
            for (int i = 0; i < 10; i++) yield return null;
            yield return MenuTests.Capture(MenuUi.Current, "M8_hud_run");

            MenuUi.Current.OpenPause();
            yield return null;
            Assert.AreEqual(DisplayStyle.None, HudLayer.Root.resolvedStyle.display, "no HUD over the pause menu");
            MenuUi.Current.Resume();
            yield return null;
            yield return null;
            Assert.AreEqual(DisplayStyle.Flex, HudLayer.Root.resolvedStyle.display, "back with the game");

            // Clip mode (M8.7, F10): just the game on screen.
            MenuUi.Current.ClipMode = true;
            yield return null;
            Assert.AreEqual(DisplayStyle.None, HudLayer.Root.resolvedStyle.display, "clip mode hides the HUD");
            MenuUi.Current.ClipMode = false;
            yield return null;
            Assert.AreEqual(DisplayStyle.Flex, HudLayer.Root.resolvedStyle.display, "and brings it back");
        }
    }
}
