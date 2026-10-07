using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// Every UI icon by name (UI overhaul): "item/medkit" (gear and things, colour), "icon/gear" (menus,
    /// white), "board/skull" (HUD symbols, white), "mouse/mouse_left" (prompt glyphs). Built by the rebuild
    /// from the CC0 packs under Art/ThirdParty/Kenney; new gear gets its icon by dropping "item/&lt;id&gt;.png" in.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/UI/Ui Icons", fileName = "UiIcons")]
    public class UiIcons : ScriptableObject
    {
        public const string ResourcePath = "UiIcons";

        [System.Serializable]
        public struct Entry
        {
            public string Id;
            public Texture2D Texture;
        }

        [SerializeField] private List<Entry> entries = new();

        private Dictionary<string, Texture2D> byId;
        private static UiIcons instance;

        public static UiIcons Instance => instance != null ? instance : instance = Resources.Load<UiIcons>(ResourcePath);

        /// <summary>The icon, or null (callers fall back to text).</summary>
        public static Texture2D Get(string id)
        {
            UiIcons icons = Instance;
            if (icons == null || string.IsNullOrEmpty(id)) return null;
            if (icons.byId == null)
            {
                icons.byId = new Dictionary<string, Texture2D>();
                foreach (Entry e in icons.entries) if (e.Texture != null && !string.IsNullOrEmpty(e.Id)) icons.byId[e.Id] = e.Texture;
            }
            return icons.byId.TryGetValue(id, out Texture2D t) ? t : null;
        }

        public int Count => entries.Count;

#if UNITY_EDITOR
        public void EditorSet(List<Entry> list)
        {
            entries = list;
            byId = null;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;
    }
}
