using System.Collections.Generic;
using UnityEngine;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Where monsters may appear on Gallows Rock: through the cellhouse (its aisles, the dining hall, the galleries on
    /// both tiers), down in the basement, in the warden's house and the lighthouse store, the officers' club, the
    /// yard and the far lawns, and the barracks' upper floors. Never on the quay or the pier, and every point at
    /// least 30 m from the boat, so the crew always steps off into a moment's quiet.
    /// </summary>
    public static class IslandThreatPlan
    {
        private const float MinFromBoat = 30f;

        private static readonly Vector3[] Candidates =
        {
            // Cellhouse ground floor.
            new(36f, L.PlateauY, 76f), new(72f, L.PlateauY, 62f), new(54f, L.PlateauY, 77f), new(53f, L.PlateauY, 90f), new(36f, L.PlateauY, 57f),
            // Galleries (deck tops): C-block's west spine and east tier 2, B-block's east tier 1 and west tier 2.
            new(39.8f, L.PlateauY + 4f, 70f), new(47.8f, L.PlateauY + 8f, 64f), new(68.2f, L.PlateauY + 4f, 60f), new(60.2f, L.PlateauY + 8f, 58f),
            // Basement: laundry, showers, boiler room.
            new(44f, L.BasementY, 87f), new(57f, L.BasementY, 85f), new(70.5f, L.BasementY, 83.5f),
            // Warden's house (study, upstairs), the lighthouse store, the officers' club.
            new(46.5f, L.PlateauY, 108.5f), new(47f, L.PlateauY + L.Storey, 108.5f), new(69f, L.PlateauY, 106.5f), new(100f, L.PlateauY, 36f),
            // Outside on the plateau: the yard, the north lawn, the water tower.
            new(16f, L.PlateauY, 70f), new(90f, L.PlateauY, 110f), new(110f, L.PlateauY, 100f), new(20f, L.PlateauY, 104f),
            // The barracks' upper floors.
            new(110f, 2f * L.Storey, 9f), new(124f, L.Storey, 6f),
        };

        public static List<Vector3> Points(out List<string> problems)
        {
            problems = new List<string>();
            var points = new List<Vector3>();
            Vector3 boat = L.BoatStern;
            foreach (Vector3 p in Candidates)
            {
                if (Vector3.Distance(new Vector3(p.x, 0f, p.z), new Vector3(boat.x, 0f, boat.z)) < MinFromBoat)
                {
                    problems.Add($"{p} is closer than {MinFromBoat} m to the boat");
                    continue;
                }
                points.Add(p);
            }
            return points;
        }
    }
}
