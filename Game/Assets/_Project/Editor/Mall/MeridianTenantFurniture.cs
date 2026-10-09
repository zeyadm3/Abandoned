using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Individual recognisable furnishings, grouped into showroom arrangements rather than boxed display bays.</summary>
    public static class MeridianTenantFurniture
    {
        public static void Sofa(MeridianTenantPlacement fit, Transform room, int floor, float x, float z, float yaw = 0)
        {
            Transform item = fit.Assembly("ShowroomSofa", room, floor, x, z, new Vector3(2.8f, 1.04f, 1.12f), yaw);
            if (item == null) return;
            Part(item, "TimberBase", new(0, .28f, 0), new(2.65f, .30f, 1.0f), "Wood");
            foreach (float c in new[] { -.88f, 0f, .88f })
            {
                Part(item, "IndividualSeatCushion", new(c, .52f, -.07f), new(.84f, .20f, .9f), "Upholstery");
                Part(item, "LooseBackCushion", new(c, .81f, .42f), new(.84f, .46f, .22f), "Upholstery");
            }
            foreach (float side in new[] { -1f, 1f })
            {
                Part(item, "PaddedArm", new(side * 1.3f, .62f, 0), new(.2f, .6f, 1.1f), "Upholstery");
                foreach (float end in new[] { -.4f, .4f }) Part(item, "Leg", new(side * 1.13f, .065f, end), new(.10f, .13f, .10f), "Wood");
            }
        }

        public static void CoffeeTable(MeridianTenantPlacement fit, Transform room, int floor, float x, float z)
        {
            Transform item = fit.Assembly("CoffeeTable", room, floor, x, z, new Vector3(1.6f, .53f, .82f));
            if (item == null) return;
            Part(item, "TableTop", new(0, .48f, 0), new(1.6f, .1f, .82f), "Wood");
            Part(item, "LowerShelf", new(0, .16f, 0), new(1.4f, .055f, .66f), "Wood");
            foreach (float a in new[] { -.66f, .66f })
            foreach (float b in new[] { -.3f, .3f }) Part(item, "Leg", new(a, .23f, b), new(.075f, .46f, .075f), "Wood");
        }

        public static void Armchair(MeridianTenantPlacement fit, Transform room, int floor, float x, float z, float yaw)
        {
            Transform item = fit.Assembly("ShowroomArmchair", room, floor, x, z, new Vector3(.96f, 1.0f, .96f), yaw);
            if (item != null) Chair(item, Vector3.zero, true);
        }

        public static void DiningSet(MeridianTenantPlacement fit, Transform room, int floor, float x, float z)
        {
            Transform item = fit.Assembly("DiningTableAndChairs", room, floor, x, z, new Vector3(3f, 1.05f, 2.7f));
            if (item == null) return;
            Part(item, "DiningTop", new(0, .78f, 0), new(2.2f, .1f, 1.05f), "Wood");
            foreach (float a in new[] { -.94f, .94f })
            foreach (float b in new[] { -.4f, .4f }) Part(item, "TableLeg", new(a, .365f, b), new(.09f, .73f, .09f), "Wood");
            foreach (float a in new[] { -.65f, .65f })
            foreach (float b in new[] { -1f, 1f })
            {
                Transform seat = Group("DiningChair", item);
                seat.localPosition = new Vector3(a, 0, b);
                seat.localRotation = Quaternion.Euler(0, b < 0 ? 0 : 180, 0);
                Chair(seat, Vector3.zero, false);
            }
        }

        public static void Bed(MeridianTenantPlacement fit, Transform room, int floor, float x, float z)
        {
            Transform item = fit.Assembly("DisplayBed", room, floor, x, z, new Vector3(2.25f, 1.15f, 3f));
            if (item == null) return;
            Part(item, "BedFrame", new(0, .25f, 0), new(2.16f, .35f, 2.82f), "Wood");
            Part(item, "BareMattress", new(0, .51f, -.05f), new(2.06f, .26f, 2.66f), "Plaster");
            Part(item, "Headboard", new(0, .74f, 1.43f), new(2.25f, .82f, .14f), "Wood");
            foreach (float side in new[] { -.55f, .55f }) Part(item, "DustyPillow", new(side, .7f, .93f), new(.82f, .16f, .54f), "Upholstery");
            Part(item, "FoldedBedRunner", new(0, .665f, -.68f), new(2.04f, .04f, .58f), "Upholstery", false);
        }

        public static void FittingRoom(MeridianTenantPlacement fit, Transform room, int floor, float x, float z)
        {
            Transform item = fit.Assembly("FittingCubicle", room, floor, x, z, new Vector3(2.04f, 2.26f, 1.84f));
            if (item == null) return;
            foreach (float side in new[] { -1f, 1f }) Part(item, "SidePartition", new(side, 1.1f, 0), new(.08f, 2.2f, 1.82f), "Wood");
            Part(item, "CubicleBack", new(0, 1.1f, .87f), new(1.92f, 2.2f, .08f), "Wood");
            Part(item, "Mirror", new(0, 1.25f, .819f), new(.62f, 1.66f, .025f), "EscalatorSteel", false);
            Part(item, "CurtainTrack", new(0, 2.23f, -.86f), new(2.02f, .055f, .065f), "EscalatorSteel", false);
            Part(item, "HalfDrawnCurtain", new(-.68f, 1.22f, -.86f), new(.52f, 1.92f, .055f), "Upholstery");
            Part(item, "ChangingBench", new(0, .42f, .57f), new(1.35f, .1f, .38f), "Wood");
            foreach (float side in new[] { -.5f, .5f }) Part(item, "BenchLeg", new(side, .19f, .57f), new(.085f, .38f, .31f), "Wood");
        }

        public static void DeskPartition(MeridianTenantPlacement fit, Transform room, int floor, float x, float z)
        {
            Transform item = fit.Assembly("OfficeScreen", room, floor, x, z, new Vector3(2.5f, 1.45f, .10f));
            if (item == null) return;
            Part(item, "FabricScreen", new(0, .8f, 0), new(2.4f, 1.3f, .08f), "Upholstery");
            foreach (float side in new[] { -1.22f, 1.22f }) Part(item, "ScreenUpright", new(side, .72f, 0), new(.055f, 1.44f, .1f), "FixtureTrim");
        }

        public static void Arcade(MeridianTenantPlacement fit, Transform room, int floor, float x, float z)
        {
            Transform item = fit.Assembly("DeadArcadeMachine", room, floor, x, z, new Vector3(.82f, 1.86f, .9f), 90);
            if (item == null) return;
            Part(item, "Cabinet", new(0, .64f, 0), new(.76f, 1.28f, .72f), "StorefrontTrim");
            Part(item, "MonitorHood", new(0, 1.46f, .06f), new(.78f, .65f, .60f), "FixtureTrim");
            Part(item, "DarkMonitor", new(0, 1.47f, .373f), new(.59f, .44f, .022f), "Glass", false);
            Part(item, "ControlPanel", new(0, 1.14f, .34f), new(.81f, .09f, .40f), "EscalatorSteel");
            Part(item, "BlankMarquee", new(0, 1.81f, .37f), new(.67f, .11f, .024f), "Plaster", false);
            foreach (float side in new[] { -.22f, .18f }) Part(item, "FixedButton", new(side, 1.205f, .45f), new(.07f, .03f, .07f), "Rust", false);
        }

        public static void ActivityTable(MeridianTenantPlacement fit, Transform room, int floor, float x, float z)
        {
            Transform item = fit.Assembly("ChildrenActivityTable", room, floor, x, z, new Vector3(2.12f, .95f, 1.45f));
            if (item == null) return;
            Part(item, "ActivityTop", new(0, .59f, 0), new(2.12f, .10f, 1.35f), "Wood");
            foreach (float a in new[] { -.9f, .9f })
            foreach (float b in new[] { -.55f, .55f }) Part(item, "ShortLeg", new(a, .27f, b), new(.12f, .54f, .12f), "Wood");
            for (int i = 0; i < 5; i++) Part(item, "AbandonedBuildingBlock", new(-.7f + i * .33f, .68f + (i % 2) * .07f, .09f),
                new(.2f, .16f + (i % 2) * .14f, .22f), i % 2 == 0 ? "Upholstery" : "Tile", false);
        }

        public static void EmptyPackaging(Transform shelf)
        {
            for (int i = 0; i < 4; i++) Part(shelf, "EmptyToyBox", new(-.64f + i * .41f, .73f + (i % 2) * .94f, .06f),
                new(.28f, .24f, .32f), i % 2 == 0 ? "BoardWood" : "Upholstery", false);
        }

        private static void Chair(Transform item, Vector3 at, bool padded)
        {
            string finish = padded ? "Upholstery" : "Wood";
            float width = padded ? .82f : .62f;
            Part(item, "Seat", at + new Vector3(0, .47f, 0), new(width, .14f, .68f), finish);
            Part(item, "Back", at + new Vector3(0, .77f, -.3f), new(width, .46f, .12f), finish);
            foreach (float a in new[] { -.25f, .25f })
            foreach (float b in new[] { -.24f, .24f }) Part(item, "ChairLeg", at + new Vector3(a, .2f, b), new(.08f, .4f, .08f), "Wood");
            if (padded) foreach (float side in new[] { -.43f, .43f }) Part(item, "Arm", at + new Vector3(side, .61f, 0), new(.10f, .30f, .86f), finish);
        }

        public static GameObject Part(Transform parent, string name, Vector3 at, Vector3 size, string finish, bool solid = true)
            => Box(name, parent, at, size, MeridianArt.Material(finish), solid);
    }
}
