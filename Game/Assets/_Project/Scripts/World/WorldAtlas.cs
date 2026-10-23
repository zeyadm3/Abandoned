using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>Every place in the open world and where the van starts (Data/World/Resources/WorldAtlas, written by the world builder).</summary>
    [CreateAssetMenu(menuName = "Abandoned/World/World Atlas")]
    public class WorldAtlas : ScriptableObject
    {
        public const string WorldScene = "World";

        [SerializeField] private List<WorldPoi> pois = new();
        [Tooltip("Index of the HQ in the list.")]
        [SerializeField] private int home;
        [SerializeField] private List<WorldRoad> roads = new();

        public IReadOnlyList<WorldPoi> Pois => pois;
        public int Home => home;
        public WorldPoi HomePoi => pois.Count > home ? pois[home] : null;
        public IReadOnlyList<WorldRoad> Roads => roads;

        /// <summary>
        /// The road point <paramref name="ahead"/> metres on from the nearest road point to <paramref name="world"/>,
        /// going the way <paramref name="heading"/> points, with the road's direction there. False if no road is near.
        /// </summary>
        public bool RoadAhead(Vector3 world, Vector3 heading, float ahead, float maxOff, out Vector3 point, out Vector3 along)
        {
            point = along = Vector3.zero;
            WorldRoad best = null;
            int bestIndex = -1;
            float bestDistance = maxOff * maxOff;
            foreach (WorldRoad r in roads)
                for (int i = 0; i < r.points.Length; i++)
                {
                    Vector3 d = r.points[i] - world;
                    float sq = d.x * d.x + d.z * d.z;
                    if (sq < bestDistance) { bestDistance = sq; best = r; bestIndex = i; }
                }
            if (best == null || best.points.Length < 2) return false;
            int next = Mathf.Min(bestIndex + 1, best.points.Length - 1), prev = Mathf.Max(bestIndex - 1, 0);
            Vector3 dir = best.points[next] - best.points[prev];
            int step = Vector3.Dot(dir, heading) >= 0f ? 1 : -1;
            int target = Mathf.Clamp(bestIndex + step * Mathf.RoundToInt(ahead / 10f), 0, best.points.Length - 1);
            point = best.points[target];
            int a = Mathf.Clamp(target - step, 0, best.points.Length - 1);
            along = (best.points[target] - best.points[a]).normalized;
            if (along == Vector3.zero) along = heading;
            return true;
        }

        public int IndexOf(string scene)
        {
            for (int i = 0; i < pois.Count; i++) if (pois[i].scene == scene) return i;
            return -1;
        }

        public void EditorSetup(List<WorldPoi> places, int homeIndex, List<WorldRoad> roadLines)
        {
            pois = places;
            home = homeIndex;
            roads = roadLines;
        }

        private static WorldAtlas cached;

        public static WorldAtlas Load()
        {
            if (cached == null) cached = Resources.Load<WorldAtlas>("WorldAtlas");
            return cached;
        }
    }
}
