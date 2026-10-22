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
            public readonly string Id,Name,Tag;
            public readonly int Value,Model;
            public readonly float Weight;
            public readonly CarryClass Carry;
            public readonly Vector3 Size;
            public readonly bool Jackpot;
            public Item(string id,string name,string tag,int value,float weight,CarryClass carry,Vector3 size,int model,bool jackpot=false)
            {Id="hospital_"+id;Name=name;Tag="hospital_"+tag;Value=value;Weight=weight;Carry=carry;Size=size;Model=model;Jackpot=jackpot;}
        }
        // Models: instrument, glass case, machine, archive, wheeled apparatus, microscope, brass dial.
        public static readonly Item[] Items={
            new("stethoscope","Brass Stethoscope","clinical",380,.3f,CarryClass.Pocket,new(.2f,.1f,.12f),0),
            new("otoscope","Diagnostic Otoscope","clinical",500,.6f,CarryClass.Pocket,new(.2f,.12f,.08f),0),
            new("defibrillator","Manual Defibrillator","clinical",1600,12,CarryClass.TwoHand,new(.5f,.3f,.4f),2),
            new("suction","Suction Pump","clinical",2100,58,CarryClass.Heavy,new(.65f,.9f,.55f),4),
            new("vials","Sealed Apothecary Case","pharmacy",650,2,CarryClass.OneHand,new(.32f,.24f,.25f),1),
            new("balance","Dispensary Balance","pharmacy",1500,7,CarryClass.TwoHand,new(.5f,.45f,.3f),6),
            new("mortar","Porcelain Mortar Set","pharmacy",450,3,CarryClass.OneHand,new(.25f,.2f,.25f),1),
            new("refrigerator","Vaccine Refrigerator","pharmacy",4200,140,CarryClass.Heavy,new(.8f,1.2f,.75f),2),
            new("bell","Reception Call Bell","reception",280,.4f,CarryClass.Pocket,new(.12f,.1f,.12f),6),
            new("ledger","Admissions Ledger","reception",500,2,CarryClass.OneHand,new(.32f,.1f,.25f),3),
            new("clock","Nurses Station Clock","ward",650,3,CarryClass.OneHand,new(.35f,.35f,.1f),6),
            new("infusion","Infusion Controller","ward",1300,8,CarryClass.TwoHand,new(.3f,.65f,.3f),2),
            new("resuscitator","Portable Resuscitator","ward",1900,16,CarryClass.TwoHand,new(.55f,.38f,.4f),4),
            new("instrument_case","Surgical Instrument Case","theatre",1100,6,CarryClass.OneHand,new(.42f,.12f,.3f),0),
            new("microscope","Operating Microscope","theatre",3600,65,CarryClass.Heavy,new(.65f,.95f,.6f),5),
            new("slides","Pathology Slide Cabinet","records",1700,9,CarryClass.TwoHand,new(.4f,.45f,.35f),3),
            new("archive","Research Archive Crate","records",2400,24,CarryClass.TwoHand,new(.6f,.45f,.45f),3),
            new("xray","Mobile X-Ray Generator","imaging_prize",32000,420,CarryClass.Huge,new(1.25f,1.9f,1.05f),4,true),
            new("anaesthetic","Anaesthetic Workstation","theatre_prize",38000,350,CarryClass.Huge,new(1.25f,1.4f,.85f),2,true)
        };
        public static void Setup(LootDefinition d,Item i)
        {
            d.EditorSetup(i.Id,i.Name,i.Value/2,i.Value,i.Weight,i.Carry,i.Size,
                i.Model==1||i.Model==5?Fragility.High:Fragility.Medium,
                i.Model==1?SurfaceMaterial.Glass:i.Model==3?SurfaceMaterial.Paper:SurfaceMaterial.Metal,i.Carry>=CarryClass.Heavy? .7f:.25f,
                i.Jackpot? .15f:1f,PlaceholderShape.Cube,new Color(.6f,.7f,.64f));
            d.EditorSetSpawning(new[]{i.Tag},i.Jackpot);
            if(i.Model==4)d.EditorSetQuirks(LootQuirk.Rattles);
        }
    }
}
