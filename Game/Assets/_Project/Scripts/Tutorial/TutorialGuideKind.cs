namespace Abandoned.Tutorial
{
    /// <summary>
    /// What the training's guide asks of the player at its target: it picks the marker's words, its colour and
    /// whether arrows on the floor lead there. Sent as a byte: append only.
    /// </summary>
    public enum TutorialGuideKind : byte
    {
        /// <summary>No target: the lesson is about something you do where you stand.</summary>
        None,
        /// <summary>Walk there (floor arrows).</summary>
        Go,
        /// <summary>Pick this up (floor arrows; the marker goes once someone holds it).</summary>
        Take,
        /// <summary>Press interact on this: a crate, a door, the breaker, the lever (floor arrows).</summary>
        Use,
        /// <summary>Carry it into the truck (floor arrows up the ramp).</summary>
        Load,
        /// <summary>Hide here (floor arrows).</summary>
        Hide,
        /// <summary>Get off this floor: outlined in red, no arrows toward it.</summary>
        Away,
        /// <summary>Stand on this floor and brace it with the support jack (floor arrows).</summary>
        Brace,
        /// <summary>Look at this: a marker only.</summary>
        Look,
        /// <summary>Stay put and wait.</summary>
        Wait,
    }
}
