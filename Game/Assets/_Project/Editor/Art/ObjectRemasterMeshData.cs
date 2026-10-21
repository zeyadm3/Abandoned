using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Read-only extraction and strict box recognition, including rotated parts in combined FBX meshes.</summary>
    public sealed class ObjectRemasterMeshData
    {
        public struct Triangle
        {
            public int A, B, C, Material;
            public Triangle(int a, int b, int c, int material) { A = a; B = b; C = c; Material = material; }
        }

        public sealed class Box
        {
            public Vector3 Centre, Half;
            public Vector3[] Axes;
            public readonly List<int>[] Faces = Enumerable.Range(0, 6).Select(_ => new List<int>()).ToArray();
            public readonly List<int> Triangles = new();
            public Vector3 Point(Vector3 local) => Centre + Axes[0] * local.x + Axes[1] * local.y + Axes[2] * local.z;
        }

        public Vector3[] Positions, Normals;
        public Color[] Colors;
        public readonly Vector4[][] UV = new Vector4[8][];
        public readonly int[] UvDimensions = new int[8];
        public readonly List<Triangle> Triangles = new();
        public int MaterialCount;

        public static ObjectRemasterMeshData Read(Mesh mesh)
        {
            // The editor variant can inspect non-readable FBX meshes without changing the importer or retaining a
            // second CPU copy of every imported prop in the player.
            using Mesh.MeshDataArray access = MeshUtility.AcquireReadOnlyMeshData(mesh);
            Mesh.MeshData data = access[0];
            int count = data.vertexCount;
            var result = new ObjectRemasterMeshData { MaterialCount = data.subMeshCount };
            using (var into = new NativeArray<Vector3>(count, Allocator.Temp))
            {
                data.GetVertices(into);
                result.Positions = into.ToArray();
                if (data.HasVertexAttribute(VertexAttribute.Normal))
                { data.GetNormals(into); result.Normals = into.ToArray(); }
                else result.Normals = new Vector3[count];
            }
            if (data.HasVertexAttribute(VertexAttribute.Color))
            {
                using var into = new NativeArray<Color>(count, Allocator.Temp);
                data.GetColors(into);
                result.Colors = into.ToArray();
            }
            for (int channel = 0; channel < 8; channel++)
            {
                var attribute = (VertexAttribute)((int)VertexAttribute.TexCoord0 + channel);
                if (!data.HasVertexAttribute(attribute)) continue;
                result.UvDimensions[channel] = data.GetVertexAttributeDimension(attribute);
                using var into = new NativeArray<Vector4>(count, Allocator.Temp);
                data.GetUVs(channel, into);
                result.UV[channel] = into.ToArray();
            }
            for (int sub = 0; sub < data.subMeshCount; sub++)
            {
                SubMeshDescriptor descriptor = data.GetSubMesh(sub);
                if (descriptor.topology != MeshTopology.Triangles) return null;
                using var indices = new NativeArray<int>(descriptor.indexCount, Allocator.Temp);
                data.GetIndices(indices, sub, true);
                for (int i = 0; i + 2 < indices.Length; i += 3)
                    result.Triangles.Add(new Triangle(indices[i], indices[i + 1], indices[i + 2], sub));
            }
            return result;
        }

        public List<Box> Boxes(bool[] opaque)
        {
            int[] parent = Enumerable.Range(0, Positions.Length).ToArray();
            int Root(int n) { while (parent[n] != n) { parent[n] = parent[parent[n]]; n = parent[n]; } return n; }
            void Join(int a, int b) { a = Root(a); b = Root(b); if (a != b) parent[b] = a; }
            var welded = new Dictionary<(long, long, long), int>();
            // Source coordinates can be in centimetres; relative tolerance stays well below any authored gap.
            float reach = 0;
            foreach (Vector3 p in Positions) reach = Mathf.Max(reach, p.magnitude);
            double quantum = Math.Max(1e-6, reach * 1e-7);
            for (int i = 0; i < Positions.Length; i++)
            {
                Vector3 p = Positions[i];
                var key = ((long)Math.Round(p.x / quantum), (long)Math.Round(p.y / quantum), (long)Math.Round(p.z / quantum));
                if (welded.TryGetValue(key, out int same)) Join(i, same);
                else welded[key] = i;
            }
            foreach (Triangle tri in Triangles) { Join(tri.A, tri.B); Join(tri.B, tri.C); }
            var islands = new Dictionary<int, List<int>>();
            for (int i = 0; i < Triangles.Count; i++)
            {
                int root = Root(Triangles[i].A);
                if (!islands.TryGetValue(root, out var list)) islands[root] = list = new List<int>();
                list.Add(i);
            }
            var boxes = new List<Box>();
            foreach (List<int> island in islands.Values)
            {
                // A watertight six-face box has twelve triangles. More elaborate geometry is kept verbatim.
                if (island.Count != 12 || island.Any(i => Triangles[i].Material >= opaque.Length || !opaque[Triangles[i].Material])) continue;
                Box box = Recognize(island, (float)quantum * 8f);
                if (box != null) boxes.Add(box);
            }
            return boxes;
        }

        private Box Recognize(List<int> island, float tolerance)
        {
            Triangle first = Triangles[island[0]];
            Vector3 a = Positions[first.A], b = Positions[first.B], c = Positions[first.C];
            Vector3[] edges = { b - a, c - b, a - c };
            Vector3 x = edges.OrderBy(e => e.sqrMagnitude).First().normalized;
            Vector3 z = Vector3.Cross(b - a, c - a).normalized;
            if (x.sqrMagnitude < .9f || z.sqrMagnitude < .9f) return null;
            Vector3 y = Vector3.Cross(z, x).normalized;
            var axes = new[] { x, y, z };
            Vector3 Project(Vector3 p) => new(Vector3.Dot(p, x), Vector3.Dot(p, y), Vector3.Dot(p, z));
            Vector3 min = Vector3.one * float.MaxValue, max = Vector3.one * float.MinValue;
            var indices = island.SelectMany(i => new[] { Triangles[i].A, Triangles[i].B, Triangles[i].C }).Distinct().ToArray();
            foreach (int i in indices) { Vector3 p = Project(Positions[i]); min = Vector3.Min(min, p); max = Vector3.Max(max, p); }
            Vector3 size = max - min;
            if (Mathf.Min(size.x, size.y, size.z) <= tolerance) return null;
            var box = new Box { Axes = axes, Half = size * .5f, Centre = x * (min.x + max.x) * .5f + y * (min.y + max.y) * .5f + z * (min.z + max.z) * .5f };
            var corners = new HashSet<int>();
            var cornerIds = new Dictionary<int, int>();
            foreach (int index in indices)
            {
                Vector3 p = Project(Positions[index]);
                int corner = 0;
                for (int axis = 0; axis < 3; axis++)
                {
                    if (Mathf.Abs(p[axis] - min[axis]) <= tolerance) continue;
                    if (Mathf.Abs(p[axis] - max[axis]) > tolerance) return null;
                    corner |= 1 << axis;
                }
                corners.Add(corner);
                cornerIds[index] = corner;
            }
            if (corners.Count != 8) return null;
            foreach (int index in island)
            {
                Triangle triangle = Triangles[index];
                Vector3 p = Project(Positions[triangle.A]), q = Project(Positions[triangle.B]), r = Project(Positions[triangle.C]);
                int face = -1;
                for (int axis = 0; axis < 3; axis++)
                {
                    if (Mathf.Abs(p[axis] - q[axis]) > tolerance || Mathf.Abs(p[axis] - r[axis]) > tolerance) continue;
                    if (Mathf.Abs(p[axis] - min[axis]) <= tolerance) face = axis * 2;
                    else if (Mathf.Abs(p[axis] - max[axis]) <= tolerance) face = axis * 2 + 1;
                }
                if (face < 0) return null;
                box.Faces[face].Add(index);
            }
            foreach (List<int> face in box.Faces)
            {
                if (face.Count != 2 || Triangles[face[0]].Material != Triangles[face[1]].Material) return null;
                Triangle firstFace = Triangles[face[0]], secondFace = Triangles[face[1]];
                var firstCorners = new HashSet<int> { cornerIds[firstFace.A], cornerIds[firstFace.B], cornerIds[firstFace.C] };
                var secondCorners = new HashSet<int> { cornerIds[secondFace.A], cornerIds[secondFace.B], cornerIds[secondFace.C] };
                if (firstCorners.Count != 3 || secondCorners.Count != 3 || firstCorners.Union(secondCorners).Count() != 4) return null;
                int[] shared = firstCorners.Intersect(secondCorners).ToArray();
                if (shared.Length != 2) return null;
                // The shared edge must be the face diagonal. A perimeter edge indicates overlapping triangles
                // and a missing section of the source surface, which must never be repaired speculatively.
                int diagonal = shared[0] ^ shared[1];
                if (diagonal != 3 && diagonal != 5 && diagonal != 6) return null;
            }
            box.Triangles.AddRange(island);
            return box;
        }
    }
}
