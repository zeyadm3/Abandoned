using Abandoned.Networking;
using NUnit.Framework;
using Unity.Netcode;

namespace Abandoned.Tests
{
    public class ConnectionGateTests
    {
        private const string HostKey = "0.3.0+abc1234";

        private static bool Admit(ulong id, string key, SpawnSlots slots, out int slot, out string reason) =>
            ConnectionGate.Admit(id, key == null ? null : ConnectionGate.Payload(key), HostKey, slots, 4, out slot, out reason);

        [Test]
        public void TheSameBuildGetsASpawnSlot()
        {
            var slots = new SpawnSlots(4);
            Assert.IsTrue(Admit(NetworkManager.ServerClientId, HostKey, slots, out int hostSlot, out _));
            Assert.IsTrue(Admit(1, HostKey, slots, out int slot, out string reason));
            Assert.AreEqual(0, hostSlot);
            Assert.AreEqual(1, slot);
            Assert.IsEmpty(reason);
        }

        [Test]
        public void AnotherBuildIsRefusedWithBothVersionsAndNoSlot()
        {
            var slots = new SpawnSlots(4);
            Assert.IsFalse(Admit(1, "0.3.1+abc1234", slots, out _, out string reason));
            StringAssert.Contains("0.3.0 (abc1234)", reason);
            StringAssert.Contains("0.3.1 (abc1234)", reason);
            Assert.AreEqual(0, slots.Count);
        }

        [Test]
        public void MissingOrOversizedPayloadsAreRefused()
        {
            var slots = new SpawnSlots(4);
            Assert.IsFalse(Admit(1, null, slots, out _, out string reason));
            StringAssert.Contains("unknown", reason);
            Assert.IsFalse(ConnectionGate.Admit(2, new byte[ConnectionGate.MaxPayloadBytes + 1], HostKey, slots, 4, out _, out _));
        }

        [Test]
        public void TheEditorPlaysWithAnyBuildOfTheSameVersion()
        {
            Assert.IsTrue(Admit(1, "0.3.0+editor", new SpawnSlots(4), out _, out _));
        }

        [Test]
        public void TheHostsOwnClientIsNeverVersionChecked()
        {
            Assert.IsTrue(ConnectionGate.Admit(NetworkManager.ServerClientId, null, HostKey, new SpawnSlots(4), 4, out _, out _));
        }

        [Test]
        public void AFifthPlayerIsToldTheGameIsFull()
        {
            var slots = new SpawnSlots(4);
            for (ulong id = 0; id < 4; id++) Assert.IsTrue(Admit(id, HostKey, slots, out _, out _));
            Assert.IsFalse(Admit(4, HostKey, slots, out _, out string reason));
            Assert.AreEqual(SessionMessages.Full(4), reason);
        }

        [Test]
        public void OnlyTheHostsReasonsReachPlayers()
        {
            Assert.AreEqual(SessionMessages.HostLeft, SessionMessages.FromHost(SessionMessages.HostLeft));
            Assert.AreEqual(string.Empty, SessionMessages.FromHost("[Disconnect Event][Client-1][TransportClientId-0] ProtocolTimeout"));
            Assert.AreEqual(string.Empty, SessionMessages.FromHost(null));
            StringAssert.Contains("0.3.0 (abc1234)", SessionMessages.CouldNotJoin("127.0.0.1:7777", "0.3.0 (abc1234)"));
        }
    }
}
