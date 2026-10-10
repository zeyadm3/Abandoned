using Abandoned.Extraction;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Fitted cab, weatherproof box body and visible loading equipment around the established low deck.</summary>
    public static class CollectionTruckBody
    {
        public static void Build(CollectionTruckGeometry g)
        {
            Cargo(g); Cab(g);
        }

        private static void Cargo(CollectionTruckGeometry g)
        {
            g.Box("CargoDeck", new Vector3(0, .385f, 0), new Vector3(2.62f, .05f, 5.02f), "Steel");
            g.Box("BoxRoof", new Vector3(0, 2.70f, 0), new Vector3(2.83f, .10f, 5.15f), "Paint");
            g.Box("BoxBulkhead", new Vector3(0, 1.50f, 2.555f), new Vector3(2.79f, 2.20f, .08f), "Paint");
            foreach (float side in new[] { -1f, 1f })
            {
                g.Box("CargoSkin", new Vector3(side * 1.385f, 1.51f, 0), new Vector3(.07f, 2.21f, 5.06f), "Paint");
                g.Box("BoxLowerRubRail", new Vector3(side * 1.436f, .51f, 0), new Vector3(.07f, .10f, 5.14f), "Dark");
                g.Box("BoxUpperExtrusion", new Vector3(side * 1.431f, 2.64f, 0), new Vector3(.06f, .07f, 5.14f), "Chrome");
                g.Box("CargoInteriorKickPlate", new Vector3(side * 1.288f, .57f, 0), new Vector3(.016f, .34f, 4.98f), "Steel");
                g.Box("CargoTieDownRail", new Vector3(side * 1.281f, 1.13f, 0), new Vector3(.025f, .055f, 4.88f), "Dark");
                g.Box("CargoHighTieDownRail", new Vector3(side * 1.281f, 1.83f, 0), new Vector3(.025f, .055f, 4.88f), "Dark");
                foreach (float z in new[] { -2.51f, 2.51f })
                    g.Box("CornerExtrusion", new Vector3(side * 1.427f, 1.50f, z), new Vector3(.06f, 2.29f, .07f), "Chrome");
                if (g.Detail < 2)
                {
                    for (int rib = 0; rib < 25; rib++)
                    {
                        float z = -2.36f + rib * .195f;
                        g.Box("PressedPanelRib", new Vector3(side * 1.43f, 1.51f, z), new Vector3(.011f, 2.08f, .012f), "Paint");
                    }
                    for (int point = 0; point < 8; point++)
                    {
                        float z = -2.15f + point * .60f;
                        g.Box("InteriorLashingSlot", new Vector3(side * 1.266f, 1.13f, z), new Vector3(.018f, .025f, .08f), "Chrome");
                        g.Box("ReflectiveSideMarker", new Vector3(side * 1.485f, .68f, z), new Vector3(.015f, .06f, .13f), "Amber");
                    }
                }
            }
            g.Box("RearThreshold", new Vector3(0, .43f, -2.52f), new Vector3(2.72f, .06f, .15f), "Steel");
            g.Box("RearHeader", new Vector3(0, 2.62f, -2.57f), new Vector3(2.85f, .16f, .12f), "Dark");
            g.Box("RearUnderrunBeam", new Vector3(0, .20f, -2.61f), new Vector3(2.75f, .12f, .13f), "Steel");
            foreach (float side in new[] { -1f, 1f })
            {
                g.Box("RearCombinationLight", new Vector3(side * 1.10f, .30f, -2.686f), new Vector3(.30f, .14f, .035f), "Red");
                g.Box("RearIndicatorLens", new Vector3(side * 1.27f, .30f, -2.69f), new Vector3(.095f, .14f, .04f), "Amber");
                g.Box("LoadingGrip", new Vector3(side * 1.42f, 1.44f, -2.63f), new Vector3(.04f, .58f, .05f), "Chrome");
            }
            if (g.Detail == 0)
            {
                for (int line = 0; line < 18; line++)
                    g.Box("DeckGripStrip", new Vector3(0, .417f, -2.30f + line * .27f), new Vector3(2.43f, .006f, .022f), "Dark");
                for (int panel = 0; panel < 4; panel++)
                    g.Box("RoofSheetJoint", new Vector3(0, 2.754f, -1.75f + panel * 1.16f), new Vector3(2.65f, .007f, .032f), "Steel");
            }
        }

        private static void Cab(CollectionTruckGeometry g)
        {
            g.Profile("CabLowerShell", new[] { new Vector2(.60f, 2.66f), new Vector2(.60f, 5.10f),
                new Vector2(1.42f, 5.10f), new Vector2(1.60f, 4.95f), new Vector2(1.60f, 2.66f) }, 2.46f, 0, "Paint");
            g.Box("CabRearWall", new Vector3(0, 2.05f, 2.77f), new Vector3(2.45f, 1.45f, .12f), "Paint");
            g.Profile("CabRoof", new[] { new Vector2(2.69f, 2.67f), new Vector2(2.69f, 4.83f),
                new Vector2(2.79f, 4.78f), new Vector2(2.84f, 3.05f), new Vector2(2.82f, 2.67f) }, 2.53f, 0, "Paint");
            g.Box("Windshield", new Vector3(0, 2.13f, 4.964f), new Vector3(2.16f, 1.07f, .035f), "Glass", new Vector3(-8, 0, 0));
            g.Box("WindscreenLowerSeal", new Vector3(0, 1.60f, 5.05f), new Vector3(2.38f, .065f, .05f), "Dark");
            g.Box("FrontBumper", new Vector3(0, .66f, 5.13f), new Vector3(2.65f, .23f, .19f), "Dark");
            g.Box("FrontGrilleFrame", new Vector3(0, 1.03f, 5.122f), new Vector3(1.34f, .43f, .05f), "Dark");
            g.Box("LicensePlate", new Vector3(0, .71f, 5.18f), new Vector3(.58f, .14f, .012f), "Paint");
            foreach (float side in new[] { -1f, 1f })
            {
                g.Profile("WindshieldPillar", new[] { new Vector2(1.54f, 5.06f), new Vector2(2.73f, 4.86f),
                    new Vector2(2.73f, 4.72f), new Vector2(1.54f, 4.90f) }, .11f, side * 1.18f, "Paint");
                g.Box("SideWindow", new Vector3(side * 1.237f, 2.14f, 3.93f), new Vector3(.025f, 1.04f, 1.55f), "Glass");
                g.Box("DoorRearPillar", new Vector3(side * 1.24f, 2.08f, 3.07f), new Vector3(.075f, 1.23f, .14f), "Paint");
                g.Box("WindowDividingPost", new Vector3(side * 1.254f, 2.16f, 4.36f), new Vector3(.035f, 1.06f, .033f), "Dark");
                g.Box("DoorWindowSeal", new Vector3(side * 1.261f, 1.60f, 3.91f), new Vector3(.035f, .06f, 1.85f), "Dark");
                g.Box("HeadlightHousing", new Vector3(side * .89f, 1.17f, 5.11f), new Vector3(.43f, .29f, .08f), "Dark");
                g.Box("HeadlightLens", new Vector3(side * .89f, 1.17f, 5.157f), new Vector3(.36f, .23f, .022f), "Lamp");
                g.Box("TurnIndicator", new Vector3(side * 1.19f, 1.19f, 5.142f), new Vector3(.12f, .21f, .04f), "Amber");
                g.Box("CabSideStep", new Vector3(side * 1.22f, .45f, 3.10f), new Vector3(.31f, .07f, .67f), "Steel");
                g.Box("MirrorHousing", new Vector3(side * 1.57f, 2.13f, 4.29f), new Vector3(.10f, .44f, .25f), "Dark");
                g.Box("MirrorGlass", new Vector3(side * 1.583f, 2.13f, 4.14f), new Vector3(.082f, .37f, .018f), "Chrome");
                if (g.Detail < 2)
                {
                    g.Bar("MirrorArm", new Vector3(side * 1.23f, 1.77f, 4.51f), new Vector3(side * 1.58f, 1.94f, 4.32f), .035f, "Dark");
                    g.Bar("UpperMirrorArm", new Vector3(side * 1.23f, 2.40f, 4.51f), new Vector3(side * 1.58f, 2.34f, 4.32f), .03f, "Dark");
                    g.Box("DoorHandle", new Vector3(side * 1.256f, 1.35f, 3.38f), new Vector3(.04f, .043f, .22f), "Dark");
                    g.Box("CabDoorGap", new Vector3(side * 1.235f, 1.08f, 3.07f), new Vector3(.02f, .97f, .012f), "Dark");
                    g.Box("DriverSeatCushion", new Vector3(side * .64f, 1.15f, 3.32f), new Vector3(.58f, .20f, .58f), "Seat");
                    g.Box("DriverSeatBack", new Vector3(side * .64f, 1.66f, 3.01f), new Vector3(.58f, .81f, .18f), "Seat", new Vector3(-8, 0, 0));
                    g.Box("SeatHeadrest", new Vector3(side * .64f, 2.17f, 3.01f), new Vector3(.40f, .20f, .14f), "Dark");
                    g.Bar("WindshieldWiper", new Vector3(side * .65f, 1.66f, 5.07f), new Vector3(side * .46f, 2.05f, 5.002f), .023f, "Dark");
                }
            }
            if (g.Detail < 2)
            {
                g.Box("Dashboard", new Vector3(0, 1.56f, 4.56f), new Vector3(2.25f, .13f, .42f), "Dark");
                g.Cylinder("SteeringWheel", new Vector3(-.62f, 1.65f, 4.27f), new Vector3(.40f, .028f, .40f), "Dark", new Vector3(35, 0, 0));
                for (int slot = 0; slot < 5; slot++)
                    g.Box("GrilleLouver", new Vector3(0, .86f + slot * .08f, 5.157f), new Vector3(1.23f, .033f, .018f), "Chrome");
            }
        }

        public static void Doors(Transform art, TruckSanctuary sanctuary)
        {
            Transform left = Door(art, -1f), right = Door(art, 1f);
            sanctuary?.EditorSetupRearDoors(left, right, 0f, 0f);
        }

        private static Transform Door(Transform art, float side)
        {
            Transform hinge = GreyboxFactory.Group(side < 0 ? "RearDoorHinge_L" : "RearDoorHinge_R", art);
            hinge.localPosition = new Vector3(side * 1.37f, .40f, -2.55f);
            var lods = new LOD[3];
            for (int level = 0; level < 3; level++)
            {
                Transform source = GreyboxFactory.Group("DoorSource", hinge);
                var g = new CollectionTruckGeometry(source, level); float mid = -side * .665f;
                g.Box("RearDoorPanel", new Vector3(mid, 1.10f, 0), new Vector3(1.32f, 2.20f, .055f), "Paint");
                g.Box("DoorLowerKickPlate", new Vector3(mid, .22f, -.033f), new Vector3(1.25f, .37f, .020f), "Steel");
                foreach (float y in new[] { .045f, 2.155f })
                    g.Box("DoorEdgeExtrusion", new Vector3(mid, y, 0), new Vector3(1.33f, .06f, .085f), "Chrome");
                if (level < 2)
                {
                    g.Bar("DoorLockRod", new Vector3(mid - side * .24f, .14f, -.068f), new Vector3(mid - side * .24f, 2.07f, -.068f), .034f, "Steel");
                    g.Box("LockHandle", new Vector3(mid - side * .20f, .89f, -.090f), new Vector3(.28f, .035f, .035f), "Dark");
                    for (int rib = 0; rib < 7; rib++)
                        g.Box("DoorPanelPressing", new Vector3(-side * (.09f + rib * .18f), 1.10f, -.037f), new Vector3(.010f, 2.03f, .011f), "Paint");
                    foreach (float y in new[] { .22f, 1.10f, 1.94f })
                        g.Box("RearHingeHardware", new Vector3(0, y, -.06f), new Vector3(.17f, .10f, .10f), "Steel");
                }
                Renderer[] renderers = g.SaveCombined(hinge, side < 0 ? "Truck_DoorL_LOD" + level : "Truck_DoorR_LOD" + level);
                Object.DestroyImmediate(source.gameObject);
                lods[level] = new LOD(level == 0 ? .15f : level == 1 ? .055f : .006f, renderers);
            }
            var group = hinge.gameObject.AddComponent<LODGroup>(); group.SetLODs(lods); group.RecalculateBounds();
            hinge.localRotation = Quaternion.Euler(0, side < 0 ? 105f : -105f, 0);
            return hinge;
        }
    }
}
