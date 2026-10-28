using UnityEngine;
using Surface = Abandoned.EditorTools.Remake.ArchStyle.Surface;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Columns, beams and glazing: a steel column becomes a rolled H-section on a bolted base plate; a concrete or plastered
    /// one gets chamfered arrises, a plinth and a capital; a clad one stone joints and a metal foot. A steel beam becomes an
    /// I-section, a concrete one gets eased soffit edges. A glass pane gets a frame wherever its edge is free. All inside
    /// the original box (plates and plinths stand a few centimetres proud, as real ones do), so the collider still fits.
    /// </summary>
    public static class ArchColumn
    {
        public static void Column(ArchContext c)
        {
            ArchBox b = c.Box;
            ArchMesh m = c.Mesh;
            Vector3 h = b.Half;
            bool floor = c.Solid(Vector3.down * (h.y + .04f)) || c.Solid(new Vector3(h.x + .05f, -h.y - .04f, 0f));
            bool ceiling = c.Solid(Vector3.up * (h.y + .04f));
            Material trim = c.Finish(c.Style.ColumnTrim);
            // A pad or plinth already round the foot: the column's own foot starts on top of it.
            float foot = -h.y;
            while (foot < -h.y + 1.2f && c.Solid(new Vector3(h.x + .04f, foot + .02f, 0f)) && c.Solid(new Vector3(-h.x - .04f, foot + .02f, 0f)))
                foot += .02f;

            if (b.Surface == Surface.Steel && c.Style.Columns == ArchStyle.ColumnKind.SteelH && h.x > .07f && h.z > .07f)
            {
                // Flanges across the longer side (a real UC turns its strong axis to the frame).
                bool flangesX = h.x >= h.z;
                Vector3 fa = flangesX ? Vector3.right : Vector3.forward, wa = flangesX ? Vector3.forward : Vector3.right;
                float width = (flangesX ? h.x : h.z) * 2f, depth = (flangesX ? h.z : h.x) * 2f;
                float tf = Mathf.Clamp(depth * .08f, .012f, .04f), tw = Mathf.Clamp(depth * .05f, .008f, .025f);
                float plate = floor ? .025f : 0f, cap = ceiling ? .02f : 0f;
                float shaft = h.y * 2f - plate - cap, mid = (plate - cap) * .5f;
                foreach (float s in new[] { -1f, 1f })
                    m.Block(wa * (s * (depth * .5f - tf * .5f)) + Vector3.up * mid, ArchWall.Abs(fa * width + Vector3.up * shaft + wa * tf), c.Own);
                m.Block(Vector3.up * mid, ArchWall.Abs(fa * tw + Vector3.up * shaft + wa * (depth - tf * 2f)), c.Own, Faces(fa, true));
                if (floor)
                {
                    Vector3 plateSize = ArchWall.Abs(fa * (width + .12f) + Vector3.up * plate + wa * (depth + .12f));
                    m.Block(Vector3.up * (foot + plate * .5f), plateSize, trim, 4);
                    foreach (float sx in new[] { -1f, 1f })
                    foreach (float sz in new[] { -1f, 1f })
                    {
                        Vector3 at = fa * (sx * (width * .5f + .035f)) + wa * (sz * (depth * .5f - .03f)) + Vector3.up * (foot + plate + .012f);
                        m.Block(at, new Vector3(.034f, .024f, .034f), trim, 4);
                    }
                }
                if (ceiling) m.Block(Vector3.up * (h.y - cap * .5f), ArchWall.Abs(fa * (width + .04f) + Vector3.up * cap + wa * (depth + .04f)), trim, 8);
                return;
            }

            float ch = b.Surface switch
            {
                Surface.Steel => .004f,
                Surface.Flat or Surface.Tile => .012f,
                _ => .025f,
            };
            ch = Mathf.Min(ch, h.x * .3f, h.z * .3f);
            Shaft(c, h, ch);
            if (b.Surface == Surface.Masonry)
            {
                if (!ceiling) m.Block(Vector3.up * (h.y + .03f), new Vector3(h.x * 2f + .06f, .06f, h.z * 2f + .06f), c.Finish(c.Style.Coping?.Material), 4);
                return;
            }
            Material collar = b.Surface == Surface.Stone || c.Style.Columns == ArchStyle.ColumnKind.Clad ? trim : c.Own;
            if (floor && h.y > 1f)
                m.Block(Vector3.up * (foot + .06f), new Vector3(h.x * 2f + .04f, .12f, h.z * 2f + .04f), collar, 4);
            if (ceiling && h.y > 1.2f && c.Style.Columns != ArchStyle.ColumnKind.Clad)
            {
                m.Block(Vector3.up * (h.y - .05f), new Vector3(h.x * 2f + .06f, .1f, h.z * 2f + .06f), c.Own, 8);
                m.Block(Vector3.up * (h.y - .13f), new Vector3(h.x * 2f + .03f, .06f, h.z * 2f + .03f), c.Own);
            }
        }

        /// <summary>Bits for <see cref="ArchMesh.Block"/> that leave out the two faces along <paramref name="axis"/> (or the others).</summary>
        private static int Faces(Vector3 axis, bool skipAlong) => axis == Vector3.right ? (skipAlong ? 16 | 32 : 1 | 2) : (skipAlong ? 1 | 2 : 16 | 32);

        /// <summary>An upright shaft with all four arrises chamfered; clad columns get stone joints on their faces.</summary>
        private static void Shaft(ArchContext c, Vector3 h, float ch)
        {
            ArchMesh m = c.Mesh;
            bool clad = c.Style.Columns == ArchStyle.ColumnKind.Clad && c.Box.Surface is Surface.Stone or Surface.Flat or Surface.Concrete;
            Vector3[] n = { Vector3.right, Vector3.forward, Vector3.left, Vector3.back };
            for (int i = 0; i < 4; i++)
            {
                Vector3 axis = n[i], across = Vector3.Cross(Vector3.up, axis);
                float reach = Mathf.Abs(Vector3.Dot(h, axis)), half = Mathf.Abs(Vector3.Dot(h, across)) - ch;
                var face = new ArchRelief.Face { Centre = axis * reach, U = across, V = Vector3.up, N = axis, HalfU = half, HalfV = h.y };
                face.PhaseU = c.Phase(face.Centre, across);
                face.PhaseV = c.World(face.Centre).y;
                if (clad && half > .2f && h.y <= 6f) ArchRelief.Panels(m, face, c.Own, c.Style.StonePanel, .01f);
                else m.Face(face.Centre, across * half, Vector3.up * h.y, axis, c.Own);
                if (ch <= 0f) continue;
                Vector3 next = n[(i + 1) % 4];
                float nextReach = Mathf.Abs(Vector3.Dot(h, next));
                Vector3 a = axis * reach + across * half, d = next * nextReach + axis * (reach - ch);
                // a: this face's edge toward 'across'; d: the next face's edge toward this one.
                if (Vector3.Dot(across, next) < 0f) a = axis * reach - across * half;
                Vector3 up = Vector3.up * h.y;
                m.Quad(a - up, a + up, d + up, d - up, (axis + next).normalized, (d - a).normalized, Vector3.up, c.Own);
            }
            var plan = new System.Collections.Generic.List<Vector3>();
            for (int i = 0; i < 4; i++)
            {
                Vector3 axis = n[i], next = n[(i + 1) % 4];
                float reach = Mathf.Abs(Vector3.Dot(h, axis)), nextReach = Mathf.Abs(Vector3.Dot(h, next));
                plan.Add(axis * reach + next * (nextReach - ch));
                plan.Add(next * nextReach + axis * (reach - ch));
            }
            m.Polygon(plan.ConvertAll(p => p + Vector3.up * h.y), Vector3.up, c.Own);
            m.Polygon(plan.ConvertAll(p => p - Vector3.up * h.y), Vector3.down, c.Own);
        }

        public static void Beam(ArchContext c)
        {
            ArchBox b = c.Box;
            ArchMesh m = c.Mesh;
            Vector3 h = b.Half;
            bool alongX = h.x >= h.z;
            Vector3 run = alongX ? Vector3.right : Vector3.forward, side = alongX ? Vector3.forward : Vector3.right;
            float length = (alongX ? h.x : h.z) * 2f, width = (alongX ? h.z : h.x) * 2f, depth = h.y * 2f;
            if (b.Surface == Surface.Steel && width > .08f)
            {
                float tf = Mathf.Clamp(depth * .08f, .01f, .035f), tw = Mathf.Clamp(width * .08f, .007f, .02f);
                foreach (float s in new[] { -1f, 1f })
                    m.Block(Vector3.up * (s * (h.y - tf * .5f)), ArchWall.Abs(run * length + Vector3.up * tf + side * width), c.Own);
                m.Block(Vector3.zero, ArchWall.Abs(run * length + Vector3.up * (depth - tf * 2f) + side * tw), c.Own, 4 | 8);
                return;
            }
            // Concrete, plaster or stone: the two soffit arrises eased.
            float ch = Mathf.Min(.02f, width * .2f, depth * .2f);
            foreach (float s in new[] { -1f, 1f })
                m.Face(side * (s * h.z * 0f + s * (alongX ? h.z : h.x)) + Vector3.up * (ch * .5f), run * (length * .5f), Vector3.up * (h.y - ch * .5f), side * s, c.Own);
            m.Face(Vector3.up * h.y, run * (length * .5f), side * (width * .5f), Vector3.up, c.Own);
            m.Face(Vector3.down * h.y, run * (length * .5f), side * (width * .5f - ch), Vector3.down, c.Own);
            foreach (float s in new[] { -1f, 1f })
            {
                Vector3 a = side * (s * (width * .5f)) + Vector3.down * (h.y - ch), d = side * (s * (width * .5f - ch)) + Vector3.down * h.y;
                m.Quad(a - run * (length * .5f), a + run * (length * .5f), d + run * (length * .5f), d - run * (length * .5f),
                    (side * s + Vector3.down).normalized, run, (d - a).normalized, c.Own);
            }
            foreach (float e in new[] { -1f, 1f })
                m.Face(run * (e * length * .5f), side * (width * .5f), Vector3.up * h.y, run * e, c.Own);
        }

        public static void Pane(ArchContext c)
        {
            ArchBox b = c.Box;
            ArchMesh m = c.Mesh;
            Vector3 h = b.Half;
            m.Block(Vector3.zero, b.Size, c.Own);
            bool alongX = h.x >= h.z;
            Vector3 run = alongX ? Vector3.right : Vector3.forward, thick = alongX ? Vector3.forward : Vector3.right;
            float hl = alongX ? h.x : h.z, ht = alongX ? h.z : h.x;
            if (hl < .25f || h.y < .2f) return;
            Material frame = c.Finish(c.Style.FrameMaterial);
            const float fw = .05f;
            float fd = Mathf.Max(ht * 2f + .03f, .06f);
            bool Free(Vector3 local) => !c.Solid(local);
            bool left = Free(-run * (hl + .02f)), right = Free(run * (hl + .02f));
            bool bottom = Free(Vector3.down * (h.y + .02f)), top = Free(Vector3.up * (h.y + .02f));
            if (left) m.Block(-run * (hl - fw * .5f), ArchWall.Abs(run * fw + Vector3.up * h.y * 2f + thick * fd), frame);
            if (right) m.Block(run * (hl - fw * .5f), ArchWall.Abs(run * fw + Vector3.up * h.y * 2f + thick * fd), frame);
            float inner = hl - (left ? fw : 0f) * .5f - (right ? fw : 0f) * .5f, shift = ((left ? fw : 0f) - (right ? fw : 0f)) * .5f;
            if (bottom) m.Block(run * shift + Vector3.down * (h.y - fw * .5f), ArchWall.Abs(run * inner * 2f + Vector3.up * fw + thick * (fd + .01f)), frame);
            if (top) m.Block(run * shift + Vector3.up * (h.y - fw * .5f), ArchWall.Abs(run * inner * 2f + Vector3.up * fw + thick * (fd - .01f)), frame);
        }
    }
}
