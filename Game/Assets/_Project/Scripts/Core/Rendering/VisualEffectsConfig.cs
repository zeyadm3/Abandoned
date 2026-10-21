using UnityEngine;

namespace Abandoned.Core
{
    [CreateAssetMenu(menuName = "Abandoned/Rendering/Visual Effects", fileName = "VisualEffectsConfig")]
    public sealed class VisualEffectsConfig : ScriptableObject
    {
        [SerializeField] private Vector2 snowflakeSize = new(.022f, .05f);
        [SerializeField] private Vector2 dustSize = new(.008f, .022f);
        [SerializeField, Range(.01f, .5f)] private float dustOpacity = .22f;
        [SerializeField, Range(.01f, .5f)] private float steamOpacity = .18f;
        [SerializeField, Min(16)] private int dustPerEmitter = 120;
        [SerializeField, Min(64)] private int snowPerEmitter = 600;
        public Vector2 SnowflakeSize => OrderedSize(snowflakeSize);
        public Vector2 DustSize => OrderedSize(dustSize);
        public float DustOpacity => Mathf.Clamp01(dustOpacity);
        public float SteamOpacity => Mathf.Clamp01(steamOpacity);
        public int DustPerEmitter => Mathf.Max(1, dustPerEmitter);
        public int SnowPerEmitter => Mathf.Max(1, snowPerEmitter);

        private static Vector2 OrderedSize(Vector2 size) => new(
            Mathf.Max(.001f, Mathf.Min(size.x, size.y)),
            Mathf.Max(.001f, Mathf.Max(size.x, size.y)));
    }
}
