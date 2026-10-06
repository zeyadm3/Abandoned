using System;

namespace Abandoned.Networking
{
    /// <summary>
    /// Client-side: one outstanding request of each kind per item. A snagged item asks to be dropped
    /// every physics step until the host answers; without this each step would be another RPC.
    /// Cleared when the host's answer (a new hold state) arrives.
    /// </summary>
    public sealed class LootRequestGuard
    {
        private readonly float[] sentAt = new float[Enum.GetValues(typeof(LootRequest)).Length];

        public LootRequestGuard() => Clear();

        public bool TryBegin(LootRequest request, float now, float repeatGuard)
        {
            int i = (int)request;
            if (now - sentAt[i] < repeatGuard) return false;
            sentAt[i] = now;
            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < sentAt.Length; i++) sentAt[i] = float.NegativeInfinity;
        }
    }
}
