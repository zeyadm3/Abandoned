using Abandoned.Audio;
using Abandoned.Extraction;
using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>Doors and lockers (0.16.0): the doorway geometry, the tunables, and the words that explain them.</summary>
    public class DoorTests
    {
        // A four-metre doorway, three metres high, in its own space: X along it, Z through it.
        private const float Width = 4f, Height = 3f;

        [Test]
        public void AWalkThroughTheOpeningCrossesIt()
        {
            Assert.IsTrue(DoorMath.Crosses(new Vector3(0f, 0f, -3f), new Vector3(0f, 0f, 5f), Width, Height), "straight through the middle");
            Assert.IsTrue(DoorMath.Crosses(new Vector3(-1.5f, 0f, 2f), new Vector3(1.5f, 0f, -2f), Width, Height), "diagonally, from the other side");
            Assert.IsTrue(DoorMath.Crosses(new Vector3(1.9f, 0f, -1f), new Vector3(1.9f, 0f, 1f), Width, Height), "hugging the jamb");
        }

        [Test]
        public void AWalkBesideOrAlongTheWallDoesNot()
        {
            Assert.IsFalse(DoorMath.Crosses(new Vector3(0f, 0f, -3f), new Vector3(1f, 0f, -0.5f), Width, Height), "both ends on the same side");
            Assert.IsFalse(DoorMath.Crosses(new Vector3(6f, 0f, -3f), new Vector3(6f, 0f, 3f), Width, Height), "through the wall beside the opening");
            Assert.IsFalse(DoorMath.Crosses(new Vector3(-8f, 0f, -1f), new Vector3(8f, 0f, 0.2f), Width, Height, 0.3f), "crossing the wall's line far from the opening");
            Assert.IsFalse(DoorMath.Crosses(new Vector3(0f, 4.2f, -3f), new Vector3(0f, 4.2f, 3f), Width, Height), "on the storey above");
        }

        [Test]
        public void TheSlotIsTheStripTheLeavesCloseIn()
        {
            Assert.IsTrue(DoorMath.InSlot(new Vector3(0.5f, 0f, 0.1f), Width, Height, 0.45f), "standing in the doorway");
            Assert.IsFalse(DoorMath.InSlot(new Vector3(0.5f, 0f, 0.9f), Width, Height, 0.45f), "a step inside the room");
            Assert.IsFalse(DoorMath.InSlot(new Vector3(2.6f, 0f, 0f), Width, Height, 0.45f), "beside the opening, against the wall");
            Assert.IsFalse(DoorMath.InSlot(new Vector3(0f, 4f, 0f), Width, Height, 0.45f), "upstairs");
        }

        [Test]
        public void DistanceIsMeasuredToTheOpeningNotItsMiddle()
        {
            Assert.AreEqual(1.5f, DoorMath.FlatDistance(new Vector3(1.9f, 0f, 1.5f), Width), 1e-4f, "in front of it, off centre");
            Assert.AreEqual(5f, DoorMath.FlatDistance(new Vector3(5f, 0f, 4f), Width), 1e-4f, "3 m past the jamb and 4 m out");
        }

        [Test]
        public void TheDefaultsGiveAShutDoorAFewSecondsAndKeepLockersFair()
        {
            var config = ScriptableObject.CreateInstance<DoorConfig>();
            try
            {
                float bought = config.BashHits * config.BashInterval;
                Assert.GreaterOrEqual(bought, 2.5f, "a shut door must buy time worth having");
                Assert.LessOrEqual(bought, 8f, "but not a safe room");
                Assert.Less(config.StoreShutChance, 0.5f, "most stores start a job open, as before 0.16.0");
                Assert.Greater(config.BashNoticeRange, config.BashReach);
                Assert.Greater(config.HideReach, 1f, "the spot in front of a locker must be within reach of itself");
                Assert.Greater(config.WitnessRange, config.HideReach);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void TheNewSoundsAreSynthesisedAndADoorNeverCreaks()
        {
            foreach (SoundId id in new[] { SoundId.DoorSlide, SoundId.DoorShut, SoundId.DoorBash, SoundId.DoorBreak, SoundId.LockerOpen, SoundId.LockerShut })
            {
                SoundInfo info = SoundCatalog.Info(id);
                Assert.AreEqual(SoundMode.Synth, info.Mode, $"{id} has no recording");
                Assert.Greater(info.MaxDistance, info.MinDistance, $"{id}: distances");
                if (info.Caption != null) StringAssert.DoesNotContain("creak", info.Caption.ToLowerInvariant(), $"{id}: a creak only ever means the floor");
            }
            // The crew is meant to hear the Hunter at a door from across the building: walls dull it, never hide it.
            Assert.AreEqual(SoundOcclusion.Soft, SoundCatalog.Info(SoundId.DoorBash).Occlusion);
            Assert.AreEqual(SoundOcclusion.Soft, SoundCatalog.Info(SoundId.DoorBreak).Occlusion);
        }

        [Test]
        public void TheThreatFilesExplainDoorsAndLockers()
        {
            ThreatFiles.Entry hunter = ThreatFiles.For(ThreatKind.Hunter);
            StringAssert.Contains("door", hunter.Movement);
            StringAssert.Contains("locker", hunter.Counter);
            StringAssert.Contains("locker", ThreatFiles.For(ThreatKind.BlindOne).Counter);
            StringAssert.Contains("locker", ThreatFiles.For(ThreatKind.Crawlers).Counter);
            StringAssert.Contains("door", ThreatFiles.For(ThreatKind.LastHunter).Movement);
        }
    }
}
