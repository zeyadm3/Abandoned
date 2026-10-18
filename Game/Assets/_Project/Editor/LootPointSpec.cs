using Abandoned.Interaction;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>One authored salvage point: what <see cref="LocationPlumbing.Populate"/> turns into a LootSpawnPoint.</summary>
    public readonly struct LootPointSpec
    {
        public readonly string Name, Tag;
        public readonly Vector3 At;
        public readonly CarryClass Min, Max;
        public readonly bool IsJackpot;

        public LootPointSpec(string name, string tag, Vector3 at, CarryClass min, CarryClass max, bool isJackpot)
        { Name = name; Tag = tag; At = at; Min = min; Max = max; IsJackpot = isJackpot; }
    }
}
