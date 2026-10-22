using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Skyscraper
{
    /// <summary>
    /// Host-authoritative elevator connecting the street lobby, mechanical floor, and skyscraper restaurant.
    /// Manages cab motion, landing gates, weight-limited cable snap, and sector power dependencies.
    /// </summary>
    [DefaultExecutionOrder(-60)]
    public class SkyscraperElevator : NetworkBehaviour
    {
        [Header("Floors & Platform")]
        [SerializeField] private MovingPlatform platform;
        [Tooltip("Floor elevations in metres: 0 = Lobby (0m), 1 = Mechanical (20m), 2 = Restaurant (44m).")]
        [SerializeField] private float[] floorElevations = { 0f, 20f, 44f };
        [SerializeField] private GameObject[] landingGates;
        [SerializeField] private Vector2 deckSize = new(3.5f, 3.5f);

        [Header("Performance & Limits")]
        [SerializeField, Min(1f)] private float travelSpeed = 4.0f;
        [SerializeField, Min(50f)] private float maxSafeLoadKg = 450f;
        [SerializeField, Min(0f)] private float dropDamage = 40f;
        [SerializeField] private int powerSector = 1;

        private readonly NetworkVariable<float> currentHeight = new(0f);
        private readonly NetworkVariable<int> activeFloor = new(0);
        private readonly NetworkVariable<int> destinationFloor = new(0);
        private readonly NetworkVariable<bool> isMoving = new(false);
        private readonly NetworkVariable<bool> isBroken = new(false);

        private Vector3 basePlatformPos;
        private float fallSpeed;
        private float nextNoiseTime;
        private float overloadTimer;
        private bool hasSnapped;

        public static SkyscraperElevator Active { get; private set; }
        public int CurrentFloor => activeFloor.Value;
        public bool IsMoving => isMoving.Value;
        public bool IsBroken => isBroken.Value;
        public bool IsPowered => SectorPower.Live(powerSector);
        public Vector3 DeckPosition => platform != null ? platform.transform.position : transform.position;

        private void Awake()
        {
            basePlatformPos = platform != null ? platform.transform.position : transform.position;
        }

        public override void OnNetworkSpawn()
        {
            Active = this;
        }

        public override void OnNetworkDespawn()
        {
            if (Active == this) Active = null;
        }

        public bool OnDeck(Vector3 worldPoint)
        {
            if (platform == null) return false;
            Vector3 local = platform.transform.InverseTransformPoint(worldPoint);
            return Mathf.Abs(local.x) <= deckSize.x * 0.5f &&
                   Mathf.Abs(local.z) <= deckSize.y * 0.5f &&
                   local.y > -0.3f && local.y < 3.0f;
        }

        public bool CanAcceptRequest(int targetFloor)
        {
            if (isBroken.Value || isMoving.Value || !IsPowered) return false;
            if (targetFloor < 0 || targetFloor >= floorElevations.Length) return false;
            return activeFloor.Value != targetFloor;
        }

        public void RequestFloor(int targetFloor)
        {
            if (CanAcceptRequest(targetFloor))
                RequestFloorRpc(targetFloor);
        }

        [Rpc(SendTo.Server)]
        private void RequestFloorRpc(int targetFloor, RpcParams rpcParams = default)
        {
            if (!IsServer || !CanAcceptRequest(targetFloor)) return;

            float load = CalculateLoad();
            destinationFloor.Value = targetFloor;
            isMoving.Value = true;
            overloadTimer = load > maxSafeLoadKg ? 2.5f : -1f;
            hasSnapped = false;

            NoiseSystem.Emit(DeckPosition, 0.75f, NoiseSource.Other);
            BroadcastSoundRpc(SoundId.LiftStart);
            Debug.Log($"[SkyscraperElevator] Moving to floor {targetFloor} (load: {load:0} kg / max: {maxSafeLoadKg} kg).");
        }

        private void Update()
        {
            if (IsServer)
                UpdateServer();

            // Client and host both update moving platform and landing gates from synced height
            UpdateCabAndGates();
        }

        private void UpdateServer()
        {
            if (isBroken.Value)
            {
                // Free fall until hitting buffer springs at bottom
                if (currentHeight.Value > floorElevations[0])
                {
                    fallSpeed += 9.81f * Time.deltaTime * 2.5f;
                    float newH = Mathf.Max(floorElevations[0], currentHeight.Value - fallSpeed * Time.deltaTime);
                    currentHeight.Value = newH;

                    if (newH <= floorElevations[0])
                    {
                        // Impact at bottom
                        BroadcastSoundRpc(SoundId.Crash);
                        NoiseSystem.Emit(DeckPosition, 1.0f, NoiseSource.Other);
                        CameraShake.Emit(DeckPosition, 2500f);

                        // Damage all players on deck
                        foreach (NetworkPlayer p in NetworkPlayer.All)
                        {
                            if (p != null && !p.IsDead && OnDeck(p.transform.position))
                                p.DealDamage(dropDamage, Loc.Mark("fell with the elevator"));
                        }
                    }
                }
                return;
            }

            if (!isMoving.Value) return;

            if (!IsPowered)
            {
                // Emergency freeze on power outage
                isMoving.Value = false;
                Debug.LogWarning("[SkyscraperElevator] Power lost! Elevator stalled mid-shaft.");
                return;
            }

            // Check for overload snap countdown
            if (overloadTimer > 0f)
            {
                overloadTimer -= Time.deltaTime;
                if (overloadTimer <= 0f && !hasSnapped)
                {
                    hasSnapped = true;
                    isBroken.Value = true;
                    fallSpeed = 0f;
                    BroadcastSoundRpc(SoundId.LiftSnap);
                    CameraShake.Emit(DeckPosition, 1200f);
                    Debug.LogWarning("[SkyscraperElevator] Worn cables snapped under excessive payload!");
                    return;
                }
            }

            float targetHeight = floorElevations[destinationFloor.Value];
            float step = travelSpeed * Time.deltaTime;
            float newHeight = Mathf.MoveTowards(currentHeight.Value, targetHeight, step);
            currentHeight.Value = newHeight;

            if (Time.time >= nextNoiseTime)
            {
                nextNoiseTime = Time.time + 1.6f;
                NoiseSystem.Emit(DeckPosition, 0.6f, NoiseSource.Other);
            }

            if (Mathf.Abs(newHeight - targetHeight) < 0.01f)
            {
                currentHeight.Value = targetHeight;
                activeFloor.Value = destinationFloor.Value;
                isMoving.Value = false;
                BroadcastSoundRpc(SoundId.LiftArrive);
                Debug.Log($"[SkyscraperElevator] Arrived at floor {activeFloor.Value}.");
            }
        }

        private void UpdateCabAndGates()
        {
            if (platform != null)
                platform.MoveTo(basePlatformPos + Vector3.up * currentHeight.Value);

            if (landingGates != null)
            {
                for (int i = 0; i < landingGates.Length; i++)
                {
                    if (landingGates[i] == null) continue;
                    bool cabHereAndStopped = !isMoving.Value && !isBroken.Value &&
                                             Mathf.Abs(currentHeight.Value - floorElevations[Mathf.Clamp(i, 0, floorElevations.Length - 1)]) < 0.15f;
                    // Gate is active (closed) if elevator is moving or parked at a different level
                    landingGates[i].SetActive(!cabHereAndStopped);
                }
            }
        }

        public float CalculateLoad()
        {
            float total = 0f;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.IsDead || !OnDeck(p.transform.position)) continue;
                total += p.TryGetComponent(out CarrierLoad load) && load.LoadWeight > 0f ? load.LoadWeight : 80f;
            }

            IReadOnlyList<ILoadSource> sources = LoadSources.All;
            for (int i = 0; i < sources.Count; i++)
            {
                if (sources[i] is LootItem item && item != null && OnDeck(item.transform.position))
                    total += item.LoadWeight;
            }
            return total;
        }

        [Rpc(SendTo.Everyone)]
        private void BroadcastSoundRpc(SoundId sound)
        {
            GameAudio.Play(sound, DeckPosition + Vector3.up * 1.5f, 1f, 40f);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;

            // 1. Elevator Cab Wire Box (green = stopped, yellow = moving, red = broken)
            Gizmos.color = isBroken.Value ? Color.red : (isMoving.Value ? Color.yellow : Color.green);
            Vector3 cabCenter = DeckPosition + Vector3.up * 1.4f;
            Gizmos.DrawWireCube(cabCenter, new Vector3(deckSize.x, 2.8f, deckSize.y));

            // 2. Floor Stops along the shaft
            if (floorElevations != null)
            {
                for (int i = 0; i < floorElevations.Length; i++)
                {
                    Gizmos.color = (i == activeFloor.Value && !isMoving.Value) ? Color.cyan : new Color(0.2f, 0.6f, 1.0f, 0.6f);
                    Vector3 stopPos = (platform != null ? basePlatformPos : transform.position) + Vector3.up * floorElevations[i];
                    Gizmos.DrawWireCube(stopPos + Vector3.up * 0.1f, new Vector3(deckSize.x, 0.2f, deckSize.y));
                }
            }

            // 3. Load indicator sphere (white = safe, magenta = overloaded)
            float currentLoad = CalculateLoad();
            Gizmos.color = (currentLoad > maxSafeLoadKg) ? Color.magenta : Color.white;
            Gizmos.DrawWireSphere(cabCenter + Vector3.up * 1.6f, 0.4f);
        }
#endif

#if UNITY_EDITOR
        public void EditorSetup(MovingPlatform deck, float[] floors, GameObject[] gates, Vector2 size, float speed, float maxLoad, int sector)
        {
            platform = deck;
            floorElevations = floors;
            landingGates = gates;
            deckSize = size;
            travelSpeed = speed;
            maxSafeLoadKg = maxLoad;
            powerSector = sector;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active = null;
    }
}
