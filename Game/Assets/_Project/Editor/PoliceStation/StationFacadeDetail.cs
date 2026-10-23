using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using Hole = Abandoned.EditorTools.StationWalls.Hole;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The station's outer walls dressed like a civic building of the 1930s, after <see cref="StationFacade"/> has cut
    /// them: each window gets its frame, mullion and transom in the reveal, a stone lintel and keystone over it and the
    /// grime streak under its sill; brick pilasters stand proud between the bays, stone quoins climb the corners, a
    /// dentilled cornice runs under the coping and downpipes come down from the parapet. Visual only (the shell is the
    /// solid); everything goes under <c>Shell/Detail</c>, so it is static and occludes like the walls.
    /// </summary>
    public static class StationFacadeDetail
    {
        public static int Apply(Transform root, StationPlan plan, StationArt art)
        {
            Transform shell = root.Find("Shell");
            if (shell == null) return 0;
            Transform detail = Group("Detail", shell);
            Material frame = art.Plain("WindowFrame", new Color(.16f, .15f, .13f), .45f, .3f);
            Material gutter = art.Plain("Downpipe", new Color(.2f, .21f, .2f), .4f, .5f);
            int made = 0;
            foreach (P.Side side in new[] { P.Side.South, P.Side.North, P.Side.West, P.Side.East })
                foreach ((float from, float to, List<Vector2Int> cells) in StationFacade.Runs(plan, side))
                {
                    bool alongX = side is P.Side.South or P.Side.North;
                    float outward = side is P.Side.South or P.Side.West ? -1f : 1f;
                    float line = side switch
                    {
                        P.Side.South => 0f, P.Side.North => plan.Depth * P.Tile, P.Side.West => 0f, _ => plan.Width * P.Tile,
                    };
                    float face = line + outward * P.ShellThickness * .5f, length = alongX ? plan.Width * P.Tile : plan.Depth * P.Tile;
                    List<Hole> holes = StationFacade.Openings(plan, side, from, to, cells);
                    Vector3 At(float along, float y, float proud) =>
                        alongX ? new Vector3(along, y, face + outward * proud) : new Vector3(face + outward * proud, y, along);
                    Vector3 Size(float along, float y, float deep) => alongX ? new Vector3(along, y, deep) : new Vector3(deep, y, along);
                    Transform run = Group($"{side}_{from:0}", detail);

                    foreach (Hole h in holes)
                    {
                        if (!h.Glazed || h.Bottom < .2f) continue;
                        float w = h.Width, hh = h.Top - h.Bottom, mid = h.Bottom + hh * .5f;
                        // In the reveal, at the pane: the frame, a mullion and a transom.
                        float pane = -P.ShellThickness * .5f + .03f;
                        Box("FrameHead", run, At(h.Centre, h.Top - .04f, pane), Size(w, .08f, .07f), frame, false);
                        Box("FrameFoot", run, At(h.Centre, h.Bottom + .04f, pane), Size(w, .08f, .07f), frame, false);
                        foreach (float s in new[] { -1f, 1f })
                            Box("FrameJamb", run, At(h.Centre + s * (w * .5f - .04f), mid, pane), Size(.08f, hh, .07f), frame, false);
                        Box("Mullion", run, At(h.Centre, mid, pane), Size(.05f, hh, .06f), frame, false);
                        Box("Transom", run, At(h.Centre, h.Bottom + hh * .68f, pane), Size(w, .05f, .06f), frame, false);
                        // Outside: the stone lintel and its keystone, and the streak the sill has wept down the brick.
                        Box("Lintel", run, At(h.Centre, h.Top + .13f, .04f), Size(w + .36f, .26f, .08f), art.Trim, false);
                        Box("Keystone", run, At(h.Centre, h.Top + .17f, .07f), Size(.28f, .36f, .06f), art.Trim, false);
                        float streak = Mathf.Min(1.4f, h.Bottom - .6f);
                        if (streak > .2f) Box("Streak", run, At(h.Centre, h.Bottom - .12f - streak * .5f, .012f), Size(w * .55f, streak, .004f), art.Grime, false);
                        made += 9;
                    }
                    // Pilasters on the bay lines (every 8 m) wherever no opening is near.
                    for (float p = Mathf.Ceil((from + .1f) / 8f) * 8f; p < to - .1f; p += 8f)
                    {
                        if (holes.Any(h => Mathf.Abs(h.Centre - p) < h.Width * .5f + .45f)) continue;
                        Box("Pilaster", run, At(p, (plan.ParapetTop - .6f + .45f) * .5f, .06f), Size(.62f, plan.ParapetTop - 1.05f, .12f), art.BrickDark, false);
                        Box("PilasterCap", run, At(p, plan.ParapetTop - .68f, .1f), Size(.78f, .16f, .2f), art.Trim, false);
                        made += 2;
                    }
                    // Stone quoins up the building's corners.
                    foreach (float end in new[] { from, to })
                    {
                        if (end > .01f && end < length - .01f) continue;
                        float inward = end <= .01f ? 1f : -1f;
                        for (int i = 0; i * .6f < plan.ParapetTop - .9f; i++)
                        {
                            float wide = i % 2 == 0 ? .55f : .32f;
                            Box("Quoin", run, At(end + inward * wide * .5f, .75f + i * .6f, .02f), Size(wide, .27f, .05f), art.Trim, false);
                            made++;
                        }
                    }
                    // The cornice under the coping: a band, a crown, and dentils.
                    float span = to - from, centre = (from + to) * .5f, cy = plan.ParapetTop - .55f;
                    Box("CorniceBand", run, At(centre, cy, .05f), Size(span, .22f, .1f), art.Trim, false);
                    Box("CorniceCrown", run, At(centre, cy + .2f, .13f), Size(span, .14f, .26f), art.Trim, false);
                    for (float d = from + .25f; d < to - .2f; d += .36f)
                        Box("Dentil", run, At(d, cy - .16f, .07f), Size(.12f, .1f, .09f), art.Trim, false);
                    made += 2;
                    // Downpipes a metre in from each end of the run, from a hopper under the coping to a shoe at the foot.
                    foreach (float d in new[] { from + 1f, to - 1f })
                    {
                        if (holes.Any(h => Mathf.Abs(h.Centre - d) < h.Width * .5f + .3f && h.Bottom < .5f)) continue;
                        Box("Downpipe", run, At(d, (plan.ParapetTop - .7f) * .5f, .08f), Size(.11f, plan.ParapetTop - .7f, .11f), gutter, false);
                        Box("Hopper", run, At(d, plan.ParapetTop - .8f, .1f), Size(.3f, .26f, .2f), gutter, false);
                        Box("Shoe", run, At(d, .08f, .16f), Size(.13f, .14f, .26f), gutter, false);
                        made += 3;
                    }
                }
            MarkStatic(detail);
            StationSite.TrimOccluders(detail, 100f);
            return made;
        }
    }
}
