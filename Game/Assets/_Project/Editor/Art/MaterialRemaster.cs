using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Rebuilds opaque PBR surfaces at a measured physical scale. Material GUIDs, printed artwork and palette UVs
    /// survive. Shared portable materials never use world-space grime; their local detail follows the object.
    /// </summary>
    public static partial class MaterialRemaster
    {
        public const string OriginalsPath = "Assets/_Project/Art/Scanned/MaterialRemasterOriginals.json";
        private static readonly Dictionary<string, ScannedSet> scans = new();
        private static readonly string[] AssetFolders = { "Assets/_Project/Art", "Assets/_Project/Data" };
        private static readonly string[] PortableFolders = { "Assets/_Project/Prefabs/Loot", "Assets/_Project/Prefabs/Equipment", "Assets/_Project/Prefabs/Player.prefab" };

        [Serializable] private class Record { public string guid; public Color original, tint; }
        [Serializable] private class Records { public List<Record> entries = new(); }

        /// <summary>Run after map construction and MaterialUpgrade, before scene dressing/review. Saves assets only.</summary>
        public static void Run()
        {
            var setup = EditorSceneManager.GetSceneManagerSetup();
            scans.Clear();
            try
            {
                Dictionary<string, GraphicsAudit.UvDensity> density = Measure();
                HashSet<string> moving = PortableMaterials();
                Dictionary<string, Record> originals = Read(OriginalsPath), previous = Read(MaterialUpgrade.OriginalsPath);
                var counts = new Dictionary<SurfaceKind, int>();
                int artwork = 0;
                foreach (string guid in AssetDatabase.FindAssets("t:Material", AssetFolders))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.EndsWith(".mat", StringComparison.Ordinal) || ExcludedPath(path)) continue;
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (!Eligible(material) || Kind(material) == SurfaceKind.None) continue;
                    bool preserve = IsArtwork(material) || Kind(material) == SurfaceKind.Shingle;
                    Color current = material.GetColor("_BaseColor");
                    Color original = current;
                    if (!preserve)
                    {
                        if (originals.TryGetValue(guid, out Record ours) && Close(current, ours.tint)) original = ours.original;
                        else if (previous.TryGetValue(guid, out Record old) && Close(current, old.tint)) original = old.original;
                        else original = (current.linear * Mean(material.GetTexture("_BaseMap"))).gamma;
                    }
                    density.TryGetValue(guid, out GraphicsAudit.UvDensity d);
                    bool tri = d.P50 < 1e-4f || d.P20 <= 1e-5f || d.P80 / Mathf.Max(d.P20, 1e-5f) > 2.5f;
                    if (!Apply(material, d.P50, tri, moving.Contains(path), original, preserve)) continue;
                    originals[guid] = new Record { guid = guid, original = original, tint = material.GetColor("_BaseColor") };
                    SurfaceKind kind = Kind(material);
                    counts[kind] = counts.TryGetValue(kind, out int value) ? value + 1 : 1;
                    if (preserve) artwork++;
                    EditorUtility.SetDirty(material);
                }
                File.WriteAllText(OriginalsPath, JsonUtility.ToJson(new Records { entries = originals.Values.OrderBy(e => e.guid).ToList() }, true));
                AssetDatabase.ImportAsset(OriginalsPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[MaterialRemaster] {counts.Values.Sum()} opaque materials remastered ({artwork} retain artwork/palette/roof patterns). " +
                    string.Join(", ", counts.OrderByDescending(p => p.Value).Select(p => $"{p.Key}: {p.Value}")));
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        /// <summary>Explicit source colour makes this operation repeatable, including tests and authoring previews.</summary>
        public static bool Apply(Material material, float uvDensity, bool triplanar, bool moving, Color sourceColour, bool preserveArtwork = false)
        {
            if (!Eligible(material)) return false;
            SurfaceKind kind = Kind(material);
            if (kind == SurfaceKind.None) return false;
            if (kind == SurfaceKind.Shingle) RestoreShingleArtwork(material);
            Profile p = For(kind);
            ScannedSet scan = Scan(p.Scan);
            if (scan == null) return false;
            Shader shader = Shader.Find(MaterialUpgrade.SurfaceShader);
            if (shader == null) throw new InvalidOperationException("Missing Abandoned/Surface shader.");
            material.shader = shader;
            material.SetFloat("_WorkflowMode", 1f);
            material.SetFloat("_SpecularHighlights", 1f);
            material.SetFloat("_EnvironmentReflections", 1f);
            Keyword(material, "_SPECULAR_SETUP", false);
            Keyword(material, "_SPECULARHIGHLIGHTS_OFF", false);
            Keyword(material, "_ENVIRONMENTREFLECTIONS_OFF", false);
            // Portable items cannot sample world space: the surface must follow a lifted or rotating object.
            triplanar &= !moving;
            float density = uvDensity > 1e-4f ? uvDensity : 1f;
            float tiling = triplanar ? 1f / p.Metres : Mathf.Clamp(1f / (p.Metres * density), .001f, 10000f);
            if (preserveArtwork)
            {
                // Palette coordinates encode a prop's colour selection. Retain them and add an independent detail layer.
                material.SetTexture("_DetailNormalMap", scan.Normal);
                Vector2 baseScale = material.GetTextureScale("_BaseMap");
                material.SetTextureScale("_DetailAlbedoMap", new Vector2(tiling / Mathf.Max(.0001f, Mathf.Abs(baseScale.x)),
                    tiling / Mathf.Max(.0001f, Mathf.Abs(baseScale.y))));
                material.SetTextureOffset("_DetailAlbedoMap", Vector2.zero);
                material.SetFloat("_DetailAlbedoMapScale", 0f);
                material.SetFloat("_DetailNormalMapScale", p.Normal);
                material.SetFloat("_Smoothness", p.Smoothness * .65f);
                material.SetFloat("_Metallic", p.Metal ? .85f : 0f);
                // A legacy packed mask bypasses scalar metalness; its alpha also silently darkens the new finish.
                material.SetTexture("_MetallicGlossMap", null);
                material.SetTexture("_SpecGlossMap", null);
                material.SetFloat("_SmoothnessTextureChannel", 0f);
                Keyword(material, "_METALLICSPECGLOSSMAP", false);
                Keyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", false);
                material.SetTexture("_ParallaxMap", null);
                Keyword(material, "_PARALLAXMAP", false);
                Texture baseNormal = material.GetTexture("_BumpMap");
                if (baseNormal != null && AssetDatabase.GetAssetPath(baseNormal).Contains("/Scanned/", StringComparison.Ordinal))
                    material.SetTexture("_BumpMap", null);
                material.SetFloat("_BumpScale", p.Normal);
                material.SetFloat("_OcclusionStrength", Mathf.Min(.65f, material.GetFloat("_OcclusionStrength")));
                Keyword(material, "_NORMALMAP", material.GetTexture("_BumpMap") != null);
                Keyword(material, "_OCCLUSIONMAP", material.GetTexture("_OcclusionMap") != null);
                Keyword(material, "_DETAIL_SCALED", true);
                Keyword(material, "_DETAIL_MULX2", false);
                // A printed/map-authored base texture cannot use triplanar or randomized UV offsets.
                Keyword(material, "_TRIPLANAR", false);
                Keyword(material, "_ANTITILE", false);
            }
            else
            {
                material.SetTexture("_BaseMap", scan.Color);
                material.SetTexture("_BumpMap", scan.Normal);
                material.SetTexture("_MetallicGlossMap", scan.Packed(p.Metal));
                material.SetTexture("_OcclusionMap", scan.Occlusion);
                material.SetTextureScale("_BaseMap", Vector2.one * tiling);
                material.SetTextureOffset("_BaseMap", Vector2.zero);
                Color colour = Tint(sourceColour, scan.MeanLuminance, kind);
                material.SetColor("_BaseColor", colour);
                if (material.HasProperty("_Color")) material.SetColor("_Color", colour);
                material.SetFloat("_BumpScale", p.Normal);
                material.SetFloat("_Smoothness", p.Smoothness);
                material.SetFloat("_Metallic", p.Metal ? 1f : 0f);
                material.SetFloat("_OcclusionStrength", .65f);
                material.SetFloat("_SmoothnessTextureChannel", 0f);
                Keyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", false);
                Keyword(material, "_NORMALMAP", true);
                Keyword(material, "_METALLICSPECGLOSSMAP", material.GetTexture("_MetallicGlossMap") != null);
                Keyword(material, "_OCCLUSIONMAP", scan.Occlusion != null);
                Keyword(material, "_TRIPLANAR", triplanar);
                Keyword(material, "_ANTITILE", p.AntiTile && !triplanar);
                Keyword(material, "_DETAIL_SCALED", false);
                Keyword(material, "_DETAIL_MULX2", false);
            }
            // Broad grime darkened every fitting equally, erasing the difference between paint, timber and stone.
            material.SetFloat("_Variation", p.Variation);
            material.SetFloat("_Grime", p.Grime);
            material.SetFloat("_Dust", p.Dust);
            material.SetFloat("_Weathering", moving ? 0f : 1f);
            Keyword(material, "_WEATHERING", !moving);
            material.SetFloat("_Triplanar", !preserveArtwork && triplanar ? 1f : 0f);
            material.SetFloat("_AntiTile", !preserveArtwork && p.AntiTile && !triplanar ? 1f : 0f);
            return true;
        }

        private static Color Tint(Color source, float mean, SurfaceKind kind)
        {
            Color linear = source.linear;
            float luminance = MaterialUpgrade.Luminance(linear);
            if (kind == SurfaceKind.Snow || kind == SurfaceKind.TroddenSnow)
                linear = new Color(.78f, .8f, .82f) * (kind == SurfaceKind.TroddenSnow ? .85f : 1f);
            else if (kind == SurfaceKind.Plaster && luminance > .08f)
                linear *= Mathf.Clamp(luminance, .23f, .7f) / Mathf.Max(luminance, .001f);
            Color tint = linear / Mathf.Max(.025f, mean);
            if (tint.maxColorComponent > 1.5f) tint *= 1.5f / tint.maxColorComponent;
            tint = tint.gamma;
            tint.a = source.a;
            return tint;
        }

        private static bool Eligible(Material material) => material != null && material.shader != null && material.HasProperty("_BaseColor") &&
            (material.shader.name == MaterialUpgrade.SurfaceShader || material.shader.name.StartsWith("Universal Render Pipeline/", StringComparison.Ordinal) &&
                material.shader.name.Contains("Lit", StringComparison.Ordinal) && !material.shader.name.Contains("Particles", StringComparison.Ordinal)) &&
            (!material.HasProperty("_Surface") || material.GetFloat("_Surface") < .5f) && !material.IsKeywordEnabled("_EMISSION") &&
            !material.IsKeywordEnabled("_ALPHATEST_ON") && (!material.HasProperty("_AlphaClip") || material.GetFloat("_AlphaClip") < .5f);

        private static bool ExcludedPath(string path) => path.Contains("/Characters/") || path.Contains("/Cosmetics/") || path.Contains("/UI/") || path.Contains("/Threats/") ||
            path.Contains("/CollectionTruck0157/"); // The vehicle has authored paint, wear, rubber and packed response maps.
        private static SurfaceKind Kind(Material material)
        {
            SurfaceKind kind = Classify(material.name);
            // Some generic "wall" materials were authored as brick; the existing scan retains that semantic evidence.
            if (kind == SurfaceKind.Plaster && material.GetTexture("_BaseMap") is Texture texture &&
                AssetDatabase.GetAssetPath(texture).Contains("/Bricks106A/", StringComparison.Ordinal)) return SurfaceKind.Brick;
            return kind;
        }

        private static void RestoreShingleArtwork(Material material)
        {
            bool house = material.name.StartsWith("House_Shingles", StringComparison.Ordinal);
            string prefix = house ? TutorialHouseMaterials.Folder + "/Textures/Shingles" :
                material.name == "Island_Shingle" ? "Assets/_Project/Art/Factory/Textures/Shingle" : null;
            if (prefix == null) return;
            Texture2D artwork = AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + "_BaseColor.png");
            if (artwork == null || material.GetTexture("_BaseMap") == artwork) return;
            material.SetTexture("_BaseMap", artwork);
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + "_Normal.png"));
            material.SetTexture("_OcclusionMap", AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + "_Mask.png"));
            material.SetTextureScale("_BaseMap", Vector2.one);
            material.SetTextureOffset("_BaseMap", Vector2.zero);
            if (!house) return;
            // Street variants share the main house's roof UVs; earlier scan passes left gravel or no map on them.
            Color colour = material.name.EndsWith("Brown", StringComparison.Ordinal) ? new Color(.30f, .22f, .16f) :
                material.name.EndsWith("Weathered", StringComparison.Ordinal) ? new Color(.36f, .36f, .35f) :
                material.name.EndsWith("Green", StringComparison.Ordinal) ? new Color(.20f, .26f, .21f) : new Color(.19f, .19f, .20f);
            material.SetColor("_BaseColor", colour);
            if (material.HasProperty("_Color")) material.SetColor("_Color", colour);
        }
        private static bool IsArtwork(Material material)
        {
            Texture texture = material.GetTexture("_BaseMap");
            string path = texture != null ? AssetDatabase.GetAssetPath(texture).ToLowerInvariant() : "";
            if (path.Contains("/scanned/")) return false;
            return path.Contains("kenney") || path.Contains("colormap") || path.Contains("palette") || path.Contains("blueprint") ||
                path.Contains("letter") || path.Contains("poster") || path.Contains("certificate") || path.Contains("label");
        }

        private static ScannedSet Scan(string id)
        {
            if (scans.TryGetValue(id, out ScannedSet loaded)) return loaded;
            loaded = ScannedSet.Load(id);
            // New paper scans are optional until their download has been imported.
            if (loaded == null && id == "Paper001") loaded = ScannedSet.Load("Plastic010");
            if (loaded == null && id == "Cardboard001") loaded = ScannedSet.Load("Chipboard004");
            scans[id] = loaded;
            return loaded;
        }

        private static HashSet<string> PortableMaterials()
        {
            var roots = new List<string>();
            foreach (string folder in PortableFolders)
                if (folder.EndsWith(".prefab")) roots.Add(folder);
                else if (AssetDatabase.IsValidFolder(folder)) roots.AddRange(AssetDatabase.FindAssets("t:Prefab", new[] { folder }).Select(AssetDatabase.GUIDToAssetPath));
            foreach (string guid in AssetDatabase.FindAssets("t:LootDefinition")) roots.Add(AssetDatabase.GUIDToAssetPath(guid));
            return new HashSet<string>(AssetDatabase.GetDependencies(roots.ToArray(), true).Where(p => p.EndsWith(".mat")));
        }

        private static Dictionary<string, GraphicsAudit.UvDensity> Measure()
        {
            var pooled = new Dictionary<string, List<(double ratio, double area)>>();
            foreach (string path in AvOverhaul.MapScenes)
            {
                if (!File.Exists(path)) continue;
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                foreach (var sample in GraphicsAudit.MeasureSamples())
                {
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sample.Key));
                    if (string.IsNullOrEmpty(guid)) continue;
                    if (!pooled.TryGetValue(guid, out var list)) pooled[guid] = list = new List<(double, double)>();
                    list.AddRange(sample.Value);
                }
            }
            return pooled.ToDictionary(pair => pair.Key, pair => GraphicsAudit.Density(pair.Value));
        }

        private static Dictionary<string, Record> Read(string path) => !File.Exists(path) ? new Dictionary<string, Record>() :
            (JsonUtility.FromJson<Records>(File.ReadAllText(path)) ?? new Records()).entries.Where(e => !string.IsNullOrEmpty(e.guid))
                .GroupBy(e => e.guid).ToDictionary(group => group.Key, group => group.Last());

        private static bool Close(Color a, Color b) => Mathf.Abs(a.r - b.r) < .002f && Mathf.Abs(a.g - b.g) < .002f && Mathf.Abs(a.b - b.b) < .002f;
        private static void Keyword(Material material, string name, bool enabled) { if (enabled) material.EnableKeyword(name); else material.DisableKeyword(name); }

        private static Color Mean(Texture texture)
        {
            if (texture == null) return Color.white;
            var target = RenderTexture.GetTemporary(16, 16, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var read = new Texture2D(16, 16, TextureFormat.RGBAFloat, false, true);
            RenderTexture previous = RenderTexture.active;
            try
            {
                Graphics.Blit(texture, target);
                RenderTexture.active = target;
                read.ReadPixels(new Rect(0, 0, 16, 16), 0, 0);
                read.Apply();
                Color result = Color.black;
                foreach (Color pixel in read.GetPixels()) result += pixel;
                return result / 256f;
            }
            finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); Object.DestroyImmediate(read); }
        }
    }
}
