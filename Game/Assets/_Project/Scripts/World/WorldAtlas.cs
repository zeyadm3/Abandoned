using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>Every place in the open world and where the van starts (Data/World/Resources/WorldAtlas, written by the world builder).</summary>
    [CreateAssetMenu(menuName = "Abandoned/World/World Atlas")]
    public class WorldAtlas : ScriptableObject
    {
        public const string WorldScene = "World";

        [SerializeField] private List<WorldPoi> pois = new();
        [Tooltip("Index of the HQ in the list.")]
        [SerializeField] private int home;

        public IReadOnlyList<WorldPoi> Pois => pois;
        public int Home => home;
        public WorldPoi HomePoi => pois.Count > home ? pois[home] : null;

        public int IndexOf(string scene)
        {
            for (int i = 0; i < pois.Count; i++) if (pois[i].scene == scene) return i;
            return -1;
        }

        public void EditorSetup(List<WorldPoi> places, int homeIndex)
        {
            pois = places;
            home = homeIndex;
        }

        private static WorldAtlas cached;

        public static WorldAtlas Load()
        {
            if (cached == null) cached = Resources.Load<WorldAtlas>("WorldAtlas");
            return cached;
        }
    }
}
