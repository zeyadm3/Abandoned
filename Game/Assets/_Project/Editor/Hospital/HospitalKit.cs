using System.Collections.Generic;
using System.IO;
using Abandoned.Core;
using Abandoned.EditorTools;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;

namespace Abandoned.Hospital
{
    // All generated assets stay in the POI's owned art folder; scans are referenced, never reimported.
    public sealed class HospitalKit
    {
        public const string Art = "Assets/_Project/Art/Hospital";
        private readonly Dictionary<string, Mesh> meshes = new();
        public readonly Material Wall, Floor, Metal, Dark, Cloth, Wood, Accent, Glass, Glow, Asphalt;
        public HospitalKit()
        {
            Ensure(Art);
            Wall = Material("Plaster", new Color(.62f,.68f,.63f), "PaintedPlaster017");
            Floor = Material("Terrazzo", new Color(.65f,.67f,.62f), "Terrazzo018");
            Metal = Material("Steel", new Color(.46f,.51f,.5f), "Metal049A", .55f);
            Dark = Material("Rubber", new Color(.055f,.065f,.06f), "Rubber004");
            Cloth = Material("Linen", new Color(.53f,.62f,.56f), "Fabric030");
            Wood = Material("Wood", new Color(.31f,.24f,.17f), "WoodFloor051");
            Accent = Material("Wayfinding", new Color(.14f,.34f,.3f), "PaintedMetal012");
            Glass = Material("OpaqueGlass", new Color(.14f,.25f,.27f), "Plastic010", .8f);
            Glow = Material("Diffuser", new Color(.77f,.88f,.8f), null);
            Glow.EnableKeyword("_EMISSION"); Glow.SetColor("_EmissionColor", new Color(1.8f,2.2f,1.9f));
            Asphalt = Material("Asphalt", new Color(.27f,.28f,.26f), "Asphalt031");
        }
        public static void Ensure(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            Ensure(Path.GetDirectoryName(path).Replace('\\','/'));
            AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\','/'), Path.GetFileName(path));
        }
        private static Material Material(string name, Color tint, string scan, float smooth = .2f)
        {
            string path = Art + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            m = new Material(Shader.Find("Abandoned/Surface") ?? Shader.Find("Universal Render Pipeline/Lit"));
            m.name = "Hospital_" + name; m.SetColor("_BaseColor", tint); m.SetFloat("_Smoothness", smooth);
            if (scan != null)
            {
                string dir = "Assets/_Project/Art/Scanned/" + scan;
                string[] files = Directory.GetFiles(dir, "*_Color.jpg");
                if (files.Length > 0) m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(files[0]));
                files = Directory.GetFiles(dir, "*_NormalGL.jpg");
                if (files.Length > 0)
                {
                    m.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(files[0]));
                    m.EnableKeyword("_NORMALMAP");
                }
            }
            AssetDatabase.CreateAsset(m, path); return m;
        }
        public static Transform Group(string name, Transform parent) => GreyboxFactory.Group(name, parent);
        public GameObject Box(string name, Transform parent, Vector3 at, Vector3 size, Material mat, bool solid = true)
        {
            GameObject go = GreyboxFactory.Box(name, parent, at, size, mat, solid);
            // Per-face metre UVs prevent long walls and tiny fittings stretching the same texture.
            string key = FormattableString.Invariant($"{size.x:0.000}_{size.y:0.000}_{size.z:0.000}");
            if (!meshes.TryGetValue(key, out Mesh mesh))
            {
                Ensure(Art + "/Meshes"); string path = Art + "/Meshes/Box_" + key + ".asset";
                mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh == null)
                {
                    mesh = Object.Instantiate(go.GetComponent<MeshFilter>().sharedMesh); mesh.name = key;
                    Vector3[] v = mesh.vertices, n = mesh.normals; var uv = new Vector2[v.Length];
                    for (int i=0;i<v.Length;i++)
                    {
                        Vector3 q = Vector3.Scale(v[i], size);
                        uv[i] = Mathf.Abs(n[i].y) > .5f ? new Vector2(q.x,q.z) :
                            Mathf.Abs(n[i].x) > .5f ? new Vector2(q.z,q.y) : new Vector2(q.x,q.y);
                    }
                    mesh.uv = uv; AssetDatabase.CreateAsset(mesh,path);
                }
                meshes[key] = mesh;
            }
            go.GetComponent<MeshFilter>().sharedMesh = mesh; return go;
        }
        public void Label(Transform parent, string text, Vector3 at, float yaw = 0f, float size = .065f)
        {
            GreyboxFactory.Label(parent,"Sign_"+text,text,at,Quaternion.Euler(0,yaw,0),size,new Color(.82f,.88f,.79f));
        }
        public void Rail(Transform parent, Vector3 a, Vector3 b)
        {
            Vector3 d=b-a;
            Transform rail=Group("Guard",parent); rail.localPosition=(a+b)*.5f;
            rail.localRotation=Quaternion.LookRotation(d);
            Box("Top",rail,new Vector3(0,1.08f,0),new Vector3(.06f,.06f,d.magnitude),Metal,false);
            Box("Kick",rail,new Vector3(0,.12f,0),new Vector3(.06f,.2f,d.magnitude),Metal,false);
            var c=rail.gameObject.AddComponent<BoxCollider>(); c.center=new Vector3(0,.55f,0); c.size=new Vector3(.08f,1.1f,d.magnitude);
            for(float z=-d.magnitude*.5f;z<=d.magnitude*.5f;z+=1.5f)
                Box("Post",rail,new Vector3(0,.55f,z),new Vector3(.045f,1.1f,.045f),Metal,false);
        }
        public void Attach(Transform prop, StructuralSection section)
        {
            if(section!=null) prop.gameObject.AddComponent<SectionProp>().EditorSetup(section);
        }
    }
}
