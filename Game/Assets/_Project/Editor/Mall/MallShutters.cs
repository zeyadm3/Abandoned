using System.Linq;
using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    public static class MallShutters
    {
        public static int Place(Transform mall)
        {
            Transform group=Group("Shutters",mall);int index=0;
            for(int floor=0;floor<Floors;floor++)foreach(Zone zone in ZonesByFloor[floor]) {
                if(zone.Kind!=Kind.Store||HasFallback(zone,floor))continue;
                var doors=Doors.Where(d=>d.Floor==floor&&(ZoneAt(d.A,floor)?.Name==zone.Name)!=(ZoneAt(d.B,floor)?.Name==zone.Name)).ToArray();
                if(doors.Length!=1)continue;
                Door d=doors[0];Vector3 at=(TileTopCenter(d.A,floor)+TileTopCenter(d.B,floor))/2;
                Vector3 along=d.A.x!=d.B.x?Vector3.forward:Vector3.right;
                Bounds room=new(new Vector3(zone.Area.center.x*Tile,FloorY(floor)+2,zone.Area.center.y*Tile),new Vector3(zone.Area.width*Tile,4,zone.Area.height*Tile));
                Shutter(group,$"Shutter_{floor}_{zone.Name}",index++,false,at,along,room);
            }
            foreach(var e in Exterior.Where(e=>e.floor==0&&e.side=='S'&&(e.column==6||e.column==7)))
                Shutter(group,$"Shutter_Entrance_{e.column}",index++,true,ExteriorPosition(e),Vector3.right,default);
            return index;
        }
        private static void Shutter(Transform parent,string name,int index,bool entrance,Vector3 at,Vector3 along,Bounds room)
        {
            if(index>=32)throw new System.InvalidOperationException("More than32 shutters.");
            const float width=MallWalls.WideWidth,height=MallWalls.WideHeight;
            Transform root=Group(name,parent);root.localPosition=at;root.localRotation=Quaternion.FromToRotation(Vector3.right,along);
            var blocker=root.gameObject.AddComponent<BoxCollider>();blocker.center=Vector3.up*height/2;blocker.size=new Vector3(width,height,.12f);
            Transform curtain=Group("Curtain",root);curtain.localPosition=Vector3.up*height;
            if(MeridianArt.UseKit) MeridianArt.Place("RollerShutter",curtain,Vector3.down*height,0,new Vector3(width/3.4f,height/3.1f,1));
            else Box("Slats",curtain,Vector3.down*height/2,new Vector3(width,height,.12f),MeridianArt.Material("Metal"),false);
            Box("Housing",root,Vector3.up*(height+.15f),new Vector3(width+.2f,.3f,.28f),MeridianArt.Material("Metal"),false);
            var obstacle=root.gameObject.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.center=blocker.center;
            obstacle.size=new Vector3(width,height,.4f);obstacle.carving=true;obstacle.enabled=false;
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            root.gameObject.AddComponent<RollerShutter>().EditorSetup(index,entrance,curtain,blocker,obstacle,room);
        }
    }
}
