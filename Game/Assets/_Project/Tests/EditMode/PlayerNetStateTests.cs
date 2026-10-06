using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    public class PlayerNetStateTests
    {
        [Test]
        public void FlagsRoundTrip()
        {
            PlayerNetState s = PlayerNetState.From(true, false, true, false, true, Vector3.zero);
            Assert.IsTrue(s.Grounded);
            Assert.IsFalse(s.Sprinting);
            Assert.IsTrue(s.Crouching);
            Assert.IsFalse(s.Ragdolled);
            Assert.IsTrue(s.BodyResting);
        }

        [Test]
        public void BodyPositionOnlyMattersWhileRagdolledAndIsSnapped()
        {
            Assert.AreEqual(Vector3.zero, PlayerNetState.From(true, false, false, false, false, new Vector3(3f, 2f, 1f)).BodyPosition,
                "standing players don't resend their pelvis every frame");
            PlayerNetState a = PlayerNetState.From(false, false, false, true, false, new Vector3(1.001f, 2f, 3f));
            PlayerNetState b = PlayerNetState.From(false, false, false, true, false, new Vector3(1.004f, 2f, 3f));
            Assert.AreEqual(a.BodyPosition, b.BodyPosition, "millimetre jitter doesn't change the replicated value");
            Assert.AreEqual(1f, a.BodyPosition.x, 0.011f);
        }
    }
}
