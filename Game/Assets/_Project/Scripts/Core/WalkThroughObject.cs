using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Marks a visible object that players pass through on purpose (0.14.0), with the reason. Everything
    /// else with volume is made solid by the rebuild; the walk-through audit lists these instead of
    /// reporting them. The mirror of <see cref="IntentionalInvisibleCollider"/>.
    /// </summary>
    public class WalkThroughObject : MonoBehaviour
    {
        [SerializeField] private string reason;

        public string Reason => reason;

#if UNITY_EDITOR
        public void EditorSetup(string why) => reason = why;
#endif
    }
}
