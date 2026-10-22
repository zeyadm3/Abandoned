using System.Collections.Generic;
using Abandoned.Player;
using Abandoned.World;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds Scenes/World: six kilometres of wooded hills and roads (<see cref="WorldTerrain"/>, <see cref="WorldRoads"/>)
    /// under the movable <see cref="WorldRoot"/>, the TestMap's photographed sky, the session objects, the van at the
    /// HQ's yard, and the streamer that loads each place's own scene as the van nears it. Writes the atlas
    /// (Data/World/Resources/WorldAtlas) the game reads. Realtime light only: the land moves under the places.
    /// </summary>
    public static class WorldBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/World.unity";
        public const string AtlasPath = WorldVanBuilder.ConfigFolder + "/WorldAtlas.asset";
        private const string VolumePath = "Assets/_Project/Data/World/World_Volume.asset";

        [MenuItem(ToolMenu.OpenWorld + "Rebuild World", priority = ToolMenu.OpenWorldP)]
        public static void BuildMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        public static void Build()
        {
            List<WorldPoi> places = WorldPlan.Places();
            WriteAtlas(places);
            GameObject vanPrefab = WorldVanBuilder.Build();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            WorldVanTestScene.AddCameraAndDebug();
            Sky(scene);

            var root = new GameObject("WorldRoot").AddComponent<WorldRoot>();
            Debug.Log("[WorldBuild] Shaping the land...");
            List<List<Vector3>> roads = WorldTerrain.Shape(places, WorldPlan.Roads(places));
            Transform land = GreyboxFactory.Group("Land", root.transform);
            WorldTerrain.Tiles(land, places);
            Debug.Log("[WorldBuild] Terrain done; roads...");
            WorldRoads.Build(root.transform, roads);
            root.EditorSetup(WorldRoads.StandIns(root.transform, places));
            Horizon(root.transform);

            NetworkSceneBuilder.Add();
            // The crew starts at the HQ, the first origin: these are the HQ's own crew spawns.
            Transform spawns = new GameObject("PlayerSpawns").transform;
            for (int i = 0; i < 4; i++)
            {
                var spawn = new GameObject($"PlayerSpawn_{i}").transform;
                spawn.SetParent(spawns, false);
                spawn.localPosition = new Vector3(26.2f + i * 1.5f, .05f, 1.8f);
                spawn.gameObject.AddComponent<PlayerSpawnPoint>().EditorSetup(i);
            }
            WorldPoi home = places[0];
            var vanSpawn = new GameObject("VanSpawn").AddComponent<WorldVanSpawner>();
            vanSpawn.transform.SetPositionAndRotation(new Vector3(home.parking.x, .3f, home.parking.z), Quaternion.Euler(0f, 180f, 0f));
            vanSpawn.EditorSetup(vanPrefab.GetComponent<NetworkObject>());
            var world = new GameObject("World");
            world.AddComponent<PoiStreamer>();
            world.AddComponent<WorldDebugView>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[WorldBuild] done: {ScenePath}, {places.Count} places, {roads.Count} roads.");
        }

        private static void WriteAtlas(List<WorldPoi> places)
        {
            PolishAssets.EnsureFolder(WorldVanBuilder.ConfigFolder);
            var atlas = AssetDatabase.LoadAssetAtPath<WorldAtlas>(AtlasPath);
            if (atlas == null)
            {
                atlas = ScriptableObject.CreateInstance<WorldAtlas>();
                AssetDatabase.CreateAsset(atlas, AtlasPath);
            }
            atlas.EditorSetup(places, 0);
            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();
        }

        // The TestMap's golden-hour photograph and grade (copied once into the world's own profile), a low warm sun.
        private static void Sky(Scene scene)
        {
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.LookRotation(-ShowcaseSky.ToSun);
            sun.color = ShowcaseSky.SunColour;
            sun.intensity = ShowcaseSky.SunIntensity;
            sun.shadows = LightShadows.Soft;
            sun.renderingLayerMask = -1;
            RenderSettings.sun = sun;
            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/TestMap/Sky/TestMap_Sky.mat");
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = .65f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.72f, .74f, .76f);
            RenderSettings.fogDensity = .0006f;

            if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumePath) == null)
                AssetDatabase.CopyAsset("Assets/_Project/Art/Lighting/TestMap_Volume.asset", VolumePath);
            var post = new GameObject("PostProcessing").AddComponent<Volume>();
            post.isGlobal = true;
            post.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumePath);
            DynamicGI.UpdateEnvironment();
        }

        // A ring of far hills past the land, so the horizon is land and not the sky's empty lower half.
        private static void Horizon(Transform parent)
        {
            const int segments = 180, rings = 7;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (int r = 0; r < rings; r++)
            {
                float t = r / (rings - 1f);
                float radius = Mathf.Lerp(WorldPlan.Half * 1.35f, 14000f, Mathf.Pow(t, 1.4f));
                for (int s = 0; s <= segments; s++)
                {
                    float a = s * Mathf.PI * 2f / segments;
                    var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    float sky = ShowcaseSitePlan.Fbm(dir.x * 2.2f + 5f, dir.y * 2.2f + 5f, 4);
                    float h = r == 0 ? 60f : Mathf.Lerp(120f, 160f + sky * 900f, Mathf.Pow(t, .6f));
                    vertices.Add(new Vector3(dir.x * radius, h, dir.y * radius));
                }
            }
            for (int r = 0; r < rings - 1; r++)
            for (int s = 0; s < segments; s++)
            {
                int a = r * (segments + 1) + s, b = a + segments + 1;
                triangles.AddRange(new[] { a, a + 1, b, a + 1, b + 1, b });
            }
            var mesh = new Mesh { name = "World_Horizon" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            string path = WorldTerrain.Folder + "/World_Horizon.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            var go = new GameObject("Horizon");
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/TestMap/Materials/TestMap_Distant_Forest.mat");
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }
}
