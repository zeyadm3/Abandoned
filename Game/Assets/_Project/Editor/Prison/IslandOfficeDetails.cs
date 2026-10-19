using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;
using MeshKit = System.Collections.Generic.List<(UnityEngine.Mesh mesh, UnityEngine.Material material)>;

namespace Abandoned.EditorTools
{
    /// <summary>Fixed records, library and visiting-hall fittings: no loose replicas of collectible prison items.</summary>
    public static class IslandOfficeDetails
    {
        private const float Y = L.PlateauY;

        public static void Build(Transform interiors, IslandPrisonArt art)
        {
            Transform details = Group("RoomDetails", interiors);
            details.gameObject.AddComponent<WalkThroughObject>().EditorSetup("Wall-backed joinery and fitted equipment use the existing walls and furniture; fixed visiting seats have explicit small colliders.");
            Filing(details, interiors.Find("Admin"), art);
            DeadControlPanel(details, art);
            Notice(details, new Vector3(30.57f, Y + 2.25f, 46.8f), Quaternion.Euler(0f, -90f, 0f), art);
            Notice(details, new Vector3(71f, Y + 2.6f, 40.57f), Quaternion.Euler(0f, 180f, 0f), art);
            Visiting(details, art);
            Library(details, art);
        }

        private static void Filing(Transform g, Transform admin, IslandPrisonArt art)
        {
            MeshKit kit = IslandMeshKit.Get("Admin_FilingDrawerFaces", r =>
            {
                foreach (float y in new[] { .22f, .55f, .88f, 1.21f })
                {
                    Box("InsetDrawer", r, new Vector3(0f, y, -.307f), new Vector3(.445f, .28f, .025f), art.Railing, false);
                    Box("DrawerPull", r, new Vector3(0f, y + .035f, -.34f), new Vector3(.15f, .025f, .04f), art.Steel, false);
                    Box("CardHolder", r, new Vector3(0f, y - .065f, -.325f), new Vector3(.12f, .05f, .012f), art.Brass, false);
                    Box("FadedCard", r, new Vector3(0f, y - .065f, -.333f), new Vector3(.09f, .028f, .004f), art.Paper, false);
                }
                Box("ToeRecess", r, new Vector3(0f, .055f, -.307f), new Vector3(.45f, .07f, .012f), art.Bars, false);
            });
            foreach (Transform cabinet in admin)
                if (cabinet.name == "Filing")
                    IslandMeshKit.Place(kit, "FittedDrawerFaces", g, cabinet.position - Vector3.up * .7f, Quaternion.Euler(0f, -90f, 0f));
            // The gun rack remains empty: its fitted saddles and retaining rail cannot be mistaken for loose guns.
            MeshKit rack = IslandMeshKit.Get("Admin_EmptyRackHardware", r =>
            {
                foreach (float y in new[] { .45f, 1.35f })
                    Box("RetainingRail", r, new Vector3(0f, y, -.17f), new Vector3(2.3f, .06f, .08f), art.Steel, false);
                for (int i = 0; i < 7; i++)
                {
                    float x = -.96f + i * .32f;
                    Box("EmptySaddle", r, new Vector3(x, .42f, -.22f), new Vector3(.18f, .04f, .18f), art.Bars, false);
                    foreach (float dx in new[] { -.09f, .09f })
                        Box("RackProng", r, new Vector3(x + dx, 1.35f, -.23f), new Vector3(.025f, .08f, .15f), art.Rust, false);
                }
            });
            IslandMeshKit.Place(rack, "EmptyArmouryRack", g, new Vector3(30.7f, Y + .2f, 50.3f), Quaternion.Euler(0f, -90f, 0f));
        }

