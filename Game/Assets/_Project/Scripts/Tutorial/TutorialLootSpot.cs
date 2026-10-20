using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// Where one particular piece of training salvage stands (the table lamp, the fridge, the ring). The host
    /// spawns it from the loot catalog by its definition id when the run starts, and again when a chapter is
    /// reset; lessons name it by Key. The origin is on the surface it stands on.
    /// </summary>
    public class TutorialLootSpot : MonoBehaviour
    {
        private static readonly List<TutorialLootSpot> Live = new();

        [SerializeField] private string key = "item";
        [Tooltip("The LootDefinition's Id (the catalog's), e.g. table_lamp.")]
        [SerializeField] private string definitionId = "table_lamp";
        [Tooltip("The chapter it belongs to: resetting that chapter puts it back here.")]
        [SerializeField, Min(0)] private int chapter;

        public static IReadOnlyList<TutorialLootSpot> All => Live;
        public string Key => key;
        public string DefinitionId => definitionId;
        public int Chapter => chapter;

        private void OnEnable() => Live.Add(this);
        private void OnDisable() => Live.Remove(this);

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && !DebugView.Visible) return;
            Gizmos.color = new Color(0.95f, 0.75f, 0.2f, 0.9f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.15f, new Vector3(0.3f, 0.3f, 0.3f));
        }

#if UNITY_EDITOR
        public void EditorSetup(string itemKey, string definition, int lessonChapter)
        {
            key = itemKey;
            definitionId = definition;
            chapter = lessonChapter;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}
