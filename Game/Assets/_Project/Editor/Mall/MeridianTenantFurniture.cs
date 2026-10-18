using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Individual recognisable furnishings, grouped into showroom arrangements rather than boxed display bays.</summary>
    public static class MeridianTenantFurniture
    {
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

        public static GameObject Part(Transform parent, string name, Vector3 at, Vector3 size, string finish, bool solid = true)
            => Box(name, parent, at, size, MeridianArt.Material(finish), solid);
    }
}
