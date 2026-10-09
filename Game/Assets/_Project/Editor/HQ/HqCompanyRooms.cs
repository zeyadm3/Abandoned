using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.HqSalvageProps;

namespace Abandoned.EditorTools
{
    /// <summary>Working records and crew welfare make the service wing feel occupied between recovery shifts.</summary>
    public static class HqCompanyRooms
    {
        public static void Build(Transform parent)
        {
            Transform rooms = Group("OfficeAndCrewLife", parent);
            FilingCabinet(rooms, new Vector3(35.15f, 0f, 1.35f), "CONTRACTS / CLOSED");
            FilingCabinet(rooms, new Vector3(35.15f, 0f, 2.55f), "LOAD RECORDS / LOGGED");
            OfficeRecords(rooms);
            LunchRoom(rooms);
            CrewEquipment(rooms);
            MarkStatic(rooms);
        }

        private static void FilingCabinet(Transform parent, Vector3 floor, string label)
        {
            Transform cabinet = At("CompanyFilingCabinet", parent, floor, 90f);
            Material steel = PolishAssets.Material("HQ_FilingCabinet", new Color(.38f, .41f, .35f), .86f);
            Box("Cabinet", cabinet, Vector3.up * .78f, new Vector3(.83f, 1.56f, .69f), steel);
            for (int drawer = 0; drawer < 4; drawer++)
            {
                float y = .23f + drawer * .37f;
                Box("DrawerFront", cabinet, new Vector3(0f, y, -.36f), new Vector3(.76f, .32f, .028f), steel, false);
                Box("DrawerRecess", cabinet, new Vector3(0f, y + .035f, -.381f), new Vector3(.25f, .05f, .012f), CustomMallArt.Material("Rubber"), false);
                Rod("DrawerHandle", cabinet, new Vector3(-.10f, y + .015f, -.4f), new Vector3(.10f, y + .015f, -.4f), .019f, CustomMallArt.Material("Metal"));
                Box("DrawerLabel", cabinet, new Vector3(0f, y -.076f, -.382f), new Vector3(.37f, .062f, .012f), CustomMallArt.Material("Paper"), false);
                Label(cabinet, "FILE " + (drawer + 1).ToString("00"), new Vector3(0f, y -.076f, -.391f), .027f);
            }
            Box("RecordsBox", cabinet, new Vector3(0f, 1.7f, 0f), new Vector3(.68f, .28f, .5f), CustomMallArt.Material("Cardboard"), false);
            Label(cabinet, label, new Vector3(0f, 1.71f, -.256f), .033f);
        }

        private static void OfficeRecords(Transform parent)
        {
            Material paper = CustomMallArt.Material("Paper");
            Transform records = At("DispatchShiftBoard", parent, new Vector3(30.9f, 2.27f, 9.82f));
            Box("RecordsBacking", records, Vector3.zero, new Vector3(4.5f, .87f, .055f), CustomMallArt.Material("Metal"), false);
            Label(records, "ABANDONED SALVAGING / SHIFT HANDOVER", new Vector3(0f, .65f, -.04f), .075f, new Color(.67f, .64f, .48f));
            string[] headings = { "OUTBOUND\nCREW / CONTRACT", "INCOMING\nWEIGH / LOG", "HANDOVER\nFAULTS / REPAIRS" };
            for (int i = 0; i < 3; i++)
            {
                Box("ShiftSheet", records, new Vector3(-1.42f + i * 1.42f, 0f, -.044f), new Vector3(1.24f, .68f, .015f), paper, false);
                Label(records, headings[i], new Vector3(-1.42f + i * 1.42f, .14f, -.057f), .052f);
                for (int line = 0; line < 3; line++)
                    Box("RecordLine", records, new Vector3(-1.42f + i * 1.42f, -.09f - line * .1f, -.06f), new Vector3(.97f, .007f, .003f), CustomMallArt.Material("Stain"), false);
            }
            Mug(parent, HqLayout.Dispatch + new Vector3(1.45f, 1.133f, .48f));
            for (int i = 0; i < 3; i++)
            {
                GameObject sheet = Box("DispatchPaperwork", parent, HqLayout.Dispatch + new Vector3(1.35f, 1.136f + i * .006f, .12f),
                    new Vector3(.32f, .004f, .24f), paper, false);
                sheet.transform.localRotation = Quaternion.Euler(0f, i * 5f - 4f, 0f);
            }
            Transform chair = At("OfficeVisitorSeat", parent, new Vector3(28.65f, 0f, 2.9f), 165f);
            Box("Seat", chair, Vector3.up * .47f, new Vector3(.57f, .075f, .54f), CustomMallArt.Material("Cloth"));
            Box("SeatBack", chair, new Vector3(0f, .8f, .21f), new Vector3(.57f, .62f, .065f), CustomMallArt.Material("Cloth"), false);
            foreach (float x in new[] { -.22f, .22f })
            foreach (float z in new[] { -.2f, .2f })
                Box("ChairLeg", chair, new Vector3(x, .23f, z), new Vector3(.037f, .46f, .037f), CustomMallArt.Material("Metal"), false);
        }

