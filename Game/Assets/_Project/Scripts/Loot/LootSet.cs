using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>A loot set (0.18.3): bring one of each member home in the same truck and the set pays a bonus. Data only.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Loot/Loot Set", fileName = "LootSet_New")]
    public class LootSet : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; } = "New Set";
        /// <summary>The name as screens show it, in the player's language; <see cref="DisplayName"/> stays English for logic and the network.</summary>
        public string LocalName => Loc.T(DisplayName);
        [field: SerializeField] public LootDefinition[] Members { get; private set; } = System.Array.Empty<LootDefinition>();
        [field: SerializeField, Min(0)] public int Bonus { get; private set; } = 1500;

#if UNITY_EDITOR
        public void EditorSetup(string displayName, LootDefinition[] members, int bonus)
        {
            DisplayName = displayName;
            Members = members;
            Bonus = bonus;
        }
#endif
    }
}
