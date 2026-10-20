using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using F = Abandoned.EditorTools.TutorialHouseFurniture;
using P = Abandoned.EditorTools.TutorialHouseFittings;
using L = Abandoned.EditorTools.TutorialHouseLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What was left in each room when the family went: the living room still dressed for Christmas, the kitchen run,
    /// the dining set, the bedrooms, both bathrooms, the laundry alcove. Sparse on purpose (2026-10-21, the user: "it's
    /// so tight, you get stuck everywhere"): every piece stands against a wall, and the hauling routes keep 2 m or more
    /// (the band's south half from the hallway to the front door, round the dining table for the fridge, the north
    /// bedroom to the hallway for the clock). Salvage sits on these surfaces (TutorialHouseFittings).
    /// </summary>
    public static class TutorialHouseRooms
    {
        public static void Build(Transform root, TutorialHouseArt art, TutorialHouseStructure.Result structure)
        {
            Transform rooms = Group("Rooms", root);
            Living(Group("Living", rooms), art);
            Kitchen(Group("Kitchen", rooms), art);
            Dining(Group("Dining", rooms), art);
            Laundry(Group("Laundry", rooms), art);
            BedroomA(Group("BedroomA", rooms), art);
            BedroomB(Group("BedroomB", rooms), art);
            HallBath(Group("HallBath", rooms), art);
            Ensuite(Group("Ensuite", rooms), art);
            Master(Group("Master", rooms), art);
            Hallway(Group("Hallway", rooms), art);
            TutorialHouseDetails.Rooms(rooms, art);
            BindToFloors(rooms, structure);
        }

        private static Vector3 At(float x, float z, float y = L.Floor) => new(x, y, z);
        private static Vector3 Up(Vector3 floor, float y) => new(floor.x, y, floor.z);

        // Sofa and TV face each other across the room's north half; the band's south half (the hallway-to-front-door
        // route) stays open, 4 m wide.
        private static void Living(Transform r, TutorialHouseArt art)
        {
            F.Sofa(r, "Sofa", P.Sofa, 180f, art);
            F.Table(r, "SideTable", P.SideTable, 0f, new Vector3(.45f, P.SideTableTop, .45f), art.Oak, art.Oak);
            F.Table(r, "CoffeeTable", P.CoffeeTable, 0f, new Vector3(1.1f, P.CoffeeTableTop, .6f), art.OakDark, art.OakDark);
            F.Armchair(r, "Armchair_West", P.ArmchairWest, 90f, art);
            F.Armchair(r, "Armchair_East", P.ArmchairEast, -90f, art);
            F.Table(r, "TvStand", P.TvStand, 0f, new Vector3(1.4f, P.TvStandTop, .45f), art.OakDark, art.OakDark);
            F.ChristmasTree(r, At(L.Living.xMin + .8f, L.BandNorth - .8f), art);
            F.Boxes(r, "EvictionBoxes", At(L.HouseEast - .5f, L.BandNorth - .5f), 10f, 3, art);
            F.Rug(r, "LivingRug", At(23f, 11.9f), new Vector2(3.2f, 2.4f), 0f, art.Rug);
            // Coats still on the hooks beside the front door, boots on the mat with the snow long melted off them.
            float wall = L.HouseEast - L.ExteriorWall * .5f;
            float hooks = L.FrontDoorZ + L.FrontDoorWidth * .5f + 1.4f;
            Box("CoatHookRail", r, At(wall - .02f, hooks, 1.75f), new Vector3(.04f, .06f, .8f), art.OakDark, false);
            for (int i = 0; i < 3; i++)
                Box("Coat", r, At(wall - .09f, hooks - .28f + i * .28f, 1.35f), new Vector3(.1f, .75f, .26f), i == 1 ? art.Bedding : art.SofaFabric, false);
            Box("DoorMat", r, At(wall - .5f, L.FrontDoorZ, .006f), new Vector3(.7f, .012f, 1.4f), art.Rubber, false);
            foreach (float z in new[] { hooks - .2f, hooks + .05f })
                Box("Boot", r, At(wall - .15f, z, .1f), new Vector3(.28f, .2f, .11f), art.Rubber, false);
            Box("Thermostat", r, At(L.Living.xMin + .08f, 9.5f, 1.5f), new Vector3(.03f, .12f, .12f), art.Trim, false);
        }

        // Counters on the north and west walls, the island in the middle: 1.3 m or more all round it.
        private static void Kitchen(Transform r, TutorialHouseArt art)
        {
            F.Counter(r, "Counter_North", P.CounterNorth, 180f, P.CounterNorthLength, art, uppers: true, sink: true);
            F.Counter(r, "Counter_West", P.CounterWest, 90f, P.CounterWestLength, art, uppers: true);
            F.Counter(r, "Island", P.Island, 0f, P.IslandLength, art, uppers: false);
            // The range hood over the stove's slot (the stove itself is salvage), the fridge's nook beside the run.
            Box("RangeHood", r, Up(P.Stove, 1.95f) + Vector3.left * .05f, new Vector3(.5f, .2f, .8f), art.Chrome, false);
            Box("FridgeNookPanel", r, At(P.Fridge.x + .5f, P.Fridge.z, 1.2f), new Vector3(.04f, 2.4f, .7f), art.Cabinet);
            Box("CabinetOverFridge", r, At(P.Fridge.x, P.Fridge.z + .1f, 2.3f), new Vector3(.9f, .5f, .5f), art.Cabinet, false);
            F.Rug(r, "SinkMat", At(10.6f, P.CounterNorth.z - .65f), new Vector2(.9f, .5f), 0f, art.Bedding);
        }

        private static void Dining(Transform r, TutorialHouseArt art)
        {
            F.DiningSet(r, P.DiningTable, art);
            F.Dresser(r, "Sideboard", P.Sideboard, 180f, 1.4f, art);
            F.Boxes(r, "DiningBoxes", At(L.Dining.xMax - .6f, L.BandNorth - .5f), -6f, 7, art);
            Box("PendantRod", r, Up(P.DiningTable, L.Ceiling - .3f), new Vector3(.02f, .6f, .02f), art.Brass, false);
        }

        private static void Laundry(Transform r, TutorialHouseArt art)
        {
            Box("WallShelf", r, P.LaundryShelf, new Vector3(2f, .03f, .3f), art.Oak, false);
            foreach (float x in new[] { P.LaundryShelf.x - .9f, P.LaundryShelf.x + .9f })
                Box("ShelfBracket", r, new Vector3(x, P.LaundryShelf.y - .1f, P.LaundryShelf.z - .04f), new Vector3(.03f, .2f, .2f), art.DarkMetal, false);
            Box("DetergentJug", r, new Vector3(P.LaundryShelf.x - .6f, P.LaundryShelf.y + .14f, P.LaundryShelf.z), new Vector3(.2f, .26f, .12f), art.OrnamentRed, false);
            Box("LaundryBasket", r, At(L.Laundry.xMax - .45f, L.Laundry.yMin + .35f, .2f), new Vector3(.4f, .4f, .3f), art.Blind, false);
        }

        // The clock's corner and the run from it to the hallway door stay empty.
        private static void BedroomA(Transform r, TutorialHouseArt art)
        {
            F.Bed(r, "TwinBed", P.TwinBed, 180f, 1f, 2f, art.Bedding, art);
            F.Nightstand(r, "Nightstand", P.NightstandA, 180f, art);
            F.Dresser(r, "Dresser", At(.4f, 15.2f), 90f, 1f, art);
            F.Desk(r, "Desk", At(5f, L.WestWing.yMax - .42f), 180f, art);
            F.Rug(r, "BedroomARug", At(3.6f, 14.2f), new Vector2(2f, 1.6f), 0f, art.Curtain);
            F.Boxes(r, "BedroomABoxes", At(.55f, L.WestWing.yMax - .5f), 4f, 11, art);
        }

        // Bed and nightstand stand on the rotten patch (they fall with it); the dresser and boxes on the floor that holds.
        private static void BedroomB(Transform r, TutorialHouseArt art)
        {
            F.Bed(r, "QueenBed", P.QueenBedB, 90f, 1.5f, 2f, art.BeddingB, art);
            F.Nightstand(r, "Nightstand", P.NightstandB, 90f, art);
            F.Dresser(r, "Dresser", At(6.2f, .4f), 0f, 1f, art);
            F.Boxes(r, "BedroomBBoxes", At(.55f, L.BedroomB.yMax - .5f), 0f, 19, art);
            F.Rug(r, "BedroomBRug", At(3.2f, 2.9f), new Vector2(1.6f, 2.2f), 0f, art.Rug);
        }

        private static void HallBath(Transform r, TutorialHouseArt art)
        {
            Rect b = L.HallBath;
            F.BuiltInTub(r, "Tub", At(b.xMin + .5f, 8.7f), 90f, 1.6f, art);
            F.Toilet(r, "Toilet", At(2.4f, b.yMin + .42f), 0f, art);
            F.Vanity(r, "Vanity", At(3.4f, b.yMax - .34f), 180f, 1f, art);
            Box("TowelBar", r, At(1.2f, b.yMax - .1f, 1.2f), new Vector3(.5f, .03f, .03f), art.Chrome, false);
            F.Rug(r, "BathMat", At(1.5f, 8.7f), new Vector2(.5f, .8f), 0f, art.Bedding);
        }

        private static void Ensuite(Transform r, TutorialHouseArt art)
        {
            Rect e = L.Ensuite;
            F.BuiltInTub(r, "Tub", At(15.75f, e.yMin + .5f), 0f, 1.6f, art);
            F.Toilet(r, "Toilet", At(e.xMin + .47f, 2.4f), 90f, art);
            F.Vanity(r, "Vanity", P.EnsuiteVanity, 180f, 1.2f, art);
            F.Rug(r, "BathMat", At(16f, 2.2f), new Vector2(.8f, .5f), 0f, art.Pillow);
        }

        private static void Master(Transform r, TutorialHouseArt art)
        {
            F.Bed(r, "QueenBed", P.MasterBed, -90f, 1.5f, 2f, art.Bedding, art);
            F.Nightstand(r, "Nightstand_South", At(L.HouseEast - .37f, P.MasterBed.z - 1.15f), -90f, art);
            F.Nightstand(r, "Nightstand_North", At(L.HouseEast - .37f, P.MasterBed.z + 1.15f), -90f, art);
            F.Dresser(r, "Dresser", P.MasterDresser, 0f, 1.3f, art);
            F.Armchair(r, "ReadingChair", At(23.2f, .8f), -20f, art);
            F.Rug(r, "MasterRug", At(23f, 3f), new Vector2(2.4f, 2.8f), 0f, art.Curtain);
        }

        private static void Hallway(Transform r, TutorialHouseArt art)
        {
            Rect h = L.Hallway;
            F.Rug(r, "Runner", At(h.center.x, h.center.y), new Vector2(1.1f, 3.8f), 0f, art.Rug);
            Box("LinenShelf", r, At(h.xMax - .1f, 10.2f, 1.7f), new Vector3(.08f, .03f, 1f), art.Trim, false);
        }

        // Furniture on the south bedroom's rotten patch falls with it; everything else stands on floors that hold.
        private static void BindToFloors(Transform rooms, TutorialHouseStructure.Result structure)
        {
            foreach (Transform room in rooms)
            foreach (Transform piece in room)
            {
                StructuralSection under = structure.Under(piece.position);
                if (under == null || !under.CanCollapse) continue;
                var prop = piece.GetComponent<SectionProp>() ?? piece.gameObject.AddComponent<SectionProp>();
                prop.EditorSetup(under);
            }
        }
    }
}
