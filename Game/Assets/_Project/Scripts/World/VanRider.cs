using UnityEngine;

namespace Abandoned.World
{
    /// <summary>Per-player bookkeeping for riding the van, on each machine (added at run time by <see cref="VanSeats"/>).</summary>
    public class VanRider : MonoBehaviour
    {
        /// <summary>This machine has told physics to ignore this player against the van.</summary>
        public bool IgnoringVan { get; set; }

        /// <summary>Owner: the seat this machine has put its player in (-1 = none).</summary>
        public int OwnerSeat { get; set; } = -1;

        /// <summary>Owner: the van's heading last frame, so the view turns with the van.</summary>
        public float LastVanYaw { get; set; }

        /// <summary>Owner: the frame the player sat down, so the same E press doesn't also climb out.</summary>
        public int SeatedFrame { get; set; }
    }
}
