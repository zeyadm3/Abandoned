using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// The trims' materials for one map: each <see cref="ArchStyle.Finish"/> is a copy of one of the map's own surfaces
    /// (found among the materials the scene draws with, else anywhere in the project) with its tint, smoothness and
    /// metalness, saved as Art/Remake/Maps/Arch/Materials/&lt;Map&gt;_&lt;Key&gt;.mat and updated in place on later runs
    /// (same GUID). A source that can't be found becomes a plain URP Lit colour.
    /// </summary>
    public static class ArchMaterials
    {
        public const string Folder = RemakePaths.Maps + "/Arch/Materials";

        public static System.Func<string, Material> For(Scene scene, ArchStyle style)
        {
            var byName = new Dictionary<string, Material>();
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
                    foreach (Material m in r.sharedMaterials)
                        if (m != null) byName.TryAdd(m.name, m);
            var made = new Dictionary<string, Material>();
            foreach (ArchStyle.Finish finish in style.Finishes)
                made[finish.Key] = Make(style.Map, finish, byName);
            return key => made.TryGetValue(key, out Material m) ? m : null;
        }

        private static Material Make(string map, ArchStyle.Finish finish, Dictionary<string, Material> byName)
        {
            PolishAssets.EnsureFolder(Folder);
            string path = $"{Folder}/{map}_{finish.Key}.mat";
            if (!byName.TryGetValue(finish.Source, out Material source)) source = Find(finish.Source);
            Material target = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool fresh = target == null;
            if (source != null)
            {
                if (fresh) target = new Material(source);
                else
                {
                    target.shader = source.shader;
                    target.CopyPropertiesFromMaterial(source);
                    target.shaderKeywords = source.shaderKeywords;
                }
            }
            else
            {
                Shader lit = Shader.Find("Universal Render Pipeline/Lit");
                if (fresh) target = new Material(lit);
                else target.shader = lit;
                target.SetColor("_BaseColor", Color.white);
            }
            target.name = $"{map}_{finish.Key}";
            if (finish.Paint && target.HasProperty("_BaseColor")) target.SetColor("_BaseColor", finish.Tint);
            else Tint(target, "_BaseColor", finish.Tint);
            if (target.HasProperty("_Smoothness")) target.SetFloat("_Smoothness", finish.Smoothness);
            if (target.HasProperty("_Metallic")) target.SetFloat("_Metallic", finish.Metallic);
            if (fresh) AssetDatabase.CreateAsset(target, path);
            else EditorUtility.SetDirty(target);
            return target;
        }

        private static void Tint(Material m, string property, Color tint)
        {
            if (!m.HasProperty(property)) return;
            Color c = m.GetColor(property);
            m.SetColor(property, new Color(c.r * tint.r, c.g * tint.g, c.b * tint.b, c.a));
        }

        private static Material Find(string name)
        {
            foreach (string guid in AssetDatabase.FindAssets($"{name} t:Material"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == name) return AssetDatabase.LoadAssetAtPath<Material>(path);
            }
            return null;
        }
    }
}
