using System.Collections;
using System.Linq;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Abandoned.Tests
{
    /// <summary>Real loot prefabs falling, hitting and shattering on flat ground.</summary>
    public class LootDamageTests
    {
        private PlayerTestRig rig;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // A level left loaded by an earlier test would be in the way.
            yield return NetTestHarness.CleanWorld();
            rig = PlayerTestRig.OnFlatGround(new Vector3(0f, 0.05f, -20f));
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GameAuthority.SetHostCheck(null);
            foreach (LootItem item in Object.FindObjectsByType<LootItem>(FindObjectsSortMode.None))
                Object.DestroyImmediate(item.gameObject);
            rig.Destroy();
            yield return null;
        }

        public static LootItem SpawnLoot(string id, Vector3 position, int seed = 7)
        {
            GameObject prefab = null;
#if UNITY_EDITOR
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Prefabs/Loot/Loot_{id}.prefab");
#endif
            Assert.IsNotNull(prefab, id);
            LootItem item = Object.Instantiate(prefab, position, Quaternion.identity).GetComponent<LootItem>();
            item.Initialize(seed);
            Physics.SyncTransforms();
            return item;
        }

        private static IEnumerator WaitFixed(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
        }

        [UnityTest]
        public IEnumerator VaseDroppedFromHandHeightShattersToZero()
        {
            LootItem vase = SpawnLoot("antique_vase", new Vector3(0f, 1.5f, 0f));
            int full = vase.FullValue;
            Assert.Greater(full, 0);
            bool shattered = false;
            vase.Shattered += (_, _) => shattered = true;
            int soundsBefore = GameAudio.ImpactCount;

            yield return WaitFixed(1.5f);

            Assert.IsTrue(shattered, "vase should shatter");
            Assert.IsTrue(vase == null, "shattered item is removed");
            Assert.Greater(GameAudio.ImpactCount, soundsBefore);
            Assert.AreEqual(SurfaceMaterial.Glass, GameAudio.LastImpactMaterial);
            Assert.Greater(FloatingTextOverlay.Instance.ActiveCount, 0, "shows $X → $0");
            int debris = GameLayers.DebrisLayer;
            Assert.IsTrue(Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Any(b => b.gameObject.layer == debris),
                "cosmetic shards on the Debris layer");
        }

        [UnityTest]
        public IEnumerator LaptopDroppedFromHeightLosesSomeValue()
        {
            LootItem laptop = SpawnLoot("laptop", new Vector3(0f, 3f, 0f));
            int full = laptop.FullValue;
            int lost = 0;
            laptop.Damaged += (_, loss, _) => lost += loss;
            yield return WaitFixed(2f);
            Assert.Greater(lost, 0, "a 3 m drop should cost something");
            Assert.Less(laptop.CurrentValue, full);
            Assert.Greater(laptop.CurrentValue, 0, "Medium fragility doesn't zero out from one drop");
            Assert.AreEqual(full - lost, laptop.CurrentValue);
        }

        [UnityTest]
        public IEnumerator GentlePlacementCostsNothing()
        {
            LootItem laptop = SpawnLoot("laptop", new Vector3(0f, 0.05f, 0f));
            int full = laptop.FullValue;
            yield return WaitFixed(1f);
            Assert.AreEqual(full, laptop.CurrentValue);
        }

        [UnityTest]
        public IEnumerator CashIsNeverDamaged()
        {
            rig.AddBox("Wall", new Vector3(0f, 1f, 3f), new Vector3(4f, 2f, 0.3f));
            LootItem cash = SpawnLoot("cash_bundle", new Vector3(0f, 1f, 0f));
            int full = cash.FullValue;
            cash.GetComponent<Rigidbody>().linearVelocity = Vector3.forward * 20f;
            yield return WaitFixed(1.5f);
            Assert.AreEqual(full, cash.CurrentValue);
        }

        [UnityTest]
        public IEnumerator ClientsDoNotApplyDamage()
        {
            LootItem laptop = SpawnLoot("laptop", new Vector3(0f, 3f, 0f));
            int full = laptop.FullValue;
            GameAuthority.SetHostCheck(() => false);
            yield return WaitFixed(2f);
            Assert.AreEqual(full, laptop.CurrentValue, "only the host changes value");
        }

        [UnityTest]
        public IEnumerator OneLandingCountsOnceWithinTheCooldown()
        {
            LootItem tv = SpawnLoot("flatscreen_tv", new Vector3(0f, 0.4f, 0f));
            yield return WaitFixed(0.5f);
            int before = tv.CurrentValue;
            tv.ApplyImpact(6f, tv.transform.position);
            int afterFirst = tv.CurrentValue;
            tv.ApplyImpact(6f, tv.transform.position);
            Assert.Less(afterFirst, before);
            Assert.AreEqual(afterFirst, tv.CurrentValue, "second contact inside the cooldown is ignored");
        }

        [UnityTest]
        public IEnumerator ShatteringWhileHeldEmptiesTheHands()
        {
            rig.Teleport(new Vector3(0f, 0.05f, 0f));
            rig.Settle();
            var carrier = rig.Player.GetComponent<PlayerCarrier>();
            LootItem vase = SpawnLoot("antique_vase", carrier.HoldPoint);
            yield return null;
            InteractionService.Handler.RequestPickup(carrier, vase.GetComponent<Grabbable>());
            Assert.IsNotNull(carrier.Held);
            vase.ApplyImpact(10f, vase.transform.position);
            Assert.IsNull(carrier.Held);
            yield return WaitFixed(0.2f);
        }

        [UnityTest]
        public IEnumerator PocketedCashCountsInTheInventoryTotal()
        {
            rig.Teleport(new Vector3(0f, 0.05f, 0f));
            rig.Settle();
            var carrier = rig.Player.GetComponent<PlayerCarrier>();
            LootItem cash = SpawnLoot("cash_bundle", new Vector3(0f, 0.05f, 1.2f));
            yield return WaitFixed(0.3f);
            InteractionService.Handler.RequestPickup(carrier, cash.GetComponent<Grabbable>());
            Assert.AreEqual(1, carrier.Inventory.Count);
            Assert.IsTrue(carrier.Inventory.Items[0].TryGetComponent(out IValuable v));
            Assert.AreEqual(cash.CurrentValue, v.CurrentValue);
        }
    }
}
