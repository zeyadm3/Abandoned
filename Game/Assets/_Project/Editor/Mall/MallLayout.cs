using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Abandoned.EditorTools
{
    // The envelope is a budget, not a box: setbacks and the entry court shape the occupied cells.
    public static partial class MallLayout
    {
        public const float Tile = 4f, StoryHeight = 4f, SlabThickness = .25f;
        public const int TilesX = 14, TilesZ = 12, Floors = 3;
        public enum Kind { Store, Walkway, Bridge, Concourse }
        public enum FlightKind { Escalator, Stair, HaulRamp }

        public readonly struct Zone
        {
            public readonly string Name, Tag, DisplayName;
            public readonly RectInt Area;
            public readonly Kind Kind;
            public Zone(string name, string tag, Kind kind, int x, int z, int w, int d, string display = null)
            { Name = name; Tag = tag; Kind = kind; Area = new RectInt(x,z,w,d); DisplayName = display ?? name; }
            public bool Contains(Vector2Int c) => Area.Contains(c);
        }

        public readonly struct Door
        {
            public readonly int Floor;
            public readonly Vector2Int A, B;
            public readonly bool Wide;
            public Door(int floor,int ax,int az,int bx,int bz,bool wide=false)
            { Floor=floor; A=new Vector2Int(ax,az); B=new Vector2Int(bx,bz); Wide=wide; }
        }

        public readonly struct Flight
        {
            public readonly string Name;
            public readonly int Floor;
            public readonly Vector2Int Start, Dir;
            public readonly bool CanCollapse;
            public readonly FlightKind Kind;
            public readonly Vector3 Foot;
            public readonly float Rise, Length, Width;
            public Flight(string name,int floor,int x,int z,Vector2Int dir,bool collapse)
            {
                Name=name; Floor=floor; Start=new Vector2Int(x,z); Dir=dir; CanCollapse=collapse;
                Kind=collapse?FlightKind.Escalator:FlightKind.Stair; Rise=4; Length=8; Width=collapse?2.4f:3;
                Foot=TileTopCenter(Start,floor)-new Vector3(dir.x,0,dir.y)*2;
            }
            public Flight(string name,int floor,Vector3 foot,Vector2Int dir,FlightKind kind,float rise,float width)
            {
                Name=name; Floor=floor; Foot=foot; Dir=dir; Kind=kind; Rise=rise; Length=8; Width=width;
                CanCollapse=kind==FlightKind.Escalator;
                Vector3 start=foot+new Vector3(dir.x,0,dir.y)*2;
                Start=new Vector2Int(Mathf.FloorToInt(start.x/Tile),Mathf.FloorToInt(start.z/Tile));
            }
            public IEnumerable<Vector2Int> Tiles => new[] { Start, Start+Dir };
        }

        public static readonly RectInt Atrium = new(5,4,4,4), Ring = new(4,3,6,6), Basement = new(10,6,4,6);
        public static Bounds BuildingBounds => new(new Vector3(28,4,24),new Vector3(56,16,48));
        public static Vector3 LoadingDoor => new(56,0,34);
        public static Vector3 TruckRear => LoadingDoor+Vector3.right*6;
        public static Vector3 SpawnCentre => new(25,.05f,-12);
        public static Vector3 EntranceTarget => new(28,1.2f,5);
        public static float FloorY(int floor) => floor*StoryHeight;
        public static Vector3 TileTopCenter(Vector2Int c,int floor) => new((c.x+.5f)*Tile,FloorY(floor),(c.y+.5f)*Tile);
        public static bool InGrid(Vector2Int c) => c.x>=0 && c.x<TilesX && c.y>=0 && c.y<TilesZ;
        public static bool InFootprint(Vector2Int c)
        {
            if(!InGrid(c)) return false;
            if(c.y<2 && (c.x<2 || c.x>=11)) return false;
            if(c.y==0 && c.x>=5 && c.x<=8) return false;
            if(c.y==11 && c.x<2) return false;
            if(c.x==13 && c.y<4) return false;
            return true;
        }
        public static bool IsAtriumVoid(Vector2Int c)
        {
            if(!Atrium.Contains(c)) return false;
            return !((c.x==5 || c.x==8) && (c.y==4 || c.y==7));
        }
        public static bool IsBridge(Vector2Int c,int floor) => floor==1 && c.y==6 && c.x>=5 && c.x<=8;
        public static bool IsCoreOpening(Vector2Int c) => c.x>=11 && c.y>=9 && (c.y<=10 || c.x>=12);
        public static bool IsVoid(Vector2Int c,int floor)
        {
            if(!InFootprint(c)) return true;
            if(IsCoreOpening(c)) return true;
            if(floor<0) return !Basement.Contains(c);
            if(floor==2 && c.y>=10 && c.x>=7 && c.x<=9) return true;
            if(floor>0 && IsAtriumVoid(c) && !IsBridge(c,floor)) return true;
            return Flights.Any(f=>f.Kind==FlightKind.Escalator && f.Floor==floor-1 && f.Tiles.Contains(c));
        }
        public static Zone? ZoneAt(Vector2Int c,int floor)
        {
            if(IsVoid(c,floor)) return null;
            if(floor<0) return new Zone("BasementStock","stock",Kind.Store,10,6,4,6,"SERVICE / LEVEL -1");
            foreach(Zone z in ZonesByFloor[floor]) if(z.Contains(c)) return z;
            return null;
        }
        public static bool IsWalkwayTile(Vector2Int c,int floor)
        {
            if(floor<=0 || IsVoid(c,floor)) return false;
            return IsBridge(c,floor) || (ZoneAt(c,floor)?.Kind==Kind.Walkway && Ring.Contains(c));
        }
        public static bool IsProtected(Vector2Int c,int floor)
        {
            if(IsVoid(c,floor)) return false;
            if(floor<0) return c.x==10 || c.y==8;
            // Continuous permanent slabs reach every landing; shortcuts retain the risk.
            return c.x==0 || c.x==10 || c.y==3 || c.y==(floor==2?8:9) ||
                (c.x>=11 && c.y>=8) || (floor==0 && c.x>=5 && c.x<=8 && c.y<=2) ||
                (floor==2 && c.x==5 && c.y==11);
        }
        public static bool IsFlightOpening(Vector2Int a,Vector2Int b,int floor)
        {
            if((a.x>=11 || b.x>=11) && (a.y>=8 && b.y>=8)) return true;
            foreach(Flight f in Flights.Where(f=>f.Kind==FlightKind.Escalator))
            {
                Vector2Int foot=f.Start-f.Dir,head=f.Start+f.Dir*2,top=f.Start+f.Dir;
                if((f.Floor==floor && Match(a,b,foot,f.Start)) || (f.Floor+1==floor && Match(a,b,top,head))) return true;
            }
            return false;
        }
        private static bool Match(Vector2Int a,Vector2Int b,Vector2Int p,Vector2Int q) => (a==p && b==q)||(a==q && b==p);
        public static Door? DoorBetween(Vector2Int a,Vector2Int b,int floor)
        {
            foreach(Door d in Doors) if(d.Floor==floor && Match(a,b,d.A,d.B)) return d;
            return null;
        }
        public static bool SameSpace(Zone a,Zone b) => a.Name==b.Name || (a.Kind!=Kind.Store && b.Kind!=Kind.Store);
    }
}
