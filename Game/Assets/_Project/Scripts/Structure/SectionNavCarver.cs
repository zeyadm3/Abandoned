using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Structure
{
    /// <summary>
    /// Keeps monsters' paths honest without rebaking the NavMesh (decision log, M5): the level's mesh is
    /// baked once with every floor intact, and each collapsible section owns a carving obstacle over its
    /// footprint that switches on when it collapses (a hole) and off when it's restored (next run).
    /// </summary>
    [RequireComponent(typeof(StructuralSection), typeof(NavMeshObstacle))]
    public class SectionNavCarver : MonoBehaviour
    {
        private StructuralSection section;
        private NavMeshObstacle obstacle;

        private void Awake()
        {
            section = GetComponent<StructuralSection>();
            obstacle = GetComponent<NavMeshObstacle>();
            obstacle.carving = true;
            obstacle.carveOnlyStationary = false;
            obstacle.enabled = section.IsCollapsed;
        }

        private void OnEnable()
        {
            section.Collapsed += OnCollapsed;
            section.Restored += OnRestored;
        }

        private void OnDisable()
        {
            section.Collapsed -= OnCollapsed;
            section.Restored -= OnRestored;
        }

        private void OnCollapsed(StructuralSection s) => obstacle.enabled = true;

        private void OnRestored(StructuralSection s) => obstacle.enabled = false;

#if UNITY_EDITOR
        /// <summary>Sizes the obstacle to the tile: walking surface at the pivot, a little thicker than the slab.</summary>
        public static void EditorAdd(GameObject piece, Vector3 size)
        {
            var o = piece.AddComponent<NavMeshObstacle>();
            o.shape = NavMeshObstacleShape.Box;
            o.size = new Vector3(size.x * 0.95f, 1f, size.z * 0.95f);
            o.center = new Vector3(0f, 0.2f, 0f);
            o.carving = true;
            o.enabled = false;
            piece.AddComponent<SectionNavCarver>();
        }
#endif
    }
}
