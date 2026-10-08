using System.Linq;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Manufactured landmarks announce the cinema and food court before the crew enters them.</summary>
    public static class MeridianDressingLandmarks
    {
        public static void Place(Transform mall, Transform group)
        {
            Cinema(mall, Group("FinalReel", group));
            FoodCourt(mall, Group("FoodForThought", group));
        }

        private static void Cinema(Transform mall, Transform group)
        {
            foreach (MallLayout.Door door in MallLayout.Doors)
            {
                if (door.Floor != 2) continue;
                MallLayout.Zone? a = MallLayout.ZoneAt(door.A, door.Floor), b = MallLayout.ZoneAt(door.B, door.Floor);
                if (!a.HasValue || !b.HasValue) continue;
                bool cinemaA = a.Value.Tag == "cinema", cinemaB = b.Value.Tag == "cinema";
                if (cinemaA == cinemaB || (cinemaA ? b.Value.Tag : a.Value.Tag) == "stock") continue;
                Vector2Int direction = cinemaA ? door.B - door.A : door.A - door.B;
                Vector3 front = new(direction.x, 0f, direction.y);
                Vector3 at = (MallLayout.TileTopCenter(door.A, 2) + MallLayout.TileTopCenter(door.B, 2)) * 0.5f;
                Vector3 basePosition = at + front * 0.22f + Vector3.up * (MallWalls.WideHeight + 0.01f);
                GameObject marquee = Prop("CinemaMarquee", group, basePosition, Quaternion.LookRotation(front), null,
                    new Vector3(1f, 0.30f, 1f));
                if (marquee != null)
                    marquee.AddComponent<LightFixture>().EditorSetup(null, marquee.GetComponentsInChildren<Renderer>(true).FirstOrDefault(),
                        isFaulty: true, isDead: false, fixtureSeed: 67);
                MeridianDressingSigns.Sign(group, basePosition + front * 0.33f + Vector3.up * 0.165f,
                    front, "CINEMA", 1.8f, 0.20f);
                break;
            }
        }

        private static void FoodCourt(Transform mall, Transform group)
        {
            for (int floor = 0; floor < MallLayout.Floors; floor++)
            foreach (MallLayout.Zone zone in MallLayout.ZonesByFloor[floor])
            {
                if (zone.Tag != "food") continue;
                for (int x = zone.Area.xMin; x < zone.Area.xMax; x++)
                {
                    int row = zone.Area.yMin;
                    while (row < zone.Area.yMax && MallLayout.ZoneAt(new Vector2Int(x, row), floor)?.Name != zone.Name) row++;
                    if (row >= zone.Area.yMax) continue;
                    var cell = new Vector2Int(x, row);
                    StructuralSection ceiling = floor + 1 < MallLayout.Floors ? Support(mall, cell, floor + 1) : null;
                    if (floor + 1 < MallLayout.Floors && ceiling == null) continue;
                    Vector3 at = new((x + 0.5f) * MallLayout.Tile, MallLayout.FloorY(floor) + 3.58f, row * MallLayout.Tile);
                    GameObject bulkhead = Box(group, at, new Vector3(MallLayout.Tile, 0.40f, 0.45f));
                    if (ceiling != null) bulkhead.AddComponent<SectionProp>().EditorSetup(ceiling);
                    if (x == zone.Area.xMin + zone.Area.width / 2)
                    {
                        MeridianDressingSigns.Sign(group, at + Vector3.back * 0.255f, Vector3.back,
                            "FOOD COURT", 2f, 0.20f);
                        Transform sign = group.GetChild(group.childCount - 1);
                        if (ceiling != null) sign.gameObject.AddComponent<SectionProp>().EditorSetup(ceiling);
                    }
                }
            }
        }

        private static GameObject Box(Transform group, Vector3 at, Vector3 size)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "FoodCourtBulkhead";
            box.transform.SetParent(group, false);
            box.transform.localPosition = at;
            box.transform.localScale = size;
            Object.DestroyImmediate(box.GetComponent<Collider>());
            box.GetComponent<Renderer>().sharedMaterial = MeridianArt.Material("StorefrontTrim");
            return box;
        }
    }
}
