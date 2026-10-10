using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Floor types (0.17.2): capacity and health against concrete for glass, timber and tile sections.
    /// Data/Structure/Resources/FloorKindConfig; code defaults until then.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Structure/Floor Kind Config", fileName = "FloorKindConfig")]
    public class FloorKindConfig : ScriptableObject
    {
        public const string ResourcePath = "FloorKindConfig";

        [Tooltip("Load each kind holds against concrete (concrete, glass, timber, tile).")]
        [SerializeField] private float[] capacity = { 1f, 0.45f, 0.75f, 0.9f };
        [Tooltip("Health (how much battering it takes) against concrete.")]
        [SerializeField] private float[] health = { 1f, 0.5f, 0.8f, 1f };

        public float Capacity(FloorKind kind) => (int)kind < capacity.Length ? capacity[(int)kind] : 1f;
        public float Health(FloorKind kind) => (int)kind < health.Length ? health[(int)kind] : 1f;

        private static FloorKindConfig current;

        public static FloorKindConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<FloorKindConfig>(ResourcePath);
                if (current == null) current = CreateInstance<FloorKindConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}
