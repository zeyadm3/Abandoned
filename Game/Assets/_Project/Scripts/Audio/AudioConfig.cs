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

        [Header("Mastering (0.24.12, MasterBus on the listener)")]
        [SerializeField] private bool mastering = true;
        [SerializeField, Range(-40f, 0f)] private float compressorThresholdDb = -16f;
        [SerializeField, Range(1f, 10f)] private float compressorRatio = 3f;
        [SerializeField, Range(0.0005f, 0.1f)] private float attackSeconds = 0.008f;
        [SerializeField, Range(0.01f, 2f)] private float releaseSeconds = 0.18f;
        [Tooltip("Gain after the compressor. 0 keeps the mix as loud as before (the 0.14.0 pass was found far too loud).")]
        [SerializeField, Range(0f, 12f)] private float makeupDb = 0f;
        [SerializeField, Range(-6f, 0f)] private float ceilingDb = -1f;

        [Header("States and ducking (0.24.12)")]
        [Tooltip("Low-pass (Hz) on the world while you hide: you hear the room through the locker's steel.")]
        [SerializeField] private float hidingCutoff = 1500f;
        [Tooltip("How low the ambience sinks under a loud structural warning or monster nearby (1 = never).")]
        [SerializeField, Range(0.2f, 1f)] private float sidechainFloor = 0.55f;
        [SerializeField, Min(0.01f)] private float sidechainAttack = 0.06f;
        [SerializeField, Min(0.05f)] private float sidechainRelease = 1.4f;

        [Header("Spatial audio (0.24.12)")]
        [Tooltip("Steam Audio's head-related panning, occlusion and transmission when the plugin is present; else the built-in path.")]
        [SerializeField] private bool steamAudio = true;

        public bool Mastering => mastering;
        public float CompressorThresholdDb => compressorThresholdDb;
        public float CompressorRatio => compressorRatio;
        public float AttackSeconds => attackSeconds;
        public float ReleaseSeconds => releaseSeconds;
        public float MakeupDb => makeupDb;
        public float CeilingDb => ceilingDb;
        public float HidingCutoff => hidingCutoff;
        public float SidechainFloor => sidechainFloor;
        public float SidechainAttack => sidechainAttack;
        public float SidechainRelease => sidechainRelease;
        public bool SteamAudio => steamAudio;

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
