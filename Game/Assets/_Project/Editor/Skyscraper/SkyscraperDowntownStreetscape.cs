using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using S = Abandoned.EditorTools.SkyscraperDowntownStreets;
using Street = Abandoned.EditorTools.StationStreet;
using V = Abandoned.EditorTools.StationVehicleWear;
using C = Abandoned.EditorTools.StationVehicleKind;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The downtown's street life, scene-only (under <c>Exterior/Skyline</c>): on every block's pavement lamps at the
    /// corners and mid-block, signals on the central junctions, hydrants, news boxes, mailboxes, benches, bins, parking
    /// meters, bus shelters and tree pits, each block merged into one saved mesh with its own level of detail; street trees
    /// in the pits; cars left at the kerbs and taxis among them; Broadway gridlocked outbound where the evacuation stalled,
    /// a police barricade either side of the tower with cruisers and an ambulance; and the few lamps by the plaza still lit.
    /// </summary>
    public static class SkyscraperDowntownStreetscape
    {
        public static void Build(Transform skyline, SkyscraperArt a, SkyscraperDowntownMaterials p, StationArt k, StationRingStock stock)
        {
            Transform street = Group("Streetscape", skyline);
            Transform blocks = Group("BlockFurniture", street), cars = Group("StreetCars", street), trees = Group("StreetTrees", street);
            (GameObject prefab, float height)[] forest = Forest();
            for (int bz = -3; bz <= 3; bz++)
            for (int bx = -3; bx <= 3; bx++)
            {
                if (bx == 0 && bz == 0) continue;
                int seed = (bx + 4) * 173 + (bz + 4) * 71;
                bool central = Mathf.Abs(bx) <= 2 && Mathf.Abs(bz) <= 2;
                var pits = new List<Vector3>();
                int design = stock.Add($"Streetscape_{bx}_{bz}", 86f, 86f, t => Block(t, a, p, k, central, seed, pits));
                var centre = new Vector3(bx * 100f, 0f, bz * 100f + 22f);
                stock.Place(blocks, design, centre, 0f);
                var rng = new System.Random(seed);
                foreach (Vector3 pit in pits) Tree(trees, forest, centre + pit, rng);
            }
            Cars(cars, a, k, stock);
            Barricades(street, a, k, stock);
            foreach ((Vector3 foot, Vector3 toward) in new[]
                     {
                         (new Vector3(-60f, 0f, -19.4f), new Vector3(-60f, 0f, -26f)), (new Vector3(60f, 0f, -19.4f), new Vector3(60f, 0f, -26f)),
                         (new Vector3(-30f, 0f, -36.6f), new Vector3(-30f, 0f, -30f)), (new Vector3(30f, 0f, -36.6f), new Vector3(30f, 0f, -30f)),
                         (new Vector3(-60f, 0f, 63.4f), new Vector3(-60f, 0f, 70f)), (new Vector3(60f, 0f, 63.4f), new Vector3(60f, 0f, 70f)),
                     })
                Street.Lamp(street, k, foot, toward, 8f);
        }

        // One block's pavement furniture in its own frame (the pavement is ±43 m, its top at 0; the podium ±32 x ±31).
        private static void Block(Transform t, SkyscraperArt a, SkyscraperDowntownMaterials p, StationArt k, bool central, int seed, List<Vector3> pits)
        {
            var rng = new System.Random(seed);
            foreach (float sx in new[] { -1f, 1f })
            foreach (float sz in new[] { -1f, 1f })
                if (!central || sx != sz) CityLamp(t, a, p, new Vector3(sx * 40.6f, 0f, sz * 40.6f), sz < 0 ? 0f : 180f); // signals take those corners
            foreach (float s in new[] { -1f, 1f })
            {
                CityLamp(t, a, p, new Vector3(s * 20f, 0f, -40.6f), 0f);
                CityLamp(t, a, p, new Vector3(-s * 20f, 0f, 40.6f), 180f);
                CityLamp(t, a, p, new Vector3(40.6f, 0f, s * 20f), 270f);
                CityLamp(t, a, p, new Vector3(-40.6f, 0f, s * 20f), 90f);
            }
            if (central)
            {
                Street.TrafficLight(t, k, new Vector3(-41.4f, 0f, -41.4f), 180f, 6f);
                Street.TrafficLight(t, k, new Vector3(41.4f, 0f, 41.4f), 0f, 6f);
            }
            Street.Hydrant(t, k, new Vector3(36f + rng.Next(3), 0f, -41.4f), 180f);
            Street.Hydrant(t, k, new Vector3(-41.4f, 0f, 30f - rng.Next(3)), 90f);
            Street.NewsBoxes(t, k, new Vector3(-12f, 0f, -40.2f), 180f);
            Street.Mailbox(t, k, new Vector3(-16f, 0f, -40.4f), 180f);
            foreach (float x in new[] { -26f, 26f })
            {
                Street.Bench(t, k, new Vector3(x, 0f, -39.6f), 180f);
                StationScans.Put(t, "metal_trash_can", new Vector3(x + 2.2f, 0f, -40.4f), 0f, new Vector3(.6f, 1f, .6f), solid: true);
            }
            if (seed % 3 == 0) Street.BusShelter(t, k, new Vector3(14f, 0f, 39.8f), 0f);
            else Street.Bench(t, k, new Vector3(14f, 0f, 39.6f), 0f);
            for (float z = -28f; z <= 28f; z += 8f)
                if (rng.NextDouble() < .6) Street.ParkingMeter(t, k, new Vector3(41.6f, 0f, z), 270f);
            if (rng.NextDouble() < .5) Street.Litter(t, k, new Vector3(rng.Next(-30, 30), 0f, 38f), seed);
            if (rng.NextDouble() < .4) Street.Dumpster(t, k, new Vector3(-37.5f, 0f, -10f + rng.Next(20)), 90f, rng.Next(2) == 0);
            // Tree pits along both avenue fronts (never in front of an entrance), each with its grate.
            foreach (float sz in new[] { -1f, 1f })
                for (float x = -36f; x <= 36f; x += 12f)
                {
                    if (Mathf.Abs(x) < 8f || sz > 0 && Mathf.Abs(x - 14f) < 4f || rng.NextDouble() > .5) continue;
                    var pit = new Vector3(x, 0f, sz * 38.4f);
                    Box("TreeGrate", t, pit + Vector3.up * .006f, new Vector3(1.4f, .012f, 1.4f), k.DarkSteel, false);
                    pits.Add(pit);
                }
        }

        // A city lamp: base, round pole, an arm out over the road and its lantern (emissive, no light).
        private static void CityLamp(Transform t, SkyscraperArt a, SkyscraperDowntownMaterials p, Vector3 at, float yaw)
        {
            Transform g = Group("CityStreetLamp", t);
            g.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Primitive(PrimitiveType.Cylinder, "Base", g, new Vector3(0, .4f, 0), new Vector3(.36f, .4f, .36f), a.GraniteDark, false);
            Primitive(PrimitiveType.Cylinder, "Pole", g, new Vector3(0, 3.6f, 0), new Vector3(.16f, 3.2f, .16f), a.Steel, false);
            Box("Arm", g, new Vector3(0, 6.7f, -.8f), new Vector3(.1f, .1f, 1.7f), a.Steel, false);
            Box("Head", g, new Vector3(0, 6.62f, -1.6f), new Vector3(.45f, .16f, .8f), a.Steel, false);
            Box("Lantern", g, new Vector3(0, 6.53f, -1.6f), new Vector3(.4f, .03f, .7f), p.WindowLight, false);
            var solid = g.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0, 3.4f, 0);
            solid.size = new Vector3(.4f, 6.8f, .4f);
        }

        private static (GameObject prefab, float height)[] Forest()
        {
            var tall = new List<(GameObject, float)>();
            foreach ((GameObject prefab, float height) in MapTrees.Forest(false))
                if (prefab != null && height >= 4f) tall.Add((prefab, height));
            return tall.ToArray();
        }

        private static void Tree(Transform parent, (GameObject prefab, float height)[] forest, Vector3 at, System.Random rng)
        {
            if (forest.Length == 0) return;
            (GameObject prefab, float height) = forest[rng.Next(forest.Length)];
            var tree = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            if (tree == null) return;
            tree.name = "StreetTree";
            tree.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f));
            tree.transform.localScale = Vector3.one * ((6.5f + (float)rng.NextDouble() * 3f) / height);
        }

        // Cars at the kerbs of every avenue but Broadway and one side of every cross street, then Broadway's outbound
        // lanes jammed where the evacuation stopped (from a block past the barricades).
        private static void Cars(Transform s, SkyscraperArt a, StationArt k, StationRingStock stock)
        {
            var designs = new List<int>
            {
                stock.Car(C.Sedan, 601, V.Dusty), stock.Car(C.Hatchback, 603, V.Dusty | V.FlatFront), stock.Car(C.Suv, 605, V.Dusty),
                stock.Car(C.Sedan, 607, V.Dusty | V.Smashed), stock.Car(C.Pickup, 609, V.Dusty | V.DoorOpen), stock.Car(C.Suv, 611, V.Burnt),
                stock.Car(C.Sedan, 613, V.Dusty, a.CarPaintYellow), stock.Car(C.Sedan, 615, V.Dusty | V.DoorOpen, a.CarPaintYellow),
                stock.Car(C.Hatchback, 617, V.Dusty | V.FlatRear), stock.Car(C.Sedan, 619, V.Dusty, a.CarPaintBlack),
            };
            var rng = new System.Random(6100);
            for (int n = -3; n <= 4; n++)
            for (int m = -3; m <= 3; m++)
            {
                float x0 = S.StreetX(m) + S.Half + 12f, x1 = S.StreetX(m + 1) - S.Half - 12f;
                foreach (float side in new[] { -1f, 1f })
                    Kerb(s, stock, designs, rng, true, S.AvenueZ(n) + side * 5.4f, x0, x1, side > 0 ? 270f : 90f, n == 0 ? 1000f : 0f); // Broadway has its jam instead
            }
            for (int m = -3; m <= 4; m++)
            for (int n = -3; n <= 3; n++)
                Kerb(s, stock, designs, rng, false, S.StreetX(m) + 5.4f, S.AvenueZ(n) + S.Half + 12f, S.AvenueZ(n + 1) - S.Half - 12f, 180f, 0f);
            foreach (float dir in new[] { -1f, 1f })
                for (float x = 100f; x < 340f; x += 6.4f + (float)rng.NextDouble() * 2.5f)
                {
                    float wx = dir * x;
                    if (Mathf.Abs(Mathf.Repeat(wx + 50f, 100f) - 50f) > 38f || rng.NextDouble() < .2) continue;
                    stock.Place(s, designs[rng.Next(designs.Count)], new Vector3(wx, SkyscraperDowntown.StreetTop, S.AvenueZ(0) - dir * 2.1f),
                        (dir > 0 ? 90f : 270f) + (float)rng.NextDouble() * 10f - 5f);
                }
        }

        private static void Kerb(Transform s, StationRingStock stock, List<int> designs, System.Random rng, bool alongX, float line, float from, float to,
            float yaw, float clearOfTower)
        {
            for (float u = from + (float)rng.NextDouble() * 4f; u < to; u += 6.5f + (float)rng.NextDouble() * 9f)
            {
                if (Mathf.Abs(u) < clearOfTower || rng.NextDouble() > .32) continue;
                Vector3 at = alongX ? new Vector3(u, SkyscraperDowntown.StreetTop, line) : new Vector3(line, SkyscraperDowntown.StreetTop, u);
                stock.Place(s, designs[rng.Next(designs.Count)], at, yaw + (float)rng.NextDouble() * 4f - 2f);
            }
        }

        // Police barricades across Broadway a block either side of the tower, a gap left in each, cruisers nosed in, an
        // ambulance abandoned with its doors shut.
        private static void Barricades(Transform s, SkyscraperArt a, StationArt k, StationRingStock stock)
        {
            float z = S.AvenueZ(0), top = SkyscraperDowntown.StreetTop;
            int cruiser = stock.Car(C.Cruiser, 631, V.Dusty | V.DoorOpen, k.Plain("PatrolBlack", new Color(.06f, .065f, .08f), .55f, .3f));
            foreach (float dir in new[] { -1f, 1f })
            {
                float x = dir * 76f;
                Street.Barriers(s, new Vector3(x, top, z - 6.4f), new Vector3(x, top, z - .5f), 3);
                Street.Sawhorse(s, k, new Vector3(x + dir * 2.5f, top, z + 3.8f), 90f);
                stock.Place(s, cruiser, new Vector3(x + dir * 7f, top, z - 3.2f), dir > 0 ? 60f : 300f);
                stock.Place(s, cruiser, new Vector3(x + dir * 13f, top, z + 2.6f), dir > 0 ? 115f : 245f);
            }
            Transform ambulance = Group("AbandonedAmbulance", s);
            ambulance.SetPositionAndRotation(new Vector3(-118f, top, z + 5.2f), Quaternion.Euler(0f, 268f, 0f));
            StationApparatus.BoxVan(ambulance, k, k.White, k.Red, 5);
            var solid = ambulance.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, 1.4f, 0f);
            solid.size = new Vector3(2.3f, 2.8f, 6.2f);
        }
    }
}
