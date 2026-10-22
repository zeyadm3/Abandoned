using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.ShowcaseSitePlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The TestMap yard's furniture (ShowcaseSitePlan has the plot): the chain-link fence and its gate, the power line
    /// coming up the road on wooden poles with its wires sagging between them, scanned rocks and fallen logs where the
    /// forest meets the fence, the yard's clutter, and the warehouse's outside fittings (gutters, downpipes, lamps).
    /// </summary>
    public static class ShowcaseSite
    {
        private const string Fence = "modular_chainlink_fence";

        public static void Build(Transform root)
        {
            Transform site = Group("Site", root);
            FenceLine(Group("Fence", site));
            PowerLine(Group("PowerLine", site));
            Wilds(Group("Wilds", site));
            Clutter(Group("Clutter", site));
            Facade(Group("Facade", site));
            MarkStatic(site);
        }

        private static void FenceLine(Transform fence)
        {
            var corners = new[] { new Vector2(Yard.xMin, Yard.yMin), new Vector2(Yard.xMax, Yard.yMin), new Vector2(Yard.xMax, Yard.yMax), new Vector2(Yard.xMin, Yard.yMax) };
            for (int i = 0; i < 4; i++)
            {
                Vector2 a = corners[i], b = corners[(i + 1) % 4];
                if (i == 0)
                {
                    // The south side is broken by the gate.
                    Run(fence, a, new Vector2(Gate.xMin, a.y));
                    Run(fence, new Vector2(Gate.xMax, a.y), b);
                }
                else Run(fence, a, b);
            }
            foreach (Vector2 c in corners) ShowcaseNature.Part(Fence, $"{Fence}_post", fence, new Vector3(c.x, Ground, c.y), 0f);
            // The gate: one leaf left standing open against the fence, the other gone; concrete blocks across the verge.
            GameObject leaf = ShowcaseNature.Part(Fence, $"{Fence}_door_gate", fence, new Vector3(Gate.xMax + .6f, Ground, Gate.yMin + 1.4f), 70f, 1.6f);
            for (int i = 0; i < 2; i++)
                ShowcaseNature.Part("concrete_road_barrier", null, fence, new Vector3(Gate.xMin - 3f - i * 3.4f, Ground, Gate.yMax + 2f), 6f * i, 1f, solid: true);
        }

        // Panels a little under two metres, a post at every joint; each panel turned so its long side follows the run.
        private static void Run(Transform fence, Vector2 a, Vector2 b)
        {
            float length = Vector2.Distance(a, b);
            int panels = Mathf.Max(1, Mathf.RoundToInt(length / 2.06f));
            float step = length / panels;
            Vector2 dir = (b - a) / length;
            float yaw = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            var random = new System.Random((int)(a.x * 31 + a.y * 7));
            for (int i = 0; i < panels; i++)
            {
                Vector2 c = a + dir * (i + .5f) * step;
                // Here and there a panel has gone, or leans.
                if (random.NextDouble() < .04) continue;
                GameObject panel = ShowcaseNature.Part(Fence, $"{Fence}_double", fence, new Vector3(c.x, Ground, c.y), 0f);
                if (panel == null) return;
                Bounds bounds = ShowcaseModels.LocalBounds(panel.transform);
                float along = bounds.size.x >= bounds.size.z ? 90f : 0f;
                panel.transform.rotation = Quaternion.Euler(random.NextDouble() < .06 ? 6f : 0f, yaw + along - 90f + 90f, 0f);
                panel.transform.localScale = new Vector3(step / Mathf.Max(bounds.size.x, bounds.size.z) * 1.02f, 1f, 1f);
                Vector2 p = a + dir * i * step;
                ShowcaseNature.Part(Fence, $"{Fence}_post", fence, new Vector3(p.x, Ground, p.y), yaw);
            }
            var box = new GameObject("FenceCollider").AddComponent<BoxCollider>();
            box.transform.SetParent(fence, false);
            box.transform.SetPositionAndRotation(new Vector3((a.x + b.x) / 2f, 1.2f, (a.y + b.y) / 2f), Quaternion.Euler(0f, yaw, 0f));
            box.size = new Vector3(.12f, 2.4f, length);
        }

        // The power line: poles along the road from the forest, through the gate to the warehouse's east corner.
        private static void PowerLine(Transform line)
        {
            var poles = new List<(Vector3 at, Vector3 along)>();
            float length = RoadLength;
            for (float s = 150f; s >= 12f; s -= 34f)
            {
                Vector2 c = RoadAt(s), ahead = RoadAt(Mathf.Max(0f, s - 4f));
                Vector2 dir = (ahead - c).normalized, side = new Vector2(dir.y, -dir.x);
                Vector2 p = c + side * (RoadHalfWidth + 3.2f);
                poles.Add((new Vector3(p.x, ShowcaseTerrain.HeightAt(p.x, p.y), p.y), new Vector3(dir.x, 0f, dir.y)));
            }
            foreach (Vector2 p in new[] { new Vector2(21f, -46f), new Vector2(25f, -26f), new Vector2(25f, -6f) })
                poles.Add((new Vector3(p.x, Ground, p.y), Vector3.forward));
            var tops = new List<Vector3[]>();
            for (int i = 0; i < poles.Count; i++)
            {
                (Vector3 at, Vector3 along) = poles[i];
                Vector3 next = i + 1 < poles.Count ? poles[i + 1].at : at + along;
                Vector3 run = next - at;
                run.y = 0f;
                float yaw = Mathf.Atan2(run.x, run.z) * Mathf.Rad2Deg;
                Transform pole = ShowcaseNature.Group("modular_electricity_poles", i % 3 == 1 ? "preset_02" : "preset_01", line, at, yaw + 90f, 1.45f);
                tops.Add(Attachments(pole, at));
            }
            // Three conductors from crossarm to crossarm, sagging; the last span drops to a bracket on the warehouse.
            var wires = new List<(Vector3 a, Vector3 b, float sag)>();
            for (int i = 0; i + 1 < tops.Count; i++)
                for (int k = 0; k < 3; k++)
                    wires.Add((tops[i][k], tops[i + 1][k], Vector3.Distance(tops[i][k], tops[i + 1][k]) * .035f));
            Vector3 last = tops[^1][1];
            wires.Add((last, new Vector3(20.35f, 6.4f, 2.5f), .6f));
            Wires(line, wires);
        }

        // Where the wires leave a pole: the ends and middle of its widest crossarm, measured from what was placed.
        private static Vector3[] Attachments(Transform pole, Vector3 foot)
        {
            Bounds arm = default;
            float widest = 0f;
            foreach (Renderer r in pole.GetComponentsInChildren<Renderer>())
            {
                if (!r.name.Contains("connection_large")) continue;
                float w = Mathf.Max(r.bounds.size.x, r.bounds.size.z);
                if (w > widest) { widest = w; arm = r.bounds; }
            }
            if (widest == 0f) return new[] { foot + Vector3.up * 8f, foot + Vector3.up * 8f, foot + Vector3.up * 8f };
            Vector3 axis = arm.size.x > arm.size.z ? Vector3.right : Vector3.forward;
            float half = Mathf.Max(arm.extents.x, arm.extents.z) * .85f;
            Vector3 top = new(arm.center.x, arm.max.y, arm.center.z);
            return new[] { top - axis * half, top, top + axis * half };
        }

        private static void Wires(Transform parent, List<(Vector3 a, Vector3 b, float sag)> wires)
        {
            const int along = 24, around = 4;
            const float radius = .014f;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            foreach ((Vector3 a, Vector3 b, float sag) in wires)
            {
                int start = vertices.Count;
                for (int i = 0; i <= along; i++)
                {
                    float t = i / (float)along;
                    Vector3 p = Vector3.Lerp(a, b, t) + Vector3.down * sag * 4f * t * (1f - t);
                    Vector3 tangent = (b - a).normalized;
                    Vector3 side = Vector3.Cross(tangent, Vector3.up).normalized, up = Vector3.Cross(side, tangent);
                    for (int k = 0; k < around; k++)
                    {
                        float angle = k * Mathf.PI * 2f / around;
                        vertices.Add(p + (side * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius);
                    }
                }
                for (int i = 0; i < along; i++)
                for (int k = 0; k < around; k++)
                {
                    int a0 = start + i * around + k, a1 = start + i * around + (k + 1) % around;
                    int b0 = a0 + around, b1 = a1 + around;
                    triangles.AddRange(new[] { a0, b0, a1, a1, b0, b1 });
                }
            }
            var mesh = new Mesh { name = "TestMap_PowerWires", indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            string path = "Assets/_Project/Art/TestMap/Terrain/TestMap_PowerWires.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            var go = new GameObject("Wires");
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = ShowcaseMaterials.Plain("TestMap_Cable_Rubber", new Color(.035f, .035f, .035f), .35f);
        }

        // Where the forest meets the fence: mossy boulders, fallen trunks, stumps, brush.
        private static void Wilds(Transform wilds)
        {
            var random = new System.Random(911);
            string[] rocks = { "rock_moss_set_01_rock01", "rock_moss_set_01_rock02", "rock_moss_set_01_rock03", "rock_moss_set_01_rock04",
                "rock_moss_set_01_rock05", "rock_moss_set_01_rock06" };
            int placed = 0;
            for (int i = 0; i < 400 && placed < 70; i++)
            {
                float x = Mathf.Lerp(Yard.xMin - 40f, Yard.xMax + 40f, (float)random.NextDouble());
                float z = Mathf.Lerp(Yard.yMin - 40f, Yard.yMax + 45f, (float)random.NextDouble());
                float outside = OutsideYard(x, z);
                if (outside < 2.5f || outside > 38f || ToRoad(x, z).distance < RoadHalfWidth + 3f) continue;
                float y = ShowcaseTerrain.HeightAt(x, z);
                double kind = random.NextDouble();
                float yaw = (float)random.NextDouble() * 360f;
                if (kind < .55)
                    ShowcaseNature.Part("rock_moss_set_01", rocks[random.Next(rocks.Length)], wilds, new Vector3(x, y - .25f, z), yaw,
                        Mathf.Lerp(.5f, 1.3f, (float)random.NextDouble()), solid: true);
                else if (kind < .7) ShowcaseNature.Part("dead_tree_trunk", null, wilds, new Vector3(x, y - .1f, z), yaw, 1f, solid: true);
                else if (kind < .82) ShowcaseNature.Part("tree_stump_01", null, wilds, new Vector3(x, y - .05f, z), yaw, 1f, solid: true);
                else if (kind < .92) ShowcaseNature.Part("dry_branches_medium_01", null, wilds, new Vector3(x, y, z), yaw);
                else ShowcaseNature.Part("boulder_01", null, wilds, new Vector3(x, y - .4f, z), yaw, Mathf.Lerp(.6f, 1f, (float)random.NextDouble()), solid: true);
                placed++;
            }
        }

        // The yard's leavings: barrels, crates, tyres, gas bottles, the welder under the lean-to, a hydrant, a searchlight.
        private static void Clutter(Transform clutter)
        {
            // Barrel_01, old_tyre and the wooden crates are the warehouse's single-material scans (ShowcaseModels).
            var scans = new HashSet<string> { "Barrel_01", "old_tyre", "wooden_crate_01", "wooden_crate_02" };
            (string id, Vector3 at, float yaw, bool solid)[] list =
            {
                ("Barrel_02", new(47f, 0f, -47.5f), 10f, true), ("Barrel_02", new(47.9f, 0f, -47.2f), 70f, true), ("Barrel_01", new(48.6f, 0f, -48f), 0f, true),
                ("barrel_stove", new(-24f, 0f, -6f), 0f, true), ("plastic_crate_03", new(50.5f, 0f, -47.8f), 20f, true),
                ("plastic_crate_03", new(50.6f, .4f, -47.7f), 35f, true), ("old_military_crate", new(52.5f, 0f, -48f), 90f, true),
                ("small_lpg_tank", new(55f, 0f, -48.4f), 0f, true), ("small_lpg_tank", new(55.7f, 0f, -48.3f), 40f, true),
                ("portable_welding_cart", new(53.5f, 0f, -45.8f), 200f, true), ("fire_hydrant", new(22.6f, 0f, -1.2f), 90f, true),
                ("portable_searchlight", new(29f, 0f, -18f), 230f, true), ("rusted_wheel_rim_01", new(40f, 0f, -14f), 0f, false),
                ("old_tyre", new(39.4f, 0f, -12.6f), 30f, true), ("old_tyre", new(39.6f, .22f, -12.5f), 75f, true),
                ("wooden_crate_01", new(-17f, 0f, 15.6f), 15f, true), ("wooden_crate_02", new(-16.2f, 0f, 16.8f), 40f, true),
                ("utility_box_02", new(-1.2f, 0f, -18f), 90f, true), ("ladder_sectioned_01", new(-15.4f, 0f, 6f), 90f, false),
            };
            foreach ((string id, Vector3 at, float yaw, bool solid) in list)
            {
                Vector3 p = at + Vector3.up * Ground;
                if (scans.Contains(id)) ShowcaseModels.Place(id, clutter, p, yaw, new Vector3(3f, 3f, 3f), solid);
                else ShowcaseNature.Part(id, null, clutter, p, yaw, 1f, solid);
            }
        }

        // The warehouse's outside: a gutter along each eave with downpipes at the corners, security lamps, a camera.
        private static void Facade(Transform facade)
        {
            const float eave = 7.2f, skin = .32f;
            foreach (var (x, z, yaw) in new[] { (-skin - .1f, -skin - .1f, 0f), (20f + skin + .1f, -skin - .1f, 0f), (-skin - .1f, 16f + skin + .1f, 180f), (20f + skin + .1f, 16f + skin + .1f, 180f) })
            {
                for (float y = .4f; y < eave; y += 1f)
                    ShowcaseNature.Part("modular_metal_gutter", "modular_metal_gutter_section", facade, new Vector3(x, y, z), yaw);
                ShowcaseNature.Part("modular_metal_gutter", "modular_metal_gutter_outlet", facade, new Vector3(x, 0f, z), yaw);
            }
            foreach (Vector3 at in new[] { new Vector3(3f, 3.6f, -skin - .02f), new Vector3(17.5f, 5.6f, -skin - .02f), new Vector3(20f + skin + .02f, 3.4f, 8f) })
            {
                GameObject lamp = ShowcaseNature.Part("security_light", null, facade, at, at.x > 20f ? 90f : 180f, 1f, ground: false);
                if (lamp != null) lamp.name = "SecurityLamp";
            }
            ShowcaseNature.Part("security_camera_01", null, facade, new Vector3(19.6f, 6.4f, -skin - .1f), 210f, 1f, ground: false);
        }
    }
}
