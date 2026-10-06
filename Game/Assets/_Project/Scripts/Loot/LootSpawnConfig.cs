using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>How full a building is per run (one asset in Data/Loot).</summary>
    [CreateAssetMenu(menuName = "Abandoned/Loot/Loot Spawn Config", fileName = "LootSpawnConfig")]
    public class LootSpawnConfig : ScriptableObject, IValidatable
    {
        [Tooltip("Chance an ordinary spawn point gets an item each run.")]
        [field: SerializeField, Range(0f, 1f)] public float FillChance { get; private set; } = 0.6f;
        [Tooltip("Jackpots per run (GDD 7.4: 1-2), picked from the level's jackpot points.")]
        [field: SerializeField, Range(0, 4)] public int JackpotsMin { get; private set; } = 1;
        [field: SerializeField, Range(0, 4)] public int JackpotsMax { get; private set; } = 2;

        public void Validate(List<string> errors)
        {
            if (JackpotsMax < JackpotsMin) errors.Add($"{name}: JackpotsMax is below JackpotsMin.");
        }
    }
}
