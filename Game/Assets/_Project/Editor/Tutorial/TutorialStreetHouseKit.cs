using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.TutorialHouseLayout;
using W = Abandoned.EditorTools.TutorialHouseWalls;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The training house as the street's houses (2026-10-21, the user: "copy paste the house and make the surrounding
    /// houses the same as the tutorial house with some variations ... lock everything so the player can't walk
    /// inside, remove the basement and basement stairs"). Its outside walls, windows and roofs come from the same code
    /// as 14 Larch Court, so the two never drift apart; the front door and the patio slider are shut, the patio is
    /// whole (no cellar stairs, no basement, no window wells or furnace flue), and behind the blinds there is only a
    /// dark room. Built once per rebuild into saved meshes, near (everything) and far (no laps, casings, blinds or
    /// icicles), with the pieces a copy may add: a garage, a hood over the stoop, a chimney. Local frame = the
    /// training house's: the street side is +X.
    /// </summary>
    public static class TutorialStreetHouseKit
    {
        /// <summary>The garage: free-standing beside the band's north wall, 1.5 m off it (clear of the band's icicles).</summary>
        public static readonly Rect Garage = Rect.MinMaxRect(19f, 15.5f, 26.5f, 22.5f);
        public const float GarageWallTop = 2.5f, GarageDoorZ0 = 16.4f, GarageDoorZ1 = 21.6f, GarageDoorHeight = 2.25f;
        /// <summary>The stoop's ramp foot, the nearest the house comes to the street; and the back of its roof.</summary>
        public const float FrontX = 30.9f, BackX = -.6f;
        /// <summary>The house's sides (roof overhangs and gutters included), without and with the garage.</summary>
        public const float SideMin = -.6f, SideMax = 18.1f, GarageSideMax = 23.1f;

        /// <summary>The saved pieces a street copy is drawn from.</summary>
        public sealed class Kit
        {
            public TutorialMeshMerge.Merged House, HouseFar, Garage, GarageFar, Canopy, Chimney;
        }

        public static Kit Build(TutorialHouseArt art) => new()
        {
            House = Piece("StreetHouse_Near", t => House(t, art, full: true), art),
            HouseFar = Piece("StreetHouse_Far", t => House(t, art, full: false), art),
            Garage = Piece("StreetGarage_Near", t => GarageShell(t, art, full: true), art),
            GarageFar = Piece("StreetGarage_Far", t => GarageShell(t, art, full: false), art),
            Canopy = Piece("StreetPorchHood", t => Hood(t, art), art),
            Chimney = Piece("StreetChimney", t => TutorialHouseRoof.Chimney(t, Vector3.zero, art), art),
        };

        // Built at the origin out of the usual boxes, given metre-scale UVs, merged and saved; the boxes go.
        private static TutorialMeshMerge.Merged Piece(string name, System.Action<Transform> build, TutorialHouseArt art)
        {
            Transform template = new GameObject(name + "_Template").transform;
            try
            {
                build(template);
                BurleySchoolSurfaces.Apply(template, art.Tiling);
                return TutorialMeshMerge.Save(template, name);
            }
            finally
            {
                Object.DestroyImmediate(template.gameObject);
            }
        }

        private static void House(Transform t, TutorialHouseArt art, bool full)
        {
            Transform walls = Group("Walls", t);
            W.Exterior(Group("Exterior", walls), Group("Openings", walls), art, full);
            TutorialHouseRoof.Gables(Group("Roof", t), art, full);
            FrontDoor(Group("FrontDoor", t), art, full);
            Slider(Group("PatioSlider", t), art, full);
            Stoop(Group("Porch", t), art, full);
            Patio(Group("Patio", t), art, full);
            DarkRooms(Group("Inside", t), art);
        }

        // The front door, shut and locked: two leaves meeting in the middle, set in the wall like the training
        // house's, with raised panels and knobs up close.
        private static void FrontDoor(Transform door, TutorialHouseArt art, bool full)
        {
            float x = L.HouseEast, half = L.FrontDoorWidth * .5f, h = L.DoorHeight;
            foreach (float side in new[] { -1f, 1f })
            {
                float z = L.FrontDoorZ + side * half * .5f;
                Box("Leaf", door, new Vector3(x, h * .5f, z), new Vector3(.06f, h - .01f, half - .01f), art.FrontDoor, false);
                if (!full) continue;
                foreach (float y in new[] { .7f, 1.65f })
                    Box("Panel", door, new Vector3(x + .035f, y, z), new Vector3(.015f, .7f, half - .28f), art.FrontDoor, false);
                Primitive(PrimitiveType.Sphere, "Knob", door, new Vector3(x + .07f, 1.05f, L.FrontDoorZ + side * .1f), Vector3.one * .06f, art.Brass, false);
            }
            Sill(door, "DoorSill", x, L.FrontDoorZ, along: false, L.FrontDoorWidth, art);
        }

        // The patio slider shut, its vertical blinds drawn behind the glass.
        private static void Slider(Transform slider, TutorialHouseArt art, bool full)
        {
            float z = L.BandSouth, x0 = L.SliderX0, x1 = L.SliderX1, mid = (x0 + x1) * .5f, h = L.DoorHeight;
            Box("Pane_West", slider, new Vector3((x0 + mid) * .5f, h * .5f, z + .03f), new Vector3(mid - x0, h - .02f, .025f), art.Glass, false);
            Box("Pane_East", slider, new Vector3((mid + x1) * .5f, h * .5f, z - .03f), new Vector3(x1 - mid, h - .02f, .025f), art.Glass, false);
            Box("VerticalBlinds", slider, new Vector3(mid, h * .5f, z + .17f), new Vector3(x1 - x0 - .06f, h - .1f, .015f), art.Blind, false);
            Sill(slider, "SliderSill", mid, z, along: true, x1 - x0, art);
            if (!full) return;
            Box("Head", slider, new Vector3(mid, h - .04f, z), new Vector3(x1 - x0, .08f, .14f), art.Chrome, false);
            Box("Track", slider, new Vector3(mid, .02f, z), new Vector3(x1 - x0, .04f, .14f), art.Chrome, false);
            foreach (float x in new[] { x0 + .03f, mid, x1 - .03f })
                Box("Stile", slider, new Vector3(x, h * .5f, z), new Vector3(.06f, h, .1f), art.Chrome, false);
        }

        // Under a doorway the foundation is cut away (the training house's floor and stoop fill it); here a concrete
        // sill closes it from the snow up to the floor.
        private static void Sill(Transform parent, string name, float x, float z, bool along, float width, TutorialHouseArt art)
        {
            float bottom = L.Grade - .05f, height = L.Floor - bottom;
            Vector3 size = along ? new Vector3(width, height, L.ExteriorWall) : new Vector3(L.ExteriorWall, height, width);
            Box(name, parent, new Vector3(x, bottom + height * .5f, z), size, art.Concrete, false);
        }

        // The same concrete stoop and trolley ramp as the training house's.
        private static void Stoop(Transform porch, TutorialHouseArt art, bool full)
        {
            Rect stoop = Rect.MinMaxRect(L.Porch.xMin + L.ExteriorWall * .5f, L.Porch.yMin, L.Porch.xMax, L.Porch.yMax);
            Box("Stoop", porch, new Vector3(stoop.center.x, L.Floor - .2f, stoop.center.y), new Vector3(stoop.width, .4f, stoop.height), art.Concrete, false);
            TutorialHouseStructure.Ramp(porch, "StoopRamp", new Vector3(stoop.xMax, L.Floor, stoop.center.y), Vector3.right, .9f, L.Grade - L.Floor, stoop.height, art.Concrete);
            if (full) Box("StoopSnow", porch, new Vector3(stoop.xMax - .15f, L.Floor + .015f, stoop.yMax - .15f), new Vector3(.3f, .03f, .3f), art.SnowDrift, false);
        }

        // The patio between the wings, whole: no bulkhead, no stairs, nothing under it. A few centimetres proud of
        // the snow so the slab never flickers against the ground.
        private static void Patio(Transform patio, TutorialHouseArt art, bool full)
        {
            float inset = L.ExteriorWall * .5f;
            Rect p = Rect.MinMaxRect(L.Patio.xMin + inset, L.Patio.yMin - .4f, L.Patio.xMax - inset, L.Patio.yMax - inset);
            Box("PatioSlab", patio, new Vector3(p.center.x, L.Grade - .08f, p.center.y), new Vector3(p.width, .24f, p.height), art.Concrete, false);
            Box("PatioSnow", patio, new Vector3(p.center.x, L.Grade + .052f, p.center.y - .2f), new Vector3(p.width - .3f, .024f, p.height - .7f), art.Snow, false);
            if (full) Box("SlabDrift", patio, new Vector3(p.center.x, L.Grade + .1f, p.yMin + .5f), new Vector3(p.width - .6f, .12f, .6f), art.SnowDrift, false);
        }

        // Nothing inside: a dark room behind every window, so the glass never shows daylight through an empty shell.
        // Inset from the outside walls (the blinds hang between); overlapping where the wings meet.
        private static void DarkRooms(Transform inside, TutorialHouseArt art)
        {
            const float i = .4f;
            Rect[] rooms =
            {
                Rect.MinMaxRect(L.WestWing.xMin + i, L.WestWing.yMin + i, L.WestWing.xMax - i, L.WestWing.yMax - i),
                Rect.MinMaxRect(L.Band.xMin - 1f, L.Band.yMin + i, L.Band.xMax - i, L.Band.yMax - i),
                Rect.MinMaxRect(L.Suite.xMin + i, L.Suite.yMin + i, L.Suite.xMax - i, L.Suite.yMax + 1f),
            };
            foreach (Rect r in rooms)
                Box("DarkRoom", inside, new Vector3(r.center.x, (L.Grade + L.Ceiling) * .5f, r.center.y), new Vector3(r.width, L.Ceiling - L.Grade, r.height), art.Window, false);
        }

        // A one-car-and-a-bit garage: the house's siding and roof at a lower eave, its door shut, a window each side.
        private static void GarageShell(Transform t, TutorialHouseArt art, bool full)
        {
            Rect g = Garage;
            W.Hole window = W.Win(g.center.x - .7f, g.center.x + .7f, 1.2f, 1.9f);
            W.OuterWall[] walls =
            {
                new("Garage_West", false, g.xMin, g.yMin, g.yMax, -1f),
                new("Garage_South", true, g.yMin, g.xMin, g.xMax, -1f, window),
                new("Garage_North", true, g.yMax, g.xMin, g.xMax, 1f, window),
                new("Garage_East", false, g.xMax, g.yMin, g.yMax, 1f, new W.Hole(GarageDoorZ0, GarageDoorZ1, L.Grade, GarageDoorHeight, false)),
            };
            Transform shell = Group("Walls", t), openings = Group("Openings", shell);
            foreach (W.OuterWall wall in walls) W.Outer(shell, openings, art, wall, full, GarageWallTop);
            W.CornerBoards(Group("Corners", shell), walls, GarageWallTop, art);
            TutorialHouseRoof.Gable(t, "GarageRoof", g, ridgeAlongZ: false, rise: g.height * TutorialHouseRoof.Pitch, GarageWallTop + .3f, art, full);

            // The sectional door, down: panels in the house's trim colour, a row of dark lights along the top.
            Transform door = Group("GarageDoor", t);
            float x = g.xMax + .02f, mid = (GarageDoorZ0 + GarageDoorZ1) * .5f, width = GarageDoorZ1 - GarageDoorZ0, height = GarageDoorHeight - L.Grade;
            Box("Door", door, new Vector3(x, L.Grade + height * .5f, mid), new Vector3(.05f, height, width), art.Trim, false);
            if (full)
            {
                for (int k = 1; k < 4; k++)
                    Box("Joint", door, new Vector3(x + .03f, L.Grade + height * k / 4f, mid), new Vector3(.012f, .02f, width), art.SidingDark, false);
                for (int k = 0; k < 4; k++)
                    Box("Light", door, new Vector3(x + .03f, L.Grade + height * .86f, GarageDoorZ0 + width * (k + .5f) / 4f), new Vector3(.012f, .22f, width / 4f - .3f), art.Window, false);
                Box("DoorDrift", door, new Vector3(x + .3f, L.Grade + .06f, mid), new Vector3(.5f, .12f, width - .2f), art.SnowDrift, false);
            }
            Box("DarkInside", t, new Vector3(g.center.x, (L.Grade + GarageWallTop) * .5f, g.center.y), new Vector3(g.width - .8f, GarageWallTop - L.Grade, g.height - .8f), art.Window, false);
            Box("Slab", t, new Vector3(g.center.x, L.Grade - .08f, g.center.y), new Vector3(g.width, .24f, g.height), art.Concrete, false);
        }

        // A gabled hood over the stoop on two posts, its ridge running out from the gable wall over the front door.
        private static void Hood(Transform t, TutorialHouseArt art)
        {
            Rect r = Rect.MinMaxRect(L.HouseEast, L.Porch.yMin + .1f, L.Porch.xMax - .1f, L.Porch.yMax - .1f);
            const float eave = 2.75f;
            TutorialHouseRoof.Gable(t, "Hood", r, ridgeAlongZ: false, rise: 1f, eave, art, full: true);
            foreach (float z in HoodPostsZ)
                Box("Post", t, new Vector3(HoodPostX, eave * .5f, z), new Vector3(.14f, eave, .14f), art.Trim, false);
        }

        public const float HoodPostX = 29.75f;
        public static readonly float[] HoodPostsZ = { L.Porch.yMin + .25f, L.Porch.yMax - .25f };
    }
}
