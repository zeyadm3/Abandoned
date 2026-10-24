using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A = Abandoned.EditorTools.SkyscraperArchitecture;

namespace Abandoned.EditorTools
{
    public static class SkyscraperTowerShell
    {
        public static TruckCargo Build(Transform root, SkyscraperArt a)
        {
            Transform exterior=Group("Exterior",root);
            SkyscraperDowntown.Build(exterior,a);
            TruckCargo truck=SkyscraperPlaza.Build(exterior,a);
            Transform g=Group("TowerFacade",exterior);
            foreach(float x in new[]{-17.65f,17.65f})
            foreach(float z in new[]{-9.65f,21.65f})
                Box("CornerPier",g,new Vector3(x,SkyscraperLayout.RoofY*.5f,z),new Vector3(.7f,SkyscraperLayout.RoofY,.7f),a.GraniteDark);
            for(int f=0; f<Mathf.CeilToInt(SkyscraperLayout.RestaurantY/3.8f)+1; f++)
            {
                float bottom=f==0 ? 0 : 6+(f-1)*3.8f;
                float top=f==0 ? 6 : Mathf.Min(bottom+3.8f,SkyscraperLayout.RestaurantY);
                if(bottom>=SkyscraperLayout.RestaurantY) break;
                Face(g,a,bottom,top);
            }
            Face(g,a,SkyscraperLayout.RestaurantY,SkyscraperLayout.UpperRestaurantY);
            Face(g,a,SkyscraperLayout.UpperRestaurantY,SkyscraperLayout.RoofY);
            Transform canopy=Group("PorteCochere",g);
            Box("CanopyRoof",canopy,new Vector3(0,4.5f,-13.1f),new Vector3(11f,.25f,6.2f),a.GraniteDark);
            Box("BronzeFascia",canopy,new Vector3(0,4.3f,-16.25f),new Vector3(11f,.22f,.12f),a.Brass,false);
            for(float x=-5;x<=5;x+=2.5f)
                Box("CanopyRib",canopy,new Vector3(x,4.3f,-13.1f),new Vector3(.08f,.15f,6f),a.Brass,false);
            Label(canopy,"TowerIdentity","C L O U D L I N E",new Vector3(0,4.65f,-16.34f),Quaternion.identity,.12f,new Color(.9f,.78f,.5f));
            Transform roof=Group("RoofCrown",g);
            A.Deck(roof,SkyscraperLayout.RoofY,a.Concrete,true);
            A.Rail(roof,new Vector3(-17.7f,SkyscraperLayout.RoofY,-9.7f),new Vector3(17.7f,SkyscraperLayout.RoofY,-9.7f),a.Steel);
            A.Rail(roof,new Vector3(-17.7f,SkyscraperLayout.RoofY,21.7f),new Vector3(17.7f,SkyscraperLayout.RoofY,21.7f),a.Steel);
            A.Rail(roof,new Vector3(-17.7f,SkyscraperLayout.RoofY,-9.7f),new Vector3(-17.7f,SkyscraperLayout.RoofY,21.7f),a.Steel);
            A.Rail(roof,new Vector3(17.7f,SkyscraperLayout.RoofY,-9.7f),new Vector3(17.7f,SkyscraperLayout.RoofY,21.7f),a.Steel);
            return truck;
        }
        private static void Face(Transform g,SkyscraperArt a,float bottom,float top)
        {
            float cy=(bottom+top)/2f,h=top-bottom;
            bool occupied=bottom<6 || top>SkyscraperLayout.RestaurantY || (top>120 && bottom<124) || (top>200 && bottom<205) || (top>280 && bottom<284);
            Material glazing=occupied?a.Glass:a.TowerCladding;
            foreach(float x in new[]{-18f,18f})
            {
                Box("CurtainGlassSide",g,new Vector3(x,cy,6),new Vector3(.1f,h,32f),glazing);
                for(float z=-10;z<=22;z+=4)
                    Box("FacadeMullion",g,new Vector3(x,cy,z),new Vector3(.18f,h,.18f),a.Steel,false);
                Box("FacadeSpandrel",g,new Vector3(x,top-.2f,6),new Vector3(.24f,.4f,32f),a.GraniteDark,false);
            }
            foreach(float z in new[]{-10f,22f})
            {
                // The only street entrance is a six metre opening, without a revolving-door collider.
                if(bottom==0 && z<0)
                {
                    foreach(float x in new[]{-10.5f,10.5f})
                        Box("LobbyGlazing",g,new Vector3(x,cy,z),new Vector3(15f,h,.1f),a.Glass);
                    Box("EntryTransom",g,new Vector3(0,4.7f,z),new Vector3(6f,2.6f,.1f),a.Glass);
                    A.Portal(g,new Vector3(0,0,z),6f,3.4f,a.Brass);
                }
                else Box("CurtainGlassEnd",g,new Vector3(0,cy,z),new Vector3(36f,h,.1f),glazing);
                for(float x=-18;x<=18;x+=4)
                {
                    if(bottom==0 && z<0 && Mathf.Abs(x)<3.1f) continue;
                    Box("FacadeMullion",g,new Vector3(x,cy,z),new Vector3(.18f,h,.18f),a.Steel,false);
                }
                Box("FacadeSpandrel",g,new Vector3(0,top-.2f,z),new Vector3(36f,.4f,.24f),a.GraniteDark,false);
            }
        }
    }
}
