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
            SkyscraperFineDining.Table(p,a,at,dressed);
        }

        public static void Worktop(Transform p,SkyscraperArt a,Vector3 at,Vector2 size)
        {
            Box("StainlessWorktop",p,at+Vector3.up*.92f,new Vector3(size.x,.1f,size.y),a.Steel);
            foreach(float x in new[]{-.5f,.5f}) foreach(float z in new[]{-.5f,.5f})
                Box("WorktopLeg",p,at+new Vector3(x*(size.x-.2f),.44f,z*(size.y-.2f)),new Vector3(.06f,.88f,.06f),a.Steel,false);
        }
    }
}
