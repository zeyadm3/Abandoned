using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.HqSalvageProps;

namespace Abandoned.EditorTools
{
    /// <summary>Tools, charging equipment and spare gear make the shop a working repair station.</summary>
    public static class HqMaintenanceDressing
    {
        public static void Build(Transform parent)
        {
            Transform maintenance = Group("MaintenanceAndGear", parent);
            Pegboard(maintenance);
            BenchEquipment(maintenance);
            SpareGear(maintenance);
            MarkStatic(maintenance);
        }

        private static void Pegboard(Transform parent)
        {
            Transform wall = At("MaintenancePegboard", parent, new Vector3(18f, 2.02f, 15.82f));
            Material board = PolishAssets.Material("HQ_Pegboard", new Color(.36f, .31f, .23f), .94f);
            Material metal = CustomMallArt.Material("Metal"), black = CustomMallArt.Material("Black");
            Box("PerforatedBoard", wall, Vector3.zero, new Vector3(4.24f, 1.7f, .065f), board, false);
            foreach (float x in new[] { -2.15f, 2.15f })
                Box("Frame", wall, new Vector3(x, 0f, -.015f), new Vector3(.085f, 1.81f, .095f), metal, false);
            foreach (float y in new[] { -.89f, .89f })
                Box("Frame", wall, new Vector3(0f, y, -.015f), new Vector3(4.38f, .085f, .095f), metal, false);
            for (int x = 0; x < 21; x++)
            for (int y = 0; y < 8; y++)
                Box("PegHole", wall, new Vector3(-1.94f + x * .193f, -.68f + y * .193f, -.035f),
                    new Vector3(.017f, .017f, .004f), black, false);
            Box("ShopHeader", wall, new Vector3(0f, 1.1f, -.01f), new Vector3(3.2f, .25f, .03f), CustomMallArt.Material("Paper"), false);
            Label(wall, "MAINTENANCE / PUT IT BACK WORKING", new Vector3(0f, 1.1f, -.03f), .061f);

            for (int i = 0; i < 3; i++)
            {
                Transform tool = At("HangingSpanner", wall, new Vector3(-1.67f + i * .3f, .5f, -.09f));
                Spanner(tool, .34f + i * .055f);
                Hook(wall, new Vector3(-1.67f + i * .3f, .47f, -.035f));
            }
            Hammer(wall, new Vector3(-.47f, .52f, -.1f));
            Screwdriver(wall, new Vector3(.11f, .48f, -.1f), .36f);
            Screwdriver(wall, new Vector3(.39f, .48f, -.1f), .31f);
            Saw(wall, new Vector3(1.13f, .12f, -.115f));
            for (int i = 0; i < 3; i++)
            {
                Vector3 at = new Vector3(-1.52f + i * .38f, -.53f, -.095f);
                Flashlight(wall, at, Quaternion.identity);
                Hook(wall, at + new Vector3(0f, .25f, .06f));
            }
            Box("FlashlightShelf", wall, new Vector3(-1.1f, -.57f, -.12f), new Vector3(1.45f, .04f, .25f), metal, false);
            Box("ToolReminder", wall, new Vector3(.36f, -.52f, -.056f), new Vector3(.79f, .36f, .016f), CustomMallArt.Material("Paper"), false);
            Label(wall, "CHECK THE BEAM\nCHECK THE BATTERY\nTHEN LEAVE", new Vector3(.36f, -.52f, -.07f), .044f);
        }

        private static void BenchEquipment(Transform parent)
        {
            Transform tools = At("BenchToolbox", parent, new Vector3(16.77f, 1.117f, 13.49f), -3f);
            Material red = PolishAssets.Material("HQ_ToolboxPaint", new Color(.37f, .17f, .12f), .86f);
            Material metal = CustomMallArt.Material("Metal"), rubber = CustomMallArt.Material("Rubber");
            Box("Toolbox", tools, Vector3.up * .12f, new Vector3(.58f, .24f, .36f), red);
            Box("Lid", tools, new Vector3(0f, .25f, 0f), new Vector3(.6f, .04f, .38f), red, false);
            foreach (float x in new[] { -.14f, .14f })
            {
                Box("Latch", tools, new Vector3(x, .2f, -.19f), new Vector3(.04f, .09f, .02f), metal, false);
                Rod("HandleFoot", tools, new Vector3(x, .27f, 0f), new Vector3(x, .31f, 0f), .027f, metal);
            }
            Rod("CarryHandle", tools, new Vector3(-.14f, .31f, 0f), new Vector3(.14f, .31f, 0f), .03f, rubber);
            Flashlight(parent, new Vector3(17.25f, 1.148f, 13.23f), Quaternion.Euler(90f, 0f, -18f));

            Transform charger = At("BatteryChargingDock", parent, new Vector3(19.14f, 1.117f, 13.53f));
            Box("ChargingBase", charger, Vector3.up * .048f, new Vector3(.57f, .095f, .35f), rubber);
            for (int i = 0; i < 3; i++)
            {
                float x = -.18f + i * .18f;
                Box("DockSlot", charger, new Vector3(x, .1f, 0f), new Vector3(.115f, .018f, .19f), metal, false);
                Battery(charger, new Vector3(x, .109f, 0f));
                Box("ChargingIndicator", charger, new Vector3(x, .099f, -.132f), new Vector3(.02f, .013f, .015f),
                    PolishAssets.Material("HQ_ChargeLED", new Color(.42f, .56f, .27f), emission: .6f), false);
            }
            Box("ChargerLabel", charger, new Vector3(0f, .05f, -.18f), new Vector3(.43f, .063f, .009f), CustomMallArt.Material("Paper"), false);
            Label(charger, "CHARGE / READY", new Vector3(0f, .05f, -.188f), .027f);
            Rod("PowerLead", charger, new Vector3(.27f, .06f, .12f), new Vector3(.4f, .025f, .29f), .014f, rubber);
            Box("Rag", parent, new Vector3(16.39f, 1.132f, 13.55f), new Vector3(.18f, .018f, .28f), CustomMallArt.Material("Cloth"), false);
        }

