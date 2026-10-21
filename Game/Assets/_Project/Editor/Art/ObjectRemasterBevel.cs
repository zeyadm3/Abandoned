using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Closed chamfers with smooth edge normals, preserving each original face's UVs and material slot.</summary>
    public static class ObjectRemasterBevel
    {
        public static Mesh Build(ObjectRemasterMeshData source, IReadOnlyList<ObjectRemasterMeshData.Box> boxes,
            Matrix4x4 world, Material[] materials, ObjectRemasterConfig config, out int changed)
        {
            changed = 0;
            var accepted = new List<(ObjectRemasterMeshData.Box box, Vector3 bevel)>();
            foreach (ObjectRemasterMeshData.Box box in boxes)
            {
                Vector3 scales = new(world.MultiplyVector(box.Axes[0]).magnitude, world.MultiplyVector(box.Axes[1]).magnitude,
                    world.MultiplyVector(box.Axes[2]).magnitude);
                Vector3 size = Vector3.Scale(box.Half * 2f, scales);
                float shortest = Mathf.Min(size.x, size.y, size.z);
                if (shortest < config.SmallestPart || Mathf.Max(size.x, size.y, size.z) > config.LargestPart) continue;
                float radius = config.HardEdge;
                foreach (List<int> face in box.Faces)
                {
                    int slot = source.Triangles[face[0]].Material;
                    string name = slot < materials.Length && materials[slot] != null ? materials[slot].name.ToLowerInvariant() : "";
                    if (name.Contains("wood") || name.Contains("oak") || name.Contains("timber")) radius = Mathf.Max(radius, config.WoodEdge);
                    if (name.Contains("fabric") || name.Contains("leather") || name.Contains("upholst") || name.Contains("cushion")) radius = Mathf.Max(radius, config.PaddedEdge);
                }
                radius = Mathf.Min(radius, shortest * config.ThicknessShare);
                if (radius < .0005f) continue;
                accepted.Add((box, new Vector3(radius / scales.x, radius / scales.y, radius / scales.z)));
            }
            if (accepted.Count == 0) return null;
            var output = new Builder(source);
            var removed = new HashSet<int>(accepted.SelectMany(x => x.box.Triangles));
            for (int i = 0; i < source.Triangles.Count; i++) if (!removed.Contains(i)) output.Keep(source.Triangles[i]);
            foreach (var item in accepted) output.Chamfer(item.box, item.bevel);
            changed = accepted.Count;
            return output.Finish();
        }

        private sealed class Builder
        {
            private readonly ObjectRemasterMeshData source;
            private readonly List<Vector3> positions = new(), normals = new();
            private readonly List<Color> colors = new();
            private readonly List<Vector4>[] uv = new List<Vector4>[8];
            private readonly List<int>[] indices;
            private readonly Dictionary<int, int> copied = new();

            public Builder(ObjectRemasterMeshData source)
            {
                this.source = source;
                indices = Enumerable.Range(0, source.MaterialCount).Select(_ => new List<int>()).ToArray();
                for (int i = 0; i < uv.Length; i++) if (source.UV[i] != null) uv[i] = new List<Vector4>();
            }

            public void Keep(ObjectRemasterMeshData.Triangle tri)
            {
                Vector3 flat = Vector3.Cross(source.Positions[tri.B] - source.Positions[tri.A], source.Positions[tri.C] - source.Positions[tri.A]).normalized;
                foreach (int old in new[] { tri.A, tri.B, tri.C })
                {
                    if (!copied.TryGetValue(old, out int index))
                    {
                        index = positions.Count;
                        copied[old] = index;
                        positions.Add(source.Positions[old]);
                        normals.Add(source.Normals[old].sqrMagnitude > .1f ? source.Normals[old] : flat);
                        if (source.Colors != null) colors.Add(source.Colors[old]);
                        for (int c = 0; c < uv.Length; c++) uv[c]?.Add(source.UV[c][old]);
                    }
                    indices[tri.Material].Add(index);
                }
            }

            public void Chamfer(ObjectRemasterMeshData.Box box, Vector3 bevel)
            {
                Vector3 inset = box.Half - bevel;
                for (int axis = 0; axis < 3; axis++)
                for (int side = -1; side <= 1; side += 2)
                {
                    int b = (axis + 1) % 3, c = (axis + 2) % 3, face = axis * 2 + (side > 0 ? 1 : 0);
                    Vector3 n = box.Axes[axis] * side;
                    var p = new Vector3[4];
                    int[] bs = { -1, 1, 1, -1 }, cs = { -1, -1, 1, 1 };
                    for (int k = 0; k < 4; k++) { p[k][axis] = side * box.Half[axis]; p[k][b] = bs[k] * inset[b]; p[k][c] = cs[k] * inset[c]; }
                    Polygon(box, face, p, new[] { n, n, n, n });
                }
                for (int free = 0; free < 3; free++)
                for (int sb = -1; sb <= 1; sb += 2)
                for (int sc = -1; sc <= 1; sc += 2)
                {
                    int b = (free + 1) % 3, c = (free + 2) % 3;
                    var p = new Vector3[4];
                    p[0][free] = p[3][free] = -inset[free]; p[1][free] = p[2][free] = inset[free];
                    p[0][b] = p[1][b] = sb * box.Half[b]; p[0][c] = p[1][c] = sc * inset[c];
                    p[2][b] = p[3][b] = sb * inset[b]; p[2][c] = p[3][c] = sc * box.Half[c];
                    int fb = b * 2 + (sb > 0 ? 1 : 0), fc = c * 2 + (sc > 0 ? 1 : 0);
                    int face = FaceArea(box, b) >= FaceArea(box, c) ? fb : fc;
                    Vector3 nb = box.Axes[b] * sb, nc = box.Axes[c] * sc;
                    Polygon(box, face, p, new[] { nb, nb, nc, nc });
                }
                for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    var signs = new Vector3(sx, sy, sz);
                    var p = new Vector3[3];
                    var n = new Vector3[3];
                    int largest = FaceArea(box, 0) >= FaceArea(box, 1) ? 0 : 1;
                    if (FaceArea(box, 2) > FaceArea(box, largest)) largest = 2;
                    for (int axis = 0; axis < 3; axis++)
                    {
                        p[axis] = Vector3.Scale(inset, signs);
                        p[axis][axis] = signs[axis] * box.Half[axis];
                        n[axis] = box.Axes[axis] * signs[axis];
                    }
                    Polygon(box, largest * 2 + (signs[largest] > 0 ? 1 : 0), p, n);
                }
            }

            private static float FaceArea(ObjectRemasterMeshData.Box box, int axis) =>
                box.Half[(axis + 1) % 3] * box.Half[(axis + 2) % 3];

            private void Polygon(ObjectRemasterMeshData.Box box, int face, Vector3[] local, Vector3[] normal)
            {
                int start = positions.Count;
                foreach (int k in Enumerable.Range(0, local.Length))
                {
                    Vector3 p = box.Point(local[k]);
                    positions.Add(p); normals.Add(normal[k]);
                    var (triangle, bary) = SampleFace(box, face, local[k]);
                    if (source.Colors != null) colors.Add(source.Colors[triangle.A] * bary.x + source.Colors[triangle.B] * bary.y + source.Colors[triangle.C] * bary.z);
                    for (int c = 0; c < uv.Length; c++)
                        uv[c]?.Add(source.UV[c][triangle.A] * bary.x + source.UV[c][triangle.B] * bary.y + source.UV[c][triangle.C] * bary.z);
                }
                Vector3 expected = normal.Aggregate(Vector3.zero, (sum, n) => sum + n);
                bool reverse = Vector3.Dot(Vector3.Cross(positions[start + 1] - positions[start], positions[start + 2] - positions[start]), expected) < 0;
                int material = source.Triangles[box.Faces[face][0]].Material;
                for (int i = 1; i + 1 < local.Length; i++)
                    indices[material].AddRange(reverse ? new[] { start, start + i + 1, start + i } : new[] { start, start + i, start + i + 1 });
            }

            private (ObjectRemasterMeshData.Triangle triangle, Vector3 bary) SampleFace(ObjectRemasterMeshData.Box box, int face, Vector3 local)
            {
                local[face / 2] = (face % 2 == 0 ? -1 : 1) * box.Half[face / 2];
                Vector3 p = box.Point(local), lastBary = Vector3.zero;
                ObjectRemasterMeshData.Triangle last = default;
                foreach (int index in box.Faces[face])
                {
                    last = source.Triangles[index];
                    Vector3 a = source.Positions[last.A], v0 = source.Positions[last.B] - a, v1 = source.Positions[last.C] - a, v2 = p - a;
                    float d00 = Vector3.Dot(v0, v0), d01 = Vector3.Dot(v0, v1), d11 = Vector3.Dot(v1, v1);
                    float d20 = Vector3.Dot(v2, v0), d21 = Vector3.Dot(v2, v1), denominator = d00 * d11 - d01 * d01;
                    float v = (d11 * d20 - d01 * d21) / denominator, w = (d00 * d21 - d01 * d20) / denominator;
                    lastBary = new Vector3(1 - v - w, v, w);
                    if (Mathf.Min(lastBary.x, lastBary.y, lastBary.z) >= -.001f) return (last, lastBary);
                }
                return (last, lastBary);
            }

            public Mesh Finish()
            {
                var mesh = new Mesh { name = "Remastered prop", indexFormat = positions.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                mesh.SetVertices(positions); mesh.SetNormals(normals);
                if (source.Colors != null) mesh.SetColors(colors);
                for (int c = 0; c < uv.Length; c++)
                {
                    if (uv[c] == null) continue;
                    if (source.UvDimensions[c] <= 2) mesh.SetUVs(c, uv[c].Select(p => new Vector2(p.x, p.y)).ToList());
                    else if (source.UvDimensions[c] == 3) mesh.SetUVs(c, uv[c].Select(p => new Vector3(p.x, p.y, p.z)).ToList());
                    else mesh.SetUVs(c, uv[c]);
                }
                mesh.subMeshCount = indices.Length;
                for (int sub = 0; sub < indices.Length; sub++) mesh.SetTriangles(indices[sub], sub, false);
                mesh.RecalculateBounds();
                if (uv[0] != null) mesh.RecalculateTangents();
                return mesh;
            }
        }
    }
}
