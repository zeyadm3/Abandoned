using Abandoned.Company;
using Abandoned.Threats;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Original workshop furniture with separate, reachable interaction targets.</summary>
    public static class HqSalvageStations
    {
        private static Material Steel => PolishAssets.Material("HQ_SalvageSteel", new Color(.22f, .25f, .24f), .64f);
        private static Material Rust => CustomMallArt.Material("Rust");
        private static Material Rubber => CustomMallArt.Material("Rubber");

        public static void Build(Transform root)
        {
            Transform desk = Bench(root, "DispatchWorkbench", HqLayout.Dispatch, 3.8f, 1.6f);
            Box("ReceptionFascia", desk, new Vector3(0f, .72f, -.835f), new Vector3(3.65f, .56f, .07f), Steel);
            Box("CompanyNameplate", desk, new Vector3(0f, .82f, -.877f), new Vector3(2.78f, .26f, .022f), CustomMallArt.Material("Brass"), false);
            RotaryPhone(desk);
            FieldGuide(desk);
            TutorialHqClipboard.Place(desk);
            Box("DispatchTray", desk, new Vector3(-1.4f, 1.13f, .32f), new Vector3(.56f, .065f, .34f), Steel, false);
            for (int i = 0; i < 4; i++)
                Box("CarbonJobSheet", desk, new Vector3(-1.41f + i * .012f, 1.17f + i * .002f, .32f),
                    new Vector3(.46f, .002f, .28f), CustomMallArt.Material("Paper"), false);
            Label(desk, "ABANDONED SALVAGING", new Vector3(0f, .82f, -.895f), 0f, .071f);
            Label(desk, "RECEPTION / CONTRACT DISPATCH", new Vector3(0f, .62f, -.883f), 0f, .043f);
            Bench(root, "MaintenanceWorkbench", HqLayout.Maintenance, 4.8f, 1.35f);
            HqOperationsStations.Build(root);
            Depot(root);
            HqSortingIsland.Build(root);
        }

        private static Transform Bench(Transform root, string name, Vector3 floor, float width, float depth)
        {
            Transform bench = Group(name, root);
            bench.localPosition = floor;
            Box("WornSteelTop", bench, new Vector3(0f, 1.05f, 0f), new Vector3(width, .13f, depth), Steel);
            Box("ApronFront", bench, new Vector3(0f, .87f, depth * .5f - .06f), new Vector3(width - .16f, .22f, .08f), Rust);
            Box("ApronBack", bench, new Vector3(0f, .87f, -depth * .5f + .06f), new Vector3(width - .16f, .22f, .08f), Rust);
            foreach (float x in new[] { -width * .5f + .13f, width * .5f - .13f })
            foreach (float z in new[] { -depth * .5f + .12f, depth * .5f - .12f })
            {
                Box("SquareTubeLeg", bench, new Vector3(x, .48f, z), new Vector3(.10f, .96f, .10f), Steel);
                Box("FootPlate", bench, new Vector3(x, .04f, z), new Vector3(.22f, .08f, .22f), Rust, false);
                Primitive(PrimitiveType.Cylinder, "TopBolt", bench, new Vector3(x, 1.121f, z), new Vector3(.025f, .005f, .025f), Rust, false);
            }
            Box("LowerShelf", bench, new Vector3(0f, .30f, 0f), new Vector3(width - .24f, .055f, depth - .24f), Steel);
            for (int i = 0; i < 8; i++)
            {
                var scratch = Box("TopWear", bench, new Vector3(-width * .35f + i * width * .10f, 1.117f, -.42f + i % 3 * .3f),
                    new Vector3(.21f + i % 3 * .06f, .002f, .007f), Rust, false);
                scratch.transform.localRotation = Quaternion.Euler(0f, 20f + i * 29f, 0f);
            }
            return bench;
        }

        private static void RotaryPhone(Transform desk)
        {
            Transform phone = Group("DispatchTelephone", desk);
            phone.localPosition = new Vector3(-.65f, 1.125f, -.27f);
            var hit = phone.gameObject.AddComponent<BoxCollider>();
            hit.center = new Vector3(0f, .12f, 0f);
            hit.size = new Vector3(.58f, .24f, .45f);
            phone.gameObject.AddComponent<DispatchTelephone>();
            Box("BakeliteBase", phone, new Vector3(0f, .065f, 0f), new Vector3(.48f, .13f, .37f), Rubber, false);
            Box("DialHousing", phone, new Vector3(0f, .135f, .025f), new Vector3(.39f, .07f, .29f), Rubber, false);
            Primitive(PrimitiveType.Cylinder, "RotaryDial", phone, new Vector3(0f, .178f, .047f),
                new Vector3(.225f, .009f, .225f), CustomMallArt.Material("Brass"), false);
            Primitive(PrimitiveType.Cylinder, "DialCentre", phone, new Vector3(0f, .190f, .047f),
                new Vector3(.097f, .004f, .097f), CustomMallArt.Material("Paper"), false);
            for (int i = 0; i < 10; i++)
            {
                float angle = (32f + i * 29f) * Mathf.Deg2Rad;
                Primitive(PrimitiveType.Cylinder, "FingerHole_" + i, phone,
                    new Vector3(Mathf.Sin(angle) * .086f, .19f, .047f + Mathf.Cos(angle) * .086f),
                    new Vector3(.025f, .003f, .025f), Rubber, false);
            }
            foreach (float x in new[] { -.175f, .175f })
            {
                Box("Cradle", phone, new Vector3(x, .16f, -.135f), new Vector3(.065f, .12f, .075f), Steel, false);
                Primitive(PrimitiveType.Cylinder, "ReceiverCup", phone, new Vector3(x, .232f, -.135f),
                    new Vector3(.116f, .035f, .116f), Rubber, false);
            }
            Box("Handset", phone, new Vector3(0f, .25f, -.135f), new Vector3(.37f, .045f, .07f), Rubber, false);
            for (int i = 0; i < 24; i++)
                Primitive(PrimitiveType.Sphere, "CoiledCord", phone,
                    new Vector3(-.255f - i * .009f, .022f + Mathf.Sin(i * 2.1f) * .011f, -.14f + Mathf.Cos(i * 2.1f) * .018f),
                    Vector3.one * .021f, Rubber, false);
            Label(desk, "DISPATCH / E", new Vector3(-.65f, 1.01f, -.835f), 0f, .03f);
        }

        private static void FieldGuide(Transform desk)
        {
            Transform book = Group("FieldGuide", desk);
            book.SetLocalPositionAndRotation(new Vector3(.62f, 1.13f, -.27f), Quaternion.Euler(0f, -12f, 0f));
            var hit = book.gameObject.AddComponent<BoxCollider>();
            hit.center = new Vector3(0f, .06f, 0f);
            hit.size = new Vector3(.55f, .12f, .70f);
            book.gameObject.AddComponent<ThreatFilesTerminal>();
            Material leather = PolishAssets.Material("HQ_FieldGuideLeather", new Color(.18f, .095f, .055f), .94f);
            Material pages = PolishAssets.Material("HQ_FieldGuidePages", new Color(.65f, .59f, .44f));
            Box("PageBlock", book, new Vector3(.009f, .047f, 0f), new Vector3(.51f, .06f, .66f), pages, false);
            Box("LeatherBack", book, new Vector3(0f, .009f, 0f), new Vector3(.55f, .018f, .70f), leather, false);
            Box("LeatherCover", book, new Vector3(0f, .088f, 0f), new Vector3(.55f, .022f, .70f), leather, false);
            Box("BoundSpine", book, new Vector3(-.263f, .048f, 0f), new Vector3(.031f, .095f, .70f), leather, false);
            for (int i = 0; i < 6; i++)
                Box("PageEdge", book, new Vector3(.266f, .024f + i * .009f, .005f), new Vector3(.002f, .001f, .64f), Rust, false);
            foreach (float z in new[] { -.29f, -.17f, .17f, .29f })
                Box("SpineBand", book, new Vector3(-.283f, .052f, z), new Vector3(.012f, .089f, .025f), Rust, false);
            Box("Bookmark", book, new Vector3(.14f, .04f, .37f), new Vector3(.035f, .003f, .12f), CustomMallArt.Material("Cloth"), false);
            TextMesh title = Label(book, "FIELD GUIDE\n\nINCIDENT LOG\nVOL. 01", new Vector3(0f, .101f, 0f), 0f, .027f);
            title.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            for (int i = 0; i < 5; i++)
                Box("CoverScuff", book, new Vector3(-.20f + i * .09f, .101f, -.285f + i % 2 * .57f),
                    new Vector3(.07f, .001f, .011f), Rust, false);
            Label(desk, "FIELD GUIDE / E", new Vector3(.62f, 1.01f, -.835f), 0f, .03f);
        }

        private static void Depot(Transform root)
        {
            Transform bay = Group("ScrapQuotaDepot", root);
            bay.localPosition = HqLayout.Depot;
            Material hazard = PolishAssets.Material("HQ_SalvageHazard", new Color(.64f, .47f, .10f));
            Box("ScaleBase", bay, new Vector3(0f, .12f, 0f), new Vector3(2.3f, .24f, 1.6f), Rust);
            Box("WeighingPlatform", bay, new Vector3(0f, .29f, 0f), new Vector3(2.2f, .10f, 1.5f), Steel);
            for (int i = 0; i < 10; i++)
                Box("PlatformRib", bay, new Vector3(-.99f + i * .22f, .346f, 0f), new Vector3(.025f, .012f, 1.42f), Rust, false);
            Box("ReadoutPost", bay, new Vector3(-1.35f, .65f, .40f), new Vector3(.09f, 1.3f, .09f), Steel);
            Transform readout = Group("ScaleReadout", bay);
            readout.localPosition = new Vector3(-1.35f, 1.29f, .4f);
            Box("ReadoutHousing", readout, Vector3.zero, new Vector3(.70f, .35f, .17f), Steel, false);
            Box("ReadoutGlass", readout, new Vector3(0f, 0f, -.096f), new Vector3(.61f, .27f, .02f), Rubber, false);
            var screenHit = readout.gameObject.AddComponent<BoxCollider>();
            screenHit.size = new Vector3(.70f, .35f, .20f);
            readout.gameObject.AddComponent<ScrapDepot>().EditorSetup(null);

            Transform chute = Group("ReinforcedDepositChute", bay);
            chute.localPosition = new Vector3(2.8f, 0f, .45f);
            Box("ChuteFoot", chute, new Vector3(0f, .12f, 0f), new Vector3(1.6f, .24f, 1.3f), Rust);
            Box("ChuteBody", chute, new Vector3(0f, .60f, 0f), new Vector3(1.4f, .95f, 1.14f), Steel);
            foreach (float x in new[] { -.73f, .73f })
                Box("ChuteReinforcement", chute, new Vector3(x, .72f, 0f), new Vector3(.11f, 1.2f, 1.22f), Rust);
            Box("OpeningDarkness", chute, new Vector3(0f, 1.085f, 0f), new Vector3(1.28f, .012f, 1.02f), Rubber, false);
            foreach (float x in new[] { -.67f, .67f })
                Box("HopperSide", chute, new Vector3(x, 1.28f, 0f), new Vector3(.09f, .42f, 1.2f), Steel);
            Box("HopperBack", chute, new Vector3(0f, 1.28f, .57f), new Vector3(1.4f, .42f, .09f), Steel);
            Box("IntakeLip", chute, new Vector3(0f, 1.12f, -.59f), new Vector3(1.45f, .13f, .13f), hazard);
            for (int i = 0; i < 7; i++)
                Box("CautionStripe", chute, new Vector3(-.6f + i * .20f, 1.12f, -.66f), new Vector3(.09f, .13f, .005f), Rubber, false);
            Transform intake = Group("DepositTarget", chute);
            intake.localPosition = new Vector3(0f, .80f, -.61f);
            var hit = intake.gameObject.AddComponent<BoxCollider>();
            hit.size = new Vector3(1.15f, .44f, .12f);
            Transform receivingBay = Group("ReceivingBay", chute);
            receivingBay.localPosition = new Vector3(0f, 1.04f, 0f);
            var receiving = receivingBay.gameObject.AddComponent<BoxCollider>();
            receiving.isTrigger = true;
            receiving.size = new Vector3(1.22f, .75f, .96f);
            intake.gameObject.AddComponent<ScrapDepot>().EditorSetup(receiving);
            Label(chute, "SCRAP INTAKE / E", new Vector3(0f, .85f, -.682f), 0f, .038f);
            Label(chute, "KEEP HANDS CLEAR", new Vector3(0f, .66f, -.682f), 0f, .031f);
            for (int side = -1; side <= 1; side += 2)
                Box("ProcessingBayLine", bay, new Vector3(side * 1.5f, .008f, -.15f), new Vector3(.06f, .008f, 2.25f), hazard, false);
            foreach (float x in new[] { -1.65f, 4f })
                Box("DepotSignPost", bay, new Vector3(x, 1.26f, 1.4f), new Vector3(.10f, 2.52f, .10f), Steel);
            Box("DepotSignBack", bay, new Vector3(1.17f, 2.26f, 1.4f), new Vector3(5.85f, .58f, .10f), Steel);
            Label(bay, "01 / RECEIVING & WEIGHING", new Vector3(1.17f, 2.30f, 1.338f), 0f, .09f);
            Label(bay, "SCRAP INTAKE     /     QUOTA RECEIPTS", new Vector3(1.17f, 2.10f, 1.338f), 0f, .036f);
        }

        private static TextMesh Label(Transform parent, string text, Vector3 at, float yaw, float size)
        {
            TextMesh label = new GameObject("Label_" + text.Split('\n')[0]).AddComponent<TextMesh>();
            label.transform.SetParent(parent, false);
            label.transform.SetLocalPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 64;
            label.characterSize = size;
            label.color = new Color(.72f, .67f, .52f);
            WorldTextMaterial.Apply(label, AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf"));
            return label;
        }
    }
}
