using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Structure;
using Unity.AI.Navigation;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A station's slabs and flights. Every slab is one 4 m structural section (a pierced one keeps a small opening for
    /// a fire pole), with its own ceiling face underneath where there is a room below; every flight is a Stair section
    /// that never falls, tied end to end for monsters by a NavMesh link and filled solid underneath so nobody walks
    /// under the low end of it. Which slabs may fall, and how strong they are, is the plan's
    /// (<see cref="StationPlan.Collapsible"/>).
    /// </summary>
    public static class StationFloors
    {
        public sealed class Result
        {
            public readonly Dictionary<Vector3Int, Transform> Tiles = new();
            public readonly List<Transform> Stairs = new();
        }

        private const int Treads = 18;

        public static Result Build(Transform root, StationPlan plan, StationArt art)
        {
            var result = new Result();
            Transform levels = Group("Levels", root);
            foreach (int f in plan.Floors.Keys.OrderBy(f => f))
            {
                Transform group = Group(f < 0 ? "Basement" : f == 0 ? "GroundFloor" : "Floor_" + f, levels);
                foreach (Vector2Int c in plan.Floors[f].OrderBy(c => c.y).ThenBy(c => c.x))
                    result.Tiles[P.Key(f, c)] = Tile(group, plan, art, f, c);
            }
            Transform flights = Group("Flights", root);
            foreach (P.Flight flight in plan.Stairs) result.Stairs.Add(Stair(flights, flight, art));
            Transform under = Group("UnderStairs", root);
            foreach (P.Flight flight in plan.Stairs) UnderFill(under, flight, art);
            MarkStatic(under);
            return result;
        }

        private static Transform Tile(Transform group, StationPlan plan, StationArt art, int f, Vector2Int c)
        {
            Transform tile = Group($"Tile_{f}_{c.x}_{c.y}", group);
            Vector3 top = P.CellCentre(c, f);
            tile.position = top;
            P.Room room = plan.RoomAt(f, c);
            Material floor = room != null ? art.FloorFor(room.Finish, f) : f < 0 ? art.FloorConcrete : f == 0 ? art.Terrazzo : art.Vinyl;
            Transform visual = Group("Visual", tile);
            if (plan.Pierced.TryGetValue(P.Key(f, c), out float hole))
            {
                // Four strips round a square opening, each its own solid.
                float side = (P.Tile - hole) * .5f, o = (hole + side) * .5f;
                foreach ((Vector3 at, Vector3 size) in new[]
                {
                    (new Vector3(-o, 0f, 0f), new Vector3(side, P.Slab, P.Tile)), (new Vector3(o, 0f, 0f), new Vector3(side, P.Slab, P.Tile)),
                    (new Vector3(0f, 0f, -o), new Vector3(hole, P.Slab, side)), (new Vector3(0f, 0f, o), new Vector3(hole, P.Slab, side)),
                })
                {
                    Vector3 centre = at + Vector3.down * (P.Slab * .5f);
                    Box("Slab", visual, centre, size, floor, false);
                    var strip = tile.gameObject.AddComponent<BoxCollider>();
                    strip.center = centre;
                    strip.size = size;
                }
            }
            else
            {
                var collider = tile.gameObject.AddComponent<BoxCollider>();
                collider.center = Vector3.down * (P.Slab * .5f);
                collider.size = new Vector3(P.Tile, P.Slab, P.Tile);
                Box("Slab", visual, collider.center, collider.size, floor, false);
                if (plan.HasFloor(f - 1, c) || plan.IsHole(f - 1, c))
                    Box("Ceiling", visual, Vector3.down * (P.Slab + .012f), new Vector3(P.Tile - .01f, .02f, P.Tile - .01f), art.Ceiling, false);
            }
            tile.gameObject.AddComponent<SurfaceTag>().EditorSet(Surface(floor, art));
            return tile;
        }

        private static SurfaceMaterial Surface(Material floor, StationArt art) =>
            floor == art.Carpet ? SurfaceMaterial.Carpet : floor == art.Vinyl ? SurfaceMaterial.Tile :
            floor == art.Terrazzo ? SurfaceMaterial.Terrazzo : SurfaceMaterial.Concrete;

        public static Transform Stair(Transform parent, P.Flight flight, StationArt art)
        {
            float length = P.Flight.Length, width = P.Flight.Width, rise = flight.Rise;
            Transform root = Group(flight.Name, parent);
            root.SetPositionAndRotation(flight.Foot, Quaternion.LookRotation(flight.Direction, Vector3.up));
            float angle = Mathf.Atan2(rise, length) * Mathf.Rad2Deg;
            float slope = Mathf.Sqrt(length * length + rise * rise);
            Transform ramp = Group("Ramp", root);
            ramp.localPosition = new Vector3(0f, rise * .5f, length * .5f);
            ramp.localRotation = Quaternion.Euler(-angle, 0f, 0f);
            ramp.gameObject.AddComponent<BoxCollider>().size = new Vector3(width, .25f, slope);

            Transform visual = Group("Visual", root);
            for (int i = 0; i < Treads; i++)
            {
                float t0 = i / (float)Treads, t1 = (i + 1) / (float)Treads;
                // Solid steps, each from the slope's underside up to its own tread: a concrete flight, not a ramp.
                // The tread sits level with the walking surface (the ramp collider) at the step's middle.
                float top = rise * (t0 + t1) * .5f + .07f, bottom = Mathf.Max(0f, rise * t0 - .3f);
                Box($"Step_{i:00}", visual, new Vector3(0f, (top + bottom) * .5f, length * (t0 + t1) * .5f),
                    new Vector3(width, top - bottom, length / Treads), art.Terrazzo, false);
                Box($"Nosing_{i:00}", visual, new Vector3(0f, top - .01f, length * t0 + .04f), new Vector3(width - .1f, .025f, .06f), art.DarkSteel, false);
            }
            foreach (float side in new[] { -1f, 1f })
            {
                GameObject stringer = Box(side < 0 ? "Stringer_L" : "Stringer_R", visual,
                    new Vector3(side * (width * .5f + .06f), rise * .5f, length * .5f), new Vector3(.12f, .45f, slope), art.Concrete, false);
                stringer.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
                GameObject rail = Box(side < 0 ? "Handrail_L" : "Handrail_R", visual,
                    new Vector3(side * (width * .5f + .08f), rise * .5f + .95f, length * .5f), new Vector3(.06f, .06f, slope), art.Safety, false);
                rail.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
                for (int p = 0; p <= 4; p++)
                {
                    float t = p / 4f;
                    Box("Baluster", visual, new Vector3(side * (width * .5f + .08f), rise * t + .48f, length * t), new Vector3(.04f, .96f, .04f), art.Steel, false);
                }
                // The guard along the open side: nothing slips off the flight.
                var guard = Group(side < 0 ? "Guard_L" : "Guard_R", root);
                guard.localPosition = new Vector3(side * (width * .5f + .08f), rise * .5f + .55f, length * .5f);
                guard.localRotation = Quaternion.Euler(-angle, 0f, 0f);
                guard.gameObject.AddComponent<BoxCollider>().size = new Vector3(.08f, 1.1f, slope);
            }
            // Ties both landings together for monsters; the bake can leave a hairline gap at a flight's ends.
            var link = root.gameObject.AddComponent<NavMeshLink>();
            link.startPoint = Vector3.back * .75f;
            link.endPoint = Vector3.forward * (length + .75f) + Vector3.up * rise;
            link.width = width - .35f;
            link.bidirectional = true;
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            return root;
        }

        // Stepped solid under the flight, just below its slope: no headroom trap, no loot rolling under.
        public static void UnderFill(Transform parent, P.Flight flight, StationArt art)
        {
            Transform fill = Group(flight.Name + "_Under", parent);
            fill.SetPositionAndRotation(flight.Foot, Quaternion.LookRotation(flight.Direction, Vector3.up));
            const int blocks = 4;
            float step = P.Flight.Length / blocks;
            for (int j = 1; j < blocks; j++)
            {
                float height = flight.Rise * (j * step) / P.Flight.Length - .45f;
                if (height < .2f) continue;
                Box($"Fill_{j}", fill, new Vector3(0f, height * .5f, (j + .5f) * step), new Vector3(P.Flight.Width - .02f, height, step - .01f), art.Plaster);
            }
        }

        /// <summary>Registers the slabs and flights with the structure simulation; returns each slab's section by key.</summary>
        public static Dictionary<Vector3Int, StructuralSection> AddSections(StructureSceneSetup setup, StationPlan plan, Result built)
        {
            var sections = new Dictionary<Vector3Int, StructuralSection>();
            foreach (KeyValuePair<Vector3Int, Transform> pair in built.Tiles)
            {
                bool falls = plan.Collapsible.TryGetValue(pair.Key, out Vector2 strength);
                bool pierced = plan.Pierced.ContainsKey(pair.Key);
                StructuralSection section = setup.AddSection(pair.Value, SectionType.Floor, falls && !pierced,
                    falls ? strength.x : 1f, falls ? strength.y : 1f, fractured: !pierced);
                if (falls && !pierced) SectionNavCarver.EditorAdd(pair.Value.gameObject, new Vector3(P.Tile, P.Slab, P.Tile));
                if (plan.KeepClear.Contains(pair.Key)) pair.Value.gameObject.AddComponent<SectionKeepClear>();
                sections[pair.Key] = section;
            }
            foreach (Transform stair in built.Stairs) setup.AddSection(stair, SectionType.Stair, false, fractured: false);
            return sections;
        }
    }
}
