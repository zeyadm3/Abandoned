using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
namespace Abandoned.EditorTools
{
    public static class SkyscraperDowntownBuildings
    {
        public static void Tower(Transform city,SkyscraperArt a,SkyscraperDowntownMaterials p,Vector3 at,float height,int seed)
        {
            Transform g=Group($"CityTower_{seed}_{height:0}m",city);g.position=at;
            int style=seed%4;
            float width=32+seed%5*3,depth=34+seed%4*4;
            Box("CityBlockPodium",g,new Vector3(0,5,0),new Vector3(64,10,62),style%2==0?p.Limestone:a.GraniteDark);
            for(int side=-1;side<=1;side+=2)
            {
                Box("Shopfront",g,new Vector3(0,2.8f,side*31.05f),new Vector3(58,4.8f,.08f),p.Shop,false);
                for(int k=-3;k<=3;k++)
                    Box("ShopPier",g,new Vector3(k*9,3,side*31.2f),new Vector3(.5f,6,.4f),p.Limestone,false);
                Box("PodiumCornice",g,new Vector3(0,9.5f,side*31.3f),new Vector3(65,.8f,.7f),a.GraniteDark,false);
                Box("EntranceCanopy",g,new Vector3(0,4.5f,side*33),new Vector3(10,.25f,4),a.Steel,false);
            }
            float setback=style==1?height*.72f:height-16;
            Body(g,p.Facades[style],width,depth,10,setback-10,seed,0);
            Box("SetbackTerrace",g,new Vector3(0,setback,0),new Vector3(width,.22f,depth),a.GraniteDark,false);
            Body(g,p.Facades[style],width*.78f,depth*.78f,setback,height-setback,seed,1);
            // A single solid footprint prevents entering the decorative tower shell.
            var solid=g.gameObject.AddComponent<BoxCollider>();solid.center=new Vector3(0,height*.5f,0);solid.size=new Vector3(width,height,depth);
            Box("TowerRoof",g,new Vector3(0,height+.15f,0),new Vector3(width*.8f,.3f,depth*.8f),a.GraniteDark,false);
            foreach(float side in new[]{-1f,1f})
                Box("CornerFin",g,new Vector3(side*(width*.5f+.1f),setback*.5f,0),new Vector3(.35f,setback,.45f),style==1?a.Brass:a.Steel,false);
            if(style==2)
            {
                Box("CrownBlade",g,new Vector3(0,height+4,0),new Vector3(width*.5f,8,1),a.Steel,false);
                Box("Antenna",g,new Vector3(0,height+10,0),new Vector3(.15f,12,.15f),a.Steel,false);
            }
            else if(style==3)
                foreach(float x in new[]{-width*.25f,width*.25f})Box("CrownFin",g,new Vector3(x,height+2,0),new Vector3(.5f,4,depth*.8f),p.Limestone,false);
            foreach(float x in new[]{-6f,6f})
            {
                Box("RoofPlant",g,new Vector3(x,height+1.1f,3),new Vector3(4,2,6),a.Steel,false);
                Primitive(PrimitiveType.Cylinder,"CoolingFan",g,new Vector3(x,height+2.2f,3),new Vector3(2,.1f,2),a.Rubber,false);
            }
            Box("AviationBeacon",g,new Vector3(0,height+3,-depth*.3f),new Vector3(.25f,.3f,.25f),a.RedEmergencyEmissive,false);
        }
        private static void Body(Transform parent,Material material,float width,float depth,float y,float height,int seed,int tier)
        {
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();
            void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float span)
            {
                int n=vertices.Count;vertices.AddRange(new[]{a,b,c,d});
                float offset=seed%8;
                uv.AddRange(new[]{new Vector2(offset,0),new Vector2(offset+span/12,0),new Vector2(offset+span/12,height/32),new Vector2(offset,height/32)});
                indices.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});
            }
            float x=width*.5f,z=depth*.5f,t=y+height;
            Face(new Vector3(-x,y,-z),new Vector3(x,y,-z),new Vector3(x,t,-z),new Vector3(-x,t,-z),width);
            Face(new Vector3(x,y,z),new Vector3(-x,y,z),new Vector3(-x,t,z),new Vector3(x,t,z),width);
            Face(new Vector3(x,y,-z),new Vector3(x,y,z),new Vector3(x,t,z),new Vector3(x,t,-z),depth);
            Face(new Vector3(-x,y,z),new Vector3(-x,y,-z),new Vector3(-x,t,-z),new Vector3(-x,t,z),depth);
            var mesh=new Mesh{name=$"CityTower_{seed}_{tier}"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
            string path=SkyscraperDowntownMaterials.Folder+"/"+mesh.name+".asset";
            Mesh saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(saved==null){saved=mesh;AssetDatabase.CreateAsset(saved,path);}else{EditorUtility.CopySerialized(mesh,saved);Object.DestroyImmediate(mesh);}
            Transform g=Group("TowerFacade",parent);g.gameObject.AddComponent<MeshFilter>().sharedMesh=saved;
            g.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
    }
}
