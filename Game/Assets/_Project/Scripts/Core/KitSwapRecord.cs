using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.Core
{
    /// <summary>
    /// Kept on a level's "_KitSwap" object (Editor/Remake/KitSwap, 2026 remake): the renderers the map remake took off
    /// a code-built prop (a desk of boxes) when it stood a modelled one in its place, and what it added. Colliders and
    /// every other component stay where they were. Running the pass again puts everything back first, so it can always
    /// be redone or undone. Does nothing at run time.
    /// </summary>
    public class KitSwapRecord : MonoBehaviour
    {
        [System.Serializable]
        public class Removed
        {
            public GameObject owner;
            public Mesh mesh;
            public Material[] materials;
            public uint renderingLayers;
            public ShadowCastingMode shadows;
        }

        /// <summary>A LOD group whose renderers were taken off: its levels, to be rebuilt on undo.</summary>
        [System.Serializable]
        public class Levels
        {
            public LODGroup group;
            public float[] heights;
            public GameObject[] owners;
            public int[] level;
        }

        [SerializeField] private List<Removed> removed = new();
        [SerializeField] private List<GameObject> added = new();
        [SerializeField] private List<Levels> silenced = new();

        public IReadOnlyList<Removed> RemovedRenderers => removed;
        public IReadOnlyList<GameObject> Added => added;
        public IReadOnlyList<Levels> Silenced => silenced;

#if UNITY_EDITOR
        public void EditorRemoved(Removed entry) => removed.Add(entry);
        public void EditorAdded(GameObject kit) => added.Add(kit);
        public void EditorSilenced(Levels levels) => silenced.Add(levels);

        public void EditorClear()
        {
            removed.Clear();
            added.Clear();
            silenced.Clear();
        }
#endif
    }
}
