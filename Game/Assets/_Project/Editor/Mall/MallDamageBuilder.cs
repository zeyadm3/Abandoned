using System.Collections.Generic;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    public static class MallDamageBuilder
    {
        private static readonly Dictionary<string,GameObject> Fractures=new();
        public static void Reset()=>Fractures.Clear();
        public static void Floor(StructureSceneSetup setup,Transform tile,SectionType type,bool collapsible,
            string finish,float health=1,float capacity=1)
        {
            Transform visual=tile.Find("Visual"),stable=Group("Stable",visual);
            foreach(Transform child in new List<Transform>(Children(visual)))if(child!=stable)child.SetParent(stable,false);
            Vector3 size=tile.GetComponent<BoxCollider>().size,scale=new(size.x/4,1,size.z/4);
            Transform cracking=MeridianArt.Place("Floor"+finish+"Cracked",visual,Vector3.zero,0,scale).transform;
            Transform failing=MeridianArt.Place("Floor"+finish+"Sagged",visual,Vector3.zero,0,scale).transform;
            var stages=tile.gameObject.AddComponent<StructureStageVisuals>();stages.EditorSetup(stable,cracking,failing);
            setup.AddSection(tile,type,collapsible,health,capacity,fracturedPrefab:Fracture("Floor"+finish+"Fractured",scale),
                stageVisuals:stages,preserveFractureMaterials:true);
        }
        public static void Flight(StructureSceneSetup setup,Transform flight,MallLayout.Flight data)
        {
            Transform visual=flight.Find("Visual");
            // Physics stays active as the intact art changes warning stages.
            foreach(Transform child in new List<Transform>(Children(visual)))if(child.GetComponent<Collider>()!=null)child.SetParent(flight,true);
            string key=data.Kind==MallLayout.FlightKind.HaulRamp?"HaulRamp":data.Kind==MallLayout.FlightKind.Stair?"ServiceStair":"Escalator";
            Transform stable=visual.Find(key);
            Transform cracking=MeridianArt.Place(key,visual,Vector3.zero).transform;
            Transform failing=MeridianArt.Place(key,visual,Vector3.down*.08f).transform;
            cracking.name="Cracking";failing.name="Failing";
            WarningLines(cracking,data,.025f);WarningLines(failing,data,.05f);
            var stages=flight.gameObject.AddComponent<StructureStageVisuals>();stages.EditorSetup(stable,cracking,failing);
            setup.AddSection(flight,SectionType.Stair,data.CanCollapse,fractured:false,fracturedPrefab:Fracture(key+"Fractured",Vector3.one),
                stageVisuals:stages,preserveFractureMaterials:true);
        }
        private static void WarningLines(Transform group,MallLayout.Flight flight,float width)
        {
            for(int i=1;i<=3;i++) {
                float z=flight.Length*i/4,y=flight.Rise*z/flight.Length+(flight.Kind==MallLayout.FlightKind.HaulRamp?.046f:.12f);
                var crack=Box("SplitTread",group,new Vector3(0,y,z),new Vector3(flight.Width*.85f,.008f,width),MeridianArt.Material("Crack"),false);
                crack.transform.localRotation=Quaternion.Euler(-Mathf.Atan2(flight.Rise,flight.Length)*Mathf.Rad2Deg,0,8*i);
            }
        }
        private static IEnumerable<Transform> Children(Transform group){foreach(Transform child in group)yield return child;}
        private static GameObject Fracture(string key,Vector3 scale)
        {
            string name=string.Format(System.Globalization.CultureInfo.InvariantCulture,"{0}_{1:0.00}_{2:0.00}",key,scale.x,scale.z);
            if(Fractures.TryGetValue(name,out GameObject cached))return cached;
            string folder="Assets/_Project/Prefabs/Meridian";PolishAssets.EnsureFolder(folder);
            var root=new GameObject(name);MeridianArt.Place(key,root.transform,Vector3.zero,0,scale);
            GameObject prefab=PrefabUtility.SaveAsPrefabAsset(root,$"{folder}/{name}.prefab");Object.DestroyImmediate(root);
            Fractures[name]=prefab;return prefab;
        }
    }
}
