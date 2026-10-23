using System.Collections.Generic;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A=Abandoned.EditorTools.SkyscraperArchitecture;
namespace Abandoned.EditorTools
{
    public static class SkyscraperRestaurantFloor
    {
        public static List<Transform> Build(Transform root,SkyscraperArt a,out List<Transform> weak)
        {
            Transform g=Group("RestaurantFloor_404m",root);
            var slabs=A.Deck(Group("FloorSlabs",g),404f,a.Parquet);
            weak=new List<Transform>();
            foreach(Transform s in slabs)
                if(s.position.z>18 && s.position.x>-7 && s.position.x<7) weak.Add(s);
            Transform arrival=Group("SkyLobby",g);
            Box("HostPodium",arrival,new Vector3(5.8f,404.55f,-6.8f),new Vector3(.9f,1.1f,.7f),a.Mahogany);
            Label(arrival,"RestaurantIdentity","A P E X",new Vector3(5.8f,407.3f,-9.85f),Quaternion.Euler(0,180,0),.4f,new Color(.9f,.75f,.4f));
            Label(arrival,"Directions","DINING  →     ←  STAIRS",new Vector3(0,407.2f,-2.55f),Quaternion.identity,.11f,Color.white);
            // The central aisle remains four metres wide from the arrival hall to the north view.
            foreach(Vector2 table in A.DiningTables)
                SkyscraperFurnishings.Table(g,a,new Vector3(table.x,404f,table.y));
            for(float z=5;z<=21;z+=4)
                Box("AisleBrassInlay",g,new Vector3(0,404.012f,z),new Vector3(.06f,.015f,2),a.Brass,false);
            foreach(float x in new[]{-7.7f,7.7f})
            {
                Box("DiningPier",g,new Vector3(x,407f,12),new Vector3(.4f,6,.4f),a.GraniteDark);
                Box("CeilingBeam",g,new Vector3(x,409.6f,12),new Vector3(.5f,.3f,20),a.Mahogany,false);
            }
            // Low window-side banquettes leave room for traffic behind the dining chairs.
            SkyscraperFurnishings.Bench(g,a,new Vector3(-4.5f,404f,21),0);
            SkyscraperFurnishings.Bench(g,a,new Vector3(4.5f,404f,21),0);
            BuildBar(g,a);
            SkyscraperRestrooms.Build(g,a);
            SkyscraperWineVault.Build(g,a);
            return slabs;
        }
        private static void BuildBar(Transform g,SkyscraperArt a)
        {
            Transform bar=Group("SkyLounge",g);
            Box("BarCabinet",bar,new Vector3(14.7f,404.55f,3.5f),new Vector3(1.2f,1.1f,11),a.Mahogany);
            Box("BarStoneTop",bar,new Vector3(14.7f,405.15f,3.5f),new Vector3(1.5f,.12f,11.3f),a.GraniteDark);
            for(int i=0;i<3;i++)
            {
                float y=405.1f+i*.65f;
                Box("BackBarShelf",bar,new Vector3(17.25f,y,3.5f),new Vector3(.8f,.08f,11),a.Brass);
                for(int b=0;b<18;b++)
                    Primitive(PrimitiveType.Cylinder,"DustyBottle",bar,new Vector3(17.2f,y+.22f,-1.5f+b*.57f),new Vector3(.11f,.17f,.11f),b%3==0?a.CarPaintBurgundy:a.Glass,false);
            }
            foreach(float z in new[]{0f,3f,6f})
            {
                Primitive(PrimitiveType.Cylinder,"StoolStem",bar,new Vector3(12.8f,404.4f,z),new Vector3(.1f,.4f,.1f),a.Brass);
                Primitive(PrimitiveType.Cylinder,"StoolSeat",bar,new Vector3(12.8f,404.84f,z),new Vector3(.55f,.06f,.55f),a.Leather);
            }
            SkyscraperFurnishings.Bench(bar,a,new Vector3(10,404f,10.8f),0);
            SkyscraperFurnishings.Table(bar,a,new Vector3(10,404f,8.6f),false);
            Box("BarSoffit",bar,new Vector3(15.3f,409.2f,3.5f),new Vector3(4.8f,.35f,12),a.Mahogany,false);
        }
    }
}
