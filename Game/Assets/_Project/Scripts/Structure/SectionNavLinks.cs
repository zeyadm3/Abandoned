using Unity.AI.Navigation;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// A collapsible flight's NavMesh links go with it: while the section is down, monsters can no longer path up a
    /// stair that isn't there (the carver only cuts the baked mesh; a link would still bridge the gap). Restored with
    /// the section on the next run. Every machine, but only the host's monsters care.
    /// </summary>
    [RequireComponent(typeof(StructuralSection))]
    public class SectionNavLinks : MonoBehaviour
    {
        private StructuralSection section;
        private NavMeshLink[] links;

        private void Awake()
        {
            section = GetComponent<StructuralSection>();
            links = GetComponentsInChildren<NavMeshLink>(true);
            Apply(section.IsCollapsed);
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

        private void OnCollapsed(StructuralSection s) => Apply(true);

        private void OnRestored(StructuralSection s) => Apply(false);

        private void Apply(bool down)
        {
            if (links == null) return;
            foreach (NavMeshLink link in links)
                if (link != null) link.enabled = !down;
        }
    }
}
