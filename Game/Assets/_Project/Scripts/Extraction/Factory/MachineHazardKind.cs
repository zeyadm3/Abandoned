namespace Abandoned.Extraction
{
    /// <summary>What a running factory machine does (<see cref="MachineHazard"/>). Append only: scenes store the number.</summary>
    public enum MachineHazardKind : byte
    {
        /// <summary>Slams on a cycle: a striped zone behind it is crushed.</summary>
        Press,
        /// <summary>Mill-stand rolls: anyone riding the roller table into the bite is caught.</summary>
        Rollers,
        /// <summary>Vents steam on a cycle: the ring around it scalds.</summary>
        Steam,
        /// <summary>A moving belt or roller table: it carries whoever stands on it.</summary>
        Belt,
    }
}
