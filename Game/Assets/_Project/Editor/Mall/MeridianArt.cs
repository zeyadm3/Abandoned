using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    public static class MeridianArt
    {
        public const string Folder="Assets/_Project/Art/Custom/Environment/Meridian";
        public static bool UseKit=>true;
        public static bool Dress=>false;
        public static bool Illuminate=>false;
        public static void Prepare() => MeridianMaterials.Prepare();
        public static Material Material(string key) => MeridianMaterials.Get(key);
        public static GameObject Model(string key) => AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder}/{key}.fbx");
        public static GameObject Place(string key,Transform parent,Vector3 position,float yaw=0,Vector3? scale=null)
            => Place(key,parent,position,Quaternion.Euler(0,yaw,0),scale);
        public static GameObject Place(string key,Transform parent,Vector3 position,Quaternion rotation,Vector3? scale=null)
        {
            var source=Model(key);
            if(source==null) throw new InvalidOperationException($"Meridian kit missing: {key}. Regenerate Tools/Blender/meridian.py.");
            var holder=new GameObject(key);
            holder.transform.SetParent(parent,false);
            holder.transform.SetLocalPositionAndRotation(position,rotation);
            holder.transform.localScale=scale??Vector3.one;
            var geometry=(GameObject)PrefabUtility.InstantiatePrefab(source,holder.transform);
            geometry.name="Geometry";
            // Unity recognises Blender's LOD names automatically; replace its imported groups with one owner.
            foreach(LODGroup imported in geometry.GetComponentsInChildren<LODGroup>(true))UnityEngine.Object.DestroyImmediate(imported);
            foreach(Collider c in geometry.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(c);
            foreach(Renderer r in geometry.GetComponentsInChildren<Renderer>(true))
                r.sharedMaterials=r.sharedMaterials.Select(m=>m!=null?Material(m.name):throw new InvalidOperationException($"Unbound Meridian slot {key}")).ToArray();
            AddLods(holder);
            return holder;
        }
        private static void AddLods(GameObject holder)
        {
            Transform[] groups=holder.GetComponentsInChildren<Transform>().Where(t=>t.name=="LOD0"||t.name=="LOD1"||t.name=="LOD2").ToArray();
            if(groups.Length!=3) return;
            var lod=holder.AddComponent<LODGroup>();
            lod.SetLODs(new[]{new LOD(.35f,groups.First(t=>t.name=="LOD0").GetComponentsInChildren<Renderer>()),
                new LOD(.12f,groups.First(t=>t.name=="LOD1").GetComponentsInChildren<Renderer>()),
                new LOD(.025f,groups.First(t=>t.name=="LOD2").GetComponentsInChildren<Renderer>())});
            lod.RecalculateBounds();
        }
        public static GameObject Fracture(string key)
        {
            string folder="Assets/_Project/Prefabs/Meridian";
            PolishAssets.EnsureFolder(folder);
            var root=Place(key,null,Vector3.zero);
            var prefab=PrefabUtility.SaveAsPrefabAsset(root,$"{folder}/{key}.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }
    }
}
