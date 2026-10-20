namespace Abandoned.Tutorial
{
    /// <summary>
    /// What ends a training lesson (<see cref="TutorialStepDefinition.Check"/>). The step's Key names the item,
    /// area, section or door it is about; Amount is its number (seconds, metres, degrees, dollars, a stage).
    /// Personal checks are judged on each player's own machine (input, the scan view) and reported to the host;
    /// every other check is the host's view of the world. Append only: step assets store the number.
    /// </summary>
    public enum TutorialCheck
    {
        /// <summary>A card to read: ends after Amount seconds (0 = the config's read time).</summary>
        Read,
        /// <summary>Personal: turned the view Amount degrees.</summary>
        Look,
        /// <summary>Personal: walked Amount metres.</summary>
        Walk,
        /// <summary>Personal: sprinted for Amount seconds.</summary>
        Sprint,
        /// <summary>Personal: crouched for Amount seconds.</summary>
        Crouch,
        /// <summary>Personal: jumped.</summary>
        Jump,
        /// <summary>Personal: scanned (the values are showing).</summary>
        Scan,
        /// <summary>Personal: held the pockets open (the inventory key) for a moment.</summary>
        Pockets,
        /// <summary>Personal: threw something hard (let go of it moving at Amount m/s or more).</summary>
        Throw,
        /// <summary>Someone is inside area Key.</summary>
        EnterArea,
        /// <summary>Someone's flashlight is on.</summary>
        Flashlight,
        /// <summary>Item Key is in someone's hands (or pockets).</summary>
        HoldItem,
        /// <summary>Item Key is in someone's pockets (any item when Key is empty).</summary>
        PocketItem,
        /// <summary>Item Key is out of everyone's hands and pockets (set down, thrown, dropped from a pocket).</summary>
        DropItem,
        /// <summary>Item Key has moved Amount metres from where it started.</summary>
        MoveItem,
        /// <summary>Item Key is inside the step's Area.</summary>
        ItemInArea,
        /// <summary>Item Key is in the truck's cargo bay (or in the pockets of someone standing in it).</summary>
        ItemInTruck,
        /// <summary>Someone switched hand slots.</summary>
        SwitchSlot,
        /// <summary>Someone has gear Key (an equipment id) in a hand slot.</summary>
        HasGear,
        /// <summary>Door number Amount is shut.</summary>
        DoorShut,
        /// <summary>Door number Amount is open.</summary>
        DoorOpen,
        /// <summary>Section Key has reached stage Amount (1 stressed, 2 cracking, 3 failing, 4 down).</summary>
        SectionStage,
        /// <summary>Nobody has stood on section Key for Amount seconds.</summary>
        OffSection,
        /// <summary>Section Key is braced (a support jack is under it).</summary>
        SectionBraced,
        /// <summary>Section Key has collapsed.</summary>
        SectionDown,
        /// <summary>A breaker has been thrown.</summary>
        Breaker,
        /// <summary>Someone is hiding.</summary>
        Hide,
        /// <summary>The lesson's monster has gone.</summary>
        ThreatGone,
        /// <summary>The truck holds at least Amount dollars (0 = the quota).</summary>
        Haul,
        /// <summary>The truck has been started (the lever).</summary>
        Lever,
        /// <summary>Someone has a free place among the four shared by hand slots and pockets (met at once when nobody is full).</summary>
        FreeSlot,
    }
}
