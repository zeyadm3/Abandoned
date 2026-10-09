using System;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.EditorTools;
using NUnit.Framework;
using UnityEditor;

namespace Abandoned.Tests
{
    /// <summary>M7.3: the sound bank covers every surface and every named sound.</summary>
    public class SoundBankTests
    {
        private SoundBank bank;

        [SetUp]
        public void SetUp()
        {
            bank = AssetDatabase.LoadAssetAtPath<SoundBank>(SoundBankBuilder.BankPath);
            Assert.IsNotNull(bank, "run Rebuild Content");
        }

        [Test]
        public void EverySurfaceHasLibraryImpactsAndFootsteps()
        {
            foreach (SurfaceMaterial m in Enum.GetValues(typeof(SurfaceMaterial)))
            {
                Assert.IsTrue(bank.Impact(m, 0.2f)?.HasClips == true, $"{m}: light impact");
                Assert.IsTrue(bank.Impact(m, 1f)?.HasClips == true, $"{m}: heavy impact");
                Assert.IsTrue(bank.Footstep(m)?.HasClips == true, $"{m}: footsteps");
            }
        }

        [Test]
        public void EveryNamedSoundHasACueAndRecordingsWhereTheCatalogSaysSo()
        {
            // 0.14.0: the SoundCatalog says which sounds are recorded (or a recording with a synthesised layer);
            // the rest have an empty row and play their synthesised sound.
            foreach (SoundId id in Enum.GetValues(typeof(SoundId)))
            {
                SoundCue cue = bank.Get(id);
                Assert.IsNotNull(cue, $"{id} has no cue");
                bool recorded = SoundCatalog.Info(id).Mode != SoundMode.Synth;
                Assert.AreEqual(recorded, cue.HasClips, $"{id}: library clips expected exactly when the catalog marks it recorded or layered");
                Assert.Greater(cue.MaxDistance, cue.MinDistance, $"{id}: distances");
            }
            Assert.AreEqual(SoundCatalog.Revision, bank.Revision, "the bank was built for this catalog: run Tools/Abandoned/Audio/Build Sound Bank");
        }
    }
}
