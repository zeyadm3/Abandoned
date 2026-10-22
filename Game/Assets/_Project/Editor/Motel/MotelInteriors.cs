using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.EditorTools;
using Abandoned.Interaction;
using UnityEngine;

namespace Abandoned.Motel
{
    public static class MotelInteriors
    {
        public static List<LootPointSpec> Build(Transform root,MotelKit k,MotelStructure floors)
        {
            Transform rooms=MotelKit.Group("Interiors",root);var loot=new List<LootPointSpec>();
            for(int level=0;level<2;level++)for(int room=0;room<8;room++)
            {
                float x=MotelLayout.RoomX(room),y=level*4;
                Transform r=MotelKit.Group("Room_"+MotelLayout.RoomNumber(level,room),rooms);
                Bed(r,k,floors,new Vector3(x+1.5f,y,20),room%3);
                Desk(r,k,floors,new Vector3(x+5,y,23));
                Bath(r,k,floors,new Vector3(x,y,24));
                MotelHiding.Build(r,k,new Vector3(x+.4f,y,17.6f),90,96+level*8+room,floors.At(new Vector3(x+.4f,y,17.6f)));
                loot.Add(new LootPointSpec($"Motel_{level}_{room}_Bedside","motel_guest",new Vector3(x+4.8f,y+.05f,19),CarryClass.Pocket,CarryClass.TwoHand));
                loot.Add(new LootPointSpec($"Motel_{level}_{room}_Luggage","motel_guest",new Vector3(x+4.8f,y+.05f,21.3f),CarryClass.OneHand,CarryClass.Heavy));
                loot.Add(new LootPointSpec($"Motel_{level}_{room}_Bathroom","motel_bath",new Vector3(x+3.5f,y+.05f,26.5f),CarryClass.Pocket,CarryClass.OneHand));
                if(room==7)loot.Add(new LootPointSpec("Motel_SuitePrize_"+level,"motel_suite_prize",new Vector3(x+4,y+.05f,20),CarryClass.Heavy,CarryClass.Huge,true));
                Transform sound=MotelKit.Group("BathroomDrip",r);sound.localPosition=new Vector3(x+3.5f,y+1,26);
                sound.gameObject.AddComponent<HorrorAreaAmbience>().EditorSetup(new Vector3(6,3,4),2);
            }
            Services(rooms,k,floors,loot);return loot;
        }
        private static Transform Prop(Transform p,MotelKit k,MotelStructure floors,string name,Vector3 at)
        {
            Transform t=MotelKit.Group(name,p);t.localPosition=at;k.Attach(t,floors.At(at));return t;
        }
        private static void Bed(Transform p,MotelKit k,MotelStructure floors,Vector3 at,int variant)
        {
            Transform t=Prop(p,k,floors,"BoltedGuestBed",at);
            k.Box("Base",t,new Vector3(0,.3f,0),new Vector3(1.7f,.6f,2.2f),k.Wood);
            k.Box("Mattress",t,new Vector3(0,.7f,0),new Vector3(1.65f,.2f,2.1f),k.Cloth,false);
            k.Box("Headboard",t,new Vector3(0,1.05f,1.05f),new Vector3(1.9f,1.2f,.12f),k.Wood,false);
            foreach(float x in new[]{-.4f,.4f})k.Box("Pillow",t,new Vector3(x,.85f,.65f),new Vector3(.65f,.12f,.4f),k.Cloth,false);
            if(variant!=1)k.Box("Coverlet",t,new Vector3(0,.82f,-.4f),new Vector3(1.66f,.04f,1.1f),variant==0?k.Accent:k.Dark,false);
            k.Box("BedsideShelf",t,new Vector3(1.15f,.65f,.7f),new Vector3(.5f,.12f,.5f),k.Wood);
        }
        private static void Desk(Transform p,MotelKit k,MotelStructure floors,Vector3 at)
        {
            Transform t=Prop(p,k,floors,"FittedVanity",at);
            k.Box("Desk",t,new Vector3(0,.65f,0),new Vector3(1.8f,1.3f,.7f),k.Wood);
            k.Box("MirrorFrame",t,new Vector3(0,1.9f,.37f),new Vector3(1.5f,1,.08f),k.Metal,false);
            k.Box("OpaqueMirror",t,new Vector3(0,1.9f,.315f),new Vector3(1.35f,.86f,.025f),k.Glass,false);
            // These fixed wall units have no silhouette in common with the portable loot television.
            k.Box("AirConditioner",t,new Vector3(0,2.95f,.35f),new Vector3(1.4f,.35f,.35f),k.Accent,false);
            for(int i=0;i<7;i++)k.Box("VentSlat",t,new Vector3(-.55f+i*.18f,2.94f,.16f),new Vector3(.06f,.2f,.04f),k.Metal,false);
        }
        private static void Bath(Transform p,MotelKit k,MotelStructure floors,Vector3 at)
        {
            Transform t=Prop(p,k,floors,"BathroomFittings",at);
            k.Box("TubPlinth",t,new Vector3(1.3f,.25f,2),new Vector3(1.8f,.5f,2.2f),k.Wall);
            k.Box("TubInset",t,new Vector3(1.3f,.51f,2),new Vector3(1.5f,.03f,1.85f),k.Glass,false);
            k.Box("SinkPedestal",t,new Vector3(5.9f,.5f,2),new Vector3(.65f,1,.6f),k.Wall);
            k.Box("Basin",t,new Vector3(5.9f,1.05f,2),new Vector3(.85f,.12f,.7f),k.Metal,false);
            k.Box("ShowerPipe",t,new Vector3(.35f,1.6f,2.6f),new Vector3(.05f,1.9f,.05f),k.Metal,false);
            k.Box("ShowerHead",t,new Vector3(.48f,2.5f,2.6f),new Vector3(.3f,.08f,.2f),k.Metal,false);
        }
        private static void Services(Transform p,MotelKit k,MotelStructure floors,List<LootPointSpec> loot)
        {
            Transform desk=Prop(p,k,floors,"ReceptionCounter",new Vector3(29.5f,0,19.5f));
            k.Box("Base",desk,new Vector3(0,.55f,0),new Vector3(1.2f,1.1f,3),k.Accent);
            k.Box("Top",desk,new Vector3(0,1.13f,0),new Vector3(1.35f,.06f,3.1f),k.Wood,false);
            Transform keys=Prop(p,k,floors,"EmptyKeyBoard",new Vector3(28.2f,0,20));
            k.Box("Board",keys,new Vector3(0,1.9f,0),new Vector3(.12f,1.2f,2),k.Wood,false);
            for(int i=0;i<8;i++)k.Box("EmptyHook",keys,new Vector3(.1f,1.5f+(i/4)*.4f,-.75f+(i%4)*.5f),new Vector3(.1f,.04f,.04f),k.Metal,false);
            for(int i=0;i<4;i++)loot.Add(new LootPointSpec("Motel_Office_"+i,"motel_office",new Vector3(34,.05f,17.5f+i),CarryClass.Pocket,CarryClass.Heavy));
            Transform laundry=Prop(p,k,floors,"FixedLaundryBank",new Vector3(29.3f,0,25.5f));
            k.Box("Plinth",laundry,new Vector3(0,.5f,0),new Vector3(1.2f,1,3.6f),k.Wall);
            for(int i=0;i<3;i++)k.Box("WasherDoor",laundry,new Vector3(.62f,.6f,-1.2f+i*1.2f),new Vector3(.04f,.65f,.65f),k.Glass,false);
            for(int i=0;i<4;i++)loot.Add(new LootPointSpec("Motel_Laundry_"+i,"motel_laundry",new Vector3(34,.05f,23.1f+i),CarryClass.OneHand,CarryClass.Heavy));
            loot.Add(new LootPointSpec("Motel_LaundryPrize","motel_laundry_prize",new Vector3(32,.05f,26),CarryClass.Heavy,CarryClass.Huge,true));
            Transform sound=MotelKit.Group("LaundryDuct",p);sound.localPosition=new Vector3(32,2,25);
            sound.gameObject.AddComponent<HorrorAreaAmbience>().EditorSetup(new Vector3(8,3,6),1);
        }
    }
}
