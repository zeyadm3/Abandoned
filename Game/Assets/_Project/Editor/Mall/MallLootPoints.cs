using System.Linq;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    public static class MallLootPoints
    {
        private static readonly (int floor,int x,int z,string tag,string id,bool jackpot)[] Prizes={
            (1,2,7,"gallery","marble_statue",true),(1,2,5,"furniture","designer_sofa",false),
            (-1,12,7,"stock","military_generator",true),(1,6,6,"furniture","grand_piano",true)
        };
        public static int Place(Transform parent)
        {
            int count=0;
            foreach(var prize in Prizes) {
                Point(parent,prize.floor,new Vector2Int(prize.x,prize.z),prize.tag,
                    prize.jackpot?CarryClass.Huge:CarryClass.Heavy,prize.jackpot?CarryClass.Huge:CarryClass.Heavy,
                    prize.jackpot,Vector2.zero,new[]{prize.id},true);count++;
            }
            for(int f=-1;f<Floors;f++)for(int x=0;x<TilesX;x++)for(int z=0;z<TilesZ;z++) {
                var c=new Vector2Int(x,z);var zone=ZoneAt(c,f);
                if(!zone.HasValue||IsJackpotTile(c,f)||IsProtected(c,f)||UnderFlight(c,f)||LandmarkReserved(c,f))continue;
                if((x+z+f)%3!=0)continue;
                bool heavy=zone.Value.Kind==Kind.Store&&(x+z)%7==0;
                // Fittings are authored first; select open pockets beside them instead of scattering through them.
                Vector2 offset=Vector2.zero;
                bool found=false;
                foreach(Vector2 candidate in new[]{Vector2.zero,new Vector2(1.1f,0),new Vector2(-1.1f,0),new Vector2(0,1.1f),new Vector2(0,-1.1f)}) {
                    if(!Clear(parent.root,TileTopCenter(c,f)+new Vector3(candidate.x,0,candidate.y),heavy?1.35f:.65f))continue;
                    offset=candidate;found=true;break;
                }
                if(!found)continue;
                Point(parent,f,c,IsWalkwayTile(c,f)?"walkway":zone.Value.Tag,heavy?CarryClass.OneHand:CarryClass.Pocket,
                    heavy?CarryClass.Heavy:CarryClass.TwoHand,false,offset);count++;
            }
            return count;
        }
        private static void Point(Transform parent,int floor,Vector2Int c,string tag,CarryClass min,CarryClass max,bool jackpot,Vector2 offset,string[] ids=null,bool featured=false)
        {
            Transform p=GreyboxFactory.Group($"LootPoint_{floor}_{c.x:00}_{c.y:00}{(featured?"_Prize":"")}",parent);
            p.position=TileTopCenter(c,floor)+new Vector3(offset.x,0,offset.y);
            p.gameObject.AddComponent<LootSpawnPoint>().EditorSetup(tag,min,max,jackpot,ids,featured);
        }
        private static bool UnderFlight(Vector2Int c,int floor)=>AllFlights.Any(f=>f.Floor==floor&&f.Tiles.Contains(c));
        private static bool Clear(Transform mall,Vector3 at,float radius)
        {
            foreach(Collider collider in mall.GetComponentsInChildren<Collider>(true)) {
                if(collider.isTrigger||collider.GetComponentInParent<StructuralSection>()!=null)continue;
                Bounds bounds=collider.bounds;
                if(bounds.max.y<at.y+.15f||bounds.min.y>at.y+1.5f)continue;
                Vector3 nearest=bounds.ClosestPoint(at+Vector3.up*.5f);
                if(new Vector2(nearest.x-at.x,nearest.z-at.z).sqrMagnitude<radius*radius)return false;
            }
            return true;
        }
        // Loose floor loot would be hidden inside the fixed fountain basin or planter at these sites.
        private static bool LandmarkReserved(Vector2Int c,int floor)=>floor==0&&
            ((c.x==6&&c.y==6)||(c.x==8&&c.y==7)||(c.x==4&&c.y==2)); // 0.14.0: the mall directory is solid too
        public static bool IsJackpotTile(Vector2Int c,int floor)=>Prizes.Any(p=>p.floor==floor&&p.x==c.x&&p.z==c.y);
    }
}
