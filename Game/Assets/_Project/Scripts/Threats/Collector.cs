using System.Collections.Generic;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Threats
{
    public enum CollectorState : byte { Idle, Seeking, Carrying, Fleeing }

    /// <summary>
    /// The Collector (GDD 9): never hurts anyone. It picks up loot nobody is near (not in the truck, not
    /// in anyone's hands) and takes it to its nest, one place far from the truck for the whole run, where
    /// the hoard tinkles now and then (QA D-08: stolen loot can be found again). Get close and it drops what
    /// it has and flees. Counterplay: guard the loot pile, carry things straight to the truck, raid the nest.
    /// Host only; the nest's place and size are replicated so everyone can hear it.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class Collector : Threat
    {
        [SerializeField] private CollectorConfig config;

        private readonly NetworkVariable<CollectorState> state = new();
        // Where the hoard is and how much it has taken there (0 = no nest yet).
        private readonly NetworkVariable<Vector3> nest = new();
        private readonly NetworkVariable<byte> hoard = new();
        private float nextNestSound;
        private NavMeshAgent agent;
        private NetworkLoot prey, carried;
        private Vector3 hideAt;
        private float nextTheft, seekUntil;
        // Items it couldn't get to (a collapsed island, past the NavMesh): not tried again this run.
        private readonly HashSet<NetworkLoot> unreachable = new();

        public override string DisplayName => "Collector";
        public CollectorState State => state.Value;
        public override ThreatMotion DesiredMotion => State == CollectorState.Carrying ? ThreatMotion.Special : State == CollectorState.Fleeing ? ThreatMotion.Chase : ThreatMotion.Idle;
        public NetworkLoot Carried => carried;
        public int Stolen { get; private set; }

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
        }

        public override void OnNetworkDespawn()
        {
            Drop();
            base.OnNetworkDespawn();
        }

        private float nextJingle;

        private void Update()
        {
            // Every machine: a loaded Collector jingles as it runs, so you hear your loot leaving.
            if (IsSpawned && state.Value == CollectorState.Carrying && Time.time >= nextJingle)
            {
                nextJingle = Time.time + 0.8f;
                Audio.GameAudio.Play(Audio.SoundId.CollectorJingle, transform.position + Vector3.up, 0.8f);
            }
            // Every machine: the hoard gives itself away with a quiet jingle every so often.
            if (IsSpawned && hoard.Value > 0 && Time.time >= nextNestSound)
            {
                nextNestSound = Time.time + Random.Range(6f, 10f);
                Audio.GameAudio.Play(Audio.SoundId.CollectorJingle, nest.Value + Vector3.up * 0.5f, 0.45f);
            }
            if (!IsServer || !IsSpawned) return;
            if (!agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position + Vector3.down * 4f, out NavMeshHit hit, 5f, NavMesh.AllAreas)) agent.Warp(hit.position);
                return;
            }
            if (RunState.Current != null && RunState.Current.State.Phase == RunPhase.Departed) return;

            if (state.Value != CollectorState.Fleeing && NearestPlayer() < config.ScareRadius)
            {
                Drop();
                Flee();
                return;
            }
            agent.speed = (state.Value == CollectorState.Fleeing ? config.FleeSpeed : config.Speed) * SpeedScale;
            switch (state.Value)
            {
                case CollectorState.Idle:
                    if (Time.time >= nextTheft) Seek();
                    break;
                case CollectorState.Seeking:
                    if (prey == null || !prey.IsSpawned || prey.Hold.Mode != LootHoldMode.Free) { Set(CollectorState.Idle); break; }
                    Steer(agent, prey.transform.position);
                    if (Time.time > seekUntil || (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathInvalid))
                    {
                        unreachable.Add(prey);
                        prey = null;
                        nextTheft = Time.time + 1f;
                        Set(CollectorState.Idle);
                        break;
                    }
                    // Near it on the same floor: a path that runs under an item upstairs mustn't grab it through the ceiling.
                    Vector3 offset = prey.transform.position - transform.position;
                    if (Flat(offset).magnitude < 1.3f && Mathf.Abs(offset.y) < 1.5f) PickUp();
                    break;
                case CollectorState.Carrying:
                    if (carried == null || !carried.IsSpawned || carried.Hold.Mode != LootHoldMode.Free) { Drop(); Set(CollectorState.Idle); break; }
                    HoldCarried();
                    if (!agent.pathPending && agent.remainingDistance < 1f)
                    {
                        Drop();
                        Stolen++;
                        if (hoard.Value < byte.MaxValue) hoard.Value++;
                        nextTheft = Time.time + config.Cooldown / Mathf.Min(Aggression, 1.7f);
                        Set(CollectorState.Idle);
                    }
                    break;
                case CollectorState.Fleeing:
                    if (!agent.pathPending && agent.remainingDistance < 1f)
                    {
                        nextTheft = Time.time + config.Cooldown / Mathf.Min(Aggression, 1.7f);
                        Set(CollectorState.Idle);
                    }
                    break;
            }
        }

        private void Seek()
        {
            TruckCargo truck = TruckCargo.Current;
            // QA P-02: the bay is checked once per search, not once per item in the building.
            var inTruck = truck != null ? new HashSet<LootItem>(truck.ItemsInside()) : null;
            Vector3 home = HasNest ? nest.Value : Vector3.positiveInfinity;
            prey = FindObjectsByType<NetworkLoot>(FindObjectsSortMode.None)
                .Where(l => l.IsSpawned && l.NetworkManager == NetworkManager && l.Hold.Mode == LootHoldMode.Free && !l.Item.IsShattered
                            && !unreachable.Contains(l) && !l.Item.Definition.Utility && !l.Item.Definition.Jackpot && l.Item.Definition.CarryClass <= CarryClass.TwoHand
                            && (inTruck == null || !inTruck.Contains(l.Item)) && Unattended(l.transform.position)
                            && (l.transform.position - home).sqrMagnitude > NestRadius * NestRadius * 4f)
                .OrderBy(l => Vector3.Distance(l.transform.position, transform.position))
                .FirstOrDefault();
            if (prey == null)
            {
                nextTheft = Time.time + 3f;
                return;
            }
            agent.speed = config.Speed * SpeedScale;
            seekUntil = Time.time + config.SeekTimeout;
            Set(CollectorState.Seeking);
        }

        private void PickUp()
        {
            carried = prey;
            prey = null;
            carried.Grabbable.SetExternallyHeld(true);
            if (!HasNest) ChooseNest();
            // Spread around the nest so the hoard isn't one stack.
            hideAt = nest.Value;
            Vector2 spread = Random.insideUnitCircle * NestRadius;
            if (NavMesh.SamplePosition(nest.Value + new Vector3(spread.x, 0f, spread.y), out NavMeshHit spot, NestRadius, NavMesh.AllAreas))
                hideAt = spot.position;
            agent.SetDestination(hideAt);
            Set(CollectorState.Carrying);
            Debug.Log($"[Threat] The Collector took {carried.Item.Definition.DisplayName}.");
        }

        private const float NestRadius = 1.5f;

        private bool HasNest => hoard.Value > 0 || nest.Value != Vector3.zero;

        // Once per run: somewhere reachable, far from where it is and as far from the truck as it can find.
        private void ChooseNest()
        {
            Vector3 truck = TruckCargo.Current != null ? TruckCargo.Current.transform.position : transform.position;
            Vector3 best = transform.position;
            float bestScore = float.MinValue;
            for (int i = 0; i < 16; i++)
            {
                Vector3 candidate = transform.position + Random.insideUnitSphere * config.HideDistance * 1.6f;
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 4f, NavMesh.AllAreas)) continue;
                if (Vector3.Distance(hit.position, transform.position) < config.HideDistance) continue;
                float score = Vector3.Distance(hit.position, truck);
                if (score > bestScore) (best, bestScore) = (hit.position, score);
            }
            nest.Value = best == Vector3.zero ? best + Vector3.up * 0.01f : best;
            Debug.Log($"[Threat] The Collector's nest is at {nest.Value}.");
        }

        private void HoldCarried()
        {
            Vector3 hold = transform.position + transform.forward * 0.5f + Vector3.up * 1.1f;
            carried.Grabbable.Body.MovePosition(hold);
            carried.transform.position = hold;
        }

        // Set down, not dropped from hand height: a fall of a metre shatters a glass sculpture (and the
        // bang would call the Blind One). In front of it where that's clear, else at its feet (on the
        // NavMesh, so never inside a wall).
        private void Drop()
        {
            if (carried == null) return;
            NetworkLoot item = carried;
            carried = null;
            if (item == null || item.Grabbable == null) return;
            Grabbable g = item.Grabbable;
            if (item.IsSpawned && item.Hold.Mode == LootHoldMode.Free && g.HasPhysicsAuthority)
            {
                Bounds bounds = g.GetBounds();
                Vector3 feet = transform.position + Vector3.up * (bounds.extents.y + 0.03f);
                Vector3 front = feet + Flat(transform.forward).normalized * (agent.radius + Mathf.Max(bounds.extents.x, bounds.extents.z) + 0.05f);
                bool clear = LineOfSight(feet, front) &&
                             !Physics.CheckBox(front, bounds.extents * 0.95f, Quaternion.identity, WallMask, QueryTriggerInteraction.Ignore);
                g.transform.position = clear ? front : feet;
                g.SnapBodyToTransform();
            }
            g.SetExternallyHeld(false);
            if (!g.Body.isKinematic) g.Body.linearVelocity = Vector3.zero;
        }

        private void Flee()
        {
            Vector3 from = transform.position;
            Vector3 away = transform.position - NearestPlayerPosition();
            Vector3 target = from + Flat(away).normalized * config.HideDistance;
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, 6f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
            agent.speed = config.FleeSpeed * SpeedScale;
            Set(CollectorState.Fleeing);
        }

        private bool Unattended(Vector3 at) =>
            !NetworkPlayer.All.Any(p => p != null && p.NetworkManager == NetworkManager && !p.IsDead && Vector3.Distance(PositionOf(p), at) < config.GuardRadius);

        private float NearestPlayer() => Vector3.Distance(NearestPlayerPosition(), transform.position);

        private Vector3 NearestPlayerPosition()
        {
            Vector3 best = transform.position + Vector3.one * 1000f;
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && p.NetworkManager == NetworkManager && !p.IsDead && Vector3.Distance(PositionOf(p), transform.position) < Vector3.Distance(best, transform.position))
                    best = PositionOf(p);
            return best;
        }

        private void Set(CollectorState s)
        {
            if (state.Value != s) state.Value = s;
        }

        private static Vector3 Flat(Vector3 v) => new(v.x, 0f, v.z);
    }
}
