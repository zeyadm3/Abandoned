using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// A named place and facing in the training house: where a chapter restarts ("chapter_3"), where the lesson's
    /// monster comes in ("threat_in"), wanders ("threat_patrol") and leaves ("threat_out").
    /// </summary>
    public class TutorialSpot : MonoBehaviour
    {
        private static readonly List<TutorialSpot> Live = new();

        [SerializeField] private string key = "spot";

        public static IReadOnlyList<TutorialSpot> All => Live;
        public string Key => key;
        public Pose Pose => new(transform.position, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));

        public static TutorialSpot Find(string spotKey)
        {
            foreach (TutorialSpot s in Live) if (s != null && s.key == spotKey) return s;
            return null;
        }

        /// <summary>Chapter <paramref name="chapter"/>'s restart spot, else the nearest earlier chapter's.</summary>
        public static TutorialSpot ForChapter(int chapter)
        {
            for (int c = chapter; c >= 0; c--)
                if (Find("chapter_" + c) is TutorialSpot found) return found;
            return null;
        }

        private void OnEnable() => Live.Add(this);
        private void OnDisable() => Live.Remove(this);

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && !DebugView.Visible) return;
            Gizmos.color = key.StartsWith("threat") ? new Color(1f, 0.3f, 0.25f, 0.8f) : new Color(1f, 0.85f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.9f, 0.35f);
            Gizmos.DrawLine(transform.position + Vector3.up * 0.9f, transform.position + Vector3.up * 0.9f + transform.forward * 0.8f);
        }

#if UNITY_EDITOR
        public void EditorSetup(string spotKey) => key = spotKey;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}
