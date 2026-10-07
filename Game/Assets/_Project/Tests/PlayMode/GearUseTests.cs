using System.Collections;
using System.Linq;
using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Networking;
using Abandoned.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>Single-use gear over in-process NGO (M6.4b): medkit revives, planks appear, the noise maker shrieks.</summary>
    public class GearUseTests
    {
        private NetTestHarness net;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return CleanWorld();
            net = new NetTestHarness();
            net.BuildArena();
            yield return net.StartSession(clients: 1);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            net.Destroy();
            yield return null;
        }

        private static PlayerEquipment GearOf(NetworkBootstrap machine) =>
            PlayerEquipment.All.Single(e => e.NetworkManager == machine.Manager && e.IsOwner);

        private static IEnumerator Use(PlayerEquipment gear, string id)
        {
            gear.RequestEquip(0, gear.Catalog.IndexOf(id));
            yield return WaitFor(() => gear.InSlot(0) != null && gear.InSlot(0).Id == id, $"{id} in hand");
            var reader = gear.GetComponent<PlayerInputReader>();
            reader.Override = new PlayerInputFrame(default, default, false, false, false, false, true, true, false, false, false, false);
            yield return null;
            reader.Override = default(PlayerInputFrame);
            yield return WaitFor(() => gear.InSlot(0) == null, $"the {id} used up", 3f);
        }

        [UnityTest]
        public IEnumerator AMedkitGetsADownedCrewmateUp()
        {
            NetworkBootstrap client = net.Clients.First();
            NetworkPlayer victim = PlayerOf(net.Host, client.Manager.LocalClientId);
            victim.ServerKill();
            NetworkPlayer onClient = OwnPlayer(client);
            yield return WaitFor(() => onClient.IsDead && onClient.Ragdoll.IsRagdolled, "the client down on its own screen");

            OwnPlayer(net.Host).OwnerTeleport(victim.Ragdoll.BodyPosition + Vector3.right * 1f);
            yield return Use(GearOf(net.Host), "medkit");
            yield return WaitFor(() => !onClient.IsDead && !onClient.Ragdoll.IsRagdolled, "back up on its own screen", 3f);
            Assert.IsFalse(victim.IsDead);
        }

        [UnityTest]
        public IEnumerator PlanksAppearForEveryoneToCarry()
        {
            int before = Object.FindObjectsByType<NetworkLoot>(FindObjectsSortMode.None).Count(l => l.Item.Definition.Id == "plank");
            yield return Use(GearOf(net.Host), "planks");
            yield return WaitFor(() => Object.FindObjectsByType<NetworkLoot>(FindObjectsSortMode.None)
                .Count(l => l.IsSpawned && l.Item.Definition.Id == "plank") >= before + 2, "a plank on host and client");
            NetworkLoot plank = Object.FindObjectsByType<NetworkLoot>(FindObjectsSortMode.None).First(l => l.Item.Definition.Id == "plank");
            Assert.AreEqual(0, plank.Item.FullValue, "worth nothing");
            Assert.IsTrue(plank.Item.Definition.Utility);
        }

        [UnityTest]
        public IEnumerator TheNoiseMakerShrieksForTheMonsters()
        {
            float since = Time.time;
            yield return Use(GearOf(net.Host), "noise_maker");
            NoiseMakerDevice device = null;
            yield return WaitFor(() => (device = Object.FindObjectsByType<NoiseMakerDevice>(FindObjectsSortMode.None).FirstOrDefault(d => d.IsServer)) != null, "it's thrown");
            Vector3 thrownFrom = OwnPlayer(net.Host).transform.position;
            yield return WaitFor(() => device != null && device.Shrieking, "the shriek after its fuse", 5f);
            yield return WaitSeconds(0.2f);
            var shrieks = NoiseSystem.RecentEvents.Where(e => e.Source == NoiseSource.Other && e.Time >= since).ToList();
            Assert.IsNotEmpty(shrieks, "threats hear it");
            Assert.Greater(shrieks[shrieks.Count - 1].Loudness, 0.5f);
            Assert.Greater(Vector3.Distance(shrieks[shrieks.Count - 1].Position, thrownFrom), 2f, "it was thrown away from the thrower");
            Assert.IsTrue(Object.FindObjectsByType<NoiseMakerDevice>(FindObjectsSortMode.None).Any(d => !d.IsServer && d.Shrieking), "the client hears it too");
        }
    }
}
