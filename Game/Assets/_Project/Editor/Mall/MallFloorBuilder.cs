using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    public static class MallFloorBuilder
    {
        public static int Build(Transform tiles)
        {
            int count=0;
            for(int f=-1;f<Floors;f++) {
                Transform floor=Group($"Floor_{f}",tiles);
                for(int x=0;x<TilesX;x++) for(int z=0;z<TilesZ;z++) {
                    var c=new Vector2Int(x,z);var zone=ZoneAt(c,f);if(!zone.HasValue)continue;
                    Transform tile=Group(MallBuilder.TileName(c,f),floor);tile.localPosition=TileTopCenter(c,f);
                    var collider=tile.gameObject.AddComponent<BoxCollider>();
                    collider.center=Vector3.down*SlabThickness/2;collider.size=new Vector3(Tile,SlabThickness,Tile);
                    string finish=Finish(c,f);
                    if(MeridianArt.UseKit) MeridianArt.Place("Floor"+finish,Group("Visual",tile),Vector3.zero);
                    else Box("Slab",Group("Visual",tile),collider.center,collider.size,MeridianArt.Material(finish),false);
                    Transform surface=tile.Find("Visual");
                    // Divider strips visibly identify the structural module without blocking a trolley.
                    if(!MeridianArt.UseKit)foreach(float edge in new[]{-1f,1f}) {
                        Box("DividerX",surface,new Vector3(0,.002f,edge*1.985f),new Vector3(3.97f,.008f,.03f),MeridianArt.Material("Brass"),false);
                        Box("DividerZ",surface,new Vector3(edge*1.985f,.002f,0),new Vector3(.03f,.008f,3.97f),MeridianArt.Material("Brass"),false);
                    }
                    tile.gameObject.AddComponent<SurfaceTag>().EditorSet(Surface(finish,f,c));count++;
                }
            }
            return count;
        }
        public static string Finish(Vector2Int cell,int floor)
        {
            if(floor<0)return "Concrete";
            if(IsBridge(cell,floor))return "Bridge";
            string tag=ZoneAt(cell,floor)?.Tag;
            return tag switch {"cinema" or "clothing" or "office"=>"Carpet","furniture"=>"Wood",
                "stock" or "gallery"=>"Concrete","electronics" or "food" or "jewelry" or "toys"=>"Tile",_=>"Terrazzo"};
        }
        private static SurfaceMaterial Surface(string finish,int floor,Vector2Int cell)
        {
            if(floor<0&&cell.x>=11&&cell.y<8)return SurfaceMaterial.Water;
            return finish switch {"Terrazzo"=>SurfaceMaterial.Terrazzo,"Tile"=>SurfaceMaterial.Tile,"Carpet"=>SurfaceMaterial.Carpet,
                "Wood"=>SurfaceMaterial.Wood,"Bridge"=>SurfaceMaterial.Metal,_=>SurfaceMaterial.Concrete};
        }
    }
}
