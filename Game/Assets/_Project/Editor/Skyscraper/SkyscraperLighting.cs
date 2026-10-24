using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
namespace Abandoned.EditorTools
{
    public static class SkyscraperLighting
    {
        public static int Build(Transform root,SkyscraperArt a)
        {
            Transform g=Group("Lights",root);int count=0;
            foreach(float x in new[]{-5f,5f}) Lamp(g,a,new Vector3(x,4.1f,-13),true,ref count);
            foreach(float y in new[]{120f,200f,280f})
            foreach(float x in new[]{-5f,6f,14f})
            foreach(float z in new[]{-6f,6f,17f}) Lamp(g,a,new Vector3(x,y+(y==0?5.4f:3.1f),z),y==0,ref count);
            foreach(float x in new[]{-4.5f,4.5f})
            foreach(float z in new[]{8f,13f,18f}) Lamp(g,a,new Vector3(x,408.2f,z),true,ref count);
            foreach(Vector3 at in new[]{new Vector3(4,408f,-6),new Vector3(-12,407.4f,-7),new Vector3(-5.5f,406.8f,-7),
                new Vector3(-12,408.6f,11),new Vector3(-12,408.6f,17),new Vector3(12,408.6f,17),new Vector3(14.7f,408.7f,0),new Vector3(14.7f,408.7f,6)})
                Lamp(g,a,at,at.x>-8,ref count);
            foreach(Vector3 at in new[]{new Vector3(-4.8f,414.2f,-6.7f),new Vector3(.2f,414.2f,-6.7f),
                new Vector3(13,414.2f,2.5f),new Vector3(13,414.2f,8),new Vector3(13,414.2f,17.5f),
                new Vector3(-13,414.2f,12.5f),new Vector3(-13,414.2f,17.5f),new Vector3(7.2f,414.5f,3.5f),
                new Vector3(0,411.3f,8),new Vector3(0,411.3f,16.5f)})
                Lamp(g,a,at,true,ref count);
            BuildAtriumChandelier(g,a,ref count);
            for(int i=0;i<SkyscraperStairwell.HalfFlights;i++)
            {
                // Each luminaire sits over its landing and stays below the next one.
                float y=i*2f; bool north=i%2==1;
                Lamp(g,a,new Vector3(-12,y+1.75f,north?6.5f:-4.5f),false,ref count);
            }
            Transform car=root.Find("ElevatorShaft/ElevatorCab/CabDeck");
            if(car!=null)
            {
                Transform holder=Group("CabLuminaire",car);holder.localPosition=new Vector3(0,2.6f,0);
                Lamp(holder,a,Vector3.zero,true,ref count,true);
            }
            CreateBreakerPanel(g,a,0,new Vector3(5,0,-2.8f),Quaternion.identity);
            CreateBreakerPanel(g,a,2,new Vector3(3.5f,404f,-2.7f),Quaternion.identity);
            CreateBreakerPanel(g,a,3,new Vector3(-8.2f,404f,9),Quaternion.Euler(0,90,0));
            CreateBreakerPanel(g,a,4,new Vector3(12,SkyscraperLayout.UpperRestaurantY,13.2f),Quaternion.Euler(0,180,0));
            return count;
        }
        private static void BuildAtriumChandelier(Transform p,SkyscraperArt a,ref int count)
        {
            Transform g=Group("SignatureAtriumChandelier",p);
            g.position=new Vector3(0,SkyscraperLayout.RestaurantY,12.25f);
            for(int tier=0;tier<3;tier++)
            {
                float h=9.5f-tier*.9f,r=3.8f-tier;
                SkyscraperFineDining.Halo(g,"ContinuousBrassHalo",new Vector3(0,h,0),r,a.Brass);
                SkyscraperFineDining.Halo(g,"LuminousHalo",new Vector3(0,h-.06f,0),r-.055f,a.LampEmissive);
                for(int i=0;i<32;i++)
                {
                    float t=i*Mathf.PI*2/32;
                    Vector3 at=new Vector3(Mathf.Sin(t)*r,h,Mathf.Cos(t)*r);
                    Box("CrystalPendant",g,at+Vector3.down*.25f,new Vector3(.055f,.42f,.055f),a.Glass,false);
                    if(i%8==0)Box("SuspensionCable",g,new Vector3(at.x,(h+11.5f)*.5f,at.z),new Vector3(.012f,11.5f-h,.012f),a.Brass,false);
                }
            }
            Lamp(p,a,new Vector3(0,414.1f,12.25f),true,ref count);
        }
        private static void Lamp(Transform parent,SkyscraperArt a,Vector3 at,bool warm,ref int count,bool local=false)
        {
            Transform g=Group($"Practical_{count:000}",parent);
            if(local)g.localPosition=at;else g.position=at;
            Box("Housing",g,new Vector3(0,.04f,0),new Vector3(warm?.7f:1.1f,.1f,.3f),warm?a.Brass:a.Steel,false);
            GameObject diffuser=Box("Diffuser",g,new Vector3(0,-.025f,0),new Vector3(warm?.6f:1f,.03f,.22f),a.LampEmissive,false);
            Transform emitter=Group("Emitter",g);emitter.localPosition=Vector3.down*.1f;emitter.localRotation=Quaternion.Euler(90,0,0);
            if(warm && at.y>407f && Mathf.Abs(at.x)<6f && at.z>6f)
            {
                Box("PendantStem",g,new Vector3(0,.65f,0),new Vector3(.035f,1.3f,.035f),a.Brass,false);
                for(int i=0;i<24;i++)
                {
                    float angle=i*15f;
                    Vector3 pos=Quaternion.Euler(0,angle,0)*new Vector3(0,-.2f,.9f);
                    GameObject segment=Box("ChandelierRing",g,pos,new Vector3(.25f,.045f,.045f),a.Brass,false);
                    segment.transform.localRotation=Quaternion.Euler(0,angle,0);
                    if(i%2==0) Primitive(PrimitiveType.Sphere,"OpalGlobe",g,pos+Vector3.down*.1f,new Vector3(.12f,.18f,.12f),a.LampEmissive,false);
                }
            }
            Light light=emitter.gameObject.AddComponent<Light>();light.type=LightType.Spot;
            light.range=12;light.spotAngle=110;light.innerSpotAngle=60;light.intensity=12;
            light.color=warm?new Color(1,.82f,.58f):new Color(.72f,.84f,1);light.shadows=LightShadows.None;
            g.gameObject.AddComponent<LightFixture>().EditorSetup(light,diffuser.GetComponent<Renderer>(),count%17==4,false,4100+count);
            count++;
        }
        public static BreakerPanel CreateBreakerPanel(Transform parent, SkyscraperArt art, int sector, Vector3 at, Quaternion rot)
        {
            Transform breakerGroup = Group($"Sector{sector}_Breaker", parent);
            breakerGroup.SetPositionAndRotation(at, rot);
            Box("Cabinet", breakerGroup, new Vector3(0f, 1.2f, 0f), new Vector3(0.8f, 1.2f, 0.35f), art.Steel);
            Transform handle = Group("LeverHandle", breakerGroup);
            handle.localPosition = new Vector3(0.45f, 1.2f, 0f);
            Box("LeverArm", handle, new Vector3(0f, 0.15f, 0f), new Vector3(0.06f, 0.3f, 0.06f), art.Brass, false);
            handle.localRotation = Quaternion.Euler(35f, 0f, 0f);
            GameObject lamp = Box("BreakerLamp", breakerGroup, new Vector3(-0.25f, 1.65f, 0.18f),
                new Vector3(0.1f, 0.1f, 0.08f), art.LampEmissive, false);
            var breaker = breakerGroup.gameObject.AddComponent<BreakerPanel>();
            breaker.EditorSetup(sector, handle, lamp.GetComponent<Renderer>(), true);
            return breaker;
        }
    }
}
