using System;
using System.Collections;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Abandoned.Tests
{
    /// <summary>Steam's lifetime in Play mode, against a fake client (never the real Steam).</summary>
    public class SteamBootstrapLifetimeTests
    {
        private NetworkConfig config;
        private FakeSteamClient fake;
        private SteamBootstrap bootstrap;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<NetworkConfig>();
            fake = new FakeSteamClient();
            bootstrap = SteamBootstrap.Create(config, fake);
        }

        [TearDown]
        public void TearDown()
        {
            // Immediate: the next SetUp runs before end-of-frame destruction and would find this Instance.
            if (bootstrap != null) Object.DestroyImmediate(bootstrap.gameObject);
            Object.Destroy(config);
        }

        [UnityTest]
        public IEnumerator StartInBatchModeLeavesSteamOff()
        {
            yield return null;
            if (!Application.isBatchMode) Assert.Ignore("Only meaningful in batch mode.");
            Assert.AreEqual(0, fake.InitCalls, "Batch mode without -steam must not call Init.");
            Assert.IsFalse(bootstrap.IsAvailable);
            Assert.AreEqual(SteamInitPolicy.DisabledInBatchMessage, bootstrap.LastError);
            Assert.AreSame(bootstrap, SteamBootstrap.Instance);
        }

        [UnityTest]
        public IEnumerator RunsCallbacksEveryFrameAndShutsDownWhenDestroyed()
        {
            yield return null;
            Assert.IsTrue(bootstrap.TryInitialize(false, new string[0]));
            int before = fake.RunCallbacksCalls;
            for (int i = 0; i < 5; i++) yield return null;
            Assert.GreaterOrEqual(fake.RunCallbacksCalls - before, 5);

            Object.Destroy(bootstrap.gameObject);
            yield return null;
            Assert.AreEqual(1, fake.ShutdownCalls);
            Assert.IsNull(SteamBootstrap.Instance);
        }

        [UnityTest]
        public IEnumerator SecondBootstrapIsDiscarded()
        {
            var second = new GameObject("SecondSteamBootstrap").AddComponent<SteamBootstrap>();
            yield return null;
            Assert.IsTrue(second == null, "Duplicate should destroy itself.");
            Assert.AreSame(bootstrap, SteamBootstrap.Instance);
        }

        [UnityTest]
        public IEnumerator SteamDyingMidGameStopsPumpingWithoutThrowing()
        {
            yield return null;
            Assert.IsTrue(bootstrap.TryInitialize(false, new string[0]));
            int before = fake.RunCallbacksCalls;
            fake.RunCallbacksException = new InvalidOperationException("pipe closed");
            yield return null;
            yield return null;
            Assert.IsFalse(bootstrap.IsAvailable);
            Assert.AreEqual(SteamErrorMessages.LostConnection, bootstrap.LastError);
            Assert.AreEqual(1, fake.RunCallbacksCalls - before, "Stops calling a dead client.");
        }
    }
}
