using UnityEngine;
using static Abandoned.EditorTools.SalvageModelFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.9 fixed store fixtures that take the place of the loot models the mall used as fittings. Each keeps
    /// the footprint (and shelf heights) of the item it replaces, so the stock authored on it still sits right,
    /// but reads as part of the building: floor plinths, bolt plates, back panels, conduit, no wheels.
    /// </summary>
    internal static class SalvageFixtureModels
    {
        public static bool Build(string id, Transform root, int lod)
        {
            switch (id)
            {
                case "fixed_gondola_shelving": Gondola(root, lod); return true;
                case "fixed_garment_rail": GarmentRail(root, lod); return true;
                case "fixed_service_counter": ServiceCounter(root, lod); return true;
                case "plastic_pallet": PlasticPallet(root, lod); return true;
                case "waste_compactor": WasteCompactor(root, lod); return true;
                default: return false;
            }
        }

        private static int Sides(int lod) => lod == 0 ? 12 : lod == 1 ? 8 : 6;

        private static void Gondola(Transform p, int lod)
        {
            // Shelf tops match modular_shelving (.141 + .54 n): stock was placed against those heights.
            Box(p, "Kick plinth", new Vector3(0, .05f, -.02f), new Vector3(1.10f, .10f, .40f), "Paint");
            Box(p, "Solid back panel", new Vector3(0, .96f, .207f), new Vector3(1.10f, 1.72f, .026f), "Paint");
            for (int x = -1; x <= 1; x += 2)
                Box(p, "Rear upright", new Vector3(x * .535f, .96f, .19f), new Vector3(.05f, 1.92f, .05f), "Metal");
            for (int i = 0; i < 4; i++)
            {
                float y = .125f + i * .54f;
                Box(p, "Steel gondola shelf", new Vector3(0, y, -.01f), new Vector3(1.07f, .032f, .43f), "Metal");
                if (lod == 2) continue;
                Box(p, "Price rail", new Vector3(0, y - .028f, -.222f), new Vector3(1.07f, .05f, .012f), "Plastic");
                for (int x = -1; x <= 1; x += 2)
                    Box(p, "Shelf bracket", new Vector3(x * .50f, y - .045f, 0), new Vector3(.02f, .06f, .40f), "Metal");
            }
            if (lod == 2) return;
            Box(p, "Header panel", new Vector3(0, 1.86f, .19f), new Vector3(1.10f, .12f, .06f), "Paint");
            if (lod != 0) return;
            for (int x = -1; x <= 1; x += 2)
                Box(p, "Floor bolt plate", new Vector3(x * .535f, .004f, .19f), new Vector3(.12f, .008f, .10f), "Metal");
            for (int row = 0; row < 6; row++)
                for (int column = 0; column < 7; column++)
                    Box(p, "Pegboard hole", new Vector3(-.42f + column * .14f, .40f + row * .24f, .193f),
                        new Vector3(.012f, .012f, .002f), "Rubber");
        }

        private static void GarmentRail(Transform p, int lod)
        {
            int s = Sides(lod);
            Box(p, "Floor fixing rail", new Vector3(0, .012f, 0), new Vector3(1.36f, .024f, .30f), "Metal");
            for (int side = -1; side <= 1; side += 2)
                Box(p, "Square post", new Vector3(side * .62f, .86f, 0), new Vector3(.05f, 1.70f, .05f), "Paint");
            Bar(p, "Hanging rail", new Vector3(-.66f, 1.63f, 0), new Vector3(.66f, 1.63f, 0), .032f, "Metal", s);
            Box(p, "Header bar", new Vector3(0, 1.74f, 0), new Vector3(1.36f, .08f, .04f), "Paint");
            Box(p, "Lower stretcher", new Vector3(0, .35f, 0), new Vector3(1.24f, .035f, .035f), "Metal");
            if (lod == 2) return;
            for (int side = -1; side <= 1; side += 2)
                Box(p, "Post base plate", new Vector3(side * .62f, .03f, 0), new Vector3(.14f, .012f, .14f), "Metal");
            if (lod != 0) return;
            for (int side = -1; side <= 1; side += 2)
                for (int z = -1; z <= 1; z += 2)
                    Cylinder(p, "Anchor bolt", new Vector3(side * .62f + .045f, .04f, z * .045f), new Vector3(.018f, .012f, .018f), "Metal", null, 6);
        }

        private static void ServiceCounter(Transform p, int lod)
        {
            // Stone top and painted panels instead of the loot counter's timber and plastic.
            Box(p, "Recessed plinth", new Vector3(0, .05f, 0), new Vector3(1.66f, .10f, .62f), "Rubber");
            Box(p, "Front panel", new Vector3(0, .58f, .36f), new Vector3(1.72f, .96f, .075f), "Paint");
            for (int side = -1; side <= 1; side += 2)
                Box(p, "End panel", new Vector3(side * .81f, .58f, 0), new Vector3(.10f, .96f, .71f), "Paint");
            Box(p, "Stone counter top", new Vector3(0, 1.08f, 0), new Vector3(1.80f, .06f, .82f), "Concrete");
            Box(p, "Return wing top", new Vector3(.58f, .87f, -.55f), new Vector3(.62f, .045f, .39f), "Concrete");
            Box(p, "Return wing end", new Vector3(.81f, .48f, -.54f), new Vector3(.10f, .76f, .39f), "Paint");
            Cylinder(p, "Floor power conduit", new Vector3(-.70f, .30f, -.28f), new Vector3(.06f, .60f, .06f), "Cable", null, Sides(lod));
            if (lod == 2) return;
            Box(p, "Under-counter shelf", new Vector3(.15f, .40f, .02f), new Vector3(1.25f, .035f, .55f), "Wood");
            Box(p, "Conduit floor box", new Vector3(-.70f, .02f, -.28f), new Vector3(.16f, .04f, .16f), "Metal");
            if (lod != 0) return;
            Cylinder(p, "Cable grommet", new Vector3(-.55f, 1.112f, .1f), new Vector3(.07f, .008f, .07f), "Rubber", null, 8);
            Bar(p, "Cut screen bracket", new Vector3(-.2f, 1.11f, .15f), new Vector3(-.2f, 1.31f, .15f), .03f, "Metal", 8);
        }

        private static void PlasticPallet(Transform p, int lod)
        {
            // Same 1.24 x 0.146 x 1.0 m as the wooden loot pallet, moulded plastic in one piece.
            Box(p, "Moulded deck", new Vector3(0, .128f, 0), new Vector3(1.24f, .036f, 1.0f), "Plastic");
            for (int x = -1; x <= 1; x++)
            {
                Box(p, "Bottom runner", new Vector3(x * .50f, .006f, 0), new Vector3(.16f, .012f, 1.0f), "Plastic");
                for (int z = -1; z <= 1; z++)
                    Box(p, "Moulded foot", new Vector3(x * .50f, .061f, z * .40f), new Vector3(.16f, .11f, .15f), "Plastic");
            }
            if (lod == 2) return;
            for (int i = 0; i < 6; i++)
                Box(p, "Deck grip rib", new Vector3(-.5f + i * .2f, .1475f, 0), new Vector3(.03f, .003f, .96f), "Rubber");
        }

        private static void WasteCompactor(Transform p, int lod)
        {
            int s = Sides(lod);
            Box(p, "Anchored base frame", new Vector3(0, .06f, 0), new Vector3(2.6f, .12f, 1.36f), "Metal");
            Box(p, "Compaction chamber", new Vector3(-.25f, .82f, 0), new Vector3(2.0f, 1.42f, 1.30f), "Paint");
            Box(p, "Ram housing", new Vector3(1.0f, .57f, 0), new Vector3(.55f, .90f, 1.20f), "Metal");
            Bar(p, "Hydraulic cylinder", new Vector3(.82f, 1.0f, 0), new Vector3(1.18f, 1.38f, 0), .12f, "Metal", s);
            Box(p, "Charging chute lid", new Vector3(-.25f, 1.565f, 0), new Vector3(2.0f, .07f, 1.36f), "Plastic");
            Box(p, "Control box", new Vector3(1.12f, 1.25f, .62f), new Vector3(.30f, .40f, .15f), "Safety");
            Bar(p, "Power conduit", new Vector3(1.12f, 1.05f, .66f), new Vector3(1.12f, .12f, .66f), .04f, "Cable", s);
            if (lod == 2) return;
            Box(p, "Hazard stripe", new Vector3(-.25f, .30f, .655f), new Vector3(2.0f, .10f, .01f), "Safety");
            if (lod != 0) return;
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Box(p, "Floor anchor plate", new Vector3(x * 1.22f, .005f, z * .60f), new Vector3(.18f, .01f, .18f), "Metal");
            for (int i = 0; i < 2; i++)
                Cylinder(p, "Control button", new Vector3(1.06f + i * .12f, 1.3f, .70f), new Vector3(.05f, .02f, .05f), i == 0 ? "Safety" : "Rubber", new Vector3(90, 0, 0), 8);
        }
    }
}
