using System.Linq;
using Abandoned.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Abandoned.Tests
{
    public class EnvironmentRemasterTests
    {
        private const string Folder = "Assets/_Project/Art/Remaster/Environment";
        private const string ScenePath = "Assets/_EnvironmentRemasterTest.unity";
        private Material wallMaterial;

        [SetUp]
        public void Setup()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Assert.IsTrue(EditorSceneManager.SaveScene(scene, ScenePath));
            wallMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "QA_PlasterWall" };
            Box("Floor", Vector3.down * .1f, new Vector3(20, .2f, 20));
            Box("Ceiling", Vector3.up * 3.1f, new Vector3(20, .2f, 20));
        }

        [TearDown]
        public void Cleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Object.DestroyImmediate(wallMaterial);
            AssetDatabase.DeleteAsset(ScenePath);
            AssetDatabase.DeleteAsset(Folder + "/_EnvironmentRemasterTest");
            AssetDatabase.DeleteAsset(Folder + "/_EnvironmentRemasterTest_ContactSeal.mat");
            AssetDatabase.DeleteAsset(Folder + "/_EnvironmentRemasterTest_Skirting.mat");
        }

        private GameObject Box(string name, Vector3 at, Vector3 size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = at;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = wallMaterial;
            return go;
        }

        [Test]
        public void FittedDetailsAreRepeatableAndNeverIntroduceCollision()
        {
            Box("Wall", Vector3.up * 1.5f, new Vector3(6, 3, .2f));
            int before = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length;
            var scene = EditorSceneManager.GetActiveScene();
            var first = EnvironmentRemaster.ApplyToScene(scene);
            Assert.Greater(first.Junctions, 0, "a supported interior wall should acquire a fitted junction");
            Assert.AreEqual(before, Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length);
            var again = EnvironmentRemaster.ApplyToScene(scene);
            Assert.AreEqual(first.ToString(), again.ToString());
            Assert.AreEqual(1, scene.GetRootGameObjects().Count(r => r.name == EnvironmentRemaster.RootName));
        }

        [Test]
        public void AnOpenDoorwayDoesNotGainAContinuousSkirtingStrip()
        {
            Box("WallLeft", new Vector3(-2, 1.5f, 0), new Vector3(2.5f, 3, .2f));
            Box("WallRight", new Vector3(2, 1.5f, 0), new Vector3(2.5f, 3, .2f));
            EnvironmentRemaster.ApplyToScene(EditorSceneManager.GetActiveScene());
            Transform root = GameObject.Find(EnvironmentRemaster.RootName).transform;
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>())
            foreach (Vector3 vertex in filter.sharedMesh.vertices)
                Assert.GreaterOrEqual(Mathf.Abs(vertex.x), .72f, "the existing clear doorway must stay clear of even cosmetic geometry");
        }

        [Test]
        public void UnsupportedOutdoorWallsDoNotAcquireInteriorTrim()
        {
            Object.DestroyImmediate(GameObject.Find("Ceiling"));
            Box("Wall", Vector3.up * 1.5f, new Vector3(6, 3, .2f));
            var result = EnvironmentRemaster.ApplyToScene(EditorSceneManager.GetActiveScene());
            Assert.AreEqual(0, result.Junctions);
        }

        [Test]
        public void TheSkirtingProfileHasDepthAndClosedEndsWithinTwoCentimetresOfItsWall()
        {
            var builder = new EnvironmentRemasterMesh();
            builder.Skirting(Vector3.zero, Vector3.right * 3, Vector3.forward, .125f);
            Mesh mesh = builder.Build("Test skirting");
            try
            {
                Assert.That(mesh.bounds.size.z, Is.InRange(.017f, .021f));
                Assert.That(mesh.bounds.size.y, Is.InRange(.12f, .126f));
                Assert.IsTrue(mesh.normals.Any(n => n.x > .99f));
                Assert.IsTrue(mesh.normals.Any(n => n.x < -.99f));
                foreach (int index in mesh.triangles) Assert.That(index, Is.InRange(0, mesh.vertexCount - 1));
            }
            finally { Object.DestroyImmediate(mesh); }
        }
    }
}
