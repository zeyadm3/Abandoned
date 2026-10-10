using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Extraction
{
    public enum DoorKind : byte { Store, Service }

    /// <summary>
    /// A sliding door across a doorway (0.16.0, GDD 9 "close doors"): E slides it open or shut, as the run's
    /// <see cref="RunDoors"/> says. Shut, it blocks the doorway for players, loot, sight and monsters (a
    /// carved NavMesh hole). Only the Hunter gets through a shut one, by breaking it down; a broken door
    /// hangs open for the rest of the job. Scene object on every machine; nothing here is networked itself.
    /// Its own space: X along the opening, Z through it, origin on the floor in the middle.
    /// </summary>
    public class SlidingDoor : MonoBehaviour, IUsable
    {
        // The leaves close in this much of the doorway's depth either side of the door (m).
        private const float SlotHalfDepth = 0.45f;
        // Blocks from this far shut (0 = shut, 1 = open): nobody squeezes through the last of a closing gap.
        private const float BlockingBelow = 0.6f;

        private static readonly List<SlidingDoor> Live = new();
        private static readonly Collider[] SlotHits = new Collider[8];

        [Tooltip("Its bit in the run's door masks: unique per level, 0-63.")]
        [SerializeField, Range(0, 63)] private int index;
        [SerializeField] private DoorKind kind;
        [Tooltip("The sliding panels. Each one's place when shut and when open, in this door's own space.")]
        [SerializeField] private Transform[] leaves;
        [SerializeField] private Vector3[] shutAt;
        [SerializeField] private Vector3[] openAt;
        [Tooltip("The shut doorway's solid: on while the door is shut or nearly shut.")]
        [SerializeField] private Collider blocker;
        [OptionalReference, SerializeField] private NavMeshObstacle obstacle;
        [SerializeField, Min(0.5f)] private float width = 4f;
        [SerializeField, Min(0.5f)] private float height = 3f;

        private const float RattleSeconds = 0.35f;

        private float shown = 1f; // 0 = shut, 1 = open
        private bool leaning, rattling;
        private float rattleUntil;

        public static IReadOnlyList<SlidingDoor> All => Live;
        public int Index => index;
        public DoorKind Kind => kind;
        public float Width => width;
        public float Height => height;
        public bool IsShut => RunDoors.Current != null && RunDoors.Current.IsShut(index);
        public bool IsBroken => RunDoors.Current != null && RunDoors.Current.IsBroken(index);
        /// <summary>The middle of the opening at chest height (where its sounds come from).</summary>
        public Vector3 Centre => transform.position + Vector3.up * Mathf.Min(1.2f, height * 0.5f);

        public static SlidingDoor ByIndex(int doorIndex)
        {
            foreach (SlidingDoor door in Live)
                if (door != null && door.index == doorIndex) return door;
            return null;
        }

        // Open until a run says otherwise: a level with no run yet (or a test) must never be walled off.
        private void Awake() => Show(1f);

        private void OnEnable() => Live.Add(this);

        private void OnDisable() => Live.Remove(this);

        private void Update()
        {
            // No run yet, or a new run still rolling its doors: hold still rather than flap.
            RunDoors run = RunDoors.Current;
            if (run == null || !run.Ready) return;
            bool broken = run.IsBroken(index);
            float target = run.IsShut(index) ? 0f : 1f;
            if (!Mathf.Approximately(shown, target))
            {
                DoorConfig config = DoorConfig.Current;
                Show(Mathf.MoveTowards(shown, target, Time.deltaTime / (broken ? config.BreakSeconds : config.SlideSeconds)));
            }
            if (broken != leaning) Lean(broken);
            if (rattling) Shake();
        }

        /// <summary>Every machine: something hit the shut door; its leaves jump on their track for a moment.</summary>
        public void Rattle()
        {
            rattleUntil = Time.time + RattleSeconds;
            rattling = true;
        }

        private void Shake()
        {
            float left = rattleUntil - Time.time;
            if (left <= 0f)
            {
                rattling = false;
                Show(shown);
                return;
            }
            Pose(Mathf.Sin(Time.time * 70f) * 0.035f * (left / RattleSeconds));
        }

        private void Show(float open)
        {
            shown = open;
            Pose(0f);
            bool blocking = open < BlockingBelow;
            if (blocker != null) blocker.enabled = blocking;
            if (obstacle != null) obstacle.enabled = blocking;
        }

        // Puts the leaves where "shown" says, pushed through the doorway by a rattle's worth.
        private void Pose(float push)
        {
            if (leaves == null || shutAt == null || openAt == null) return;
            // Eased at both ends: the leaf has weight.
            float eased = Mathf.SmoothStep(0f, 1f, shown);
            for (int i = 0; i < leaves.Length && i < shutAt.Length && i < openAt.Length; i++)
                if (leaves[i] != null) leaves[i].localPosition = Vector3.Lerp(shutAt[i], openAt[i], eased) + Vector3.forward * push;
        }

        // Broken down: the leaves hang off their track, each its own way, so it reads as wrecked from across the hall.
        private void Lean(bool broken)
        {
            leaning = broken;
            for (int i = 0; leaves != null && i < leaves.Length; i++)
            {
                if (leaves[i] == null) continue;
                float side = i % 2 == 0 ? 1f : -1f;
                leaves[i].localRotation = broken ? Quaternion.Euler(5f + 2f * i, 0f, 3.5f * side) : Quaternion.identity;
            }
        }

        /// <summary>A world point in this door's own space.</summary>
        public Vector3 ToLocal(Vector3 world) => transform.InverseTransformPoint(world);

        /// <summary>Would walking straight from one point to the other go through this doorway?</summary>
        public bool Between(Vector3 from, Vector3 to) => DoorMath.Crosses(ToLocal(from), ToLocal(to), width, height);

        /// <summary>Flat distance (m) from a point to the opening.</summary>
        public float DistanceTo(Vector3 world) => DoorMath.FlatDistance(ToLocal(world), width);

        /// <summary>
        /// Flat distance (m) from a point to the nearest thing a hand can push: the opening, or a leaf parked
        /// beside it. The host checks a player's reach against this.
        /// </summary>
        public float ReachDistance(Vector3 world)
        {
            if (Mathf.Abs(ToLocal(world).y) > height + 1f) return float.MaxValue; // another storey
            float best = DistanceTo(world);
            float halfLeaf = width * 0.5f / Mathf.Max(1, leaves != null ? leaves.Length : 1);
            for (int i = 0; leaves != null && i < leaves.Length; i++)
            {
                if (leaves[i] == null) continue;
                Vector3 to = world - leaves[i].position;
                to.y = 0f;
                best = Mathf.Min(best, Mathf.Max(0f, to.magnitude - halfLeaf));
            }
            return best;
        }

        /// <summary>A spot on the floor in front of the middle of the door, on the same side as <paramref name="from"/>.</summary>
        public Vector3 StandPoint(Vector3 from, float distance)
        {
            float side = ToLocal(from).z >= 0f ? 1f : -1f;
            return transform.position + transform.forward * (side * distance);
        }

        /// <summary>
        /// Is anyone alive (or their body), or any loot, in the slot the leaves close in? Each machine asks
        /// with its own view: the prompt on the player's, the decision on the host's.
        /// </summary>
        public bool SlotBlocked()
        {
            // The leaves run a little to one side of the wall's middle; the slot is centred on them.
            float track = blocker is BoxCollider box ? box.center.z : 0f;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || p.IsDead) continue;
                Vector3 at = ToLocal(p.Ragdoll != null && p.Ragdoll.IsRagdolled ? p.Ragdoll.BodyPosition : p.transform.position);
                at.z -= track;
                if (DoorMath.InSlot(at, width, height, SlotHalfDepth)) return true;
            }
            int lootMask = LayerMask.GetMask(GameLayers.Loot);
            Vector3 centre = transform.TransformPoint(new Vector3(0f, height * 0.5f, track));
            return Physics.OverlapBoxNonAlloc(centre, new Vector3(width * 0.5f, height * 0.5f, 0.2f), SlotHits,
                transform.rotation, lootMask, QueryTriggerInteraction.Ignore) > 0;
        }

        /// <summary>The nearest shut, unbroken door on this storey that stands between two points, or null.</summary>
        public static SlidingDoor ShutBetween(Vector3 from, Vector3 to, float maxDistance)
        {
            RunDoors run = RunDoors.Current;
            if (run == null || !run.Ready) return null;
            SlidingDoor best = null;
            float bestDistance = maxDistance;
            foreach (SlidingDoor door in Live)
            {
                if (door == null || !run.IsShut(door.index) || run.IsBroken(door.index) || !door.Between(from, to)) continue;
                float distance = door.DistanceTo(from);
                if (distance > bestDistance) continue;
                best = door;
                bestDistance = distance;
            }
            return best;
        }

        public string UsePrompt(GameObject user)
        {
            RunDoors run = RunDoors.Current;
            if (run == null || !run.Ready || run.IsBroken(index)) return null;
            if (run.IsShut(index)) return "Slide the door open";
            return SlotBlocked() ? "Something is in the doorway" : "Slide the door shut";
        }

        public void Use(GameObject user) => RunDoors.Current?.RequestToggle(index);

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible && Application.isPlaying) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = IsBroken ? new Color(1f, 0.6f, 0.1f, 0.8f) : IsShut ? new Color(1f, 0.3f, 0.2f, 0.8f) : new Color(0.3f, 1f, 0.4f, 0.5f);
            Gizmos.DrawWireCube(new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, SlotHalfDepth * 2f));
        }

#if UNITY_EDITOR
        public void EditorSetup(int doorIndex, DoorKind doorKind, Transform[] panels, Vector3[] shut, Vector3[] open,
            Collider blockerCollider, NavMeshObstacle navObstacle, float openingWidth, float openingHeight)
        {
            index = doorIndex;
            kind = doorKind;
            leaves = panels;
            shutAt = shut;
            openAt = open;
            blocker = blockerCollider;
            obstacle = navObstacle;
            width = openingWidth;
            height = openingHeight;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}
