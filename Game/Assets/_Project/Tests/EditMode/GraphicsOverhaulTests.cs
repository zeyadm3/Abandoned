using System.Linq;
using Abandoned.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>0.24.13: the coplanar-face fix, the surface shader and the scan rules (Docs/GRAPHICS_0.24.13.md).</summary>
    public class GraphicsOverhaulTests
    {
        [SetUp]
        public void NewScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        private static Transform Box(string name, Vector3 centre, Vector3 size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = centre;
            go.transform.localScale = size;
            return go.transform;
        }

        private static float ClosestGap() =>
            GraphicsAudit.FindFights(out _, 0.01f).Select(f => f.Gap).DefaultIfEmpty(1f).Min();

        [Test]
        public void FlushBoxesAreFoundAsOverlappingCoplanarFaces()
        {
            // A kerb laid on its walk: same bottom plane, overlapping footprint.
            Box("Walk", new Vector3(0f, 0.1f, 0f), new Vector3(4f, 0.2f, 2f));
            Box("Kerb", new Vector3(0f, 0.15f, 0.9f), new Vector3(4f, 0.3f, 0.2f));
            var fights = GraphicsAudit.FindFights(out _);
            Assert.That(fights.Any(f => f.Gap < 0.0001f && f.Area > 0.5f), "the shared bottom plane should be found");
        }

        [Test]
        public void TheFixLeavesEveryOverlappingPairAtLeastAMillimetreApart()
        {
            Box("Walk", new Vector3(0f, 0.1f, 0f), new Vector3(4f, 0.2f, 2f));
            Box("Kerb", new Vector3(0f, 0.15f, 0.9f), new Vector3(4f, 0.3f, 0.2f));
            // Three panels in a chain, all flush: the case plain "move the smaller one" got wrong.
            Box("PanelA", new Vector3(0f, 2f, 5f), new Vector3(2f, 2f, 0.1f));
            Box("PanelB", new Vector3(1f, 2f, 5f), new Vector3(2f, 2f, 0.1f));
            Box("PanelC", new Vector3(2f, 2f, 5f), new Vector3(2f, 2f, 0.1f));
            Assert.Less(ClosestGap(), 0.0001f);
            var (_, _, left) = CoplanarFix.FixOpenScene(save: false);
            Assert.AreEqual(0, left);
            Assert.GreaterOrEqual(ClosestGap(), 0.001f);
        }

        [Test]
        public void TheFixMovesNothingWhenNothingTouches()
        {
            Transform a = Box("A", Vector3.zero, Vector3.one);
            Transform b = Box("B", new Vector3(3f, 0f, 0f), Vector3.one);
            var (moved, faces, left) = CoplanarFix.FixOpenScene(save: false);
            Assert.AreEqual((0, 0, 0), (moved, faces, left));
            Assert.AreEqual(Vector3.zero, a.position);
            Assert.AreEqual(new Vector3(3f, 0f, 0f), b.position);
        }

        [Test]
        public void TheSurfaceShaderCompiles()
        {
            Shader shader = Shader.Find(MaterialUpgrade.SurfaceShader);
            Assert.IsNotNull(shader);
            Assert.IsFalse(ShaderUtil.ShaderHasError(shader), "Abandoned/Surface has compile errors");
            Assert.IsTrue(shader.isSupported);
        }

        [TestCase("IronFactory_Ballast", "", "Gravel023")] // not steel: the map prefix is ignored
        [TestCase("Island_Wall", "SootBrick_BaseColor", "Bricks106A")] // the drawn map says what the wall is
        [TestCase("MALL_Ceiling", "MALL_Ceiling_BaseColor", "OfficeCeiling001")]
        [TestCase("Burley_LockerGreen", "", "PaintedMetal012")]
        [TestCase("Winter_SnowyAsphalt", "", "Snow013")]
        [TestCase("Burley_FrostedGlass", "", null)] // glass is never scanned
        [TestCase("House_WindowGlass", "", null)]
        public void MaterialsGetTheScanTheirNameSays(string material, string map, string expected) =>
            Assert.AreEqual(expected, MaterialUpgrade.ScanFor(material, map));
    }
}
