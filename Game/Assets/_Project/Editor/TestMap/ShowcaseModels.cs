using System.IO;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The photo-scanned Poly Haven models (CC0) the TestMap showcase stands up (Tools/Art/fetch_models.py downloads
    /// them). Fixes their importers, makes one URP Lit material per model from its colour, normal and packed mask
    /// (R metal, G occlusion, A smoothness), and places a model by where its base should rest: the bounds are measured,
    /// so a model's own pivot and export scale never matter.
    /// </summary>
    public static class ShowcaseModels
    {
        public const string Root = "Assets/_Project/Art/ThirdParty/PolyHaven";
        private const string LitShader = "Universal Render Pipeline/Lit";
        // A lit tube: bright enough to bloom past the 1.25 threshold, a little warm like an old ballast.
        public static readonly Color TubeGlow = new(5.5f, 5.2f, 4.6f);

        public static bool Has(string id) => File.Exists($"{Root}/{id}/{id}.fbx");

        /// <summary>Places model <paramref name="id"/> with the bottom centre of its bounds on <paramref name="floor"/>.</summary>
        /// <param name="fit">If the model is bigger than this (x along its yaw, y up, z depth), it is scaled down to fit.</param>
        /// <param name="solid">One box collider fitted to the model: furniture a player bumps into.</param>
        /// <param name="support">The section it stands on or hangs from: it goes when that section collapses.</param>
        public static GameObject Place(string id, Transform parent, Vector3 floor, float yaw, Vector3 fit,
            bool solid = true, StructuralSection support = null) =>
            Put(id, parent, floor, yaw, fit, solid, support, hanging: false, stretch: false);

        /// <summary>A ceiling model: the top centre of its bounds at <paramref name="ceiling"/>.</summary>
        public static GameObject Hang(string id, Transform parent, Vector3 ceiling, float yaw, Vector3 fit, StructuralSection support = null) =>
            Put(id, parent, ceiling, yaw, fit, solid: false, support, hanging: true, stretch: false);

        /// <summary>Scales the model on each axis to fill <paramref name="size"/> exactly (a desk whose top must be at
        /// 0.75 m because loot rests on it). Keep it to a few per cent either way or the scan looks squashed.</summary>
        public static GameObject Stretch(string id, Transform parent, Vector3 floor, float yaw, Vector3 size, bool solid = true) =>
            Put(id, parent, floor, yaw, size, solid, null, hanging: false, stretch: true);

        private static GameObject Put(string id, Transform parent, Vector3 anchor, float yaw, Vector3 fit, bool solid,
            StructuralSection support, bool hanging, bool stretch)
        {
            GameObject source = Model(id);
            if (source == null)
            {
                Debug.LogWarning($"[Showcase] {id} is missing; run python3 Tools/Art/fetch_models.py.");
                return null;
            }
            var holder = new GameObject(id).transform;
            holder.SetParent(parent, false);
            holder.SetPositionAndRotation(anchor, Quaternion.Euler(0f, yaw, 0f));
            var model = (GameObject)PrefabUtility.InstantiatePrefab(source, holder);
            model.transform.localPosition = Vector3.zero;
            Material material = MaterialFor(id);
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                var materials = new Material[Mathf.Max(1, SubMeshes(renderer))];
                for (int i = 0; i < materials.Length; i++) materials[i] = material;
                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.On;
            }

            Bounds b = LocalBounds(holder);
            // Scans come modelled along either axis: turn the model so its long side lies along the space's long side,
            // or a shelf meant for a wall would be shrunk to fit its depth.
            if (Mathf.Abs(fit.x - fit.z) > .05f && b.size.x >= b.size.z != fit.x >= fit.z)
            {
                model.transform.localRotation = Quaternion.Euler(0f, 90f, 0f) * model.transform.localRotation;
                b = LocalBounds(holder);
            }
            Vector3 scale = Vector3.one;
            if (stretch)
                scale = new Vector3(fit.x / Mathf.Max(b.size.x, .001f), fit.y / Mathf.Max(b.size.y, .001f), fit.z / Mathf.Max(b.size.z, .001f));
            else
            {
                float s = Mathf.Min(1f, fit.x / Mathf.Max(b.size.x, .001f), fit.y / Mathf.Max(b.size.y, .001f), fit.z / Mathf.Max(b.size.z, .001f));
                scale = Vector3.one * s;
            }
            model.transform.localScale = Vector3.Scale(model.transform.localScale, scale);
            b = LocalBounds(holder);
            float y = hanging ? b.max.y : b.min.y;
            model.transform.localPosition -= new Vector3(b.center.x, y, b.center.z);
            b = LocalBounds(holder);

            if (solid)
            {
                var box = holder.gameObject.AddComponent<BoxCollider>();
                box.center = b.center;
                box.size = b.size;
            }
            if (support != null) holder.gameObject.AddComponent<SectionProp>().EditorSetup(support);
            else GreyboxFactory.MarkStatic(holder);
            return holder.gameObject;
        }

        /// <summary>The model's renderers' bounds in <paramref name="holder"/>'s space, measured from the meshes.</summary>
        public static Bounds LocalBounds(Transform holder)
        {
            Matrix4x4 toHolder = holder.worldToLocalMatrix;
            bool any = false;
            var bounds = new Bounds();
            foreach (MeshFilter filter in holder.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null) continue;
                Matrix4x4 m = toHolder * filter.transform.localToWorldMatrix;
                Bounds mesh = filter.sharedMesh.bounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    var sign = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f);
                    Vector3 p = m.MultiplyPoint3x4(mesh.center + Vector3.Scale(mesh.extents, sign));
                    if (!any) { bounds = new Bounds(p, Vector3.zero); any = true; }
                    else bounds.Encapsulate(p);
                }
            }
            return bounds;
        }

        private static int SubMeshes(Renderer renderer) =>
            renderer.TryGetComponent(out MeshFilter filter) && filter.sharedMesh != null ? filter.sharedMesh.subMeshCount :
            renderer is SkinnedMeshRenderer skinned && skinned.sharedMesh != null ? skinned.sharedMesh.subMeshCount : 1;

        private static GameObject Model(string id)
        {
            string path = $"{Root}/{id}/{id}.fbx";
            if (!File.Exists(path)) return null;
            if (AssetImporter.GetAtPath(path) is ModelImporter importer &&
                (importer.materialImportMode != ModelImporterMaterialImportMode.None || importer.importAnimation ||
                 importer.importCameras || importer.importLights || importer.animationType != ModelImporterAnimationType.None))
            {
                // We make the materials; the FBX's own would point at EXR maps that were never downloaded.
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importAnimation = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.animationType = ModelImporterAnimationType.None;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        public static Material MaterialFor(string id)
        {
            string folder = $"{Root}/{id}";
            string path = $"{folder}/PH_{id}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find(LitShader));
                AssetDatabase.CreateAsset(material, path);
            }
            Texture2D colour = Texture(folder, id + "_diff", srgb: true, normal: false);
            Texture2D normal = Texture(folder, id + "_nor_gl", srgb: false, normal: true);
            Texture2D mask = Texture(folder, id + "_mask", srgb: false, normal: false);
            Texture2D emission = Texture(folder, id + "_emission", srgb: true, normal: false);

            material.SetTexture("_BaseMap", colour);
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_BumpScale", 1f);
            Keyword(material, "_NORMALMAP", normal != null);
            // URP Lit reads metal from R and smoothness from A of this map (times _Smoothness), occlusion from G of the other.
            material.SetTexture("_MetallicGlossMap", mask);
            material.SetTexture("_OcclusionMap", mask);
            material.SetFloat("_Smoothness", 1f);
            material.SetFloat("_SmoothnessTextureChannel", 0f);
            material.SetFloat("_OcclusionStrength", 1f);
            Keyword(material, "_METALLICSPECGLOSSMAP", mask != null);
            Keyword(material, "_OCCLUSIONMAP", mask != null);
            Keyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", false);
            material.SetTexture("_EmissionMap", emission);
            material.SetColor("_EmissionColor", emission != null ? TubeGlow : Color.black);
            Keyword(material, "_EMISSION", emission != null);
            material.globalIlluminationFlags = emission != null ? MaterialGlobalIlluminationFlags.RealtimeEmissive : MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Texture2D Texture(string folder, string name, bool srgb, bool normal)
        {
            foreach (string extension in new[] { ".jpg", ".png" })
            {
                string path = $"{folder}/{name}{extension}";
                if (!File.Exists(path)) continue;
                if (AssetImporter.GetAtPath(path) is TextureImporter importer)
                {
                    TextureImporterType type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                    if (importer.textureType != type || importer.sRGBTexture != srgb || importer.anisoLevel != 8 || importer.maxTextureSize != 2048)
                    {
                        importer.textureType = type;
                        importer.sRGBTexture = srgb;
                        importer.maxTextureSize = 2048;
                        importer.mipmapEnabled = true;
                        // Floors and shelves are seen at grazing angles: keep the scan sharp instead of a smear.
                        importer.anisoLevel = 8;
                        importer.textureCompression = TextureImporterCompression.CompressedHQ;
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
