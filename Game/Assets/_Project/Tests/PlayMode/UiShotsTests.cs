using System.Collections;
using System.Linq;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>
    /// UI overhaul: renders each screen at 16:9, 16:10 and ultrawide into Game/Screenshots (UI_*) so the
    /// layout can be checked by eye. Asserts only that the screens exist; the pictures are the point.
    /// </summary>
    public class UiShotsTests
    {
        private static readonly (string tag, int w, int h)[] Sizes = { ("169", 1600, 900), ("1610", 1440, 900), ("uw", 2560, 1080) };

        internal static IEnumerator Shots(string name)
        {
            foreach ((string tag, int w, int h) in Sizes) yield return MenuTests.Capture(MenuUi.Current, $"UI_{name}_{tag}", w, h);
        }

        [UnityTest]
        public IEnumerator MenusAtEveryAspect()
        {
            yield return TestMapScene.Load("HQ");
            MenuUi menu = MenuUi.Current;
            Assert.IsNotNull(menu);
            menu.OpenPause();
            yield return null;
            yield return Shots("pause");
            menu.Push(MenuScreen.Settings);
            yield return null;
            yield return Shots("settings");
            menu.Back();
            menu.Back();
            yield return null;
            menu.Bootstrap.Disconnect();
            float until = Time.time + 8f;
            while (Time.time < until && (MenuUi.Current == null || MenuUi.Current.Showing != MenuScreen.Main)) yield return null;
            Assert.AreEqual(MenuScreen.Main, MenuUi.Current.Showing);
            yield return Shots("main");
        }

        /// <summary>Step 2: the run HUD, pockets, hand slots, a value tag on what you look at, the loot scan, holding.</summary>
        [UnityTest]
        public IEnumerator RunHudAtEveryAspect()
        {
            yield return TestMapScene.Load("Mall");
            for (int i = 0; i < 10; i++) yield return null;
            GameObject me = TestMapScene.Player;
            me.GetComponent<Player.PlayerLook>().ApplyLook(Vector2.zero);
            yield return null;
            var carrier = me.GetComponent<PlayerCarrier>();
            NetworkLoot[] loot = Object.FindObjectsByType<NetworkLoot>(FindObjectsSortMode.None);
            foreach (NetworkLoot l in loot.Where(l => l.Grabbable.CarryClass == CarryClass.Pocket).Take(2))
            {
                Place(l, me.transform.position + me.transform.forward * 0.7f + Vector3.up * 1.2f);
                InteractionService.Handler.RequestPickup(carrier, l.Grabbable);
            }
            for (int i = 0; i < 20; i++) yield return null; // settled on the ground
            NetworkLoot look = loot.First(l => l.Grabbable.CarryClass == CarryClass.TwoHand && !l.Grabbable.IsPocketed);
            Place(look, carrier.EyePosition + carrier.EyeForward * 1.4f - Vector3.up * 0.15f);
            for (int i = 0; i < 6; i++) yield return null;
            var tags = me.GetComponent<LootTags>();
            tags.Scan();
            yield return new WaitForSeconds(0.8f);
            Assert.IsNotNull(tags.Focus, $"a value tag on what you look at (target {me.GetComponent<PlayerInteractor>().Target})");
            Assert.Greater(tags.ScanCount, 0, "the scan tags loot around you");
            Assert.AreEqual(2, carrier.Inventory.Count);
            yield return Shots("hud");

            InteractionService.Handler.RequestPickup(carrier, look.Grabbable);
            for (int i = 0; i < 10; i++) yield return null;
            yield return Shots("hud_holding");
        }

        private static void Place(NetworkLoot l, Vector3 at)
        {
            l.Grabbable.Body.isKinematic = true;
            l.Grabbable.Body.position = at;
            l.transform.position = at;
            Physics.SyncTransforms();
        }
    }
}