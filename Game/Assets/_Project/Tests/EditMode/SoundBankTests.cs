using System;
using System.Linq;
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
        public void EveryNamedSoundHasACueAndOnlySignaturesAreSynthesised()
        {
            SoundId[] synthesised = { SoundId.Crash, SoundId.BlindOneClick, SoundId.Horn, SoundId.NoiseMakerShriek, SoundId.RadioStatic };
            foreach (SoundId id in Enum.GetValues(typeof(SoundId)))
            {
                SoundCue cue = bank.Get(id);
                Assert.IsNotNull(cue, $"{id} has no cue");
                Assert.AreEqual(!synthesised.Contains(id), cue.HasClips, $"{id}: library clips expected unless it's a synthesised signature");
                Assert.Greater(cue.MaxDistance, cue.MinDistance, $"{id}: distances");
            }
        }
    }
}
