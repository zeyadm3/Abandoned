using Abandoned.Audio;
using Abandoned.EditorTools;
using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;

namespace Abandoned.Motel
{
    public static class MotelServices
    {
        public static HorrorConfig Build(Transform root,MotelKit k,MotelStructure floors)
        {
            var config=SerializedWiring.LoadOrCreateAsset<HorrorConfig>(MotelLoot.Data+"/MotelHorror.asset");
            var so=new SerializedObject(config);var levels=so.FindProperty("levels");levels.arraySize=2;
            levels.GetArrayElementAtIndex(0).stringValue="Motel";levels.GetArrayElementAtIndex(1).stringValue="World";
            so.FindProperty("<Building>k__BackingField").boundsValue=MotelLayout.Bounds;
            so.FindProperty("<ArrivalFallback>k__BackingField").vector3Value=new Vector3(32,0,18);
            so.FindProperty("<ArrivalFigure>k__BackingField").vector3Value=new Vector3(52,4,14);
            so.FindProperty("<ArrivalDoor>k__BackingField").vector3Value=new Vector3(60.5f,4,16);
            so.FindProperty("<RoarPoint>k__BackingField").vector3Value=new Vector3(32,6,22);
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(config);
            Transform services=MotelKit.Group("Services",root);int id=0;
            for(int level=0;level<2;level++)
            {
                for(int room=0;room<8;room++)
                {
                    float x=MotelLayout.RoomX(room)+3.5f;
                    foreach(float z in new[]{20f,26f})Fixture(services,k,new Vector3(x,level*4+3.3f,z),id++,level==0?floors.At(new Vector3(x,4,z)):null,false);
                }
                for(float x=4;x<64;x+=8)Fixture(services,k,new Vector3(x,level*4+3.25f,14),id++,level==0?floors.At(new Vector3(x,4,14)):null,true);
            }
            foreach(float x in new[]{30f,34f})foreach(float z in new[]{19f,25f})Fixture(services,k,new Vector3(x,3.3f,z),id++,null,false);
            for(int s=0;s<6;s++)Breaker(services,k,s,new Vector3(s<3?28.4f+s*.8f:34+(s-3)*.8f,0,15.65f));
            var ambience=MotelKit.Group("Ambience",root).gameObject.AddComponent<LevelAmbience>();
            ambience.EditorSetup(.24f,.07f,0,new Vector2(26,58));
            return config;
        }
        private static void Fixture(Transform p,MotelKit k,Vector3 at,int id,StructuralSection ceiling,bool gallery)
        {
            Transform t=MotelKit.Group("MotelFixture_"+id,p);t.localPosition=at;
            k.Box("Housing",t,Vector3.zero,new Vector3(gallery? .6f:.8f,.1f,.35f),k.Metal,false);
            Renderer lens=k.Box("Diffuser",t,new Vector3(0,-.07f,0),new Vector3(gallery? .5f:.7f,.04f,.28f),k.Glow,false).GetComponent<Renderer>();
            Transform aim=MotelKit.Group("Light",t);aim.localRotation=Quaternion.Euler(90,0,0);aim.localPosition=Vector3.down*.12f;
            Light light=aim.gameObject.AddComponent<Light>();light.type=LightType.Spot;light.range=gallery?10:8;light.spotAngle=105;light.intensity=gallery?7:6;
            light.color=gallery?new Color(1,.65f,.35f):new Color(1,.83f,.61f);light.shadows=id==20||id==44?LightShadows.Soft:LightShadows.None;
            t.gameObject.AddComponent<LightFixture>().EditorSetup(light,lens,id%9==4,id%13==6,17500+id);k.Attach(t,ceiling);
        }
        private static void Breaker(Transform p,MotelKit k,int sector,Vector3 at)
        {
            Transform t=MotelKit.Group("Circuit_"+sector,p);t.localPosition=at;
            k.Box("Cabinet",t,new Vector3(0,1.4f,.12f),new Vector3(.6f,.8f,.25f),k.Metal);
            Transform handle=MotelKit.Group("Lever",t);handle.localPosition=new Vector3(.16f,1.3f,-.06f);
            k.Box("Grip",handle,new Vector3(0,.14f,0),new Vector3(.08f,.28f,.08f),k.Accent,false);
            Renderer lamp=k.Box("Indicator",t,new Vector3(-.16f,1.65f,-.03f),new Vector3(.06f,.06f,.04f),k.Glow,false).GetComponent<Renderer>();
            t.gameObject.AddComponent<BreakerPanel>().EditorSetup(sector,handle,lamp);
            k.Label(t,"C"+(sector+1),new Vector3(0,1.94f,-.03f),size:.025f);
        }
    }
}
