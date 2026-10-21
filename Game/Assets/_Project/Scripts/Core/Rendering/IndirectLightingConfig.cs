using UnityEngine;

namespace Abandoned.Core
{
    [CreateAssetMenu(menuName = "Abandoned/Rendering/Indirect Lighting", fileName = "IndirectLightingConfig")]
    public sealed class IndirectLightingConfig : ScriptableObject
    {
        [SerializeField] private int[] lightsByQuality = { 0, 2, 4, 6 };
        [SerializeField, Range(0f, 1f)] private float reflectance = .32f;
        [SerializeField, Range(1f, 8f)] private float radius = 5.5f;
        [SerializeField, Min(1f)] private float distance = 22f;
        [SerializeField, Min(.1f)] private float refreshSeconds = 1f;
        public int Lights(int quality) => lightsByQuality[Mathf.Clamp(quality, 0, lightsByQuality.Length - 1)];
        public float Reflectance => reflectance;
        public float Radius => radius;
        public float Distance => distance;
        public float RefreshSeconds => refreshSeconds;
    }
}
