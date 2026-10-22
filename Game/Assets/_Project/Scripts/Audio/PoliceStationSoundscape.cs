using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The precinct's own sound (Docs/POLICE_STATION_PLAN.md): a police scanner still crackling now and then in dispatch
    /// and the squad room, a cell door banging somewhere far off in the cell block or the basement, and water dripping
    /// in the basement and the showers. Local and cosmetic: it never follows a monster or points at anything. Existing
    /// sounds only (radio static, a door slam, the drip loop). F1 shows the points.
    /// </summary>
    public class PoliceStationSoundscape : MonoBehaviour
    {
        [Tooltip("Where the old scanners and radios crackle.")]
        [SerializeField] private Transform[] scannerPoints;
        [Tooltip("Where a cell door or a basement door bangs, far off.")]
        [SerializeField] private Transform[] slamPoints;
        [Tooltip("Where water drips all the time (a positioned loop at each).")]
        [SerializeField] private Transform[] dripPoints;
        [SerializeField] private Vector2 scannerEvery = new(18f, 46f);
        [SerializeField] private Vector2 slamEvery = new(55f, 120f);
        [SerializeField, Range(0f, 1f)] private float dripLevel = .45f;

        private float nextScanner, nextSlam;
        private int seed = 7313;

        private void Start()
        {
            nextScanner = Time.time + Random.Range(5f, 15f);
            nextSlam = Time.time + Random.Range(30f, 60f);
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
            if (Time.time >= nextScanner)
            {
                nextScanner = Time.time + Random.Range(scannerEvery.x, scannerEvery.y);
                Transform point = Pick(scannerPoints);
                if (point != null) GameAudio.Play(SoundId.RadioStatic, point.position, .7f);
            }
            if (Time.time >= nextSlam)
            {
                nextSlam = Time.time + Random.Range(slamEvery.x, slamEvery.y);
                Transform point = Pick(slamPoints);
                if (point != null) GameAudio.Play(SoundId.DoorSlam, point.position, .55f);
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
            Draw(scannerPoints, new Color(.3f, .6f, 1f, .7f));
            Draw(slamPoints, new Color(1f, .5f, .2f, .7f));
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
        public void EditorSetup(Transform[] scanners, Transform[] slams, Transform[] drips)
        {
            scannerPoints = scanners;
            slamPoints = slams;
            dripPoints = drips;
        }
#endif
    }
}
