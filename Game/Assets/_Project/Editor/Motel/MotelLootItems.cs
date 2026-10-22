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
            public readonly string Id,Name,Tag;
            public readonly int Value,Model;
            public readonly float Weight;
            public readonly CarryClass Carry;
            public readonly Vector3 Size;
            public readonly bool Jackpot;
            public Item(string id,string name,string tag,int value,float weight,CarryClass carry,Vector3 size,int model,bool jackpot=false)
            {Id="motel_"+id;Name=name;Tag="motel_"+tag;Value=value;Weight=weight;Carry=carry;Size=size;Model=model;Jackpot=jackpot;}
        }
        public static readonly Item[] Items={
            new("key","Numbered Room Key","guest",180,.1f,CarryClass.Pocket,new(.1f,.05f,.06f),6),
            new("clock","Travel Alarm Clock","guest",420,.7f,CarryClass.Pocket,new(.12f,.14f,.08f),6),
            new("suitcase","Leather Suitcase","guest",950,12,CarryClass.TwoHand,new(.65f,.45f,.22f),0),
            new("television","Portable Tube Television","guest",1800,25,CarryClass.TwoHand,new(.6f,.45f,.4f),1),
            new("radio","Bedside Valve Radio","guest",1200,6,CarryClass.OneHand,new(.35f,.25f,.2f),2),
            new("camera","Holiday Film Camera","guest",850,.8f,CarryClass.Pocket,new(.14f,.1f,.08f),1),
            new("minibar","Freestanding Minibar","guest",2600,85,CarryClass.Heavy,new(.65f,.85f,.6f),4),
            new("coat","Abandoned Winter Coat","guest",450,2,CarryClass.OneHand,new(.4f,.25f,.3f),7),
            new("perfume","Crystal Perfume Bottle","bath",750,.3f,CarryClass.Pocket,new(.08f,.13f,.08f),5),
            new("brush","Silver Vanity Brush","bath",620,.25f,CarryClass.Pocket,new(.16f,.04f,.07f),6),
            new("mirror","Hand Vanity Mirror","bath",480,.5f,CarryClass.OneHand,new(.24f,.35f,.04f),5),
            new("toiletry_case","Travellers Toiletry Case","bath",550,1.4f,CarryClass.OneHand,new(.28f,.18f,.18f),0),
            new("register","Mechanical Cash Register","office",2200,22,CarryClass.TwoHand,new(.5f,.4f,.4f),2),
            new("key_cabinet","Brass Key Cabinet","office",3000,65,CarryClass.Heavy,new(.8f,.9f,.4f),3),
            new("guestbook","Bound Guest Register","office",700,3,CarryClass.OneHand,new(.32f,.1f,.24f),3),
            new("desk_bell","Office Counter Bell","office",350,.4f,CarryClass.Pocket,new(.12f,.1f,.12f),6),
            new("linen","Linen Bale","laundry",800,18,CarryClass.TwoHand,new(.65f,.5f,.5f),7),
            new("iron","Cast Soleplate Iron","laundry",450,3,CarryClass.OneHand,new(.15f,.17f,.3f),2),
            new("sewing_machine","Laundry Sewing Machine","laundry",1700,18,CarryClass.TwoHand,new(.5f,.35f,.25f),2),
            new("heater","Copper Water Heater","laundry",3800,130,CarryClass.Heavy,new(.7f,1.3f,.7f),4),
            new("stereo","Honeymoon Suite Stereo","suite_prize",28000,280,CarryClass.Huge,new(1.15f,1.1f,.75f),8,true),
            new("coin_washer","Commercial Coin Washer","laundry_prize",34000,480,CarryClass.Huge,new(1.15f,1.45f,.85f),4,true)
        };
        public static void Setup(LootDefinition d,Item i)
        {
            d.EditorSetup(i.Id,i.Name,i.Value/2,i.Value,i.Weight,i.Carry,i.Size,
                i.Model==1||i.Model==5?Fragility.High:Fragility.Low,
                i.Model==5?SurfaceMaterial.Glass:i.Model==3?SurfaceMaterial.Paper:i.Model==7?SurfaceMaterial.Fabric:SurfaceMaterial.Metal,
                i.Carry>=CarryClass.Heavy? .65f:.2f,i.Jackpot? .15f:1,PlaceholderShape.Cube,new Color(.6f,.38f,.22f));
            d.EditorSetSpawning(new[]{i.Tag},i.Jackpot);
            if(i.Id=="motel_suitcase")d.EditorSetQuirks(LootQuirk.Rattles);
        }
    }
}
