using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using M = Abandoned.EditorTools.TutorialHouseModels;
using L = Abandoned.EditorTools.TutorialHouseLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Fixed building hardware with no new floor obstacles or salvage lookalikes.</summary>
    public static class TutorialHouseServiceDetails
    {
        public static void Build(Transform root, TutorialHouseArt art)
        {
            Basement(root.Find("BasementDressing"), art);
            Transform outside = Group("ExteriorHardware", root.Find("Walls"));
            foreach (var at in new[] { new Vector3(26.69f, 0, .28f), new Vector3(26.69f, 0, 13.72f), new Vector3(-.19f, 0, 17.22f) })
            {
                Transform drain = Group("Downspout", outside);
                drain.position = at;
                Box("Pipe", drain, new Vector3(0, 1.45f, 0), new Vector3(.075f, 2.7f, .075f), art.Trim, false);
                foreach (float y in new[] { .55f, 1.6f, 2.6f })
                    Box("PipeStrap", drain, new Vector3(.006f, y, 0), new Vector3(.09f, .025f, .09f), art.DarkMetal, false);
                float sign = at.x > 0 ? 1 : -1;
                Pipe(drain, "TopElbow", new Vector3(0, 2.8f, 0), new Vector3(sign*.3f, 3.11f, 0), .039f, art.Trim);
                Pipe(drain, "DrainShoe", new Vector3(0, .12f, 0), new Vector3(sign*.25f, .02f, 0), .039f, art.Trim);
            }
            Transform number = Group("Number14", outside);
            number.SetPositionAndRotation(new Vector3(26.66f, 1.75f, 8.95f), Quaternion.Euler(0, 90, 0));
            Box("Plaque", number, Vector3.zero, new Vector3(.30f, .20f, .025f), art.DarkMetal, false);
            Box("One", number, new Vector3(-.07f, 0, .018f), new Vector3(.018f, .13f, .01f), art.Brass, false);
            Box("OneFoot", number, new Vector3(-.07f, -.065f, .018f), new Vector3(.06f, .012f, .01f), art.Brass, false);
            Box("FourStem", number, new Vector3(.08f, 0, .018f), new Vector3(.018f, .13f, .01f), art.Brass, false);
            Box("FourCrossbar", number, new Vector3(.05f, -.015f, .018f), new Vector3(.075f, .016f, .01f), art.Brass, false);
            Box("FourUpright", number, new Vector3(.018f, .025f, .018f), new Vector3(.018f, .085f, .01f), art.Brass, false);
            Box("DoorbellPlate", outside, new Vector3(26.66f, 1.28f, 9.12f), new Vector3(.025f, .12f, .055f), art.Brass, false);
            Box("DoorbellButton", outside, new Vector3(26.68f, 1.28f, 9.12f), new Vector3(.015f, .04f, .03f), art.Porcelain, false);
            // Louver geometry replaces the blank squares at the roof peaks.
            foreach (var renderer in root.Find("Roof").GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer.name != "AtticVent") continue;
                Vector3 at = renderer.transform.position;
                bool wing = renderer.transform.parent.name == "WestWingRoof";
                float yaw = wing ? (at.z > L.WestWing.center.y ? 0 : 180) : (at.x > 18 ? 90 : -90);
                M.Place("Vent", outside, at, yaw, art, new Vector3(1.2f, 1.8f, 1));
                Object.DestroyImmediate(renderer.gameObject);
            }
        }

        private static void Basement(Transform basement, TutorialHouseArt art)
        {
            Transform furnace = basement.Find("Furnace");
            M.Place("Vent", furnace, new Vector3(0, 1.13f, .39f), 0, art, new Vector3(1.35f, 1.3f, 1));
            Box("ServiceDoorFrame", furnace, new Vector3(0, .45f, .394f), new Vector3(.64f, .54f, .012f), art.Paint, false);
            Box("BurnerInset", furnace, new Vector3(0, .45f, .404f), new Vector3(.57f, .47f, .012f), art.DarkMetal, false);
            Box("ServicePull", furnace, new Vector3(0, .65f, .43f), new Vector3(.13f, .02f, .03f), art.Chrome, false);
            foreach (float x in new[] { -.28f, .28f })
            foreach (float y in new[] { .21f, .69f })
                Box("ServiceScrew", furnace, new Vector3(x, y, .416f), Vector3.one*.015f, art.Chrome, false);
            for (int i = 0; i < 4; i++)
                Box("DuctJoint", furnace, new Vector3(0, 1.95f, .35f+i*.7f), new Vector3(.418f, .268f, .022f), art.DarkMetal, false);
            Pipe(furnace, "GasValveStem", new Vector3(.43f, .72f, -.2f), new Vector3(.53f, .72f, -.2f), .015f, art.Brass);
            Box("GasValve", furnace, new Vector3(.53f, .72f, -.2f), new Vector3(.025f, .12f, .02f), art.OrnamentRed, false);
            Transform heater = basement.Find("WaterHeater");
            foreach (float y in new[] { .055f, 1.45f })
                Primitive(PrimitiveType.Cylinder, "TankSeam", heater, new Vector3(0, y, 0), new Vector3(.565f, .012f, .565f), art.Chrome, false);
            Box("ThermostatHousing", heater, new Vector3(0, .48f, .275f), new Vector3(.16f, .20f, .04f), art.DarkMetal, false);
            Primitive(PrimitiveType.Sphere, "ThermostatDial", heater, new Vector3(0, .50f, .305f), new Vector3(.065f, .065f, .02f), art.Trim, false);
            foreach (float side in new[] { -1f, 1f })
            {
                Pipe(heater, "WaterPipe", new Vector3(side*.16f, 1.48f, 0), new Vector3(side*.16f, 1.90f, 0), .018f, art.Brass);
                Box("IsolationHandle", heater, new Vector3(side*.16f, 1.72f, .035f), new Vector3(.09f, .018f, .02f), side < 0 ? art.Bedding : art.OrnamentRed, false);
            }
            Transform bench = basement.Find("Workbench");
            for (int row = 0; row < 7; row++)
            for (int column = 0; column < 15; column++)
                Box("PegHole", bench, new Vector3(-.7f+column*.1f, 1.15f+row*.11f, -.307f), new Vector3(.009f, .009f, .002f), art.DarkMetal, false);
            Pipe(bench, "ViseScrew", new Vector3(.6f, .95f, .2f), new Vector3(.6f, .95f, .32f), .013f, art.Chrome);
            Pipe(bench, "ViseHandle", new Vector3(.6f, .86f, .32f), new Vector3(.6f, 1.04f, .32f), .009f, art.Chrome);
        }

        private static void Pipe(Transform parent, string name, Vector3 a, Vector3 b, float radius, Material material)
        {
            GameObject pipe = Primitive(PrimitiveType.Cylinder, name, parent, (a+b)*.5f, new Vector3(radius*2, Vector3.Distance(a,b)*.5f, radius*2), material, false);
            pipe.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b-a);
        }
    }
}
