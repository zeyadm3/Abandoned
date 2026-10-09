using System;
using System.Linq;
using Abandoned.Audio;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Imports original HQ voice lines and adds only their rows to the existing central bank.</summary>
    public static class HqDispatchAudioBuilder
    {
        public const string Folder = "Assets/_Project/Art/Audio/HQDispatch";

        [MenuItem("Tools/Abandoned/HQ/Author Dispatch Audio (0.14.5)")]
        public static void Build()
        {
            SoundBank bank = AssetDatabase.LoadAssetAtPath<SoundBank>(SoundBankBuilder.BankPath);
            if (bank == null) throw new InvalidOperationException("Build the existing SoundBank before authoring HQ dispatch.");
            var named = bank.Cues.ToList();
            Add(SoundId.DispatchBriefingRoutine, "dispatch_routine");
            Add(SoundId.DispatchBriefingPowerOff, "dispatch_power_off");
            Add(SoundId.DispatchBriefingHighThreat, "dispatch_high_threat");
            Add(SoundId.DispatchBriefingSealed, "dispatch_sealed");
            // Preserve impact/footstep rows and all unrelated tuning; Resources keeps the voice clips in builds.
            bank.EditorSet(named, bank.Impacts.ToList(), bank.Footsteps.ToList());
            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssets();

            void Add(SoundId id, string file)
            {
                string path = Folder + "/" + file + ".wav";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) throw new InvalidOperationException("HQ dispatch recording missing: " + path);
                importer.forceToMono = true;
                importer.loadInBackground = false;
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                settings.preloadAudioData = true;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) throw new InvalidOperationException("Could not import original HQ dispatch recording: " + path);
                SoundInfo info = SoundCatalog.Info(id);
                named.RemoveAll(row => row.Id == id);
                named.Add(new SoundBank.Named
                {
                    Id = id,
                    Cue = new SoundCue(new[] { clip }, 1f, Vector2.one, info.MinDistance, info.MaxDistance),
                });
            }
        }
    }
}
