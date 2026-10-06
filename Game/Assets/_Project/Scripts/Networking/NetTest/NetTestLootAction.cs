using System;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>What a client did with its test item in the 'loot' scenario, as it saw it.</summary>
    [Serializable]
    public class NetTestLootAction
    {
        public ulong client;
        public ulong item;
        /// <summary>The host granted the pickup and handed this client the item's physics.</summary>
        public bool gotOwnership;
        /// <summary>After the throw the host took the physics back.</summary>
        public bool returnedOwnership;
        public Vector3 start;
        public Vector3 released;
    }
}
