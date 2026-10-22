using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEngine;
using Item = Abandoned.EditorTools.StationLootKit.Item;
using L = Abandoned.EditorTools.FireStationLayout;
using Part = Abandoned.EditorTools.StationLootKit.Part;
using S = Abandoned.EditorTools.StationLootKit.Shape;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The firehouse's salvage (Docs/FIRE_STATION_PLAN.md "Loot"): apparatus-bay gear, the crew's quarters, the hose
    /// tower's brass and the watch office's old alarm equipment, on the station's own four tags. Data only. Three
    /// jackpots: the antique hand pumper (apparatus), the vintage jukebox (quarters) and the bronze station bell (tower).
    /// </summary>
    public static class FireStationLoot
    {
        public static StationLootKit Kit => new(L.LootFolder, L.ArtFolder, L.LootPrefix, "FireStation", Items);

        private static Item I(string id, string name, int min, int max, float kg, CarryClass carry, Vector3 size, Fragility fragility,
            SurfaceMaterial material, float rarity, string tag, Part[] parts, LootQuirk quirks = LootQuirk.None, bool jackpot = false,
            float noise = .2f, bool wheeled = false) => new()
        {
            Id = L.LootPrefix + id, Name = name, Min = min, Max = max, Kg = kg, Class = carry, Size = size, Fragility = fragility,
            Material = material, Rarity = rarity, Tags = new[] { tag }, Parts = parts, Quirks = quirks, Jackpot = jackpot, Noise = noise,
            Wheeled = wheeled, Color = new Color(.62f, .1f, .07f),
        };

        private static Part B(float x, float y, float z, float w, float h, float d, string m) => new(new Vector3(x, y, z), new Vector3(w, h, d), m);
        private static Part R(float x, float y, float z, float w, float h, float d, string m) => new(new Vector3(x, y, z), new Vector3(w, h, d), m, S.Ball);
        private static Part T(float x, float y, float z, float w, float h, float d, string m) => new(new Vector3(x, y, z), new Vector3(w, h, d), m, S.Tube);

        public static readonly Item[] Items =
        {
            // ---- The apparatus bay and the hose workshop ---------------------------------------------------------
            I("brass_nozzle", "Brass Hose Nozzle", 150, 450, 4f, CarryClass.OneHand, new(.12f, .12f, .45f), Fragility.None, SurfaceMaterial.Metal, 1.4f, L.Apparatus,
                new[] { new Part(new Vector3(0, 0, -.2f), new Vector3(.9f, .9f, .6f), "brass", S.Box), B(0, 0, .3f, .55f, .55f, .4f, "brass"), B(0, .55f, -.1f, .2f, .2f, .3f, "black") }),
            I("fire_axe", "Fire Axe", 60, 180, 3f, CarryClass.OneHand, new(.3f, .06f, .9f), Fragility.None, SurfaceMaterial.Metal, 1.6f, L.Apparatus,
                new[] { B(0, 0, -.1f, .15f, .7f, .85f, "wood"), B(0, 0, .42f, 1, .8f, .16f, "red"), B(.3f, 0, .42f, .3f, .9f, .12f, "steel") }),
            I("halligan_bar", "Halligan Bar", 80, 220, 5f, CarryClass.OneHand, new(.2f, .06f, .8f), Fragility.None, SurfaceMaterial.Metal, 1.4f, L.Apparatus,
                new[] { B(0, 0, 0, .2f, .8f, 1, "steel"), B(0, 0, .45f, 1, .6f, .08f, "steel"), B(0, 0, -.48f, .6f, .6f, .05f, "steel") }, noise: .4f),
            I("scba_tank", "Breathing-Air Cylinder", 100, 300, 8f, CarryClass.TwoHand, new(.18f, .62f, .18f), Fragility.Low, SurfaceMaterial.Metal, 1.4f, L.Apparatus,
                new[] { T(0, -.05f, 0, 1, .9f, 1, "yellow"), R(0, .42f, 0, .9f, .2f, .9f, "yellow"), T(0, .5f, 0, .3f, .1f, .3f, "chrome") }),
            I("leather_helmet", "Leather Fire Helmet", 200, 700, 1.8f, CarryClass.OneHand, new(.32f, .24f, .42f), Fragility.Low, SurfaceMaterial.Fabric, 1f, L.Apparatus,
                new[] { R(0, .05f, -.05f, .65f, .9f, .65f, "leather"), B(0, -.4f, 0, 1, .1f, 1, "leather"), B(0, .1f, .35f, .3f, .4f, .05f, "brass") }),
            I("thermal_camera", "Firefighter's Thermal Camera", 400, 1100, 1.5f, CarryClass.OneHand, new(.16f, .22f, .2f), Fragility.Medium, SurfaceMaterial.Plastic, .9f, L.Apparatus,
                new[] { B(0, 0, 0, 1, 1, 1, "yellow"), B(0, .1f, .51f, .7f, .5f, .02f, "black"), B(0, -.45f, -.1f, .5f, .2f, .6f, "rubber") }),
            I("rescue_spreader", "Hydraulic Rescue Spreader", 600, 1500, 20f, CarryClass.TwoHand, new(.3f, .25f, .85f), Fragility.Low, SurfaceMaterial.Metal, .8f, L.Apparatus,
                new[] { B(0, 0, -.25f, .8f, .9f, .5f, "yellow"), B(-.2f, 0, .3f, .2f, .5f, .6f, "steel"), B(.2f, 0, .3f, .2f, .5f, .6f, "steel"), T(0, .45f, -.25f, .3f, .1f, .3f, "black") }, noise: .5f),
            I("engine_siren", "Chrome Engine Siren", 300, 800, 15f, CarryClass.TwoHand, new(.35f, .35f, .45f), Fragility.Low, SurfaceMaterial.Metal, .9f, L.Apparatus,
                new[] { T(0, 0, 0, 1, .8f, 1, "chrome"), B(0, 0, .45f, .8f, .8f, .1f, "chrome"), B(0, -.45f, -.1f, .6f, .1f, .6f, "black") }),
            I("portable_pump", "Portable Fire Pump", 500, 1200, 85f, CarryClass.Heavy, new(.8f, .7f, .6f), Fragility.Low, SurfaceMaterial.Metal, .6f, L.Apparatus,
                new[] { B(0, -.1f, 0, .9f, .7f, .9f, "red"), B(0, .4f, 0, 1, .06f, 1, "steel"), T(.4f, 0, .2f, .2f, .5f, .2f, "brass"), B(-.3f, .2f, 0, .3f, .3f, .5f, "black") }, noise: .8f),
            I("foam_drum", "Drum of Foam Concentrate", 100, 300, 110f, CarryClass.Heavy, new(.6f, .9f, .6f), Fragility.Low, SurfaceMaterial.Metal, .8f, L.Apparatus,
                new[] { T(0, 0, 0, 1, 1, 1, "yellow"), T(0, .5f, 0, 1.02f, .03f, 1.02f, "steel"), T(0, 0, 0, 1.02f, .05f, 1.02f, "steel") }, LootQuirk.Sloshes, noise: .7f),
            I("hand_pumper", "Antique Hand Pumper", 25000, 45000, 480f, CarryClass.Huge, new(1.4f, 1.6f, 2.8f), Fragility.Medium, SurfaceMaterial.Wood, .1f, L.Apparatus,
                new[] { B(0, .05f, 0, .8f, .5f, .7f, "red"), T(0, .45f, .1f, .35f, .3f, .35f, "brass"), B(0, .38f, 0, 1.1f, .04f, .04f, "wood"), B(0, .38f, -.42f, .04f, .04f, .4f, "wood"),
                    T(-.45f, -.25f, .3f, .08f, .5f, .5f, "darkwood"), T(.45f, -.25f, .3f, .08f, .5f, .5f, "darkwood"), T(-.45f, -.25f, -.3f, .08f, .5f, .5f, "darkwood"), T(.45f, -.25f, -.3f, .08f, .5f, .5f, "darkwood") },
                jackpot: true, noise: 1f, wheeled: true),

            // ---- The quarters: day room, kitchen, bunk room, captain's quarters, fitness ----------------------------
            I("alarm_clock", "Wind-up Alarm Clock", 20, 80, .4f, CarryClass.Pocket, new(.12f, .14f, .07f), Fragility.Medium, SurfaceMaterial.Metal, 1.8f, L.Quarters,
                new[] { T(0, 0, 0, 1, .85f, .9f, "red"), R(-.3f, .5f, 0, .35f, .25f, .5f, "chrome"), R(.3f, .5f, 0, .35f, .25f, .5f, "chrome"), B(0, 0, .48f, .7f, .7f, .05f, "white") }, LootQuirk.Rings),
            I("chili_pot", "Firehouse Chili Pot", 30, 120, 9f, CarryClass.TwoHand, new(.38f, .3f, .38f), Fragility.Low, SurfaceMaterial.Metal, 1.5f, L.Quarters,
                new[] { T(0, 0, 0, 1, .9f, 1, "steel"), T(0, .48f, 0, 1.02f, .05f, 1.02f, "steel"), B(0, .2f, 0, 1.3f, .08f, .1f, "black") }, LootQuirk.Sloshes),
            I("dalmatian_statue", "Dalmatian Statue", 200, 600, 15f, CarryClass.TwoHand, new(.3f, .6f, .6f), Fragility.High, SurfaceMaterial.Stone, .9f, L.Quarters,
                new[] { R(0, -.05f, -.05f, .8f, .45f, .8f, "white"), R(0, .3f, .3f, .55f, .4f, .4f, "white"), R(.15f, .05f, 0, .2f, .2f, .2f, "black"), B(0, -.4f, 0, .9f, .2f, .9f, "red") }),
            I("poker_chips", "Poker Night Chip Case", 100, 350, 3f, CarryClass.OneHand, new(.4f, .1f, .25f), Fragility.Low, SurfaceMaterial.Metal, 1.2f, L.Quarters,
                new[] { B(0, 0, 0, 1, 1, 1, "chrome"), B(0, .52f, 0, .9f, .04f, .9f, "black"), B(0, 0, .51f, .2f, .3f, .02f, "chrome") }, LootQuirk.Rattles),
            I("day_tv", "Day-Room Television", 80, 250, 18f, CarryClass.TwoHand, new(.7f, .55f, .45f), Fragility.Medium, SurfaceMaterial.Plastic, 1f, L.Quarters,
                new[] { B(0, 0, -.05f, 1, 1, .9f, "black"), B(0, .02f, .42f, .85f, .8f, .02f, "glass") }),
            I("espresso_machine", "Station Espresso Machine", 200, 600, 14f, CarryClass.TwoHand, new(.45f, .42f, .45f), Fragility.Medium, SurfaceMaterial.Metal, .9f, L.Quarters,
                new[] { B(0, 0, -.1f, 1, 1, .8f, "chrome"), B(0, -.2f, .4f, .4f, .1f, .2f, "black"), T(-.25f, .1f, .38f, .12f, .15f, .12f, "black"), B(0, .52f, -.1f, .9f, .05f, .7f, "black") }),
            I("acoustic_guitar", "Acoustic Guitar", 150, 500, 3f, CarryClass.TwoHand, new(.38f, 1f, .12f), Fragility.Medium, SurfaceMaterial.Wood, 1f, L.Quarters,
                new[] { R(0, -.22f, 0, 1, .5f, 1, "wood"), R(0, .02f, 0, .75f, .35f, 1, "wood"), B(0, .35f, 0, .14f, .5f, .5f, "darkwood"), B(0, -.15f, .52f, .3f, .3f, .02f, "black") }),
            I("dumbbells", "Pair of Dumbbells", 30, 90, 20f, CarryClass.OneHand, new(.4f, .15f, .3f), Fragility.None, SurfaceMaterial.Metal, 1.4f, L.Quarters,
                new[] { B(-.3f, 0, 0, .25f, 1, 1, "black"), B(.3f, 0, 0, .25f, 1, 1, "black"), B(0, 0, 0, 1, .2f, .2f, "chrome") }, noise: .5f),
            I("jukebox", "Vintage Jukebox", 8000, 16000, 150f, CarryClass.Huge, new(.95f, 1.55f, .7f), Fragility.Medium, SurfaceMaterial.Wood, .15f, L.Quarters,
                new[] { B(0, -.15f, 0, 1, .7f, 1, "darkwood"), R(0, .3f, 0, 1, .45f, .95f, "glass"), B(0, .3f, .02f, .9f, .1f, .95f, "lamp"), B(0, -.3f, .51f, .6f, .2f, .02f, "chrome"), B(-.45f, 0, .5f, .05f, .9f, .05f, "redlamp"), B(.45f, 0, .5f, .05f, .9f, .05f, "redlamp") },
                jackpot: true, noise: .9f),

            // ---- The hose tower and the turnout room --------------------------------------------------------------
            I("hose_roll", "Rolled Canvas Hose", 40, 150, 18f, CarryClass.TwoHand, new(.55f, .55f, .14f), Fragility.None, SurfaceMaterial.Fabric, 1.6f, L.Tower,
                new[] { T(0, 0, 0, 1, 1, 1, "khaki"), T(0, 0, 0, .3f, 1.05f, .3f, "brass") }),
            I("brass_coupling", "Brass Hose Coupling", 60, 180, 1f, CarryClass.Pocket, new(.1f, .08f, .1f), Fragility.None, SurfaceMaterial.Metal, 1.8f, L.Tower,
                new[] { T(0, 0, 0, 1, 1, 1, "brass"), B(.5f, 0, 0, .2f, .6f, .2f, "brass"), B(-.5f, 0, 0, .2f, .6f, .2f, "brass") }),
            I("hydrant_spanner", "Hydrant Spanner", 30, 90, .8f, CarryClass.Pocket, new(.3f, .03f, .08f), Fragility.None, SurfaceMaterial.Metal, 1.6f, L.Tower,
                new[] { B(0, 0, 0, 1, 1, .5f, "steel"), B(.4f, 0, .2f, .2f, 1, .8f, "steel") }),
            I("signal_lantern", "Fireman's Signal Lantern", 300, 900, 2f, CarryClass.OneHand, new(.18f, .32f, .18f), Fragility.Medium, SurfaceMaterial.Glass, .9f, L.Tower,
                new[] { T(0, -.05f, 0, .9f, .6f, .9f, "glass"), T(0, -.45f, 0, 1, .1f, 1, "brass"), T(0, .32f, 0, .7f, .15f, .7f, "brass"), B(0, .46f, 0, .5f, .08f, .08f, "brass") }),
            I("brass_whistle", "Brass Fire Whistle", 100, 300, .2f, CarryClass.Pocket, new(.1f, .03f, .03f), Fragility.Low, SurfaceMaterial.Metal, 1.2f, L.Tower,
                new[] { T(0, 0, 0, 1, 1, 1, "brass") }),
            I("station_bell", "Bronze Station Bell", 9000, 16000, 160f, CarryClass.Heavy, new(.7f, .8f, .7f), Fragility.None, SurfaceMaterial.Metal, .2f, L.Tower,
                new[] { R(0, .05f, 0, .9f, .9f, .9f, "brass"), T(0, -.35f, 0, 1, .1f, 1, "brass"), B(0, .45f, 0, .3f, .1f, .3f, "black") },
                jackpot: true, noise: 1f),

            // ---- The watch office, the station office and the storage loft -----------------------------------------
            I("tape_register", "Alarm Tape Register", 800, 2000, 12f, CarryClass.TwoHand, new(.4f, .4f, .3f), Fragility.Medium, SurfaceMaterial.Metal, .7f, L.Watch,
                new[] { B(0, -.2f, 0, 1, .6f, 1, "darkwood"), R(0, .3f, 0, .8f, .4f, .8f, "glass"), T(0, .05f, .2f, .3f, .2f, .3f, "brass"), B(0, -.1f, .51f, .5f, .05f, .02f, "paper") }),
            I("pull_box", "Cast-Iron Alarm Box", 150, 400, 7f, CarryClass.OneHand, new(.2f, .32f, .14f), Fragility.Low, SurfaceMaterial.Metal, 1f, L.Watch,
                new[] { B(0, 0, 0, 1, 1, 1, "red"), B(0, .45f, 0, 1.05f, .1f, 1.05f, "red"), B(0, -.05f, .52f, .4f, .2f, .05f, "white") }),
            I("muster_trophy", "Muster Trophy Cup", 200, 600, 2f, CarryClass.OneHand, new(.2f, .4f, .2f), Fragility.Medium, SurfaceMaterial.Metal, 1f, L.Watch,
                new[] { T(0, -.4f, 0, .7f, .2f, .7f, "darkwood"), T(0, -.15f, 0, .25f, .3f, .25f, "chrome"), R(0, .25f, 0, 1, .55f, 1, "chrome") }),
            I("station_logbook", "Station Logbook", 20, 80, .5f, CarryClass.Pocket, new(.22f, .05f, .3f), Fragility.Low, SurfaceMaterial.Paper, 2f, L.Watch,
                new[] { B(0, 0, 0, 1, 1, 1, "red"), B(.03f, 0, 0, .95f, .8f, .97f, "paper") }),
            I("radio_set", "Dispatch Radio Set", 300, 800, 9f, CarryClass.TwoHand, new(.45f, .25f, .35f), Fragility.Medium, SurfaceMaterial.Metal, .8f, L.Watch,
                new[] { B(0, 0, 0, 1, 1, 1, "grey"), B(0, .1f, .51f, .8f, .4f, .02f, "black"), T(-.3f, -.25f, .52f, .1f, .05f, .1f, "chrome"), T(.3f, -.25f, .52f, .1f, .05f, .1f, "chrome") }),
            I("speaking_trumpet", "Brass Speaking Trumpet", 400, 1200, 1.5f, CarryClass.OneHand, new(.2f, .2f, .55f), Fragility.Low, SurfaceMaterial.Metal, .8f, L.Watch,
                new[] { new Part(new Vector3(0, 0, -.25f), new Vector3(.3f, .3f, .5f), "brass", S.Box), new Part(new Vector3(0, 0, .25f), new Vector3(1, 1, .5f), "brass", S.Box) }),
            I("chiefs_helmet", "Chief's White Helmet", 300, 900, 1.6f, CarryClass.OneHand, new(.32f, .24f, .42f), Fragility.Low, SurfaceMaterial.Plastic, .8f, L.Watch,
                new[] { R(0, .05f, -.05f, .65f, .9f, .65f, "white"), B(0, -.4f, 0, 1, .1f, 1, "white"), B(0, .1f, .35f, .3f, .4f, .05f, "gold") }),
            I("parade_banner", "Parade Banner", 50, 200, 3f, CarryClass.TwoHand, new(1f, .15f, .15f), Fragility.Low, SurfaceMaterial.Fabric, 1.2f, L.Watch,
                new[] { T(0, 0, 0, 1, .7f, .7f, "red"), B(.52f, 0, 0, .05f, 1, 1, "gold"), B(-.52f, 0, 0, .05f, 1, 1, "gold") }),
        };
    }
}
