using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Marks a collider that is invisible on purpose (a level-bounds wall, a gameplay volume that must
    /// block), so the rebuild's invisible-collider audit lists it instead of reporting it as a bug.
    /// </summary>
    public class IntentionalInvisibleCollider : MonoBehaviour
    {
        [SerializeField] private string reason;

        public string Reason => reason;

#if UNITY_EDITOR
        public void EditorSetup(string why) => reason = why;
#endif
    }
}
