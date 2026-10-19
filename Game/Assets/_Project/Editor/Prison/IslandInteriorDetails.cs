using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;
using MeshKit = System.Collections.Generic.List<(UnityEngine.Mesh mesh, UnityEngine.Material material)>;

namespace Abandoned.EditorTools
{
    /// <summary>Fitted room equipment: shared detailed meshes sit on their original solid furniture.</summary>
    public static class IslandInteriorDetails
    {
        private const float Y = L.PlateauY;

        public static void Build(Transform interiors, IslandPrisonArt art)
        {
            Dining(interiors.Find("DiningHall"), art);
            Kitchen(interiors.Find("Kitchen"), art);
            IslandOfficeDetails.Build(interiors, art);
        }

        private static Transform Mounted(MeshKit kit, string name, Transform solid, Vector3 at, Quaternion rotation)
        {
            // Counter/range bodies are scaled primitives. Build in world units before parenting, otherwise their
            // nonuniform scale stretches every lip, dial and hood baffle a second time.
            Transform fitted = IslandMeshKit.Place(kit, name, null, at, rotation);
            fitted.SetParent(solid, true);
            fitted.gameObject.AddComponent<WalkThroughObject>().EditorSetup("Thin fixed fittings use the supporting furniture's existing solid footprint.");
            return fitted;
        }

        private static void Dining(Transform room, IslandPrisonArt art)
        {
            MeshKit table = IslandMeshKit.Get("Dining_TableWelds3", g =>
            {
                foreach (float side in new[] { -.405f, .405f })
                    Box("FoldedTopEdge", g, new Vector3(side, .728f, 0f), new Vector3(.025f, .08f, 3.03f), art.Railing, false);
                foreach (float z in new[] { -1.11f, 1.11f })
                {
                    Box("FootPlate", g, new Vector3(0f, .018f, z), new Vector3(.51f, .035f, .33f), art.Bars, false);
                    foreach (float x in new[] { -.18f, .18f })
                        Primitive(PrimitiveType.Cylinder, "AnchorBolt", g, new Vector3(x, .044f, z), new Vector3(.055f, .012f, .055f), art.Steel, false);
                }
                for (float z = -1.1f; z < 1.21f; z += .75f)
                foreach (float x in new[] { -.6f, .6f })
                {
                    Box("SeatEdge", g, new Vector3(x, .438f, z), new Vector3(.31f, .025f, .31f), art.Railing, false);
                    Box("SeatBracket", g, new Vector3(x * .65f, .26f, z), new Vector3(.4f, .1f, .055f), art.Bars, false);
                    Primitive(PrimitiveType.Cylinder, "SeatFixing", g, new Vector3(x, .486f, z), new Vector3(.035f, .006f, .035f), art.Steel, false);
                }
                for (int i = 0; i < 5; i++)
                    K.RotBox("ScoredTop", g, new Vector3(-.24f + i * .11f, .787f, -.85f + i * .36f),
                        new Vector3(.008f, .003f, .18f), Quaternion.Euler(0f, 24f + i * 17f, 0f), art.Bars, false);
            });
            // The overturning pose is inherited from the original solid table, including its tilted fittings.
            foreach (Transform furnishing in room)
                if (furnishing.name.StartsWith("Table_")) Mounted(table, "WeldedDetails", furnishing, furnishing.position, furnishing.rotation);

            Transform counter = room.Find("ServingCounter");
            MeshKit serving = IslandMeshKit.Get("Dining_ServingWellDetails", g =>
            {
                Box("RolledCounterLip", g, new Vector3(-.515f, .89f, 0f), new Vector3(.045f, .09f, 5.03f), art.Steel, false);
                Box("KickRecess", g, new Vector3(-.505f, .09f, 0f), new Vector3(.018f, .15f, 4.9f), art.Bars, false);
                foreach (float z in new[] { -1.8f, -.6f, .6f, 1.8f })
                {
                    Box("EmptyWell", g, new Vector3(0f, .912f, z), new Vector3(.65f, .012f, .82f), art.Bars, false);
                    foreach (float x in new[] { -.34f, .34f })
                        Box("WellLip", g, new Vector3(x, .927f, z), new Vector3(.035f, .035f, .89f), art.Steel, false);
                    foreach (float zz in new[] { z - .435f, z + .435f })
                        Box("WellLip", g, new Vector3(0f, .927f, zz), new Vector3(.7f, .035f, .035f), art.Steel, false);
                    Box("CabinetSeam", g, new Vector3(-.514f, .48f, z + .47f), new Vector3(.012f, .63f, .009f), art.Bars, false);
                    Box("DoorGrip", g, new Vector3(-.54f, .7f, z), new Vector3(.06f, .035f, .24f), art.Brass, false);
                    Box("TrayRailBracket", g, new Vector3(-.66f, .64f, z), new Vector3(.3f, .045f, .045f), art.Railing, false);
                }
            });
            if (counter != null) Mounted(serving, "FittedServingWells", counter, new Vector3(64.9f, Y, 88.5f), Quaternion.identity);
        }

