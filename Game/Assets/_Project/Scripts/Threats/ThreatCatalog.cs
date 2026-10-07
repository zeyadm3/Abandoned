using UnityEngine;

namespace Abandoned.Threats
{
    [CreateAssetMenu(menuName = "Abandoned/Threats/Threat Catalog")]
    public class ThreatCatalog : ScriptableObject
    {
        [field: SerializeField] public ThreatDefinition[] Definitions { get; private set; } = System.Array.Empty<ThreatDefinition>();
#if UNITY_EDITOR
        public void EditorSetup(ThreatDefinition[] definitions) => Definitions = definitions;
#endif
    }
}
