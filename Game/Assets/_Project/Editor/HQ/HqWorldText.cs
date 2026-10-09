using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Keep HQ lettering out of the environment while retaining interaction UI.</summary>
    public static class HqWorldText
    {
        public static void Remove(Transform root)
        {
            // Apply after every dressing builder so rebuilding cannot bring the unwanted labels back.
            foreach (TextMesh label in root.GetComponentsInChildren<TextMesh>(true))
                Object.DestroyImmediate(label.gameObject);
        }
    }
}
