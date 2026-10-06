using System.Collections;
using Abandoned.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    public class SurfaceTests
    {
        [UnityTest]
        public IEnumerator TestBuildingSurfacesSoundRight()
        {
            yield return TestBuildingScene.Load();
            Assert.AreEqual(SurfaceMaterial.Concrete, Probe(new Vector3(2f, 1f, 2f)));
            Assert.AreEqual(SurfaceMaterial.Wood, Probe(new Vector3(3.5f, 5f, 13f))); // clear of the safe at (2, 14)
            Assert.AreEqual(SurfaceMaterial.Metal, Probe(new Vector3(18f, 3f, 6f)));
            Assert.AreEqual(SurfaceMaterial.Asphalt, Probe(new Vector3(8f, 1f, -8f)));
            Assert.AreEqual(SurfaceMaterial.Dirt, Probe(new Vector3(-10f, 1f, 8f)));
        }

        private static SurfaceMaterial Probe(Vector3 from)
        {
            int mask = ~(1 << GameLayers.PlayerLayer);
            Assert.IsTrue(Physics.Raycast(from, Vector3.down, out RaycastHit hit, 10f, mask, QueryTriggerInteraction.Ignore), $"nothing below {from}");
            return SurfaceTag.Of(hit.collider);
        }
    }
}
