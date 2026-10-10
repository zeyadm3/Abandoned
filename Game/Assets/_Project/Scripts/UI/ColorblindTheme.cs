using Abandoned.Core;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// Colourblind-safe tags and warnings (0.23.1): with the colourblind setting on, the whole UI's good/bad
    /// colours (quota met, warnings, toasts, loot damage, the death card) switch to blue/orange through one class
    /// on the menu root (Revamp.uss .colorblind), the same palette the stress scanner already used.
    /// </summary>
    public class ColorblindTheme : MonoBehaviour
    {
        private void Update()
        {
            var root = HudLayer.Root?.parent;
            if (root == null) return;
            bool on = GameSettings.ColorblindScanner;
            if (root.ClassListContains("colorblind") != on) root.EnableInClassList("colorblind", on);
        }
    }
}
