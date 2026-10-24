using System.Collections.Generic;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A=Abandoned.EditorTools.SkyscraperArchitecture;
using L=Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    public static class SkyscraperUpperDining
    {
        public static readonly Vector2[] GalleryTables={new(-4.8f,-6.7f),new(.2f,-6.7f),new(13f,2.5f),new(13f,8f)};

        public static List<Transform> Build(Transform root,SkyscraperArt a,out List<Transform> weak)
        {
            Transform g=Group("UpperDining_410m",root);
            float y=L.UpperRestaurantY;
            var slabs=A.Deck(Group("FloorSlabs",g),y,a.Parquet,true,A.DiningAtrium,A.GrandStairOpening);
            weak=new List<Transform>();
            foreach(Transform slab in slabs)
                if(slab.position.x>14 && slab.position.z>6 && slab.position.z<12)weak.Add(slab);
            SkyscraperGrandStair.Build(root,a);
            SkyscraperFineDining.WallPanels(g,a,y);
            foreach(Vector2 at in GalleryTables)SkyscraperFurnishings.Table(g,a,new Vector3(at.x,y,at.y));
            Rect hole=A.DiningAtrium;
            A.Rail(g,new Vector3(hole.xMin,y,hole.yMin),new Vector3(hole.xMin,y,hole.yMax),a.Brass);
            A.Rail(g,new Vector3(hole.xMax,y,hole.yMin),new Vector3(hole.xMax,y,hole.yMax),a.Brass);
            A.Rail(g,new Vector3(hole.xMin,y,hole.yMin),new Vector3(hole.xMax,y,hole.yMin),a.Brass);
            A.Rail(g,new Vector3(hole.xMin,y,hole.yMax),new Vector3(hole.xMax,y,hole.yMax),a.Brass);
            // Solid balcony fascias read from the lower room without covering its cloud views.
            foreach(float x in new[]{hole.xMin,hole.xMax})
            {
                Box("GalleryGlassBalustrade",g,new Vector3(x,y+.52f,12.25f),new Vector3(.035f,.9f,12.5f),a.Glass,false);
                Box("WalnutBalconyFascia",g,new Vector3(x,y-.2f,12.25f),new Vector3(.22f,.42f,12.5f),a.Mahogany,false);
                Box("BalconyBrassReveal",g,new Vector3(x,y-.43f,12.25f),new Vector3(.25f,.035f,12.5f),a.Brass,false);
            }
            foreach(float z in new[]{6f,18.5f})
            {
                Box("GalleryGlassBalustrade",g,new Vector3(0,y+.52f,z),new Vector3(hole.width,.9f,.035f),a.Glass,false);
                Box("WalnutBalconyFascia",g,new Vector3(0,y-.2f,z),new Vector3(hole.width,.42f,.22f),a.Mahogany,false);
                Box("BalconyBrassReveal",g,new Vector3(0,y-.43f,z),new Vector3(hole.width,.035f,.25f),a.Brass,false);
            }
            ChefTable(g,a);
            BuildSalon(g,a);
            SkyscraperPrivateDining.Build(root,a);
            return slabs;
        }

        private static void ChefTable(Transform p,SkyscraperArt a)
        {
            float y=L.UpperRestaurantY;
            Transform g=Group("ChefsTastingRoom",p);
            // A warm glazed partition frames the room; its south doorway stays 2.6m wide.
            Box("ChefPartitionSouth",g,new Vector3(-13.3f,y+1.65f,8),new Vector3(8.6f,3.3f,.12f),a.Glass);
            Box("ChefPartitionEast",g,new Vector3(-9,y+1.65f,16.3f),new Vector3(.12f,3.3f,10.6f),a.Glass);
            A.Portal(g,new Vector3(-9,y,9.5f),2.8f,3.1f,a.Brass,true);
            Box("ChefTableBase",g,new Vector3(-13,y+.4f,15),new Vector3(1.2f,.8f,4.8f),a.Mahogany);
            Box("ChefsStoneTable",g,new Vector3(-13,y+.84f,15),new Vector3(2.5f,.12f,6.8f),a.Marble);
            Box("LinenRunner",g,new Vector3(-13,y+.907f,15),new Vector3(.6f,.014f,6.6f),a.Linen,false);
            foreach(float z in new[]{12.5f,14.2f,15.8f,17.5f})foreach(float x in new[]{-14.9f,-11.1f})
            {
                SkyscraperFineDining.Chair(g,a,new Vector3(x,y,z),x<-13?90:-90);
                Transform setting=Group("ChefsTastingSetting",g);setting.position=new Vector3(x<-13?-13.85f:-12.15f,y+.92f,z);
                setting.localRotation=Quaternion.Euler(0,x<-13?-90:90,0);SkyscraperFineDining.PlaceSetting(setting,a);
            }
            foreach(float z in new[]{13f,15f,17f})SkyscraperFineDining.Candle(g,a,new Vector3(-13,y+.92f,z));
            Box("FinishingCounter",g,new Vector3(-13,y+.53f,20.7f),new Vector3(6,1.06f,.85f),a.Mahogany);
            Box("ChefPassStone",g,new Vector3(-13,y+1.1f,20.7f),new Vector3(6.15f,.08f,.95f),a.Marble);
            Label(g,"ChefsTableIdentity","L ' A T E L I E R\nCHEF'S TASTING ROOM",new Vector3(-13,y+3,8.09f),Quaternion.Euler(0,180,0),.14f,new Color(.92f,.80f,.52f));
        }

        private static void BuildSalon(Transform g,SkyscraperArt a)
        {
            float y=L.UpperRestaurantY;
            Box("GalleryLibraryWall",g,new Vector3(.3f,y+2,-9.7f),new Vector3(7.5f,4,.14f),a.Mahogany);
            foreach(float x in new[]{-3.3f,-1.5f,.3f,2.1f,3.9f})
            {
                Box("FlutedLibraryPilaster",g,new Vector3(x,y+2,-9.55f),new Vector3(.08f,3.8f,.10f),a.Brass,false);
                foreach(float h in new[]{.8f,1.7f,2.6f})
                    Box("GalleryDisplayShelf",g,new Vector3(x,y+h,-9.35f),new Vector3(1.65f,.08f,.55f),a.Mahogany,false);
                Primitive(PrimitiveType.Sphere,"GallerySculpture",g,new Vector3(x,y+1.98f,-9.35f),new Vector3(.24f,.5f,.24f),a.Brass,false);
            }
            Label(g,"GalleryIdentity","A P E X   /   T H E   G A L L E R Y",new Vector3(.3f,y+3.35f,-9.44f),Quaternion.Euler(0,180,0),.13f,new Color(.96f,.86f,.64f));
            foreach(Vector3 at in new[]{new Vector3(16,y,11.7f),new Vector3(-5,y,20.5f),new Vector3(5,y,20.5f)})SkyscraperFineDining.Planter(g,a,at);
        }
    }
}
