#if UNITY_EDITOR || DEVELOPMENT_BUILD
// The nettest harness ships only in the editor and Development builds (Tools/unity.sh nettest uses a dev build).
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

#endif
