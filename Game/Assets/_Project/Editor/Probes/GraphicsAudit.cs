using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.13 audit; changes nothing. Per map: (1) every material that is drawn, by how much surface it covers, with its
    /// shader, maps and how many UV units its meshes give a metre, and (2) every pair of faces that lie in the same plane
    /// (within <see cref="PlaneGap"/>) and overlap: the "two textures swapping" flicker. Writes Game/Logs/graphics/*.csv
    /// and a summary line per map. Batch: <c>Tools/unity.sh exec Abandoned.EditorTools.GraphicsAudit.RunAndExit</c>.
    /// <see cref="CoplanarFix"/> uses <see cref="FindFights"/> to separate the faces it finds.
    /// </summary>
    public static class GraphicsAudit
    {
        // A 24-bit depth buffer resolves about 3 mm at 50 m; closer than this two faces can trade places.
        public const float PlaneGap = 0.005f;
        // A 32-bit float depth buffer (what URP uses on Mac and Windows) resolves z/2^24, 0.003 mm at 50 m; with vertex
        // precision on top, faces further apart than this never trade places.
        public const float Flicker = 0.0001f;
        public const float MinOverlap = 0.0025f; // m², a 5 cm square: anything smaller is a sliver on a shared edge
        private static readonly string Folder = Path.Combine(Application.dataPath, "..", "Logs", "graphics");

        public static void RunAndExit()
        {
            Directory.CreateDirectory(Folder);
            int code = 0;
            foreach (string path in AvOverhaul.MapScenes)
            {
                try
                {
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    var started = DateTime.Now;
                    var fights = FindFights(out int tris);
                    WriteFights(scene.name, fights);
                    WriteMaterials(scene.name);
                    var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l => l.isActiveAndEnabled).ToList();
                    Debug.Log($"[GraphicsAudit] {scene.name}: lights " + string.Join("; ", lights.GroupBy(l => l.type).Select(g =>
                        $"{g.Key} {g.Count()} (intensity avg {g.Average(l => l.intensity):0.0} max {g.Max(l => l.intensity):0.0}, range avg {g.Average(l => l.range):0.0}, " +
                        $"shadowed {g.Count(l => l.shadows != LightShadows.None)})")) +
                        $"; ambient sky {RenderSettings.ambientSkyColor} equator {RenderSettings.ambientEquatorColor} ground {RenderSettings.ambientGroundColor}");
                    var stray = StrayLods();
                    Debug.Log($"[GraphicsAudit] {scene.name}: {stray.Count} LOD renderers drawn outside a working LOD group. " +
                              string.Join("; ", stray.Take(12).Select(r => $"{PathOf(r)} ({Why(r)})")));
                    Debug.Log($"[GraphicsAudit] {scene.name}: {tris} triangles, {fights.Count} overlapping pairs, " +
                              $"{fights.Sum(f => f.Area):0.0} m² overlapping; within 1 mm: {fights.Count(f => f.Gap < 0.001f)}, within 0.1 mm (can flicker): {fights.Count(f => f.Gap < Flicker)} ({(DateTime.Now - started).TotalSeconds:0} s).");
                }
                catch (Exception e) { Debug.LogException(e); code = 1; }
            }
            if (Application.isBatchMode) EditorApplication.Exit(code);
        }

        public sealed class Fight
        {
            public MeshRenderer A, B;
            public int SubA, SubB;
            public Material MatA, MatB;
            public float Area, Gap;
            public Vector3 Where, Normal;
            /// <summary>Each overlapping face pair, as indices into <see cref="LastTriangles"/>.</summary>
            public readonly List<(int, int)> Faces = new();
        }

        public struct Tri
        {
            public int Renderer, Sub, Index; // Index: the face's first index in its submesh's index list
            public Vector3 A, B, C, N;
            public float D;
        }

        private static List<Tri> lastTriangles = new();

        /// <summary>The faces of the last <see cref="FindFights"/>, in world space.</summary>
        public static IReadOnlyList<Tri> LastTriangles => lastTriangles;

        /// <summary>Every overlapping coplanar pair of drawn faces in the open scene(s), grouped per renderer/submesh pair.</summary>
        public static List<Fight> FindFights(out int triangleCount, float maxGap = PlaneGap)
        {
            var renderers = Drawn().Where(r => !InOuterRings(r)).ToList();
            var tris = new List<Tri>(1 << 20);
            for (int r = 0; r < renderers.Count; r++) Collect(renderers[r], r, tris);
            triangleCount = tris.Count;
            lastTriangles = tris;

            var lodLevel = new Dictionary<Renderer, (LODGroup, int)>();
            foreach (LODGroup g in Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None))
            {
                LOD[] lods = g.GetLODs();
                for (int l = 0; l < lods.Length; l++)
                    foreach (Renderer rr in lods[l].renderers) if (rr != null) lodLevel[rr] = (g, l);
            }
            // Two levels of one LOD group are never drawn together.
            bool OtherLevel(Tri ta, Tri tb)
            {
                Renderer ra = renderers[ta.Renderer], rb = renderers[tb.Renderer];
                return ra != rb && lodLevel.TryGetValue(ra, out var la) && lodLevel.TryGetValue(rb, out var lb) && la.Item1 == lb.Item1 && la.Item2 != lb.Item2;
            }

            var pairs = new Dictionary<(int, int, int, int), Fight>();
            foreach (var (ia, ib, area, gap) in FindPairs(tris, OtherLevel, maxGap))
            {
                Tri ta = tris[ia], tb = tris[ib];
                bool first = ta.Renderer < tb.Renderer || (ta.Renderer == tb.Renderer && ta.Sub <= tb.Sub);
                var key = first ? (ta.Renderer, ta.Sub, tb.Renderer, tb.Sub) : (tb.Renderer, tb.Sub, ta.Renderer, ta.Sub);
                if (!pairs.TryGetValue(key, out Fight f))
                {
                    MeshRenderer r1 = renderers[key.Item1], r2 = renderers[key.Item3];
                    pairs[key] = f = new Fight
                    {
                        A = r1, B = r2, SubA = key.Item2, SubB = key.Item4, MatA = Mat(r1, key.Item2), MatB = Mat(r2, key.Item4),
                        Gap = gap, Where = (ta.A + ta.B + ta.C) / 3f, Normal = ta.N,
                    };
                }
                f.Area += area;
                f.Gap = Mathf.Min(f.Gap, gap);
                f.Faces.Add(first ? (ia, ib) : (ib, ia));
            }
            return pairs.Values.Where(f => f.Area >= MinOverlap).OrderByDescending(f => f.Area).ToList();
        }

        /// <summary>Every pair of faces (indices into <paramref name="tris"/>) that face the same way, lie within
        /// <see cref="PlaneGap"/> of one plane and overlap, with the overlap's area and the gap. <paramref name="ignore"/>
        /// drops pairs that are never drawn together.</summary>
        public static List<(int a, int b, float area, float gap)> FindPairs(List<Tri> tris, Func<Tri, Tri, bool> ignore = null, float maxGap = PlaneGap)
        {
            // Bucket by facing (rounded normal) and plane distance (1 cm bins; each bin also takes the one above).
            var planes = new Dictionary<(int, int, int, long), List<int>>();
            for (int i = 0; i < tris.Count; i++)
            {
                var key = PlaneKey(tris[i]);
                if (!planes.TryGetValue(key, out var list)) planes[key] = list = new List<int>();
                list.Add(i);
            }
            var found = new List<(int, int, float, float)>();
            var flat = new List<Vector2>();
            var box = new List<Vector4>(); // xMin, yMin, xMax, yMax in the plane
            var grid = new Dictionary<(int, int), List<int>>();
            foreach (var kv in planes)
            {
                var candidates = new List<int>(kv.Value);
                int own = candidates.Count;
                // The bins above, as far as maxGap reaches (pairs within one bin are that bin's own).
                for (int up = 1; up <= Mathf.CeilToInt(maxGap / 0.01f); up++)
                    if (planes.TryGetValue((kv.Key.Item1, kv.Key.Item2, kv.Key.Item3, kv.Key.Item4 + up), out var above)) candidates.AddRange(above);
                if (candidates.Count < 2) continue;
                Basis(tris[kv.Value[0]].N, out Vector3 u, out Vector3 v);
                flat.Clear();
                box.Clear();
                foreach (int c in candidates)
                {
                    Tri t = tris[c];
                    Vector2 a = P(t.A, u, v), b = P(t.B, u, v), d = P(t.C, u, v);
                    flat.Add(a);
                    flat.Add(b);
                    flat.Add(d);
                    box.Add(new Vector4(Mathf.Min(a.x, Mathf.Min(b.x, d.x)), Mathf.Min(a.y, Mathf.Min(b.y, d.y)),
                        Mathf.Max(a.x, Mathf.Max(b.x, d.x)), Mathf.Max(a.y, Mathf.Max(b.y, d.y))));
                }
                // A 2D grid inside the plane so big floors don't compare every face with every face. A pair is judged
                // only in the cell holding the corner where their boxes meet, so no pair is judged twice.
                const float cell = 0.5f;
                grid.Clear();
                for (int c = 0; c < candidates.Count; c++)
                {
                    Vector4 bb = box[c];
                    int x0 = Mathf.FloorToInt(bb.x / cell), x1 = Mathf.FloorToInt(bb.z / cell);
                    int y0 = Mathf.FloorToInt(bb.y / cell), y1 = Mathf.FloorToInt(bb.w / cell);
                    if ((long)(x1 - x0 + 1) * (y1 - y0 + 1) > 65536) continue; // a sky dome or terrain sheet
                    for (int x = x0; x <= x1; x++)
                    for (int y = y0; y <= y1; y++)
                    {
                        if (!grid.TryGetValue((x, y), out var g)) grid[(x, y)] = g = new List<int>();
                        g.Add(c);
                    }
                }
                foreach (var cellKv in grid)
                {
                    var g = cellKv.Value;
                    for (int i = 0; i < g.Count; i++)
                    for (int j = i + 1; j < g.Count; j++)
                    {
                        int ci = g[i], cj = g[j];
                        if (ci >= own && cj >= own) continue; // both from the bin above: that bin's own pass
                        Vector4 p = box[ci], q = box[cj];
                        float ox = Mathf.Max(p.x, q.x), oy = Mathf.Max(p.y, q.y);
                        if (ox >= Mathf.Min(p.z, q.z) || oy >= Mathf.Min(p.w, q.w)) continue;
                        if (Mathf.FloorToInt(ox / cell) != cellKv.Key.Item1 || Mathf.FloorToInt(oy / cell) != cellKv.Key.Item2) continue;
                        int ia = candidates[ci], ib = candidates[cj];
                        Tri ta = tris[ia], tb = tris[ib];
                        if (Vector3.Dot(ta.N, tb.N) < 0.999f) continue;
                        float gap = Mathf.Abs(ta.D - tb.D);
                        if (gap > maxGap) continue;
                        if (ignore != null && ignore(ta, tb)) continue;
                        float area = Overlap(flat, ci * 3, cj * 3);
                        if (area <= 1e-5f) continue;
                        found.Add((ia, ib, area, gap));
                    }
                }
            }
            return found;
        }

        private static readonly System.Text.RegularExpressions.Regex LodName = new(@"_LOD(\d+)$");

        /// <summary>Drawn renderers named "..._LODn" that no enabled LOD group switches: they draw on top of their
        /// siblings, every level at once.</summary>
        public static List<MeshRenderer> StrayLods()
        {
            var managed = new HashSet<Renderer>();
            foreach (LODGroup g in Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None))
            {
                if (!g.enabled || !g.gameObject.activeInHierarchy) continue;
                foreach (LOD lod in g.GetLODs())
                    foreach (Renderer r in lod.renderers) if (r != null) managed.Add(r);
            }
            return Drawn().Where(r => LodName.IsMatch(r.name) && !managed.Contains(r)).ToList();
        }

        private static string Why(Renderer r)
        {
            LODGroup g = r.GetComponentInParent<LODGroup>(true);
            if (g == null) return "no LOD group above it";
            if (!g.enabled) return "its LOD group is switched off";
            return $"not listed in {g.name}'s {g.lodCount} levels";
        }

        // The new maps' outer rings of town (StationRings) are 10-17 million triangles seen from 50 m and more. Holding
        // them in world space up to nine times per CoplanarFix run froze the editor on Rebuild New Maps (2026-10-23), and
        // nothing that far away is close enough to the camera to flicker. The Skyscraper's downtown street life
        // (SkyscraperDowntownStreetscape: the same ring-kit block meshes plus scanned street trees) is 59 million more.
        private static bool InOuterRings(Renderer r)
        {
            for (Transform p = r.transform.parent; p != null; p = p.parent)
                if (p.name == StationRings.GroupName || p.name == "Streetscape") return true;
            return false;
        }

        /// <summary>Renderers that draw opaque or cut-out surfaces: active, enabled, a mesh, not debris, not shadow-only.</summary>
        public static IEnumerable<MeshRenderer> Drawn()
        {
            int debris = LayerMask.NameToLayer("Debris");
            foreach (MeshRenderer r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy || r.gameObject.layer == debris) continue;
                if (r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly) continue;
                var mf = r.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                yield return r;
            }
        }

        private static Material Mat(Renderer r, int sub)
        {
            Material[] m = r.sharedMaterials;
            return m.Length == 0 ? null : m[Mathf.Min(sub, m.Length - 1)];
        }

        private static void Collect(MeshRenderer r, int index, List<Tri> into)
        {
            Material[] mats = r.sharedMaterials;
            // Transparent submeshes write no depth (they are sorted instead), so they can't fight.
            CollectMesh(r.GetComponent<MeshFilter>().sharedMesh, r.transform.localToWorldMatrix, index,
                s => s < mats.Length && mats[s] != null && mats[s].renderQueue < 3000, into);
        }

        /// <summary>A mesh's faces through <paramref name="m"/>, for the submeshes <paramref name="drawn"/> accepts.</summary>
        public static void CollectMesh(Mesh mesh, Matrix4x4 m, int index, Func<int, bool> drawn, List<Tri> into)
        {
            var verts = new List<Vector3>();
            mesh.GetVertices(verts);
            for (int i = 0; i < verts.Count; i++) verts[i] = m.MultiplyPoint3x4(verts[i]);
            var idx = new List<int>();
            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                if (!drawn(s) || mesh.GetTopology(s) != MeshTopology.Triangles) continue;
                idx.Clear();
                mesh.GetTriangles(idx, s);
                for (int t = 0; t + 2 < idx.Count; t += 3)
                {
                    Vector3 a = verts[idx[t]], b = verts[idx[t + 1]], c = verts[idx[t + 2]];
                    Vector3 cross = Vector3.Cross(b - a, c - a);
                    float len = cross.magnitude;
                    if (len < 1e-5f) continue;
                    Vector3 n = cross / len;
                    into.Add(new Tri { Renderer = index, Sub = s, Index = t, A = a, B = b, C = c, N = n, D = Vector3.Dot(n, a) });
                }
            }
        }

        private static (int, int, int, long) PlaneKey(Tri t) =>
            (Mathf.RoundToInt(t.N.x * 40f), Mathf.RoundToInt(t.N.y * 40f), Mathf.RoundToInt(t.N.z * 40f), (long)Math.Floor(t.D / 0.01f));

        private static void Basis(Vector3 n, out Vector3 u, out Vector3 v)
        {
            u = Vector3.Cross(n, Mathf.Abs(n.y) < 0.9f ? Vector3.up : Vector3.right).normalized;
            v = Vector3.Cross(n, u);
        }

        private static Vector2 P(Vector3 p, Vector3 u, Vector3 v) => new(Vector3.Dot(p, u), Vector3.Dot(p, v));

        // Area where two triangles, already projected into their shared plane, overlap (Sutherland-Hodgman into fixed
        // buffers: this runs millions of times on the mall).
        private static readonly Vector2[] clipA = new Vector2[16], clipB = new Vector2[16];

        private static float Overlap(List<Vector2> flat, int a, int b)
        {
            Vector2[] subject = clipA, output = clipB;
            int count = 3;
            subject[0] = flat[a];
            subject[1] = flat[a + 1];
            subject[2] = flat[a + 2];
            if (Cross(subject[1] - subject[0], subject[2] - subject[0]) < 0) (subject[1], subject[2]) = (subject[2], subject[1]);
            Vector2 k0 = flat[b], k1 = flat[b + 1], k2 = flat[b + 2];
            if (Cross(k1 - k0, k2 - k0) < 0) (k1, k2) = (k2, k1);
            for (int e = 0; e < 3 && count > 0; e++)
            {
                Vector2 c0 = e == 0 ? k0 : e == 1 ? k1 : k2, c1 = e == 0 ? k1 : e == 1 ? k2 : k0;
                int n = 0;
                for (int i = 0; i < count; i++)
                {
                    Vector2 p = subject[i], q = subject[(i + 1) % count];
                    bool pin = Cross(c1 - c0, p - c0) >= 0, qin = Cross(c1 - c0, q - c0) >= 0;
                    if (pin && n < 16) output[n++] = p;
                    if (pin != qin && n < 16) output[n++] = Intersect(p, q, c0, c1);
                }
                (subject, output) = (output, subject);
                count = n;
            }
            if (count < 3) return 0f;
            float sum = 0f;
            for (int i = 0; i < count; i++) sum += Cross(subject[i], subject[(i + 1) % count]);
            return Mathf.Abs(sum * 0.5f);
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        private static Vector2 Intersect(Vector2 p, Vector2 q, Vector2 a, Vector2 b)
        {
            Vector2 r = q - p, s = b - a;
            float den = Cross(r, s);
            if (Mathf.Abs(den) < 1e-12f) return p;
            return p + r * (Cross(a - p, s) / den);
        }

        public static string PathOf(Component c)
        {
            var parts = new List<string>();
            for (Transform t = c.transform; t != null; t = t.parent) parts.Add(t.name);
            parts.Reverse();
            return string.Join("/", parts);
        }

        private static void WriteFights(string scene, List<Fight> fights)
        {
            var sb = new StringBuilder("area_m2,gap_mm,x,y,z,renderer_a,material_a,renderer_b,material_b\n");
            foreach (Fight f in fights)
                sb.Append($"{f.Area:0.000},{f.Gap * 1000f:0.0},{f.Where.x:0.0},{f.Where.y:0.0},{f.Where.z:0.0},\"{PathOf(f.A)}\",{f.MatA?.name},\"{PathOf(f.B)}\",{f.MatB?.name}\n");
            File.WriteAllText(Path.Combine(Folder, $"zfight_{scene}.csv"), sb.ToString());
        }

        /// <summary>Per material in the open scene(s): world area drawn, and UV units per metre (area-weighted 20th,
        /// 50th and 80th percentile over the renderers using it).</summary>
        public static Dictionary<Material, UvDensity> MeasureMaterials()
        {
            var result = new Dictionary<Material, UvDensity>();
            foreach (var kv in MeasureSamples()) result[kv.Key] = Density(kv.Value);
            return result;
        }

        /// <summary>Area-weighted 20th/50th/80th percentile of (UV units per metre, world area) samples.</summary>
        public static UvDensity Density(List<(double ratio, double area)> samples)
        {
            var sorted = samples.OrderBy(x => x.ratio).ToList();
            double total = sorted.Sum(x => x.area);
            double Pct(double q)
            {
                double acc = 0;
                foreach (var x in sorted)
                {
                    acc += x.area;
                    if (acc >= total * q) return x.ratio;
                }
                return sorted[^1].ratio;
            }
            return new UvDensity { Area = total, P20 = (float)Pct(0.2), P50 = (float)Pct(0.5), P80 = (float)Pct(0.8) };
        }

        /// <summary>Per material in the open scene(s), one (UV units per metre, world area) sample per renderer and
        /// submesh that draws it.</summary>
        public static Dictionary<Material, List<(double ratio, double area)>> MeasureSamples()
        {
            var samples = new Dictionary<Material, List<(double ratio, double area)>>();
            foreach (MeshRenderer r in Drawn())
            {
                Mesh mesh = r.GetComponent<MeshFilter>().sharedMesh;
                Matrix4x4 m = r.transform.localToWorldMatrix;
                var verts = new List<Vector3>();
                var uvs = new List<Vector2>();
                mesh.GetVertices(verts);
                mesh.GetUVs(0, uvs);
                bool hasUv = uvs.Count == verts.Count;
                Material[] mats = r.sharedMaterials;
                var idx = new List<int>();
                for (int s = 0; s < mesh.subMeshCount && s < mats.Length; s++)
                {
                    if (mats[s] == null || mesh.GetTopology(s) != MeshTopology.Triangles) continue;
                    idx.Clear();
                    mesh.GetTriangles(idx, s);
                    double world = 0, uv = 0;
                    for (int t = 0; t + 2 < idx.Count; t += 3)
                    {
                        Vector3 a = m.MultiplyPoint3x4(verts[idx[t]]), b = m.MultiplyPoint3x4(verts[idx[t + 1]]), c = m.MultiplyPoint3x4(verts[idx[t + 2]]);
                        world += Vector3.Cross(b - a, c - a).magnitude * 0.5;
                        if (hasUv)
                        {
                            Vector2 ua = uvs[idx[t]], ub = uvs[idx[t + 1]], uc = uvs[idx[t + 2]];
                            uv += Mathf.Abs(Cross(ub - ua, uc - ua)) * 0.5;
                        }
                    }
                    if (world <= 1e-6) continue;
                    if (!samples.TryGetValue(mats[s], out var list)) samples[mats[s]] = list = new List<(double, double)>();
                    list.Add((Math.Sqrt(uv / world), world));
                }
            }
            return samples;
        }

        public struct UvDensity
        {
            public double Area;
            public float P20, P50, P80; // UV units per metre
        }

        private static void WriteMaterials(string scene)
        {
            var sb = new StringBuilder("area_m2,uv_per_m_p20,uv_per_m_p50,uv_per_m_p80,material,shader,base_map,normal_map,scale,path\n");
            foreach (var kv in MeasureMaterials().OrderByDescending(k => k.Value.Area))
            {
                Material mat = kv.Key;
                string Tex(string p) => mat.HasProperty(p) && mat.GetTexture(p) != null ? AssetDatabase.GetAssetPath(mat.GetTexture(p)) : "";
                string scale = mat.HasProperty("_BaseMap") ? $"{mat.GetTextureScale("_BaseMap").x:0.###}" : "";
                UvDensity d = kv.Value;
                sb.Append($"{d.Area:0},{d.P20:0.###},{d.P50:0.###},{d.P80:0.###},{mat.name},{mat.shader?.name},{Tex("_BaseMap")},{Tex("_BumpMap")},{scale},{AssetDatabase.GetAssetPath(mat)}\n");
            }
            File.WriteAllText(Path.Combine(Folder, $"materials_{scene}.csv"), sb.ToString());
        }
    }
}
