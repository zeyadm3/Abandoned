using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Abandoned.Audio;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.12 recorded sound: turns the CC0 recordings Tools/Audio/fetch_library.py put in Art/Audio/LibrarySource
    /// into game-ready clips in Art/Audio/Library (silence trimmed, short fades, levelled to one loudness per kind,
    /// loops crossfaded so they never click) and puts them in the SoundBank: footsteps and impacts per surface, and
    /// named cues. A recording replaces a Kenney clip or the synthesised sound; Layered cues still add their synth
    /// layer. The heartbeat, breathing, monster signatures and songs are never here (their sound is part of the rules).
    /// </summary>
    public static class SoundLibrary
    {
        public const string Source = "Assets/_Project/Art/Audio/LibrarySource";
        public const string Output = "Assets/_Project/Art/Audio/Library";

        // Other surfaces share a recorded one's clips (the bank's pitch keeps them apart a little).
        private static readonly Dictionary<SurfaceMaterial, SurfaceMaterial> StepAlias = new()
        {
            { SurfaceMaterial.Stone, SurfaceMaterial.Concrete }, { SurfaceMaterial.Asphalt, SurfaceMaterial.Concrete },
            { SurfaceMaterial.Terrazzo, SurfaceMaterial.Tile }, { SurfaceMaterial.Plastic, SurfaceMaterial.Tile },
            { SurfaceMaterial.Fabric, SurfaceMaterial.Carpet }, { SurfaceMaterial.Paper, SurfaceMaterial.Carpet },
        };
        private static readonly Dictionary<SurfaceMaterial, SurfaceMaterial> ImpactAlias = new()
        {
            { SurfaceMaterial.Stone, SurfaceMaterial.Concrete }, { SurfaceMaterial.Asphalt, SurfaceMaterial.Concrete },
            { SurfaceMaterial.Terrazzo, SurfaceMaterial.Concrete }, { SurfaceMaterial.Tile, SurfaceMaterial.Concrete },
            { SurfaceMaterial.Carpet, SurfaceMaterial.Fabric },
        };
        // Monster vocals are people and animals; slowed down they stop sounding like either.
        private static readonly Dictionary<SoundId, Vector2> Pitch = new()
        {
            { SoundId.Roar, new Vector2(0.62f, 0.74f) }, { SoundId.Scream, new Vector2(0.78f, 0.9f) },
            { SoundId.ThreatStrike, new Vector2(0.7f, 0.82f) }, { SoundId.Whisper, new Vector2(0.86f, 0.95f) },
            { SoundId.Groan, new Vector2(0.75f, 0.9f) }, { SoundId.DeepRumble, new Vector2(0.8f, 0.95f) },
        };

        [MenuItem(ToolMenu.Audio + "Build Recorded Library (0.24.12)", priority = ToolMenu.AudioP + 2)]
        public static void Build()
        {
            int made = Process();
            Fill();
            Debug.Log($"[SoundLibrary] {made} recordings processed into {Output}; bank filled.");
        }

        // ---- processing ----

        private static int Process()
        {
            if (!AssetDatabase.IsValidFolder(Source)) { Debug.LogWarning($"[SoundLibrary] No {Source}; run Tools/Audio/fetch_library.py."); return 0; }
            int made = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { Source }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string target = Path.GetDirectoryName(path)!.Replace('\\', '/').Substring(Source.Length + 1);
                string outPath = $"{Output}/{target}/{Path.GetFileNameWithoutExtension(path)}.wav";
                if (File.Exists(outPath)) continue;
                if (AssetImporter.GetAtPath(path) is AudioImporter ai)
                {
                    AudioImporterSampleSettings s = ai.defaultSampleSettings;
                    if (s.loadType != AudioClipLoadType.DecompressOnLoad) { s.loadType = AudioClipLoadType.DecompressOnLoad; ai.defaultSampleSettings = s; ai.SaveAndReimport(); }
                }
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null || clip.samples == 0) continue;
                bool loop = target.EndsWith("Loop") || target.EndsWith("EngineIdle");
                float[] mono = Mono(clip);
                mono = loop ? Crossfade(mono, clip.frequency) : Trim(mono, clip.frequency);
                if (mono.Length < clip.frequency / 100) continue;
                Level(mono, TargetRmsDb(target));
                Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
                WriteWav(outPath, mono, clip.frequency);
                AssetDatabase.ImportAsset(outPath);
                ImportSettings(outPath, loop, mono.Length / (float)clip.frequency);
                made++;
            }
            return made;
        }

        private static float TargetRmsDb(string target) =>
            target.StartsWith("Steps") ? -21f : target.StartsWith("Impacts") ? -18f : target.EndsWith("Loop") ? -24f : -18f;

        private static float[] Mono(AudioClip clip)
        {
            var data = new float[clip.samples * clip.channels];
            clip.GetData(data, 0);
            if (clip.channels == 1) return data;
            var mono = new float[clip.samples];
            for (int i = 0; i < clip.samples; i++)
            {
                float sum = 0f;
                for (int c = 0; c < clip.channels; c++) sum += data[i * clip.channels + c];
                mono[i] = sum / clip.channels;
            }
            return mono;
        }

        // Cut the silence before and after (a footstep that starts 80 ms late feels like lag), with short fades.
        private static float[] Trim(float[] s, int rate)
        {
            float peak = s.Max(Math.Abs);
            if (peak <= 0f) return Array.Empty<float>();
            float floor = Mathf.Max(peak * 0.02f, 0.0015f);
            int start = Array.FindIndex(s, x => Math.Abs(x) > floor);
            int end = Array.FindLastIndex(s, x => Math.Abs(x) > floor * 0.25f);
            if (start < 0 || end <= start) return Array.Empty<float>();
            start = Math.Max(0, start - rate / 200);           // 5 ms before the onset
            end = Math.Min(s.Length - 1, end + rate / 20);     // 50 ms of tail
            var cut = new float[end - start + 1];
            Array.Copy(s, start, cut, 0, cut.Length);
            Fade(cut, rate / 500, rate / 40);
            return cut;
        }

        // An equal-power crossfade of the end into the start: the loop point disappears.
        private static float[] Crossfade(float[] s, int rate)
        {
            int x = Math.Min(rate * 3 / 2, s.Length / 4);
            if (x < rate / 20) return s;
            var outp = new float[s.Length - x];
            Array.Copy(s, 0, outp, 0, outp.Length);
            for (int i = 0; i < x; i++)
            {
                float t = (i + 0.5f) / x;
                outp[i] = s[i] * Mathf.Sin(t * Mathf.PI * 0.5f) + s[s.Length - x + i] * Mathf.Cos(t * Mathf.PI * 0.5f);
            }
            return outp;
        }

        private static void Fade(float[] s, int fadeIn, int fadeOut)
        {
            for (int i = 0; i < fadeIn && i < s.Length; i++) s[i] *= i / (float)fadeIn;
            for (int i = 0; i < fadeOut && i < s.Length; i++) s[s.Length - 1 - i] *= i / (float)fadeOut;
        }

        // Loudness from the sound's active part (RMS), never louder than -1 dBFS at its peak.
        private static void Level(float[] s, float targetRmsDb)
        {
            double sum = 0;
            int n = 0;
            float peak = 0f;
            foreach (float v in s)
            {
                float a = Math.Abs(v);
                if (a > peak) peak = a;
                if (a < 0.003f) continue;
                sum += v * v;
                n++;
            }
            if (peak <= 0f || n == 0) return;
            float rms = (float)Math.Sqrt(sum / n);
            float gain = Mathf.Min(Mathf.Pow(10f, targetRmsDb / 20f) / rms, Mathf.Pow(10f, -1f / 20f) / peak);
            for (int i = 0; i < s.Length; i++) s[i] *= gain;
        }

        private static void WriteWav(string path, float[] s, int rate)
        {
            using var w = new BinaryWriter(File.Create(path));
            int bytes = s.Length * 2;
            w.Write("RIFF".ToCharArray()); w.Write(36 + bytes); w.Write("WAVE".ToCharArray());
            w.Write("fmt ".ToCharArray()); w.Write(16); w.Write((short)1); w.Write((short)1); w.Write(rate); w.Write(rate * 2); w.Write((short)2); w.Write((short)16);
            w.Write("data".ToCharArray()); w.Write(bytes);
            foreach (float v in s) w.Write((short)Mathf.Clamp(Mathf.RoundToInt(v * 32767f), -32768, 32767));
        }

        private static void ImportSettings(string path, bool loop, float seconds)
        {
            if (AssetImporter.GetAtPath(path) is not AudioImporter ai) return;
            ai.forceToMono = true;
            ai.loadInBackground = loop;
            AudioImporterSampleSettings s = ai.defaultSampleSettings;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            s.quality = loop ? 0.6f : 0.75f;
            s.loadType = loop || seconds > 6f ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            s.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            ai.defaultSampleSettings = s;
            ai.SaveAndReimport();
        }

        // ---- the bank ----

        private static void Fill()
        {
            var bank = AssetDatabase.LoadAssetAtPath<SoundBank>(SoundBankBuilder.BankPath);
            if (bank == null) { SoundBankBuilder.CreateMissing(); bank = AssetDatabase.LoadAssetAtPath<SoundBank>(SoundBankBuilder.BankPath); }
            var named = bank.Cues.ToList();
            var impacts = bank.Impacts.ToList();
            var steps = bank.Footsteps.ToList();
            int cues = 0, surfaces = 0;

            foreach (SoundId id in Enum.GetValues(typeof(SoundId)))
            {
                AudioClip[] clips = Clips($"Cues/{id}");
                if (clips.Length == 0) continue;
                SoundBank.Named row = named.FirstOrDefault(n => n.Id == id);
                SoundInfo info = SoundCatalog.Info(id);
                Vector2 pitch = Pitch.TryGetValue(id, out Vector2 p) ? p : new Vector2(0.95f, 1.05f);
                var cue = new SoundCue(clips, row?.Cue.Volume ?? info.Volume, pitch, row?.Cue.MinDistance ?? info.MinDistance, row?.Cue.MaxDistance ?? info.MaxDistance);
                named.RemoveAll(n => n.Id == id);
                named.Add(new SoundBank.Named { Id = id, Cue = cue });
                cues++;
            }

            foreach (SurfaceMaterial m in Enum.GetValues(typeof(SurfaceMaterial)))
            {
                SurfaceMaterial stepFrom = StepAlias.TryGetValue(m, out SurfaceMaterial a) ? a : m;
                AudioClip[] step = Clips($"Steps/{stepFrom}");
                if (step.Length > 0)
                {
                    Vector2 pitch = stepFrom == m ? new Vector2(0.94f, 1.06f) : new Vector2(1.02f, 1.12f);
                    steps.RemoveAll(s => s.Material == m);
                    steps.Add(new SoundBank.ByMaterial { Material = m, Light = new SoundCue(step, 0.7f, pitch, 1f, 18f) });
                    surfaces++;
                }
                SurfaceMaterial hitFrom = ImpactAlias.TryGetValue(m, out SurfaceMaterial b) ? b : m;
                AudioClip[] light = Clips($"Impacts/{hitFrom}/Light"), heavy = Clips($"Impacts/{hitFrom}/Heavy");
                if (light.Length > 0 || heavy.Length > 0)
                {
                    impacts.RemoveAll(i => i.Material == m);
                    impacts.Add(new SoundBank.ByMaterial
                    {
                        Material = m,
                        Light = new SoundCue(light.Length > 0 ? light : heavy, 0.9f, new Vector2(0.93f, 1.07f), 1.5f, 30f),
                        Heavy = new SoundCue(heavy.Length > 0 ? heavy : light, 1f, new Vector2(0.9f, 1.04f), 2.5f, 40f),
                    });
                    surfaces++;
                }
            }

            bank.EditorSet(named.OrderBy(n => n.Id).ToList(), impacts.OrderBy(i => i.Material).ToList(), steps.OrderBy(s => s.Material).ToList());
            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SoundLibrary] Bank: {cues} named cues and {surfaces} surface rows now play recordings.");
        }

        private static AudioClip[] Clips(string target)
        {
            string folder = $"{Output}/{target}";
            if (!AssetDatabase.IsValidFolder(folder)) return Array.Empty<AudioClip>();
            return AssetDatabase.FindAssets("t:AudioClip", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => Path.GetDirectoryName(p)!.Replace('\\', '/') == folder)
                .OrderBy(p => p)
                .Select(AssetDatabase.LoadAssetAtPath<AudioClip>)
                .Where(c => c != null)
                .ToArray();
        }
    }
}
