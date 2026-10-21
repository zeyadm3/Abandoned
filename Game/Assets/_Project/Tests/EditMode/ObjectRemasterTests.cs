using System.Linq;
using Abandoned.EditorTools;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    public class ObjectRemasterTests
    {
        private Mesh source;
        private Material material;
        private ObjectRemasterConfig config;

        [SetUp]
        public void Setup()
        {
            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            source = Object.Instantiate(primitive.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(primitive);
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Test Wood" };
            config = ScriptableObject.CreateInstance<ObjectRemasterConfig>();
        }

        [TearDown]
        public void Cleanup()
        {
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(material);
            Object.DestroyImmediate(config);
        }

        private Mesh Build(Matrix4x4 scale, out int count)
        {
            ObjectRemasterMeshData data = ObjectRemasterMeshData.Read(source);
            return ObjectRemasterBevel.Build(data, data.Boxes(new[] { true }), scale, new[] { material }, config, out count);
        }

        [Test]
        public void BevelCreatesClosedInwardEdgesWithoutExpandingTheOriginalBox()
        {
            Mesh output = Build(Matrix4x4.identity, out int count);
            try
            {
                Assert.AreEqual(1, count);
                Assert.AreEqual(44, output.triangles.Length / 3);
                Assert.AreEqual(source.bounds.center, output.bounds.center);
                Assert.That(Vector3.Distance(source.bounds.size, output.bounds.size), Is.LessThan(1e-5f));
                foreach (Vector3 point in output.vertices)
                    Assert.IsTrue(source.bounds.Contains(point), $"a visual bevel cannot expand the authored collision envelope: {point}");
                Vector3[] vertices = output.vertices, normals = output.normals;
                int[] triangles = output.triangles;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
                    Vector3 normal = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
                    Assert.Greater(Vector3.Dot(normal, normals[a] + normals[b] + normals[c]), 0f, "no inward or degenerate triangles");
                }
                foreach (Vector2 uv in output.uv)
                    Assert.IsTrue(uv.x >= -.001f && uv.x <= 1.001f && uv.y >= -.001f && uv.y <= 1.001f, "original UV islands survive");
            }
            finally { Object.DestroyImmediate(output); }
        }

        [Test]
        public void RotatedPartsInsideCombinedMeshesAreRecognized()
        {
            Matrix4x4 transform = Matrix4x4.TRS(new Vector3(1.3f, .7f, -.8f), Quaternion.Euler(13f, 29f, 7f), new Vector3(.7f, .2f, 1.1f));
            source.vertices = source.vertices.Select(transform.MultiplyPoint3x4).ToArray();
            source.RecalculateBounds();
            Mesh output = Build(Matrix4x4.Scale(new Vector3(1.2f, .8f, 1.3f)), out int count);
            try
            {
                Assert.IsNotNull(output);
                Assert.AreEqual(1, count);
                Matrix4x4 inverse = transform.inverse;
                foreach (Vector3 point in output.vertices)
                {
                    Vector3 local = inverse.MultiplyPoint3x4(point);
                    Assert.That(Mathf.Max(Mathf.Abs(local.x), Mathf.Abs(local.y), Mathf.Abs(local.z)), Is.LessThanOrEqualTo(.5001f));
                }
            }
            finally { Object.DestroyImmediate(output); }
        }

        [Test]
        public void NonReadableImportedStyleMeshDoesNotBecomeReadable()
        {
            source.UploadMeshData(true);
            Assert.IsFalse(source.isReadable);
            Mesh output = Build(Matrix4x4.identity, out int count);
            try
            {
                Assert.IsNotNull(output);
                Assert.AreEqual(1, count);
                Assert.IsFalse(source.isReadable, "editor extraction must not change import/runtime CPU readability");
            }
            finally { Object.DestroyImmediate(output); }
        }

        [Test]
        public void OpenOrTransparentGeometryIsNeverRemodelled()
        {
            ObjectRemasterMeshData data = ObjectRemasterMeshData.Read(source);
            Assert.IsEmpty(data.Boxes(new[] { false }));
            source.triangles = source.triangles.Take(30).ToArray();
            data = ObjectRemasterMeshData.Read(source);
            Assert.IsEmpty(data.Boxes(new[] { true }), "a wall panel, open shell or damaged mesh isn't a closed box");
        }

        [Test]
        public void DuplicateFaceTrianglesAreNeverRemodelled()
        {
            int[] triangles = source.triangles;
            for (int i = 0; i < 3; i++) triangles[i + 3] = triangles[i];
            source.triangles = triangles;
            ObjectRemasterMeshData data = ObjectRemasterMeshData.Read(source);
            Assert.IsEmpty(data.Boxes(new[] { true }), "twelve triangles and eight corners do not guarantee a closed box");
        }

        [Test]
        public void ThreeDimensionalTextureChannelsKeepTheirDimension()
        {
            source.SetUVs(2, source.vertices.Select(p => new Vector3(p.x + .5f, p.y + .5f, p.z + .5f)).ToList());
            Mesh output = Build(Matrix4x4.identity, out int count);
            try
            {
                Assert.AreEqual(1, count);
                Assert.AreEqual(3, output.GetVertexAttributeDimension(UnityEngine.Rendering.VertexAttribute.TexCoord2));
            }
            finally { Object.DestroyImmediate(output); }
        }

        [Test]
        public void AFaceKeepsItsOriginalMaterialSlot()
        {
            int[] triangles = source.triangles;
            source.subMeshCount = 6;
            for (int face = 0; face < 6; face++) source.SetTriangles(triangles.Skip(face * 6).Take(6).ToArray(), face);
            ObjectRemasterMeshData data = ObjectRemasterMeshData.Read(source);
            bool[] allowed = Enumerable.Repeat(true, 6).ToArray();
            Mesh output = ObjectRemasterBevel.Build(data, data.Boxes(allowed), Matrix4x4.identity,
                Enumerable.Repeat(material, 6).ToArray(), config, out int count);
            try
            {
                Assert.AreEqual(1, count);
                Assert.AreEqual(6, output.subMeshCount);
                for (int face = 0; face < 6; face++) Assert.IsNotEmpty(output.GetTriangles(face));
            }
            finally { Object.DestroyImmediate(output); }
        }
    }
}
