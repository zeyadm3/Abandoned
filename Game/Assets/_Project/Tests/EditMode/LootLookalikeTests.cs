using System.Collections.Generic;
using System.Linq;
using Abandoned.EditorTools;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests.EditMode
{
    /// <summary>0.24.9: loot models never stand as scenery (Docs/LOOT_LOOKALIKES_0.24.9.md).</summary>
    public class LootLookalikeTests
    {
        [Test]
        public void EveryLootFamilyIsALookalikeAndNoSceneryIs()
        {
            foreach (SalvageAssetSpec spec in SalvageAssetCatalog.All)
                Assert.AreEqual(spec.IsLoot, LootLookalikes.Is(spec.Id), spec.Id);
            foreach (SalvageAssetSpec spec in SalvagePickables0157.Specs)
                Assert.IsTrue(LootLookalikes.Is(spec.Id), spec.Id);
        }

        [Test]
        public void EveryFixtureIsSceneryOnly()
        {
            string[] fixtures = LootLookalikes.FixtureIds.ToArray();
            Assert.AreEqual(5, fixtures.Length);
            foreach (string id in fixtures)
            {
                SalvageAssetSpec spec = SalvageAssetCatalog.Find(id);
                Assert.IsNotNull(spec, id);
                Assert.IsFalse(spec.IsLoot, id);
                Assert.IsFalse(LootLookalikes.Is(id), id);
            }
            Assert.AreEqual("fixed_gondola_shelving", LootLookalikes.FixtureFor("modular_shelving"));
            Assert.IsNull(LootLookalikes.FixtureFor("ergonomic_chair"));
        }

        [Test]
        public void CopiedCollidersNeverCarryALootName()
        {
            Assert.AreEqual("Collision_showroom_sofa", LootLookalikeStandIns.ProxyName("showroom_sofa"));
            Assert.AreEqual("Seat", LootLookalikeStandIns.ProxyName("Seat"));
        }

        [Test]
        public void DustSheetFacesOutwardAndCoversTheBox()
        {
            var size = new Vector3(1.2f, .9f, .7f);
            Mesh mesh = DustSheetMesh.Build(size, 7);
            try
            {
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    Vector3 a = vertices[triangles[i]], b = vertices[triangles[i + 1]], c = vertices[triangles[i + 2]];
                    Vector3 face = Vector3.Cross(b - a, c - a);
                    Vector3 centre = (a + b + c) / 3f;
                    // Unity draws the side whose normal is this cross product: it must point away from the object.
                    Vector3 outward = centre.y > size.y * .975f ? Vector3.up : new Vector3(centre.x, 0, centre.z);
                    Assert.Greater(Vector3.Dot(face, outward), 0f, $"triangle {i / 3} faces inward");
                }
                Bounds bounds = mesh.bounds;
                Assert.GreaterOrEqual(bounds.size.x, size.x - .08f);
                Assert.GreaterOrEqual(bounds.size.z, size.z - .08f);
                Assert.AreEqual(0f, bounds.min.y, .001f);
                Assert.GreaterOrEqual(bounds.max.y, size.y);
            }
            finally { Object.DestroyImmediate(mesh); }
        }

        [Test]
        public void TriangleSetCutsOnlyTheSourceTriangles()
        {
            Mesh source = Quad(Vector3.zero);
            Mesh batch = new Mesh();
            var combine = new List<CombineInstance>
            {
                new() { mesh = source, transform = Matrix4x4.Translate(new Vector3(1, 0, 0)) },
                new() { mesh = source, transform = Matrix4x4.Translate(new Vector3(5, 0, 0)) }
            };
            batch.CombineMeshes(combine.ToArray(), true, true);
            var set = new BatchTriangleSet();
            Assert.AreEqual(2, set.Add(source, Matrix4x4.Translate(new Vector3(1, 0, 0))));
            try
            {
                Assert.AreEqual(2, set.Strip(batch));
                Assert.AreEqual(6, batch.GetTriangles(0).Length);
                Assert.IsTrue(batch.GetTriangles(0).All(i => batch.vertices[i].x >= 5f), "the neighbour's quad survives");
                Assert.AreEqual(0, set.Strip(batch));
            }
            finally
            {
                Object.DestroyImmediate(source);
                Object.DestroyImmediate(batch);
            }
        }

        private static Mesh Quad(Vector3 at)
        {
            var mesh = new Mesh
            {
                vertices = new[] { at, at + Vector3.right, at + Vector3.up, at + Vector3.right + Vector3.up },
                triangles = new[] { 0, 2, 3, 0, 3, 1 }
            };
            return mesh;
        }
    }
}
