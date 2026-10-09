using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Voice;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// Every machine, for its own ears (2026-10-08): the song of the most dangerous monster coming for you.
    /// A monster counts only while it is on your floor and moving toward you, and its song starts when it
    /// could reach you within <see cref="StartSeconds"/> at its top speed (straight line: the real path is
    /// never shorter, so that is at least 15 s of warning). It stops soon after it stops coming or changes floor. Of all the monsters counting, only the highest in the hierarchy
    /// (<see cref="ThreatDefinition.HierarchyScore"/>) plays, the nearer one breaking a tie; it gets louder
    /// as it gets closer and crossfades when another takes over. Reads only replicated position and state,
    /// so nothing new is networked. This object only chooses; Audio.MusicPlayer plays the song.
    /// One per game, made on first scene load (never in batch mode).
    /// </summary>
    public class ThreatMusic : MonoBehaviour
    {
        public const float StartSeconds = 15f;
        private const float FullSeconds = 2f, Linger = 2f, Tick = 0.1f;
        private const float QuietestShare = 0.15f, ChaseBoost = 0.15f, ClosingSpeed = 0.3f;
        // Same floor: feet within this height of each other (stories are 4 m), and the ear sits ~1.6 m above the feet.
        private const float SameFloor = 2.5f, EarHeight = 1.6f;

        private class Track
        {
            public float lastDistance = -1f, lastTime, closing, observedSpeed, lastApproach = -100f;
            public Vector3 lastPosition;
            public bool active;
            public float eta = float.MaxValue;
        }

        private static ThreatMusic instance;
        private readonly Dictionary<Threat, Track> tracks = new();
        private readonly List<Threat> gone = new();
        private Threat leader;
        private float nextTick, targetVolume, speedPerLevel = 0.08f;
        private RunState lastRun;

        /// <summary>The monster whose song this machine plays now, or null.</summary>
        public static Threat Leader => instance != null ? instance.leader : null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Application.isBatchMode || instance != null) return;
            var go = new GameObject("ThreatMusic");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<ThreatMusic>();
        }

        private void Update()
        {
            RunState run = RunState.Current;
            bool live = run != null && run.IsSpawned && run.State.Phase != RunPhase.Departed;
            if (live && run != lastRun) NewRun(run);
            if (Time.time >= nextTick)
            {
                nextTick = Time.time + Tick;
                if (live) Choose(run);
                else { leader = null; tracks.Clear(); }
            }
            // The song itself is played by the one music object, with the rest of the game's music.
            if (leader != null && leader.Definition != null)
                MusicPlayer.SetThreat(SoundId.ThemeBlindOne + (int)leader.Definition.Kind, leader.Definition.MusicClip, targetVolume);
            else MusicPlayer.ClearThreat();
        }

        private void NewRun(RunState run)
        {
            lastRun = run;
            tracks.Clear();
            DangerDirector danger = FindAnyObjectByType<DangerDirector>();
            speedPerLevel = danger != null && danger.Config != null ? danger.Config.SpeedPerLevel : 0.08f;
        }

        private void Choose(RunState run)
        {
            AudioListener ear = VoiceListener.Current;
            if (ear == null) { leader = null; return; }
            Vector3 at = ear.transform.position;
            // The listener's floor: your own feet while alive, otherwise below the (ghost) ear.
            Networking.NetworkPlayer me = Networking.NetworkPlayer.Local;
            float feetY = me != null && !me.IsDead
                ? (me.Ragdoll.IsRagdolled ? me.Ragdoll.BodyPosition.y : me.transform.position.y)
                : at.y - EarHeight;
            float speedScale = 1f + run.State.Danger * speedPerLevel;

            gone.Clear();
            foreach (Threat t in tracks.Keys) if (t == null || !t.IsSpawned) gone.Add(t);
            foreach (Threat t in gone) tracks.Remove(t);

            Threat best = null;
            float bestScore = float.MinValue, bestEta = float.MaxValue;
            foreach (Threat t in Threat.All)
            {
                if (t == null || !t.IsSpawned || t.Definition == null) continue;
                if (!tracks.TryGetValue(t, out Track track)) tracks[t] = track = new Track { lastPosition = t.transform.position };
                Measure(t, track, at, feetY, speedScale);
                if (!track.active) continue;
                float score = t.Definition.HierarchyScore;
                if (score > bestScore || Mathf.Approximately(score, bestScore) && track.eta < bestEta)
                {
                    best = t;
                    bestScore = score;
                    bestEta = track.eta;
                }
            }
            leader = best;
            if (best == null) { targetVolume = 0f; return; }
            float closeness = Mathf.InverseLerp(StartSeconds, FullSeconds, bestEta);
            bool chasing = best.DesiredMotion == ThreatMotion.Chase || best.DesiredMotion == ThreatMotion.Attack;
            targetVolume = Mathf.Clamp01(Mathf.Lerp(QuietestShare, 1f, Mathf.Pow(closeness, 0.8f)) + (chasing ? ChaseBoost : 0f));
        }

        // Closing speed and time-to-reach for one monster, and whether its song may play.
        private void Measure(Threat t, Track track, Vector3 ear, float feetY, float speedScale)
        {
            Vector3 position = t.transform.position;
            float distance = Vector3.Distance(position, ear);
            float top = Mathf.Max(t.Definition.ChaseSpeed * speedScale, t.Definition.WalkSpeed, 0.5f);
            float dt = Time.time - track.lastTime;
            if (track.lastDistance >= 0f && dt > 0.01f)
            {
                track.closing = Mathf.Lerp(track.closing, (track.lastDistance - distance) / dt, 0.35f);
                // A fall or a snap of its network position isn't running speed: capped at twice its top speed.
                float seen = Mathf.Min((position - track.lastPosition).magnitude / dt, top * 2f);
                track.observedSpeed = Mathf.Max(track.observedSpeed * 0.98f, seen);
            }
            track.lastTime = Time.time;
            track.lastDistance = distance;
            track.lastPosition = position;
            // Coming toward you means the gap is actually shrinking; chasing someone else elsewhere doesn't count.
            if (track.closing > ClosingSpeed) track.lastApproach = Time.time;
            bool sameFloor = Mathf.Abs(position.y - feetY) <= SameFloor;
            // Its top speed (danger makes it faster); never slower than it has actually been seen moving.
            track.eta = distance / Mathf.Max(top, track.observedSpeed);
            bool approaching = Time.time - track.lastApproach <= (track.active ? Linger : Tick * 1.5f);
            // On at 15 s away while it comes for you on your floor; off a moment after it stops coming or leaves the floor.
            track.active = sameFloor && approaching && (track.active || track.eta <= StartSeconds);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible || RunState.Current == null) return;
            string text = leader != null && tracks.TryGetValue(leader, out Track t)
                ? $"MUSIC {leader.DisplayName} (rank {leader.Definition.HierarchyScore:0}) eta {t.eta:0.0}s vol {MusicPlayer.ThreatVolume:0.00}"
                : "MUSIC none";
            int counting = 0;
            foreach (Track track in tracks.Values) if (track.active) counting++;
            GUI.Label(new Rect(Screen.width - 360f, 96f, 350f, 22f), $"{text}  coming {counting}");
        }
#endif

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
