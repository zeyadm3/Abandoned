using System.Collections;
using System.IO;
using Abandoned.Company;
using Abandoned.Equipment;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Voice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>Gear at the HQ (M6.4): the starting kit, hand slots, the flashlight, the trolley rule, the radio rule and the shop.</summary>
    public class EquipmentTests
    {
        private string folder;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            folder = Path.Combine(Path.GetTempPath(), "AbandonedGear_" + System.Guid.NewGuid().ToString("N"));
            CompanyService.SaveFolderOverride = folder;
            yield return TestBuildingScene.Load("HQ");
            float end = Time.realtimeSinceStartup + 3f;
            while ((CompanyService.Current == null || Mine() == null) && Time.realtimeSinceStartup < end) yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            CompanyService.SaveFolderOverride = null;
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            yield return null;
        }

        private static PlayerEquipment Mine()
        {
            foreach (PlayerEquipment e in PlayerEquipment.All) if (e != null && e.IsOwner) return e;
            return null;
        }

        private static IEnumerator Frames(int n = 3)
        {
            for (int i = 0; i < n; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator EveryoneStartsWithALightAndARadioAndTheFlashlightShines()
        {
            PlayerEquipment me = Mine();
            Assert.AreEqual(EquipmentKind.Flashlight, me.InSlot(0).Kind);
            Assert.AreEqual(EquipmentKind.Radio, me.InSlot(1).Kind);
            Assert.IsTrue(NetworkVoice.RadioHolder(me.OwnerClientId), "a radio in hand: the walkie-talkie works");

            var reader = me.GetComponent<PlayerInputReader>();
            reader.Override = new PlayerInputFrame(default, default, false, false, false, false, false, false, false, false, false, false, flashlightPressed: true);
            yield return null;
            reader.Override = default(PlayerInputFrame);
            yield return Frames();
            Assert.IsTrue(me.State.LightOn);
            Assert.IsTrue(me.GetComponentInChildren<Light>(true).enabled, "the beam is on");

            me.RequestEquip(1, -1);
            yield return Frames();
            Assert.IsFalse(NetworkVoice.RadioHolder(me.OwnerClientId), "no radio, no walkie-talkie");
        }

        [UnityTest]
        public IEnumerator SoloHeavyDragsNeedTheTrolleyAndThereIsOnlyOne()
        {
            PlayerEquipment me = Mine();
            PlayerCarrier carrier = me.GetComponent<PlayerCarrier>();
            Assert.IsFalse(carrier.CanSoloDrag(CarryClass.Heavy), "no trolley in hand");
            int trolley = me.Catalog.IndexOf("hand_trolley");
            Assert.AreEqual(1, me.Available(trolley));
            me.RequestEquip(1, trolley);
            yield return Frames();
            Assert.AreEqual(EquipmentKind.HandTrolley, me.InSlot(1).Kind);
            Assert.IsTrue(carrier.CanSoloDrag(CarryClass.Heavy), "the trolley lets one person drag Heavy loot");
            Assert.AreEqual(0, me.Available(trolley), "the company's only trolley is taken");
            me.RequestEquip(0, trolley);
            yield return Frames();
            Assert.AreEqual(EquipmentKind.Flashlight, me.InSlot(0).Kind, "you can't hold the same single trolley twice");
        }

        [UnityTest]
        public IEnumerator TheShopSellsWhatTheMoneyAndLevelAllow()
        {
            CompanyService company = CompanyService.Current;
            EquipmentCatalog catalog = company.Equipment;
            int flashlight = catalog.IndexOf("flashlight"), noise = catalog.IndexOf("noise_maker");
            company.RequestBuy(flashlight);
            yield return Frames();
            Assert.AreEqual(4, company.OwnedCount(flashlight), "no money, no sale");

            company.Save.money = 1000;
            company.RequestBuy(noise);
            company.RequestBuy(flashlight);
            yield return Frames();
            Assert.AreEqual(0, company.OwnedCount(noise), "noise makers unlock at level 2");
            Assert.AreEqual(5, company.OwnedCount(flashlight));
            Assert.AreEqual(950, company.State.Money);
            Assert.AreEqual(5, new SaveStore(folder).Load().CountOf("flashlight"), "saved at once");
        }
    }
}
