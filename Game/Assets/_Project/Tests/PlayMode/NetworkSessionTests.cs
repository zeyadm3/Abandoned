using System.Collections;
using System.Linq;
using Abandoned.Core;
using Abandoned.Networking;
using Netcode.Transports.Facepunch;
using NUnit.Framework;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>NetworkBootstrap sessions: authority, transport choice, the 4-player cap, leaving.</summary>
    public class NetworkSessionTests
    {
        private NetTestHarness net;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return CleanWorld();
            net = new NetTestHarness();
            net.BuildArena();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            net.Destroy();
            yield return null;
        }

        [UnityTest]
        public IEnumerator GameAuthorityIsHostOfflineAndOnTheHostButNotOnAClient()
        {
            // The first bootstrap answers GameAuthority, so make it the client's.
            NetworkBootstrap client = net.AddMachine("Client");
            NetworkBootstrap host = net.AddMachine("Host");
            Assert.AreSame(client, NetworkBootstrap.Instance);
            Assert.IsTrue(GameAuthority.IsHost, "offline (solo, before any session) counts as host");

            net.StartHost(host);
            yield return net.ConnectClients();
            Assert.IsFalse(GameAuthority.IsHost, "a connected client must not apply host rules");
            Assert.IsTrue(NetworkBootstrap.IsHostOrOffline(host.Manager));
            Assert.IsFalse(NetworkBootstrap.IsHostOrOffline(client.Manager));

            client.Disconnect();
            yield return WaitFor(() => !client.Manager.IsListening && !client.Manager.ShutdownInProgress, "the client to stop");
            Assert.IsTrue(GameAuthority.IsHost, "back offline");
        }

        [UnityTest]
        public IEnumerator SoloHostingSpawnsTheLocalPlayer()
        {
            NetworkBootstrap host = net.AddMachine("Solo");
            net.StartHost(host);
            yield return null;
            Assert.IsNotNull(NetworkPlayer.Local, "hosting alone is how solo play works");
            Assert.IsTrue(NetworkPlayer.Local.IsOwner);
            Assert.That(host.Status, Does.StartWith("Hosting (1/4)"));
            Assert.IsTrue(GameAuthority.IsHost);
        }

        [UnityTest]
        public IEnumerator TransportChoiceSelectsTheNgoTransport()
        {
            NetworkBootstrap machine = net.AddMachine("Machine");
            Assert.AreEqual(TransportMode.UnityTransport, machine.Transport, "config default");
            Assert.IsInstanceOf<UnityTransport>(machine.Manager.NetworkConfig.NetworkTransport);
            Assert.IsTrue(machine.SelectTransport(TransportMode.Steam));
            Assert.IsInstanceOf<FacepunchTransport>(machine.Manager.NetworkConfig.NetworkTransport);

            // Tests never start Steam, so this is the "Steam isn't running" path: a clear error, no session.
            Assert.IsFalse(machine.StartClient("76561198000000001"));
            Assert.AreEqual(76561198000000001UL, machine.Manager.GetComponent<FacepunchTransport>().targetSteamId);
            Assert.IsFalse(machine.IsRunning);
            Assert.IsNotEmpty(machine.LastError);
            Assert.IsFalse(machine.StartClient("not-a-steam-id"));
            StringAssert.Contains("SteamID64", machine.LastError);

            Assert.IsTrue(machine.SelectTransport(TransportMode.UnityTransport));
            net.StartHost(machine);
            Assert.IsFalse(machine.SelectTransport(TransportMode.Steam), "can't switch mid-session");
            Assert.IsInstanceOf<UnityTransport>(machine.Manager.NetworkConfig.NetworkTransport);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AFifthPlayerIsTurnedAwayAndALeaverFreesTheirSpawn()
        {
            yield return net.StartSession(clients: 3);
            NetworkBootstrap extra = net.AddMachine("Client4");
            Assert.IsTrue(extra.StartClient("127.0.0.1", net.Port));
            yield return WaitFor(() => !extra.IsRunning && !extra.Manager.ShutdownInProgress, "the fifth player to be refused");
            StringAssert.Contains("full", extra.LastError);
            Assert.AreEqual(4, net.Host.Slots.Count);

            NetworkBootstrap leaver = net.Clients.First();
            ulong leaverId = leaver.Manager.LocalClientId;
            net.Host.Slots.TryGetSlot(leaverId, out int freed);
            leaver.Disconnect();
            yield return WaitFor(() => !net.Host.Manager.ConnectedClientsIds.Contains(leaverId), "the host to drop the leaver");
            Assert.IsFalse(net.Host.Slots.TryGetSlot(leaverId, out _));
            Assert.AreEqual(3, PlayersOn(net.Host).Count(), "the leaver's player is despawned");

            Assert.IsTrue(extra.StartClient("127.0.0.1", net.Port), extra.LastError);
            yield return WaitFor(() => extra.Manager.IsConnectedClient, "the waiting player to get in");
            Assert.IsTrue(net.Host.Slots.TryGetSlot(extra.Manager.LocalClientId, out int slot));
            Assert.AreEqual(freed, slot, "they take the free spawn point");
        }
    }
}
