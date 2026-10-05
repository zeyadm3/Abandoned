using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Gives a debris root's chunks back to the global budget when it's removed.</summary>
    public class DebrisLifetime : MonoBehaviour
    {
        private int chunks;

        public void Track(int count) => chunks = count;

        private void OnDestroy() => DebrisSpawner.Release(chunks);
    }
}
