using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L=Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    public static class SkyscraperRestaurantFinish
    {
        public static void Apply(Transform root,SkyscraperArt a)
        {
            // Preserve the material's normal weave/grain without multiplying ivory and emerald by a dark scan.
            a.Linen.SetTexture("_BaseMap",null);
            a.Linen.SetColor("_BaseColor",new Color(.92f,.87f,.76f));
            a.Velvet.SetTexture("_BaseMap",null);
            a.Velvet.SetColor("_BaseColor",new Color(.12f,.34f,.24f));
            EditorUtility.SetDirty(a.Linen);EditorUtility.SetDirty(a.Velvet);
            GeneratedMaterialRepair.Apply(a.LampEmissive);
            GeneratedMaterialRepair.Apply(a.RedEmergencyEmissive);
            Transform chandelier=root.Find("Lights/SignatureAtriumChandelier");
            if(chandelier!=null)
            {
                var fixture=Abandoned.EditorTools.SerializedWiring.GetOrAdd<LightFixture>(chandelier.gameObject);
                fixture.EditorSetup(null,chandelier.Find("LuminousHalo").GetComponent<Renderer>(),false,false,8401);
            }
            Transform old=root.Find("RestaurantAccentLighting");
            if(old!=null)Object.DestroyImmediate(old.gameObject);
            Transform g=Group("RestaurantAccentLighting",root);
            int index=0;
            foreach(float y in new[]{L.RestaurantY,L.UpperRestaurantY})
            foreach(float x in new[]{-7.7f,7.7f})
            foreach(float side in new[]{-1f,1f})
            {
                Transform s=Group("BrassPierSconce",g);s.position=new Vector3(x,y+2.35f,12+side*.29f);
                Box("BrassBackplate",s,Vector3.zero,new Vector3(.16f,.70f,.05f),a.Brass,false);
                var globe=Primitive(PrimitiveType.Sphere,"OpalSconce",s,new Vector3(0,0,side*.15f),new Vector3(.23f,.46f,.23f),a.LampEmissive,false);
                Accent(s,globe.GetComponent<Renderer>(),new Vector3(0,0,side*.36f),3.2f,6.5f,index++);
            }
            foreach(Vector3 at in new[]{new Vector3(-16.7f,410,10.3f),new Vector3(-16.7f,410,19.4f),
                new Vector3(16.8f,410,16.8f),new Vector3(3,410,3),new Vector3(10.5f,404,4.9f),new Vector3(16.6f,404,14.7f)})
            {
                Transform lamp=Group("SalonFloorLamp",g);lamp.position=at;
                Primitive(PrimitiveType.Cylinder,"WeightedBrassFoot",lamp,new Vector3(0,.04f,0),new Vector3(.34f,.04f,.34f),a.Brass,false);
                Primitive(PrimitiveType.Cylinder,"SlenderBrassStem",lamp,new Vector3(0,.95f,0),new Vector3(.035f,.9f,.035f),a.Brass,false);
                var globe=Primitive(PrimitiveType.Sphere,"OpalLampShade",lamp,new Vector3(0,1.9f,0),new Vector3(.48f,.42f,.48f),a.LampEmissive,false);
                Accent(lamp,globe.GetComponent<Renderer>(),new Vector3(0,1.9f,0),3f,7f,index++);
            }
            foreach(TextMesh label in root.GetComponentsInChildren<TextMesh>(true))
            {
                switch(label.name)
                {
                    case "Directions":label.text="DINING  /  GALLERY UPSTAIRS";label.characterSize=.035f;label.transform.localPosition=new Vector3(0,407.7f,-2.55f);break;
                    case "RestaurantIdentity":label.characterSize=.15f;break;
                    case "ServiceStandard":label.characterSize=.045f;break;
                    case "UpperDiningWayfinding":label.characterSize=.04f;break;
                    case "GalleryIdentity":label.characterSize=.04f;break;
                    case "ChefsTableIdentity":label.characterSize=.06f;break;
                    case "PrivateDiningIdentity":label.characterSize=.07f;break;
                }
            }
        }

        private static void Accent(Transform parent,Renderer diffuser,Vector3 at,float intensity,float range,int index)
        {
            Transform emitter=Group("WarmAccentEmitter",parent);emitter.localPosition=at;
            Light light=emitter.gameObject.AddComponent<Light>();light.type=LightType.Point;
            light.color=new Color(1,.78f,.48f);light.intensity=intensity;light.range=range;light.shadows=LightShadows.None;
            parent.gameObject.AddComponent<LightFixture>().EditorSetup(light,diffuser,false,false,8300+index);
        }

        public static void ApplyAndExit()=>ReleaseKit.RunAndExit(()=>
        {
            var scene=EditorSceneManager.OpenScene(L.ScenePath,OpenSceneMode.Single);
            Apply(GameObject.Find("Skyscraper").transform,new SkyscraperArt());
            AssetDatabase.SaveAssets();
            if(!EditorSceneManager.SaveScene(scene))throw new System.InvalidOperationException("Could not save Apex's finishing pass.");
        });
    }
}
