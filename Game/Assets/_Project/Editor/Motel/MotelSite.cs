using Abandoned.Core;
using Unity.AI.Navigation;
using UnityEngine;

namespace Abandoned.Motel
{
    public static class MotelSite
    {
        public static void Build(Transform root,MotelKit k)
        {
            Transform g=MotelKit.Group("Courtyard",root);
            k.Box("VanApron",g,new Vector3(32,-.2f,-12),new Vector3(80,.4f,24),k.Asphalt);
            // Four surrounds leave an actual hole over the pool basin; no invisible ground closes the depression.
            k.Box("WestCourt",g,new Vector3(5,-.2f,6),new Vector3(10,.4f,12),k.Asphalt);
            k.Box("EastCourt",g,new Vector3(43,-.2f,6),new Vector3(42,.4f,12),k.Asphalt);
            k.Box("PoolSouthWalk",g,new Vector3(16,-.2f,1),new Vector3(12,.4f,2),k.Asphalt);
            k.Box("PoolNorthWalk",g,new Vector3(16,-.2f,10),new Vector3(12,.4f,4),k.Asphalt);
            foreach(float x in new[]{-4f,68f})k.Box("SideDrive",g,new Vector3(x,-.2f,14),new Vector3(8,.4f,28),k.Asphalt);
            k.Box("RearServiceDrive",g,new Vector3(32,-.2f,31),new Vector3(80,.4f,6),k.Asphalt);
            Pool(g,k);Sign(g,k);
            Transform dock=MotelKit.Group("VanDock",root);dock.localPosition=new Vector3(32,0,-12);dock.localRotation=Quaternion.Euler(0,180,0);
            foreach(float x in new[]{28f,36f})k.Box("CarryLaneStripe",g,new Vector3(x,.012f,-1),new Vector3(.1f,.02f,22),k.Glow,false);
            for(int i=0;i<6;i++)
            {
                float x=2+i*4;
                k.Box("ParkingStripe",g,new Vector3(x,.012f,-17),new Vector3(.1f,.02f,5),k.Glow,false);
                k.Box("WheelStop",g,new Vector3(x+1.8f,.12f,-19),new Vector3(2.5f,.24f,.22f),k.Metal);
            }
            foreach(float x in new[]{-6f,70f})foreach(float z in new[]{-18f,31f})
            {
                k.Box("YardPost",g,new Vector3(x,2.6f,z),new Vector3(.16f,5.2f,.16f),k.Metal);
                Transform head=MotelKit.Group("IndependentYardLamp",g);head.localPosition=new Vector3(x,5,z);
                head.gameObject.AddComponent<PowerIndependentLight>();
                var lamp=head.gameObject.AddComponent<Light>();lamp.type=LightType.Point;lamp.range=15;lamp.intensity=2;lamp.color=new Color(1,.7f,.4f);lamp.shadows=LightShadows.None;
            }
            foreach(float x in new[]{-7.8f,71.8f})k.Box("Perimeter",g,new Vector3(x,.55f,5),new Vector3(.2f,1.1f,58),k.Accent);
            k.Box("RearFence",g,new Vector3(32,.55f,33.8f),new Vector3(80,1.1f,.2f),k.Accent);
        }
        private static void Pool(Transform p,MotelKit k)
        {
            Transform g=MotelKit.Group("DrainedPool",p);
            k.Box("PoolFloor",g,new Vector3(16,-.95f,5),new Vector3(12,.3f,6),k.Wall);
            foreach(float x in new[]{10f,22f})
            {
                k.Box("PoolWall",g,new Vector3(x,-.4f,5),new Vector3(.18f,.8f,6),k.Wall);
                k.Rail(g,new Vector3(x,0,2),new Vector3(x,0,8));
            }
            k.Box("SouthPoolWall",g,new Vector3(16,-.4f,2),new Vector3(12,.8f,.18f),k.Wall);
            k.Rail(g,new Vector3(10,0,2),new Vector3(22,0,2));
            foreach(float x in new[]{12.4f,19.6f})k.Box("NorthPoolWall",g,new Vector3(x,-.4f,8),new Vector3(4.8f,.8f,.18f),k.Wall);
            k.Rail(g,new Vector3(10,0,8),new Vector3(14.8f,0,8));k.Rail(g,new Vector3(17.2f,0,8),new Vector3(22,0,8));
            Transform ramp=MotelKit.Group("PoolExitRamp",g);ramp.localPosition=new Vector3(16,-.45f,7);ramp.localRotation=Quaternion.Euler(-21.801f,0,0);
            k.Box("Ramp",ramp,Vector3.zero,new Vector3(2.4f,.1f,Mathf.Sqrt(4.64f)),k.Wall);
            var link=g.gameObject.AddComponent<NavMeshLink>();link.startPoint=new Vector3(16,-.8f,5.6f);link.endPoint=new Vector3(16,0,8.6f);link.width=2;link.bidirectional=true;
            k.Box("Drain",g,new Vector3(18,-.79f,4),new Vector3(.35f,.02f,.35f),k.Metal,false);
            Transform water=MotelKit.Group("PoolDrip",g);water.localPosition=new Vector3(16,-.4f,5);
            water.gameObject.AddComponent<Abandoned.Audio.HorrorAreaAmbience>().EditorSetup(new Vector3(12,2,6),2);
        }
        private static void Sign(Transform p,MotelKit k)
        {
            Transform g=MotelKit.Group("RoadSign",p);g.localPosition=new Vector3(66,0,-10);
            foreach(float x in new[]{-1.4f,1.4f})k.Box("Post",g,new Vector3(x,3,0),new Vector3(.18f,6,.18f),k.Metal);
            k.Box("Marquee",g,new Vector3(0,5.4f,0),new Vector3(5,2,.3f),k.Accent);
            k.Label(g,"LAST LIGHT",new Vector3(0,5.85f,-.17f),size:.075f);
            k.Label(g,"MOTOR LODGE",new Vector3(0,5.1f,-.17f),size:.05f);
            k.Box("NeonBar",g,new Vector3(0,4.65f,-.2f),new Vector3(4,.04f,.04f),k.Glow,false);
        }
    }
}
