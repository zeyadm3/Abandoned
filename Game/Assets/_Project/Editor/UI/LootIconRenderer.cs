using System.IO;
using System.Linq;
using Abandoned.Loot;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A small picture of every loot item for the HUD's pocket slots (UI overhaul step 2): each loot prefab
    /// rendered alone on a transparent background into Art/Generated/LootIcons/&lt;id&gt;.png, registered in
    /// UiIcons as "loot/&lt;id&gt;". Only missing icons are rendered (Tools/Abandoned/UI/Render Loot Icons redoes all),
    /// so a new loot item gets its icon on the next rebuild with no extra work.
    /// </summary>
    public static class LootIconRenderer
    {
        public const string Folder = "Assets/_Project/Art/Generated/LootIcons";
        private const int Size = 128;

        [MenuItem("Tools/Abandoned/UI/Render Loot Icons (all)")]
        public static void RenderAllMenu() => Render(force: true);

        /// <summary>Renders the missing icons (all with <paramref name="force"/>); returns how many were made.</summary>
        public static int Render(bool force)
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Art/Generated")) AssetDatabase.CreateFolder("Assets/_Project/Art", "Generated");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Art/Generated", "LootIcons");
            var catalog = AssetDatabase.LoadAssetAtPath<LootCatalog>(LootCatalogBuilder.CatalogPath);
            if (catalog == null) return 0;
            string root = Directory.GetParent(Application.dataPath).FullName;
            var todo = catalog.Entries.Where(e => e.definition != null && e.prefab != null &&
                                                 (force || !File.Exists(Path.Combine(root, PathFor(e.definition.Id))))).ToList();
            if (todo.Count == 0) return 0;

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.58f);
            var sun = new GameObject("Key").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.3f;
            sun.transform.rotation = Quaternion.Euler(40f, -35f, 0f);
            var fill = new GameObject("Fill").AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.45f;
            fill.transform.rotation = Quaternion.Euler(15f, 150f, 0f);
            var camera = new GameObject("IconCamera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.fieldOfView = 22f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 100f;
            var rt = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            camera.targetTexture = rt;
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);

            int made = 0;
            foreach (LootCatalog.Entry e in todo)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(e.prefab);
                go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, 30f, 0f));
                Renderer[] renderers = go.GetComponentsInChildren<Renderer>().Where(r => r.enabled && !(r is ParticleSystemRenderer)).ToArray();
                if (renderers.Length == 0) { Object.DestroyImmediate(go); continue; }
                Bounds b = renderers[0].bounds;
                foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
                float radius = b.extents.magnitude;
                float distance = radius / Mathf.Sin(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.02f;
                // From the front, a little left and above: a three-quarter view that reads at 48 px.
                Vector3 from = new Vector3(-0.55f, 0.5f, -1f).normalized;
                camera.transform.position = b.center + from * distance;
                camera.transform.LookAt(b.center);
                camera.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
                tex.Apply();
                RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(root, PathFor(e.definition.Id)), tex.EncodeToPNG());
                Object.DestroyImmediate(go);
                made++;
            }
            camera.targetTexture = null;
            rt.Release();
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();
            Debug.Log($"[UI] Rendered {made} loot icon(s) into {Folder}.");
            return made;
        }

        public static string PathFor(string id) => $"{Folder}/{id}.png";
    }
}
