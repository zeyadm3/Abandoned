using Abandoned.Networking;
using NUnit.Framework;

namespace Abandoned.Tests
{
    public class NetTestArgsTests
    {
        [Test]
        public void ParsesEveryNetTestFlag()
        {
            NetTestArgs a = NetTestArgs.Parse(new[]
            {
                "Abandoned", "-batchmode", "-nettest", "client", "-nettestPort", "51234", "-nettestScenario", "basic",
                "-nettestOut", "/tmp/c1.json", "-nettestClients", "2", "-nettestTimeout", "45.5",
            });
            Assert.AreEqual(NetTestRole.Client, a.Role);
            Assert.AreEqual(51234, a.Port);
            Assert.AreEqual("basic", a.Scenario);
            Assert.AreEqual("/tmp/c1.json", a.OutPath);
            Assert.AreEqual(2, a.Clients);
            Assert.AreEqual(45.5f, a.Timeout, 1e-4f);
            Assert.IsNull(a.Problem());
        }

        [Test]
        public void WithoutTheNetTestFlagTheRunnerStaysInert()
        {
            NetTestArgs a = NetTestArgs.Parse(new[] { "Abandoned", "-host", "-nettestPort", "7777" });
            Assert.IsFalse(a.Active);
            Assert.IsFalse(NetTestArgs.Parse(null).Active);
            Assert.IsFalse(NetTestArgs.Parse(new[] { "-nettest", "spectator" }).Active, "unknown role");
        }

        [Test]
        public void DefaultsAndProblems()
        {
            NetTestArgs a = NetTestArgs.Parse(new[] { "-nettest", "HOST" });
            Assert.AreEqual(NetTestRole.Host, a.Role);
            Assert.AreEqual(NetTestArgs.DefaultClients, a.Clients);
            Assert.AreEqual(NetTestArgs.DefaultScenario, a.Scenario);
            Assert.AreEqual(NetTestArgs.DefaultTimeout, a.Timeout);
            StringAssert.Contains("-nettestPort", a.Problem(), "port 0 can't work: clients need the host's real port");

            NetTestArgs bad = NetTestArgs.Parse(new[] { "-nettest", "host", "-nettestPort", "99999", "-nettestTimeout", "-3" });
            Assert.AreEqual(0, bad.Port, "out-of-range port is rejected");
            Assert.AreEqual(NetTestArgs.DefaultTimeout, bad.Timeout, "negative timeout falls back to the default");
        }

        [Test]
        public void ScenarioRegistryFindsBasicAndRejectsUnknownNames()
        {
            Assert.IsTrue(NetTestScenarios.TryCreate("basic", out INetTestScenario s));
            Assert.AreEqual("basic", s.Name);
            Assert.IsTrue(NetTestScenarios.TryCreate("BASIC", out _), "case-insensitive like the other flags");
            Assert.IsFalse(NetTestScenarios.TryCreate("nope", out _));
            Assert.IsFalse(NetTestScenarios.TryCreate(null, out _));
            CollectionAssert.Contains(NetTestScenarios.Names, "basic");
        }
    }
}
