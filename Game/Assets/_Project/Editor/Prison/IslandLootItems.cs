using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Gallows Rock's salvage (the user's four kinds): inmate contraband, guard and armoury gear, the warden's
    /// valuables, and lighthouse and maritime pieces. Ordinary loot definitions, data only, no code per item. They
    /// spawn on the island's own tags (plus the factory's records and workshop tags in its offices and boiler room, and
    /// the mall's gallery tag in the warden's house: its grandfather clock and paintings), so no other location's
    /// loot changes. Like <see cref="LootCatalogBuilder"/>, it never overwrites an existing asset: Inspector tuning is kept.
    /// </summary>
    public static class IslandLootItems
    {
        public const string Contraband = "contraband", Armoury = "armoury", Warden = "warden", Maritime = "maritime";

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

            public Item(string id, string name, int min, int max, float kg, CarryClass carry, Vector3 size, Fragility fragility,
                SurfaceMaterial material, float noise, float rarity, PlaceholderShape shape, Color color, bool jackpot, params string[] tags)
            {
                Id = id; Name = name; Min = min; Max = max; Kg = kg; Class = carry; Size = size; Fragility = fragility;
                Material = material; Noise = noise; Rarity = rarity; Shape = shape; Color = color; Jackpot = jackpot; Tags = tags;
            }
        }

        private const PlaceholderShape Box = PlaceholderShape.Cube, Drum = PlaceholderShape.Cylinder, Ball = PlaceholderShape.Sphere;

        public static readonly Item[] Items =
        {
            // ---- Inmate contraband: small, odd and funny --------------------------------------------------
            new("soap_pistol", "Carved Soap Pistol", 120, 400, .25f, CarryClass.Pocket, new(.16f, .1f, .04f), Fragility.High, SurfaceMaterial.Plastic, .05f, 1.4f, Box, new(.86f, .83f, .72f), false, Contraband),
            new("dummy_head", "Papier-Mache Dummy Head", 400, 1200, 1.5f, CarryClass.TwoHand, new(.28f, .34f, .3f), Fragility.High, SurfaceMaterial.Paper, .1f, .6f, Ball, new(.78f, .62f, .5f), false, Contraband),
            new("spoon_digger", "Sharpened Spoon", 60, 200, .1f, CarryClass.Pocket, new(.18f, .02f, .04f), Fragility.None, SurfaceMaterial.Metal, .05f, 1.6f, Box, new(.7f, .7f, .7f), false, Contraband),
            new("cigarette_carton", "Cigarette Carton", 80, 300, .3f, CarryClass.OneHand, new(.27f, .1f, .13f), Fragility.Low, SurfaceMaterial.Paper, .05f, 1.5f, Box, new(.75f, .7f, .55f), false, Contraband),
            new("homemade_radio", "Homemade Radio", 300, 900, 1.2f, CarryClass.OneHand, new(.25f, .15f, .12f), Fragility.Medium, SurfaceMaterial.Wood, .2f, .8f, Box, new(.3f, .25f, .2f), false, Contraband),
            new("inmate_harmonica", "Inmate's Harmonica", 100, 350, .1f, CarryClass.Pocket, new(.1f, .025f, .03f), Fragility.Low, SurfaceMaterial.Metal, .05f, 1.2f, Box, new(.75f, .75f, .75f), false, Contraband),
            new("raincoat_raft", "Raincoat Raft", 1500, 4000, 9f, CarryClass.TwoHand, new(.9f, .35f, .6f), Fragility.Low, SurfaceMaterial.Fabric, .3f, .25f, Box, new(.58f, .48f, .24f), false, Contraband),

            // ---- Guard and armoury gear --------------------------------------------------------------------
            new("guard_keyring", "Guard's Key Ring", 150, 450, .4f, CarryClass.Pocket, new(.12f, .03f, .12f), Fragility.None, SurfaceMaterial.Metal, .1f, 1.4f, Drum, new(.7f, .6f, .35f), false, Armoury),
            new("riot_shield", "Riot Shield", 200, 500, 6f, CarryClass.TwoHand, new(.55f, .9f, .1f), Fragility.Low, SurfaceMaterial.Plastic, .3f, 1f, Box, new(.6f, .65f, .7f), false, Armoury),
            new("guard_radio", "Guard Radio", 150, 450, 1.1f, CarryClass.OneHand, new(.08f, .22f, .06f), Fragility.Medium, SurfaceMaterial.Plastic, .1f, 1.2f, Box, new(.12f, .12f, .12f), false, Armoury),
            new("nightstick", "Nightstick", 60, 180, .6f, CarryClass.OneHand, new(.05f, .05f, .6f), Fragility.None, SurfaceMaterial.Wood, .2f, 1.4f, Drum, new(.15f, .12f, .1f), false, Armoury),
            new("gas_mask", "Gas Mask", 120, 380, .8f, CarryClass.OneHand, new(.22f, .25f, .2f), Fragility.Low, SurfaceMaterial.Plastic, .1f, 1.1f, Ball, new(.2f, .22f, .2f), false, Armoury),
            new("armoury_gun_safe", "Armoury Gun Safe", 6000, 12000, 280f, CarryClass.Heavy, new(.7f, 1.5f, .5f), Fragility.None, SurfaceMaterial.Metal, .9f, .2f, Box, new(.2f, .24f, .22f), true, Armoury),

            // ---- The warden's valuables ---------------------------------------------------------------------
            new("warden_portrait", "Warden's Portrait", 800, 2500, 6f, CarryClass.TwoHand, new(.7f, .9f, .06f), Fragility.High, SurfaceMaterial.Wood, .2f, .7f, Box, new(.4f, .28f, .15f), false, Warden),
            new("silver_tea_set", "Silver Tea Service", 400, 1200, 3f, CarryClass.OneHand, new(.4f, .22f, .25f), Fragility.Medium, SurfaceMaterial.Metal, .2f, 1f, Box, new(.75f, .75f, .78f), false, Warden),
            new("gramophone", "Gramophone", 600, 1500, 7f, CarryClass.TwoHand, new(.4f, .55f, .45f), Fragility.Medium, SurfaceMaterial.Wood, .3f, .7f, Box, new(.4f, .25f, .12f), false, Warden),
            new("warden_pocket_watch", "Warden's Pocket Watch", 300, 900, .1f, CarryClass.Pocket, new(.05f, .06f, .015f), Fragility.Medium, SurfaceMaterial.Metal, .02f, 1f, Drum, new(.75f, .62f, .3f), false, Warden),
            new("warden_piano", "Warden's Upright Piano", 20000, 35000, 260f, CarryClass.Huge, new(1.5f, 1.25f, .6f), Fragility.Medium, SurfaceMaterial.Wood, 1f, .1f, Box, new(.12f, .08f, .06f), true, Warden),

            // ---- Lighthouse and maritime ----------------------------------------------------------------------
            new("ships_bell", "Ship's Bell", 500, 1400, 18f, CarryClass.TwoHand, new(.35f, .4f, .35f), Fragility.None, SurfaceMaterial.Metal, .6f, .8f, Drum, new(.7f, .52f, .22f), false, Maritime),
            new("diving_helmet", "Brass Diving Helmet", 1500, 4000, 16f, CarryClass.TwoHand, new(.42f, .48f, .42f), Fragility.Low, SurfaceMaterial.Metal, .5f, .5f, Ball, new(.68f, .5f, .22f), false, Maritime),
            new("brass_sextant", "Brass Sextant", 600, 1800, 1.4f, CarryClass.OneHand, new(.25f, .25f, .1f), Fragility.Medium, SurfaceMaterial.Metal, .1f, .8f, Box, new(.68f, .52f, .24f), false, Maritime),
            new("signal_lamp", "Signal Lamp", 250, 700, 3f, CarryClass.OneHand, new(.25f, .3f, .25f), Fragility.Medium, SurfaceMaterial.Metal, .2f, 1.1f, Drum, new(.2f, .3f, .25f), false, Maritime),
            new("life_ring", "Life Ring", 40, 120, 2f, CarryClass.OneHand, new(.7f, .12f, .7f), Fragility.None, SurfaceMaterial.Plastic, .1f, 1.8f, Drum, new(.85f, .45f, .35f), false, Maritime),
            new("ship_in_bottle", "Ship in a Bottle", 300, 1000, .8f, CarryClass.OneHand, new(.3f, .1f, .1f), Fragility.High, SurfaceMaterial.Glass, .05f, .9f, Drum, new(.6f, .75f, .7f), false, Maritime),
            new("fresnel_lens", "Lighthouse Fresnel Lens", 35000, 55000, 450f, CarryClass.Huge, new(1.4f, 1.6f, 1.4f), Fragility.Medium, SurfaceMaterial.Glass, 1f, .1f, Drum, new(.75f, .85f, .8f), true, Maritime),
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
                    AssetDatabase.CreateAsset(definition, path);
                    made++;
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
