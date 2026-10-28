using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Draws one face of a box with the relief its construction has: plain, trapezoid ribs (profiled cladding), a grid of
    /// V-grooves (formwork joints, stone or panel joints) or board grooves. Relief is cut into the face, never out of it,
    /// so nothing stands proud of the collider. Grooves and ribs are placed on world coordinates (phase), so the joints of
    /// two boxes side by side line up.
    /// </summary>
    public static class ArchRelief
    {
        public enum Kind { Plain, Ribs, Grid, VBoards, HBoards }

        public struct Face
        {
            public Vector3 Centre, U, V, N; // unit axes; N points out of the box
            public float HalfU, HalfV;
            public float PhaseU, PhaseV;    // world coordinate of Centre along U and V
        }

        public static void Draw(ArchMesh mesh, Face f, Kind kind, Material material, ArchStyle style, float maxDepth)
        {
            switch (kind)
            {
                case Kind.Ribs when f.HalfU > style.RibPitch && maxDepth > .02f:
                    Ribs(mesh, f, material, style.RibPitch, Mathf.Min(style.RibDepth, maxDepth));
                    return;
                case Kind.Grid:
                    Grid(mesh, f, material, style.ConcretePanel.x, style.ConcretePanel.y, .012f, Mathf.Min(.008f, maxDepth));
                    return;
                case Kind.VBoards:
                    Grid(mesh, f, material, style.BoardPitch, 0f, .007f, Mathf.Min(.004f, maxDepth));
                    return;
                case Kind.HBoards:
                    Grid(mesh, f, material, 0f, style.BoardPitch, .007f, Mathf.Min(.005f, maxDepth));
                    return;
                default:
                    mesh.Face(f.Centre, f.U * f.HalfU, f.V * f.HalfV, f.N, material);
                    return;
            }
        }

        /// <summary>Stone or panel joints at a given panel size.</summary>
        public static void Panels(ArchMesh mesh, Face f, Material material, Vector2 panel, float maxDepth) =>
            Grid(mesh, f, material, panel.x, panel.y, .008f, Mathf.Min(.006f, maxDepth));

        /// <summary>Positions in (-half, half) where (phase + u) is a multiple of the step, at least 4 cm from the edges.</summary>
        private static List<float> Lines(float half, float phase, float step)
        {
            var lines = new List<float>();
            if (step <= .01f) return lines;
            float first = Mathf.Ceil((phase - half) / step) * step - phase;
            for (float u = first; u < half; u += step)
                if (u > -half + .04f && u < half - .04f) lines.Add(u);
            return lines;
        }

        private static void Grid(ArchMesh mesh, Face f, Material material, float stepU, float stepV, float width, float depth)
        {
            List<float> us = Lines(f.HalfU, f.PhaseU, stepU), vs = Lines(f.HalfV, f.PhaseV, stepV);
            if (depth < .002f || us.Count + vs.Count == 0)
            {
                mesh.Face(f.Centre, f.U * f.HalfU, f.V * f.HalfV, f.N, material);
                return;
            }
            float g = width * .5f;
            // Panel edges along U and V: each groove takes [line - g, line + g] out of the face.
            List<Vector2> spansU = Spans(f.HalfU, us, g), spansV = Spans(f.HalfV, vs, g);
            foreach (Vector2 su in spansU)
            foreach (Vector2 sv in spansV)
                Rect(mesh, f, su, sv, 0f, f.N, material);
            // Vertical grooves (lines across U) run the full height; horizontal ones run between them.
            foreach (float u in us)
            {
                Rect(mesh, f, new Vector2(u - g, u + g), new Vector2(-f.HalfV, f.HalfV), -depth, f.N, material);
                Wall(mesh, f, u - g, -f.HalfV, f.HalfV, depth, f.U, material);
                Wall(mesh, f, u + g, -f.HalfV, f.HalfV, depth, -f.U, material);
            }
            foreach (float v in vs)
            foreach (Vector2 su in spansU)
            {
                Rect(mesh, f, su, new Vector2(v - g, v + g), -depth, f.N, material);
                var flip = new Face { Centre = f.Centre, U = f.V, V = f.U, N = f.N, HalfU = f.HalfV, HalfV = f.HalfU };
                Wall(mesh, flip, v - g, su.x, su.y, depth, f.V, material);
                Wall(mesh, flip, v + g, su.x, su.y, depth, -f.V, material);
            }
        }

        private static List<Vector2> Spans(float half, List<float> lines, float g)
        {
            var spans = new List<Vector2>();
            float at = -half;
            foreach (float l in lines)
            {
                spans.Add(new Vector2(at, l - g));
                at = l + g;
            }
            spans.Add(new Vector2(at, half));
            return spans;
        }

        /// <summary>A rectangle on the face plane (offset along N by <paramref name="offset"/>).</summary>
        private static void Rect(ArchMesh mesh, Face f, Vector2 u, Vector2 v, float offset, Vector3 normal, Material material)
        {
            Vector3 c = f.Centre + f.U * ((u.x + u.y) * .5f) + f.V * ((v.x + v.y) * .5f) + f.N * offset;
            mesh.Face(c, f.U * ((u.y - u.x) * .5f), f.V * ((v.y - v.x) * .5f), normal, material);
        }

        /// <summary>A groove's side: the strip at u from the face down to the groove bottom, along V from v0 to v1.</summary>
        private static void Wall(ArchMesh mesh, Face f, float u, float v0, float v1, float depth, Vector3 normal, Material material)
        {
            Vector3 c = f.Centre + f.U * u + f.V * ((v0 + v1) * .5f) - f.N * (depth * .5f);
            mesh.Face(c, f.V * ((v1 - v0) * .5f), f.N * (depth * .5f), normal, material);
        }

        /// <summary>Trapezoid ribs running along V, the valleys cut <paramref name="depth"/> into the face.</summary>
        private static void Ribs(ArchMesh mesh, Face f, Material material, float pitch, float depth)
        {
            // One pitch, from a valley's start: valley 45 %, slope 10 %, crest 35 %, slope 10 %.
            float[] at = { 0f, .45f, .55f, .9f, 1f };
            float[] z = { -depth, -depth, 0f, 0f, -depth };
            var profile = new List<Vector2>();
            float start = Mathf.Floor((f.PhaseU - f.HalfU) / pitch) * pitch - f.PhaseU;
            for (float p = start; p < f.HalfU; p += pitch)
                for (int i = 0; i < 4; i++)
                {
                    float u0 = p + at[i] * pitch, u1 = p + at[i + 1] * pitch;
                    float a = Mathf.Max(u0, -f.HalfU), b = Mathf.Min(u1, f.HalfU);
                    if (b <= a) continue;
                    float za = Mathf.Lerp(z[i], z[i + 1], (a - u0) / (u1 - u0)), zb = Mathf.Lerp(z[i], z[i + 1], (b - u0) / (u1 - u0));
                    profile.Add(new Vector2(a, za));
                    profile.Add(new Vector2(b, zb));
                }
            for (int i = 0; i + 1 < profile.Count; i += 2)
            {
                Vector2 a = profile[i], b = profile[i + 1];
                Vector3 pa = f.Centre + f.U * a.x + f.N * a.y, pb = f.Centre + f.U * b.x + f.N * b.y;
                Vector3 edge = pb - pa;
                Vector3 normal = Vector3.Cross(f.V, edge).normalized;
                if (Vector3.Dot(normal, f.N) < 0f) normal = -normal;
                Vector3 lo = -f.V * f.HalfV, hi = f.V * f.HalfV;
                mesh.Quad(pa + lo, pa + hi, pb + hi, pb + lo, normal, f.V, edge.normalized, material);
                // Close the rib's ends (the region between the profile and the valley plane).
                if (a.y > -depth + 1e-4f || b.y > -depth + 1e-4f)
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Vector3 off = f.V * (f.HalfV * side), down = -f.N * depth;
                        Vector3 qa = f.Centre + f.U * a.x + off, qb = f.Centre + f.U * b.x + off;
                        mesh.Quad(pa + off, pb + off, qb + down, qa + down, f.V * side, f.U, f.N, material);
                    }
            }
        }
    }
}
