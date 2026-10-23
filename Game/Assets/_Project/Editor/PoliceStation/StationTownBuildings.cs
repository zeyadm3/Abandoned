using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using F = Abandoned.EditorTools.StationTownFacade;
using M = Abandoned.EditorTools.StationMeshes;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The neighbourhood round a station: original buildings made in code, exterior only (their doors stay shut and
    /// they have no rooms), each one solid. A building stands with the middle of its front at the given point, its front
    /// facing +z of its yaw (toward the street), running back <c>depth</c> metres. Row houses, shops with flats over them
    /// and apartment blocks here; houses, warehouses, a garage, a church and a gas station in StationTownLandmarks.cs; office
    /// blocks, parking decks and strip malls in StationTownCommercial.cs.
    /// </summary>
    public static partial class StationTownBuildings
    {
        public enum Kind { Rowhouse, Shop, Apartment, Warehouse, House, Church, GasStation, Garage, Office, ParkingDeck, StripMall }

        public static Transform Build(Transform parent, Kind kind, Vector3 front, float yaw, float width, float depth, int storeys, int seed, StationArt a)
        {
            StationTownArt t = StationTownArt.For(a);
            Transform b = Group($"{kind}_{seed}", parent);
            b.SetPositionAndRotation(front, Quaternion.Euler(0f, yaw, 0f));
            switch (kind)
            {
                case Kind.Rowhouse: Rowhouse(b, a, t, width, depth, Mathf.Max(2, storeys), seed); break;
                case Kind.Shop: Shop(b, a, t, width, depth, Mathf.Max(1, storeys), seed); break;
                case Kind.Apartment: Apartment(b, a, t, width, depth, Mathf.Max(3, storeys), seed); break;
                case Kind.Warehouse: Warehouse(b, a, t, width, depth, seed); break;
                case Kind.House: House(b, a, t, width, depth, seed); break;
                case Kind.Church: Church(b, a, t, width, depth, seed); break;
                case Kind.GasStation: GasStation(b, a, t, width, depth, seed); break;
                case Kind.Office: Office(b, a, t, width, depth, Mathf.Max(3, storeys), seed); break;
                case Kind.ParkingDeck: ParkingDeck(b, a, t, width, depth, Mathf.Max(2, storeys), seed); break;
                case Kind.StripMall: StripMall(b, a, t, width, depth, seed); break;
                default: Garage(b, a, t, width, depth, seed); break;
            }
            return b;
        }

        // The solid mass of a building (its only collider), its front at z 0.
        private static void Mass(Transform b, float width, float depth, float height, Material wall) =>
            Box("Mass", b, new Vector3(0f, height * .5f, -depth * .5f), new Vector3(width, height, depth), wall);

        // A scan on a building, in the building's own frame.
        private static void Scan(Transform b, string id, Vector3 local, float yaw, Vector3 fit) =>
            StationScans.Put(b, id, b.TransformPoint(local), b.eulerAngles.y + yaw, fit);

        private static float[] Sills(float first, float storey, int from, int count)
        {
            var s = new float[count];
            for (int i = 0; i < count; i++) s[i] = first + storey * (from + i);
            return s;
        }

        // A narrow brick row house over a half-basement: a stoop to the door, tall windows, a bracketed cornice.
        private static void Rowhouse(Transform b, StationArt a, StationTownArt t, float w, float d, int storeys, int seed)
        {
            const float rise = .9f, storey = 3.3f;
            float h = rise + storeys * storey + .2f;
            Material wall = seed % 4 == 3 ? t.Stone : t.Brick(seed);
            Mass(b, w, d, h, wall);
            Transform front = F.Face(b, F.Side.Front, w, d);
            float doorX = (seed % 2 == 0 ? -1f : 1f) * (w * .5f - 1.3f);
            F.Door(front, a, t, doorX, 1.1f, 2.5f, seed % 3 == 0 ? t.DoorWood : t.Paint(seed), rise, seed % 11 == 5);
            F.WallLamp(front, a, doorX + .95f, rise + 2.4f);
            float otherX = -doorX * .55f;
            F.Window(front, a, t, otherX, rise + .7f, 1.1f, 2.1f, F.PaneFor(seed), t.Frame, wall == t.Stone ? a.Trim : t.Stone);
            F.Window(front, a, t, otherX, .12f, .9f, .5f, F.Pane.Dark, t.FrameDark, wall, false, false);
            F.WindowGrid(front, a, t, w - .6f, Sills(rise + .7f, storey, 1, storeys - 1), w > 7f ? 3 : 2, 1.05f, 2.1f, t.Frame, t.Stone, seed);
            F.Cornice(front, t.Paint(seed + 3), t.Paint(seed + 3), w, h - .55f);
            // The areaway railing in front of the basement window.
            for (float x = -w * .5f + .2f; x < w * .5f - .1f; x += .14f)
                if (Mathf.Abs(x - doorX) > 1.4f) Box("Picket", front, new Vector3(x, .5f, 1.6f), new Vector3(.025f, 1f, .025f), t.Lattice, false);
            Box("PicketRail", front, new Vector3(-doorX * .45f, .98f, 1.6f), new Vector3(w - 2.9f, .04f, .04f), t.Lattice, false);
            Transform back = F.Face(b, F.Side.Back, w, d);
            F.WindowGrid(back, a, t, w - .6f, Sills(rise + .9f, storey, 0, storeys), 2, 1f, 1.7f, t.Frame, wall, seed + 5);
            F.FlatRoof(b, a, t, wall, w, d, h, .5f);
            Chimney(b, a, wall, new Vector3(w * .5f - .5f, h, -d + 1.2f), 1.6f);
            if (seed % 3 == 1) Scan(b, "exterior_aircon_unit", new Vector3(-w * .2f, h, -d * .55f), 0f, new Vector3(1.2f, 1.3f, 1.2f));
        }

        // A shop with flats over it: a shopfront, the flats' own door at one side, a cornice with dentils.
        private static void Shop(Transform b, StationArt a, StationTownArt t, float w, float d, int storeys, int seed)
        {
            const float ground = 4.2f, storey = 3.2f;
            float h = ground + (storeys - 1) * storey + .3f;
            Material wall = seed % 5 == 4 ? t.ScanBrick : seed % 5 == 2 ? t.Stucco : t.Brick(seed + 1);
            Mass(b, w, d, h, wall);
            Transform front = F.Face(b, F.Side.Front, w, d);
            bool flatsDoor = storeys > 1 && w > 7f;
            float shopWidth = flatsDoor ? w - 1.8f : w - .3f, shopX = flatsDoor ? -.85f : 0f;
            Transform sf = Group("Shop", front);
            sf.localPosition = new Vector3(shopX, 0f, 0f);
            F.Storefront(sf, a, t, shopWidth, ground, seed);
            if (flatsDoor) F.Door(front, a, t, w * .5f - .85f, 1f, 2.4f, t.DoorSteel);
            if (storeys > 1)
                F.WindowGrid(front, a, t, w - .5f, Sills(ground + .9f, storey, 0, storeys - 1), Mathf.Max(2, Mathf.RoundToInt(w / 2.6f)), 1.1f, 1.8f,
                    t.Frame, wall == t.Stucco ? a.Trim : t.Stone, seed);
            F.Cornice(front, a.Trim, a.Trim, w, h - .6f);
            F.FlatRoof(b, a, t, wall, w, d, h, 1f);
            Transform back = F.Face(b, F.Side.Back, w, d);
            F.Door(back, a, t, w * .3f, 1f, 2.3f, t.DoorSteel);
            F.WallLamp(back, a, w * .3f, 2.8f);
            if (storeys > 1) F.WindowGrid(back, a, t, w - .5f, Sills(ground + .9f, storey, 0, storeys - 1), 2, 1f, 1.5f, t.Frame, wall, seed + 9);
            Scan(b, "exterior_aircon_unit", new Vector3(w * .2f, h, -d * .4f), 90f, new Vector3(1.2f, 1.3f, 1.2f));
            Vent(b, a, new Vector3(-w * .25f, h, -d * .7f));
        }

        // A walk-up apartment block: a canopied entrance, ranks of windows, a fire escape, a water tank on the roof.
        private static void Apartment(Transform b, StationArt a, StationTownArt t, float w, float d, int storeys, int seed)
        {
            const float ground = 3.6f, storey = 3.1f;
            float h = ground + (storeys - 1) * storey + .4f;
            Material wall = seed % 3 == 0 ? t.ScanBrick : t.Brick(seed + 2);
            Mass(b, w, d, h, wall);
            Box("Base", b, new Vector3(0f, .6f, .03f), new Vector3(w + .1f, 1.2f, .08f), t.Stone, false);
            Transform front = F.Face(b, F.Side.Front, w, d);
            int columns = Mathf.Max(3, Mathf.RoundToInt(w / 2.8f));
            F.Door(front, a, t, 0f, 1.8f, 2.6f, t.DoorSteel, .45f);
            Box("Canopy", front, new Vector3(0f, 3.25f, .9f), new Vector3(3.2f, .15f, 1.8f), t.Paint(seed), false);
            foreach (float s in new[] { -1f, 1f }) M.Strut(front, "CanopyRod", new Vector3(s * 1.4f, 4.3f, .05f), new Vector3(s * 1.4f, 3.3f, 1.75f), .03f, t.Lattice);
            F.WallLamp(front, a, -1.4f, 2.7f);
            F.WallLamp(front, a, 1.4f, 2.7f);
            F.WindowGrid(front, a, t, w - .8f, new[] { 1.1f }, columns, 1.2f, 1.8f, t.Frame, t.Stone, seed, 0f, 1.8f);
            F.WindowGrid(front, a, t, w - .8f, Sills(ground + .9f, storey, 0, storeys - 1), columns, 1.2f, 1.8f, t.Frame, t.Stone, seed + 1);
            for (int i = 1; i < storeys; i++)
                Box("Band", front, new Vector3(0f, ground + storey * (i - 1) + .45f, .05f), new Vector3(w + .04f, .14f, .1f), t.Stone, false);
            float escapeX = (seed % 2 == 0 ? -1f : 1f) * (w * .5f - (w - .8f) / columns);
            var levels = new float[storeys - 1];
            for (int i = 0; i < levels.Length; i++) levels[i] = ground + storey * i + .85f;
            F.FireEscape(front, t, escapeX, (w - .8f) / columns * 1.7f, levels);
            F.Cornice(front, t.Stone, t.Stone, w, h - .65f);
            F.FlatRoof(b, a, t, wall, w, d, h, 1.1f);
            foreach (F.Side side in new[] { F.Side.Left, F.Side.Right })
            {
                Transform f = F.Face(b, side, w, d);
                F.WindowGrid(f, a, t, d - 2f, Sills(ground + .9f, storey, 0, storeys - 1), Mathf.Max(2, Mathf.RoundToInt(d / 4f)), 1f, 1.6f, t.Frame, t.Stone, seed + (int)side * 7);
            }
            Transform back = F.Face(b, F.Side.Back, w, d);
            F.WindowGrid(back, a, t, w - .8f, Sills(ground + .9f, storey, 0, storeys - 1), columns, 1.1f, 1.6f, t.Frame, wall, seed + 3);
            WaterTank(b, a, t, new Vector3(-w * .2f, h, -d * .6f));
            Bulkhead(b, a, t, wall, new Vector3(w * .25f, h, -d * .35f));
            Chimney(b, a, wall, new Vector3(-w * .5f + .7f, h, -d + .8f), 2.2f);
            if (seed % 2 == 0) Dish(b, a, new Vector3(w * .35f, h + .1f, -d * .75f), seed);
        }

        // ---- Roofscape --------------------------------------------------------------------------------------------

        private static void Chimney(Transform b, StationArt a, Material wall, Vector3 at, float tall)
        {
            Box("Chimney", b, at + Vector3.up * tall * .5f, new Vector3(.8f, tall, .6f), wall, false);
            Box("ChimneyCap", b, at + Vector3.up * (tall + .05f), new Vector3(.95f, .1f, .75f), a.Trim, false);
            foreach (float x in new[] { -.18f, .18f })
                Primitive(PrimitiveType.Cylinder, "Pot", b, at + new Vector3(x, tall + .3f, 0f), new Vector3(.22f, .22f, .22f), a.Trim, false);
        }

        private static void Vent(Transform b, StationArt a, Vector3 at)
        {
            M.Draw(b, "TurbineVent", M.Lathe(a, "TurbineVent", new[]
            {
                new Vector2(.12f, 0f), new Vector2(.12f, .45f), new Vector2(.12f, .45f), new Vector2(.28f, .55f), new Vector2(.3f, .7f),
                new Vector2(.26f, .85f), new Vector2(.26f, .85f), new Vector2(0f, .92f),
            }, 16), at, Vector3.zero, Vector3.one, a.Steel);
        }

        private static void WaterTank(Transform b, StationArt a, StationTownArt t, Vector3 at)
        {
            Transform tank = Group("WaterTank", b);
            tank.localPosition = at;
            foreach (float x in new[] { -1.1f, 1.1f })
            foreach (float z in new[] { -1.1f, 1.1f })
                Box("Leg", tank, new Vector3(x, .75f, z), new Vector3(.18f, 1.5f, .18f), a.DarkSteel, false);
            Box("BeamX", tank, new Vector3(0f, 1.55f, 0f), new Vector3(2.6f, .16f, .2f), a.DarkSteel, false);
            Box("BeamZ", tank, new Vector3(0f, 1.55f, 0f), new Vector3(.2f, .16f, 2.6f), a.DarkSteel, false);
            M.Draw(tank, "Barrel", M.Lathe(a, "WaterTank", new[]
            {
                new Vector2(0f, 0f), new Vector2(1.4f, 0f), new Vector2(1.4f, 0f), new Vector2(1.45f, 1.3f), new Vector2(1.4f, 2.6f), new Vector2(1.4f, 2.6f),
                new Vector2(1.55f, 2.6f), new Vector2(1.55f, 2.6f), new Vector2(0f, 3.5f),
            }, 20), new Vector3(0f, 1.63f, 0f), Vector3.zero, Vector3.one, t.WaterTank);
            for (int i = 0; i < 4; i++)
                Primitive(PrimitiveType.Cylinder, "Hoop", tank, new Vector3(0f, 1.95f + i * .65f, 0f), new Vector3(2.96f, .025f, 2.96f), a.DarkSteel, false);
            M.Strut(tank, "Ladder", new Vector3(1.5f, 0f, 0f), new Vector3(1.5f, 4.2f, 0f), .05f, a.DarkSteel, .4f);
        }

        // The stair bulkhead on a flat roof: a little brick hut with a steel door.
        private static void Bulkhead(Transform b, StationArt a, StationTownArt t, Material wall, Vector3 at)
        {
            Box("Bulkhead", b, at + new Vector3(0f, 1.3f, 0f), new Vector3(2.6f, 2.6f, 3.2f), wall, false);
            Box("BulkheadRoof", b, at + new Vector3(0f, 2.65f, 0f), new Vector3(2.8f, .1f, 3.4f), t.TarRoof, false);
            Box("BulkheadDoor", b, at + new Vector3(0f, 1.05f, 1.61f), new Vector3(.9f, 2.05f, .04f), t.DoorSteel, false);
        }

        private static void Dish(Transform b, StationArt a, Vector3 at, int seed)
        {
            Transform dish = Group("Dish", b);
            dish.localPosition = at;
            dish.localRotation = Quaternion.Euler(0f, 160f + seed % 40, 0f);
            Box("Mast", dish, new Vector3(0f, .45f, 0f), new Vector3(.05f, .9f, .05f), a.DarkSteel, false);
            GameObject bowl = M.Draw(dish, "Bowl", M.Lathe(a, "Dish", new[]
            {
                new Vector2(0f, 0f), new Vector2(.45f, .14f), new Vector2(.45f, .14f), new Vector2(0f, .04f),
            }, 20), new Vector3(0f, .95f, .1f), new Vector3(-55f, 0f, 0f), Vector3.one, a.White);
            bowl.name = "Bowl";
            M.Strut(dish, "Feed", new Vector3(0f, .95f, .1f), new Vector3(0f, 1.25f, .4f), .02f, a.DarkSteel);
        }
    }
}
