using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEngine;
namespace Abandoned.Hospital
{
    public static class HospitalLootItems
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
                Id = "hospital_" + id;
                Name = name;
                Tag = "hospital_" + tag;
                Value = value;
                Weight = weight;
                Carry = carry;
                Size = size;
                Model = model;
                Jackpot = jackpot;
            }
        }
        // Models: instrument, glass case, machine, archive, wheeled apparatus, microscope, brass dial.
        public static readonly Item [] Items =
        {
            new("stethoscope", "Brass Stethoscope", "clinical", 380, .3f, CarryClass.Pocket, new(.2f, .1f, .12f), 0),
            new("otoscope", "Diagnostic Otoscope", "clinical", 500, .6f, CarryClass.Pocket, new(.2f, .12f, .08f), 0),
            new("defibrillator", "Manual Defibrillator", "clinical", 1600, 12, CarryClass.TwoHand, new(.5f, .3f, .4f), 2),
            new("suction", "Suction Pump", "clinical", 2100, 58, CarryClass.Heavy, new(.65f, .9f, .55f), 4),
            new("vials", "Sealed Apothecary Case", "pharmacy", 650, 2, CarryClass.OneHand, new(.32f, .24f, .25f), 1),
            new("balance", "Dispensary Balance", "pharmacy", 1500, 7, CarryClass.TwoHand, new(.5f, .45f, .3f), 6),
            new("mortar", "Porcelain Mortar Set", "pharmacy", 450, 3, CarryClass.OneHand, new(.25f, .2f, .25f), 1),
            new("refrigerator", "Vaccine Refrigerator", "pharmacy", 4200, 140, CarryClass.Heavy, new(.8f, 1.2f, .75f), 2),
            new("bell", "Reception Call Bell", "reception", 280, .4f, CarryClass.Pocket, new(.12f, .1f, .12f), 6),
            new("ledger", "Admissions Ledger", "reception", 500, 2, CarryClass.OneHand, new(.32f, .1f, .25f), 3),
            new("clock", "Nurses Station Clock", "ward", 650, 3, CarryClass.OneHand, new(.35f, .35f, .1f), 6),
            new("infusion", "Infusion Controller", "ward", 1300, 8, CarryClass.TwoHand, new(.3f, .65f, .3f), 2),
            new("resuscitator", "Portable Resuscitator", "ward", 1900, 16, CarryClass.TwoHand, new(.55f, .38f, .4f), 4),
            new("instrument_case", "Surgical Instrument Case", "theatre", 1100, 6, CarryClass.OneHand, new(.42f, .12f, .3f), 0),
            new("microscope", "Operating Microscope", "theatre", 3600, 65, CarryClass.Heavy, new(.65f, .95f, .6f), 5),
            new("slides", "Pathology Slide Cabinet", "records", 1700, 9, CarryClass.TwoHand, new(.4f, .45f, .35f), 3),
            new("archive", "Research Archive Crate", "records", 2400, 24, CarryClass.TwoHand, new(.6f, .45f, .45f), 3),
            new("xray", "Mobile X-Ray Generator", "imaging_prize", 32000, 420, CarryClass.Huge, new(1.25f, 1.9f, 1.05f), 4, true),
            new("anaesthetic", "Anaesthetic Workstation", "theatre_prize", 38000, 350, CarryClass.Huge, new(1.25f, 1.4f, .85f), 2, true),
            new("thermometer", "Mercury Thermometer Case", "clinical", 620, .2f, CarryClass.Pocket, new(.2f, .04f, .05f), 0),
            new("surgical_saw", "Surgical Bone Saw", "theatre", 1700, 4f, CarryClass.OneHand, new(.5f, .18f, .12f), 0),
            new("stretcher", "Folding Rescue Stretcher", "clinical", 3100, 70f, CarryClass.Heavy, new(.75f, .35f, 1.8f), 4),
            new("ward_frame", "Antique Hospital Bed Frame", "ward", 4500, 180f, CarryClass.Huge, new(1.1f, .95f, 2.1f), 4),
            new("mortuary_scale", "Mortuary Weighing Scale", "morgue", 2300, 65f, CarryClass.Heavy, new(.8f, 1.1f, .7f), 6),
            new("specimen_case", "Sealed Specimen Case", "morgue", 950, 3f, CarryClass.OneHand, new(.35f, .2f, .25f), 1),
            new("autopsy_lamp", "Autopsy Examination Lamp", "morgue", 1500, 16f, CarryClass.TwoHand, new(.5f, .9f, .5f), 5),
            new("pathology_log", "Pathologist's Logbook", "morgue", 1100, 2f, CarryClass.OneHand, new(.3f, .08f, .23f), 3),
            new("pressure_gauge", "Boiler Pressure Gauge", "plant", 650, .8f, CarryClass.Pocket, new(.14f, .14f, .07f), 6),
            new("copper_coil", "Copper Heat Exchanger", "plant", 3200, 90f, CarryClass.Heavy, new(.7f, .8f, .7f), 2),
            new("valve_wheel", "Brass Isolation Valve", "plant", 950, 15f, CarryClass.TwoHand, new(.5f, .5f, .2f), 6),
            new("tool_chest", "Engineer's Tool Chest", "plant", 2200, 58f, CarryClass.Heavy, new(.75f, .5f, .45f), 3),
            new("chapel_cross", "Chapel Altar Cross", "chapel", 1800, 5f, CarryClass.OneHand, new(.35f, .6f, .1f), 6),
            new("hymnal", "Leather Hymnal", "chapel", 450, .8f, CarryClass.Pocket, new(.16f, .04f, .12f), 3),
            new("communion_case", "Silver Communion Case", "chapel", 2600, 7f, CarryClass.TwoHand, new(.4f, .2f, .3f), 1),
            new("tea_urn", "Cafeteria Tea Urn", "kitchen", 1600, 18f, CarryClass.TwoHand, new(.4f, .65f, .4f), 2),
            new("mixer", "Catering Stand Mixer", "kitchen", 2800, 75f, CarryClass.Heavy, new(.6f, .75f, .55f), 2),
            new("cutlery", "Silver Service Cutlery", "kitchen", 1300, 5f, CarryClass.OneHand, new(.4f, .12f, .3f), 0),
        };
        public static void Setup(LootDefinition d, Item i)
        {
            d.EditorSetup(i.Id, i.Name, i.Value / 2, i.Value, i.Weight, i.Carry, i.Size, i.Model == 1 || i.Model == 5 ? Fragility.High : Fragility.Medium, i.Model == 1 ? SurfaceMaterial.Glass : i.Model == 3 ? SurfaceMaterial.Paper : SurfaceMaterial.Metal, i.Carry >= CarryClass.Heavy ? .7f : .25f, i.Jackpot ? .15f : 1f, PlaceholderShape.Cube, new Color(.6f, .7f, .64f));
            d.EditorSetSpawning(new []
            {
                i.Tag
            }, i.Jackpot);
            if (i.Model == 4)
            {
                d.EditorSetQuirks(LootQuirk.Rattles);
            }
        }
    }
}
