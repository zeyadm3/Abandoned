using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Commercial sandwich-panel box body: broad painted faces, fitted extrusions and a practical plywood loading bay.</summary>
    public static class CollectionTruckCargoRedo
    {
        public static void Build(CollectionTruckGeometry g)
        {
            Shell(g);
            LoadingBay(g);
            RearFrame(g);
        }

        private static void Shell(CollectionTruckGeometry g)
        {
            g.Box("CargoRoof", new Vector3(0, 2.695f, 0), new Vector3(2.82f, .11f, 5.22f), "Paint");
            g.Box("CargoFrontPanel", new Vector3(0, 1.51f, 2.57f), new Vector3(2.78f, 2.22f, .09f), "Paint");
            foreach (float side in new[] { -1f, 1f })
            {
                // Large flat sheet faces make the body read as a working box truck rather than a ribbed shipping container.
                g.Box("CargoSidePanel", new Vector3(side * 1.3575f, 1.505f, 0), new Vector3(.065f, 2.21f, 5.18f), "Paint");
                g.Box("UpperEdgeExtrusion", new Vector3(side * 1.395f, 2.635f, 0), new Vector3(.043f, .075f, 5.22f), "Steel");
                g.Box("LowerBodyExtrusion", new Vector3(side * 1.388f, .434f, 0), new Vector3(.034f, .065f, 5.20f), "Steel");
                g.Box("RubberScuffRail", new Vector3(side * 1.406f, .56f, 0), new Vector3(.042f, .085f, 5.17f), "Dark");
                foreach (float z in new[] { -2.56f, 2.56f })
                    g.Box("BoxCornerCap", new Vector3(side * 1.392f, 1.52f, z), new Vector3(.034f, 2.29f, .085f), "Steel");
                if (g.Detail < 2)
                {
                    // Two very narrow bonded-sheet joints are sufficient; the wear lives in the paint maps.
                    foreach (float z in new[] { -1.05f, 1.08f })
                        g.Box("BondedSheetJoint", new Vector3(side * 1.3908f, 1.555f, z), new Vector3(.002f, 1.93f, .006f), "Steel");
                    foreach (float z in new[] { -2.05f, 1.90f })
                    {
                        g.Box("SideMarkerMount", new Vector3(side * 1.433f, .58f, z), new Vector3(.025f, .085f, .145f), "Dark");
                        g.Box("SideMarkerLens", new Vector3(side * 1.448f, .58f, z), new Vector3(.014f, .054f, .112f), "Amber");
                    }
                }
            }
            if (g.Detail == 0)
                g.Box("RoofPanelLap", new Vector3(0, 2.751f, .63f), new Vector3(2.69f, .002f, .012f), "Steel");
        }

        private static void LoadingBay(CollectionTruckGeometry g)
        {
            g.Box("PlywoodCargoDeck", new Vector3(0, .37f, 0), new Vector3(2.60f, .06f, 5.02f), "Deck");
            g.Box("CargoFrontLining", new Vector3(0, 1.51f, 2.516f), new Vector3(2.596f, 2.12f, .016f), "CargoLining");
            g.Box("FrontKickPlate", new Vector3(0, .515f, 2.499f), new Vector3(2.594f, .23f, .015f), "Steel");
            foreach (float side in new[] { -1f, 1f })
            {
                g.Box("CargoWallLining", new Vector3(side * 1.304f, 1.51f, 0), new Vector3(.012f, 2.12f, 5.00f), "CargoLining");
                g.Box("InteriorWallKickPlate", new Vector3(side * 1.288f, .515f, 0), new Vector3(.018f, .23f, 4.99f), "Steel");
                g.Box("RecessedLoadRestraintRail", new Vector3(side * 1.291f, 1.235f, .02f), new Vector3(.012f, .045f, 4.87f), "Steel");
                if (g.Detail < 2)
                {
                    for (int point = 0; point < 6; point++)
                        g.Box("LashingRailSlot", new Vector3(side * 1.282f, 1.235f, -2.08f + point * .83f), new Vector3(.004f, .022f, .063f), "Dark");
                    foreach (float z in new[] { -1.90f, 1.90f })
                    {
                        g.Box("AnchorPlate", new Vector3(side * 1.284f, .76f, z), new Vector3(.018f, .13f, .10f), "Steel");
                        g.Bar("AnchorLoopTop", new Vector3(side * 1.245f, .802f, z - .033f), new Vector3(side * 1.245f, .802f, z + .033f), .018f, "Chrome");
                        g.Bar("AnchorLoopFront", new Vector3(side * 1.245f, .802f, z + .033f), new Vector3(side * 1.281f, .724f, z + .033f), .018f, "Chrome");
                        g.Bar("AnchorLoopRear", new Vector3(side * 1.245f, .802f, z - .033f), new Vector3(side * 1.281f, .724f, z - .033f), .018f, "Chrome");
                    }
                }
            }
            g.Box("CargoCeilingLining", new Vector3(0, 2.633f, 0), new Vector3(2.59f, .014f, 4.99f), "CargoLining");
            if (g.Detail < 2)
            {
                foreach (float z in new[] { -1.52f, 1.18f })
                {
                    g.Box("CargoLampMount", new Vector3(0, 2.615f, z), new Vector3(.44f, .028f, .12f), "Steel");
                    g.Box("CargoLampDiffuser", new Vector3(0, 2.595f, z), new Vector3(.37f, .014f, .082f), "Lamp");
                }
            }
            if (g.Detail == 0)
                foreach (float x in new[] { -.78f, -.26f, .26f, .78f })
                    g.Box("DeckSheetJoint", new Vector3(x, .401f, 0), new Vector3(.004f, .002f, 4.93f), "Dark");
        }

        private static void RearFrame(CollectionTruckGeometry g)
        {
            g.Box("LoadingThreshold", new Vector3(0, .371f, -2.546f), new Vector3(2.70f, .058f, .16f), "Steel");
            g.Box("RearOpeningHeader", new Vector3(0, 2.63f, -2.574f), new Vector3(2.81f, .13f, .105f), "Steel");
            g.Box("HeaderWeatherSeal", new Vector3(0, 2.570f, -2.564f), new Vector3(2.604f, .022f, .027f), "Dark");
            foreach (float side in new[] { -1f, 1f })
            {
                g.Box("RearOpeningPost", new Vector3(side * 1.352f, 1.50f, -2.574f), new Vector3(.092f, 2.20f, .105f), "Steel");
                g.Box("DoorJambSeal", new Vector3(side * 1.300f, 1.50f, -2.565f), new Vector3(.020f, 2.18f, .026f), "Dark");
                g.Box("LoadingGrabHandleTopMount", new Vector3(side * 1.396f, 1.70f, -2.638f), new Vector3(.053f, .035f, .046f), "Steel");
                g.Box("LoadingGrabHandleLowerMount", new Vector3(side * 1.396f, 1.24f, -2.638f), new Vector3(.053f, .035f, .046f), "Steel");
                g.Bar("LoadingGrabHandle", new Vector3(side * 1.396f, 1.24f, -2.675f), new Vector3(side * 1.396f, 1.70f, -2.675f), .026f, "Chrome");
                g.Box("RearLampHousing", new Vector3(side * 1.08f, .28f, -2.66f), new Vector3(.36f, .15f, .061f), "Dark");
                g.Box("RearStopLens", new Vector3(side * 1.012f, .28f, -2.696f), new Vector3(.155f, .113f, .016f), "Red");
                g.Box("RearIndicatorLens", new Vector3(side * 1.18f, .28f, -2.696f), new Vector3(.10f, .113f, .016f), "Amber");
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
            hinge.localPosition = new Vector3(side * 1.35f, .40f, -2.55f);
            var lods = new LOD[3];
            for (int level = 0; level < 3; level++)
            {
                Transform source = GreyboxFactory.Group("CargoDoorSource", hinge);
                var g = new CollectionTruckGeometry(source, level);
                float mid = -side * .665f;
                g.Box("SandwichDoorPanel", new Vector3(mid, 1.10f, -.015f), new Vector3(1.30f, 2.20f, .065f), "Paint");
                g.Box("DoorInnerLining", new Vector3(mid, 1.10f, .023f), new Vector3(1.22f, 2.10f, .010f), "CargoLining");
                foreach (float y in new[] { .025f, 2.175f })
                    g.Box("DoorHorizontalFrame", new Vector3(mid, y, -.013f), new Vector3(1.31f, .045f, .078f), "Steel");
                foreach (float x in new[] { mid - .637f, mid + .637f })
                    g.Box("DoorVerticalFrame", new Vector3(x, 1.10f, -.013f), new Vector3(.027f, 2.20f, .078f), "Steel");
                g.Box("DoorMeetingSeal", new Vector3(-side * 1.325f, 1.10f, .010f), new Vector3(.055f, 2.20f, .045f), "Dark");
                g.Box("DoorInnerKickPlate", new Vector3(mid, .17f, .033f), new Vector3(1.22f, .24f, .015f), "Steel");
                Hardware(g, side, mid);
                Renderer[] renderers = g.SaveCombined(hinge,
                    (side < 0 ? "CargoRedoDoorLeft_LOD" : "CargoRedoDoorRight_LOD") + level);
                Object.DestroyImmediate(source.gameObject);
                lods[level] = new LOD(level == 0 ? .15f : level == 1 ? .055f : .006f, renderers);
            }
            var group = hinge.gameObject.AddComponent<LODGroup>();
            group.SetLODs(lods); group.RecalculateBounds();
            hinge.localRotation = Quaternion.Euler(0, side < 0 ? 105f : -105f, 0);
            foreach (Transform child in hinge.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, 0);
            return hinge;
        }

        private static void Hardware(CollectionTruckGeometry g, float side, float mid)
        {
            float rodX = mid - side * .39f;
            g.Bar("DoorLockingRod", new Vector3(rodX, .13f, -.079f), new Vector3(rodX, 2.08f, -.079f), .024f, "Chrome");
            g.Box("DoorLockHandle", new Vector3(rodX + side * .105f, .965f, -.100f), new Vector3(.215f, .035f, .026f), "Dark");
            foreach (float y in new[] { .18f, 1.10f, 1.96f })
            {
                g.Box("DoorHingeStrap", new Vector3(-side * .12f, y, -.063f), new Vector3(.26f, .084f, .020f), "Steel");
                g.Cylinder("DoorHingePin", new Vector3(0, y, -.084f), new Vector3(.052f, .14f, .052f), "Steel");
                if (g.Detail < 2)
                {
                    g.Box("LockRodGuide", new Vector3(rodX, y, -.072f), new Vector3(.075f, .07f, .045f), "Steel");
                    g.Cylinder("HingePinCap", new Vector3(0, y + .078f, -.084f), new Vector3(.060f, .017f, .060f), "Chrome");
                }
            }
            if (g.Detail == 0)
            {
                g.Box("HandleRetainer", new Vector3(rodX + side * .192f, .965f, -.067f), new Vector3(.054f, .083f, .043f), "Steel");
                foreach (float y in new[] { .13f, 2.08f })
                    g.Box("LockCamKeeper", new Vector3(rodX, y, -.071f), new Vector3(.14f, .066f, .044f), "Steel");
            }
        }
    }
}
