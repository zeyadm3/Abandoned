using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.13: the "two textures swapping" flicker. Builders often lay pieces flush on each other (a kerb on its walk,
    /// a plant on a roof, ceiling coffers whose trims overlap their neighbours', boxes merged into one mesh), so two
    /// faces share a plane exactly and the depth test picks a different winner every frame. Each such pair found by
    /// <see cref="GraphicsAudit.FindFights"/> is pulled apart in steps of <see cref="Lift"/> (2.5 mm): invisible,
    /// and a thousand times what a 32-bit depth buffer resolves at the far end of any map.
    ///  - Objects (a renderer, or its whole LOD group so every level moves together): per plane direction, the touching
    ///    objects form a graph that is coloured greedily, largest first. Colour 0 stays put; colour k moves k steps toward
    ///    the side its faces look at. Touching objects never share a colour, so they always end at different depths, even
    ///    in a grid of identical coffers or a piece pinned between two others.
    ///  - One merged mesh with two faces on top of each other: the smaller face gets its own copy of its three corners,
    ///    lifted. Imported models are fixed when they import (<see cref="CoplanarModelImport"/>).
    /// Runs on every map after its builder (Rebuild All Maps, through AvOverhaul) and is a no-op the second time.
    /// </summary>
    public static class CoplanarFix
    {
        // Objects step out in multiples of Lift; faces inside a mesh by odd multiples of half of it (FaceLift), so an
        // object moved k steps can never land exactly on a neighbour's lifted face (the difference is always an odd half-step).
        public const float Lift = 0.0025f;
        private static float FaceLift(int layer) => Lift * 0.5f * (2 * layer - 1);
        private const float Touching = 0.001f; // closer than this counts as the same plane; the fix leaves 2 mm or more

        [MenuItem(ToolMenu.Art + "Separate Coplanar Faces (Open Scene)", priority = ToolMenu.ArtP + 80)]
        public static void FixOpenSceneMenu()
        {
            var (moved, faces, left) = FixOpenScene();
            EditorUtility.DisplayDialog("Coplanar faces", $"{moved} objects moved, {faces} faces lifted, {left} pairs left.", "OK");
        }

        // COPLANAR_DEBUG=<part of a name>: log every step decision for objects whose name contains it.
        private static readonly string debugName = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("COPLANAR_DEBUG")) ? null : Environment.GetEnvironmentVariable("COPLANAR_DEBUG");

        /// <summary>Batch: <c>Tools/unity.sh exec Abandoned.EditorTools.CoplanarFix.FixMapsAndExit</c>, every map
        /// (COPLANAR_MAP=<scene name> for one).</summary>
        public static void FixMapsAndExit()
        {
            int code = 0;
            string only = Environment.GetEnvironmentVariable("COPLANAR_MAP");
            foreach (string path in AvOverhaul.MapScenes)
            {
                if (!string.IsNullOrEmpty(only) && !path.Contains(only)) continue;
                try
                {
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    if (FixOpenScene().left > 0) code = 1;
                }
                catch (Exception e) { Debug.LogException(e); code = 1; }
            }
            EditorApplication.Exit(code);
        }

        /// <summary>Separates every touching coplanar pair in the open scene and (unless told not to) saves it. Returns
        /// objects moved, faces lifted and the pairs still closer than 1 mm afterwards.</summary>
        public static (int moved, int faces, int left) FixOpenScene(bool save = true)
        {
            var scene = EditorSceneManager.GetActiveScene();
            RestoreModels();
            int moved = 0, faces = 0;
            // Faces inside one mesh first: moving whole objects never changes those, so they stay fixed below.
            for (int pass = 0; pass < 4; pass++)
            {
                var inside = GraphicsAudit.FindFights(out _).Where(f => f.Gap < Touching && f.A == f.B).ToList();
                if (inside.Count == 0) break;
                faces += LiftFaces(inside);
            }
            // Then objects, against every overlapping neighbour within reach and where it already sits, so a step never
            // lands one piece on another. Later passes catch what a move in another plane family brought together.
            for (int pass = 0; pass < 4; pass++)
            {
                var near = GraphicsAudit.FindFights(out _, Reach).Where(f => f.A != f.B).ToList();
                if (!near.Any(f => f.Gap < Touching)) break;
                moved += MoveObjects(near);
            }
            var after = GraphicsAudit.FindFights(out _);
            int left = after.Count(f => f.Gap < Touching), flicker = after.Count(f => f.Gap < GraphicsAudit.Flicker);
            if (save)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
            }
            Debug.Log($"[CoplanarFix] {scene.name}: {moved} objects moved, {faces} faces lifted, {left} pairs closer than 1 mm left, {flicker} closer than 0.1 mm (can flicker).");
            return (moved, faces, left);
        }

        private const float Reach = 0.025f; // neighbours this close are taken into account when choosing a step
        private const float Clear = 0.0011f; // the least a step may leave between two overlapping faces

        // What moves for a renderer: its LOD group, so the levels stay together.
        private static Transform Unit(Renderer r)
        {
            LODGroup g = r.GetComponentInParent<LODGroup>();
            return g != null ? g.transform : r.transform;
        }

        // A plane direction without its sign: a face looking down and one looking up fight in the same plane family.
        private static (int, int, int) Axis(Vector3 n)
        {
            var q = (Mathf.RoundToInt(n.x * 20f), Mathf.RoundToInt(n.y * 20f), Mathf.RoundToInt(n.z * 20f));
            bool flip = q.Item1 < 0 || (q.Item1 == 0 && (q.Item2 < 0 || (q.Item2 == 0 && q.Item3 < 0)));
            return flip ? (-q.Item1, -q.Item2, -q.Item3) : q;
        }

        private sealed class Node
        {
            public Transform Unit;
            public float Size, Facing; // Facing: along the axis, which side its overlapping faces look at
            public float? Step; // chosen move along the axis, metres
            // Per neighbour: the gaps (neighbour minus this, along the axis) between each pair of overlapping faces.
            public readonly Dictionary<Node, HashSet<float>> Gaps = new();
        }

        private static int MoveObjects(List<GraphicsAudit.Fight> fights)
        {
            IReadOnlyList<GraphicsAudit.Tri> tris = GraphicsAudit.LastTriangles;
            // A LOD group with overlapping parts of its own can't move as one: its renderers move instead.
            var split = new HashSet<Transform>(fights.Where(f => Unit(f.A) == Unit(f.B)).Select(f => Unit(f.A)));
            Transform NodeOf(Renderer r) => split.Contains(Unit(r)) ? r.transform : Unit(r);

            var groups = new Dictionary<(int, int, int), Dictionary<Transform, Node>>();
            foreach (GraphicsAudit.Fight f in fights)
            {
                Transform ua = NodeOf(f.A), ub = NodeOf(f.B);
                if (ua == ub) continue;
                // One pair of objects can overlap in several directions (a kerb's bottom and its side): each face pair
                // goes to its own plane family.
                foreach (var (ia, ib) in f.Faces)
                {
                    GraphicsAudit.Tri ta = tris[ia], tb = tris[ib];
                    var axisKey = Axis(ta.N);
                    Vector3 axis = new Vector3(axisKey.Item1, axisKey.Item2, axisKey.Item3).normalized;
                    if (!groups.TryGetValue(axisKey, out var nodes)) groups[axisKey] = nodes = new Dictionary<Transform, Node>();
                    Node a = NodeFor(nodes, ua, f.A), b = NodeFor(nodes, ub, f.B);
                    float facing = Vector3.Dot(ta.N, axis);
                    a.Facing += facing;
                    b.Facing += facing;
                    if (!a.Gaps.TryGetValue(b, out var ab)) a.Gaps[b] = ab = new HashSet<float>();
                    if (!b.Gaps.TryGetValue(a, out var ba)) b.Gaps[a] = ba = new HashSet<float>();
                    // Along the face's own normal (signed to the family's axis): a crate turned 2 degrees is still exact.
                    float gap = Vector3.Dot(tb.A - ta.A, ta.N) * Mathf.Sign(facing);
                    gap = Mathf.Round(gap * 20000f) / 20000f; // 0.05 mm buckets keep the sets small
                    ab.Add(gap);
                    ba.Add(-gap);
                }
            }
            var offsets = new Dictionary<Transform, Vector3>();
            foreach (var kv in groups)
            {
                Vector3 axis = new Vector3(kv.Key.Item1, kv.Key.Item2, kv.Key.Item3).normalized;
                // Largest first, so floors, walls and slabs keep still and the details step out in front of them.
                foreach (Node n in kv.Value.Values.OrderByDescending(n => n.Size).ThenBy(n => n.Unit.GetInstanceID()))
                {
                    float side = n.Facing >= 0f ? 1f : -1f;
                    n.Step = 0f;
                    for (int k = 0; k <= 24; k++)
                    {
                        // 0, then out toward the side the faces look at, then (rarely) back behind them.
                        float step = (k <= 16 ? k : -(k - 16)) * side * Lift;
                        if (n.Gaps.All(g => g.Value.All(gap => Mathf.Abs(gap + (g.Key.Step ?? 0f) - step) >= Clear)))
                        {
                            n.Step = step;
                            break;
                        }
                    }
                    if (Mathf.Abs(n.Step.Value) > 1e-6f) Add(offsets, n.Unit, axis * n.Step.Value);
                    if (debugName != null && n.Unit.name.Contains(debugName))
                        Debug.Log($"[CoplanarFix] debug {n.Unit.name}#{n.Unit.GetInstanceID()} axis {axis} size {n.Size:0} facing {n.Facing:0.0} step {n.Step * 1000f:0.0}mm; " +
                                  string.Join("; ", n.Gaps.Select(g => $"{g.Key.Unit.name}#{g.Key.Unit.GetInstanceID()} at {(g.Key.Step ?? 0f) * 1000f:0.0}mm gaps {string.Join(",", g.Value.Select(v => (v * 1000f).ToString("0.00")))}")));
                }
            }
            foreach (var kv in offsets)
            {
                Undo.RecordObject(kv.Key, "Separate coplanar faces");
                kv.Key.position += kv.Value;
                PrefabUtility.RecordPrefabInstancePropertyModifications(kv.Key);
            }
            return offsets.Count;
        }

        private static Node NodeFor(Dictionary<Transform, Node> nodes, Transform unit, Renderer r)
        {
            if (!nodes.TryGetValue(unit, out Node n)) nodes[unit] = n = new Node { Unit = unit, Size = Size(r) };
            n.Size = Mathf.Max(n.Size, Size(r));
            return n;
        }

        private static void Add(Dictionary<Transform, Vector3> offsets, Transform t, Vector3 v) =>
            offsets[t] = offsets.TryGetValue(t, out Vector3 old) ? old + v : v;

        private static float Size(Renderer r) => r.bounds.size.x * r.bounds.size.y + r.bounds.size.y * r.bounds.size.z + r.bounds.size.x * r.bounds.size.z;

        public const string FixedMeshFolder = "Assets/_Project/Art/Generated/CoplanarMeshes";

        // Inside one mesh: the smaller face of each pair gets three new corners of its own, lifted along its normal.
        // A mesh of an imported model can't change, and the kit is modelled in centimetres and scaled up in the scenes,
        // so the lift is only right at a known scale: each (model mesh, scale) gets a fixed copy, saved under
        // FixedMeshFolder, and that scale's renderers draw the copy.
        private static int LiftFaces(IEnumerable<GraphicsAudit.Fight> fights)
        {
            IReadOnlyList<GraphicsAudit.Tri> tris = GraphicsAudit.LastTriangles;
            var perMesh = new Dictionary<(Mesh, Vector3Int), (MeshRenderer renderer, List<(GraphicsAudit.Tri, GraphicsAudit.Tri)> pairs)>();
            foreach (GraphicsAudit.Fight f in fights)
            {
                Mesh mesh = f.A.GetComponent<MeshFilter>().sharedMesh;
                var key = (mesh, Imported(mesh) ? ScaleKey(f.A.transform) : Vector3Int.zero);
                if (!perMesh.TryGetValue(key, out var entry)) perMesh[key] = entry = (f.A, new List<(GraphicsAudit.Tri, GraphicsAudit.Tri)>());
                entry.pairs.AddRange(f.Faces.Select(p => (tris[p.Item1], tris[p.Item2])));
            }
            int lifted = 0;
            foreach (var kv in perMesh)
            {
                Mesh mesh = kv.Key.Item1;
                Matrix4x4 toWorld = kv.Value.renderer.transform.localToWorldMatrix;
                Dictionary<(int, int), float> layers = Offsets(kv.Value.pairs);
                if (!Imported(mesh))
                {
                    lifted += LiftInMesh(mesh, toWorld, layers, true);
                    continue;
                }
                Mesh copy = FixedCopy(mesh, kv.Key.Item2);
                lifted += LiftInMesh(copy, toWorld, layers, true);
                foreach (MeshFilter filter in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
                {
                    if (filter.sharedMesh != mesh || ScaleKey(filter.transform) != kv.Key.Item2) continue;
                    Undo.RecordObject(filter, "Separate coplanar faces");
                    filter.sharedMesh = copy;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(filter);
                }
            }
            AssetDatabase.SaveAssets();
            return lifted;
        }

        private static bool Imported(Mesh mesh)
        {
            string path = AssetDatabase.GetAssetPath(mesh);
            return !string.IsNullOrEmpty(path) && !path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase);
        }

        // Millimetre-rounded absolute scale: instances this close share a copy.
        private static Vector3Int ScaleKey(Transform t)
        {
            Vector3 s = t.lossyScale;
            return new Vector3Int(Mathf.RoundToInt(Mathf.Abs(s.x) * 1000f), Mathf.RoundToInt(Mathf.Abs(s.y) * 1000f), Mathf.RoundToInt(Mathf.Abs(s.z) * 1000f));
        }

        // A fresh copy of the model's mesh, saved over any older copy in place (same GUID, so other maps keep theirs),
        // and noted in the registry so the next run can point its renderers back at the model first.
        private static Mesh FixedCopy(Mesh source, Vector3Int scale)
        {
            PolishAssets.EnsureFolder(FixedMeshFolder);
            string modelPath = AssetDatabase.GetAssetPath(source);
            string model = System.IO.Path.GetFileNameWithoutExtension(modelPath);
            string path = $"{FixedMeshFolder}/{model}_{source.name}_{scale.x}x{scale.y}x{scale.z}.asset";
            Mesh fresh = Object.Instantiate(source);
            fresh.name = System.IO.Path.GetFileNameWithoutExtension(path);
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            Mesh copy;
            if (existing == null)
            {
                AssetDatabase.CreateAsset(fresh, path);
                copy = fresh;
            }
            else
            {
                EditorUtility.CopySerialized(fresh, existing);
                existing.name = fresh.name;
                Object.DestroyImmediate(fresh);
                copy = existing;
            }
            Registry registry = LoadRegistry();
            string guid = AssetDatabase.AssetPathToGUID(path);
            registry.entries.RemoveAll(e => e.copy == guid);
            registry.entries.Add(new Source { copy = guid, model = modelPath, mesh = source.name });
            System.IO.File.WriteAllText(RegistryPath, JsonUtility.ToJson(registry, true));
            return copy;
        }

        private static string RegistryPath => FixedMeshFolder + "/Sources.json";

        [Serializable]
        private class Source
        {
            public string copy, model, mesh;
        }

        [Serializable]
        private class Registry
        {
            public List<Source> entries = new();
        }

        private static Registry LoadRegistry() =>
            System.IO.File.Exists(RegistryPath) ? JsonUtility.FromJson<Registry>(System.IO.File.ReadAllText(RegistryPath)) ?? new Registry() : new Registry();

        // Every renderer drawing a fixed copy goes back to its model's own mesh, so a changed model or scale is fixed
        // from scratch rather than through a stale copy.
        private static void RestoreModels()
        {
            var sources = LoadRegistry().entries.ToDictionary(e => e.copy, e => e);
            foreach (MeshFilter filter in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh == null) continue;
                string path = AssetDatabase.GetAssetPath(mesh);
                if (!path.StartsWith(FixedMeshFolder + "/")) continue;
                if (!sources.TryGetValue(AssetDatabase.AssetPathToGUID(path), out Source s)) continue;
                Mesh original = AssetDatabase.LoadAllAssetsAtPath(s.model).OfType<Mesh>().FirstOrDefault(m => m.name == s.mesh);
                if (original == null) continue;
                filter.sharedMesh = original;
                PrefabUtility.RecordPrefabInstancePropertyModifications(filter);
            }
        }

        /// <summary>Batch, once: reimport every environment model (the 0.24.13 work briefly had an import step that
        /// lifted faces at the wrong scale).</summary>
        public static void ReimportModelsAndExit()
        {
            int n = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets/_Project/Art/Custom/Environment", "Assets/_Project/Art/ThirdParty/Kenney" }))
            {
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
                n++;
            }
            Debug.Log($"[CoplanarFix] {n} models reimported.");
            EditorApplication.Exit(0);
        }

        /// <summary>How far each overlapping face moves along its own normal (metres): largest face first, each takes the
        /// smallest offset (0, then out or back by odd multiples of <see cref="FaceLift"/>) that leaves every face it
        /// overlaps at least <see cref="Clear"/> away, given where those already sit. Out wins the depth test, back loses
        /// it; either way the same face wins every frame.</summary>
        private static Dictionary<(int, int), float> Offsets(List<(GraphicsAudit.Tri a, GraphicsAudit.Tri b)> pairs)
        {
            var area = new Dictionary<(int, int), float>();
            // Per face: each overlapping face and its gap (that face minus this one, along this one's normal).
            var near = new Dictionary<(int, int), List<((int, int) other, float gap)>>();
            foreach (var (a, b) in pairs)
            {
                (int, int) ka = (a.Sub, a.Index), kb = (b.Sub, b.Index);
                area[ka] = Area(a);
                area[kb] = Area(b);
                float gap = Vector3.Dot(b.A - a.A, a.N);
                if (!near.TryGetValue(ka, out var la)) near[ka] = la = new List<((int, int), float)>();
                if (!near.TryGetValue(kb, out var lb)) near[kb] = lb = new List<((int, int), float)>();
                la.Add((kb, gap));
                lb.Add((ka, -gap));
            }
            var offset = new Dictionary<(int, int), float>();
            foreach (var face in area.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).Select(kv => kv.Key))
            {
                float chosen = 0f;
                for (int k = 0; k <= 16; k++)
                {
                    float candidate = k == 0 ? 0f : FaceLift((k + 1) / 2) * (k % 2 == 1 ? 1f : -1f);
                    if (near[face].All(n => Mathf.Abs(n.gap + (offset.TryGetValue(n.other, out float o) ? o : 0f) - candidate) >= Clear))
                    {
                        chosen = candidate;
                        break;
                    }
                }
                offset[face] = chosen;
            }
            return offset.Where(kv => Mathf.Abs(kv.Value) > 1e-7f).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static float Area(GraphicsAudit.Tri t) => Vector3.Cross(t.B - t.A, t.C - t.A).magnitude;

        /// <summary>Gives each listed face (submesh, first index) three corners of its own, moved its offset (metres)
        /// along the face's normal as <paramref name="toWorld"/> places the mesh. Returns the faces lifted.</summary>
        public static int LiftInMesh(Mesh mesh, Matrix4x4 toWorld, Dictionary<(int sub, int index), float> faces, bool undo)
        {
            if (faces.Count == 0) return 0;
            if (undo) Undo.RecordObject(mesh, "Separate coplanar faces");
            var pos = new List<Vector3>();
            var nrm = new List<Vector3>();
            var tan = new List<Vector4>();
            var col = new List<Color32>();
            mesh.GetVertices(pos);
            mesh.GetNormals(nrm);
            mesh.GetTangents(tan);
            mesh.GetColors(col);
            var uv = new List<Vector4>[8];
            for (int c = 0; c < 8; c++)
            {
                if (!mesh.HasVertexAttribute(VertexAttribute.TexCoord0 + c)) continue;
                uv[c] = new List<Vector4>();
                mesh.GetUVs(c, uv[c]);
            }
            int vertexCount = pos.Count;
            // World lift to mesh space: the renderer's own scale may be anything.
            Matrix4x4 toLocal = toWorld.inverse;
            var subs = new List<int>[mesh.subMeshCount];
            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                subs[s] = new List<int>();
                mesh.GetTriangles(subs[s], s);
            }
            // Decided once: the lists grow with every lifted face, so a per-face "Count == vertexCount" test only held for
            // the first one and every later face got zero normals, tangents and UVs (black faces; fixed 2026-10-24).
            bool hasNormals = nrm.Count == vertexCount, hasTangents = tan.Count == vertexCount, hasColors = col.Count == vertexCount;
            var hasUv = new bool[8];
            for (int ch = 0; ch < 8; ch++) hasUv[ch] = uv[ch] != null && uv[ch].Count == vertexCount;
            int lifted = 0;
            foreach (var ((sub, index), lift) in faces)
            {
                List<int> tri = subs[sub];
                Vector3 a = toWorld.MultiplyPoint3x4(pos[tri[index]]), b = toWorld.MultiplyPoint3x4(pos[tri[index + 1]]), c = toWorld.MultiplyPoint3x4(pos[tri[index + 2]]);
                Vector3 n = Vector3.Cross(b - a, c - a).normalized;
                Vector3 offset = toLocal.MultiplyVector(n * lift);
                for (int k = 0; k < 3; k++)
                {
                    int src = tri[index + k];
                    pos.Add(pos[src] + offset);
                    if (hasNormals) nrm.Add(nrm[src]);
                    if (hasTangents) tan.Add(tan[src]);
                    if (hasColors) col.Add(col[src]);
                    for (int ch = 0; ch < 8; ch++) if (hasUv[ch]) uv[ch].Add(uv[ch][src]);
                    tri[index + k] = pos.Count - 1;
                }
                lifted++;
            }
            if (pos.Count > 65535) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(pos);
            if (nrm.Count == pos.Count) mesh.SetNormals(nrm);
            if (tan.Count == pos.Count) mesh.SetTangents(tan);
            if (col.Count == pos.Count) mesh.SetColors(col);
            for (int ch = 0; ch < 8; ch++)
            {
                if (uv[ch] == null || uv[ch].Count != pos.Count) continue;
                // Keep each channel's width: widening a 2D channel to 4D would grow every vertex for nothing.
                int dim = mesh.GetVertexAttributeDimension(VertexAttribute.TexCoord0 + ch);
                if (dim == 2) mesh.SetUVs(ch, uv[ch].Select(x => new Vector2(x.x, x.y)).ToList());
                else if (dim == 3) mesh.SetUVs(ch, uv[ch].Select(x => new Vector3(x.x, x.y, x.z)).ToList());
                else mesh.SetUVs(ch, uv[ch]);
            }
            for (int s = 0; s < mesh.subMeshCount; s++) mesh.SetTriangles(subs[s], s, false);
            mesh.RecalculateBounds();
            if (undo) EditorUtility.SetDirty(mesh);
            return lifted;
        }
    }
}
