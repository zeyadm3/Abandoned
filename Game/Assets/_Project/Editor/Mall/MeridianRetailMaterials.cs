using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    public static class MeridianRetailMaterials
    {
        private const string Folder = MeridianArt.Folder + "/RetailMaterials";

        // Load existing assets first: inspector tuning survives a later authoring pass.
        public static Material Accent(string tag) => Create("Tenant_" + tag, "StorefrontTrim", tag switch
        {
            "electronics" => new Color(.16f, .34f, .35f),
            "clothing" => new Color(.43f, .24f, .29f),
            "furniture" => new Color(.36f, .32f, .22f),
            "toys" => new Color(.44f, .34f, .17f),
            "food" => new Color(.34f, .38f, .24f),
            "cinema" => new Color(.36f, .14f, .16f),
            "jewelry" => new Color(.34f, .29f, .19f),
            "gallery" => new Color(.34f, .36f, .36f),
            _ => new Color(.25f, .31f, .32f)
        }, .2f);

        public static Material Stone => Create("AgedLimestone", "FacadeStone", new Color(.72f, .67f, .56f), .12f);
        public static Material Pale => Create("FadedIvory", "Plaster", new Color(.68f, .65f, .55f), .12f);
        public static Material Dark => Create("DeadDisplay", "FixtureTrim", new Color(.1f, .12f, .12f), .18f);
        public static Material Brass => Create("OxidizedBrass", "StorefrontTrim", new Color(.42f, .32f, .16f), .32f);
        public static Material Rust => Create("CorrodedTrack", "Rust", new Color(.45f, .32f, .25f), .12f);
        public static Material Wood => Create("DisplayTimber", "Wood", new Color(.62f, .52f, .4f), .15f);
        public static Material Tile => Create("KitchenTiles", "Tile", new Color(.68f, .69f, .59f), .25f);

        private static Material Create(string name, string source, Color tint, float smoothness)
        {
            string path = $"{Folder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            PolishAssets.EnsureFolder(Folder);
            Material basis = AssetDatabase.LoadAssetAtPath<Material>($"{MeridianArt.Folder}/Materials/MALL_{source}.mat");
            material = basis != null ? new Material(basis) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.name = name;
            material.SetColor("_BaseColor", tint);
            material.SetFloat("_Smoothness", smoothness);
            material.SetColor("_EmissionColor", Color.black);
            material.DisableKeyword("_EMISSION");
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
