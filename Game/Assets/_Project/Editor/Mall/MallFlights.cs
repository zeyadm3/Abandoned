using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    public static class MallFlights
    {
        public const string RampName="Ramp",VisualName="Visual";
        public const int Steps=20;
        public static float Length=>8;
        public static float Rise=>4;
        public static float WidthOf(MallLayout.Flight f)=>f.Width;
        public static Transform Build(MallLayout.Flight f,Transform parent)
        {
            var root=Group(f.Name,parent);
            root.localPosition=f.Foot;
            root.localRotation=Quaternion.LookRotation(new Vector3(f.Dir.x,0,f.Dir.y));
            root.gameObject.AddComponent<SectionFlightGeometry>().EditorSetup(f.Length,f.Rise,f.Width);
            var visual=Group(VisualName,root);
            float angle=Mathf.Atan2(f.Rise,f.Length),slope=Mathf.Sqrt(f.Length*f.Length+f.Rise*f.Rise);
            float lift=f.Kind==MallLayout.FlightKind.HaulRamp?.04f:f.Rise/Steps/2;
            Vector3 up=new(0,Mathf.Cos(angle),-Mathf.Sin(angle));
            Vector3 mid=new(0,f.Rise/2+lift,f.Length/2);
            Quaternion tilt=Quaternion.Euler(-angle*Mathf.Rad2Deg,0,0);
            var ramp=Group(RampName,root);
            ramp.localPosition=mid-up*.125f;ramp.localRotation=tilt;
            ramp.gameObject.AddComponent<BoxCollider>().size=new Vector3(f.Width,.25f,slope);
            if(MeridianArt.UseKit)
            {
                string key=f.Kind==MallLayout.FlightKind.HaulRamp?"HaulRamp":f.Kind==MallLayout.FlightKind.Stair?"ServiceStair":"Escalator";
                MeridianArt.Place(key,visual,Vector3.zero);
            }
            else
            {
                var deck=Box("Deck",visual,ramp.localPosition,new Vector3(f.Width,.25f,slope),MeridianArt.Material(f.CanCollapse?"Metal":"Concrete"),false);
                deck.transform.localRotation=tilt;
                if(f.Kind!=MallLayout.FlightKind.HaulRamp)
                    for(int i=0;i<Steps;i++) Box($"Tread_{i}",visual,new Vector3(0,(i+.5f)*f.Rise/Steps,(i+.5f)*f.Length/Steps),
                        new Vector3(f.Width,.04f,f.Length/Steps),MeridianArt.Material("Metal"),false);
            }
            foreach(float side in new[]{-1f,1f})
            {
                var rail=Box(side<0?"SideRail_L":"SideRail_R",visual,mid+up*.525f+Vector3.right*side*(f.Width/2+.075f),
                    new Vector3(.15f,1.05f,slope),MeridianArt.Material("Metal"));
                rail.transform.localRotation=tilt;
                if(MeridianArt.UseKit) rail.GetComponent<Renderer>().enabled=false;
            }
            return root;
        }
        public static void Landings(Transform parent)
        {
            for(int floor=-1;floor<MallLayout.Floors-1;floor++)
            {
                var slab=Group($"RampTurn_{floor}",parent);
                slab.localPosition=new Vector3(51.75f,MallLayout.FloorY(floor)+2,46);
                var collider=slab.gameObject.AddComponent<BoxCollider>();
                collider.center=Vector3.down*.125f;collider.size=new Vector3(8.4f,.25f,4);
                if(MeridianArt.UseKit)MeridianArt.Place("FloorConcrete",Group("Visual",slab),Vector3.zero,0,new Vector3(2.1f,1,1));
                else Box("Visual",slab,collider.center,collider.size,MeridianArt.Material("Concrete"),false);
            }
        }
    }
}
