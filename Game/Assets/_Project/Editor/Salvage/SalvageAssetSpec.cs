using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;

namespace Abandoned.EditorTools
{
    /// <summary>Initial authoring recipe; generated LootDefinitions remain the editable gameplay data.</summary>
    public sealed class SalvageAssetSpec
    {
        public readonly string Id, Name, Tag;
        public readonly SalvageCategory Category;
        public readonly float Weight;
        public readonly int ValueMin, ValueMax;
        public readonly CarryClass Carry;
        public readonly SurfaceMaterial Surface;
        public readonly Fragility Fragility;
        public readonly bool IsLoot, HasLods;
        public string DefinitionId => "salvage_" + Id;

        public SalvageAssetSpec(string id, string name, SalvageCategory category, float weight,
            int min, int max, CarryClass carry, string tag, SurfaceMaterial surface,
            Fragility fragility = Fragility.Low, bool isLoot = true, bool hasLods = true)
        {
            Id = id; Name = name; Category = category; Weight = weight; ValueMin = min; ValueMax = max;
            Carry = carry; Tag = tag; Surface = surface; Fragility = fragility; IsLoot = isLoot; HasLods = hasLods;
        }
    }
}
