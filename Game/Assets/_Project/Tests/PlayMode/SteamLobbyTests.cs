using System.Collections;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>The in-scene SteamLobby adapter on a real NetworkBootstrap, with fake Steam lobbies.</summary>
    public class SteamLobbyTests
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

        private static SteamLobby AddLobby(NetworkBootstrap machine, FakeSteamLobbies fake)
        {
            var lobby = machine.gameObject.AddComponent<SteamLobby>();
            lobby.Setup(machine);
            lobby.UseLobbies(fake);
            return lobby;
        }

        [UnityTest]
        public IEnumerator ATestRunNeverStartsSteamForInvites()
        {
            NetworkBootstrap machine = net.AddMachine("Machine");
            var lobby = machine.gameObject.AddComponent<SteamLobby>();
            lobby.Setup(machine);
            yield return null;
            yield return null;
            Assert.IsTrue(SteamBootstrap.Instance == null || !SteamBootstrap.Instance.IsAvailable);
            Assert.IsNull(lobby.Flow, "no Steam, no lobby flow");
        }

        [UnityTest]
        public IEnumerator DirectIpHostingOpensNoSteamLobby()
        {
            var fake = new FakeSteamLobbies();
            NetworkBootstrap host = net.AddMachine("Host");
            SteamLobby lobby = AddLobby(host, fake);
            yield return null;
            net.StartHost(host);
            yield return null;
            Assert.IsFalse(lobby.Flow.InLobby);
            Assert.IsEmpty(fake.Lobbies);
        }

        [UnityTest]
        public IEnumerator AcceptedInviteConnectsOverSteamAndExplainsWhenSteamIsMissing()
        {
            var fake = new FakeSteamLobbies();
            NetworkBootstrap client = net.AddMachine("Client");
            SteamLobby lobby = AddLobby(client, fake);
            yield return null;
            FakeSteamLobbies.FakeLobby friends = fake.Add(76561198000000002UL, "Host");
            friends.Data[SteamLobbyFlow.VersionKey] = lobby.CompatibilityKey;
            fake.AcceptInvite(fake.IdOf(friends));
            yield return null;

            // Tests never start Steam: the join reaches the Steam transport and fails with the player-readable reason.
            Assert.AreEqual(TransportMode.Steam, client.Transport);
            Assert.AreEqual(76561198000000002UL, client.Manager.GetComponent<Netcode.Transports.Facepunch.FacepunchTransport>().targetSteamId);
            Assert.IsFalse(client.IsRunning);
            Assert.IsNotEmpty(lobby.Flow.LastError);
            Assert.AreEqual(client.LastError, lobby.Flow.LastError);
            Assert.IsFalse(lobby.Flow.InLobby, "the lobby is left again when NGO can't connect");
            CollectionAssert.Contains(fake.Left, fake.IdOf(friends));
        }
    }
}
