using System;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abandoned.Tests
{
    public class SteamBootstrapTests
    {
        private static readonly string[] NoArgs = new string[0];
        private GameObject go;
        private NetworkConfig config;
        private SteamBootstrap bootstrap;
        private FakeSteamClient fake;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<NetworkConfig>();
            fake = new FakeSteamClient();
            bootstrap = SteamBootstrap.Create(config, fake);
            go = bootstrap.gameObject;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void PolicyKeepsSteamOffInBatchUnlessAsked()
        {
            Assert.IsFalse(SteamInitPolicy.Allows(true, NoArgs, out string reason));
            Assert.AreEqual(SteamInitPolicy.DisabledInBatchMessage, reason);
            Assert.IsTrue(SteamInitPolicy.Allows(true, new[] { "Unity", "-batchmode", "-STEAM" }, out _));
            Assert.IsTrue(SteamInitPolicy.Allows(false, NoArgs, out _));
            Assert.IsFalse(SteamInitPolicy.Allows(false, new[] { "-nosteam" }, out reason));
            Assert.AreEqual(SteamInitPolicy.DisabledByFlagMessage, reason);
            Assert.IsFalse(SteamInitPolicy.Allows(true, new[] { "-steam", "-nosteam" }, out _), "-nosteam wins.");
        }

        [Test]
        public void BatchModeReportsUnavailableWithoutTouchingSteam()
        {
            // The real client: the policy must stop before any native call.
            bootstrap.UseClient(new FacepunchSteamClient());
            bool ok = true;
            Assert.DoesNotThrow(() => ok = bootstrap.TryInitialize(true, NoArgs));
            Assert.IsFalse(ok);
            Assert.IsFalse(bootstrap.IsAvailable);
            Assert.AreEqual(SteamInitPolicy.DisabledInBatchMessage, bootstrap.LastError);
            Assert.IsFalse(new FacepunchSteamClient().IsValid);
        }

        [Test]
        public void SteamNotRunningGivesPlayerReadableError()
        {
            fake.InitException = new Exception("SteamApi_Init failed with NoSteamClient - error: Could not determine Steam client install directory.");
            bool ok = true;
            Assert.DoesNotThrow(() => ok = bootstrap.TryInitialize(false, NoArgs));
            Assert.IsFalse(ok);
            Assert.IsFalse(bootstrap.IsAvailable);
            Assert.AreEqual("Steam isn't running - start Steam and try again.", bootstrap.LastError);
            Assert.AreEqual(0UL, bootstrap.LocalSteamId);
        }

        [Test]
        public void MissingNativeLibraryGivesPlayerReadableError()
        {
            fake.InitException = new DllNotFoundException("libsteam_api (have 'x86_64', need 'arm64')");
            Assert.IsFalse(bootstrap.TryInitialize(false, NoArgs));
            Assert.AreEqual(SteamErrorMessages.LibraryMissing, bootstrap.LastError);
        }

        [Test]
        public void ErrorMappingCoversKnownFailures()
        {
            Assert.AreEqual(SteamErrorMessages.NeedsUpdate, SteamErrorMessages.FromException(new Exception("SteamApi_Init failed with VersionMismatch - error: x")));
            Assert.AreEqual(SteamErrorMessages.Generic, SteamErrorMessages.FromException(new Exception("SteamApi_Init failed with FailedGeneric - error: x")));
            Assert.AreEqual(SteamErrorMessages.Generic, SteamErrorMessages.FromException(null));
        }

        [Test]
        public void MissingConfigIsReportedNotThrown()
        {
            Object.DestroyImmediate(go);
            bootstrap = SteamBootstrap.Create(null, fake);
            go = bootstrap.gameObject;
            Assert.IsFalse(bootstrap.TryInitialize(false, NoArgs));
            Assert.AreEqual(SteamErrorMessages.ConfigMissing, bootstrap.LastError);
            Assert.AreEqual(0, fake.InitCalls);
        }

        [Test]
        public void SuccessfulInitUsesConfiguredAppIdAndShutsDownOnce()
        {
            int changes = 0;
            bootstrap.AvailabilityChanged += _ => changes++;
            Assert.IsTrue(bootstrap.TryInitialize(false, NoArgs));
            Assert.IsTrue(bootstrap.IsAvailable);
            Assert.AreEqual(string.Empty, bootstrap.LastError);
            Assert.AreEqual(NetworkConfig.DevelopmentAppId, fake.InitAppId);
            Assert.AreEqual(76561190000000001UL, bootstrap.LocalSteamId);

            Assert.IsTrue(bootstrap.TryInitialize(false, NoArgs), "Second call is a no-op.");
            Assert.AreEqual(1, fake.InitCalls);

            bootstrap.ShutdownSteam();
            bootstrap.ShutdownSteam();
            Assert.AreEqual(1, fake.ShutdownCalls);
            Assert.IsFalse(bootstrap.IsAvailable);
            Assert.AreEqual(2, changes);
        }

        [Test]
        public void DefaultConfigIsDevAppIdAndValid()
        {
            var errors = new System.Collections.Generic.List<string>();
            config.Validate(errors);
            Assert.IsEmpty(errors);
            Assert.AreEqual(480u, config.SteamAppId);
        }
    }
}
