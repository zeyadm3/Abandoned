using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Extraction;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// What happens on the road between places (decision 4), on the van's NetworkObject. The host, while the van is being
    /// driven at speed and well away from every place, rolls a road event every so often, placed on the road ahead:
    /// a <b>monster</b> waiting on it (<see cref="RoadMonster"/>), a <b>hazard</b> across it (a fallen tree or a wreck,
    /// <see cref="RoadHazard"/>), or a <b>jump scare</b> (a figure by the roadside and a scream, on each machine,
    /// cosmetic; skipped for players who asked for fewer scares).
    /// </summary>
    public class RoadEventDirector : NetworkBehaviour
    {
        public enum RoadEvent { Monster, Hazard, JumpScare }

        [SerializeField] private WorldVan van;
        [Tooltip("Seconds between road events (random in this range).")]
        [SerializeField] private Vector2 every = new(45f, 120f);
        [SerializeField, Range(0f, 1f)] private float monsterWeight = .35f;
        [SerializeField, Range(0f, 1f)] private float hazardWeight = .3f;
        [SerializeField, Range(0f, 1f)] private float jumpScareWeight = .35f;
        [Tooltip("How far ahead of the van (m, along the road) an event is placed.")]
        [SerializeField] private Vector2 ahead = new(110f, 170f);
        [Tooltip("No events this close (m) to any place's ground: the places have their own.")]
        [SerializeField] private float clearOfPlaces = 300f;
        [SerializeField] private float minSpeedKmh = 20f;
        [SerializeField] private NetworkObject[] hazards = new NetworkObject[0];
        [SerializeField] private NetworkObject monster;
        [Tooltip("The figure a jump scare shows (the horror director's).")]
        [SerializeField] private GameObject scareModel;

        /// <summary>Every machine: an event happened (kind, where, in game coordinates).</summary>
        public static event System.Action<RoadEvent, Vector3> Triggered;

        public float NextIn => Mathf.Max(0f, next - Time.time);
        public RoadEvent? Last { get; private set; }

        private float next;
        private readonly List<NetworkObject> spawned = new();

        public override void OnNetworkSpawn()
        {
            if (van == null) van = GetComponent<WorldVan>();
            next = Time.time + Random.Range(every.x, every.y);
        }

        private void Update()
        {
            if (!IsSpawned || !IsServer || WorldState.Current == null || van == null) return;
            if (Time.time < next || !van.Seats.HasDriver || van.Drive.SpeedKmh < minSpeedKmh) return;
            WorldState state = WorldState.Current;
            for (int i = 0; i < state.Atlas.Pois.Count; i++)
                if (WorldJob.VanFrom(i) < clearOfPlaces) return;
            Vector3 vanWorld = state.ToWorld(van.transform.position);
            float distance = Random.Range(ahead.x, ahead.y);
            if (!state.Atlas.RoadAhead(vanWorld, van.transform.forward, distance, 40f, out Vector3 point, out Vector3 along)) return;
            next = Time.time + Random.Range(every.x, every.y);
            float total = monsterWeight + hazardWeight + jumpScareWeight, pick = Random.value * total;
            RoadEvent e = pick < monsterWeight ? RoadEvent.Monster : pick < monsterWeight + hazardWeight ? RoadEvent.Hazard : RoadEvent.JumpScare;
            Vector3 at = Ground(state.ToEngine(point));
            Quaternion facing = Quaternion.LookRotation(-along);
            switch (e)
            {
                case RoadEvent.Monster when monster != null:
                    Spawn(monster, at, facing);
                    break;
                case RoadEvent.Hazard when hazards.Length > 0:
                    // Across the road, a little off centre, so there is room to squeeze past on the verge.
                    Vector3 side = Vector3.Cross(Vector3.up, along) * Random.Range(-1.5f, 1.5f);
                    Spawn(hazards[Random.Range(0, hazards.Length)], Ground(at + side), Quaternion.LookRotation(Vector3.Cross(Vector3.up, along)) * Quaternion.Euler(0f, Random.Range(-15f, 15f), 0f));
                    break;
                default:
                    e = RoadEvent.JumpScare;
                    ScareRpc(at + Vector3.Cross(Vector3.up, along) * Random.Range(5f, 9f) * (Random.value < .5f ? -1f : 1f), Random.Range(0, 360));
                    break;
            }
            Last = e;
            Debug.Log($"[Road] {e} {distance:0} m ahead of the van at world {point:F0}.");
            if (e != RoadEvent.JumpScare) TriggeredRpc((int)e, at);
        }

        private void Spawn(NetworkObject prefab, Vector3 at, Quaternion rotation)
        {
            spawned.RemoveAll(o => o == null || !o.IsSpawned);
            spawned.Add(NetworkManager.SpawnManager.InstantiateAndSpawn(prefab, position: at, rotation: rotation));
        }

        // The road's surface (or the ground) under a point.
        private static Vector3 Ground(Vector3 at)
        {
            int mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            return Physics.Raycast(at + Vector3.up * 30f, Vector3.down, out RaycastHit hit, 80f, mask, QueryTriggerInteraction.Ignore) ? hit.point : at;
        }

        [Rpc(SendTo.Everyone)]
        private void TriggeredRpc(int kind, Vector3 at) => Triggered?.Invoke((RoadEvent)kind, at);

        [Rpc(SendTo.Everyone)]
        private void ScareRpc(Vector3 at, int seed)
        {
            Triggered?.Invoke(RoadEvent.JumpScare, at);
            GameAudio.Play(SoundId.Scream, at + Vector3.up * 1.5f, .8f);
            if (scareModel == null || GameSettings.FewerJumpScares) return;
            GameObject shape = Instantiate(scareModel, at, Quaternion.Euler(0f, seed, 0f));
            foreach (Collider c in shape.GetComponentsInChildren<Collider>()) Destroy(c);
            shape.AddComponent<HorrorScare>().Setup(seed % 2 == 0 ? 0 : 2);
        }

        public void EditorSetup(WorldVan worldVan, NetworkObject[] hazardPrefabs, NetworkObject monsterPrefab, GameObject scare)
        {
            van = worldVan;
            hazards = hazardPrefabs;
            monster = monsterPrefab;
            scareModel = scare;
        }
    }
}
