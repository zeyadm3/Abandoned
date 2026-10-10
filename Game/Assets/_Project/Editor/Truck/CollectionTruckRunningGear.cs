using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Rounded dual rear tires, pressed steel hubs, ladder chassis, suspension and service hardware.</summary>
    public static class CollectionTruckRunningGear
    {
        public static void Build(CollectionTruckGeometry g)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                g.Box("ChassisLongitudinal", new Vector3(side * .82f, .22f, 1.05f), new Vector3(.15f, .21f, 7.37f), "Dark");
                g.Box("RearMudFlap", new Vector3(side * 1.26f, .30f, -2.16f), new Vector3(.55f, .46f, .035f), "Tire");
                Wheel(g, side, -1.64f, 1.475f, .29f);
                Wheel(g, side, -1.64f, 1.175f, .26f, false);
                Wheel(g, side, 4.05f, 1.28f, .32f);
                g.Box("RearSuspensionSpring", new Vector3(side * .86f, .24f, -1.65f), new Vector3(.11f, .05f, 1.02f), "Steel");
                if (g.Detail < 2)
                {
                    g.Box("FuelTankBracket", new Vector3(side * .93f, .24f, 1.39f), new Vector3(.045f, .39f, .66f), "Dark");
                    g.Box("WheelArchBackStop", new Vector3(side * 1.23f, .78f, 3.47f), new Vector3(.15f, .24f, .12f), "Dark");
                    for (int arc = 0; arc < 12; arc++)
                    {
                        float angle = arc * Mathf.PI / 11;
                        g.Box("FrontWheelArch", new Vector3(side * 1.275f, .47f + Mathf.Sin(angle) * .51f, 4.05f + Mathf.Cos(angle) * .51f),
                            new Vector3(.06f, .055f, .16f), "Dark", new Vector3(angle * Mathf.Rad2Deg, 0, 0));
                    }
                    g.Box("CargoWheelSprayGuard", new Vector3(side * 1.32f, 1.01f, -1.64f), new Vector3(.66f, .035f, 1.07f), "Dark");
                    g.Bar("ShockAbsorber", new Vector3(side * .92f, .16f, -1.56f), new Vector3(side * .88f, .38f, -1.15f), .065f, "Steel");
                    g.Box("BatteryServiceBox", new Vector3(side * 1.12f, .27f, .35f), new Vector3(.34f, .30f, .61f), "Dark");
                }
            }
            foreach (float z in new[] { -2.30f, -.90f, .65f, 2.12f, 3.31f, 4.68f })
                g.Box("ChassisCrossmember", new Vector3(0, .22f, z), new Vector3(1.72f, .14f, .13f), "Dark");
            foreach (float z in new[] { -1.64f, 4.05f })
                g.Cylinder("Axle", new Vector3(0, .47f, z), new Vector3(.105f, 2.57f, .105f), "Dark", new Vector3(0, 0, 90));
            g.Cylinder("FuelTank", new Vector3(-1.01f, .32f, 1.35f), new Vector3(.45f, 1.30f, .45f), "Steel", new Vector3(90, 0, 0));
            g.Cylinder("ExhaustSilencer", new Vector3(.96f, .27f, 1.33f), new Vector3(.24f, .85f, .24f), "Rust", new Vector3(90, 0, 0));
            if (g.Detail == 0)
            {
                g.Cylinder("RearDifferential", new Vector3(0, .42f, -1.64f), new Vector3(.28f, .38f, .28f), "Dark", new Vector3(0, 0, 90));
                g.Bar("DriveShaft", new Vector3(0, .35f, -1.64f), new Vector3(0, .37f, 3.44f), .085f, "Dark");
                g.Bar("ExhaustTailpipe", new Vector3(.96f, .27f, 1.30f), new Vector3(1.43f, .24f, 1.05f), .075f, "Rust");
                foreach (float side in new[] { -1f, 1f })
                    for (int strip = 0; strip < 4; strip++)
                        g.Box("RearLeafSpringLayer", new Vector3(side * .86f, .18f - strip * .018f, -1.65f),
                            new Vector3(.105f, .016f, .96f - strip * .14f), "Rust");
            }
        }

        private static void Wheel(CollectionTruckGeometry g, float side, float z, float x, float width, bool outer = true)
        {
            Vector3 at = new Vector3(side * x, .47f, z); g.Tire("CommercialTire", at, width);
            float face = side * (x + width * .38f);
            g.Cylinder("SteelWheelDish", new Vector3(face, .47f, z), new Vector3(.50f, .070f, .50f), "Steel", new Vector3(0, 0, 90));
            g.Cylinder("WheelHub", new Vector3(face + side * .034f, .47f, z), new Vector3(.19f, .085f, .19f), "Dark", new Vector3(0, 0, 90));
            if (g.Detail == 2 || !outer) return;
            int tread = g.Detail == 0 ? 28 : 14;
            for (int point = 0; point < tread; point++)
            {
                float angle = point * Mathf.PI * 2 / tread;
                g.Box("TreadBlock", at + new Vector3(0, Mathf.Sin(angle) * .466f, Mathf.Cos(angle) * .466f),
                    new Vector3(width * .74f, .018f, .057f), "Tire", new Vector3(-angle * Mathf.Rad2Deg, 0, 0));
            }
            for (int bolt = 0; bolt < 8; bolt++)
            {
                float angle = bolt * Mathf.PI * .25f;
                g.Cylinder("WheelLug", new Vector3(face + side * .047f, .47f + Mathf.Sin(angle) * .13f, z + Mathf.Cos(angle) * .13f),
                    new Vector3(.027f, .037f, .027f), "Chrome", new Vector3(0, 0, 90));
                g.Cylinder("RimVent", new Vector3(face + side * .039f, .47f + Mathf.Sin(angle + .19f) * .202f, z + Mathf.Cos(angle + .19f) * .202f),
                    new Vector3(.050f, .008f, .050f), "Dark", new Vector3(0, 0, 90));
            }
        }
    }
}
