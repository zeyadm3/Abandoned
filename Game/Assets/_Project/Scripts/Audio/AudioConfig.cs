using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The sound system's tunables (0.14.0): bus trims, ducking, what walls do to sound, and the
    /// listener's room. Lives at Data/Audio/Resources/AudioConfig (Tools/Abandoned/Content/Audio/Build Sound Bank
    /// makes it); the defaults below apply until that asset exists.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Audio/Audio Config", fileName = "AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        public const string ResourcePath = "AudioConfig";

        [Header("Voices")]
        [Tooltip("One-shot sounds that can play at once. When all are busy the oldest of equal or lower importance is cut.")]
        [SerializeField, Range(16, 64)] private int voices = 40;

        [Header("Loudness")]
        [Tooltip("Under every bus: how loud the whole game is with the player's sliders at 100%. Crew voices are not affected.")]
        [SerializeField, Range(0.05f, 1f)] private float baseLevel = 0.5f;

        [Header("Bus trims (on top of the player's volume settings)")]
        [SerializeField, Range(0f, 2f)] private float effects = 1f;
        [SerializeField, Range(0f, 2f)] private float footsteps = 0.9f;
        [SerializeField, Range(0f, 2f)] private float structure = 1f;
        [SerializeField, Range(0f, 2f)] private float threats = 1f;
        [SerializeField, Range(0f, 2f)] private float ambience = 1f;
        [SerializeField, Range(0f, 2f)] private float music = 1f;
        [SerializeField, Range(0f, 2f)] private float ui = 1f;
        [SerializeField, Range(0f, 2f)] private float vitals = 1f;
        [Tooltip("Ambience and music under audible crew speech (1 = no ducking).")]
        [SerializeField, Range(0.2f, 1f)] private float speechDuck = 0.65f;
        [Tooltip("How loud the synthesised layer is beside a recording, for sounds that play both.")]
        [SerializeField, Range(0f, 1.5f)] private float layerGain = 0.7f;

        [Header("Walls and floors between a sound and the listener")]
        [SerializeField, Range(1, 4)] private int maxWalls = 3;
        [Tooltip("Volume kept per wall.")]
        [SerializeField, Range(0.2f, 1f)] private float wallGain = 0.62f;
        [Tooltip("Monsters are never quieter than this behind walls: dulled, not hidden.")]
        [SerializeField, Range(0.2f, 1f)] private float softFloor = 0.6f;
        [Tooltip("Low-pass cutoff (Hz) behind one wall and behind the most walls counted.")]
        [SerializeField] private Vector2 wallCutoff = new(3200f, 850f);
        [Tooltip("From this share of a sound's range the air starts taking its top end off.")]
        [SerializeField, Range(0.1f, 1f)] private float farFrom = 0.45f;
        [Tooltip("Low-pass cutoff (Hz) at the edge of a sound's range.")]
        [SerializeField] private float farCutoff = 4800f;

        [Header("The listener's room (live reverb)")]
        [SerializeField] private bool reverb = true;
        [Tooltip("Average distance to the surfaces around the listener (m): a closet, and the atrium.")]
        [SerializeField] private Vector2 roomSize = new(2.5f, 15f);
        [Tooltip("Reverb decay (s) in the smallest and the largest room.")]
        [SerializeField] private Vector2 decay = new(0.5f, 3.1f);
        [Tooltip("Reverb level (millibels) in the smallest and the largest room.")]
        [SerializeField] private Vector2 wetLevel = new(-1700f, -650f);
        [Tooltip("Reverb level (millibels) under open sky.")]
        [SerializeField] private float outdoorLevel = -4200f;
        [Tooltip("Seconds for the room to change as the listener moves.")]
        [SerializeField, Min(0.05f)] private float roomBlend = 1.2f;

        public int Voices => voices;
        public float BaseLevel => baseLevel;
        public float SpeechDuck => speechDuck;
        public float LayerGain => layerGain;
        public int MaxWalls => maxWalls;
        public float WallGain => wallGain;
        public float SoftFloor => softFloor;
        public Vector2 WallCutoff => wallCutoff;
        public float FarFrom => farFrom;
        public float FarCutoff => farCutoff;
        public bool Reverb => reverb;
        public Vector2 RoomSize => roomSize;
        public Vector2 Decay => decay;
        public Vector2 WetLevel => wetLevel;
        public float OutdoorLevel => outdoorLevel;
        public float RoomBlend => roomBlend;

        public float Trim(AudioBus bus) => bus switch
        {
            AudioBus.Footsteps => footsteps,
            AudioBus.Structure => structure,
            AudioBus.Threats => threats,
            AudioBus.Ambience => ambience,
            AudioBus.Music => music,
            AudioBus.Ui => ui,
            AudioBus.Vitals => vitals,
            _ => effects,
        };

        private static AudioConfig current;

        public static AudioConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<AudioConfig>(ResourcePath);
                if (current == null) current = CreateInstance<AudioConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}
