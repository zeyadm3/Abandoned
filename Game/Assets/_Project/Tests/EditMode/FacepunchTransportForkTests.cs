using System.IO;
using System.Text.RegularExpressions;
using Abandoned.Networking;
using Netcode.Transports.Facepunch;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>The three ABANDONED patches on the embedded Facepunch transport fork.</summary>
    public class FacepunchTransportForkTests
    {
        private const string PackageRoot = "Packages/com.community.netcode.transport.facepunch";
        private GameObject go;
        private FacepunchTransport transport;

        [SetUp]
        public void SetUp()
        {
            // These tests are about behaviour without Steam; if a developer has it up, they can't prove anything.
            Assume.That(new FacepunchSteamClient().IsValid, Is.False, "Steam is initialised in this editor.");
            go = new GameObject("FacepunchTransportTest");
            transport = go.AddComponent<FacepunchTransport>();
            LogAssert.Expect(LogType.Warning, new Regex("Initialized without Steam"));
            transport.Initialize();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(go);

        [Test]
        public void InitializeDoesNotStartSteam()
        {
            Assert.IsFalse(new FacepunchSteamClient().IsValid);
        }

        [Test]
        public void StartClientWithoutSteamReturnsFalseWithClearLog()
        {
            LogAssert.Expect(LogType.Error, new Regex(Regex.Escape(FacepunchTransport.SteamNotRunningMessage)));
            Assert.IsFalse(transport.StartClient());
        }

        [Test]
        public void StartServerWithoutSteamReturnsFalseWithClearLog()
        {
            LogAssert.Expect(LogType.Error, new Regex(Regex.Escape(FacepunchTransport.SteamNotRunningMessage)));
            Assert.IsFalse(transport.StartServer());
        }

        [Test]
        public void ShutdownWithoutSessionDoesNotThrowAndLeavesSteamAlone()
        {
            Assert.DoesNotThrow(() => transport.Shutdown());
            // Guard against an upstream merge bringing back the lifetime calls SteamBootstrap owns.
            // Comments explain the patch and mention the calls, so only code counts.
            string source = Regex.Replace(File.ReadAllText($"{PackageRoot}/Runtime/FacepunchTransport.cs"), "//.*", "");
            StringAssert.DoesNotContain("SteamClient.Shutdown(", source);
            StringAssert.DoesNotContain("SteamClient.Init(", source);
            StringAssert.DoesNotContain("SteamClient.RunCallbacks(", source);
        }

        [Test]
        public void RttComesFromSteamPing()
        {
            Assert.AreEqual(87UL, FacepunchTransport.RttFromPing(87));
            Assert.AreEqual(0UL, FacepunchTransport.RttFromPing(-1), "Steam reports unknown ping as negative.");
            Assert.AreEqual(0UL, transport.GetCurrentRtt(5), "No connection, no RTT, no exception.");
            StringAssert.Contains("QuickStatus().Ping", File.ReadAllText($"{PackageRoot}/Runtime/FacepunchTransport.cs"));
        }

        [Test]
        public void PackageIsTheVersionedFork()
        {
            StringAssert.Contains("\"version\": \"2.0.0-abandoned.", File.ReadAllText($"{PackageRoot}/package.json"));
            StringAssert.Contains("2.0.0-abandoned.1", File.ReadAllText($"{PackageRoot}/CHANGELOG.md"));
        }
    }
}
