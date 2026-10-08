using System;

namespace Abandoned.Company
{
    /// <summary>What the host lets the rest of the crew do (QA B-08). The host can always do all of it.</summary>
    [Flags]
    public enum CrewRule : byte
    {
        None = 0,
        /// <summary>Buy gear, truck upgrades and wardrobe items with company money.</summary>
        Spend = 1,
        /// <summary>Start the van to a job from the HQ.</summary>
        Drive = 2,
        /// <summary>Pull the truck's lever to leave a job.</summary>
        Lever = 4,
    }
}
