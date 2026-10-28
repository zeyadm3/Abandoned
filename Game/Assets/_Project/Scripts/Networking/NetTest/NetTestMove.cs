#if UNITY_EDITOR || DEVELOPMENT_BUILD
// The nettest harness ships only in the editor and Development builds (Tools/unity.sh nettest uses a dev build).
using System;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>A scripted move as its owner saw it: where it started and where it stopped.</summary>
    [Serializable]
    public class NetTestMove
    {
        public ulong owner;
        public Vector3 start;
        public Vector3 end;

        public float HorizontalDistance
        {
            get
            {
                Vector3 d = end - start;
                d.y = 0f;
                return d.magnitude;
            }
        }
    }
}

#endif
