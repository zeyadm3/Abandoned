using Abandoned.Company;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>The original HQ interactions, authored into the depot's office, crew room and workshop.</summary>
    public static class HqOperationsStations
    {
        private static Material Steel => PolishAssets.Material("HQ_SalvageSteel", new Color(.22f, .25f, .24f), .64f);
        private static Material Rust => CustomMallArt.Material("Rust");

        public static void Build(Transform root)
        {
            Van(root);
            Workshop(root);
            CrewLockers(root);
            Voicemail(root);
            Reception(root);
        }

        private static void Van(Transform root)
        {
            GameObject van = Box("Van", root, HqLayout.Van, new Vector3(2.4f, 2.4f, 5f), CustomMallArt.Material("Trim"));
            van.AddComponent<HqVan>();
            GameObject model = Vehicles.Dress(van, Vehicles.Van);
            Transform rendered = model != null ? model.transform : van.transform;
            foreach (Renderer renderer in rendered.GetComponentsInChildren<Renderer>())
            {
                if (renderer.sharedMaterial == null) continue;
                var block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", new Color(.33f, .36f, .31f));
                renderer.SetPropertyBlock(block);
            }
        }

        private static void Workshop(Transform root)
        {
            GameObject terminal = Box("ShopTerminal", root, HqLayout.Maintenance + new Vector3(0f, 1.4f, .10f),
                new Vector3(.8f, .5f, .3f), CustomMallArt.Material("Trim"));
            terminal.AddComponent<Abandoned.Equipment.ShopTerminal>();
            HqDressingBuilder.TerminalFace(terminal.transform);

            Transform rack = HqSalvageProps.Shelf(root, HqLayout.GearRack, 2.4f, 2.4f, .70f, 90f);
            rack.name = "GearRack";
            rack.gameObject.AddComponent<Abandoned.Equipment.GearRack>();
            var target = rack.gameObject.AddComponent<BoxCollider>();
            target.center = new Vector3(0f, 1.1f, -.37f);
            target.size = new Vector3(2.33f, 2.15f, .11f);
            Box("CrewIssueHeader", rack, new Vector3(0f, 2.36f, -.397f), new Vector3(2.36f, .23f, .036f), Steel, false);
            HqSalvageProps.Label(rack, "FIELD GEAR / CREW ISSUE", new Vector3(0f, 2.36f, -.42f), .065f,
                new Color(.76f, .71f, .55f));
        }

        private static void CrewLockers(Transform root)
        {
            Transform lockers = HqSalvageProps.At("Lockers", root, HqLayout.Lockers, 90f);
            Material paint = PolishAssets.Material("HQ_CrewLockerPaint", new Color(.24f, .32f, .29f), .86f);
            Material dark = CustomMallArt.Material("Rubber");
            Box("LockerPlinth", lockers, new Vector3(0f, .09f, 0f), new Vector3(2.94f, .18f, .76f), Rust);
            for (int i = 0; i < 4; i++)
            {
                float x = -1.05f + i * .70f;
                Box("SteelLocker_" + (i + 1), lockers, new Vector3(x, 1.10f, 0f), new Vector3(.67f, 1.96f, .65f), paint);
                Box("RecessedDoor", lockers, new Vector3(x, 1.1f, -.333f), new Vector3(.59f, 1.83f, .035f), Steel, false);
                Box("DoorHandle", lockers, new Vector3(x + .20f, 1.06f, -.365f), new Vector3(.03f, .15f, .045f), Rust, false);
                foreach (float y in new[] { .43f, .49f, .55f, 1.73f, 1.79f, 1.85f })
                    Box("VentSlot", lockers, new Vector3(x, y, -.356f), new Vector3(.36f, .018f, .007f), dark, false);
                Box("CrewNumberPlate", lockers, new Vector3(x, 1.47f, -.358f), new Vector3(.27f, .16f, .012f), CustomMallArt.Material("Paper"), false);
                HqSalvageProps.Label(lockers, $"CREW {i + 1:00}", new Vector3(x, 1.47f, -.368f), .034f);
                foreach (float y in new[] { .31f, 1.25f, 1.97f })
                    Box("Hinge", lockers, new Vector3(x - .31f, y, -.349f), new Vector3(.031f, .09f, .035f), Rust, false);
            }
            lockers.gameObject.AddComponent<Abandoned.UI.WardrobeLocker>();
            var target = lockers.gameObject.AddComponent<BoxCollider>();
            target.center = new Vector3(0f, 1.08f, -.38f);
            target.size = new Vector3(2.82f, 1.95f, .08f);
            Box("LockerHeader", lockers, new Vector3(0f, 2.28f, -.28f), new Vector3(2.85f, .26f, .045f), Steel, false);
            HqSalvageProps.Label(lockers, "CREW ROOM / CHANGE & STORE", new Vector3(0f, 2.28f, -.309f), .068f,
                new Color(.76f, .71f, .55f));
        }

        private static void Voicemail(Transform root)
        {
            Transform machine = HqSalvageProps.At("AnsweringMachine", root, HqLayout.Dispatch + new Vector3(0f, 1.125f, -.48f));
            var hit = machine.gameObject.AddComponent<BoxCollider>();
            hit.center = new Vector3(.02f, .05f, 0f);
            hit.size = new Vector3(.42f, .1f, .28f);
            CustomMallArt.Place("AnsweringMachine", machine, Vector3.zero, Quaternion.identity);
            Material blink = PolishAssets.Material("HQ_MessageLamp", new Color(.62f, .23f, .075f), emission: 2.4f);
            GameObject lamp = Primitive(PrimitiveType.Cylinder, "MessageLight", machine, new Vector3(.105f, .064f, .07f),
                new Vector3(.016f, .003f, .016f), blink, false);
            machine.gameObject.AddComponent<AnsweringMachine>().EditorSetup(lamp.GetComponent<Renderer>());
        }

        private static void Reception(Transform root)
        {
            Transform office = HqSalvageProps.At("DispatchReceptionFurniture", root, HqLayout.Dispatch);
            Chair(office, new Vector3(-.90f, 0f, -1.95f), 180f, "VisitorChair");
            Chair(office, new Vector3(.90f, 0f, -1.95f), 180f, "VisitorChair");
            Chair(office, new Vector3(0f, 0f, 1.58f), 0f, "DispatcherChair");
            Transform forms = HqSalvageProps.At("DispatchFormsCabinet", office, new Vector3(1.36f, 0f, .25f));
            Box("CabinetBody", forms, new Vector3(0f, .46f, 0f), new Vector3(.74f, .92f, .74f), Steel);
            foreach (float y in new[] { .17f, .45f, .73f })
            {
                Box("DrawerFront", forms, new Vector3(0f, y, .379f), new Vector3(.66f, .24f, .027f), Rust, false);
                Box("DrawerPull", forms, new Vector3(0f, y, .404f), new Vector3(.21f, .034f, .023f), Steel, false);
            }
            Transform sign = HqSalvageProps.At("DispatchReceptionSign", root, new Vector3(30f, 2.35f, .20f), 180f);
            Box("WallPlate", sign, Vector3.zero, new Vector3(2.45f, .64f, .055f), Steel, false);
            HqSalvageProps.Label(sign, "ABANDONED SALVAGING\nRECEPTION & DISPATCH", new Vector3(0f, 0f, -.038f), .075f,
                new Color(.77f, .72f, .57f));
        }

        private static void Chair(Transform parent, Vector3 floor, float yaw, string name)
        {
            Transform chair = HqSalvageProps.At(name, parent, floor, yaw);
            Material upholstery = PolishAssets.Material("HQ_ReceptionChair", new Color(.25f, .28f, .25f), .96f);
            Box("UpholsteredSeat", chair, new Vector3(0f, .49f, 0f), new Vector3(.53f, .12f, .53f), upholstery);
            Box("SeatBack", chair, new Vector3(0f, .81f, .23f), new Vector3(.53f, .57f, .085f), upholstery);
            foreach (float x in new[] { -.22f, .22f })
            foreach (float z in new[] { -.21f, .21f })
                Box("TubularLeg", chair, new Vector3(x, .22f, z), new Vector3(.034f, .44f, .034f), Steel);
            foreach (float x in new[] { -.285f, .285f })
            {
                Box("Armrest", chair, new Vector3(x, .70f, -.03f), new Vector3(.06f, .055f, .40f), upholstery);
                Box("ArmUpright", chair, new Vector3(x, .60f, -.17f), new Vector3(.034f, .2f, .034f), Steel, false);
            }
        }
    }
}
