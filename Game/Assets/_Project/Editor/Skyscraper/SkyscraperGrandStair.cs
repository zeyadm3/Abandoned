using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A=Abandoned.EditorTools.SkyscraperArchitecture;
using L=Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    public static class SkyscraperGrandStair
    {
        public static void Build(Transform root,SkyscraperArt a)
        {
            Transform g=Group("GrandStaircase",root);
            float y=L.RestaurantY;
            Flight(g,a,5.8f,y,true);
            A.Slab(g,new Vector3(7.2f,y+3f,3.65f),new Vector2(5.2f,2.3f),a.Marble).name="GrandStairMidLanding";
            Flight(g,a,8.6f,y+3f,false);
            A.Slab(g,new Vector3(8.6f,y+6f,-7.15f),new Vector2(2.4f,1.9f),a.Marble).name="GrandStairTopLanding";
            A.Rail(g,new Vector3(4.6f,y+3,4.7f),new Vector3(9.8f,y+3,4.7f),a.Brass);
            A.Rail(g,new Vector3(4.6f,y+3,2.5f),new Vector3(4.6f,y+3,4.7f),a.Brass);
            A.Rail(g,new Vector3(9.8f,y+3,2.5f),new Vector3(9.8f,y+3,4.7f),a.Brass);
            // Upper guards follow the opening, leaving the return flight's south landing open.
            A.Rail(g,new Vector3(4.55f,y+6,-7.8f),new Vector3(4.55f,y+6,4.8f),a.Brass);
            A.Rail(g,new Vector3(9.85f,y+6,-7.8f),new Vector3(9.85f,y+6,4.8f),a.Brass);
            A.Rail(g,new Vector3(4.55f,y+6,4.8f),new Vector3(9.85f,y+6,4.8f),a.Brass);
            A.Rail(g,new Vector3(4.55f,y+6,-7.8f),new Vector3(7.35f,y+6,-7.8f),a.Brass);
            Label(g,"UpperDiningWayfinding","THE GALLERY  /  CHEF'S TABLE",new Vector3(7.2f,y+5.2f,4.65f),Quaternion.identity,.13f,new Color(.91f,.78f,.48f));
        }

        private static void Flight(Transform p,SkyscraperArt a,float x,float y,bool north)
        {
            Transform g=Group(north?"ArrivalFlight":"GalleryFlight",p);
            float angle=Mathf.Atan2(3,9)*Mathf.Rad2Deg;
            var ramp=Box("ContinuousCarryRamp",g,new Vector3(x,y+1.5f-.10f/Mathf.Cos(angle*Mathf.Deg2Rad),-2),new Vector3(2.25f,.2f,Mathf.Sqrt(90)),a.Marble);
            ramp.transform.localRotation=Quaternion.Euler(north?-angle:angle,0,0);
            for(int i=0;i<20;i++)
            {
                float z=north?-6.5f+(i+.5f)*.45f:2.5f-(i+.5f)*.45f;
                // Visual risers hug the continuous collider for smooth two-player carrying.
                Box("MarbleTread",g,new Vector3(x,y+(i+.5f)*.15f+.015f,z),new Vector3(2.25f,.035f,.45f),a.Marble,false);
                Box("BrassNosing",g,new Vector3(x,y+(i+.5f)*.15f+.04f,z+(north?.21f:-.21f)),new Vector3(2.20f,.015f,.025f),a.Brass,false);
                Box("WalnutRiser",g,new Vector3(x,y+i*.15f,z+(north?-.21f:.21f)),new Vector3(2.25f,.15f,.03f),a.Mahogany,false);
            }
            foreach(float edge in new[]{x-1.16f,x+1.16f})
                A.Rail(g,new Vector3(edge,y,north?-6.5f:2.5f),new Vector3(edge,y+3,north?2.5f:-6.5f),a.Brass);
        }
    }
}
