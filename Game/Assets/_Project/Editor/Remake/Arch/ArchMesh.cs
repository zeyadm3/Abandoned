using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// One architecture drawing in metres, in the box's own axes (origin at the box centre): flat-shaded quads and
    /// extruded profiles, one submesh per material. UVs are metres along each face (the map surfaces are triplanar in
    /// object metres, so they ignore UVs; the trims' baked sets tile once per metre).
    /// </summary>
    public sealed class ArchMesh
    {
        private readonly List<Vector3> vertices = new();
        private readonly List<Vector3> normals = new();
        private readonly List<Vector2> uvs = new();
        private readonly List<Material> slots = new();
        private readonly List<List<int>> triangles = new();

        public int Quads { get; private set; }

        private List<int> Slot(Material material)
        {
            int i = slots.IndexOf(material);
            if (i >= 0) return triangles[i];
            slots.Add(material);
            triangles.Add(new List<int>());
            return triangles[^1];
        }

        /// <summary>A rectangle centred on <paramref name="centre"/>, spanning ±<paramref name="u"/> and ±<paramref name="v"/>
        /// (half extents as vectors), facing <paramref name="normal"/>.</summary>
        public void Face(Vector3 centre, Vector3 u, Vector3 v, Vector3 normal, Material material)
        {
            if (u.sqrMagnitude < 1e-10f || v.sqrMagnitude < 1e-10f || material == null) return;
            if (Vector3.Dot(Vector3.Cross(v, u), normal) < 0f) u = -u;
            Vector3 du = u.normalized, dv = v.normalized;
            Quad(centre - u - v, centre - u + v, centre + u + v, centre + u - v, normal, du, dv, material);
        }

        /// <summary>Four corners in order around the quad; it is wound to face <paramref name="normal"/>.</summary>
        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Vector3 du, Vector3 dv, Material material)
        {
            if (material == null) return;
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) < 0f) (b, d) = (d, b);
            List<int> tris = Slot(material);
            int start = vertices.Count;
            foreach (Vector3 p in new[] { a, b, c, d })
            {
                vertices.Add(p);
                normals.Add(normal.normalized);
                uvs.Add(new Vector2(Vector3.Dot(p, du), Vector3.Dot(p, dv)));
            }
            tris.Add(start); tris.Add(start + 1); tris.Add(start + 2);
            tris.Add(start); tris.Add(start + 2); tris.Add(start + 3);
            Quads++;
        }

        /// <summary>An axis-aligned (in the drawing's axes) block; <paramref name="skip"/> leaves faces out
        /// (bit order -x, +x, -y, +y, -z, +z).</summary>
        public void Block(Vector3 centre, Vector3 size, Material material, int skip = 0)
        {
            Vector3 h = size * .5f;
            if ((skip & 1) == 0) Face(centre - Vector3.right * h.x, Vector3.forward * h.z, Vector3.up * h.y, Vector3.left, material);
            if ((skip & 2) == 0) Face(centre + Vector3.right * h.x, Vector3.forward * h.z, Vector3.up * h.y, Vector3.right, material);
            if ((skip & 4) == 0) Face(centre - Vector3.up * h.y, Vector3.right * h.x, Vector3.forward * h.z, Vector3.down, material);
            if ((skip & 8) == 0) Face(centre + Vector3.up * h.y, Vector3.right * h.x, Vector3.forward * h.z, Vector3.up, material);
            if ((skip & 16) == 0) Face(centre - Vector3.forward * h.z, Vector3.right * h.x, Vector3.up * h.y, Vector3.back, material);
            if ((skip & 32) == 0) Face(centre + Vector3.forward * h.z, Vector3.right * h.x, Vector3.up * h.y, Vector3.forward, material);
        }

        /// <summary>
        /// A profile swept along a straight run. Profile points are (out, up) in metres in the plane across the run:
        /// <paramref name="outward"/> is the profile's +x, <paramref name="up"/> its +y. The profile is an open polyline
        /// that starts and ends on the wall (x = 0), going away from it first, so the solid is always on the left of the
        /// direction of travel (a skirting: (0,0), (t,0), (t,h), (0,h)). Ends are capped when asked.
        /// </summary>
        public void Sweep(Vector2[] profile, Vector3 start, Vector3 end, Vector3 outward, Vector3 up, Material material,
            bool capStart = true, bool capEnd = true)
        {
            Vector3 run = end - start;
            if (run.sqrMagnitude < 1e-6f || profile.Length < 2 || material == null) return;
            Vector3 along = run.normalized;
            for (int i = 0; i + 1 < profile.Length; i++)
            {
                Vector2 e = profile[i + 1] - profile[i];
                if (e.sqrMagnitude < 1e-10f) continue;
                Vector3 p0 = outward * profile[i].x + up * profile[i].y;
                Vector3 p1 = outward * profile[i + 1].x + up * profile[i + 1].y;
                // Solid on the left of travel, so the outside is on the right: (dy, -dx).
                Vector3 normal = (outward * e.y - up * e.x).normalized;
                Vector3 a = start + p0, b = start + p1, c = end + p1, d = end + p0;
                if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) < 0f) (b, d) = (d, b);
                Quad(a, b, c, d, normal, along, (p1 - p0).normalized, material);
            }
            if (capStart) Cap(profile, start, outward, up, -along, material);
            if (capEnd) Cap(profile, end, outward, up, along, material);
        }

        /// <summary>Closes a profile's end as a fan from its foot on the wall (profiles start and end on the wall plane).</summary>
        private void Cap(Vector2[] profile, Vector3 at, Vector3 outward, Vector3 up, Vector3 normal, Material material)
        {
            List<int> tris = Slot(material);
            int start = vertices.Count;
            Vector3 du = outward, dv = up;
            foreach (Vector2 p in profile)
            {
                Vector3 q = at + outward * p.x + up * p.y;
                vertices.Add(q);
                normals.Add(normal);
                uvs.Add(new Vector2(Vector3.Dot(q, du), Vector3.Dot(q, dv)));
            }
            for (int i = 1; i + 1 < profile.Length; i++)
            {
                int a = start, b = start + i, c = start + i + 1;
                Vector3 n = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
                if (n.sqrMagnitude < 1e-12f) continue;
                if (Vector3.Dot(n, normal) < 0f) (b, c) = (c, b);
                tris.Add(a); tris.Add(b); tris.Add(c);
            }
        }

        /// <summary>A flat convex polygon (a wall's top in plan), fanned from its first corner.</summary>
        public void Polygon(IReadOnlyList<Vector3> points, Vector3 normal, Material material)
        {
            if (points.Count < 3 || material == null) return;
            List<int> tris = Slot(material);
            int start = vertices.Count;
            Vector3 du = (points[1] - points[0]).normalized, dv = Vector3.Cross(normal, du);
            foreach (Vector3 p in points)
            {
                vertices.Add(p);
                normals.Add(normal);
                uvs.Add(new Vector2(Vector3.Dot(p, du), Vector3.Dot(p, dv)));
            }
            for (int i = 1; i + 1 < points.Count; i++)
            {
                int a = start, b = start + i, c = start + i + 1;
                Vector3 n = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
                if (n.sqrMagnitude < 1e-12f) continue;
                if (Vector3.Dot(n, normal) < 0f) (b, c) = (c, b);
                tris.Add(a); tris.Add(b); tris.Add(c);
            }
            Quads++;
        }

        public bool Empty => vertices.Count == 0;

        /// <summary>The mesh and the material of each submesh, in order.</summary>
        public Mesh Build(string name, out Material[] materials)
        {
            var mesh = new Mesh { name = name };
            if (vertices.Count > 65000) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = slots.Count;
            for (int i = 0; i < slots.Count; i++) mesh.SetTriangles(triangles[i], i);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            materials = slots.ToArray();
            return mesh;
        }

        /// <summary>Puts <paramref name="first"/> in submesh 0 if it is used (collapse debris takes the first renderer's first
        /// material, which should stay the box's own surface).</summary>
        public void Lead(Material first)
        {
            int i = slots.IndexOf(first);
            if (i <= 0) return;
            (slots[0], slots[i]) = (slots[i], slots[0]);
            (triangles[0], triangles[i]) = (triangles[i], triangles[0]);
        }
    }
}
