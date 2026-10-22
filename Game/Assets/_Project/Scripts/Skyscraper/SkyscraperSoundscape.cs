using Abandoned.Audio;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Skyscraper
{
    /// <summary>
    /// Environmental soundscape for Cloudline Tower & The Apex Restaurant.
    /// Emits high-altitude wind howling through shattered glass, elevator hoisting cables creaking in the shaft,
    /// a fallen piano lid thudding in the lounge, glasses rattling in the back-bar, and dripping water on flooded floors.
    /// F1 toggles Gizmos for all sound nodes via DebugView.
    /// </summary>
    public class SkyscraperSoundscape : MonoBehaviour
    {
        [Header("Sound Nodes")]
        [Tooltip("Points along broken windows where wind gusts whistle.")]
        [SerializeField] private Transform[] glassWindPoints;
        [Tooltip("Points in the elevator shaft where cables creak and groan.")]
        [SerializeField] private Transform[] cableCreakPoints;
        [Tooltip("Points near the lounge grand piano where the lid drops.")]
        [SerializeField] private Transform[] pianoLidPoints;
        [Tooltip("Points at the bar and wine wall where crystal glasses rattle.")]
        [SerializeField] private Transform[] glassRattlePoints;
        [Tooltip("Points on the flooded floor where burst pipes drip.")]
        [SerializeField] private Transform[] waterDripPoints;

        [Header("Intervals")]
        [SerializeField] private Vector2 windEvery = new(12f, 28f);
        [SerializeField] private Vector2 creakEvery = new(18f, 45f);
        [SerializeField] private Vector2 pianoEvery = new(45f, 110f);
        [SerializeField] private Vector2 rattleEvery = new(20f, 50f);
        [SerializeField, Range(0f, 1f)] private float dripLevel = 0.40f;

        private float nextWind;
        private float nextCreak;
        private float nextPiano;
        private float nextRattle;
        private int rngSeed = 4291;

        private void Start()
        {
            nextWind = Time.time + Random.Range(3f, 10f);
            nextCreak = Time.time + Random.Range(6f, 18f);
            nextPiano = Time.time + Random.Range(25f, 60f);
            nextRattle = Time.time + Random.Range(10f, 22f);

            if (Application.isBatchMode || waterDripPoints == null) return;
            foreach (Transform pt in waterDripPoints)
            {
                if (pt == null) continue;
                SoundLoop loop = GameAudio.Loop(SoundId.WaterDrip, pt, "TowerDrip", spatial: true);
                loop.Level = dripLevel;
                loop.SetRange(1.5f, 14f);
            }
        }

        private void Update()
        {
            if (Application.isBatchMode) return;

            // 1. Wind whistling through broken windows
            if (Time.time >= nextWind)
            {
                nextWind = Time.time + Random.Range(windEvery.x, windEvery.y);
                Transform pt = Pick(glassWindPoints);
                if (pt != null) GameAudio.Play(SoundId.GhostWhoosh, pt.position, 0.65f, 30f);
            }

            // 2. Elevator hoist cables creaking in the shaft
            if (Time.time >= nextCreak)
            {
                nextCreak = Time.time + Random.Range(creakEvery.x, creakEvery.y);
                Transform pt = Pick(cableCreakPoints);
                if (pt != null) GameAudio.Play(SoundId.Groan, pt.position, 0.70f, 40f);
            }

            // 3. Piano lid falling in the lounge
            if (Time.time >= nextPiano)
            {
                nextPiano = Time.time + Random.Range(pianoEvery.x, pianoEvery.y);
                Transform pt = Pick(pianoLidPoints);
                if (pt != null) GameAudio.Play(SoundId.DoorSlam, pt.position, 0.60f, 32f);
            }

            // 4. Glasses rattling on the bar rack and sommelier wall
            if (Time.time >= nextRattle)
            {
                nextRattle = Time.time + Random.Range(rattleEvery.x, rattleEvery.y);
                Transform pt = Pick(glassRattlePoints);
                if (pt != null) GameAudio.Play(SoundId.LootRattle, pt.position, 0.50f, 22f);
            }
        }

        private Transform Pick(Transform[] points)
        {
            if (points == null || points.Length == 0) return null;
            rngSeed = rngSeed * 1664525 + 1013904223;
            return points[Mathf.Abs(rngSeed >> 8) % points.Length];
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            DrawGroup(glassWindPoints, new Color(0.3f, 0.8f, 1.0f, 0.8f));
            DrawGroup(cableCreakPoints, new Color(1.0f, 0.6f, 0.2f, 0.8f));
            DrawGroup(pianoLidPoints, new Color(0.9f, 0.3f, 0.9f, 0.8f));
            DrawGroup(glassRattlePoints, new Color(1.0f, 0.9f, 0.3f, 0.8f));
            DrawGroup(waterDripPoints, new Color(0.2f, 1.0f, 0.8f, 0.8f));
        }

        private static void DrawGroup(Transform[] points, Color color)
        {
            if (points == null) return;
            Gizmos.color = color;
            foreach (Transform pt in points)
            {
                if (pt != null) Gizmos.DrawWireSphere(pt.position, 0.45f);
            }
        }
#endif

#if UNITY_EDITOR
        public void EditorSetup(Transform[] wind, Transform[] cables, Transform[] piano, Transform[] glass, Transform[] drips)
        {
            glassWindPoints = wind;
            cableCreakPoints = cables;
            pianoLidPoints = piano;
            glassRattlePoints = glass;
            waterDripPoints = drips;
        }
#endif
    }
}
