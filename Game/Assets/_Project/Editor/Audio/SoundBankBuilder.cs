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
    /// edits in the inspector survive a rebuild. Cues with no fitting library sound stay empty and
    /// play the synthesised placeholder (the Blind One's click, the horn, the shriek, static).
    /// </summary>
    public static class SoundBankBuilder
    {
        public const string Folder = "Assets/_Project/Data/Audio/Resources";
        public const string BankPath = Folder + "/SoundBank.asset";
        private const string Impact = ThirdPartyModelImport.Root + "Kenney/ImpactSounds/";
        private const string Ui = ThirdPartyModelImport.Root + "Kenney/InterfaceSounds/";
        private const string Rpg = ThirdPartyModelImport.Root + "Kenney/RPGAudio/";

        private static readonly Vector2 Normal = new(0.94f, 1.06f), Low = new(0.55f, 0.7f), VeryLow = new(0.35f, 0.45f);

        [MenuItem("Tools/Abandoned/Audio/Build Sound Bank")]
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

            var named = bank.Cues.ToList();
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
            // Far-off settling: metal ticking and a pot rolling somewhere, never a creak (creaks are warnings).
            Cue(SoundId.DistantSettle, new SoundCue(Clips(Rpg, "metalPot").Concat(Clips(Impact, "impactTin_medium")).ToArray(), 0.5f, Low, 6f, 40f));
            // Menus (M7.4).
            Cue(SoundId.UiClick, new SoundCue(Clips(Ui, "click_"), 0.6f, Normal, 1f, 5f));
            Cue(SoundId.UiConfirm, new SoundCue(Clips(Ui, "confirmation_"), 0.7f, Normal, 1f, 5f));
            Cue(SoundId.UiBack, new SoundCue(Clips(Ui, "back_"), 0.6f, Normal, 1f, 5f));
            Cue(SoundId.UiError, new SoundCue(Clips(Ui, "error_"), 0.6f, Normal, 1f, 5f));
            Cue(SoundId.UiOpen, new SoundCue(Clips(Ui, "open_"), 0.6f, Normal, 1f, 5f));
            Cue(SoundId.UiClose, new SoundCue(Clips(Ui, "close_"), 0.6f, Normal, 1f, 5f));

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
