using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Iron Factory soundscape (L6): local cosmetic audio playing distant chain rattles high up in the trusses,
    /// and deep thermal expansion groans around the hot channel and steel frame. Never near a real threat,
    /// never points to anything.
    /// </summary>
    public class FactorySoundscape : MonoBehaviour
    {
        [Tooltip("Points high in the roof trusses where hanging chains rattle far off.")]
        [SerializeField] private Transform[] chainPoints;

        [Tooltip("Points around the molten channel and frame columns where iron groans under heat.")]
        [SerializeField] private Transform[] groanPoints;

        [Tooltip("Seconds between distant chain rattles (random range).")]
        [SerializeField] private Vector2 chainEvery = new(24f, 55f);

        [Tooltip("Seconds between structural thermal groans (random range).")]
        [SerializeField] private Vector2 groanEvery = new(16f, 42f);

        private float nextChain, nextGroan;
        private int seed = 4409;

        private void Start()
        {
            nextChain = Time.time + Random.Range(6f, 18f);
            nextGroan = Time.time + Random.Range(10f, 25f);
        }

        private void Update()
        {
            if (Application.isBatchMode) return;

            if (chainPoints != null && chainPoints.Length > 0 && Time.time >= nextChain)
            {
                seed = seed * 1664525 + 1013904223;
                Transform point = chainPoints[Mathf.Abs(seed >> 8) % chainPoints.Length];
                nextChain = Time.time + Random.Range(chainEvery.x, chainEvery.y);
                if (point != null) GameAudio.Play(SoundId.FactoryChainRattle, point.position, 0.85f);
            }

            if (groanPoints != null && groanPoints.Length > 0 && Time.time >= nextGroan)
            {
                seed = seed * 1664525 + 1013904223;
                Transform point = groanPoints[Mathf.Abs(seed >> 8) % groanPoints.Length];
                nextGroan = Time.time + Random.Range(groanEvery.x, groanEvery.y);
                if (point != null) GameAudio.Play(SoundId.FactoryThermalGroan, point.position, 0.9f);
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(Transform[] chains, Transform[] groans)
        {
            chainPoints = chains;
            groanPoints = groans;
        }
#endif
    }
}
