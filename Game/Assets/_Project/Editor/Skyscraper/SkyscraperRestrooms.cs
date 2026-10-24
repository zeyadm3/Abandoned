using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A=Abandoned.EditorTools.SkyscraperArchitecture;
namespace Abandoned.EditorTools
{
    public static class SkyscraperRestrooms
    {
        public static void Build(Transform parent,SkyscraperArt a)
        {
            Transform g=Group("Restrooms",parent);
            // Move the powder room to the southeast, leaving the cellar approach clear.
            g.localPosition=Vector3.right*20f;
            Box("RestroomWallWest",g,new Vector3(-8.1f,405.6f,-7.6f),new Vector3(.18f,3.2f,4.8f),a.WallPlaster);
            Box("RestroomWallEast",g,new Vector3(-3f,405.6f,-7.6f),new Vector3(.18f,3.2f,4.8f),a.WallPlaster);
            Box("RestroomWallFront",g,new Vector3(-6.5f,405.6f,-5.2f),new Vector3(3.2f,3.2f,.18f),a.WallPlaster);
            A.Portal(g,new Vector3(-4,404f,-5.2f),1.8f,2.6f,a.Brass);
            Box("Vanity",g,new Vector3(-5.5f,404.85f,-9.3f),new Vector3(3.5f,.12f,.9f),a.Marble);
            Box("Mirror",g,new Vector3(-5.5f,405.8f,-9.7f),new Vector3(3.5f,1.3f,.06f),a.Glass,false);
            foreach(float x in new[]{-6.3f,-4.7f})
            {
                Primitive(PrimitiveType.Cylinder,"Basin",g,new Vector3(x,404.94f,-9.2f),new Vector3(.5f,.04f,.4f),a.RoadWhite,false);
                Box("Tap",g,new Vector3(x,405.1f,-9.5f),new Vector3(.04f,.3f,.04f),a.Brass,false);
            }
        }
    }
}
