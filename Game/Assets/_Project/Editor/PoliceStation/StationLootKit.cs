using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Makes a station's loot playable from data: the definitions (in the station's own Data/Loot folder, ids prefixed
    /// so they never clash with another location's), original low-poly models built from each item's part list inside
    /// its Size box (so the art never changes a collider or a floor load), the network loot prefabs, the catalog, the
    /// network prefab list and the inventory icons. The data here is the source: each run writes it into the definitions
    /// again (tune an item in its list, not in the Inspector), and only remakes models and prefabs that are missing or
    /// whose item changed size or carry class, so it is cheap to run again. Adding an item is adding a line of data.
    /// </summary>
    public sealed class StationLootKit
    {
        public enum Shape { Box, Ball, Tube }

        public readonly struct Part
        {
            public readonly Vector3 At, Dims;
            public readonly string Material;
            public readonly Shape Shape;
            /// <summary>A part placed by fractions of the item's Size box (centre and dimensions).</summary>
            public Part(Vector3 at, Vector3 dims, string material, Shape shape = Shape.Box)
            { At = at; Dims = dims; Material = material; Shape = shape; }
        }

        public sealed class Item
        {
            public string Id, Name;
            public int Min, Max;
            public float Kg, Noise = .2f, Rarity = 1f;
            public CarryClass Class;
            public Vector3 Size;
            public Fragility Fragility;
            public SurfaceMaterial Material;
            public PlaceholderShape Placeholder = PlaceholderShape.Cube;
            public Color Color = Color.grey;
            public bool Jackpot, Wheeled;
            public LootQuirk Quirks;
            public string[] Tags;
            public Part[] Parts;
        }

        private readonly string dataFolder, modelFolder, prefix, logTag;
        private readonly IReadOnlyList<Item> items;
        private readonly Dictionary<string, Material> materials = new();

        public StationLootKit(string dataFolder, string artFolder, string prefix, string logTag, IReadOnlyList<Item> items)
        {
            this.dataFolder = dataFolder;
            modelFolder = artFolder + "/Loot";
            this.prefix = prefix;
            this.logTag = logTag;
            this.items = items;
        }

        public string PathFor(string id) => $"{dataFolder}/Loot_{id}.asset";
        public LootDefinition Load(string id) => AssetDatabase.LoadAssetAtPath<LootDefinition>(PathFor(id));

        public void EnsureContent()
        {
            var changed = new HashSet<string>();
            int defined = Apply(changed);
            int modelled = AssignMissing();
            int prefabs = 0;
            foreach (Item item in items)
            {
                LootDefinition definition = Load(item.Id);
                if (definition == null) continue;
                bool missing = AssetDatabase.LoadAssetAtPath<GameObject>(LootPrefabGenerator.PrefabPathFor(definition)) == null;
                if (!missing && modelled == 0 && !changed.Contains(item.Id)) continue;
                LootPrefabGenerator.Generate(definition);
                prefabs++;
            }
            if (defined + modelled + prefabs + changed.Count == 0) return;
            LootPrefabGenerator.WriteCatalog();
            NetworkContentBuilder.RegisterNetworkPrefabs();
            if (LootIconRenderer.Render(force: false) > 0) UiContentBuilder.BuildIcons();
            AssetDatabase.SaveAssets();
            Debug.Log($"[{logTag}] Loot: {defined} new definitions, {changed.Count} changed, {modelled} new models, {prefabs} prefabs (re)built.");
        }

        // The station's items follow this code: every run writes each definition's data again (none of them is tuned by
        // hand), and an item whose size, carry class or jackpot flag changed gets its prefab (and, for a new size, its
        // model) made again. Returns how many definitions were new; fills in which existing ones changed.
        private int Apply(HashSet<string> changed)
        {
            PolishAssets.EnsureFolder(dataFolder);
            int made = 0;
            foreach (Item item in items)
            {
                if (!item.Id.StartsWith(prefix)) Debug.LogError($"[{logTag}] Loot id '{item.Id}' must start with '{prefix}'.");
                string path = PathFor(item.Id);
                var definition = AssetDatabase.LoadAssetAtPath<LootDefinition>(path);
                bool isNew = definition == null;
                if (isNew)
                {
                    definition = ScriptableObject.CreateInstance<LootDefinition>();
                    AssetDatabase.CreateAsset(definition, path);
                    made++;
                }
                CarryClass oldClass = definition.CarryClass;
                Vector3 oldSize = definition.Size;
                bool oldJackpot = definition.Jackpot;
                definition.EditorSetup(item.Id, item.Name, item.Min, item.Max, item.Kg, item.Class, item.Size, item.Fragility,
                    item.Material, item.Noise, item.Rarity, item.Placeholder, item.Color);
                definition.EditorSetQuirks(item.Quirks);
                definition.EditorSetWheeled(item.Wheeled);
                definition.EditorSetSpawning(item.Tags, item.Jackpot);
                if (!isNew)
                {
                    bool resized = (oldSize - item.Size).sqrMagnitude > 1e-6f;
                    if (resized) definition.EditorSetModel(null, 0f, item.Size);
                    if (resized || oldClass != item.Class || oldJackpot != item.Jackpot) changed.Add(item.Id);
                }
                EditorUtility.SetDirty(definition);
            }
            return made;
        }

        private int AssignMissing()
        {
            PolishAssets.EnsureFolder(modelFolder + "/Meshes");
            int made = 0;
            foreach (Item item in items)
            {
                LootDefinition definition = Load(item.Id);
                if (definition == null || definition.Model != null) continue;
                var root = new GameObject(item.Id);
                try
                {
                    Vector3 size = definition.Size;
                    Part[] parts = item.Parts != null && item.Parts.Length > 0 ? item.Parts : new[] { new Part(Vector3.zero, Vector3.one, "paint") };
                    int n = 0;
                    foreach (Part part in parts)
                    {
                        PrimitiveType type = part.Shape == Shape.Ball ? PrimitiveType.Sphere : part.Shape == Shape.Tube ? PrimitiveType.Cylinder : PrimitiveType.Cube;
                        GameObject go = GameObject.CreatePrimitive(type);
                        Object.DestroyImmediate(go.GetComponent<Collider>());
                        go.name = "Part_" + n++;
                        go.transform.SetParent(root.transform, false);
                        go.transform.localPosition = Vector3.Scale(size, part.At);
                        // Unity's cylinder is two units tall: halve its height so a Tube part fills its box.
                        Vector3 dims = Vector3.Scale(size, part.Dims);
                        if (type == PrimitiveType.Cylinder) dims.y *= .5f;
                        go.transform.localScale = dims;
                        go.GetComponent<MeshRenderer>().sharedMaterial = Mat(part.Material, item);
                    }
                    Combine(root, item.Id);
                    GameObject model = PrefabUtility.SaveAsPrefabAsset(root, $"{modelFolder}/{item.Id}.prefab");
                    definition.EditorSetModel(model, 0f, definition.Size);
                    EditorUtility.SetDirty(definition);
                    made++;
                }
                finally { Object.DestroyImmediate(root); }
            }
            return made;
        }

        private Material Mat(string key, Item item)
        {
            string name = key == "paint" ? $"{prefix}loot_{item.Id}" : $"{prefix}loot_{key}";
            if (materials.TryGetValue(name, out Material cached)) return cached;
            (Color colour, float smooth, float metal, Color emit) = key switch
            {
                "brass" => (new Color(.7f, .52f, .22f), .6f, .85f, Color.black),
                "gold" => (new Color(.86f, .66f, .2f), .75f, 1f, Color.black),
                "steel" => (new Color(.55f, .57f, .6f), .5f, .8f, Color.black),
                "chrome" => (new Color(.8f, .82f, .84f), .85f, .95f, Color.black),
                "black" => (new Color(.06f, .06f, .065f), .35f, .1f, Color.black),
                "rubber" => (new Color(.08f, .08f, .08f), .2f, 0f, Color.black),
                "wood" => (new Color(.52f, .35f, .2f), .35f, 0f, Color.black),
                "darkwood" => (new Color(.24f, .14f, .09f), .45f, 0f, Color.black),
                "paper" => (new Color(.86f, .83f, .72f), .1f, 0f, Color.black),
                "card" => (new Color(.6f, .46f, .3f), .1f, 0f, Color.black),
                "glass" => (new Color(.62f, .74f, .76f), .9f, 0f, Color.black),
                "cloth" => (new Color(.3f, .3f, .32f), .1f, 0f, Color.black),
                "khaki" => (new Color(.55f, .5f, .36f), .1f, 0f, Color.black),
                "leather" => (new Color(.3f, .18f, .1f), .4f, 0f, Color.black),
                "red" => (new Color(.68f, .08f, .06f), .55f, .2f, Color.black),
                "blue" => (new Color(.1f, .18f, .38f), .5f, .2f, Color.black),
                "green" => (new Color(.16f, .3f, .18f), .3f, .1f, Color.black),
                "yellow" => (new Color(.9f, .72f, .12f), .45f, .1f, Color.black),
                "white" => (new Color(.88f, .88f, .86f), .45f, 0f, Color.black),
                "grey" => (new Color(.42f, .43f, .44f), .4f, .2f, Color.black),
                "ivory" => (new Color(.88f, .85f, .76f), .55f, 0f, Color.black),
                "money" => (new Color(.52f, .62f, .44f), .1f, 0f, Color.black),
                "lamp" => (new Color(1f, .85f, .5f), .6f, 0f, new Color(1.6f, 1.2f, .6f)),
                "redlamp" => (new Color(.9f, .1f, .08f), .6f, 0f, new Color(1.6f, .1f, .08f)),
                _ => (item.Color, .45f, .1f, Color.black),
            };
            PolishAssets.EnsureFolder(modelFolder + "/Materials");
            string path = $"{modelFolder}/Materials/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
                AssetDatabase.CreateAsset(m, path);
            }
            m.SetColor("_BaseColor", colour);
            m.SetFloat("_Smoothness", smooth);
            m.SetFloat("_Metallic", metal);
            if (emit != Color.black) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", emit); }
            if (key == "glass")
            {
                m.SetFloat("_Surface", 1f);
                m.SetFloat("_Blend", 0f);
                m.SetFloat("_ZWrite", 0f);
                m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.SetOverrideTag("RenderType", "Transparent");
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                m.SetColor("_BaseColor", new Color(colour.r, colour.g, colour.b, .4f));
            }
            GeneratedMaterialRepair.Apply(m);
            EditorUtility.SetDirty(m);
            materials[name] = m;
            return m;
        }

        // One mesh per material, saved beside the prefab.
        private void Combine(GameObject root, string id)
        {
            var groups = new Dictionary<Material, List<CombineInstance>>();
            MeshFilter[] parts = root.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter part in parts)
            {
                Material m = part.GetComponent<Renderer>().sharedMaterial;
                if (!groups.TryGetValue(m, out List<CombineInstance> list)) groups[m] = list = new List<CombineInstance>();
                list.Add(new CombineInstance { mesh = part.sharedMesh, transform = root.transform.worldToLocalMatrix * part.transform.localToWorldMatrix });
            }
            foreach (MeshFilter part in parts) Object.DestroyImmediate(part.gameObject);
            foreach (KeyValuePair<Material, List<CombineInstance>> pair in groups)
            {
                string path = $"{modelFolder}/Meshes/{id}_{pair.Key.name}.asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh == null) { mesh = new Mesh { name = id + "_" + pair.Key.name }; AssetDatabase.CreateAsset(mesh, path); }
                else mesh.Clear();
                mesh.CombineMeshes(pair.Value.ToArray(), true, true);
                mesh.RecalculateBounds();
                EditorUtility.SetDirty(mesh);
                var go = new GameObject(pair.Key.name);
                go.transform.SetParent(root.transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = pair.Key;
            }
        }
    }
}
