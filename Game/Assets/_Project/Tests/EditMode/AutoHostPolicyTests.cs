using Abandoned.Networking;
using NUnit.Framework;

namespace Abandoned.Tests
{
    public class AutoHostPolicyTests
    {
        private static readonly NetworkLaunchArgs NoArgs = NetworkLaunchArgs.Parse(new string[0]);

        [Test]
        public void MainEditorPlayAutoHostsWhenEnabled()
        {
            Assert.IsTrue(AutoHostPolicy.ShouldAutoHost(true, isEditor: true, isMainEditor: true, NoArgs));
            Assert.IsFalse(AutoHostPolicy.ShouldAutoHost(false, true, true, NoArgs), "toggle off");
        }

        [Test]
        public void VirtualPlayersAndClientLaunchesNeverAutoHost()
        {
            Assert.IsFalse(AutoHostPolicy.ShouldAutoHost(true, true, isMainEditor: false, NoArgs), "MPPM virtual player");
            Assert.IsFalse(AutoHostPolicy.ShouldAutoHost(true, true, true, NetworkLaunchArgs.Parse(new[] { "-client" })));
            Assert.IsFalse(AutoHostPolicy.ShouldAutoHost(true, true, true, NetworkLaunchArgs.Parse(new[] { "-connect", "127.0.0.1" })));
            Assert.IsFalse(AutoHostPolicy.ShouldAutoHost(true, true, true, NetworkLaunchArgs.Parse(new[] { "-host", "-client" })),
                "client wins over host");
        }

        [Test]
        public void BuildsOnlyHostWithTheHostFlag()
        {
            Assert.IsFalse(AutoHostPolicy.ShouldAutoHost(true, isEditor: false, isMainEditor: true, NoArgs), "a build waits for the menu");
            Assert.IsTrue(AutoHostPolicy.ShouldAutoHost(false, false, true, NetworkLaunchArgs.Parse(new[] { "-host" })));
        }
    }
}
