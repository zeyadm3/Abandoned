using System.Collections.Generic;

namespace Abandoned.Core
{
    /// <summary>
    /// Something that weighs on the structure (a player and what they carry, resting loot, later
    /// monsters). The structure system finds what's under each point and splits the weight evenly
    /// across the points that land on a structural section — the logical load model, not physics contacts.
    /// </summary>
    public interface ILoadSource
    {
        /// <summary>Gameplay kg currently pressing down; 0 when airborne, held by someone, pocketed...</summary>
        float LoadWeight { get; }

        /// <summary>
        /// Adds the points to sample beneath (feet, footprint corners), placed at or above the body's
        /// own bottom so the downward ray never starts inside the section it rests on (sloped ramps).
        /// Called only when LoadWeight > 0.
        /// </summary>
        void GetLoadPoints(List<LoadPoint> points);
    }
}
