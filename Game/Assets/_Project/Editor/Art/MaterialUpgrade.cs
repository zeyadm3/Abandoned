using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.12 surfaces: every flat or procedurally textured environment material (no normal map of its own) whose
    /// name says what it is gets a CC0 photo-scanned set from Art/Scanned (Tools/Art/fetch_textures.py): colour,
    /// normal, ambient occlusion and a packed metal/smoothness map. Materials are changed in place (same GUIDs), so no
    /// scene or prefab changes. The builders recreate their materials on a rebuild, so this runs after Rebuild All
    /// Maps (and the menu runs it on its own). The colour keeps the material's own hue and its brightness (worked out
    /// in linear space): the scan brings detail, not a new palette, so every map's tuned lighting still reads the same.
    /// Each material's colour before its first upgrade is kept in <see cref="OriginalsPath"/>, so running this again
    /// never compounds.
    /// </summary>
    public static class MaterialUpgrade
    {
        public const string ScannedRoot = "Assets/_Project/Art/Scanned";
        public const string OriginalsPath = ScannedRoot + "/MaterialUpgradeOriginals.json";
        private const int DetailMapSize = 2048, SupportMapSize = 1024; // colour and normal; AO and metal/smoothness
        private const float MaxBase = 1.25f; // how far above white the base colour may go to keep a dark scan's brightness

        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap"), BaseColor = Shader.PropertyToID("_BaseColor"),
            BumpMap = Shader.PropertyToID("_BumpMap"), MetalMap = Shader.PropertyToID("_MetallicGlossMap"),
            Occlusion = Shader.PropertyToID("_OcclusionMap"), Smoothness = Shader.PropertyToID("_Smoothness"),
            Metallic = Shader.PropertyToID("_Metallic"), Surface = Shader.PropertyToID("_Surface"),
            BumpScale = Shader.PropertyToID("_BumpScale"), OcclusionStrength = Shader.PropertyToID("_OcclusionStrength");

        private readonly struct Rule
        {
            public readonly string[] Keys;
            public readonly string Set;
            public readonly float Metres, Tint;
            public readonly bool Bare;

            /// <param name="metres">How much surface one texture repeat covers (UVs are metres).</param>
            /// <param name="tint">How much of the material's own hue the scan takes (paint 1, brick 0.3).</param>
            /// <param name="bare">Keep the scan's metalness. Painted metal is not metallic: with no reflections to show,
            /// a metallic locker in a dark room renders black.</param>
            public Rule(string set, float metres, float tint, bool bare, params string[] keys)
            { Set = set; Metres = metres; Tint = tint; Bare = bare; Keys = keys; }
        }

        // First match wins, so the specific words come before the general ones.
        private static readonly Rule[] Rules =
        {
            new("Terrazzo018", 2f, 0.4f, false, "terrazzo", "mall_floor_concourse", "mall_floor_walkway", "mall_floor_food", "mall_floor_gallery", "mall_floor_jewelry"),
            new("Carpet016", 1.5f, 0.85f, false, "mall_floor_cinema", "mall_floor_office"),
            new("Concrete034", 2.5f, 0.45f, false, "mall_floor_stock"),
            new("Tiles107", 1.2f, 0.6f, false, "mall_floor_"),
            new("Tiles107", 1.2f, 0.6f, false, "tile", "lino", "epoxy"),
            new("Carpet016", 1.5f, 0.85f, false, "carpet", "rug"),
            new("WoodFloor051", 2f, 0.4f, false, "oakfloor", "gymwood", "stagewood", "woodfloor"),
            new("MetalWalkway014", 1.2f, 0.4f, true, "grating", "walkway", "galleryplate"),
            new("Metal063", 2f, 0.3f, true, "rust", "oxide", "oxidised", "corroded", "wetiron", "moldiron", "coolingiron"),
            new("CorrugatedSteel009", 2.5f, 0.7f, true, "cladding", "corrugated"),
            new("Planks037A", 2f, 0.85f, false, "fenceweathered", "boardedshop"),
            new("Bricks106A", 1.6f, 0.35f, false, "brick", "block"),
            new("Asphalt031", 1.6f, 0.3f, false, "asphalt", "road", "carpark", "streetrecess"),
            new("Concrete034", 2.5f, 0.45f, false, "sidewalk", "pavement", "pavers", "curb", "footing", "forecourt"),
            new("Concrete042A", 3f, 0.45f, false, "dampconcrete", "saltconcrete", "basement"),
            new("Concrete031", 3f, 0.45f, false, "concrete", "cinder", "slag", "floor", "pier", "ironfactory_wall", "roomwall"),
            new("PaintedPlaster017", 2.5f, 1f, false, "hallpaint", "institutionalpaint", "paintedplaster"),
            new("Plaster001", 2.5f, 1f, false, "plaster", "wall", "dado", "facade", "ceiling", "limestone", "stone"),
            new("Metal049A", 1.5f, 0.75f, false, "steel", "metal", "iron", "bars", "railing", "fence", "pole", "guard", "console", "locker"),
            new("Gravel023", 2f, 0.4f, false, "gravel", "ballast", "shingle"),
            new("Grass004", 3f, 0.6f, false, "grass", "turf"),
            new("Ground037", 3f, 0.5f, false, "dirt", "earth", "ground", "soil", "clay", "infield", "dormant"),
            new("Rock051", 4f, 0.4f, false, "rock", "cliff"),
            new("Planks037A", 2f, 0.85f, false, "siding", "planks", "timber", "dunnage", "board", "deck"),
            new("WoodFloor051", 2f, 0.5f, false, "oak", "wood"),
            new("Leather037", 1f, 0.8f, false, "leather"),
        };

        // Never touched: light, glass, liquids, effects, foliage cards, printed things, characters and kit colours, and
        // roof shingles (no scanned roofing; gravel on a roof reads wrong).
        private static readonly string[] Skip =
        {
            "glow", "glass", "glazing", "lamp", "light", "led", "crt", "emiss", "flame", "spark", "lava", "steam", "dust", "shaft",
            "sky", "sea", "water", "foam", "smoke", "veil", "particle", "outline", "ink", "stripe", "lanepaint", "roadpaint",
            "window", "mirror", "chrome", "gold", "brass", "copper", "bronze", "porcelain", "ornament", "tree", "leaves", "leaf",
            "bark", "branch", "needle", "crown", "ivy", "moss", "scrub", "iceplant", "guano", "grime", "stain", "mould", "chalk",
            "label", "sign", "screen", "lens", "backdrop", "greybox", "hat_", "acc_", "env_", "mattress", "pillow", "bedding",
            "curtain", "blanket", "canvas", "cloth", "wool", "denim", "felt", "tape", "paper", "card", "book", "blueprint",
            "clipboard", "cork", "pegboard", "marker", "rubber", "warning", "hazard", "safety", "caution", "beacon", "signal",
            "display", "mug", "drum", "bin_", "forklift", "palletjack", "toolbox", "loot", "threat", "crew", "skin", "cosmetic",
            "trim", "decal", "puddle", "wet", "soot", "vacantwindows", "distantmass", "mass", "silhouette", "icon", "ui_",
            "shingles",
        };

        private static readonly string[] Folders = { "Assets/_Project/Art", "Assets/_Project/Data" };

        [Serializable]
        private class Original
        {
            public string guid;
            public Color original, tint;
        }

        [Serializable]
        private class Originals
        {
            public List<Original> entries = new();
        }

        [MenuItem(ToolMenu.Art + "Upgrade Surfaces (Scanned Textures)", priority = ToolMenu.ArtP + 61)]
        public static void Run()
        {
            var sets = new Dictionary<string, ScannedSet>();
            foreach (string set in Rules.Select(r => r.Set).Distinct())
            {
                ScannedSet s = ScannedSet.Load(set);
                if (s != null) sets[set] = s;
                else Debug.LogWarning($"[Surfaces] {set} is not in {ScannedRoot}; run Tools/Art/fetch_textures.py. Its materials keep their old look.");
            }
            Dictionary<string, Original> book = LoadOriginals();
            int upgraded = 0, skipped = 0;
            var perSet = new Dictionary<string, int>();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", Folders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/ThirdParty/") || path.Contains("/Characters/") || path.Contains("/UI/")) continue;
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null || !Eligible(m)) { skipped++; continue; }
                string name = m.name.ToLowerInvariant();
                if (Skip.Any(name.Contains)) { skipped++; continue; }
                Rule? rule = null;
                foreach (Rule r in Rules)
                    if (r.Keys.Any(name.Contains)) { rule = r; break; }
                if (rule == null || !sets.TryGetValue(rule.Value.Set, out ScannedSet scan)) { skipped++; continue; }
                Apply(m, guid, scan, rule.Value, book);
                perSet[rule.Value.Set] = perSet.TryGetValue(rule.Value.Set, out int n) ? n + 1 : 1;
                upgraded++;
            }
            SaveOriginals(book);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Surfaces] {upgraded} materials upgraded, {skipped} left as they were. " +
                      string.Join(", ", perSet.OrderByDescending(p => p.Value).Select(p => $"{p.Key} {p.Value}")));
        }

        // URP Lit (or Complex Lit), opaque, not glowing, and without a normal map of its own: an authored PBR set is kept.
        private static bool Eligible(Material m)
        {
            if (m.shader == null || !m.shader.name.StartsWith("Universal Render Pipeline/") || !m.shader.name.Contains("Lit")) return false;
            if (m.shader.name.Contains("Unlit") || m.shader.name.Contains("Particles")) return false;
            if (m.HasProperty(Surface) && m.GetFloat(Surface) > 0.5f) return false;
            if (m.IsKeywordEnabled("_EMISSION")) return false;
            if (m.HasProperty(BumpMap) && m.GetTexture(BumpMap) != null)
            {
                // Ours from a previous run is refreshed; anyone else's normal map means authored PBR.
                string bump = AssetDatabase.GetAssetPath(m.GetTexture(BumpMap));
                return bump.StartsWith(ScannedRoot);
            }
            return true;
        }

        private static void Apply(Material m, string guid, ScannedSet scan, Rule rule, Dictionary<string, Original> book)
        {
            Color current = m.HasProperty(BaseColor) ? m.GetColor(BaseColor) : Color.white;
            // The colour this material had before any upgrade: from the book while the material still carries the tint
            // we wrote; a builder that set a new colour since has given it a new original.
            Color original = book.TryGetValue(guid, out Original seen) && Close(current, seen.tint) ? seen.original : current;

            // Keep the material's hue (as much as the rule asks) and its overall brightness, both in linear space,
            // where the shader multiplies the base colour into the scan.
            Color oLin = original.linear;
            float top = Mathf.Max(1e-4f, oLin.maxColorComponent);
            Color hueLin = Color.Lerp(Color.white, new Color(oLin.r / top, oLin.g / top, oLin.b / top), rule.Tint);
            float wanted = Luminance(oLin), got = scan.MeanLuminance * Luminance(hueLin);
            Color tintLin = hueLin * Mathf.Clamp(wanted / Mathf.Max(1e-4f, got), 0.05f, 4f);
            float peak = tintLin.maxColorComponent;
            if (peak > MaxBase) tintLin *= MaxBase / peak;
            Color tint = tintLin.gamma;
            tint.a = original.a;
            book[guid] = new Original { guid = guid, original = original, tint = tint };

            m.SetTexture(BaseMap, scan.Color);
            m.SetTextureScale(BaseMap, Vector2.one / rule.Metres);
            m.SetTextureOffset(BaseMap, Vector2.zero);
            m.SetColor(BaseColor, tint);
            m.SetTexture(BumpMap, scan.Normal);
            // Asphalt's scan has deep aggregate that reads as lumps at walking height.
            m.SetFloat(BumpScale, rule.Set.StartsWith("Asphalt") ? 0.35f : 1f);
            m.EnableKeyword("_NORMALMAP");
            Texture2D packed = scan.Packed(rule.Bare);
            if (packed != null)
            {
                m.SetTexture(MetalMap, packed);
                m.SetFloat(Smoothness, 0.9f); // multiplies the map's smoothness
                m.SetFloat(Metallic, rule.Bare ? 1f : 0f);
                m.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            if (scan.Occlusion != null)
            {
                m.SetTexture(Occlusion, scan.Occlusion);
                m.SetFloat(OcclusionStrength, 1f);
                m.EnableKeyword("_OCCLUSIONMAP");
            }
            EditorUtility.SetDirty(m);
        }

        private static bool Close(Color a, Color b) =>
            Mathf.Abs(a.r - b.r) < 0.002f && Mathf.Abs(a.g - b.g) < 0.002f && Mathf.Abs(a.b - b.b) < 0.002f;

        private static float Luminance(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;

        private static Dictionary<string, Original> LoadOriginals()
        {
            if (!File.Exists(OriginalsPath)) return new Dictionary<string, Original>();
            var data = JsonUtility.FromJson<Originals>(File.ReadAllText(OriginalsPath)) ?? new Originals();
            return data.entries.Where(e => !string.IsNullOrEmpty(e.guid)).GroupBy(e => e.guid).ToDictionary(g => g.Key, g => g.Last());
        }

        private static void SaveOriginals(Dictionary<string, Original> book)
        {
            var data = new Originals { entries = book.Values.OrderBy(e => e.guid).ToList() };
            File.WriteAllText(OriginalsPath, JsonUtility.ToJson(data, true));
            AssetDatabase.ImportAsset(OriginalsPath);
        }

        /// <summary>One downloaded set, with its importers fixed and the metal/smoothness maps packed.</summary>
        private sealed class ScannedSet
        {
            public Texture2D Color, Normal, Occlusion;
            public float MeanLuminance = 0.5f;
            private string folder, id, rough, metal;

            public static ScannedSet Load(string id)
            {
                string folder = $"{ScannedRoot}/{id}";
                string color = Find(folder, "_Color.jpg"), normal = Find(folder, "_NormalGL.jpg");
                if (color == null || normal == null) return null;
                string ao = Find(folder, "_AmbientOcclusion.jpg");
                Import(color, srgb: true, normalMap: false, DetailMapSize);
                Import(normal, srgb: false, normalMap: true, DetailMapSize);
                if (ao != null) Import(ao, srgb: false, normalMap: false, SupportMapSize);
                return new ScannedSet
                {
                    Color = AssetDatabase.LoadAssetAtPath<Texture2D>(color),
                    Normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normal),
                    Occlusion = ao != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(ao) : null,
                    MeanLuminance = Mean(color),
                    folder = folder,
                    id = id,
                    rough = Find(folder, "_Roughness.jpg"),
                    metal = Find(folder, "_Metalness.jpg"),
                };
            }

            /// <summary>The metal/smoothness map: with the scan's metalness, or (painted) with none.</summary>
            public Texture2D Packed(bool bare)
            {
                if (rough == null) return null;
                bool withMetal = bare && metal != null;
                string outPath = $"{folder}/{id}_{(withMetal ? "MetalSmooth" : "Smooth")}.png";
                if (!File.Exists(outPath)) Pack(outPath, rough, withMetal ? metal : null);
                Import(outPath, srgb: false, normalMap: false, SupportMapSize);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(outPath);
            }

            private static string Find(string folder, string suffix)
            {
                if (!Directory.Exists(folder)) return null;
                string hit = Directory.GetFiles(folder, "*" + suffix).FirstOrDefault();
                return hit?.Replace('\\', '/');
            }

            private static void Import(string path, bool srgb, bool normalMap, int size)
            {
                if (AssetImporter.GetAtPath(path) is not TextureImporter ti) return;
                TextureImporterType type = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
                if (ti.textureType == type && ti.sRGBTexture == srgb && ti.maxTextureSize == size && ti.mipmapEnabled) return;
                ti.textureType = type;
                ti.sRGBTexture = srgb;
                ti.maxTextureSize = size;
                ti.mipmapEnabled = true;
                ti.anisoLevel = 4;
                ti.textureCompression = TextureImporterCompression.CompressedHQ;
                ti.SaveAndReimport();
            }

            // Average linear brightness of the colour map, read straight from the file (the asset need not be readable).
            private static float Mean(string path)
            {
                var tex = new Texture2D(2, 2);
                if (!tex.LoadImage(File.ReadAllBytes(path))) return 0.5f;
                Color32[] px = tex.GetPixels32();
                double sum = 0;
                int step = Mathf.Max(1, px.Length / 65536);
                int n = 0;
                for (int i = 0; i < px.Length; i += step, n++)
                    sum += Luminance(new Color(px[i].r / 255f, px[i].g / 255f, px[i].b / 255f).linear);
                Object.DestroyImmediate(tex);
                return Mathf.Max(0.02f, (float)(sum / Mathf.Max(1, n)));
            }

            // URP Lit reads metallic from R and smoothness from A of one map.
            private static void Pack(string outPath, string roughPath, string metalPath)
            {
                var rough = new Texture2D(2, 2);
                rough.LoadImage(File.ReadAllBytes(roughPath));
                Texture2D metal = null;
                if (metalPath != null)
                {
                    metal = new Texture2D(2, 2);
                    metal.LoadImage(File.ReadAllBytes(metalPath));
                    if (metal.width != rough.width || metal.height != rough.height) { Object.DestroyImmediate(metal); metal = null; }
                }
                Color32[] r = rough.GetPixels32();
                Color32[] mt = metal != null ? metal.GetPixels32() : null;
                var packed = new Color32[r.Length];
                for (int i = 0; i < r.Length; i++)
                    packed[i] = new Color32(mt != null ? mt[i].r : (byte)0, 0, 0, (byte)(255 - r[i].r));
                var tex = new Texture2D(rough.width, rough.height, TextureFormat.RGBA32, false, true);
                tex.SetPixels32(packed);
                File.WriteAllBytes(outPath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(rough);
                if (metal != null) Object.DestroyImmediate(metal);
                AssetDatabase.ImportAsset(outPath);
            }
        }
    }
}