        private static void DeadControlPanel(Transform g, IslandPrisonArt art)
        {
            MeshKit kit = IslandMeshKit.Get("Admin_DeadControlFace", r =>
            {
                Box("PanelGasket", r, Vector3.zero, new Vector3(1.54f, .94f, .018f), art.Bars, false);
                foreach (float x in new[] { -.48f, 0f, .48f })
                {
                    GameObject bezel = Primitive(PrimitiveType.Cylinder, "InstrumentBezel", r, new Vector3(x, .2f, -.023f), new Vector3(.25f, .018f, .25f), art.Brass, false);
                    bezel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    GameObject face = Primitive(PrimitiveType.Cylinder, "DeadInstrument", r, new Vector3(x, .2f, -.044f), new Vector3(.2f, .005f, .2f), art.Paper, false);
                    face.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    K.RotBox("RestingNeedle", r, new Vector3(x - .022f, .18f, -.052f), new Vector3(.012f, .12f, .006f), Quaternion.Euler(0f, 0f, -55f), art.Bars, false);
                    Box("CircuitLegend", r, new Vector3(x, -.045f, -.025f), new Vector3(.27f, .06f, .008f), art.Canvas, false);
                    for (int i = 0; i < 3; i++)
                        Box("VacantSwitchSlot", r, new Vector3(x - .11f + i * .11f, -.22f, -.025f), new Vector3(.045f, .14f, .01f), art.FacadeDark, false);
                }
                foreach (float x in new[] { -.7f, .7f })
                foreach (float y in new[] { -.39f, .39f })
                    Box("PanelFastener", r, new Vector3(x, y, -.025f), new Vector3(.025f, .025f, .01f), art.Steel, false);
            });
            IslandMeshKit.Place(kit, "DeadTierControlInstruments", g, new Vector3(32.4f, Y + 1.3f, 51.615f), Quaternion.identity);
        }

        private static void Notice(Transform g, Vector3 at, Quaternion rotation, IslandPrisonArt art)
        {
            MeshKit kit = IslandMeshKit.Get("Interior_RemnantNoticeBoard", r =>
            {
                Box("Backing", r, Vector3.zero, new Vector3(1.4f, .8f, .035f), art.Board, false);
                foreach (float x in new[] { -.7f, .7f })
                    Box("Frame", r, new Vector3(x, 0f, -.025f), new Vector3(.045f, .86f, .06f), art.DarkWood, false);
                foreach (float y in new[] { -.41f, .41f })
                    Box("Frame", r, new Vector3(0f, y, -.025f), new Vector3(1.43f, .045f, .06f), art.DarkWood, false);
                for (int i = 0; i < 5; i++)
                {
                    float x = -.5f + i * .25f, y = -.03f + (i % 2) * .14f;
                    Box("OldPinHole", r, new Vector3(x, y + .17f, -.022f), new Vector3(.02f, .02f, .006f), art.Bars, false);
                    if (i == 1) continue;
                    K.RotBox("TornMountedPaper", r, new Vector3(x, y, -.026f), new Vector3(.21f, .25f - i * .018f, .008f), Quaternion.Euler(0f, 0f, i * 4f - 7f), art.Paper, false);
                    Box("Staple", r, new Vector3(x, y + .1f, -.035f), new Vector3(.035f, .007f, .006f), art.Steel, false);
                }
            });
            IslandMeshKit.Place(kit, "FadedWallNotices", g, at, rotation, false);
        }

        private static void Visiting(Transform g, IslandPrisonArt art)
        {
            MeshKit kit = IslandMeshKit.Get("VisitingBooth_FittedHardware", r =>
            {
                foreach (float x in new[] { -.575f, .575f })
                    Box("GlazingGasket", r, new Vector3(x, 1.3f, .026f), new Vector3(.035f, .91f, .025f), art.Bars, false);
                foreach (float y in new[] { .85f, 1.75f })
                    Box("GlazingEdge", r, new Vector3(0f, y, .026f), new Vector3(1.15f, .035f, .025f), art.Bars, false);
                Box("SpeakingPlate", r, new Vector3(.13f, 1.07f, .045f), new Vector3(.26f, .16f, .025f), art.Steel, false);
                for (int i = 0; i < 5; i++)
                    Box("SpeakingSlot", r, new Vector3(.045f + i * .043f, 1.07f, .061f), new Vector3(.016f, .08f, .006f), art.Bars, false);
                Box("HandsetCradle", r, new Vector3(-.32f, 1.28f, .58f), new Vector3(.15f, .3f, .025f), art.Steel, false);
                for (int i = 0; i < 9; i++)
                    Box("TetherCord", r, new Vector3(-.32f + (i % 2 == 0 ? -.012f : .012f), 1.06f - i * .025f, .63f), new Vector3(.035f, .035f, .018f), art.Bars, false);
                Box("CounterEdge", r, new Vector3(0f, .774f, .6f), new Vector3(1.16f, .07f, .035f), art.Railing, false);
                Primitive(PrimitiveType.Cylinder, "FixedSeat", r, new Vector3(0f, .46f, 1.05f), new Vector3(.36f, .025f, .36f), art.Railing, false);
                Primitive(PrimitiveType.Cylinder, "SeatPedestal", r, new Vector3(0f, .225f, 1.05f), new Vector3(.08f, .225f, .08f), art.Steel, false);
                Box("BoltedSeatBase", r, new Vector3(0f, .025f, 1.05f), new Vector3(.29f, .05f, .29f), art.Bars, false);
            });
            foreach (float x0 in new[] { 41f, 61f })
            for (int i = 0; i < 5; i++)
            {
                float x = x0 + .92f + i * 1.25f;
                Transform booth = IslandMeshKit.Place(kit, "FixedBoothHardware", g, new Vector3(x, Y, 41.2f), Quaternion.identity);
                IslandMeshKit.Solid(booth, new Vector3(0f, .24f, 1.05f), new Vector3(.38f, .48f, .38f));
            }
        }

