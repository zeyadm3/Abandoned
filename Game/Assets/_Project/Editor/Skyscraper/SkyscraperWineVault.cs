using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A=Abandoned.EditorTools.SkyscraperArchitecture;
namespace Abandoned.EditorTools
{
    public static class SkyscraperWineVault
    {
        public static void Build(Transform parent,SkyscraperArt a)
        {
            Transform g=Group("WineVault",parent);
            // The north side meets the stair core; cellar access is through its east face.
            Box("VaultWallEastSouth",g,new Vector3(-8.15f,405.8f,-9.55f),new Vector3(.12f,3.6f,.9f),a.Glass);
            Box("VaultWallEastNorth",g,new Vector3(-8.15f,405.8f,-5.25f),new Vector3(.12f,3.6f,.5f),a.Glass);
            A.Portal(g,new Vector3(-8.15f,404f,-7.3f),3.6f,3.2f,a.Brass,true);
            for(int level=0;level<5;level++)
            {
                float y=404.4f+level*.5f;
                Box("WineRackShelf",g,new Vector3(-13, y,-9.3f),new Vector3(8,.07f,.7f),a.Mahogany);
                for(int i=0;i<22;i++)
                    Primitive(PrimitiveType.Cylinder,"ReserveBottle",g,new Vector3(-16.5f+i*.34f,y+.2f,-9.3f),new Vector3(.1f,.15f,.1f),a.CarPaintBurgundy,false);
            }
            Box("CaskCradle",g,new Vector3(-15,404.2f,-7),new Vector3(2.4f,.4f,1.3f),a.Mahogany);
            SkyscraperFurnishings.Worktop(g,a,new Vector3(-10,404f,-7.8f),new Vector2(1.6f,1));
        }
    }
}
