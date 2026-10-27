using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Phase 6 of the 2026 remake: a map keeps its layout, colliders and gameplay, and its code-built props (a desk of
    /// boxes, a car of boxes, a window of boxes) are drawn by modelled ones (<see cref="KitFamilies"/>, models from
    /// Tools/Blender/remake/maps/kit.py). A prop is swapped only when it is purely drawing: renderers, colliders, LOD
    /// groups and section props, nothing that does anything. Its renderers are taken off and recorded
    /// (<see cref="KitSwapRecord"/>), so the pass can always be run again or undone; its colliders stay, so NavMesh and
    /// occlusion need no rebake. Runs in <see cref="AvOverhaul.AfterMaps"/> before the surfaces and the coplanar fix.
    /// </summary>
    public static partial class KitSwap
    {
        public const string KitFolder = RemakePaths.Maps + "/Kit";
        private const string RootName = "_KitSwap";
        private const int MaxRenderers = 160;

        /// <summary>Markers that stay valid when only the drawing changes: footstep surfaces, NavMesh hints, colliders
        /// kept on purpose. A loot spawn point does not: it sits on the old model's shelves and tops.</summary>
        private static readonly System.Text.RegularExpressions.Regex StandIn =
            new(@"(?i)dust_?sheet|packaging|^fixed_|plastic_pallet|waste_compactor");

        private static readonly HashSet<string> Passive = new()
        {
            "SurfaceTag", "NavMeshModifier", "NavMeshModifierVolume", "NavMeshObstacle", "IntentionalInvisibleCollider"
        };

        private sealed class Candidate
        {
            public Transform Group;
            public KitFamily Family;
            public Bounds Box;
            public List<MeshRenderer> Renderers;
        }

        [MenuItem(ToolMenu.Art + "Remake: Swap Kit Props in Open Map", priority = ToolMenu.ArtP + 103)]
        public static void ApplyOpenMenu()
        {
            Prepare();
            Debug.Log(Apply(SceneManager.GetActiveScene()));
        }

        /// <summary>The kit's models get their baked materials (models imported before them come in with empty slots).</summary>
        public static void Prepare()
        {
            if (!AssetDatabase.IsValidFolder(KitFolder)) return;
            AssetDatabase.ImportAsset(KitFolder, ImportAssetOptions.ImportRecursive);
            RemakeMaterials.EnsureModelMaterials(KitFolder);
        }

        [MenuItem(ToolMenu.Art + "Remake: Undo Kit Props in Open Map", priority = ToolMenu.ArtP + 104)]
        public static void UndoOpenMenu() => Debug.Log($"[KitSwap] restored {Undo(SceneManager.GetActiveScene())} renderers.");

        [MenuItem(ToolMenu.Art + "Remake: Survey Kit Props in All Maps", priority = ToolMenu.ArtP + 105)]
        public static void SurveyMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Debug.Log(Survey(AvOverhaul.MapScenes));
        }

        /// <summary>Every saved map, one after another (Rebuild All Maps runs this through AvOverhaul).</summary>
        public static void ApplySaved()
        {
            Prepare();
            foreach (string path in AvOverhaul.MapScenes)
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                Debug.Log(Apply(scene));
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        /// <summary>Which props each map would swap, what was turned down and why, and the prop-like names no family
        /// knows yet. Writes Game/Logs/remake/maps/kit_survey.txt.</summary>
        public static string Survey(IEnumerable<string> scenes)
        {
            var text = new StringBuilder();
            var total = new Dictionary<string, int>();
            foreach (string path in scenes)
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var rejects = new Dictionary<string, int>();
                var unknown = new Dictionary<string, int>();
                List<Candidate> found = Find(scene, rejects, unknown);
                text.AppendLine($"== {scene.name}: {found.Count} props");
                foreach (var g in found.GroupBy(c => c.Family.Id).OrderByDescending(g => g.Count()))
                {
                    Vector3 mean = g.Aggregate(Vector3.zero, (a, c) => a + RealSize(c)) / g.Count();
                    string names = string.Join(", ", g.GroupBy(c => Clean(c.Group.name)).OrderByDescending(n => n.Count()).Take(8)
                        .Select(n => $"{n.Key} {n.Count()}"));
                    text.AppendLine($"   {g.Key,-18} {g.Count(),5}   mean {mean.x:0.00} x {mean.y:0.00} x {mean.z:0.00}   [{names}]");
                    total.TryGetValue(g.Key, out int n);
                    total[g.Key] = n + g.Count();
                }
                text.AppendLine("   turned down: " + string.Join(", ", rejects.OrderByDescending(r => r.Value).Take(30).Select(r => $"{r.Key} {r.Value}")));
                text.AppendLine("   unknown: " + string.Join(", ", unknown.OrderByDescending(r => r.Value).Take(60).Select(r => $"{r.Key} {r.Value}")));
            }
            text.Insert(0, "== all maps: " + string.Join(", ", total.OrderByDescending(t => t.Value).Select(t => $"{t.Key} {t.Value}")) + "\n");
            Directory.CreateDirectory(MapInventory.OutFolder);
            File.WriteAllText(Path.Combine(MapInventory.OutFolder, "kit_survey.txt"), text.ToString());
            return text.ToString();
        }

        private static List<Candidate> Find(Scene scene, Dictionary<string, int> rejects, Dictionary<string, int> unknown)
        {
            var found = new List<Candidate>();
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name != RootName && root.name != "_MapSite") Visit(root.transform);
            return found;

            void Visit(Transform t)
            {
                if (!t.gameObject.activeSelf || t.name.StartsWith("RM_Kit_", StringComparison.Ordinal)) return;
                string name = Clean(t.name);
                // The 0.24.9 look-alike stand-ins (dust sheets, packaging, fixed fittings) stay exactly as they are.
                KitFamily family = StandIn.IsMatch(name) ? null : KitFamilies.All.FirstOrDefault(f => f.Matches(name));
                if (family != null)
                {
                    string why = Pure(t, out List<MeshRenderer> renderers);
                    Bounds box = default;
                    if (why == null)
                    {
                        box = ModelFit.LocalBounds(t.gameObject, t);
                        why = family.Sized(Vector3.Scale(box.size, Abs(t.lossyScale)));
                    }
                    if (why == null)
                    {
                        found.Add(new Candidate { Group = t, Family = family, Box = box, Renderers = renderers });
                        return;
                    }
                    Count(rejects, family.Id + ":" + why);
                }
                else if (unknown != null && t.childCount >= 2 && t.childCount <= 40 && Pure(t, out List<MeshRenderer> parts) == null &&
                         parts.Count >= 2 && MaxSide(Vector3.Scale(ModelFit.LocalBounds(t.gameObject, t).size, Abs(t.lossyScale))) < 6f)
                    Count(unknown, name);
                for (int i = 0; i < t.childCount; i++) Visit(t.GetChild(i));
            }
        }

        /// <summary>Null when the object only draws (and collides); otherwise why it can't be swapped.</summary>
        private static string Pure(Transform t, out List<MeshRenderer> renderers)
        {
            renderers = new List<MeshRenderer>();
            foreach (Component c in t.GetComponentsInChildren<Component>(true))
            {
                switch (c)
                {
                    case null: return "missing-script";
                    case Transform or MeshFilter or Collider or SectionProp or WalkThroughObject: continue;
                    case LODGroup: continue;
                    case MeshRenderer r:
                        Mesh mesh = r.GetComponent<MeshFilter>() != null ? r.GetComponent<MeshFilter>().sharedMesh : null;
                        if (mesh == null) continue;
                        string path = AssetDatabase.GetAssetPath(mesh);
                        if (path.Contains("/ThirdParty/PolyHaven/") || path.StartsWith(RemakePaths.Root + "/", StringComparison.Ordinal))
                            return "modelled";
                        renderers.Add(r);
                        continue;
                    default:
                        if (Passive.Contains(c.GetType().Name)) continue;
                        return c.GetType().Name;
                }
            }
            if (renderers.Count == 0) return "empty";
            return renderers.Count > MaxRenderers ? "many" : null;
        }

        /// <summary>"Desk (3)", "Desk_12" and "desk.004" all read "Desk".</summary>
        private static string Clean(string name) =>
            System.Text.RegularExpressions.Regex.Replace(name, @"[\s_\-\.]*(\(\d+\)|\d+)$", "");

        private static Vector3 RealSize(Candidate c) => Vector3.Scale(c.Box.size, Abs(c.Group.lossyScale));
        private static Vector3 Abs(Vector3 v) => new(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        private static float MaxSide(Vector3 v) => Mathf.Max(v.x, Mathf.Max(v.y, v.z));

        private static void Count(Dictionary<string, int> into, string key)
        {
            into.TryGetValue(key, out int n);
            into[key] = n + 1;
        }

        private static KitSwapRecord RecordFor(Scene scene, bool create)
        {
            GameObject root = scene.GetRootGameObjects().FirstOrDefault(g => g.name == RootName);
            if (root == null && !create) return null;
            if (root == null)
            {
                root = new GameObject(RootName);
                SceneManager.MoveGameObjectToScene(root, scene);
            }
            KitSwapRecord record = root.GetComponent<KitSwapRecord>();
            return record != null ? record : root.AddComponent<KitSwapRecord>();
        }

        private static void Destroy(Object o)
        {
            if (o != null) Object.DestroyImmediate(o);
        }
    }
}
