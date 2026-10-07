using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Set dressing (a shelf, a sofa) standing on a structural section: it goes when its floor
    /// collapses (the debris covers the moment) and comes back when the section is restored (next run).
    /// Props are cosmetic: no load, no network state; every machine hides its own copy from the
    /// replicated collapse.
    /// </summary>
    public class SectionProp : MonoBehaviour
    {
        [SerializeField] private StructuralSection section;

        private Renderer[] renderers;
        private Collider[] colliders;

        public StructuralSection Section => section;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            colliders = GetComponentsInChildren<Collider>(true);
        }

        private void OnEnable()
        {
            if (section == null) return;
            section.Collapsed += OnCollapsed;
            section.Restored += OnRestored;
            Show(!section.IsCollapsed);
        }

        private void OnDisable()
        {
            if (section == null) return;
            section.Collapsed -= OnCollapsed;
            section.Restored -= OnRestored;
        }

        private void OnCollapsed(StructuralSection s) => Show(false);

        private void OnRestored(StructuralSection s) => Show(true);

        private void Show(bool visible)
        {
            foreach (Renderer r in renderers) r.enabled = visible;
            foreach (Collider c in colliders) c.enabled = visible;
        }

#if UNITY_EDITOR
        public void EditorSetup(StructuralSection standsOn) => section = standsOn;
#endif
    }
}
