using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// A named part of the training house (a room, the porch, the basement) that lessons check: "someone is in
    /// the kitchen", "the clock is in the kitchen". A box in its own space; no collider, nothing networked.
    /// </summary>
    public class TutorialArea : MonoBehaviour
    {
        private static readonly List<TutorialArea> Live = new();

        [SerializeField] private string key = "area";
        [SerializeField] private Vector3 size = new(4f, 3f, 4f);

        public static IReadOnlyList<TutorialArea> All => Live;
        public string Key => key;
        /// <summary>The box's size in its own space (the guide outlines it on the floor).</summary>
        public Vector3 Size => size;

        public bool Contains(Vector3 world)
        {
            Vector3 local = transform.InverseTransformPoint(world);
            Vector3 half = size * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }

        public static TutorialArea Find(string areaKey)
        {
            foreach (TutorialArea a in Live) if (a != null && a.key == areaKey) return a;
            return null;
        }

        private void OnEnable() => Live.Add(this);
        private void OnDisable() => Live.Remove(this);

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && !DebugView.Visible) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.35f);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

#if UNITY_EDITOR
        public void EditorSetup(string areaKey, Vector3 boxSize)
        {
            key = areaKey;
            size = boxSize;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}
