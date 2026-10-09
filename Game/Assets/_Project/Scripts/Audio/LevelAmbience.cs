using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// A level's background sound on every machine: wind (loud outside, sheltered indoors, howling in a
    /// storm), the hum of its lights (as loud as the share of ceiling fixtures still lit: power off =
    /// silence), the dead air of the building once you are inside it, and now and then something settling
    /// far off or a failing tube buzzing nearby. Cosmetic: never a gameplay noise, never a structure warning.
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
        [Tooltip("How loud the building's dead air is once you are fully inside.")]
        [SerializeField, Range(0f, 1f)] private float roomToneVolume = 0.16f;
        [Tooltip("Seconds between a nearby failing light buzzing (0 = never).")]
        [SerializeField] private Vector2 buzzInterval = new(5f, 14f);
        [Tooltip("Seconds between rolls of thunder on a storm job.")]
        [SerializeField] private Vector2 thunderInterval = new(18f, 45f);

        private const float BuzzRange = 11f;

        private SoundLoop wind, hum, roomTone;
        private float nextSettle, nextBuzz, nextThunder;

        public float HumLevel { get; private set; }

        private void Awake()
        {
            wind = GameAudio.Loop(SoundId.WindLoop, transform, "Wind");
            hum = GameAudio.Loop(SoundId.HumLoop, transform, "Hum");
            roomTone = GameAudio.Loop(SoundId.RoomToneLoop, transform, "RoomTone");
            ScheduleSettle();
            nextBuzz = Time.time + Random.Range(buzzInterval.x, buzzInterval.y);
            nextThunder = Time.time + Random.Range(4f, thunderInterval.x);
        }

        private void Update()
        {
            // A storm job (M9.2) howls through the building.
            bool storm = Extraction.RunState.Current != null && Extraction.RunState.Current.IsSpawned && Extraction.RunState.Current.State.Storm;
            float inside = BuildingInteriorAtmosphere.Present ? BuildingInteriorAtmosphere.InsideBlend : 0f;
            float shelter = Mathf.Lerp(1f, storm ? 0.6f : 0.3f, inside);
            wind.Level = Mathf.Min(1f, windVolume * (storm ? 2.6f : 1f)) * shelter;
            wind.Pitch = storm ? 1.25f : 1f;
            HumLevel = Mathf.MoveTowards(HumLevel, LitShare(), Time.deltaTime * 2f);
            float interiorHum = BuildingInteriorAtmosphere.Present ? Mathf.Lerp(0.55f, 1f, inside) : 1f;
            hum.Level = humVolume * HumLevel * interiorHum;
            roomTone.Level = roomToneVolume * inside;

            Camera ear = Camera.main;
            if (ear == null) return;
            Vector3 at = ear.transform.position;
            if (settleInterval.y > 0f && Time.time >= nextSettle)
            {
                ScheduleSettle();
                Vector2 dir = Random.insideUnitCircle.normalized;
                Vector3 from = at + new Vector3(dir.x, Random.Range(-2f, 4f), dir.y) * Random.Range(settleDistance.x, settleDistance.y);
                GameAudio.Play(SoundId.DistantSettle, from, Random.Range(0.4f, 0.8f));
            }
            if (buzzInterval.y > 0f && Time.time >= nextBuzz)
            {
                nextBuzz = Time.time + Random.Range(buzzInterval.x, buzzInterval.y);
                Buzz(at);
            }
            if (storm && thunderInterval.y > 0f && Time.time >= nextThunder)
            {
                nextThunder = Time.time + Random.Range(thunderInterval.x, thunderInterval.y);
                // Heard through the roof once you are in.
                GameAudio.PlayUi(SoundId.Thunder, Mathf.Lerp(0.9f, 0.45f, inside));
            }
        }

        // The nearest lit, failing fixture in earshot sputters: the lights you see flicker are the ones you hear.
        private static void Buzz(Vector3 ear)
        {
            LightFixture nearest = null;
            float best = BuzzRange * BuzzRange;
            foreach (LightFixture f in LightFixture.All)
            {
                if (!f.Faulty || !f.Lit) continue;
                float d = (f.transform.position - ear).sqrMagnitude;
                if (d >= best) continue;
                best = d;
                nearest = f;
            }
            if (nearest != null) GameAudio.Play(SoundId.LightBuzz, nearest.transform.position, Random.Range(0.6f, 1f));
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (DebugView.Visible)
                GUI.Label(new Rect(Screen.width - 360f, 74f, 350f, 22f),
                    $"AMBIENCE wind {wind.Source.volume:0.00}  hum {hum.Source.volume:0.00}  air {roomTone.Source.volume:0.00}  sounds {GameAudio.SoundCount}");
        }
#endif

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
