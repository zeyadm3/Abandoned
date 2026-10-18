using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>
    /// 0.24.10 soak: real jobs in the mall for minutes, Crew Director on and off, with real monsters arriving and
    /// attacking a player who stands inside (healed between hits so a job lasts; a one-touch kill ends it and the
    /// next job starts). Checks the rules hold under real play and logs a "[Soak]" report. Explicit: run by name.
    /// </summary>
    [Explicit("Minutes long: Tools/unity.sh playmode MallPacingSoakTests")]
    public class MallPacingSoakTests
    {
        private const float Seconds = 300f;
        private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;

        private readonly List<string> arrivals = new();
        private readonly Dictionary<string, float> damageBy = new();
        private float hpLost;
        private int arrivalsInQuiet, hits, deaths, jobs, peaks;
        private float quietTime, longestQuiet, quietSince = -1f, highest;
        private bool watching;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // NUnit keeps one fixture for both tests: each soak starts from zero.
            arrivals.Clear();
            damageBy.Clear();
            hpLost = 0f;
            arrivalsInQuiet = hits = deaths = jobs = peaks = 0;
            quietTime = longestQuiet = highest = 0f;
            quietSince = -1f;
            yield return TestMapScene.Load("Mall");
            for (int i = 0; i < 60 && (RunState.Current == null || ThreatDirector.Current == null || CrewDirector.Current == null); i++) yield return null;
            Application.logMessageReceived += OnLog;
            NetworkPlayer.HealthChanged += OnHealth;
            NetworkPlayer.DeathChanged += OnDeath;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Application.logMessageReceived -= OnLog;
            NetworkPlayer.HealthChanged -= OnHealth;
            NetworkPlayer.DeathChanged -= OnDeath;
            SetEnabled(true);
            yield return null;
        }

        private static bool OpeningDone => ThreatDirector.Current != null
            && (float)typeof(ThreatDirector).GetField("spawnAt", Hidden).GetValue(ThreatDirector.Current) == 0f;

        private void OnLog(string message, string stack, LogType type)
        {
            if (!watching || !message.StartsWith("[Threat] ")) return;
            if (!message.Contains(" appears at ") && !message.Contains(" returns near the crew")) return;
            arrivals.Add($"{Time.time:0}s {(CrewDirector.Quiet ? "QUIET " : "")}{message.Substring(9)}");
            if (CrewDirector.Quiet && OpeningDone && !message.Contains("Last Hunter")) arrivalsInQuiet++;
        }

        private void OnHealth(NetworkPlayer p, float before, float after)
        {
            if (!watching || after >= before) return;
            hits++;
            hpLost += before - after;
            // Whatever monster is nearest at that moment (the cause string isn't sent with the change).
            Threat nearest = null;
            float best = 6f;
            foreach (Threat t in Threat.All)
            {
                if (t == null || !t.IsSpawned) continue;
                float d = Vector3.Distance(t.transform.position, p.transform.position);
                if (d < best) { best = d; nearest = t; }
            }
            string who = nearest != null ? nearest.DisplayName : "no monster within 6 m";
            damageBy[who] = (damageBy.TryGetValue(who, out float so) ? so : 0f) + before - after;
        }
        private void OnDeath(NetworkPlayer p, bool dead) { if (watching && dead) deaths++; }

        private static void SetEnabled(bool on)
        {
            CrewDirectorConfig config = CrewDirectorConfig.Current;
            typeof(CrewDirectorConfig).GetField("<Enabled>k__BackingField", Hidden).SetValue(config, on);
        }

        private IEnumerator StandInside()
        {
            NetworkPlayer.Local.OwnerTeleport(MallTestSpots.BestConnected(ThreatDirector.Current) + Vector3.up * 0.1f);
            yield return new WaitForSeconds(0.3f);
        }

        private IEnumerator Soak(bool director)
        {
            SetEnabled(director);
            yield return StandInside();
            CrewDirectorConfig config = CrewDirectorConfig.Current;
            float maxQuiet = config.MaxFade + Mathf.Max(config.RelaxSeconds.x, config.RelaxSeconds.y) + 1f;
            CrewDirectorPhase last = CrewDirector.Current.Cycle.Phase;
            float end = Time.time + Seconds, nextHeal = 0f;
            jobs = 1;
            watching = true;
            while (Time.time < end)
            {
                RunState run = RunState.Current;
                if (run.State.Phase == RunPhase.Departed)
                {
                    Object.FindAnyObjectByType<RunDirector>().StartNextRun();
                    jobs++;
                    yield return new WaitForSeconds(0.5f);
                    yield return StandInside();
                    continue;
                }
                NetworkPlayer me = NetworkPlayer.Local;
                if (Time.time >= nextHeal && !me.IsDead) { me.ServerHeal(100f); nextHeal = Time.time + 1f; }

                highest = Mathf.Max(highest, CrewDirector.Current.Highest);
                CrewDirectorPhase phase = CrewDirector.Current.Cycle.Phase;
                if (phase == CrewDirectorPhase.SustainPeak && last != phase) peaks++;
                last = phase;
                bool quiet = CrewDirector.Quiet;
                if (quiet && quietSince < 0f) quietSince = Time.time;
                if (!quiet && quietSince >= 0f) { Close(); }
                if (quietSince >= 0f)
                    Assert.LessOrEqual(Time.time - quietSince, maxQuiet, "a quiet never outlasts its cap");
                if (!director) Assert.IsFalse(quiet, "switched off, it never holds anything");
                yield return null;
            }
            if (quietSince >= 0f) Close();
            watching = false;

            var report = new StringBuilder();
            report.AppendLine($"[Soak] Director {(director ? "ON" : "OFF")}: {Seconds:0}s, {jobs} job(s), {arrivals.Count} arrivals " +
                $"({arrivalsInQuiet} during a quiet after the opening), {hits} hits ({hpLost:0} HP), {deaths} deaths, {peaks} peaks, " +
                $"quiet {quietTime:0}s total (longest {longestQuiet:0}s), highest stress {highest:0}.");
            foreach (var pair in damageBy) report.AppendLine($"[Soak]   damage near {pair.Key}: {pair.Value:0} HP");
            foreach (string a in arrivals) report.AppendLine($"[Soak]   {a}");
            Debug.Log(report.ToString());
            Assert.AreEqual(0, arrivalsInQuiet, "no monster arrives during a quiet once the opening wave is out");
        }

        private void Close()
        {
            float length = Time.time - quietSince;
            quietTime += length;
            longestQuiet = Mathf.Max(longestQuiet, length);
            quietSince = -1f;
        }

        [UnityTest, Timeout(600000)]
        public IEnumerator FiveMinutesWithTheDirector() => Soak(true);

        [UnityTest, Timeout(600000)]
        public IEnumerator FiveMinutesWithoutIt() => Soak(false);
    }
}
