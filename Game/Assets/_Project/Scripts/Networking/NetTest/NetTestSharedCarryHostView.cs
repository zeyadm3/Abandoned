using System;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>What the host (who simulates a shared carry) measured during the 'sharedcarry' scenario.</summary>
    [Serializable]
    public class NetTestSharedCarryHostView
    {
        public ulong item;
        public int maxCarriers;
        /// <summary>Highest the item's bottom rose above where it rested (m).</summary>
        public float maxLift;
        /// <summary>Hold targets accepted from the carriers' clients.</summary>
        public int targetsReceived;
        public int endCarriers;
        public Vector3 start;
        public Vector3 end;
    }
}
