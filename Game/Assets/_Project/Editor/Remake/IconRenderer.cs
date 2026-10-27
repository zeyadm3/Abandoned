using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Abandoned.Company;
using Abandoned.Equipment;
using Abandoned.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Phase 8 of the 2026 remake: a picture of every remade model for the UI, rendered alone on a transparent ground in an
    /// empty scene: gear (Art/Remake/Icons/Gear/&lt;id&gt;.png, registered as "item/&lt;id&gt;" so it replaces the drawn
    /// silhouette of the same name), hats and accessories, workers and outfits ("cosmetic/&lt;id&gt;"), truck fittings
    /// ("truckcos/&lt;id&gt;"); loot is re-rendered by <see cref="LootIconRenderer"/> with the remastered models. Then the
    /// icon registry (UiIcons) is rebuilt. Every run redoes them all.
    /// </summary>
    public static class IconRenderer
    {
        public const string Gear = RemakePaths.Icons + "/Gear";
        public const string Cosmetics = RemakePaths.Icons + "/Cosmetics";
        public const string Truck = RemakePaths.Icons + "/Truck";
        private const int Size = 192;

        [MenuItem(ToolMenu.UI + "Render Remake Icons (gear, cosmetics, truck, loot)", priority = ToolMenu.UiP + 5)]
        public static void RenderAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() && !Application.isBatchMode) return;
            int made = 0;
            using (var studio = new Studio())
            {
                foreach (string guid in AssetDatabase.FindAssets("t:EquipmentDefinition"))
                {
                    var d = AssetDatabase.LoadAssetAtPath<EquipmentDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                    GameObject model = d != null ? GearModels.For(d.Kind) : null;
                    if (model != null && studio.Shoot(Instantiate(model), $"{Gear}/{d.Id}.png", new Vector3(-1f, .45f, .35f))) made++;
                }
                foreach (string guid in AssetDatabase.FindAssets("t:CosmeticDefinition"))
                {
                    var d = AssetDatabase.LoadAssetAtPath<CosmeticDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                    if (d == null || string.IsNullOrEmpty(d.Id)) continue;
                    GameObject subject = d.HatPrefab != null ? Instantiate(d.HatPrefab)
                        : d.LookMesh != null ? Static(d.LookMesh, d.LookMaterials)
                        : d.BuildMeshes != null && d.BuildMeshes.Length > 0 && d.BuildMeshes[0] != null ? Static(d.BuildMeshes[0], d.LookMaterials)
                        : null;
                    if (subject == null) continue;
                    // Workers are portraits (front, eye level); everything else a three-quarter view.
                    Vector3 from = d.Kind == CosmeticKind.Character ? new Vector3(-.25f, .08f, 1f) : d.Kind == CosmeticKind.Outfit
                        ? new Vector3(-.4f, .15f, 1f) : new Vector3(-.6f, .45f, 1f);
                    if (studio.Shoot(subject, $"{Cosmetics}/{d.Id}.png", from)) made++;
                }
                foreach (string guid in AssetDatabase.FindAssets("t:TruckCosmeticDefinition"))
                {
                    var d = AssetDatabase.LoadAssetAtPath<TruckCosmeticDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                    if (d != null && d.Part != null && studio.Shoot(Instantiate(d.Part), $"{Truck}/{d.Id}.png", new Vector3(-.7f, .4f, 1f))) made++;
                }
            }
            AssetDatabase.Refresh();
            int loot = LootIconRenderer.Render(force: true);
            UiContentBuilder.BuildIcons();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Remake] {made} remake icons and {loot} loot icons rendered; icon registry rebuilt.");
        }

        private static GameObject Instantiate(GameObject asset)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return go;
        }

        /// <summary>A look mesh (skinned, in feet space at its rest pose) drawn as a plain mesh.</summary>
        private static GameObject Static(Mesh mesh, Material[] materials)
        {
            var go = new GameObject("Look");
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterials = materials != null && materials.Length > 0 ? materials : new[] { new Material(Shader.Find("Universal Render Pipeline/Lit")) };
            return go;
        }

        /// <summary>An empty scene with a key, a fill, a rim and a camera into a transparent texture.</summary>
        private sealed class Studio : IDisposable
        {
            private readonly Camera camera;
            private readonly RenderTexture rt;
            private readonly Texture2D tex;
            private readonly string root = Directory.GetParent(Application.dataPath).FullName;

            public Studio()
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(.42f, .44f, .48f);
                Lamp("Key", 1.35f, new Vector3(38f, -40f, 0f), new Color(1f, .95f, .88f));
                Lamp("Fill", .45f, new Vector3(12f, 140f, 0f), new Color(.75f, .82f, 1f));
                Lamp("Rim", .9f, new Vector3(20f, 175f, 0f), new Color(1f, .85f, .6f));
                camera = new GameObject("IconCamera").AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
                camera.fieldOfView = 20f;
                camera.nearClipPlane = .01f;
                camera.farClipPlane = 200f;
                rt = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                camera.targetTexture = rt;
                tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            }

            private static void Lamp(string name, float intensity, Vector3 euler, Color color)
            {
                var light = new GameObject(name).AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = intensity;
                light.color = color;
                light.transform.rotation = Quaternion.Euler(euler);
            }

            public bool Shoot(GameObject subject, string assetPath, Vector3 from)
            {
                try
                {
                    // Only the closest level of detail, and nothing that isn't a drawn surface.
                    foreach (LODGroup g in subject.GetComponentsInChildren<LODGroup>()) g.ForceLOD(0);
                    Renderer[] renderers = subject.GetComponentsInChildren<Renderer>()
                        .Where(r => r.enabled && !(r is ParticleSystemRenderer) && !r.name.Contains("_LOD1") && !r.name.Contains("_LOD2")).ToArray();
                    foreach (Renderer r in subject.GetComponentsInChildren<Renderer>())
                        if (!renderers.Contains(r)) r.enabled = false;
                    if (renderers.Length == 0) return false;
                    Bounds b = renderers[0].bounds;
                    foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
                    float distance = b.extents.magnitude / Mathf.Sin(camera.fieldOfView * .5f * Mathf.Deg2Rad) * 1.04f;
                    camera.transform.position = b.center + from.normalized * distance;
                    camera.transform.LookAt(b.center);
                    camera.Render();
                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
                    tex.Apply();
                    RenderTexture.active = null;
                    string file = Path.Combine(root, assetPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(file));
                    File.WriteAllBytes(file, tex.EncodeToPNG());
                    return true;
                }
                finally { Object.DestroyImmediate(subject); }
            }

            public void Dispose()
            {
                camera.targetTexture = null;
                rt.Release();
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(tex);
            }
        }
    }
}
