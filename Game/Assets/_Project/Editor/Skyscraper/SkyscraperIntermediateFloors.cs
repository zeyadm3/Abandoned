using System.Collections.Generic;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A=Abandoned.EditorTools.SkyscraperArchitecture;
namespace Abandoned.EditorTools
{
    public static class SkyscraperIntermediateFloors
    {
        public static List<Transform> Build(Transform root,SkyscraperArt a,out List<Transform> weak,out List<Transform> sounds)
        {
            Transform g=Group("IntermediateFloors",root);
            var slabs=new List<Transform>();weak=new List<Transform>();sounds=new List<Transform>();
            foreach(float y in new[]{120f,280f})
            {
                Transform floor=Group(y==120?"FloodedFloor_120m":"LootedOffice_280m",g);
                slabs.AddRange(A.Deck(floor,y,a.Concrete));
                A.Deck(Group("OfficeCeiling",floor),y+3.5f,a.CeilingTile);
                for(int i=0;i<4;i++)
                {
                    float x=-5+i*6;
                    SkyscraperFurnishings.Worktop(floor,a,new Vector3(x,y,9),new Vector2(2,1));
                    Box("AbandonedMonitor",floor,new Vector3(x,y+1.22f,9.2f),new Vector3(.7f,.5f,.25f),a.GraniteDark);
                    Box("OfficeDivider",floor,new Vector3(x,y+.65f,10),new Vector3(3,1.3f,.1f),a.WallPlaster);
                }
                Box("SprinklerPipe",floor,new Vector3(-5,y+3.1f,5),new Vector3(20,.08f,.08f),a.Rust,false);
                Transform sound=Group("WindOrDrip",floor);sound.position=new Vector3(-5,y+1.5f,5);sounds.Add(sound);
                if(y==120) Box("ShallowPuddle",floor,new Vector3(6,y+.012f,14),new Vector3(8,.015f,5),a.WaterMurky,false);
            }
            Transform roof=Group("RooftopInfrastructure",g);
            foreach(float x in new[]{5f,12f})
            {
                Box("RooftopChiller",roof,new Vector3(x,SkyscraperLayout.RoofY+1.1f,-5),new Vector3(4,2.2f,4),a.Steel);
                foreach(float z in new[]{-6f,-4f})
                    Primitive(PrimitiveType.Cylinder,"ChillerFan",roof,new Vector3(x,SkyscraperLayout.RoofY+2.25f,z),new Vector3(1.5f,.08f,1.5f),a.GraniteDark,false);
            }
            // Walkable roof access emerges north of the stair core, clear of plant equipment.
            Box("GantryTrack",roof,new Vector3(0,SkyscraperLayout.RoofY+.12f,20),new Vector3(28,.2f,.3f),a.Steel);
            Box("GantryMast",roof,new Vector3(3,SkyscraperLayout.RoofY+2f,20),new Vector3(.3f,4,.3f),a.Steel);
            Box("GantryBoom",roof,new Vector3(3,SkyscraperLayout.RoofY+4f,21),new Vector3(.3f,.3f,4),a.Steel);
            for(float x=-3;x<=3;x+=6)
            {
                Box("HelipadBar",roof,new Vector3(x,SkyscraperLayout.RoofY+.015f,12),new Vector3(.4f,.015f,6),a.RoadWhite,false);
            }
            Box("HelipadCrossbar",roof,new Vector3(0,SkyscraperLayout.RoofY+.015f,12),new Vector3(6,.015f,.4f),a.RoadWhite,false);
            for(float x=-12;x<=12;x+=6)
                Box("RoofEdgeBeacon",roof,new Vector3(x,SkyscraperLayout.RoofY+.4f,21.5f),new Vector3(.16f,.2f,.16f),a.RedEmergencyEmissive,false);
            SkyscraperFurnishings.Worktop(roof,a,new Vector3(-5,SkyscraperLayout.RoofY,18),new Vector2(2,1));
            return slabs;
        }
    }
}