        private static void LunchRoom(Transform parent)
        {
            Transform table = At("CrewLunchTable", parent, new Vector3(31f, 0f, 14f));
            Material wood = CustomMallArt.Material("Wood"), metal = CustomMallArt.Material("Metal");
            Box("LunchTableTop", table, Vector3.up * .84f, new Vector3(3.2f, .08f, 1.3f), wood);
            foreach (float x in new[] { -1.13f, 1.13f })
            {
                Box("TableTrestle", table, new Vector3(x, .42f, 0f), new Vector3(.13f, .84f, .9f), metal);
                foreach (float z in new[] { -1.08f, 1.08f })
                    Box("BenchLeg", table, new Vector3(x, .23f, z), new Vector3(.09f, .46f, .3f), metal);
            }
            foreach (float z in new[] { -1.08f, 1.08f })
                Box("CrewBench", table, new Vector3(0f, .49f, z), new Vector3(3.3f, .075f, .39f), wood);
            Mug(table, new Vector3(-.37f, .884f, -.25f));
            Mug(table, new Vector3(.55f, .884f, .22f));
            Box("LunchTin", table, new Vector3(-1.07f, .94f, .12f), new Vector3(.34f, .13f, .23f), CustomMallArt.Material("Metal"), false);
            Box("WrappedSandwich", table, new Vector3(.15f, .905f, -.12f), new Vector3(.18f, .042f, .13f), CustomMallArt.Material("Paper"), false);
            Rod("CrewThermos", table, new Vector3(1.13f, .88f, .2f), new Vector3(1.13f, 1.14f, .2f), .085f, CustomMallArt.Material("Trim"));
            Rod("ThermosCap", table, new Vector3(1.13f, 1.14f, .2f), new Vector3(1.13f, 1.17f, .2f), .094f, metal);
            Transform clock = At("CrewTimeClock", parent, new Vector3(27.15f, 2.04f, 11.75f), 270f);
            Box("ClockHousing", clock, Vector3.zero, new Vector3(.44f, .61f, .17f), metal, false);
            Box("ClockDisplay", clock, new Vector3(0f, .105f, -.09f), new Vector3(.33f, .2f, .014f), CustomMallArt.Material("Rubber"), false);
            Label(clock, "06:42", new Vector3(0f, .105f, -.103f), .065f, new Color(.66f, .63f, .45f));
            Box("PunchSlot", clock, new Vector3(0f, -.18f, -.09f), new Vector3(.28f, .028f, .018f), CustomMallArt.Material("Black"), false);
            Label(clock, "SHIFT / PUNCH IN", new Vector3(0f, -.43f, -.065f), .05f, new Color(.67f, .61f, .45f));

            Transform water = At("CrewWaterStation", parent, new Vector3(28.18f, 0f, 16.85f));
            Box("WaterCooler", water, Vector3.up * .48f, new Vector3(.53f, .96f, .49f), CustomMallArt.Material("Ceiling"));
            Rod("WaterBottle", water, Vector3.up * .96f, Vector3.up * 1.34f, .33f, CustomMallArt.Material("Glass"));
            Box("Tap", water, new Vector3(0f, .78f, -.275f), new Vector3(.095f, .1f, .08f), CustomMallArt.Material("Trim"), false);
            Label(water, "DRINKING WATER", new Vector3(0f, .57f, -.254f), .038f);
        }

