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
    /// Environment surfaces (0.24.12, rebuilt in 0.24.13). Every environment material moves to the Abandoned/Surface
    /// shader (URP Lit plus anti-tiling and weathering: Art/Shaders/AbandonedSurfaceInput.hlsl), and every one whose name
    /// or old map says what it is, and whose maps are flat or drawn by our own builders, gets a CC0 photo-scanned set
    /// from Art/Scanned (Tools/Art/fetch_textures.py). Authored maps (the Kenney packs, anything not ours) are kept.
    /// The scan's size comes from how many UV units the material's meshes really give a metre, measured on every map
    /// (0.24.12 assumed one per metre; most meshes don't, so scans came out tens of metres wide). Materials whose meshes
    /// disagree get triplanar mapping in the object's own metres instead. The colour keeps the material's own hue and
    /// brightness, including that of the drawn map it replaces, worked out in linear space, so each map's tuned lighting
    /// reads the same. Materials change in place (same GUIDs); the builders recreate theirs, so this runs after every map
    /// rebuild. Each material's colour before its first upgrade is kept in <see cref="OriginalsPath"/>, so running this
    /// again never compounds. Threat, player and cosmetic materials are never touched; materials loot or gear share get
    /// scans but no weathering (it is in world space and would slide over a carried crate).
    /// </summary>
    public static partial class MaterialUpgrade
    {
        public const string ScannedRoot = "Assets/_Project/Art/Scanned";
        public const string OriginalsPath = ScannedRoot + "/MaterialUpgradeOriginals.json";
        public const string SurfaceShader = "Abandoned/Surface";
        private const float MaxBase = 1.25f; // how far above white the base colour may go to keep a dark scan's brightness
        private const float MaxSpread = 2.5f; // UV density 80th/20th percentile beyond this: no single tiling fits

        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap"), BaseColor = Shader.PropertyToID("_BaseColor"),
            BumpMap = Shader.PropertyToID("_BumpMap"), MetalMap = Shader.PropertyToID("_MetallicGlossMap"),
            Occlusion = Shader.PropertyToID("_OcclusionMap"), Smoothness = Shader.PropertyToID("_Smoothness"),
            Metallic = Shader.PropertyToID("_Metallic"), Surface = Shader.PropertyToID("_Surface"),
            BumpScale = Shader.PropertyToID("_BumpScale"), OcclusionStrength = Shader.PropertyToID("_OcclusionStrength"),
            AlphaClip = Shader.PropertyToID("_AlphaClip"), WorkflowMode = Shader.PropertyToID("_WorkflowMode");

        // Builders' own drawn maps: replaceable by a scan. Anything else with a normal map is authored PBR.
        private static readonly string[] OurMaps =
        {
            ScannedRoot + "/", "Assets/_Project/Art/Custom/Environment/", "Assets/_Project/Art/Factory/Textures/",
            "Assets/_Project/Art/School/Textures/", "Assets/_Project/Art/Generated/", "Assets/_Project/Art/Polish/",
        };

        private static readonly string[] Folders = { "Assets/_Project/Art", "Assets/_Project/Data" };
        private static readonly string[] MovingPrefabs =
        {
            "Assets/_Project/Prefabs/Loot", "Assets/_Project/Prefabs/Threats", "Assets/_Project/Prefabs/Cosmetics",
            "Assets/_Project/Prefabs/Equipment", "Assets/_Project/Prefabs/Player.prefab",
        };

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
        public static void RunMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Run();
        }

        public static void Run()
        {
            var sets = new Dictionary<string, ScannedSet>();
            foreach (string set in Rules.SelectMany(r => new[] { r.Set, r.DarkSet }).Where(s => s != null).Distinct())
            {
                ScannedSet s = ScannedSet.Load(set);
                if (s != null) sets[set] = s;
                else Debug.LogWarning($"[Surfaces] {set} is not in {ScannedRoot}; run Tools/Art/fetch_textures.py. Its materials keep their maps.");
            }
            Shader surface = Shader.Find(SurfaceShader);
            if (surface == null) throw new InvalidOperationException($"[Surfaces] No {SurfaceShader} shader.");
            Dictionary<string, GraphicsAudit.UvDensity> density = MeasureAcrossMaps();
            HashSet<string> moving = MovingMaterials();
            Dictionary<string, Original> book = LoadOriginals();
            int scanned = 0, weathered = 0, triplanar = 0, skipped = 0;
            var perSet = new Dictionary<string, int>();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", Folders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // Materials inside imported models are read-only; only .mat files can change.
                if (!path.EndsWith(".mat") || path.Contains("/Characters/") || path.Contains("/UI/") || path.Contains("/Threats/") || path.Contains("/Cosmetics/")) { skipped++; continue; }
                // Shared with loot or gear: scanned like the rest, but no world-space weathering (it would slide).
                bool moves = moving.Contains(path);
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null || !Candidate(m)) { skipped++; continue; }

                string core = StripPrefix(m.name.ToLowerInvariant());
                string oldMap = MapPath(m, BaseMap);
                bool ours = Ours(oldMap) && Ours(MapPath(m, BumpMap)) && !path.Contains("/ThirdParty/");
                // A drawn map's name says what the builder meant ("SootBrick" on a material called "Wall").
                string hint = ours && oldMap.Length > 0 && !oldMap.StartsWith(ScannedRoot) ? Path.GetFileNameWithoutExtension(oldMap).ToLowerInvariant() : "";
                Rule? rule = Match(core + " " + hint, out string key);
                string rest = Words(rule != null && core.Contains(key) ? core.Remove(core.IndexOf(key, StringComparison.Ordinal), key.Length) : core);
                if (Skip.Any(s => rest.Contains("_" + s))) { skipped++; continue; }

                m.shader = surface;
                Wear wear = rule?.Wear ?? Wear.Plain;
                bool tri = false, antiTile = false;
                if (rule != null && ours && sets.ContainsKey(rule.Value.Set))
                {
                    Rule r = rule.Value;
                    density.TryGetValue(guid, out GraphicsAudit.UvDensity d);
                    tri = d.P50 < 1e-4f || d.P20 <= 1e-5f || d.P80 / d.P20 > MaxSpread;
                    float uvScale = tri ? 1f / r.Metres : Mathf.Clamp(1f / (r.Metres * d.P50), 1e-3f, 1e3f);
                    string set = Apply(m, guid, sets, r, uvScale, book);
                    antiTile = r.AntiTile && !tri;
                    perSet[set] = perSet.TryGetValue(set, out int n) ? n + 1 : 1;
                    scanned++;
                    if (tri) triplanar++;
                }
                SyncKeywords(m);
                SetWeathering(m, wear, antiTile, tri, !moves);
                EditorUtility.SetDirty(m);
                weathered++;
            }
            SaveOriginals(book);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Surfaces] {weathered} materials on {SurfaceShader}, {scanned} with scans ({triplanar} triplanar), {skipped} left as they were. " +
                      string.Join(", ", perSet.OrderByDescending(p => p.Value).Select(p => $"{p.Key} {p.Value}")));
        }

        // URP Lit (or ours from a previous run), opaque, not cut out, not glowing.
        private static bool Candidate(Material m)
        {
            if (m.shader == null) return false;
            string s = m.shader.name;
            bool lit = s == SurfaceShader || (s.StartsWith("Universal Render Pipeline/") && s.Contains("Lit") && !s.Contains("Unlit") && !s.Contains("Particles") && !s.Contains("Baked") && !s.Contains("Terrain"));
            if (!lit) return false;
            if (m.HasProperty(Surface) && m.GetFloat(Surface) > 0.5f) return false;
            if ((m.HasProperty(AlphaClip) && m.GetFloat(AlphaClip) > 0.5f) || m.IsKeywordEnabled("_ALPHATEST_ON")) return false;
            return !m.IsKeywordEnabled("_EMISSION");
        }

        private static string MapPath(Material m, int id) =>
            m.HasProperty(id) && m.GetTexture(id) != null ? AssetDatabase.GetAssetPath(m.GetTexture(id)) : "";

        // No map, an in-memory map, or one drawn by our builders.
        private static bool Ours(string mapPath) => mapPath.Length == 0 || OurMaps.Any(mapPath.StartsWith) || !mapPath.StartsWith("Assets/");

        private static string Apply(Material m, string guid, Dictionary<string, ScannedSet> sets, Rule rule, float uvScale, Dictionary<string, Original> book)
        {
            Color current = m.HasProperty(BaseColor) ? m.GetColor(BaseColor) : Color.white;
            // The colour this surface had before any upgrade: from the book while the material still carries the tint
            // we wrote; otherwise the builder's colour times its drawn map's average (a green tile map on white is green).
            Color original;
            if (book.TryGetValue(guid, out Original seen) && Close(current, seen.tint)) original = seen.original;
            else
            {
                Texture old = m.HasProperty(BaseMap) ? m.GetTexture(BaseMap) : null;
                Color mean = old != null && !AssetDatabase.GetAssetPath(old).StartsWith(ScannedRoot) ? MeanOf(old) : Color.white;
                original = (current.linear * mean).gamma;
                original.a = current.a;
            }

            Color oLin = original.linear;
            string id = rule.DarkSet != null && Luminance(oLin) < 0.12f && sets.ContainsKey(rule.DarkSet) ? rule.DarkSet : rule.Set;
            ScannedSet scan = sets[id];
            // Keep the material's hue (as much as the rule asks) and its overall brightness, both in linear space,
            // where the shader multiplies the base colour into the scan.
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
            m.SetTextureScale(BaseMap, Vector2.one * uvScale);
            m.SetTextureOffset(BaseMap, Vector2.zero);
            m.SetColor(BaseColor, tint);
            if (m.HasProperty("_Color")) m.SetColor("_Color", tint);
            m.SetTexture(BumpMap, scan.Normal);
            // Asphalt's scan has deep aggregate that reads as lumps at walking height.
            m.SetFloat(BumpScale, id.StartsWith("Asphalt") ? 0.35f : 1f);
            m.SetFloat(WorkflowMode, 1f);
            Texture2D packed = scan.Packed(rule.Bare);
            m.SetTexture(MetalMap, packed);
            m.SetFloat(Smoothness, packed != null ? 0.9f : 0.3f); // multiplies the map's smoothness
            m.SetFloat(Metallic, rule.Bare && scan.HasMetal ? 1f : 0f);
            m.SetTexture(Occlusion, scan.Occlusion);
            m.SetFloat(OcclusionStrength, 1f);
            return id;
        }

        // URP's keywords follow the maps present, as its own inspector would set them.
        private static void SyncKeywords(Material m)
        {
            Toggle(m, "_NORMALMAP", MapPath(m, BumpMap).Length > 0 || (m.HasProperty(BumpMap) && m.GetTexture(BumpMap) != null));
            Toggle(m, "_METALLICSPECGLOSSMAP", m.HasProperty(MetalMap) && m.GetTexture(MetalMap) != null);
            Toggle(m, "_OCCLUSIONMAP", m.HasProperty(Occlusion) && m.GetTexture(Occlusion) != null);
            Toggle(m, "_SPECULAR_SETUP", m.HasProperty(WorkflowMode) && m.GetFloat(WorkflowMode) < 0.5f);
        }

        private static void SetWeathering(Material m, Wear wear, bool antiTile, bool triplanar, bool weather)
        {
            var (variation, grime, dust) = Weathering(wear);
            m.SetFloat("_Variation", variation);
            m.SetFloat("_Grime", grime);
            m.SetFloat("_Dust", dust);
            m.SetFloat("_Weathering", weather ? 1f : 0f);
            m.SetFloat("_AntiTile", antiTile ? 1f : 0f);
            m.SetFloat("_Triplanar", triplanar ? 1f : 0f);
            Toggle(m, "_WEATHERING", weather);
            Toggle(m, "_ANTITILE", antiTile);
            Toggle(m, "_TRIPLANAR", triplanar);
        }

        private static void Toggle(Material m, string keyword, bool on)
        {
            if (on) m.EnableKeyword(keyword);
            else m.DisableKeyword(keyword);
        }

        /// <summary>UV units per metre for every material drawn on any map, pooled over all of them (by GUID).</summary>
        private static Dictionary<string, GraphicsAudit.UvDensity> MeasureAcrossMaps()
        {
            var pooled = new Dictionary<string, List<(double, double)>>();
            foreach (string scenePath in AvOverhaul.MapScenes)
            {
                if (!File.Exists(scenePath)) continue;
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                foreach (var kv in GraphicsAudit.MeasureSamples())
                {
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(kv.Key));
                    if (string.IsNullOrEmpty(guid)) continue;
                    if (!pooled.TryGetValue(guid, out var list)) pooled[guid] = list = new List<(double, double)>();
                    list.AddRange(kv.Value);
                }
            }
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            return pooled.ToDictionary(kv => kv.Key, kv => GraphicsAudit.Density(kv.Value));
        }

        // Materials on things that move (carried loot, monsters, players, gear): world-space weathering would slide.
        private static HashSet<string> MovingMaterials()
        {
            var roots = new List<string>();
            foreach (string folder in MovingPrefabs)
            {
                if (folder.EndsWith(".prefab")) roots.Add(folder);
                else if (AssetDatabase.IsValidFolder(folder))
                    roots.AddRange(AssetDatabase.FindAssets("t:Prefab", new[] { folder }).Select(AssetDatabase.GUIDToAssetPath));
            }
            foreach (string guid in AssetDatabase.FindAssets("t:LootDefinition"))
                roots.Add(AssetDatabase.GUIDToAssetPath(guid));
            return new HashSet<string>(AssetDatabase.GetDependencies(roots.ToArray(), true).Where(p => p.EndsWith(".mat")));
        }

        // Average linear colour of any texture, through a small linear render target (works for drawn .asset maps too).
        private static Color MeanOf(Texture texture)
        {
            var rt = RenderTexture.GetTemporary(64, 64, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var read = new Texture2D(64, 64, TextureFormat.RGBAFloat, false, true);
            RenderTexture previous = RenderTexture.active;
            try
            {
                Graphics.Blit(texture, rt);
                RenderTexture.active = rt;
                read.ReadPixels(new Rect(0, 0, 64, 64), 0, 0);
                read.Apply();
                Color sum = Color.black;
                foreach (Color c in read.GetPixels()) sum += c;
                sum /= 64 * 64;
                return sum.maxColorComponent > 1e-4f ? sum : new Color(0.5f, 0.5f, 0.5f);
            }
            catch (Exception) { return new Color(0.5f, 0.5f, 0.5f); }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                Object.DestroyImmediate(read);
            }
        }

        private static readonly System.Text.RegularExpressions.Regex WordBreak =
            new(@"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|[_\s]+|(?<=\D)(?=\d)|(?<=\d)(?=\D)");

        // "_word_word_" in lower case, so a skip entry matches only at the start of a word.
        private static string Words(string name) =>
            "_" + string.Join("_", WordBreak.Split(name).Where(w => w.Length > 0).Select(w => w.ToLowerInvariant())) + "_";

        private static bool Close(Color a, Color b) =>
            Mathf.Abs(a.r - b.r) < 0.002f && Mathf.Abs(a.g - b.g) < 0.002f && Mathf.Abs(a.b - b.b) < 0.002f;

        public static float Luminance(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;

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
    }
}
