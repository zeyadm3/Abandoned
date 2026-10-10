using System;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Additions to the saved HQ (0.18.7+), which is never regenerated: opens and saves it, and finds bare
    /// walls in a floor-plan rectangle, clear of solids, dressing, player spawns and anything usable.
    /// </summary>
    public static class HqSites
    {
        private static List<Bounds> dressing, taken;
        private static List<Vector3> keepClear;

        public static Transform OpenHq(out Scene scene, string group)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Author the HQ in Edit mode.");
            scene = SceneManager.GetSceneByPath(HqBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return null;
                scene = EditorSceneManager.OpenScene(HqBuilder.ScenePath, OpenSceneMode.Single);
            }
            SceneManager.SetActiveScene(scene);
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == group) Object.DestroyImmediate(root);
            Begin(scene);
            var made = new GameObject(group);
            SceneManager.MoveGameObjectToScene(made, scene);
            return made.transform;
        }

        public static void SaveHq(Scene scene, string tag)
        {
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, HqBuilder.ScenePath)) throw new InvalidOperationException($"Unity could not save the {tag} HQ.");
            Debug.Log($"[{tag}] Saved the HQ as {BuildScript.Version}.");
        }

        private static void Begin(Scene scene)
        {
            Physics.SyncTransforms();
            dressing = new List<Bounds>();
            taken = new List<Bounds>();
            keepClear = new List<Vector3>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Renderer r in root.GetComponentsInChildren<Renderer>(false))
                {
                    if (!r.enabled || r is ParticleSystemRenderer || r.sharedMaterial == null || r.sharedMaterial.renderQueue >= 3000) continue;
                    Vector3 size = r.bounds.size;
                    if (Mathf.Max(size.x, size.z) > 5f || size.y > 5f) continue; // the building itself
                    dressing.Add(r.bounds);
                }
                foreach (PlayerSpawnPoint p in root.GetComponentsInChildren<PlayerSpawnPoint>(true)) keepClear.Add(p.transform.position);
                foreach (MonoBehaviour m in root.GetComponentsInChildren<MonoBehaviour>(true))
                    if (m is IUsable) keepClear.Add(m.transform.position);
            }
        }

        /// <summary>Bare wall spots in [xMin,xMax]×[zMin,zMax] on the ground floor for a box <paramref name="size"/> raised <paramref name="lift"/>.</summary>
        public static List<MallSites.Site> WallSites(Rect area, Vector3 size, float lift = 0f)
        {
            var found = new List<MallSites.Site>();
            Vector3[] dirs = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
            for (float x = area.xMin + 0.75f; x < area.xMax; x += 1f)
            for (float z = area.yMin + 0.75f; z < area.yMax; z += 1f)
            foreach (Vector3 dir in dirs)
            {
                Vector3 origin = new(x, 1.3f, z);
                if (!Physics.Raycast(origin, dir, out RaycastHit hit, 2.5f, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (Vector3.Dot(hit.normal, -dir) < 0.95f) continue;
                Vector3 at = new(hit.point.x, 0f, hit.point.z);
                if (Check(at, -dir, size, lift)) found.Add(new MallSites.Site(at, -dir, 0, Vector2Int.zero, null));
            }
            return found.GroupBy(s => new Vector3Int(Mathf.RoundToInt(s.At.x * 2f), 0, Mathf.RoundToInt(s.At.z * 2f))).Select(g => g.First()).ToList();
        }

        public static void Claim(MallSites.Site site, Vector3 size, float lift = 0f) => taken.Add(Box(site.At, site.Outward, size, lift));

        public static bool Check(Vector3 at, Vector3 outward, Vector3 size, float lift)
        {
            Quaternion facing = Quaternion.LookRotation(outward);
            Bounds body = Box(at, outward, size, lift);
            Vector3 centre = at + outward * (size.z * 0.5f + 0.04f) + Vector3.up * (lift + size.y * 0.5f);
            if (Physics.CheckBox(centre, size * 0.5f - Vector3.one * 0.03f, facing, ~0, QueryTriggerInteraction.Ignore)) return false;
            Vector3 front = at + outward * (size.z + 0.6f) + Vector3.up * 1f;
            if (Physics.CheckBox(front, new Vector3(Mathf.Max(0.35f, size.x * 0.5f), 0.85f, 0.45f), facing, ~0, QueryTriggerInteraction.Ignore)) return false;
            if (!Physics.Raycast(at + outward * 0.5f + Vector3.up * 0.5f, Vector3.down, 0.8f, ~0, QueryTriggerInteraction.Ignore)) return false;
            var frontBox = new Bounds(front, new Vector3(1f, 1.7f, 1f));
            if (dressing.Any(d => d.Intersects(body) || d.Intersects(frontBox)) || taken.Any(t => t.Intersects(body))) return false;
            return keepClear.All(p => Vector3.Distance(new Vector3(p.x, 0f, p.z), body.ClosestPoint(new Vector3(p.x, body.center.y, p.z))) > 1.5f);
        }

        private static Bounds Box(Vector3 at, Vector3 outward, Vector3 size, float lift)
        {
            Quaternion facing = Quaternion.LookRotation(outward);
            Vector3 centre = at + outward * (size.z * 0.5f + 0.04f) + Vector3.up * (lift + size.y * 0.5f);
            Vector3 s = facing * size;
            return new Bounds(centre, new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z)));
        }
    }
}
