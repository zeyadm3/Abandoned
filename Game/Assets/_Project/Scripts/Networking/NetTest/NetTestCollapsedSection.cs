#if UNITY_EDITOR || DEVELOPMENT_BUILD
// The nettest harness ships only in the editor and Development builds (Tools/unity.sh nettest uses a dev build).
using System;

namespace Abandoned.Networking
{
    /// <summary>One collapsed section as a machine sees it.</summary>
    [Serializable]
    public class NetTestCollapsedSection
    {
        public int id;
        public string name;
        public int seed;
        public bool collidersOff;
    }
}

#endif
