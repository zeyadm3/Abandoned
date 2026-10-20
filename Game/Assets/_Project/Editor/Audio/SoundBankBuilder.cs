using System.Collections.Generic;
using System.Linq;
using Abandoned.Audio;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds Data/Audio/Resources/SoundBank from the imported Kenney CC0 packs (M7.3). Cues and
    /// materials already in the bank keep their clips and tuning; only missing ones are added, so
    /// edits in the inspector survive a rebuild. Since 0.14.0 every <see cref="SoundId"/> has a row:
    /// recordings where the SoundCatalog says the sound is recorded or layered, and an empty row (which
    /// plays the synthesised sound, and can be given a clip to replace it) for the rest. A bank built
    /// before a sound moved to synthesis has that sound's old recordings cleared once, on upgrade.
    /// Also makes Data/Audio/Resources/AudioConfig, the sound system's tunables.
    /// </summary>
    public static class SoundBankBuilder
    {
        public const string Folder = "Assets/_Project/Data/Audio/Resources";
        public const string BankPath = Folder + "/SoundBank.asset";
        public const string ConfigPath = Folder + "/AudioConfig.asset";
        private const string Impact = ThirdPartyModelImport.Root + "Kenney/ImpactSounds/";
        private const string Ui = ThirdPartyModelImport.Root + "Kenney/InterfaceSounds/";
        private const string Rpg = ThirdPartyModelImport.Root + "Kenney/RPGAudio/";

        private static readonly Vector2 Normal = new(0.94f, 1.06f), Low = new(0.55f, 0.7f), VeryLow = new(0.35f, 0.45f);

        [MenuItem(ToolMenu.Audio + "Build Sound Bank", priority = ToolMenu.AudioP)]
        public static void CreateMissing()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Audio")) AssetDatabase.CreateFolder("Assets/_Project/Data", "Audio");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Data/Audio", "Resources");
            var bank = AssetDatabase.LoadAssetAtPath<SoundBank>(BankPath);
            if (bank == null)
            {
                bank = ScriptableObject.CreateInstance<SoundBank>();
                AssetDatabase.CreateAsset(bank, BankPath);
            }

            if (AssetDatabase.LoadAssetAtPath<AudioConfig>(ConfigPath) == null)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AudioConfig>(), ConfigPath);

            var named = bank.Cues.ToList();
            // Upgrading an older bank: sounds that are synthesised now give up the recordings they used to play.
            if (bank.Revision < SoundCatalog.Revision)
            {
                int cleared = 0;
                foreach (SoundBank.Named n in named)
                {
                    if (SoundCatalog.Info(n.Id).Mode != SoundMode.Synth || !n.Cue.HasClips) continue;
                    n.Cue.EditorSetClips(System.Array.Empty<AudioClip>());
                    cleared++;
                }
                Debug.Log($"[SoundBank] Upgraded to catalog revision {SoundCatalog.Revision}: {cleared} cue(s) now play their synthesised sound.");
            }
            void Cue(SoundId id, SoundCue cue)
            {
                if (named.Any(n => n.Id == id && n.Cue.HasClips)) return;
                named.RemoveAll(n => n.Id == id);
                named.Add(new SoundBank.Named { Id = id, Cue = cue });
            }
            // The building: timber creaks, pitched down until they groan; a plank snapping; debris on a collapse.
            Cue(SoundId.Creak, new SoundCue(Clips(Rpg, "creak"), 0.9f, Low, 3f, 35f));
            Cue(SoundId.Groan, new SoundCue(Clips(Rpg, "creak"), 1f, VeryLow, 4f, 45f));
            Cue(SoundId.Snap, new SoundCue(Clips(Impact, "impactPlank_medium"), 1f, new Vector2(0.7f, 0.85f), 3f, 45f));
            Cue(SoundId.Crash, new SoundCue(null, 1f, Normal, 5f, 70f)); // the synthesised rumble
            Cue(SoundId.CrashDebris, new SoundCue(Clips(Impact, "impactWood_heavy").Concat(Clips(Impact, "impactMining")).ToArray(), 1f, new Vector2(0.5f, 0.7f), 5f, 60f));
            // Threats and equipment.
            Cue(SoundId.BlindOneClick, new SoundCue(null, 1f, Normal, 3f, 30f));
            Cue(SoundId.CollectorJingle, new SoundCue(Clips(Rpg, "handleCoins").Concat(Clips(Rpg, "beltHandle")).ToArray(), 0.9f, new Vector2(1f, 1.2f), 2f, 25f));
            Cue(SoundId.Horn, new SoundCue(null, 1f, Normal, 8f, 120f));
            Cue(SoundId.Lever, new SoundCue(Clips(Rpg, "metalLatch"), 1f, new Vector2(0.8f, 0.9f), 2f, 25f));
            Cue(SoundId.NoiseMakerShriek, new SoundCue(null, 1f, Normal, 5f, 60f));
            Cue(SoundId.RadioStatic, new SoundCue(null, 1f, Normal, 1f, 15f));
            Cue(SoundId.FlashlightClick, new SoundCue(Clips(Rpg, "metalClick"), 0.8f, new Vector2(1.2f, 1.35f), 1f, 10f));
            // Loot changing hands; payday.
            Cue(SoundId.LootPickup, new SoundCue(Clips(Rpg, "cloth"), 0.8f, Normal, 1f, 12f));
            Cue(SoundId.LootPocket, new SoundCue(Clips(Rpg, "handleSmallLeather"), 0.9f, Normal, 1f, 10f));
            Cue(SoundId.Coins, new SoundCue(Clips(Rpg, "handleCoins"), 1f, Normal, 1f, 10f));
            // The Hunter (M9.1): its heavy tread, under the synthesised weight of it.
            Cue(SoundId.HunterStep, new SoundCue(Clips(Impact, "impactSoft_heavy"), 1f, new Vector2(0.42f, 0.52f), 4f, 38f));
            // Gear (M9.4): the crowbar's clang, the jack braced.
            Cue(SoundId.CrowbarHit, new SoundCue(Clips(Impact, "impactMetal_heavy"), 1f, new Vector2(0.85f, 1f), 3f, 40f));
            Cue(SoundId.JackPlaced, new SoundCue(Clips(Rpg, "metalLatch"), 1f, new Vector2(0.6f, 0.7f), 2f, 20f));
            // Locked stores (M10.4): the snip of bolt cutters (the shutters themselves are synthesised).
            Cue(SoundId.BoltCut, new SoundCue(Clips(Rpg, "metalClick"), 0.9f, new Vector2(0.7f, 0.8f), 1f, 12f));
            // Menus (M7.4).
            Cue(SoundId.UiClick, new SoundCue(Clips(Ui, "click_"), 0.6f, Normal, 1f, 5f));
            Cue(SoundId.UiConfirm, new SoundCue(Clips(Ui, "confirmation_"), 0.7f, Normal, 1f, 5f));
            Cue(SoundId.UiBack, new SoundCue(Clips(Ui, "back_"), 0.6f, Normal, 1f, 5f));
            Cue(SoundId.UiError, new SoundCue(Clips(Ui, "error_"), 0.6f, Normal, 1f, 5f));
            Cue(SoundId.UiOpen, new SoundCue(Clips(Ui, "open_"), 0.6f, Normal, 1f, 5f));
            Cue(SoundId.UiClose, new SoundCue(Clips(Ui, "close_"), 0.6f, Normal, 1f, 5f));

            // Everything else is synthesised: an empty row with the catalog's range, so it shows in the inspector
            // and a recording dropped into it takes over.
            for (int i = 0; i < SoundCatalog.Count; i++)
            {
                var id = (SoundId)i;
                if (named.Any(n => n.Id == id)) continue;
                SoundInfo info = SoundCatalog.Info(id);
                named.Add(new SoundBank.Named
                {
                    Id = id,
                    Cue = new SoundCue(System.Array.Empty<AudioClip>(), info.Volume, new Vector2(info.PitchLow, info.PitchHigh), info.MinDistance, info.MaxDistance),
                });
            }

            var impacts = bank.Impacts.ToList();
            void Hit(SurfaceMaterial m, string light, string heavy, float volume = 1f)
            {
                if (impacts.Any(i => i.Material == m && i.Light.HasClips)) return;
                impacts.RemoveAll(i => i.Material == m);
                impacts.Add(new SoundBank.ByMaterial
                {
                    Material = m,
                    Light = new SoundCue(Clips(Impact, light), volume, Normal, 1.5f, 30f),
                    Heavy = new SoundCue(Clips(Impact, heavy), volume, Normal, 2.5f, 40f),
                });
            }
            Hit(SurfaceMaterial.Glass, "impactGlass_light", "impactGlass_heavy");
            Hit(SurfaceMaterial.Metal, "impactMetal_light", "impactMetal_heavy");
            Hit(SurfaceMaterial.Wood, "impactWood_light", "impactWood_heavy");
            Hit(SurfaceMaterial.Plastic, "impactGeneric_light", "impactPlate_medium", 0.8f);
            Hit(SurfaceMaterial.Stone, "impactMining", "impactSoft_heavy");
            Hit(SurfaceMaterial.Concrete, "impactMining", "impactSoft_heavy");
            Hit(SurfaceMaterial.Asphalt, "impactSoft_medium", "impactSoft_heavy");
            Hit(SurfaceMaterial.Paper, "impactSoft_medium", "impactSoft_medium", 0.7f);
            Hit(SurfaceMaterial.Fabric, "impactSoft_medium", "impactSoft_heavy", 0.8f);
            Hit(SurfaceMaterial.Dirt, "impactSoft_medium", "impactSoft_heavy", 0.8f);
            Hit(SurfaceMaterial.Terrazzo, "impactMining", "impactSoft_heavy");
            Hit(SurfaceMaterial.Tile, "impactMining", "impactSoft_heavy");
            Hit(SurfaceMaterial.Carpet, "impactSoft_medium", "impactSoft_heavy", 0.8f);
            // No imported splash clip is available: an empty authored entry uses the original synthesis.
            if (!impacts.Any(i => i.Material == SurfaceMaterial.Water))
                impacts.Add(new SoundBank.ByMaterial { Material = SurfaceMaterial.Water });

            var steps = bank.Footsteps.ToList();
            void Step(SurfaceMaterial m, string clips, float volume, Vector2 pitch)
            {
                if (steps.Any(s => s.Material == m && s.Light.HasClips)) return;
                steps.RemoveAll(s => s.Material == m);
                steps.Add(new SoundBank.ByMaterial { Material = m, Light = new SoundCue(Clips(Impact, clips), volume, pitch, 1f, 18f) });
            }
            Step(SurfaceMaterial.Concrete, "footstep_concrete", 0.7f, Normal);
            Step(SurfaceMaterial.Stone, "footstep_concrete", 0.7f, Normal);
            Step(SurfaceMaterial.Asphalt, "footstep_concrete", 0.7f, new Vector2(0.85f, 0.95f));
            Step(SurfaceMaterial.Plastic, "footstep_concrete", 0.6f, new Vector2(1.05f, 1.15f));
            Step(SurfaceMaterial.Metal, "footstep_concrete", 0.7f, new Vector2(1.2f, 1.35f));
            Step(SurfaceMaterial.Wood, "footstep_wood", 0.7f, Normal);
            Step(SurfaceMaterial.Fabric, "footstep_carpet", 0.6f, Normal);
            Step(SurfaceMaterial.Paper, "footstep_carpet", 0.6f, Normal);
            Step(SurfaceMaterial.Dirt, "footstep_grass", 0.6f, Normal);
            Step(SurfaceMaterial.Glass, "footstep_snow", 0.7f, new Vector2(1.1f, 1.25f)); // crunching on shards
            Step(SurfaceMaterial.Terrazzo, "footstep_concrete", 0.7f, Normal);
            Step(SurfaceMaterial.Tile, "footstep_concrete", 0.7f, new Vector2(1.05f, 1.15f));
            Step(SurfaceMaterial.Carpet, "footstep_carpet", 0.6f, Normal);
            if (!steps.Any(s => s.Material == SurfaceMaterial.Water))
                steps.Add(new SoundBank.ByMaterial { Material = SurfaceMaterial.Water });

            bank.EditorSet(named.OrderBy(n => n.Id).ToList(), impacts.OrderBy(i => i.Material).ToList(), steps.OrderBy(s => s.Material).ToList());
            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssets();
        }

        /// <summary>Every clip in a folder whose file name starts with the prefix, in name order.</summary>
        private static AudioClip[] Clips(string folder, string prefix) =>
            AssetDatabase.FindAssets("t:AudioClip", new[] { folder.TrimEnd('/') })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => System.IO.Path.GetFileName(p).StartsWith(prefix))
                .OrderBy(p => p)
                .Select(AssetDatabase.LoadAssetAtPath<AudioClip>)
                .Where(c => c != null)
                .ToArray();
    }
}
