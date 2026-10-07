using System.Collections;
using System.IO;
using System.Linq;
using Abandoned.Company;
using Abandoned.Contracts;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>The company loop, solo: HQ -> contract -> the mall on its terms -> payday (saved) -> back to HQ.</summary>
    public class CompanyLoopTests
    {
        private string folder;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            folder = Path.Combine(Path.GetTempPath(), "AbandonedCompanyLoop_" + System.Guid.NewGuid().ToString("N"));
            CompanyService.SaveFolderOverride = folder;
            yield return TestBuildingScene.Load("HQ", randomRun: true);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            CompanyService.SaveFolderOverride = null;
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            yield return null;
        }

        private static IEnumerator WaitUntil(System.Func<bool> condition, float seconds, string what)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < end) yield return null;
            Assert.IsTrue(condition(), what);
        }

        [UnityTest]
        public IEnumerator TakeAJobHaulItPaydayAndHome()
        {
            yield return WaitUntil(() => CompanyService.Current != null && SessionTravel.Current != null, 3f, "the session's company and travel");
            CompanyService company = CompanyService.Current;
            Assert.AreEqual(0, company.State.Money, "a new company");
            Assert.AreEqual(3, company.Board.Count, "three contracts on the board");
            Assert.IsNull(NetworkBootstrap.JoinBlocker(), "anyone can join at the HQ");

            var van = Object.FindAnyObjectByType<HqVan>();
            StringAssert.Contains("Pick a contract", van.UsePrompt(null), "no job yet");
            company.Select(0);
            Contract job = company.Board[0];
            StringAssert.StartsWith("Drive to", van.UsePrompt(null));
            van.Use(null);

            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Mall" && RunState.Current != null && RunState.Current.IsSpawned,
                10f, "everyone drove to the mall");
            RunNetState s = RunState.Current.State;
            Assert.AreEqual(company.QuotaFor(job), s.Quota, "the contract's quota for this crew");
            Assert.AreEqual(job.WindowSeconds, s.Window, 0.01f, "the contract's window");
            Assert.AreEqual(job.Seed, s.Seed, "the contract's seed");
            Assert.AreEqual(job.PowerOff, s.PowerOff);
            Assert.AreEqual(job.Stability, Object.FindAnyObjectByType<Abandoned.Structure.StructureSimulation>().Stability, 0.001f, "the contract's stability");
            Assert.IsNotNull(NetworkBootstrap.JoinBlocker(), "no joining while out on a job");

            // Load one item, get in, leave.
            var truck = TruckCargo.Current;
            NetworkLoot cargo = Object.FindAnyObjectByType<NetworkLootSpawner>().Spawned
                .First(l => l != null && l.Item.Definition.CarryClass == CarryClass.OneHand && l.Item.Definition.Fragility <= Abandoned.Loot.Fragility.Medium);
            Vector3 bay = truck.transform.TransformPoint(new Vector3(0f, 1.2f, -1.2f));
            cargo.Grabbable.Body.position = bay;
            cargo.transform.position = bay;
            PlayerTestRig rig = ForExisting(TestBuildingScene.Player);
            rig.Teleport(truck.transform.position + Vector3.up * 0.45f + truck.transform.forward * 0.8f);
            rig.Settle();
            yield return new WaitForSeconds(0.6f);
            int haul = RunState.Current.State.Haul;
            Assert.Greater(haul, 0);
            RunState.Current.RequestDepart();
            yield return WaitUntil(() => RunState.Current.Results != null && company.LastOutcome.Run > 0, RunState.Current.Config.HonkSeconds + 4f, "payday");
            yield return null;
            yield return null;
            yield return MenuTests.Capture(Abandoned.UI.MenuUi.Current, "M8_appraisal_payday");

            OutcomeNet o = company.LastOutcome;
            Assert.IsFalse(o.QuotaMet, "one item won't make quota");
            Assert.AreEqual(haul, o.Payout);
            Assert.Greater(o.Penalty, 0, "the shortfall costs money");
            Assert.AreEqual(o.Payout - o.Penalty - o.Costs, company.State.Money, "debt is negative money");
            Assert.AreEqual(1, company.State.MissedQuotas);
            Assert.IsTrue(File.Exists(Path.Combine(folder, SaveStore.FileName)), "saved on the host's machine");
            Assert.AreEqual(company.State.Money, new SaveStore(folder).Load().money, "the file says the same");

            int oldBoard = company.Board[0].Seed;
            company.ReturnToHq();
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "HQ" && SessionTravel.LevelReady, 10f, "home again");
            Assert.AreEqual(-1, company.Selected, "a fresh board");
            Assert.AreNotEqual(oldBoard, company.Board[0].Seed);
            Assert.IsFalse(company.Active.IsValid);
            Assert.IsNull(RunState.Current, "the run stayed at the mall");
            Assert.IsNull(NetworkBootstrap.JoinBlocker(), "friends can join again");
        }
    }
}
