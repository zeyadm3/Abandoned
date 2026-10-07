using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Where every game sound is played from: library clips from the SoundBank, the synthesised
    /// placeholder when a cue has none, through the shared source pool at the effects volume.
    /// Cosmetic and local: each machine plays what it sees happen.
    /// </summary>
    public static class GameAudio
    {
        private static SoundBank bank;
        private static bool bankLoaded;

        /// <summary>Last sounds played and counts; used by tests and the debug overlay.</summary>
        public static SurfaceMaterial? LastImpactMaterial { get; private set; }
        public static int ImpactCount { get; private set; }
        public static SurfaceMaterial? LastFootstepMaterial { get; private set; }
        public static StructureSound? LastStructureSound { get; private set; }
        public static int StructureSoundCount { get; private set; }
        public static SoundId? LastSound { get; private set; }
        public static int SoundCount { get; private set; }

        public static SoundBank Bank
        {
            get
            {
                if (!bankLoaded)
                {
                    bank = Resources.Load<SoundBank>(SoundBank.ResourcePath);
                    bankLoaded = true;
                }
                return bank;
            }
        }

        public static void Play(SoundId id, Vector3 position, float volume01 = 1f)
        {
            LastSound = id;
            SoundCount++;
            SoundCue cue = Bank != null ? Bank.Get(id) : null;
            Emit(cue, Fallback(id), position, volume01, spatial: true);
            SubtitleFeed.Report(id, position, cue?.MaxDistance ?? 40f);
        }

        /// <summary>A flat (non-positional) sound: menus and screens.</summary>
        public static void PlayUi(SoundId id, float volume01 = 1f)
        {
            LastSound = id;
            SoundCount++;
            Emit(Bank != null ? Bank.Get(id) : null, Fallback(id), Vector3.zero, volume01, spatial: false);
        }

        public static void PlayHeartbeat(float volume01) =>
            Emit(null, PlayerVitalAudio.Heartbeat, Vector3.zero, volume01, spatial: false, priority: 72);

        public static void PlayBreath(float volume01) =>
            Emit(null, PlayerVitalAudio.Breath, Vector3.zero, volume01, spatial: false, priority: 96);

        public static void PlayImpact(SurfaceMaterial material, Vector3 position, float volume01)
        {
            LastImpactMaterial = material;
            ImpactCount++;
            Emit(Bank != null ? Bank.Impact(material, volume01) : null, PlaceholderAudio.GetImpactClip(material), position, volume01, spatial: true);
        }

        public static void PlayFootstep(SurfaceMaterial material, Vector3 position, float volume01)
        {
            LastFootstepMaterial = material;
            Emit(Bank != null ? Bank.Footstep(material) : null, PlaceholderAudio.GetFootstepClip(material), position, volume01, spatial: true);
        }

        /// <summary>The building's warnings (GDD 16); a collapse is a crash with debris on top.</summary>
        public static void PlayStructure(StructureSound sound, Vector3 position, float volume01)
        {
            LastStructureSound = sound;
            StructureSoundCount++;
            SoundId id = sound switch
            {
                StructureSound.Creak => SoundId.Creak,
                StructureSound.Groan => SoundId.Groan,
                StructureSound.Snap => SoundId.Snap,
                _ => SoundId.Crash,
            };
            SoundCue cue = Bank != null ? Bank.Get(id) : null;
            Emit(cue, PlaceholderAudio.GetStructureClip(sound), position, volume01, spatial: true, priority: 48);
            SubtitleFeed.Report(id, position, cue?.MaxDistance ?? 40f);
            if (sound == StructureSound.Crash && Bank != null && Bank.Get(SoundId.CrashDebris) is { HasClips: true } debris)
                Emit(debris, null, position, volume01 * 0.75f, spatial: true, priority: 64);
        }

        private static void Emit(SoundCue cue, AudioClip fallback, Vector3 position, float volume01, bool spatial, int priority = 128)
        {
            float volume = Mathf.Clamp01(volume01) * AudioLevels.Sfx;
            if (cue != null && cue.HasClips)
                AudioPool.Play(cue.Pick(), position, volume * cue.Volume, cue.PickPitch(), cue.MinDistance, cue.MaxDistance, spatial, priority);
            else if (fallback != null)
                AudioPool.Play(fallback, position, volume, 1f, cue?.MinDistance ?? 2f, cue?.MaxDistance ?? 40f, spatial, priority);
        }

        private static AudioClip Fallback(SoundId id) => id switch
        {
            SoundId.Creak => PlaceholderAudio.GetStructureClip(StructureSound.Creak),
            SoundId.Groan => PlaceholderAudio.GetStructureClip(StructureSound.Groan),
            SoundId.Snap => PlaceholderAudio.GetStructureClip(StructureSound.Snap),
            SoundId.Crash => PlaceholderAudio.GetStructureClip(StructureSound.Crash),
            SoundId.BlindOneClick => PlaceholderAudio.Click(),
            SoundId.Horn => PlaceholderAudio.Horn(),
            SoundId.NoiseMakerShriek => PlaceholderAudio.Shriek(),
            SoundId.RadioStatic => PlaceholderAudio.Static(),
            SoundId.ScanPing => PlaceholderAudio.Ping(),
            _ => null,
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            bank = null;
            bankLoaded = false;
            LastImpactMaterial = null;
            LastFootstepMaterial = null;
            LastStructureSound = null;
            LastSound = null;
            ImpactCount = StructureSoundCount = SoundCount = 0;
        }
    }
}
