using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using F = Abandoned.EditorTools.TutorialHouseFurniture;
using L = Abandoned.EditorTools.TutorialHouseLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The unfinished basement: the gas furnace and its ducts (pilot long out), the water heater, steel shelving,
    /// a workbench in the strip under the kitchen, holiday bins. Two places stay empty on purpose: under the south bedroom's
    /// rotten floor (someone lands there) and under the hallway (the support jack's post stands there).
    /// </summary>
    public static class TutorialHouseBasement
    {
        /// <summary>The furnace's place along the west wall; the breaker panel hangs on the same wall just south of it.</summary>
        public const float FurnaceZ = 13.8f;

        public static void Build(Transform root, TutorialHouseArt art)
        {
            Transform b = Group("BasementDressing", root);
            float y = L.BasementFloor;

            Transform furnace = F.Piece(b, "Furnace", new Vector3(1.5f, y, FurnaceZ), 90f);
            Box("Cabinet", furnace, new Vector3(0f, .75f, 0f), new Vector3(.8f, 1.5f, .75f), art.Paint, false);
            Box("BurnerDoor", furnace, new Vector3(0f, .45f, .38f), new Vector3(.6f, .5f, .02f), art.DarkMetal, false);
            Box("PilotWindow", furnace, new Vector3(.15f, .5f, .392f), new Vector3(.06f, .04f, .01f), art.Window, false);
            Box("Plenum", furnace, new Vector3(0f, 1.75f, 0f), new Vector3(.7f, .5f, .7f), art.Chrome, false);
            // The trunk duct runs east under the joists toward the hallway; the return drops in beside the plenum.
            Box("SupplyDuct", furnace, new Vector3(0f, 1.95f, 1.45f), new Vector3(.4f, .25f, 2.8f), art.Chrome, false);
            Box("ReturnDuct", furnace, new Vector3(-.3f, 1.9f, -.25f), new Vector3(.35f, .3f, .55f), art.Chrome, false);
            Primitive(PrimitiveType.Cylinder, "Flue", furnace, new Vector3(-.2f, 1.95f, 0f), new Vector3(.15f, .2f, .15f), art.DarkMetal, false);
            Box("GasLine", furnace, new Vector3(.42f, .6f, -.2f), new Vector3(.03f, 1.2f, .03f), art.Brass, false);
            F.Solid(furnace, new Vector3(.8f, 1.5f, .75f), Core.SurfaceMaterial.Metal);

            Transform heater = F.Piece(b, "WaterHeater", new Vector3(3.6f, y, L.BasementMain.yMax - .55f), 0f);
            Primitive(PrimitiveType.Cylinder, "Tank", heater, new Vector3(0f, .75f, 0f), new Vector3(.55f, .75f, .55f), art.Paint, false);
            Primitive(PrimitiveType.Cylinder, "TopPipe", heater, new Vector3(0f, 1.7f, 0f), new Vector3(.08f, .2f, .08f), art.Brass, false);
            F.Solid(heater, new Vector3(.56f, 1.5f, .56f), Core.SurfaceMaterial.Metal);

            Transform shelf = F.Shelf(b, "SteelShelving", new Vector3(.45f, y, 16.2f), 90f, 1.6f, 1.8f, 4, art.Steel, art);
            for (int i = 0; i < 6; i++)
                Primitive(PrimitiveType.Cylinder, "PaintCan", shelf, new Vector3(-.55f + i * .22f, .1f + (i % 3) * .55f + .1f, 0f), new Vector3(.16f, .1f, .16f),
                    i % 2 == 0 ? art.Paint : art.Rust, false);
            F.Boxes(b, "HolidayBins", new Vector3(5.4f, y, L.BasementMain.yMax - .45f), 0f, 31, art);
            F.Boxes(b, "MoreBins", new Vector3(.45f, y, 8.2f), 90f, 37, art);

            Transform bench = F.Piece(b, "Workbench", new Vector3(L.BasementStrip.xMax - .34f, y, 9.6f), -90f);
            Box("Top", bench, new Vector3(0f, .88f, 0f), new Vector3(1.6f, .06f, .6f), art.Oak, false);
            Box("Shelf", bench, new Vector3(0f, .25f, 0f), new Vector3(1.5f, .04f, .55f), art.Oak, false);
            foreach (float x in new[] { -.75f, .75f })
            foreach (float z in new[] { -.25f, .25f })
                Box("Leg", bench, new Vector3(x, .44f, z), new Vector3(.06f, .88f, .06f), art.Oak, false);
            Box("Pegboard", bench, new Vector3(0f, 1.5f, -.32f), new Vector3(1.6f, .9f, .02f), art.Cardboard, false);
            Box("Vise", bench, new Vector3(.6f, .97f, .2f), new Vector3(.18f, .12f, .14f), art.DarkMetal, false);
            F.Solid(bench, new Vector3(1.6f, .92f, .62f));

            // A dehumidifier, an old sump pit cover, and the cold: frost on the window wells.
            Box("SumpCover", b, new Vector3(9.3f, y + .01f, 7.6f), new Vector3(.5f, .02f, .5f), art.DarkMetal, false);
            Box("WindowWell", b, new Vector3(.06f, -.55f, 2.5f), new Vector3(.04f, .4f, .9f), art.Ice, false);
            Box("WindowWell", b, new Vector3(.06f, -.55f, 14.4f), new Vector3(.04f, .4f, .9f), art.Ice, false);
        }
    }
}
