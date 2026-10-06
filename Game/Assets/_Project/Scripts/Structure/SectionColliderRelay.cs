using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Unity only sends collision messages to the GameObject that owns the static collider, so a
    /// section whose collider lives on a child (a stair ramp) needs this to hear impacts.
    /// </summary>
    public class SectionColliderRelay : MonoBehaviour
    {
        [SerializeField] private StructuralSection section;

        private void OnCollisionEnter(Collision collision) => section.HandleCollision(collision);

#if UNITY_EDITOR
        public void EditorSetup(StructuralSection owner) => section = owner;
#endif
    }
}
