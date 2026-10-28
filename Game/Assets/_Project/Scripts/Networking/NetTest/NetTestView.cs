#if UNITY_EDITOR || DEVELOPMENT_BUILD
// The nettest harness ships only in the editor and Development builds (Tools/unity.sh nettest uses a dev build).
using System;
using System.Collections.Generic;

namespace Abandoned.Networking
{
    /// <summary>Every player one machine (<see cref="observer"/>) sees at one moment.</summary>
    [Serializable]
    public class NetTestView
    {
        public ulong observer;
        public List<NetTestPlayerSnapshot> players = new();
    }
}

#endif
