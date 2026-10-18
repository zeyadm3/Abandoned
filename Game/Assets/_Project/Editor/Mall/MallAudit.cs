using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Every [MallValidation] check against the saved mall, changing nothing (0.24.11). The 0.15.0 rebuild dropped
    /// the flight, invisible-collider, walk-through and reachability audits from MallBuilder, and nothing has rebuilt
    /// the mall from scratch since, so they had stopped running. <see cref="Levels"/> runs the walk-through and
    /// invisible-collider audits on the other levels. Batch: Tools/unity.sh exec Abandoned.EditorTools.MallAudit.RunAndExit.
    /// </summary>
    public static class MallAudit
    {
        [MenuItem("Tools/Abandoned/Mall/Audit Saved Mall")]
        public static int Run()
        {
            Scene scene = SceneManager.GetSceneByPath(MallBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded) scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            Transform mall = scene.GetRootGameObjects().FirstOrDefault(r => r.name == "Mall")?.transform;
            if (mall == null) throw new InvalidOperationException("The saved Mall root is missing.");
            int flights = MallFlightValidator.Run(mall);
            int invisible = InvisibleColliderAudit.Run(mall, "Mall");
            int walkThrough = SolidObjectsPass.Audit(mall, "Mall");
            int cut = new MallRouteClearance(mall).Reachability();
            int lookalikes = LootLookalikeAudit.Run(mall, "Mall");
            int stairs = Stairs(mall, "Mall");
            Debug.Log($"[MallAudit] flights {flights}, invisible colliders {invisible}, walk-through {walkThrough}, cut off {cut}, loot look-alikes {lookalikes}, blocked stairs {stairs}.");
            return flights + invisible + cut + lookalikes + stairs;
        }

        [MenuItem("Tools/Abandoned/Mall/Audit All Levels")]
        public static int Levels()
        {
            int problems = 0;
            foreach (string path in new[] { HqBuilder.ScenePath, SchoolBuilder.ScenePath, FactoryBuilder.ScenePath, PrisonBuilder.ScenePath })
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                GameObject root = scene.GetRootGameObjects().OrderByDescending(r => r.GetComponentsInChildren<Renderer>(true).Length).First();
                int walk = SolidObjectsPass.Audit(root.transform, scene.name);
                int invisible = InvisibleColliderAudit.Run(root.transform, scene.name);
                int stairs = Stairs(root.transform, scene.name);
                Debug.Log($"[LevelAudit] {scene.name}: walk-through {walk}, invisible colliders {invisible}, blocked stairs {stairs}.");
                problems += walk + invisible + stairs;
            }
            return problems;
        }

        /// <summary>
        /// A player-sized capsule up each flight's own walking surface, along three lines across it: anything else it
        /// touches is reported (0.24.11: the mall's basement flights ran into the district ground slab, the Prison's
        /// tunnel stair into the ground floor and a cell).
        /// </summary>
        public static int Stairs(Transform level, string levelName)
        {
            Physics.SyncTransforms();
            int blocked = 0;
            foreach (Transform flights in level.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Flights"))
            foreach (Transform f in flights.Cast<Transform>())
            {
                Collider[] own = f.GetComponentsInChildren<Collider>().Where(c => !c.isTrigger).ToArray();
                if (own.Length == 0) continue;
                Bounds b = own[0].bounds;
                foreach (Collider c in own) b.Encapsulate(c.bounds);
                bool alongZ = b.size.z > b.size.x;
                foreach (float across in new[] { .25f, .5f, .75f })
                for (float k = .05f; k < .95f; k += .05f)
                {
                    float x = Mathf.Lerp(b.min.x, b.max.x, alongZ ? across : k), z = Mathf.Lerp(b.min.z, b.max.z, alongZ ? k : across);
                    RaycastHit[] hits = Physics.RaycastAll(new Vector3(x, b.max.y + .5f, z), Vector3.down, b.size.y + 1f)
                        .Where(h => h.collider.transform.IsChildOf(f)).OrderBy(h => h.distance).ToArray();
                    if (hits.Length == 0) continue;
                    Vector3 foot = hits[0].point;
                    Collider hit = Physics.OverlapCapsule(foot + Vector3.up * .4f, foot + Vector3.up * 1.5f, .3f)
                        .FirstOrDefault(c => !c.isTrigger && !c.transform.IsChildOf(f));
                    if (hit == null) continue;
                    blocked++;
                    Debug.LogWarning($"[MallValidation] {levelName}: stair {f.name} blocked at {foot} by {hit.name}.");
                    break;
                }
            }
            return blocked;
        }

        public static void RunAndExit() => ReleaseKit.RunAndExit(() => { Run(); Levels(); });
    }
}
