using Abandoned.Networking;
using NUnit.Framework;

namespace Abandoned.Tests
{
    /// <summary>M7.6: the perf nettest's frame statistics.</summary>
    public class NetTestFrameStatsTests
    {
        [Test]
        public void EmptyStatsAreZero()
        {
            var s = new NetTestFrameStats();
            Assert.AreEqual(0, s.Count);
            Assert.AreEqual(0f, s.Percentile(0.95f));
            Assert.AreEqual(0f, s.Worst);
        }
    }
}
