using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    public static class SkyscraperLobbyDetails
    {
        public static void Build(Transform lobby,SkyscraperArt a)
        {
            Transform g=Group("CorporateLobbyDetails",lobby);
            LiftBank(g,a);
            foreach(float z in new[]{10f,17f})
            {
                Sofa(g,a,new Vector3(12.5f,0,z),90);
                Sofa(g,a,new Vector3(-4.8f,0,z),-90);
                CoffeeTable(g,a,new Vector3(10.5f,0,z));
                CoffeeTable(g,a,new Vector3(-2.8f,0,z));
            }
            foreach(Vector3 at in new[]{new Vector3(-16.6f,0,9),new Vector3(-16.6f,0,20),new Vector3(16.6f,0,20)})
                SkyscraperFineDining.Planter(g,a,at);
            Box("GalleryStonePlinth",g,new Vector3(6,.45f,18.8f),new Vector3(.85f,.9f,.85f),a.Marble);
            Transform sculpture=Group("CorporateBronzeSculpture",g);sculpture.position=new Vector3(6,1.65f,18.8f);sculpture.rotation=Quaternion.Euler(75,20,0);
            SkyscraperFineDining.Halo(sculpture,"BronzeOrbit",Vector3.zero,.65f,a.Brass);
            SkyscraperFineDining.Halo(sculpture,"InnerOrbit",new Vector3(0,.16f,0),.43f,a.LiftSteel);
            Lighting(g,a);
        }

        private static void LiftBank(Transform g,SkyscraperArt a)
        {
            // Discrete solid bays flank one real portal; no decorative wall spans the operational doorway.
            foreach(float x in new[]{-5.2f,5.2f})
            {
                Transform bay=Group("PrivateLiftBay",g);bay.position=new Vector3(x,0,-2.65f);
                Box("StonePierL",bay,new Vector3(-1.4f,2.8f,0),new Vector3(.65f,5.6f,.2f),a.Marble);
                Box("StonePierR",bay,new Vector3(1.4f,2.8f,0),new Vector3(.65f,5.6f,.2f),a.Marble);
                Box("StoneHeader",bay,new Vector3(0,4.3f,0),new Vector3(2.25f,2.6f,.2f),a.Marble);
                Box("BrushedMetalDoors",bay,new Vector3(0,1.45f,-.035f),new Vector3(2.1f,2.9f,.1f),a.LiftSteel);
                Box("DoorCenterJoint",bay,new Vector3(0,1.45f,-.095f),new Vector3(.018f,2.9f,.01f),a.GraniteDark,false);
                Label(bay,"PrivateLiftMark","PRIVATE",new Vector3(0,3.35f,-.12f),Quaternion.identity,.026f,new Color(.4f,.39f,.35f));
                foreach(float edge in new[]{-1.12f,1.12f})
                    Box("ChampagneDoorReveal",bay,new Vector3(edge,1.5f,-.09f),new Vector3(.055f,3,.04f),a.Brass,false);
            }
            foreach(float x in new[]{-2.7f,2.7f})
                Box("CentralWalnutLiftPier",g,new Vector3(x,2.8f,-2.65f),new Vector3(1.7f,5.6f,.2f),a.LobbyWalnut);
            Box("CentralStoneLiftHeader",g,new Vector3(0,4.45f,-2.65f),new Vector3(3.7f,2.3f,.2f),a.Marble);
            Label(g,"LobbyLiftIdentity","A P E X",new Vector3(0,4.1f,-2.78f),Quaternion.identity,.065f,new Color(.45f,.42f,.33f));
            Label(g,"LobbyStairDirection","STAIRS",new Vector3(-7.5f,2.8f,-3.5f),Quaternion.Euler(0,-90,0),.03f,Color.white);
        }

        private static void Sofa(Transform p,SkyscraperArt a,Vector3 at,float angle)
        {
            Transform g=Group("ExecutiveLobbySofa",p);g.position=at;g.rotation=Quaternion.Euler(0,angle,0);
            Box("WalnutFloatingBase",g,new Vector3(0,.19f,0),new Vector3(3.1f,.25f,.9f),a.LobbyWalnut);
            Box("UpholsteredBack",g,new Vector3(0,.75f,.35f),new Vector3(3.1f,.8f,.2f),a.Linen);
            foreach(float x in new[]{-1.02f,0,1.02f})
                Box("IvorySeatCushion",g,new Vector3(x,.43f,-.04f),new Vector3(.99f,.22f,.8f),a.Linen,false);
            foreach(float x in new[]{-1.58f,1.58f})
                Box("SofaArm",g,new Vector3(x,.6f,0),new Vector3(.14f,.5f,.95f),a.Linen);
        }

        private static void CoffeeTable(Transform p,SkyscraperArt a,Vector3 at)
        {
            Transform g=Group("ExecutiveCoffeeTable",p);g.position=at;
            Box("StoneTop",g,new Vector3(0,.42f,0),new Vector3(1.25f,.08f,1.8f),a.GraniteDark);
            foreach(float z in new[]{-.65f,.65f})Box("BrushedSteelLeg",g,new Vector3(0,.2f,z),new Vector3(.9f,.4f,.05f),a.LiftSteel,false);
            Box("ArchitectureJournal",g,new Vector3(.12f,.476f,.2f),new Vector3(.32f,.026f,.4f),a.Porcelain,false);
            Box("JournalCoverLine",g,new Vector3(.12f,.492f,.15f),new Vector3(.25f,.005f,.035f),a.Brass,false);
        }

        private static void Lighting(Transform g,SkyscraperArt a)
        {
            int index=0;
            foreach(float z in new[]{-6f,7f,17f})foreach(float x in new[]{-4.5f,4.5f,13f})
            {
                Transform fixture=Group("LobbyRecessedLinear",g);fixture.position=new Vector3(x,5.52f,z);
                Box("RecessedCeilingSlot",fixture,Vector3.zero,new Vector3(2.6f,.08f,.25f),a.GraniteDark,false);
                var panel=Box("WarmLinearDiffuser",fixture,new Vector3(0,-.05f,0),new Vector3(2.4f,.025f,.14f),a.LampEmissive,false);
                Transform emitter=Group("Downlight",fixture);emitter.localPosition=Vector3.down*.10f;emitter.localRotation=Quaternion.Euler(90,0,0);
                Light light=emitter.gameObject.AddComponent<Light>();light.type=LightType.Spot;light.color=new Color(1,.91f,.76f);
                light.intensity=28;light.range=12;light.spotAngle=105;light.innerSpotAngle=55;light.shadows=LightShadows.None;
                fixture.gameObject.AddComponent<LightFixture>().EditorSetup(light,panel.GetComponent<Renderer>(),false,false,9100+index++);
            }
            foreach(float x in new[]{-5.2f,0f,5.2f,10f,14f})
            {
                Transform fixture=Group("LobbyFeatureWallWash",g);fixture.position=new Vector3(x,4.5f,-3.1f);
                var panel=Box("WallWashSlot",fixture,Vector3.zero,new Vector3(.6f,.05f,.12f),a.LampEmissive,false);
                Transform emitter=Group("WallWasher",fixture);emitter.localRotation=Quaternion.Euler(48,0,0);
                Light light=emitter.gameObject.AddComponent<Light>();light.type=LightType.Spot;light.color=new Color(1,.85f,.65f);
                light.intensity=10;light.range=7;light.spotAngle=100;light.shadows=LightShadows.None;
                fixture.gameObject.AddComponent<LightFixture>().EditorSetup(light,panel.GetComponent<Renderer>(),false,false,9100+index++);
            }
        }
    }
}
