using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Measures imported models and fits instances of them into boxes (loot, props, vehicles).</summary>
    public static class ModelFit
    {
        /// <summary>World bounds of the authored renderer boxes, including model import transforms.</summary>
        public static Bounds RendererBounds(GameObject root) => LocalBounds(root, null);

        public static Vector3 SizeFor(GameObject model, float yaw, float maxSide)
        {
            var probe = new GameObject("Model measurement");
            probe.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Object.Instantiate(model, probe.transform, false);
            try
            {
                Vector3 size = RendererBounds(probe).size;
                float largest = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
                return largest > 1e-5f ? size * (maxSide / largest) : Vector3.one * maxSide;
            }
            finally { Object.DestroyImmediate(probe); }
        }

        /// <summary>A neutral holder carries placement and fitting; its model retains FBX unit and axis corrections.</summary>
        public static GameObject Place(GameObject model, Transform parent, Vector3 boxCentre, Vector3 boxSize, float yaw, bool standOnFloor = false)
        {
            var instance = new GameObject("Model");
            instance.transform.SetParent(parent, false);
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            GameObject imported = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(model);
            imported.transform.SetParent(instance.transform, false);
            foreach (Collider collider in imported.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(collider);

            Bounds bounds = LocalBounds(instance, parent);
            Vector3 size = bounds.size;
            float scale = Mathf.Min(boxSize.x / Mathf.Max(size.x, 1e-5f), Mathf.Min(boxSize.y / Mathf.Max(size.y, 1e-5f), boxSize.z / Mathf.Max(size.z, 1e-5f)));
            instance.transform.localScale = Vector3.one * scale;
            bounds = LocalBounds(instance, parent);
            Vector3 target = standOnFloor ? boxCentre - Vector3.up * (boxSize.y * 0.5f - bounds.extents.y) : boxCentre;
            instance.transform.localPosition += target - bounds.center;
            return instance;
        }

        /// <summary>Transforms each renderer's eight local bound corners directly into the requested space.</summary>
        public static Bounds LocalBounds(GameObject instance, Transform parent)
        {
            Matrix4x4 toParent = parent != null ? parent.worldToLocalMatrix : Matrix4x4.identity;
            Bounds result = new Bounds(toParent.MultiplyPoint3x4(instance.transform.position), Vector3.zero);
            bool hasPoint = false;
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>())
            {
                Bounds local = renderer.localBounds;
                Matrix4x4 transform = toParent * renderer.localToWorldMatrix;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 offset = new Vector3((corner & 1) == 0 ? -local.extents.x : local.extents.x,
                        (corner & 2) == 0 ? -local.extents.y : local.extents.y,
                        (corner & 4) == 0 ? -local.extents.z : local.extents.z);
                    Vector3 point = transform.MultiplyPoint3x4(local.center + offset);
                    if (!hasPoint) { result = new Bounds(point, Vector3.zero); hasPoint = true; }
                    else result.Encapsulate(point);
                }
            }
            return result;
        }
    }
}
