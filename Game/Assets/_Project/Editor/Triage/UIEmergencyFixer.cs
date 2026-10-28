using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Emergency repair for uGUI canvases in the loaded scenes: screen canvases scale from 1920x1080, graphics that take no
    /// input stop catching raycasts (so an overlay can't swallow clicks meant for the buttons under it), and a scene keeps
    /// one EventSystem. Undoable; save the scenes to keep it. This game's own menus and HUD are UI Toolkit, which this
    /// does not touch.
    /// </summary>
    public static class UIEmergencyFixer
    {
        [MenuItem(ToolMenu.Fixes + "Emergency Triage: Auto-Fix All UI Canvases", priority = ToolMenu.ProjectP + 23)]
        public static void FixAll()
        {
            var objects = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                    objects.AddRange(root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject));
            }
            int scalers = Scalers(objects), graphics = Raycasts(objects), systems = EventSystems();
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isLoaded) EditorSceneManager.MarkSceneDirty(SceneManager.GetSceneAt(i));
            Debug.Log($"[Triage] UI fixed: {scalers} canvas scaler(s) standardised, raycast target off on {graphics} graphic(s), " +
                      $"{systems} duplicate EventSystem(s) removed. Save the scenes to keep it.");
        }

        private static int Scalers(List<GameObject> objects)
        {
            int changed = 0;
            foreach (GameObject go in objects)
            {
                if (!go.TryGetComponent(out CanvasScaler scaler) || !go.TryGetComponent(out Canvas canvas) || canvas.renderMode == RenderMode.WorldSpace) continue;
                Undo.RecordObject(scaler, "Standardise canvas scaler");
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                EditorUtility.SetDirty(scaler);
                changed++;
            }
            return changed;
        }

        private static int Raycasts(List<GameObject> objects)
        {
            HashSet<Graphic> needed = UiRaycastRules.Needed(objects);
            int changed = 0;
            foreach (GameObject go in objects)
            foreach (Graphic graphic in go.GetComponents<Graphic>())
            {
                if (!graphic.raycastTarget || UiRaycastRules.IsInteractive(graphic, needed)) continue;
                Undo.RecordObject(graphic, "Raycast target off");
                graphic.raycastTarget = false;
                EditorUtility.SetDirty(graphic);
                changed++;
            }
            return changed;
        }

        // Only the active scene: a second EventSystem in an additively loaded scene may be that scene's on purpose.
        private static int EventSystems()
        {
            Scene active = SceneManager.GetActiveScene();
            EventSystem[] systems = active.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<EventSystem>(true))
                .OrderByDescending(e => e.isActiveAndEnabled).ToArray();
            for (int i = 1; i < systems.Length; i++)
            {
                Debug.Log($"[Triage] Removed duplicate EventSystem on {TriageReport.PathOf(systems[i].transform)}", systems[i].gameObject);
                // The module beside it is useless without it, and Unity refuses to remove the EventSystem while one needs it.
                foreach (BaseInputModule module in systems[i].GetComponents<BaseInputModule>()) Undo.DestroyObjectImmediate(module);
                Undo.DestroyObjectImmediate(systems[i]);
            }
            return Mathf.Max(0, systems.Length - 1);
        }
    }
}
