using Abandoned.EditorTools;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.Hospital
{
    public static class HospitalShell
    {
        public static void Build(Transform root,HospitalKit k,HospitalStructure floors)
        {
            Transform shell=HospitalKit.Group("Shell",root);
            for(int level=0;level<2;level++)
            {
                float y=level*4;
                foreach(float z in new[]{0f,40f})
                {
                    // Wide front/rear ground exits; upper perimeter has a wall across its centre.
                    foreach(float x in new[]{10f,38f}) Facade(shell,k,new Vector3(x,y,z),20,true);
                    if(level==0) Lintel(shell,k,new Vector3(24,y,z),8);
                    else Facade(shell,k,new Vector3(24,y,z),8,false);
                }
                foreach(float x in new[]{0f,48f})
                {
                    Transform side=HospitalKit.Group("SideFacade",shell);side.localPosition=new Vector3(x,y,0);side.localRotation=Quaternion.Euler(0,90,0);
                    Facade(side,k,new Vector3(-8,0,0),16,true);Facade(side,k,new Vector3(-32,0,0),16,true);
                    Lintel(side,k,new Vector3(-20,0,0),8);
                }
                for(int side=0;side<2;side++) for(int bay=0;bay<2;bay++)
                {
                    float edge=side==0?20:28,z=bay==0?8:32;
                    Transform wall=HospitalKit.Group("ClinicalPartition",shell);wall.localPosition=new Vector3(edge,y,z);wall.localRotation=Quaternion.Euler(0,90,0);
                    DoorWall(wall,k,16,2.4f);
                    // Room identification is mounted on the corridor side, clear of the opening.
                    Transform sign=HospitalKit.Group("RoomSign",shell);sign.localPosition=new Vector3(edge+(side==0?.14f:-.14f),y+2.8f,z+2.6f);
                    sign.localRotation=Quaternion.Euler(0,side==0?-90:90,0);
                    k.Box("Backplate",sign,Vector3.zero,new Vector3(2.2f,.42f,.06f),k.Accent,false);
                    k.Label(sign,HospitalLayout.Rooms[level,side*2+bay],new Vector3(0,0,-.04f),size:.035f);
                }
                foreach(float z in new[]{16f,24f}) foreach(float x in new[]{10f,38f})
                    k.Box("CrossHallWall",shell,new Vector3(x,y+1.82f,z),new Vector3(20,3.64f,.18f),k.Wall);
                // Skirting and continuous wall protection replace loose objects in the haul spine.
                foreach(float x in new[]{19.88f,28.12f}) foreach(float z in new[]{3f,13f,27f,37f})
                    k.Box("BumperRail",shell,new Vector3(x,y+.85f,z),new Vector3(.08f,.16f,4.8f),k.Accent,false);
                // Ground-floor ceiling follows the floor slab above it; roof sits above upper clear headroom.
                for(int z=0;z<10;z++) for(int x=0;x<12;x++)
                {
                    Transform panel=HospitalKit.Group("CeilingPanel",shell);
                    panel.localPosition=new Vector3(x*4+2,y+3.62f,z*4+2);
                    k.Box("Panel",panel,Vector3.zero,new Vector3(3.96f,.08f,3.96f),k.Wall,false);
                    if(level==0)k.Attach(panel,floors.At(new Vector3(x*4+2,4,z*4+2)));
                }
            }
            Transform roof=HospitalKit.Group("Roof",root);
            k.Box("RoofDeck",roof,new Vector3(24,7.91f,20),new Vector3(48,.18f,40),k.Dark);
            for(float z=6;z<40;z+=12)
            {
                k.Box("AirHandling",roof,new Vector3(10,8.6f,z),new Vector3(3,1.2f,2),k.Metal);
                k.Box("Duct",roof,new Vector3(18,8.3f,z),new Vector3(13,.45f,.7f),k.Metal,false);
            }
            Transform entry=HospitalKit.Group("EntranceCanopy",root);
            k.Box("Canopy",entry,new Vector3(24,3.3f,-3),new Vector3(12,.25f,6),k.Accent);
            foreach(float x in new[]{18.5f,29.5f})k.Box("Post",entry,new Vector3(x,1.6f,-5.5f),new Vector3(.25f,3.2f,.25f),k.Metal);
            k.Box("HospitalSign",entry,new Vector3(24,2.8f,-5.96f),new Vector3(9,.6f,.08f),k.Accent,false);
            k.Label(entry,"SAINT BRIGID HOSPITAL",new Vector3(24,2.8f,-6.01f));
        }
        private static void DoorWall(Transform g,HospitalKit k,float width,float opening)
        {
            float side=(width-opening)*.5f;
            foreach(float s in new[]{-1f,1f})k.Box("JambWall",g,new Vector3(s*(opening+side)*.5f,1.82f,0),new Vector3(side,3.64f,.18f),k.Wall);
            Lintel(g,k,Vector3.zero,opening);
        }
        private static void Lintel(Transform p,HospitalKit k,Vector3 at,float width) => k.Box("Lintel",p,at+new Vector3(0,3.23f,0),new Vector3(width,.82f,.2f),k.Wall);
        private static void Facade(Transform p,HospitalKit k,Vector3 at,float width,bool windows)
        {
            k.Box("SillWall",p,at+new Vector3(0,.6f,0),new Vector3(width,1.2f,.24f),k.Wall);
            k.Box("Header",p,at+new Vector3(0,3.22f,0),new Vector3(width,.84f,.24f),k.Wall);
            k.Box("WindowBand",p,at+new Vector3(0,2,0),new Vector3(width,1.6f,.08f),windows?k.Glass:k.Wall);
            for(float x=-width*.5f+.12f;x<width*.5f;x+=4)
                k.Box("Mullion",p,at+new Vector3(x,2,0),new Vector3(.12f,1.6f,.27f),k.Metal,false);
        }
    }
}
