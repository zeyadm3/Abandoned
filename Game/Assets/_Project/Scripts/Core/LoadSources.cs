using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>Registry of active load sources; components register in OnEnable and leave in OnDisable.</summary>
    public static class LoadSources
    {
        private static readonly List<ILoadSource> Sources = new();

        public static IReadOnlyList<ILoadSource> All => Sources;

        public static void Register(ILoadSource source)
        {
            if (!Sources.Contains(source)) Sources.Add(source);
        }

        public static void Unregister(ILoadSource source) => Sources.Remove(source);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Sources.Clear();
    }
}
