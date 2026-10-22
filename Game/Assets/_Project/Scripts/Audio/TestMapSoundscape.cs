using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The TestMap site's outdoor sound, local and cosmetic on every machine: birdsong and wind in the trees at points
    /// along the forest's edge (spatial, so the woods are loudest where you stand nearest them, and dulled once you are
    /// indoors), crickets in the long grass, and a crow now and then from somewhere in the treetops. Never a clue,
    /// never near a threat.
    /// </summary>
    public class TestMapSoundscape : MonoBehaviour
    {
        [SerializeField] private Transform[] birds;
        [SerializeField] private Transform[] wind;
        [SerializeField] private Transform[] crickets;
        [Tooltip("Treetops a crow may call from.")]
        [SerializeField] private Transform[] crows;
        [Tooltip("Seconds between crow calls (random in this range).")]
        [SerializeField] private Vector2 crowEvery = new(14f, 40f);
        [SerializeField, Range(0f, 1f)] private float birdLevel = .55f;
        [SerializeField, Range(0f, 1f)] private float windLevel = .5f;
        [SerializeField, Range(0f, 1f)] private float cricketLevel = .35f;

        private float nextCrow;

        private void Start()
        {
            nextCrow = Time.time + Random.Range(5f, 12f);
            if (Application.isBatchMode) return;
            Beds(birds, SoundId.BirdsongLoop, "Birds", birdLevel, 10f, 70f);
            Beds(wind, SoundId.TreeWindLoop, "TreeWind", windLevel, 10f, 60f);
            Beds(crickets, SoundId.CricketsLoop, "Crickets", cricketLevel, 3f, 28f);
        }

        private static void Beds(Transform[] points, SoundId id, string name, float level, float near, float far)
        {
            if (points == null) return;
            foreach (Transform point in points)
            {
                if (point == null) continue;
                SoundLoop loop = GameAudio.Loop(id, point, name, spatial: true);
                loop.SetRange(near, far, AudioRolloffMode.Logarithmic);
                // Start each bed at a different moment of its recording so neighbouring points don't sing in unison.
                if (loop.Source.clip != null) loop.Source.time = Random.Range(0f, loop.Source.clip.length * .9f);
                loop.Level = level;
            }
        }

        private void Update()
        {
            if (Application.isBatchMode || crows == null || crows.Length == 0 || Time.time < nextCrow) return;
            nextCrow = Time.time + Random.Range(crowEvery.x, crowEvery.y);
            Transform from = crows[Random.Range(0, crows.Length)];
            if (from != null) GameAudio.Play(SoundId.CrowCall, from.position, Random.Range(.6f, 1f));
        }

#if UNITY_EDITOR
        public void EditorSetup(Transform[] birdPoints, Transform[] windPoints, Transform[] cricketPoints, Transform[] crowPoints)
        {
            birds = birdPoints;
            wind = windPoints;
            crickets = cricketPoints;
            crows = crowPoints;
        }
#endif
    }
}
