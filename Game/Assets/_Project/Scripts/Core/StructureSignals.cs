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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => SectionCollapsed = null;
    }
}
