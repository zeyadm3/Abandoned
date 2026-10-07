using System.Collections;
using System.Linq;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>M7.3: library sounds play through the pool; the mall's hum follows its lights.</summary>
    public class GameAudioTests
    {
        [UnityTest]
        public IEnumerator ImpactsPlayALibraryClipFromThePool()
        {
            yield return TestBuildingScene.Load("Mall");
            int before = GameAudio.ImpactCount;
            GameAudio.PlayImpact(SurfaceMaterial.Glass, new Vector3(1f, 1f, 1f), 1f);
            yield return null;
            Assert.AreEqual(before + 1, GameAudio.ImpactCount);
            AudioSource source = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None)
                .FirstOrDefault(s => s.isPlaying && s.transform.position == new Vector3(1f, 1f, 1f));
            Assert.IsNotNull(source, "a pooled source plays at the impact");
            StringAssert.StartsWith("impactGlass_heavy", source.clip.name, "the library's heavy glass, not the synthesised placeholder");
            Assert.AreEqual(1f, source.spatialBlend, "positional");
        }

        [UnityTest]
        public IEnumerator TheMallHumsOnlyWhileItsLightsAreOn()
        {
            yield return TestBuildingScene.Load("Mall");
            LevelAmbience ambience = Object.FindAnyObjectByType<LevelAmbience>();
            Assert.IsNotNull(ambience, "the mall has ambience");
            yield return new WaitForSeconds(0.8f);
            Assert.Greater(ambience.HumLevel, 0.8f, "lights on: humming");
            foreach (LightFixture f in LightFixture.All.ToList()) f.SetPowered(false);
            yield return new WaitForSeconds(0.8f);
            Assert.AreEqual(0f, ambience.HumLevel, 1e-3f, "power off: silence but the wind");
        }
    }
}
