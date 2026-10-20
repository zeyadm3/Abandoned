namespace Abandoned.Tutorial
{
    /// <summary>
    /// What kind of thing the training's guide points at; with the guide's key it names the target in a lesson's
    /// Guide line ("take item:lamp", "use crate:hand_trolley", "load truck"). Sent as a byte: append only.
    /// </summary>
    public enum TutorialGuideTarget : byte
    {
        None,
        /// <summary>A <see cref="TutorialArea"/> by key (a room, the porch, the basement).</summary>
        Area,
        /// <summary>A <see cref="TutorialSpot"/> by key.</summary>
        Spot,
        /// <summary>A training salvage item by key (followed as it moves).</summary>
        Item,
        /// <summary>The truck's cargo bay, by way of the foot of its ramp.</summary>
        Truck,
        /// <summary>The truck's lever.</summary>
        Lever,
        /// <summary>The kit crate that hands out this equipment id.</summary>
        Crate,
        /// <summary>A sliding door by number.</summary>
        Door,
        /// <summary>The breaker panel.</summary>
        Breaker,
        /// <summary>A closet (hiding place) by number.</summary>
        Closet,
        /// <summary>A named floor section (<see cref="TutorialSection"/>).</summary>
        Section,
    }
}
