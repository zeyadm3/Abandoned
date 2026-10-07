using System.Collections;
using System.Linq;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>
    /// The 10 placed loot items in TestMap spawn cleanly and settle without damage, and in the
    /// solo session (the player hosts) they are spawned network objects simulated and valued here.
    /// </summary>
    public class TestMapLootTests
    {
        [UnityTest]
        public IEnumerator PlacedLootSettlesWithoutFallingOrBreaking()
        {
            yield return TestMapScene.Load();
            LootItem[] items = Object.FindObjectsByType<LootItem>(FindObjectsSortMode.None);
            Assert.AreEqual(13, items.Length, "10 M1 items + 3 M2 structure test pieces");
            Assert.GreaterOrEqual(items.Count(i => i.transform.position.y > 3.5f), 3, "some loot upstairs");
            Assert.IsTrue(items.Any(i => i.Definition.Fragility == Fragility.Extreme), "fragile items present");
            Assert.IsTrue(items.Any(i => i.Definition.CarryClass >= CarryClass.Heavy), "heavy items present");

            for (float t = 0f; t < 3f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

            items = Object.FindObjectsByType<LootItem>(FindObjectsSortMode.None);
            Assert.AreEqual(13, items.Length, "something shattered while settling");
            foreach (LootItem item in items)
            {
                Assert.Greater(item.transform.position.y, -0.5f, $"{item.name} fell through the floor");
                Assert.AreEqual(item.FullValue, item.CurrentValue, $"{item.name} was damaged by spawning");
            }

            ScreenshotCapture.CaptureFrom(new Vector3(10f, 16f, -2f), new Vector3(10f, 0f, 9f), "M1_3_loot_overview");
            ScreenshotCapture.CaptureFrom(new Vector3(9f, 6f, 6f), new Vector3(10f, 4f, 13f), "M1_3_loot_upstairs");
        }

        [UnityTest]
        public IEnumerator SoloHostSpawnsPlacedLootAsHostOwnedNetworkObjects()
        {
            yield return TestMapScene.Load();
            NetworkLoot[] loot = Object.FindObjectsByType<NetworkLoot>(FindObjectsSortMode.None);
            Assert.AreEqual(13, loot.Length);
            Assert.IsInstanceOf<NetworkInteractionHandler>(InteractionService.Handler);
            foreach (NetworkLoot l in loot)
            {
                Assert.IsTrue(l.IsSpawned && l.IsServer && l.IsOwner, $"{l.name}: spawned by the solo host and owned by it");
                Assert.IsTrue(l.NetworkObject.InScenePlaced, $"{l.name}: scene-placed (clients respawn it from its prefab)");
                Assert.IsTrue(l.Grabbable.HasPhysicsAuthority && !l.Grabbable.Body.isKinematic, $"{l.name}: simulated here");
                Assert.IsTrue(l.Item.HasValueAuthority && l.Item.FullValue > 0, $"{l.name}: valued here");
                Assert.AreEqual(l.Item.FullValue, l.Value.FullValue, $"{l.name}: replicated value matches");
            }
            Assert.AreEqual(loot.Length, loot.Select(l => l.Item.Seed).Distinct().Count(), "each placed item keeps its own seed");
        }
    }
}
