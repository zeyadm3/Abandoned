using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Abandoned.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Abandoned.Tests
{
    public class ObjectRemasterMigrationTests
    {
        private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
        private object previousBook;
        private HashSet<string> previousCacheKeys;
        private string folder, outputPath;
        private bool createdFolder, createdConfig;
        private GameObject instance;
        private Mesh source, previousOutput;
        private Material material;
        private static FieldInfo BookField => typeof(ObjectRemaster).GetField("book", PrivateStatic);
        private static IDictionary Cache => (IDictionary)typeof(ObjectRemaster).GetField("cache", PrivateStatic).GetValue(null);

        [SetUp]
        public void Setup()
        {
            createdFolder = createdConfig = false;
            previousBook = BookField.GetValue(null);
            previousCacheKeys = Cache.Keys.Cast<string>().ToHashSet();
            createdFolder = !AssetDatabase.IsValidFolder(ObjectRemaster.Folder);
            typeof(ObjectRemaster).GetMethod("FolderAt", PrivateStatic).Invoke(null, new object[] { ObjectRemaster.Folder });
            if (AssetDatabase.LoadAssetAtPath<ObjectRemasterConfig>(ObjectRemaster.ConfigPath) == null)
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ObjectRemasterConfig>(), ObjectRemaster.ConfigPath);
                createdConfig = true;
            }
            string token = System.Guid.NewGuid().ToString("N");
            folder = ObjectRemaster.Folder + "/_MigrationTest_" + token;
            AssetDatabase.CreateFolder(ObjectRemaster.Folder, "_MigrationTest_" + token);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            source = Object.Instantiate(cube.GetComponent<MeshFilter>().sharedMesh);
            source.name = "MigrationCabinet";
            Object.DestroyImmediate(cube);
            string sourcePath = folder + "/source.asset";
            AssetDatabase.CreateAsset(source, sourcePath);
            previousOutput = Object.Instantiate(source);
            outputPath = ObjectRemaster.Folder + "/Prop_MigrationTest_" + token + ".asset";
            AssetDatabase.CreateAsset(previousOutput, outputPath);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out string guid, out long localId);
            string json = $"{{\"entries\":[{{\"output\":\"{outputPath}\",\"sourcePath\":\"{sourcePath}\",\"sourceGuid\":\"{guid}\",\"sourceId\":{localId}}}]}}";
            BookField.SetValue(null, JsonUtility.FromJson(json, BookField.FieldType));
            instance = new GameObject("MigrationCabinet");
            instance.AddComponent<MeshFilter>().sharedMesh = previousOutput;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Migration Steel" };
            instance.AddComponent<MeshRenderer>().sharedMaterial = material;
            instance.AddComponent<MeshCollider>().sharedMesh = source;
        }

        [TearDown]
        public void Cleanup()
        {
            if (instance != null) Object.DestroyImmediate(instance);
            if (material != null) Object.DestroyImmediate(material);
            BookField.SetValue(null, previousBook);
            foreach (string key in Cache.Keys.Cast<string>().Where(key => !previousCacheKeys.Contains(key)).ToArray()) Cache.Remove(key);
            if (!string.IsNullOrEmpty(outputPath)) AssetDatabase.DeleteAsset(outputPath);
            if (!string.IsNullOrEmpty(folder)) AssetDatabase.DeleteAsset(folder);
            if (createdConfig) AssetDatabase.DeleteAsset(ObjectRemaster.ConfigPath);
            if (createdFolder) AssetDatabase.DeleteAsset(ObjectRemaster.Folder);
        }

        private ObjectRemaster.Result Apply() => (ObjectRemaster.Result)typeof(ObjectRemaster)
            .GetMethod("Apply", PrivateStatic).Invoke(null, new object[] { instance.transform });

        [TestCase("candidate")]
        [TestCase("transparent")]
        [TestCase("malformed")]
        [TestCase("unsupported")]
        public void RejectedPreviousOutputRestoresOriginalAndReportsChange(string reason)
        {
            if (reason == "candidate") instance.name = "Wall";
            if (reason == "transparent") material.renderQueue = 3000;
            if (reason == "malformed")
            {
                int[] triangles = source.triangles;
                for (int i = 0; i < 3; i++) triangles[i + 3] = triangles[i];
                source.triangles = triangles;
            }
            if (reason == "unsupported") source.SetIndices(new[] { 0, 1 }, MeshTopology.Lines, 0);

            ObjectRemaster.Result result = Apply();
            Assert.AreSame(source, instance.GetComponent<MeshFilter>().sharedMesh);
            Assert.AreSame(source, instance.GetComponent<MeshCollider>().sharedMesh, "migration changes only the renderer mesh");
            Assert.AreEqual(1, result.ChangedRendererCount, "prefab saving must include restored original meshes");
            Assert.AreEqual(0, result.GeneratedMeshCount);
            Assert.AreEqual(0, result.BevelledPartCount);
            Assert.AreEqual(0, Apply().ChangedRendererCount, "an already-restored original is unchanged");

            instance.GetComponent<MeshFilter>().sharedMesh = previousOutput;
            Assert.AreEqual(1, Apply().ChangedRendererCount, "repeated rejections, including cached null builds, still migrate");
            Assert.AreSame(source, instance.GetComponent<MeshFilter>().sharedMesh);
        }

        [Test]
        public void RestoringOriginalRecordsThePrefabMeshOverride()
        {
            var savedMaterial = new Material(material);
            AssetDatabase.CreateAsset(savedMaterial, folder + "/material.mat");
            instance.GetComponent<MeshRenderer>().sharedMaterial = savedMaterial;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, folder + "/cabinet.prefab");
            Object.DestroyImmediate(instance);
            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "Wall";

            Assert.AreEqual(1, Apply().ChangedRendererCount);
            Assert.IsTrue(PrefabUtility.GetPropertyModifications(instance)
                .Any(change => change.propertyPath == "m_Mesh" && change.objectReference == source));
            Assert.AreSame(source, instance.GetComponent<MeshCollider>().sharedMesh);
        }
    }
}
