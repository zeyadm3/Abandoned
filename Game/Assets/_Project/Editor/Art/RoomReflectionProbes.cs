using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.13: room reflection probes for a map, from its NavMesh. Walkable ground is cut into cells per storey; every
    /// cell with a ceiling over it gets a realtime, box-projected probe at head height, its box pushed out to the walls
    /// around it (raycasts) and up to the ceiling, so reflections line up with the room. Outdoors keeps the sky. Cells a
    /// builder's own probe already covers are left to it. All of them sit under one <see cref="RoomReflections"/>, which
    /// draws them at run time. Replaced on every run (the `_AV_RoomReflections` object).
    /// </summary>
    public static class RoomReflectionProbes
    {
        public const string ObjectName = "_AV_RoomReflections";
        private const int MaxProbes = 160; // 64 px HDR cubes: about 0.3 MB each
        private const float MinArea = 6f, EyeHeight = 1.6f, MaxCeiling = 14f, StoreyBand = 3f;

        public static int Place(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == ObjectName) Object.DestroyImmediate(root);
            NavMeshTriangulation nav = NavMesh.CalculateTriangulation();
            if (nav.indices.Length < 3) return 0;
            Physics.SyncTransforms();
            var existing = Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None).Where(p => p.gameObject.scene == scene).ToList();

            // Grow the cell until the count fits the budget. The largest size is used whatever the count (the biggest
            // cells get the probes): a tower of many storeys never fits, and once fell out of the loop with none.
            for (float cell = 10f; cell <= 40f; cell *= 1.25f)
            {
                var cells = Cells(nav, cell);
                if (cells.Count > MaxProbes * 1.5f && cell * 1.25f <= 40f) continue;
                var go = new GameObject(ObjectName);
                SceneManager.MoveGameObjectToScene(go, scene);
                int placed = 0;
                int mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast", "UI");
                foreach (var (centre, floor) in cells.OrderByDescending(c => c.area).Take(MaxProbes).Select(c => (c.centre, c.floor)))
                {
                    Vector3 eye = new(centre.x, floor + EyeHeight, centre.z);
                    if (existing.Any(p => p.bounds.Contains(eye))) continue;
                    // Indoors only: a ceiling within reach above.
                    if (!Physics.Raycast(eye, Vector3.up, out RaycastHit up, MaxCeiling, mask, QueryTriggerInteraction.Ignore)) continue;
                    float reach = cell * 0.5f + 1f;
                    float Wall(Vector3 dir) => Physics.Raycast(eye, dir, out RaycastHit h, reach, mask, QueryTriggerInteraction.Ignore) ? h.distance : reach;
                    float east = Wall(Vector3.right), west = Wall(Vector3.left), north = Wall(Vector3.forward), south = Wall(Vector3.back);
                    var min = new Vector3(eye.x - west, floor - 0.1f, eye.z - south);
                    var max = new Vector3(eye.x + east, up.point.y + 0.1f, eye.z + north);
                    var probeGo = new GameObject($"Room_{placed:000}");
                    probeGo.transform.SetParent(go.transform, false);
                    probeGo.transform.position = eye;
                    var probe = probeGo.AddComponent<ReflectionProbe>();
                    probe.mode = ReflectionProbeMode.Realtime;
                    probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                    probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
                    probe.resolution = 64;
                    probe.hdr = true;
                    probe.boxProjection = true;
                    probe.size = max - min;
                    probe.center = (min + max) * 0.5f - eye;
                    probe.blendDistance = 1f;
                    probe.importance = 1;
                    probe.intensity = 1f;
                    probe.nearClipPlane = 0.1f;
                    probe.farClipPlane = 80f;
                    probe.shadowDistance = 20f;
                    probe.cullingMask = mask;
                    probe.clearFlags = ReflectionProbeClearFlags.Skybox;
                    placed++;
                }
                go.AddComponent<RoomReflections>();
                return placed;
            }
            return 0;
        }

        private static List<(Vector3 centre, float floor, float area)> Cells(NavMeshTriangulation nav, float cell)
        {
            var sums = new Dictionary<(int, int, int), (Vector3 weighted, float area, float floor)>();
            for (int t = 0; t + 2 < nav.indices.Length; t += 3)
            {
                Vector3 a = nav.vertices[nav.indices[t]], b = nav.vertices[nav.indices[t + 1]], c = nav.vertices[nav.indices[t + 2]];
                float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;
                if (area <= 0f) continue;
                Vector3 m = (a + b + c) / 3f;
                var key = (Mathf.FloorToInt(m.x / cell), Mathf.FloorToInt(m.z / cell), Mathf.FloorToInt((m.y + 0.5f) / StoreyBand));
                sums.TryGetValue(key, out var s);
                sums[key] = (s.weighted + m * area, s.area + area, s.area == 0f ? m.y : Mathf.Min(s.floor, m.y));
            }
            return sums.Values.Where(s => s.area >= MinArea)
                .Select(s => (s.weighted / s.area, s.floor, s.area)).ToList();
        }
    }
}
