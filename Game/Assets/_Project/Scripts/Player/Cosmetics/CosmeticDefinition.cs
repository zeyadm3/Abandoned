using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// One cosmetic (GDD 18): a coverall colour or a hat, unlocked by playing. No gameplay effect.
    /// Adding one is a new asset in the catalog, no code.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Player/Cosmetic", fileName = "Cosmetic")]
    public class CosmeticDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public CosmeticKind Kind { get; private set; }
        [field: Tooltip("Coverall colour.")]
        [field: SerializeField] public Color Color { get; private set; } = Color.white;
        [field: Tooltip("Hat model, origin at the bottom centre (sits on the crown of the head). Empty = no hat.")]
        [field: SerializeField] public GameObject HatPrefab { get; private set; }

        [field: Header("Unlock (all must be met)")]
        [field: SerializeField, Min(0)] public int RequiredRuns { get; private set; }
        [field: Tooltip("Runs you made it out of alive.")]
        [field: SerializeField, Min(0)] public int RequiredEscapes { get; private set; }
        [field: Tooltip("Lifetime haul of the crews you were in ($).")]
        [field: SerializeField, Min(0)] public long RequiredHaul { get; private set; }

        public bool IsUnlocked(int runs, int escapes, long haul) => runs >= RequiredRuns && escapes >= RequiredEscapes && haul >= RequiredHaul;

        /// <summary>What's still needed, for the wardrobe ("3 more runs").</summary>
        public string Requirement(int runs, int escapes, long haul)
        {
            if (runs < RequiredRuns) return $"Play {RequiredRuns - runs} more run{(RequiredRuns - runs == 1 ? "" : "s")}";
            if (escapes < RequiredEscapes) return $"Make it out {RequiredEscapes - escapes} more time{(RequiredEscapes - escapes == 1 ? "" : "s")}";
            if (haul < RequiredHaul) return $"Haul ${RequiredHaul - haul:N0} more";
            return "";
        }

#if UNITY_EDITOR
        public void EditorSetup(string id, string displayName, CosmeticKind kind, Color color, GameObject hat, int runs, int escapes, long haul)
        {
            Id = id;
            DisplayName = displayName;
            Kind = kind;
            Color = color;
            HatPrefab = hat;
            RequiredRuns = runs;
            RequiredEscapes = escapes;
            RequiredHaul = haul;
        }
#endif
    }
}
