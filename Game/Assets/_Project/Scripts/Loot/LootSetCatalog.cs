using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>Every loot set (0.18.3). Data/Loot/Resources/LootSetCatalog.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Loot/Loot Set Catalog", fileName = "LootSetCatalog")]
    public class LootSetCatalog : ScriptableObject
    {
        public const string ResourcePath = "LootSetCatalog";

        [SerializeField] private List<LootSet> sets = new();

        public IReadOnlyList<LootSet> Sets => sets;

        private static LootSetCatalog current;

        public static LootSetCatalog Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<LootSetCatalog>(ResourcePath);
                if (current == null) current = CreateInstance<LootSetCatalog>();
                return current;
            }
        }

#if UNITY_EDITOR
        public void EditorSet(List<LootSet> all) => sets = all;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}
