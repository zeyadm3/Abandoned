using System;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Structure events other areas react to without depending on the Structure area
    /// (e.g. players standing on a collapsing section go ragdoll).
    /// </summary>
    public static class StructureSignals
    {
        /// <summary>A section collapsed; bounds of its walking surface (world space).</summary>
        public static event Action<Bounds> SectionCollapsed;

        public static void RaiseCollapsed(Bounds surface) => SectionCollapsed?.Invoke(surface);

        /// <summary>A non-physics impact at a point (gameplay kg × m/s), e.g. a player landing from a jump or fall.</summary>
        public static event Action<Vector3, float> Impact;

        public static void RaiseImpact(Vector3 position, float momentum) => Impact?.Invoke(position, momentum);

        /// <summary>A solid rubble mound appeared (world bounds of its solid part); anyone standing in it climbs out on top.</summary>
        public static event Action<Bounds> RubbleLanded;

        public static void RaiseRubbleLanded(Bounds solid) => RubbleLanded?.Invoke(solid);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            SectionCollapsed = null;
            Impact = null;
            RubbleLanded = null;
        }
    }
}
