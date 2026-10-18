using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>
    /// 0.24.11: the real player walks random NavMesh routes through each level, leg by leg; a leg the player can't
    /// finish is reported with whatever stopped them. Catches solid props, slabs or fences across walkable routes.
    /// Explicit: run by name, it takes minutes.
    /// </summary>
    [Explicit("Minutes long: Tools/unity.sh playmode LevelWalkTests")]
    public class LevelWalkTests
    {
        private const int Routes = 40;

        [UnityTest, Timeout(900000)] public IEnumerator Mall() => Walk("Mall");
        [UnityTest, Timeout(900000)] public IEnumerator School() => Walk("School");
        [UnityTest, Timeout(900000)] public IEnumerator Factory() => Walk("Factory");
        [UnityTest, Timeout(900000)] public IEnumerator Prison() => Walk("Prison");

        private static IEnumerator Walk(string scene)
        {
            yield return TestMapScene.Load(scene);
            PlayerTestRig rig = ForExisting(TestMapScene.Player);
            // No monsters or decay in the way of a walking test.
            foreach (var t in Object.FindObjectsByType<Abandoned.Threats.ThreatDirector>(FindObjectsSortMode.None)) t.enabled = false;
            NavMeshTriangulation mesh = NavMesh.CalculateTriangulation();
            var random = new System.Random(scene.GetHashCode());
            var blocked = new List<string>();
            var path = new NavMeshPath();
            int legs = 0, stairs = 0;
            int walls = ~LayerMask.GetMask("Player", "Loot", "Debris", "Ignore Raycast");
            for (int r = 0; r < Routes; r++)
            {
                Vector3 a = Point(mesh, random), b = Point(mesh, random);
                if (!NavMesh.CalculatePath(a, b, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete) continue;
                Vector3[] corners = path.corners;
                for (int i = 0; i + 1 < corners.Length; i++)
                {
                    Vector3 from = corners[i], to = corners[i + 1];
                    Vector3 flat = new(to.x - from.x, 0f, to.z - from.z);
                    float rise = to.y - from.y;
                    // Drops (off-mesh links, holes) aren't walked; stairs and ramps are, up and down.
                    if (flat.magnitude < 0.5f || Mathf.Abs(rise) > 4.5f || rise < -0.5f && flat.magnitude < Mathf.Abs(rise)) continue;
                    legs++;
                    if (Mathf.Abs(rise) > 0.5f) stairs++;
                    rig.Teleport(from + Vector3.up * 0.05f, Quaternion.LookRotation(flat).eulerAngles.y);
                    rig.Settle();
                    float t = rig.RunUntil(Frame(Vector2.up), () =>
                        Vector2.Distance(new Vector2(rig.Player.transform.position.x, rig.Player.transform.position.z), new Vector2(to.x, to.z)) < 0.6f
                        && Mathf.Abs(rig.Player.transform.position.y - to.y) < 0.6f,
                        flat.magnitude / 2.5f + 2f);
                    if (t >= 0f) continue;
                    Vector3 p = rig.Player.transform.position;
                    string what = Physics.CapsuleCast(p + Vector3.up * 0.4f, p + Vector3.up * 1.5f, 0.3f, flat.normalized, out RaycastHit hit, 1.5f, walls, QueryTriggerInteraction.Ignore)
                        ? $"{Path(hit.collider.transform)} ({hit.collider.GetType().Name})" : "nothing in front";
                    blocked.Add($"{from} -> {to}: stopped at {p} by {what}");
                }
                yield return null;
            }
            Debug.Log($"[Walk] {scene}: {legs} legs walked ({stairs} climbing or descending), {blocked.Count} blocked." + string.Concat(blocked.Select(s => "\n[Walk]   " + s)));
            Assert.Greater(legs, 20, $"{scene}: enough routes to mean something");
            Assert.LessOrEqual(blocked.Count, legs / 20, $"{scene}: blocked legs\n" + string.Join("\n", blocked));
        }

        private static Vector3 Point(NavMeshTriangulation mesh, System.Random random)
        {
            int tri = random.Next(mesh.indices.Length / 3) * 3;
            return (mesh.vertices[mesh.indices[tri]] + mesh.vertices[mesh.indices[tri + 1]] + mesh.vertices[mesh.indices[tri + 2]]) / 3f;
        }

        private static string Path(Transform t) => t.parent == null ? t.name : Path(t.parent) + "/" + t.name;
    }
}
