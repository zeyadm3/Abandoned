using Abandoned.Structure;
using UnityEngine;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Floor-mounted interior fittings belong to the dining slab that carries them.</summary>
    public static class IslandInteriorSupport
    {
        public static void Bind(Transform root)
        {
            BindRoom("Interiors/DiningHall", kitchen: false);
            BindRoom("Interiors/Kitchen", kitchen: true);

            void BindRoom(string path, bool kitchen)
            {
                Transform room = root.Find(path);
                if (room == null) return;
                foreach (Transform item in room)
                {
                    string name = item.name;
                    bool floorMounted = kitchen ? name.StartsWith("Range") || name == "Prep"
                        : name.StartsWith("Table_") || name.StartsWith("Serving") || name is "Tray" or "Litter";
                    if (!floorMounted) continue;
                    int x = Mathf.FloorToInt((item.position.x - L.Cellhouse.xMin) / L.Tile);
                    int z = Mathf.FloorToInt((item.position.z - L.Cellhouse.yMin) / L.Tile);
                    Transform tile = root.Find($"Levels/GroundFloor/Tile_{x}_{z}");
                    StructuralSection section = tile != null ? tile.GetComponent<StructuralSection>() : null;
                    if (section == null || !section.CanCollapse) continue;
                    // Keep the furniture outside the section hierarchy: StructuralSection includes descendant
                    // colliders in its surface bounds, which would turn the table top into the slab's floor height.
                    // SectionProp owns visibility and collision when the supporting slab collapses or restores.
                    SectionProp prop = item.GetComponent<SectionProp>() ?? item.gameObject.AddComponent<SectionProp>();
                    prop.EditorSetup(section);
                }
            }
        }
    }
}
