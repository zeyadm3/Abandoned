using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Gives a debris root's chunks back to the global budget when it's removed, and remembers how the
    /// break started (each chunk's launch velocity) so machines can be compared for the same seed.
    /// </summary>
    public class DebrisLifetime : MonoBehaviour
    {
        private int chunks;
        private readonly List<Vector3> launches = new();

        /// <summary>Launch velocity of each chunk, in spawn order.</summary>
        public IReadOnlyList<Vector3> Launches => launches;

        public void Track(int count) => chunks = count;

        internal void RecordLaunch(Vector3 velocity) => launches.Add(velocity);

        private void OnDestroy() => DebrisSpawner.Release(chunks);
    }
}
