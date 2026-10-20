using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Tutorial;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.TutorialHouseLayout;
using P = Abandoned.EditorTools.TutorialHouseFittings;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What the lessons use: named areas (rooms, the porch, the basement), restart and monster spots, the salvage by
    /// key (real loot from the catalog, spawned by the host), the kit crates, the two sliding doors (the laundry's
    /// bypass doors and the patio slider) and three closets to hide in. Keys here are the ones the lesson assets name.
    /// </summary>
    public static class TutorialHouseGameplay
    {
        public static void Build(Transform root, TutorialHouseArt art)
        {
            Transform play = Group("Training", root);
            Areas(Group("Areas", play));
            Spots(Group("Spots", play));
            Salvage(Group("Salvage", play));
            Crates(Group("KitCrates", play), art);
            Transform doors = Group("Doors", play);
            BypassDoors(doors, art);
            PatioSlider(doors, art);
            Transform closets = Group("Closets", play);
            Closet(closets, 0, "MasterCloset", L.MasterCloset, faceSouth: true, art);
            Closet(closets, 1, "ClosetA", L.ClosetA, faceSouth: false, art);
            Closet(closets, 2, "ClosetB", L.ClosetB, faceSouth: false, art);
        }

        public static List<Vector3> ThreatSpawns() => new() { new Vector3(12f, L.Grade, -1.5f), new Vector3(-8f, L.Grade, 9f) };

        private static void Areas(Transform parent)
        {
            void Area(string key, Rect r, float y0, float y1)
            {
                Transform t = Group("Area_" + key, parent);
                Bounds b = L.Box(r, y0, y1);
                t.position = b.center;
                t.gameObject.AddComponent<TutorialArea>().EditorSetup(key, b.size);
            }
            const float roomTop = L.Ceiling;
            Area("driveway", Rect.MinMaxRect(L.Porch.xMax, 4f, L.StreetX, 20f), -1f, 3f);
            Area("porch", Rect.MinMaxRect(L.Porch.xMin, L.Porch.yMin, L.Porch.xMax + .9f, L.Porch.yMax), -.5f, 3f);
            Area("living", L.Living, -.3f, roomTop);
            Area("dining", L.Dining, -.3f, roomTop);
            Area("kitchen", L.Kitchen, -.3f, roomTop);
            Area("laundry", L.Laundry, -.3f, roomTop);
            Area("hallway", L.Hallway, -.3f, roomTop);
            Area("hall_bath", L.HallBath, -.3f, roomTop);
            Area("bedroom_a", L.BedroomA, -.3f, roomTop);
            Area("bedroom_b", L.BedroomB, -.3f, roomTop);
            Area("master", L.Master, -.3f, roomTop);
            Area("ensuite", L.Ensuite, -.3f, roomTop);
            Area("basement", Rect.MinMaxRect(L.BasementMain.xMin, L.BasementMain.yMin, L.BasementStrip.xMax, L.BasementMain.yMax), L.BasementFloor - .3f, -.3f);
            Area("patio", Rect.MinMaxRect(L.Patio.xMin, L.Patio.yMin - 2f, L.Patio.xMax, L.Patio.yMax), -.6f, 3f);
        }

        private static void Spots(Transform parent)
        {
            void Spot(string key, Vector3 at, float yaw)
            {
                Transform t = Group("Spot_" + key, parent);
                t.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
                t.gameObject.AddComponent<TutorialSpot>().EditorSetup(key);
            }
            Spot("chapter_0", new Vector3(53.5f, L.Grade, 15.2f), -90f);
            Spot("chapter_1", new Vector3(L.Porch.center.x, L.Floor, L.FrontDoorZ), -90f);
            Spot("chapter_2", new Vector3(15.2f, L.Floor, 8.2f), 0f);
            Spot("chapter_3", new Vector3(15.75f, L.Floor, 8.4f), 180f);
            Spot("chapter_4", new Vector3(10.2f, L.Floor, 8f), -90f);
            Spot("chapter_5", new Vector3(12.2f, L.Grade, 2.8f), 180f);
            Spot("chapter_6", new Vector3(16.75f, L.Floor, 8.2f), 180f);
            Spot("chapter_7", new Vector3(20.8f, L.Floor, 4.6f), 90f);
            Spot("chapter_8", new Vector3(L.Porch.center.x, L.Floor, L.FrontDoorZ), 90f);
            Spot("threat_in", new Vector3(12.4f, L.Grade, 1.2f), 0f);
            Spot("threat_patrol", new Vector3(16.75f, L.Floor, 8f), 0f);
            Spot("threat_out", new Vector3(12f, L.Grade, -8f), 180f);
            // The guide's way into the truck: the foot of the ramp, then the middle of the cargo bay's floor.
            Spot(TutorialGuides.TruckRampSpot, L.TruckRear - L.TruckForward * (TruckBuilder.RampLength + .3f), 0f);
            Spot(TutorialGuides.TruckBaySpot, L.TruckRear + L.TruckForward * (TruckBuilder.BayLength * .5f) + Vector3.up * TruckBuilder.FloorHeight, 0f);
        }

        // Real salvage from the loot catalog, by id: what each lesson hands you, plus a little extra to find.
        private static void Salvage(Transform parent)
        {
            void Item(string key, string id, int chapter, Vector3 at, float yaw = 0f)
            {
                Transform t = Group("Loot_" + key, parent);
                t.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
                t.gameObject.AddComponent<TutorialLootSpot>().EditorSetup(key, id, chapter);
            }
            Vector3 On(Vector3 floor, float top, float dx = 0f, float dz = 0f) => new(floor.x + dx, top, floor.z + dz);
            // 1: the living room. Light things to pick up, pocket and throw.
            Item("lamp", "table_lamp", 1, On(P.SideTable, P.SideTableTop));
            Item("cushion", "silk_cushion", 1, On(P.Sofa, P.SofaSeat, .55f, .05f), 180f);
            Item("cash", "cash_bundle", 1, On(P.CoffeeTable, P.CoffeeTableTop, -.2f));
            Item("phone", "smartphone", 1, On(P.ArmchairWest, P.ArmchairSeat));
            Item("tv", "flatscreen_tv", 1, On(P.TvStand, P.TvStandTop));
            // 2: the kitchen and dining room. The fragile vase, the fridge (Heavy), the stove, small things on the counters.
            Item("vase", "antique_vase", 2, On(P.DiningTable, P.DiningTableTop));
            Item("fridge", "fridge", 2, P.Fridge, 180f);
            Item("stove", "kitchen_stove", 2, P.Stove, 90f);
            Item("toaster", "toaster", 2, On(P.CounterNorth, P.CounterTop, -1.4f, .05f), 180f);
            Item("radio", "retro_radio", 2, On(P.CounterNorth, P.CounterTop, 1.3f, .08f), 180f);
            Item("teaset", "silver_tea_set", 2, On(P.Sideboard, P.SideboardTop), 180f);
            // 3: the laundry alcove behind its doors: the watch on the shelf over the machines.
            Item("watch", "gold_watch", 3, new Vector3(P.LaundryShelf.x + .7f, P.LaundryShelf.y + .02f, P.LaundryShelf.z));
            Item("washer", "washing_machine", 3, P.Washer);
            Item("dryer", "tumble_dryer", 3, P.Dryer);
            // 4: the west wing. The grandfather clock (Heavy, fragile) for the hallway; the necklace on the rotten floor.
            Item("clock", "grandfather_clock", 4, P.Clock, 90f);
            Item("books", "book_stack", 4, On(P.NightstandA, P.NightstandTop));
            Item("necklace", "pearl_necklace", 4, On(P.NightstandB, P.NightstandTop));
            // 7: the ensuite, only reachable through the master bedroom. The best thing in the house fits in a pocket.
            Item("ring", "diamond_ring", 7, On(P.EnsuiteVanity, P.VanityTop, .3f, -.04f));
            Item("painting", "small_painting", 7, On(P.MasterDresser, P.DresserTop));
        }

        private static void Crates(Transform parent, TutorialHouseArt art)
        {
            Crate(parent, 0, "hand_trolley", L.TrolleyCrate, -90f, art.OrnamentGold, art);
            Crate(parent, 1, "support_jack", L.JackCrate, -90f, art.OrnamentRed, art);
            Crate(parent, 2, "medkit", L.MedkitCrate, -90f, art.Pillow, art);
        }

        // A company kit crate: plywood, steel corners, a coloured band for what's inside, its lid off.
        private static void Crate(Transform parent, int index, string gear, Vector3 at, float yaw, Material band, TutorialHouseArt art)
        {
            Transform c = Group($"KitCrate_{gear}", parent);
            c.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Box("Body", c, new Vector3(0f, .3f, 0f), new Vector3(.7f, .6f, .55f), art.Cardboard, false);
            Box("Band", c, new Vector3(0f, .42f, .278f), new Vector3(.7f, .1f, .01f), band, false);
            foreach (float x in new[] { -.34f, .34f })
            foreach (float z in new[] { -.26f, .26f })
                Box("Corner", c, new Vector3(x, .3f, z), new Vector3(.05f, .62f, .05f), art.DarkMetal, false);
            Box("Lid", c, new Vector3(-.15f, .63f, -.4f), new Vector3(.7f, .03f, .55f), art.Cardboard, false).transform.localRotation = Quaternion.Euler(-60f, 0f, 0f);
            Box("Packing", c, new Vector3(0f, .58f, 0f), new Vector3(.6f, .04f, .45f), art.Blind, false);
            var solid = c.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, .3f, 0f);
            solid.size = new Vector3(.7f, .6f, .55f);
            c.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Wood);
            c.gameObject.AddComponent<TutorialGearCrate>().EditorSetup(index, gear);
        }

        // The laundry alcove's bypass doors: the back leaf slides behind the front one, opening half the alcove.
        private static void BypassDoors(Transform parent, TutorialHouseArt art)
        {
            float width = L.LaundryDoorX1 - L.LaundryDoorX0, half = width * .5f;
            Transform root = Group("LaundryDoors", parent);
            root.position = new Vector3((L.LaundryDoorX0 + L.LaundryDoorX1) * .5f, L.Floor, L.BandSouth);
            Transform back = Leaf(root, "BackLeaf", half, L.DoorHeight - .02f, art.Trim, art.OakDark, art);
            Transform front = Leaf(root, "FrontLeaf", half, L.DoorHeight - .02f, art.Trim, art.OakDark, art);
            Vector3 backShut = new(-half * .5f, 0f, -.03f), backOpen = new(half * .5f, 0f, -.03f), frontAt = new(half * .5f, 0f, .03f);
            Finish(root, 0, new[] { back, front }, new[] { backShut, frontAt }, new[] { backOpen, frontAt }, width, L.DoorHeight);
        }

        // The patio slider: the fixed pane on the west half (the cellar stairs' side), the sliding pane on the east half.
        // Open, the sliding pane stands behind the fixed one, so the way out is the east half, onto open patio and clear
        // of the stairwell (2026-10-21, the user: it opened right in front of the stairs; "swap it"). Starts open: the
        // visitor walks in through it.
        private static void PatioSlider(Transform parent, TutorialHouseArt art)
        {
            float width = L.SliderX1 - L.SliderX0, half = width * .5f;
            Transform root = Group("PatioSlider", parent);
            root.position = new Vector3((L.SliderX0 + L.SliderX1) * .5f, L.Floor, L.BandSouth);
            Transform moving = Leaf(root, "SlidingPane", half, L.DoorHeight - .02f, art.Glass, art.Chrome, art, glazed: true, pullOnWest: false);
            Transform fixedPane = Leaf(root, "FixedPane", half, L.DoorHeight - .02f, art.Glass, art.Chrome, art, glazed: true);
            Vector3 shut = new(half * .5f, 0f, .04f), open = new(-half * .5f, 0f, .04f), fixedAt = new(-half * .5f, 0f, -.04f);
            Finish(root, 1, new[] { moving, fixedPane }, new[] { shut, fixedAt }, new[] { open, fixedAt }, width, L.DoorHeight);
        }

        private static Transform Leaf(Transform root, string name, float width, float height, Material panel, Material frame, TutorialHouseArt art, bool glazed = false,
            bool pullOnWest = true)
        {
            Transform leaf = Group(name, root);
            const float stile = .05f;
            Box("Panel", leaf, new Vector3(0f, height * .5f, 0f), new Vector3(width - stile * 2f, height - stile * 2f, glazed ? .02f : .03f), panel, false);
            foreach (float side in new[] { -1f, 1f })
                Box("Stile", leaf, new Vector3(side * (width - stile) * .5f, height * .5f, 0f), new Vector3(stile, height, .045f), frame, false);
            Box("TopRail", leaf, new Vector3(0f, height - stile * .5f, 0f), new Vector3(width, stile, .045f), frame, false);
            Box("BottomRail", leaf, new Vector3(0f, stile * .5f, 0f), new Vector3(width, stile, .045f), frame, false);
            // The pull on the edge it is pulled by (the slider's moving pane: its east edge, pulled west).
            Box("Pull", leaf, new Vector3((pullOnWest ? -1f : 1f) * (width * .5f - .08f), 1.05f, .03f), new Vector3(.03f, .22f, .03f), art.Chrome, false);
            var solid = leaf.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, height * .5f, 0f);
            solid.size = new Vector3(width, height, .05f);
            leaf.gameObject.AddComponent<SurfaceTag>().EditorSet(glazed ? SurfaceMaterial.Glass : SurfaceMaterial.Wood);
            return leaf;
        }

        // As the mall's doors: a blocker and a carving obstacle that only work while shut; saved open.
        private static void Finish(Transform root, int index, Transform[] leaves, Vector3[] shut, Vector3[] open, float width, float height)
        {
            var blocker = root.gameObject.AddComponent<BoxCollider>();
            blocker.center = new Vector3(0f, height * .5f, 0f);
            blocker.size = new Vector3(width, height, .3f);
            var obstacle = root.gameObject.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = new Vector3(0f, height * .5f, 0f);
            obstacle.size = new Vector3(width + .2f, height, .6f);
            obstacle.carving = true;
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Wood);
            root.gameObject.AddComponent<SlidingDoor>().EditorSetup(index, DoorKind.Store, leaves, shut, open, blocker, obstacle, width, height);
            for (int i = 0; i < leaves.Length; i++) leaves[i].localPosition = open[i];
            blocker.enabled = false;
            obstacle.enabled = false;
        }

        // A reach-in closet as a hiding place: one solid box, a sliding panel as its door, clothes on a rod inside.
        private static void Closet(Transform parent, int index, string name, Rect r, bool faceSouth, TutorialHouseArt art)
        {
            float width = faceSouth ? r.width : r.height, depth = faceSouth ? r.height : r.width;
            const float height = 2.3f;
            Transform root = Group(name, parent);
            // Origin on the floor at the back, +Z out of the door.
            Vector3 back = faceSouth ? new Vector3(r.center.x, L.Floor, r.yMax) : new Vector3(r.xMax, L.Floor, r.center.y);
            root.SetPositionAndRotation(back, Quaternion.Euler(0f, faceSouth ? 180f : -90f, 0f));
            Box("Header", root, new Vector3(0f, height + .2f, depth - .03f), new Vector3(width, .4f, .06f), art.Plaster, false);
            foreach (float side in new[] { -1f, 1f })
                Box("SideWall", root, new Vector3(side * (width * .5f - .05f), height * .5f, depth * .5f), new Vector3(.1f, height, depth), art.Plaster, false);
            Box("Rod", root, new Vector3(0f, 1.75f, depth * .45f), new Vector3(width - .2f, .03f, .03f), art.Chrome, false);
            var random = new System.Random(index * 13 + 5);
            for (float x = -width * .5f + .25f; x < width * .5f - .2f; x += .14f)
                Box("Garment", root, new Vector3(x, 1.25f, depth * .45f), new Vector3(.05f, .9f + (float)random.NextDouble() * .3f, .45f),
                    random.Next(3) == 0 ? art.Bedding : random.Next(2) == 0 ? art.SofaFabric : art.Curtain, false);
            // The fixed panel on the left half; the door (the right half) slides behind it.
            Box("FixedPanel", root, new Vector3(-width * .25f, height * .5f, depth - .02f), new Vector3(width * .5f, height - .04f, .03f), art.Trim, false);
            Transform door = Group("SlidingPanel", root);
            door.localPosition = new Vector3(width * .25f, 0f, depth - .06f);
            Box("Panel", door, new Vector3(0f, height * .5f, 0f), new Vector3(width * .5f, height - .04f, .03f), art.Trim, false);
            Box("Pull", door, new Vector3(-width * .25f + .06f, 1.05f, .02f), new Vector3(.025f, .18f, .025f), art.Chrome, false);
            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, height * .5f, depth * .5f);
            solid.size = new Vector3(width, height, depth);
            var obstacle = root.gameObject.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = solid.center;
            obstacle.size = solid.size;
            obstacle.carving = true;
            Transform inside = Group("Inside", root);
            inside.localPosition = new Vector3(width * .25f, .05f, depth * .45f);
            Transform exit = Group("Exit", root);
            exit.localPosition = new Vector3(width * .25f, .05f, depth + .7f);
            root.gameObject.AddComponent<HidingLocker>().EditorSetup(index, door, inside, exit, null, HideSpotKind.Closet);
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Wood);
        }
    }
}
