using System.Collections.Generic;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The rock as a landform, not a box: one faceted terrain mesh (2 m grid, walkable, with its own collider) whose
    /// height comes from <see cref="Height"/>. The plateau the buildings stand on is flat at 12 m; beyond its edge the
    /// ground breaks away in rocky steps (a shelf about halfway down) to an irregular coast, with sheer faces where the
    /// coast is close (the yard's west side) and long scrubby slopes where it is far (the north point under the
    /// lighthouse, the east shore). Below the waterline the slope carries on to the seabed, so the sea has a shore, not
    /// an edge. Grass on the flats, bare earth on the gentle slopes, rock on the steep ones and underwater. The cellhouse
    /// footprint and the road are left open (their own geometry fills them); the quay sits on it.
    /// </summary>
    public static class IslandLandform
    {
        public const string GroupName = "Landform";
        private const string Folder = IslandPrisonArt.Folder + "/Terrain";
        private const float Step = 2f;
        private static readonly Rect Extent = new(-70f, -70f, 290f, 280f);

        // ---- The height function: every other builder samples this to sit things on the ground ----------------

        /// <summary>Ground height at (x, z). 12 on every building pad, rolling ground between them, 0 on the quay,
        /// terraced slopes and coves down to the seabed.</summary>
        public static float Height(float x, float z)
        {
            // The dock: the quay itself (its concrete sits on top), and a dredged channel out to sea.
            // The quay row at its back edge counts too: the grid's step up to the plateau then hides inside QuayCliff.
            if (x >= L.Quay.xMin && x <= L.Quay.xMax && z >= L.Quay.yMin && z <= L.Quay.yMax + .01f) return L.QuayY - .3f;
            if (x > L.Slip.xMin - 4f && x < L.Slip.xMax + 4f && z < 0f) return -14f;
            // In front of the quay the water is deep all along it (no sand bar between the boat and the bay).
            if (z < L.Quay.yMin && x > L.Quay.xMin - 2f && x < L.Quay.xMax + 2f) return Mathf.Min(L.SeaY - 3f + z * .15f, -6f);

            float pad = PadDistance(x, z);
            float d = OutsidePlateau(x, z, pad);
            float h;
            if (d <= 0f) h = L.PlateauY + Relief(x, z, pad, -d);
            else h = Slope(x, z, d);
            // West of the quay the shore climbs from the quay's level: a steep goat path up the rocks.
            if (z < L.Plateau.yMin + 2f && x < L.Quay.xMin && x > L.Quay.xMin - 40f)
                h = Mathf.Min(h, L.QuayY + Mathf.Max(0f, (L.Quay.xMin - x) * .75f) + Mathf.Max(0f, (L.Plateau.yMin - z) * .4f) - .3f);
            return h;
        }

        /// <summary>Is the ground here left to other geometry (the cellhouse floor, the road, the turn)?</summary>
        public static bool Open(float x, float z)
        {
            var p = new Vector2(x, z);
            return L.Cellhouse.Contains(p) || L.RoadA.Contains(p) || L.RoadB.Contains(p) || L.RoadTurn.Contains(p) || L.Terrace.Contains(p);
        }

        /// <summary>Ground that must stay dead flat at 12 m: every building and its approach, the paths, the yard, and
        /// every outdoor fitting (benches, sheds, the pickup, fences, the wall and guard post).</summary>
        public static readonly Rect[] FlatZones =
        {
            Grow(L.Cellhouse, 4f), Grow(L.RecYard, 2f), new(26f, 92f, 28f, 22f), new(55f, 100f, 21f, 14f), Grow(L.WaterTower, 4f),
            Grow(L.OfficersClub, 3f), new(6f, 18f, 24f, 22f), new(48f, 18f, 12f, 24f), new(48f, 20f, 80f, 10f), new(112f, 20f, 12f, 30f),
            new(76f, 42f, 46f, 8f), new(84f, 18f, 24f, 9f), new(6f, 84f, 8f, 20f), new(34f, 88f, 10f, 14f),
            new(42f, 26f, 8f, 8f), new(62f, 26f, 6f, 8f), new(84f, 56f, 8f, 8f), new(22f, 90f, 8f, 8f), new(66f, 92f, 8f, 8f),
            new(106f, 82f, 12f, 12f), new(80f, 100f, 8f, 8f), new(108f, 31f, 8f, 9f), new(78f, 94f, 42f, 4f), new(82f, 48f, 4f, 42f),
            new(36f, 26f, 10f, 10f), new(56f, 28f, 8f, 8f), new(74f, 30f, 8f, 8f), new(86f, 23f, 8f, 8f), new(100f, 24.6f, 8f, 8f),
            new(118f, 36f, 8f, 8f), new(23.5f, 43f, 8f, 8f), new(76.5f, 90f, 8f, 8f), new(62f, 94f, 8f, 8f), new(50.5f, 94.5f, 8f, 8f),
            new(40f, 93.5f, 8f, 8f), new(92f, 91f, 8f, 8f), new(106f, 88f, 8f, 8f), new(116f, 72f, 8f, 8f), new(117f, 52f, 8f, 8f),
        };

        private static Rect Grow(Rect r, float m) => new(r.xMin - m, r.yMin - m, r.width + 2f * m, r.height + 2f * m);

        // Distance (m) from the nearest pad; 0 on one.
        private static float PadDistance(float x, float z)
        {
            float best = float.MaxValue;
            foreach (Rect r in FlatZones)
            {
                float dx = Mathf.Max(r.xMin - x, 0f, x - r.xMax), dz = Mathf.Max(r.yMin - z, 0f, z - r.yMax);
                float d = dx * dx + dz * dz;
                if (d < best) best = d;
                if (best <= 0f) return 0f;
            }
            return Mathf.Sqrt(best);
        }

        // The open ground between the buildings rolls: knolls up to 3 m and hollows down to 1.5 m, fading flat within
        // 2-10 m of anything built and within 4-9 m of the brink (so the edge, and its parapet, stays level).
        private static float Relief(float x, float z, float pad, float inside)
        {
            float fade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((pad - 2f) / 8f)) * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((inside - 4f) / 5f));
            if (fade <= 0f) return 0f;
            float n = Noise(x * .03f, z * .03f, 17f) * .75f + Noise(x * .09f, z * .09f, 19f) * .25f;
            return Mathf.Clamp((n - .45f) * 9f, -1.5f, 3f) * fade;
        }

        // How far outside the plateau's brink a point is; negative inside. The brink is a rounded rectangle that
        // wanders: out by up to 9 m, and in by up to 7 m wherever nothing is built near (inlets of broken ground
        // reaching into the plateau). Behind the quay it stays a straight cliff.
        private static float OutsidePlateau(float x, float z, float pad)
        {
            Rect p = L.Plateau;
            var c = p.center;
            var half = new Vector2(p.width * .5f, p.height * .5f);
            float r = z > c.y ? (x < c.x ? 22f : 30f) : (x < c.x ? 4f : 0f);
            var q = new Vector2(Mathf.Abs(x - c.x) - half.x + r, Mathf.Abs(z - c.y) - half.y + r);
            float outside = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - r;
            bool quayFace = z < p.yMin + .5f && x > L.Quay.xMin - .5f && x < L.RoadA.xMax;
            if (quayFace) return outside;
            float n = Noise(x * .04f, z * .04f, 3f);
            float inward = Mathf.Clamp01((pad - 10f) / 8f) * 7f;
            float wander = Mathf.Lerp(-inward, 9f, n);
            float edge = outside - wander;
            // A built pad is never on the slope: it is always at least 4 m inside the brink.
            return Mathf.Min(edge, pad - 4f);
        }

        // Off the brink: a steep break, a rock bench about two-thirds of the way up, a second bench, then the last
        // drop into the water. Where the shore is a cove the drop is gentle and ends in a shingle beach; on a
        // headland it is short and sheer. Below the waterline it runs on to the seabed.
        private static float Slope(float x, float z, float d)
        {
            float reach = CoastReach(x, z);
            float cove = Cove(x, z);
            float t = d / reach;
            if (t >= 1f) return L.SeaY - 1f - Mathf.Min(12f, (t - 1f) * reach * (.6f - cove * .45f));
            float bench1 = 7.5f + (Noise(x * .05f, z * .05f, 23f) - .5f) * 2f;
            float bench2 = 3f + (Noise(x * .05f, z * .05f, 29f) - .5f) * 1.6f;
            float h;
            if (t < .28f) h = Mathf.Lerp(L.PlateauY, bench1, Mathf.SmoothStep(0f, 1f, t / .28f));
            else if (t < .42f) h = bench1 - (t - .28f) * 3f;
            else if (t < .62f) h = Mathf.Lerp(bench1 - .42f, bench2, Mathf.SmoothStep(0f, 1f, (t - .42f) / .2f));
            else if (t < .74f) h = bench2 - (t - .62f) * 3f;
            else
            {
                float u = (t - .74f) / .26f;
                // A cove ends in a gentle beach; elsewhere the rock drops straight in.
                float beach = Mathf.Lerp(L.SeaY - 1f, L.SeaY + .2f, cove);
                h = Mathf.Lerp(bench2 - .36f, beach, cove > .5f ? u : Mathf.SmoothStep(0f, 1f, u));
                if (cove > .5f && u > .6f) h = Mathf.Lerp(h, L.SeaY - (u - .6f) * 3f, (cove - .5f) * 2f);
            }
            // Broken rock everywhere off the brink: lumps and gullies, calmer on the benches and beaches.
            float rough = Noise(x * .12f, z * .12f, 7f) - .5f;
            float calm = (t > .28f && t < .42f) || (t > .62f && t < .74f) ? .35f : 1f;
            h += rough * 2.6f * calm * (1f - cove * .7f) * Mathf.Clamp01(t * 6f);
            return h;
        }

        // Three coves round the island: the north-east bay, the west inlet below the yard's north end, the south-west
        // beach beyond the goat path. 1 in a cove's middle, 0 away from it.
        private static float Cove(float x, float z)
        {
            float best = 0f;
            foreach ((float cx, float cz, float r) in new[] { (140f, 140f, 34f), (-14f, 104f, 22f), (6f, -6f, 26f) })
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (z - cz) * (z - cz));
                best = Mathf.Max(best, Mathf.Clamp01(1f - d / r));
            }
            return Mathf.SmoothStep(0f, 1f, best);
        }

        // How far the slope runs from the brink to the waterline, by where you are round the island.
        private static float CoastReach(float x, float z)
        {
            Rect p = L.Plateau;
            float n = Noise(x * .02f, z * .02f, 11f);
            float reach = 16f + n * 18f;
            if (x < p.xMin) reach = 7f + n * 8f;                                    // the yard's cliffs
            if (z > p.yMax) reach = 24f + n * 20f;                                  // the long north slope
            // The point under the lighthouse runs out into the bay.
            float point = Mathf.Exp(-Mathf.Pow((x - L.Lighthouse.center.x) / 26f, 2f)) * Mathf.Clamp01((z - p.yMax + 4f) / 6f);
            reach += point * 30f;
            if (x > p.xMax && z > L.RoadTurn.yMax) reach = 16f + n * 14f;          // the east shore past the road
            if (z < p.yMin && x < L.Quay.xMin) reach = 18f + n * 12f;              // the south-west rocks
            return reach + Cove(x, z) * 22f;
        }

        /// <summary>Sea stacks: rock pillars standing off the shore (positions where the seabed is already deep).</summary>
        public static IEnumerable<(Vector3 at, Vector3 size)> Stacks()
        {
            var centre = new Vector2(L.Plateau.center.x, L.Plateau.center.y);
            for (int i = 0; i < 16; i++)
            {
                float a = (i / 16f + IslandKit.Hash(i, 701) * .04f) * Mathf.PI * 2f;
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                float shore = -1f;
                for (float r = 40f; r < 260f; r += 2f)
                {
                    Vector2 p = centre + dir * r;
                    if (Open(p.x, p.y) || Height(p.x, p.y) > L.SeaY) continue;
                    shore = r;
                    break;
                }
                if (shore < 0f) continue;
                Vector2 at = centre + dir * (shore + 6f + IslandKit.Hash(i, 702) * 18f);
                // Never in the boat's approach.
                if (at.y < 4f && at.x > 40f && at.x < 150f) continue;
                if (IslandKit.Hash(i, 703) < .35f) continue;
                float tall = 5f + IslandKit.Hash(i, 704) * 12f, w = 3f + IslandKit.Hash(i, 705) * 5f;
                yield return (new Vector3(at.x, L.SeaY - 3f, at.y), new Vector3(w, tall + 3f, w * (.7f + IslandKit.Hash(i, 706) * .5f)));
            }
        }

        // Tileable-enough value noise in 0..1 (two octaves of Perlin, offset by a seed).
        private static float Noise(float x, float z, float seed) =>
            Mathf.PerlinNoise(x + seed * 13.1f, z + seed * 7.7f) * .7f + Mathf.PerlinNoise(x * 2.3f + seed, z * 2.3f - seed) * .3f;

        // ---- The mesh ------------------------------------------------------------------------------------------

        public static void Build(Transform root, IslandPrisonArt art)
        {
            Transform g = Group(GroupName, root);
            int nx = Mathf.CeilToInt(Extent.width / Step), nz = Mathf.CeilToInt(Extent.height / Step);
            var h = new float[nx + 1, nz + 1];
            for (int i = 0; i <= nx; i++)
            for (int j = 0; j <= nz; j++)
                h[i, j] = Height(Extent.xMin + i * Step, Extent.yMin + j * Step);

            // Unwelded triangles: faceted, and each picks its own material and UVs by slope.
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var subs = new[] { new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>() };   // grass, earth, rock, seabed, shingle
            var foam = new List<Vector3>();
            for (int i = 0; i < nx; i++)
            for (int j = 0; j < nz; j++)
            {
                float x0 = Extent.xMin + i * Step, z0 = Extent.yMin + j * Step;
                if (Open(x0 + Step * .5f, z0 + Step * .5f)) continue;
                var a = new Vector3(x0, h[i, j], z0);
                var b = new Vector3(x0, h[i, j + 1], z0 + Step);
                var c = new Vector3(x0 + Step, h[i + 1, j + 1], z0 + Step);
                var d = new Vector3(x0 + Step, h[i + 1, j], z0);
                // Alternate the diagonal so the facets don't line up in rows.
                if (((i + j) & 1) == 0) { Tri(a, b, c, verts, uvs, subs); Tri(a, c, d, verts, uvs, subs); }
                else { Tri(a, b, d, verts, uvs, subs); Tri(b, c, d, verts, uvs, subs); }
                float lo = Mathf.Min(Mathf.Min(a.y, b.y), Mathf.Min(c.y, d.y)), hi = Mathf.Max(Mathf.Max(a.y, b.y), Mathf.Max(c.y, d.y));
                if (lo < L.SeaY && hi > L.SeaY - .2f) foam.Add(new Vector3(x0 + Step * .5f, L.SeaY, z0 + Step * .5f));
            }
            PolishAssets.EnsureFolder(Folder);
            Mesh mesh = Asset("IslandTerrain");
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = subs.Length;
            for (int s = 0; s < subs.Length; s++) mesh.SetTriangles(subs[s], s);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);

            var terrain = new GameObject("Terrain");
            terrain.transform.SetParent(g, false);
            terrain.AddComponent<MeshFilter>().sharedMesh = mesh;
            terrain.AddComponent<MeshRenderer>().sharedMaterials = new[] { art.Grass, art.Ground, art.Rock, art.RockDark, art.Sand };
            terrain.AddComponent<MeshCollider>().sharedMesh = mesh;
            terrain.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Dirt);
            Foam(g, foam, art);
            // The face behind the quay: one straight rock wall. It runs 2 m back into the plateau, so its flat top is
            // the ground there and covers the grid's step from the quay up to the plateau.
            IslandKit.Span("QuayCliff", g, new Vector3(L.Quay.xMin, L.QuayY - .5f, L.Plateau.yMin - .4f),
                new Vector3(L.RoadB.xMin, L.PlateauY, L.Plateau.yMin + 2f), art.Rock);
            MarkStatic(g);
        }

        private static void Tri(Vector3 a, Vector3 b, Vector3 c, List<Vector3> verts, List<Vector2> uvs, List<int>[] subs)
        {
            Vector3 n = Vector3.Cross(b - a, c - a).normalized;
            float y = (a.y + b.y + c.y) / 3f;
            // Shingle at the water's edge where it is gentle, turf on anything flat above it (the plateau and the benches),
            // earth on the moderate slopes, rock on the steep ones, dark rock under water.
            int sub = y < L.SeaY - .9f ? 3 : y < L.SeaY + 1.6f && n.y > .8f ? 4 : n.y > .9f ? 0 : n.y > .72f ? 1 : 2;
            int start = verts.Count;
            foreach (Vector3 v in new[] { a, b, c })
            {
                verts.Add(v);
                // Flats map from above, faces from the side they face: true-size texture everywhere.
                uvs.Add(n.y > .7f ? new Vector2(v.x, v.z) / 4f : Mathf.Abs(n.x) > Mathf.Abs(n.z) ? new Vector2(v.z, v.y) / 5f : new Vector2(v.x, v.y) / 5f);
            }
            subs[sub].Add(start); subs[sub].Add(start + 1); subs[sub].Add(start + 2);
        }

        // Where the rock meets the water: a broken band of foam, one small quad per shore cell.
        private static void Foam(Transform g, List<Vector3> at, IslandPrisonArt art)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();
            int n = 0;
            foreach (Vector3 c in at)
            {
                if (IslandKit.Hash((int)c.x, (int)c.z) < .2f) continue;
                float s = 1.4f + IslandKit.Hash((int)c.z, (int)c.x) * 1.4f, yaw = IslandKit.Hash(n, 3) * Mathf.PI;
                Vector3 u = new Vector3(Mathf.Cos(yaw), 0f, Mathf.Sin(yaw)) * s, v = new Vector3(-u.z, 0f, u.x);
                // The water box's top is SeaY + .05: foam floats clearly above it, never in the same plane.
                Vector3 y = Vector3.up * .16f;
                int k = verts.Count;
                verts.Add(c - u - v + y); verts.Add(c - u + v + y); verts.Add(c + u + v + y); verts.Add(c + u - v + y);
                uvs.Add(Vector2.zero); uvs.Add(Vector2.up); uvs.Add(Vector2.one); uvs.Add(Vector2.right);
                tris.AddRange(new[] { k, k + 1, k + 2, k, k + 2, k + 3 });
                n++;
            }
            Mesh mesh = Asset("IslandFoam");
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            var go = new GameObject("Foam");
            go.transform.SetParent(g, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = art.Foam;
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static Mesh Asset(string name)
        {
            string path = $"{Folder}/{name}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null) { mesh = new Mesh { name = name }; AssetDatabase.CreateAsset(mesh, path); }
            else mesh.Clear();
            return mesh;
        }
    }
}
