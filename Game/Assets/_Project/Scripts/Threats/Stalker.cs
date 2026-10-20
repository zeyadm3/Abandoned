using Abandoned.Core;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Threats
{
    /// <summary>
    /// The Stalker (GDD 9): follows the most isolated player at a distance and stares; while that player
    /// looks at it, it doesn't move; if they're alone and look away too long, it rushes and kills on
    /// contact. Counterplay: stay grouped, keep checking behind you. Host only; state replicated.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class Stalker : Threat
    {
        [SerializeField] private StalkerConfig config;

        private readonly NetworkVariable<StalkerState> state = new();
        private NavMeshAgent agent;
        private StalkerBrain brain;
        private NetworkPlayer target;
        private float nextRetarget, nextBreath, nextHalt, lastSightAt;
        private Vector3 lastKnown;
        // Replicated so the prey's own machine can warn them (2026-10-09: testers never understood it): who it
        // is after, and how close to a rush it is in sixteenths (changes a couple of times a second, not every frame).
        private const int DreadSteps = 16;
        private readonly NetworkVariable<ulong> prey = new(ulong.MaxValue);
        private readonly NetworkVariable<byte> dread = new();
        private StalkerState heardState;

        public override string DisplayName => Loc.Mark("Stalker");
        public override string DeathLine => Loc.Mark("You looked away from the Stalker.");
        public StalkerState State => state.Value;
        public override ThreatMotion DesiredMotion => State == StalkerState.Frozen ? ThreatMotion.Special : State == StalkerState.Rush ? ThreatMotion.Chase : ThreatMotion.Idle;
        public NetworkPlayer Target => target;
        /// <summary>Every machine: 0 = calm, 1 = about to rush (or rushing).</summary>
        public float Dread01 => state.Value == StalkerState.Rush ? 1f : dread.Value / (float)DreadSteps;
        public bool Hunts(NetworkPlayer p) => p != null && prey.Value == p.OwnerClientId;

        /// <summary>Every machine: the Stalker working up to a rush on this player (the nearest to rushing), or null.</summary>
        public static Stalker PressingOn(NetworkPlayer p)
        {
            Stalker worst = null;
            foreach (Threat t in All)
                if (t is Stalker s && s.IsSpawned && s.Hunts(p) && (worst == null || s.Dread01 > worst.Dread01)) worst = s;
            return worst;
        }
        public StalkerBrain Brain => brain;

        protected override void Awake()
        {
            base.Awake();
            agent = GetComponent<NavMeshAgent>();
            agent.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;
            brain = new StalkerBrain(config);
            agent.enabled = true;
            agent.Warp(transform.position);
        }

        private void Update()
        {
            if (IsSpawned) Breathe();
            if (!IsServer || !IsSpawned || brain == null) return;
            if (!agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position + Vector3.down * 4f, out NavMeshHit hit, 5f, NavMesh.AllAreas)) agent.Warp(hit.position);
                return;
            }
            bool active = RunState.Current == null || RunState.Current.State.Phase != RunPhase.Departed;
            if (!active) { agent.isStopped = true; return; }
            if (TickWithdrawal(agent, config.FollowSpeed, target != null, lastKnown))
            {
                ClearPrey();
                return;
            }
            if (target != null && (!target.IsSpawned || target.IsDead || Sheltered(PositionOf(target))))
            {
                LeaveEncounter();
                return;
            }
            if (Time.time >= nextRetarget || target == null) PickTarget();
            if (target == null)
            {
                ClearPrey();
                IdleRoam(agent, config.FollowSpeed, config.WatchRange);
                return;
            }

            bool visible = !HiddenFromMe(target) && Sees(target);
            if (visible) { lastKnown = SpotOf(target); lastSightAt = Time.time; }
            else
            {
                // A wall or an unwitnessed hiding place stops live tracking. Check the last lead,
                // then actually leave instead of standing outside the locker until the player exits.
                brain.Reset();
                state.Value = StalkerState.Follow;
                dread.Value = 0;
                if (Time.time - lastSightAt >= ThreatPacingConfig.Current.LostSightSearchSeconds)
                {
                    LeaveEncounter();
                    return;
                }
                agent.isStopped = false;
                agent.speed = config.FollowSpeed * SpeedScale;
                Steer(agent, lastKnown);
                return;
            }

            // A prey it watched climb into a locker is reached at the locker's door.
            Vector3 at = lastKnown;
            brain.Config = config;
            // Only a visible, isolated prey builds dread; a lost trail is handled by the bounded search above.
            brain.Tick(IsWatchedBy(target), IsIsolated(target), Time.deltaTime * Aggression);
            state.Value = brain.State;
            byte build = (byte)Mathf.RoundToInt(Mathf.Clamp01(brain.Dread / config.RushAfter) * DreadSteps);
            if (dread.Value != build) dread.Value = build;
            if (prey.Value != target.OwnerClientId) prey.Value = target.OwnerClientId;
            switch (brain.State)
            {
                case StalkerState.Frozen:
                    agent.isStopped = true;
                    break;
                case StalkerState.Follow:
                    agent.isStopped = false;
                    agent.speed = config.FollowSpeed * SpeedScale;
                    // Hang back: walk to a point FollowDistance short of them.
                    Vector3 away = Flat(transform.position - at).normalized;
                    if (away.sqrMagnitude < 0.01f) away = -target.transform.forward;
                    if (Flat(transform.position - at).magnitude <= config.FollowDistance) agent.isStopped = true;
                    else Steer(agent, at + away * config.FollowDistance);
                    break;
                case StalkerState.Rush:
                    agent.isStopped = false;
                    agent.speed = config.RushSpeed * SpeedScale;
                    Steer(agent, at);
                    // Prey it can't reach (at the truck, off the NavMesh): give up rather than camp the doorway.
                    if (!agent.pathPending && agent.pathStatus != NavMeshPathStatus.PathComplete) LeaveEncounter();
                    else if (KillWithinReach(config.AttackRange) != null) LeaveEncounter();
                    break;
            }
            // Frozen, it keeps staring at its target (moving, the agent turns it).
            if (brain.State == StalkerState.Frozen && Flat(at - transform.position).sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(Flat(at - transform.position));
        }

        private void ClearPrey()
        {
            target = null;
            brain.Reset();
            state.Value = StalkerState.Follow;
            dread.Value = 0;
            prey.Value = ulong.MaxValue;
        }

        private void LeaveEncounter()
        {
            BeginWithdrawal(agent, lastKnown);
            ClearPrey();
        }

        // Isolation chooses among players it can actually perceive, never the entire building.
        private void PickTarget()
        {
            nextRetarget = Time.time + config.RetargetInterval;
            var alive = NetworkPlayer.All.Where(p => p != null && p.IsSpawned && p.NetworkManager == NetworkManager && !p.IsDead && !Sheltered(PositionOf(p))).ToList();
            // Two players are always equally far from each other: then the nearer one to it.
            NetworkPlayer picked = alive.Where(p => !HiddenFromMe(p) && Sees(p))
                .OrderByDescending(p => alive.Where(o => o != p).Select(o => Vector3.Distance(PositionOf(o), PositionOf(p))).DefaultIfEmpty(float.MaxValue).Min())
                .ThenBy(p => Vector3.Distance(PositionOf(p), transform.position))
                .FirstOrDefault();
            // A new prey hasn't been stalked yet: the dread built on someone else doesn't carry over.
            if (picked != null)
            {
                if (picked != target) brain.Reset();
                target = picked;
            }
        }

        private bool Sees(NetworkPlayer p) => Vector3.Distance(transform.position, SpotOf(p)) <= config.WatchRange
            && (p.IsConcealed && FindsHider(p) || LineOfSight(transform.position + Vector3.up * 1.5f, p.SightPoint));

        // Already rushing this player: it can watch them climb into a locker, and then it reaches in (0.16.0).
        protected override bool Pursuing(NetworkPlayer p) => target == p && brain != null && brain.State == StalkerState.Rush;

        private bool IsIsolated(NetworkPlayer p) =>
            !NetworkPlayer.All.Any(o => o != null && o != p && o.NetworkManager == NetworkManager && !o.IsDead
                                        && Vector3.Distance(PositionOf(o), PositionOf(p)) < config.IsolationRadius);

        // Watching = facing it (horizontally) within the angle, near enough, with nothing in between.
        public bool IsWatchedBy(NetworkPlayer p)
        {
            Vector3 eye = PositionOf(p) + Vector3.up * 1.6f, chest = transform.position + Vector3.up * 1.5f;
            Vector3 to = chest - eye;
            if (to.magnitude > config.WatchRange) return false;
            // Right on top of you there is no direction to look in (Vector3.Angle of a zero vector is 0, which read as
            // "watched"): after a strike it froze inside its prey for as long as they stood still.
            if (Flat(to).sqrMagnitude < 0.09f) return false;
            if (Vector3.Angle(Flat(p.transform.forward), Flat(to)) > config.WatchAngle) return false;
            return LineOfSight(eye, chest);
        }

        private static Vector3 Flat(Vector3 v) => new(v.x, 0f, v.z);

        // Every machine: a slow breath when it's close behind you. Its own breath, never the structural creak:
        // a creak must only ever mean the floor (QA B-14).
        private void Breathe()
        {
            StalkerState now = state.Value;
            if (now != heardState)
            {
                // The two moments worth hearing: it breaks into a rush, and it stops dead because someone looked.
                if (now == StalkerState.Rush)
                    Audio.GameAudio.Play(Audio.SoundId.Scream, transform.position + Vector3.up * 1.8f, 0.85f, 40f);
                else if (now == StalkerState.Frozen && Time.time >= nextHalt)
                {
                    nextHalt = Time.time + 3f;
                    Audio.GameAudio.Play(Audio.SoundId.Knock, transform.position + Vector3.up * 1.2f, 0.5f, 30f);
                }
                heardState = now;
            }
            if (Time.time < nextBreath) return;
            // The breathing quickens and swells as it works up to a rush.
            float worked = Dread01;
            nextBreath = Time.time + Mathf.Lerp(2.2f, 0.6f, worked);
            Audio.GameAudio.Play(Audio.SoundId.StalkerBreath, transform.position + Vector3.up * 1.8f, Mathf.Lerp(0.35f, 0.8f, worked), 18f);
        }
    }
}
