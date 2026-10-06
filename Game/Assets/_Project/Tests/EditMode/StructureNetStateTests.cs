using Abandoned.Networking;
using Abandoned.Structure;
using NUnit.Framework;

namespace Abandoned.Tests
{
    /// <summary>The replicated section entry: quantization, Failing clock and change detection.</summary>
    public class StructureNetStateTests
    {
        [Test]
        public void HealthRoundTripsWithinHalfAStep()
        {
            for (float f = 0f; f <= 1f; f += 0.013f)
            {
                var e = new SectionNetState { Health = SectionNetState.QuantizeHealth(f) };
                Assert.AreEqual(f, e.HealthFraction, 0.5f / 255f + 1e-6f, $"health {f}");
            }
            Assert.AreEqual(0, SectionNetState.QuantizeHealth(-1f));
            Assert.AreEqual(255, SectionNetState.QuantizeHealth(2f));
        }

        [Test]
        public void LoadIsKeptInFiveKiloSteps()
        {
            Assert.AreEqual(95f, new SectionNetState { Load = SectionNetState.QuantizeLoad(95.4f) }.LoadKg);
            Assert.AreEqual(0f, new SectionNetState { Load = SectionNetState.QuantizeLoad(-3f) }.LoadKg);
            Assert.AreEqual(ushort.MaxValue * SectionNetState.LoadStep, new SectionNetState { Load = SectionNetState.QuantizeLoad(1e9f) }.LoadKg);
        }

        [Test]
        public void FailingClockFollowsServerTimeAndClamps()
        {
            var failing = new SectionNetState { Stage = (byte)StructuralStage.Failing, FailingSince = 100.0 };
            Assert.AreEqual(0.75f, failing.FailingTimeAt(100.75, 2f), 1e-4f);
            Assert.AreEqual(0f, failing.FailingTimeAt(99.0, 2f), "a client clock slightly behind never goes negative");
            Assert.AreEqual(2f, failing.FailingTimeAt(130.0, 2f), "never past the window");
            var cracking = new SectionNetState { Stage = (byte)StructuralStage.Cracking, FailingSince = 100.0 };
            Assert.AreEqual(0f, cracking.FailingTimeAt(101.0, 2f), "only Failing has a clock");
        }

        [Test]
        public void AnyFieldChangeIsAChange()
        {
            var a = new SectionNetState { Stage = 1, Health = 200, Load = 10, Seed = 0, FailingSince = 0, Generation = 1 };
            Assert.IsTrue(a.Equals(a));
            Assert.IsFalse(a.Equals(new SectionNetState { Stage = 2, Health = 200, Load = 10, Generation = 1 }));
            Assert.IsFalse(a.Equals(new SectionNetState { Stage = 1, Health = 199, Load = 10, Generation = 1 }));
            Assert.IsFalse(a.Equals(new SectionNetState { Stage = 1, Health = 200, Load = 11, Generation = 1 }));
            Assert.IsFalse(a.Equals(new SectionNetState { Stage = 1, Health = 200, Load = 10, Seed = 5, Generation = 1 }));
            Assert.IsFalse(a.Equals(new SectionNetState { Stage = 1, Health = 200, Load = 10, FailingSince = 3, Generation = 1 }));
            Assert.IsFalse(a.Equals(new SectionNetState { Stage = 1, Health = 200, Load = 10, Generation = 2 }), "a re-roll rewrites every entry");
        }
    }
}
