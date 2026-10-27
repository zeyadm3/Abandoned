using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// A wardrobe-style card's picture (2026 remake): the item's rendered icon from UiIcons when it has one
    /// (Editor/Remake/IconRenderer), else its first letter as before.
    /// </summary>
    public static class IconChip
    {
        public static void Fill(VisualElement chip, string iconId, string name)
        {
            var icon = UiIcons.Get(iconId);
            if (icon != null)
            {
                chip.style.backgroundImage = new StyleBackground(icon);
                chip.AddToClassList("wardrobe__chip--icon");
                return;
            }
            if (!string.IsNullOrEmpty(name)) chip.Add(new Label(name.Substring(0, 1)) { pickingMode = PickingMode.Ignore });
        }
    }
}
