using System.Collections;
using System.Linq;
using Abandoned.Core;
using Abandoned.Loot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>The 10 placed loot items in TestBuilding spawn cleanly and settle without damage.</summary>
    public class TestBuildingLootTests
    {
        [UnityTest]
        public IEnumerator PlacedLootSettlesWithoutFallingOrBreaking()
        {
            yield return SceneManager.LoadSceneAsync("TestBuilding", LoadSceneMode.Single);
            LootItem[] items = Object.FindObjectsByType<LootItem>(FindObjectsSortMode.None);
            Assert.AreEqual(10, items.Length);
            Assert.GreaterOrEqual(items.Count(i => i.transform.position.y > 3.5f), 3, "some loot upstairs");
            Assert.IsTrue(items.Any(i => i.Definition.Fragility == Fragility.Extreme), "fragile items present");
            Assert.IsTrue(items.Any(i => i.Definition.CarryClass >= Abandoned.Interaction.CarryClass.Heavy), "heavy items present");

            for (float t = 0f; t < 3f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();

            items = Object.FindObjectsByType<LootItem>(FindObjectsSortMode.None);
            Assert.AreEqual(10, items.Length, "something shattered while settling");
            foreach (LootItem item in items)
            {
                Assert.Greater(item.transform.position.y, -0.5f, $"{item.name} fell through the floor");
                Assert.AreEqual(item.FullValue, item.CurrentValue, $"{item.name} was damaged by spawning");
            }

            ScreenshotCapture.CaptureFrom(new Vector3(10f, 16f, -2f), new Vector3(10f, 0f, 9f), "M1_3_loot_overview");
            ScreenshotCapture.CaptureFrom(new Vector3(9f, 6f, 6f), new Vector3(10f, 4f, 13f), "M1_3_loot_upstairs");
        }
    }
}
