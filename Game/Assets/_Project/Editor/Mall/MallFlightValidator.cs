using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Player;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Rebuild-time check that every intact flight in a level is climbable with the player's own controller
    /// limits (ground under a player-sized capsule all the way, headroom, slope, step lip) and that the
    /// baked NavMesh lets monsters take it. Logs one error per problem; the rebuild log is the report.
    /// </summary>
    public static class MallFlightValidator
    {
        private const string MovementConfigPath = "Assets/_Project/Data/Player/PlayerMovementConfig.asset";
        private const int Samples = 24;
        private const float Approach = 1f;

        public static int Run(Transform level)
        {
            Physics.SyncTransforms();
            var movement = AssetDatabase.LoadAssetAtPath<PlayerMovementConfig>(MovementConfigPath);
            float radius = movement != null ? movement.Radius : 0.35f;
            float height = movement != null ? movement.StandingHeight : 1.8f;
            float step = movement != null ? movement.StepOffset : 0.35f;
            float slopeLimit = movement != null ? movement.SlopeLimit : 45f;
            int mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris);
            int checkedFlights = 0, problems = 0;

            foreach (StructuralSection section in level.GetComponentsInChildren<StructuralSection>(true))
            {
                if (section.Type != SectionType.Stair) continue;
                checkedFlights++;
                Transform flight = section.transform;
                var found = new SortedSet<string>();
                Collider ramp = flight.Find(MallFlights.RampName)?.GetComponent<Collider>();
                if (ramp == null || !ramp.enabled || ramp.isTrigger || ramp.gameObject.layer == GameLayers.DebrisLayer)
                    found.Add("has no solid ramp collider on a player layer");

                for (int i = 0; i <= Samples; i++)
                {
                    float d = Mathf.Lerp(-Approach, MallFlights.Length + Approach, i / (float)Samples);
                    float expected = Mathf.Clamp01(d / MallFlights.Length) * MallFlights.Rise;
                    Vector3 at = flight.TransformPoint(new Vector3(0f, expected, d));
                    if (!Physics.Raycast(at + Vector3.up * 1.2f, Vector3.down, out RaycastHit hit, 2.4f, mask, QueryTriggerInteraction.Ignore))
                    {
                        found.Add($"no ground at {d:0.0} m along the flight");
                        continue;
                    }
                    if (Mathf.Abs(hit.point.y - at.y) > step)
                        found.Add($"ground {hit.point.y - at.y:+0.00;-0.00} m off the walking line at {d:0.0} m ({Path(hit.collider)})");
                    if (Vector3.Angle(hit.normal, Vector3.up) > slopeLimit)
                        found.Add($"surface steeper than {slopeLimit}° at {d:0.0} m ({Path(hit.collider)})");
                    Vector3 bottom = hit.point + Vector3.up * (step + radius), top = hit.point + Vector3.up * (height - radius);
                    foreach (Collider blocker in Physics.OverlapCapsule(bottom, top, radius * 0.9f, mask, QueryTriggerInteraction.Ignore))
                        if (!blocker.transform.IsChildOf(flight))
                            found.Add($"{Path(blocker)} blocks a standing player at {d:0.0} m");
                }

                string nav = CheckNavMesh(flight);
                if (nav != null) found.Add(nav);
                foreach (string problem in found) Debug.LogError($"[MallValidation] Flight {flight.name}: {problem}.");
                problems += found.Count;
            }
            Debug.Log($"[MallValidation] Flights: {checkedFlights} checked, {problems} problem(s).");
            return problems;
        }

        private static string CheckNavMesh(Transform flight)
        {
            Vector3 foot = flight.TransformPoint(new Vector3(0f, 0f, -Approach));
            Vector3 head = flight.TransformPoint(new Vector3(0f, MallFlights.Rise, MallFlights.Length + Approach));
            if (!NavMesh.SamplePosition(foot, out NavMeshHit a, 1.5f, NavMesh.AllAreas)) return "no NavMesh at its foot";
            if (!NavMesh.SamplePosition(head, out NavMeshHit b, 1.5f, NavMesh.AllAreas)) return "no NavMesh at its head";
            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete)
                return "monsters can't path from its foot to its head";
            float length = 0f;
            for (int i = 1; i < path.corners.Length; i++) length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            // A complete path that's far longer than the flight means monsters route around it, not up it.
            float direct = Mathf.Sqrt(MallFlights.Length * MallFlights.Length + MallFlights.Rise * MallFlights.Rise) + 2f * Approach;
            return length > direct * 1.5f ? $"monsters walk {length:0} m around it instead of {direct:0} m up it (NavMesh gap on the flight)" : null;
        }

        private static string Path(Component c)
        {
            string path = c.name;
            for (Transform t = c.transform.parent; t != null && t.parent != null; t = t.parent) path = t.name + "/" + path;
            return path;
        }
    }
}
