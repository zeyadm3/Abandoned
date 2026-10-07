using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Kenney Car Kit vehicles (M7.1): dresses a greybox box (which keeps its collider and any
    /// gameplay component) with a model, or parks a plain vehicle as scenery.
    /// </summary>
    public static class Vehicles
    {
        public const string Folder = ThirdPartyModelImport.Root + "Kenney/CarKit/";
        public const string Van = "van", Delivery = "delivery", Truck = "truck";

        /// <summary>Hides the box's greybox look and shows the model fitted to it (Kenney cars face +Z).</summary>
        public static GameObject Dress(GameObject box, string model)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder}{model}.fbx");
            if (asset == null)
            {
                Debug.LogWarning($"[Art] No vehicle model {model}.");
                return null;
            }
            Object.DestroyImmediate(box.GetComponent<MeshRenderer>());
            Object.DestroyImmediate(box.GetComponent<MeshFilter>());
            Transform t = box.transform;
            // The box is scaled to its size; the model goes beside it so it isn't stretched with it.
            var holder = new GameObject($"{box.name}_Model").transform;
            holder.SetParent(t.parent, false);
            holder.SetPositionAndRotation(t.position, t.rotation);
            return ModelFit.Place(asset, holder, Vector3.zero, t.lossyScale, 0f, standOnFloor: true);
        }

        /// <summary>A parked vehicle (scenery with a box collider), standing on the ground at a position.</summary>
        public static GameObject Park(Transform parent, string model, Vector3 position, float yaw, Vector3 size)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = $"Parked_{model}";
            box.transform.SetParent(parent, false);
            box.transform.SetPositionAndRotation(position + Vector3.up * size.y / 2f, Quaternion.Euler(0f, yaw, 0f));
            box.transform.localScale = size;
            Dress(box, model);
            GameObjectUtility.SetStaticEditorFlags(box, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
            return box;
        }
    }
}
