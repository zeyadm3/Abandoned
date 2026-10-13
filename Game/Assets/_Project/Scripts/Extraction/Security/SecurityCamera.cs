using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// A wall-mounted security camera (0.16.7). Its Camera only renders while someone on this machine is
    /// watching it from the security desk; nothing is networked (monsters and crew are already synced).
    /// </summary>
    public class SecurityCamera : MonoBehaviour
    {
        private static readonly List<SecurityCamera> Live = new();

        [SerializeField] private Camera lens;
        [SerializeField] private string label = "CAM";

        public static IReadOnlyList<SecurityCamera> All => Live;
        public Camera Lens => lens;
        public string Label => label;

        /// <summary>
        /// The label in the player's language. The scene keeps it in English as "CAM 3  FOOD COURT": the number
        /// stays, the place is worded. @loc "SERVICE", "FOOD COURT", "CINEMA", "MERIDIAN GALLERY", "CLOSED OFFICES", "TOY ARCHIVE"
        /// </summary>
        public string LocalLabel
        {
            get
            {
                int gap = label.IndexOf("  ", System.StringComparison.Ordinal);
                if (gap <= 0 || !label.StartsWith("CAM ", System.StringComparison.Ordinal)) return Core.Loc.T(label);
                return Core.Loc.T("CAM {0}  {1}", label.Substring(4, gap - 4), Core.Loc.T(label.Substring(gap + 2)));
            }
        }

        private void OnEnable()
        {
            Live.Add(this);
            Live.Sort((a, b) => string.CompareOrdinal(a.label, b.label));
            if (lens != null) lens.enabled = false;
        }

        private void OnDisable() => Live.Remove(this);

#if UNITY_EDITOR
        public void EditorSetup(Camera camera, string name)
        {
            lens = camera;
            label = name;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}
