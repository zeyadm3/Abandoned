using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Primitive and material helpers for Editor-built greybox levels.
    /// </summary>
    public static class GreyboxFactory
    {
        public const string MaterialFolder = "Assets/_Project/Art/Greybox";

        private const string LitShader = "Universal Render Pipeline/Lit";

        private const StaticEditorFlags LevelStatic =
            StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic |
            StaticEditorFlags.OccludeeStatic | StaticEditorFlags.BatchingStatic |
            StaticEditorFlags.ReflectionProbeStatic;

        /// <summary>Loads or creates a URP Lit material asset; re-running updates the colour in place.</summary>
        public static Material GetMaterial(string name, Color color)
        {
            EnsureFolder(MaterialFolder);
            string path = $"{MaterialFolder}/{name}.mat";

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find(LitShader);
                if (shader == null)
                {
                    Debug.LogError($"Shader '{LitShader}' not found; is URP the active pipeline?");
                    return null;
                }
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Transform Group(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        /// <summary>
        /// A scaled cube. Fine for walls and props; for anything that will later carry gameplay
        /// components (tiles, stairs), put this under an unscaled root instead.
        /// </summary>
        public static GameObject Box(string name, Transform parent, Vector3 localCenter, Vector3 size,
            Material material, bool withCollider = true)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localCenter;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (!withCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        public static void MarkStatic(Transform root)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, LevelStatic);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}
