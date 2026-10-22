using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelLootItems
    {
        public readonly struct Item
        {
            public readonly string Id,
            Name,
            Tag;
            public readonly int Value,
            Model;
            public readonly float Weight;
            public readonly CarryClass Carry;
            public readonly Vector3 Size;
            public readonly bool Jackpot;
            public Item(string id, string name, string tag, int value, float weight, CarryClass carry, Vector3 size, int model, bool jackpot = false)
            {
                Id = "motel_" + id;
                Name = name;
                Tag = "motel_" + tag;
                Value = value;
                Weight = weight;
                Carry = carry;
                Size = size;
                Model = model;
                Jackpot = jackpot;
            }
        }
        public static readonly Item [] Items =
        {
            new("key", "Numbered Room Key", "guest", 180, .1f, CarryClass.Pocket, new(.1f, .05f, .06f), 6),
            new("clock", "Travel Alarm Clock", "guest", 420, .7f, CarryClass.Pocket, new(.12f, .14f, .08f), 6),
            new("suitcase", "Leather Suitcase", "guest", 950, 12, CarryClass.TwoHand, new(.65f, .45f, .22f), 0),
            new("television", "Portable Tube Television", "guest", 1800, 25, CarryClass.TwoHand, new(.6f, .45f, .4f), 1),
            new("radio", "Bedside Valve Radio", "guest", 1200, 6, CarryClass.OneHand, new(.35f, .25f, .2f), 2),
            new("camera", "Holiday Film Camera", "guest", 850, .8f, CarryClass.Pocket, new(.14f, .1f, .08f), 1),
            new("minibar", "Freestanding Minibar", "guest", 2600, 85, CarryClass.Heavy, new(.65f, .85f, .6f), 4),
            new("coat", "Abandoned Winter Coat", "guest", 450, 2, CarryClass.OneHand, new(.4f, .25f, .3f), 7),
            new("perfume", "Crystal Perfume Bottle", "bath", 750, .3f, CarryClass.Pocket, new(.08f, .13f, .08f), 5),
            new("brush", "Silver Vanity Brush", "bath", 620, .25f, CarryClass.Pocket, new(.16f, .04f, .07f), 6),
            new("mirror", "Hand Vanity Mirror", "bath", 480, .5f, CarryClass.OneHand, new(.24f, .35f, .04f), 5),
            new("toiletry_case", "Travellers Toiletry Case", "bath", 550, 1.4f, CarryClass.OneHand, new(.28f, .18f, .18f), 0),
            new("register", "Mechanical Cash Register", "office", 2200, 22, CarryClass.TwoHand, new(.5f, .4f, .4f), 2),
            new("key_cabinet", "Brass Key Cabinet", "office", 3000, 65, CarryClass.Heavy, new(.8f, .9f, .4f), 3),
            new("guestbook", "Bound Guest Register", "office", 700, 3, CarryClass.OneHand, new(.32f, .1f, .24f), 3),
            new("desk_bell", "Office Counter Bell", "office", 350, .4f, CarryClass.Pocket, new(.12f, .1f, .12f), 6),
            new("linen", "Linen Bale", "laundry", 800, 18, CarryClass.TwoHand, new(.65f, .5f, .5f), 7),
            new("iron", "Cast Soleplate Iron", "laundry", 450, 3, CarryClass.OneHand, new(.15f, .17f, .3f), 2),
            new("sewing_machine", "Laundry Sewing Machine", "laundry", 1700, 18, CarryClass.TwoHand, new(.5f, .35f, .25f), 2),
            new("heater", "Copper Water Heater", "laundry", 3800, 130, CarryClass.Heavy, new(.7f, 1.3f, .7f), 4),
            new("stereo", "Honeymoon Suite Stereo", "suite_prize", 28000, 280, CarryClass.Huge, new(1.15f, 1.1f, .75f), 8, true),
            new("coin_washer", "Commercial Coin Washer", "laundry_prize", 34000, 480, CarryClass.Huge, new(1.15f, 1.45f, .85f), 4, true),
            new("percolator", "Diner Coffee Percolator", "diner", 1900, 16, CarryClass.TwoHand, new(.4f, .7f, .4f), 9),
            new("griddle", "Cast Iron Diner Griddle", "diner", 5200, 145, CarryClass.Heavy, new(1.2f, .6f, .8f), 10),
            new("freezer", "Diner Chest Freezer", "diner", 7200, 280, CarryClass.Huge, new(1.2f, 1.2f, .85f), 11),
            new("cutlery", "Diner Silver Cutlery Roll", "diner", 1100, 2, CarryClass.OneHand, new(.3f, .1f, .25f), 3),
            new("milkshake", "Milkshake Mixer", "diner", 1200, 5, CarryClass.OneHand, new(.22f, .55f, .25f), 12),
            new("syrup_case", "Glass Syrup Crate", "diner", 1600, 19, CarryClass.TwoHand, new(.6f, .35f, .4f), 5),
            new("toolchest", "Mechanic's Rolling Tool Chest", "maintenance", 4300, 115, CarryClass.Heavy, new(.85f, 1f, .6f), 3),
            new("compressor", "Workshop Air Compressor", "maintenance", 3900, 95, CarryClass.Heavy, new(.9f, .8f, .6f), 13),
            new("transformer", "Roadside Neon Transformer", "maintenance", 6800, 260, CarryClass.Huge, new(1.3f, 1.1f, .7f), 14),
            new("pressure_gauge", "Workshop Pressure Gauge", "maintenance", 650, .5f, CarryClass.Pocket, new(.12f, .12f, .06f), 6),
            new("pool_pump", "Drained Pool Circulation Pump", "pool", 3200, 90, CarryClass.Heavy, new(.8f, .75f, .6f), 13),
            new("whistle", "Lifeguard's Silver Whistle", "pool", 500, .1f, CarryClass.Pocket, new(.07f, .04f, .03f), 6),
            new("desk_phone", "Manager's Rotary Telephone", "office", 950, 3, CarryClass.OneHand, new(.3f, .2f, .25f), 2),
            new("payroll", "Manager's Payroll Ledger", "office", 1300, 2, CarryClass.OneHand, new(.3f, .08f, .23f), 3),
            new("postcards", "Unsent Holiday Postcards", "guest", 320, .1f, CarryClass.Pocket, new(.14f, .02f, .09f), 3),
            new("locket", "Guest's Silver Locket", "guest", 900, .1f, CarryClass.Pocket, new(.05f, .05f, .02f), 5)
        };
        public static void Setup(LootDefinition d, Item i)
        {
            d.EditorSetup(i.Id, i.Name, i.Value / 2, i.Value, i.Weight, i.Carry, i.Size, i.Model == 1 || i.Model == 5 ? Fragility.High : Fragility.Low, i.Model == 5 ? SurfaceMaterial.Glass : i.Model == 3 ? SurfaceMaterial.Paper : i.Model == 7 ? SurfaceMaterial.Fabric : SurfaceMaterial.Metal, i.Carry >= CarryClass.Heavy ? .65f : .2f, i.Jackpot ? .15f : 1, PlaceholderShape.Cube, new Color(.6f, .38f, .22f));
            d.EditorSetSpawning(new []
            {
                i.Tag
            }, i.Jackpot);
            if (i.Id == "motel_suitcase" || i.Id == "motel_syrup_case")
            {
                d.EditorSetQuirks(LootQuirk.Rattles);
            }
            if (i.Id == "motel_desk_phone")
            {
                d.EditorSetQuirks(LootQuirk.Rings);
            }
        }
    }
}
