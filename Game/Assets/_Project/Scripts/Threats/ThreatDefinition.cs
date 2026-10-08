using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Threats
{
    public enum ThreatKind { BlindOne, Stalker, Collector, Hunter, Weight, Thing, Crawlers, LastHunter }

    /// <summary>Roster identity, escalation and attack tuning shared by host AI and presentation.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Threats/Threat Definition")]
    public class ThreatDefinition : ScriptableObject
    {
        [field: SerializeField] public ThreatKind Kind { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public NetworkObject Prefab { get; private set; }
        [field: SerializeField, Min(0f)] public float OpeningWeight { get; private set; }
        [field: SerializeField, Range(0, 10)] public int MinimumDanger { get; private set; }
        [field: SerializeField] public bool FinalOnly { get; private set; }
        [field: SerializeField, Min(0f)] public float SpawnWarningSeconds { get; private set; }
        [field: SerializeField, Min(0f)] public float Damage { get; private set; } = 65f;
        [field: SerializeField, Min(0.3f)] public float AttackCooldown { get; private set; } = 2.5f;
        [field: SerializeField] public bool LethalContact { get; private set; }
        [field: SerializeField, Min(0.1f)] public float WalkSpeed { get; private set; } = 1.8f;
        [field: SerializeField, Min(0.1f)] public float ChaseSpeed { get; private set; } = 4.8f;
        [field: SerializeField, Min(0.1f)] public float AttackRange { get; private set; } = 1.25f;
        [field: SerializeField, Min(0.1f)] public float SenseRange { get; private set; } = 22f;
        [field: SerializeField, Min(0.1f)] public float SpecialInterval { get; private set; } = 8f;
        [field: SerializeField, Min(0f)] public float StructuralMomentum { get; private set; } = 1800f;
        [field: SerializeField, Min(0f)] public float AggressionPerDanger { get; private set; } = 0.09f;
        [field: SerializeField, Min(0f)] public float SignatureInterval { get; private set; } = 3f;
        [field: SerializeField, Min(1f)] public float AudibleRange { get; private set; } = 38f;
        [Tooltip("Light-shy threats (the Crawlers): a beam within this range (m) and cone (degrees) scatters them for ScatterSeconds.")]
        [field: SerializeField, Min(0f)] public float LightRepelRange { get; private set; } = 16f;
        [field: SerializeField, Range(1f, 90f)] public float LightRepelAngle { get; private set; } = 30f;
        [field: SerializeField, Min(0f)] public float ScatterSeconds { get; private set; } = 2.5f;

#if UNITY_EDITOR
        public void EditorSetup(ThreatKind kind, string displayName, NetworkObject prefab, float weight, int minimumDanger,
            float damage, float walk, float chase, bool lethal = false, bool final = false)
        {
            Kind = kind; DisplayName = displayName; Prefab = prefab; OpeningWeight = weight; MinimumDanger = minimumDanger;
            Damage = damage; WalkSpeed = walk; ChaseSpeed = chase; LethalContact = lethal; FinalOnly = final;
            if (kind == ThreatKind.Weight) { SpecialInterval = 6f; StructuralMomentum = 3800f; SignatureInterval = 5f; AudibleRange = 55f; }
            if (kind == ThreatKind.Thing) { SpecialInterval = 5f; AttackCooldown = 4f; SignatureInterval = 4.5f; }
            if (kind == ThreatKind.Crawlers) { AttackRange = 1.65f; AttackCooldown = 4f; SenseRange = 16f; SignatureInterval = 2f; }
            if (kind == ThreatKind.LastHunter) { SpawnWarningSeconds = 8f; AttackRange = 1.9f; SpecialInterval = 1.8f; StructuralMomentum = 5400f; AudibleRange = 90f; SignatureInterval = 2.5f; }
        }
#endif
    }
}
