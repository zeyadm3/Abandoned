using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    public static class SkyscraperEntryFinish
    {
        public static void Apply(Transform root,SkyscraperArt a)
        {
            a.LobbyWalnut.SetColor("_BaseColor",new Color(.65f,.54f,.42f));EditorUtility.SetDirty(a.LobbyWalnut);
            Transform details=root.Find("Lobby/CorporateLobbyDetails");
            if(details!=null)foreach(Transform fixture in details)
            {
                if(fixture.name!="LobbyFeatureWallWash")continue;
                fixture.position=new Vector3(fixture.position.x,5.5f,-4.3f);
                Transform emitter=fixture.Find("WallWasher");emitter.localRotation=Quaternion.Euler(60,0,0);
                Light light=emitter.GetComponent<Light>();light.intensity=14;light.range=8;light.spotAngle=95;
                light.color=new Color(1,.91f,.77f);
            }
            Transform body=root.Find("ElevatorShaft/ElevatorCab/CabDeck/CabBody");
            if(body==null)return;
            Transform fill=body.Find("CabInteriorLight")??Group("CabInteriorLight",body);
            fill.localPosition=new Vector3(0,2.3f,0);
            Light lamp=fill.GetComponent<Light>();
            if(lamp==null)lamp=fill.gameObject.AddComponent<Light>();
            lamp.type=LightType.Point;lamp.color=new Color(1,.91f,.77f);lamp.intensity=3.5f;lamp.range=5;
            lamp.shadows=LightShadows.None;
            var fixtureBody=body.GetComponent<LightFixture>();
            fixtureBody.EditorSetup(lamp,body.Find("CabCeilingLuminousPanel").GetComponent<Renderer>(),false,false,9400);
            Transform directions=root.Find("RestaurantFloor_404m/SkyLobby/Directions");
            if(directions!=null)directions.localPosition=new Vector3(0,407.7f,-2.55f);
        }

        public static void ApplyAndExit()=>ReleaseKit.RunAndExit(()=>
        {
            var scene=EditorSceneManager.OpenScene(SkyscraperLayout.ScenePath,OpenSceneMode.Single);
            Apply(GameObject.Find("Skyscraper").transform,new SkyscraperArt());
            AssetDatabase.SaveAssets();
            if(!EditorSceneManager.SaveScene(scene))throw new System.InvalidOperationException("Could not save the Cloudline entry lighting.");
        });
    }
}
