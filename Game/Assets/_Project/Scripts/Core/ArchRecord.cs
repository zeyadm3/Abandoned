using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.Core
{
    /// <summary>
    /// Kept on a level's "_Arch" object (Editor/Remake/Arch, remodel v2): the box renderers the architecture rebuild took
    /// off a wall, slab, column or step when it drew modelled construction in its place, and the drawings it added. The
    /// box keeps its collider and every other component, so layout, NavMesh and occlusion are untouched. Running the pass
    /// again puts everything back first, so it can always be redone or undone. Does nothing at run time.
    /// </summary>
    public class ArchRecord : MonoBehaviour
    {
        [System.Serializable]
        public class Removed
        {
            public GameObject owner;
            public Mesh mesh;
            public Material[] materials;
            public uint renderingLayers;
            public ShadowCastingMode shadows;
            public bool receiveShadows;
        }

        [SerializeField] private List<Removed> removed = new();
        [SerializeField] private List<GameObject> added = new();

        public IReadOnlyList<Removed> RemovedRenderers => removed;
        public IReadOnlyList<GameObject> Added => added;

#if UNITY_EDITOR
        public void EditorRemoved(Removed entry) => removed.Add(entry);
        public void EditorAdded(GameObject drawing) => added.Add(drawing);

        public void EditorClear()
        {
            removed.Clear();
            added.Clear();
        }
#endif
    }
}
