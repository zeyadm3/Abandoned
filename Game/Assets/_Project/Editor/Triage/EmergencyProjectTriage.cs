using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Level = Abandoned.EditorTools.TriageReport.Level;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Emergency triage: reports, never changes, what in the loaded scenes would draw magenta or not at all, warp a
    /// skinned body, stop a monster spawning or swallow UI clicks. Run it with the map scene open.
    /// </summary>
    public static class EmergencyProjectTriage
    {
        private const float NavRadius = 5f;
        // A component whose type name has one of these is treated as a spawner or a monster.
        private static readonly string[] SpawnerWords = { "Spawn", "Monster", "Enemy", "Threat", "Wave" };
        private static readonly string[] MonsterWords = { "Monster", "Enemy", "Threat" };

        [MenuItem(ToolMenu.Checks + "Emergency Triage: Run Full Project Audit", priority = ToolMenu.ChecksP + 40)]
        public static void Run()
        {
            var report = new TriageReport();
            report.Finish(string.Join(", ", AuditLoaded(report)));
        }

        // Adds the loaded scenes' findings to the report and returns their names; the batch runner calls it per scene.
        public static List<string> AuditLoaded(TriageReport report)
        {
            var scenes = new List<string>();
            var objects = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                scenes.Add(scene.name);
                foreach (GameObject root in scene.GetRootGameObjects())
                    objects.AddRange(root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject));
            }
            MissingScripts(objects, report);
            Renderers(objects, report);
            Spawners(objects, report);
            Ui(objects, report);
            return scenes;
        }

        private static string Where(GameObject go) => $"{go.scene.name}:{TriageReport.PathOf(go.transform)}";

        private static void MissingScripts(List<GameObject> objects, TriageReport report)
        {
            report.Section("Missing scripts");
            foreach (GameObject go in objects)
            {
                int missing = go.GetComponents<Component>().Count(c => c == null);
                if (missing > 0) report.Add(Level.P0, $"{missing} missing script(s) on {Where(go)}", go);
            }
        }

        private static void Renderers(List<GameObject> objects, TriageReport report)
        {
            report.Section("Renderers and materials");
            bool srp = GraphicsSettings.currentRenderPipeline != null;
            foreach (GameObject go in objects)
            {
                if (go.TryGetComponent(out MeshRenderer meshRenderer) && meshRenderer.enabled && !DrawsOwnMesh(go))
                {
                    if (!go.TryGetComponent(out MeshFilter filter)) report.Add(Level.P0, $"MeshRenderer without a MeshFilter on {Where(go)}", go);
                    else if (filter.sharedMesh == null) report.Add(Level.P0, $"missing mesh on {Where(go)}", go);
                    Materials(meshRenderer, srp, report);
                }
                if (go.TryGetComponent(out SkinnedMeshRenderer skinned) && skinned.enabled)
                {
                    Skin(skinned, report);
                    Materials(skinned, srp, report);
                }
            }
        }

        // World text (TextMesh, TextMeshPro) builds its mesh itself and has no MeshFilter mesh to check.
        private static bool DrawsOwnMesh(GameObject go) =>
            go.GetComponent<TextMesh>() != null || go.GetComponents<Component>().Any(c => c != null && c.GetType().Name.StartsWith("TextMeshPro"));

        private static void Skin(SkinnedMeshRenderer skinned, TriageReport report)
        {
            GameObject go = skinned.gameObject;
            Mesh mesh = skinned.sharedMesh;
            if (mesh == null) { report.Add(Level.P0, $"skinned mesh missing on {Where(go)}", go); return; }
            if (skinned.rootBone == null) report.Add(Level.P1, $"no root bone on {Where(go)} (bounds follow the renderer, not the body)", go);
            Transform[] bones = skinned.bones;
            int nulls = bones.Count(b => b == null);
            if (nulls > 0) report.Add(Level.P0, $"{nulls} of {bones.Length} bones are missing on {Where(go)} (those vertices collapse)", go);
            // The direct cause of a warped body: vertices are weighted to bone indices that don't exist or mean another joint.
            if (mesh.bindposeCount > 0 && bones.Length != mesh.bindposeCount)
                report.Add(Level.P0, $"{bones.Length} bones for {mesh.bindposeCount} bind poses on {Where(go)} ({mesh.name}): the body will warp", go);
        }

        private static void Materials(Renderer renderer, bool srp, TriageReport report)
        {
            GameObject go = renderer.gameObject;
            foreach (Material m in renderer.sharedMaterials)
            {
                if (m == null) { report.Add(Level.P0, $"empty material slot on {Where(go)} (draws magenta)", go); continue; }
                Shader shader = m.shader;
                if (shader == null || shader.name == "Hidden/InternalErrorShader" || !shader.isSupported)
                    report.Add(Level.P0, $"broken shader on material {m.name} at {Where(go)}", m);
                else if (srp && (shader.name == "Standard" || shader.name == "Standard (Specular setup)" || shader.name.StartsWith("Legacy Shaders/")))
                    report.Add(Level.P0, $"built-in shader {shader.name} on material {m.name} at {Where(go)} under a render pipeline (draws magenta)", m);
            }
        }

        private static void Spawners(List<GameObject> objects, TriageReport report)
        {
            report.Section("Spawners, monsters and spawn points");
            bool navMesh = NavMesh.CalculateTriangulation().vertices.Length > 0;
            if (!navMesh) report.Add(Level.P1, "no NavMesh is loaded, so spawn points were not checked against it (bake or open the map's NavMeshSurface)");
            foreach (GameObject go in objects)
            foreach (MonoBehaviour behaviour in go.GetComponents<MonoBehaviour>())
            {
                if (behaviour == null) continue;
                string type = behaviour.GetType().Name;
                if (!SpawnerWords.Any(type.Contains)) continue;
                References(behaviour, report);
                bool monsterSide = MonsterWords.Any(type.Contains);
                if (navMesh && monsterSide && type.EndsWith("SpawnPoint")) OnNavMesh(go.transform, type, report);
                if (navMesh && monsterSide) SpawnPointFields(behaviour, report);
            }
        }

        private static void References(MonoBehaviour behaviour, TriageReport report)
        {
            var so = new SerializedObject(behaviour);
            SerializedProperty p = so.GetIterator();
            for (bool enter = true; p.NextVisible(enter); enter = true)
            {
                if (p.propertyType != SerializedPropertyType.ObjectReference || p.propertyPath == "m_Script" || p.objectReferenceValue != null) continue;
                if (Optional(behaviour.GetType(), p.propertyPath)) continue;
                // An instance id with no object is a reference to something deleted; zero is a slot nobody filled.
                bool broken = p.objectReferenceInstanceIDValue != 0;
                report.Add(broken ? Level.P0 : Level.P1,
                    $"{(broken ? "broken" : "unassigned")} reference {behaviour.GetType().Name}.{p.propertyPath} on {Where(behaviour.gameObject)}", behaviour);
            }
        }

        // Fields marked [OptionalReference] are allowed to stay empty (Abandoned.Core).
        private static bool Optional(Type type, string propertyPath)
        {
            string name = propertyPath.Split('.')[0];
            for (; type != null && type != typeof(MonoBehaviour); type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetCustomAttribute<OptionalReferenceAttribute>() != null;
            }
            return false;
        }

        private static void SpawnPointFields(MonoBehaviour behaviour, TriageReport report)
        {
            var so = new SerializedObject(behaviour);
            SerializedProperty p = so.GetIterator();
            for (bool enter = true; p.NextVisible(enter); enter = true)
            {
                if (p.propertyType != SerializedPropertyType.ObjectReference || !p.propertyPath.ToLowerInvariant().Contains("spawn")) continue;
                Transform point = p.objectReferenceValue switch
                {
                    Transform t => t,
                    GameObject g => g.transform,
                    Component c => c.transform,
                    _ => null,
                };
                if (point != null && point.gameObject.scene.IsValid()) OnNavMesh(point, $"{behaviour.GetType().Name}.{p.propertyPath}", report);
            }
        }

        private static void OnNavMesh(Transform point, string what, TriageReport report)
        {
            if (NavMesh.SamplePosition(point.position, out NavMeshHit hit, NavRadius, NavMesh.AllAreas))
            {
                if (Vector3.Distance(hit.position, point.position) > 1.75f)
                    report.Add(Level.P2, $"{what} at {Where(point.gameObject)} is {Vector3.Distance(hit.position, point.position):0.0} m from the NavMesh", point);
                return;
            }
            report.Add(Level.P0, $"{what} at {Where(point.gameObject)} {point.position} has no NavMesh within {NavRadius} m: nothing can spawn there", point);
        }

        private static void Ui(List<GameObject> objects, TriageReport report)
        {
            report.Section("UI");
            HashSet<Graphic> needed = UiRaycastRules.Needed(objects);
            foreach (GameObject go in objects)
            foreach (Graphic graphic in go.GetComponents<Graphic>())
            {
                if (!graphic.raycastTarget || UiRaycastRules.IsInteractive(graphic, needed)) continue;
                bool blocks = graphic.isActiveAndEnabled && UiRaycastRules.CoversScreen(graphic);
                report.Add(blocks ? Level.P1 : Level.P2,
                    $"{graphic.GetType().Name} on {Where(go)} is a raycast target but takes no input{(blocks ? " and covers the screen: it blocks clicks below it" : "")}", graphic);
            }

            foreach (IGrouping<Scene, EventSystem> scene in objects.SelectMany(o => o.GetComponents<EventSystem>()).GroupBy(e => e.gameObject.scene))
                if (scene.Count() > 1)
                    report.Add(Level.P0, $"{scene.Count()} EventSystems in {scene.Key.name}: {string.Join(", ", scene.Select(e => TriageReport.PathOf(e.transform)))}");
#if !ENABLE_LEGACY_INPUT_MANAGER
            // With only the Input System active, the legacy module throws every frame and no UI receives input.
            foreach (GameObject go in objects)
                if (go.GetComponent<StandaloneInputModule>() != null)
                    report.Add(Level.P0, $"StandaloneInputModule on {Where(go)} while only the Input System is active: replace it with InputSystemUIInputModule", go);
#endif
            foreach (GameObject go in objects)
            {
                if (!go.TryGetComponent(out CanvasScaler scaler) || !go.TryGetComponent(out Canvas canvas) || canvas.renderMode == RenderMode.WorldSpace) continue;
                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize || scaler.referenceResolution != new Vector2(1920, 1080) ||
                    scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight || !Mathf.Approximately(scaler.matchWidthOrHeight, 0.5f))
                    report.Add(Level.P2, $"CanvasScaler on {Where(go)} is not 1920x1080 scale-with-screen, match 0.5", scaler);
            }

            // This game's menus and HUD are UI Toolkit: their click-through is each element's pickingMode, set in code.
            int documents = objects.Count(o => o.GetComponent<UnityEngine.UIElements.UIDocument>() != null);
            if (documents > 0) report.Add(Level.P2, $"{documents} UI Toolkit document(s): not checked here (picking is per element, PickingMode.Ignore)");
        }
    }
}
