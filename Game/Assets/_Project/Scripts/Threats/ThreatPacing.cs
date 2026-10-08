using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Threats
{
    /// <summary>
    /// Host, beside the <see cref="ThreatDirector"/>: keeps monsters from idling forever in an empty wing.
    /// One left alone for <see cref="ThreatPacingConfig.DriftAfter"/> gets a drift point near a random living
    /// player (its wandering centres there; hunting rules are untouched). One far from everyone for
    /// <see cref="ThreatPacingConfig.RelocateAfter"/> is replaced near the crew, out of sight.
    /// Hunters that already track people (the Stalker, the Collector going for loot, the final hunter) are left alone.
    /// </summary>
    public class ThreatPacing : MonoBehaviour
    {
        private const float TickInterval = 1f, HintRefresh = 20f;

        private class Track { public float lastNear, lastClose, hintAt; }

        private readonly Dictionary<Threat, Track> tracks = new();
        private readonly List<Threat> gone = new();
        private readonly List<NetworkPlayer> alive = new();
        private ThreatDirector director;
        private float nextTick;
        private System.Random random = new(7);
        private int drifting;

        private void Awake() => director = GetComponent<ThreatDirector>();

        private void OnEnable() => RunDirector.RunStarted += OnRunStarted;
        private void OnDisable() => RunDirector.RunStarted -= OnRunStarted;

        private void OnRunStarted(int seed)
        {
            tracks.Clear();
            random = new System.Random(seed ^ 0x0D21F7);
        }

        private void Update()
        {
            if (!GameAuthority.IsHost || Time.time < nextTick) return;
            nextTick = Time.time + TickInterval;
            RunState run = RunState.Current;
            if (run == null || !run.IsSpawned || run.State.Phase != RunPhase.Running) return;
            alive.Clear();
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && p.IsSpawned && !p.IsDead) alive.Add(p);
            gone.Clear();
            foreach (Threat t in tracks.Keys) if (t == null || !t.IsSpawned) gone.Add(t);
            foreach (Threat t in gone) tracks.Remove(t);
            if (alive.Count == 0) return;

            ThreatPacingConfig config = ThreatPacingConfig.Current;
            drifting = 0;
            // A copy: relocating despawns and spawns while we walk the roster.
            foreach (Threat t in new List<Threat>(Threat.All))
            {
                if (t == null || !t.IsSpawned || !Paced(t)) continue;
                if (!tracks.TryGetValue(t, out Track track)) tracks[t] = track = new Track { lastNear = Time.time, lastClose = Time.time };
                float nearest = Nearest(t.transform.position);
                if (nearest <= config.DriftRange) { track.lastNear = Time.time; t.RoamHint = null; }
                if (nearest <= config.RelocateDistance) track.lastClose = Time.time;

                if (Time.time - track.lastClose >= config.RelocateAfter && director != null)
                {
                    tracks.Remove(t);
                    director.Relocate(t);
                    continue;
                }
                if (Time.time - track.lastNear < config.DriftAfter) continue;
                drifting++;
                if (t.RoamHint.HasValue && Time.time - track.hintAt < HintRefresh) continue;
                track.hintAt = Time.time;
                t.RoamHint = DriftPoint(config.DriftScatter);
            }
        }

        // Monsters with their own way of finding people keep it.
        private static bool Paced(Threat t) => !(t is Stalker) && !(t is Collector) && !(t is LastHunter);

        private float Nearest(Vector3 at)
        {
            float best = float.MaxValue;
            foreach (NetworkPlayer p in alive) best = Mathf.Min(best, Vector3.Distance(at, p.transform.position));
            return best;
        }

        // Somewhere walkable around one living player, never right on top of them.
        private Vector3? DriftPoint(float scatter)
        {
            NetworkPlayer p = alive[random.Next(alive.Count)];
            for (int i = 0; i < 6; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float distance = Mathf.Lerp(scatter * 0.4f, scatter, (float)random.NextDouble());
                Vector3 guess = p.transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
                if (NavMesh.SamplePosition(guess, out NavMeshHit hit, 4f, NavMesh.AllAreas)) return hit.position;
            }
            return null;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible || director == null || !GameAuthority.IsHost || RunState.Current == null) return;
            float next = director.NextArrivalIn;
            GUI.Label(new Rect(Screen.width - 360f, 52f, 350f, 22f),
                $"THREATS {director.Alive}/{director.AliveCap}  next {(next < 0f ? "opening" : next.ToString("0") + "s")}  drifting {drifting}");
        }
#endif

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            Gizmos.color = new Color(1f, 0.4f, 0.1f);
            foreach (Threat t in Threat.All)
                if (t != null && t.RoamHint is Vector3 hint)
                {
                    Gizmos.DrawLine(t.transform.position + Vector3.up, hint + Vector3.up);
                    Gizmos.DrawWireSphere(hint, 1f);
                }
        }
    }
}
