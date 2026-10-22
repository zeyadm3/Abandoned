using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Battery lights bypass mains power, but still go with their supporting slab.</summary>
    public sealed class HospitalEmergencyLight : MonoBehaviour
    {
        [SerializeField] private Light lamp;
        [SerializeField] private StructuralSection support;

        private void LateUpdate()
        {
            if (lamp != null)
                lamp.enabled = support == null || !support.IsCollapsed;
        }

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible)
                return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, .3f);
        }

#if UNITY_EDITOR
        public void EditorSetup(Light source, StructuralSection section)
        {
            lamp = source;
            support = section;
        }
#endif
    }
}
