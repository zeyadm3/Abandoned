using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>
    /// One kind of loot (GDD 7.1). Everything about an item lives here, so adding an item is
    /// "create a definition, generate its prefab" with no code.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Loot/Loot Definition", fileName = "Loot_New")]
    public class LootDefinition : ScriptableObject, IValidatable
    {
        public const float MinPhysicsMass = 0.1f;
        public const float MaxPhysicsMass = 80f;

        [field: Header("Identity")]
        [field: SerializeField] public string Id { get; private set; } = "new_item";
        [field: SerializeField] public string DisplayName { get; private set; } = "New Item";

        [field: Header("Value")]
        [field: SerializeField, Min(0)] public int ValueMin { get; private set; } = 100;
        [field: SerializeField, Min(0)] public int ValueMax { get; private set; } = 100;
        [field: Tooltip("Starting condition multiplier range (dusty 0.6 … pristine 1.2).")]
        [field: SerializeField, Min(0f)] public float ConditionMin { get; private set; } = 0.8f;
        [field: SerializeField, Min(0f)] public float ConditionMax { get; private set; } = 1f;

        [field: Header("Weight and carrying")]
        [field: Tooltip("Gameplay weight in kg: carry speed, stamina, floor load, carriers needed.")]
        [field: SerializeField, Min(0.01f)] public float GameplayWeight { get; private set; } = 1f;
        [field: Tooltip("Rigidbody mass, clamped to a stable range. Not the gameplay weight.")]
        [field: SerializeField, Range(MinPhysicsMass, MaxPhysicsMass)] public float PhysicsMass { get; private set; } = 1f;
        [field: SerializeField] public CarryClass CarryClass { get; private set; } = CarryClass.OneHand;
        [field: Tooltip("Bounding size in metres (also the placeholder visual's size).")]
        [field: SerializeField] public Vector3 Size { get; private set; } = new(0.3f, 0.3f, 0.3f);

        [field: Header("Carrying together (Heavy/Huge only)")]
        [field: Tooltip("People needed to lift it. 0 = the carry class default in SharedCarryConfig (Heavy 2, Huge 3).")]
        [field: SerializeField, Range(0, SharedCarryable.MaxPoints)] public int RequiredCarriers { get; private set; }
        [field: Tooltip("Hand-placed carry points in the item's local space (pivot = centre). Empty = generated from Size.")]
        [field: SerializeField] public Vector3[] CarryPoints { get; private set; } = System.Array.Empty<Vector3>();

        [field: Header("Damage, sound, spawning")]
        [field: SerializeField] public Fragility Fragility { get; private set; } = Fragility.Medium;
        [field: SerializeField] public SurfaceMaterial Material { get; private set; } = SurfaceMaterial.Plastic;
        [field: Tooltip("0–1: how loud moving/dropping it is (threats hear this in M2+).")]
        [field: SerializeField, Range(0f, 1f)] public float Noise { get; private set; } = 0.3f;
        [field: Tooltip("Relative spawn weight; higher = more common.")]
        [field: SerializeField, Min(0f)] public float Rarity { get; private set; } = 1f;
        [field: Tooltip("Where it can spawn: the store kinds of LootSpawnPoints (jewelry, electronics, gallery...).")]
        [field: SerializeField] public string[] SpawnTags { get; private set; } = System.Array.Empty<string>();
        [field: Tooltip("A run's 1-2 big prizes (GDD 7.4): only spawns on jackpot points, never on ordinary ones.")]
        [field: SerializeField] public bool Jackpot { get; private set; }
        [Tooltip("Gear that behaves like loot (a plank to carry and lay down): never spawned in a run, worth nothing.")]
        [field: SerializeField] public bool Utility { get; private set; }

        [field: Header("Placeholder visual")]
        [field: SerializeField] public PlaceholderShape Shape { get; private set; } = PlaceholderShape.Cube;
        [field: SerializeField] public Color Color { get; private set; } = Color.white;

        public void Validate(List<string> errors)
        {
            string n = $"Loot '{name}'";
            if (string.IsNullOrWhiteSpace(Id)) errors.Add($"{n}: Id is empty.");
            if (ValueMax < ValueMin) errors.Add($"{n}: ValueMax is below ValueMin.");
            if (ConditionMax < ConditionMin) errors.Add($"{n}: ConditionMax is below ConditionMin.");
            if (ConditionMin <= 0f) errors.Add($"{n}: ConditionMin must be above 0.");
            if (Size.x <= 0f || Size.y <= 0f || Size.z <= 0f) errors.Add($"{n}: Size must be positive.");
            if (CarryClass == CarryClass.Pocket && GameplayWeight > 1f) errors.Add($"{n}: Pocket items should weigh under 1 kg.");
            if (CarryClass >= CarryClass.Heavy && GameplayWeight < 50f) errors.Add($"{n}: Heavy/Huge items should weigh 50 kg or more.");
            if (CarryClass < CarryClass.Heavy && (RequiredCarriers > 0 || (CarryPoints?.Length ?? 0) > 0))
                errors.Add($"{n}: only Heavy/Huge items are carried together; clear RequiredCarriers/CarryPoints.");
            int authored = CarryPoints?.Length ?? 0;
            if (authored > SharedCarryable.MaxPoints) errors.Add($"{n}: at most {SharedCarryable.MaxPoints} carry points.");
            if (authored > 0 && RequiredCarriers > authored) errors.Add($"{n}: RequiredCarriers is more than its carry points.");
            if (PhysicsMass < MinPhysicsMass || PhysicsMass > MaxPhysicsMass) errors.Add($"{n}: PhysicsMass outside the stable range.");
            if (!Utility && (SpawnTags == null || SpawnTags.Length == 0)) errors.Add($"{n}: no SpawnTags, so it never spawns in a run.");
        }

        public bool HasSpawnTag(string tag)
        {
            if (SpawnTags == null) return false;
            foreach (string t in SpawnTags) if (t == tag) return true;
            return false;
        }

#if UNITY_EDITOR
        /// <summary>Editor-only setup used by the catalog builder to author definitions in code.</summary>
        public void EditorSetup(string id, string displayName, int valueMin, int valueMax, float weight,
            CarryClass carryClass, Vector3 size, Fragility fragility, SurfaceMaterial material, float noise,
            float rarity, PlaceholderShape shape, Color color, float conditionMin = 0.8f, float conditionMax = 1f, int requiredCarriers = 0)
        {
            Id = id;
            DisplayName = displayName;
            ValueMin = valueMin;
            ValueMax = valueMax;
            GameplayWeight = weight;
            PhysicsMass = Mathf.Clamp(weight, MinPhysicsMass, MaxPhysicsMass);
            CarryClass = carryClass;
            Size = size;
            Fragility = fragility;
            Material = material;
            Noise = noise;
            Rarity = rarity;
            Shape = shape;
            Color = color;
            ConditionMin = conditionMin;
            ConditionMax = conditionMax;
            RequiredCarriers = requiredCarriers;
        }

        /// <summary>Editor-only: spawn data, set separately so it can be filled in on existing definitions.</summary>
        public void EditorSetSpawning(string[] tags, bool jackpot)
        {
            SpawnTags = tags;
            Jackpot = jackpot;
        }

        public void EditorSetUtility(bool utility) => Utility = utility;
#endif
    }
}
