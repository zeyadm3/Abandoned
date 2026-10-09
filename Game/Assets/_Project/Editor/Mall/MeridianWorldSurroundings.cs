using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>The mall sits within a closed, neglected retail estate rather than an isolated parking slab.</summary>
    public static class MeridianWorldSurroundings
    {
        public const string GroupName = "MeridianWorldSurroundings";

        public static void Build(Transform mall)
        {
            Transform previous = mall.Find(GroupName);
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            Transform world = Group(GroupName, mall);

            // Reuse the company's original town art, with enough setback for the larger mall and lot.
            Transform district = Group("WiderDistrict", world);
            district.localPosition = new Vector3(-1.7f, 0f, .9f);
            district.localScale = new Vector3(1.65f, 1f, 1.65f);
            HqDistrictLandscape.Build(district);
            HqDistrictBuildings.Build(district);
            MeridianWorldRetailBlocks.Build(world);
            MeridianWorldStreetFurniture.Build(world);
            MeridianWorldBoundary.Build(world);

            // Distant fixtures illuminate their street pools without adding shadow-map demand.
            foreach (Light light in world.GetComponentsInChildren<Light>()) light.shadows = LightShadows.None;
            foreach (Transform item in world.GetComponentsInChildren<Transform>())
                GameObjectUtility.SetStaticEditorFlags(item.gameObject,
                    StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
        }

        internal static Material Material(string name, string source, Color color)
        {
            const string folder = "Assets/_Project/Art/Generated/MeridianWorld";
            PolishAssets.EnsureFolder(folder);
            string path = folder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Material basis = AssetDatabase.LoadAssetAtPath<Material>($"{MeridianArt.Folder}/Materials/MALL_{source}.mat")
                ?? MeridianArt.Material(source);
            material = new Material(basis) { name = name, enableInstancing = true };
            material.SetColor("_BaseColor", color);
            material.SetColor("_EmissionColor", Color.black);
            material.DisableKeyword("_EMISSION");
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        internal static Transform Scenery(string name, Transform parent)
        {
            Transform root = Group(name, parent);
            root.gameObject.AddComponent<WalkThroughObject>()
                .EditorSetup("Decorative estate scenery lies beyond the mall's closed perimeter fence.");
            return root;
        }

        internal static GameObject Panel(string name, Transform parent, Vector3 at, Vector3 size, Material material)
        {
            GameObject panel = Box(name, parent, at, size, material, false);
            panel.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            return panel;
        }
    }
}
