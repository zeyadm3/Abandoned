using Abandoned.Core;
using Abandoned.Loot;
using Abandoned.Interaction;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    public static class MallBasementBuilder
    {
        public static bool AboveBasement(Vector3 at) => at.x > 40f && at.z > 24f;

        public static void Build(Transform mall)
        {
            Transform basement = Group("Basement", mall);
            Material concrete = PolishAssets.Material("Basement_Concrete", new Color(0.24f,0.26f,0.25f));
            Transform tiles = Group("Tiles", basement);
            for (int x=10; x<14; x++)
            for (int z=6; z<12; z++)
            {
                Transform tile = Group($"Basement_{x}_{z}", tiles);
                tile.localPosition = new Vector3(x*4+2,-4,z*4+2);
                var collider = tile.gameObject.AddComponent<BoxCollider>();
                collider.center = new Vector3(0,-0.15f,0);
                collider.size = new Vector3(4,0.3f,4);
                CustomMallArt.Place("FloorConcrete", tile, Vector3.zero, Quaternion.identity, new Vector3(1f, .3f / .18f, 1f));
                tile.GetChild(0).name = "Visual";
                tile.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
                if ((x+z)%3==0 && !OnFlightLane(new Vector2Int(x,z)))
                {
                    var point = new GameObject("FloodedLootPoint");
                    point.transform.SetParent(basement,false);
                    point.transform.position = tile.position + new Vector3(-0.8f,0,0.8f);
                    point.AddComponent<LootSpawnPoint>().EditorSetup(z>9?"stock":"electronics", CarryClass.Pocket,CarryClass.TwoHand,false);
                }
            }
            GreyboxWall.Panel(basement,"WestRetainingWall",new Vector3(40,-4,24),Vector3.forward,24,4,0.3f,null,concrete,concrete);
            GreyboxWall.Panel(basement,"EastRetainingWall",new Vector3(56,-4,24),Vector3.forward,24,4,0.3f,null,concrete,concrete);
            GreyboxWall.Panel(basement,"SouthRetainingWall",new Vector3(40,-4,24),Vector3.right,16,4,0.3f,null,concrete,concrete);
            GreyboxWall.Panel(basement,"NorthRetainingWall",new Vector3(40,-4,48),Vector3.right,16,4,0.3f,null,concrete,concrete);
            foreach (string name in new[] { "WestRetainingWall", "EastRetainingWall", "SouthRetainingWall", "NorthRetainingWall" })
                foreach (Renderer renderer in basement.Find(name).GetComponentsInChildren<Renderer>()) renderer.enabled = false;
            for (int z = 6; z < 12; z++)
            {
                CustomMallArt.Place("WallSolid", basement, new Vector3(40f, -4f, z * 4f + 2f), Quaternion.Euler(0f, 90f, 0f), new Vector3(1f, 4f / 3.82f, 1.5f));
                CustomMallArt.Place("WallSolid", basement, new Vector3(56f, -4f, z * 4f + 2f), Quaternion.Euler(0f, 90f, 0f), new Vector3(1f, 4f / 3.82f, 1.5f));
            }
            for (int x = 10; x < 14; x++)
            {
                CustomMallArt.Place("WallSolid", basement, new Vector3(x * 4f + 2f, -4f, 24f), Quaternion.identity, new Vector3(1f, 4f / 3.82f, 1.5f));
                CustomMallArt.Place("WallSolid", basement, new Vector3(x * 4f + 2f, -4f, 48f), Quaternion.identity, new Vector3(1f, 4f / 3.82f, 1.5f));
            }
            MallFlights.Build(MallLayout.BasementFlight,basement);
            for (int z=7;z<=11;z+=2)
            {
                CustomMallArt.Place("pillar",basement,new Vector3(46,-4,z*4),Quaternion.identity);
                CustomMallArt.Place(z==11?"barricade":"debris",basement,new Vector3(42,-3.97f,z*4+1),Quaternion.Euler(0,23*z,0));
            }
            for(int x=10;x<14;x++)
            for(int z=10;z<12;z++)
                CustomMallArt.Place("puddle",basement,new Vector3(x*4+2,-3.86f,z*4+2),Quaternion.identity,new Vector3(3.1f,1,3.1f));
            MallMysteryBuilder.Sign(basement,new Vector3(55.8f,-1.8f,44),Vector3.left,"LEVEL -1 / WATER LINE",1.9f,0.32f);
            MallMysteryBuilder.Sign(basement,new Vector3(40.2f,-1.4f,30),Vector3.right,"PARKING C / NO SIGNAL",2.8f,0.35f);
            var zone=new GameObject("BasementAir"); zone.transform.SetParent(basement,false);
            zone.transform.position=new Vector3(48,-2,40);
            zone.AddComponent<Abandoned.Audio.HorrorAreaAmbience>().EditorSetup(new Vector3(16,4,16),2);
            var spawn=new GameObject("ThreatSpawn_Basement"); spawn.transform.SetParent(basement,false);
            spawn.transform.position=new Vector3(43,-4,45); spawn.AddComponent<Abandoned.Threats.ThreatSpawnPoint>();
        }

        // Loot never spawns on the flight or the tile you step off it onto.
        private static bool OnFlightLane(Vector2Int c)
        {
            MallLayout.Flight f = MallLayout.BasementFlight;
            return c == f.Start - f.Dir || System.Linq.Enumerable.Contains(f.Tiles, c);
        }

        public static void AddStructure(Transform mall,StructureSceneSetup setup)
        {
            foreach(Transform tile in mall.Find("Basement/Tiles")) setup.AddSection(tile,SectionType.Floor,false);
            setup.AddSection(mall.Find($"Basement/{MallLayout.BasementFlight.Name}"),SectionType.Stair,false,fractured:false);
        }
    }
}
