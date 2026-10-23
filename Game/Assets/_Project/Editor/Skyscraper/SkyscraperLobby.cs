using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A=Abandoned.EditorTools.SkyscraperArchitecture;
namespace Abandoned.EditorTools
{
    public static class SkyscraperLobby
    {
        public static void Build(Transform root,SkyscraperArt a)
        {
            Transform g=Group("Lobby",root);
            A.Deck(g,0,a.Marble);
            // This soffit uses the same holes as every deck, preserving stair and cab headroom.
            Transform ceiling=Group("LobbyCeiling",g); A.Deck(ceiling,6f,a.CeilingTile);
            foreach(float x in new[]{-6f,6f})
            {
                Box("MarblePier",g,new Vector3(x,3,12),new Vector3(.6f,6,.6f),a.GraniteDark);
                Box("BrassCapital",g,new Vector3(x,5.7f,12),new Vector3(.8f,.12f,.8f),a.Brass,false);
            }
            Transform reception=Group("Concierge",g);
            Box("DeskFront",reception,new Vector3(10, .55f,-6.5f),new Vector3(5,1.1f,1),a.Mahogany);
            Box("StoneTop",reception,new Vector3(10,1.14f,-6.5f),new Vector3(5.2f,.08f,1.15f),a.Marble);
            Label(g,"LiftWayfinding","LIFT  /  APEX  ↑",new Vector3(0,3.5f,-2.5f),Quaternion.identity,.17f,Color.white);
            Label(g,"StairWayfinding","←  STAIRS",new Vector3(-5.8f,2.6f,-2.6f),Quaternion.identity,.12f,Color.white);
            for(int i=0;i<3;i++)
            {
                float z=7+i*5;
                SkyscraperFurnishings.Bench(g,a,new Vector3(13,0,z),90);
                SkyscraperFurnishings.Bench(g,a,new Vector3(-5,0,z),-90);
            }
            Box("ReceptionFeatureWall",g,new Vector3(10,2.5f,-9.3f),new Vector3(10,5,.2f),a.Mahogany);
            Label(g,"Identity","C L O U D L I N E",new Vector3(10,3,-9.15f),Quaternion.Euler(0,180,0),.18f,new Color(.85f,.72f,.45f));
        }
    }
}
