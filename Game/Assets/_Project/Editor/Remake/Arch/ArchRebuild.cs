using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Abandoned.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Role = Abandoned.EditorTools.Remake.ArchBox.Role;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Remodel v2 (Docs/remodelv2.md): every map's architecture rebuilt as construction, layouts kept. Each wall, slab,
    /// ceiling, column, beam, step, pane and roof sheet a builder drew as a box is drawn again by a modelled mesh fitted to
    /// that box (<see cref="ArchWall"/>, <see cref="ArchSlab"/>, <see cref="ArchColumn"/>), in the map's style
    /// (<see cref="ArchStyles"/>). The box keeps its collider and every component, so layout, NavMesh, occlusion, collapse
    /// and gameplay are untouched; only its renderer is taken off (recorded in <see cref="ArchRecord"/>), and the drawing is a
    /// child of the box, so it hides with its floor. Runs in <see cref="AvOverhaul.AfterMaps"/> right after the kit props,
    /// before the surfaces and the coplanar fix. Meshes go to one bank per map (Art/Remake/Maps/Arch/&lt;Map&gt;).
    /// </summary>
    public static class ArchRebuild
    {
        private const string RootName = "_Arch";
        public const string DrawingName = "RM_Arch";
        public const string LogFolder = "Logs/remake/arch";
        private const int MaxQuads = 4000;

        [MenuItem(ToolMenu.Art + "Remake: Rebuild Architecture in Open Map", priority = ToolMenu.ArtP + 107)]
        public static void ApplyOpenMenu() => Debug.Log(Apply(SceneManager.GetActiveScene()));

        [MenuItem(ToolMenu.Art + "Remake: Undo Architecture in Open Map", priority = ToolMenu.ArtP + 108)]
        public static void UndoOpenMenu() => Debug.Log($"[Arch] restored {Undo(SceneManager.GetActiveScene())} box renderers.");

        [MenuItem(ToolMenu.Art + "Remake: Rebuild Architecture in All Maps", priority = ToolMenu.ArtP + 109)]
        public static void ApplyAllMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ApplySaved();
        }

        /// <summary>Every saved map, one after another (Rebuild All Maps runs this through AvOverhaul).</summary>
        public static void ApplySaved()
        {
            foreach (string path in AvOverhaul.MapScenes) Debug.Log(ApplyScene(path));
        }

        /// <summary>One saved map: open, rebuild, save. For one-map-per-call MCP runs.</summary>
        public static string ApplyScene(string path)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            string result = Apply(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            return result;
        }

        public static string Apply(Scene scene)
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            int restored = Undo(scene);
            ArchStyle style = ArchStyles.For(scene.name);
            if (style == null) return $"[Arch] {scene.name}: no style, left as it is.";
            System.Func<string, Material> finishes = ArchMaterials.For(scene, style);
            var skipped = new Dictionary<string, int>();
            List<ArchBox> boxes = ArchClassify.Scan(scene, style, out List<ArchBox> solids, skipped);
            var space = new ArchSpace(solids);
            ArchRecord record = RecordFor(scene, true);
            var bank = new ArchMeshBank(scene.name);

            var built = new Dictionary<Role, int>();
            int failed = 0, quads = 0, heavy = 0;
            var errors = new StringBuilder();
            foreach (ArchBox box in boxes)
            {
                if (style.Bodies.TryGetValue(box.Surface, out string body) && finishes(body) != null) box.Material = finishes(body);
                try
                {
                    var mesh = new ArchMesh();
                    var ctx = new ArchContext { Box = box, Style = style, Space = space, Finishes = finishes, Mesh = mesh };
                    switch (box.Kind)
                    {
                        case Role.Wall: ArchWall.Build(ctx); break;
                        case Role.Slab:
                        case Role.Step: ArchSlab.Build(ctx); break;
                        case Role.Roof: ArchSlab.Roof(ctx); break;
                        case Role.Column: ArchColumn.Column(ctx); break;
                        case Role.Beam: ArchColumn.Beam(ctx); break;
                        case Role.Pane: ArchColumn.Pane(ctx); break;
                    }
                    if (mesh.Empty) continue;
                    // A pathological box (a 200 m fin with joints every few centimetres) stays a plain box: never a render cost.
                    if (mesh.Quads > MaxQuads)
                    {
                        mesh = new ArchMesh();
                        mesh.Block(Vector3.zero, box.Size, box.Material);
                        heavy++;
                    }
                    mesh.Lead(box.Material);
                    Mesh built0 = mesh.Build($"{box.Transform.name}_{box.Kind}", out Material[] materials);
                    Mesh shared = bank.Add(built0);
                    Stand(box, shared, materials, record);
                    built.TryGetValue(box.Kind, out int n);
                    built[box.Kind] = n + 1;
                    quads += mesh.Quads;
                }
                catch (System.Exception e)
                {
                    failed++;
                    if (failed <= 20) errors.AppendLine($"  {Path(box.Transform)}: {e.Message}");
                }
            }
            bank.Save();
            EditorUtility.SetDirty(record);
            string summary = $"[Arch] {scene.name}: rebuilt {built.Values.Sum()} of {boxes.Count} boxes " +
                $"({string.Join(", ", built.OrderBy(k => k.Key).Select(k => $"{k.Key} {k.Value}"))}), {bank.Count} meshes, " +
                $"{quads} quads, {heavy} kept plain (over {MaxQuads} quads), {failed} failed, {restored} restored first, {clock.Elapsed.TotalSeconds:0}s. Left alone: " +
                string.Join(", ", skipped.OrderByDescending(s => s.Value).Select(s => $"{s.Key} {s.Value}"));
            Directory.CreateDirectory(LogFolder);
            File.WriteAllText($"{LogFolder}/{scene.name}_rebuild.txt", summary + "\n" + errors);
            return summary;
        }

        /// <summary>Stands the drawing in its box (a child with the box's scale undone, so the mesh reads in metres) and takes
        /// the box's renderer off, keeping a record of it.</summary>
        private static void Stand(ArchBox box, Mesh mesh, Material[] materials, ArchRecord record)
        {
            Transform t = box.Transform;
            MeshRenderer old = box.Renderer;
            MeshFilter filter = t.GetComponent<MeshFilter>();
            var drawing = new GameObject(DrawingName);
            drawing.layer = t.gameObject.layer;
            drawing.transform.SetParent(t, false);
            drawing.transform.localPosition = filter.sharedMesh.bounds.center;
            drawing.transform.localRotation = Quaternion.identity;
            Vector3 s = t.lossyScale;
            drawing.transform.localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z);
            drawing.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer r = drawing.AddComponent<MeshRenderer>();
            r.sharedMaterials = materials;
            r.renderingLayerMask = old.renderingLayerMask;
            // Columns and beams are the shadow shapes a room reads by; glass never casts; the rest keep the map's choice.
            r.shadowCastingMode = box.Kind switch
            {
                Role.Column or Role.Beam => UnityEngine.Rendering.ShadowCastingMode.On,
                Role.Pane => UnityEngine.Rendering.ShadowCastingMode.Off,
                _ => old.shadowCastingMode,
            };
            r.receiveShadows = old.receiveShadows;
            r.lightProbeUsage = old.lightProbeUsage;
            r.reflectionProbeUsage = old.reflectionProbeUsage;
            r.staticShadowCaster = old.staticShadowCaster;
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(t.gameObject);
            GameObjectUtility.SetStaticEditorFlags(drawing, flags & ~(StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic));
            record.EditorRemoved(new ArchRecord.Removed
            {
                owner = t.gameObject, mesh = filter.sharedMesh, materials = old.sharedMaterials, renderingLayers = old.renderingLayerMask,
                shadows = old.shadowCastingMode, receiveShadows = old.receiveShadows,
            });
            record.EditorAdded(drawing);
            Object.DestroyImmediate(old);
        }

        /// <summary>Puts every box renderer back and removes the drawings. Returns how many renderers came back.</summary>
        public static int Undo(Scene scene)
        {
            ArchRecord record = RecordFor(scene, false);
            if (record == null) return 0;
            foreach (GameObject drawing in record.Added)
                if (drawing != null) Object.DestroyImmediate(drawing);
            int restored = 0;
            foreach (ArchRecord.Removed r in record.RemovedRenderers)
            {
                if (r.owner == null) continue;
                // Not ??: a missing component is Unity's fake null, not a C# null.
                MeshFilter filter = r.owner.GetComponent<MeshFilter>();
                if (filter == null) filter = r.owner.AddComponent<MeshFilter>();
                filter.sharedMesh = r.mesh;
                MeshRenderer renderer = r.owner.GetComponent<MeshRenderer>();
                if (renderer == null) renderer = r.owner.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = r.materials;
                renderer.renderingLayerMask = r.renderingLayers;
                renderer.shadowCastingMode = r.shadows;
                renderer.receiveShadows = r.receiveShadows;
                restored++;
            }
            record.EditorClear();
            EditorUtility.SetDirty(record);
            return restored;
        }

        private static ArchRecord RecordFor(Scene scene, bool create)
        {
            GameObject root = scene.GetRootGameObjects().FirstOrDefault(g => g.name == RootName);
            if (root == null && !create) return null;
            if (root == null)
            {
                root = new GameObject(RootName);
                SceneManager.MoveGameObjectToScene(root, scene);
            }
            ArchRecord record = root.GetComponent<ArchRecord>();
            return record != null ? record : root.AddComponent<ArchRecord>();
        }

        private static string Path(Transform t)
        {
            string p = t.name;
            for (Transform a = t.parent; a != null; a = a.parent) p = a.name + "/" + p;
            return p;
        }
    }
}
