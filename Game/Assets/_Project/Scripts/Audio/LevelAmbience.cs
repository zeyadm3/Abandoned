using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// A level's background sound on every machine: wind, the hum of its lights (as loud as the share
    /// of ceiling fixtures still lit: power off = silence), and now and then something settling far
    /// off. Cosmetic: never a gameplay noise, never a structure warning.
    /// </summary>
    public class LevelAmbience : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float windVolume = 0.22f;
        [SerializeField, Range(0f, 1f)] private float humVolume = 0.1f;
        [Tooltip("Hum level when the level has no ceiling fixtures (the HQ's plain lamps).")]
        [SerializeField, Range(0f, 1f)] private float humWithoutFixtures;
        [Tooltip("Seconds between distant settling sounds (0 = never).")]
        [SerializeField] private Vector2 settleInterval = new(20f, 50f);
        [SerializeField] private Vector2 settleDistance = new(14f, 28f);

        private AudioSource wind, hum;
        private float nextSettle;

        public float HumLevel { get; private set; }

        private void Awake()
        {
            wind = Loop("Wind", AmbienceSynth.Wind(12f, 5));
            hum = Loop("Hum", AmbienceSynth.Hum(4f));
            ScheduleSettle();
        }

        private AudioSource Loop(string name, AudioClip clip)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            AudioSource s = go.AddComponent<AudioSource>();
            s.clip = clip;
            s.loop = true;
            s.spatialBlend = 0f;
            s.volume = 0f;
            s.Play();
            return s;
        }

        private void Update()
        {
            float level = AudioLevels.Ambience * AudioLevels.Sfx * AudioLevels.BackgroundDuck;
            // A storm job (M9.2) howls through the building.
            bool storm = Extraction.RunState.Current != null && Extraction.RunState.Current.IsSpawned && Extraction.RunState.Current.State.Storm;
            float shelter = Mathf.Lerp(1f, storm ? 0.6f : 0.3f, BuildingInteriorAtmosphere.InsideBlend);
            wind.volume = Mathf.Min(1f, windVolume * (storm ? 2.6f : 1f)) * level * shelter;
            wind.pitch = storm ? 1.25f : 1f;
            HumLevel = Mathf.MoveTowards(HumLevel, LitShare(), Time.deltaTime * 2f);
            float interiorHum = BuildingInteriorAtmosphere.Present ? Mathf.Lerp(0.55f, 1f, BuildingInteriorAtmosphere.InsideBlend) : 1f;
            hum.volume = humVolume * HumLevel * level * interiorHum;

            if (settleInterval.y <= 0f || Time.time < nextSettle) return;
            ScheduleSettle();
            Camera ear = Camera.main;
            if (ear == null) return;
            Vector2 dir = Random.insideUnitCircle.normalized;
            Vector3 at = ear.transform.position + new Vector3(dir.x, Random.Range(-2f, 4f), dir.y) * Random.Range(settleDistance.x, settleDistance.y);
            GameAudio.Play(SoundId.DistantSettle, at, Random.Range(0.4f, 0.8f));
        }

        private float LitShare()
        {
            var fixtures = LightFixture.All;
            if (fixtures.Count == 0) return humWithoutFixtures > 0f ? humWithoutFixtures / Mathf.Max(0.001f, humVolume) : 0f;
            int lit = 0;
            foreach (LightFixture f in fixtures) if (f.Lit) lit++;
            return lit / (float)fixtures.Count;
        }

        private void ScheduleSettle() => nextSettle = Time.time + Random.Range(settleInterval.x, settleInterval.y);

        private void OnGUI()
        {
            if (DebugView.Visible)
                GUI.Label(new Rect(Screen.width - 360f, 74f, 350f, 22f), $"AMBIENCE wind {wind.volume:0.00}  hum {hum.volume:0.00}  sounds {GameAudio.SoundCount}");
        }

#if UNITY_EDITOR
        public void EditorSetup(float windLevel, float humLevel, float plainHum, Vector2 settleEvery)
        {
            windVolume = windLevel;
            humVolume = humLevel;
            humWithoutFixtures = plainHum;
            settleInterval = settleEvery;
        }
#endif
    }
}
