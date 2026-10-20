using System;
using Abandoned.Tutorial;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The training clipboard on the HQ's dispatch bench, beside the field guide: E (the host) takes the crew to
    /// the practice house. The HQ builder places it on every rebuild; the menu adds it to the saved HQ in place,
    /// leaving everything else in the scene (the user's own HQ edits included) as it is.
    /// </summary>
    public static class TutorialHqClipboard
    {
        public const string Name = "TrainingClipboard";

        [MenuItem(ToolMenu.Tutorial + "Add Training Clipboard to HQ", priority = ToolMenu.TutorialP + 1)]
        public static void AddToSavedHq()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Add the clipboard in Edit mode.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.OpenScene(HqBuilder.ScenePath, OpenSceneMode.Single);
            Transform desk = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                desk = Find(root.transform, "DispatchWorkbench");
                if (desk != null) break;
            }
            if (desk == null) throw new InvalidOperationException("The HQ has no DispatchWorkbench: rebuild the HQ, which places the clipboard itself.");
            Place(desk);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Unity could not save the HQ.");
            Debug.Log("[Tutorial] Training clipboard placed on the HQ dispatch bench.");
        }

        /// <summary>Batch mode: Tools/unity.sh exec Abandoned.EditorTools.TutorialHqClipboard.AddAndExit</summary>
        public static void AddAndExit()
        {
            try
            {
                AddToSavedHq();
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        /// <summary>On the dispatch bench (its local space: the phone at x -0.65, the field guide at +0.62).</summary>
        public static void Place(Transform desk)
        {
            Transform old = desk.Find(Name);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Material board = PolishAssets.Material("HQ_ClipboardMasonite", new Color(.42f, .30f, .19f), .8f);
            Material paper = PolishAssets.Material("HQ_ClipboardPaper", new Color(.86f, .84f, .76f), .9f);
            Material clip = PolishAssets.Material("HQ_ClipboardClip", new Color(.62f, .63f, .62f), .3f);
            Material ink = PolishAssets.Material("HQ_ClipboardInk", new Color(.12f, .14f, .2f), .9f);
            Transform c = Group(Name, desk);
            c.SetLocalPositionAndRotation(new Vector3(1.38f, 1.125f, -.3f), Quaternion.Euler(0f, 14f, 0f));
            Box("Board", c, new Vector3(0f, .006f, 0f), new Vector3(.24f, .012f, .33f), board, false);
            for (int i = 0; i < 3; i++)
                Box("Sheet", c, new Vector3(i * .004f, .014f + i * .002f, -.005f), new Vector3(.21f, .002f, .28f), paper, false);
            Box("Clip", c, new Vector3(0f, .024f, .145f), new Vector3(.1f, .02f, .04f), clip, false);
            // A pencilled floor plan on the top sheet: the house, drawn in four lines and a door.
            Box("PlanNorth", c, new Vector3(0f, .0215f, .07f), new Vector3(.15f, .001f, .006f), ink, false);
            Box("PlanSouth", c, new Vector3(0f, .0215f, -.08f), new Vector3(.15f, .001f, .006f), ink, false);
            Box("PlanWest", c, new Vector3(-.075f, .0215f, -.005f), new Vector3(.006f, .001f, .15f), ink, false);
            Box("PlanEast", c, new Vector3(.075f, .0215f, -.005f), new Vector3(.006f, .001f, .15f), ink, false);
            Box("PlanHall", c, new Vector3(-.03f, .0215f, -.005f), new Vector3(.004f, .001f, .1f), ink, false);
            var hit = c.gameObject.AddComponent<BoxCollider>();
            hit.center = new Vector3(0f, .05f, 0f);
            hit.size = new Vector3(.32f, .12f, .4f);
            c.gameObject.AddComponent<TrainingClipboard>();
        }

        private static Transform Find(Transform at, string name)
        {
            if (at.name == name) return at;
            foreach (Transform child in at)
                if (Find(child, name) is Transform found) return found;
            return null;
        }
    }
}
