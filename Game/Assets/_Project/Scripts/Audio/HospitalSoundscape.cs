using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>Local environmental sounds; no gameplay noise and no knowledge of monsters.</summary>
    public sealed class HospitalSoundscape : MonoBehaviour
    {
        [SerializeField] private HospitalSoundConfig config;
        [SerializeField] private Transform[] monitors;
        [SerializeField] private LightFixture[] monitorPower;
        [SerializeField] private Transform[] speakers;
        [SerializeField] private Transform[] trolleyRoute;
        [SerializeField] private Transform[] drips;
        private readonly List<SoundLoop> loops = new();
        private float nextMonitor;
        private float nextAnnouncement;
        private float nextTrolley;
        private float rollingUntil;
        private float nextWheel;
        private int cursor;

        private void Start()
        {
            if (Application.isBatchMode || config == null)
                return;
            nextMonitor = Time.time + 7f;
            nextAnnouncement = Time.time + 25f;
            nextTrolley = Time.time + 45f;
            foreach (Transform point in drips)
            {
                SoundLoop loop = GameAudio.Loop(SoundId.WaterDrip, point, "MorgueDrip", spatial: true);
                loop.Level = config.DripLevel;
                loop.SetRange(1f, 11f);
                loops.Add(loop);
            }
        }

        private void Update()
        {
            if (Application.isBatchMode || config == null)
                return;
            if (Time.time >= nextMonitor)
            {
                nextMonitor = Time.time + Random.Range(config.MonitorEvery.x, config.MonitorEvery.y);
                int index = cursor++ % monitors.Length;
                if (monitorPower[index] != null && monitorPower[index].Lit)
                    GameAudio.Play(SoundId.VoicemailBeep, monitors[index].position, config.MonitorLevel, 12f);
            }
            if (Time.time >= nextAnnouncement)
            {
                nextAnnouncement = Time.time + Random.Range(config.AnnouncementEvery.x, config.AnnouncementEvery.y);
                int index = cursor++ % speakers.Length;
                // A dead PA stays dead; the drip and distant wheels are independent of mains power.
                if (PoweredNear(speakers[index].position))
                    GameAudio.Play(SoundId.RadioStatic, speakers[index].position, config.AnnouncementLevel, 20f);
            }
            if (Time.time >= nextTrolley)
            {
                nextTrolley = Time.time + Random.Range(config.TrolleyEvery.x, config.TrolleyEvery.y);
                rollingUntil = Time.time + config.TrolleySeconds;
                nextWheel = Time.time;
            }
            if (Time.time >= rollingUntil || Time.time < nextWheel || trolleyRoute.Length < 2)
                return;
            nextWheel = Time.time + .65f;
            float progress = 1f - (rollingUntil - Time.time) / Mathf.Max(.1f, config.TrolleySeconds);
            Vector3 at = Vector3.Lerp(trolleyRoute[0].position, trolleyRoute[1].position, progress);
            GameAudio.Play(SoundId.LootRattle, at, .25f, 22f);
        }

        private bool PoweredNear(Vector3 at)
        {
            foreach (LightFixture fixture in monitorPower)
            {
                if (fixture != null && fixture.Lit && Vector3.Distance(fixture.transform.position, at) < 18f)
                    return true;
            }
            return false;
        }

        private void OnDisable()
        {
            foreach (SoundLoop loop in loops)
                loop.Level = 0f;
        }

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible)
                return;
            Draw(monitors, Color.green);
            Draw(speakers, Color.yellow);
            Draw(drips, Color.cyan);
            Draw(trolleyRoute, Color.magenta);
            if (trolleyRoute != null && trolleyRoute.Length > 1)
                Gizmos.DrawLine(trolleyRoute[0].position, trolleyRoute[1].position);
        }

        private static void Draw(Transform[] points, Color colour)
        {
            if (points == null)
                return;
            Gizmos.color = colour;
            foreach (Transform point in points)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, .35f);
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(HospitalSoundConfig settings, Transform[] screens, LightFixture[] power,
            Transform[] pa, Transform[] route, Transform[] water)
        {
            config = settings;
            monitors = screens;
            monitorPower = power;
            speakers = pa;
            trolleyRoute = route;
            drips = water;
        }
#endif
    }
}
