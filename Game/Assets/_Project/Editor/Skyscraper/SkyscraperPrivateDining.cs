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
            Box("VIPWallSouth",g,new Vector3(14,406.2f,13),new Vector3(8,4.4f,.2f),a.Mahogany);
            Box("VIPWallWestNorth",g,new Vector3(8,406.2f,19),new Vector3(.18f,4.4f,6),a.Mahogany);
            Box("VIPWallWestSouth",g,new Vector3(8,406.2f,13.5f),new Vector3(.18f,4.4f,1),a.Mahogany);
            A.Portal(g,new Vector3(8,404f,15),2f,2.9f,a.Brass,true);
            Box("BanquetTop",g,new Vector3(13,404.84f,17.5f),new Vector3(2.4f,.12f,4.8f),a.Mahogany);
            foreach(float z in new[]{16f,19f})
                Box("BanquetBase",g,new Vector3(13,404.4f,z),new Vector3(1.2f,.8f,.6f),a.GraniteDark);
            foreach(float x in new[]{10.6f,15.4f})
            foreach(float z in new[]{16,17.5f,19})
                SkyscraperFurnishings.Bench(g,a,new Vector3(x,404f,z),x<13?-90:90,true);
            Box("SafePlinth",g,new Vector3(16.2f,404.15f,14.3f),new Vector3(1.5f,.3f,1.2f),a.GraniteDark);
            Box("Credenza",g,new Vector3(16,404.5f,20.7f),new Vector3(2.8f,1,.8f),a.Mahogany);
        }
    }
}
