using System.Collections.Generic;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L=Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    public static class SkyscraperRestaurantDetails
    {
        public static void Build(Transform g,SkyscraperArt a,List<Transform> slabs)
        {
            float y=L.RestaurantY;
            foreach(Transform slab in slabs)
            {
                var box=slab.GetComponent<BoxCollider>();
                if(slab.position.z<4)slab.GetComponentInChildren<Renderer>().sharedMaterial=a.Marble;
                if(slab.position.z<6 || Mathf.Abs(slab.position.x)>2.1f)continue;
                // Each inlaid panel follows its structural tile when the window bay gives way.
                Transform visual=slab.Find("Visual");
                Box("MarbleAislePanel",visual,new Vector3(0,.012f,0),new Vector3(Mathf.Min(3.8f,box.size.x-.12f),.02f,box.size.z-.10f),a.Marble,false);
                foreach(float x in new[]{-1.92f,1.92f})
                    if(Mathf.Abs(x)<box.size.x*.5f)Box("AisleEdgeInlay",visual,new Vector3(x,.027f,0),new Vector3(.025f,.01f,box.size.z-.1f),a.Brass,false);
            }
            SkyscraperFineDining.WallPanels(g,a,y);
            foreach(float x in new[]{-7.7f,7.7f})
            {
                Box("UpperDiningPier",g,new Vector3(x,y+9,12),new Vector3(.4f,6,.4f),a.GraniteDark);
                foreach(float dx in new[]{-.22f,.22f})
                    Box("PierBrassFlute",g,new Vector3(x+dx,y+6,11.78f),new Vector3(.035f,11.8f,.04f),a.Brass,false);
            }
            // Coffered ceilings sit beneath the gallery wings, framing the twelve-metre atrium.
            foreach(float x in new[]{-4.5f,4.5f,13f})
            foreach(float z in new[]{-7f,-3f,1f})
            {
                if(x>4 && x<11)continue;
                Box("CeilingCoffer",g,new Vector3(x,y+5.52f,z),new Vector3(3.7f,.12f,3.6f),a.WallPlaster,false);
                foreach(float dx in new[]{-1.9f,1.9f})Box("CofferBrassFrame",g,new Vector3(x+dx,y+5.44f,z),new Vector3(.04f,.04f,3.8f),a.Brass,false);
                foreach(float dz in new[]{-1.9f,1.9f})Box("CofferBrassFrame",g,new Vector3(x,y+5.44f,z+dz),new Vector3(3.8f,.04f,.04f),a.Brass,false);
            }
            Box("AtriumCeilingInset",g,new Vector3(0,L.RoofY-.43f,12.25f),new Vector3(13.4f,.10f,11.9f),a.WallPlaster,false);
            foreach(float x in new[]{-6.8f,6.8f})
                Box("AtriumCeilingReveal",g,new Vector3(x,L.RoofY-.5f,12.25f),new Vector3(.04f,.06f,12.2f),a.Brass,false);
            BuildLounge(g,a);
            foreach(Vector3 at in new[]{new Vector3(-6.8f,y,4.5f),new Vector3(16.5f,y,10.8f),new Vector3(-6.5f,y,-8.5f)})
                SkyscraperFineDining.Planter(g,a,at);
            Label(g,"ServiceStandard","A P E X\nSEASONAL TASTING  /  RESERVATIONS",new Vector3(3.2f,y+2.3f,-9.8f),Quaternion.Euler(0,180,0),.13f,new Color(.91f,.77f,.45f));
        }

        private static void BuildLounge(Transform g,SkyscraperArt a)
        {
            Transform lounge=Group("ChampagneSalon",g);
            SkyscraperFurnishings.Table(lounge,a,new Vector3(12.8f,404f,17f));
            SkyscraperFurnishings.Bench(lounge,a,new Vector3(16.7f,404f,17),90);
            Box("ChampagneConsole",lounge,new Vector3(13,404.5f,20.7f),new Vector3(5,1,.8f),a.Mahogany);
            Box("ConsoleMarble",lounge,new Vector3(13,405.04f,20.7f),new Vector3(5.1f,.08f,.9f),a.Marble);
            for(float x=10.8f;x<=15.3f;x+=.25f)
                Box("ConsoleFluting",lounge,new Vector3(x,404.5f,20.28f),new Vector3(.025f,.85f,.025f),a.Brass,false);
            Transform piano=Group("SalonGrandPiano",lounge);piano.position=new Vector3(10.4f,404f,12.2f);
            Primitive(PrimitiveType.Sphere,"LacqueredPianoBody",piano,new Vector3(0,.95f,0),new Vector3(2.2f,.32f,2.6f),a.GraniteDark);
            Box("KeyboardCase",piano,new Vector3(0,.94f,-1.03f),new Vector3(1.65f,.20f,.5f),a.GraniteDark);
            for(int i=0;i<28;i++)
            {
                Box("IvoryKey",piano,new Vector3(-.73f+i*.054f,1.052f,-1.08f),new Vector3(.049f,.018f,.3f),a.Porcelain,false);
                if(i%7!=2 && i%7!=6)Box("EbonyKey",piano,new Vector3(-.705f+i*.054f,1.074f,-1.02f),new Vector3(.026f,.03f,.18f),a.GraniteDark,false);
            }
            foreach(Vector3 at in new[]{new Vector3(-.7f,.45f,-.8f),new Vector3(.7f,.45f,-.8f),new Vector3(0,.45f,.8f)})
                Box("PianoLeg",piano,at,new Vector3(.12f,.9f,.12f),a.GraniteDark,false);
            Box("PianistBench",piano,new Vector3(0,.5f,-1.9f),new Vector3(1.1f,.18f,.5f),a.Velvet);
        }
    }
}
