using UnityEngine;
using Surface = Abandoned.EditorTools.Remake.ArchStyle.Surface;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Horizontal construction: floor slabs (the walking surface stays exactly where it was; exposed top edges are eased
    /// and, where there is a drop, get a nosing), their undersides (a soffit finish instead of the floor's), suspended
    /// ceilings (a T-bar grid on world lines), stair treads (an anti-slip nosing on the front) and pitched roof sheets
    /// (profiled ribs down the slope).
    /// </summary>
    public static class ArchSlab
    {
        public static void Build(ArchContext c)
        {
            ArchBox b = c.Box;
            ArchMesh m = c.Mesh;
            Vector3 h = b.Half;
            bool ceiling = b.Surface == Surface.Ceiling;
            bool step = b.Kind == ArchBox.Role.Step;

            // Which side faces are exposed (something to see, not buried in a neighbour).
            var open = new bool[4]; // -x, +x, -z, +z
            var drop = new bool[4];
            Vector3[] n = { Vector3.left, Vector3.right, Vector3.back, Vector3.forward };
            for (int i = 0; i < 4; i++)
            {
                Vector3 axis = n[i];
                Vector3 across = i < 2 ? Vector3.forward : Vector3.right;
                float half = i < 2 ? h.z : h.x, reach = i < 2 ? h.x : h.z;
                Vector3 edge = axis * (reach + .03f) + Vector3.up * (h.y - Mathf.Min(.03f, h.y * .5f));
                open[i] = ArchContext.Share(c.Open(edge - across * half, edge + across * half)) > .5f;
                Vector3 below = axis * (reach + .12f) + Vector3.down * (h.y + 1.1f);
                drop[i] = open[i] && !c.Solid(below) && !c.Solid(below + across * half * .6f) && !c.Solid(below - across * half * .6f);
            }
            // A face is left out only when it is buried all over (a wall standing on a tile's middle hides only a strip).
            int underFree = Free(c, h, -(h.y + .05f)), topFree = Free(c, h, h.y + .03f);
            bool underOpen = underFree > 0, topOpen = topFree > 0;

            float ease = ceiling || h.y < .03f ? 0f : Mathf.Min(.008f, h.y * .4f);
            var cut = new float[4];
            for (int i = 0; i < 4; i++) cut[i] = open[i] && topOpen ? ease : 0f;

            // Top: inset on eased edges, the walking plane unchanged.
            Vector3 top = Vector3.up * h.y;
            var topPts = new[]
            {
                top + new Vector3(-h.x + cut[0], 0f, -h.z + cut[2]), top + new Vector3(h.x - cut[1], 0f, -h.z + cut[2]),
                top + new Vector3(h.x - cut[1], 0f, h.z - cut[3]), top + new Vector3(-h.x + cut[0], 0f, h.z - cut[3]),
            };
            if (topOpen || ceiling) m.Polygon(topPts, Vector3.up, c.Own);
            // Underside: the soffit finish where a floor is seen from below; a ceiling keeps its own.
            Material under = b.Surface == Surface.Floor && underFree >= 3 ? c.Finish(c.Style.SoffitMaterial) : c.Own;
            if (underOpen || ceiling)
            {
                var face = new ArchRelief.Face
                {
                    Centre = Vector3.down * h.y, U = Vector3.right, V = Vector3.forward, N = Vector3.down, HalfU = h.x, HalfV = h.z,
                };
                face.PhaseU = c.Phase(face.Centre, face.U);
                face.PhaseV = c.Phase(face.Centre, face.V);
                if (!ceiling && c.Style.Ceiling == ArchStyle.CeilingKind.Formwork && h.x > 1f && h.z > 1f)
                    ArchRelief.Draw(m, face, ArchRelief.Kind.Grid, under, c.Style, h.y * .3f);
                else m.Face(face.Centre, face.U * h.x, face.V * h.z, face.N, under);
            }
            // Sides, dropped by their ease; the eased strip joins them to the top.
            for (int i = 0; i < 4; i++)
            {
                Vector3 axis = n[i];
                Vector3 across = i < 2 ? Vector3.forward : Vector3.right;
                float half = i < 2 ? h.z : h.x, reach = i < 2 ? h.x : h.z;
                float sideTop = h.y - cut[i];
                Vector3 centre = axis * reach + Vector3.up * ((sideTop - h.y) * .5f);
                m.Face(centre, across * half, Vector3.up * ((sideTop + h.y) * .5f), axis, c.Own);
                if (cut[i] > 0f)
                {
                    int lo = i < 2 ? 2 : 0, hi = lo + 1;
                    float sLo = cut[lo], sHi = cut[hi];
                    Vector3 a = axis * reach + Vector3.up * sideTop - across * half, d = axis * reach + Vector3.up * sideTop + across * half;
                    Vector3 bb = axis * (reach - cut[i]) + top - across * (half - sLo), cc = axis * (reach - cut[i]) + top + across * (half - sHi);
                    m.Quad(a, bb, cc, d, (axis + Vector3.up).normalized, across, (bb - a).normalized, c.Own);
                }
                if (drop[i] && !ceiling && c.Style.Nosing != null && h.y >= .08f)
                    Nosing(c, axis, across, half, reach, h.y, step ? .045f : .06f);
            }
            if (ceiling && c.Style.Ceiling == ArchStyle.CeilingKind.Grid) Grid(c, h);
            if (step) StepNosing(c, h, n, open);
        }

        /// <summary>How many of five points (the middle and four quarter points) just above (y &gt; 0) or below the slab are free.</summary>
        private static int Free(ArchContext c, Vector3 h, float y)
        {
            int free = 0;
            foreach (Vector2 p in new[] { Vector2.zero, new(-.5f, -.5f), new(.5f, -.5f), new(-.5f, .5f), new(.5f, .5f) })
                if (!c.Solid(new Vector3(p.x * h.x, y, p.y * h.z))) free++;
            return free;
        }

        /// <summary>A steel angle along a slab edge someone could fall from: a strip on the face, 3 mm proud of the top.</summary>
        private static void Nosing(ArchContext c, Vector3 axis, Vector3 across, float half, float reach, float hy, float depth)
        {
            Material mat = c.Finish(c.Style.Nosing.Material);
            Vector3 centre = axis * (reach + .003f) + Vector3.up * (hy - depth * .5f + .0015f);
            Vector3 size = ArchWall.Abs(across * (half * 2f) + Vector3.up * (depth + .003f) + axis * .006f);
            c.Mesh.Block(centre, size, mat);
            Vector3 lip = axis * (reach - .02f) + Vector3.up * (hy + .0015f);
            c.Mesh.Block(lip, ArchWall.Abs(across * (half * 2f) + Vector3.up * .003f + axis * .046f), mat, 4);
        }

        /// <summary>A stair tread's front: whichever long side has open air above it and the next tread behind the other.</summary>
        private static void StepNosing(ArchContext c, Vector3 h, Vector3[] n, bool[] open)
        {
            if (c.Style.Nosing == null) return;
            int front = -1;
            for (int i = 0; i < 4; i++)
            {
                bool longSide = i < 2 ? h.z > h.x : h.x > h.z;
                if (!longSide || !open[i]) continue;
                float reach = i < 2 ? h.x : h.z;
                bool clearAbove = !c.Solid(n[i] * (reach + .1f) + Vector3.up * (h.y + .08f));
                bool behind = c.Solid(-n[i] * (reach + .1f) + Vector3.up * (h.y + .08f));
                if (clearAbove && behind) front = i;
            }
            if (front < 0) return;
            Vector3 axis = n[front], across = front < 2 ? Vector3.forward : Vector3.right;
            float half = front < 2 ? h.z : h.x, edge = front < 2 ? h.x : h.z;
            // An existing nosing (some maps model one) wins.
            Vector3 probe = axis * (edge - .025f) + Vector3.up * (h.y + .002f);
            if (ArchContext.Share(c.Open(probe - across * half * .9f, probe + across * half * .9f)) < .5f) return;
            c.Mesh.Block(axis * (edge - .025f) + Vector3.up * (h.y + .0015f),
                ArchWall.Abs(across * (half * 2f - .04f) + Vector3.up * .003f + axis * .05f), c.Finish(c.Style.Nosing.Material), 4);
        }

        /// <summary>A 1200 x 600 suspended-ceiling grid under a ceiling tile, on world lines so tiles meet seamlessly.</summary>
        private static void Grid(ArchContext c, Vector3 h)
        {
            Material mat = c.Finish(c.Style.GridMaterial);
            float y = -h.y;
            const float width = .024f;
            Vector3 centre = c.World(Vector3.zero);
            void Bars(Vector3 runAxis, Vector3 stepAxis, float runHalf, float stepHalf, float pitch, float depth)
            {
                float phase = Vector3.Dot(centre, c.Box.DirToWorld(stepAxis));
                float first = Mathf.Ceil((phase - stepHalf) / pitch) * pitch - phase;
                for (float s = first; s <= stepHalf - .01f; s += pitch)
                {
                    if (s < -stepHalf + .01f) continue;
                    Vector3 at = stepAxis * s + Vector3.up * (y - depth * .5f);
                    // Ends left open: the neighbouring tile's bar carries on from them.
                    int skip = runAxis == Vector3.right ? 1 | 2 | 8 : 16 | 32 | 8;
                    c.Mesh.Block(at, ArchWall.Abs(runAxis * (runHalf * 2f) + Vector3.up * depth + stepAxis * width), mat, skip);
                }
            }
            Bars(Vector3.right, Vector3.forward, h.x, h.z, .6f, .014f);
            Bars(Vector3.forward, Vector3.right, h.z, h.x, 1.2f, .011f);
        }

        /// <summary>A pitched sheet roof: ribs down the slope on its upper face, the rest plain.</summary>
        public static void Roof(ArchContext c)
        {
            ArchBox b = c.Box;
            Vector3 h = b.Half;
            // The slope runs along whichever horizontal local axis tilts most from level.
            bool slopeX = Mathf.Abs(b.DirToWorld(Vector3.right).y) >= Mathf.Abs(b.DirToWorld(Vector3.forward).y);
            Vector3 down = slopeX ? Vector3.right : Vector3.forward, across = slopeX ? Vector3.forward : Vector3.right;
            float halfDown = slopeX ? h.x : h.z, halfAcross = slopeX ? h.z : h.x;
            Vector3 upFace = b.DirToWorld(Vector3.up).y >= 0f ? Vector3.up : Vector3.down;
            var face = new ArchRelief.Face
            {
                Centre = upFace * h.y, U = across, V = down, N = upFace, HalfU = halfAcross, HalfV = halfDown,
            };
            face.PhaseU = c.Phase(face.Centre, across);
            face.PhaseV = c.Phase(face.Centre, down);
            ArchRelief.Kind kind = b.Surface is Surface.Ribbed or Surface.Steel ? ArchRelief.Kind.Ribs : ArchRelief.Kind.HBoards;
            ArchRelief.Draw(c.Mesh, face, kind, c.Own, c.Style, h.y * .45f);
            int skip = upFace == Vector3.up ? 8 : 4;
            c.Mesh.Block(Vector3.zero, b.Size, c.Own, skip);
        }
    }
}
