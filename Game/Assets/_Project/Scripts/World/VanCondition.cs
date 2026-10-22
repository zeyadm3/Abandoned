using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// The van's fuel, damage and breakdowns. Host-authoritative: the host feeds it distance driven, idle time and
    /// impacts; an empty tank or a breakdown stops the engine until refuelled or repaired. <see cref="WorldVan"/> sends
    /// the result to every machine.
    /// </summary>
    public class VanCondition : MonoBehaviour
    {
        [SerializeField] private VanConfig config;

        public float Fuel { get; private set; }
        /// <summary>0 = like new, 100 = wrecked.</summary>
        public float Damage { get; private set; }
        public bool BrokenDown { get; private set; }
        public bool CanDrive => Fuel > 0f && !BrokenDown;

        private void Awake()
        {
            if (config == null) config = VanConfig.Load();
            Fuel = config.TankLitres;
        }

        /// <summary>Host: the van moved <paramref name="metres"/> with the engine running.</summary>
        public void Drive(float metres)
        {
            if (!CanDrive) return;
            float km = metres / 1000f;
            Fuel = Mathf.Max(0f, Fuel - km * config.LitresPerKm);
            float wear = Damage / 100f;
            float chance = (config.BaseBreakdownPerKm + config.BreakdownPerKmAtFullDamage * wear * wear) * km;
            if (Random.value < chance) BrokenDown = true;
        }

        /// <summary>Host: the engine idled for <paramref name="seconds"/>.</summary>
        public void Idle(float seconds)
        {
            if (Fuel > 0f) Fuel = Mathf.Max(0f, Fuel - seconds / 60f * config.LitresPerMinuteIdle);
        }

        /// <summary>Host: the van hit something at <paramref name="speed"/> m/s.</summary>
        public void Impact(float speed)
        {
            if (speed <= config.SafeImpactSpeed) return;
            Damage = Mathf.Min(100f, Damage + (speed - config.SafeImpactSpeed) * config.DamagePerSpeed);
            if (Damage >= 100f) BrokenDown = true;
        }

        /// <summary>Host: carry the van's condition over from the company save.</summary>
        public void Restore(float fuel, float damage)
        {
            Fuel = Mathf.Clamp(fuel, 0f, config.TankLitres);
            Damage = Mathf.Clamp(damage, 0f, 100f);
            BrokenDown = Damage >= 100f;
        }

        public void Refuel(float litres) => Fuel = Mathf.Min(config.TankLitres, Fuel + litres);

        /// <summary>A roadside fix: the van runs again, a little of the damage patched.</summary>
        public void Repair(float patch = 10f)
        {
            BrokenDown = false;
            Damage = Mathf.Max(0f, Damage - patch);
        }
    }
}
