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

        /// <summary>Step 3: toasts, the truck leaving (banner + the haul board from the ramp), the death camera and card.</summary>
        [UnityTest]
        public IEnumerator EventsAtEveryAspect()
        {
            yield return TestMapScene.Load("Mall");
            for (int i = 0; i < 20; i++) yield return null;
            GameObject me = TestMapScene.Player;
            var rig = PlayerTestRig.ForExisting(me);
            ToastFeed.Show("PLAYER 2 JOINED", "Another pair of hands.", "icon/multiplayer", ToastFeed.Kind.Good);
            ToastFeed.Show("ACHIEVEMENT UNLOCKED", "Occupational Hazard: die on the job.", "icon/trophy", ToastFeed.Kind.Good);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(2, Object.FindAnyObjectByType<ToastFeed>().Titles.Count(), "two toasts up");

            // Start the truck from the lever, then stand on the ramp looking at the board.
            var truck = Extraction.TruckCargo.Current;
            rig.Teleport(truck.Ignition.position - truck.transform.right * 0.8f + Vector3.down * 1.0f, truck.transform.eulerAngles.y);
            Extraction.RunState.Current.RequestDepart();
            yield return null;
            Assert.AreEqual(Extraction.RunPhase.Honking, Extraction.RunState.Current.State.Phase, "the truck is leaving");
            // On the ramp, looking into the bay at the board on its front wall.
            rig.Teleport(truck.transform.position - truck.transform.forward * 3f + Vector3.up * 0.2f, truck.transform.eulerAngles.y);
            me.GetComponent<Player.PlayerLook>().SyncYawFromTransform();
            me.GetComponent<Player.PlayerLook>().ApplyLook(Vector2.zero);
            for (int i = 0; i < 10; i++) yield return null;
            yield return Shots("truck");

            // Die: the camera circles the body, then the card.
            me.GetComponent<NetworkPlayer>().ServerKill("The Hunter ran you down.");
            yield return new WaitForSeconds(1.2f);
            yield return Shots("death_cam");
            var ghost = me.GetComponent<GhostSpectator>();
            float until = Time.time + 4f;
            while (!ghost.ShowingDeathCard && Time.time < until) yield return null;
            Assert.IsTrue(ghost.ShowingDeathCard, "YOU DIED after the death camera");
            yield return Shots("death_card");
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