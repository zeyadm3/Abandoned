using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>Every achievement, in display order (Data/Core/Resources/Achievements).</summary>
    [CreateAssetMenu(menuName = "Abandoned/Core/Achievement Catalog", fileName = "Achievements")]
    public class AchievementCatalog : ScriptableObject, IValidatable
    {
        public const string ResourcePath = "Achievements";

        [SerializeField] private List<AchievementDefinition> items = new();

        public IReadOnlyList<AchievementDefinition> Items => items;

        public void Validate(List<string> errors)
        {
            var ids = new HashSet<string>();
            foreach (AchievementDefinition a in items)
                if (a == null || string.IsNullOrEmpty(a.Id) || string.IsNullOrEmpty(a.Stat) || !ids.Add(a.Id))
                    errors.Add($"{name}: an achievement is missing, unnamed, statless or duplicated.");
        }

#if UNITY_EDITOR
        public void EditorSet(List<AchievementDefinition> list) => items = list;
#endif
    }
}
