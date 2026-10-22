using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Motel
{
    public static class MotelHiding
    {
        public static void Build(Transform parent,MotelKit k,Vector3 at,float yaw,int index,StructuralSection section)
        {
            Transform t=MotelKit.Group("GuestWardrobe_"+index,parent);t.localPosition=at;t.localRotation=Quaternion.Euler(0,yaw,0);
            k.Box("Back",t,new Vector3(0,1.15f,0),new Vector3(1.25f,2.3f,.06f),k.Wood,false);
            foreach(float x in new[]{-.6f,.6f})k.Box("Side",t,new Vector3(x,1.15f,.5f),new Vector3(.06f,2.3f,1),k.Wood,false);
            k.Box("Top",t,new Vector3(0,2.27f,.5f),new Vector3(1.25f,.06f,1),k.Wood,false);
            Transform door=MotelKit.Group("Door",t);door.localPosition=new Vector3(-.6f,0,1);
            k.Box("Lower",door,new Vector3(.6f,.68f,0),new Vector3(1.18f,1.36f,.045f),k.Wood,false);
            k.Box("Upper",door,new Vector3(.6f,2.12f,0),new Vector3(1.18f,.3f,.045f),k.Wood,false);
            for(int i=0;i<6;i++)k.Box("Louvre",door,new Vector3(.6f,1.42f+i*.09f,0),new Vector3(1.18f,.035f,.05f),k.Metal,false);
            k.Box("Handle",door,new Vector3(1.05f,1,-.04f),new Vector3(.04f,.2f,.06f),k.Metal,false);
            k.Box("Pilot",t,new Vector3(0,2.2f,1.04f),new Vector3(.1f,.04f,.02f),k.Glow,false);
            var solid=t.gameObject.AddComponent<BoxCollider>();solid.center=new Vector3(0,1.15f,.5f);solid.size=new Vector3(1.25f,2.3f,1);
            var obstacle=t.gameObject.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;
            obstacle.center=solid.center;obstacle.size=solid.size;obstacle.carving=true;
            Transform inside=MotelKit.Group("Inside",t);inside.localPosition=new Vector3(0,.03f,.5f);
            Transform exit=MotelKit.Group("Exit",t);exit.localPosition=new Vector3(0,.05f,1.9f);
            t.gameObject.AddComponent<HidingLocker>().EditorSetup(index,door,inside,exit,section,HideSpotKind.Closet);
            t.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);k.Attach(t,section);
        }
    }
}
