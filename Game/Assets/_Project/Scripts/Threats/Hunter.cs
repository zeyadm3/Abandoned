using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Threats
{
    /// <summary>
    /// The Hunter (GDD 9): patrols, chases whoever it sees, kills on contact. Counterplay: break line of
    /// sight, crouch to hide, outrun it while your stamina lasts, or lure it onto a weak floor: it's
    /// heavy (a load source on the structure), and when its floor gives way it falls and lies stunned.
    /// 0.16.0: a shut door stops it seeing you, and it breaks down one shut between it and where it last
    /// saw you (a few loud blows); a locker hides you unless it watched you climb in.
    /// Host only; its state and heavy footsteps reach every machine.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public partial class Hunter : Threat, ILoadSource
    {
        [SerializeField] private HunterConfig config;

        private readonly NetworkVariable<HunterState> state = new();
        private NavMeshAgent agent;
        private NetworkPlayer target;
        private Vector3 lastSeen;
        private float lostAt, searchUntil, stunnedUntil, nextStep, fallSpeed, nextPatrolPick;
        private bool falling;
        private Vector3 lastFooting;
        private StructuralSection standingOn;

        public override string DisplayName => Loc.Mark("Hunter");
        public override string DeathLine => Loc.Mark("The Hunter ran you down.");
        public HunterState State => state.Value;
        // At a door it stands and swings (each blow plays the attack); on its way to one it is still charging.
        public override ThreatMotion DesiredMotion => State == HunterState.Chase || State == HunterState.Breach && !atDoor ? ThreatMotion.Chase
            : State == HunterState.Stunned ? ThreatMotion.Special : ThreatMotion.Idle;
        public NetworkPlayer Target => target;
        public HunterConfig Config => config;
        /// <summary>Tests/F1: the section it last stood on.</summary>
        public StructuralSection StandingOn => standingOn;

        public float LoadWeight => IsServer && IsSpawned && !falling ? config.Weight : 0f;

        public void GetLoadPoints(List<LoadPoint> points) => points.Add(new LoadPoint(transform.position + Vector3.up * 0.2f));

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
            agent.enabled = true;
            agent.Warp(transform.position);
            LoadSources.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            LoadSources.Unregister(this);
        }

        private void Update()
        {
            if (IsSpawned) Footsteps();
            if (!IsServer || !IsSpawned) return;
            if (falling || state.Value == HunterState.Stunned) { Fall(); return; }
            if (FloorGaveWay()) { StartFall(); return; }
            if (!agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position + Vector3.down * 4f, out NavMeshHit hit, 5f, NavMesh.AllAreas)) agent.Warp(hit.position);
                return;
            }
            bool active = RunState.Current == null || RunState.Current.State.Phase != RunPhase.Departed;
            if (!active) { agent.isStopped = true; return; }

            if (TickWithdrawal(agent, config.PatrolSpeed, State == HunterState.Chase || State == HunterState.Breach || State == HunterState.Search, lastSeen))
            {
                target = null;
                breaching = null;
                atDoor = false;
                Set(HunterState.Patrol);
                return;
            }

            NetworkPlayer seen = Spotted();
            if (seen != null)
            {
                target = seen;
                lastSeen = SpotOf(seen);
                lostAt = Time.time;
                if (state.Value != HunterState.Breach) Set(HunterState.Chase);
            }
            switch (state.Value)
            {
                case HunterState.Chase:
                case HunterState.Breach:
                    agent.speed = config.ChaseSpeed * SpeedScale;
                    if (AtShutDoor())
                    {
                        Set(HunterState.Breach);
                        break;
                    }
                    Set(HunterState.Chase);
                    agent.isStopped = false;
                    Steer(agent, lastSeen);
                    if (KillWithinReach(config.AttackRange) != null) { target = null; SearchFrom(Time.time); }
                    else if (seen == null && Time.time - lostAt > Mathf.Min(config.LoseAfter * Mathf.Min(Aggression, 1.8f),
                        ThreatPacingConfig.Current.LostSightSearchSeconds * 0.5f)) SearchFrom(lostAt);
                    break;
                case HunterState.Search:
                    agent.isStopped = false;
                    agent.speed = config.PatrolSpeed * SpeedScale;
                    if (!agent.pathPending && agent.remainingDistance < 1f) transform.Rotate(0f, 90f * Time.deltaTime, 0f); // looking around
                    Steer(agent, lastSeen);
                    if (Time.time > searchUntil)
                    {
                        target = null;
                        breaching = null;
                        atDoor = false;
                        BeginWithdrawal(agent, lastSeen);
                        Set(HunterState.Patrol);
                        return;
                    }
                    break;
                default:
                    agent.isStopped = false;
                    agent.speed = config.PatrolSpeed * SpeedScale;
                    if (!agent.pathPending && (agent.remainingDistance < 1f || !agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete)) PatrolSomewhere();
                    break;
            }
            // Walking into it is fatal too, not only being chased down.
            if (state.Value != HunterState.Chase) KillWithinReach(config.AttackRange);
        }

        private void Set(HunterState s)
        {
            if (state.Value != s) state.Value = s;
        }

        private void SearchFrom(float trailTime)
        {
            Set(HunterState.Search);
            searchUntil = Mathf.Min(Time.time + config.SearchSeconds, trailTime + ThreatPacingConfig.Current.LostSightSearchSeconds);
        }

        // The living player it sees best: in its cone, in range (crouchers only up close), in the open; or anyone right next to it.
        private NetworkPlayer Spotted()
        {
            NetworkPlayer best = null;
            float bestDistance = float.MaxValue;
            Vector3 eye = transform.position + Vector3.up * 2f;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.NetworkManager != NetworkManager || p.IsDead || !p.IsSpawned || Sheltered(PositionOf(p))) continue;
                if (p.IsConcealed)
                {
                    // In a locker: only if it watched them climb in, and then it knows exactly where they are.
                    if (p.InVent || !FindsHider(p)) continue;
                    float away = Vector3.Distance(transform.position, SpotOf(p));
                    if (away < bestDistance) { best = p; bestDistance = away; }
                    continue;
                }
                Vector3 at = p.SightPoint;
                Vector3 to = at - eye;
                float distance = to.magnitude;
                bool close = distance < config.CloseSense;
                float range = (p.State.Crouching ? config.CrouchSightRange : config.SightRange) * Mathf.Min(Aggression, 1.4f);
                if (!close && (distance > range || Vector3.Angle(transform.forward, new Vector3(to.x, 0f, to.z)) > config.SightAngle / 2f)) continue;
                if (!LineOfSight(eye, at)) continue;
                RememberHider(p);
                if (distance < bestDistance) { best = p; bestDistance = distance; }
            }
            return best;
        }

        private void PatrolSomewhere()
        {
            if (Time.time < nextPatrolPick) return;
            nextPatrolPick = Time.time + 0.75f;
            for (int i = 0; i < 8; i++)
            {
                Vector2 r = Random.insideUnitCircle * config.PatrolRadius;
                Vector3 guess = RoamCentre + new Vector3(r.x, 0f, r.y);
                if (ReachableGoal(agent, guess, out Vector3 destination))
                {
                    Steer(agent, destination);
                    return;
                }
            }
        }

        // The section it stood on collapsed. Checked on the section, not by looking down: the collapse carves
        // the NavMesh at once and the agent gets pushed out of the hole onto the next tile in the same frame.
        private bool FloorGaveWay()
        {
            if (standingOn != null && standingOn.IsCollapsed) return true;
            StructuralSection under = SectionQuery.Under(transform.position, 1f);
            if (under != null)
            {
                standingOn = under;
                lastFooting = transform.position;
            }
            return false;
        }

        private void StartFall()
        {
            // Back over the hole (the NavMesh may already have shoved it onto the neighbouring tile).
            transform.position = lastFooting;
            standingOn = null;
            falling = true;
            fallSpeed = 0f;
            agent.enabled = false;
            target = null;
            Set(HunterState.Stunned);
            stunnedUntil = Time.time + config.StunSeconds;
            Debug.Log("[Threat] The Hunter fell through the floor.");
        }

        // Down to whatever is below, then lie there; then find the NavMesh again and patrol.
        private void Fall()
        {
            if (falling)
            {
                fallSpeed = Mathf.Min(fallSpeed + 20f * Time.deltaTime, 30f);
                float step = fallSpeed * Time.deltaTime;
                if (Physics.Raycast(transform.position + Vector3.up * 0.3f, Vector3.down, out RaycastHit hit, step + 0.3f, WallMask, QueryTriggerInteraction.Ignore))
                {
                    transform.position = hit.point;
                    falling = false;
                    stunnedUntil = Time.time + config.StunSeconds;
                    standingOn = hit.collider.GetComponentInParent<StructuralSection>();
                    CameraShake.Emit(hit.point, config.Weight * fallSpeed);
                    LandedRpc(hit.point);
                }
                else transform.position += Vector3.down * step;
                // Nothing below at all (out of the world): it's gone, rather than falling forever.
                if (transform.position.y < -30f) NetworkObject.Despawn(true);
                return;
            }
            if (Time.time < stunnedUntil) return;
            agent.enabled = true;
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit nav, 3f, NavMesh.AllAreas)) agent.Warp(nav.position);
            lastSeen = transform.position;
            SearchFrom(Time.time);
        }

        [Rpc(SendTo.Everyone)]
        private void LandedRpc(Vector3 at)
        {
            Audio.GameAudio.Play(Audio.SoundId.HunterLanding, at, 1f);
            // Seen it happen (near enough): the "lure it onto a weak floor" achievement.
            Camera ear = Camera.main;
            if (ear != null && Vector3.Distance(ear.transform.position, at) < 25f) Achievements.Increment(Achievements.StatHunterFalls);
        }

        // Every machine: heavy steps, quicker when it charges (its signature, GDD 9: one clear rule).
        private void Footsteps()
        {
            HunterState s = state.Value;
            // At a door the blows speak for it.
            if (s == HunterState.Stunned || s == HunterState.Breach || Time.time < nextStep) return;
            nextStep = Time.time + (s == HunterState.Chase ? 0.32f : 0.62f);
            Audio.GameAudio.Play(Audio.SoundId.HunterStep, transform.position, s == HunterState.Chase ? 1f : 0.75f);
        }

#if UNITY_EDITOR
        public void EditorSetup(HunterConfig hunterConfig) => config = hunterConfig;
#endif
    }
}
