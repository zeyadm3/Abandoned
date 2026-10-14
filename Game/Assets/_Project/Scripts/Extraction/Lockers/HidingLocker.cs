using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// A steel locker one player can hide in (0.16.0, GDD 9 "hide"). E in front of it with empty hands asks
    /// the host; the host writes who is inside onto that player (<see cref="NetworkPlayer.HidingIn"/>) and
    /// the player's own machine steps them in. Inside you look out through the slats and nothing that hunts
    /// by sight finds you, unless it watched you climb in. Scene object on every machine; nothing here is
    /// networked itself. Its own space: Z points out of the door, the origin is on the floor at the back.
    /// </summary>
    public class HidingLocker : MonoBehaviour, IUsable
    {
        private const float DoorSwingSeconds = 0.18f;

        private static readonly List<HidingLocker> Live = new();

        [Tooltip("Unique per level, 0-126: what a hiding player's state names.")]
        [SerializeField, Range(0, 126)] private int index;
        [Tooltip("The door, pivoting on its hinge edge.")]
        [SerializeField] private Transform door;
        [Tooltip("Where the occupant stands (feet), facing out through the slats.")]
        [SerializeField] private Transform inside;
        [Tooltip("On the floor in front of the door: where they step out to, and where a monster reaches in from.")]
        [SerializeField] private Transform exit;
        [Tooltip("The floor it stands on: when that collapses the locker is gone and whoever was in it is out.")]
        [OptionalReference, SerializeField] private StructuralSection floor;
        [SerializeField, Range(30f, 120f)] private float doorSwing = 95f;

        private NetworkPlayer shownOccupant;
        private float doorOpenUntil, doorShown;
        private bool shutSoundDue;

        public static IReadOnlyList<HidingLocker> All => Live;
        public int Index => index;
        public Vector3 InsidePoint => inside != null ? inside.position : transform.position;
        public Quaternion Facing => Quaternion.Euler(0f, (inside != null ? inside : transform).eulerAngles.y, 0f);
        public Vector3 ExitPoint => exit != null ? exit.position : transform.position + transform.forward * 1.1f;
        /// <summary>False once the floor under it has collapsed.</summary>
        public bool Usable => isActiveAndEnabled && (floor == null || !floor.IsCollapsed);

        /// <summary>Whoever is hiding in it, on this machine's view of the players; null when empty.</summary>
        public NetworkPlayer Occupant
        {
            get
            {
                foreach (NetworkPlayer p in NetworkPlayer.All)
                    if (p != null && p.IsSpawned && p.HidingIn == index) return p;
                return null;
            }
        }

        public static HidingLocker ByIndex(int lockerIndex)
        {
            if (lockerIndex < 0) return null;
            foreach (HidingLocker locker in Live)
                if (locker != null && locker.index == lockerIndex) return locker;
            return null;
        }

        private void OnEnable() => Live.Add(this);

        private void OnDisable() => Live.Remove(this);

        // Every machine: the door swings open when someone climbs in or out, then shuts behind them.
        private void Update()
        {
            NetworkPlayer occupant = Occupant;
            if (occupant != shownOccupant)
            {
                shownOccupant = occupant;
                doorOpenUntil = Time.time + DoorConfig.Current.LockerDoorSeconds;
                shutSoundDue = true;
                GameAudio.Play(SoundId.LockerOpen, DoorPoint, 1f);
            }
            float target = Time.time < doorOpenUntil ? 1f : 0f;
            if (Mathf.Approximately(doorShown, target)) return;
            doorShown = Mathf.MoveTowards(doorShown, target, Time.deltaTime / DoorSwingSeconds);
            // Hinged on its left edge; the free edge swings out toward the room.
            if (door != null) door.localRotation = Quaternion.Euler(0f, -doorSwing * doorShown, 0f);
            if (shutSoundDue && doorShown <= 0f)
            {
                shutSoundDue = false;
                GameAudio.Play(SoundId.LockerShut, DoorPoint, 1f);
            }
        }

        private Vector3 DoorPoint => (door != null ? door.position : transform.position) + Vector3.up * 1.1f;

        public string UsePrompt(GameObject user)
        {
            NetworkPlayer me = user != null ? user.GetComponentInParent<NetworkPlayer>() : null;
            if (me == null || !me.IsSpawned || !Usable) return null;
            if (me.HidingIn == index) return Loc.T("Climb out");
            if (Occupant != null) return Loc.T("Someone is hiding in here");
            return Loc.T("Hide in the locker");
        }

        public void Use(GameObject user)
        {
            NetworkPlayer me = user != null ? user.GetComponentInParent<NetworkPlayer>() : null;
            if (me == null || !me.IsSpawned || !Usable) return;
            if (me.HidingIn == index)
            {
                me.RequestLeaveHiding();
                return;
            }
            if (Occupant != null) return;
            // Nothing bigger than a pocket fits in there with you (the prompt is hidden while hands are full).
            if (me.Carrier != null && me.Carrier.Held != null)
            {
                me.Carrier.ShowHint(Loc.Mark("Put that down to hide in the locker"));
                return;
            }
            me.RequestHide(index);
        }

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible && Application.isPlaying) return;
            Gizmos.color = Application.isPlaying && Occupant != null ? new Color(1f, 0.3f, 0.2f, 0.9f) : new Color(0.3f, 0.8f, 1f, 0.7f);
            Gizmos.DrawWireSphere(InsidePoint + Vector3.up * 0.9f, 0.3f);
            Gizmos.DrawLine(InsidePoint + Vector3.up * 0.9f, ExitPoint + Vector3.up * 0.9f);
            Gizmos.DrawWireSphere(ExitPoint, 0.15f);
        }

#if UNITY_EDITOR
        public void EditorSetup(int lockerIndex, Transform doorLeaf, Transform insidePoint, Transform exitPoint, StructuralSection standsOn)
        {
            index = lockerIndex;
            door = doorLeaf;
            inside = insidePoint;
            exit = exitPoint;
            floor = standsOn;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}
