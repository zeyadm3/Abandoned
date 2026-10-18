using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests.EditMode
{
    /// <summary>0.24.10: the Crew Director's stress and pacing rules, on the code defaults.</summary>
    public class CrewDirectorTests
    {
        private CrewDirectorConfig config;

        [SetUp] public void SetUp() => config = ScriptableObject.CreateInstance<CrewDirectorConfig>();
        [TearDown] public void TearDown() => Object.DestroyImmediate(config);

        [Test]
        public void StressDrainsButNeverWhileChased()
        {
            var track = new IntensityTrack();
            track.Add(50f, 0f);
            track.Tick(1f, false, 1f, config);
            Assert.AreEqual(50f, track.Value, 0.001f, "nothing drains straight after a rise");
            track.Tick(1f, false, 3f, config);
            Assert.AreEqual(50f - config.DrainPerSecond, track.Value, 0.001f, "then it drains");
            float before = track.Value;
            track.Tick(1f, true, 10f, config);
            Assert.Greater(track.Value, before, "being chased only adds");
            track.Add(500f, 11f);
            Assert.AreEqual(IntensityTrack.Max, track.Value, "capped");
        }

        [Test]
        public void ThePeakHoldsThenFadesThenRelaxesThenBuildsAgain()
        {
            var cycle = new CrewDirectorCycle(7);
            Run(cycle, config.MinBuildUp + 1f, 0f);
            Assert.AreEqual(CrewDirectorPhase.BuildUp, cycle.Phase, "calm crew: still building");
            Assert.IsFalse(cycle.Quiet);

            cycle.Tick(0.25f, config.Peak, config);
            Assert.AreEqual(CrewDirectorPhase.SustainPeak, cycle.Phase);
            Assert.IsFalse(cycle.Quiet, "the peak is held, not cut");
            Run(cycle, config.SustainSeconds.y + 0.5f, config.Peak);
            Assert.AreEqual(CrewDirectorPhase.PeakFade, cycle.Phase);
            Assert.IsTrue(cycle.Quiet);

            cycle.Tick(0.25f, config.FadeEndsBelow - 1f, config);
            Assert.AreEqual(CrewDirectorPhase.Relax, cycle.Phase);
            float relax = cycle.Remaining;
            Assert.That(relax, Is.InRange(config.RelaxSeconds.x, config.RelaxSeconds.y));
            Run(cycle, relax + 0.5f, 0f);
            Assert.AreEqual(CrewDirectorPhase.BuildUp, cycle.Phase, "and it builds again");
        }

        [Test]
        public void LeftoverStressCannotSkipTheBuildUpOrHoldTheFadeOpen()
        {
            var cycle = new CrewDirectorCycle(3);
            Run(cycle, config.MinBuildUp - 1f, 100f);
            Assert.AreEqual(CrewDirectorPhase.BuildUp, cycle.Phase, "a build-up always lasts MinBuildUp");
            Run(cycle, 2f + config.SustainSeconds.y, 100f);
            Assert.AreEqual(CrewDirectorPhase.PeakFade, cycle.Phase);
            Run(cycle, config.MaxFade + 0.5f, 100f);
            Assert.AreEqual(CrewDirectorPhase.Relax, cycle.Phase, "a crew stuck at full stress still gets the quiet, capped");
        }

        [Test]
        public void TheLongestQuietIsAboutAMinute()
        {
            Assert.LessOrEqual(config.MaxFade + config.RelaxSeconds.y, 60f);
            Assert.Less(config.FadeEndsBelow, config.Peak);
        }

        private void Run(CrewDirectorCycle cycle, float seconds, float highest)
        {
            for (float t = 0f; t < seconds; t += 0.25f) cycle.Tick(0.25f, highest, config);
        }
    }
}