        private static void CrewEquipment(Transform parent)
        {
            Material rubber = CustomMallArt.Material("Rubber"), yellow = CustomMallArt.Material("Hazard");
            Transform mat = At("CrewBootMat", parent, new Vector3(34.05f, .006f, 14.1f));
            Box("PPEMat", mat, Vector3.zero, new Vector3(1.25f, .015f, 2.85f), rubber, false);
            for (int tread = 0; tread < 11; tread++)
                Box("MatTread", mat, new Vector3(0f, .012f, -1.2f + tread * .24f), new Vector3(1.17f, .009f, .037f), CustomMallArt.Material("Stain"), false);
            for (int pair = 0; pair < 2; pair++)
            foreach (float side in new[] { -.15f, .15f })
            {
                Vector3 at = new Vector3(side, .10f, -.68f + pair * 1.02f);
                Box("WorkBootToe", mat, at, new Vector3(.17f, .18f, .34f), rubber, false);
                Box("WorkBootAnkle", mat, at + new Vector3(0f, .105f, .07f), new Vector3(.16f, .18f, .16f), rubber, false);
            }
            Transform vest = At("CrewPPEHooks", parent, new Vector3(35.79f, 1.63f, 16.67f), 90f);
            Box("HookRail", vest, new Vector3(0f, .42f, 0f), new Vector3(.9f, .075f, .065f), CustomMallArt.Material("Metal"), false);
            Box("SafetyVest", vest, new Vector3(0f, -.05f, -.085f), new Vector3(.57f, .76f, .1f), yellow, false);
            foreach (float x in new[] { -.16f, .16f })
                Box("VestReflector", vest, new Vector3(x, -.04f, -.142f), new Vector3(.065f, .64f, .013f), CustomMallArt.Material("White"), false);
            Box("VestReflector", vest, new Vector3(0f, -.21f, -.144f), new Vector3(.56f, .068f, .013f), CustomMallArt.Material("White"), false);
            Rod("HardHatHook", vest, new Vector3(.39f, .42f, -.025f), new Vector3(.39f, .38f, -.13f), .018f, CustomMallArt.Material("Metal"));
            Primitive(PrimitiveType.Sphere, "HardHat", vest, new Vector3(.42f, .24f, -.12f), new Vector3(.25f, .13f, .29f), yellow, false);
            Box("HardHatBrim", vest, new Vector3(.42f, .2f, -.13f), new Vector3(.29f, .018f, .33f), yellow, false);
        }

        private static void Mug(Transform parent, Vector3 floor)
        {
            Transform mug = At("CrewMug", parent, floor);
            Material enamel = PolishAssets.Material("HQ_EnamelMug", new Color(.59f, .58f, .45f), .56f);
            Rod("MugBody", mug, Vector3.up * .01f, Vector3.up * .13f, .089f, enamel);
            Rod("Coffee", mug, Vector3.up * .127f, Vector3.up * .131f, .073f, CustomMallArt.Material("Black"));
            Rod("MugHandle", mug, new Vector3(.066f, .032f, 0f), new Vector3(.066f, .104f, 0f), .014f, enamel);
            Rod("HandleJoin", mug, new Vector3(.04f, .033f, 0f), new Vector3(.066f, .033f, 0f), .014f, enamel);
            Rod("HandleJoin", mug, new Vector3(.04f, .104f, 0f), new Vector3(.066f, .104f, 0f), .014f, enamel);
        }
    }
}
