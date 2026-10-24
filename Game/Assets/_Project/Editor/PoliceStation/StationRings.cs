using System.Collections.Generic;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using Plan = Abandoned.EditorTools.StationTownPlan;
using Site = Abandoned.EditorTools.StationSite;
using Street = Abandoned.EditorTools.StationStreet;
using Way = Abandoned.EditorTools.StationRingSpec.Way;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Two more rings of town round a map's neighbourhood, from the edge of its ground outward: a ring road with its
    /// sidewalks, a block of buildings back to back (one row facing the town, one facing out), a second ring road, and an
    /// outer row facing in with yards behind. The town's own streets run on straight through both rings to the new edge,
    /// with stop signs and crossings where they meet the ring roads. Ground, roads and sidewalks are slabs (the open
    /// world reads them as the site's ground); buildings and cars are copies from a <see cref="StationRingStock"/>. The
    /// first ring road's lamps still light; the second ring is dark. Exterior only, off the NavMesh.
    /// </summary>
    public static partial class StationRings
    {
        public const string GroupName = "OuterRings";
        public const float Walk = 4f, Block = 50f, Edge = 36f, RowDepth = 23f, EdgeDepth = 30f;

        private static readonly string[] SideName = { "S", "N", "W", "E" };

        /// <summary>How far the rings reach out past the town's edge, for ring roads <paramref name="road"/> wide.</summary>
        public static float Reach(float road) => Walk * 4f + road * 2f + Block + Edge;

        public static Transform Build(Transform root, Transform ext, StationArt a, StationRingSpec spec, StationRingStock stock)
        {
            var l = new Layout(spec.Inner, spec.Road);
            Transform rings = Group(GroupName, ext);
            Transform marks = Group("RingMarkings", rings), blocks = Group("RingBlocks", rings), street = Group("RingFurniture", rings);
            var rng = new System.Random(spec.Seed);
            (GameObject prefab, float height)[] trees = Trees(spec.Winter);
            for (int side = 0; side < 4; side++)
            {
                Ground(root, marks, a, spec, l, side);
                Crossings(street, marks, a, spec, l, side);
            }
            for (int side = 0; side < 4; side++) Rows(blocks, street, a, spec, stock, l, side, rng, trees);
            for (int side = 0; side < 4; side++) Dressing(street, a, spec, stock, l, side, rng, trees);
            return rings;
        }

        /// <summary>After the exterior is marked static: the copies draw on their own (static batching would copy every
        /// mesh once per building) and only cull, never hide others (the occlusion bake stays on the town's masses).</summary>
        public static int Settle(Transform ext)
        {
            Transform rings = ext.Find(GroupName);
            if (rings == null) return 0;
            int n = 0;
            foreach (Renderer r in rings.GetComponentsInChildren<Renderer>(true))
            {
                if (r.name != StationRingStock.NearName && r.name != StationRingStock.FarName && r.name != StationRingStock.DetailName) continue;
                StaticEditorFlags f = GameObjectUtility.GetStaticEditorFlags(r.gameObject);
                GameObjectUtility.SetStaticEditorFlags(r.gameObject, f & ~(StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic));
                n++;
            }
            return n;
        }

        // Ring roads, their sidewalks, the town's streets out through them, and the blocks' ground. Nothing overlaps.
        private static void Ground(Transform root, Transform marks, StationArt a, StationRingSpec spec, Layout l, int side)
        {
            bool x = Layout.AlongX(side);
            List<Way> ways = spec.Ways(side), town = ways.FindAll(entry => !entry.Spur);
            string n = SideName[side];
            foreach ((float o0, float o1, string ring) in new[] { (l.Ar0, l.Ar1, "RingRoadA"), (l.Br0, l.Br1, "RingRoadB") })
            {
                Vector2 s = l.Span(side, o0, o1);
                Street.Road(root, marks, a, $"{ring}_{n}", l.R(side, s.x, s.y, o0, o1), x);
            }
            float outward = Layout.Out(side);
            foreach ((float o0, float o1, float kerb, string w, List<Way> cut) in new[]
                     { (0f, l.Ar0, outward, "AIn", town), (l.Ar1, l.A1, -outward, "AOut", ways), (l.B0, l.Br0, outward, "BIn", ways), (l.Br1, l.B1, -outward, "BOut", ways) })
                foreach (Vector2 p in Cut(l.Span(side, o0, o1), cut, 0f))
                    Street.Sidewalk(root, marks, a, $"RingWalk{w}_{n}_{p.x:0}", l.R(side, p.x, p.y, o0, o1), x, kerb, 3.2f);
            foreach (Way way in ways)
            {
                foreach ((float o0, float o1) in new[] { (0f, l.Ar0), (l.Ar1, l.Br0), (l.Br1, l.T) })
                    if (o0 > 0f || !way.Spur)
                        Street.Road(root, marks, a, $"OutRoad_{n}_{way.From:0}_{o0:0}", l.R(side, way.From, way.To, o0, o1), !x);
                foreach ((float o0, float o1) in new[] { (l.A1, l.B0), (l.B1, l.T) })
                {
                    Street.Sidewalk(root, marks, a, $"OutWalk_{n}_{way.From:0}_L{o0:0}", l.R(side, way.From - Walk, way.From, o0, o1), !x, 1f, 3.2f);
                    Street.Sidewalk(root, marks, a, $"OutWalk_{n}_{way.From:0}_R{o0:0}", l.R(side, way.To, way.To + Walk, o0, o1), !x, -1f, 3.2f);
                }
            }
            foreach ((float o0, float o1, Material m, SurfaceMaterial s, string w) in new[]
                     { (l.A1, l.B0, spec.Lot, spec.LotSurface, "Block"), (l.B1, l.T, spec.Yard, spec.YardSurface, "Edge") })
                foreach (Vector2 p in Cut(l.Span(side, o0, o1), ways, Walk))
                    Plan.Ground(root, $"Ring{w}_{n}_{p.x:0}", l.R(side, p.x, p.y, o0, o1), m, s);
        }

        // Zebras where the town's streets cross the ring roads, stop signs on each approach, a blank name blade.
        private static void Crossings(Transform s, Transform marks, StationArt a, StationRingSpec spec, Layout l, int side)
        {
            bool x = Layout.AlongX(side);
            float inward = Layout.Inward(side);
            foreach (Way way in spec.Ways(side))
            {
                float mid = (way.From + way.To) * .5f, width = way.To - way.From;
                foreach ((float r0, float r1, bool first) in new[] { (l.Ar0, l.Ar1, true), (l.Br0, l.Br1, false) })
                {
                    Street.Crosswalk(marks, a, l.P(side, mid, r1 + 2.5f), !x, width);
                    if (!first || !way.Spur) Street.Crosswalk(marks, a, l.P(side, mid, r0 - 2.5f), !x, width);
                    float ring = (r0 + r1) * .5f;
                    Street.Crosswalk(marks, a, l.P(side, way.From - 2.5f, ring), x, l.W);
                    Street.Crosswalk(marks, a, l.P(side, way.To + 2.5f, ring), x, l.W);
                }
                Street.StopSign(s, a, l.P(side, way.From - .6f, l.A1 + .6f), inward + 180f);
                if (!way.Spur) Street.StopSign(s, a, l.P(side, way.To + .6f, l.Ar0 - .6f), inward);
                Street.StopSign(s, a, l.P(side, way.From - .6f, l.B1 + .6f), inward + 180f);
                Street.StopSign(s, a, l.P(side, way.To + .6f, l.B0 - .6f), inward);
                Street.StreetSign(s, a, l.P(side, way.To + .6f, l.A1 + .6f), 0f);
            }
        }

        // A span with the ways cut out of it (each widened by pad), slivers dropped.
        private static List<Vector2> Cut(Vector2 span, List<Way> ways, float pad)
        {
            var parts = new List<Vector2> { span };
            foreach (Way w in ways)
            {
                float h0 = w.From - pad, h1 = w.To + pad;
                var next = new List<Vector2>();
                foreach (Vector2 p in parts)
                {
                    if (h1 <= p.x || h0 >= p.y) { next.Add(p); continue; }
                    if (h0 > p.x) next.Add(new Vector2(p.x, h0));
                    if (h1 < p.y) next.Add(new Vector2(h1, p.y));
                }
                parts = next;
            }
            parts.RemoveAll(p => p.y - p.x < .05f);
            return parts;
        }

        private static bool Clear(float u, List<Way> ways, float pad)
        {
            foreach (Way w in ways)
                if (u > w.From - pad && u < w.To + pad) return false;
            return true;
        }

        /// <summary>
        /// Offsets out from the town's edge (o) and positions along each side (u): world x on the south and north sides,
        /// world z on the west and east. The south and north pieces of every ring take its corners.
        /// </summary>
        private sealed class Layout
        {
            public readonly Rect In;
            public readonly float W, Ar0, Ar1, A1, B0, Br0, Br1, B1, T;

            public Layout(Rect inner, float road)
            {
                In = inner;
                W = road;
                Ar0 = Walk;
                Ar1 = Ar0 + road;
                A1 = Ar1 + Walk;
                B0 = A1 + Block;
                Br0 = B0 + Walk;
                Br1 = Br0 + road;
                B1 = Br1 + Walk;
                T = B1 + Edge;
            }

            public static bool AlongX(int side) => side < 2;

            /// <summary>Sign of the outward direction on the axis across the side (z for south and north, x for west and east).</summary>
            public static float Out(int side) => side == 0 || side == 2 ? -1f : 1f;

            /// <summary>The yaw a front facing back toward the town has on this side.</summary>
            public static float Inward(int side) => side switch { 0 => 0f, 1 => 180f, 2 => 90f, _ => -90f };

            public static Vector3 Along(int side) => side < 2 ? Vector3.right : Vector3.forward;

            public Rect R(int side, float u0, float u1, float o0, float o1) => side switch
            {
                0 => Rect.MinMaxRect(u0, In.yMin - o1, u1, In.yMin - o0),
                1 => Rect.MinMaxRect(u0, In.yMax + o0, u1, In.yMax + o1),
                2 => Rect.MinMaxRect(In.xMin - o1, u0, In.xMin - o0, u1),
                _ => Rect.MinMaxRect(In.xMax + o0, u0, In.xMax + o1, u1),
            };

            public Vector3 P(int side, float u, float o) => side switch
            {
                0 => new Vector3(u, Site.GroundTop, In.yMin - o),
                1 => new Vector3(u, Site.GroundTop, In.yMax + o),
                2 => new Vector3(In.xMin - o, Site.GroundTop, u),
                _ => new Vector3(In.xMax + o, Site.GroundTop, u),
            };

            /// <summary>Along-range of a ring layer [o0, o1] on a side.</summary>
            public Vector2 Span(int side, float o0, float o1) =>
                side < 2 ? new Vector2(In.xMin - o1, In.xMax + o1) : new Vector2(In.yMin - o0, In.yMax + o0);
        }
    }
}
