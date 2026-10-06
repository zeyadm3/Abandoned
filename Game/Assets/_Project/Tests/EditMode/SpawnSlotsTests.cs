using Abandoned.Networking;
using NUnit.Framework;

namespace Abandoned.Tests
{
    public class SpawnSlotsTests
    {
        [Test]
        public void EachClientGetsTheLowestFreeSlotUntilFull()
        {
            var slots = new SpawnSlots(4);
            for (ulong id = 0; id < 4; id++)
            {
                Assert.IsTrue(slots.TryAssign(id, out int slot));
                Assert.AreEqual((int)id, slot);
            }
            Assert.IsFalse(slots.TryAssign(9, out int none), "a fifth player is refused");
            Assert.AreEqual(-1, none);
        }

        [Test]
        public void ReassigningKeepsTheSlotAndLeavingFreesIt()
        {
            var slots = new SpawnSlots(4);
            slots.TryAssign(0, out _);
            slots.TryAssign(5, out int five);
            slots.TryAssign(6, out _);
            Assert.IsTrue(slots.TryAssign(5, out int again));
            Assert.AreEqual(five, again);

            slots.Release(5);
            Assert.IsFalse(slots.TryGetSlot(5, out _));
            Assert.IsTrue(slots.TryAssign(7, out int reused));
            Assert.AreEqual(five, reused, "the freed slot (1) is reused before slot 3");
        }
    }
}
