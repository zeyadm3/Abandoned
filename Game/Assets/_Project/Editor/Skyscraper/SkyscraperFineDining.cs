using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    // Small reusable details establish the restaurant's scale and service standard.
    public static class SkyscraperFineDining
    {
        public static void Table(Transform parent, SkyscraperArt a, Vector3 at, bool dressed)
        {
            Transform g=Group("DiningTable",parent);g.position=at;
            Primitive(PrimitiveType.Cylinder,"PedestalFoot",g,new Vector3(0,.04f,0),new Vector3(.7f,.04f,.7f),a.Brass);
            Primitive(PrimitiveType.Cylinder,"FlutedPedestal",g,new Vector3(0,.41f,0),new Vector3(.22f,.35f,.22f),a.Mahogany);
            Primitive(PrimitiveType.Cylinder,"StoneTop",g,new Vector3(0,.8f,0),new Vector3(1.9f,.045f,1.9f),a.Marble);
            if(dressed)
            {
                Primitive(PrimitiveType.Cylinder,"IvoryLinenDrape",g,new Vector3(0,.73f,0),new Vector3(1.94f,.13f,1.94f),a.Linen,false);
                for(int i=0;i<4;i++)
                {
                    float angle=i*90f;
                    Transform place=Group("TastingMenuPlaceSetting",g);
                    place.localPosition=Quaternion.Euler(0,angle,0)*new Vector3(0,.873f,.62f);
                    place.localRotation=Quaternion.Euler(0,angle,0);
                    PlaceSetting(place,a);
                }
                Primitive(PrimitiveType.Cylinder,"BudVase",g,new Vector3(.17f,.96f,.02f),new Vector3(.08f,.10f,.08f),a.Brass,false);
                Stem(g,a,new Vector3(.17f,1.09f,.02f));
                Candle(g,a,new Vector3(-.19f,.87f,0));
            }
            for(int i=0;i<4;i++)
                Chair(g,a,Quaternion.Euler(0,i*90f,0)*new Vector3(0,0,1.34f),i*90f);
        }

        public static void PlaceSetting(Transform p,SkyscraperArt a)
        {
            Primitive(PrimitiveType.Cylinder,"GoldCharger",p,Vector3.zero,new Vector3(.34f,.009f,.34f),a.Brass,false);
            Primitive(PrimitiveType.Cylinder,"BoneChina",p,new Vector3(0,.014f,0),new Vector3(.30f,.008f,.30f),a.Porcelain,false);
            Primitive(PrimitiveType.Cylinder,"PlateWell",p,new Vector3(0,.024f,0),new Vector3(.20f,.003f,.20f),a.Linen,false);
            var napkin=Box("FoldedLinen",p,new Vector3(0,.05f,.015f),new Vector3(.095f,.035f,.16f),a.Linen,false);
            napkin.transform.localRotation=Quaternion.Euler(0,18,0);
            Box("NapkinRing",p,new Vector3(0,.073f,.015f),new Vector3(.10f,.012f,.025f),a.Brass,false);
            foreach(float x in new[]{-.22f,.22f})
            {
                Box(x<0?"ForkHandle":"KnifeHandle",p,new Vector3(x,.012f,.015f),new Vector3(.016f,.012f,.19f),a.Brass,false);
                Box(x<0?"ForkHead":"KnifeBlade",p,new Vector3(x,.014f,-.10f),new Vector3(.026f,.009f,.07f),a.Steel,false);
            }
            Glass(p,a,new Vector3(.22f,.01f,-.23f),1);
            Glass(p,a,new Vector3(.32f,.01f,-.16f),.8f);
            Transform menu=Group("PrintedTastingMenu",p);menu.localPosition=new Vector3(-.32f,.005f,-.12f);
            Box("MenuCard",menu,Vector3.zero,new Vector3(.12f,.008f,.20f),a.Porcelain,false);
            for(int i=0;i<4;i++) Box("MenuTypesetting",menu,new Vector3(0,.005f,-.06f+i*.035f),new Vector3(i==0?.07f:.05f,.002f,.004f),a.Brass,false);
        }

        public static void Chair(Transform p,SkyscraperArt a,Vector3 at,float angle)
        {
            Transform c=Group("EmeraldDiningChair",p);c.localPosition=at;c.localRotation=Quaternion.Euler(0,angle,0);
            Primitive(PrimitiveType.Sphere,"VelvetSeat",c,new Vector3(0,.48f,0),new Vector3(.64f,.18f,.61f),a.Velvet,false);
            Primitive(PrimitiveType.Sphere,"SculptedVelvetBack",c,new Vector3(0,.86f,.27f),new Vector3(.65f,.70f,.16f),a.Velvet,false);
            var collider=c.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(0,.51f,.05f);collider.size=new Vector3(.6f,1.02f,.65f);
            foreach(float x in new[]{-.23f,.23f}) foreach(float z in new[]{-.21f,.21f})
                Primitive(PrimitiveType.Cylinder,"TaperedBrassLeg",c,new Vector3(x,.23f,z),new Vector3(.035f,.23f,.035f),a.Brass,false);
            foreach(float x in new[]{-.25f,.25f})
                Box("BackFrame",c,new Vector3(x,.73f,.29f),new Vector3(.025f,.54f,.025f),a.Brass,false);
        }

        private static void Glass(Transform p,SkyscraperArt a,Vector3 at,float scale)
        {
            Transform g=Group("CrystalStemware",p);g.localPosition=at;g.localScale=Vector3.one*scale;
            Primitive(PrimitiveType.Cylinder,"Foot",g,Vector3.zero,new Vector3(.075f,.004f,.075f),a.Glass,false);
            Primitive(PrimitiveType.Cylinder,"Stem",g,new Vector3(0,.055f,0),new Vector3(.008f,.05f,.008f),a.Glass,false);
            Primitive(PrimitiveType.Sphere,"CrystalBowl",g,new Vector3(0,.14f,0),new Vector3(.085f,.105f,.085f),a.Glass,false);
        }

        public static void Candle(Transform p,SkyscraperArt a,Vector3 at)
        {
            Transform g=Group("TableCandle",p);g.localPosition=at;
            Primitive(PrimitiveType.Cylinder,"BrassHolder",g,new Vector3(0,.02f,0),new Vector3(.09f,.02f,.09f),a.Brass,false);
            Primitive(PrimitiveType.Cylinder,"IvoryWax",g,new Vector3(0,.075f,0),new Vector3(.045f,.05f,.045f),a.Porcelain,false);
            Primitive(PrimitiveType.Sphere,"Flame",g,new Vector3(0,.14f,0),new Vector3(.018f,.038f,.018f),a.LampEmissive,false);
        }

        private static void Stem(Transform p,SkyscraperArt a,Vector3 at)
        {
            Box("FlowerStem",p,at,new Vector3(.008f,.20f,.008f),a.Velvet,false);
            for(int i=0;i<5;i++)
            {
                float t=i*Mathf.PI*2/5;
                Primitive(PrimitiveType.Sphere,"IvoryPetal",p,at+new Vector3(Mathf.Sin(t)*.035f,.11f,Mathf.Cos(t)*.035f),new Vector3(.06f,.035f,.06f),a.Porcelain,false);
            }
        }

        public static void Halo(Transform p,string name,Vector3 at,float radius,Material material)
        {
            string path=SkyscraperArt.Folder+"/Meshes/Apex_Halo.asset";
            Mesh mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh==null || mesh.name!="ApexContinuousHalo_v2")
            {
                Mesh existing=mesh;
                const int n=64,m=8;
                var v=new Vector3[n*m];var uv=new Vector2[v.Length];var triangles=new int[n*m*6];
                for(int i=0;i<n;i++) for(int j=0;j<m;j++)
                {
                    float t=i*Mathf.PI*2/n,s=j*Mathf.PI*2/m;
                    v[i*m+j]=new Vector3(Mathf.Sin(t)*(1+.018f*Mathf.Cos(s)),.018f*Mathf.Sin(s),Mathf.Cos(t)*(1+.018f*Mathf.Cos(s)));
                    uv[i*m+j]=new Vector2((float)i/n,(float)j/m);
                    int k=(i*m+j)*6,a0=i*m+j,b=((i+1)%n)*m+j,c=((i+1)%n)*m+(j+1)%m,d=i*m+(j+1)%m;
                    triangles[k]=a0;triangles[k+1]=b;triangles[k+2]=c;triangles[k+3]=a0;triangles[k+4]=c;triangles[k+5]=d;
                }
                mesh=new Mesh{name="ApexContinuousHalo_v2",vertices=v,uv=uv,triangles=triangles};mesh.RecalculateNormals();mesh.RecalculateBounds();
                if(existing==null)AssetDatabase.CreateAsset(mesh,path);
                else
                {
                    EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(mesh);
                }
            }
            Transform g=Group(name,p);g.localPosition=at;g.localScale=Vector3.one*radius;
            g.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;g.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
        }

        public static void Planter(Transform p,SkyscraperArt a,Vector3 at)
        {
            Transform g=Group("SculpturalOlivePlanter",p);g.localPosition=at;
            Primitive(PrimitiveType.Cylinder,"StoneVessel",g,new Vector3(0,.32f,0),new Vector3(.72f,.32f,.72f),a.Marble);
            Primitive(PrimitiveType.Cylinder,"Trunk",g,new Vector3(0,1.2f,0),new Vector3(.07f,.85f,.07f),a.Mahogany,false);
            for(int i=0;i<12;i++)
            {
                float t=i*137.5f*Mathf.Deg2Rad;
                var leaf=Primitive(PrimitiveType.Sphere,"OliveFoliage",g,new Vector3(Mathf.Sin(t)*.35f,1.7f+i*.045f,Mathf.Cos(t)*.35f),new Vector3(.65f,.28f,.30f),a.Velvet,false);
                leaf.transform.localRotation=Quaternion.Euler(0,-i*137.5f,15);
            }
        }

        public static void WallPanels(Transform p,SkyscraperArt a,float y)
        {
            foreach(float x in new[]{-17.8f,17.8f})
            {
                Box("WalnutWindowPlinth",p,new Vector3(x,y+.32f,6),new Vector3(.16f,.64f,31.6f),a.Mahogany);
                Box("BrassPlinthCap",p,new Vector3(x,y+.66f,6),new Vector3(.18f,.035f,31.6f),a.Brass,false);
            }
            Box("NorthWindowPlinth",p,new Vector3(0,y+.32f,21.8f),new Vector3(35.6f,.64f,.16f),a.Mahogany);
            Box("NorthBrassCap",p,new Vector3(0,y+.66f,21.8f),new Vector3(35.6f,.035f,.18f),a.Brass,false);
        }
    }
}
