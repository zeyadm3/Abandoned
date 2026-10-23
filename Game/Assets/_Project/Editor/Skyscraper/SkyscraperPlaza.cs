using Abandoned.Extraction;
using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L=Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    public static class SkyscraperPlaza
    {
        public static TruckCargo Build(Transform exterior,SkyscraperArt a)
        {
            Transform g=Group("Plaza",exterior);
            Box("Street",g,new Vector3(0,-.2f,-28),new Vector3(48,.4f,14),a.Asphalt);
            Box("Forecourt",g,new Vector3(0,-.2f,-15.5f),new Vector3(48,.4f,11),a.PlazaPaver);
            foreach(float x in new[]{-22f,22f})
                Box("Sidewalk",g,new Vector3(x,-.2f,6),new Vector3(8,.4f,32),a.PlazaPaver);
            for(float x=-22;x<=22;x+=4)
                Box("StreetStripe",g,new Vector3(x,.015f,-29),new Vector3(2,.015f,.14f),a.RoadYellow,false);
            foreach(float x in new[]{-11f,11f})
            {
                Box("Planter",g,new Vector3(x,.5f,-15),new Vector3(4,1,3),a.GraniteDark);
                Box("Soil",g,new Vector3(x,1.02f,-15),new Vector3(3.7f,.03f,2.7f),a.Mahogany,false);
                for(int i=0;i<5;i++)
                    Box("DryReed",g,new Vector3(x-1.4f+i*.7f,1.5f,-15),new Vector3(.025f,1,.025f),a.Rust,false);
                Box("BenchSeat",g,new Vector3(x,.5f,-18.2f),new Vector3(3,.16f,.7f),a.Mahogany);
                foreach(float dx in new[]{-1f,1f})
                    Box("BenchLeg",g,new Vector3(x+dx,.23f,-18.2f),new Vector3(.1f,.46f,.5f),a.Steel);
            }
            // A flush central approach connects the collection bay to the six metre entry.
            for(float z=-20;z<-10;z+=2)
                Box("ApproachInlay",g,new Vector3(0,.012f,z),new Vector3(5,.012f,.06f),a.Brass,false);
            var cargo=TruckBuilder.Build(Group("Truck",exterior),L.TruckRear,L.TruckForward);
            var parking=Group("VanParking",exterior);
            parking.position=L.TruckRear;
            parking.rotation=Quaternion.LookRotation(L.TruckForward);
            return cargo;
        }
    }
}
