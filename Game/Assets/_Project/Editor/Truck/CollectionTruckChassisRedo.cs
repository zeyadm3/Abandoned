using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Commercial running gear: continuous profiled tyres, recessed pressed-steel wheels and a sprung ladder frame.</summary>
    public static class CollectionTruckChassisRedo
    {
        public static void Build(CollectionTruckGeometry g)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                g.Box("FrameRail", new Vector3(side * .73f, .83f, .80f), new Vector3(.10f, .18f, 6.75f), "Dark");
                Wheel(g, side, 3.75f, 1.16f, .28f, true);
                Wheel(g, side, -1.65f, 1.075f, .26f, false);
                Wheel(g, side, -1.65f, 1.355f, .26f, true);
                Springs(g, side, -1.65f);
                Springs(g, side, 3.75f);
                RearFender(g, side);
                g.Box("RubberSprayFlap", new Vector3(side * 1.235f, .40f, -2.22f), new Vector3(.49f, .66f, .021f), "Tire");
                g.Box("FlapRetainingStrip", new Vector3(side * 1.235f, .73f, -2.231f), new Vector3(.47f, .035f, .018f), "Steel");
                Steps(g, side);
                if (g.Detail < 2)
                {
                    g.Bar("RearDamper", new Vector3(side * .84f, .48f, -1.69f), new Vector3(side * .77f, .82f, -1.15f), .044f, "Dark");
                    g.Bar("FrontDamper", new Vector3(side * .86f, .49f, 3.66f), new Vector3(side * .76f, .86f, 3.25f), .040f, "Steel");
                    g.Box("SideGuardLowerRail", new Vector3(side * 1.20f, .48f, .75f), new Vector3(.042f, .055f, 3.08f), "Steel");
                    g.Box("SideGuardUpperRail", new Vector3(side * 1.20f, .65f, .75f), new Vector3(.042f, .050f, 3.08f), "Steel");
                    foreach (float z in new[] { -.50f, 1.95f })
                        g.Box("SideGuardBracket", new Vector3(side * 1.16f, .69f, z), new Vector3(.045f, .29f, .045f), "Dark");
                }
            }
            foreach (float z in new[] { -2.38f, -.85f, .60f, 2.05f, 3.82f })
                g.Box("FrameCrossmember", new Vector3(0, .82f, z), new Vector3(1.49f, .115f, .083f), "Dark");
            g.Bar("RearAxleBeam", new Vector3(-1.20f, .48f, -1.65f), new Vector3(1.20f, .48f, -1.65f), .095f, "Dark");
            g.Bar("FrontAxleBeam", new Vector3(-1.04f, .48f, 3.75f), new Vector3(1.04f, .48f, 3.75f), .078f, "Dark");
            g.Bar("SteeringTieRod", new Vector3(-.99f, .445f, 3.52f), new Vector3(.99f, .445f, 3.52f), .025f, "Steel");
            Tank(g);
            g.Box("BatteryServiceCase", new Vector3(1.01f, .59f, 1.10f), new Vector3(.36f, .40f, .66f), "Dark");
            g.Box("BatteryCaseLid", new Vector3(1.01f, .804f, 1.10f), new Vector3(.38f, .025f, .69f), "Steel");
            g.Cylinder("ExhaustSilencer", new Vector3(.94f, .56f, 2.08f), new Vector3(.21f, .73f, .21f), "Dark", new Vector3(90, 0, 0));
            if (g.Detail == 0)
            {
                Lathe(g, "DifferentialHousing", new Vector3(0, .48f, -1.65f), new[]
                {
                    new Vector2(-.19f, 0), new Vector2(-.19f, .08f), new Vector2(-.12f, .17f),
                    new Vector2(0, .205f), new Vector2(.12f, .17f), new Vector2(.19f, .08f), new Vector2(.19f, 0)
                }, 24, "Dark");
                g.Bar("DriveShaft", new Vector3(0, .54f, -1.65f), new Vector3(0, .67f, 3.19f), .069f, "Dark");
                g.Bar("ExhaustTailpipe", new Vector3(.94f, .56f, 1.74f), new Vector3(1.27f, .49f, 1.49f), .054f, "Dark");
                foreach (float side in new[] { -1f, 1f })
                {
                    g.Cylinder("SteeringKnuckle", new Vector3(side * 1.035f, .48f, 3.75f), new Vector3(.105f, .15f, .105f), "Dark");
                    foreach (float z in new[] { -2.38f, -.85f, .60f, 2.05f })
                        g.Box("BodyMountPad", new Vector3(side * .72f, .951f, z), new Vector3(.14f, .065f, .20f), "Tire");
                }
            }
        }

        private static void Wheel(CollectionTruckGeometry g, float side, float z, float x, float width, bool exposed)
        {
            Vector3 at = new Vector3(side * x, .48f, z);
            var tyre = new List<Vector2>(); float axialScale = width / .28f;
            foreach (Vector2 p in new[]
            {
                new Vector2(-.098f, .255f), new Vector2(-.128f, .281f), new Vector2(-.141f, .345f),
                new Vector2(-.139f, .414f), new Vector2(-.128f, .458f), new Vector2(-.105f, .478f),
                new Vector2(-.072f, .480f), new Vector2(-.068f, .473f), new Vector2(-.061f, .473f),
                new Vector2(-.057f, .480f), new Vector2(-.012f, .480f), new Vector2(-.008f, .473f),
                new Vector2(.008f, .473f), new Vector2(.012f, .480f), new Vector2(.057f, .480f),
                new Vector2(.061f, .473f), new Vector2(.068f, .473f), new Vector2(.072f, .480f),
                new Vector2(.105f, .478f), new Vector2(.128f, .458f), new Vector2(.139f, .414f),
                new Vector2(.141f, .345f), new Vector2(.128f, .281f), new Vector2(.098f, .255f)
            }) tyre.Add(new Vector2(p.x * axialScale, p.y));
            int segments = g.Detail == 0 ? 96 : g.Detail == 1 ? 40 : 24;
            Lathe(g, "ContinuousCommercialTyre", at, tyre.ToArray(), segments, "Tire", g.Detail == 0);

            // One connected metal section includes the bead seat, narrow lip and dished wheel face.
            Vector2[] rim =
            {
                new Vector2(.060f, 0), new Vector2(.062f, .071f), new Vector2(.101f, .102f),
                new Vector2(.086f, .157f), new Vector2(.060f, .213f), new Vector2(.074f, .242f),
                new Vector2(.116f, .255f), new Vector2(.122f, .266f), new Vector2(.116f, .277f),
                new Vector2(.079f, .277f), new Vector2(.058f, .263f), new Vector2(.023f, .254f),
                new Vector2(-.091f, .252f), new Vector2(-.098f, .267f), new Vector2(-.106f, .267f),
                new Vector2(-.109f, .248f), new Vector2(-.088f, .236f), new Vector2(.014f, .235f),
                new Vector2(.029f, .180f), new Vector2(.029f, .070f), new Vector2(.029f, 0)
            };
            System.Array.Reverse(rim);
            if (side < 0)
            {
                for (int i = 0; i < rim.Length; i++) rim[i].x = -rim[i].x;
                System.Array.Reverse(rim);
            }
            Lathe(g, "RecessedPressedSteelWheel", at, rim, g.Detail == 0 ? 64 : 32, "Steel");
            g.Cylinder("AxleHubCap", at + new Vector3(side * .109f, 0, 0),
                new Vector3(.116f, .045f, .116f), "Steel", new Vector3(0, 0, 90));
            if (g.Detail == 2 || !exposed) return;
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4;
                Vector3 lugAt = at + new Vector3(side * .105f, Mathf.Sin(angle) * .091f, Mathf.Cos(angle) * .091f);
                Lathe(g, "HexWheelLug", lugAt, new[]
                {
                    new Vector2(-.011f, 0), new Vector2(-.011f, .014f),
                    new Vector2(.011f, .014f), new Vector2(.012f, .010f), new Vector2(.012f, 0)
                }, 6, "Steel");
                float ventAngle = angle + Mathf.PI / 8;
                Vector3 ventAt = at + new Vector3(side * .069f, Mathf.Sin(ventAngle) * .191f, Mathf.Cos(ventAngle) * .191f);
                g.Cylinder("RecessedCoolingVent", ventAt, new Vector3(.051f, .005f, .040f), "Dark",
                    new Vector3(ventAngle * Mathf.Rad2Deg, 0, 90));
            }
        }

        private static void Springs(CollectionTruckGeometry g, float side, float z)
        {
            int leaves = g.Detail == 0 ? 4 : 2;
            for (int leaf = 0; leaf < leaves; leaf++)
            {
                float length = .65f - leaf * .105f, mid = .585f - leaf * .017f;
                var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
                for (int i = 0; i <= 6; i++)
                {
                    float offset = Mathf.Lerp(-length, length, i / 6f);
                    float y = mid + offset * offset * .29f, x = side * .73f;
                    vertices.AddRange(new[] { new Vector3(x - .039f, y, z + offset), new Vector3(x + .039f, y, z + offset),
                        new Vector3(x + .039f, y - .012f, z + offset), new Vector3(x - .039f, y - .012f, z + offset) });
                    for (int corner = 0; corner < 4; corner++) uv.Add(new Vector2(corner / 4f, i / 6f));
                    if (i == 6) continue;
                    for (int corner = 0; corner < 4; corner++)
                    {
                        int v = i * 4 + corner, next = i * 4 + (corner + 1) % 4;
                        triangles.AddRange(new[] { v, v + 4, next + 4, v, next + 4, next });
                    }
                }
                triangles.AddRange(new[] { 0, 1, 2, 0, 2, 3, 24, 26, 25, 24, 27, 26 });
                var mesh = new Mesh { name = "CurvedLeafSpring" };
                mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
                mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds(); g.Add("CurvedLeafSpring", mesh, "Dark");
            }
            foreach (float end in new[] { -.65f, .65f })
                g.Box("SpringHanger", new Vector3(side * .73f, .745f, z + end), new Vector3(.094f, .13f, .051f), "Dark");
            if (g.Detail == 2) return;
            foreach (float offset in new[] { -.068f, .068f })
                g.Bar("AxleSpringClamp", new Vector3(side * .73f + offset, .41f, z), new Vector3(side * .73f + offset, .632f, z), .017f, "Steel");
            g.Box("AxleClampPlate", new Vector3(side * .73f, .412f, z), new Vector3(.176f, .024f, .146f), "Dark");
        }

        private static void Steps(CollectionTruckGeometry g, float side)
        {
            foreach (float y in new[] { .35f, .61f })
            {
                g.Box("CabAccessStep", new Vector3(side * 1.23f, y, 2.975f), new Vector3(.255f, .040f, .47f), "Steel");
                if (g.Detail == 0)
                    for (int i = 0; i < 6; i++)
                        g.Box("StepGripSlot", new Vector3(side * 1.23f, y + .021f, 2.79f + i * .071f), new Vector3(.186f, .003f, .022f), "Dark");
            }
            g.Box("CabStepMount", new Vector3(side * 1.13f, .58f, 2.975f), new Vector3(.043f, .52f, .38f), "Dark");
        }

        private static void Tank(CollectionTruckGeometry g)
        {
            Vector3 center = new Vector3(-1.01f, .63f, 1.22f);
            g.Cylinder("AluminiumFuelTank", center, new Vector3(.43f, 1.26f, .43f), "Steel", new Vector3(90, 0, 0));
            if (g.Detail == 2) return;
            foreach (float z in new[] { .79f, 1.65f })
                Lathe(g, "FuelTankRetentionBand", new Vector3(center.x, center.y, z), new[]
                {
                    new Vector2(-.017f, .215f), new Vector2(-.017f, .222f),
                    new Vector2(.017f, .222f), new Vector2(.017f, .215f)
                }, 24, "Dark", false, Quaternion.Euler(0, 90, 0));
            g.Bar("FuelFillerNeck", new Vector3(-1.13f, .77f, 1.59f), new Vector3(-1.22f, .86f, 1.59f), .044f, "Steel");
            g.Cylinder("FuelFillerCap", new Vector3(-1.23f, .86f, 1.59f), new Vector3(.060f, .024f, .060f), "Dark", new Vector3(0, 0, 45));
        }

        private static void RearFender(CollectionTruckGeometry g, float side)
        {
            Lathe(g, "CurvedRearSprayWing", new Vector3(side * 1.215f, .48f, -1.65f), new[]
            {
                new Vector2(-.275f, .530f), new Vector2(-.275f, .544f),
                new Vector2(.275f, .544f), new Vector2(.275f, .530f)
            }, g.Detail == 0 ? 32 : 16, "Dark", false, default, Mathf.PI);
        }

        private static void Lathe(CollectionTruckGeometry g, string name, Vector3 at, Vector2[] profile, int segments,
            string finish, bool tread = false, Quaternion rotation = default, float arc = Mathf.PI * 2)
        {
            if (rotation == default) rotation = Quaternion.identity;
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            int count = profile.Length;
            for (int segment = 0; segment <= segments; segment++)
            {
                float angle = arc * segment / segments;
                for (int p = 0; p < count; p++)
                {
                    Vector2 current = profile[p], tangent = profile[(p + 1) % count] - profile[(p + count - 1) % count];
                    float radius = current.y;
                    // Shallow staggered grooves are cut into the continuous tread surface, never placed as blocks.
                    if (tread && radius > .474f && (segment + p / 4) % 6 == 0) radius -= .0025f;
                    vertices.Add(at + rotation * new Vector3(current.x, Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius));
                    normals.Add(rotation * new Vector3(-tangent.y, tangent.x * Mathf.Sin(angle), tangent.x * Mathf.Cos(angle)).normalized);
                    uv.Add(new Vector2(segment / (float)segments * 4, p / (float)count));
                    if (segment == segments) continue;
                    int v = segment * count + p, next = segment * count + (p + 1) % count;
                    triangles.AddRange(new[] { v, next, next + count, v, next + count, v + count });
                }
            }
            if (arc < Mathf.PI * 2 - .01f)
                for (int p = 1; p < count - 1; p++)
                {
                    triangles.AddRange(new[] { 0, p + 1, p });
                    int end = segments * count; triangles.AddRange(new[] { end, end + p, end + p + 1 });
                }
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents(); mesh.RecalculateBounds(); g.Add(name, mesh, finish);
        }
    }
}
