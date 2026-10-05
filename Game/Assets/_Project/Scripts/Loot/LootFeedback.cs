using Abandoned.Audio;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>
    /// Local presentation for a loot item: impact sounds by material, floating "-$X" on damage,
    /// cosmetic shards on shatter. Never changes value.
    /// </summary>
    [RequireComponent(typeof(LootItem))]
    public class LootFeedback : MonoBehaviour
    {
        [SerializeField] private LootDamageConfig damageConfig;

        private static readonly Color LossColor = new(1f, 0.35f, 0.3f);
        private LootItem item;

        private void Awake() => item = GetComponent<LootItem>();

        private void OnEnable()
        {
            item.Impacted += OnImpacted;
            item.Damaged += OnDamaged;
            item.Shattered += OnShattered;
        }

        private void OnDisable()
        {
            item.Impacted -= OnImpacted;
            item.Damaged -= OnDamaged;
            item.Shattered -= OnShattered;
        }

        private void OnImpacted(LootItem loot, float speed, Vector3 point)
        {
            if (speed < damageConfig.MinSoundSpeed) return;
            float volume = Mathf.Clamp01(speed / damageConfig.FullVolumeSpeed);
            PlaceholderAudio.PlayImpact(loot.Definition.Material, point, volume);
        }

        private void OnDamaged(LootItem loot, int loss, Vector3 point) =>
            FloatingText.Show(point + Vector3.up * 0.2f, $"-${loss:N0}", LossColor);

        private void OnShattered(LootItem loot, Vector3 point)
        {
            FloatingText.Show(point + Vector3.up * 0.3f, $"${loot.FullValue:N0} → $0", LossColor);
            PlaceholderAudio.PlayImpact(loot.Definition.Material, point, 1f);
            var visual = loot.GetComponentInChildren<Renderer>();
            ShatterEffect.Spawn(loot.transform.position, loot.Definition.Size, visual != null ? visual.sharedMaterial : null);
        }
    }
}
