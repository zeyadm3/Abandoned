using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Measures imported models and fits instances of them into boxes (loot, props, vehicles).</summary>
    public static class ModelFit
    {
        /// <summary>World bounds of every renderer under a root (empty bounds at its position when none).</summary>
        public static Bounds RendererBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(root.transform.position, Vector3.zero);
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }

        /// <summary>The model's size when turned by yaw and scaled so its largest side is maxSide.</summary>
        public static Vector3 SizeFor(GameObject model, float yaw, float maxSide)
        {
            GameObject probe = Object.Instantiate(model, Vector3.zero, Quaternion.Euler(0f, yaw, 0f));
            try
            {
                Vector3 size = RendererBounds(probe).size;
                float largest = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
                return largest > 1e-5f ? size * (maxSide / largest) : Vector3.one * maxSide;
            }
            finally
            {
                Object.DestroyImmediate(probe);
            }
        }

        /// <summary>
        /// Adds an instance of the model under parent, turned by yaw and uniformly scaled to fit inside
        /// a box of the given size; centred on the box's centre (local), or standing on its floor.
        /// </summary>
        public static GameObject Place(GameObject model, Transform parent, Vector3 boxCentre, Vector3 boxSize, float yaw, bool standOnFloor = false)
        {
            var instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(model, parent);
            instance.name = "Model";
            Transform t = instance.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.Euler(0f, yaw, 0f);
            t.localScale = Vector3.one;
            foreach (Collider c in instance.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);

            Bounds b = LocalBounds(instance, parent);
            Vector3 s = b.size;
            float scale = Mathf.Min(boxSize.x / Mathf.Max(s.x, 1e-5f), Mathf.Min(boxSize.y / Mathf.Max(s.y, 1e-5f), boxSize.z / Mathf.Max(s.z, 1e-5f)));
            t.localScale = Vector3.one * scale;
            b = LocalBounds(instance, parent);
            Vector3 target = standOnFloor ? boxCentre - Vector3.up * (boxSize.y * 0.5f - b.extents.y) : boxCentre;
            t.localPosition += target - b.center;
            return instance;
        }

        /// <summary>Renderer bounds in the parent's space (axis-aligned there; fine for quarter-turn yaws).</summary>
        public static Bounds LocalBounds(GameObject instance, Transform parent)
        {
            Bounds world = RendererBounds(instance);
            if (parent == null) return world;
            Vector3 c = parent.InverseTransformPoint(world.center);
            Vector3 s = parent.InverseTransformVector(world.size);
            return new Bounds(c, new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z)));
        }
    }
}
