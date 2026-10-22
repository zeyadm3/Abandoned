using System;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A station's hiding places, all run by <see cref="HidingLocker"/> (the mall's rules; only the look and the prompt
    /// differ): steel lockers, wide cabinets, closets with a sliding panel and barred holding cells. Each is built in
    /// its own space: origin on the floor at the back, +z out through the door. One solid box the occupant stands
    /// inside, a carved NavMesh hole, the point they step in to and the point they step out to (where a monster reaches
    /// in from). A spot on a slab that can fall goes with it. Spots go through <see cref="StationProps.Place"/>, so a
    /// spot that would block a doorway or flight is left out.
    /// </summary>
    public sealed class StationHiding
    {
        public const int Max = 127;
        private readonly StationProps props;
        private readonly Func<Vector3, StructuralSection> support;
        public int Count { get; private set; }
        public int Lockers, Cabinets, Closets, Cells;

        public StationHiding(StationProps props, Func<Vector3, StructuralSection> support)
        {
            this.props = props;
            this.support = support;
        }

        private const float Front = 1.1f;

        /// <summary>A locker backed onto one of a room's walls, <paramref name="along"/> metres from its west or south end.</summary>
        public Transform Locker(StationPlan.Room room, StationPlan.Side wall, float along) =>
            Locker(room.Name, StationProps.Against(room, wall, along, .6f + Front, out float yaw), yaw);
        public Transform Cabinet(StationPlan.Room room, StationPlan.Side wall, float along) =>
            Cabinet(room.Name, StationProps.Against(room, wall, along, .72f + Front, out float yaw), yaw);
        public Transform Closet(StationPlan.Room room, StationPlan.Side wall, float along) =>
            Closet(room.Name, StationProps.Against(room, wall, along, .85f + Front, out float yaw), yaw);
        public Transform Cell(StationPlan.Room room, StationPlan.Side wall, float along, float width = 2.6f, float depth = 2.6f) =>
            Cell(room.Name, StationProps.Against(room, wall, along, depth + Front, out float yaw, .02f), yaw, width, depth);

        /// <summary><paramref name="at"/> is the middle of the spot's footprint, its front clearance included (see Against).</summary>
        public Transform Locker(string room, Vector3 at, float yaw) => Spot(room, HideSpotKind.Locker, at, yaw, new Vector3(.62f, 2f, .6f));
        public Transform Cabinet(string room, Vector3 at, float yaw) => Spot(room, HideSpotKind.Cabinet, at, yaw, new Vector3(1.1f, 2f, .72f));
        public Transform Closet(string room, Vector3 at, float yaw) => Spot(room, HideSpotKind.Closet, at, yaw, new Vector3(1.2f, 2.3f, .85f));

        private Transform Spot(string room, HideSpotKind kind, Vector3 at, float yaw, Vector3 size)
        {
            if (Count >= Max) return null;
            // The front must stay clear too: that is where the occupant steps out and a monster reaches in.
            Transform root = props.Place(room, $"{kind}_{Count:00}", at, yaw, size + new Vector3(0f, 0f, Front), solid: false);
            if (root == null) return null;
            // Place() centres on the footprint; the spot's own origin is its back wall.
            Transform spot = Group("Spot", root);
            spot.localPosition = new Vector3(0f, 0f, -(size.z + Front) * .5f);
            StationArt a = props.Art;
            float w = size.x, h = size.y, d = size.z;
            const float sheet = .03f;
            Material shell = kind == HideSpotKind.Closet ? a.Plaster : kind == HideSpotKind.Cabinet ? a.DarkSteel : a.AccentDark;
            Transform body = Group("Shell", spot);
            Box("Back", body, new Vector3(0f, h * .5f, sheet * .5f), new Vector3(w, h, sheet), shell, false);
            foreach (float side in new[] { -1f, 1f })
                Box("Side", body, new Vector3(side * (w - sheet) * .5f, h * .5f, d * .5f), new Vector3(sheet, h, d), shell, false);
            Box("Top", body, new Vector3(0f, h - sheet * .5f, d * .5f), new Vector3(w, sheet, d), shell, false);
            Transform door = kind == HideSpotKind.Closet ? SlidingPanel(spot, w, h, d, a) : HingedDoor(spot, w, h, d, kind == HideSpotKind.Cabinet ? a.Steel : a.AccentDark, a);
            Wire(spot, door, size, kind, at);
            switch (kind)
            {
                case HideSpotKind.Locker: Lockers++; break;
                case HideSpotKind.Cabinet: Cabinets++; break;
                default: Closets++; break;
            }
            return spot;
        }

        /// <summary>
        /// A holding cell: block walls each side and behind, a barred front with a sliding barred door, a steel bunk and
        /// a steel toilet inside. <paramref name="at"/> is the middle of its footprint, front clearance included.
        /// </summary>
        public Transform Cell(string room, Vector3 at, float yaw, float width = 2.6f, float depth = 2.6f, float height = 2.7f)
        {
            if (Count >= Max) return null;
            var size = new Vector3(width, height, depth);
            Transform root = props.Place(room, $"Cell_{Count:00}", at, yaw, size + new Vector3(0f, 0f, Front), solid: false);
            if (root == null) return null;
            Transform spot = Group("Spot", root);
            spot.localPosition = new Vector3(0f, 0f, -(depth + Front) * .5f);
            StationArt a = props.Art;
            Transform body = Group("Shell", spot);
            Box("Back", body, new Vector3(0f, height * .5f, .1f), new Vector3(width, height, .2f), a.Block, false);
            foreach (float side in new[] { -1f, 1f })
                Box("Side", body, new Vector3(side * (width * .5f - .1f), height * .5f, depth * .5f), new Vector3(.2f, height, depth), a.Block, false);
            Box("Top", body, new Vector3(0f, height - .04f, depth * .5f), new Vector3(width, .08f, depth), a.Bars, false);
            Box("Bunk", body, new Vector3(-width * .5f + .55f, .45f, depth * .5f + .1f), new Vector3(.7f, .06f, 1.9f), a.Steel, false);
            Box("BunkPad", body, new Vector3(-width * .5f + .55f, .52f, depth * .5f + .1f), new Vector3(.62f, .07f, 1.8f), a.Mattress, false);
            Box("Toilet", body, new Vector3(width * .5f - .45f, .25f, .45f), new Vector3(.42f, .5f, .5f), a.Chrome, false);
            // The fixed half of the front, then the sliding half (the door HidingLocker moves).
            Bars(body, new Vector3(width * .25f, 0f, depth), width * .5f - .02f, height - .08f, a);
            Transform door = Group("Door", spot);
            door.localPosition = new Vector3(-width * .25f, 0f, depth - .06f);
            Bars(door, Vector3.zero, width * .5f - .02f, height - .08f, a);
            Box("Lock", door, new Vector3(width * .25f - .08f, 1.05f, .04f), new Vector3(.12f, .18f, .08f), a.DarkSteel, false);
            Wire(spot, door, size, HideSpotKind.Cell, at);
            Cells++;
            return spot;
        }

        private static void Bars(Transform parent, Vector3 origin, float width, float height, StationArt a)
        {
            for (float x = -width * .5f + .06f; x <= width * .5f - .05f; x += .13f)
                Box("Bar", parent, origin + new Vector3(x, height * .5f, 0f), new Vector3(.03f, height, .03f), a.Bars, false);
            foreach (float y in new[] { .12f, 1.1f, height - .1f })
                Box("Rail", parent, origin + new Vector3(0f, y, 0f), new Vector3(width, .05f, .05f), a.Bars, false);
        }

        // Hinged on its left edge at the front, slats at eye height to look out between.
        private static Transform HingedDoor(Transform spot, float w, float h, float d, Material m, StationArt a)
        {
            Transform door = Group("Door", spot);
            door.localPosition = new Vector3(-w * .5f, 0f, d);
            const float from = 1.4f, to = 1.8f;
            Box("Lower", door, new Vector3(w * .5f, from * .5f, 0f), new Vector3(w - .02f, from, .025f), m, false);
            Box("Upper", door, new Vector3(w * .5f, (to + h) * .5f, 0f), new Vector3(w - .02f, h - to - .02f, .025f), m, false);
            for (int i = 0; i < 5; i++)
            {
                GameObject slat = Box("Slat", door, new Vector3(w * .5f, from + .04f + i * .075f, 0f), new Vector3(w - .12f, .018f, .035f), m, false);
                slat.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
            }
            Box("Handle", door, new Vector3(w - .1f, 1.05f, .03f), new Vector3(.03f, .16f, .04f), a.Chrome, false);
            return door;
        }

        // A panel that slides aside along the front, with a louvre to look out through.
        private static Transform SlidingPanel(Transform spot, float w, float h, float d, StationArt a)
        {
            Transform door = Group("Door", spot);
            door.localPosition = new Vector3(0f, 0f, d - .03f);
            Box("Panel", door, new Vector3(0f, h * .5f - .02f, 0f), new Vector3(w - .06f, h - .06f, .03f), a.Panel, false);
            Box("Louvre", door, new Vector3(0f, 1.62f, .02f), new Vector3(w - .4f, .3f, .01f), a.DarkWood, false);
            Box("Pull", door, new Vector3(w * .5f - .12f, 1.05f, .025f), new Vector3(.03f, .2f, .03f), a.Chrome, false);
            return door;
        }

        private void Wire(Transform spot, Transform door, Vector3 size, HideSpotKind kind, Vector3 at)
        {
            var solid = spot.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, size.y * .5f, size.z * .5f);
            solid.size = size;
            var carve = spot.gameObject.AddComponent<NavMeshObstacle>();
            carve.shape = NavMeshObstacleShape.Box;
            carve.center = solid.center;
            carve.size = solid.size;
            carve.carving = true;
            Transform inside = Group("Inside", spot);
            inside.localPosition = new Vector3(0f, .02f, Mathf.Min(size.z * .5f, kind == HideSpotKind.Cell ? 1.3f : .55f));
            Transform exit = Group("Exit", spot);
            exit.localPosition = new Vector3(0f, .05f, size.z + .6f);
            spot.gameObject.AddComponent<SurfaceTag>().EditorSet(kind == HideSpotKind.Closet ? SurfaceMaterial.Wood : SurfaceMaterial.Metal);
            spot.gameObject.AddComponent<HidingLocker>().EditorSetup(Count++, door, inside, exit, support?.Invoke(at), kind);
        }
    }
}
