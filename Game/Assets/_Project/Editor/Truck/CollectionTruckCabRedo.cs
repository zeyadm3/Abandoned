using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>A hollow cab-over shell: formed panel surfaces, open wheel arches and inset glazed apertures.</summary>
    public static class CollectionTruckCabRedo
    {
        public static void Build(CollectionTruckGeometry g)
        {
            Shell(g); Glazing(g); Front(g); Doors(g); Interior(g);
        }

        private static void Shell(CollectionTruckGeometry g)
        {
            int facets = g.Detail == 0 ? 16 : g.Detail == 1 ? 10 : 6;
            foreach (float side in new[] { -1f, 1f })
            {
                var contour = new List<Vector2> { new Vector2(.66f, 2.76f), new Vector2(1.66f, 2.76f),
                    new Vector2(1.66f, 4.96f), new Vector2(1.54f, 5.10f), new Vector2(.72f, 5.15f) };
                for (int i = 0; i <= facets; i++)
                {
                    float a = Mathf.Lerp(18, 162, i / (float)facets) * Mathf.Deg2Rad;
                    contour.Add(new Vector2(.48f + Mathf.Sin(a) * .60f, 3.75f + Mathf.Cos(a) * .60f));
                }
                CollectionTruckCabMesh.Side(g, "FormedCabSide", contour.ToArray(), side * 1.205f, side * .035f, "Paint");
                CollectionTruckCabMesh.Surface(g, "RoundedWheelArchLip", "Paint", facets, 2, (u, v) =>
                {
                    float a = Mathf.Lerp(18, 162, u) * Mathf.Deg2Rad, r = Mathf.Lerp(.60f, .654f, v);
                    return new Vector3(side * (1.24f + Mathf.Cos(v * Mathf.PI * .5f) * .075f), .48f + Mathf.Sin(a) * r, 3.75f + Mathf.Cos(a) * r);
                }, side > 0);
                CollectionTruckCabMesh.Surface(g, "RecessedWheelArchLiner", "Dark", facets, 1, (u, v) =>
                {
                    float a = Mathf.Lerp(16, 164, u) * Mathf.Deg2Rad;
                    return new Vector3(side * Mathf.Lerp(1.015f, 1.315f, v), .48f + Mathf.Sin(a) * .60f, 3.75f + Mathf.Cos(a) * .60f);
                }, side < 0);
                CollectionTruckCabMesh.Quad(g, "CabRearQuarter", "Paint", new Vector3(side * 1.24f, 1.65f, 2.76f),
                    new Vector3(side * 1.24f, 1.65f, 3.015f), new Vector3(side * 1.20f, 2.72f, 3.015f),
                    new Vector3(side * 1.20f, 2.72f, 2.76f), true);
                CollectionTruckCabMesh.Quad(g, "SweptFrontPillar", "Paint", new Vector3(side * 1.24f, 1.65f, 4.88f),
                    new Vector3(side * 1.18f, 1.65f, 5.065f), new Vector3(side * 1.12f, 2.70f, 4.76f),
                    new Vector3(side * 1.20f, 2.70f, 4.68f), true);
                CollectionTruckCabMesh.Quad(g, "SideRoofRail", "Paint", new Vector3(side * 1.24f, 2.64f, 3.015f),
                    new Vector3(side * 1.20f, 2.64f, 4.70f), new Vector3(side * 1.20f, 2.74f, 4.75f),
                    new Vector3(side * 1.20f, 2.78f, 3.015f), true);
                CollectionTruckCabMesh.Quad(g, "RoofEdgeReturn", "Paint", new Vector3(side * 1.20f, 2.78f, 2.76f),
                    new Vector3(side * 1.20f, 2.74f, 4.71f), new Vector3(side * 1.245f, 2.74f, 4.71f),
                    new Vector3(side * 1.245f, 2.78f, 2.76f), true);
            }
            // Continuous crowned roof and radiused nose establish the silhouette instead of stacked cubes.
            CollectionTruckCabMesh.Surface(g, "CrownedCabRoof", "Paint", facets, 5, (u, v) =>
            {
                float x = Mathf.Lerp(-1.245f, 1.245f, u), a = Mathf.Abs(x / 1.245f);
                float z = Mathf.Lerp(2.74f, 4.79f - Mathf.Pow(a, 5) * .08f, v);
                float y = 2.78f + .047f * (1 - a * a) - .04f * v * v;
                return new Vector3(x, y, z);
            });
            CollectionTruckCabMesh.Surface(g, "FormedCabNose", "Paint", facets, 6, (u, v) =>
            {
                float a = Mathf.Lerp(-1, 1, u), y = Mathf.Lerp(.70f, 1.67f, v);
                float front = 5.10f + Mathf.Sin(v * Mathf.PI) * .042f - Mathf.Pow(Mathf.Abs(a), 6) * .065f;
                return new Vector3(a * 1.24f, y, front);
            }, true);
            CollectionTruckCabMesh.Quad(g, "CabBackPanel", "Paint", new Vector3(-1.20f, .90f, 2.76f),
                new Vector3(1.20f, .90f, 2.76f), new Vector3(1.20f, 2.77f, 2.76f), new Vector3(-1.20f, 2.77f, 2.76f), true);
            CollectionTruckCabMesh.Quad(g, "CabRoofFrontReturn", "Paint", new Vector3(-1.12f, 2.65f, 4.78f),
                new Vector3(1.12f, 2.65f, 4.78f), new Vector3(1.245f, 2.74f, 4.71f), new Vector3(-1.245f, 2.74f, 4.71f), true);
        }

        private static void Glazing(CollectionTruckGeometry g)
        {
            int segments = g.Detail == 0 ? 12 : 6;
            CollectionTruckCabMesh.Surface(g, "PanoramicWindscreen", "Glass", segments, 2, (u, v) =>
            {
                float a = Mathf.Lerp(-1, 1, u), halfWidth = Mathf.Lerp(1.095f, 1.06f, v);
                return new Vector3(a * halfWidth, Mathf.Lerp(1.73f, 2.62f, v),
                    Mathf.Lerp(5.078f, 4.793f, v) - Mathf.Pow(Mathf.Abs(a), 4) * .032f);
            }, true);
            Vector3[] frontOuter = { new(-1.135f, 1.695f, 5.053f), new(1.135f, 1.695f, 5.053f),
                new(1.105f, 2.657f, 4.764f), new(-1.105f, 2.657f, 4.764f) };
            Vector3[] frontInner = { new(-1.095f, 1.73f, 5.047f), new(1.095f, 1.73f, 5.047f),
                new(1.06f, 2.62f, 4.762f), new(-1.06f, 2.62f, 4.762f) };
            CollectionTruckCabMesh.Ring(g, "WindshieldRubberGasket", "Dark", frontOuter, frontInner);
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3[] outer = { new(side * 1.244f, 1.68f, 3.015f), new(side * 1.244f, 1.68f, 4.865f),
                    new(side * 1.204f, 2.665f, 4.70f), new(side * 1.204f, 2.695f, 3.015f) };
                Vector3[] inner = { new(side * 1.235f, 1.73f, 3.065f), new(side * 1.235f, 1.73f, 4.81f),
                    new(side * 1.195f, 2.615f, 4.655f), new(side * 1.195f, 2.645f, 3.065f) };
                CollectionTruckCabMesh.Ring(g, "DoorGlazingGasket", "Dark", outer, inner);
                CollectionTruckCabMesh.Quad(g, "RecessedDoorGlass", "Glass", inner[0], inner[1], inner[2], inner[3], true);
                if (g.Detail < 2)
                {
                    g.Bar("QuarterGlassDivision", new Vector3(side * 1.237f, 1.725f, 4.44f),
                        new Vector3(side * 1.197f, 2.621f, 4.40f), .018f, "Dark");
                    g.Bar("WiperArm", new Vector3(side * .73f, 1.70f, 5.085f), new Vector3(side * .61f, 1.93f, 5.030f), .013f, "Dark");
                    g.Bar("WiperBlade", new Vector3(side * .59f, 1.94f, 5.027f), new Vector3(side * .16f, 1.89f, 5.043f), .018f, "Dark");
                }
            }
        }

        private static void Front(CollectionTruckGeometry g)
        {
            int round = g.Detail == 0 ? 5 : 3;
            CollectionTruckCabMesh.FrontRounded(g, "MouldedBumper", new Vector3(0, .72f, 5.12f), new Vector3(2.60f, .29f, .24f), .075f, "Dark", round);
            CollectionTruckCabMesh.FrontRounded(g, "RecessedLowerAirIntake", new Vector3(0, .73f, 5.249f), new Vector3(1.18f, .13f, .020f), .023f, "Dark", round);
            CollectionTruckCabMesh.FrontRounded(g, "RecessedRadiatorGrille", new Vector3(0, 1.18f, 5.153f), new Vector3(1.24f, .45f, .030f), .035f, "Dark", round);
            CollectionTruckCabMesh.FrontRounded(g, "FleetPlate", new Vector3(0, .70f, 5.266f), new Vector3(.52f, .115f, .012f), .009f, "Paint", 2);
            for (int louver = 0; louver < (g.Detail == 2 ? 3 : 5); louver++)
            {
                float y = 1.005f + louver * (g.Detail == 2 ? .155f : .08f);
                CollectionTruckCabMesh.FrontRounded(g, "GrilleLouver", new Vector3(0, y, 5.176f), new Vector3(1.15f, .018f, .014f), .007f, "Steel", 2);
            }
            foreach (float side in new[] { -1f, 1f })
            {
                CollectionTruckCabMesh.FrontRounded(g, "RecessedHeadlampHousing", new Vector3(side * .895f, 1.17f, 5.125f),
                    new Vector3(.46f, .285f, .080f), .045f, "Dark", round);
                CollectionTruckCabMesh.FrontRounded(g, "HeadlampReflector", new Vector3(side * .895f, 1.17f, 5.169f),
                    new Vector3(.385f, .217f, .012f), .029f, "Chrome", round);
                CollectionTruckCabMesh.FrontRounded(g, "HeadlampLens", new Vector3(side * .895f, 1.17f, 5.179f),
                    new Vector3(.371f, .204f, .016f), .026f, "Lamp", round);
                CollectionTruckCabMesh.FrontRounded(g, "CornerTurnSignal", new Vector3(side * 1.159f, 1.17f, 5.132f),
                    new Vector3(.103f, .237f, .050f), .025f, "Amber", round);
                CollectionTruckCabMesh.FrontRounded(g, "LowerFoglamp", new Vector3(side * 1.07f, .72f, 5.249f),
                    new Vector3(.17f, .09f, .025f), .025f, "Lamp", round);
                if (g.Detail == 0)
                {
                    g.Bar("TowEye", new Vector3(side * .73f, .69f, 5.25f), new Vector3(side * .73f, .64f, 5.25f), .023f, "Steel");
                    for (int rib = 0; rib < 5; rib++)
                        g.Box("MouldedLensFlute", new Vector3(side * .895f - .135f + rib * .068f, 1.17f, 5.19f), new Vector3(.002f, .15f, .002f), "Glass");
                }
            }
        }

        private static void Doors(CollectionTruckGeometry g)
        {
            int segments = g.Detail == 0 ? 12 : 6;
            foreach (float side in new[] { -1f, 1f })
            {
                var shape = new List<Vector2> { new(1.65f, 3.04f), new(1.65f, 4.79f), new(1.18f, 4.74f) };
                for (int i = 0; i <= segments; i++)
                {
                    float angle = Mathf.Lerp(18, 162, i / (float)segments) * Mathf.Deg2Rad;
                    shape.Add(new Vector2(.48f + Mathf.Sin(angle) * .625f, 3.75f + Mathf.Cos(angle) * .625f));
                }
                shape.Add(new Vector2(.68f, 3.12f)); shape.Add(new Vector2(1.04f, 3.04f));
                CollectionTruckCabMesh.Side(g, "StampedDoorPanel", shape.ToArray(), side * 1.242f, side * .009f, "Paint");
                if (g.Detail < 2)
                {
                    for (int edge = 0; edge < shape.Count; edge++)
                    {
                        Vector2 a = shape[edge], b = shape[(edge + 1) % shape.Count];
                        g.Bar("DoorShutLine", new Vector3(side * 1.253f, a.x, a.y), new Vector3(side * 1.253f, b.x, b.y), .006f, "Dark");
                    }
                    g.Box("RecessedDoorHandle", new Vector3(side * 1.258f, 1.475f, 3.25f), new Vector3(.023f, .060f, .22f), "Dark");
                    g.Box("DoorPull", new Vector3(side * 1.277f, 1.487f, 3.25f), new Vector3(.014f, .023f, .16f), "Paint");
                    g.Bar("UpperMirrorArm", new Vector3(side * 1.21f, 2.38f, 4.56f), new Vector3(side * 1.50f, 2.38f, 4.34f), .024f, "Dark");
                    g.Bar("LowerMirrorArm", new Vector3(side * 1.25f, 1.83f, 4.69f), new Vector3(side * 1.50f, 1.89f, 4.34f), .024f, "Dark");
                }
                // A tall mirror case is radiused in the view plane and faces toward the rear of the truck.
                CollectionTruckCabMesh.FrontRounded(g, "MirrorCase", new Vector3(side * 1.515f, 2.135f, 4.33f),
                    new Vector3(.16f, .49f, .16f), .06f, "Dark", g.Detail == 0 ? 5 : 3);
                CollectionTruckCabMesh.FrontRounded(g, "MirrorFace", new Vector3(side * 1.515f, 2.135f, 4.243f),
                    new Vector3(.127f, .437f, .008f), .04f, "Chrome", g.Detail == 0 ? 5 : 3);
            }
        }

        private static void Interior(CollectionTruckGeometry g)
        {
            // Opaque geometry stays below the belt line; transparent glazing exposes a genuine cab interior.
            g.Box("CabRearFloor", new Vector3(0, .86f, 2.985f), new Vector3(2.33f, .07f, .45f), "Dark");
            g.Box("EngineCover", new Vector3(0, 1.12f, 3.735f), new Vector3(.80f, .14f, 1.12f), "Dark");
            g.Box("CabFootwell", new Vector3(0, 1.025f, 4.58f), new Vector3(2.22f, .075f, .73f), "Dark");
            if (g.Detail == 2) return;
            g.Profile("SlopedDashboard", new[] { new Vector2(1.48f, 4.49f), new Vector2(1.57f, 4.91f),
                new Vector2(1.68f, 4.89f), new Vector2(1.63f, 4.43f) }, 2.18f, 0, "Dark");
            foreach (float side in new[] { -1f, 1f })
            {
                CollectionTruckCabMesh.FrontRounded(g, "UpholsteredSeatBack", new Vector3(side * .63f, 1.66f, 3.105f),
                    new Vector3(.52f, .77f, .15f), .07f, "Seat", 4);
                g.Box("SeatCushion", new Vector3(side * .63f, 1.24f, 3.33f), new Vector3(.52f, .16f, .56f), "Seat");
                CollectionTruckCabMesh.FrontRounded(g, "SeatHeadrest", new Vector3(side * .63f, 2.115f, 3.105f),
                    new Vector3(.33f, .16f, .13f), .04f, "Dark", 3);
                g.Box("SeatBase", new Vector3(side * .63f, 1.095f, 3.32f), new Vector3(.35f, .15f, .39f), "Steel");
                if (g.Detail == 0)
                    g.Bar("SeatBelt", new Vector3(side * .96f, 2.04f, 3.015f), new Vector3(side * .45f, 1.25f, 3.38f), .018f, "Dark");
            }
            g.Bar("SteeringColumn", new Vector3(-.63f, 1.29f, 4.69f), new Vector3(-.63f, 1.63f, 4.30f), .062f, "Dark");
            int rim = g.Detail == 0 ? 24 : 12;
            Vector3 wheelCenter = new Vector3(-.63f, 1.70f, 4.31f);
            Vector3 across = Vector3.right, upward = new Vector3(0, .72f, .69f);
            for (int i = 0; i < rim; i++)
            {
                float a = i / (float)rim * Mathf.PI * 2, b = (i + 1) / (float)rim * Mathf.PI * 2;
                g.Bar("SteeringRim", wheelCenter + (across * Mathf.Cos(a) + upward * Mathf.Sin(a)) * .185f,
                    wheelCenter + (across * Mathf.Cos(b) + upward * Mathf.Sin(b)) * .185f, .026f, "Dark");
            }
            for (int spoke = 0; spoke < 3; spoke++)
            {
                float a = spoke * Mathf.PI * 2 / 3;
                g.Bar("SteeringSpoke", wheelCenter, wheelCenter + (across * Mathf.Cos(a) + upward * Mathf.Sin(a)) * .17f, .018f, "Dark");
            }
        }
    }
}
