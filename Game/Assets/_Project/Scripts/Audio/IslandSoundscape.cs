using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The island prison's outdoor sound, local and cosmetic on every machine: surf loops at points around the
    /// waterline (spatial, so the sea is loudest at the cliff edges), and a foghorn sounding now and then from far
    /// out in the bay, from one of a few fixed bearings. Never near a real threat, never a clue.
    /// </summary>
    public class IslandSoundscape : MonoBehaviour
    {
        [Tooltip("Where the surf breaks: one spatial loop each.")]
        [SerializeField] private Transform[] surf;
        [Tooltip("Where the foghorn can sound from (far out in the bay).")]
        [SerializeField] private Transform[] foghorns;
        [Tooltip("Seconds between foghorn blasts (random in this range).")]
        [SerializeField] private Vector2 foghornEvery = new(35f, 80f);
        [SerializeField, Range(0f, 1f)] private float surfLevel = .55f;

        private float nextHorn;
        private int seed = 911;

        private void Start()
        {
            nextHorn = Time.time + Random.Range(8f, 20f);
            if (Application.isBatchMode || surf == null) return;
            foreach (Transform point in surf)
            {
                if (point == null) continue;
                SoundLoop loop = GameAudio.Loop(SoundId.SurfLoop, point, "Surf", spatial: true);
                loop.SetRange(6f, 45f, AudioRolloffMode.Logarithmic);
                loop.Level = surfLevel;
            }
        }

        private void Update()
        {
            if (Application.isBatchMode || foghorns == null || foghorns.Length == 0 || Time.time < nextHorn) return;
            seed = seed * 1103515245 + 12345;
            Transform from = foghorns[Mathf.Abs(seed >> 8) % foghorns.Length];
            nextHorn = Time.time + Random.Range(foghornEvery.x, foghornEvery.y);
            if (from != null) GameAudio.Play(SoundId.Foghorn, from.position, 1f);
        }

#if UNITY_EDITOR
        public void EditorSetup(Transform[] surfPoints, Transform[] hornPoints)
        {
            surf = surfPoints;
            foghorns = hornPoints;
        }
#endif
    }
}
