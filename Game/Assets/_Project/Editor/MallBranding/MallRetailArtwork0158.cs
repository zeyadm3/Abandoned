using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>Former retail campaigns remain in their windows while the clearance crew strips the mall.</summary>
    public static class MallRetailArtwork0158
    {
        public const string GroupName = "_RETAIL_ARTWORK_0158";

        public static int Build(Transform mall)
        {
            MallRetailArtworkMaterials0158.Prepare();
            Transform previous = mall.Find(GroupName);
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            Transform root = Group(GroupName, mall);
            root.gameObject.AddComponent<WalkThroughObject>().EditorSetup(
                "Printed paper, vinyl, suspension cords and shallow sign rails are nonblocking retail dressing.");
            int count = 0;
            foreach (MallLayout.Door door in MallLayout.Doors)
            {
                MallLayout.Zone? first = MallLayout.ZoneAt(door.A, door.Floor);
                MallLayout.Zone? second = MallLayout.ZoneAt(door.B, door.Floor);
                if (!first.HasValue || !second.HasValue) continue;
                bool storeFirst = first.Value.Kind == MallLayout.Kind.Store;
                MallLayout.Zone zone = storeFirst ? first.Value : second.Value;
                string brand = Brand(zone.Tag);
                if (brand == null) continue;
                Vector2Int cell = storeFirst ? door.A : door.B, outside = storeFirst ? door.B : door.A;
                Vector2Int direction = outside - cell;
                Transform tenant = Group(brand + "_Floor_" + door.Floor, root);
                Vector3 at = Boundary(cell, direction, door.Floor);
                Transform entry = Mount("EntranceIdentity", tenant, mall, at, direction, cell, door.Floor);
                // The shutter housing shares this header; the small door badge stays above its clear aperture.
                Graphic(entry, brand + "_Logo", new Vector3(0, 3.555f, .175f), new Vector2(1.08f, .36f), "FasciaPrint");
                count++;

                List<Vector2Int> windows = Windows(zone, direction, door.Floor, cell);
                for (int n = 0; n < Mathf.Min(2, windows.Count); n++)
                {
                    Vector2Int window = windows[n];
                    Transform display = Mount("WindowCampaign_" + n, tenant, mall,
                        Boundary(window, direction, door.Floor), direction, window, door.Floor);
                    // Glazing spans local z=-0.015..0.065. Paper and its tape stay entirely inside the shop.
                    Vector3 printAt = new Vector3(n == 0 ? -.98f : .98f, 1.80f, -.145f);
                    Graphic(display, brand + "_Sale", printAt, new Vector2(.86f, 1.29f), "WindowPaper");
                    Graphic(display, "PaperReverse", printAt - Vector3.forward * .003f,
                        new Vector2(.86f, 1.29f), "WindowPaper", true);
                    Tape(display, printAt, new Vector2(.86f, 1.29f), n);
                    count++;
                    if (n != 0) continue;
                    Graphic(display, brand + "_Logo", new Vector3(0, 3.37f, .168f), new Vector2(1.8f, .6f), "FasciaPrint");
                    count++;
                    Vector3 hangingAt = MallLayout.TileTopCenter(window, door.Floor)
                        - new Vector3(direction.x, 0, direction.y) * .25f + Vector3.up * 3.00f;
                    Transform hanging = Mount("ShowroomHangingCampaign", tenant, mall, hangingAt,
                        direction, window, door.Floor == 0 ? 1 : door.Floor);
                    Hanging(hanging, brand + "_Sale", new Vector2(.66f, .99f), .67f);
                    count++;
                }
                InteriorPrints(mall, tenant, zone, brand, door.Floor);
                count += 2;
            }
            count += Court(mall, root);
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, 0);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Mall0158] Installed {count} generated retail artwork placements, five tenant entrances and both Nordic Habitat floors.");
            return count;
        }

        private static string Brand(string tag) => tag switch
        {
            "electronics" => "ApexCircuit", "furniture" => "NordicHabitat",
            "hardware" => "TitanTools", "clothing" => "VogueThread", _ => null
        };

        private static List<Vector2Int> Windows(MallLayout.Zone zone, Vector2Int face, int floor, Vector2Int doorCell)
        {
            var result = new List<Vector2Int>();
            for (int x = zone.Area.xMin; x < zone.Area.xMax; x++)
            for (int z = zone.Area.yMin; z < zone.Area.yMax; z++)
            {
                var cell = new Vector2Int(x, z);
                MallLayout.Zone? inside = MallLayout.ZoneAt(cell, floor), outside = MallLayout.ZoneAt(cell + face, floor);
                if (!inside.HasValue || inside.Value.Name != zone.Name || !outside.HasValue ||
                    outside.Value.Kind == MallLayout.Kind.Store || MallLayout.DoorBetween(cell, cell + face, floor).HasValue) continue;
                result.Add(cell);
            }
            result.Sort((a, b) =>
            {
                int distance = Distance(a, doorCell).CompareTo(Distance(b, doorCell));
                return distance != 0 ? distance : a.y != b.y ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x);
            });
            return result;
        }

        private static int Distance(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        private static Vector3 Boundary(Vector2Int cell, Vector2Int side, int floor)
            => MallLayout.TileTopCenter(cell, floor) + new Vector3(side.x, 0, side.y) * (MallLayout.Tile * .5f);

        private static void InteriorPrints(Transform mall, Transform parent, MallLayout.Zone zone, string brand, int floor)
        {
            // South perimeter stock ends below these prints; no poster protrudes into its display cabinets.
            float front = zone.Area.yMin * MallLayout.Tile + .17f;
            float width = zone.Area.width * MallLayout.Tile;
            for (int n = 0; n < 2; n++)
            {
                float x = zone.Area.xMin * MallLayout.Tile + width * (n == 0 ? .25f : .75f);
                var cell = new Vector2Int(Mathf.FloorToInt(x / MallLayout.Tile), zone.Area.yMin);
                Transform wall = Mount("InteriorCampaign_" + n, parent, mall,
                    new Vector3(x, MallLayout.FloorY(floor) + 2.89f, front), Vector2Int.up, cell, floor);
                Vector2 size = n == 0 ? new Vector2(2.1f, .7f) : new Vector2(.62f, .93f);
                Graphic(wall, brand + (n == 0 ? "_Logo" : "_Sale"), Vector3.zero, size, n == 0 ? "FasciaPrint" : "WindowPaper");
                Rail(wall, new Vector3(0, size.y * .5f + .016f, -.006f), new Vector3(size.x + .05f, .027f, .022f));
                Rail(wall, new Vector3(0, -size.y * .5f - .016f, -.006f), new Vector3(size.x + .05f, .027f, .022f));
            }
        }

        private static int Court(Transform mall, Transform root)
        {
            // Supersede the old blank cloth strips instead of stacking new artwork through them.
            foreach (Transform existing in mall.GetComponentsInChildren<Transform>(true))
                if (existing != null && existing.name == "FadedUnletteredBanner") Object.DestroyImmediate(existing.gameObject);
            Transform front = Mount("MeridianMainEntranceIdentity", root, mall,
                new Vector3(28, 6.60f, -.23f), Vector2Int.down, new Vector2Int(6, 0), 1);
            Graphic(front, "MeridianMall_Logo", Vector3.zero, new Vector2(3.6f, 1.2f), "FasciaPrint");
            Rail(front, new Vector3(0, .615f, -.006f), new Vector3(3.65f, .028f, .025f));
            Rail(front, new Vector3(0, -.615f, -.006f), new Vector3(3.65f, .028f, .025f));
            foreach (var slot in new[]
            {
                (new Vector3(25.3f, 7.25f, 21.0f), 16f, new Vector2Int(4, 5), new Vector2(1.2f, 1.8f), 1.80f),
                (new Vector3(30.7f, 7.25f, 29.8f), -12f, new Vector2Int(9, 7), new Vector2(1.2f, 1.8f), 1.80f),
                (new Vector3(28f, 7.00f, 12.0f), 0f, new Vector2Int(6, 2), new Vector2(.9f, 1.35f), .72f)
            })
            {
                Transform banner = Mount("MeridianSuspendedSale", root, mall, slot.Item1,
                    Vector2Int.down, slot.Item3, 1);
                banner.localRotation *= Quaternion.Euler(0, slot.Item2, 0);
                // Bottom edge is above 6.3 m, including the upper gallery's 4 m walking surface.
                Hanging(banner, "MeridianMall_Sale", slot.Item4, slot.Item5);
            }
            return 4;
        }

        private static Transform Mount(string name, Transform parent, Transform mall, Vector3 at,
            Vector2Int face, Vector2Int cell, int supportFloor)
        {
            Transform mount = Group(name, parent);
            mount.localPosition = at;
            mount.localRotation = Quaternion.LookRotation(new Vector3(face.x, 0, face.y));
            StructuralSection support = MeridianDressingUtility.Support(mall, cell, supportFloor);
            if (support != null) mount.gameObject.AddComponent<SectionProp>().EditorSetup(support);
            return mount;
        }

        private static void Graphic(Transform parent, string artwork, Vector3 at, Vector2 size, string surface,
            bool reverse = false)
        {
            Transform print = Group(artwork + (reverse ? "_Reverse" : "_Print"), parent);
            print.localPosition = at;
            print.localRotation = reverse ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            print.localScale = new Vector3(size.x, size.y, 1);
            print.gameObject.AddComponent<MeshFilter>().sharedMesh = MallRetailArtworkMesh0158.Surface(surface);
            MeshRenderer renderer = print.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = MallRetailArtworkMaterials0158.Get(artwork);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }

        private static void Hanging(Transform parent, string artwork, Vector2 size, float anchor)
        {
            Graphic(parent, artwork, new Vector3(0, 0, .008f), size, "HangingFabric");
            Graphic(parent, artwork, new Vector3(0, 0, -.008f), size, "HangingFabric", true);
            foreach (float edge in new[] { -.5f, .5f })
                Rail(parent, new Vector3(0, edge * size.y, 0), new Vector3(size.x + .055f, .026f, .035f));
            float top = size.y * .5f, length = Mathf.Max(.08f, anchor - top);
            foreach (float side in new[] { -1f, 1f })
                Part("SuspensionCord", parent, new Vector3(side * size.x * .38f, top + length * .5f, 0),
                    new Vector3(.008f, length, .008f), "MountingCord");
        }

        private static void Tape(Transform parent, Vector3 at, Vector2 size, int variant)
        {
            foreach (float x in new[] { -1f, 1f })
            foreach (float y in new[] { -1f, 1f })
            {
                GameObject tape = Part("YellowedWindowTape", parent,
                    at + new Vector3(x * size.x * .45f, y * size.y * .47f, .003f), new Vector3(.065f, .027f, .002f), "AgedTape");
                tape.transform.localRotation = Quaternion.Euler(0, 0, x * (variant == 0 ? 17 : -13));
            }
        }

        private static void Rail(Transform parent, Vector3 at, Vector3 size) => Part("CampaignRail", parent, at, size, "DisplayRail");
        private static GameObject Part(string name, Transform parent, Vector3 at, Vector3 size, string material)
            => GreyboxFactory.Box(name, parent, at, size, MallRetailArtworkMaterials0158.Get(material), false);

        private static Transform Group(string name, Transform parent) => GreyboxFactory.Group(name, parent);
    }
}
