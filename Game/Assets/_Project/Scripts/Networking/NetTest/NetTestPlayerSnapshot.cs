using System;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>Where one machine sees one player (JsonUtility-friendly).</summary>
    [Serializable]
    public class NetTestPlayerSnapshot
    {
        public ulong owner;
        public Vector3 position;

        public NetTestPlayerSnapshot() { }

        public NetTestPlayerSnapshot(ulong owner, Vector3 position)
        {
            this.owner = owner;
            this.position = position;
        }
    }
}
