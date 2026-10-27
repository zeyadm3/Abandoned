using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Improves prop silhouettes in place. Only positively identified props and closed box islands are changed;
    /// source meshes, colliders, renderer material slots, transforms, LOD membership and gameplay components survive.
    /// </summary>
    public static class ObjectRemaster
    {
        public const string Folder = "Assets/_Project/Art/Generated/ObjectRemaster";
        public const string ConfigPath = Folder + "/ObjectRemasterConfig.asset";
        private const string RegistryPath = Folder + "/Sources.json";
        private const string Version = "bevel-2";
        private static Book book;
        private static readonly Dictionary<string, Mesh> cache = new();
        private static readonly Dictionary<string, int> counts = new();

        private static readonly string[] Props =
        {
            "chair", "table", "bench", "desk", "cabinet", "locker", "crate", "pallet", "barrel", "bin", "trash",
            "computer", "monitor", "server", "radio", "telephone", "printer", "fridge", "cooker", "kitchen", "toilet",
            "sink", "bathtub", "sofa", "couch", "bed", "mattress", "cushion", "furniture", "loot", "tool", "machine",
            "conveyor", "boiler", "furnace", "pump", "workshop", "battery", "breaker", "fixture", "lamp", "drawer",
            "shelv", "counter", "appliance", "stove", "oven", "washer", "generator", "spool", "suitcase", "luggage",
            "storage", "container", "bureau", "bookcase", "wardrobe", "pedestal", "seat", "filing", "bulkhead"
        };
        private static readonly string[] Architecture =
        {
            "wall", "floor", "ground", "terrain", "ceiling", "roof", "stair", "catwalk", "road", "street", "facade",
            "window", "slab", "foundation", "rubble", "fractur", "mountain", "landscape", "horizon", "snowdrift", "curb",
            "kerb", "sidewalk", "coffer", "side walk", "handrail", "railing", "fence", "column", "pillar"
        };

        public readonly struct Result
        {
            public int ChangedRendererCount { get; }
            public int BevelledPartCount { get; }
            public int GeneratedMeshCount { get; }
            public Result(int renderers, int parts, int assets) { ChangedRendererCount = renderers; BevelledPartCount = parts; GeneratedMeshCount = assets; }
            public static Result operator +(Result a, Result b) => new(a.ChangedRendererCount + b.ChangedRendererCount, a.BevelledPartCount + b.BevelledPartCount, a.GeneratedMeshCount + b.GeneratedMeshCount);
            public override string ToString() => $"{ChangedRendererCount} prop renderers updated, {BevelledPartCount} closed parts bevelled, {GeneratedMeshCount} mesh assets created";
        }

        [Serializable] private sealed class Entry
        {
            public string output, sourcePath, sourceGuid, builtin;
            public long sourceId;
            public int parts;
        }
        [Serializable] private sealed class Book { public List<Entry> entries = new(); }

        public static Result ApplyToScene(Scene scene)
        {
            Ensure();
            Result result = default;
            foreach (GameObject root in scene.GetRootGameObjects()) result += Apply(root.transform);
            SaveBook();
            return result;
        }

        public static Result ApplyToRoot(Transform root)
        {
            Ensure();
            Result result = Apply(root);
            SaveBook();
            return result;
        }

        /// <summary>
        /// Explicit maintenance after all authoring changes have been saved. The dependency graph comes from
        /// persisted assets, so callers must save scene and prefab changes before invoking this method.
        /// </summary>
        public static int PruneUnusedCache()
        {
            Ensure();
            string[] paths = AssetDatabase.GetAllAssetPaths();
            bool OwnedOutput(string path) => path.StartsWith(Folder + "/Prop_", StringComparison.Ordinal) &&
                path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) && path.LastIndexOf('/') == Folder.Length;
            bool DependencyRoot(string path)
            {
                if (OwnedOutput(path)) return false;
                string extension = Path.GetExtension(path);
                return extension.Equals(".unity", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".prefab", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".asset", StringComparison.OrdinalIgnoreCase);
            }
            string[] roots = paths.Where(DependencyRoot).ToArray();
            // Complete the dependency query before deleting anything. Referenced generated meshes survive even
            // when they are reached indirectly through prefab variants, ScriptableObjects or another scene.
            var referenced = new HashSet<string>(AssetDatabase.GetDependencies(roots, true), StringComparer.Ordinal);
            var removed = new HashSet<string>(StringComparer.Ordinal);
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string path in paths.Where(OwnedOutput).OrderBy(p => p, StringComparer.Ordinal))
                {
                    if (referenced.Contains(path) || AssetDatabase.GetMainAssetTypeAtPath(path) != typeof(Mesh)) continue;
                    if (!AssetDatabase.DeleteAsset(path)) continue;
                    removed.Add(path);
                    cache.Remove(path);
                    counts.Remove(path);
                }
            }
            finally { AssetDatabase.StopAssetEditing(); }
            book.entries.RemoveAll(entry => removed.Contains(entry.output));
            SaveBook();
            return removed.Count;
        }

        private static Result Apply(Transform root)
        {
            int changed = 0, parts = 0, assets = 0;
            ObjectRemasterConfig config = AssetDatabase.LoadAssetAtPath<ObjectRemasterConfig>(ConfigPath);
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                Mesh source = Source(filter.sharedMesh);
                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                if (source == null) continue;
                void RestoreSource()
                {
                    // Source differs only for a registered generated input. Rejected original inputs stay untouched.
                    if (filter.sharedMesh == source) return;
                    Assign(filter, source);
                    changed++;
                }
                if (renderer == null || !Candidate(filter, source, config)) { RestoreSource(); continue; }
                Material[] materials = renderer.sharedMaterials;
                bool[] opaque = materials.Select(m => m != null && m.renderQueue < 2450 &&
                    (!m.HasProperty("_Surface") || m.GetFloat("_Surface") < .5f)).ToArray();
                if (!opaque.Any(x => x)) { RestoreSource(); continue; }
                string sourcePath = AssetDatabase.GetAssetPath(source);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out string guid, out long id);
                string identity = string.IsNullOrEmpty(sourcePath) || sourcePath.StartsWith("Library/") || guid == "0000000000000000e000000000000000"
                    ? "builtin:" + source.name : guid + ":" + id;
                string dependency = sourcePath.StartsWith("Assets/") ? AssetDatabase.GetAssetDependencyHash(sourcePath).ToString() : source.name;
                Matrix4x4 world = filter.transform.localToWorldMatrix;
                string key = identity + ":" + dependency + ":" + Metric(world) + ":" + JsonUtility.ToJson(config) + ":" +
                    string.Join("|", materials.Select((m, i) => (opaque[i] ? "1" : "0") + (m != null ? m.name : ""))) + ":" + Version;
                string path = Folder + "/Prop_" + Hash128.Compute(key) + ".asset";
                bool generated = false;
                int count;
                if (!cache.TryGetValue(path, out Mesh refined))
                {
                    refined = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if (refined == null)
                    {
                        ObjectRemasterMeshData data = ObjectRemasterMeshData.Read(source);
                        if (data == null) { RestoreSource(); continue; }
                        refined = ObjectRemasterBevel.Build(data, data.Boxes(opaque), world, materials, config, out count);
                        if (refined == null) { cache[path] = null; RestoreSource(); continue; }
                        refined.name = source.name + " • refined edges";
                        AssetDatabase.CreateAsset(refined, path);
                        book.entries.Add(new Entry { output = path, sourcePath = sourcePath, sourceGuid = guid, sourceId = id,
                            builtin = identity.StartsWith("builtin:") ? source.name : "", parts = count });
                        generated = true;
                    }
                    cache[path] = refined;
                    counts[path] = book.entries.LastOrDefault(e => e.output == path)?.parts ?? 0;
                }
                if (refined == null) { RestoreSource(); continue; }
                if (filter.sharedMesh == refined) continue;
                Assign(filter, refined);
                changed++; parts += counts[path]; if (generated) assets++;
            }
            return new Result(changed, parts, assets);
        }

        private static void Assign(MeshFilter filter, Mesh mesh)
        {
            filter.sharedMesh = mesh;
            EditorUtility.SetDirty(filter);
            // Rendering metadata follows the original object. MeshColliders intentionally keep the original mesh.
            if (PrefabUtility.IsPartOfPrefabInstance(filter)) PrefabUtility.RecordPrefabInstancePropertyModifications(filter);
        }

        private static bool Candidate(MeshFilter filter, Mesh source, ObjectRemasterConfig config)
        {
            if (source.vertexCount < 8 || source.vertexCount > config.MaximumSourceVertices || source.blendShapeCount > 0) return false;
            string path = AssetDatabase.GetAssetPath(source).ToLowerInvariant();
            // Unsaved procedural meshes have no durable identity for cache reuse or subsequent authoring passes.
            if (string.IsNullOrEmpty(path)) return false;
            // The 2026 remake's models arrive finished (bevelled, baked, with LODs).
            if (path.Contains("/art/remake/")) return false;
            if (path.Contains("/characters/") || path.Contains("/threats/") || path.Contains("/cosmetics/") || path.Contains("/ui/") ||
                path.Contains("landscape") || path.Contains("horizon") || path.Contains("street/streethouse")) return false;
            string own = (source.name + " " + filter.name).ToLowerInvariant();
            if (own.Contains("lod1") || own.Contains("lod2") || own.Contains("_far") || Architecture.Any(own.Contains)) return false;
            // Structural visuals are deliberately square so adjacent tiles keep their authored seams and coverage.
            if (filter.GetComponentInParent<StructuralSection>() != null) return false;
            LODGroup group = filter.GetComponentInParent<LODGroup>();
            if (group != null)
            {
                LOD[] lods = group.GetLODs();
                if (lods.Length > 0 && !lods[0].renderers.Contains(filter.GetComponent<Renderer>())) return false;
            }
            string context = own;
            Transform t = filter.transform.parent;
            for (int depth = 0; t != null && depth < 4; depth++, t = t.parent) context += " " + t.name.ToLowerInvariant();
            return path.Contains("/salvage0155/") || path.Contains("/salvage0156/") || path.Contains("/salvage0157/") ||
                path.Contains("/loot/") || Props.Any(context.Contains);
        }

        private static Mesh Source(Mesh mesh)
        {
            if (mesh == null) return null;
            string path = AssetDatabase.GetAssetPath(mesh);
            if (!path.StartsWith(Folder + "/Prop_", StringComparison.Ordinal)) return mesh;
            Entry entry = book.entries.LastOrDefault(e => e.output == path);
            if (entry == null) return null; // Never bevel an already-refined mesh a second time.
            if (!string.IsNullOrEmpty(entry.builtin))
            {
                if (entry.builtin != "Cube") return null;
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Mesh original = cube.GetComponent<MeshFilter>().sharedMesh;
                Object.DestroyImmediate(cube);
                return original;
            }
            string originalPath = AssetDatabase.GUIDToAssetPath(entry.sourceGuid);
            if (string.IsNullOrEmpty(originalPath)) originalPath = entry.sourcePath;
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(originalPath))
            {
                if (asset is not Mesh candidate) continue;
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(candidate, out string _, out long localId);
                if (localId == entry.sourceId) return candidate;
            }
            return null;
        }

        private static string Metric(Matrix4x4 matrix)
        {
            Vector3 x = matrix.GetColumn(0), y = matrix.GetColumn(1), z = matrix.GetColumn(2);
            return string.Join(",", new[] { Vector3.Dot(x, x), Vector3.Dot(y, y), Vector3.Dot(z, z), Vector3.Dot(x, y), Vector3.Dot(x, z), Vector3.Dot(y, z) }
                .Select(v => v.ToString("G5", CultureInfo.InvariantCulture)));
        }

        private static void Ensure()
        {
            FolderAt(Folder);
            if (AssetDatabase.LoadAssetAtPath<ObjectRemasterConfig>(ConfigPath) == null)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ObjectRemasterConfig>(), ConfigPath);
            if (book == null) book = File.Exists(RegistryPath) ? JsonUtility.FromJson<Book>(File.ReadAllText(RegistryPath)) : new Book();
            book ??= new Book();
        }

        private static void SaveBook()
        {
            string json = JsonUtility.ToJson(book, true);
            if (File.Exists(RegistryPath) && File.ReadAllText(RegistryPath) == json) return;
            File.WriteAllText(RegistryPath, json);
            AssetDatabase.ImportAsset(RegistryPath);
        }

        private static void FolderAt(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            FolderAt(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
