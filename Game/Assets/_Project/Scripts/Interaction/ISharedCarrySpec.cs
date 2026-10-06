using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// Per-item data for carrying together (loot gets it from its LootDefinition), so the Interaction
    /// area doesn't depend on Loot. Empty authored points mean "generate them from the size".
    /// </summary>
    public interface ISharedCarrySpec
    {
        /// <summary>Players needed to lift it; 0 = the carry class default.</summary>
        int RequiredCarriersOverride { get; }
        IReadOnlyList<Vector3> AuthoredCarryPoints { get; }
        /// <summary>Local bounding size, centred on the pivot.</summary>
        Vector3 CarrySize { get; }
    }
}
