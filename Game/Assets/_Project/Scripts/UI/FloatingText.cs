using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>World-space popups like "-$1,200". Static entry point; drawing is done by <see cref="FloatingTextOverlay"/>.</summary>
    public static class FloatingText
    {
        public static void Show(Vector3 worldPosition, string text, Color color) =>
            FloatingTextOverlay.Instance.Add(worldPosition, text, color);
    }
}
