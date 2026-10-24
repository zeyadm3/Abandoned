using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A=Abandoned.EditorTools.SkyscraperArchitecture;
namespace Abandoned.EditorTools
{
    public static class SkyscraperPrivateDining
    {
        public static void Build(Transform root,SkyscraperArt a)
        {
            Transform g=Group("PrivateDining",root);
            float y=SkyscraperLayout.UpperRestaurantY;
            Box("VIPWallSouth",g,new Vector3(13.5f,y+2.2f,13),new Vector3(9,4.4f,.2f),a.Mahogany);
            Box("VIPWallWestNorth",g,new Vector3(9,y+2.2f,19),new Vector3(.18f,4.4f,6),a.Mahogany);
            Box("VIPWallWestSouth",g,new Vector3(9,y+2.2f,13.5f),new Vector3(.18f,4.4f,1),a.Mahogany);
            A.Portal(g,new Vector3(9,y,15),2f,2.9f,a.Brass,true);
            Box("BanquetTop",g,new Vector3(13,y+.84f,17.5f),new Vector3(2.4f,.12f,4.8f),a.Mahogany);
            foreach(float z in new[]{16f,19f})
                Box("BanquetBase",g,new Vector3(13,y+.4f,z),new Vector3(1.2f,.8f,.6f),a.GraniteDark);
            foreach(float x in new[]{10.9f,15.4f})
            foreach(float z in new[]{16,17.5f,19})
                SkyscraperFineDining.Chair(g,a,new Vector3(x,y,z),x<13?90:-90);
            Dress(g,a);
            Box("SafePlinth",g,new Vector3(16.2f,y+.15f,14.3f),new Vector3(1.5f,.3f,1.2f),a.GraniteDark);
            Box("Credenza",g,new Vector3(16,y+.5f,20.7f),new Vector3(2.8f,1,.8f),a.Mahogany);
        }
        private static void Dress(Transform g,SkyscraperArt a)
        {
            float y=SkyscraperLayout.UpperRestaurantY;
            Box("BanquetLinenRunner",g,new Vector3(13,y+.91f,17.5f),new Vector3(.6f,.012f,4.6f),a.Linen,false);
            foreach(float z in new[]{16f,17.5f,19f})foreach(float x in new[]{12.2f,13.8f})
            {
                Transform setting=Group("PrivateTastingSetting",g);setting.localPosition=new Vector3(x,y+.92f,z);
                setting.localRotation=Quaternion.Euler(0,x<13?-90:90,0);SkyscraperFineDining.PlaceSetting(setting,a);
            }
            foreach(float z in new[]{16.5f,18.5f})SkyscraperFineDining.Candle(g,a,new Vector3(13,y+.92f,z));
            Label(g,"PrivateDiningIdentity","S A L O N   P R I V E",new Vector3(13,y+3.2f,13.12f),Quaternion.Euler(0,180,0),.15f,new Color(.94f,.82f,.53f));
            for(float x=10;x<=17;x+=.7f)
                Box("PrivateWallFluting",g,new Vector3(x,y+2.1f,13.13f),new Vector3(.035f,3.8f,.04f),a.Brass,false);
        }
    }
}
