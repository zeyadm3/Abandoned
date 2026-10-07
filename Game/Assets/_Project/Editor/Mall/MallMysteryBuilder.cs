using Abandoned.Interaction;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    public static class MallMysteryBuilder
    {
        public static void Place(Transform mall)
        {
            Transform root=Group("Unanswered",mall);
            Sign(root,new Vector3(28,3.72f,-0.22f),Vector3.back,"MERIDIAN / CLOSED UNTIL FURTHER NOTICE",8,0.5f);
            Sign(root,new Vector3(40.12f,2.4f,12),Vector3.right,"IT HEARS YOU",2.8f,0.5f);
            Sign(root,new Vector3(15.85f,6.4f,18),Vector3.right,"DON'T GO UP",2.4f,0.55f);
            Sign(root,new Vector3(24,10.7f,11.85f),Vector3.back,"QUARANTINE / NO ENTRY",3.5f,0.45f);
            Sign(root,new Vector3(44.15f,2.2f,22),Vector3.left,"NIGHT GUARD MISSING / 17 NOVEMBER",2.2f,0.42f);
            CustomMallArt.Place("StrangeSymbol",root,new Vector3(16.12f,1.8f,31),Quaternion.Euler(0,0,-90));
            CustomMallArt.Place("barricade",root,new Vector3(20,8,6),Quaternion.Euler(0,12,0));
            CustomMallArt.Place("AbandonedBag",root,new Vector3(44.8f,0,28),Quaternion.identity);
            CustomMallArt.Place("SalvageGear",root,new Vector3(46,0,29),Quaternion.Euler(0,22,0));
            Note(root,new Vector3(49,1.15f,16.14f),"Security log","03:17 — All cameras show the same corridor.\n03:19 — Knocking in the walls.\n03:20 — The corridor has no end.\nDo not send anyone upstairs.");
            Note(root,new Vector3(3,0.15f,37),"Crew manifest","CREW 06 / four signed in.\nSomeone keeps adding a fifth name.\nIf the radio asks for your name, switch it off.\nWe left the good gear downstairs.");
            Note(root,new Vector3(24,8.2f,7),"Quarantine notice","Temporary closure.\nDo not move the display figures.\nDo not enter after the emergency tone.\nThe inspection date has been scratched away.");
            for(int f=0;f<Floors;f++)foreach(Zone zone in ZonesByFloor[f])
            {
                var air=new GameObject("Area_"+zone.Name);air.transform.SetParent(root,false);
                air.transform.position=new Vector3(zone.Area.center.x*Tile,FloorY(f)+2,zone.Area.center.y*Tile);
                air.AddComponent<Abandoned.Audio.HorrorAreaAmbience>().EditorSetup(new Vector3(zone.Area.width*Tile,4,zone.Area.height*Tile),zone.Tag=="cinema"?1:0);
            }
            for(int i=0;i<10;i++)
            {
                var red=new GameObject("HorrorAlarm_"+i);red.transform.SetParent(root,false);
                red.transform.position=new Vector3(18+(i%2)*20,3+(i/4)*4,8+(i%4)*10);
                var lamp=red.AddComponent<Light>();lamp.type=LightType.Point;lamp.color=new Color(1,0.015f,0.005f);lamp.intensity=1.4f;lamp.range=12;lamp.enabled=false;
            }
            foreach(var exit in Exterior)
            {
                if(exit.window)continue;
                Vector3 at=exit.side=='S'?new Vector3(exit.column*Tile+2,2.8f,0.2f):exit.side=='E'?new Vector3(TilesX*Tile-0.2f,2.8f,exit.column*Tile+2):new Vector3(0.2f,2.8f,exit.column*Tile+2);
                Vector3 facing=exit.side=='S'?Vector3.forward:exit.side=='E'?Vector3.left:Vector3.right;
                Sign(root,at,facing,"EXIT",1,0.3f);
            }
            // The collapsed wing is authored geometry; only the surviving lane hosts valuable loot.
            for(int x=7;x<=9;x++)for(int z=10;z<12;z++)
            {
                CustomMallArt.Place("BrokenSlabEdge",root,new Vector3(x*Tile,8,z*Tile),Quaternion.Euler(0,x*27,0));
                CustomMallArt.Place("ExposedRebar",root,new Vector3(x*Tile+1,7.7f,z*Tile),Quaternion.identity);
                CustomMallArt.Place("debris",root,new Vector3(x*Tile+2,4.02f,z*Tile+2),Quaternion.Euler(0,z*21,0));
            }
        }
        private static void Note(Transform root,Vector3 at,string title,string text)
        {
            Quaternion rotation = title == "Security log" ? Quaternion.identity : Quaternion.Euler(90f, 0f, 0f);
            GameObject note = CustomMallArt.Place("MissingPoster", root, at, rotation, new Vector3(.65f, .65f, .65f));
            Bounds bounds = ModelFit.LocalBounds(note, note.transform);
            var collider = note.AddComponent<BoxCollider>();
            collider.center = bounds.center;
            collider.size = new Vector3(Mathf.Max(bounds.size.x, .15f), Mathf.Max(bounds.size.y, .03f), Mathf.Max(bounds.size.z, .025f));
            note.AddComponent<MysteryNote>().EditorSetup(title,text);
        }
        public static void Sign(Transform root,Vector3 at,Vector3 front,string text,float width,float height)
        {
            Transform sign=Group("Notice_"+text,root);sign.SetPositionAndRotation(at,Quaternion.LookRotation(front));
            Box("Paper",sign,Vector3.zero,new Vector3(width,height,0.035f),PolishAssets.Material("MysteryNotice",new Color(0.22f,0.24f,0.21f)),false);
            var words=new GameObject("FadedLetters").AddComponent<TextMesh>();words.transform.SetParent(sign,false);
            words.transform.localPosition=Vector3.forward*0.022f;words.transform.localRotation=Quaternion.Euler(0,180,0);
            words.text=text;words.anchor=TextAnchor.MiddleCenter;words.alignment=TextAlignment.Center;words.fontSize=64;
            words.characterSize=Mathf.Min(height*0.65f,width/Mathf.Max(1,text.Length)*1.4f);words.color=new Color(0.67f,0.62f,0.5f);
            var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf");
            if(font!=null){words.font=font;words.GetComponent<Renderer>().sharedMaterial=font.material;}
        }
    }
}
