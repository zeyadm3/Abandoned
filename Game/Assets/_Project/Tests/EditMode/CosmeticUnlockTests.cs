using Abandoned.EditorTools;
using Abandoned.Player;
using NUnit.Framework;
using UnityEditor;

namespace Abandoned.Tests
{
    /// <summary>M7.5: cosmetics unlock by playing, and say what's still needed.</summary>
    public class CosmeticUnlockTests
    {
        [Test]
        public void UnlocksNeedEveryThresholdAndExplainTheGap()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<CosmeticCatalog>(CosmeticsBuilder.CatalogPath);
            Assert.IsNotNull(catalog, "run Rebuild Content");
            CosmeticDefinition cone = catalog.Hat(CosmeticCatalog.IndexOf(catalog.Hats, "cone"));
            Assert.IsFalse(cone.IsUnlocked(0, 0, 0));
            Assert.AreEqual("Play 3 more runs", cone.Requirement(2, 0, 0));
            Assert.IsTrue(cone.IsUnlocked(5, 0, 0));
            CosmeticDefinition gold = catalog.Coverall(CosmeticCatalog.IndexOf(catalog.Coveralls, "gold"));
            Assert.AreEqual("Haul $50,000 more", gold.Requirement(9, 9, 100000));
            Assert.IsTrue(catalog.Coverall(0).IsUnlocked(0, 0, 0), "the default coverall is free");
            Assert.IsTrue(catalog.Hat(catalog.DefaultHat).IsUnlocked(0, 0, 0), "so is the default hat");
            Assert.AreEqual("hardhat", catalog.Hat(catalog.DefaultHat).Id);
        }
    }
}
