using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Rebuild-time material audit for everything players see: the Mall and HQ scenes, every prefab
    /// (loot, threats, cosmetics, the player) and our material assets. Third-party FBX materials are
    /// remapped to editable URP copies; non-URP shaders become URP Lit; materials without a main texture
    /// get an original procedural map (no flat-grey or blank-white surfaces). Whatever can't be fixed is
    /// logged as an error, one line per material, so the rebuild log should end with zero entries.
    /// </summary>
    public static class MaterialAudit
    {
        public const string RemapFolder = "Assets/_Project/Art/Generated/ThirdPartyMaterials";
        private const string LitShader = "Universal Render Pipeline/Lit";
        private static readonly string[] MaterialFolders = { "Assets/_Project/Art" };
        private static readonly string[] PrefabFolders = { "Assets/_Project/Prefabs" };

        public static int Run()
        {
            RemapThirdPartyMaterials();
            Dictionary<Material, string> used = Collect(out List<string> missing);
            foreach (Material material in used.Keys) Fix(material);
            AssetDatabase.SaveAssets();

            int problems = missing.Count;
            foreach (string where in missing) Debug.LogError($"[MaterialAudit] {where}: renderer slot has no material (renders pink).");
            foreach (KeyValuePair<Material, string> entry in used)
            {
                string problem = Problem(entry.Key);
                if (problem == null) continue;
                problems++;
                Debug.LogError($"[MaterialAudit] {AssetDatabase.GetAssetPath(entry.Key)} ({entry.Key.name}, used by {entry.Value}): {problem}.");
            }
            Debug.Log($"[MaterialAudit] {used.Count} materials checked, {problems} problem(s).");
            return problems;
        }

        /// <summary>FBX materials live inside the model (read-only); give each an editable URP copy and remap to it.</summary>
        public static void RemapThirdPartyMaterials()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { ThirdPartyModelImport.Root.TrimEnd('/') }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not ModelImporter importer) continue;
                Dictionary<AssetImporter.SourceAssetIdentifier, Object> map = importer.GetExternalObjectMap();
                bool changed = false;
                foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is not Material embedded) continue;
                    var id = new AssetImporter.SourceAssetIdentifier(typeof(Material), embedded.name);
                    if (map.ContainsKey(id)) continue;
                    importer.AddRemap(id, EditableCopy(path, embedded));
                    changed = true;
                }
                if (changed) importer.SaveAndReimport();
            }
        }

        private static Material EditableCopy(string modelPath, Material embedded)
        {
            PolishAssets.EnsureFolder(RemapFolder);
            string kit = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(modelPath));
            Color colour = embedded.HasProperty("_BaseColor") ? embedded.GetColor("_BaseColor") : embedded.HasProperty("_Color") ? embedded.color : Color.white;
            // Same name and colour across a kit's models is the same surface: share one material.
            string path = $"{RemapFolder}/{kit}_{Safe(embedded.name)}_{ColorUtility.ToHtmlStringRGB(colour)}.mat";
            var copy = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (copy != null) return copy;
            copy = new Material(Shader.Find(LitShader)) { name = System.IO.Path.GetFileNameWithoutExtension(path) };
            copy.SetColor("_BaseColor", colour);
            Texture texture = embedded.HasProperty("_BaseMap") ? embedded.GetTexture("_BaseMap") : embedded.HasProperty("_MainTex") ? embedded.mainTexture : null;
            copy.SetTexture("_BaseMap", texture);
            copy.SetFloat("_Smoothness", embedded.HasProperty("_Smoothness") ? Mathf.Min(0.35f, embedded.GetFloat("_Smoothness")) : 0.2f);
            AssetDatabase.CreateAsset(copy, path);
            return copy;
        }

        private static Dictionary<Material, string> Collect(out List<string> missing)
        {
            var used = new Dictionary<Material, string>();
            missing = new List<string>();
            foreach (string scenePath in new[] { MallBuilder.ScenePath, HqBuilder.ScenePath })
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                foreach (GameObject root in scene.GetRootGameObjects())
                    Gather(root, System.IO.Path.GetFileNameWithoutExtension(scenePath), used, missing);
            }
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", PrefabFolders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) Gather(prefab, System.IO.Path.GetFileNameWithoutExtension(path), used, missing);
            }
            foreach (string guid in AssetDatabase.FindAssets("t:Material", MaterialFolders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // Fonts and style sheets carry their own GUI materials; only renderers can expose those.
                if (!path.EndsWith(".mat")) continue;
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material != null && !used.ContainsKey(material)) used[material] = "asset";
            }
            return used;
        }

        private static void Gather(GameObject root, string owner, Dictionary<Material, string> used, List<string> missing)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null)
                    {
                        // A LineRenderer/TrailRenderer slot filled at runtime is fine; meshes must have one.
                        if (renderer is MeshRenderer or SkinnedMeshRenderer) missing.Add($"{owner}: {Path(renderer.transform)} slot {i}");
                        continue;
                    }
                    if (!used.ContainsKey(materials[i])) used[materials[i]] = $"{owner}/{renderer.name}";
                }
            }
        }

        private static void Fix(Material material)
        {
            string path = AssetDatabase.GetAssetPath(material);
            // Only our own .mat assets are editable; FBX and font sub-assets are remapped or replaced upstream.
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".mat") || !path.StartsWith("Assets/_Project/")) return;
            bool changed = false;
            if (!IsPipelineShader(material.shader))
            {
                Color colour = material.HasProperty("_Color") ? material.color : Color.white;
                Texture texture = material.HasProperty("_MainTex") ? material.mainTexture : null;
                material.shader = Shader.Find(LitShader);
                material.SetColor("_BaseColor", colour);
                material.SetTexture("_BaseMap", texture);
                changed = true;
            }
            string slot = MainTextureSlot(material);
            if (slot != null && material.GetTexture(slot) == null)
            {
                material.SetTexture(slot, IsParticle(material.shader) ? SoftParticle() : PolishAssets.Texture("SurfaceGrain"));
                changed = true;
            }
            if (!changed) return;
            GeneratedMaterialRepair.Apply(material);
            EditorUtility.SetDirty(material);
        }

        private static string Problem(Material material)
        {
            Shader shader = material.shader;
            if (shader == null || ShaderUtil.ShaderHasError(shader) || shader.name == "Hidden/InternalErrorShader") return "broken or unsupported shader (renders pink)";
            if (!IsPipelineShader(shader)) return $"non-URP shader '{shader.name}'";
            string slot = MainTextureSlot(material);
            if (slot != null && material.GetTexture(slot) == null) return $"no main texture ({slot})";
            return null;
        }

        private static bool IsPipelineShader(Shader shader) =>
            shader != null && (shader.name.StartsWith("Universal Render Pipeline/") || shader.name.StartsWith("Abandoned/") || shader.name.StartsWith("Shader Graphs/"));

        private static bool IsParticle(Shader shader) => shader != null && shader.name.Contains("/Particles/");

        private static string MainTextureSlot(Material material) =>
            material.HasProperty("_BaseMap") ? "_BaseMap" : material.HasProperty("_MainTex") ? "_MainTex" : null;

        // Soft round sprite for particle materials (dust motes, smoke) that were left untextured.
        private static Texture2D SoftParticle()
        {
            const string path = PolishAssets.Folder + "/ParticleSoft.asset";
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture != null) return texture;
            const int n = 64;
            texture = new Texture2D(n, n, TextureFormat.RGBA32, true) { name = "ParticleSoft", wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f) / n, new Vector2(0.5f, 0.5f)) * 2f;
                float a = Mathf.Clamp01(1f - d);
                pixels[y * n + x] = new Color(1f, 1f, 1f, a * a);
            }
            texture.SetPixels(pixels);
            texture.Apply(true, false);
            PolishAssets.EnsureFolder(PolishAssets.Folder);
            AssetDatabase.CreateAsset(texture, path);
            return texture;
        }

        private static string Safe(string name) => string.Join("_", name.Split(System.IO.Path.GetInvalidFileNameChars()));

        private static string Path(Transform t)
        {
            string path = t.name;
            for (Transform p = t.parent; p != null; p = p.parent) path = p.name + "/" + path;
            return path;
        }
    }
}
