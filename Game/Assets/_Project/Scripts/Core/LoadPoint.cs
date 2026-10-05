using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>One sample point where a load source presses down on whatever structure is beneath it.</summary>
    public readonly struct LoadPoint
    {
        public readonly Vector3 Position;

        public LoadPoint(Vector3 position) => Position = position;
    }
}
