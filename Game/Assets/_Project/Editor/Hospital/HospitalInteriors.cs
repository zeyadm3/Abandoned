using System.Collections.Generic;
using Abandoned.EditorTools;
using Abandoned.Interaction;
using UnityEngine;

namespace Abandoned.Hospital
{
    public static class HospitalInteriors
    {
        public static List<LootPointSpec> Build(Transform root,HospitalKit k,HospitalStructure floors)
        {
            var loot=new List<LootPointSpec>();Transform interiors=HospitalKit.Group("Interiors",root);
            for(int level=0;level<2;level++)for(int side=0;side<2;side++)for(int bay=0;bay<2;bay++)
            {
                float y=level*4,x=side==0?8:36,z=bay==0?7:31;
                string tag=HospitalLayout.Tag(level,side,bay);
                Transform room=HospitalKit.Group(HospitalLayout.Rooms[level,side*2+bay],interiors);
                // Salvage points sit in reserved empty floor bays: no fixed prop impersonates loose loot.
                for(int i=0;i<6;i++)
                    loot.Add(new LootPointSpec($"Hospital_{level}_{side}_{bay}_{i:00}",tag,new Vector3(x+5+(i%2)*3,y+.05f,z-4+(i/2)*4),CarryClass.Pocket,CarryClass.Heavy));
                if(side==1)loot.Add(new LootPointSpec($"Hospital_Jackpot_{level}_{bay}",level==0?"hospital_imaging_prize":"hospital_theatre_prize",
                    new Vector3(44,y+.05f,z+4),CarryClass.Heavy,CarryClass.Huge,true));
                if(level==0&&side==0&&bay==0)
                {
                    Counter(room,k,floors,new Vector3(6,y,4),7);
                    for(int i=0;i<4;i++)Bench(room,k,floors,new Vector3(3+i*3,y,12));
                    Shelves(room,k,floors,new Vector3(2,y,2),4);
                }
                else if(level==0&&side==0)
                {
                    for(int i=0;i<4;i++)Shelves(room,k,floors,new Vector3(2+i*3,y,37),2.2f);
                    Counter(room,k,floors,new Vector3(8,y,27),6);
                }
                else if(level==1&&side==1&&bay==0)
                {
                    Bed(room,k,floors,new Vector3(32,y,7),true);
                    k.Box("TheatreBoom",room,new Vector3(32,y+3.1f,7),new Vector3(3,.12f,.12f),k.Metal,false);
                    Counter(room,k,floors,new Vector3(40,y,2),6);
                }
                else if(level==1&&side==1)
                {
                    Counter(room,k,floors,new Vector3(32,y,27),4);
                    for(int i=0;i<4;i++)Shelves(room,k,floors,new Vector3(30+i*3,y,38),2.2f);
                }
                else
                {
                    for(int i=0;i<3;i++)Bed(room,k,floors,new Vector3(side==0?3:31,y,z-4+i*4),false);
                    Counter(room,k,floors,new Vector3(side==0?8:36,y,z-5),4);
                }
                // Fixed pipework, wall sockets and ceiling vents carry the room's identity without aisle clutter.
                for(int i=0;i<3;i++)
                {
                    Vector3 at=new(side==0?.3f:47.7f,y+1.1f,z-3+i*3);
                    k.Box("MedicalGasPanel",room,at,new Vector3(.1f,.35f,.7f),k.Accent,false);
                }
                Transform audio=HospitalKit.Group("RoomSound",room);audio.localPosition=new Vector3(x,y+1.5f,z);
                audio.gameObject.AddComponent<Abandoned.Audio.HorrorAreaAmbience>().EditorSetup(new Vector3(18,3,14),bay==1&&side==1?2:1);
            }
            return loot;
        }
        private static Transform Prop(Transform parent,HospitalKit k,HospitalStructure floors,string name,Vector3 at)
        {
            Transform t=HospitalKit.Group(name,parent);t.localPosition=at;k.Attach(t,floors.At(at));return t;
        }
        private static void Bed(Transform p,HospitalKit k,HospitalStructure floors,Vector3 at,bool theatre)
        {
            Transform t=Prop(p,k,floors,theatre?"FixedOperatingTable":"BoltedWardBed",at);
            k.Box("Frame",t,new Vector3(0,.55f,0),new Vector3(1.05f,.18f,2.2f),k.Metal);
            k.Box("Pad",t,new Vector3(0,.75f,0),new Vector3(.96f,.22f,2),theatre?k.Dark:k.Cloth,false);
            foreach(float x in new[]{-.42f,.42f})foreach(float z in new[]{-.85f,.85f})
            {
                k.Box("Leg",t,new Vector3(x,.26f,z),new Vector3(.07f,.52f,.07f),k.Metal,false);
                k.Box("Caster",t,new Vector3(x,.08f,z),new Vector3(.14f,.16f,.14f),k.Dark,false);
            }
            foreach(float z in new[]{-1f,1f}) k.Box("EndBoard",t,new Vector3(0,1,z),new Vector3(1,.5f,.06f),k.Accent,false);
            if(!theatre) k.Box("Pillow",t,new Vector3(0,.91f,.65f),new Vector3(.7f,.12f,.4f),k.Cloth,false);
        }
        private static void Counter(Transform p,HospitalKit k,HospitalStructure floors,Vector3 at,float width)
        {
            Transform t=Prop(p,k,floors,"FittedWorkstation",at);
            k.Box("Cabinet",t,new Vector3(0,.5f,0),new Vector3(width,1,.75f),k.Accent);
            k.Box("Worktop",t,new Vector3(0,1.04f,0),new Vector3(width+.08f,.08f,.85f),k.Metal,false);
            for(float x=-width*.5f+.4f;x<width*.5f;x+=.8f)
                k.Box("DrawerPull",t,new Vector3(x,.7f,-.4f),new Vector3(.3f,.03f,.04f),k.Metal,false);
        }
        private static void Shelves(Transform p,HospitalKit k,HospitalStructure floors,Vector3 at,float width)
        {
            Transform t=Prop(p,k,floors,"BuiltInStorage",at);
            k.Box("Back",t,new Vector3(0,1.3f,0),new Vector3(width,2.6f,.4f),k.Accent);
            for(int i=0;i<5;i++)k.Box("ShelfEdge",t,new Vector3(0,.2f+i*.5f,-.23f),new Vector3(width,.06f,.12f),k.Metal,false);
        }
        private static void Bench(Transform p,HospitalKit k,HospitalStructure floors,Vector3 at)
        {
            Transform t=Prop(p,k,floors,"WaitingBench",at);
            k.Box("Base",t,new Vector3(0,.3f,0),new Vector3(2,.6f,.65f),k.Dark);
            k.Box("Seat",t,new Vector3(0,.6f,0),new Vector3(2,.12f,.7f),k.Cloth,false);
            k.Box("Back",t,new Vector3(0,.95f,.3f),new Vector3(2,.65f,.12f),k.Cloth,false);
        }
    }
}
