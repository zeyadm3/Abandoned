using UnityEngine;

namespace Abandoned.Hospital
{
    public static class HospitalSite
    {
        public static void Build(Transform root,HospitalKit k)
        {
            Transform site=HospitalKit.Group("Forecourt",root);
            // Ground exists only outside the footprint, so fallen upper slabs open real holes to the floor below.
            k.Box("SouthApron",site,new Vector3(24,-.2f,-14),new Vector3(72,.4f,28),k.Asphalt);
            k.Box("NorthAccess",site,new Vector3(24,-.2f,45),new Vector3(72,.4f,10),k.Asphalt);
            foreach(float x in new[]{-6f,54f})k.Box("SideAccess",site,new Vector3(x,-.2f,20),new Vector3(12,.4f,40),k.Asphalt);
            foreach(float x in new[]{20f,28f})k.Box("HaulLaneEdge",site,new Vector3(x,.012f,-8),new Vector3(.1f,.02f,16),k.Glow,false);
            Transform dock=HospitalKit.Group("VanDock",root);dock.localPosition=new Vector3(24,0,-14);dock.localRotation=Quaternion.Euler(0,180,0);
            k.Label(site,"LOADING",new Vector3(31,.025f,-12),yaw:0,size:.06f);
            for(int i=0;i<5;i++)
            {
                float x=2+i*4;
                k.Box("ParkingStripe",site,new Vector3(x,.012f,-22),new Vector3(.1f,.02f,5),k.Glow,false);
                k.Box("WheelStop",site,new Vector3(x+1.8f,.12f,-24),new Vector3(2.5f,.24f,.25f),k.Metal);
            }
            foreach(float x in new[]{-9f,57f})foreach(float z in new[]{-22f,45f})
            {
                k.Box("LampPost",site,new Vector3(x,3,z),new Vector3(.18f,6,.18f),k.Metal);
                Transform head=HospitalKit.Group("BatteryYardLight",site);head.localPosition=new Vector3(x,5.8f,z);
                head.gameObject.AddComponent<Abandoned.Core.PowerIndependentLight>();
                var light=head.gameObject.AddComponent<Light>();light.type=LightType.Point;light.range=15;light.intensity=2;light.color=new Color(1,.76f,.48f);light.shadows=LightShadows.None;
            }
            // Low perimeter walls leave the whole 12 m road mouth open to the world's road.
            foreach(float x in new[]{-11.8f,59.8f})k.Box("Boundary",site,new Vector3(x,.6f,10),new Vector3(.25f,1.2f,76),k.Wall);
            k.Box("RearBoundary",site,new Vector3(24,.6f,49.8f),new Vector3(72,1.2f,.25f),k.Wall);
        }
    }
}
