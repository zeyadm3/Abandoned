using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.12 surfaces: every flat or procedurally textured environment material (no normal map of its own) whose
    /// name says what it is gets a CC0 photo-scanned set from Art/Scanned (Tools/Art/fetch_textures.py): colour,
    /// normal, ambient occlusion and a packed metal/smoothness map. Materials are changed in place (same GUIDs), so no
    /// scene or prefab changes. The builders recreate their materials on a rebuild, so this runs after Rebuild All
    /// Maps (and the menu runs it on its own). The colour keeps the material's own hue and brightness: the scan
    /// brings detail, not a new palette, so every map's tuned lighting still reads the same.
    /// </summary>
    public static class MaterialUpgrade
    {
        public const string ScannedRoot = "Assets/_Project/Art/Scanned";
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

            /// <param name="metres">How much surface one texture repeat covers (UVs are metres).</param>
            /// <param name="tint">How much of the material's own hue the scan takes (paint 1, brick 0.3).</param>
            public Rule(string set, float metres, float tint, params string[] keys) { Set = set; Metres = metres; Tint = tint; Keys = keys; }
        }

        // First match wins, so the specific words come before the general ones.
        private static readonly Rule[] Rules =
        {
            new("Terrazzo018", 2f, 0.4f, "terrazzo", "mall_floor_concourse", "mall_floor_walkway", "mall_floor_food", "mall_floor_gallery", "mall_floor_jewelry"),
            new("Carpet016", 1.5f, 0.85f, "mall_floor_cinema", "mall_floor_office"),
            new("Concrete034", 2.5f, 0.45f, "mall_floor_stock"),
            new("Tiles107", 1.2f, 0.6f, "mall_floor_"),
            new("Tiles107", 1.2f, 0.6f, "tile", "lino", "epoxy"),
            new("Carpet016", 1.5f, 0.85f, "carpet", "rug"),
            new("WoodFloor051", 2f, 0.4f, "oakfloor", "gymwood", "stagewood", "woodfloor"),
            new("MetalWalkway014", 1.2f, 0.4f, "grating", "walkway", "galleryplate"),
            new("Metal063", 2f, 0.3f, "rust", "oxide", "oxidised", "corroded", "wetiron", "moldiron", "coolingiron"),
            new("CorrugatedSteel009", 2.5f, 0.7f, "cladding", "corrugated"),
            new("Bricks106A", 1.6f, 0.35f, "brick", "block"),
            new("Asphalt031", 1.6f, 0.3f, "asphalt", "road", "carpark", "streetrecess"),
            new("Concrete034", 2.5f, 0.45f, "sidewalk", "pavement", "pavers", "curb", "footing", "forecourt"),
            new("Concrete042A", 3f, 0.45f, "dampconcrete", "saltconcrete", "basement"),
            new("Concrete031", 3f, 0.45f, "concrete", "cinder", "slag", "floor", "pier", "ironfactory_wall", "roomwall"),
            new("PaintedPlaster017", 2.5f, 1f, "hallpaint", "institutionalpaint", "paintedplaster", "lockerpaint"),
            new("Plaster001", 2.5f, 1f, "plaster", "wall", "dado", "facade", "ceiling", "limestone", "stone"),
            new("Metal049A", 1.5f, 0.75f, "steel", "metal", "iron", "bars", "railing", "fence", "pole", "guard", "console", "locker"),
            new("Gravel023", 2f, 0.4f, "gravel", "ballast", "shingle"),
            new("Grass004", 3f, 0.6f, "grass", "turf"),
            new("Ground037", 3f, 0.5f, "dirt", "earth", "ground", "soil", "clay", "infield", "dormant"),
            new("Rock051", 4f, 0.4f, "rock", "cliff"),
            new("Planks037A", 2f, 0.85f, "siding", "planks", "timber", "dunnage", "board", "deck"),
            new("WoodFloor051", 2f, 0.5f, "oak", "wood"),
            new("Leather037", 1f, 0.8f, "leather"),
        };

        // Never touched: light, glass, liquids, effects, foliage cards, printed things, characters and kit colours.
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
        };

        private static readonly string[] Folders = { "Assets/_Project/Art", "Assets/_Project/Data" };

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
                Apply(m, scan, rule.Value);
                perSet[rule.Value.Set] = perSet.TryGetValue(rule.Value.Set, out int n) ? n + 1 : 1;
                upgraded++;
            }
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

        private static void Apply(Material m, ScannedSet scan, Rule rule)
        {
            Color original = m.HasProperty(BaseColor) ? m.GetColor(BaseColor) : Color.white;
            // Keep the material's hue (as much as the rule asks) and its overall brightness.
            float brightest = Mathf.Max(0.05f, original.maxColorComponent);
            Color hue = Color.Lerp(Color.white, new Color(original.r / brightest, original.g / brightest, original.b / brightest), rule.Tint);
            float wanted = Luminance(original), got = scan.MeanLuminance * Luminance(hue);
            float scale = Mathf.Clamp(wanted / Mathf.Max(0.02f, got), 0.35f, 2.2f);
            Color tint = hue * scale;
            tint.a = original.a;
            float top = tint.maxColorComponent;
            if (top > 1f) tint = new Color(tint.r / top, tint.g / top, tint.b / top, tint.a);

            m.SetTexture(BaseMap, scan.Color);
            m.SetTextureScale(BaseMap, Vector2.one / rule.Metres);
            m.SetTextureOffset(BaseMap, Vector2.zero);
            m.SetColor(BaseColor, tint);
            m.SetTexture(BumpMap, scan.Normal);
            // Asphalt's scan has deep aggregate that reads as lumps at walking height.
            m.SetFloat(BumpScale, rule.Set.StartsWith("Asphalt") ? 0.35f : 1f);
            m.EnableKeyword("_NORMALMAP");
            if (scan.MetalSmooth != null)
            {
                m.SetTexture(MetalMap, scan.MetalSmooth);
                m.SetFloat(Smoothness, 0.9f); // multiplies the map's smoothness
                m.SetFloat(Metallic, 1f);
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

        private static float Luminance(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;

        /// <summary>One downloaded set, with its importers fixed and the metal/smoothness map packed.</summary>
        private sealed class ScannedSet
        {
            public Texture2D Color, Normal, Occlusion, MetalSmooth;
            public float MeanLuminance = 0.5f;

            public static ScannedSet Load(string id)
            {
                string folder = $"{ScannedRoot}/{id}";
                string color = File(folder, "_Color.jpg"), normal = File(folder, "_NormalGL.jpg");
                if (color == null || normal == null) return null;
                string rough = File(folder, "_Roughness.jpg"), ao = File(folder, "_AmbientOcclusion.jpg"), metal = File(folder, "_Metalness.jpg");
                Import(color, srgb: true, normalMap: false);
                Import(normal, srgb: false, normalMap: true);
                if (ao != null) Import(ao, srgb: false, normalMap: false);
                var set = new ScannedSet
                {
                    Color = AssetDatabase.LoadAssetAtPath<Texture2D>(color),
                    Normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normal),
                    Occlusion = ao != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(ao) : null,
                    MeanLuminance = Mean(color),
                };
                if (rough != null) set.MetalSmooth = Pack(folder, id, rough, metal);
                return set;
            }

            private static string File(string folder, string suffix)
            {
                if (!Directory.Exists(folder)) return null;
                string hit = Directory.GetFiles(folder, "*" + suffix).FirstOrDefault();
                return hit?.Replace('\\', '/');
            }

            private static void Import(string path, bool srgb, bool normalMap)
            {
                if (AssetImporter.GetAtPath(path) is not TextureImporter ti) return;
                TextureImporterType type = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
                if (ti.textureType == type && ti.sRGBTexture == srgb && ti.maxTextureSize == 2048 && ti.mipmapEnabled) return;
                ti.textureType = type;
                ti.sRGBTexture = srgb;
                ti.maxTextureSize = 2048;
                ti.mipmapEnabled = true;
                ti.anisoLevel = 4;
                ti.textureCompression = TextureImporterCompression.CompressedHQ;
                ti.SaveAndReimport();
            }

            // Average brightness of the colour map, read straight from the file (no need to make the asset readable).
            private static float Mean(string path)
            {
                var tex = new Texture2D(2, 2);
                if (!tex.LoadImage(System.IO.File.ReadAllBytes(path))) return 0.5f;
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
            private static Texture2D Pack(string folder, string id, string roughPath, string metalPath)
            {
                string outPath = $"{folder}/{id}_MetalSmooth.png";
                if (!System.IO.File.Exists(outPath))
                {
                    var rough = new Texture2D(2, 2);
                    rough.LoadImage(System.IO.File.ReadAllBytes(roughPath));
                    Texture2D metal = null;
                    if (metalPath != null)
                    {
                        metal = new Texture2D(2, 2);
                        metal.LoadImage(System.IO.File.ReadAllBytes(metalPath));
                        if (metal.width != rough.width || metal.height != rough.height) { Object.DestroyImmediate(metal); metal = null; }
                    }
                    Color32[] r = rough.GetPixels32();
                    Color32[] mt = metal != null ? metal.GetPixels32() : null;
                    var packed = new Color32[r.Length];
                    for (int i = 0; i < r.Length; i++)
                        packed[i] = new Color32(mt != null ? mt[i].r : (byte)0, 0, 0, (byte)(255 - r[i].r));
                    var tex = new Texture2D(rough.width, rough.height, TextureFormat.RGBA32, false, true);
                    tex.SetPixels32(packed);
                    System.IO.File.WriteAllBytes(outPath, tex.EncodeToPNG());
                    Object.DestroyImmediate(tex);
                    Object.DestroyImmediate(rough);
                    if (metal != null) Object.DestroyImmediate(metal);
                    AssetDatabase.ImportAsset(outPath);
                }
                Import(outPath, srgb: false, normalMap: false);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(outPath);
            }
        }
    }
}
