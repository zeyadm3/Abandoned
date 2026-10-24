using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using Plan = Abandoned.EditorTools.StationTownPlan;
using Site = Abandoned.EditorTools.StationSite;
using Street = Abandoned.EditorTools.StationStreet;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The neighbourhood round Burley under snow (Docs/SCHOOL_REVAMP.md "Neighbourhood and campus, 2026-10-24"), built
    /// with the station town kit and its outer rings. Rose Hill Drive along the front, Concord Avenue south off it,
    /// Henry Avenue up the west side, Madison Avenue down the east, all with sidewalks, plow banks, crossings, lamps and
    /// cars left at the kerb; houses, row houses, corner shops and churches facing them on every block outside the campus;
    /// then two more rings of the same out to x -192..392, z -192..352. Snow is a material swap: the kit's roofs, lots
    /// and roads become the school's snow and snowy asphalt. The campus itself (paths, bus lane, portables, service
    /// yard) is BurleySchoolCampus. Exterior only, off the NavMesh (under CampusSurroundings).
    /// </summary>
    public static class BurleySchoolTown
    {
        /// <summary>The streets' top: the old roads' height, a little over the campus snow (-0.1).</summary>
        public const float Top = .04f;
        public const float West = -70f, East = 270f, South = -70f, North = 230f;

        public static void Build(Transform surroundings, BurleySchoolArt art)
        {
            var a = new StationArt(BurleySchoolArt.Folder, "SchoolTown", new Color(.62f, .38f, .3f), new Color(.12f, .32f, .2f), new Color(1f, .78f, .5f));
            StationTownArt t = StationTownArt.For(a);
            var stock = new StationRingStock(a);
            Dictionary<Material, Material> snow = stock.Remap;
            snow[a.Asphalt] = art.SnowyAsphalt;
            snow[a.Sidewalk] = art.Sidewalk;
            snow[a.Grass] = art.Snow;
            snow[a.Dirt] = art.Snow;
            snow[a.Gravel] = art.Snow;
            snow[t.TarRoof] = art.Snow;
            snow[t.Shingles] = art.Snow;
            var spec = new StationRingSpec
            {
                Inner = Rect.MinMaxRect(West, South, East, North), Road = 10f, Seed = 951, Vacancy = .1f, Lot = art.Snow, Yard = art.Snow,
                LampIntensity = 7f, Winter = true,
            };
            StationRingPools.Suburb(stock, spec, 9500);
            float previous = Site.GroundTop;
            Site.GroundTop = Top;
            try
            {
                Transform ext = Site.Exterior(surroundings);
                Transform marks = Group("TownMarkings", ext), blocks = Group("TownBlocks", ext), street = Group("TownFurniture", ext);
                Streets(surroundings, marks, a);
                Blocks(surroundings, blocks, street, art, a, stock, spec);
                BurleySchoolTownStreet.Build(street, marks, art, a, stock, spec);
                Rings(surroundings, ext, a, stock, spec);
                BurleySchoolCampus.Build(surroundings, ext, art, a, stock, spec.Cars);
                StationSurfaces.Apply(surroundings, a);
                StationRingStock.Repaint(surroundings, snow);
            }
            finally
            {
                Site.GroundTop = previous;
            }
        }

        // Roads first (Rose Hill runs the full width, the avenues stop at it), then sidewalks round them. Nothing overlaps.
        private static void Streets(Transform root, Transform marks, StationArt a)
        {
            Street.Road(root, marks, a, "RoseHillDrive", Rect.MinMaxRect(West, -18.5f, East, -9.5f), true);
            Street.Road(root, marks, a, "ConcordAvenue", Rect.MinMaxRect(21.5f, South, 30.5f, -18.5f), false);
            Street.Road(root, marks, a, "HenryAvenue", Rect.MinMaxRect(-22.5f, -9.5f, -13.5f, North), false);
            Street.Road(root, marks, a, "MadisonAvenue_S", Rect.MinMaxRect(235.5f, South, 244.5f, -18.5f), false);
            Street.Road(root, marks, a, "MadisonAvenue_N", Rect.MinMaxRect(235.5f, -9.5f, 244.5f, North), false);
            // The service drive from Henry to the boiler yard (BurleySchoolCampus carries it on to the staff lot).
            Street.Road(root, marks, a, "ServiceDrive", Rect.MinMaxRect(-13.5f, 100f, 36f, 108f), true);
            // The drop-off loop meets Rose Hill across its whole frontage (the truck stands in it).
            Plan.Ground(root, "LoopMouth", Rect.MinMaxRect(22f, -9.5f, 70f, -7f), a.Asphalt, SurfaceMaterial.Asphalt);
            Plan.Ground(root, "BusLaneMouthS", Rect.MinMaxRect(-13.5f, 18f, -9.5f, 24f), a.Asphalt, SurfaceMaterial.Asphalt);
            Plan.Ground(root, "BusLaneMouthN", Rect.MinMaxRect(-13.5f, 52f, -9.5f, 58f), a.Asphalt, SurfaceMaterial.Asphalt);

            foreach ((float x0, float x1) in new[] { (West, -22.5f), (-13.5f, 22f), (70f, 235.5f), (244.5f, East) })
                Street.Sidewalk(root, marks, a, $"RoseHillN_{x0:0}", Rect.MinMaxRect(x0, -9.5f, x1, -5.5f), true, -1f);
            foreach ((float x0, float x1) in new[] { (West, 21.5f), (30.5f, 235.5f), (244.5f, East) })
                Street.Sidewalk(root, marks, a, $"RoseHillS_{x0:0}", Rect.MinMaxRect(x0, -22.5f, x1, -18.5f), true, 1f);
            Street.Sidewalk(root, marks, a, "ConcordW", Rect.MinMaxRect(17.5f, South, 21.5f, -22.5f), false, 1f);
            Street.Sidewalk(root, marks, a, "ConcordE", Rect.MinMaxRect(30.5f, South, 34.5f, -22.5f), false, -1f);
            Street.Sidewalk(root, marks, a, "HenryW", Rect.MinMaxRect(-26.5f, -5.5f, -22.5f, North), false, 1f);
            foreach ((float z0, float z1) in new[] { (-5.5f, 18f), (24f, 52f), (58f, 100f), (108f, North) })
                Street.Sidewalk(root, marks, a, $"HenryE_{z0:0}", Rect.MinMaxRect(-13.5f, z0, -9.5f, z1), false, -1f);
            foreach ((float z0, float z1) in new[] { (South, -22.5f), (-5.5f, North) })
            {
                Street.Sidewalk(root, marks, a, $"MadisonW_{z0:0}", Rect.MinMaxRect(231.5f, z0, 235.5f, z1), false, 1f);
                Street.Sidewalk(root, marks, a, $"MadisonE_{z0:0}", Rect.MinMaxRect(244.5f, z0, 248.5f, z1), false, -1f);
            }

            // Zebras on every arm of the three junctions on Rose Hill.
            foreach (float x in new[] { -25f, -11f, 19f, 33f, 232f, 248f })
                Street.Crosswalk(marks, a, new Vector3(x, 0f, -14f), true, 9f);
            Street.Crosswalk(marks, a, new Vector3(-18f, 0f, -7f), false, 9f);
            Street.Crosswalk(marks, a, new Vector3(26f, 0f, -21f), false, 9f);
            Street.Crosswalk(marks, a, new Vector3(240f, 0f, -21f), false, 9f);
            Street.Crosswalk(marks, a, new Vector3(240f, 0f, -7f), false, 9f);
        }

        // Every block outside the campus: houses and corner shops facing the streets, a second row backing onto them that
        // faces the first ring road. A snow lot under each block, at the streets' height.
        private static void Blocks(Transform root, Transform blocks, Transform street, BurleySchoolArt art, StationArt a, StationRingStock stock,
            StationRingSpec spec)
        {
            foreach (Rect r in new[]
                     {
                         Rect.MinMaxRect(West, South, 17.5f, -22.5f), Rect.MinMaxRect(34.5f, South, 231.5f, -22.5f), Rect.MinMaxRect(248.5f, South, East, -22.5f),
                         Rect.MinMaxRect(West, -5.5f, -26.5f, North), Rect.MinMaxRect(248.5f, -5.5f, East, North), Rect.MinMaxRect(-9.5f, 108f, 11f, North),
                     })
                Plan.Ground(root, $"TownLot_{r.xMin:0}_{r.yMin:0}", r, art.Snow, SurfaceMaterial.Dirt);
            int seed = 9600;
            // South of Rose Hill: the old craftsman street, facing the school, and the row behind facing out.
            foreach ((float x0, float x1) in new[] { (West, 17.5f), (34.5f, 231.5f), (248.5f, East) })
            {
                StationRings.Frontage(blocks, street, a, stock, spec.Front, new Vector3(x0, 0f, -22.8f), Vector3.right, x1 - x0, 0f, 21f, .08f, seed++, true);
                StationRings.Frontage(blocks, street, a, stock, spec.Back, new Vector3(x0, 0f, South + .3f), Vector3.right, x1 - x0, 180f, 21f, .12f, seed++, true);
            }
            // West of Henry, both sides of the block; east of Madison one row; Henry's east side north of the service drive.
            StationRings.Frontage(blocks, street, a, stock, spec.Front, new Vector3(-26.8f, 0f, -5.5f), Vector3.forward, North + 5.5f, 90f, 19f, .08f, seed++, true);
            StationRings.Frontage(blocks, street, a, stock, spec.Back, new Vector3(West + .3f, 0f, -5.5f), Vector3.forward, North + 5.5f, -90f, 19f, .12f, seed++, true);
            StationRings.Frontage(blocks, street, a, stock, spec.Front, new Vector3(248.8f, 0f, -5.5f), Vector3.forward, North + 5.5f, -90f, 20.9f, .08f, seed++, true);
            StationRings.Frontage(blocks, street, a, stock, spec.Front, new Vector3(-9.2f, 0f, 112f), Vector3.forward, North - 112f, -90f, 19.5f, .1f, seed, true);
        }

        // Rose Hill runs out west and east, Concord and Madison south, Henry and Madison north; a spur on the east, the side
        // the open world's road comes in from, meets its middle.
        private static void Rings(Transform root, Transform ext, StationArt a, StationRingStock stock, StationRingSpec spec)
        {
            foreach (int side in new[] { StationRingSpec.West, StationRingSpec.East }) spec.Through(side, -18.5f, -9.5f);
            spec.Through(StationRingSpec.South, 21.5f, 30.5f);
            spec.Through(StationRingSpec.South, 235.5f, 244.5f);
            spec.Through(StationRingSpec.North, -22.5f, -13.5f);
            spec.Through(StationRingSpec.North, 235.5f, 244.5f);
            spec.Spur(StationRingSpec.East, 75f, 85f);
            StationRings.Build(root, ext, a, spec, stock);
        }
    }
}
