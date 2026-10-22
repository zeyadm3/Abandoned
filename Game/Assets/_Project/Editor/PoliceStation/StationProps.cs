using System.Collections.Generic;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Places a station's fittings. Every piece is a root at floor level (local +z is its front, away from the wall it
    /// backs onto) with one box collider fitted to its footprint, the mall's rule for solid furniture, and visual parts
    /// without colliders. Before a piece goes in, its footprint is checked against what must stay clear: doorways and
    /// the space either side of them, flights and their landings, facade openings, and every piece already placed. A
    /// piece that would block any of them is left out and reported, never forced in.
    /// </summary>
    public sealed class StationProps
    {
        public readonly Transform Root;
        public readonly StationArt Art;
        public readonly StationPlan Plan;
        public readonly List<string> Problems = new();
        public int Placed { get; private set; }
        private readonly List<Bounds> keepOut = new();
        private readonly List<Bounds> occupied = new();
        private readonly Dictionary<string, Transform> groups = new();

        public StationProps(Transform root, StationPlan plan, StationArt art)
        {
            Root = Group("Interiors", root);
            Plan = plan;
            Art = art;
            foreach (P.Doorway d in plan.Doors) KeepOut(DoorApron(d));
            foreach (P.Flight f in plan.Stairs) foreach (Bounds b in FlightBounds(f)) KeepOut(b);
            foreach (P.Opening o in plan.Facade) KeepOut(FacadeApron(plan, o));
            foreach (KeyValuePair<Vector3Int, float> pierced in plan.Pierced)
            {
                Vector3 c = P.CellCentre(new Vector2Int(pierced.Key.x, pierced.Key.z), pierced.Key.y);
                KeepOut(new Bounds(c + Vector3.up * 1f, new Vector3(pierced.Value + 1.6f, 2.4f, pierced.Value + 1.6f)));
                KeepOut(new Bounds(c + Vector3.down * 3f, new Vector3(pierced.Value + 1.6f, 2.4f, pierced.Value + 1.6f)));
            }
        }

        public void KeepOut(Bounds b) => keepOut.Add(b);

        /// <summary>A rectangle of floor (world metres) kept clear: a haul lane, a hiding place's front, a loot site.</summary>
        public void KeepOut(int floor, Rect r, float height = 2.4f) =>
            keepOut.Add(new Bounds(new Vector3(r.center.x, P.FloorY(floor) + height * .5f, r.center.y), new Vector3(r.width, height, r.height)));

        public static Bounds DoorApron(P.Doorway d)
        {
            Vector3 a = P.CellCentre(d.A, d.Floor), b = P.CellCentre(d.B, d.Floor);
            Vector3 mid = (a + b) * .5f + Vector3.up * 1.2f;
            bool alongX = d.A.y != d.B.y; // the wall runs along x
            const float reach = 2.6f;
            return new Bounds(mid, alongX ? new Vector3(d.Width + .4f, 2.4f, reach) : new Vector3(reach, 2.4f, d.Width + .4f));
        }

        /// <summary>A flight and the landing at each end, each at its own level: nothing stands on or beside them.</summary>
        public static IEnumerable<Bounds> FlightBounds(P.Flight f)
        {
            Vector3 across = Vector3.Cross(Vector3.up, f.Direction) * (P.Flight.Width * .5f + .3f);
            Bounds Span(Vector3 a, Vector3 b, float low, float high)
            {
                var box = new Bounds(new Vector3(a.x, low, a.z), Vector3.zero);
                foreach (Vector3 p in new[] { a + across, a - across, b + across, b - across })
                {
                    box.Encapsulate(new Vector3(p.x, low, p.z));
                    box.Encapsulate(new Vector3(p.x, high, p.z));
                }
                return box;
            }
            Vector3 foot = f.Foot, end = f.Foot + f.Direction * P.Flight.Length;
            yield return Span(foot - f.Direction * 1.6f, foot, f.Foot.y, f.Foot.y + 2.3f);
            yield return Span(foot, end, f.Foot.y, f.Top.y + 2.3f);
            yield return Span(end, end + f.Direction * 1.6f, f.Top.y, f.Top.y + 2.3f);
        }

        private static Bounds FacadeApron(StationPlan plan, P.Opening o)
        {
            float w = plan.Width * P.Tile, d = plan.Depth * P.Tile, h = Mathf.Min(o.Top - o.Bottom, 3f);
            Vector3 size = o.Side is P.Side.South or P.Side.North ? new Vector3(o.Width + .6f, h, 6f) : new Vector3(6f, h, o.Width + .6f);
            Vector3 c = o.Side switch
            {
                P.Side.South => new Vector3(o.Centre, o.Bottom + h * .5f, 0f),
                P.Side.North => new Vector3(o.Centre, o.Bottom + h * .5f, d),
                P.Side.West => new Vector3(0f, o.Bottom + h * .5f, o.Centre),
                _ => new Vector3(w, o.Bottom + h * .5f, o.Centre),
            };
            return new Bounds(c, size);
        }

        /// <summary>
        /// Starts a piece of <paramref name="size"/> (width, height, depth in its own frame) standing at
        /// <paramref name="at"/>, turned <paramref name="yaw"/> degrees. Returns its root, or null if it would block
        /// something (then it is reported).
        /// </summary>
        public Transform Place(string room, string name, Vector3 at, float yaw, Vector3 size, bool solid = true)
        {
            bool turned = Mathf.Abs(Mathf.DeltaAngle(yaw, 90f)) < 45f || Mathf.Abs(Mathf.DeltaAngle(yaw, 270f)) < 45f;
            Vector3 world = turned ? new Vector3(size.z, size.y, size.x) : size;
            var footprint = new Bounds(at + Vector3.up * (size.y * .5f), world);
            var test = new Bounds(footprint.center, footprint.size - new Vector3(.06f, .06f, .06f));
            foreach (Bounds k in keepOut)
                if (k.Intersects(test)) return Refuse(room, name, at, "a doorway, flight or opening");
            foreach (Bounds o in occupied)
                if (o.Intersects(test)) return Refuse(room, name, at, "another fitting");
            if (!groups.TryGetValue(room, out Transform parent)) groups[room] = parent = Group(room, Root);
            Transform t = Group(name, parent);
            t.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            if (solid && size.y >= .3f)
            {
                var box = t.gameObject.AddComponent<BoxCollider>();
                box.center = Vector3.up * (size.y * .5f);
                box.size = size;
            }
            occupied.Add(footprint);
            Placed++;
            return t;
        }

        /// <summary>A piece hung on a wall or ceiling: nothing to walk into, so only kept off doorways.</summary>
        public Transform Hang(string room, string name, Vector3 at, float yaw)
        {
            if (!groups.TryGetValue(room, out Transform parent)) groups[room] = parent = Group(room, Root);
            Transform t = Group(name, parent);
            t.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Placed++;
            return t;
        }

        private Transform Refuse(string room, string name, Vector3 at, string what)
        {
            Problems.Add($"{room}/{name} at {at}: left out, it would block {what}.");
            return null;
        }

        /// <summary>A visual part in the piece's own frame, without a collider.</summary>
        public static GameObject B(Transform t, string name, Vector3 centre, Vector3 size, Material m) => Box(name, t, centre, size, m, false);

        public static GameObject C(Transform t, string name, Vector3 centre, Vector3 size, Material m)
        {
            GameObject go = Primitive(PrimitiveType.Cylinder, name, t, centre, size, m, false);
            return go;
        }

        /// <summary>
        /// A spot against one of a room's walls: <paramref name="along"/> metres from the wall's west or south end,
        /// the piece's back <paramref name="gap"/> off the wall. Yaw turns the piece's front into the room.
        /// </summary>
        public static Vector3 Against(P.Room room, P.Side wall, float along, float depth, out float yaw, float gap = .14f)
        {
            Rect r = room.Meters;
            float inset = .1f + gap + depth * .5f;
            switch (wall)
            {
                case P.Side.South: yaw = 0f; return new Vector3(r.xMin + along, room.Y, r.yMin + inset);
                case P.Side.North: yaw = 180f; return new Vector3(r.xMin + along, room.Y, r.yMax - inset);
                case P.Side.West: yaw = 90f; return new Vector3(r.xMin + inset, room.Y, r.yMin + along);
                default: yaw = 270f; return new Vector3(r.xMax - inset, room.Y, r.yMin + along);
            }
        }

        /// <summary>A point in a room by fractions of its floor (0-1 each way).</summary>
        public static Vector3 In(P.Room room, float u, float v)
        {
            Rect r = room.Meters;
            return new Vector3(Mathf.Lerp(r.xMin, r.xMax, u), room.Y, Mathf.Lerp(r.yMin, r.yMax, v));
        }
    }
}
