using Abandoned.Audio;
using Abandoned.EditorTools;
using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;

namespace Abandoned.Hospital
{
    public static class HospitalServices
    {
        public static HorrorConfig Build(Transform root,HospitalKit k,HospitalStructure floors)
        {
            string path=HospitalLoot.Data+"/HospitalHorror.asset";
            var config=SerializedWiring.LoadOrCreateAsset<HorrorConfig>(path);
            var so=new SerializedObject(config);var levels=so.FindProperty("levels");levels.arraySize=2;
            levels.GetArrayElementAtIndex(0).stringValue="Hospital";levels.GetArrayElementAtIndex(1).stringValue="World";
            so.FindProperty("<Building>k__BackingField").boundsValue=HospitalLayout.Bounds;
            so.FindProperty("<ArrivalFallback>k__BackingField").vector3Value=new Vector3(24,0,18);
            so.FindProperty("<ArrivalFigure>k__BackingField").vector3Value=new Vector3(24,4,32);
            so.FindProperty("<ArrivalDoor>k__BackingField").vector3Value=new Vector3(28,4,32);
            so.FindProperty("<RoarPoint>k__BackingField").vector3Value=new Vector3(24,6,36);
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(config);
            Transform services=HospitalKit.Group("Services",root);
            int id=0;
            for(int level=0;level<2;level++)
            {
                for(float z=4;z<40;z+=8)foreach(float x in new[]{8f,24f,40f})
                    Fixture(services,k,new Vector3(x,level*4+3.35f,z),id++,level==0?floors.At(new Vector3(x,4,z)):null);
                foreach(float x in new[]{3f,45f})foreach(float z in new[]{14f,38f})
                {
                    Vector3 at=new(x,level*4,z);
                    HospitalHiding.Build(services,k,at,x<24?90:-90,id++,floors.At(at));
                }
            }
            for(int sector=0;sector<6;sector++)Breaker(services,k,sector,new Vector3(29.1f+sector*1.25f,0,24.3f));
            var ambience=HospitalKit.Group("Ambience",root).gameObject.AddComponent<LevelAmbience>();
            ambience.EditorSetup(.14f,.12f,0,new Vector2(22,48));
            return config;
        }
        private static void Fixture(Transform p,HospitalKit k,Vector3 at,int id,StructuralSection ceiling)
        {
            Transform t=HospitalKit.Group("ClinicalFixture_"+id,p);t.localPosition=at;
            k.Box("Housing",t,Vector3.zero,new Vector3(1.3f,.12f,.5f),k.Metal,false);
            Renderer lens=k.Box("Diffuser",t,new Vector3(0,-.08f,0),new Vector3(1.18f,.04f,.4f),k.Glow,false).GetComponent<Renderer>();
            Transform aim=HospitalKit.Group("Light",t);aim.localPosition=Vector3.down*.13f;aim.localRotation=Quaternion.Euler(90,0,0);
            Light light=aim.gameObject.AddComponent<Light>();light.type=LightType.Spot;light.range=10;light.spotAngle=110;light.intensity=8;
            light.color=new Color(.79f,.94f,.84f);light.shadows=LightShadows.None;
            t.gameObject.AddComponent<LightFixture>().EditorSetup(light,lens,id%7==3,id%11==5,17000+id);
            k.Attach(t,ceiling);
        }
        private static void Breaker(Transform p,HospitalKit k,int sector,Vector3 at)
        {
            Transform t=HospitalKit.Group("Breaker_"+sector,p);t.localPosition=at;
            k.Box("Cabinet",t,new Vector3(0,1.4f,.12f),new Vector3(.7f,.9f,.25f),k.Metal);
            Transform handle=HospitalKit.Group("Lever",t);handle.localPosition=new Vector3(.2f,1.3f,.3f);
            k.Box("Grip",handle,new Vector3(0,.15f,0),new Vector3(.1f,.3f,.08f),k.Accent,false);
            Renderer lamp=k.Box("Indicator",t,new Vector3(-.2f,1.7f,-.02f),new Vector3(.07f,.07f,.04f),k.Glow,false).GetComponent<Renderer>();
            t.gameObject.AddComponent<BreakerPanel>().EditorSetup(sector,handle,lamp);
            k.Label(t,"CIRCUIT "+(sector+1),new Vector3(0,2,-.025f),size:.025f);
        }
    }
}
