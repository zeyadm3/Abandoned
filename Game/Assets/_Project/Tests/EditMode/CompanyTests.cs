using System.IO;
using Abandoned.Company;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;

namespace Abandoned.Tests
{
    public class CompanyTests
    {
        private CompanyConfig config;
        private string folder;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<CompanyConfig>();
            folder = Path.Combine(Path.GetTempPath(), "AbandonedSaveTest_" + System.Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
        }

        [Test]
        public void ANewCompanyHasTheStarterKitAndNoMoney()
        {
            CompanySave save = CompanySave.New();
            Assert.AreEqual(0, save.money);
            Assert.AreEqual(1, save.level);
            foreach ((string id, int count) in CompanySave.StarterKit) Assert.AreEqual(count, save.CountOf(id), id);
            Assert.AreEqual(4, save.CountOf("flashlight"), "one each for a full crew");
        }

        [Test]
        public void TheSaveSurvivesQuittingAndACorruptFileStartsOver()
        {
            var store = new SaveStore(folder);
            Assert.IsFalse(store.Exists);
            CompanySave save = store.Load();
            save.money = 12345;
            save.Add("rope", 2);
            store.Save(save);
            store.Save(save); // replacing an existing file works too

            CompanySave again = new SaveStore(folder).Load();
            Assert.AreEqual(12345, again.money);
            Assert.AreEqual(2, again.CountOf("rope"));

            File.WriteAllText(store.Path, "{ not json");
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("unreadable"));
            CompanySave fresh = store.Load();
            Assert.AreEqual(0, fresh.money);
            Assert.IsTrue(File.Exists(store.Path + ".corrupt"), "the broken file is kept for the player");
        }

        [Test]
        public void MeetingTheQuotaPaysTheHaulWithTheBonusAndEarnsExperience()
        {
            CompanySave save = CompanySave.New();
            save.missedQuotas = 2;
            RunOutcome o = CompanyLedger.Apply(save, config, haul: 50000, quota: 40000, payoutBonus: 0.2f);
            Assert.IsTrue(o.QuotaMet);
            Assert.AreEqual(60000, o.Payout);
            Assert.AreEqual(0, o.Penalty);
            Assert.AreEqual(60000, save.money);
            Assert.AreEqual(0, save.missedQuotas, "the streak resets");
            Assert.AreEqual(50 + config.QuotaXp, save.xp);
            Assert.AreEqual(config.LevelFor(save.xp), save.level);
            Assert.AreEqual(1, save.runs);
        }

        [Test]
        public void ASaveFromANewerBuildIsNeverOverwritten()
        {
            var store = new SaveStore(folder);
            Directory.CreateDirectory(folder);
            string newer = "{\"version\": " + (CompanySave.CurrentVersion + 1) + ", \"money\": 777}";
            File.WriteAllText(store.Path, newer);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("newer build"));
            CompanySave save = store.Load();
            Assert.IsFalse(store.Writable);
            save.money = 5;
            Assert.IsFalse(store.Save(save), "an old zip mustn't wipe the newer company");
            Assert.AreEqual(newer, File.ReadAllText(store.Path));
        }

        [Test]
        public void AJobLeftUnfinishedIsSettledAsAnEmptyHaul()
        {
            CompanySave save = CompanySave.New();
            Assert.IsNull(CompanyLedger.SettleAbandoned(save, config), "nothing pending");
            save.pendingQuota = 20000;
            save.pendingBonus = 0.5f;
            RunOutcome? o = CompanyLedger.SettleAbandoned(save, config);
            Assert.IsTrue(o.HasValue);
            Assert.IsFalse(o.Value.QuotaMet);
            Assert.AreEqual(1, save.missedQuotas, "quitting mid-job counts as a miss");
            Assert.AreEqual(0, save.pendingQuota, "settled once");
            Assert.IsNull(CompanyLedger.SettleAbandoned(save, config));
        }

        [Test]
        public void MissingTheQuotaCostsMoneyAndThreeInARowIsBankruptcy()
        {
            CompanySave save = CompanySave.New();
            save.money = 1000;
            save.Add("rope");
            RunOutcome o = CompanyLedger.Apply(save, config, 10000, 40000, 0.5f);
            Assert.IsFalse(o.QuotaMet);
            Assert.AreEqual(10000, o.Payout, "no bonus when the quota is missed");
            Assert.AreEqual(15000, o.Penalty, "half the shortfall");
            Assert.AreEqual(1000 + 10000 - 15000, save.money, "debt is negative money, not a lost save");

            CompanyLedger.Apply(save, config, 0, 40000, 0f);
            RunOutcome third = CompanyLedger.Apply(save, config, 0, 40000, 0f);
            Assert.IsTrue(third.Bankrupt);
            Assert.AreEqual(0, save.money, "a new company");
            Assert.AreEqual(0, save.CountOf("rope"));
            Assert.AreEqual(4, save.CountOf("flashlight"), "with the starter kit");
            Assert.AreEqual(1, save.bankruptcies);
        }

        [Test]
        public void LevelsComeFromExperience()
        {
            Assert.AreEqual(1, config.LevelFor(0));
            Assert.AreEqual(2, config.LevelFor(config.LevelXp[0]));
            Assert.AreEqual(config.LevelXp.Length + 1, config.LevelFor(int.MaxValue));
        }

        [Test]
        public void BuyingNeedsTheMoney()
        {
            CompanySave save = CompanySave.New();
            save.money = 500;
            Assert.IsFalse(CompanyLedger.TryBuy(save, "scanner", 800));
            Assert.IsTrue(CompanyLedger.TryBuy(save, "rope", 300));
            Assert.AreEqual(200, save.money);
            Assert.AreEqual(1, save.CountOf("rope"));
        }
    }
}
