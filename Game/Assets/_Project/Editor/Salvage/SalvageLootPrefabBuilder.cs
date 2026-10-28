using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Finishes only the salvage_ definitions inside the standard network loot generator.</summary>
    public static class SalvageLootPrefabBuilder
    {
        public const string OutlineMaterialPath = SalvageAssetBuilder.ArtFolder + "/Materials/FocusOutline.mat";

        public static void Apply(GameObject root, LootDefinition definition)
        {
            SalvageAssetSpec spec = SalvageAssetCatalog.Find(definition.Id) ?? SalvagePickables0157.Find(definition.Id);
            if (spec == null || !spec.IsLoot || definition.Id != spec.DefinitionId) return;

            // Keep the standard Visual placeholder transform so the existing generator retains its shape data;
            // collision itself is replaced by proxies in loot-centred coordinates.
            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(collider);
            Transform previousProxies = root.transform.Find("_CollisionProxies");
            if (previousProxies != null) Object.DestroyImmediate(previousProxies.gameObject);
            Transform previousOutline = root.transform.Find("OutlineVisual");
            if (previousOutline != null) Object.DestroyImmediate(previousOutline.gameObject);

            Bounds bounds = new Bounds(Vector3.zero, definition.Size);
            PhysicsMaterial friction = definition.CarryClass >= Abandoned.Interaction.CarryClass.Heavy
                ? LootPrefabGenerator.HeavyFriction() : null;
            if (SalvagePickables0157.Find(definition.Id) != null)
                SalvagePickupGeometry0157.AddColliders(root.transform, spec.Id, bounds, friction);
            else
                SalvagePrefabGeometry.AddColliders(root.transform, spec.Id, bounds, dynamic: true, friction: friction);
            SalvagePrefabGeometry.AuthorSockets(root.transform, spec.Id, bounds, nested: false);
            SurfaceTag surface = Abandoned.EditorTools.SerializedWiring.GetOrAdd<SurfaceTag>(root);
            surface.EditorSet(definition.Material);

            Rigidbody body = root.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.mass = Mathf.Clamp(definition.PhysicsMass, LootDefinition.MinPhysicsMass, LootDefinition.MaxPhysicsMass);
                body.useGravity = true;
                body.isKinematic = false;
                body.interpolation = RigidbodyInterpolation.Interpolate;
            }

            var outline = new GameObject("OutlineVisual");
            outline.transform.SetParent(root.transform, false);
            Material material = FocusMaterial();
            Transform model = root.transform.Find("Model");
            if (model != null)
            {
                var selected = new HashSet<Renderer>();
                LODGroup group = model.GetComponentInChildren<LODGroup>(true);
                if (group != null)
                {
                    LOD[] levels = group.GetLODs();
                    if (levels.Length > 0)
                        foreach (Renderer renderer in levels[0].renderers) if (renderer != null) selected.Add(renderer);
                }
                else
                    foreach (MeshRenderer renderer in model.GetComponentsInChildren<MeshRenderer>(true))
                        if (renderer.GetComponent<TextMesh>() == null) selected.Add(renderer);

                foreach (Renderer renderer in selected)
                {
                    MeshFilter source = renderer.GetComponent<MeshFilter>();
                    if (source == null || source.sharedMesh == null) continue;
                    var shell = new GameObject(source.name, typeof(MeshFilter), typeof(MeshRenderer));
                    shell.transform.SetParent(outline.transform, false);
                    Matrix4x4 local = root.transform.worldToLocalMatrix * source.transform.localToWorldMatrix;
                    shell.transform.localPosition = local.GetColumn(3);
                    shell.transform.localRotation = Quaternion.LookRotation(local.GetColumn(2), local.GetColumn(1));
                    shell.transform.localScale = new Vector3(local.GetColumn(0).magnitude,
                        local.GetColumn(1).magnitude, local.GetColumn(2).magnitude);
                    shell.GetComponent<MeshFilter>().sharedMesh = source.sharedMesh;
                    MeshRenderer output = shell.GetComponent<MeshRenderer>();
                    var materials = new Material[source.sharedMesh.subMeshCount];
                    for (int i = 0; i < materials.Length; i++) materials[i] = material;
                    output.sharedMaterials = materials;
                    output.shadowCastingMode = ShadowCastingMode.Off;
                    output.receiveShadows = false;
                    output.lightProbeUsage = LightProbeUsage.Off;
                    output.reflectionProbeUsage = ReflectionProbeUsage.Off;
                }
            }
            outline.SetActive(false);
            SalvageProp prop = Abandoned.EditorTools.SerializedWiring.GetOrAdd<SalvageProp>(root);
            SerializedWiring.SetInt(prop, "category", (int)spec.Category);
            SerializedWiring.Set(prop, "outlineVisual", outline);
            SerializedWiring.SetLayerRecursively(root, GameLayers.LootLayer);
        }

        private static Material FocusMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterialPath);
            Shader shader = Shader.Find("Abandoned/Salvage Outline");
            if (shader == null) throw new System.InvalidOperationException("Salvage Outline shader is required to author salvage interaction visuals.");
            if (material == null)
            {
                PolishAssets.EnsureFolder(SalvageAssetBuilder.ArtFolder + "/Materials");
                material = new Material(shader) { name = "FocusOutline" };
                AssetDatabase.CreateAsset(material, OutlineMaterialPath);
            }
            material.shader = shader;
            material.SetColor("_OutlineColor", new Color(.95f, .67f, .20f, 1));
            material.SetFloat("_Width", .005f);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
