using Abandoned.Core;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// A door of the van (a solid collider on the body): E climbs in, taking the first free seat in this door's list.
    /// The host decides; <see cref="VanSeats"/> moves the player.
    /// </summary>
    public class VanSeat : MonoBehaviour, IUsable
    {
        [Tooltip("Seats this door leads to, first choice first (0 = the driver's).")]
        [SerializeField] private int[] seats = { 0 };

        private VanSeats van;

        private void Awake() => van = GetComponentInParent<VanSeats>();

        public string UsePrompt(GameObject user)
        {
            if (van == null || !van.IsSpawned) return null;
            int seat = van.FirstFree(seats);
            if (seat < 0) return Loc.T("The van is full");
            return seat == VanSeats.DriverSeat ? Loc.T("Drive the van") : Loc.T("Get in the van");
        }

        public void Use(GameObject user)
        {
            if (van == null) return;
            int seat = van.FirstFree(seats);
            if (seat >= 0) van.RequestSeat(seat);
        }

        public void EditorSetup(params int[] doorSeats) => seats = doorSeats;
    }
}
