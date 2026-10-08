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
                new("Escalator_G1",0,5,4,Vector2Int.up,true), new("Escalator_12",1,8,5,Vector2Int.down,true),
                new("Stairs_G1",0,new Vector3(45.55f,0,36),Vector2Int.up,FlightKind.Stair,4,3),
                new("Stairs_12",1,new Vector3(45.55f,4,36),Vector2Int.up,FlightKind.Stair,4,3),
            };
            for(int floor=-1;floor<2;floor++)
            {
                list.Add(new Flight($"HaulRamp_{floor}_A",floor,new Vector3(49.65f,FloorY(floor),36),Vector2Int.up,FlightKind.HaulRamp,2,4));
                list.Add(new Flight($"HaulRamp_{floor}_B",floor,new Vector3(53.85f,FloorY(floor)+2,44),Vector2Int.down,FlightKind.HaulRamp,2,4));
            }
            return list.ToArray();
        }
        public static readonly Zone[][] ZonesByFloor = {
            new[] {
                new Zone("ServiceRear","stock",Kind.Concourse,0,2,1,9,"SERVICE / WEST"),
                new Zone("NorthPassage","stock",Kind.Concourse,0,9,11,1,"DELIVERY PASSAGE"),
                new Zone("GrandAtrium","concourse",Kind.Concourse,4,0,6,10,"MERIDIAN COURT"),
                new Zone("DeadCircuit","electronics",Kind.Store,0,0,4,5,"DEAD CIRCUIT"),
                new Zone("Threadbare","clothing",Kind.Store,0,5,4,5,"THREADBARE"),
                new Zone("FoodCourt","food",Kind.Concourse,0,10,10,2,"FOOD FOR THOUGHT"),
                new Zone("ServicePassage","stock",Kind.Concourse,10,0,1,12,"SERVICE / EAST"),
                new Zone("Jewelry","jewelry",Kind.Store,11,0,3,4,"LAST CHANCE JEWELS"),
                new Zone("Security","office",Kind.Store,11,4,3,4,"MALL SECURITY"),
                new Zone("LoadingBay","stock",Kind.Concourse,11,8,3,4,"GOODS IN / CREW OUT"),
            },
            new[] {
                new Zone("ServiceRear","stock",Kind.Concourse,0,2,1,9,"SERVICE / WEST"),
                new Zone("NorthPassage","stock",Kind.Concourse,0,9,11,1,"DELIVERY PASSAGE"),
                new Zone("Walkway1","walkway",Kind.Walkway,4,3,6,6,"MERIDIAN COURT / 01"),
                new Zone("Furniture","furniture",Kind.Store,0,0,4,12,"RECLINE & FALL"),
                new Zone("ToyArchive","toys",Kind.Store,4,0,6,3,"TOY ARCHIVE"),
                new Zone("FoodTerrace","food",Kind.Walkway,4,9,6,3,"FOOD FOR THOUGHT / TERRACE"),
                new Zone("ServicePassage","stock",Kind.Concourse,10,0,1,12,"SERVICE / EAST"),
                new Zone("Electronics2","electronics",Kind.Store,11,0,3,8,"DEAD CIRCUIT / ANNEX"),
                new Zone("Stockroom","stock",Kind.Concourse,11,8,3,4,"RECEIVING"),
            },
            new[] {
                new Zone("ServiceRear","stock",Kind.Concourse,0,2,1,9,"SERVICE / WEST"),
                new Zone("NorthPassage","stock",Kind.Concourse,0,8,11,1,"DELIVERY PASSAGE"),
                new Zone("Walkway2","walkway",Kind.Walkway,4,3,6,6,"MERIDIAN COURT / 02"),
                new Zone("Cinema","cinema",Kind.Store,0,0,4,12,"FINAL REEL"),
                new Zone("Quarantine","jewelry",Kind.Store,4,0,6,3,"LAST CHANCE / CLEARANCE"),
                new Zone("CollapsedGallery","gallery",Kind.Store,4,9,6,3,"STILL LIFE GALLERY"),
                new Zone("ServicePassage","stock",Kind.Concourse,10,0,1,12,"SERVICE / EAST"),
                new Zone("Offices","office",Kind.Store,11,0,3,8,"MERIDIAN MANAGEMENT"),
                new Zone("StairTop","stock",Kind.Concourse,11,8,3,4,"SERVICE CORE"),
            }
        };
        public static readonly Door[] Doors = CreateDoors();
        private static Door[] CreateDoors()
        {
            var d=new List<Door>();
            for(int f=0;f<3;f++) {
                d.Add(new(f,0,3,1,3,true)); d.Add(new(f,3,3,4,3,true));
                d.Add(new(f,3,8,4,8,true)); d.Add(new(f,0,8,1,8,true));
                d.Add(new(f,9,3,10,3,true)); d.Add(new(f,9,8,10,8,true));
                d.Add(new(f,10,8,11,8,true)); d.Add(new(f,10,11,11,11,true));
            }
            d.Add(new(0,3,7,4,7,true)); d.Add(new(0,0,7,1,7,true));
            d.Add(new(0,10,2,11,2,true)); d.Add(new(0,10,5,11,5,true));
            d.Add(new(1,6,2,6,3,true)); d.Add(new(1,10,3,11,3,true));
            d.Add(new(2,6,2,6,3,true)); d.Add(new(2,6,8,6,9,true)); d.Add(new(2,10,3,11,3,true));
            return d.ToArray();
        }
        public static readonly (int floor,char side,int column,bool wide,bool window)[] Exterior = {
            (0,'S',6,true,false),(0,'S',7,true,false),(0,'E',8,true,false),(0,'W',2,true,false),
            (1,'W',6,true,true),(1,'E',4,true,true),(2,'W',5,true,true),(2,'N',5,true,true)
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
