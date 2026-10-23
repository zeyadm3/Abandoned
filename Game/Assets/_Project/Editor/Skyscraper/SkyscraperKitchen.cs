using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A=Abandoned.EditorTools.SkyscraperArchitecture;
namespace Abandoned.EditorTools
{
    public static class SkyscraperKitchen
    {
        public static void Build(Transform root,SkyscraperArt a)
        {
            Transform g=Group("Kitchen",root);
            // An open 3m service portal on the south, plus a 3m dining-room pass to the east.
            Box("KitchenSouthWall",g,new Vector3(-15,406f,7.3f),new Vector3(6,4,.2f),a.KitchenTile);
            Box("KitchenSouthPier",g,new Vector3(-8.5f,406f,7.3f),new Vector3(1,4,.2f),a.KitchenTile);
            A.Portal(g,new Vector3(-10.5f,404f,7.3f),3f,3f,a.Steel);
            Box("KitchenEastWallSouth",g,new Vector3(-8,406f,10),new Vector3(.2f,4,5.2f),a.KitchenTile);
            Box("KitchenEastWallNorth",g,new Vector3(-8,406f,18.8f),new Vector3(.2f,4,6.4f),a.KitchenTile);
            Box("KitchenPassHeader",g,new Vector3(-8,407.5f,14.1f),new Vector3(.2f,1,3),a.KitchenTile);
            A.Portal(g,new Vector3(-8,404f,14.1f),3f,3f,a.Steel,true);
            Box("Cookline",g,new Vector3(-16.6f,404.5f,13.8f),new Vector3(1.7f,1,9),a.Steel);
            for(int i=0;i<8;i++)
                Primitive(PrimitiveType.Cylinder,"Burner",g,new Vector3(-16.6f,405.025f,10+i),new Vector3(.42f,.015f,.42f),a.Rubber,false);
            Box("ExtractHood",g,new Vector3(-16.5f,407.1f,13.8f),new Vector3(2,.65f,9.3f),a.Steel,false);
            Box("ExtractDuct",g,new Vector3(-16.5f,408.6f,13.8f),new Vector3(.8f,2.4f,.8f),a.Steel,false);
            SkyscraperFurnishings.Worktop(g,a,new Vector3(-12.8f,404f,13.7f),new Vector2(1.4f,5));
            SkyscraperFurnishings.Worktop(g,a,new Vector3(-12,404f,20.8f),new Vector2(6,.9f));
            for(int i=0;i<3;i++)
                Box("SinkBasin",g,new Vector3(-14+i*2,404.99f,20.8f),new Vector3(1.2f,.04f,.6f),a.GraniteDark,false);
            // A cold-store alcove can be entered through its full two metre opening.
            Box("ColdStoreFrontL",g,new Vector3(-17,406f,18.7f),new Vector3(2,4,.2f),a.ColdStoreWall);
            Box("ColdStoreFrontR",g,new Vector3(-13.6f,406f,18.7f),new Vector3(.8f,4,.2f),a.ColdStoreWall);
            A.Portal(g,new Vector3(-15,404f,18.7f),2f,2.8f,a.Steel);
            Label(g,"ServiceDirection","SERVICE / EXIT  ↓",new Vector3(-10.5f,407.5f,7.1f),Quaternion.identity,.1f,Color.white);
        }
    }
}
