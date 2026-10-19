using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The two ways between the dock and the plateau. The switchback road (the haul route: wide, gentle, and it never
    /// falls) climbs the east shore in two legs around a turn, with a broken guard rail on its sea side and a low
    /// kerb against the cut. The cliff stair (the shortcut: three rusted steel flights and their landings bolted to
    /// the south face) is quick and can collapse, after which the road is the only way.
    /// </summary>
    public static class IslandRoad
    {
        private const float Embankment = 22f;

        public static List<IslandFlights.Built> Build(Transform root, IslandPrisonArt art)
        {
            Transform road = Group("Road", root);
            Legs(road, art);
            Rails(Group("RoadRails", road), art);
            MarkStatic(road);
            return CliffStair(Group("CliffStair", root), art);
        }

        private static void Legs(Transform g, IslandPrisonArt art)
        {
            Rect a = L.RoadA, turn = L.RoadTurn, b = L.RoadB, t = L.Terrace;
            // Leg A: quay (y 0, z 20) north to the turn (y 6). The embankment runs deep into the bay.
            GameObject legA = K.Ramp("LegA", g, new Vector3(a.center.x, L.QuayY, a.yMin), Vector3.forward, a.height, L.TurnY, a.width, Embankment, art.Rock);
            legA.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);
            K.Ramp("LegA_Surface", g, new Vector3((L.RoadAWidthFrom + L.RoadAWidthTo) * .5f, L.QuayY + .02f, a.yMin), Vector3.forward,
                a.height, L.TurnY, L.RoadAWidthTo - L.RoadAWidthFrom, .05f, art.Asphalt, false);
            // The turn: flat, with a passing bay.
            GameObject landing = K.Span("Turn", g, new Vector3(turn.xMin, L.TurnY - Embankment, turn.yMin), new Vector3(turn.xMax, L.TurnY, turn.yMax), art.Rock);
            landing.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);
            K.Span("Turn_Surface", g, new Vector3(turn.xMin + .5f, L.TurnY, turn.yMin + .5f), new Vector3(turn.xMax - .5f, L.TurnY + .02f, turn.yMax - .5f), art.Asphalt, false);
            // Leg B: from the turn back south, rising to the terrace (y 12). Its east side is the cut above leg A.
            GameObject legB = K.Ramp("LegB", g, new Vector3(b.center.x, L.TurnY, b.yMax), Vector3.back, b.height, L.PlateauY - L.TurnY, b.width, Embankment, art.Rock);
            legB.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);
            K.Ramp("LegB_Surface", g, new Vector3((L.RoadBWidthFrom + L.RoadBWidthTo) * .5f, L.TurnY + .02f, b.yMax), Vector3.back,
                b.height, L.PlateauY - L.TurnY, L.RoadBWidthTo - L.RoadBWidthFrom, .05f, art.Asphalt, false);
            // The terrace where the road meets the plateau, beside the cellhouse approach.
            GameObject terrace = K.Span("Terrace", g, new Vector3(t.xMin, L.PlateauY - Embankment, t.yMin), new Vector3(t.xMax, L.PlateauY, t.yMax), art.Rock);
            terrace.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);
            K.Span("Terrace_Surface", g, new Vector3(t.xMin, L.PlateauY, t.yMin + .5f), new Vector3(t.xMax - .5f, L.PlateauY + .02f, t.yMax), art.Asphalt, false);
            // A paved path from the terrace to the cellhouse's main door, and one round to the east door.
            K.Span("Path_Main", g, new Vector3(L.MainDoorX - 2.5f, L.PlateauY, L.Plateau.yMin + 2f), new Vector3(L.MainDoorX + 2.5f, L.PlateauY + .02f, L.Cellhouse.yMin), art.Asphalt, false);
            K.Span("Path_Terrace", g, new Vector3(L.MainDoorX - 2.5f, L.PlateauY, L.Plateau.yMin + 2f), new Vector3(t.xMin, L.PlateauY + .02f, L.Plateau.yMin + 7f), art.Asphalt, false);
            K.Span("Path_East", g, new Vector3(L.Cellhouse.xMax, L.PlateauY, L.EastDoorZ - 2f), new Vector3(t.xMin + 4f, L.PlateauY + .02f, L.EastDoorZ + 2f), art.Gravel, false);
            K.Span("Path_EastDown", g, new Vector3(t.xMin, L.PlateauY, t.yMax), new Vector3(t.xMin + 4f, L.PlateauY + .02f, L.EastDoorZ - 2f), art.Gravel, false);
            // Kerb stones along the cut side of each leg (visual, 15 cm).
            K.Span("Kerb_A", g, new Vector3(L.RoadAWidthFrom - .25f, 0f, a.yMin), new Vector3(L.RoadAWidthFrom, .15f, a.yMin + .5f), art.Facade, false);
        }

        // The sea side of leg A and the turn, and the drop from leg B to leg A: a dented rail with stretches gone.
        private static void Rails(Transform g, IslandPrisonArt art)
        {
            Rect a = L.RoadA, b = L.RoadB, turn = L.RoadTurn;
            float Ya(float z) => Mathf.Lerp(L.QuayY, L.TurnY, (z - a.yMin) / a.height);
            float Yb(float z) => Mathf.Lerp(L.PlateauY, L.TurnY, (z - b.yMin) / b.height);
            // (from z, to z): the gaps between are where the rail has gone.
            foreach ((float z0, float z1) in new[] { (21f, 31f), (34f, 47f), (52f, 63f) })
                K.Railing(g, "SeaRail_A", new Vector3(a.xMax - .3f, Ya(z0), z0), new Vector3(a.xMax - .3f, Ya(z1), z1), 1f, art.Railing);
            K.Railing(g, "SeaRail_Turn_E", new Vector3(turn.xMax - .3f, L.TurnY, turn.yMin), new Vector3(turn.xMax - .3f, L.TurnY, turn.yMax - .3f), 1f, art.Railing);
            K.Railing(g, "SeaRail_Turn_N", new Vector3(turn.xMin + 4f, L.TurnY, turn.yMax - .3f), new Vector3(turn.xMax - .3f, L.TurnY, turn.yMax - .3f), 1f, art.Railing);
            foreach ((float z0, float z1) in new[] { (29f, 40f), (44f, 56f), (58f, 63.5f) })
                K.Railing(g, "CutRail_B", new Vector3(b.xMax - .2f, Yb(z0), z0), new Vector3(b.xMax - .2f, Yb(z1), z1), 1f, art.Railing);
            // Lamp-post stumps where the road lamps were (the working ones are in IslandLighting).
            for (float z = 24f; z < a.yMax; z += 12f)
                Box("PostStump", g, new Vector3(a.xMax - .8f, Ya(z) + .5f, z), new Vector3(.18f, 1f, .18f), art.Rust, false);
        }

        private static List<IslandFlights.Built> CliffStair(Transform g, IslandPrisonArt art)
        {
            var built = new List<IslandFlights.Built>();
            foreach (L.Flight f in L.CliffFlights) built.Add(IslandFlights.Flight(g, f, art.Plate, art.Rust));
            foreach (L.Landing l in L.CliffLandings)
            {
                Rect r = l.Area;
                // Rails on the open sides: the outer (south) edge always, the far end where no flight arrives.
                Vector3 sw = new(r.xMin, l.Y, r.yMin), se = new(r.xMax, l.Y, r.yMin), ne = new(r.xMax, l.Y, r.yMax), nw = new(r.xMin, l.Y, r.yMax);
                built.Add(l.Name switch
                {
                    "CliffLanding_1" => IslandFlights.Landing(g, l, art.Plate, art.Rust, sw, se, se, ne),
                    "CliffLanding_2" => IslandFlights.Landing(g, l, art.Plate, art.Rust, sw, se, nw, sw),
                    _ => IslandFlights.Landing(g, l, art.Plate, art.Rust, se, ne, sw, se),
                });
            }
            // Brackets bolting the stair to the rock, and a rusted cage at the bottom where the gate hung.
            for (float x = 90f; x < 103f; x += 3f)
            foreach (float y in new[] { 3.6f, 7.6f, 11.6f })
                Box("Bracket", g, new Vector3(x, y, 19.6f), new Vector3(.12f, .12f, .8f), art.Rust, false);
            Box("GatePost_L", g, new Vector3(91.6f, 1.1f, 16.9f), new Vector3(.1f, 2.2f, .1f), art.Rust, false);
            Box("GatePost_R", g, new Vector3(91.6f, 1.1f, 19.1f), new Vector3(.1f, 2.2f, .1f), art.Rust, false);
            return built;
        }
    }
}
