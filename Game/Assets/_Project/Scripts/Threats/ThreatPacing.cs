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
        private const float TickInterval = 1f, HintRefresh = 6f;

        private class Track { public float lastNear, lastClose, hintAt; public bool retreating; }

        private readonly Dictionary<Threat, Track> tracks = new();
        private readonly List<Threat> gone = new();
        private readonly List<NetworkPlayer> alive = new();
        private ThreatDirector director;
        private float nextTick;
        private System.Random random = new(7);
        private int drifting, retreating;
        // One path for every route question (made on first use: it can't be built in a field initialiser).
        private NavMeshPath routePath;

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
            retreating = 0;
            bool quiet = CrewDirector.Quiet;
            // A copy: relocating despawns and spawns while we walk the roster.
            foreach (Threat t in new List<Threat>(Threat.All))
            {
                if (t == null || !t.IsSpawned || !Paced(t)) continue;
                if (!tracks.TryGetValue(t, out Track track)) tracks[t] = track = new Track { lastNear = Time.time, lastClose = Time.time };
                if (quiet) { Retreat(t, track); continue; }
                if (track.retreating) { track.retreating = false; t.RoamHint = null; }
                float nearest = NearestRoute(t);
                if (nearest <= config.DriftRange) { track.lastNear = Time.time; t.RoamHint = null; }
                if (nearest <= config.RelocateDistance) track.lastClose = Time.time;

                if (Time.time - track.lastClose >= config.RelocateAfter && director != null)
                {
                    if (director.Relocate(t) != null) { tracks.Remove(t); continue; }
                    // Keep its history and retry soon if every fair marker is occupied.
                    track.lastClose = Time.time - config.RelocateAfter + config.SpawnRetry;
                }
                if (Time.time - track.lastNear < config.DriftAfter) continue;
                drifting++;
                if (t.RoamHint.HasValue && Time.time - track.hintAt < HintRefresh) continue;
                track.hintAt = Time.time;
                t.RoamHint = DriftPoint(t, config.DriftScatter);
            }
        }

        // 0.24.10, the Crew Director's quiet: no drift toward the crew and no relocation near it (their clocks
        // stand still), and each monster's wandering centres on a spawn point well away from everyone. The hint
        // only steers wandering, so a hunt or a fight already on ends on its own first, then it leaves.
        private void Retreat(Threat t, Track track)
        {
            track.lastNear = track.lastClose = Time.time;
            retreating++;
            if (track.retreating && t.RoamHint.HasValue) return;
            track.retreating = true;
            track.hintAt = Time.time;
            t.RoamHint = RetreatPoint(CrewDirectorConfig.Current.RetreatDistance);
        }

        private Vector3? RetreatPoint(float distance)
        {
            if (director == null || director.Points.Count == 0) return null;
            var far = new List<Vector3>();
            Vector3? farthest = null;
            float best = -1f;
            foreach (ThreatSpawnPoint point in director.Points)
            {
                if (point == null) continue;
                Vector3 at = point.transform.position;
                float nearest = float.MaxValue;
                foreach (NetworkPlayer p in alive) nearest = Mathf.Min(nearest, Vector3.Distance(at, p.transform.position));
                if (nearest >= distance) far.Add(at);
                if (nearest > best) { best = nearest; farthest = at; }
            }
            Vector3? pick = far.Count > 0 ? far[random.Next(far.Count)] : farthest;
            return pick is Vector3 v && NavMesh.SamplePosition(v, out NavMeshHit hit, 2f, NavMesh.AllAreas) ? hit.position : pick;
        }

        // Monsters with their own way of finding people keep it.
        private static bool Paced(Threat t) => !(t is Stalker) && !(t is Collector) && !(t is LastHunter);

        private float NearestRoute(Threat threat)
        {
            float best = float.MaxValue;
            NavMeshPath path = routePath ??= new NavMeshPath();
            foreach (NetworkPlayer p in alive)
            {
                if (!NavMesh.SamplePosition(p.transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas)
                    || Mathf.Abs(hit.position.y - p.transform.position.y) > 1.5f) continue;
                if (!NavMesh.CalculatePath(threat.transform.position, hit.position, NavMesh.AllAreas, path)
                    || path.status != NavMeshPathStatus.PathComplete) continue;
                float length = 0f;
                Vector3[] corners = path.corners;
                for (int i = 1; i < corners.Length; i++) length += Vector3.Distance(corners[i - 1], corners[i]);
                if (Mathf.Abs(threat.transform.position.y - hit.position.y) > 2.5f) length += 8f;
                best = Mathf.Min(best, length);
            }
            return best;
        }

        // Somewhere walkable around one living player, never right on top of them.
        private Vector3? DriftPoint(Threat threat, float scatter)
        {
            NavMeshPath path = routePath ??= new NavMeshPath();
            for (int i = 0; i < 12; i++)
            {
                NetworkPlayer p = alive[random.Next(alive.Count)];
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float distance = Mathf.Lerp(scatter * 0.4f, scatter, (float)random.NextDouble());
                Vector3 guess = p.transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
                if (NavMesh.SamplePosition(guess, out NavMeshHit hit, 1.75f, NavMesh.AllAreas)
                    && Mathf.Abs(hit.position.y - p.transform.position.y) < 1.5f
                    && NavMesh.CalculatePath(threat.transform.position, hit.position, NavMesh.AllAreas, path)
                    && path.status == NavMeshPathStatus.PathComplete) return hit.position;
            }
            return null;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible || director == null || !GameAuthority.IsHost || RunState.Current == null) return;
            float next = director.NextArrivalIn;
            GUI.Label(new Rect(Screen.width - 360f, 52f, 350f, 22f),
                $"THREATS {Contracts.Contract.ThreatName((byte)RunState.Current.Terms.ThreatLevel)} {director.Alive}/{director.AliveCap}  next {(next < 0f ? "opening" : next.ToString("0") + "s")}  drift {drifting} away {retreating} retries {director.PlacementRetries}");
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