        private static void SpareGear(Transform parent)
        {
            // This dresses the existing interactive GearRack, rather than introducing a second inventory station.
            Transform rack = At("SpareGearStock", parent, new Vector3(21.6f, 0f, 13.3f));
            for (int i = 0; i < 4; i++)
                Flashlight(rack, new Vector3(-.72f + i * .28f, .55f, -.15f), Quaternion.identity);
            for (int i = 0; i < 4; i++)
                Battery(rack, new Vector3(-.69f + i * .23f, 1.06f, -.04f));
            Material caseMaterial = CustomMallArt.Material("Canvas");
            for (int i = 0; i < 2; i++)
            {
                Vector3 at = new Vector3(-.52f + i * .9f, 1.57f, 0f);
                Box("FieldToolCase", rack, at + Vector3.up * .14f, new Vector3(.66f, .28f, .38f), caseMaterial);
                Box("CaseStrap", rack, at + new Vector3(0f, .15f, -.2f), new Vector3(.07f, .31f, .03f), CustomMallArt.Material("Rubber"), false);
            }
            Box("GearRackLabel", rack, new Vector3(0f, 2.2f, -.29f), new Vector3(1.8f, .24f, .025f), CustomMallArt.Material("Paper"), false);
            Label(rack, "FIELD GEAR / CREW ISSUE", new Vector3(0f, 2.2f, -.309f), .052f);
        }

        private static void Battery(Transform parent, Vector3 floor)
        {
            Material casing = PolishAssets.Material("HQ_BatteryCasing", new Color(.4f, .35f, .16f), .85f);
            Box("RechargeablePack", parent, floor + Vector3.up * .09f, new Vector3(.105f, .18f, .105f), casing, false);
            foreach (float x in new[] { -.028f, .028f })
                Box("BatteryContact", parent, floor + new Vector3(x, .185f, 0f), new Vector3(.023f, .013f, .036f), CustomMallArt.Material("Metal"), false);
            Box("BatteryBand", parent, floor + new Vector3(0f, .09f, -.055f), new Vector3(.09f, .036f, .006f), CustomMallArt.Material("Rubber"), false);
        }

        private static void Hook(Transform parent, Vector3 at)
        {
            Material metal = CustomMallArt.Material("Metal");
            Rod("PegHook", parent, at, at + Vector3.back * .06f, .016f, metal);
            Rod("HookTip", parent, at + Vector3.back * .06f, at + new Vector3(0f, .027f, -.06f), .016f, metal);
        }

        private static void Spanner(Transform tool, float length)
        {
            Material metal = CustomMallArt.Material("Metal");
            Box("SpannerHandle", tool, new Vector3(0f, -length * .5f, 0f), new Vector3(.05f, length, .025f), metal, false);
            Rod("RingEnd", tool, new Vector3(0f, -.013f, -.01f), new Vector3(0f, -.013f, .013f), .095f, metal);
            Rod("RingOpening", tool, new Vector3(0f, -.013f, -.012f), new Vector3(0f, -.013f, -.018f), .05f, CustomMallArt.Material("Black"));
            foreach (float x in new[] { -.033f, .033f })
                Box("OpenJaw", tool, new Vector3(x, -length - .007f, 0f), new Vector3(.03f, .075f, .028f), metal, false);
        }

        private static void Hammer(Transform parent, Vector3 at)
        {
            Transform hammer = At("ClawHammer", parent, at);
            Rod("HammerHandle", hammer, new Vector3(0f, -.42f, 0f), Vector3.zero, .045f, CustomMallArt.Material("Wood"));
            Box("HammerHead", hammer, Vector3.zero, new Vector3(.21f, .08f, .075f), CustomMallArt.Material("Metal"), false);
            Hook(parent, at + new Vector3(0f, -.015f, .065f));
        }

        private static void Screwdriver(Transform parent, Vector3 at, float length)
        {
            Transform driver = At("Screwdriver", parent, at);
            Rod("DriverGrip", driver, new Vector3(0f, -.13f, 0f), Vector3.zero, .048f, CustomMallArt.Material("Orange"));
            Rod("DriverShaft", driver, new Vector3(0f, -length, 0f), new Vector3(0f, -.12f, 0f), .016f, CustomMallArt.Material("Metal"));
            Hook(parent, at + new Vector3(0f, -.045f, .065f));
        }

        private static void Saw(Transform parent, Vector3 at)
        {
            Transform saw = At("HandSaw", parent, at);
            Box("SawBlade", saw, Vector3.zero, new Vector3(.78f, .14f, .018f), CustomMallArt.Material("Metal"), false);
            for (int tooth = 0; tooth < 12; tooth++)
                Bar("SawTooth", saw, new Vector3(-.38f + tooth * .053f, -.07f, 0f), new Vector3(-.35f + tooth * .053f, -.098f, 0f),
                    .018f, .02f, CustomMallArt.Material("Metal"));
            Box("SawHandle", saw, new Vector3(.46f, .015f, 0f), new Vector3(.16f, .22f, .035f), CustomMallArt.Material("Wood"), false);
            Box("HandleOpening", saw, new Vector3(.46f, .015f, -.02f), new Vector3(.07f, .1f, .008f), CustomMallArt.Material("Black"), false);
            Hook(parent, at + new Vector3(.39f, .08f, .08f));
        }
    }
}
