using System.Collections;
using Abandoned.Networking;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Abandoned.Tests
{
    /// <summary>
    /// Steam must outlive NGO's shutdown: with the Facepunch transport, closing Steam first makes NGO's
    /// disconnect and transport shutdown call into torn-down Steam interfaces.
    /// </summary>
    public class SteamShutdownOrderTests
    {
        private Abandoned.Networking.NetworkConfig config;
        private FakeSteamClient fake;
        private SteamBootstrap bootstrap;
        private NetworkManager network;
        private bool? listeningWhenSteamShutDown;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            config = ScriptableObject.CreateInstance<Abandoned.Networking.NetworkConfig>();
            fake = new FakeSteamClient();
            fake.OnShutdown = () => listeningWhenSteamShutDown = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
            bootstrap = SteamBootstrap.Create(config, fake);
            yield return null;
            Assert.IsTrue(bootstrap.TryInitialize(false, new string[0]));

            var go = new GameObject("TestNetworkManager");
            var transport = go.AddComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 7791);
            network = go.AddComponent<NetworkManager>();
            network.NetworkConfig = new Unity.Netcode.NetworkConfig { NetworkTransport = transport };
            yield return null;
            Assert.IsTrue(network.StartHost(), "Host should start on localhost.");
            yield return null;
            Assert.IsTrue(network.IsListening);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (network != null)
            {
                if (network.IsListening) network.Shutdown();
                yield return null;
                Object.DestroyImmediate(network.gameObject);
            }
            if (bootstrap != null) Object.DestroyImmediate(bootstrap.gameObject);
            Object.Destroy(config);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ShutdownSteamWhileHostingStopsNetworkFirst()
        {
            bootstrap.ShutdownSteam();
            Assert.IsFalse(bootstrap.IsAvailable, "Steam is off for the game as soon as shutdown is asked for.");
            Assert.AreEqual(0, fake.ShutdownCalls, "Steam must wait for NGO to close its sockets.");
            Assert.IsTrue(network.ShutdownInProgress, "Shutting Steam down should shut the session down.");

            for (int i = 0; i < 30 && network.IsListening; i++) yield return null;
            Assert.IsFalse(network.IsListening);
            Assert.AreEqual(1, fake.ShutdownCalls);
            Assert.AreEqual(false, listeningWhenSteamShutDown, "NGO was still listening when Steam shut down.");
        }

        // Unity doesn't order OnApplicationQuit between objects; the bad order (Steam's first) is the one that broke.
        [UnityTest]
        public IEnumerator QuitWithSteamCallbackFirstStillShutsNetworkFirst()
        {
            bootstrap.SendMessage("OnApplicationQuit");
            Assert.AreEqual(0, fake.ShutdownCalls);
            network.SendMessage("OnApplicationQuit");
            Assert.IsFalse(network.IsListening, "NGO's quit handler shuts down synchronously.");
            Assert.AreEqual(1, fake.ShutdownCalls);
            Assert.AreEqual(false, listeningWhenSteamShutDown);
            yield return null;
        }

        [UnityTest]
        public IEnumerator QuitWithNetworkCallbackFirstShutsSteamImmediately()
        {
            network.SendMessage("OnApplicationQuit");
            bootstrap.SendMessage("OnApplicationQuit");
            Assert.AreEqual(1, fake.ShutdownCalls);
            Assert.AreEqual(false, listeningWhenSteamShutDown);
            yield return null;
        }
    }
}
