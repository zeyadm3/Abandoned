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