        private static void Library(Transform g, IslandPrisonArt art)
        {
            MeshKit kit = IslandMeshKit.Get("Library_CardIndexJoinery", r =>
            {
                Box("MountedBackboard", r, Vector3.zero, new Vector3(1.55f, .58f, .045f), art.DarkWood, false);
                for (int row = 0; row < 3; row++)
                for (int col = 0; col < 5; col++)
                {
                    Vector3 at = new(-.61f + col * .305f, -.185f + row * .185f, -.035f);
                    Box("IndexDrawer", r, at, new Vector3(.28f, .16f, .03f), art.Wood, false);
                    Box("IndexCardFrame", r, at + new Vector3(0f, -.01f, -.021f), new Vector3(.13f, .055f, .009f), art.Brass, false);
                    Box("EmptyLabel", r, at + new Vector3(0f, -.01f, -.027f), new Vector3(.1f, .03f, .004f), art.Canvas, false);
                }
            });
            IslandMeshKit.Place(kit, "WallCardIndex", g, new Vector3(77.43f, Y + 1.7f, 43.45f), Quaternion.Euler(0f, 90f, 0f));
        }

        public static void Cage(Transform parent, float x0, float x1, float z0, float z1, IslandPrisonArt art)
        {
            Transform cage = Group("ControlRoomCage", parent);
            cage.gameObject.AddComponent<WalkThroughObject>().EditorSetup("Thin grille bars share fitted wall-panel collision; the open 1.3 m gate has no panel collider.");
            Segment(new Vector3(x0, Y, z0), new Vector3(32.55f, Y, z0));
            Segment(new Vector3(33.85f, Y, z0), new Vector3(x1, Y, z0));
            Segment(new Vector3(x1, Y, z0), new Vector3(x1, Y, z1));
            Box("GateLintel", cage, new Vector3(33.2f, Y + 2.4f, z0), new Vector3(1.3f, .4f, .055f), art.Bars, true);

            void Segment(Vector3 from, Vector3 to)
            {
                float length = Vector3.Distance(from, to);
                MeshKit panel = IslandMeshKit.Get($"ControlCage_{length:0.00}".Replace(',', '.'), r =>
                {
                    foreach (float x in new[] { .025f, length - .025f })
                        Box("CagePost", r, new Vector3(x, 1.3f, 0f), new Vector3(.045f, 2.6f, .055f), art.Steel, false);
                    foreach (float y in new[] { .06f, 1.05f, 2.55f })
                        Box("CrossRail", r, new Vector3(length * .5f, y, 0f), new Vector3(length, .045f, .055f), art.Steel, false);
                    for (float x = .13f; x < length - .08f; x += .14f)
                        Box("CageBar", r, new Vector3(x, 1.3f, 0f), new Vector3(.024f, 2.55f, .024f), art.Bars, false);
                });
                Quaternion facing = Quaternion.FromToRotation(Vector3.right, (to - from).normalized);
                Transform section = IslandMeshKit.Place(panel, "CagePanel", cage, from, facing);
                IslandMeshKit.Solid(section, new Vector3(length * .5f, 1.3f, 0f), new Vector3(length, 2.6f, .055f));
            }
        }
    }
}
