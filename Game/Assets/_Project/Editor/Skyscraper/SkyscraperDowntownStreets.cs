using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
namespace Abandoned.EditorTools
{
    public static class SkyscraperDowntownStreets
    {
        public static void Build(Transform city,SkyscraperArt a,SkyscraperDowntownMaterials p)
        {
            Transform g=Group("StreetGrid",city);
            Rect r=SkyscraperDowntown.Ground;
            Box("DowntownGround",g,new Vector3(0,-.43f,22),new Vector3(r.width,.5f,r.height),a.Asphalt);
            for(int n=-3;n<=4;n++)
            {
                float x=n*100-50,z=n*100-28;
                // The original Broadway arrival bay supplies the centre of this one road.
                if(n==0)
                {
                    Box("BroadwayWest",g,new Vector3(-192,-.12f,z),new Vector3(336,.12f,14),a.Asphalt);
                    Box("BroadwayEast",g,new Vector3(192,-.12f,z),new Vector3(336,.12f,14),a.Asphalt);
                }
                else Box("Avenue",g,new Vector3(0,-.12f,z),new Vector3(720,.12f,14),a.Asphalt);
                Box("CrossStreet",g,new Vector3(x,-.12f,22),new Vector3(14,.12f,720),a.Asphalt);
                for(float d=-340;d<=340;d+=16)
                {
                    if(n!=0 || Mathf.Abs(d)>28)
                        Box("LaneMarking",g,new Vector3(d,-.048f,z),new Vector3(5,.01f,.13f),a.RoadYellow,false);
                    Box("LaneMarking",g,new Vector3(x,-.048f,d+22),new Vector3(.13f,.01f,5),a.RoadYellow,false);
                }
            }
            for(int z=-3;z<=3;z++)for(int x=-3;x<=3;x++)
            {
                float px=x*100,pz=z*100+22;
                if(x!=0 || z!=0)
                    Box("BlockPavement",g,new Vector3(px,-.075f,pz),new Vector3(82,.15f,82),p.Sidewalk);
                else
                {
                    // Fill around the saved plaza without introducing coplanar road or lobby surfaces.
                    Box("HomeBlockNorth",g,new Vector3(0,-.075f,43),new Vector3(82,.15f,42),p.Sidewalk);
                    foreach(float side in new[]{-1f,1f})
                        Box("HomeBlockSide",g,new Vector3(side*33.5f,-.075f,0),new Vector3(15,.15f,44),p.Sidewalk);
                }
                foreach(float side in new[]{-1f,1f})
                {
                    Lamp(g,a,p,new Vector3(px+side*39,0,pz-39));
                    for(int k=-2;k<=2;k++)
                        Box("CrosswalkPaint",g,new Vector3(px+side*44,-.04f,pz-4+k*1.4f),new Vector3(3,.015f,.65f),a.RoadWhite,false);
                }
                if(x==0 && z==0)continue;
                int seed=(x+4)*173+(z+4)*71;
                if(seed%2==0) Car(g,a,new Vector3(px+27,0,pz-46),seed);
                if(seed%3==0)
                {
                    Box("BusShelterCanopy",g,new Vector3(px-21,2.8f,pz-40),new Vector3(5,.16f,2),a.Steel,false);
                    foreach(float offset in new[]{-2f,2f})
                        Box("ShelterPost",g,new Vector3(px-21+offset,1.35f,pz-39.4f),new Vector3(.08f,2.7f,.08f),a.Steel,false);
                    Box("BusBench",g,new Vector3(px-21,.45f,pz-39.5f),new Vector3(4,.18f,.7f),a.Mahogany,false);
                }
            }
        }
        private static void Lamp(Transform p,SkyscraperArt a,SkyscraperDowntownMaterials palette,Vector3 at)
        {
            Transform g=Group("CityStreetLamp",p);g.position=at;
            Box("Pole",g,new Vector3(0,3.3f,0),new Vector3(.12f,6.6f,.12f),a.Steel,false);
            Box("Arm",g,new Vector3(0,6.5f,-.65f),new Vector3(.1f,.1f,1.4f),a.Steel,false);
            Box("Lantern",g,new Vector3(0,6.4f,-1.2f),new Vector3(.5f,.12f,.75f),palette.WindowLight,false);
        }
        private static void Car(Transform p,SkyscraperArt a,Vector3 at,int seed)
        {
            Transform g=Group("ParkedCityCar",p);g.position=at;
            Box("Body",g,new Vector3(0,.6f,0),new Vector3(4.6f,.7f,1.85f),seed%3==0?a.CarPaintBurgundy:a.CarPaintBlack,false);
            Box("Cabin",g,new Vector3(.1f,1.12f,0),new Vector3(2.4f,.55f,1.65f),a.TowerCladding,false);
            foreach(float x in new[]{-1.4f,1.4f})foreach(float z in new[]{-.88f,.88f})
                Primitive(PrimitiveType.Sphere,"Wheel",g,new Vector3(x,.34f,z),new Vector3(.65f,.65f,.25f),a.Rubber,false);
        }
    }
}
