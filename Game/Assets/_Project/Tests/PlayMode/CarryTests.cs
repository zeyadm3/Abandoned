using System.Collections;
using Abandoned.Interaction;
using Abandoned.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>Pickup, hold, throw, drop and pockets with real physics and the real Player prefab.</summary>
    public class CarryTests
    {
        private PlayerTestRig rig;
        private PlayerCarrier carrier;
        private PlayerInteractor interactor;
        private CarryConfig config;
        private readonly System.Collections.Generic.List<GameObject> items = new();

        // A table in front of the player so items sit at eye height (eye is at y = 1.65), placed
        // beyond the hold point (z ≈ 1.1) so a held item doesn't rest against it.
        private static readonly Vector3 OnTable = new(0f, 1.6f, 2f);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            rig = PlayerTestRig.OnFlatGround(new Vector3(0f, 0.05f, 0f));
            rig.AddBox("Table", new Vector3(0f, 0.7f, 2.2f), new Vector3(2f, 1.4f, 1f));
            carrier = rig.Player.GetComponent<PlayerCarrier>();
            interactor = rig.Player.GetComponent<PlayerInteractor>();
            interactor.enabled = false; // driven manually through Tick
            config = carrier.Config;
            yield return null;
            rig.Settle();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (GameObject go in items) if (go != null) Object.DestroyImmediate(go);
            items.Clear();
            rig.Destroy();
            yield return null;
        }

        private Grabbable Spawn(Vector3 position, CarryClass carryClass, float weight, float size = 0.4f)
        {
            Grabbable g = TestCarryable.Create(position, carryClass, weight, size);
            items.Add(g.gameObject);
            return g;
        }

        private static PlayerInputFrame Input(bool interact = false, bool useHeld = false, bool drop = false, bool inventory = false) =>
            new(Vector2.zero, Vector2.zero, false, false, false, false, false, useHeld, interact, drop, inventory, false);

        private static IEnumerator Steps(int fixedSteps)
        {
            for (int i = 0; i < fixedSteps; i++) yield return new WaitForFixedUpdate();
        }

        [UnityTest]
        public IEnumerator PicksUpAnItemInReachWithE()
        {
            Grabbable item = Spawn(OnTable, CarryClass.OneHand, 2f);
            yield return Steps(2);
            interactor.Tick(Input(), PlayerTestRig.Dt);
            Assert.AreEqual(item, interactor.Target, "should be aiming at the item");
            interactor.Tick(Input(interact: true), PlayerTestRig.Dt);
            Assert.AreEqual(item, carrier.Held);
            Assert.IsFalse(item.Body.useGravity);
        }

        [UnityTest]
        public IEnumerator CannotPickUpBeyondReach()
        {
            Grabbable item = Spawn(new Vector3(0f, 0.3f, 6f), CarryClass.OneHand, 2f);
            yield return Steps(2);
            InteractionService.Handler.RequestPickup(carrier, item);
            Assert.IsNull(carrier.Held);
            Assert.AreEqual("Too far away", carrier.Hint);
        }

        [UnityTest]
        public IEnumerator HeavyItemsAreDraggedAndHugeItemsCannotBeMovedAlone()
        {
            Grabbable heavy = Spawn(OnTable + Vector3.left * 0.5f, CarryClass.Heavy, 200f);
            Grabbable huge = Spawn(OnTable + Vector3.right * 0.5f, CarryClass.Huge, 2000f);
            yield return Steps(2);
            InteractionService.Handler.RequestPickup(carrier, heavy);
            Assert.AreEqual(heavy, carrier.Held, "solo drag stands in for the hand trolley");
            Assert.IsTrue(carrier.IsDragging);
            Assert.IsTrue(heavy.Body.useGravity, "dragged items stay on the floor");
            Assert.AreEqual(0f, carrier.CarriedWeight, 1e-3f, "a dragged item rests its own weight");
            InteractionService.Handler.RequestDrop(carrier);
            InteractionService.Handler.RequestPickup(carrier, huge);
            Assert.IsNull(carrier.Held);
            StringAssert.Contains("3–4 people", carrier.Hint);
        }

        [UnityTest]
        public IEnumerator PocketItemsGoToInventoryUntilFull()
        {
            var pocketItems = new Grabbable[config.PocketSlots + 1];
            for (int i = 0; i < pocketItems.Length; i++)
                pocketItems[i] = Spawn(OnTable + Vector3.right * (i * 0.15f - 0.4f), CarryClass.Pocket, 0.2f, 0.1f);
            yield return Steps(2);

            for (int i = 0; i < config.PocketSlots; i++)
            {
                InteractionService.Handler.RequestPickup(carrier, pocketItems[i]);
                Assert.IsTrue(pocketItems[i].IsPocketed);
                Assert.IsFalse(pocketItems[i].GetComponent<Renderer>().enabled, "pocketed items are hidden");
            }
            Assert.AreEqual(config.PocketSlots, carrier.Inventory.Count);
            Assert.IsNull(carrier.Held, "pocketing doesn't use the hands");

            InteractionService.Handler.RequestPickup(carrier, pocketItems[^1]);
            Assert.IsFalse(pocketItems[^1].IsPocketed);
            Assert.AreEqual("Pockets full", carrier.Hint);
        }

        [UnityTest]
        public IEnumerator DropFromPocketPutsTheItemBackInTheWorld()
        {
            Grabbable item = Spawn(OnTable, CarryClass.Pocket, 0.2f, 0.1f);
            yield return Steps(2);
            InteractionService.Handler.RequestPickup(carrier, item);
            interactor.Tick(Input(drop: true, inventory: true), PlayerTestRig.Dt);
            Assert.AreEqual(0, carrier.Inventory.Count);
            Assert.IsFalse(item.IsPocketed);
            Assert.IsTrue(item.GetComponent<Renderer>().enabled);
            yield return Steps(60);
            Assert.Less(item.transform.position.y, 1.9f, "should fall under gravity");
        }

        [UnityTest]
        public IEnumerator HeldItemSettlesAtTheHoldPoint()
        {
            Grabbable item = Spawn(OnTable, CarryClass.OneHand, 2f);
            yield return Steps(2);
            InteractionService.Handler.RequestPickup(carrier, item);
            yield return Steps(50);
            float error = Vector3.Distance(carrier.HoldPoint, item.Body.worldCenterOfMass);
            Assert.Less(error, 0.15f);
            Abandoned.Core.ScreenshotCapture.CaptureFrom(carrier.EyePosition,
                carrier.EyePosition + carrier.EyeForward * 5f, "M1_2_holding_item", 75f);
        }

        [UnityTest]
        public IEnumerator CarriedWeightSlowsThePlayer()
        {
            Grabbable item = Spawn(OnTable, CarryClass.TwoHand, 30f, 0.5f);
            yield return Steps(2);
            InteractionService.Handler.RequestPickup(carrier, item);
            yield return null; // PlayerCarrier.Update applies the multipliers
            Assert.AreEqual(config.SpeedMultiplierFor(30f), rig.Motor.SpeedMultiplier, 0.001f);
            Assert.Less(rig.Motor.SpeedMultiplier, 1f);
            Assert.Greater(rig.Motor.StaminaDrainMultiplier, 1f);

            InteractionService.Handler.RequestDrop(carrier);
            yield return null;
            Assert.AreEqual(1f, rig.Motor.SpeedMultiplier, 0.001f);
        }

        [UnityTest]
        public IEnumerator DropReleasesWithGravity()
        {
            Grabbable item = Spawn(OnTable, CarryClass.OneHand, 2f);
            yield return Steps(2);
            InteractionService.Handler.RequestPickup(carrier, item);
            yield return Steps(10);
            interactor.Tick(Input(drop: true), PlayerTestRig.Dt);
            Assert.IsNull(carrier.Held);
            Assert.IsTrue(item.Body.useGravity);
            Assert.IsTrue(item.IsAvailable);
        }

        [UnityTest]
        public IEnumerator FullChargeThrowsFasterThanATap()
        {
            Grabbable item = Spawn(OnTable, CarryClass.OneHand, 1f);
            yield return Steps(2);
            InteractionService.Handler.RequestPickup(carrier, item);
            interactor.Tick(Input(useHeld: true), PlayerTestRig.Dt);
            interactor.Tick(Input(), PlayerTestRig.Dt);
            float tapSpeed = item.Body.linearVelocity.magnitude;
            Assert.IsNull(carrier.Held);

            yield return Steps(60);
            rig.Teleport(new Vector3(0f, 0.05f, 0f));
            item.Body.linearVelocity = Vector3.zero;
            item.transform.position = OnTable;
            Physics.SyncTransforms();
            yield return Steps(2);
            InteractionService.Handler.RequestPickup(carrier, item);
            for (float t = 0f; t < config.ThrowChargeTime + 0.1f; t += PlayerTestRig.Dt)
                interactor.Tick(Input(useHeld: true), PlayerTestRig.Dt);
            Assert.AreEqual(1f, interactor.Charge, 0.001f);
            interactor.Tick(Input(), PlayerTestRig.Dt);
            float fullSpeed = item.Body.linearVelocity.magnitude;

            Assert.AreEqual(config.ThrowMaxSpeed, fullSpeed, 0.5f);
            Assert.Less(tapSpeed, config.ThrowMinSpeed + 1f);
        }

        [Test]
        public void HeavyThingsAreThrownSlower()
        {
            float light = config.ThrowSpeedFor(1f, 1f);
            float heavy = config.ThrowSpeedFor(1f, 20f);
            Assert.AreEqual(config.ThrowMaxSpeed, light, 0.001f);
            Assert.AreEqual(config.ThrowMaxSpeed * Mathf.Sqrt(config.ThrowReferenceWeight / 20f), heavy, 0.001f);
        }

        [UnityTest]
        public IEnumerator HostClampsAbsurdThrowRequests()
        {
            Grabbable item = Spawn(OnTable, CarryClass.OneHand, 1f);
            yield return Steps(2);
            InteractionService.Handler.RequestPickup(carrier, item);
            InteractionService.Handler.RequestThrow(carrier, Vector3.forward * 1000f);
            Assert.LessOrEqual(item.Body.linearVelocity.magnitude, config.ThrowMaxSpeed + 0.01f);
        }

        [UnityTest]
        public IEnumerator SnaggedItemIsDroppedInsteadOfDraggingThePlayer()
        {
            Grabbable item = Spawn(OnTable, CarryClass.OneHand, 2f);
            yield return Steps(2);
            InteractionService.Handler.RequestPickup(carrier, item);
            // Hold the item in place behind a wall by pinning it, then move the player away.
            item.Body.isKinematic = true;
            rig.Teleport(new Vector3(0f, 0.05f, -6f));
            yield return Steps(Mathf.CeilToInt((config.BreakGraceTime + 0.1f) / Time.fixedDeltaTime));
            Assert.IsNull(carrier.Held);
            item.Body.isKinematic = false;
        }

        [UnityTest]
        public IEnumerator HandsFullBlocksASecondPickup()
        {
            Grabbable first = Spawn(OnTable + Vector3.left * 0.4f, CarryClass.OneHand, 2f);
            Grabbable second = Spawn(OnTable + Vector3.right * 0.4f, CarryClass.OneHand, 2f);
            yield return Steps(2);
            InteractionService.Handler.RequestPickup(carrier, first);
            InteractionService.Handler.RequestPickup(carrier, second);
            Assert.AreEqual(first, carrier.Held);
            Assert.AreEqual("Hands full", carrier.Hint);
        }
    }
}
