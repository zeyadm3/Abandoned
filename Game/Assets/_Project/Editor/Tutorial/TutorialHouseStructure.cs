using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using Abandoned.Tutorial;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.TutorialHouseLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The house's floors as structural sections (timber: they creak and crack like any level's), the basement under
    /// the west wing, the bulkhead stair down to it from the patio, the yard and the ceilings. Two sections teach:
    /// the hallway (a weak floor that cracks under a heavy carry but never gives way) and
    /// the middle of the south bedroom (it falls under the first player to stand on it, 2.4 m into the basement).
    /// Everything else can never collapse.
    /// </summary>
    public static class TutorialHouseStructure
    {
        public const string HallwayKey = "hallway", BedroomBKey = "bedroom_b";

        // The hallway holds a person and anything up to two-handed; a person and the grandfather clock overload it.
        public const float HallwayCapacity = 0.05f, HallwayNeverBelow = 0.2f;

        public sealed class Result
        {
            public readonly List<StructuralSection> Sections = new();
            public StructuralSection Hallway, Patch;
            public StructureSimulation Simulation;

            /// <summary>
            /// The house floor section under a point near floor level, or null. From each tile's own box, not
            /// Collider.bounds, which can still be empty for colliders made a moment ago in the editor.
            /// </summary>
            public StructuralSection Under(Vector3 at)
            {
                foreach (StructuralSection s in Sections)
                {
                    var box = s.GetComponent<BoxCollider>();
                    if (box == null) continue;
                    Vector3 c = s.transform.position + box.center, half = box.size * .5f;
                    float top = c.y + half.y;
                    if (Mathf.Abs(at.x - c.x) <= half.x && Mathf.Abs(at.z - c.z) <= half.z && Mathf.Abs(at.y - top) < .4f) return s;
                }
                return null;
            }
        }

        private readonly struct Piece
        {
            public readonly string Name;
            public readonly Rect Area;
            public readonly Material Material;
            public readonly SurfaceMaterial Surface;
            public Piece(string name, Rect area, Material material, SurfaceMaterial surface) { Name = name; Area = area; Material = material; Surface = surface; }
        }

        public static Result Build(Transform root, TutorialHouseArt art)
        {
            var result = new Result();
            Transform floors = Group("Floors", root);
            var setup = new StructureSceneSetup();
            Rect patch = L.BedroomBPatch;

            Rect b = L.BedroomB;
            var pieces = new List<Piece>
            {
                new("Floor_BedroomA", L.BedroomA, art.Carpet, SurfaceMaterial.Carpet),
                new("Floor_HallBath", L.HallBath, art.Tile, SurfaceMaterial.Tile),
                // The south bedroom around its rotten middle: the door strip (north), the closet side (east), two slivers.
                new("Floor_BedroomB_North", Rect.MinMaxRect(b.xMin, patch.yMax, b.xMax, b.yMax), art.Carpet, SurfaceMaterial.Carpet),
                new("Floor_BedroomB_East", Rect.MinMaxRect(patch.xMax, b.yMin, b.xMax, patch.yMax), art.Carpet, SurfaceMaterial.Carpet),
                new("Floor_BedroomB_South", Rect.MinMaxRect(b.xMin, b.yMin, patch.xMax, patch.yMin), art.Carpet, SurfaceMaterial.Carpet),
                new("Floor_BedroomB_West", Rect.MinMaxRect(b.xMin, patch.yMin, patch.xMin, patch.yMax), art.Carpet, SurfaceMaterial.Carpet),
                new("Floor_Kitchen", L.Kitchen, art.Linoleum, SurfaceMaterial.Tile),
                new("Floor_Dining", L.Dining, art.Oak, SurfaceMaterial.Wood),
                new("Floor_Living", L.Living, art.Carpet, SurfaceMaterial.Carpet),
                new("Floor_Laundry", L.Laundry, art.Linoleum, SurfaceMaterial.Tile),
                new("Floor_Ensuite", L.Ensuite, art.Tile, SurfaceMaterial.Tile),
                new("Floor_Master", L.Master, art.Carpet, SurfaceMaterial.Carpet),
            };
            foreach (Piece p in pieces)
            {
                Transform tile = Tile(floors, p.Name, p.Area, p.Material, p.Surface, planks: false, art);
                StructuralSection s = setup.AddSection(tile, SectionType.Floor, false, fractured: false);
                s.EditorSetFloorKind(FloorKind.Timber);
                tile.gameObject.AddComponent<SectionKeepClear>();
                result.Sections.Add(s);
            }

            // The hallway: weak timber over the basement that cracks and groans, and can be braced from below.
            Transform hall = Tile(floors, "Floor_Hallway", L.Hallway, art.Oak, SurfaceMaterial.Wood, planks: true, art);
            result.Hallway = setup.AddSection(hall, SectionType.Floor, true, 1f, HallwayCapacity, fractured: false);
            result.Hallway.EditorSetFloorKind(FloorKind.Timber);
            result.Hallway.EditorSetNeverBelow(HallwayNeverBelow);
            hall.gameObject.AddComponent<TutorialSection>().EditorSetup(HallwayKey);
            result.Sections.Add(result.Hallway);

            // The south bedroom's middle: rotten joists. It falls in boards (the debris is its own planks).
            Transform fall = Tile(floors, "Floor_BedroomB_Rotten", patch, art.Carpet, SurfaceMaterial.Carpet, planks: true, art);
            // Sound until the lesson: a floor that creaked all job would teach the wrong thing about creaks.
            result.Patch = setup.AddSection(fall, SectionType.Floor, true, 1f, 1f, fractured: false);
            result.Patch.EditorSetFloorKind(FloorKind.Timber);
            fall.gameObject.AddComponent<TutorialSection>().EditorSetup(BedroomBKey);
            SectionNavCarver.EditorAdd(fall.gameObject, new Vector3(patch.width, L.Slab, patch.height));
            result.Sections.Add(result.Patch);

            // Outside, at the porch: a concrete stoop a step up from the walk, with a gentle ramp for the trolley.
            Transform porch = Group("Porch", root);
            Rect stoop = Rect.MinMaxRect(L.Porch.xMin + L.ExteriorWall * .5f, L.Porch.yMin, L.Porch.xMax, L.Porch.yMax);
            GameObject slab = Box("Stoop", porch, new Vector3(stoop.center.x, L.Floor - .2f, stoop.center.y), new Vector3(stoop.width, .4f, stoop.height), art.Concrete);
            slab.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            Ramp(porch, "StoopRamp", new Vector3(stoop.xMax, L.Floor, stoop.center.y), Vector3.right, .9f, L.Grade - L.Floor, stoop.height, art.Concrete);
            Box("StoopSnow", porch, new Vector3(stoop.xMax - .15f, L.Floor + .015f, stoop.yMax - .15f), new Vector3(.3f, .03f, .3f), art.SnowDrift, false);

            Basement(root, art);
            Bulkhead(root, art);
            Ground(root, art);
            Ceilings(root, art);

            result.Simulation = setup.AddSimulation(stability: 1f, seed: 1214);
            return result;
        }

        // A floor section: an unscaled root at the slab's top centre, its collider, and a Visual (boards when it must break up).
        private static Transform Tile(Transform parent, string name, Rect area, Material material, SurfaceMaterial surface, bool planks, TutorialHouseArt art)
        {
            Transform tile = Group(name, parent);
            tile.position = L.Centre(area);
            var collider = tile.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.down * (L.Slab * .5f);
            collider.size = new Vector3(area.width, L.Slab, area.height);
            Transform visual = Group("Visual", tile);
            if (!planks)
            {
                Box("Surface", visual, Vector3.down * (L.Slab * .5f), new Vector3(area.width, L.Slab, area.height), material, false);
            }
            else
            {
                // Boards across the short side, so a collapse spills planks (and the hallway's cracks run between them).
                bool alongX = area.width >= area.height;
                float span = alongX ? area.height : area.width, length = alongX ? area.width : area.height;
                int boards = Mathf.Max(4, Mathf.RoundToInt(span / .35f));
                int segments = Mathf.Max(1, Mathf.RoundToInt(length / 1.3f));
                for (int b = 0; b < boards; b++)
                for (int s = 0; s < segments; s++)
                {
                    float across = -span * .5f + (b + .5f) * span / boards, down = -length * .5f + (s + .5f) * length / segments;
                    Vector3 at = alongX ? new Vector3(down, -L.Slab * .5f, across) : new Vector3(across, -L.Slab * .5f, down);
                    Vector3 size = alongX ? new Vector3(length / segments - .01f, L.Slab, span / boards - .008f) : new Vector3(span / boards - .008f, L.Slab, length / segments - .01f);
                    Box($"Board_{b}_{s}", visual, at, size, (b + s) % 5 == 2 ? art.OakDark : material, false);
                }
            }
            tile.gameObject.AddComponent<SurfaceTag>().EditorSet(surface);
            return tile;
        }

        private static void Basement(Transform root, TutorialHouseArt art)
        {
            Transform basement = Group("Basement", root);
            float y = L.BasementFloor;
            foreach ((string name, Rect r) in new[] { ("Main", L.BasementMain), ("Strip", L.BasementStrip) })
            {
                GameObject floor = Box("Slab_" + name, basement, new Vector3(r.center.x, y - .15f, r.center.y), new Vector3(r.width, .3f, r.height), art.Concrete);
                floor.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            }
            float top = -L.Slab, h = top - y + .2f, mid = (top + y - .2f) * .5f;
            const float t = .25f;
            Rect m = L.BasementMain, s = L.BasementStrip;
            void Wall(string name, float x0, float z0, float x1, float z1) =>
                Box(name, basement, new Vector3((x0 + x1) * .5f, mid, (z0 + z1) * .5f), new Vector3(Mathf.Max(t, x1 - x0), h, Mathf.Max(t, z1 - z0)), art.BasementWall);
            // The L of foundation walls: the wing's west and north, its east wall north of the strip, the strip's
            // north and east, and one south wall under both.
            Wall("Wall_West", m.xMin - t, m.yMin - t, m.xMin, m.yMax + t);
            Wall("Wall_North", m.xMin - t, m.yMax, m.xMax + t, m.yMax + t);
            Wall("Wall_Jog", m.xMax, s.yMax, m.xMax + t, m.yMax);
            Wall("Wall_StripNorth", s.xMin, s.yMax, s.xMax + t, s.yMax + t);
            Wall("Wall_StripEast", s.xMax, s.yMin - t, s.xMax + t, s.yMax + t);
            Wall("Wall_South", m.xMin - t, m.yMin - t, s.xMax + t, m.yMin);
            // Joists under the wing's floors, except over the rotten patch (a falling player must not land on one).
            Rect patch = L.BedroomBPatch;
            for (float z = m.yMin + .4f; z < m.yMax - .1f; z += .6f)
            {
                bool overPatch = z > patch.yMin - .1f && z < patch.yMax + .1f;
                float x0 = overPatch ? patch.xMax + .05f : m.xMin;
                if (m.xMax - x0 > .2f) Box("Joist", basement, new Vector3((x0 + m.xMax) * .5f, -L.Slab - .1f, z), new Vector3(m.xMax - x0, .2f, .05f), art.Oak, false);
            }
            // The beam and its columns run north-south through the middle, clear of the patch and of the hallway.
            const float beamX = 3.6f;
            float beamFrom = patch.yMax + .4f;
            Box("SteelBeam", basement, new Vector3(beamX, -L.Slab - .17f, (beamFrom + m.yMax) * .5f), new Vector3(.12f, .14f, m.yMax - beamFrom), art.Steel, false);
            for (float z = beamFrom + 1.6f; z < m.yMax - .5f; z += 3.2f)
                Primitive(PrimitiveType.Cylinder, "LallyColumn", basement, new Vector3(beamX, (y - L.Slab - .24f) * .5f, z), new Vector3(.1f, (-L.Slab - .24f - y) * .5f, .1f), art.DarkMetal);
        }

        // The bulkhead: concrete cheeks, a stair down northward, steel doors flung open, rails at the top.
        private static void Bulkhead(Transform root, TutorialHouseArt art)
        {
            Transform bulk = Group("Bulkhead", root);
            Rect b = L.Bulkhead;
            float depth = L.Grade - L.BasementFloor;
            Transform stair = Ramp(bulk, "BulkheadStair", new Vector3(b.center.x, L.Grade, b.yMin), Vector3.forward, b.height, -depth, b.width, art.Concrete, steps: 12);
            stair.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            // The patio slab over the basement strip, with the stairwell left open.
            Rect s = Rect.MinMaxRect(L.BasementStrip.xMin, L.BasementStrip.yMin, L.BasementStrip.xMax, L.BandSouth);
            void Slab(string name, Rect r)
            {
                if (r.width <= .01f || r.height <= .01f) return;
                Box(name, bulk, new Vector3(r.center.x, L.Grade - .125f, r.center.y), new Vector3(r.width, .25f, r.height), art.Concrete).AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
                Box(name + "_Snow", bulk, new Vector3(r.center.x, L.Grade + .012f, r.center.y), new Vector3(r.width, .024f, r.height), art.Snow, false);
            }
            Slab("PatioSlab_West", Rect.MinMaxRect(s.xMin, s.yMin, b.xMin, s.yMax));
            Slab("PatioSlab_East", Rect.MinMaxRect(b.xMax, s.yMin, s.xMax, s.yMax));
            Slab("PatioSlab_South", Rect.MinMaxRect(b.xMin, s.yMin, b.xMax, b.yMin));
            Slab("PatioSlab_North", Rect.MinMaxRect(b.xMin, b.yMax, b.xMax, s.yMax));
            foreach (float x in new[] { b.xMin - .08f, b.xMax + .08f })
            {
                Box("Cheek", bulk, new Vector3(x, L.Grade + .2f, b.center.y), new Vector3(.16f, .4f, b.height), art.Concrete);
                Box("RailTop", bulk, new Vector3(x, L.Grade + 1.05f, b.center.y), new Vector3(.05f, .05f, b.height), art.DarkMetal, false);
                // One continuous invisible barrier keeps hauled salvage from sliding off the side.
                GameObject barrier = new("RailBarrier");
                barrier.transform.SetParent(bulk, false);
                barrier.transform.position = new Vector3(x, L.Grade + .7f, b.center.y);
                barrier.AddComponent<BoxCollider>().size = new Vector3(.1f, .7f, b.height);
                barrier.AddComponent<IntentionalInvisibleCollider>().EditorSetup("Bulkhead railing: one solid face between the posts so salvage can't slip off the stair's side.");
                for (float z = b.yMin; z <= b.yMax + .01f; z += .8f)
                    Box("RailPost", bulk, new Vector3(x, L.Grade + .7f, z), new Vector3(.05f, .7f, .05f), art.DarkMetal, false);
            }
            // The bulkhead's steel leaves: the east one thrown open flat on the snow, the west one leaning on the house.
            Box("BulkheadLeaf_East", bulk, new Vector3(b.xMax + .65f, L.Grade + .03f, b.yMin + .9f), new Vector3(.9f, .05f, 1.6f), art.Rust, false);
            Box("BulkheadLeaf_West", bulk, new Vector3(L.WingEast + L.ExteriorWall * .5f + .06f, L.Grade + .8f, b.yMin + .9f), new Vector3(.05f, 1.6f, 1.6f), art.Rust, false);
            // Across the top of the well on the house side: the patio slider opens right beside it.
            Box("RailTop_North", bulk, new Vector3(b.center.x, L.Grade + 1.05f, b.yMax), new Vector3(b.width + .16f, .05f, .05f), art.DarkMetal, false);
            Box("Cheek_North", bulk, new Vector3(b.center.x, L.Grade + .2f, b.yMax + .08f), new Vector3(b.width + .32f, .4f, .16f), art.Concrete);
            GameObject north = new("RailBarrier_North");
            north.transform.SetParent(bulk, false);
            north.transform.position = new Vector3(b.center.x, L.Grade + .7f, b.yMax);
            north.AddComponent<BoxCollider>().size = new Vector3(b.width, .7f, .1f);
            north.AddComponent<IntentionalInvisibleCollider>().EditorSetup("Bulkhead railing on the house side of the stairwell.");
        }

        /// <summary>
        /// A sloped walking surface (one box collider, like the levels' stairs) from <paramref name="start"/> along
        /// <paramref name="direction"/>, ending <paramref name="rise"/> higher (negative: lower), with visible steps or a smooth top.
        /// </summary>
        public static Transform Ramp(Transform parent, string name, Vector3 start, Vector3 direction, float run, float rise, float width, Material material, int steps = 0)
        {
            Transform ramp = Group(name, parent);
            ramp.position = start;
            ramp.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            // Pitching about X by a positive angle tips +Z downward, so a rising ramp pitches negative.
            float angle = -Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
            float slope = Mathf.Sqrt(run * run + rise * rise);
            Transform surface = Group("Surface", ramp);
            surface.localPosition = new Vector3(0f, rise * .5f - .06f, run * .5f);
            surface.localRotation = Quaternion.Euler(angle, 0f, 0f);
            surface.gameObject.AddComponent<BoxCollider>().size = new Vector3(width, .12f, slope);
            if (steps <= 0)
            {
                Box("Top", surface, Vector3.zero, new Vector3(width, .12f, slope), material, false);
                return ramp;
            }
            float riser = Mathf.Abs(rise / steps);
            for (int i = 0; i < steps; i++)
            {
                // Each tread's top sits on the slope at its far edge (going up) or near edge (going down).
                float top = rise * (rise > 0f ? i + 1f : i + 0f) / steps;
                Box($"Step_{i:00}", ramp, new Vector3(0f, top - riser * .5f, run * (i + .5f) / steps), new Vector3(width, riser, run / steps), material, false);
            }
            return ramp;
        }

        // The yard: snow over the lot, cut around the house, the patio slab over the cellar strip and the porch. The
        // town's land around the lot is TutorialTown's (outside the NavMesh).
        private static void Ground(Transform root, TutorialHouseArt art)
        {
            Transform ground = Group("Grounds", root);
            var holes = new List<Rect>
            {
                L.WestWing, L.Band, L.Suite, L.Porch,
                Rect.MinMaxRect(L.BasementStrip.xMin, L.BasementStrip.yMin, L.BasementStrip.xMax, L.BandSouth),
            };
            List<Rect> pieces = TutorialRects.Subtract(L.Lot, holes);
            for (int i = 0; i < pieces.Count; i++)
            {
                Rect r = pieces[i];
                GameObject g = Box($"Ground_{i:00}", ground, new Vector3(r.center.x, L.Grade - .2f, r.center.y), new Vector3(r.width, .4f, r.height), art.Snow);
                g.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Dirt);
            }
        }

        private static void Ceilings(Transform root, TutorialHouseArt art)
        {
            Transform ceilings = Group("Ceilings", root);
            foreach ((string name, Rect r) in new[] { ("WestWing", L.WestWing), ("Band", L.Band), ("Suite", L.Suite) })
                Box("Ceiling_" + name, ceilings, new Vector3(r.center.x, L.Ceiling + .05f, r.center.y), new Vector3(r.width, .1f, r.height), art.Ceiling);
        }
    }
}
