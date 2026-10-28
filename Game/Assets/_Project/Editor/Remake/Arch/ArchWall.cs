using System.Collections.Generic;
using UnityEngine;
using Surface = Abandoned.EditorTools.Remake.ArchStyle.Surface;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// A wall, header or pier rebuilt as construction: its body (chamfered where a corner is exposed, its faces cut with
    /// the relief of what it is made of), then the trims a builder would fit where they belong: skirting where the face
    /// meets a floor, a cornice where it meets a ceiling, a chair rail and wainscot inside, casings round the openings it
    /// frames, coping on an exposed top, a plinth and storey bands outside. Each runs only along the open stretches.
    /// </summary>
    public static class ArchWall
    {
        public static void Build(ArchContext c)
        {
            ArchBox b = c.Box;
            ArchStyle style = c.Style;
            ArchMesh m = c.Mesh;
            bool alongX = b.Size.x >= b.Size.z;
            Vector3 U = alongX ? Vector3.right : Vector3.forward, W = alongX ? Vector3.forward : Vector3.right, Y = Vector3.up;
            float L = alongX ? b.Size.x : b.Size.z, T = alongX ? b.Size.z : b.Size.x, H = b.Size.y;
            float hl = L * .5f, ht = T * .5f, hh = H * .5f;
            Vector3 P(float u, float y, float w) => U * u + Y * y + W * w;

            // What is around it.
            var endOpen = new bool[2];
            var endSpans = new List<Vector2>[2];
            for (int i = 0; i < 2; i++)
            {
                float e = i == 0 ? -1f : 1f;
                endSpans[i] = c.Open(P(e * (hl + .03f), -hh, 0f), P(e * (hl + .03f), hh, 0f));
                endOpen[i] = ArchContext.Share(endSpans[i]) > .3f;
            }
            var visible = new bool[2];
            var outside = new bool[2];
            for (int i = 0; i < 2; i++)
            {
                float s = i == 0 ? -1f : 1f;
                float share = 0f;
                foreach (float y in new[] { -hh * .8f, 0f, hh * .8f })
                    share = Mathf.Max(share, ArchContext.Share(c.Open(P(-hl, y, s * (ht + .02f)), P(hl, y, s * (ht + .02f)))));
                visible[i] = share > .02f;
                int sky = 0;
                foreach (float u in new[] { -hl * .6f, 0f, hl * .6f })
                    if (c.Outside(P(u, 0f, s * (ht + .3f)))) sky++;
                outside[i] = sky >= 2;
            }
            bool topOpen = !c.Solid(P(0f, hh + .03f, 0f)) || !c.Solid(P(-hl * .7f, hh + .03f, 0f)) || !c.Solid(P(hl * .7f, hh + .03f, 0f));
            bool bottomOpen = !c.Solid(P(0f, -hh - .03f, 0f)) || !c.Solid(P(-hl * .7f, -hh - .03f, 0f)) || !c.Solid(P(hl * .7f, -hh - .03f, 0f));

            Surface surface = b.Surface;
            bool trims = T >= .05f && surface is Surface.Flat or Surface.Masonry or Surface.Concrete or Surface.Tile or
                Surface.Boards or Surface.Stone or Surface.Ribbed;
            bool coping = topOpen && trims && style.Coping != null && surface != Surface.Ribbed && H < 4f;

            // ── Body ─────────────────────────────────────────────────────────────
            float ch = T < .05f ? 0f : surface switch
            {
                Surface.Masonry or Surface.Concrete or Surface.Stone => .015f,
                Surface.Flat or Surface.Tile => .01f,
                Surface.Boards => .006f,
                Surface.Steel => .004f,
                _ => 0f,
            };
            ch = Mathf.Min(ch, T * .25f, L * .1f);
            // Plan outline, corners (-u-w, +u-w, +u+w, -u+w); a chamfered corner is cut from each of its edges.
            float Cut(int end, int side) => endOpen[end] && visible[side] ? ch : 0f;
            float cut00 = Cut(0, 0), cut10 = Cut(1, 0), cut11 = Cut(1, 1), cut01 = Cut(0, 1);

            ArchRelief.Kind relief = surface switch
            {
                Surface.Ribbed => ArchRelief.Kind.Ribs,
                Surface.Concrete when L > 1.5f && H > 1.5f => ArchRelief.Kind.Grid,
                Surface.Boards => b.Material != null && b.Material.name.ToLowerInvariant().Contains("siding")
                    ? ArchRelief.Kind.HBoards : ArchRelief.Kind.VBoards,
                _ => ArchRelief.Kind.Plain,
            };
            for (int i = 0; i < 2; i++)
            {
                float s = i == 0 ? -1f : 1f;
                float u0 = -hl + (i == 0 ? cut00 : cut01), u1 = hl - (i == 0 ? cut10 : cut11);
                var face = new ArchRelief.Face
                {
                    Centre = P((u0 + u1) * .5f, 0f, s * ht), U = U, V = Y, N = W * s,
                    HalfU = (u1 - u0) * .5f, HalfV = hh,
                };
                face.PhaseU = c.Phase(face.Centre, U);
                face.PhaseV = c.World(face.Centre).y;
                if (!visible[i]) ArchRelief.Draw(m, face, ArchRelief.Kind.Plain, c.Own, style, 0f);
                else if (surface == Surface.Stone && L > 1.2f) ArchRelief.Panels(m, face, c.Own, style.StonePanel, T * .2f);
                else ArchRelief.Draw(m, face, relief, c.Own, style, T * .25f);
            }
            for (int i = 0; i < 2; i++)
            {
                float e = i == 0 ? -1f : 1f;
                float w0 = -ht + (i == 0 ? cut00 : cut10), w1 = ht - (i == 0 ? cut01 : cut11);
                m.Face(P(e * hl, 0f, (w0 + w1) * .5f), W * ((w1 - w0) * .5f), Y * hh, U * e, c.Own);
            }
            Chamfer(m, c.Own, P, -hl, -ht, cut00, -1f, -1f, hh, U, W);
            Chamfer(m, c.Own, P, hl, -ht, cut10, 1f, -1f, hh, U, W);
            Chamfer(m, c.Own, P, hl, ht, cut11, 1f, 1f, hh, U, W);
            Chamfer(m, c.Own, P, -hl, ht, cut01, -1f, 1f, hh, U, W);
            var plan = new List<Vector2>
            {
                new(-hl + cut00, -ht), new(hl - cut10, -ht), new(hl, -ht + cut10), new(hl, ht - cut11),
                new(hl - cut11, ht), new(-hl + cut01, ht), new(-hl, ht - cut01), new(-hl, -ht + cut00),
            };
            if (topOpen && !coping) m.Polygon(plan.ConvertAll(p => P(p.x, hh, p.y)), Y, c.Own);
            if (bottomOpen) m.Polygon(plan.ConvertAll(p => P(p.x, -hh, p.y)), -Y, c.Own);

            if (!trims) return;

            // ── Trims ────────────────────────────────────────────────────────────
            for (int i = 0; i < 2; i++)
            {
                if (!visible[i]) continue;
                float s = i == 0 ? -1f : 1f;
                List<Vector2> Line(float y, float off) => c.Open(P(-hl, y, s * (ht + off)), P(hl, y, s * (ht + off)));
                List<Vector2> floor = ArchContext.Invert(Line(-hh - .03f, .06f));
                List<Vector2> baseRuns = ArchContext.AtLeast(ArchContext.Intersect(Line(-hh + .04f, .01f), floor), L, .15f);
                void Run(ArchStyle.Trim trim, float y, List<Vector2> runs)
                {
                    if (trim == null || trim.Profile == null || trim.Profile.Length < 2) return;
                    Material mat = c.Finish(trim.Material);
                    foreach (Vector2 r in runs)
                        m.Sweep(trim.Profile, P(-hl + r.x * L, y, s * ht), P(-hl + r.y * L, y, s * ht), W * s, Y, mat);
                }
                if (!outside[i])
                {
                    if (style.Wainscot > 0f && surface == Surface.Flat && H > style.Wainscot + .6f && baseRuns.Count > 0)
                    {
                        float wh = style.Wainscot;
                        Run(new ArchStyle.Trim(style.WainscotMaterial, ArchProfiles.KickPlate(wh)), -hh, baseRuns);
                        Run(style.WainscotCap, -hh + wh, baseRuns);
                    }
                    Run(style.Skirting, -hh, baseRuns);
                    if (style.ChairRail != null && style.ChairRailHeight > 0f && H > style.ChairRailHeight + .5f && baseRuns.Count > 0)
                    {
                        float y = -hh + style.ChairRailHeight;
                        Run(style.ChairRail, y, ArchContext.AtLeast(ArchContext.Intersect(Line(y, .01f), floor), L, .3f));
                    }
                    List<Vector2> ceiling = ArchContext.Invert(Line(hh + .03f, .06f));
                    Run(style.Cornice, hh, ArchContext.AtLeast(ArchContext.Intersect(Line(hh - .04f, .01f), ceiling), L, .3f));
                }
                else
                {
                    Run(style.Plinth, -hh, baseRuns);
                    if (style.StringCourse != null && style.StoreyBands > 0f && H > 3f)
                        for (float yw = Mathf.Ceil((b.Bottom + 1f) / style.StoreyBands) * style.StoreyBands; yw < b.Top - .6f; yw += style.StoreyBands)
                        {
                            float y = yw - b.Centre.y;
                            Run(style.StringCourse, y, ArchContext.AtLeast(Line(y, .01f), L, .5f));
                        }
                }
            }
            Openings(c, P, U, W, Y, hl, ht, hh, L, endOpen, endSpans, visible, outside);
            if (coping)
            {
                float over = .025f;
                float u0 = -hl - (endOpen[0] ? over : 0f), u1 = hl + (endOpen[1] ? over : 0f);
                Vector3 size = Abs(U * (u1 - u0) + Y * .05f + W * (T + over * 2f));
                m.Block(P((u0 + u1) * .5f, hh + .025f, 0f), size, c.Finish(style.Coping.Material), 4);
            }
        }

        /// <summary>Casings up the exposed ends (an opening's jambs) and along an exposed underside (its head), inside.</summary>
        private static void Openings(ArchContext c, System.Func<float, float, float, Vector3> P, Vector3 U, Vector3 W, Vector3 Y,
            float hl, float ht, float hh, float L, bool[] endOpen, List<Vector2>[] endSpans, bool[] visible, bool[] outside)
        {
            ArchStyle.Trim casing = c.Style.Casing;
            if (casing == null || casing.Profile == null) return;
            Material mat = c.Finish(casing.Material);
            float H = hh * 2f;
            for (int side = 0; side < 2; side++)
            {
                if (!visible[side] || outside[side]) continue;
                float s = side == 0 ? -1f : 1f;
                for (int end = 0; end < 2; end++)
                {
                    if (!endOpen[end] || L < casing.Depth * 4f + .1f) continue;
                    float e = end == 0 ? -1f : 1f;
                    foreach (Vector2 span in endSpans[end])
                    {
                        if ((span.y - span.x) * H < .5f) continue;
                        float y0 = -hh + span.x * H, y1 = -hh + span.y * H;
                        c.Mesh.Sweep(casing.Profile, P(e * hl, y0, s * ht), P(e * hl, y1, s * ht), W * s, U * -e, mat);
                    }
                }
                List<Vector2> head = ArchContext.AtLeast(c.Open(P(-hl, -hh - .03f, 0f), P(hl, -hh - .03f, 0f)), L, .5f);
                if (head.Count == 0 || c.Box.Bottom - Ground(c, P, hh) < 1.2f) continue;
                foreach (Vector2 r in head)
                    c.Mesh.Sweep(casing.Profile, P(-hl + r.x * L, -hh, s * ht), P(-hl + r.y * L, -hh, s * ht), W * s, Y, mat);
            }
        }

        /// <summary>World height of whatever is under the wall's middle (within 4 m), or its own bottom.</summary>
        private static float Ground(ArchContext c, System.Func<float, float, float, Vector3> P, float hh)
        {
            Vector3 from = c.World(P(0f, -hh - .02f, 0f));
            return Physics.Raycast(from, Vector3.down, out RaycastHit hit, 4f, ~0, QueryTriggerInteraction.Ignore) ? hit.point.y : c.Box.Bottom - 4f;
        }

        private static void Chamfer(ArchMesh m, Material mat, System.Func<float, float, float, Vector3> P, float u, float w, float cut,
            float su, float sw, float hh, Vector3 U, Vector3 W)
        {
            if (cut <= 0f) return;
            Vector3 a = P(u - su * cut, -hh, w), b = P(u, -hh, w - sw * cut);
            Vector3 up = Vector3.up * hh * 2f;
            m.Quad(a, a + up, b + up, b, (U * su + W * sw).normalized, (b - a).normalized, Vector3.up, mat);
        }

        public static Vector3 Abs(Vector3 v) => new(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }
}
