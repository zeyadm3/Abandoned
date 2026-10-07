using System.Linq;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Threats
{
    public enum CollectorState : byte { Idle, Seeking, Carrying, Fleeing }

    /// <summary>
    /// The Collector (GDD 9): never hurts anyone. It picks up loot nobody is near (not in the truck, not
    /// in anyone's hands) and hides it somewhere far away in the building; get close and it drops what
    /// it has and flees. Counterplay: guard the loot pile, carry things straight to the truck. Host only.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class Collector : Threat
    {
        [SerializeField] private CollectorConfig config;

        private readonly NetworkVariable<CollectorState> state = new();
        private NavMeshAgent agent;
        private NetworkLoot prey, carried;
        private Vector3 hideAt;
        private float nextTheft;

        public override string DisplayName => "Collector";
        public CollectorState State => state.Value;
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

        private void Update()
        {
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
            switch (state.Value)
            {
                case CollectorState.Idle:
                    if (Time.time >= nextTheft) Seek();
                    break;
                case CollectorState.Seeking:
                    if (prey == null || !prey.IsSpawned || prey.Hold.Mode != LootHoldMode.Free) { Set(CollectorState.Idle); break; }
                    agent.SetDestination(prey.transform.position);
                    if (Vector3.Distance(Flat(transform.position), Flat(prey.transform.position)) < 1.3f) PickUp();
                    break;
                case CollectorState.Carrying:
                    if (carried == null || !carried.IsSpawned || carried.Hold.Mode != LootHoldMode.Free) { carried = null; Set(CollectorState.Idle); break; }
                    HoldCarried();
                    if (!agent.pathPending && agent.remainingDistance < 1f)
                    {
                        Drop();
                        Stolen++;
                        nextTheft = Time.time + config.Cooldown;
                        Set(CollectorState.Idle);
                    }
                    break;
                case CollectorState.Fleeing:
                    if (!agent.pathPending && agent.remainingDistance < 1f)
                    {
                        nextTheft = Time.time + config.Cooldown;
                        Set(CollectorState.Idle);
                    }
                    break;
            }
        }

        private void Seek()
        {
            TruckCargo truck = TruckCargo.Current;
            prey = FindObjectsByType<NetworkLoot>(FindObjectsSortMode.None)
                .Where(l => l.IsSpawned && l.NetworkManager == NetworkManager && l.Hold.Mode == LootHoldMode.Free && !l.Item.IsShattered
                            && !l.Item.Definition.Utility && !l.Item.Definition.Jackpot && l.Item.Definition.CarryClass <= CarryClass.TwoHand
                            && (truck == null || !truck.ItemsInside().Contains(l.Item)) && Unattended(l.transform.position))
                .OrderBy(l => Vector3.Distance(l.transform.position, transform.position))
                .FirstOrDefault();
            if (prey == null)
            {
                nextTheft = Time.time + 3f;
                return;
            }
            agent.speed = config.Speed * SpeedScale;
            Set(CollectorState.Seeking);
        }

        private void PickUp()
        {
            carried = prey;
            prey = null;
            carried.Grabbable.Body.isKinematic = true;
            // Somewhere far from where it took it (and from the truck), on the NavMesh.
            hideAt = transform.position;
            for (int i = 0; i < 12; i++)
            {
                Vector3 candidate = transform.position + Random.insideUnitSphere * config.HideDistance * 1.6f;
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 4f, NavMesh.AllAreas)) continue;
                if (Vector3.Distance(hit.position, transform.position) < config.HideDistance) continue;
                hideAt = hit.position;
                break;
            }
            agent.SetDestination(hideAt);
            Set(CollectorState.Carrying);
            Debug.Log($"[Threat] The Collector took {carried.Item.Definition.DisplayName}.");
        }

        private void HoldCarried()
        {
            Vector3 hold = transform.position + transform.forward * 0.5f + Vector3.up * 1.1f;
            carried.Grabbable.Body.MovePosition(hold);
            carried.transform.position = hold;
        }

        private void Drop()
        {
            if (carried == null) return;
            if (carried.Grabbable != null && carried.Grabbable.Body != null) carried.Grabbable.Body.isKinematic = false;
            carried = null;
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
