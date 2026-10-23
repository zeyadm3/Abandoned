using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Smooth meshes made in code for the stations' vehicles, street furniture and fittings, where a box would read as a
    /// box: a <b>loft</b> (rounded cross-sections strung along z: car bodies, cabs, hoods, cushions) and a <b>lathe</b> (a
    /// profile turned round y: tyres, rims, lamps, hydrants, tanks, bollards). Each mesh is saved once as an asset in the
    /// station's art folder (<c>Meshes/</c>) and rewritten in place on the next rebuild, so scenes keep their references.
    /// </summary>
    public static class StationMeshes
    {
        /// <summary>
        /// One cross-section of a loft at <see cref="Z"/>: from <see cref="Bottom"/> to <see cref="Top"/>, half as wide as
        /// <see cref="BottomHalf"/> at the bottom and <see cref="TopHalf"/> at the top (a car's tumblehome), its corners
        /// rounded by a superellipse of exponent <see cref="Round"/> (2 an ellipse, 4 a soft box, 10 nearly square).
        /// </summary>
        public readonly struct Ring
        {
            public readonly float Z, Bottom, Top, BottomHalf, TopHalf, Round;
            public Ring(float z, float bottom, float top, float bottomHalf, float topHalf, float round = 4f)
            { Z = z; Bottom = bottom; Top = top; BottomHalf = bottomHalf; TopHalf = topHalf; Round = round; }
        }

        private static readonly Dictionary<string, Mesh> Cache = new();

        /// <summary>A loft through <paramref name="rings"/> (in increasing z), closed at both ends.</summary>
        public static Mesh Loft(StationArt art, string key, IList<Ring> rings, int sides = 28) =>
            Asset(art, "Loft_" + key, () => BuildLoft(rings, sides));

        /// <summary>A profile (x radius, y height) turned about y. Trace sides upward and top faces inward (bottom faces
        /// outward) so normals point out; repeat a point for a sharp edge there.</summary>
        /// <param name="sweep">Degrees turned (360 a full turn; 180 a half, e.g. a wheel well), from local +x toward +z.</param>
        public static Mesh Lathe(StationArt art, string key, IList<Vector2> profile, int segments = 24, float sweep = 360f) =>
            Asset(art, "Lathe_" + key, () => BuildLathe(profile, segments, sweep));

        /// <summary>A tyre on its side (axis along y, centre at the origin): radius, width and how round its shoulder is.</summary>
        public static Mesh Tyre(StationArt art, float radius, float width)
        {
            float r = radius, w = width * .5f, rim = radius * .62f, shoulder = Mathf.Min(.05f, width * .22f);
            return Lathe(art, $"Tyre_{Mathf.RoundToInt(radius * 100)}_{Mathf.RoundToInt(width * 100)}", new[]
            {
                new Vector2(rim, -w + .01f), new Vector2(rim, -w + .01f), new Vector2(r - shoulder * 1.6f, -w),
                new Vector2(r - shoulder * .4f, -w + shoulder * .5f), new Vector2(r, -w + shoulder * 1.4f),
                new Vector2(r + .004f, 0f), new Vector2(r, w - shoulder * 1.4f), new Vector2(r - shoulder * .4f, w - shoulder * .5f),
                new Vector2(r - shoulder * 1.6f, w), new Vector2(rim, w - .01f), new Vector2(rim, w - .01f),
            }, 32);
        }

        /// <summary>A dished wheel rim (axis along y, its face toward +y): steel or chrome.</summary>
        public static Mesh Rim(StationArt art, float radius, float width)
        {
            float r = radius, w = width * .5f;
            return Lathe(art, $"Rim_{Mathf.RoundToInt(radius * 100)}_{Mathf.RoundToInt(width * 100)}", new[]
            {
                // Up the barrel, then in across the face to the hub: a face is traced inward so its normal points out.
                new Vector2(r, -w), new Vector2(r, w), new Vector2(r, w), new Vector2(r * .9f, w * .75f), new Vector2(r * .72f, w * .35f),
                new Vector2(r * .34f, w * .45f), new Vector2(r * .3f, w * .6f), new Vector2(r * .3f, w * .6f), new Vector2(0f, w * .6f),
            }, 28);
        }

        /// <summary>A flat half disc in the local x-z plane (z &gt;= 0), its face toward +y: a wheel well's dark liner.</summary>
        public static Mesh HalfDisc(StationArt art, float radius) =>
            Lathe(art, $"HalfDisc_{Mathf.RoundToInt(radius * 100)}", new[] { new Vector2(radius, 0f), new Vector2(0f, 0f) }, 16, 180f);

        /// <summary>The rotation that stands a <see cref="HalfDisc"/> up over a wheel: its round edge on top, its face
        /// toward +x (<paramref name="right"/>) or -x.</summary>
        public static Quaternion WellTurn(bool right) => Quaternion.LookRotation(Vector3.up, right ? Vector3.right : Vector3.left);

        /// <summary>A triangular prism: a 1 x 1 triangle (base on y 0 from x -0.5 to 0.5, apex at y 1) run 1 m along z,
        /// centred. Scale it to a roof's span, rise and length (a gable end, an attic, a dormer).</summary>
        public static Mesh Prism(StationArt art) => Asset(art, "Prism", BuildPrism);

        /// <summary>Draws <paramref name="mesh"/> as a child (no collider).</summary>
        public static GameObject Draw(Transform parent, string name, Mesh mesh, Vector3 at, Vector3 euler, Vector3 scale, Material m,
            bool shadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = at;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = m;
            r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            return go;
        }

        /// <summary>A box from <paramref name="a"/> to <paramref name="b"/> (local), <paramref name="thick"/> square: a strut,
        /// a pillar, a pipe run, a cable.</summary>
        public static GameObject Strut(Transform parent, string name, Vector3 a, Vector3 b, float thick, Material m, float deep = -1f)
        {
            Vector3 d = b - a;
            GameObject go = GreyboxFactory.Box(name, parent, (a + b) * .5f, new Vector3(thick, deep > 0f ? deep : thick, Mathf.Max(.001f, d.magnitude)), m, false);
            if (d.sqrMagnitude > 1e-6f)
            {
                Vector3 up = Mathf.Abs(Vector3.Dot(d.normalized, Vector3.up)) > .95f ? Vector3.forward : Vector3.up;
                go.transform.localRotation = Quaternion.LookRotation(d.normalized, up);
            }
            return go;
        }

        // ---- Assets ---------------------------------------------------------------------------------------------

        private static Mesh Asset(StationArt art, string key, System.Func<Mesh> build)
        {
            string folder = art.Folder + "/Meshes";
            string name = $"{art.Prefix}_{key}";
            string path = $"{folder}/{name}.asset";
            if (Cache.TryGetValue(path, out Mesh cached) && cached != null) return cached;
            PolishAssets.EnsureFolder(folder);
            Mesh mesh = build();
            mesh.name = name;
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(mesh, existing);
                existing.name = name;
                Object.DestroyImmediate(mesh);
                EditorUtility.SetDirty(existing);
                mesh = existing;
            }
            else AssetDatabase.CreateAsset(mesh, path);
            Cache[path] = mesh;
            return mesh;
        }

        // ---- Loft -----------------------------------------------------------------------------------------------

        private static Vector3 RingPoint(Ring ring, int i, int sides)
        {
            // Start at the bottom so the seam (where the ring closes) is underneath, out of sight.
            float t = -Mathf.PI * .5f + 2f * Mathf.PI * i / sides;
            float c = Mathf.Cos(t), s = Mathf.Sin(t), e = 2f / Mathf.Max(1f, ring.Round);
            float u = Mathf.Sign(c) * Mathf.Pow(Mathf.Abs(c), e);
            float v = Mathf.Sign(s) * Mathf.Pow(Mathf.Abs(s), e);
            float k = (v + 1f) * .5f;
            float half = Mathf.Lerp(ring.BottomHalf, ring.TopHalf, k);
            return new Vector3(u * half, Mathf.Lerp(ring.Bottom, ring.Top, k), ring.Z);
        }

        private static Mesh BuildLoft(IList<Ring> rings, int sides)
        {
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            int row = sides + 1;
            float along = 0f;
            for (int r = 0; r < rings.Count; r++)
            {
                if (r > 0) along += Mathf.Abs(rings[r].Z - rings[r - 1].Z);
                for (int i = 0; i <= sides; i++)
                {
                    vertices.Add(RingPoint(rings[r], i % sides, sides));
                    uv.Add(new Vector2((float)i / sides * 3f, along));
                }
            }
            for (int r = 0; r < rings.Count - 1; r++)
            for (int i = 0; i < sides; i++)
            {
                int a = r * row + i, b = a + 1, c = a + row, d = c + 1;
                triangles.Add(a); triangles.Add(b); triangles.Add(c);
                triangles.Add(b); triangles.Add(d); triangles.Add(c);
            }
            Cap(rings[0], false);
            Cap(rings[rings.Count - 1], true);

            void Cap(Ring ring, bool end)
            {
                int centre = vertices.Count;
                vertices.Add(new Vector3(0f, (ring.Bottom + ring.Top) * .5f, ring.Z));
                uv.Add(new Vector2(.5f, .5f));
                for (int i = 0; i < sides; i++)
                {
                    Vector3 p = RingPoint(ring, i, sides);
                    vertices.Add(p);
                    uv.Add(new Vector2(p.x, p.y));
                }
                for (int i = 0; i < sides; i++)
                {
                    int p0 = centre + 1 + i, p1 = centre + 1 + (i + 1) % sides;
                    if (end) { triangles.Add(centre); triangles.Add(p0); triangles.Add(p1); }
                    else { triangles.Add(centre); triangles.Add(p1); triangles.Add(p0); }
                }
            }

            Mesh mesh = Finish(vertices, uv, triangles);
            // The seam's two copies of each ring point take one normal between them.
            Vector3[] normals = mesh.normals;
            for (int r = 0; r < rings.Count; r++)
            {
                int a = r * row, b = a + sides;
                Vector3 n = (normals[a] + normals[b]).normalized;
                normals[a] = n;
                normals[b] = n;
            }
            mesh.normals = normals;
            mesh.RecalculateTangents();
            return mesh;
        }

        // ---- Prism ----------------------------------------------------------------------------------------------

        private static Mesh BuildPrism()
        {
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            Vector3 l0 = new(-.5f, 0f, -.5f), r0 = new(.5f, 0f, -.5f), t0 = new(0f, 1f, -.5f);
            Vector3 l1 = new(-.5f, 0f, .5f), r1 = new(.5f, 0f, .5f), t1 = new(0f, 1f, .5f);
            void Face(params Vector3[] p)
            {
                int first = vertices.Count;
                Vector3 n = Vector3.Cross(p[1] - p[0], p[2] - p[0]).normalized;
                Vector3 u = (p[1] - p[0]).normalized, w = Vector3.Cross(n, u);
                foreach (Vector3 q in p)
                {
                    vertices.Add(q);
                    uv.Add(new Vector2(Vector3.Dot(q, u), Vector3.Dot(q, w)));
                }
                for (int i = 1; i + 1 < p.Length; i++) { triangles.Add(first); triangles.Add(first + i); triangles.Add(first + i + 1); }
            }
            Face(l0, t0, r0);                 // the back gable, facing -z
            Face(l1, r1, t1);                 // the front gable, facing +z
            Face(l0, l1, t1, t0);             // the west slope
            Face(r0, t0, t1, r1);             // the east slope
            Face(l0, r0, r1, l1);             // the underside
            return Finish(vertices, uv, triangles);
        }

        // ---- Lathe ----------------------------------------------------------------------------------------------

        private static Mesh BuildLathe(IList<Vector2> profile, int segments, float sweep)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            int row = segments + 1, count = profile.Count;
            float length = 0f;
            for (int j = 0; j < count; j++)
            {
                if (j > 0) length += Vector2.Distance(profile[j], profile[j - 1]);
                Vector2 n2 = ProfileNormal(profile, j);
                for (int i = 0; i <= segments; i++)
                {
                    float a = sweep * Mathf.Deg2Rad * i / segments, c = Mathf.Cos(a), s = Mathf.Sin(a);
                    vertices.Add(new Vector3(profile[j].x * c, profile[j].y, profile[j].x * s));
                    normals.Add(new Vector3(n2.x * c, n2.y, n2.x * s));
                    uv.Add(new Vector2((float)i / segments * 2f, length));
                }
            }
            for (int j = 0; j < count - 1; j++)
            for (int i = 0; i < segments; i++)
            {
                int a = j * row + i, b = a + 1, c = a + row, d = c + 1;
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(b); triangles.Add(c); triangles.Add(d);
            }
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        // The outward normal (radius, height) of a profile point from the segments either side; a repeated point takes
        // only the segment on its own side, which makes the crease.
        private static Vector2 ProfileNormal(IList<Vector2> profile, int j)
        {
            Vector2 prev = j > 0 ? profile[j] - profile[j - 1] : Vector2.zero;
            Vector2 next = j < profile.Count - 1 ? profile[j + 1] - profile[j] : Vector2.zero;
            if (j > 0 && j < profile.Count - 1)
            {
                if (next.sqrMagnitude < 1e-8f) next = Vector2.zero;
                else if (prev.sqrMagnitude < 1e-8f) prev = Vector2.zero;
            }
            Vector2 n = Vector2.zero;
            if (prev.sqrMagnitude > 1e-8f) n += new Vector2(prev.y, -prev.x).normalized;
            if (next.sqrMagnitude > 1e-8f) n += new Vector2(next.y, -next.x).normalized;
            return n.sqrMagnitude > 1e-8f ? n.normalized : Vector2.right;
        }

        private static Mesh Finish(List<Vector3> vertices, List<Vector2> uv, List<int> triangles)
        {
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
