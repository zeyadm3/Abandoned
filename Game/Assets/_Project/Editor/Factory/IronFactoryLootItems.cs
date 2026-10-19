using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The Iron Factory's salvage (the user's four kinds: iron and steel goods, tools and gauges, huge machine
    /// parts, office records) as ordinary loot definitions: data only, no code per item. They spawn only on
    /// the factory's own tags, so no other location's loot changes. Like <see cref="LootCatalogBuilder"/>,
    /// it never overwrites an existing asset: Inspector tuning is kept.
    /// </summary>
    public static class IronFactoryLootItems
    {
        public const string Foundry = "foundry", Workshop = "workshop", Machinery = "machinery", Records = "records";

        public readonly struct Item
        {
            public readonly string Id, Name;
            public readonly int Min, Max;
            public readonly float Kg, Noise, Rarity;
            public readonly CarryClass Class;
            public readonly Vector3 Size;
            public readonly Fragility Fragility;
            public readonly SurfaceMaterial Material;
            public readonly PlaceholderShape Shape;
            public readonly Color Color;
            public readonly string[] Tags;
            public readonly bool Jackpot;
            public readonly LootQuirk Quirks;

            public Item(string id, string name, int min, int max, float kg, CarryClass carry, Vector3 size, Fragility fragility,
                SurfaceMaterial material, float noise, float rarity, PlaceholderShape shape, Color color, bool jackpot, LootQuirk quirks, params string[] tags)
            {
                Id = id; Name = name; Min = min; Max = max; Kg = kg; Class = carry; Size = size; Fragility = fragility;
                Material = material; Noise = noise; Rarity = rarity; Shape = shape; Color = color; Jackpot = jackpot; Quirks = quirks; Tags = tags;
            }

            public Item(string id, string name, int min, int max, float kg, CarryClass carry, Vector3 size, Fragility fragility,
                SurfaceMaterial material, float noise, float rarity, PlaceholderShape shape, Color color, bool jackpot, params string[] tags)
                : this(id, name, min, max, kg, carry, size, fragility, material, noise, rarity, shape, color, jackpot, LootQuirk.None, tags) { }
        }

        private const PlaceholderShape Box = PlaceholderShape.Cube, Drum = PlaceholderShape.Cylinder;

        public static readonly Item[] Items =
        {
            // ---- Iron and steel goods: heavy for their worth ------------------------------------------
            new("iron_ingot", "Iron Ingot", 150, 300, 12f, CarryClass.OneHand, new(.3f, .09f, .12f), Fragility.None, SurfaceMaterial.Metal, .3f, 2f, Box, new(.32f, .3f, .29f), false, LootQuirk.Hot, Foundry),
            new("steel_billet", "Steel Billet", 300, 600, 35f, CarryClass.TwoHand, new(.16f, .16f, .9f), Fragility.None, SurfaceMaterial.Metal, .5f, 1.2f, Box, new(.45f, .47f, .5f), false, LootQuirk.Hot, Foundry),
            new("cast_gear", "Cast Gear", 400, 900, 25f, CarryClass.TwoHand, new(.6f, .12f, .6f), Fragility.Low, SurfaceMaterial.Metal, .5f, 1f, Drum, new(.36f, .3f, .26f), false, Foundry, Machinery),
            new("chain_coil", "Chain Coil", 300, 700, 30f, CarryClass.TwoHand, new(.5f, .28f, .5f), Fragility.None, SurfaceMaterial.Metal, .6f, 1f, Drum, new(.3f, .28f, .27f), false, LootQuirk.Rattles, Foundry),
            new("anvil", "Anvil", 1200, 2500, 110f, CarryClass.Heavy, new(.75f, .45f, .3f), Fragility.None, SurfaceMaterial.Metal, .8f, .5f, Box, new(.2f, .2f, .22f), false, Foundry, Workshop),
            new("founders_bell", "Founder's Bronze Bell", 30000, 45000, 700f, CarryClass.Huge, new(1.2f, 1.3f, 1.2f), Fragility.Low, SurfaceMaterial.Metal, 1f, .1f, Drum, new(.62f, .45f, .2f), true, Foundry),

            // ---- Tools and gauges: small and portable ----------------------------------------------
            new("brass_gauge", "Brass Pressure Gauge", 150, 500, .8f, CarryClass.Pocket, new(.12f, .12f, .05f), Fragility.Medium, SurfaceMaterial.Glass, .05f, 1.5f, Drum, new(.7f, .55f, .25f), false, Workshop, Machinery),
            new("foreman_stopwatch", "Foreman's Stopwatch", 200, 600, .15f, CarryClass.Pocket, new(.06f, .07f, .02f), Fragility.Medium, SurfaceMaterial.Metal, .03f, .8f, Drum, new(.75f, .75f, .72f), false, Workshop, Records),
            new("micrometer_set", "Micrometer Set", 300, 800, 1.2f, CarryClass.OneHand, new(.3f, .06f, .18f), Fragility.Low, SurfaceMaterial.Wood, .1f, 1f, Box, new(.3f, .18f, .1f), false, Workshop),
            new("torque_wrench", "Torque Wrench", 120, 350, 1.5f, CarryClass.OneHand, new(.6f, .05f, .06f), Fragility.None, SurfaceMaterial.Metal, .2f, 1.4f, Box, new(.5f, .52f, .55f), false, Workshop),
            new("welding_mask", "Welding Mask", 80, 250, 1f, CarryClass.OneHand, new(.25f, .3f, .22f), Fragility.Low, SurfaceMaterial.Plastic, .1f, 1.3f, Box, new(.15f, .15f, .15f), false, Workshop),
            new("machinist_vise", "Machinist's Vise", 250, 600, 18f, CarryClass.TwoHand, new(.35f, .25f, .2f), Fragility.None, SurfaceMaterial.Metal, .4f, .9f, Box, new(.2f, .35f, .45f), false, Workshop),
            new("casting_pattern", "Wooden Casting Pattern", 1500, 4000, 20f, CarryClass.TwoHand, new(.8f, .5f, .5f), Fragility.High, SurfaceMaterial.Wood, .3f, .6f, Box, new(.65f, .45f, .25f), false, Workshop),

            // ---- Huge machine parts: crew jobs ----------------------------------------------------
            new("steam_governor", "Steam Governor", 6000, 12000, 45f, CarryClass.TwoHand, new(.5f, .9f, .5f), Fragility.Medium, SurfaceMaterial.Metal, .5f, .3f, Drum, new(.72f, .56f, .22f), false, Machinery),
            new("electric_motor", "Electric Motor", 2500, 5000, 180f, CarryClass.Heavy, new(.7f, .6f, 1f), Fragility.Low, SurfaceMaterial.Metal, .8f, .5f, Drum, new(.25f, .35f, .42f), false, Machinery),
            new("flywheel", "Flywheel", 4000, 8000, 600f, CarryClass.Huge, new(1.6f, .35f, 1.6f), Fragility.None, SurfaceMaterial.Metal, 1f, .25f, Drum, new(.28f, .27f, .26f), false, LootQuirk.TopHeavy, Machinery),
            new("crucible", "Crucible", 30000, 50000, 900f, CarryClass.Huge, new(1.3f, 1.4f, 1.3f), Fragility.Low, SurfaceMaterial.Metal, 1f, .1f, Drum, new(.35f, .22f, .15f), true, LootQuirk.Hot | LootQuirk.Sloshes, Machinery),

            // ---- Office records ------------------------------------------------------------------
            new("production_ledgers", "Production Ledgers", 200, 900, 5f, CarryClass.OneHand, new(.35f, .25f, .25f), Fragility.Low, SurfaceMaterial.Paper, .1f, 1.3f, Box, new(.3f, .22f, .15f), false, Records),
            new("typewriter", "Typewriter", 300, 900, 12f, CarryClass.TwoHand, new(.45f, .25f, .4f), Fragility.Medium, SurfaceMaterial.Metal, .4f, 1f, Box, new(.12f, .12f, .12f), false, Records),
            new("company_seal", "Company Seal Press", 500, 1500, 4f, CarryClass.OneHand, new(.18f, .22f, .25f), Fragility.Low, SurfaceMaterial.Metal, .2f, .8f, Box, new(.2f, .25f, .2f), false, Records),
            new("foremans_safe", "Foreman's Safe", 4000, 9000, 250f, CarryClass.Heavy, new(.6f, .8f, .6f), Fragility.None, SurfaceMaterial.Metal, .8f, .4f, Box, new(.25f, .28f, .22f), false, Records),
        };

        /// <summary>Creates any missing definition and fills in empty spawn tags. Returns how many were new.</summary>
        public static int CreateMissing()
        {
            int made = 0;
            foreach (Item item in Items)
            {
                string path = LootCatalogBuilder.PathFor(item.Id);
                var definition = AssetDatabase.LoadAssetAtPath<LootDefinition>(path);
                if (definition == null)
                {
                    definition = ScriptableObject.CreateInstance<LootDefinition>();
                    definition.EditorSetup(item.Id, item.Name, item.Min, item.Max, item.Kg, item.Class, item.Size, item.Fragility,
                        item.Material, item.Noise, item.Rarity, item.Shape, item.Color);
                    definition.EditorSetQuirks(item.Quirks);
                    AssetDatabase.CreateAsset(definition, path);
                    made++;
                }
                else if (definition.Quirks != item.Quirks)
                {
                    definition.EditorSetQuirks(item.Quirks);
                    EditorUtility.SetDirty(definition);
                }
                if (definition.SpawnTags == null || definition.SpawnTags.Length == 0)
                {
                    definition.EditorSetSpawning(item.Tags, item.Jackpot);
                    EditorUtility.SetDirty(definition);
                }
            }
            return made;
        }
    }
}
