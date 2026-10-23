using System.Collections.Generic;
using Abandoned.EditorTools;
using Abandoned.Structure;
using UnityEngine;
using P = Abandoned.EditorTools.StationPlan;
namespace Abandoned.Hospital
{
    public static class HospitalLighting
    {
        public static void Build(Transform parent, HospitalStructure floors)
        {
            StationContext c = floors.Context;
            Transform root = HospitalKit.Group("Lights", parent);
            var lit = new HashSet<P.Room>();
            int id = 0;
            foreach (var floor in c.Plan.Floors)
            {
                foreach (Vector2Int cell in floor.Value)
                {
                    int f = floor.Key;
                    if ((cell.x + cell.y) % 3 != 0)
                    {
                        continue;
                    }
                    bool atrium = f == 0 && cell.x >= 6 && cell.x <8 && cell.y <9;
                    if (c.Plan.IsHole(f + 1, cell) && ! atrium)
                    {
                        continue;
                    }
                    Vector3 at = P.CellCentre(cell, f);
                    at.y = atrium ? 7.75f :(f + 1) * 4f - .25f;
                    int roll = Mathf.Abs(cell.x * 11 + cell.y * 7 + f * 13) % 100;
                    StationLighting.Troffer(root, "ClinicalFixture_" + id, at, c.Art, new Color(.83f, .92f, .88f), roll, 17023 + id++, false);
                    P.Room room = c.Plan.RoomAt(f, cell);
                    if (room != null)
                    {
                        lit.Add(room);
                    }
                }
            }
            foreach (P.Room room in c.Plan.Rooms)
            {
                if (lit.Contains(room))
                {
                    continue;
                }
                Vector3 at = room.Centre;
                at.y = room.Y + 3.75f;
                StationLighting.Troffer(root, "RoomFixture_" + id, at, c.Art, new Color(.9f, .91f, .85f), 55, 17023 + id++, true);
            }
            foreach (LightFixture fixture in root.GetComponentsInChildren<LightFixture>())
            {
                StructuralSection ceiling = floors.At(fixture.transform.position);
                if (ceiling != null && ceiling.CanCollapse)
                {
                    fixture.gameObject.AddComponent<SectionProp>().EditorSetup(ceiling);
                }
            }
        }
    }
}
