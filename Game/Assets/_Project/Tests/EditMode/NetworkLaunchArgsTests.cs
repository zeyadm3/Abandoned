using Abandoned.Networking;
using NUnit.Framework;

namespace Abandoned.Tests
{
    public class NetworkLaunchArgsTests
    {
        [Test]
        public void ParsesConnectWithPortAndTransport()
        {
            var args = NetworkLaunchArgs.Parse(new[] { "Game", "-connect", "192.168.1.5:7800", "-transport", "UNITY" });
            Assert.AreEqual("192.168.1.5", args.ConnectAddress);
            Assert.AreEqual(7800, args.ConnectPort);
            Assert.AreEqual(TransportMode.UnityTransport, args.Transport);
            Assert.IsTrue(args.IsClientLaunch);
            Assert.IsFalse(args.Host);
        }

        [Test]
        public void SteamIdConnectKeepsTheWholeIdAndNoPort()
        {
            var args = NetworkLaunchArgs.Parse(new[] { "-transport", "steam", "-connect", "76561198000000001" });
            Assert.AreEqual("76561198000000001", args.ConnectAddress);
            Assert.AreEqual(0, args.ConnectPort);
            Assert.AreEqual(TransportMode.Steam, args.Transport);
        }

        [Test]
        public void HostAndClientFlags()
        {
            Assert.IsTrue(NetworkLaunchArgs.Parse(new[] { "-host" }).Host);
            var client = NetworkLaunchArgs.Parse(new[] { "-client" });
            Assert.IsTrue(client.Client);
            Assert.IsTrue(client.IsClientLaunch, "-client alone (MPPM virtual player) is a client launch");
            Assert.IsFalse(NetworkLaunchArgs.Parse(new[] { "-batchmode", "-nographics" }).IsClientLaunch);
            Assert.IsNull(NetworkLaunchArgs.Parse(new[] { "-transport", "carrier-pigeon" }).Transport);
        }

        [Test]
        public void MissingValuesAndNullAreSafe()
        {
            Assert.IsFalse(NetworkLaunchArgs.Parse(null).IsClientLaunch);
            Assert.IsNull(NetworkLaunchArgs.Parse(new[] { "-connect" }).ConnectAddress, "-connect without a value is ignored");
        }

        [Test]
        public void SteamInviteLaunchJoinsTheLobbyAndNeverAutoHosts()
        {
            var args = NetworkLaunchArgs.Parse(new[] { "Abandoned", "+connect_lobby", "109775241234567890" });
            Assert.AreEqual(109775241234567890UL, args.ConnectLobby);
            Assert.IsTrue(args.IsClientLaunch);
            Assert.AreEqual(0UL, NetworkLaunchArgs.Parse(new[] { "+connect_lobby", "nope" }).ConnectLobby);
            Assert.AreEqual(0UL, NetworkLaunchArgs.Parse(new[] { "+connect_lobby" }).ConnectLobby);
        }

        [TestCase("127.0.0.1", "127.0.0.1", 0)]
        [TestCase(" 10.0.0.2:9000 ", "10.0.0.2", 9000)]
        [TestCase("host.local:notaport", "host.local", 0)]
        [TestCase("::1", "::1", 0)]
        public void SplitsAddresses(string text, string address, int port)
        {
            NetworkLaunchArgs.SplitAddress(text, out string a, out ushort p);
            Assert.AreEqual(address, a);
            Assert.AreEqual(port, p);
        }
    }
}
