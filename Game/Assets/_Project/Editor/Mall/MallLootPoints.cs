using System.Linq;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEngine;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    public static class MallLootPoints
    {
        private static readonly (int floor,int x,int z,string tag,string id,bool jackpot)[] Prizes={
            (2,5,11,"gallery","marble_statue",true),(1,2,10,"furniture","designer_sofa",false),
            (-1,12,7,"stock","military_generator",true),(1,7,6,"furniture","grand_piano",true)
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
                if(!zone.HasValue||IsJackpotTile(c,f)||IsProtected(c,f)||UnderFlight(c,f))continue;
                if((x+z+f)%3!=0)continue;
                bool heavy=zone.Value.Kind==Kind.Store&&(x+z)%7==0;
                // Stocking at wall edges preserves the middle of the carry aisle.
                Vector2 offset=heavy?Vector2.zero:new Vector2((x%2==0?1:-1)*.8f,(z%2==0?1:-1)*.65f);
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
        public static bool IsJackpotTile(Vector2Int c,int floor)=>Prizes.Any(p=>p.floor==floor&&p.x==c.x&&p.z==c.y);
    }
}
