using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Every synthesised sound as an AudioClip, made once. A worker thread renders the whole set in the
    /// background from the first scene on, and the pool turns one finished render into a clip per frame, so
    /// nothing stalls a frame mid-game. A sound asked for before its render is done is rendered on the spot
    /// (<see cref="Get"/>), or reported as not ready for callers that can wait (<see cref="TryGet"/>: loops
    /// and music). The same seed always renders the same sound.
    /// </summary>
    public static class SynthLibrary
    {
        public const int MaxTakes = 8;
        private const int ImpactBase = 100000, StepBase = 200000, SurfaceTakes = 3;

        private static readonly Dictionary<int, AudioClip> clips = new();
        private static readonly HashSet<int> missing = new();
        private static readonly ConcurrentDictionary<int, SynthSamples> rendered = new();
        private static readonly ConcurrentDictionary<int, byte> claimed = new();
        private static volatile int generation;
        private static bool warming;

        /// <summary>For the F1 overlay: sounds made so far, and how many the background pass covers.</summary>
        public static int Rendered => clips.Count + rendered.Count;
        public static int Total { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            // Tests and headless hosts render only what they actually play.
            if (!Application.isBatchMode) Warm();
        }

        private static int Takes(SoundId id) => Mathf.Clamp(SoundCatalog.Info(id).Variations, 1, MaxTakes);

        private static int Key(SoundId id, int take) => (int)id * MaxTakes + Mathf.Abs(take) % Takes(id);

        public static AudioClip Get(SoundId id, int take = 0) => Clip(Key(id, take), renderNow: true);

        /// <summary>One of the sound's takes, at random.</summary>
        public static AudioClip Pick(SoundId id) => Get(id, Random.Range(0, Takes(id)));

        /// <summary>The clip if it has been rendered; never renders on the calling thread.</summary>
        public static bool TryGet(SoundId id, int take, out AudioClip clip)
        {
            clip = Clip(Key(id, take), renderNow: false);
            return clip != null;
        }

        public static AudioClip Impact(SurfaceMaterial material, bool heavy) =>
            Clip(ImpactBase + (int)material * 16 + (heavy ? 8 : 0) + Random.Range(0, SurfaceTakes), renderNow: true);

        public static AudioClip Footstep(SurfaceMaterial material) =>
            Clip(StepBase + (int)material * 16 + Random.Range(0, SurfaceTakes), renderNow: true);

        private static AudioClip Clip(int key, bool renderNow)
        {
            if (clips.TryGetValue(key, out AudioClip clip) && clip != null) return clip;
            if (missing.Contains(key)) return null;
            if (!rendered.TryRemove(key, out SynthSamples samples))
            {
                if (!renderNow)
                {
                    if (!Application.isBatchMode) Warm();
                    return null;
                }
                // Claimed before rendering here, not after: the worker may be on this very sound (the first
                // ones it renders are the first ones the menu plays), and it must see the claim when it finishes.
                claimed[key] = 1;
                samples = Render(key);
            }
            claimed[key] = 1;
            // The worker's copy, if it landed while this thread rendered its own: left in `rendered` it was
            // never freed, and Adopt, which takes the first entry it finds, met it every frame for the rest
            // of the session and adopted nothing else.
            rendered.TryRemove(key, out _);
            if (samples == null || samples.Data == null || samples.Data.Length < samples.Channels)
            {
                missing.Add(key);
                return null;
            }
            clip = AudioClip.Create(samples.Name, samples.Data.Length / samples.Channels, samples.Channels, samples.Rate, false);
            clip.SetData(samples.Data, 0);
            clips[key] = clip;
            return clip;
        }

        private static SynthSamples Render(int key)
        {
            if (key >= StepBase) return SurfacePatches.Footstep((SurfaceMaterial)((key - StepBase) / 16), (key - StepBase) % 16);
            if (key >= ImpactBase)
            {
                int k = key - ImpactBase;
                return SurfacePatches.Impact((SurfaceMaterial)(k / 16), k % 16 >= 8, k % 8);
            }
            return SoundPatches.Render((SoundId)(key / MaxTakes), key % MaxTakes);
        }

        /// <summary>Called by the pool once a frame: one finished render becomes a clip, and its samples are freed.</summary>
        internal static void Adopt()
        {
            if (rendered.IsEmpty) return;
            foreach (KeyValuePair<int, SynthSamples> pair in rendered)
            {
                // Already a clip (or known bad): a stale render, dropped so it can't block the queue.
                if (clips.ContainsKey(pair.Key) || missing.Contains(pair.Key))
                {
                    rendered.TryRemove(pair.Key, out _);
                    continue;
                }
                Clip(pair.Key, renderNow: false);
                return;
            }
        }

        /// <summary>Starts rendering everything on a worker thread (once).</summary>
        public static void Warm()
        {
            if (warming) return;
            warming = true;
            int mine = generation;
            List<int> keys = WarmOrder();
            Total = keys.Count;
            Task.Run(() =>
            {
                foreach (int key in keys)
                {
                    if (mine != generation) return; // a new play session started: this pass is stale
                    if (claimed.ContainsKey(key) || rendered.ContainsKey(key)) continue;
                    try
                    {
                        SynthSamples samples = Render(key);
                        if (samples != null && mine == generation && !claimed.ContainsKey(key)) rendered[key] = samples;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            });
        }

        // What is needed soonest first: the menu's sounds and its music, then the job's sounds, then the songs.
        private static List<int> WarmOrder()
        {
            var keys = new List<int>();
            void Add(SoundId id)
            {
                for (int take = 0; take < Takes(id); take++)
                {
                    int key = (int)id * MaxTakes + take;
                    if (!keys.Contains(key)) keys.Add(key);
                }
            }
            Add(SoundId.UiHover);
            Add(SoundId.UiToast);
            Add(SoundId.ChatBlip);
            Add(SoundId.MusicHq);
            Add(SoundId.HumLoop);
            Add(SoundId.WindLoop);
            Add(SoundId.RoomToneLoop);
            for (int i = 0; i < SoundCatalog.Count; i++)
                if ((SoundId)i < SoundId.MusicHq) Add((SoundId)i);
            for (int i = (int)SoundId.MusicHq; i < SoundCatalog.Count; i++) Add((SoundId)i);
            return keys;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            generation++;
            warming = false;
            Total = 0;
            clips.Clear();
            missing.Clear();
            rendered.Clear();
            claimed.Clear();
        }
    }
}
