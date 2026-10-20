using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.TutorialHouseLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The subdivision around 14 Larch Court (2026-10-21, the user: "build more homes and roads ... so the player feels
    /// there's a world around them"): a grid of plowed streets lined with copies of the training house (then the
    /// same day: "make the surrounding houses the same as the tutorial house with some variations ... lock everything
    /// so the player can't walk inside, remove the basement and basement stairs"; <see cref="TutorialStreetHouse"/>),
    /// set back on their lots with a drive and a front walk each, out to where the December fog takes them; a
    /// treeline beyond. The crew may walk Larch Court and the blocks round it; a board fence with an invisible wall
    /// behind it closes that area, so nobody walks off the world. Outside the NavMesh: only the lot is the level.
    /// </summary>
    public static class TutorialTown
    {
        /// <summary>North-south streets (centrelines); Larch Court is the one in front of the house.</summary>
        public static readonly float[] RoadsX = { -154f, -44f, L.StreetX + L.StreetWidth * .5f, 176f, 286f };
        /// <summary>East-west streets (centrelines).</summary>
        public static readonly float[] RoadsZ = { -230f, -70f, 90f, 250f };
        public const float RoadWidth = 8f;

        /// <summary>Where the crew may walk: the lot, Larch Court and the streets round its block.</summary>
        public static readonly Rect Playable = Rect.MinMaxRect(-62f, -98f, 196f, 118f);
        // Houses stand out to here; past it the fog has everything anyway.
        private static readonly Rect Built = Rect.MinMaxRect(-178f, -188f, 312f, 208f);
        private static readonly Rect World = Rect.MinMaxRect(-700f, -700f, 800f, 700f);
        private static readonly Vector2 Centre = new(66f, 10f);
        // A copy of the house is 18.7 m along the street (23.7 m with a garage) and 31.5 m deep, stoop included.
        private const float LotPitch = 30f, Setback = 7f;

        public static void Build(Transform parent, TutorialHouseArt art)
        {
            Transform town = Group("Town", parent);
            Land(Group("Land", town), art);
            List<Rect> roads = Roads(Group("Roads", town), art);
            List<Rect> houses = Houses(Group("Houses", town), roads, art);
            Treeline(Group("DistantTreeline", town), houses, art);
            Boundary(Group("Boundary", town), art);
        }

        // Snow-covered ground everywhere but the lot (which has its own, in the NavMesh).
        private static void Land(Transform parent, TutorialHouseArt art)
        {
            List<Rect> pieces = TutorialRects.Subtract(World, new[] { L.Lot });
            for (int i = 0; i < pieces.Count; i++)
            {
                Rect r = pieces[i];
                GameObject g = Box($"Land_{i}", parent, new Vector3(r.center.x, L.Grade - .5f, r.center.y), new Vector3(r.width, 1f, r.height), art.Snow);
                g.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Dirt);
            }
        }

        // Plain asphalt, a hair above the snow (east-west a hair lower again, so crossings don't flicker).
        private static List<Rect> Roads(Transform parent, TutorialHouseArt art)
        {
            var roads = new List<Rect>();
            float z0 = Built.yMin - 80f, z1 = Built.yMax + 80f, x0 = Built.xMin - 80f, x1 = Built.xMax + 80f;
            foreach (float x in RoadsX)
            {
                var r = Rect.MinMaxRect(x - RoadWidth * .5f, z0, x + RoadWidth * .5f, z1);
                roads.Add(r);
                Box("Street_NS", parent, new Vector3(x, L.Grade + .006f, r.center.y), new Vector3(RoadWidth, .012f, r.height), art.Asphalt, false);
            }
            foreach (float z in RoadsZ)
            {
                var r = Rect.MinMaxRect(x0, z - RoadWidth * .5f, x1, z + RoadWidth * .5f);
                roads.Add(r);
                Box("Street_EW", parent, new Vector3(r.center.x, L.Grade + .005f, z), new Vector3(r.width, .012f, RoadWidth), art.Asphalt, false);
            }
            // Larch Court has its sidewalks; the house's drive meets the one on this side.
            float court = L.StreetX + L.StreetWidth * .5f;
            foreach (float side in new[] { -1f, 1f })
                Box("Sidewalk", parent, new Vector3(court + side * (RoadWidth * .5f + .8f), L.Grade + .008f, (z0 + z1) * .5f), new Vector3(1.6f, .012f, z1 - z0), art.Sidewalk, false);
            return roads;
        }

        // A copy of the training house on every lot along every street, facing it, unless it would sit on a road, the
        // training lot, the boundary fence or another house. Solid only where the crew can reach.
        private static List<Rect> Houses(Transform parent, List<Rect> roads, TutorialHouseArt art)
        {
            TutorialStreetHouseKit.Kit kit = TutorialStreetHouseKit.Build(art);
            var random = new System.Random(2412);
            var placed = new List<Rect>();
            var clear = new List<Rect> { Inflate(L.Lot, 1f) };
            foreach (Rect road in roads) clear.Add(Inflate(road, 2f));
            float t = 3f;
            clear.Add(Rect.MinMaxRect(Playable.xMin - t, Playable.yMin - t, Playable.xMax + t, Playable.yMin + t));
            clear.Add(Rect.MinMaxRect(Playable.xMin - t, Playable.yMax - t, Playable.xMax + t, Playable.yMax + t));
            clear.Add(Rect.MinMaxRect(Playable.xMin - t, Playable.yMin - t, Playable.xMin + t, Playable.yMax + t));
            clear.Add(Rect.MinMaxRect(Playable.xMax - t, Playable.yMin - t, Playable.xMax + t, Playable.yMax + t));

            void Try(float roadAt, float along, bool northSouth, float side)
            {
                TutorialStreetHouse.Style style = TutorialStreetHouse.Roll(random, art);
                float setback = Setback + (float)random.NextDouble() * 4f;
                along += ((float)random.NextDouble() - .5f) * 2f;
                TutorialStreetHouse.Placement place = TutorialStreetHouse.Place(style, roadAt + side * RoadWidth * .5f, along, northSouth, side, setback);
                Rect body = place.Footprint;
                if (!Built.Overlaps(body) || Blocked(body, clear, placed)) return;
                placed.Add(body);
                bool solid = TutorialRects.Overlaps(body, Playable, 6f);
                TutorialStreetHouse.Build(parent, kit, style, place, solid, art);
            }

            foreach (float x in RoadsX)
                foreach (float side in new[] { -1f, 1f })
                    for (float z = Built.yMin + LotPitch * .5f; z < Built.yMax; z += LotPitch)
                        Try(x, z, northSouth: true, side);
            foreach (float z in RoadsZ)
                foreach (float side in new[] { -1f, 1f })
                    for (float x = Built.xMin + LotPitch * .5f; x < Built.xMax; x += LotPitch)
                        Try(z, x, northSouth: false, side);
            return placed;
        }

        private static bool Blocked(Rect body, List<Rect> clear, List<Rect> placed)
        {
            foreach (Rect c in clear) if (TutorialRects.Overlaps(body, c)) return true;
            foreach (Rect p in placed) if (TutorialRects.Overlaps(body, p, 3f)) return true;
            return false;
        }

        private static Rect Inflate(Rect r, float by) => Rect.MinMaxRect(r.xMin - by, r.yMin - by, r.xMax + by, r.yMax + by);

        // Woods all round, past the last houses, where the fog turns them to shapes.
        private static void Treeline(Transform parent, List<Rect> houses, TutorialHouseArt art)
        {
            var random = new System.Random(48);
            for (int i = 0; i < 120; i++)
            {
                float a = i * Mathf.PI * 2f / 120f + (float)random.NextDouble() * .03f, radius = 250f + (float)random.NextDouble() * 30f;
                var at = new Vector3(Centre.x + Mathf.Cos(a) * radius, L.Grade, Centre.y + Mathf.Sin(a) * radius);
                bool onHouse = false;
                foreach (Rect h in houses) onHouse |= h.Contains(new Vector2(at.x, at.z));
                if (onHouse) continue;
                if (i % 3 == 0) TutorialNeighbourhood.BareTree(parent, at, i, art, scale: 1.8f, collider: false);
                else TutorialNeighbourhood.Spruce(parent, "FarSpruce", at, 11f + (float)random.NextDouble() * 7f, art, collider: false);
            }
        }

        // The edge of where the crew may go: a tall board fence (a reason you can't go on), and an invisible wall
        // behind it too high to jump.
        private static void Boundary(Transform parent, TutorialHouseArt art)
        {
            Rect p = Playable;
            (string name, Vector3 a, Vector3 b)[] sides =
            {
                ("South", new Vector3(p.xMin, 0f, p.yMin), new Vector3(p.xMax, 0f, p.yMin)),
                ("North", new Vector3(p.xMin, 0f, p.yMax), new Vector3(p.xMax, 0f, p.yMax)),
                ("West", new Vector3(p.xMin, 0f, p.yMin), new Vector3(p.xMin, 0f, p.yMax)),
                ("East", new Vector3(p.xMax, 0f, p.yMin), new Vector3(p.xMax, 0f, p.yMax)),
            };
            foreach ((string name, Vector3 a, Vector3 b) in sides)
            {
                Vector3 mid = (a + b) * .5f;
                bool alongX = Mathf.Abs(b.x - a.x) > 1f;
                float length = Vector3.Distance(a, b) + 1f;
                Box("Fence_" + name, parent, mid + Vector3.up * (L.Grade + 1.1f), alongX ? new Vector3(length, 2.2f, .08f) : new Vector3(.08f, 2.2f, length), art.FenceWood);
                Box("FenceSnow_" + name, parent, mid + Vector3.up * (L.Grade + 2.23f), alongX ? new Vector3(length, .06f, .14f) : new Vector3(.14f, .06f, length), art.Snow, false);
                var wall = new GameObject("Bound_" + name);
                wall.transform.SetParent(parent, false);
                wall.transform.position = mid + Vector3.up * 6f;
                wall.AddComponent<BoxCollider>().size = alongX ? new Vector3(length, 14f, 1f) : new Vector3(1f, 14f, length);
                wall.AddComponent<IntentionalInvisibleCollider>().EditorSetup("Training town bounds: the crew stays on Larch Court and the blocks round it; the fence shows where.");
            }
        }
    }
}
