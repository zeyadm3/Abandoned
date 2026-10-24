using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Lays the city into Scenes/World in place of the wooded hills (Docs/CITY_PLAN.md step 7). Everything the world
    /// already does stays: the movable root and its anchor, the van, the place streaming, the road events, the job
    /// flow and the atlas. Only the land changes.
    ///
    /// No terrain at all in this mode. The city's own ground reaches the world's edge, so there is nothing for a
    /// Unity Terrain to do, and leaving it out keeps the build from having to flatten hills under six kilometres of
    /// streets. The hills come back outside the band in a later pass; the draft is flat.
    /// </summary>
    public static class CityWorld
    {
        /// <summary>Ground past the city band, far enough to reach the corners of an 8 km world.</summary>
        private const float WorldOutskirts = 2600f;

        /// <summary>
        /// Builds the whole city under <paramref name="land"/>. The places are only read, for the ground each one
        /// needs kept clear; the world's own stand-ins and streaming still own what happens at a site.
        /// </summary>
        public static void Build(Transform land)
        {
            Material[] blockMaterials = BlockMaterials();
            CityMesh.Reset();
            CityCells.Reset();
            CityGround.Outskirts = WorldOutskirts;

            Debug.Log("[CityWorld] Measuring the ten places...");
            CitySites.Measure();
            Debug.Log("[CityWorld] Ground...");
            int cells = CityGround.Build(land, Paving(), Park(), Wild());
            Debug.Log("[CityWorld] Water...");
            int crossings = CityWater.Build(land, Sea(), River(), Bank(), Pier());
            Debug.Log("[CityWorld] Roads...");
            (int roadCells, int bridges) = CityRoads.Build(land, Asphalt(), Deck());
            Debug.Log("[CityWorld] Blocks...");
            (int blockCells, int buildings) = CityMassing.Build(land, blockMaterials);
            float crossing = CityIsland.Build(land, Rock(), Paving());
            (int streamed, int farTriangles) = CityCells.Finish(land, Far());

            Debug.Log($"[CityWorld] {cells} ground cells, {roadCells} with road, {blockCells} with blocks; " +
                      $"{buildings} buildings, {bridges} bridge slabs, {crossings} crossings; " +
                      $"{streamed} streamed cells ({farTriangles} far triangles); " +
                      $"Gallows Rock {crossing:0} m off the quay.");
        }

        /// <summary>
        /// The roads the atlas carries, for placing road events ahead of the van. Only the arterials the drawing
        /// names and the spurs into the places: every street in the city would be thousands of lines, and an event
        /// belongs on a road you are actually driving along.
        /// </summary>
        public static List<List<Vector3>> AtlasRoads()
        {
            var roads = new List<List<Vector3>>();
            foreach (CityRoadNetwork.Line line in CityRoadNetwork.All())
            {
                if (!line.Arterial || line.Points.Length < 2 || !Carried(line.Name)) continue;
                var points = new List<Vector3>();
                foreach (Vector2 p in Walk(line.Points)) points.Add(new Vector3(p.x, CityRoads.RoadY, p.y));
                if (points.Count >= 2) roads.Add(points);
            }
            return roads;
        }

        // A district's grid lines are named "<District>_<offset>"; those are streets, however wide, and there are
        // far too many of them to ride in the atlas.
        private static bool Carried(string name)
        {
            foreach (CityPlan.District d in CityPlan.Districts)
                if (name.StartsWith(d.Name + "_")) return false;
            return true;
        }

        // A point every ten metres, which is what the road-event director expects of an atlas road.
        private static IEnumerable<Vector2> Walk(Vector2[] points)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                float len = Vector2.Distance(points[i], points[i + 1]);
                for (float t = 0f; t < len; t += 10f)
                    yield return Vector2.Lerp(points[i], points[i + 1], t / Mathf.Max(len, .001f));
            }
            yield return points[points.Length - 1];
        }

        private static Material Paving() => PolishAssets.Material("City_Paving", new Color(.40f, .39f, .38f), .88f);
        private static Material Park() => PolishAssets.Material("City_Park", new Color(.26f, .33f, .22f), .95f);
        private static Material Wild() => PolishAssets.Material("City_Outskirts", new Color(.30f, .32f, .24f), .95f);
        private static Material Sea() => PolishAssets.Material("City_Sea", new Color(.13f, .20f, .26f), .22f);
        private static Material River() => PolishAssets.Material("City_River", new Color(.16f, .22f, .25f), .25f);
        private static Material Bank() => PolishAssets.Material("City_Bank", new Color(.28f, .27f, .25f), .92f);
        private static Material Pier() => PolishAssets.Material("City_Pier", new Color(.33f, .32f, .30f), .9f);
        private static Material Asphalt() => PolishAssets.Material("City_Asphalt", new Color(.17f, .17f, .18f), .82f);
        private static Material Deck() => PolishAssets.Material("City_Deck", new Color(.21f, .21f, .22f), .84f);
        private static Material Rock() => PolishAssets.Material("City_Rock", new Color(.30f, .29f, .27f), .94f);
        private static Material Far() => PolishAssets.Material("City_Far", new Color(.42f, .42f, .43f), .9f);

        // One material per district kind, indexed by the enum, so the city reads as quarters from the air.
        private static Material[] BlockMaterials()
        {
            var byKind = new Material[System.Enum.GetValues(typeof(CityKind)).Length];
            byKind[(int)CityKind.Downtown] = PolishAssets.Material("City_Block_Downtown", new Color(.46f, .47f, .50f), .7f);
            byKind[(int)CityKind.Retail] = PolishAssets.Material("City_Block_Retail", new Color(.56f, .50f, .43f), .82f);
            byKind[(int)CityKind.Civic] = PolishAssets.Material("City_Block_Civic", new Color(.57f, .55f, .57f), .78f);
            byKind[(int)CityKind.Residential] = PolishAssets.Material("City_Block_Residential", new Color(.50f, .44f, .39f), .85f);
            byKind[(int)CityKind.Industrial] = PolishAssets.Material("City_Block_Industrial", new Color(.44f, .41f, .37f), .86f);
            byKind[(int)CityKind.Waterfront] = PolishAssets.Material("City_Block_Waterfront", new Color(.47f, .45f, .41f), .86f);
            byKind[(int)CityKind.Suburb] = PolishAssets.Material("City_Block_Suburb", new Color(.54f, .50f, .44f), .88f);
            byKind[(int)CityKind.Park] = PolishAssets.Material("City_Block_Park", new Color(.30f, .36f, .26f), .92f);
            return byKind;
        }
    }
}
