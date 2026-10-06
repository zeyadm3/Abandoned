using Abandoned.Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Who holds which carry point of a shared item, as the host decided it, plus each carrier's grip
    /// offset (so every machine computes the same hold target and tether). Fixed size: at most
    /// <see cref="SharedCarryable.MaxPoints"/> points. Holder ids are player NetworkObjectIds; 0 = free.
    /// </summary>
    public struct SharedCarryState : INetworkSerializeByMemcpy
    {
        public ulong Holder0, Holder1, Holder2, Holder3;
        public Vector3 Grip0, Grip1, Grip2, Grip3;

        public ulong HolderAt(int point) => point switch
        {
            0 => Holder0,
            1 => Holder1,
            2 => Holder2,
            3 => Holder3,
            _ => 0,
        };

        public Vector3 GripAt(int point) => point switch
        {
            0 => Grip0,
            1 => Grip1,
            2 => Grip2,
            3 => Grip3,
            _ => Vector3.zero,
        };

        public void Set(int point, ulong holder, Vector3 grip)
        {
            if (holder == 0) grip = Vector3.zero;
            switch (point)
            {
                case 0: Holder0 = holder; Grip0 = grip; break;
                case 1: Holder1 = holder; Grip1 = grip; break;
                case 2: Holder2 = holder; Grip2 = grip; break;
                case 3: Holder3 = holder; Grip3 = grip; break;
            }
        }

        public int Count
        {
            get
            {
                int n = 0;
                for (int i = 0; i < SharedCarryable.MaxPoints; i++) if (HolderAt(i) != 0) n++;
                return n;
            }
        }

        public int IndexOf(ulong holder)
        {
            if (holder == 0) return -1;
            for (int i = 0; i < SharedCarryable.MaxPoints; i++) if (HolderAt(i) == holder) return i;
            return -1;
        }

        public override string ToString() => Count == 0 ? "no carriers" : $"carriers #{Holder0}/#{Holder1}/#{Holder2}/#{Holder3}";
    }
}
