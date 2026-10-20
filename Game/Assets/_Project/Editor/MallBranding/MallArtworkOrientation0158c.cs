using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Abandoned.EditorTools
{
    public static class MallArtworkOrientation0158c
    {
        [MenuItem(ToolMenu.MallUpdates + "0.15.8c Fix Artwork Orientation", priority = ToolMenu.MallP + 27)]
        public static void Author()
        {
            var scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            foreach (string surface in new[] { "FasciaPrint", "WindowPaper", "HangingFabric" })
                MallRetailArtworkMesh0158.Surface(surface);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform artwork = root.transform.Find(MallRetailArtwork0158.GroupName);
                if (artwork == null) continue;
                foreach (MeshFilter print in artwork.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (!print.name.EndsWith("_Print") && !print.name.EndsWith("_Reverse")) continue;
                    bool reverse = print.name.EndsWith("_Reverse");
                    print.transform.localRotation = reverse ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
                    Vector3 size = print.transform.localScale;
                    print.transform.localScale = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
                }
            }
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException("Could not save corrected artwork orientation.");
            Debug.Log("[Mall0158c] Saved readable front/back artwork UVs and outward-facing prints as 0.15.8c.EEA.");
        }

        public static void AuthorAndExit()
        {
            try { Author(); EditorApplication.Exit(0); }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }
    }
}
