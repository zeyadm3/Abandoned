using System.Collections;
using System.Linq;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Extraction;
using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Tests
{
    /// <summary>Where a test player can stand inside the mall so monsters can actually reach them (0.24.10).</summary>
    public static class MallTestSpots
    {
        // Some markers sit in pockets with no route out, some by wet ground; pick a dry one most others can walk to from 14 m+ away.
        public static Vector3 BestConnected(ThreatDirector threats)
        {
            var path = new NavMeshPath();
            Vector3 best = default;
            int most = -1;
            foreach (ThreatSpawnPoint from in threats.Points)
            {
                if (!NavMesh.SamplePosition(from.transform.position, out NavMeshHit a, 2f, NavMesh.AllAreas)) continue;
                // Never on a puddle or flood: wet ground splashes (a monster hears it) and a live flood shocks.
                if (NoisyGround.All.Any(g => g != null && (g.Covers(a.position) || g.Covers(a.position + Vector3.right * 1.5f) || g.Covers(a.position - Vector3.right * 1.5f) || g.Covers(a.position + Vector3.forward * 1.5f) || g.Covers(a.position - Vector3.forward * 1.5f)))) continue;
                int reached = threats.Points.Count(to => Vector3.Distance(to.transform.position, a.position) >= 14f
                    && NavMesh.SamplePosition(to.transform.position, out NavMeshHit b, 2f, NavMesh.AllAreas)
                    && NavMesh.CalculatePath(b.position, a.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete);
                if (reached > most) { most = reached; best = a.position; }
            }
            Assert.Greater(most, 2, "somewhere in the mall that monsters can reach");
            return best;
        }

        /// <summary>
        /// The spawns are in the front court, off the mall's NavMesh, where no monster may appear: walk the local
        /// player in first (to the best connected dry marker).
        /// </summary>
        public static IEnumerator StepInside()
        {
            NetworkPlayer.Local.OwnerTeleport(BestConnected(ThreatDirector.Current) + Vector3.up * 0.1f);
            yield return new WaitForSeconds(0.3f);
        }

        /// <summary>A new monster plays its song before it may hurt anyone (2026-10-08, SpawnWarningSeconds): skip the wait.</summary>
        public static void SkipWarning(Threat threat) =>
            typeof(Threat).GetField("attackReadyAt", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(threat, 0f);

        private static bool Dry(Vector3 at) =>
            !NoisyGround.All.Any(g => g != null && (g.Covers(at) || g.Covers(at + Vector3.right) || g.Covers(at - Vector3.right)
                                                    || g.Covers(at + Vector3.forward) || g.Covers(at - Vector3.forward)));

        /// <summary>
        /// Two dry ground-floor points <paramref name="length"/> apart along +z inside the mall, walkable all the way
        /// and in plain sight of each other at eye height: room for a monster to see a player.
        /// </summary>
        public static (Vector3 from, Vector3 to) OpenLine(float length)
        {
            int walls = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            for (float x = 2f; x < 56f; x += 1f)
            for (float z = 2f; z + length < 48f; z += 1f)
            {
                if (!NavMesh.SamplePosition(new Vector3(x, 0f, z), out NavMeshHit a, 0.3f, NavMesh.AllAreas) || Mathf.Abs(a.position.y) > 0.3f) continue;
                if (!NavMesh.SamplePosition(new Vector3(x, 0f, z + length), out NavMeshHit b, 0.3f, NavMesh.AllAreas) || Mathf.Abs(b.position.y) > 0.3f) continue;
                if (NavMesh.Raycast(a.position, b.position, out _, NavMesh.AllAreas)) continue;
                if (Physics.Linecast(a.position + Vector3.up * 1.6f, b.position + Vector3.up * 1.6f, walls, QueryTriggerInteraction.Ignore)) continue;
                if (!Dry(a.position) || !Dry(b.position)) continue;
                return (a.position, b.position);
            }
            Assert.Fail($"no open {length} m line on the mall's ground floor");
            return default;
        }
    }
}
