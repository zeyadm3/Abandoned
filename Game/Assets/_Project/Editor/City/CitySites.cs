using System.Collections.Generic;
using Abandoned.World;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The ground each salvage location stands on in the city (Docs/CITY_PLAN.md step 6). Every map's real footprint
    /// is read from its own saved scene by the existing <see cref="WorldSurvey"/>, rather than guessed: the Mall's
    /// district is 640 m across and the Hospital's campus 88 m, so one keep-clear figure for all ten would either
    /// pave over the Mall or leave a hole round the Hospital.
    ///
    /// What it leaves behind: a flat pad where each map will stand, the rectangle the blocks must keep off
    /// (<see cref="Cleared"/>), the spur the roads need to reach it (<see cref="CityRoadNetwork"/> reads
    /// <see cref="Approach"/>), and the marker the van parks at.
    /// </summary>
    public static class CitySites
    {
        /// <summary>Used until a map's own scene has been measured: enough for a yard and an approach.</summary>
        public const float UnmeasuredHalfExtent = 120f;
        /// <summary>Kept clear round a map's ground, for its apron and the road coming in.</summary>
        private const float Margin = 40f;

        private static readonly Dictionary<string, Rect> ground = new();

        /// <summary>Every site's world rectangle, including its margin. Empty until <see cref="Measure"/> has run.</summary>
        public static IEnumerable<Rect> Cleared
        {
            get
            {
                foreach (KeyValuePair<string, Rect> entry in ground) yield return entry.Value;
            }
        }

        /// <summary>A site's cleared rectangle, measured if its scene exists and guessed if not.</summary>
        public static Rect GroundOf(CityPlan.Site site) =>
            ground.TryGetValue(site.Scene, out Rect r) ? r : Default(site);

        /// <summary>Where the road has to come from, and how far out of the site it has to start.</summary>
        public static (Vector2 from, Vector2 to) Approach(CityPlan.Site site)
        {
            Rect r = GroundOf(site);
            Vector2 on = CityRoadNetwork.Bearing(site.Approach);
            float reach = Mathf.Max(r.width, r.height) * .5f;
            return (site.Centre + on * reach, site.Centre + on * (reach + 420f));
        }

        /// <summary>
        /// Opens each map's saved scene, measures its ground and remembers it. Slow (ten scenes), and run once at the
        /// start of a city build, before anything is laid out round the sites.
        /// </summary>
        public static void Measure()
        {
            ground.Clear();
            var places = new List<WorldPoi>();
            foreach (CityPlan.Site site in CityPlan.Sites)
                places.Add(new WorldPoi
                {
                    scene = site.Scene,
                    worldPosition = new Vector3(site.Centre.x, 0f, site.Centre.y),
                    ground = Rect.MinMaxRect(-UnmeasuredHalfExtent, -UnmeasuredHalfExtent,
                                             UnmeasuredHalfExtent, UnmeasuredHalfExtent),
                    built = true,
                    measure = true,
                });
            WorldSurvey.Capture(places);

            foreach (WorldPoi place in places)
            {
                Rect r = place.WorldGround;
                ground[place.scene] = Rect.MinMaxRect(r.xMin - Margin, r.yMin - Margin,
                                                      r.xMax + Margin, r.yMax + Margin);
                Debug.Log($"[CitySite] {place.scene}: {r.width:0} x {r.height:0} m at " +
                          $"({place.worldPosition.x:0}, {place.worldPosition.z:0}).");
            }
        }

        /// <summary>Lays a pad and a marker for every site. Returns how many it placed.</summary>
        public static int Build(Transform parent, Material pad)
        {
            Transform group = GreyboxFactory.Group("Sites", parent);
            int placed = 0;
            foreach (CityPlan.Site site in CityPlan.Sites)
            {
                if (site.Scene == "Prison") continue;   // its ground is the island, built by CityIsland
                Rect r = GroundOf(site);
                Transform at = GreyboxFactory.Group(site.Scene, group);

                // A pad a hair above the paving: the real scene streams in over it and hides it (PoiAdapter).
                var piece = new CityMesh.Piece();
                piece.Flat(r.xMin, r.yMin, r.xMax, r.yMax, CityPlan.GroundY + .02f);
                CityMesh.Render("Pad", at, piece.Save($"Site_{site.Scene}"), pad);

                (Vector2 from, Vector2 _) = Approach(site);
                var marker = new GameObject("VanParking").transform;
                marker.SetParent(at, false);
                Vector3 toward = new Vector3(site.Centre.x - from.x, 0f, site.Centre.y - from.y).normalized;
                marker.SetPositionAndRotation(new Vector3(from.x, CityPlan.GroundY, from.y),
                                              Quaternion.LookRotation(toward, Vector3.up));
                CityCells.Register(CityPlan.CellOf(site.Centre), at);
                placed++;
            }
            return placed;
        }

        // What a site is given before its scene has been opened: a square big enough for a yard.
        private static Rect Default(CityPlan.Site site) => Rect.MinMaxRect(
            site.Centre.x - UnmeasuredHalfExtent - Margin, site.Centre.y - UnmeasuredHalfExtent - Margin,
            site.Centre.x + UnmeasuredHalfExtent + Margin, site.Centre.y + UnmeasuredHalfExtent + Margin);
    }
}
