using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Abandoned.EditorTools
{
    public static partial class MallLayout
    {
        public static readonly Flight BasementFlight = new("BasementServiceStairs",-1,new Vector3(45.55f,-4,36),Vector2Int.up,FlightKind.Stair,4,3);
        public static readonly Flight[] Flights = CreateFlights();
        public static IEnumerable<Flight> AllFlights => Flights.Append(BasementFlight);
        private static Flight[] CreateFlights()
        {
            var list=new List<Flight> {
                new("Escalator_G1_West",0,5,4,Vector2Int.up,true),
                new("Escalator_G1_East",0,8,4,Vector2Int.up,true),
                new("Stairs_G1",0,new Vector3(45.55f,0,36),Vector2Int.up,FlightKind.Stair,4,3),
            };
            for(int floor=-1;floor<Floors-1;floor++)
            {
                list.Add(new Flight($"HaulRamp_{floor}_A",floor,new Vector3(49.65f,FloorY(floor),36),Vector2Int.up,FlightKind.HaulRamp,2,4));
                list.Add(new Flight($"HaulRamp_{floor}_B",floor,new Vector3(53.85f,FloorY(floor)+2,44),Vector2Int.down,FlightKind.HaulRamp,2,4));
            }
            return list.ToArray();
        }
        public static readonly Zone[][] ZonesByFloor = {
            new[] {
                new Zone("ServiceCore","stock",Kind.Concourse,11,8,3,4,"SERVICE"),
                new Zone("GrandAtrium","concourse",Kind.Concourse,4,3,6,6,"MERIDIAN COURT"),
                new Zone("EntryPromenade","concourse",Kind.Concourse,6,0,2,3,"MERIDIAN COURT"),
                new Zone("RearPromenade","concourse",Kind.Concourse,0,8,11,1,"FOOD COURT"),
                new Zone("FoodCourt","food",Kind.Concourse,0,9,11,3,"FOOD COURT"),
                new Zone("DeadCircuit","electronics",Kind.Store,0,0,6,4,"APEX CIRCUIT"),
                new Zone("Threadbare","clothing",Kind.Store,0,4,4,4,"VOGUE THREAD"),
                new Zone("Jewelry","furniture",Kind.Store,8,0,6,4,"NORDIC HABITAT"),
                new Zone("Security","hardware",Kind.Store,10,4,4,4,"TITAN TOOLS"),
            },
            new[] {
                new Zone("ServiceCore","stock",Kind.Concourse,11,8,3,4,"SERVICE"),
                new Zone("Walkway1","walkway",Kind.Walkway,4,3,6,6,"MERIDIAN GALLERY"),
                new Zone("EntryGallery","concourse",Kind.Concourse,6,0,2,3,"MERIDIAN GALLERY"),
                new Zone("RearGallery","concourse",Kind.Concourse,0,8,11,1,"DINING TERRACE"),
                new Zone("FoodTerrace","food",Kind.Concourse,0,9,11,3,"DINING TERRACE"),
                new Zone("Cinema","cinema",Kind.Store,0,0,6,4,"CINEMA"),
                new Zone("Furniture","office",Kind.Store,0,4,4,4,"CLOSED OFFICES"),
                new Zone("Offices","furniture",Kind.Store,8,0,6,4,"NORDIC HABITAT"),
                new Zone("ToyArchive","toys",Kind.Store,10,4,4,4,"TOY ARCHIVE"),
            }
        };
        public static readonly Door[] Doors = CreateDoors();
        private static Door[] CreateDoors()
        {
            var d=new List<Door>();
            for(int f=0;f<Floors;f++) {
                d.Add(new(f,5,1,6,1,true));
                d.Add(new(f,7,1,8,1,true));
                d.Add(new(f,3,5,4,5,true));
                d.Add(new(f,9,5,10,5,true));
            }
            return d.ToArray();
        }
        public static readonly (int floor,char side,int column,bool wide,bool window)[] Exterior = {
            // Two adjoining four-metre panels form one uninterrupted main entrance.
            (0,'S',6,true,false),(0,'S',7,true,false)
        };
        public static bool HasFallback(Zone zone,int floor) => Exterior.Any(e=>e.floor==floor && e.window &&
            (e.side=='W'?zone.Area.xMin==0 && e.column>=zone.Area.yMin && e.column<zone.Area.yMax:
             e.side=='E'?zone.Area.xMax==14 && e.column>=zone.Area.yMin && e.column<zone.Area.yMax:
             zone.Area.yMax==12 && e.column>=zone.Area.xMin && e.column<zone.Area.xMax));
        public static Vector3 BoundaryCentre(Vector2Int c,char side,int floor) => TileTopCenter(c,floor)+
            (side=='S'?Vector3.back:side=='N'?Vector3.forward:side=='E'?Vector3.right:Vector3.left)*2;
        public static Vector3 ExteriorPosition((int floor,char side,int column,bool wide,bool window) e)
        {
            bool vertical=e.side=='E'||e.side=='W';
            int limit=vertical?TilesX:TilesZ;
            for(int i=0;i<limit;i++)
            {
                int k=e.side=='N'||e.side=='E'?limit-1-i:i;
                Vector2Int c=vertical?new Vector2Int(k,e.column):new Vector2Int(e.column,k);
                if(InFootprint(c)) return BoundaryCentre(c,e.side,e.floor);
            }
            throw new System.InvalidOperationException($"No footprint at exit {e.side}{e.column}.");
        }
    }
}
