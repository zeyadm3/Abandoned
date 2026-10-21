using System.Linq;
using Abandoned.EditorTools;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Abandoned.Tests
{
    public class TutorialHouseArtTests
    {
        private Scene scene;
        private Transform house;

        [SetUp]
        public void OpenHouse()
        {
            scene = EditorSceneManager.OpenScene(TutorialHouseLayout.ScenePath, OpenSceneMode.Single);
            house = scene.GetRootGameObjects().First(g => g.name == "Tutorial").transform;
        }

        [TearDown]
        public void CloseHouse() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        [Test]
        public void OriginalModelsHaveBoundMaterialsAndMetricBounds()
        {
            foreach (string path in new[] { "Rooms/Living/Sofa", "Rooms/BedroomA/TwinBed", "Rooms/BedroomB/QueenBed", "Rooms/HallBath/Toilet" })
            {
                Transform piece = house.Find(path);
                Assert.IsNotNull(piece, path);
                var group = piece.GetComponentInChildren<LODGroup>();
                Assert.IsNotNull(group, path);
                Assert.AreEqual(3, group.lodCount, path);
                Bounds bounds = new Bounds(piece.position, Vector3.zero);
                foreach (var renderer in group.GetLODs()[0].renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                    // URP Lit, or its 0.24.13 drop-in with weathering (MaterialUpgrade).
                    Assert.IsTrue(renderer.sharedMaterials.All(m => m != null && (m.shader.name == "Universal Render Pipeline/Lit" || m.shader.name == MaterialUpgrade.SurfaceShader)), path);
                    Assert.IsTrue(renderer.sharedMaterials.All(m => !m.name.StartsWith("MALL_")), path);
                }
                Assert.Greater(bounds.size.magnitude, .4f, "FBX import must not shrink to centimetres: " + path);
                Assert.Less(bounds.size.magnitude, 3.5f, "FBX import must not grow 100x: " + path);
                Assert.Less(Mathf.Abs(bounds.min.y-piece.position.y), .12f, "floor pivot: " + path);
            }
            Assert.IsNotNull(house.Find("Rooms/BedroomB/QueenBed").GetComponent<SectionProp>(), "replacement bed retains its collapse owner");
        }

        [Test]
        public void HouseFinishesHaveCorrectColourNormalAndLinearMaskImports()
        {
            foreach (string name in new[] { "CarpetBeige", "OakFloor", "BathTile", "KitchenLino", "CabinetOak", "SofaFabric", "BasementBlock", "InteriorPlaster" })
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>($"Assets/_Project/Art/Polish/House_{name}.mat");
                Assert.IsNotNull(material, name);
                foreach (string property in new[] { "_BaseMap", "_BumpMap", "_MetallicGlossMap" }) Assert.IsNotNull(material.GetTexture(property), name + property);
                // 0.24.13: a scanned set may come without an occlusion map (Tiles107 has none); a drawn one always has it.
                bool scanned = AssetDatabase.GetAssetPath(material.GetTexture("_BaseMap")).StartsWith(MaterialUpgrade.ScannedRoot);
                if (!scanned) Assert.IsNotNull(material.GetTexture("_OcclusionMap"), name + "_OcclusionMap");
                var colour = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(material.GetTexture("_BaseMap")));
                var normal = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(material.GetTexture("_BumpMap")));
                var mask = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(material.GetTexture("_MetallicGlossMap")));
                Assert.IsTrue(colour.sRGBTexture, name);
                Assert.AreEqual(TextureImporterType.NormalMap, normal.textureType, name);
                Assert.IsFalse(mask.sRGBTexture, name);
                Assert.IsTrue(colour.mipmapEnabled, name);
            }
        }

        [Test]
        public void MainHaulingRoutesRemainConnectedOnTheSavedNavMesh()
        {
            Vector3 destination = new Vector3(28, 0, 10.25f);
            Assert.IsTrue(NavMesh.SamplePosition(destination, out var end, 1, NavMesh.AllAreas));
            foreach (var start in new[] { new Vector3(6.8f,0,14), new Vector3(6.8f,0,8), new Vector3(11,0,12), new Vector3(20.8f,0,4.5f) })
            {
                Assert.IsTrue(NavMesh.SamplePosition(start, out var hit, 1, NavMesh.AllAreas), start.ToString());
                var path = new NavMeshPath();
                Assert.IsTrue(NavMesh.CalculatePath(hit.position, end.position, NavMesh.AllAreas, path), start.ToString());
                Assert.AreEqual(NavMeshPathStatus.PathComplete, path.status, start.ToString());
            }
        }

        [Test]
        public void NoRoofIciclesHangInsideTheHouse()
        {
            foreach (var renderer in house.Find("Roof").GetComponentsInChildren<MeshRenderer>())
                if (renderer.name == "Shingles") Assert.Greater(renderer.bounds.min.y, TutorialHouseLayout.Ceiling + .05f, "roof underside must clear adjoining ceilings");
            foreach (Transform item in house.Find("Roof").GetComponentsInChildren<Transform>())
            {
                if (item.name != "Icicle") continue;
                var p = new Vector2(item.position.x, item.position.z);
                Assert.IsFalse(TutorialHouseLayout.WestWing.Contains(p) || TutorialHouseLayout.Band.Contains(p) || TutorialHouseLayout.Suite.Contains(p), item.position.ToString());
            }
        }
    }
}
