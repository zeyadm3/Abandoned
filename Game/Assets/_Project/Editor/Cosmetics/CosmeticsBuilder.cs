using System.Collections.Generic;
using System.Linq;
using Abandoned.Player;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The wardrobe (GDD 18; redone in 0.12.5): coverall colours, hats and accessories as CosmeticDefinition
    /// assets in a catalog. Hats and accessories are original Blender models (Tools/Blender/environment.py,
    /// Environment/Cosmetics) fitted to the crew worker's head and body. Each item is free, bought once with
    /// company money, or awarded by an achievement. The catalog order is the network index; every machine in
    /// a session runs the same version, so it is rebuilt as listed here.
    /// </summary>
    public static class CosmeticsBuilder
    {
        public const string DataFolder = "Assets/_Project/Data/Player/Cosmetics";
        public const string CatalogPath = DataFolder + "/CosmeticCatalog.asset";
        public const string PrefabFolder = "Assets/_Project/Prefabs/Cosmetics";

        private readonly struct Item
        {
            public readonly string Id, Name, About, Reward;
            public readonly int Price;
            public readonly Color Color;
            public readonly bool Body;

            public Item(string id, string name, string about, int price = 0, string reward = "", Color color = default, bool body = false)
            {
                Id = id; Name = name; About = about; Price = price; Reward = reward; Color = color; Body = body;
            }
        }

        private static readonly Item[] Coveralls =
        {
            new("orange", "Ashline Orange", "Company issue. Easy to spot in the dark, which cuts both ways.", color: new Color(0.95f, 0.45f, 0.1f)),
            new("yellow", "Hi-Vis Yellow", "Reflective tape, mostly still attached.", color: new Color(0.95f, 0.8f, 0.15f)),
            new("navy", "Night Navy", "For crews who'd rather not be seen.", color: new Color(0.15f, 0.22f, 0.42f)),
            new("blue", "Crew Blue", "The colour on the recruitment poster.", color: new Color(0.24f, 0.55f, 0.72f)),
            new("grey", "Concrete Grey", "Matches the walls. Matches the dust.", 750, color: new Color(0.5f, 0.5f, 0.52f)),
            new("olive", "Olive Drab", "Army surplus. Smells like it.", 1200, color: new Color(0.33f, 0.36f, 0.2f)),
            new("rust", "Rust Brown", "Hides the stains you'd rather not explain.", 1200, color: new Color(0.42f, 0.24f, 0.14f)),
            new("teal", "Harbour Teal", "From a dock crew that never clocked out.", 1800, color: new Color(0.1f, 0.5f, 0.52f)),
            new("white", "Hazmat White", "Spotless. For now.", 2500, color: new Color(0.9f, 0.9f, 0.88f)),
            new("green", "Biohazard Green", "Six figures hauled. It glows a little. Probably fine.", reward: "six_figures", color: new Color(0.3f, 0.6f, 0.2f)),
            new("red", "Bloodline Red", "For the ones who didn't make it back. Once.", reward: "occupational_hazard", color: new Color(0.7f, 0.1f, 0.08f)),
            new("black", "Night Shift", "For getting out with the lights off.", reward: "lights_out", color: new Color(0.07f, 0.07f, 0.08f)),
            new("purple", "Royal Purple", "Ten quotas met. The boss almost smiled.", reward: "quota_crusher", color: new Color(0.4f, 0.18f, 0.55f)),
            new("gold", "Gold Plated", "A million dollars of other people's things.", reward: "millionaire", color: new Color(0.92f, 0.72f, 0.22f)),
        };

        private static readonly Item[] Hats =
        {
            new("none", "No Hat", "Bare-headed. Brave."),
            new("hardhat", "Ashline Hard Hat", "Does nothing against a floor. Mandatory anyway."),
            new("beanie", "Knit Beanie", "Warm, and the buildings never are."),
            new("cap", "Work Cap", "Ashline logo, sweat stains included.", 1000),
            new("bucket", "Bucket Hat", "Keeps the ceiling drips off your neck.", 1500),
            new("miner", "Miner's Helmet", "The lamp is decorative. Bring a flashlight.", 3000),
            new("welding", "Welding Mask", "You can't see much. Neither can they.", 4000),
            new("fedora", "Foreman's Fedora", "For the one who signs the job sheets.", 6000),
            new("box", "Cardboard Box", "Your first run. It suits you.", reward: "first_job"),
            new("cone", "Traffic Cone", "A memento of the day it went wrong.", reward: "occupational_hazard"),
            new("nvg", "Night-Vision Rig", "Doesn't work. Looks like it does.", reward: "lights_out"),
            new("saucepan", "Saucepan", "You saw the Hunter fall. This felt safer.", reward: "timber"),
            new("crown", "Scrap Crown", "Bolted together from ten quotas' worth of junk.", reward: "quota_crusher"),
            new("hazmat", "Hazmat Hood", "Twenty-five buildings and you're still breathing.", reward: "career_salvager"),
            new("goldhat", "Gold Hard Hat", "A million hauled. Still does nothing against a floor.", reward: "millionaire"),
        };

        private static readonly Item[] Accessories =
        {
            new("none", "Nothing", "Just you."),
            new("goggles", "Safety Goggles", "Scratched, but the dust stays out."),
            new("dustmask", "Dust Mask", "Paper-thin. So is the ceiling."),
            new("bandana", "Bandana", "Keeps the plaster out of your teeth.", 800),
            new("earmuffs", "Ear Defenders", "Hear less of the building. Maybe that's good.", 1200),
            new("toolbelt", "Tool Belt", "Every pocket full of something useless.", 2000, body: true),
            new("rucksack", "Salvage Rucksack", "Holds nothing in-game. Feels like it should.", 2500, body: true),
            new("gasmask", "Gas Mask", "The filter expired years ago.", 3500),
            new("dogtags", "Dog Tags", "Made it out. Somebody should know who you were.", reward: "made_it_out", body: true),
            new("radio", "Shoulder Radio", "For the medic who brought someone back.", reward: "medic", body: true),
            new("moustache", "Handlebar Moustache", "Grown on the way out with the jackpot.", reward: "the_greed_item"),
            new("shades", "Aviator Shades", "Ten floors gave way. You didn't blink.", reward: "gravity_wins"),
        };

        [MenuItem("Tools/Abandoned/Cosmetics/Build Cosmetics")]
        public static void CreateMissing()
        {
            PolishAssets.EnsureFolder(DataFolder);
            PolishAssets.EnsureFolder(PrefabFolder);
            var keep = new HashSet<string>();
            List<CosmeticDefinition> coveralls = Coveralls.Select(c => Definition("Coverall", c, CosmeticKind.Coverall, null, keep)).ToList();
            List<CosmeticDefinition> hats = Hats.Select(h => Definition("Hat", h, CosmeticKind.Hat, h.Id == "none" ? null : Model("Hat", h.Id, keep), keep)).ToList();
            List<CosmeticDefinition> extras = Accessories.Select(a => Definition("Accessory", a, CosmeticKind.Accessory, a.Id == "none" ? null : Model("Acc", a.Id, keep), keep)).ToList();

            var catalog = AssetDatabase.LoadAssetAtPath<CosmeticCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<CosmeticCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            catalog.EditorSet(coveralls, hats, extras, 1);
            EditorUtility.SetDirty(catalog);
            keep.Add(CatalogPath);
            // The old primitive hats and their definitions are replaced, not kept around.
            foreach (string folder in new[] { DataFolder, PrefabFolder })
                foreach (string guid in AssetDatabase.FindAssets("", new[] { folder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!AssetDatabase.IsValidFolder(path) && !keep.Contains(path)) AssetDatabase.DeleteAsset(path);
                }
            AssetDatabase.SaveAssets();
        }

        private static CosmeticDefinition Definition(string prefix, Item item, CosmeticKind kind, GameObject model, HashSet<string> keep)
        {
            string path = $"{DataFolder}/{prefix}_{item.Id}.asset";
            keep.Add(path);
            var d = AssetDatabase.LoadAssetAtPath<CosmeticDefinition>(path);
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<CosmeticDefinition>();
                AssetDatabase.CreateAsset(d, path);
            }
            d.EditorSetup(item.Id, item.Name, kind, item.About, item.Color == default ? Color.white : item.Color, model, item.Price, item.Reward, item.Body);
            EditorUtility.SetDirty(d);
            return d;
        }

        /// <summary>A wearable prefab: the kit model under a neutral holder whose origin is the worker's crown.</summary>
        private static GameObject Model(string prefix, string id, HashSet<string> keep)
        {
            var root = new GameObject($"{prefix}_{id}");
            CustomMallArt.Place($"Cosmetics/{prefix}_{id}", root.transform, Vector3.zero, Quaternion.identity);
            foreach (Collider c in root.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            string path = $"{PrefabFolder}/{prefix}_{id}.prefab";
            keep.Add(path);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }
    }
}
