using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>Everything one box's drawing needs: the box, its map's style and finishes, and questions about the space
    /// around it asked in the box's own (local, metric) coordinates.</summary>
    public sealed class ArchContext
    {
        public ArchBox Box;
        public ArchStyle Style;
        public ArchSpace Space;
        public Func<string, Material> Finishes;
        public ArchMesh Mesh;

        public Material Own => Box.Material;
        public Material Finish(string key) => (key != null ? Finishes(key) : null) ?? Box.Material;

        public Vector3 World(Vector3 local) => Box.ToWorld(local);
        public bool Solid(Vector3 local) => Space.Solid(World(local), Box);

        /// <summary>Open stretches of the local segment a→b, as (t0, t1) in 0..1.</summary>
        public List<Vector2> Open(Vector3 a, Vector3 b) => Space.Open(World(a), World(b), Box);

        /// <summary>Stretches of a→b with something solid along them (the floor under a wall, the ceiling over it).</summary>
        public List<Vector2> Covered(Vector3 a, Vector3 b) => Invert(Open(a, b));

        /// <summary>Nothing overhead for 80 m: outside.</summary>
        public bool Outside(Vector3 local) => !Space.Hit(World(local), Vector3.up, 80f, Box);

        /// <summary>World coordinate of a local point along a local axis (for joints that line up across boxes).</summary>
        public float Phase(Vector3 local, Vector3 axis) => Vector3.Dot(World(local), Box.DirToWorld(axis));

        public static List<Vector2> Invert(List<Vector2> open)
        {
            var result = new List<Vector2>();
            float at = 0f;
            foreach (Vector2 o in open)
            {
                if (o.x > at + 1e-4f) result.Add(new Vector2(at, o.x));
                at = Mathf.Max(at, o.y);
            }
            if (at < 1f - 1e-4f) result.Add(new Vector2(at, 1f));
            return result;
        }

        public static List<Vector2> Intersect(List<Vector2> a, List<Vector2> b)
        {
            var result = new List<Vector2>();
            int i = 0, j = 0;
            while (i < a.Count && j < b.Count)
            {
                float lo = Mathf.Max(a[i].x, b[j].x), hi = Mathf.Min(a[i].y, b[j].y);
                if (hi > lo + 1e-4f) result.Add(new Vector2(lo, hi));
                if (a[i].y < b[j].y) i++;
                else j++;
            }
            return result;
        }

        public static float Share(List<Vector2> spans)
        {
            float s = 0f;
            foreach (Vector2 v in spans) s += v.y - v.x;
            return s;
        }

        /// <summary>Drops stretches shorter than <paramref name="metres"/> on a run of the given length.</summary>
        public static List<Vector2> AtLeast(List<Vector2> spans, float length, float metres)
        {
            spans.RemoveAll(s => (s.y - s.x) * length < metres);
            return spans;
        }
    }
}
