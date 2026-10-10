using System;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Authors the original 0.15.5 mall salvage models, static dressing and registered network loot.
    /// Source recipes use metres and a floor-contact pivot; generated loot retains its centre pivot.
    /// This is content creation only: it does not run validation, bake navigation or enter play mode.
    /// </summary>
    public static class SalvageAssetBuilder
    {
        public const string ArtFolder = "Assets/_Project/Art/Generated/Salvage0155";
        public const string ModelFolder = ArtFolder + "/Models";
        public const string MeshFolder = ModelFolder + "/Meshes";
        public const string DressingFolder = "Assets/_Project/Prefabs/SalvageDressing";

        public static string ModelPath(string id) => $"{ModelFolder}/{id}.prefab";
        public static string DressingPath(string id) => $"{DressingFolder}/{id}.prefab";

        [MenuItem("Tools/Abandoned/0.15.5/Author Salvage Asset Library")]
        public static void AuthorAll()
        {
            PolishAssets.EnsureFolder(ModelFolder);
            PolishAssets.EnsureFolder(MeshFolder);
            PolishAssets.EnsureFolder(DressingFolder);
            PolishAssets.EnsureFolder(LootCatalogBuilder.Folder);
            PolishAssets.EnsureFolder("Assets/_Project/Data/Networking");
            PolishAssets.EnsureFolder("Assets/_Project/Data/Interaction");
            SalvageMaterials.Prepare();
            var manifest = new AuthoredManifest();

            foreach (SalvageAssetSpec spec in SalvageAssetCatalog.All)
            {
                GameObject model = AuthorModel(spec, out int[] triangles);
                AuthorDressing(spec, model);
                if (spec.IsLoot) AuthorLoot(spec, model);
                LootDefinition definition = spec.IsLoot ? LootCatalogBuilder.Load(spec.DefinitionId) : null;
                manifest.assets.Add(new AuthoredAsset
                {
                    id = spec.Id, name = spec.Name, category = spec.Category.ToString(),
                    modelPath = ModelPath(spec.Id), dressingPrefabPath = DressingPath(spec.Id),
                    lootPrefabPath = definition != null ? LootPrefabGenerator.PrefabPathFor(definition) : string.Empty,
                    definitionId = definition != null ? definition.Id : string.Empty,
                    hasRecoverableVariant = spec.IsLoot, dressingHasRigidbody = false,
                    gameplayWeight = definition != null ? definition.GameplayWeight : 0,
                    physicsMass = definition != null ? definition.PhysicsMass : 0,
                    valueMin = definition != null ? definition.ValueMin : 0,
                    valueMax = definition != null ? definition.ValueMax : 0,
                    carryClass = definition != null ? definition.CarryClass.ToString() : "Static dressing",
                    requiredCarriers = definition != null ? definition.RequiredCarriers : 0,
                    dimensions = ModelFit.LocalBounds(model, null).size,
                    meshTrianglesPerLod = triangles
                });
            }

            LootPrefabGenerator.WriteCatalog();
            NetworkContentBuilder.RegisterNetworkPrefabs();
            string manifestPath = ArtFolder + "/SalvageManifest.json";
            System.IO.File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));
            AssetDatabase.ImportAsset(manifestPath);
            AssetDatabase.SaveAssets();
        }

        /// <summary>Places an owned/static dressing variant with its bottom pivot at a local position.</summary>
        public static GameObject Place(string id, Transform parent, Vector3 at, float yaw = 0f)
        {
            SalvageAssetSpec spec = SalvageAssetCatalog.Find(id);
            if (spec == null) throw new ArgumentException($"Unknown salvage asset '{id}'.", nameof(id));
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DressingPath(spec.Id));
            if (prefab == null)
                throw new InvalidOperationException($"Salvage dressing '{id}' has not been authored. Run Author Salvage Asset Library first.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = spec.Id;
            instance.transform.SetLocalPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        private static GameObject AuthorModel(SalvageAssetSpec spec, out int[] triangles)
        {
            var root = new GameObject(spec.Id);
            try
            {
                int levels = spec.HasLods ? 3 : 1;
                triangles = new int[levels];
                var lods = new LOD[levels];
                float[] thresholds = { .38f, .15f, .035f };
                for (int lod = 0; lod < levels; lod++)
                {
                    var level = new GameObject($"LOD{lod}");
                    level.transform.SetParent(root.transform, false);
                    bool authored = SalvageTechModels.Build(spec.Id, level.transform, lod)
                        || SalvageFurnitureModels.Build(spec.Id, level.transform, lod)
                        || SalvageHardwareModels.Build(spec.Id, level.transform, lod)
                        || SalvageApparelModels.Build(spec.Id, level.transform, lod)
                        || SalvageOperationsModels.Build(spec.Id, level.transform, lod);
                    if (!authored) throw new InvalidOperationException($"No geometry recipe for salvage asset '{spec.Id}'.");
                    triangles[lod] = CombineAndPersist(level.transform, spec.Id, lod);
                    lods[lod] = new LOD(thresholds[lod], level.GetComponentsInChildren<Renderer>());
                }

                if (spec.HasLods)
                {
                    var group = root.AddComponent<LODGroup>();
                    group.fadeMode = LODFadeMode.None;
                    group.SetLODs(lods);
                    group.RecalculateBounds();
                }

                Bounds geometry = ModelFit.LocalBounds(root, root.transform);
                SalvagePrefabGeometry.AuthorSockets(root.transform, spec.Id, geometry, nested: true);
                // Art prefabs are deliberately collider-free. Collision belongs to dressing/loot variants.
                foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
                    UnityEngine.Object.DestroyImmediate(collider);
                return PrefabUtility.SaveAsPrefabAsset(root, ModelPath(spec.Id));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static int CombineAndPersist(Transform level, string id, int lod)
        {
            var byMaterial = new Dictionary<Material, List<CombineInstance>>();
            MeshFilter[] parts = level.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter part in parts)
            {
                var renderer = part.GetComponent<MeshRenderer>();
                Mesh mesh = part.sharedMesh;
                if (renderer == null || mesh == null) continue;
                Material[] materials = renderer.sharedMaterials;
                for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
                {
                    Material material = materials.Length > 0
                        ? materials[Mathf.Min(submesh, materials.Length - 1)] : null;
                    if (material == null) continue;
                    if (!byMaterial.TryGetValue(material, out List<CombineInstance> instances))
                        byMaterial.Add(material, instances = new List<CombineInstance>());
                    instances.Add(new CombineInstance
                    {
                        mesh = mesh,
                        subMeshIndex = submesh,
                        transform = level.worldToLocalMatrix * part.transform.localToWorldMatrix
                    });
                }
            }

            // Preserve non-rendering helper transforms, but source render pieces are replaced by saved meshes.
            foreach (MeshFilter part in parts)
            {
                MeshRenderer renderer = part.GetComponent<MeshRenderer>();
                if (renderer != null) UnityEngine.Object.DestroyImmediate(renderer);
                UnityEngine.Object.DestroyImmediate(part);
            }

            int triangleCount = 0;
            foreach (KeyValuePair<Material, List<CombineInstance>> entry in byMaterial
                .OrderBy(pair => AssetDatabase.GetAssetPath(pair.Key), StringComparer.Ordinal))
            {
                string finish = CleanName(entry.Key.name);
                var combined = new Mesh { name = $"{id}_LOD{lod}_{finish}" };
                if (entry.Value.Sum(part => (long)part.mesh.vertexCount) > ushort.MaxValue)
                    combined.indexFormat = IndexFormat.UInt32;
                combined.CombineMeshes(entry.Value.ToArray(), true, true, false);
                combined.RecalculateBounds();
                triangleCount += (int)(combined.GetIndexCount(0) / 3);
                Mesh saved = SaveMesh(combined, $"{MeshFolder}/{id}_LOD{lod}_{finish}.asset");
                var render = new GameObject(finish, typeof(MeshFilter), typeof(MeshRenderer));
                render.transform.SetParent(level, false);
                render.GetComponent<MeshFilter>().sharedMesh = saved;
                MeshRenderer output = render.GetComponent<MeshRenderer>();
                output.sharedMaterial = entry.Key;
                output.shadowCastingMode = ShadowCastingMode.On;
                output.receiveShadows = true;
            }
            RemoveEmptySourceChildren(level);
            return triangleCount;
        }

        private static void RemoveEmptySourceChildren(Transform parent)
        {
            // Leave the combined render children and any recipe-supplied sockets intact.
            for (int child = parent.childCount - 1; child >= 0; child--)
            {
                Transform node = parent.GetChild(child);
                RemoveEmptySourceChildren(node);
                if (node.childCount == 0 && node.GetComponents<Component>().Length == 1)
                    UnityEngine.Object.DestroyImmediate(node.gameObject);
            }
        }

        internal static Mesh SaveMesh(Mesh mesh, string path)
        {
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }
            // Updating in place preserves references in generated and manually placed prefab instances.
            EditorUtility.CopySerialized(mesh, existing);
            EditorUtility.SetDirty(existing);
            UnityEngine.Object.DestroyImmediate(mesh);
            return existing;
        }

        private static string CleanName(string value) => new string(value
            .Select(character => char.IsLetterOrDigit(character) || character == '_' ? character : '_').ToArray());

        private static void AuthorDressing(SalvageAssetSpec spec, GameObject model)
        {
            var root = new GameObject(spec.Id);
            try
            {
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
                visual.name = "Model";
                visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                visual.transform.localScale = Vector3.one;
                Bounds bounds = ModelFit.LocalBounds(visual, root.transform);
                SalvagePrefabGeometry.AddColliders(root.transform, spec.Id, bounds, dynamic: false);
                root.AddComponent<SurfaceTag>().EditorSet(spec.Surface);
                SalvagePrefabGeometry.AuthorSockets(root.transform, spec.Id, bounds, nested: false);
                PrefabUtility.SaveAsPrefabAsset(root, DressingPath(spec.Id));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static void AuthorLoot(SalvageAssetSpec spec, GameObject model)
        {
            string path = $"{LootCatalogBuilder.Folder}/Loot_{spec.DefinitionId}.asset";
            LootDefinition definition = AssetDatabase.LoadAssetAtPath<LootDefinition>(path);
            bool isNew = definition == null;
            if (isNew)
            {
                definition = ScriptableObject.CreateInstance<LootDefinition>();
                Bounds initialBounds = ModelFit.LocalBounds(model, null);
                definition.EditorSetup(spec.DefinitionId, spec.Name, spec.ValueMin, spec.ValueMax,
                    spec.Weight, spec.Carry, PositiveSize(initialBounds.size), spec.Fragility, spec.Surface,
                    spec.Carry >= CarryClass.Heavy ? .65f : .25f, 1f, PlaceholderShape.Cube, Color.white,
                    .68f, .94f, spec.Carry == CarryClass.Huge ? 3 : spec.Carry == CarryClass.Heavy ? 2 : 0);
                AssetDatabase.CreateAsset(definition, path);
            }

            Bounds bounds = ModelFit.LocalBounds(model, null);
            definition.EditorSetModel(model, 0f, PositiveSize(bounds.size));
            if (isNew || definition.SpawnTags == null || definition.SpawnTags.Length == 0)
                definition.EditorSetSpawning(new[] { spec.Tag }, false);
            AuthorCarryPoints(definition, spec.Id);
            EditorUtility.SetDirty(definition);
            LootPrefabGenerator.Generate(definition);
        }

        internal static Vector3 PositiveSize(Vector3 size) => new Vector3(
            Mathf.Max(size.x, .001f), Mathf.Max(size.y, .001f), Mathf.Max(size.z, .001f));

        private static void AuthorCarryPoints(LootDefinition definition, string id)
        {
            var serialized = new SerializedObject(definition);
            SerializedProperty points = serialized.FindProperty("<CarryPoints>k__BackingField");
            SerializedProperty required = serialized.FindProperty("<RequiredCarriers>k__BackingField");
            bool shared = definition.CarryClass >= CarryClass.Heavy;
            Vector3[] values = shared ? SalvagePrefabGeometry.GripPoints(id, new Bounds(Vector3.zero, definition.Size))
                : Array.Empty<Vector3>();
            points.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) points.GetArrayElementAtIndex(i).vector3Value = values[i];
            if (!shared) required.intValue = 0;
            else if (definition.RequiredCarriers == 0)
                required.intValue = definition.CarryClass == CarryClass.Huge ? 3 : 2;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [Serializable]
        private sealed class AuthoredManifest
        {
            public string version = "0.15.5";
            public string status = "Authored content; tests, compilation and visual/physics verification deferred to the user.";
            public string triangleCounting = "Combined recipe geometry per LOD; no world-text status plaques.";
            public List<AuthoredAsset> assets = new List<AuthoredAsset>();
        }

        [Serializable]
        private sealed class AuthoredAsset
        {
            public string id, name, category, modelPath, dressingPrefabPath, lootPrefabPath, definitionId, carryClass;
            public bool hasRecoverableVariant, dressingHasRigidbody;
            public float gameplayWeight, physicsMass;
            public int valueMin, valueMax, requiredCarriers;
            public Vector3 dimensions;
            public int[] meshTrianglesPerLod;
        }
    }
}
