using System.Collections.Generic;
using Abandoned.Equipment;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Points every equipment kind with a remodelled model (Art/Remake/Gear/Gear_&lt;Kind&gt;.fbx, from
    /// Tools/Blender/remake/gear) at it in Data/Equipment/Resources/GearModels, which ToolModels reads.
    /// </summary>
    public static class GearModelBuilder
    {
        public const string AssetPath = "Assets/_Project/Data/Equipment/Resources/GearModels.asset";

        [MenuItem(ToolMenu.Data + "Create Gear Models (remake)", priority = ToolMenu.DataP + 8)]
        public static void Build()
        {
            PolishAssets.EnsureFolder("Assets/_Project/Data/Equipment/Resources");
            RemakeMaterials.EnsureModelMaterials(RemakePaths.Gear);
            var entries = new List<GearModels.Entry>();
            foreach (EquipmentKind kind in System.Enum.GetValues(typeof(EquipmentKind)))
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>($"{RemakePaths.Gear}/Gear_{kind}.fbx");
                if (model != null) entries.Add(new GearModels.Entry(kind, model));
            }
            var asset = AssetDatabase.LoadAssetAtPath<GearModels>(AssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<GearModels>();
                AssetDatabase.CreateAsset(asset, AssetPath);
            }
            asset.EditorSet(entries);
            EditorUtility.SetDirty(asset);
            // The thrown noise maker and the deployed drone wear the same models (their colliders and logic stay).
            Dress(EquipmentContentBuilder.NoiseMakerPrefabPath, EquipmentKind.NoiseMaker, new Vector3(0f, 0f, -.08f), "Visual");
            Dress(ReleaseKit.GearPrefabFolder + "/Drone.prefab", EquipmentKind.Drone, new Vector3(0f, 0f, -.14f), "Body", "Arm", "Rotor");
            AssetDatabase.SaveAssets();
            Debug.Log($"[Remake] gear models: {entries.Count}.");
        }

        /// <summary>Hides the prefab's primitive look (renderers named in `hide`) and adds the remade model, centred.</summary>
        private static void Dress(string prefabPath, EquipmentKind kind, Vector3 offset, params string[] hide)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>($"{RemakePaths.Gear}/Gear_{kind}.fbx");
            if (model == null || AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null) return;
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                Transform old = root.transform.Find("RemakeModel");
                if (old != null) Object.DestroyImmediate(old.gameObject);
                foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
                    if (System.Array.IndexOf(hide, r.name) >= 0) r.enabled = false;
                var holder = new GameObject("RemakeModel").transform;
                holder.SetParent(root.transform, false);
                holder.localPosition = offset;
                var copy = (GameObject)PrefabUtility.InstantiatePrefab(model, holder);
                copy.transform.localPosition = Vector3.zero;
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
