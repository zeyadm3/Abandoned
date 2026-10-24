using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The land the city stands on, cell by cell (Docs/CITY_PLAN.md step 2): paving inside the city band, open
    /// country in the outskirts round it, park ground where the drawing marks it green, and nothing at all where the
    /// sea or the river is. One mesh per material per cell, and the flat ground carries box colliders rather than a
    /// mesh collider, so the van and the crew have something cheap to stand on.
    /// </summary>
    public static class CityGround
    {
        /// <summary>How fine the coast and the riverbank are cut. A cell is <see cref="Steps"/> squares across.</summary>
        public const float SquareMetres = 16f;
        public static readonly int Steps = Mathf.RoundToInt(CityPlan.Cell / SquareMetres);

        /// <summary>Ground reaches this far past the city band, so the roads out have somewhere to go.</summary>
        public const float Outskirts = 1024f;

        private const int Paving = 0, ParkGrass = 1, Wild = 2;

        /// <summary>Builds every ground cell under <paramref name="parent"/>. Returns how many carried ground.</summary>
        public static int Build(Transform parent, Material paving, Material park, Material grass)
        {
            Rect b = CityPlan.Bounds;
            Rect extent = Rect.MinMaxRect(b.xMin - Outskirts, b.yMin - Outskirts, b.xMax + Outskirts, b.yMax + Outskirts);
            Transform group = GreyboxFactory.Group("Ground", parent);
            var materials = new[] { paving, park, grass };
            int x0 = Mathf.FloorToInt(extent.xMin / CityPlan.Cell), x1 = Mathf.CeilToInt(extent.xMax / CityPlan.Cell);
            int z0 = Mathf.FloorToInt(extent.yMin / CityPlan.Cell), z1 = Mathf.CeilToInt(extent.yMax / CityPlan.Cell);
            int built = 0;
            for (int cz = z0; cz < z1; cz++)
                for (int cx = x0; cx < x1; cx++)
                    if (Cell(group, cx, cz, materials)) built++;
            return built;
        }

        // One cell. A cell that is all one surface is a single quad; only the ones the water cuts into are
        // described square by square, which is what keeps the ground at a few thousand triangles in all.
        private static bool Cell(Transform group, int cx, int cz, Material[] materials)
        {
            float ox = cx * CityPlan.Cell, oz = cz * CityPlan.Cell;
            var solid = new bool[Steps, Steps];
            var kind = new int[Steps, Steps];
            int filled = 0, first = -1;
            bool mixed = false;
            for (int sz = 0; sz < Steps; sz++)
                for (int sx = 0; sx < Steps; sx++)
                {
                    var at = new Vector2(ox + (sx + .5f) * SquareMetres, oz + (sz + .5f) * SquareMetres);
                    if (CityPlan.AtSea(at) || CityPlan.FromRiver(at) < CityPlan.RiverWidth * .5f) continue;
                    solid[sx, sz] = true;
                    kind[sx, sz] = Surface(at);
                    filled++;
                    if (first < 0) first = kind[sx, sz];
                    else if (kind[sx, sz] != first) mixed = true;
                }
            if (filled == 0) return false;

            var pieces = new CityMesh.Piece[materials.Length];
            for (int i = 0; i < pieces.Length; i++) pieces[i] = new CityMesh.Piece();
            if (filled == Steps * Steps && !mixed)
                pieces[first].Flat(ox, oz, ox + CityPlan.Cell, oz + CityPlan.Cell, CityPlan.GroundY);
            else
                for (int sz = 0; sz < Steps; sz++)
                    for (int sx = 0; sx < Steps; sx++)
                    {
                        if (!solid[sx, sz]) continue;
                        float x = ox + sx * SquareMetres, z = oz + sz * SquareMetres;
                        pieces[kind[sx, sz]].Flat(x, z, x + SquareMetres, z + SquareMetres, CityPlan.GroundY);
                    }

            Transform cell = GreyboxFactory.Group($"Cell_{cx}_{cz}", group);
            string[] names = { "Paving", "Park", "Wild" };
            for (int i = 0; i < pieces.Length; i++)
            {
                Mesh mesh = pieces[i].Save($"Ground_{names[i]}_{cx}_{cz}");
                if (mesh != null) CityMesh.Render(names[i], cell, mesh, materials[i]);
            }
            Collide(cell, ox, oz, solid);
            return true;
        }

        // Box colliders over the solid squares, run together into as few as possible. The ground is flat, so one
        // slab half a metre thick under each rectangle is all the van and the crew ever touch.
        private static void Collide(Transform cell, float ox, float oz, bool[,] solid)
        {
            Transform group = GreyboxFactory.Group("Collision", cell);
            List<RectInt> rects = CityMesh.Rectangles(solid);
            foreach (RectInt r in rects)
            {
                var go = new GameObject($"Slab_{r.x}_{r.y}");
                go.transform.SetParent(group, false);
                BoxCollider box = go.AddComponent<BoxCollider>();
                box.center = new Vector3(
                    ox + (r.x + r.width * .5f) * SquareMetres,
                    CityPlan.GroundY - .25f,
                    oz + (r.y + r.height * .5f) * SquareMetres);
                box.size = new Vector3(r.width * SquareMetres, .5f, r.height * SquareMetres);
            }
        }

        // What a square is surfaced with. Parks beat the band, and anything outside the band is open country.
        private static int Surface(Vector2 at)
        {
            foreach (Rect p in CityPlan.Parks)
                if (p.Contains(at)) return ParkGrass;
            if (!CityPlan.Bounds.Contains(at)) return Wild;
            return CityPlan.At(at).HasValue ? Paving : ParkGrass;
        }
    }
}
