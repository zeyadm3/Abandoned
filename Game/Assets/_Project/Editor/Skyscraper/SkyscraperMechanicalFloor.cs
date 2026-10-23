using System.Collections.Generic;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A=Abandoned.EditorTools.SkyscraperArchitecture;
namespace Abandoned.EditorTools
{
    public static class SkyscraperMechanicalFloor
    {
        public static List<Transform> Build(Transform root,SkyscraperArt a,out List<Transform> weak)
        {
            Transform g=Group("MechanicalFloor_200m",root);
            var slabs=A.Deck(g,200f,a.Concrete);weak=new List<Transform>();
            A.Deck(Group("MechanicalCeiling",g),205f,a.Concrete);
            SkyscraperLighting.CreateBreakerPanel(g,a,1,new Vector3(3.5f,200f,-2.7f),Quaternion.identity);
            Label(g,"LiftRecovery","LIFT POWER / BREAKER",new Vector3(3.5f,202.3f,-2.65f),Quaternion.Euler(0,180,0),.1f,Color.white);
            foreach(float x in new[]{-12f,8f,14f})
            {
                Box("AirHandlingUnit",g,new Vector3(x,201.25f,14),new Vector3(3.4f,2.5f,5),a.Steel);
                for(int i=0;i<12;i++)
                    Box("Louvre",g,new Vector3(x,200.4f+i*.16f,11.46f),new Vector3(3,.06f,.07f),a.GraniteDark,false);
                Primitive(PrimitiveType.Cylinder,"FanCowl",g,new Vector3(x,202.6f,14),new Vector3(2,.1f,2),a.Steel,false);
            }
            SkyscraperFurnishings.Worktop(g,a,new Vector3(-5,200f,9),new Vector2(3,1));
            SkyscraperFurnishings.Worktop(g,a,new Vector3(6,200f,6),new Vector2(2,1));
            Box("VentilationTrunk",g,new Vector3(0,204.1f,17),new Vector3(32,.7f,1.2f),a.Steel,false);
            return slabs;
        }
    }
}
