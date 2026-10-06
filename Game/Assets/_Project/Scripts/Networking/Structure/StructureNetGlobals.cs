using Unity.Netcode;

namespace Abandoned.Networking
{
    /// <summary>Building-wide structure state from the host: stability, pre-damage seed, re-roll generation, layout check.</summary>
    public struct StructureNetGlobals : INetworkSerializeByMemcpy
    {
        public float Stability;
        public int Seed;
        /// <summary>Bumped by every host re-roll/stability change: clients restore every section first.</summary>
        public int Generation;
        public int SectionCount;
        /// <summary>StructureSimulation.LayoutHash on the host; a client with a different building refuses to guess.</summary>
        public int LayoutHash;
    }
}
