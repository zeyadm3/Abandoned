using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>How a cosmetic becomes yours.</summary>
    public enum CosmeticUnlock { Free, Buy, Reward }

    /// <summary>
    /// One cosmetic (GDD 18; redone in 0.12.5): a coverall colour, a hat or an accessory. No gameplay effect.
    /// It's free, bought once from the company's money at the HQ wardrobe, or a reward for an achievement.
    /// Adding one is a new asset in the catalog, no code.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Player/Cosmetic", fileName = "Cosmetic")]
    public class CosmeticDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public CosmeticKind Kind { get; private set; }
        [field: Tooltip("One line for the wardrobe's detail panel.")]
        [field: SerializeField, TextArea] public string Description { get; private set; }
        [field: Tooltip("Coverall colour.")]
        [field: SerializeField] public Color Color { get; private set; } = Color.white;
        [field: Tooltip("Hat or accessory model, origin at the crown of the head (a hat sits on it; face and body pieces are offset from it). Empty = none.")]
        [field: SerializeField] public GameObject HatPrefab { get; private set; }
        [field: Tooltip("Accessory worn on the body (a rucksack, a belt), not the head: hidden while its wearer is down.")]
        [field: SerializeField] public bool BodyMounted { get; private set; }

        [field: Header("Unlock")]
        [field: Tooltip("Company money, paid once at the wardrobe. 0 with no reward = free.")]
        [field: SerializeField, Min(0)] public int Price { get; private set; }
        [field: Tooltip("Achievement id that awards it (overrides the price).")]
        [field: SerializeField] public string RewardAchievement { get; private set; }

        public CosmeticUnlock Unlock => !string.IsNullOrEmpty(RewardAchievement) ? CosmeticUnlock.Reward : Price > 0 ? CosmeticUnlock.Buy : CosmeticUnlock.Free;

        /// <summary>The profile key: ids are only unique within a kind.</summary>
        public string Key => $"{Kind}:{Id}";

#if UNITY_EDITOR
        public void EditorSetup(string id, string displayName, CosmeticKind kind, string description, Color color, GameObject model,
            int price, string rewardAchievement, bool bodyMounted = false)
        {
            Id = id;
            DisplayName = displayName;
            Kind = kind;
            Description = description;
            Color = color;
            HatPrefab = model;
            Price = price;
            RewardAchievement = rewardAchievement ?? "";
            BodyMounted = bodyMounted;
        }
#endif
    }
}
