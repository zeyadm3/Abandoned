using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
namespace Abandoned.EditorTools
{
    public static class SkyscraperFurnishings
    {
        public static void Bench(Transform p,SkyscraperArt a,Vector3 at,float angle,bool single=false)
        {
            Transform g=Group("LeatherBanquette",p);g.position=at;g.rotation=Quaternion.Euler(0,angle,0);
            if(single)g.localScale=new Vector3(.32f,1,.8f);
            Box("Plinth",g,new Vector3(0,.2f,0),new Vector3(2.4f,.4f,.85f),a.Mahogany);
            Box("Seat",g,new Vector3(0,.48f,0),new Vector3(2.4f,.16f,.85f),a.Leather);
            Box("UpholsteredBack",g,new Vector3(0,.9f,.38f),new Vector3(2.4f,.8f,.16f),a.Leather);
            for(int i=0;i<6;i++) Box("UpholsterySeam",g,new Vector3(-1+i*.4f,.9f,.285f),new Vector3(.015f,.65f,.015f),a.Brass,false);
        }
        public static void Table(Transform p,SkyscraperArt a,Vector3 at,bool dressed=true)
        {
            Transform g=Group("DiningTable",p);g.position=at;
            Primitive(PrimitiveType.Cylinder,"Foot",g,new Vector3(0,.05f,0),new Vector3(.8f,.05f,.8f),a.Brass);
            Primitive(PrimitiveType.Cylinder,"Pedestal",g,new Vector3(0,.4f,0),new Vector3(.12f,.35f,.12f),a.Brass);
            Primitive(PrimitiveType.Cylinder,"Tabletop",g,new Vector3(0,.8f,0),new Vector3(1.9f,.045f,1.9f),a.Marble);
            if(dressed)
                for(int i=0;i<4;i++)
                {
                    float angle=i*Mathf.PI*.5f;
                    Vector3 pos=new Vector3(Mathf.Sin(angle)*.62f,.86f,Mathf.Cos(angle)*.62f);
                    Primitive(PrimitiveType.Cylinder,"Plate",g,pos,new Vector3(.28f,.012f,.28f),a.RoadWhite,false);
                    Box("LinenNapkin",g,pos+Vector3.up*.025f,new Vector3(.11f,.02f,.18f),a.RoadWhite,false);
                }
            for(int i=0;i<4;i++)
            {
                float angle=i*90;
                Transform c=Group("Chair",g);c.localPosition=Quaternion.Euler(0,angle,0)*new Vector3(0,0,1.35f);c.localRotation=Quaternion.Euler(0,angle,0);
                Box("Seat",c,new Vector3(0,.46f,0),new Vector3(.58f,.12f,.58f),a.Leather);
                Box("Back",c,new Vector3(0,.8f,.25f),new Vector3(.58f,.65f,.1f),a.Leather);
                foreach(float x in new[]{-.22f,.22f}) foreach(float z in new[]{-.22f,.22f})
                    Box("Leg",c,new Vector3(x,.22f,z),new Vector3(.04f,.44f,.04f),a.Brass,false);
            }
        }
        public static void Worktop(Transform p,SkyscraperArt a,Vector3 at,Vector2 size)
        {
            Box("StainlessWorktop",p,at+Vector3.up*.92f,new Vector3(size.x,.1f,size.y),a.Steel);
            foreach(float x in new[]{-.5f,.5f}) foreach(float z in new[]{-.5f,.5f})
                Box("WorktopLeg",p,at+new Vector3(x*(size.x-.2f),.44f,z*(size.y-.2f)),new Vector3(.06f,.88f,.06f),a.Steel,false);
        }
    }
}
