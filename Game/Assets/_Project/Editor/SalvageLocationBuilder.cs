using System;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Structure;
using Abandoned.Threats;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Authors the three post-mall salvage locations from repeatable layout data.  The maps deliberately
    /// share the same run plumbing as the mall (network session, truck, loot, structure, threats and power)
    /// while keeping their floor plans, weak routes, dressing and colour language distinct.
    /// </summary>
    public static class SalvageLocationBuilder
    {
        public const float Tile = 4f;
        private const float Story = 4f;
        private const float Slab = .25f;
        private const int Width = 12;
        private const int Depth = 10;

        private enum Theme { School, Factory, Prison }

        [Flags]
        private enum RoomDoor { South = 1, North = 2, West = 4, East = 8 }

        private sealed class Plan
        {
            public Theme Theme;
            public string Name, Path;
            public Color Shell, Floor, Accent, Prop;
            public readonly HashSet<Vector2Int> GroundHoles = new();
            public readonly HashSet<Vector2Int> Upper = new();
            public readonly HashSet<Vector2Int> Lower = new();
            public readonly HashSet<Vector2Int> ProtectedUpper = new();
            public readonly Dictionary<Vector2Int, Vector2> WeakUpper = new();
            public readonly List<Room> Rooms = new();
            public readonly List<Stair> Stairs = new();
            public readonly List<LootSite> Loot = new();
            public readonly List<Vector3> Threats = new();
            public float Stability;
            public int Seed;
        }

        private readonly struct Room
        {
            public readonly string Name, Tag;
            public readonly RectInt Cells;
            public readonly int Floor;
            public readonly bool Tall;
            public readonly RoomDoor Doors;
            /// <summary>World x of the south/north doorway's centre (NaN: the wall's middle), to line it up with a stair.</summary>
            public readonly float DoorAt;
            public Room(string name, string tag, int floor, int x, int z, int width, int depth, bool tall = false,
                RoomDoor doors = RoomDoor.South, float doorAt = float.NaN)
            {
                Name = name; Tag = tag; Floor = floor; Cells = new RectInt(x, z, width, depth); Tall = tall; Doors = doors; DoorAt = doorAt;
            }
        }

        private readonly struct Stair
        {
            public readonly string Name;
            public readonly Vector3 Foot;
            public readonly Vector3 Direction;
            public readonly float Rise;
            public Stair(string name, Vector3 foot, Vector3 direction, float rise)
            { Name = name; Foot = foot; Direction = direction.normalized; Rise = rise; }
        }

        private readonly struct LootSite
        {
            public readonly string Name, Tag;
            public readonly Vector3 At;
            public readonly CarryClass Min, Max;
            public readonly bool IsJackpot;
            public LootSite(string name, string tag, Vector3 at, CarryClass min = CarryClass.Pocket,
                CarryClass max = CarryClass.TwoHand, bool isJackpot = false)
            { Name = name; Tag = tag; At = at; Min = min; Max = max; IsJackpot = isJackpot; }
        }

        private sealed class TilePiece
        {
            public Transform Transform;
            public int Floor;
            public Vector2Int Cell;
            public bool Protected;
        }

        /// <summary>Builds a location scene in a clean scene, then writes a reusable NavMesh asset beside it.</summary>
        public static void BuildSchool() => Build(CreateSchool());
        public static void BuildFactory() => Build(CreateFactory());
        public static void BuildPrison() => Build(CreatePrison());

        private static void Build(Plan plan)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Rebuild locations in Edit mode.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject(plan.Name).transform;
            root.gameObject.AddComponent<BuildingInteriorAtmosphere>().EditorSetup(new Bounds(new Vector3(24f, 4f, 20f), new Vector3(48f, 12f, 40f)));

            Materials materials = new(plan);
            List<TilePiece> pieces = new();
            List<Transform> stairs = new();
            BuildStructure(root, plan, materials, pieces, stairs);
            BuildShell(root, plan, materials);
            BuildRooms(root, plan, materials);
            BuildSignatureDressing(root, plan, materials);
            BuildExterior(root, plan, materials);
            BuildLights(root, plan, materials);
            StructureSimulation simulation = SetupStructuralGameplay(plan, pieces, stairs);
            if (plan.Theme == Theme.School) SchoolAtmosphereBuilder.Apply(root);
            if (plan.Theme == Theme.Factory) FactoryAtmosphereBuilder.Apply(root);
            if (plan.Theme == Theme.Prison) PrisonAtmosphereBuilder.Apply(root);
            PopulateGameplay(root, plan, simulation);
            BakeNavMesh(root, plan);
            // Solid props (0.24.11), guarded by the first bake so no box cuts a route; then bake around them.
            LevelSolidity.Apply(root, plan.Name);
            BakeNavMesh(root, plan);
            ConfigureHorrorScenes();

            LevelAtmosphere.Apply(plan.Name, MoodFor(plan.Theme), root);
            PlaceCamera(plan);
            NetworkObjectIds.StampScene(scene);
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, plan.Path))
                throw new InvalidOperationException($"Unity could not save {plan.Name}.");
            Debug.Log($"[Locations] Built {plan.Name}: {pieces.Count} structural sections, {plan.Loot.Count} loot points and {plan.Threats.Count} threat markers.");
        }

        // ---- Geometry -------------------------------------------------------------------------------

        private static void BuildStructure(Transform root, Plan plan, Materials m, List<TilePiece> pieces, List<Transform> stairs)
        {
            Transform levels = Group("Levels", root);
            BuildFloor(-1, plan.Lower, levels, m, pieces, plan);
            var ground = AllCells().Where(c => !plan.GroundHoles.Contains(c));
            BuildFloor(0, ground, levels, m, pieces, plan);
            BuildFloor(1, plan.Upper, levels, m, pieces, plan);

            Transform flights = Group("Flights", root);
            foreach (Stair stair in plan.Stairs)
                stairs.Add(BuildStair(flights, stair, m.Metal));
        }

        private static void BuildFloor(int floor, IEnumerable<Vector2Int> cells, Transform levels, Materials m,
            List<TilePiece> pieces, Plan plan)
        {
            Transform group = Group(floor < 0 ? "Basement" : floor == 0 ? "GroundFloor" : "UpperFloor", levels);
            foreach (Vector2Int cell in cells.OrderBy(c => c.y).ThenBy(c => c.x))
            {
                Transform tile = Group($"Tile_{floor}_{cell.x}_{cell.y}", group);
                tile.localPosition = TileTop(cell, floor);
                BoxCollider collider = tile.gameObject.AddComponent<BoxCollider>();
                collider.center = Vector3.down * (Slab * .5f);
                collider.size = new Vector3(Tile, Slab, Tile);
                Box("Visual", tile, collider.center, collider.size, floor == 1 ? m.UpperFloor : floor < 0 ? m.Concrete : m.Floor, false);
                tile.gameObject.AddComponent<SurfaceTag>().EditorSet(floor == 1 && plan.Theme == Theme.School ? SurfaceMaterial.Wood : SurfaceMaterial.Concrete);
                pieces.Add(new TilePiece { Transform = tile, Floor = floor, Cell = cell, Protected = floor != 1 || plan.ProtectedUpper.Contains(cell) });
            }
        }

        private static Transform BuildStair(Transform parent, Stair stair, Material material)
        {
            const float length = 8f;
            const float width = 3f;
            const int steps = 16;
            Transform root = Group(stair.Name, parent);
            root.position = stair.Foot;
            root.rotation = Quaternion.LookRotation(stair.Direction, Vector3.up);
            Transform visual = Group("Visual", root);
            float angle = Mathf.Atan2(stair.Rise, length) * Mathf.Rad2Deg;
            float slope = Mathf.Sqrt(length * length + stair.Rise * stair.Rise);
            Transform ramp = Group("Ramp", root);
            ramp.localPosition = new Vector3(0f, stair.Rise * .5f, length * .5f);
            ramp.localRotation = Quaternion.Euler(-angle, 0f, 0f);
            ramp.gameObject.AddComponent<BoxCollider>().size = new Vector3(width, .25f, slope);
            for (int i = 0; i < steps; i++)
            {
                float t = (i + .5f) / steps;
                Box($"Step_{i:00}", visual, new Vector3(0f, stair.Rise * t - .10f, length * t),
                    new Vector3(width, .20f, length / steps), material, false);
            }
            foreach (float side in new[] { -1f, 1f })
            {
                GameObject rail = Box(side < 0 ? "Rail_L" : "Rail_R", visual,
                    new Vector3(side * (width * .5f + .08f), stair.Rise * .5f + .55f, length * .5f),
                    new Vector3(.12f, 1.1f, slope), material);
                rail.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
            }
            // Keep both landings explicitly connected for AI. The ramp is physically walkable for players, but
            // baking around floor openings can otherwise leave a hairline gap at one of its two transitions.
            var route = root.gameObject.AddComponent<NavMeshLink>();
            route.startPoint = Vector3.back * .75f;
            route.endPoint = Vector3.forward * (length + .75f) + Vector3.up * stair.Rise;
            route.width = width - .35f;
            route.bidirectional = true;
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            return root;
        }

        private static void BuildShell(Transform root, Plan plan, Materials m)
        {
            Transform shell = Group("Shell", root);
            const float h = 8f;
            const float t = .28f;
            float w = Width * Tile, d = Depth * Tile;
            // South has a wide permanent loading/entry opening aligned to the truck ramp.
            WallWithOpening(shell, "South", new Vector3(w * .5f, 0f, 0f), w, h, false, 24f, 5.8f, m.Shell);
            Wall(shell, "North", new Vector3(w * .5f, h * .5f, d), new Vector3(w, h, t), m.Shell);
            Wall(shell, "West", new Vector3(0f, h * .5f, d * .5f), new Vector3(t, h, d), m.Shell);
            Wall(shell, "East", new Vector3(w, h * .5f, d * .5f), new Vector3(t, h, d), m.Shell);
            // The roof makes the tall spaces (gym, production hall, cell galleries) read as real interiors.
            Box("Roof", shell, new Vector3(w * .5f, h + Slab * .5f, d * .5f), new Vector3(w, Slab, d), m.Shell);
            MarkStatic(shell);
        }

        private static void BuildRooms(Transform root, Plan plan, Materials m)
        {
            Transform rooms = Group("Rooms", root);
            foreach (Room room in plan.Rooms)
            {
                Transform holder = Group(room.Name, rooms);
                float y = FloorY(room.Floor);
                float wallHeight = room.Tall ? 7.7f : 3.1f;
                float x0 = room.Cells.x * Tile;
                float x1 = room.Cells.xMax * Tile;
                float z0 = room.Cells.y * Tile;
                float z1 = room.Cells.yMax * Tile;
                float midX = (x0 + x1) * .5f, midZ = (z0 + z1) * .5f, doorX = float.IsNaN(room.DoorAt) ? midX : room.DoorAt;
                float horizontalDoor = Mathf.Min(2.6f, x1 - x0 - .4f);
                float verticalDoor = Mathf.Min(2.6f, z1 - z0 - .4f);
                if (room.Doors.HasFlag(RoomDoor.South))
                    WallWithOpening(holder, "DoorwaySouth", new Vector3(midX, y, z0), x1 - x0, wallHeight, false, doorX, horizontalDoor, m.WallFor(room.Tag));
                else PlainWall(plan, room, holder, "SouthWall", new Vector3(midX, y, z0), x1 - x0, wallHeight, false, m.WallFor(room.Tag));
                if (room.Doors.HasFlag(RoomDoor.North))
                    WallWithOpening(holder, "DoorwayNorth", new Vector3(midX, y, z1), x1 - x0, wallHeight, false, doorX, horizontalDoor, m.WallFor(room.Tag));
                else PlainWall(plan, room, holder, "NorthWall", new Vector3(midX, y, z1), x1 - x0, wallHeight, false, m.WallFor(room.Tag));
                if (room.Doors.HasFlag(RoomDoor.West))
                    WallWithOpening(holder, "DoorwayWest", new Vector3(x0, y, midZ), z1 - z0, wallHeight, true, midZ, verticalDoor, m.WallFor(room.Tag));
                else PlainWall(plan, room, holder, "WestWall", new Vector3(x0, y, midZ), z1 - z0, wallHeight, true, m.WallFor(room.Tag));
                if (room.Doors.HasFlag(RoomDoor.East))
                    WallWithOpening(holder, "DoorwayEast", new Vector3(x1, y, midZ), z1 - z0, wallHeight, true, midZ, verticalDoor, m.WallFor(room.Tag));
                else PlainWall(plan, room, holder, "EastWall", new Vector3(x1, y, midZ), z1 - z0, wallHeight, true, m.WallFor(room.Tag));
                Nameplate(holder, room, m);
            }
            MarkStatic(rooms);
        }

        private static void Wall(Transform parent, string name, Vector3 center, Vector3 size, Material material) =>
            Box(name, parent, center, size, material);

        // A ground-floor wall standing over a stairwell (open ground on both sides of its line) gets an opening there:
        // the School's auditorium wall crossed the archive stair with 2 m of headroom and stopped players (0.24.11).
        private static void PlainWall(Plan plan, Room room, Transform holder, string name, Vector3 baseCentre, float length,
            float height, bool alongZ, Material material)
        {
            float lineAt = alongZ ? baseCentre.x : baseCentre.z, start = (alongZ ? baseCentre.z : baseCentre.x) - length * .5f;
            int line = Mathf.RoundToInt(lineAt / Tile);
            float spanStart = float.NaN, spanEnd = float.NaN;
            for (int i = Mathf.FloorToInt(start / Tile); i * Tile < start + length - .01f; i++)
            {
                Vector2Int before = alongZ ? new Vector2Int(line - 1, i) : new Vector2Int(i, line - 1);
                Vector2Int after = alongZ ? new Vector2Int(line, i) : new Vector2Int(i, line);
                if (room.Floor != 0 || !plan.GroundHoles.Contains(before) || !plan.GroundHoles.Contains(after)) continue;
                if (float.IsNaN(spanStart)) spanStart = i * Tile;
                spanEnd = (i + 1) * Tile;
            }
            if (float.IsNaN(spanStart))
            {
                Wall(holder, name, baseCentre + Vector3.up * (height * .5f), alongZ ? new Vector3(.18f, height, length) : new Vector3(length, height, .18f), material);
                return;
            }
            WallWithOpening(holder, name + "OverStairwell", baseCentre, length, height, alongZ, (spanStart + spanEnd) * .5f, spanEnd - spanStart, material);
        }

        private static void WallWithOpening(Transform parent, string name, Vector3 wallCenter, float length, float height,
            bool alongZ, float openingCenter, float openingWidth, Material material)
        {
            float start = (alongZ ? wallCenter.z : wallCenter.x) - length * .5f;
            float relative = Mathf.Clamp(openingCenter - start, openingWidth * .5f, length - openingWidth * .5f);
            float left = Mathf.Max(0f, relative - openingWidth * .5f);
            float right = Mathf.Max(0f, length - relative - openingWidth * .5f);
            Vector3 axis = alongZ ? Vector3.forward : Vector3.right;
            Vector3 thickness = alongZ ? new Vector3(.22f, height, 1f) : new Vector3(1f, height, .22f);
            if (left > .01f)
            {
                Vector3 c = wallCenter - axis * (length * .5f - left * .5f);
                Box(name + "_L", parent, c + Vector3.up * (height * .5f),
                    alongZ ? new Vector3(.22f, height, left) : new Vector3(left, height, .22f), material);
            }
            if (right > .01f)
            {
                Vector3 c = wallCenter + axis * (length * .5f - right * .5f);
                Box(name + "_R", parent, c + Vector3.up * (height * .5f),
                    alongZ ? new Vector3(.22f, height, right) : new Vector3(right, height, .22f), material);
            }
            float lintel = Mathf.Max(.1f, height - 2.8f);
            if (lintel > .01f)
            {
                Vector3 c = wallCenter + axis * (relative - length * .5f) + Vector3.up * (2.8f + lintel * .5f);
                Box(name + "_Lintel", parent, c, alongZ ? new Vector3(.22f, lintel, openingWidth) : new Vector3(openingWidth, lintel, .22f), material);
            }
        }

        private static void Nameplate(Transform parent, Room room, Materials m)
        {
            // A small colored panel gives the greybox rooms their readable purpose without depending on any external art pack.
            Vector3 center = new((room.Cells.x + room.Cells.width * .5f) * Tile, FloorY(room.Floor) + 2.5f, room.Cells.y * Tile + .13f);
            Box("Sign_" + room.Name, parent, center, new Vector3(Mathf.Min(3.1f, room.Cells.width * Tile - .8f), .42f, .03f), m.Accent, false);
            var label = new GameObject("Label_" + room.Name).AddComponent<TextMesh>();
            label.transform.SetParent(parent, false);
            label.transform.position = center + Vector3.forward * -.025f;
            label.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            label.text = room.Name.Replace('_', ' ').ToUpperInvariant();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = .16f;
            label.fontSize = 48;
            label.color = new Color(.92f, .92f, .88f);
            WorldTextMaterial.Apply(label,
                AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf"));
        }

        private static void BuildSignatureDressing(Transform root, Plan plan, Materials m)
        {
            Transform props = Group("SignatureDressing", root);
            switch (plan.Theme)
            {
                case Theme.School:
                    SchoolDressing(props, m);
                    break;
                case Theme.Factory:
                    FactoryDressing(props, m);
                    break;
                case Theme.Prison:
                    PrisonDressing(props, m);
                    break;
            }
            ClearStairwells(props, plan);
            MarkStatic(props);
        }

        // Dressing is laid out on a fixed grid (rows of cells, desks, machines): nothing may stand on a stair or over a
        // stairwell. The Prison's cell bars and bunks did, across both of its ground-floor stairs (0.24.11).
        private static void ClearStairwells(Transform props, Plan plan)
        {
            var keepOut = new List<Bounds>();
            foreach (Stair stair in plan.Stairs)
            {
                Vector3 top = stair.Foot + stair.Direction * 8f + Vector3.up * stair.Rise;
                var b = new Bounds(stair.Foot, Vector3.zero);
                b.Encapsulate(top);
                Vector3 across = Vector3.Cross(Vector3.up, stair.Direction) * 1.9f;
                b.Encapsulate(stair.Foot + across); b.Encapsulate(stair.Foot - across);
                b.Encapsulate(top + Vector3.up * 2.2f - stair.Direction * .5f);
                b.Encapsulate(stair.Foot - stair.Direction * .5f);
                keepOut.Add(b);
            }
            foreach (Vector2Int hole in plan.GroundHoles)
                keepOut.Add(new Bounds(new Vector3((hole.x + .5f) * Tile, 1.25f, (hole.y + .5f) * Tile), new Vector3(Tile, 3.5f, Tile)));
            foreach (Transform child in props.Cast<Transform>().ToList())
            {
                Renderer r = child.GetComponentInChildren<Renderer>();
                if (r == null || !keepOut.Any(k => k.Intersects(r.bounds))) continue;
                Debug.Log($"[Locations] {plan.Name}: {child.name} left out, it stood in a stairwell.");
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void SchoolDressing(Transform p, Materials m)
        {
            foreach (float x in new[] { 4f, 8f, 14f, 18f })
            foreach (float z in new[] { 2.5f, 5.5f })
                Box("ClassroomDesk", p, new Vector3(x, .45f, z), new Vector3(1.25f, .75f, .72f), m.Prop);
            for (int i = 0; i < 8; i++)
                Box("LibraryShelf", p, new Vector3(2.1f + (i % 2) * 4.2f, 1.1f, 26f + (i / 2) * 2.6f), new Vector3(1.5f, 2.2f, .46f), m.Wood);
            for (int i = 0; i < 5; i++)
                Box("GymBleacher", p, new Vector3(33f + i * 1.2f, .25f + i * .2f, 35f), new Vector3(1f, .5f + i * .1f, 4f), m.Metal);
            Box("AuditoriumStage", p, new Vector3(11.5f, .5f, 18f), new Vector3(7f, 1f, 2f), m.Wood);
            Box("BasementBoiler", p, new Vector3(14f, -3.1f, 29f), new Vector3(2.6f, 1.8f, 1.5f), m.Metal);
        }

        private static void FactoryDressing(Transform p, Materials m)
        {
            for (int i = 0; i < 4; i++)
            {
                float x = 12f + i * 6f;
                Box("Conveyor_" + i, p, new Vector3(x, .75f, 16f), new Vector3(4f, 1.1f, 1.4f), m.Metal);
                Box("Machine_" + i, p, new Vector3(x, 1.3f, 23f), new Vector3(2.4f, 2.6f, 2.4f), m.Prop);
            }
            for (int i = 0; i < 4; i++)
                Box("Boiler_" + i, p, new Vector3(34f + i * 2.5f, 1.4f, 31f), new Vector3(1.7f, 2.8f, 1.7f), m.Accent);
            // Preserve wide gaps at the two upper-office doors (x=6 and x=42) so rails do not become invisible blockers.
            float[] railCenters = { 2f, 10f, 16f, 21.5f, 27f, 32.5f, 37.5f, 46f };
            for (int i = 0; i < railCenters.Length; i++)
                Box("CatwalkRail_" + i, p, new Vector3(railCenters[i], 5.1f, 9f), new Vector3(3.2f, 1.1f, .12f), m.Metal);
            Box("LoadingCrates", p, new Vector3(39f, .75f, 7f), new Vector3(5f, 1.5f, 3f), m.Wood);
        }

        private static void PrisonDressing(Transform p, Materials m)
        {
            for (int row = 0; row < 2; row++)
            for (int i = 0; i < 8; i++)
            {
                float x = 4f + i * 4.8f;
                float z = row == 0 ? 18f : 27f;
                Box("CellBars_" + row + "_" + i, p, new Vector3(x, 1.4f, z), new Vector3(3.4f, 2.8f, .14f), m.Metal);
                Box("Bunk_" + row + "_" + i, p, new Vector3(x, .65f, z + (row == 0 ? 1.1f : -1.1f)), new Vector3(1.7f, 1.3f, .7f), m.Prop);
            }
            for (int i = 0; i < 4; i++)
                Box("VisitationBooth_" + i, p, new Vector3(5f + i * 4f, 1.2f, 6f), new Vector3(2.6f, 2.4f, .25f), m.Accent);
            // Stack machines along the west wall, leaving a direct aisle from the northern door to the salvage pad.
            for (int i = 0; i < 5; i++)
                Box("LaundryMachine_" + i, p, new Vector3(26f, .65f, 29.1f + i * 1.45f), new Vector3(1.5f, 1.3f, 1.5f), m.Prop);
            // Keep the serving line inside the kitchen instead of across its only northern doorway.
            Box("KitchenServingLine", p, new Vector3(35f, 1f, 3f), new Vector3(9f, 2f, 1.1f), m.Metal);
            Box("TunnelPipe", p, new Vector3(13f, -2.5f, 29f), new Vector3(9f, .55f, .55f), m.Metal);
        }

        private static void BuildExterior(Transform root, Plan plan, Materials m)
        {
            Transform exterior = Group("Exterior", root);
            Transform ground = Group("GroundSlabs", exterior);
            Box("Lot", ground, new Vector3(24f, -.22f, -10f), new Vector3(56f, .4f, 20f), m.Asphalt);
            Box("NorthYard", ground, new Vector3(24f, -.22f, 46f), new Vector3(56f, .4f, 14f), m.Dirt);
            // The truck's actual cargo trigger remains its own volume; this is the durable apron that
            // makes the shared collection approach visually and physically readable on every location.
            GameObject apron = Box("DockApron", ground, new Vector3(24f, -.16f, -4.5f),
                new Vector3(MeridianCollectionBayBuilder.ApproachWidth + .6f, .12f, 4.4f), m.Concrete);
            apron.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            ground.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);
            Transform truck = Group("Truck", root);
            TruckCargo cargo = TruckBuilder.Build(truck, new Vector3(24f, 0f, -11f), Vector3.back);
            MeridianCollectionBayBuilder.Build(root, cargo);
            MarkStatic(exterior);
        }

        private static void BuildLights(Transform root, Plan plan, Materials m)
        {
            Transform lights = Group("Lights", root);
            if (plan.Theme == Theme.Prison)
            {
                BuildPrisonLights(lights, m);
                return;
            }
            int index = 0;
            foreach (Vector3 at in LightPositions(plan))
            {
                Transform holder = Group("Fixture_" + index++, lights);
                holder.position = at + Vector3.up * 3.5f;
                GameObject glow = Box("Diffuser", holder, Vector3.zero, new Vector3(1.25f, .12f, .48f), m.Light, false);
                var lampRoot = Group("Light", holder);
                lampRoot.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Light lamp = lampRoot.gameObject.AddComponent<Light>();
                lamp.type = LightType.Spot;
                lamp.range = 12f;
                lamp.spotAngle = 104f;
                lamp.intensity = plan.Theme == Theme.Factory ? 7f : 5.2f;
                lamp.color = plan.Theme == Theme.Prison ? new Color(.72f, .84f, .96f) : new Color(1f, .89f, .70f);
                lamp.shadows = LightShadows.None;
                holder.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, glow.GetComponent<Renderer>(), index % 7 == 0, index % 19 == 0, 300 + index);
            }
        }

        private static void BuildPrisonLights(Transform lights, Materials m)
        {
            // The cellblocks are two storeys tall, so their fixtures live near the ceiling instead of floating
            // over head height. Service rooms remain low and claustrophobic; the dock gets only a cold outline.
            Vector3[] positions =
            {
                new(4f, 7.15f, 15f), new(12f, 7.15f, 15f), new(4f, 7.15f, 27f), new(12f, 7.15f, 27f),
                new(36f, 7.15f, 15f), new(44f, 7.15f, 15f), new(36f, 7.15f, 27f), new(44f, 7.15f, 27f),
                new(4f, 3.28f, 3.5f), new(15.5f, 3.28f, 3.5f), new(32.5f, 3.28f, 3.5f), new(43.5f, 3.28f, 3.5f),
                new(28f, 3.28f, 30f), new(36f, 3.28f, 30f), new(7f, -.72f, 27f), new(18f, -.72f, 32f),
                new(15.5f, 4.8f, -4.5f), new(32.5f, 4.8f, -4.5f),
            };
            for (int index = 0; index < positions.Length; index++)
            {
                Transform holder = Group("Fixture_" + index, lights);
                holder.position = positions[index];
                GameObject glow = Box("Diffuser", holder, Vector3.zero, new Vector3(1.32f, .12f, .52f), m.Light, false);
                var lampRoot = Group("Light", holder);
                lampRoot.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Light lamp = lampRoot.gameObject.AddComponent<Light>();
                lamp.type = LightType.Spot;
                lamp.range = index < 8 ? 15f : 11f;
                lamp.spotAngle = 106f;
                lamp.intensity = index < 8 ? 5.5f : 3.85f;
                lamp.color = new Color(.56f, .76f, .92f);
                lamp.shadows = LightShadows.None;
                holder.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, glow.GetComponent<Renderer>(),
                    index is 1 or 6 or 14, index == 7, 480 + index, m.Light.GetColor("_EmissionColor"));
            }
        }

        private static IEnumerable<Vector3> LightPositions(Plan plan)
        {
            // A fixed grid makes every facility navigable under a power-off contract while the theme changes the colour and density.
            for (int z = 1; z < Depth; z += 2)
            for (int x = 1; x < Width; x += 2)
                if ((x + z) % (plan.Theme == Theme.Factory ? 3 : 2) == 0)
                    yield return new Vector3(x * Tile, 0f, z * Tile);
            foreach (Vector2Int cell in plan.Upper.Where(c => (c.x + c.y) % 3 == 0))
                yield return TileTop(cell, 1);
        }

        // ---- Shared playable systems ----------------------------------------------------------------

        private static StructureSimulation SetupStructuralGameplay(Plan plan, IEnumerable<TilePiece> pieces, IEnumerable<Transform> stairs)
        {
            var setup = new StructureSceneSetup();
            if (!setup.IsValid) throw new InvalidOperationException("Missing structural content for location build.");
            foreach (TilePiece piece in pieces)
            {
                bool collapsible = piece.Floor == 1 && !piece.Protected;
                float health = 1f, capacity = 1f;
                if (piece.Floor == 1 && plan.WeakUpper.TryGetValue(piece.Cell, out Vector2 weak)) { health = weak.x; capacity = weak.y; }
                setup.AddSection(piece.Transform, SectionType.Floor, collapsible, health, capacity);
                if (collapsible) SectionNavCarver.EditorAdd(piece.Transform.gameObject, new Vector3(Tile, Slab, Tile));
                if (piece.Protected) piece.Transform.gameObject.AddComponent<SectionKeepClear>();
            }
            foreach (Transform stair in stairs)
                setup.AddSection(stair, SectionType.Stair, false, fractured: false);
            return setup.AddSimulation(plan.Stability, plan.Seed);
        }

        private static void PopulateGameplay(Transform root, Plan plan, StructureSimulation simulation)
        {
            new GameObject("DebugView", typeof(DebugViewToggle), typeof(PerfOverlay));
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null && camera.GetComponent<CinemachineBrain>() == null) camera.gameObject.AddComponent<CinemachineBrain>();

            Transform spawns = Group("Spawns", root);
            for (int i = 0; i < 4; i++)
            {
                Transform spawn = Group("PlayerSpawn_" + i, spawns);
                spawn.SetPositionAndRotation(new Vector3(20f + i * 1.5f, .05f, -6.5f), Quaternion.LookRotation(Vector3.forward));
                spawn.gameObject.AddComponent<PlayerSpawnPoint>().EditorSetup(i);
            }

            NetworkBootstrap bootstrap = NetworkSceneBuilder.Add();
            Transform points = Group("LootPoints", root);
            foreach (LootSite site in plan.Loot)
            {
                Transform point = Group(site.Name, points);
                point.position = site.At;
                point.gameObject.AddComponent<LootSpawnPoint>().EditorSetup(site.Tag, site.Min, site.Max, site.IsJackpot);
            }
            var spawner = new GameObject("LootSpawner").AddComponent<NetworkLootSpawner>();
            spawner.Setup(bootstrap, AssetDatabase.LoadAssetAtPath<LootCatalog>(LootCatalogBuilder.CatalogPath),
                AssetDatabase.LoadAssetAtPath<LootSpawnConfig>(LootCatalogBuilder.SpawnConfigPath), autoSpawn: false);

            var run = new GameObject("Run");
            var director = run.AddComponent<RunDirector>();
            GameObject runState = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkContentBuilder.RunStatePrefabPath);
            director.Setup(bootstrap, runState != null ? runState.GetComponent<NetworkObject>() : null, spawner, simulation);
            run.AddComponent<RunHud>();
            run.AddComponent<PowerController>();
            SerializedWiring.Set(run.AddComponent<AppraisalScreen>(), "director", director);

            Transform spawnPoints = Group("ThreatSpawns", root);
            foreach (Vector3 at in plan.Threats)
            {
                Transform marker = Group("ThreatSpawn_" + spawnPoints.childCount.ToString("00"), spawnPoints);
                marker.position = at;
                marker.gameObject.AddComponent<ThreatSpawnPoint>();
            }
            BuildThreats(bootstrap, simulation);
            var views = new GameObject("DebugViews");
            views.AddComponent<NoiseDebugView>();
            views.AddComponent<LootDebugView>();
            views.AddComponent<SharedCarryDebugView>();
        }

        private static void BuildThreats(NetworkBootstrap bootstrap, StructureSimulation simulation)
        {
            NetworkObject Prefab(string path)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                return go != null ? go.GetComponent<NetworkObject>() : null;
            }
            NetworkObject blind = Prefab(ThreatContentBuilder.BlindOnePrefabPath);
            var threats = new GameObject("Threats");
            var director = threats.AddComponent<ThreatDirector>();
            director.Setup(bootstrap, blind, new[]
            {
                blind, Prefab(ThreatContentBuilder.StalkerPrefabPath), Prefab(ThreatContentBuilder.CollectorPrefabPath), Prefab(ThreatContentBuilder.HunterPrefabPath)
            }, new[] { .4f, .2f, .2f, .2f });
            director.SetupCatalog(AssetDatabase.LoadAssetAtPath<ThreatCatalog>(NewThreatContentBuilder.CatalogPath));
            var danger = threats.AddComponent<DangerDirector>();
            danger.Setup(AssetDatabase.LoadAssetAtPath<DangerConfig>(NetworkContentBuilder.DangerConfigPath), simulation);
        }

        private static void BakeNavMesh(Transform root, Plan plan)
        {
            var surface = root.gameObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            // Collapsible floors own disabled carving obstacles. Include their floor colliders in the initial bake;
            // the obstacle only carves its tile later if that section actually collapses.
            surface.ignoreNavMeshObstacle = false;
            surface.layerMask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            // The outdoor staging area is intentionally not a threat-navigation island: crews must enter the
            // building before a threat can route to them. Roof and truck collider islands are likewise cosmetic.
            foreach (string groupPath in new[] { "Exterior", "Truck", "Shell/Roof", "MeridianCollectionBay" })
            {
                Transform group = root.Find(groupPath);
                if (group == null) continue;
                var modifier = group.GetComponent<NavMeshModifier>() ?? group.gameObject.AddComponent<NavMeshModifier>();
                modifier.ignoreFromBuild = true;
            }
            surface.BuildNavMesh();
            string path = $"Assets/_Project/Scenes/{plan.Name}_NavMesh.asset";
            var previous = AssetDatabase.LoadAssetAtPath<UnityEngine.AI.NavMeshData>(path);
            if (previous == null)
            {
                surface.navMeshData.name = plan.Name + "_NavMesh";
                AssetDatabase.CreateAsset(surface.navMeshData, path);
            }
            else
            {
                surface.RemoveData();
                EditorUtility.CopySerialized(surface.navMeshData, previous);
                Object.DestroyImmediate(surface.navMeshData);
                surface.navMeshData = previous;
                previous.name = plan.Name + "_NavMesh";
                surface.AddData();
                EditorUtility.SetDirty(previous);
            }
        }

        private static void ConfigureHorrorScenes()
        {
            HorrorConfig config = AssetDatabase.LoadAssetAtPath<HorrorConfig>(HorrorContentBuilder.Path);
            if (config == null) return;
            var serialized = new SerializedObject(config);
            SerializedProperty levels = serialized.FindProperty("levels");
            if (levels != null)
            {
                string[] names = { "Mall", "School", "Factory", "Prison" };
                levels.arraySize = names.Length;
                for (int i = 0; i < names.Length; i++) levels.GetArrayElementAtIndex(i).stringValue = names[i];
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(config);
            }
        }

        private static void PlaceCamera(Plan plan)
        {
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null) return;
            camera.transform.position = new Vector3(24f, 17f, -21f);
            camera.transform.LookAt(new Vector3(24f, 2f, 20f));
        }

        // ---- Plans ----------------------------------------------------------------------------------

        private static Plan CreateSchool()
        {
            var p = Base(Theme.School, "School", SchoolBuilder.ScenePath, new Color(.32f, .38f, .40f), new Color(.43f, .38f, .27f), new Color(.12f, .45f, .63f));
            FillUpper(p, c => !(c.x >= 7 && c.y >= 4) && c != new Vector2Int(6, 4) && c != new Vector2Int(6, 5));
            FillLower(p, new RectInt(0, 6, 5, 3));
            p.GroundHoles.UnionWith(new[] { new Vector2Int(1, 4), new Vector2Int(1, 5), new Vector2Int(6, 4), new Vector2Int(6, 5) });
            p.ProtectedUpper.UnionWith(new[] { new Vector2Int(0, 5), new Vector2Int(1, 5), new Vector2Int(2, 5), new Vector2Int(3, 5), new Vector2Int(4, 5), new Vector2Int(5, 5) });
            p.WeakUpper[new Vector2Int(4, 2)] = new Vector2(.58f, .38f);
            p.WeakUpper[new Vector2Int(5, 2)] = new Vector2(.70f, .55f);
            p.Rooms.AddRange(new[]
            {
                new Room("Classroom_A", "office", 0, 0, 0, 3, 2, doors: RoomDoor.North), new Room("Classroom_B", "office", 0, 3, 0, 2, 2, doors: RoomDoor.North),
                new Room("Cafeteria", "food", 0, 7, 0, 3, 2, doors: RoomDoor.North), new Room("Auditorium", "cinema", 0, 0, 3, 4, 2, true),
                // The east door keeps the library tied into the central hall even when the south approach is obstructed.
                new Room("Library", "gallery", 0, 0, 6, 4, 3, doors: RoomDoor.South | RoomDoor.East), new Room("Gymnasium", "furniture", 0, 7, 4, 5, 5, true),
                new Room("Science_Wing", "office", 1, 2, 0, 4, 2, doors: RoomDoor.North),
                // The archive's doorway faces its stair (x 6), not the wall's middle: it left a 0.8 m way on (0.24.11).
                new Room("Basement_Archive", "stock", -1, 0, 6, 4, 3, doorAt: 6f),
            });
            p.Stairs.Add(new Stair("MainStair", new Vector3(26f, 0f, 16f), Vector3.forward, Story));
            p.Stairs.Add(new Stair("ArchiveStair", new Vector3(6f, -Story, 24f), Vector3.back, Story));
            AddLoot(p, "school", new[] { ("office", 6f, 6f, 0), ("office", 18f, 6f, 0), ("food", 29f, 6f, 0), ("cinema", 9f, 14f, 0), ("gallery", 12f, 29f, 0), ("furniture", 39f, 29f, 0), ("stock", 8f, 29f, -1), ("office", 17f, 6f, 1) }, 4, 5);
            Threats(p, new Vector3(43f, 0f, 35f), new Vector3(34f, 0f, 26f), new Vector3(12f, 0f, 34f), new Vector3(18f, 4f, 6f), new Vector3(7f, -4f, 29f));
            return p;
        }

        private static Plan CreateFactory()
        {
            var p = Base(Theme.Factory, "Factory", FactoryBuilder.ScenePath, new Color(.22f, .24f, .25f), new Color(.25f, .28f, .27f), new Color(.78f, .39f, .08f));
            FillUpper(p, c => (c.x <= 2 || c.x >= 9 || c.y <= 1 || c.y >= 8)
                              && c != new Vector2Int(1, 4) && c != new Vector2Int(1, 5)
                              && c != new Vector2Int(10, 4) && c != new Vector2Int(10, 5));
            p.GroundHoles.UnionWith(new[] { new Vector2Int(1, 4), new Vector2Int(1, 5), new Vector2Int(10, 4), new Vector2Int(10, 5) });
            p.ProtectedUpper.UnionWith(p.Upper.Where(c => c.y == 1 || c.y == 8));
            p.WeakUpper[new Vector2Int(9, 5)] = new Vector2(.48f, .34f);
            p.WeakUpper[new Vector2Int(10, 5)] = new Vector2(.62f, .48f);
            p.Rooms.AddRange(new[]
            {
                new Room("Receiving_Dock", "stock", 0, 8, 0, 4, 3, doors: RoomDoor.West | RoomDoor.North),
                // A low equipment divider preserves a continuous catwalk route between the hall and upper offices.
                new Room("Machine_Hall", "stock", 0, 0, 2, 8, 5, doors: RoomDoor.South | RoomDoor.North),
                new Room("Boiler_Room", "stock", 0, 8, 6, 4, 3, doors: RoomDoor.West), new Room("Admin_Office", "office", 1, 0, 0, 3, 2, doors: RoomDoor.North),
                new Room("Control_Room", "office", 1, 9, 0, 3, 2, doors: RoomDoor.North), new Room("Break_Room", "food", 1, 9, 8, 3, 2),
            });
            p.Stairs.Add(new Stair("CatwalkStair", new Vector3(6f, 0f, 16f), Vector3.forward, Story));
            p.Stairs.Add(new Stair("ControlCatwalkStair", new Vector3(42f, 0f, 16f), Vector3.forward, Story));
            AddLoot(p, "factory", new[] { ("stock", 8f, 13f, 0), ("stock", 16f, 21f, 0), ("stock", 27f, 17f, 0), ("stock", 34f, 9f, 0), ("stock", 45f, 31f, 0), ("office", 6f, 6f, 1), ("office", 42f, 6f, 1), ("food", 42f, 34f, 1) }, 3, 4);
            Threats(p, new Vector3(42f, 0f, 34f), new Vector3(31f, 0f, 26f), new Vector3(10f, 0f, 30f), new Vector3(6f, 4f, 34f), new Vector3(42f, 4f, 34f));
            return p;
        }

        private static Plan CreatePrison()
        {
            var p = Base(Theme.Prison, "Prison", PrisonBuilder.ScenePath, new Color(.35f, .37f, .38f), new Color(.31f, .33f, .34f), new Color(.48f, .62f, .72f));
            FillUpper(p, c => c.y >= 3 && c.y <= 7 && (c.x <= 2 || c.x >= 9) && c != new Vector2Int(1, 4) && c != new Vector2Int(1, 5) && c != new Vector2Int(10, 4) && c != new Vector2Int(10, 5));
            FillLower(p, new RectInt(0, 6, 6, 3));
            p.GroundHoles.UnionWith(new[]
            {
                new Vector2Int(1, 4), new Vector2Int(1, 5), new Vector2Int(10, 4), new Vector2Int(10, 5),
                // The basement has its own shaft, aligned with the service-tunnel doorway rather than sharing a gallery stair.
                new Vector2Int(3, 4), new Vector2Int(3, 5)
            });
            p.ProtectedUpper.UnionWith(p.Upper.Where(c => c.x == 0 || c.x == 11));
            p.WeakUpper[new Vector2Int(9, 5)] = new Vector2(.44f, .30f);
            p.WeakUpper[new Vector2Int(10, 5)] = new Vector2(.57f, .42f);
            p.Rooms.AddRange(new[]
            {
                new Room("Visitation", "office", 0, 0, 0, 5, 2, doors: RoomDoor.North), new Room("Kitchen", "food", 0, 7, 0, 5, 2, doors: RoomDoor.North),
                new Room("Cellblock_A", "office", 0, 0, 3, 5, 4, true), new Room("Cellblock_B", "office", 0, 7, 3, 5, 4, true),
                new Room("Laundry", "stock", 0, 6, 7, 4, 2, doors: RoomDoor.North), new Room("Upper_Gallery_A", "office", 1, 0, 3, 3, 5),
                // The tunnel stair, its shaft (column 3) and the tunnel doorway line up at x 14. The stair stood at x 12,
                // half under the ground floor, beside a drop into nothing (0.24.11).
                new Room("Upper_Gallery_B", "office", 1, 9, 3, 3, 5), new Room("Service_Tunnel", "stock", -1, 0, 6, 6, 3, doorAt: 14f),
            });
            p.Stairs.Add(new Stair("GalleryStair", new Vector3(6f, 0f, 16f), Vector3.forward, Story));
            p.Stairs.Add(new Stair("EastGalleryStair", new Vector3(42f, 0f, 16f), Vector3.forward, Story));
            p.Stairs.Add(new Stair("TunnelStair", new Vector3(14f, -Story, 24f), Vector3.back, Story));
            AddLoot(p, "prison", new[] { ("office", 8f, 6f, 0), ("food", 35f, 6f, 0), ("office", 8f, 24f, 0), ("office", 38f, 24f, 0), ("stock", 29f, 32f, 0), ("office", 6f, 22f, 1), ("office", 42f, 22f, 1), ("stock", 4f, 33f, -1) }, 4, 7);
            Threats(p, new Vector3(43f, 0f, 31f), new Vector3(33f, 0f, 24f), new Vector3(9f, 0f, 24f), new Vector3(5f, 4f, 24f), new Vector3(43f, 4f, 24f), new Vector3(12f, -4f, 30f));
            return p;
        }

        private static Plan Base(Theme theme, string name, string path, Color shell, Color floor, Color accent) => new()
        {
            Theme = theme, Name = name, Path = path, Shell = shell, Floor = floor, Accent = accent,
            Prop = Color.Lerp(floor, accent, .25f), Stability = theme == Theme.Factory ? .68f : theme == Theme.Prison ? .72f : .76f,
            Seed = theme == Theme.School ? 1209 : theme == Theme.Factory ? 2209 : 3209
        };

        private static void FillUpper(Plan plan, Func<Vector2Int, bool> include)
        {
            foreach (Vector2Int cell in AllCells()) if (include(cell)) plan.Upper.Add(cell);
        }

        private static void FillLower(Plan plan, RectInt area)
        {
            for (int x = area.xMin; x < area.xMax; x++)
            for (int z = area.yMin; z < area.yMax; z++) plan.Lower.Add(new Vector2Int(x, z));
        }

        private static void AddLoot(Plan plan, string prefix, IEnumerable<(string tag, float x, float z, int floor)> sites, params int[] jackpotIndices)
        {
            int i = 0;
            foreach ((string tag, float x, float z, int floor) in sites)
            {
                bool jackpot = jackpotIndices.Contains(i);
                CarryClass max = jackpot ? CarryClass.Huge : i % 3 == 0 ? CarryClass.Heavy : CarryClass.TwoHand;
                plan.Loot.Add(new LootSite($"{prefix}_loot_{i:00}", tag, new Vector3(x, FloorY(floor), z), CarryClass.Pocket, max, jackpot));
                i++;
            }
        }

        private static void Threats(Plan plan, params Vector3[] points) => plan.Threats.AddRange(points);

        private static IEnumerable<Vector2Int> AllCells()
        {
            for (int z = 0; z < Depth; z++)
            for (int x = 0; x < Width; x++) yield return new Vector2Int(x, z);
        }

        private static float FloorY(int floor) => floor * Story;
        private static Vector3 TileTop(Vector2Int cell, int floor) => new((cell.x + .5f) * Tile, FloorY(floor), (cell.y + .5f) * Tile);

        private static LevelAtmosphere.Mood MoodFor(Theme theme) => theme switch
        {
            Theme.School => new LevelAtmosphere.Mood(new Color(.055f, .075f, .085f), new Color(.028f, .040f, .045f), new Color(.012f, .018f, .020f), new Color(.105f, .135f, .135f), .030f, .20f, .30f, .19f, -39f, 13f, new Color(.72f, .86f, .82f), .095f, new Vector3(60f, -33f, 0f)),
            Theme.Factory => new LevelAtmosphere.Mood(new Color(.075f, .075f, .065f), new Color(.05f, .047f, .038f), new Color(.02f, .018f, .014f), new Color(.18f, .15f, .11f), .028f, .20f, .27f, .2f, -42f, 15f, new Color(1f, .68f, .42f), .10f, new Vector3(62f, -35f, 0f)),
            _ => new LevelAtmosphere.Mood(new Color(.035f, .055f, .072f), new Color(.022f, .035f, .043f), new Color(.008f, .013f, .016f), new Color(.095f, .135f, .15f), .034f, .13f, .36f, .22f, -46f, 17f, new Color(.48f, .67f, .88f), .075f, new Vector3(58f, -31f, 0f)),
        };

        private sealed class Materials
        {
            public readonly Material Shell, Floor, UpperFloor, Accent, Prop, Metal, Wood, Concrete, Asphalt, Dirt, Light;
            public Materials(Plan p)
            {
                Shell = GetMaterial("Location_" + p.Name + "_Shell", p.Shell);
                Floor = GetMaterial("Location_" + p.Name + "_Floor", p.Floor);
                UpperFloor = GetMaterial("Location_" + p.Name + "_Upper", Color.Lerp(p.Floor, p.Accent, .15f));
                Accent = GetMaterial("Location_" + p.Name + "_Accent", p.Accent);
                Prop = GetMaterial("Location_" + p.Name + "_Prop", p.Prop);
                Metal = GetMaterial("Location_Metal", new Color(.22f, .25f, .27f));
                Wood = GetMaterial("Location_Wood", new Color(.34f, .22f, .12f));
                Concrete = GetMaterial("Location_Concrete", new Color(.34f, .35f, .34f));
                Asphalt = GetMaterial("Location_Asphalt", new Color(.10f, .11f, .12f));
                Dirt = GetMaterial("Location_Dirt", new Color(.20f, .22f, .18f));
                Color lightColor = p.Theme == Theme.Prison ? new Color(.36f, .64f, .84f) : new Color(1f, .92f, .72f);
                Light = GetMaterial("Location_" + p.Name + "_Light", lightColor);
                Light.SetColor("_EmissionColor", p.Theme == Theme.Prison ? new Color(.42f, .90f, 1.3f) : new Color(1.7f, 1.4f, .85f));
                Light.EnableKeyword("_EMISSION");
            }
            public Material WallFor(string tag) => tag switch
            {
                "food" => Accent,
                "stock" => Metal,
                "gallery" => Wood,
                _ => Shell,
            };
        }
    }

    public static class SchoolBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/School.unity";
        [MenuItem("Tools/Abandoned/Create School")]
        public static void Create() => SalvageLocationBuilder.BuildSchool();
        public static void Build() => SalvageLocationBuilder.BuildSchool();
    }

    public static class FactoryBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Factory.unity";
        [MenuItem("Tools/Abandoned/Create Factory")]
        public static void Create() => SalvageLocationBuilder.BuildFactory();
        public static void Build() => SalvageLocationBuilder.BuildFactory();
    }

    public static class PrisonBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Prison.unity";
        [MenuItem("Tools/Abandoned/Create Prison")]
        public static void Create() => SalvageLocationBuilder.BuildPrison();
        public static void Build() => SalvageLocationBuilder.BuildPrison();
    }
}
