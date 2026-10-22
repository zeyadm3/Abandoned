using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.Motel
{
    public static class MotelLootModels
    {
        public static GameObject Build(MotelKit k,LootDefinition d,int kind)
        {
            string folder=MotelKit.Art+"/LootModels";MotelKit.Ensure(folder);
            string path=folder+"/"+d.Id+".prefab";
            var existing=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(existing!=null)return existing;
            var go=new GameObject(d.Id);Transform p=go.transform;
            void Part(string n,Vector3 at,Vector3 size,Material mat)=>k.Box(n,p,Vector3.Scale(at,d.Size),Vector3.Scale(size,d.Size),mat,false);
            switch(kind)
            {
                case 0:
                    Part("Case",Vector3.zero,new(.95f,.8f,1),k.Wood);
                    Part("LidSeam",new(0,.25f,0),new(1,.035f,1),k.Dark);
                    Part("Handle",new(0,.46f,0),new(.4f,.08f,.18f),k.Metal);
                    foreach(float x in new[]{-.3f,.3f})
                    {
                        Part("Strap",new(x,0,0),new(.065f,.82f,1.02f),k.Accent);
                        Part("Clasp",new(x,.15f,-.51f),new(.1f,.12f,.03f),k.Metal);
                    }break;
                case 1:
                    Part("Case",Vector3.zero,new(1,.9f,.9f),k.Wood);
                    Part("ScreenBezel",new(-.08f,0,-.46f),new(.75f,.72f,.06f),k.Dark);
                    Part("Screen",new(-.08f,0,-.5f),new(.65f,.6f,.025f),k.Glass);
                    foreach(float y in new[]{-.2f,.2f})Part("Dial",new(.38f,y,-.48f),new(.12f,.12f,.07f),k.Metal);
                    Part("FootL",new(-.35f,-.47f,0),new(.13f,.06f,.6f),k.Dark);
                    Part("FootR",new(.35f,-.47f,0),new(.13f,.06f,.6f),k.Dark);break;
                case 2:
                    Part("Body",new(0,-.05f,0),new(1,.8f,1),k.Accent);
                    Part("TopHandle",new(0,.43f,0),new(.65f,.1f,.18f),k.Dark);
                    Part("DialPlate",new(.2f,.05f,-.51f),new(.35f,.3f,.03f),k.Metal);
                    for(int i=0;i<6;i++)Part("SpeakerSlot",new(-.35f+i*.065f,0,-.51f),new(.025f,.5f,.03f),k.Dark);
                    Part("Control",new(.25f,-.25f,-.53f),new(.15f,.1f,.06f),k.Dark);break;
                case 3:
                    Part("BoundCover",Vector3.zero,new(1,1,1),k.Wood);
                    for(int i=0;i<4;i++)Part("PageOrDrawer",new(0,-.3f+i*.2f,-.48f),new(.9f,.12f,.05f),k.Cloth);
                    Part("Spine",new(-.45f,0,0),new(.1f,1.02f,1.02f),k.Accent);
                    Part("NamePlate",new(0,.1f,-.53f),new(.5f,.2f,.03f),k.Metal);break;
                case 4:
                    Part("Body",Vector3.zero,new(.95f,.88f,.92f),k.Metal);
                    Part("DoorFrame",new(0,-.05f,-.47f),new(.7f,.65f,.05f),k.Dark);
                    Part("DoorGlass",new(0,-.05f,-.51f),new(.53f,.49f,.03f),k.Glass);
                    Part("ControlStrip",new(0,.34f,-.47f),new(.8f,.15f,.05f),k.Accent);
                    Part("CoinSlot",new(.2f,.34f,-.51f),new(.03f,.09f,.025f),k.Dark);
                    Part("Base",new(0,-.47f,0),new(1,.06f,1),k.Dark);break;
                case 5:
                    Part("GlassBody",new(0,-.05f,0),new(.85f,.75f,.85f),k.Glass);
                    Part("SilverRim",new(0,-.38f,0),new(1,.12f,1),k.Metal);
                    Part("Stopper",new(0,.4f,0),new(.5f,.2f,.5f),k.Metal);
                    Part("Label",new(0,-.05f,-.44f),new(.55f,.3f,.02f),k.Cloth);break;
                case 6:
                    Part("BrassBody",new(0,0,0),new(.9f,.9f,.8f),k.Metal);
                    Part("Dial",new(0,0,-.42f),new(.72f,.72f,.04f),k.Cloth);
                    Part("Needle",new(.15f,.02f,-.46f),new(.3f,.03f,.03f),k.Dark);
                    Part("Foot",new(0,-.47f,0),new(1,.06f,1),k.Wood);break;
                case 7:
                    for(int i=0;i<4;i++)Part("Fold",new(0,-.36f+i*.24f,0),new(1,.22f,.9f),k.Cloth);
                    Part("Binding",Vector3.zero,new(.12f,1.02f,1.02f),k.Accent);break;
                default:
                    Part("StereoCabinet",new(0,0,0),new(1,.85f,1),k.Wood);
                    foreach(float x in new[]{-.32f,.32f})
                    {
                        Part("Speaker",new(x,0,-.51f),new(.28f,.6f,.03f),k.Dark);
                        for(int i=0;i<4;i++)Part("SpeakerSlat",new(x,-.24f+i*.16f,-.54f),new(.26f,.025f,.025f),k.Metal);
                    }
                    Part("Tuner",new(0,.2f,-.51f),new(.28f,.12f,.04f),k.Glass);
                    Part("Lid",new(0,.44f,0),new(1,.04f,1),k.Wood);
                    foreach(float x in new[]{-.4f,.4f})foreach(float z in new[]{-.35f,.35f})Part("Leg",new(x,-.47f,z),new(.08f,.06f,.08f),k.Metal);
                    break;
            }
            GameObject prefab=PrefabUtility.SaveAsPrefabAsset(go,path);Object.DestroyImmediate(go);return prefab;
        }
    }
}
