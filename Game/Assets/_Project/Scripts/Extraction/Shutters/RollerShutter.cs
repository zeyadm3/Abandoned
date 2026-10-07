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
    /// (a carved NavMesh hole). E on it with bolt cutters or a crowbar in hand asks the host to open it.
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

        private float shown = -1f; // 0 = down, 1 = up

        public static IReadOnlyList<RollerShutter> All => Live;
        public int Index => index;
        public bool Entrance => entrance;
        public Bounds Room => room;
        public bool IsDown => RunShutters.Current != null && RunShutters.Current.IsDown(index);

        private void OnEnable() => Live.Add(this);

        private void OnDisable() => Live.Remove(this);

        private void Update()
        {
            RunShutters run = RunShutters.Current;
            if (run != null && !run.Ready) return; // a new run is rolling its locks: hold still
            float target = IsDown ? 0f : 1f;
            if (Mathf.Approximately(shown, target)) return;
            // Snap on the first frame (a level loading mid-run), roll after that.
            shown = shown < 0f ? target : Mathf.MoveTowards(shown, target, Time.deltaTime / 1.2f);
            if (curtain != null) curtain.localScale = new Vector3(1f, Mathf.Lerp(1f, 0.05f, shown), 1f);
            bool blocking = shown < 0.9f;
            if (blocker != null) blocker.enabled = blocking;
            if (obstacle != null) obstacle.enabled = blocking;
        }

        public string UsePrompt(GameObject user)
        {
            if (!IsDown) return null;
            PlayerEquipment gear = user != null ? user.GetComponentInParent<PlayerEquipment>() : null;
            if (gear != null && gear.Has(EquipmentKind.BoltCutters)) return "Cut the padlock (quiet)";
            if (gear != null && gear.Has(EquipmentKind.Crowbar)) return "Pry the shutter up (loud, a few goes)";
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
