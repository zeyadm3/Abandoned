using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Soft places (0.17.1, the crash mat): a box in the world that takes most of the speed out of anything
    /// landing in it. Loot impacts and players' falls ask <see cref="At"/>; the host decides both.
    /// </summary>
    public static class ImpactCushion
    {
        private static readonly List<(Component owner, Bounds box, float factor)> Pads = new();

        public static void Add(Component owner, Bounds box, float factor)
        {
            Remove(owner);
            Pads.Add((owner, box, factor));
        }

        public static void Remove(Component owner) => Pads.RemoveAll(p => p.owner == owner || p.owner == null);

        /// <summary>What an impact or a fall at this point keeps of its speed (1 = all of it).</summary>
        public static float At(Vector3 point)
        {
            float best = 1f;
            foreach ((Component owner, Bounds box, float factor) in Pads)
                if (owner != null && box.Contains(point)) best = Mathf.Min(best, factor);
            return best;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Pads.Clear();
    }
}
