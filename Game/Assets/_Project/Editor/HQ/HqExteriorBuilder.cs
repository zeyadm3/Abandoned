using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>The streets and vacant industrial estate surrounding the salvage company.</summary>
    public static class HqExteriorBuilder
    {
        public const string GroupName = "IndustrialEstateSurroundings";

        public static void Build(Transform root)
        {
            Transform previous = root.Find(GroupName);
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            Transform exterior = Group(GroupName, root);
            HqDistrictLandscape.Build(exterior);
            HqDistrictBuildings.Build(exterior);
            HqWorldText.Remove(exterior);
        }
    }
}
