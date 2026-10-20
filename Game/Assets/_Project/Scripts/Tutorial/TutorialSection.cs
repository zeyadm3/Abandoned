using System.Collections.Generic;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>A structural section a lesson names ("hallway", "bedroom_b"); sits on the section itself.</summary>
    [RequireComponent(typeof(StructuralSection))]
    public class TutorialSection : MonoBehaviour
    {
        private static readonly List<TutorialSection> Live = new();

        [SerializeField] private string key = "section";

        private StructuralSection section;

        public static IReadOnlyList<TutorialSection> All => Live;
        public string Key => key;
        public StructuralSection Section => section != null ? section : section = GetComponent<StructuralSection>();

        public static StructuralSection Find(string sectionKey)
        {
            foreach (TutorialSection s in Live) if (s != null && s.key == sectionKey) return s.Section;
            return null;
        }

        private void OnEnable() => Live.Add(this);
        private void OnDisable() => Live.Remove(this);

#if UNITY_EDITOR
        public void EditorSetup(string sectionKey) => key = sectionKey;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}
