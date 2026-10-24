using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The drivable surface (Docs/CITY_PLAN.md step 3). Every centreline from <see cref="CityRoadNetwork"/> is
    /// stamped into one grid of squares, and only then turned into geometry, so two roads crossing share squares
    /// instead of laying one strip over another: junctions come out solid and no two faces ever end up in the same
    /// plane, which is what CLAUDE.md's coplanar rule (0.24.13) forbids.
    ///
    /// Where a road's squares fall over the river it becomes a bridge: a deck with its own collider, standing on the
    /// piers step 2 left there. Everywhere else the ground underneath is already solid, so the road is only a
    /// surface and needs no collider of its own.
    /// </summary>
    public static class CityRoads
    {
        /// <summary>How finely a road is stamped. Four metres is a lane; a street is two squares, an arterial four.</summary>
        public const float SquareMetres = 4f;
        /// <summary>The surface sits this far above the ground, far enough that neither can flicker through.</summary>
        public const float RoadY = CityPlan.GroundY + .05f;
        private const float DeckThickness = .5f;

        private static Rect extent;
        private static int wide, tall;

        /// <summary>Builds the roads under <paramref name="parent"/>. Returns how many cells and bridge slabs.</summary>
        public static (int cells, int bridges) Build(Transform parent, Material asphalt, Material deck)
        {
            Rect b = CityPlan.Bounds;
            // Snapped to whole cells, so a road square and a ground cell always belong to the same cell index:
            // the streamer switches a cell's road and its ground together or it tears.
            extent = Rect.MinMaxRect(
                Mathf.Floor((b.xMin - CityGround.Outskirts) / CityPlan.Cell) * CityPlan.Cell,
                Mathf.Floor((b.yMin - CityGround.Outskirts) / CityPlan.Cell) * CityPlan.Cell,
                Mathf.Ceil((b.xMax + CityGround.Outskirts) / CityPlan.Cell) * CityPlan.Cell,
                Mathf.Ceil((b.yMax + CityGround.Outskirts) / CityPlan.Cell) * CityPlan.Cell);
            wide = Mathf.CeilToInt(extent.width / SquareMetres);
            tall = Mathf.CeilToInt(extent.height / SquareMetres);

            var road = new bool[wide, tall];
            var bridge = new bool[wide, tall];
            foreach (CityRoadNetwork.Line line in CityRoadNetwork.All()) Stamp(line, road, bridge);

            Transform group = GreyboxFactory.Group("Roads", parent);
            int cells = 0, bridges = 0;
            int step = Mathf.RoundToInt(CityPlan.Cell / SquareMetres);
            for (int cz = 0; cz < tall; cz += step)
                for (int cx = 0; cx < wide; cx += step)
                {
                    (bool any, int slabs) = Cell(group, road, bridge, cx, cz, step, asphalt, deck);
                    if (any) cells++;
                    bridges += slabs;
                }
            return (cells, bridges);
        }

        // Walks a centreline and marks every square it covers. The sea is never stamped, so no road runs out over
        // the water; the river is, which is what makes a bridge where a road crosses it.
        private static void Stamp(CityRoadNetwork.Line line, bool[,] road, bool[,] bridge)
        {
            int reach = Mathf.CeilToInt(line.HalfWidth / SquareMetres);
            float riverHalf = CityPlan.RiverWidth * .5f;
            for (int i = 0; i < line.Points.Length - 1; i++)
            {
                float len = Vector2.Distance(line.Points[i], line.Points[i + 1]);
                for (float t = 0f; t <= len; t += SquareMetres * .5f)
                {
                    Vector2 on = Vector2.Lerp(line.Points[i], line.Points[i + 1], t / Mathf.Max(len, .001f));
                    Vector2Int centre = Square(on);
                    for (int dz = -reach; dz <= reach; dz++)
                        for (int dx = -reach; dx <= reach; dx++)
                        {
                            int sx = centre.x + dx, sz = centre.y + dz;
                            if (sx < 0 || sz < 0 || sx >= wide || sz >= tall) continue;
                            Vector2 at = Centre(sx, sz);
                            if (Vector2.Distance(at, on) > line.HalfWidth) continue;
                            if (CityPlan.AtSea(at)) continue;
                            road[sx, sz] = true;
                            if (CityPlan.FromRiver(at) < riverHalf) bridge[sx, sz] = true;
                        }
                }
            }
        }

        // One cell's worth of surface: the road squares as one merged mesh, and the squares over the river as
        // decks with colliders, since there is no ground under those to stand on.
        private static (bool any, int slabs) Cell(Transform group, bool[,] road, bool[,] bridge,
                                                  int cx, int cz, int step, Material asphalt, Material deckMaterial)
        {
            var onLand = new bool[step, step];
            var overWater = new bool[step, step];
            bool anyLand = false, anyWater = false;
            for (int z = 0; z < step; z++)
                for (int x = 0; x < step; x++)
                {
                    int sx = cx + x, sz = cz + z;
                    if (sx >= wide || sz >= tall || !road[sx, sz]) continue;
                    if (bridge[sx, sz]) { overWater[x, z] = true; anyWater = true; }
                    else { onLand[x, z] = true; anyLand = true; }
                }
            if (!anyLand && !anyWater) return (false, 0);

            int gx = Mathf.FloorToInt(extent.xMin / CityPlan.Cell) + cx / step;
            int gz = Mathf.FloorToInt(extent.yMin / CityPlan.Cell) + cz / step;
            Transform cell = GreyboxFactory.Group($"Road_{gx}_{gz}", group);
            CityCells.Register(new Vector2Int(gx, gz), cell);
            if (anyLand) Surface(cell, onLand, cx, cz, $"Road_{gx}_{gz}", asphalt);
            int slabs = 0;
            if (anyWater) slabs = Bridge(cell, overWater, cx, cz, $"Bridge_{gx}_{gz}", deckMaterial);
            return (true, slabs);
        }

        private static void Surface(Transform cell, bool[,] mask, int cx, int cz, string name, Material material)
        {
            var piece = new CityMesh.Piece();
            foreach (RectInt r in CityMesh.Rectangles(mask)) Flat(piece, r, cx, cz, RoadY);
            Mesh mesh = piece.Save(name);
            if (mesh != null) CityMesh.Render("Surface", cell, mesh, material);
        }

        // A bridge deck: the same merged rectangles, plus a box under each one for the van to drive on.
        private static int Bridge(Transform cell, bool[,] mask, int cx, int cz, string name, Material material)
        {
            var piece = new CityMesh.Piece();
            Transform collision = GreyboxFactory.Group("Collision", cell);
            List<RectInt> rects = CityMesh.Rectangles(mask);
            foreach (RectInt r in rects)
            {
                Flat(piece, r, cx, cz, RoadY);
                float x0 = extent.xMin + (cx + r.x) * SquareMetres, z0 = extent.yMin + (cz + r.y) * SquareMetres;
                float sizeX = r.width * SquareMetres, sizeZ = r.height * SquareMetres;
                var go = new GameObject($"Deck_{r.x}_{r.y}");
                go.transform.SetParent(collision, false);
                BoxCollider box = go.AddComponent<BoxCollider>();
                box.center = new Vector3(x0 + sizeX * .5f, RoadY - DeckThickness * .5f, z0 + sizeZ * .5f);
                box.size = new Vector3(sizeX, DeckThickness, sizeZ);
            }
            Mesh mesh = piece.Save(name);
            if (mesh != null) CityMesh.Render("Deck", cell, mesh, material);
            return rects.Count;
        }

        private static void Flat(CityMesh.Piece piece, RectInt r, int cx, int cz, float y)
        {
            float x0 = extent.xMin + (cx + r.x) * SquareMetres, z0 = extent.yMin + (cz + r.y) * SquareMetres;
            piece.Flat(x0, z0, x0 + r.width * SquareMetres, z0 + r.height * SquareMetres, y);
        }

        private static Vector2Int Square(Vector2 world) => new(
            Mathf.FloorToInt((world.x - extent.xMin) / SquareMetres),
            Mathf.FloorToInt((world.y - extent.yMin) / SquareMetres));

        private static Vector2 Centre(int sx, int sz) => new(
            extent.xMin + (sx + .5f) * SquareMetres, extent.yMin + (sz + .5f) * SquareMetres);
    }
}
