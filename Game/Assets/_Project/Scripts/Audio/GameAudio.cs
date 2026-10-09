using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Voice;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The front door for every sound in the game (0.14.0). A caller names a <see cref="SoundId"/> and
    /// where it happened; this looks up what the sound is (<see cref="SoundCatalog"/>), how it is tuned
    /// (the SoundBank), what stands between it and the listener (<see cref="AudioOcclusion"/>), and hands
    /// it to the pool on its bus. Recorded clips come from the bank; synthesised ones from
    /// <see cref="SynthLibrary"/>. Cosmetic and local: each machine plays what it sees happen.
    /// </summary>
    public static class GameAudio
    {
        /// <summary>A sound that was just played; drawn as a gizmo with F1 on.</summary>
        public readonly struct Mark
        {
            public readonly Vector3 Position;
            public readonly AudioBus Bus;
            public readonly float Time;

            public Mark(Vector3 position, AudioBus bus, float time)
            {
                Position = position;
                Bus = bus;
                Time = time;
            }
        }

        private const float ImpactNear = 1.5f, ImpactFar = 30f, HeavyNear = 2.5f, HeavyFar = 40f, StepNear = 1f, StepFar = 18f;

        private static SoundBank bank;
        private static bool bankLoaded;
        private static readonly float[] nextAllowed = new float[SoundCatalog.Count];
        private static readonly Mark[] recent = new Mark[24];
        private static int recentHead;

        /// <summary>Last sounds played and counts; used by tests and the debug overlay.</summary>
        public static SurfaceMaterial? LastImpactMaterial { get; private set; }
        public static int ImpactCount { get; private set; }
        public static SurfaceMaterial? LastFootstepMaterial { get; private set; }
        public static StructureSound? LastStructureSound { get; private set; }
        public static int StructureSoundCount { get; private set; }
        public static SoundId? LastSound { get; private set; }
        public static int SoundCount { get; private set; }
        public static IReadOnlyList<Mark> Recent => recent;

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

        /// <summary>A sound at a place in the world. <paramref name="maxDistance"/> overrides how far it carries.</summary>
        public static void Play(SoundId id, Vector3 position, float volume01 = 1f, float maxDistance = 0f)
        {
            LastSound = id;
            SoundCount++;
            Emit(id, position, volume01, flat: false, maxDistance, 1f);
        }

        /// <summary>A single recorded sound whose owner can replace it or stop it when its screen closes.</summary>
        public static SoundPlayback PlayCancellableRecording(SoundId id, Vector3 position, float volume01 = 1f)
        {
            // A missing spoken recording must stay silent; a procedural placeholder cannot say its words.
            if (SoundCatalog.Info(id).Mode != SoundMode.Library || Bank == null ||
                Bank.Get(id) is not { HasClips: true }) return default;
            LastSound = id;
            SoundCount++;
            return AudioPool.Capture(Emit(id, position, volume01, flat: false, 0f, 1f));
        }

        /// <summary>A sound at a place that starts after a delay: what fell hasn't landed yet.</summary>
        public static void PlayDelayed(SoundId id, Vector3 position, float delay, float volume01 = 1f)
        {
            LastSound = id;
            SoundCount++;
            Emit(id, position, volume01, flat: false, 0f, 1f, delay);
        }

        /// <summary>A flat (non-positional) sound: menus, screens, and things heard in the player's own head.</summary>
        public static void PlayUi(SoundId id, float volume01 = 1f)
        {
            LastSound = id;
            SoundCount++;
            Emit(id, Vector3.zero, volume01, flat: true, 0f, 1f);
        }

        /// <summary>
        /// Starts a looping sound as a child of its owner. The owner sets <see cref="SoundLoop.Level"/>;
        /// it stops when the owner's object goes.
        /// </summary>
        public static SoundLoop Loop(SoundId id, Transform parent, string name, bool spatial = false)
        {
            var loop = new SoundLoop(id, parent, name, spatial);
            AudioPool.Register(loop);
            return loop;
        }

        /// <summary>The player's own pulse. The Vitals setting is applied by the bus, not by the caller.</summary>
        public static void PlayHeartbeat(float volume01, float pitch = 1f) =>
            Emit(SoundId.Heartbeat, Vector3.zero, volume01, flat: true, 0f, pitch);

        public static void PlayBreath(float volume01) => Emit(SoundId.Breath, Vector3.zero, volume01, flat: true, 0f, 1f);

        public static void PlayImpact(SurfaceMaterial material, Vector3 position, float volume01)
        {
            LastImpactMaterial = material;
            ImpactCount++;
            SoundBank b = Bank;
            bool heavy = volume01 >= (b != null ? b.HeavyFrom : 0.6f);
            SoundCue cue = b != null ? b.Impact(material, volume01) : null;
            EmitSurface(cue, cue != null && cue.HasClips ? null : SynthLibrary.Impact(material, heavy), position, volume01,
                AudioBus.Effects, 128, heavy ? HeavyNear : ImpactNear, heavy ? HeavyFar : ImpactFar);
        }

        public static void PlayFootstep(SurfaceMaterial material, Vector3 position, float volume01)
        {
            LastFootstepMaterial = material;
            SoundBank b = Bank;
            SoundCue cue = b != null ? b.Footstep(material) : null;
            EmitSurface(cue, cue != null && cue.HasClips ? null : SynthLibrary.Footstep(material), position, volume01,
                AudioBus.Footsteps, 140, StepNear, StepFar);
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
            Emit(id, position, volume01, flat: false, 0f, 1f);
            if (sound == StructureSound.Crash && Bank != null && Bank.Get(SoundId.CrashDebris) is { HasClips: true })
                Emit(SoundId.CrashDebris, position, volume01 * 0.75f, flat: false, 0f, 1f);
        }

        private static AudioSource Emit(SoundId id, Vector3 position, float volume01, bool flat, float maxDistance, float pitchScale,
            float delay = 0f)
        {
            SoundInfo info = SoundCatalog.Info(id);
            if (info.Cooldown > 0f)
            {
                if (Time.unscaledTime < nextAllowed[(int)id]) return null;
                nextAllowed[(int)id] = Time.unscaledTime + info.Cooldown;
            }
            SoundBank b = Bank;
            SoundCue cue = b != null ? b.Get(id) : null;
            bool spatial = !flat && !info.Flat;
            var request = new VoiceRequest
            {
                Position = position,
                Volume = Mathf.Clamp01(volume01) * (cue != null ? cue.Volume : info.Volume),
                MinDistance = cue != null ? cue.MinDistance : info.MinDistance,
                MaxDistance = maxDistance > 0f ? maxDistance : cue != null ? cue.MaxDistance : info.MaxDistance,
                Spatial = spatial,
                Priority = info.Priority,
                Bus = info.Bus,
                OcclusionGain = 1f,
                Cutoff = AudioVoice.OpenCutoff,
                Delay = delay,
            };
            if (spatial && TryEar(out Vector3 ear))
                AudioOcclusion.Evaluate(info.Occlusion, ear, position, request.MaxDistance, out request.OcclusionGain, out request.Cutoff);

            // A bank made before these sounds were redesigned still holds their old recordings: not those.
            bool recorded = cue != null && cue.HasClips && (info.Mode != SoundMode.Synth || b.Revision >= SoundCatalog.Revision);
            AudioSource primary = null;
            if (recorded)
            {
                request.Pitch = cue.PickPitch() * pitchScale;
                primary = AudioPool.Play(cue.Pick(), request);
            }
            if (!recorded || info.Mode == SoundMode.Layered)
            {
                if (recorded) request.Volume *= AudioConfig.Current.LayerGain;
                request.Pitch = Random.Range(info.PitchLow, info.PitchHigh) * pitchScale;
                AudioSource layer = AudioPool.Play(SynthLibrary.Pick(id), request);
                if (primary == null) primary = layer;
            }
            Remember(position, info.Bus, spatial);
            if (info.Caption != null) SubtitleFeed.Report(id, position, request.MaxDistance, !spatial);
            return primary;
        }

        // Impacts and footsteps: keyed by surface material instead of by name.
        private static void EmitSurface(SoundCue cue, AudioClip synthesised, Vector3 position, float volume01, AudioBus bus,
            int priority, float near, float far)
        {
            bool recorded = cue != null && cue.HasClips;
            var request = new VoiceRequest
            {
                Position = position,
                Volume = Mathf.Clamp01(volume01) * (cue != null ? cue.Volume : 1f),
                Pitch = recorded ? cue.PickPitch() : Random.Range(0.94f, 1.06f),
                MinDistance = recorded ? cue.MinDistance : near,
                MaxDistance = recorded ? cue.MaxDistance : far,
                Spatial = true,
                Priority = priority,
                Bus = bus,
                OcclusionGain = 1f,
                Cutoff = AudioVoice.OpenCutoff,
            };
            if (TryEar(out Vector3 ear))
                AudioOcclusion.Evaluate(SoundOcclusion.Full, ear, position, request.MaxDistance, out request.OcclusionGain, out request.Cutoff);
            AudioPool.Play(recorded ? cue.Pick() : synthesised, request);
            Remember(position, bus, true);
        }

        /// <summary>Where this machine hears from, if it has a listener at all (a headless host doesn't).</summary>
        public static bool TryEar(out Vector3 ear)
        {
            AudioListener listener = VoiceListener.Current;
            if (listener != null)
            {
                ear = listener.transform.position;
                return true;
            }
            ear = default;
            return false;
        }

        private static void Remember(Vector3 position, AudioBus bus, bool spatial)
        {
            if (!spatial) return;
            recent[recentHead] = new Mark(position, bus, Time.unscaledTime);
            recentHead = (recentHead + 1) % recent.Length;
        }

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
            System.Array.Clear(nextAllowed, 0, nextAllowed.Length);
            System.Array.Clear(recent, 0, recent.Length);
            recentHead = 0;
        }
    }
}
