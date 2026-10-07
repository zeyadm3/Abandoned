using Abandoned.EditorTools;
using Abandoned.Player;
using NUnit.Framework;
using UnityEditor;

namespace Abandoned.Tests
{
    /// <summary>0.12.5: wardrobe items are free, bought with company money, or achievement rewards.</summary>
    public class CosmeticUnlockTests
    {
        [Test]
        public void ItemsAreFreeBoughtOrEarned()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<CosmeticCatalog>(CosmeticsBuilder.CatalogPath);
            Assert.IsNotNull(catalog, "run Rebuild Content");
            Assert.AreEqual(CosmeticUnlock.Free, catalog.Coverall(0).Unlock, "the default coverall is free");
            Assert.AreEqual("hardhat", catalog.Hat(catalog.DefaultHat).Id);
            Assert.AreEqual(CosmeticUnlock.Free, catalog.Hat(catalog.DefaultHat).Unlock, "so is the default hat");
            CosmeticDefinition fedora = catalog.Hat(CosmeticCatalog.IndexOf(catalog.Hats, "fedora"));
            Assert.AreEqual(CosmeticUnlock.Buy, fedora.Unlock);
            Assert.Greater(fedora.Price, 0);
            CosmeticDefinition gold = catalog.Coverall(CosmeticCatalog.IndexOf(catalog.Coveralls, "gold"));
            Assert.AreEqual(CosmeticUnlock.Reward, gold.Unlock);
            Assert.IsNotNull(PlayerProfile.Achievement(gold.RewardAchievement), "rewards name a real achievement");
        }
    }
}