        private static void Kitchen(Transform room, IslandPrisonArt art)
        {
            MeshKit range = IslandMeshKit.Get("Kitchen_RangeHardware", g =>
            {
                foreach (float x in new[] { -.43f, .43f })
                foreach (float z in new[] { -.23f, .23f })
                {
                    Primitive(PrimitiveType.Cylinder, "BurnerRim", g, new Vector3(x, .94f, z), new Vector3(.31f, .009f, .31f), art.Steel, false);
                    Primitive(PrimitiveType.Cylinder, "BurnerCore", g, new Vector3(x, .951f, z), new Vector3(.24f, .007f, .24f), art.Bars, false);
                    foreach (float dx in new[] { -.08f, .08f })
                        Box("BurnerGrate", g, new Vector3(x + dx, .972f, z), new Vector3(.025f, .025f, .36f), art.Rust, false);
                }
                Box("OvenDoorGasket", g, new Vector3(0f, .43f, -.508f), new Vector3(1.31f, .53f, .018f), art.Bars, false);
                Box("OvenDoor", g, new Vector3(0f, .43f, -.523f), new Vector3(1.21f, .44f, .025f), art.Railing, false);
                Box("OvenPull", g, new Vector3(0f, .61f, -.58f), new Vector3(.86f, .045f, .055f), art.Steel, false);
                for (int i = 0; i < 4; i++)
                {
                    GameObject dial = Primitive(PrimitiveType.Cylinder, "ColdControlDial", g, new Vector3(-.54f + i * .36f, .79f, -.537f), new Vector3(.11f, .025f, .11f), art.Bars, false);
                    dial.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                }
                foreach (float x in new[] { -.62f, .62f })
                    Box("FixedRangeFoot", g, new Vector3(x, .055f, .25f), new Vector3(.14f, .11f, .4f), art.Bars, false);
            });
            foreach (Transform solid in room)
                if (solid.name == "Range") Mounted(range, "RangeFittings", solid, solid.position - Vector3.up * .45f, Quaternion.identity);

            Transform hood = room.Find("Hood");
            if (hood != null)
            {
                float width = hood.localScale.x;
                MeshKit baffles = IslandMeshKit.Get("Kitchen_HoodBaffles", g =>
                {
                    Box("FilterRecess", g, Vector3.zero, new Vector3(width - .12f, .02f, 1.08f), art.Bars, false);
                    for (float x = -width * .5f + .16f; x < width * .5f - .1f; x += .16f)
                        K.RotBox("GreaseBaffle", g, new Vector3(x, -.018f, 0f), new Vector3(.065f, .045f, .99f), Quaternion.Euler(0f, 0f, 20f), art.Steel, false);
                    Box("GreaseGutter", g, new Vector3(0f, -.045f, -.54f), new Vector3(width, .06f, .08f), art.Rust, false);
                });
                Mounted(baffles, "ExhaustFilters", hood, hood.position - Vector3.up * .265f, Quaternion.identity);
            }
        }

        public static Transform PrepTable(Transform parent, Vector3 at, IslandPrisonArt art)
        {
            MeshKit kit = IslandMeshKit.Get("Kitchen_OpenPrepTable", g =>
            {
                Box("RolledTop", g, new Vector3(0f, .873f, 0f), new Vector3(3.4f, .055f, .9f), art.Steel, false);
                foreach (float x in new[] { -1.52f, 1.52f })
                foreach (float z in new[] { -.31f, .31f })
                {
                    Box("TubularLeg", g, new Vector3(x, .44f, z), new Vector3(.065f, .86f, .065f), art.Steel, false);
                    Box("Foot", g, new Vector3(x, .027f, z), new Vector3(.11f, .05f, .11f), art.Bars, false);
                }
                foreach (float z in new[] { -.32f, .32f })
                    Box("LowerShelfFrame", g, new Vector3(0f, .23f, z), new Vector3(3.1f, .05f, .05f), art.Railing, false);
                for (float x = -1.45f; x < 1.5f; x += .19f)
                    Box("SlattedLowerRack", g, new Vector3(x, .255f, 0f), new Vector3(.035f, .025f, .64f), art.Steel, false);
                Box("SplashLip", g, new Vector3(0f, .95f, .43f), new Vector3(3.35f, .14f, .035f), art.Steel, false);
            });
            Transform table = IslandMeshKit.Place(kit, "Prep", parent, at, Quaternion.identity);
            IslandMeshKit.Solid(table, new Vector3(0f, .45f, 0f), new Vector3(3.4f, .9f, .9f));
            return table;
        }

        public static Transform Desk(Transform parent, string name, Vector3 at, IslandPrisonArt art)
        {
            MeshKit kit = IslandMeshKit.Get("Interior_JoineryDesk", g =>
            {
                Box("Desktop", g, new Vector3(0f, .735f, 0f), new Vector3(1.6f, .05f, .8f), art.Wood, false);
                Box("EdgeLip", g, new Vector3(0f, .712f, -.39f), new Vector3(1.61f, .055f, .035f), art.DarkWood, false);
                Box("Pedestal", g, new Vector3(-.49f, .35f, 0f), new Vector3(.48f, .7f, .7f), art.DarkWood, false);
                foreach (float z in new[] { -.3f, .3f })
                    Box("Leg", g, new Vector3(.68f, .35f, z), new Vector3(.065f, .7f, .065f), art.DarkWood, false);
                Box("ModestyPanel", g, new Vector3(.18f, .48f, .3f), new Vector3(.95f, .36f, .05f), art.DarkWood, false);
                foreach (float y in new[] { .17f, .36f, .55f })
                {
                    Box("DrawerFront", g, new Vector3(-.49f, y, -.36f), new Vector3(.43f, .17f, .025f), art.Wood, false);
                    Box("RecessedPull", g, new Vector3(-.49f, y + .035f, -.382f), new Vector3(.13f, .025f, .018f), art.Brass, false);
                }
                Box("Blotter", g, new Vector3(.38f, .764f, .16f), new Vector3(.5f, .008f, .28f), art.Dado, false);
            });
            Transform desk = IslandMeshKit.Place(kit, name, parent, at, Quaternion.identity);
            IslandMeshKit.Solid(desk, new Vector3(0f, .38f, 0f), new Vector3(1.6f, .76f, .8f));
            return desk;
        }
    }
}
