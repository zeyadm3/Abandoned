using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Equipment;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Extraction
{
    /// <summary>
    /// A store's roller shutter (GDD 8 "locked doors", M10.4): rolled up (open) or down and padlocked, as
    /// the run's <see cref="RunShutters"/> says. Down, it blocks the doorway for players, loot and monsters
    /// (a carved NavMesh hole). E on it with bolt cutters or a crowbar in hand asks the host to open it;
    /// from inside the store, bare hands do too (slowly), so a slam can never trap anyone.
    /// Scene object on every machine; nothing here is networked itself.
    /// </summary>
    public class RollerShutter : MonoBehaviour, IUsable
    {
        private static readonly List<RollerShutter> Live = new();

        [Tooltip("Its bit in the run's shutter mask: unique per level, 0-31.")]
        [SerializeField, Range(0, 31)] private int index;
        [Tooltip("The front entrance: only a Sealed job locks it.")]
        [SerializeField] private bool entrance;
        [Tooltip("The slatted curtain, pivot at the top of the doorway: it rolls up (shrinks) into the box there.")]
        [SerializeField] private Transform curtain;
        [SerializeField] private Collider blocker;
        [OptionalReference, SerializeField] private NavMeshObstacle obstacle;
        [Tooltip("The store behind it (world space): its loot is worth more while it's locked at the start of a run.")]
        [SerializeField] private Bounds room;

        private float shown = 1f; // 0 = down, 1 = up

        public static IReadOnlyList<RollerShutter> All => Live;
        public int Index => index;
        public bool Entrance => entrance;
        public Bounds Room => room;
        public bool IsDown => RunShutters.Current != null && RunShutters.Current.IsDown(index);
        /// <summary>The closed doorway's solid (world bounds), used to keep a slam off anyone standing in it.</summary>
        public Bounds DoorwayBounds => blocker != null ? blocker.bounds : new Bounds(transform.position + Vector3.up, new Vector3(1.5f, 2.3f, 0.3f));

        /// <summary>Is this point in the store behind the shutter (the side a slam can trap you on)?</summary>
        public bool InRoom(Vector3 at) => room.size.sqrMagnitude > 0.01f && room.Contains(at + Vector3.up * 0.2f);

        // Open until a run says otherwise: a level with no run yet (or a test) must never be walled off.
        private void Awake() => Show(1f);

        private void OnEnable() => Live.Add(this);

        private void OnDisable() => Live.Remove(this);

        private void Update()
        {
            // No run yet, or a new run still rolling its locks: hold still rather than flap.
            RunShutters run = RunShutters.Current;
            if (run == null || !run.Ready) return;
            float target = IsDown ? 0f : 1f;
            if (Mathf.Approximately(shown, target)) return;
            Show(Mathf.MoveTowards(shown, target, Time.deltaTime / 1.2f));
        }

        private void Show(float up)
        {
            shown = up;
            if (curtain != null) curtain.localScale = new Vector3(1f, Mathf.Lerp(1f, 0.05f, up), 1f);
            bool blocking = up < 0.9f;
            if (blocker != null) blocker.enabled = blocking;
            if (obstacle != null) obstacle.enabled = blocking;
        }

        public string UsePrompt(GameObject user)
        {
            if (!IsDown) return null;
            PlayerEquipment gear = user != null ? user.GetComponentInParent<PlayerEquipment>() : null;
            if (gear != null && gear.Has(EquipmentKind.BoltCutters)) return "Cut the padlock (quiet)";
            if (gear != null && gear.Has(EquipmentKind.Crowbar)) return "Pry the shutter up (loud, a few goes)";
            // The padlock is on the outside: from inside the store you can always heave it up (QA B-01).
            if (user != null && InRoom(user.transform.position))
                return RunShutters.Current != null && RunShutters.Current.IsLifting(index) ? "Heaving it up..." : "Heave the shutter up from inside (slow, loud)";
            return entrance ? "Shuttered. Bolt cutters or a crowbar would get you in" : "Locked. Bolt cutters or a crowbar would open it";
        }

        public void Use(GameObject user) => RunShutters.Current?.RequestOpen(index);

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible && Application.isPlaying) return;
            Gizmos.color = IsDown ? new Color(1f, 0.3f, 0.2f, 0.6f) : new Color(0.3f, 1f, 0.4f, 0.3f);
            Gizmos.DrawWireCube(room.center, room.size);
        }

#if UNITY_EDITOR
        public void EditorSetup(int shutterIndex, bool isEntrance, Transform curtainTransform, Collider blockerCollider, NavMeshObstacle navObstacle, Bounds store)
        {
            index = shutterIndex;
            entrance = isEntrance;
            curtain = curtainTransform;
            blocker = blockerCollider;
            obstacle = navObstacle;
            room = store;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}
