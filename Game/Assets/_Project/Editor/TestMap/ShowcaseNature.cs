using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The multi-material Poly Haven models of the TestMap site (Tools/Art/fetch_nature.py bakes them): trees with LODs
    /// and an impostor, grass, plants, rocks and yard kits (fence, power poles, fire escape). Every FBX material is
    /// remapped to a URP Lit material made from the packed maps beside it (<c>name_diff</c>, <c>name_nor_gl</c>,
    /// <c>name_mask</c>); a colour map saved as .png carries alpha and makes the material alpha-tested and two-sided.
    /// Models are placed by part (a child of the FBX), and trees and plants become prefabs for the terrain.
    /// </summary>
    public static class ShowcaseNature
    {
        public const string Prefabs = "Assets/_Project/Art/TestMap/Nature";
        private const string LitShader = "Universal Render Pipeline/Lit";

        public static bool Has(string id) => File.Exists(FbxPath(id));
        private static string FbxPath(string id) => $"{ShowcaseModels.Root}/{id}/{id}.fbx";

        /// <summary>The model, its importer set up and its materials remapped to ours (once).</summary>
        public static GameObject Source(string id)
        {
            string path = FbxPath(id);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[Showcase] {id} is missing; run python3 Tools/Art/fetch_nature.py.");
                return null;
            }
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            bool dirty = false;
            if (importer.materialImportMode != ModelImporterMaterialImportMode.ImportViaMaterialDescription || importer.importAnimation ||
                importer.importCameras || importer.importLights || importer.animationType != ModelImporterAnimationType.None)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                importer.importAnimation = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.animationType = ModelImporterAnimationType.None;
                dirty = true;
            }
            if (dirty) importer.SaveAndReimport();

            var names = new HashSet<string>(AssetDatabase.LoadAllAssetsAtPath(path).OfType<Material>().Select(m => m.name));
            foreach (var pair in importer.GetExternalObjectMap())
                if (pair.Key.type == typeof(Material)) names.Add(pair.Key.name);
            bool remapped = false;
            foreach (string name in names)
            {
                Material ours = MaterialFor(id, name);
                var key = new AssetImporter.SourceAssetIdentifier(typeof(Material), name);
                if (importer.GetExternalObjectMap().TryGetValue(key, out Object current) && current == ours) continue;
                importer.AddRemap(key, ours);
                remapped = true;
            }
            if (remapped) importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        /// <summary>The names of the model's parts (its direct children).</summary>
        public static string[] Parts(string id)
        {
            GameObject source = Source(id);
            return source == null ? new string[0] : source.transform.Cast<Transform>().Select(t => t.name).ToArray();
        }

        /// <summary>One part of model <paramref name="id"/> (a child of the FBX, all of it if <paramref name="part"/> is null),
        /// standing with its pivot at <paramref name="at"/>, turned <paramref name="yaw"/> and scaled.</summary>
        /// <param name="solid">A box collider fitted to what is drawn.</param>
        public static GameObject Part(string id, string part, Transform parent, Vector3 at, float yaw, float scale = 1f,
            bool solid = false, bool ground = true)
        {
            GameObject source = Source(id);
            if (source == null) return null;
            Transform from = part == null ? source.transform : source.transform.Find(part);
            if (from == null)
            {
                Debug.LogWarning($"[Showcase] {id} has no part {part}: {string.Join(", ", Parts(id))}");
                return null;
            }
            var holder = new GameObject(part ?? id).transform;
            holder.SetParent(parent, false);
            holder.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            GameObject copy = Copy(from, holder);
            copy.transform.localPosition = Vector3.zero;
            copy.transform.localRotation = from.localRotation;
            copy.transform.localScale = Vector3.Scale(from.localScale, Vector3.one * scale);
            if (ground)
            {
                // Its lowest point on the anchor: scans come with their pivots anywhere.
                Bounds b = ShowcaseModels.LocalBounds(holder);
                copy.transform.localPosition -= new Vector3(0f, b.min.y, 0f);
            }
            if (solid)
            {
                Bounds b = ShowcaseModels.LocalBounds(holder);
                var box = holder.gameObject.AddComponent<BoxCollider>();
                box.center = b.center;
                box.size = b.size;
            }
            return holder.gameObject;
        }

        /// <summary>Every part of model <paramref name="id"/> whose name starts with <paramref name="prefix"/> (a kit's
        /// ready-made assembly, e.g. one dressed power pole), kept in their places relative to each other, standing on
        /// <paramref name="at"/>.</summary>
        public static Transform Group(string id, string prefix, Transform parent, Vector3 at, float yaw, float scale = 1f)
        {
            GameObject source = Source(id);
            var holder = new GameObject(prefix).transform;
            holder.SetParent(parent, false);
            if (source == null) return holder;
            var assembly = new GameObject("Assembly").transform;
            assembly.SetParent(holder, false);
            foreach (Transform part in source.transform)
                if (part.name.StartsWith(prefix)) Copy(part, assembly);
            Bounds b = ShowcaseModels.LocalBounds(holder);
            assembly.localPosition = -new Vector3(b.center.x, b.min.y, b.center.z);
            holder.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            holder.localScale = Vector3.one * scale;
            return holder;
        }

        // A plain copy of a part of the model: meshes and materials only, so it can be cut, merged or batched freely.
        private static GameObject Copy(Transform from, Transform parent)
        {
            var go = new GameObject(from.name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = from.localPosition;
            go.transform.localRotation = from.localRotation;
            go.transform.localScale = from.localScale;
            if (from.TryGetComponent(out MeshFilter filter) && from.TryGetComponent(out MeshRenderer renderer))
            {
                go.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
                var r = go.AddComponent<MeshRenderer>();
                r.sharedMaterials = renderer.sharedMaterials;
                r.shadowCastingMode = ShadowCastingMode.On;
            }
            foreach (Transform child in from) Copy(child, go.transform);
            return go;
        }

        /// <summary>A tree variant as a prefab with a LOD group (scan, thinned scan, impostor) and a trunk collider, for
        /// the terrain's tree list. <paramref name="lod"/> are the screen heights where LOD0, LOD1 and the impostor end.</summary>
        public static GameObject TreePrefab(string id, string variant, float trunkRadius, Vector3 lod)
        {
            GameObject source = Source(id);
            Transform v = source == null ? null : source.transform.Find(variant);
            if (v == null) return null;
            var root = new GameObject(variant);
            var levels = new List<LOD>();
            float[] ends = { lod.x, lod.y, lod.z };
            for (int k = 0; k < 3; k++)
            {
                Transform level = v.Find($"{variant}_LOD{k}");
                if (level == null) continue;
                GameObject copy = Copy(level, root.transform);
                copy.transform.localPosition = Vector3.zero;
                Renderer[] renderers = copy.GetComponentsInChildren<Renderer>();
                // The impostor is two flat quads: its shadow would be a cut-out sheet; the thinned LOD casts for it.
                if (k == 2) foreach (Renderer r in renderers) r.shadowCastingMode = ShadowCastingMode.Off;
                levels.Add(new LOD(ends[k], renderers));
            }
            var group = root.AddComponent<LODGroup>();
            group.SetLODs(levels.ToArray());
            group.fadeMode = LODFadeMode.None;
            group.RecalculateBounds();
            if (trunkRadius > 0f)
            {
                var trunk = root.AddComponent<CapsuleCollider>();
                trunk.radius = trunkRadius;
                trunk.height = 4f;
                trunk.center = new Vector3(0f, 2f, 0f);
            }
            return Save(root, variant);
        }

        /// <summary>One tree (its LOD prefab, see <see cref="TreePrefab"/>) standing at <paramref name="at"/>.</summary>
        public static GameObject Tree(string id, string variant, Transform parent, Vector3 at, float yaw, float scale = 1f)
        {
            string path = $"{Prefabs}/{variant}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path) ?? TreePrefab(id, variant, .2f, new Vector3(.9f, .3f, .004f));
            if (prefab == null) return null;
            var tree = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            tree.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            tree.transform.localScale = Vector3.one * scale;
            return tree;
        }

        /// <summary>A plant part as a prefab whose root carries the mesh (terrain detail meshes must), pivot at its base.</summary>
        public static GameObject PartPrefab(string id, string part, string name = null)
        {
            GameObject source = Source(id);
            Transform from = source == null ? null : source.transform.Find(part);
            if (from == null || !from.TryGetComponent(out MeshFilter filter) || !from.TryGetComponent(out MeshRenderer renderer)) return null;
            var root = new GameObject(name ?? part);
            root.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
            var r = root.AddComponent<MeshRenderer>();
            r.sharedMaterials = renderer.sharedMaterials;
            r.shadowCastingMode = ShadowCastingMode.Off; // a field of grass casting into the shadow map costs more than it shows
            return Save(root, name ?? part);
        }

        private static GameObject Save(GameObject root, string name)
        {
            PolishAssets.EnsureFolder(Prefabs);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{Prefabs}/{name}.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        /// <summary>Our material for FBX material <paramref name="name"/> of model <paramref name="id"/>.</summary>
        public static Material MaterialFor(string id, string name)
        {
            string folder = $"{ShowcaseModels.Root}/{id}";
            string path = $"{folder}/PH_{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find(LitShader));
                AssetDatabase.CreateAsset(material, path);
            }
            bool cutout = File.Exists($"{folder}/{name}_diff.png");
            Texture2D colour = Texture($"{folder}/{name}_diff", srgb: true, normal: false, cutout);
            Texture2D normal = Texture($"{folder}/{name}_nor_gl", srgb: false, normal: true, false);
            Texture2D mask = Texture($"{folder}/{name}_mask", srgb: false, normal: false, false);
            Setup(material, colour, normal, mask, cutout);
            return material;
        }

        public static void Setup(Material material, Texture2D colour, Texture2D normal, Texture2D mask, bool cutout)
        {
            material.SetTexture("_BaseMap", colour);
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_BumpScale", 1f);
            Keyword(material, "_NORMALMAP", normal != null);
            material.SetTexture("_MetallicGlossMap", mask);
            material.SetTexture("_OcclusionMap", mask);
            // Cards are seen against the sky from below: their scanned gloss reads as frost, so it is held down.
            material.SetFloat("_Smoothness", mask == null ? .2f : cutout ? .3f : 1f);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_SmoothnessTextureChannel", 0f);
            material.SetFloat("_OcclusionStrength", 1f);
            Keyword(material, "_METALLICSPECGLOSSMAP", mask != null);
            Keyword(material, "_OCCLUSIONMAP", mask != null);
            // Leaves, needles, grass and wire: cut out by the alpha, both faces drawn.
            material.SetFloat("_AlphaClip", cutout ? 1f : 0f);
            material.SetFloat("_Cutoff", .5f);
            material.SetFloat("_Cull", cutout ? (float)CullMode.Off : (float)CullMode.Back);
            Keyword(material, "_ALPHATEST_ON", cutout);
            material.SetOverrideTag("RenderType", cutout ? "TransparentCutout" : "Opaque");
            material.renderQueue = cutout ? (int)RenderQueue.AlphaTest : -1;
            material.enableInstancing = true;
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            EditorUtility.SetDirty(material);
        }

        /// <summary>A texture at <paramref name="pathWithoutExtension"/> (.png or .jpg), its importer set for its use.</summary>
        public static Texture2D Texture(string pathWithoutExtension, bool srgb, bool normal, bool cutout)
        {
            foreach (string extension in new[] { ".png", ".jpg" })
            {
                string path = pathWithoutExtension + extension;
                if (!File.Exists(path)) continue;
                if (AssetImporter.GetAtPath(path) is TextureImporter importer)
                {
                    TextureImporterType type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                    if (importer.textureType != type || importer.sRGBTexture != srgb || importer.anisoLevel != 8 ||
                        importer.alphaSource != TextureImporterAlphaSource.FromInput ||
                        importer.mipMapsPreserveCoverage != cutout || importer.alphaIsTransparency != cutout)
                    {
                        importer.textureType = type;
                        importer.sRGBTexture = srgb;
                        importer.maxTextureSize = 2048;
                        importer.mipmapEnabled = true;
                        importer.anisoLevel = 8;
                        importer.textureCompression = TextureImporterCompression.CompressedHQ;
                        importer.alphaSource = TextureImporterAlphaSource.FromInput; // a mask keeps smoothness in A
                        importer.alphaIsTransparency = cutout;
                        // Without this, a leaf card's mips lose their alpha coverage and distant trees go bald.
                        importer.mipMapsPreserveCoverage = cutout;
                        importer.alphaTestReferenceValue = .5f;
                        importer.SaveAndReimport();
                    }
                }
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return null;
        }

        private static void Keyword(Material material, string keyword, bool on)
        {
            if (on) material.EnableKeyword(keyword);
            else material.DisableKeyword(keyword);
        }
    }
}
