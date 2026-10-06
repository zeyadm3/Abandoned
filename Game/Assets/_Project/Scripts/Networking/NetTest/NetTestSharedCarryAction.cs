using System;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>What one carrier did in the 'sharedcarry' scenario, as its own machine saw it.</summary>
    [Serializable]
    public class NetTestSharedCarryAction
    {
        public ulong client;
        public ulong item;
        /// <summary>The host gave this client a handle.</summary>
        public bool grabbed;
        /// <summary>This client's copy of the item showed the full crew (lifted) while carrying.</summary>
        public bool sawLifted;
        /// <summary>The host confirmed the let-go.</summary>
        public bool letGo;
        public Vector3 carrierStart;
        public Vector3 carrierEnd;
    }
}
