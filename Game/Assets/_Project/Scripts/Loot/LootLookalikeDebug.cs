using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>
    /// F1 report of the 0.24.9 look-alike pass: how many static copies of loot models were swapped for
    /// fixtures, dust sheets or packaging, how many are left (should be 0), and where each swap stands.
    /// </summary>
    public sealed class LootLookalikeDebug : MonoBehaviour
    {
        [SerializeField] private Bounds[] spots = System.Array.Empty<Bounds>();
        [SerializeField] private int remaining;

        public Bounds[] Spots => spots ?? System.Array.Empty<Bounds>();

#if UNITY_EDITOR
        public void EditorSetup(Bounds[] swapped, int left) { spots = swapped; remaining = left; }
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            GUI.Label(new Rect(Screen.width - 380, 232, 375, 22),
                $"LOOT LOOK-ALIKES: {Spots.Length} swapped · {remaining} left");
        }

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            // Amber: a spot where scenery that looked like loot was replaced.
            Gizmos.color = new Color(1f, .7f, .15f, .6f);
            foreach (Bounds spot in Spots) Gizmos.DrawWireCube(spot.center, spot.size);
        }
#endif
    }
}
