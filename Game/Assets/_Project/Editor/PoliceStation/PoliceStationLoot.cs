using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;
using Item = Abandoned.EditorTools.StationLootKit.Item;
using L = Abandoned.EditorTools.PoliceStationLayout;
using Part = Abandoned.EditorTools.StationLootKit.Part;
using S = Abandoned.EditorTools.StationLootKit.Shape;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The precinct's salvage (Docs/POLICE_STATION_PLAN.md "Loot"): office leftovers, booking and cell-block kit, seized
    /// evidence and the armoury's gear, on the station's own four tags. Ordinary loot definitions, data only. Five
    /// jackpots: the seized safe and the impounded motorcycle (evidence), the rifle locker (armoury), the police call box
    /// and the trophy case (office).
    /// </summary>
    public static class PoliceStationLoot
    {
        [MenuItem(ToolMenu.PoliceStation + "Add Police Station Loot", priority = ToolMenu.PoliceStationP + 20)]
        public static void Menu() => Kit.EnsureContent();

        public static StationLootKit Kit => new(L.LootFolder, L.ArtFolder, L.LootPrefix, "PoliceStation", Items);

        private static Item I(string id, string name, int min, int max, float kg, CarryClass carry, Vector3 size, Fragility fragility,
            SurfaceMaterial material, float rarity, string tag, Part[] parts, LootQuirk quirks = LootQuirk.None, bool jackpot = false,
            float noise = .2f, bool wheeled = false) => new()
        {
            Id = L.LootPrefix + id, Name = name, Min = min, Max = max, Kg = kg, Class = carry, Size = size, Fragility = fragility,
            Material = material, Rarity = rarity, Tags = new[] { tag }, Parts = parts, Quirks = quirks, Jackpot = jackpot, Noise = noise,
            Wheeled = wheeled, Color = new Color(.3f, .32f, .36f),
        };

        private static Part B(float x, float y, float z, float w, float h, float d, string m) => new(new Vector3(x, y, z), new Vector3(w, h, d), m);
        private static Part R(float x, float y, float z, float w, float h, float d, string m) => new(new Vector3(x, y, z), new Vector3(w, h, d), m, S.Ball);
        private static Part T(float x, float y, float z, float w, float h, float d, string m) => new(new Vector3(x, y, z), new Vector3(w, h, d), m, S.Tube);

        public static readonly Item[] Items =
        {
            // ---- Office: the squad room, the bureau, records, the captain ------------------------------------------
            I("case_files", "Box of Case Files", 30, 120, 8f, CarryClass.TwoHand, new(.4f, .3f, .32f), Fragility.Low, SurfaceMaterial.Paper, 2f, L.Office,
                new[] { B(0, 0, 0, 1, 1, 1, "card"), B(0, .52f, 0, 1.02f, .06f, 1.02f, "card"), B(0, .3f, .51f, .5f, .2f, .02f, "paper") }),
            I("desk_computer", "Desktop Computer", 80, 250, 9f, CarryClass.TwoHand, new(.45f, .42f, .45f), Fragility.Medium, SurfaceMaterial.Plastic, 1.4f, L.Office,
                new[] { B(0, .1f, 0, 1, .8f, .9f, "white"), B(0, .12f, .46f, .8f, .6f, .02f, "black"), B(0, -.45f, 0, .5f, .1f, .5f, "white") }),
            I("typewriter", "Old Typewriter", 150, 450, 10f, CarryClass.TwoHand, new(.45f, .2f, .4f), Fragility.Medium, SurfaceMaterial.Metal, .8f, L.Office,
                new[] { B(0, -.15f, 0, 1, .6f, 1, "black"), B(0, .3f, -.3f, .95f, .3f, .3f, "black"), T(0, .45f, -.3f, .8f, .2f, .2f, "rubber"), B(0, -.1f, .4f, .8f, .1f, .2f, "white") }),
            I("desk_phone", "Desk Phone", 20, 80, 1.2f, CarryClass.OneHand, new(.22f, .12f, .2f), Fragility.Low, SurfaceMaterial.Plastic, 1.6f, L.Office,
                new[] { B(0, -.2f, 0, 1, .6f, 1, "black"), B(0, .35f, 0, 1, .3f, .35f, "black"), B(0, .05f, .2f, .5f, .2f, .3f, "grey") }, LootQuirk.Rings),
            I("coffee_maker", "Squad-Room Coffee Maker", 30, 100, 3f, CarryClass.OneHand, new(.25f, .38f, .28f), Fragility.Medium, SurfaceMaterial.Plastic, 1.4f, L.Office,
                new[] { B(0, 0, -.25f, 1, 1, .5f, "black"), B(0, -.4f, .2f, 1, .2f, .9f, "black"), R(0, -.12f, .2f, .7f, .45f, .7f, "glass") }, LootQuirk.Sloshes),
            I("bowling_trophy", "Bowling League Trophy", 60, 200, 1.5f, CarryClass.OneHand, new(.16f, .45f, .16f), Fragility.Medium, SurfaceMaterial.Metal, 1.1f, L.Office,
                new[] { B(0, -.4f, 0, 1, .2f, 1, "darkwood"), T(0, 0, 0, .3f, .6f, .3f, "gold"), R(0, .38f, 0, .6f, .25f, .6f, "gold") }),
            I("commendation_plaque", "Commendation Plaque", 40, 150, 1.4f, CarryClass.OneHand, new(.3f, .38f, .04f), Fragility.Low, SurfaceMaterial.Wood, 1.3f, L.Office,
                new[] { B(0, 0, 0, 1, 1, 1, "darkwood"), B(0, .05f, .52f, .75f, .6f, .1f, "brass") }),
            I("dress_cap", "Dress Uniform Cap", 40, 120, .3f, CarryClass.Pocket, new(.25f, .12f, .28f), Fragility.Low, SurfaceMaterial.Fabric, 1.2f, L.Office,
                new[] { T(0, .1f, 0, .95f, .8f, .9f, "blue"), B(0, -.35f, .35f, .8f, .1f, .35f, "black"), B(0, .05f, .45f, .25f, .25f, .05f, "gold") }),
            I("fingerprint_kit", "Fingerprint Kit", 120, 350, 2f, CarryClass.OneHand, new(.35f, .14f, .25f), Fragility.Low, SurfaceMaterial.Metal, 1f, L.Office,
                new[] { B(0, 0, 0, 1, 1, 1, "grey"), B(0, .52f, 0, .9f, .05f, .9f, "steel"), B(.4f, .2f, .5f, .1f, .3f, .05f, "chrome") }),
            I("filing_cabinet", "Filing Cabinet", 100, 300, 85f, CarryClass.Heavy, new(.48f, 1.33f, .64f), Fragility.None, SurfaceMaterial.Metal, .6f, L.Office,
                new[] { B(0, 0, 0, 1, 1, 1, "grey"), B(0, .3f, .51f, .85f, .2f, .02f, "steel"), B(0, .05f, .51f, .85f, .2f, .02f, "steel"), B(0, -.2f, .51f, .85f, .2f, .02f, "steel") }, noise: .7f),
            I("police_call_box", "Restored Police Call Box", 18000, 32000, 340f, CarryClass.Huge, new(1.1f, 2.2f, 1.1f), Fragility.Medium, SurfaceMaterial.Wood, .15f, L.Office,
                new[] { B(0, 0, 0, 1, .9f, 1, "blue"), B(0, .47f, 0, 1.06f, .06f, 1.06f, "blue"), B(0, .5f, 0, .3f, .08f, .3f, "lamp"), B(0, .3f, .505f, .7f, .1f, .02f, "black"), B(0, -.05f, .505f, .7f, .5f, .02f, "glass") },
                jackpot: true, noise: 1f),
            I("trophy_case", "Precinct Trophy Case", 9000, 16000, 210f, CarryClass.Huge, new(1.6f, 1.9f, .5f), Fragility.High, SurfaceMaterial.Glass, .2f, L.Office,
                new[] { B(0, -.42f, 0, 1, .16f, 1, "darkwood"), B(0, .44f, 0, 1, .12f, 1, "darkwood"), B(0, 0, 0, .98f, .76f, .96f, "glass"), R(-.25f, 0, 0, .12f, .15f, .3f, "gold"), R(.2f, .2f, 0, .1f, .12f, .3f, "gold") },
                jackpot: true, noise: .9f),

            // ---- Booking and the cell block ---------------------------------------------------------------------
            I("booking_camera", "Booking Camera", 200, 600, 3f, CarryClass.OneHand, new(.25f, .2f, .32f), Fragility.High, SurfaceMaterial.Metal, 1f, L.Cells,
                new[] { B(0, 0, -.1f, 1, 1, .7f, "black"), T(0, 0, .42f, .6f, .6f, .3f, "glass"), B(0, .55f, -.2f, .4f, .1f, .3f, "steel") }),
            I("fingerprint_scanner", "Fingerprint Scanner", 300, 700, 7f, CarryClass.TwoHand, new(.45f, .25f, .35f), Fragility.Medium, SurfaceMaterial.Plastic, .9f, L.Cells,
                new[] { B(0, 0, 0, 1, 1, 1, "grey"), B(0, .52f, .1f, .5f, .04f, .4f, "glass"), B(-.3f, .52f, -.3f, .25f, .04f, .2f, "black") }),
            I("breathalyser", "Breathalyser", 80, 250, .4f, CarryClass.Pocket, new(.07f, .16f, .04f), Fragility.Medium, SurfaceMaterial.Plastic, 1.4f, L.Cells,
                new[] { B(0, -.1f, 0, 1, .8f, 1, "yellow"), B(0, .2f, .5f, .6f, .3f, .05f, "black"), T(0, .5f, 0, .3f, .2f, .3f, "white") }),
            I("property_bag", "Inmate Property Bag", 50, 250, .6f, CarryClass.Pocket, new(.22f, .3f, .06f), Fragility.Low, SurfaceMaterial.Plastic, 1.8f, L.Cells,
                new[] { B(0, 0, 0, 1, 1, 1, "glass"), B(0, -.1f, 0, .7f, .5f, .7f, "leather"), B(0, .45f, 0, 1, .08f, 1.1f, "red") }),
            I("height_board", "Line-up Height Board", 60, 180, 9f, CarryClass.TwoHand, new(1.2f, .9f, .05f), Fragility.Low, SurfaceMaterial.Wood, .9f, L.Cells,
                new[] { B(0, 0, 0, 1, 1, 1, "white"), B(0, .3f, .52f, .95f, .015f, .02f, "black"), B(0, 0, .52f, .95f, .015f, .02f, "black"), B(0, -.3f, .52f, .95f, .015f, .02f, "black") }),
            I("handcuffs", "Handcuffs", 30, 90, .4f, CarryClass.Pocket, new(.22f, .03f, .1f), Fragility.None, SurfaceMaterial.Metal, 2f, L.Cells,
                new[] { T(-.28f, 0, 0, .45f, 1, .9f, "chrome"), T(.28f, 0, 0, .45f, 1, .9f, "chrome"), B(0, 0, 0, .2f, .5f, .15f, "steel") }, LootQuirk.Rattles),

            // ---- Seized evidence -----------------------------------------------------------------------------------
            I("seized_cash", "Bundle of Seized Cash", 300, 900, .5f, CarryClass.Pocket, new(.16f, .08f, .07f), Fragility.Low, SurfaceMaterial.Paper, 1.2f, L.Evidence,
                new[] { B(0, 0, 0, 1, 1, 1, "money"), B(0, 0, 0, .25f, 1.02f, 1.02f, "white") }),
            I("bagged_jewellery", "Bagged Jewellery", 400, 1200, .3f, CarryClass.Pocket, new(.15f, .04f, .2f), Fragility.Medium, SurfaceMaterial.Plastic, 1f, L.Evidence,
                new[] { B(0, 0, 0, 1, 1, 1, "glass"), R(-.2f, 0, .1f, .3f, .6f, .25f, "gold"), R(.2f, 0, -.15f, .25f, .6f, .2f, "chrome"), B(0, .4f, -.45f, 1, .2f, .1f, "red") }),
            I("seized_laptop", "Seized Laptop", 200, 600, 2.4f, CarryClass.OneHand, new(.36f, .04f, .26f), Fragility.Medium, SurfaceMaterial.Plastic, 1.1f, L.Evidence,
                new[] { B(0, 0, 0, 1, 1, 1, "grey"), B(0, .52f, 0, .9f, .05f, .9f, "black") }),
            I("counterfeit_handbag", "Counterfeit Handbag", 60, 200, .9f, CarryClass.OneHand, new(.34f, .26f, .14f), Fragility.Low, SurfaceMaterial.Fabric, 1.4f, L.Evidence,
                new[] { B(0, -.1f, 0, 1, .8f, 1, "leather"), T(0, .4f, 0, .6f, .1f, .2f, "gold"), B(0, -.1f, .51f, .3f, .2f, .02f, "gold") }),
            I("car_stereo", "Seized Car Stereo", 50, 180, 1.6f, CarryClass.OneHand, new(.18f, .06f, .17f), Fragility.Medium, SurfaceMaterial.Metal, 1.5f, L.Evidence,
                new[] { B(0, 0, 0, 1, 1, 1, "black"), B(0, 0, .51f, .9f, .7f, .02f, "chrome") }),
            I("stolen_painting", "Recovered Stolen Painting", 1500, 4000, 6f, CarryClass.TwoHand, new(.9f, .7f, .06f), Fragility.High, SurfaceMaterial.Wood, .5f, L.Evidence,
                new[] { B(0, 0, 0, 1, 1, 1, "gold"), B(0, 0, .52f, .85f, .82f, .05f, "paint"), B(0, 0, -.55f, 1.02f, 1.02f, .05f, "glass") }),
            I("gold_bar", "Seized Gold Bar", 2000, 4500, 12.4f, CarryClass.OneHand, new(.26f, .05f, .08f), Fragility.None, SurfaceMaterial.Metal, .3f, L.Evidence,
                new[] { B(0, 0, 0, 1, 1, 1, "gold"), B(0, .52f, 0, .9f, .05f, .8f, "gold") }, noise: .4f),
            I("slot_machine", "Seized Slot Machine", 600, 1500, 95f, CarryClass.Heavy, new(.6f, 1.5f, .55f), Fragility.Medium, SurfaceMaterial.Metal, .5f, L.Evidence,
                new[] { B(0, 0, 0, 1, 1, 1, "red"), B(0, .15f, .51f, .8f, .3f, .02f, "glass"), B(.55f, .2f, 0, .1f, .05f, .1f, "chrome"), R(.55f, .32f, 0, .15f, .1f, .15f, "red"), B(0, -.3f, .51f, .6f, .1f, .02f, "chrome") },
                LootQuirk.Rattles, noise: .8f),
            I("seized_safe", "Seized Safe", 8000, 15000, 280f, CarryClass.Heavy, new(.7f, .9f, .7f), Fragility.None, SurfaceMaterial.Metal, .25f, L.Evidence,
                new[] { B(0, 0, 0, 1, 1, 1, "black"), B(0, 0, .505f, .85f, .85f, .02f, "grey"), T(.15f, .05f, .52f, .25f, .05f, .25f, "chrome"), B(-.3f, .05f, .53f, .05f, .3f, .05f, "chrome") },
                jackpot: true, noise: 1f),
            I("impounded_motorcycle", "Impounded Motorcycle", 12000, 22000, 190f, CarryClass.Huge, new(.8f, 1.1f, 2.1f), Fragility.Low, SurfaceMaterial.Metal, .2f, L.Evidence,
                new[] { T(0, -.3f, .35f, .12f, .55f, .3f, "rubber"), T(0, -.3f, -.35f, .12f, .55f, .3f, "rubber"), B(0, .05f, 0, .35f, .3f, .55f, "paint"), B(0, .2f, -.2f, .3f, .1f, .35f, "black"),
                    B(0, .35f, .4f, .9f, .05f, .05f, "chrome"), B(0, 0, .05f, .2f, .2f, .3f, "steel") },
                jackpot: true, noise: 1f, wheeled: true),

            // ---- The armoury, the range and the locker room ---------------------------------------------------------
            I("ammo_can", "Ammunition Can", 80, 250, 8f, CarryClass.OneHand, new(.3f, .19f, .16f), Fragility.None, SurfaceMaterial.Metal, 1.5f, L.Armoury,
                new[] { B(0, 0, 0, 1, 1, 1, "green"), B(0, .55f, 0, .4f, .1f, .2f, "black") }, LootQuirk.Rattles, noise: .4f),
            I("tactical_vest", "Tactical Vest", 150, 400, 4f, CarryClass.OneHand, new(.48f, .55f, .2f), Fragility.Low, SurfaceMaterial.Fabric, 1.2f, L.Armoury,
                new[] { B(0, 0, 0, 1, 1, .7f, "black"), B(0, .1f, .4f, .8f, .4f, .2f, "black"), B(0, .25f, .5f, .5f, .1f, .02f, "white") }),
            I("riot_helmet", "Riot Helmet", 100, 300, 1.6f, CarryClass.OneHand, new(.28f, .26f, .32f), Fragility.Low, SurfaceMaterial.Plastic, 1.3f, L.Armoury,
                new[] { R(0, -.05f, 0, 1, .9f, 1, "black"), B(0, -.05f, .45f, .8f, .5f, .05f, "glass") }),
            I("police_radio", "Handheld Police Radio", 100, 300, .8f, CarryClass.OneHand, new(.07f, .24f, .05f), Fragility.Medium, SurfaceMaterial.Plastic, 1.4f, L.Armoury,
                new[] { B(0, -.1f, 0, 1, .8f, 1, "black"), T(.25f, .4f, 0, .2f, .3f, .2f, "rubber"), B(0, .1f, .52f, .6f, .2f, .02f, "grey") }),
            I("breaching_ram", "Breaching Ram", 200, 500, 16f, CarryClass.TwoHand, new(.18f, .18f, .75f), Fragility.None, SurfaceMaterial.Metal, .8f, L.Armoury,
                new[] { new Part(Vector3.zero, new Vector3(1, 1, 1), "black", S.Box), B(0, .55f, .15f, .2f, .15f, .2f, "steel"), B(0, .55f, -.2f, .2f, .15f, .2f, "steel"), B(0, 0, .5f, 1.02f, 1.02f, .05f, "red") }, noise: .5f),
            I("spotting_scope", "Range Spotting Scope", 300, 900, 2f, CarryClass.OneHand, new(.12f, .14f, .42f), Fragility.High, SurfaceMaterial.Glass, .8f, L.Armoury,
                new[] { new Part(new Vector3(0, .1f, 0), new Vector3(.8f, .7f, 1), "green", S.Box), B(0, .1f, .51f, .6f, .6f, .02f, "glass"), B(0, -.35f, -.1f, .3f, .3f, .2f, "black") }),
            I("rifle_locker", "Armoury Rifle Locker", 7000, 13000, 320f, CarryClass.Heavy, new(.75f, 1.8f, .5f), Fragility.None, SurfaceMaterial.Metal, .2f, L.Armoury,
                new[] { B(0, 0, 0, 1, 1, 1, "green"), B(-.25f, 0, .505f, .48f, .95f, .02f, "grey"), B(.25f, 0, .505f, .48f, .95f, .02f, "grey"), B(0, .05f, .52f, .04f, .2f, .04f, "chrome") },
                jackpot: true, noise: 1f),
        };
    }
}
