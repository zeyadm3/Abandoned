using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Abandoned.Threats
{
    /// <summary>Host spawn placement: a hidden, empty point with a real route to a living crew member.</summary>
    internal static class ThreatSpawnSelector
    {
        private static readonly Collider[] Occupants = new Collider[12];

        public static bool Choose(ThreatSpawnPoint[] points, NetworkObject prefab, NetworkManager manager,
            Scene scene, float minimumDistance, System.Random random, out Vector3 position, out Quaternion rotation,
            out string marker)
        {
            position = default; rotation = Quaternion.identity; marker = null;
            NavMeshAgent agent = prefab.GetComponent<NavMeshAgent>();
            var filter = new NavMeshQueryFilter
            {
                agentTypeID = agent != null ? agent.agentTypeID : 0,
                areaMask = agent != null ? agent.areaMask : NavMesh.AllAreas
            };
            var crew = new List<(NetworkPlayer player, Vector3 floor)>();
            foreach (NetworkPlayer player in NetworkPlayer.All)
            {
                if (player == null || !player.IsSpawned || player.IsDead || player.NetworkManager != manager) continue;
                if (NavMesh.SamplePosition(player.transform.position, out NavMeshHit ground, 2f, filter)
                    && Mathf.Abs(ground.position.y - player.transform.position.y) < 1.5f)
                    crew.Add((player, ground.position));
            }
            // Outside the building the crew is not yet on its navigation island. Wait for entry.
            if (crew.Count == 0) return false;
            int walls = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            var path = new NavMeshPath();
            float best = float.MaxValue;
            foreach (ThreatSpawnPoint point in points)
            {
                if (point == null || point.gameObject.scene != scene) continue;
                if (!NavMesh.SamplePosition(point.transform.position, out NavMeshHit hit, 1.75f, filter)
                    || Mathf.Abs(hit.position.y - point.transform.position.y) > 1.2f) continue;
                if (!Hidden(hit.position, manager, minimumDistance, walls)) continue;
                float radius = Mathf.Max(0.3f, agent != null ? agent.radius : 0.4f);
                // Tall creature rigs can bend under a ceiling; reserve their locomotion footprint.
                float height = Mathf.Max(radius * 2f, Mathf.Min(agent != null ? agent.height : 1.8f, 1.85f));
                int count = Physics.OverlapCapsuleNonAlloc(hit.position + Vector3.up * (radius + 0.06f),
                    hit.position + Vector3.up * (height - radius + 0.06f), radius, Occupants, walls,
                    QueryTriggerInteraction.Ignore);
                if (count > 0) continue;
                float route = float.MaxValue;
                foreach (var member in crew)
                {
                    if (!NavMesh.CalculatePath(hit.position, member.floor, filter, path)
                        || path.status != NavMeshPathStatus.PathComplete) continue;
                    Vector3[] corners = path.corners;
                    float length = 0f;
                    for (int i = 1; i < corners.Length; i++) length += Vector3.Distance(corners[i - 1], corners[i]);
                    if (Mathf.Abs(hit.position.y - member.floor.y) > 2.5f) length += 12f;
                    route = Mathf.Min(route, length);
                }
                if (route == float.MaxValue) continue;
                float score = route + (float)random.NextDouble() * 8f;
                if (score >= best) continue;
                best = score; position = hit.position; rotation = point.transform.rotation; marker = point.name;
            }
            return marker != null;
        }

        private static bool Hidden(Vector3 at, NetworkManager manager, float minimumDistance, int walls)
        {
            foreach (NetworkPlayer player in NetworkPlayer.All)
            {
                if (player == null || !player.IsSpawned || player.IsDead || player.NetworkManager != manager) continue;
                Vector3 eye = player.transform.position + Vector3.up * 1.6f;
                if (Vector3.Distance(player.transform.position, at) < minimumDistance) return false;
                if (!Physics.Linecast(eye, at + Vector3.up * 1.2f, walls, QueryTriggerInteraction.Ignore)) return false;
            }
            return true;
        }
    }
}
