using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.9: no loot look-alikes. Every static copy of a loot model in the saved mall becomes a fixed fitting,
    /// a dust sheet or empty packaging, in place: nothing else moves, colliders are kept, the NavMesh is not
    /// rebaked (a rebake now would bake the 0.16.0 doors and lockers in). Details: Docs/LOOT_LOOKALIKES_0.24.9.md.
    /// </summary>
    public static class Release0249
    {
        [MenuItem(ToolMenu.MallUpdates + "0.24.9 Replace Loot Look-alikes", priority = ToolMenu.MallP + 42)]
        public static void Author()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Replace the look-alikes in Edit mode.");
            foreach (string fixture in LootLookalikes.FixtureIds) SalvageAssetBuilder.AuthorScenery(fixture);
            AssetDatabase.SaveAssets();

            Scene scene = SceneManager.GetSceneByPath(MallBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            Transform mall = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Mall")?.transform;
            if (mall == null) throw new InvalidOperationException("The saved Mall root is missing.");

            LootLookalikeStandIns.TakeSpots();
            LootLookalikePass.Result result = LootLookalikePass.Apply(mall);
            foreach (string missed in result.Missed)
                Debug.LogWarning($"[Lookalikes0249] Batch geometry not found, the old prop may still show: {missed}.");
            // New renderers need their storey's light layer and the flat-surface shadow rule like everything else.
            MeridianLightLayers.Apply(mall);
            ShadowCasterPass.Apply(mall, "Mall");
            int left = LootLookalikeAudit.Run(mall, "Mall");
            LootLookalikeAudit.Report(mall, LootLookalikeStandIns.TakeSpots(), left);

            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save the mall.");
            Debug.Log($"[Lookalikes0249] Renamed {result.Renamed} copied colliders. Swapped {result.Instances} placed and {result.Holders} batched look-alikes " +
                $"({result.TrianglesCut} batch triangles cut, {result.Missed.Count} batch(es) unmatched); {left} left. Version {BuildScript.Version}.");
            if (left > 0) throw new InvalidOperationException($"{left} loot look-alike(s) are still in the mall; see [MallValidation] above.");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}
