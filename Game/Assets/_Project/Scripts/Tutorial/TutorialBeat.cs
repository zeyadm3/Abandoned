namespace Abandoned.Tutorial
{
    /// <summary>
    /// What the host does to the house when a lesson begins (<see cref="TutorialStepDefinition.Beat"/>): the
    /// scripted moments. BeatKey names the spot or section; BeatAmount is the door number or a delay.
    /// Append only: step assets store the number.
    /// </summary>
    public enum TutorialBeat
    {
        None,
        /// <summary>The lesson's monster comes in at spot BeatKey and wanders toward the "threat_patrol" spot.</summary>
        SpawnThreat,
        /// <summary>After BeatAmount seconds (0 = the config's) the monster walks out to spot BeatKey and is gone.</summary>
        ThreatLeave,
        /// <summary>Section BeatKey gives way the moment someone stands on it.</summary>
        ArmCollapse,
        /// <summary>Section BeatKey gives way now.</summary>
        Collapse,
        /// <summary>Door number BeatAmount slides shut.</summary>
        ShutDoor,
        /// <summary>Door number BeatAmount slides open.</summary>
        OpenDoor,
    }
}
