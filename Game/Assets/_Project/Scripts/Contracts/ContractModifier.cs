using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Contracts
{
    /// <summary>
    /// One contract twist (GDD 14), as data: a new modifier is a new asset, not code. Each field nudges
    /// the run; the ones it doesn't use stay neutral.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Contracts/Contract Modifier", fileName = "Modifier_New")]
    public class ContractModifier : ScriptableObject, IValidatable
    {
        [field: SerializeField] public string Id { get; private set; } = "new_modifier";
        [field: SerializeField] public string DisplayName { get; private set; } = "New Modifier";
        [field: SerializeField, TextArea] public string Description { get; private set; } = "";

        [Tooltip("Added to the rolled structural stability (-0.2 = 'unstable').")]
        [field: SerializeField, Range(-0.5f, 0.5f)] public float StabilityDelta { get; private set; }
        [Tooltip("Multiplies the extraction window (0.6 = 'rush job').")]
        [field: SerializeField, Range(0.3f, 2f)] public float WindowMultiplier { get; private set; } = 1f;
        [Tooltip("Added to the payout bonus.")]
        [field: SerializeField, Range(0f, 1f)] public float BonusDelta { get; private set; }
        [Tooltip("Lights off in the building.")]
        [field: SerializeField] public bool PowerOff { get; private set; }
        [Tooltip("Spawn weight multiplier for High/Extreme fragility loot ('fragile collection').")]
        [field: SerializeField, Range(0.1f, 10f)] public float FragileRarity { get; private set; } = 1f;
        [Tooltip("Multiplies the share of loot points that get loot ('picked over' < 1).")]
        [field: SerializeField, Range(0.2f, 2f)] public float LootMultiplier { get; private set; } = 1f;
        [Tooltip("Jackpots beyond the usual 1-2 ('heavy jackpot').")]
        [field: SerializeField, Range(0, 2)] public int ExtraJackpots { get; private set; }
        [Tooltip("Night: the sun is down outside and the building is darker even with power.")]
        [field: SerializeField] public bool Night { get; private set; }
        [Tooltip("Threats that open the run on top of the usual one.")]
        [field: SerializeField, Range(0, 2)] public int ExtraThreats { get; private set; }
        [Tooltip("Scales how well threats hear (storm < 1: the noise covers footsteps).")]
        [field: SerializeField, Range(0.3f, 1.5f)] public float HearingMultiplier { get; private set; } = 1f;
        [Tooltip("Scales how fast overloaded floors decay (storm > 1).")]
        [field: SerializeField, Range(0.5f, 3f)] public float DecayMultiplier { get; private set; } = 1f;
        [Tooltip("Storm: the wind howls through the building (cosmetic; pair with hearing/decay).")]
        [field: SerializeField] public bool Storm { get; private set; }
        [Tooltip("Lowest company level this can roll at.")]
        [field: SerializeField, Range(1, 30)] public int MinLevel { get; private set; } = 1;

        public void Validate(List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(Id)) errors.Add($"{name}: Id is empty.");
        }

#if UNITY_EDITOR
        public void EditorSetup(string id, string displayName, string description, float stabilityDelta = 0f,
            float windowMultiplier = 1f, float bonusDelta = 0f, bool powerOff = false, float fragileRarity = 1f,
            float lootMultiplier = 1f, int minLevel = 1, int extraJackpots = 0, bool night = false, int extraThreats = 0,
            float hearingMultiplier = 1f, float decayMultiplier = 1f, bool storm = false)
        {
            ExtraJackpots = extraJackpots;
            Night = night;
            ExtraThreats = extraThreats;
            HearingMultiplier = hearingMultiplier;
            DecayMultiplier = decayMultiplier;
            Storm = storm;
            Id = id;
            DisplayName = displayName;
            Description = description;
            StabilityDelta = stabilityDelta;
            WindowMultiplier = windowMultiplier;
            BonusDelta = bonusDelta;
            PowerOff = powerOff;
            FragileRarity = fragileRarity;
            LootMultiplier = lootMultiplier;
            MinLevel = minLevel;
        }
#endif
    }
}
