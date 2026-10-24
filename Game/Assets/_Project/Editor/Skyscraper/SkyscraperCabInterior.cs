using Abandoned.Skyscraper;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    public static class SkyscraperCabInterior
    {
        public static void Build(Transform body,SkyscraperArt a)
        {
            foreach(float x in new[]{-.79f,.79f})foreach(float z in new[]{-.79f,.79f})
                Box("CabBookmatchedStoneFloor",body,new Vector3(x,.012f,z),new Vector3(1.55f,.02f,1.55f),a.Marble,false);
            foreach(float x in new[]{-1.52f,1.52f})
            {
                Box("CabWalnutSideLining",body,new Vector3(x,1.4f,.1f),new Vector3(.045f,2.65f,2.95f),a.LobbyWalnut,false);
                for(float z=-1.2f;z<=1.4f;z+=.65f)
                    Box("CabPanelSteelReveal",body,new Vector3(x-.035f*Mathf.Sign(x),1.4f,z),new Vector3(.025f,2.6f,.018f),a.LiftSteel,false);
            }
            Box("CabWalnutBackLining",body,new Vector3(0,1.4f,1.56f),new Vector3(3.03f,2.65f,.04f),a.LobbyWalnut,false);
            Box("CabBackMirrorFrame",body,new Vector3(0,1.85f,1.515f),new Vector3(2.6f,1.1f,.045f),a.LiftSteel,false);
            Box("CabBackMirror",body,new Vector3(0,1.85f,1.484f),new Vector3(2.48f,1.0f,.02f),a.Glass,false);
            var rail=Primitive(PrimitiveType.Cylinder,"CabBackHandrail",body,new Vector3(0,.98f,1.38f),new Vector3(.035f,1.38f,.035f),a.LiftSteel,false);
            rail.transform.localRotation=Quaternion.Euler(0,0,90);
            var sideRail=Primitive(PrimitiveType.Cylinder,"CabSideHandrail",body,new Vector3(-1.39f,.98f,0),new Vector3(.035f,1.35f,.035f),a.LiftSteel,false);
            sideRail.transform.localRotation=Quaternion.Euler(90,0,0);
            foreach(float x in new[]{-1.54f,1.54f})
                Box("CabSteelSkirting",body,new Vector3(x,.12f,0),new Vector3(.06f,.22f,3.1f),a.LiftSteel,false);
            Box("CabCeilingRecess",body,new Vector3(0,2.73f,0),new Vector3(3.1f,.06f,3.1f),a.GraniteDark,false);
            var diffuser=Box("CabCeilingLuminousPanel",body,new Vector3(0,2.69f,0),new Vector3(2.5f,.025f,1.5f),a.LampEmissive,false);
            body.gameObject.AddComponent<LightFixture>().EditorSetup(null,diffuser.GetComponent<Renderer>(),false,false,9400);
            Label(body,"CabIdentity","C L O U D L I N E",new Vector3(0,2.52f,1.48f),Quaternion.identity,.035f,new Color(.84f,.80f,.69f));
        }

        public static SkyscraperElevatorDoor Door(Transform parent,SkyscraperArt a,string name,Vector3 at,bool open)
        {
            Transform door=Group(name,parent);door.localPosition=at;
            Transform left=Box("LeftLeaf",door,new Vector3(-.55f,1.3f,0),new Vector3(1.1f,2.6f,.08f),a.LiftSteel).transform;
            Transform right=Box("RightLeaf",door,new Vector3(.55f,1.3f,0),new Vector3(1.1f,2.6f,.08f),a.LiftSteel).transform;
            foreach(Transform leaf in new[]{left,right})
            {
                // The seams move with each leaf; nothing remains across the open aperture.
                Box("DoorLeafShadowReveal",leaf,new Vector3(0,0,-.53f),new Vector3(.018f,.96f,.04f),a.GraniteDark,false);
            }
            var result=door.gameObject.AddComponent<SkyscraperElevatorDoor>();result.EditorSetup(left,right,open);
            return result;
        }
    }
}
