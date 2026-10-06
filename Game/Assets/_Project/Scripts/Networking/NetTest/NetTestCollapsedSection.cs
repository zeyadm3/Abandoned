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
