using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Mutually exclusive authored surface variants, separate from the section's authoritative collider.</summary>
    public class StructureStageVisuals : MonoBehaviour
    {
        [SerializeField] private Transform stable;
        [OptionalReference, SerializeField] private Transform cracking;
        [OptionalReference, SerializeField] private Transform failing;
        [OptionalReference, SerializeField] private Transform stressedOverlay;

        public bool HasFailing => failing != null;
        public Transform Stable => stable;
        public Transform Cracking => cracking;
        public Transform Failing => failing;

        public void Show(StructuralStage stage)
        {
            Transform selected = stage switch
            {
                StructuralStage.Collapsed => null,
                StructuralStage.Failing => failing != null ? failing : cracking != null ? cracking : stable,
                StructuralStage.Cracking => cracking != null ? cracking : stable,
                _ => stable,
            };
            Set(stable, selected == stable && selected != null);
            Set(cracking, selected == cracking && selected != null);
            Set(failing, selected == failing && selected != null);
            Set(stressedOverlay, stage == StructuralStage.Stressed);
        }

        private static void Set(Transform group, bool visible)
        {
            if (group != null && group.gameObject.activeSelf != visible) group.gameObject.SetActive(visible);
        }

        public void RestoreLods()
        {
            // A collapsed group must return to distance-based selection, never display all exported LODs.
            foreach (LODGroup lod in GetComponentsInChildren<LODGroup>(true)) lod.ForceLOD(-1);
        }

#if UNITY_EDITOR
        public void EditorSetup(Transform stableGroup, Transform crackingGroup, Transform failingGroup, Transform stressedGroup = null)
        {
            stable = stableGroup; cracking = crackingGroup; failing = failingGroup; stressedOverlay = stressedGroup;
            Show(StructuralStage.Stable);
        }
#endif
    }
}
