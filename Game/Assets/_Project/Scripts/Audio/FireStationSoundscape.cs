using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The firehouse's own sound (Docs/FIRE_STATION_PLAN.md): the watch office's radio and the engine's set crackling
    /// now and then, hose couplings knocking and chains clinking high in the hose tower when the wind gets in, and water
    /// dripping in the bay's drain, the showers and the laundry. Local and cosmetic: it never follows a monster or points
    /// at anything. Existing sounds only (radio static, the factory's distant chain rattle, the drip loop). F1 shows the
    /// points.
    /// </summary>
    public class FireStationSoundscape : MonoBehaviour
    {
        [Tooltip("Where the old radios crackle.")]
        [SerializeField] private Transform[] radioPoints;
        [Tooltip("High in the hose tower, where couplings and chains knock about.")]
        [SerializeField] private Transform[] towerPoints;
        [Tooltip("Where water drips all the time (a positioned loop at each).")]
        [SerializeField] private Transform[] dripPoints;
        [SerializeField] private Vector2 radioEvery = new(20f, 50f);
        [SerializeField] private Vector2 towerEvery = new(14f, 36f);
        [SerializeField, Range(0f, 1f)] private float dripLevel = .4f;

        private float nextRadio, nextTower;
        private int seed = 4141;

        private void Start()
        {
            nextRadio = Time.time + Random.Range(6f, 16f);
            nextTower = Time.time + Random.Range(8f, 20f);
            if (Application.isBatchMode || dripPoints == null) return;
            foreach (Transform point in dripPoints)
            {
                if (point == null) continue;
                SoundLoop loop = GameAudio.Loop(SoundId.WaterDrip, point, "Drip", spatial: true);
                loop.Level = dripLevel;
                loop.SetRange(1f, 12f);
            }
        }

        private void Update()
        {
            if (Application.isBatchMode) return;
            if (Time.time >= nextRadio)
            {
                nextRadio = Time.time + Random.Range(radioEvery.x, radioEvery.y);
                Transform point = Pick(radioPoints);
                if (point != null) GameAudio.Play(SoundId.RadioStatic, point.position, .65f);
            }
            if (Time.time >= nextTower)
            {
                nextTower = Time.time + Random.Range(towerEvery.x, towerEvery.y);
                Transform point = Pick(towerPoints);
                if (point != null) GameAudio.Play(SoundId.FactoryChainRattle, point.position, .8f);
            }
        }

        private Transform Pick(Transform[] points)
        {
            if (points == null || points.Length == 0) return null;
            seed = seed * 1664525 + 1013904223;
            return points[Mathf.Abs(seed >> 8) % points.Length];
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            Draw(radioPoints, new Color(.3f, .6f, 1f, .7f));
            Draw(towerPoints, new Color(1f, .8f, .2f, .7f));
            Draw(dripPoints, new Color(.3f, 1f, 1f, .7f));
        }

        private static void Draw(Transform[] points, Color colour)
        {
            if (points == null) return;
            Gizmos.color = colour;
            foreach (Transform p in points) if (p != null) Gizmos.DrawWireSphere(p.position, .4f);
        }
#endif

#if UNITY_EDITOR
        public void EditorSetup(Transform[] radios, Transform[] tower, Transform[] drips)
        {
            radioPoints = radios;
            towerPoints = tower;
            dripPoints = drips;
        }
#endif
    }
}
